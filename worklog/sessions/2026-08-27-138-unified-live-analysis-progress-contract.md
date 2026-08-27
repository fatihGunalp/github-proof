# Unified Live Analysis Progress Contract

**Date:** 2026-08-27
**Result:** DOCS
**Scope:** docs (1 new architecture contract + 5 amended contracts, two of them
via their top-of-file banners) + worklog
**Model Route:** Opus / high
**Model Gate:** APPROVED — user supplied `devam et`

---

## Goal

Decide how Repository Proof and Profile Proof may expose truthful live analysis
progress to the visitor — without fake percentage, fake stage transitions, or
telemetry semantics the backend cannot prove. No transport preselected; the
source decides.

Contract/architecture decision session. No implementation.

---

## Starting State

~~~text
Branch:
session/138-unified-live-analysis-progress-contract

Base HEAD:
03df70f

Pre-existing Working Tree:
?? artifacts/   (user-owned, untouched)
~~~

`main` == `origin/main` == `HEAD` == `03df70f` verified before branching.

---

## Source Audit

### Entry / routing

`IndexModel.OnPostAsync` is the single entry. `GitHubEntryClassifier.TryClassify`
is deterministic, synchronous and network-free: shape selects the owner, then the
canonical parser (`GitHubProfileHandle.Parse` / `GitHubRepositoryUrl.Parse`)
accepts or rejects. Profile targets leave via `RedirectToPagePreserveMethod`, so
the form body — and anything added to it — is re-posted to `/profile`.

**Both flows are one HTTP POST that runs the whole analysis and whose response
IS the result page.** Nothing streams. This is the decisive constraint.

### Repository pipeline

`LiveRepositoryReportService.AnalyzeAsync`:

~~~text
GitHubRepositoryUrl.Parse            sync, sub-ms
snapshotCollector.CollectAsync       ~all wall-clock, one opaque call
analysisRunner.Analyze               sync CPU
reportProjector.Project              sync CPU
~~~

Inside `GitHubRepositorySnapshotOrchestrator.CollectAsync` there is a real
metadata-then-everything-else boundary (metadata resolves 404/403/429 and gates
the rest), but it is **not observable at the Web boundary** — reaching it needs
an Infrastructure change.

### Profile pipeline

`LiveProfileReportService.AnalyzeAsync`:

~~~text
admission lease (ProfileModel)       sync; 429/409/503 terminal
snapshotCollector.CollectAsync       profile metadata + repository manifest
profilePipeline.RunAsync             ~all wall-clock, one opaque call
~~~

`ProfilePipeline.RunAsync` internally runs P3 admission+analysis (async), then
eligibility, `CrossRepositoryEvidenceComposer`, `ProfilePatternPromotionPolicy`,
`RepresentativeRepositorySelectionPolicy`, `ProfileComposer` and
`ProfileReportProjector` — **all synchronous, back-to-back, no interface seam,
no callback**. So RunAsync is one opaque interval from Web.

`ProfileRepositoryAnalysisOrchestrator` runs a fixed pool of R workers over a
shared queue (R=5 in `appsettings.Production.json`). Repository work genuinely
overlaps.

### Existing telemetry

`ProfileExecutionTelemetry` counters **are** incremented live during the run
(`RecordRepositoryAdmitted` / `RecordRepositoryCompleted` from the Web-owned
decorators, via an `AsyncLocal` scope). But `LiveProfileReportService` is
**Scoped**, and the scope is AsyncLocal — neither is reachable from another
request. It is completion/observation telemetry for operators, not a live UI
source, and `public-profile-runtime-collection.md` §9/§9.1 keeps it out of
presentation.

`GitHubProfileSnapshotProgress` already carries operational progress out of the
collector to Web, but publishes availability only, once, with no callback.

### Runtime constraints found

- `ProfileAdmissionOptions.GlobalConcurrency` default **1**;
  `SameProfileConcurrency` **1** → a duplicate same-profile POST is a 409.
- `ProfileExecutionDeadlineOptions.Timeout` is null → deadline currently
  disabled.
- Routing surface is `MapRazorPages()` only. No controllers, no minimal APIs, no
  hubs, no packages for any transport.
- Handler `CancellationToken` is `HttpContext.RequestAborted`, so refresh / back
  / tab close already cancels server work under §11.1.
- Measured: dense profile ~**55.6 s** at R5 (measured Session 133, selected as
  the production candidate Session 134). Long enough to be worth solving.

---

## Rejected User Stages

| Candidate | Reason |
|---|---|
| `Recurring traces identified` | start/completion not observable; also claims an outcome a valid empty run never produces |
| Per-repository stage or event | repositories overlap at R>1; any sequence is fiction |
| `N of M repositories examined` | no truthful denominator — discovered is not admitted, and the admitted total is knowable up front only on the concurrent path |
| Bare growing "N examined so far" | counts admitted only, can never reconcile with the report's repository count; reads as a completeness claim |
| Repository metadata as its own stage | internal to the snapshot orchestrator |
| Report projection as its own Repository stage | projection detail, folded into `repository-proof-prepared` |
| Exposing `ProfileExecutionTelemetry` counters | operator telemetry, not user progress |
| Percentage / ETA | no provable denominator, no monotonic completion metric |

---

## Decision Options

**A — keep the indeterminate state only.** Truthful, zero cost, zero new
semantics. Leaves a ~55 s unexplained wait for a dense profile.

**B — POST creates a run, browser polls, then retrieves the result.** Requires
detaching analysis from its request → background jobs, a result store, a new
deadline start boundary, and loss of `RequestAborted` cancellation. Explicitly
deferred past Milestone B by `public-profile-runtime-collection.md` §10.1.1.
REJECTED.

**C — same as B over SSE.** Same detachment problem, plus a reconnect protocol,
a held connection and proxy buffering. REJECTED for the same reason as B.

**D — existing request keeps running the analysis and delivering the result; a
separate read-only channel observes it.** No detachment, no job system, no
persistence, no change to result or failure delivery. **SELECTED.**

Transport inside D: polling beats SSE and SignalR because a run produces only
3–4 transitions in up to a minute, every ending is already delivered by the
POST, and polling needs no held second connection, no reconnect protocol and no
client runtime. It is **not** credited with surviving a future off-process move
— that is a state/store property, and this registry is process-local (see the
architecture-owner correction below).

---

## Selected Architecture

Locked in `docs/architecture/live-analysis-progress-runtime-contract.md`:

- **Stages:** Repository 4, Profile 3. Every publication point is inside
  `GitHubProof.Web` — no Domain/Application/Infrastructure change authorized.
- **States:** `PENDING → ACTIVE → COMPLETED`, forward-only, no `FAILED`,
  `CANCELLED` or `SKIPPED`.
- **Transport:** bounded polling of `GET /analysis-progress?run=<token>`, a
  read-only Razor Page GET handler returning JSON, constant 2 s interval,
  paused while hidden.
- **Run identity:** client-generated UUID in a hidden form field; survives the
  preserve-method redirect; Web-owned; never an analysis/evidence/telemetry id.
- **Server state:** one process-local ephemeral registry — token, target kind,
  current stage, completed stages, last-updated. Nothing else. Lost on restart:
  if nothing was ever known the client falls back to the indeterminate state,
  and otherwise it stops polling and keeps its last snapshot (§15). The analysis
  is unaffected either way.
- **Failure/Partial:** the POST response delivers every ending; the channel
  reports none. `Partial` is a terminal success and reaches `COMPLETED`.
- **Percent/ETA:** prohibited, on measured evidence (R3 112.209 s / R4 70.888 s
  / R5 55.557 s at a constant 662 requests — wall-clock tracks scheduling, not
  work units).
- **Cancel:** not authorized; no control may be rendered.
- **UI:** inline, in the existing status region. Not a modal.

p10 §8.3 amended in place: the indeterminate state stays canonical and is the
fallback; the pre-138 prohibition text is quoted and superseded; stage copy and
state names locked EN/TR; announcement policy narrowed to one sentence per
transition.

---

## Ownership Decision

Two owners, because two different kinds of fact are involved:

- **p10 §8.3** keeps language and visible shape. It already owned the
  processing-state copy, and adding a second copy owner would duplicate
  normative truth.
- **New `live-analysis-progress-runtime-contract.md`** owns run identity, server
  state, transport, lifecycle, terminal delivery, refresh/reconnect, security
  and update policy. These are architecture, not presentation, and they span
  Repository **and** Profile — `public-profile-runtime-collection.md` is
  Profile-only and cannot own them.

The new contract references `public-profile-runtime-collection.md` §10.1.1
one-way; that document is not edited, so its Milestone B requirement text stays
untouched.

The pre-138 position on live progress turned out to live in **seven** places
across **five** documents, not one. Successive review rounds found them: the
first found three §-level owners this session had missed, the closure round found
two top-of-file banners and a fifth document carrying a stale *permission*. All
seven are amended in place rather than left to drift, and the new contract
carries the supersession table:

| Document | Section | Treatment |
|---|---|---|
| `p10-presentation-language-contract.md` | §8.3 | amended — §8.3.1 quotes the superseded text |
| `github-proof-ui-system-light-dark-v1.md` | §27 *Analysis Progress* | amended — new §27.1 |
| `github-proof-ui-system-light-dark-v1.md` | top-of-file Session 101 banner | annotated — it asserted the indeterminate-only position hundreds of lines above §27.1 |
| `github-proof-design-principles-v1.md` | §7.3 *Analysis Progress* | amended — its own `runtime telemetry açıkça kanıtlamadıkça` escape condition is now met |
| `github-proof-design-principles-v1.md` | top-of-file Session 101 banner | annotated — same reason, above §7.3 |
| `p10-web-profile-flow-implementation-plan.md` | P10-I2A acceptance list | annotated — it records a delivered shape and stays accurate as such |
| `github-proof-milestone-b-p7-ux-working-direction.md` | §22 technology direction | annotated — a stale **permission** (`SSE gerektiğinde analysis progress / streaming`), the one place SSE could have been justified from |

None of them gains runtime semantics, and the runtime contract gains no copy or
visual shape from any of them.

---

## Unresolved / Deferred

Recorded as owner decisions, not authorized:

1. `profile-identity-read` as a 4th Profile stage — needs a publication point on
   `GitHubProfileSnapshotProgress` (Infrastructure).
2. Splitting `profile-evidence-examined` — needs a progress seam inside
   `ProfilePipeline` (Application) plus interface seams for the composer and the
   two policies. **This is why the owner's five-stage Profile mock is not
   achievable in Session 139.**
3. Explicit user cancellation.
4. Anything that must survive restart or a second instance.

Also still open, untouched by this session: the capacity-state half of
§10.1.1's user-visible presentation requirement.

---

## Validation

Docs + worklog only.

~~~text
git diff --check       clean
git status --short     only docs/ + worklog/ + untracked artifacts/
~~~

No production code, test, config or package change. `artifacts/` untouched.

---

## Independent Review

Ran `github-proof-review` in a forked cold context (author reasoning not treated
as authority; the reviewer re-derived every source claim from the code).

**Round 1: FAIL** — BLOCKER 1, HIGH 1, MEDIUM 4, LOW 4.

The reviewer independently confirmed every source claim in the audit above:
publication points, `ProfilePipeline` opacity, R=5 concurrency, the
`MapRazorPages()`-only routing surface, the 307 preserve-method token hop,
`Partial` as terminal success, and the `AnalysisProgressContractTests` claim.
Percentage/ETA/denominator absence and sequential-fiction absence also passed.

All ten findings closed in a same-session correction pass:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| F-1 | BLOCKER | Three further canonical owners of the same prohibition were missed — `github-proof-ui-system-light-dark-v1.md` §27 (literally titled *Analysis Progress*), `github-proof-design-principles-v1.md` §7.3, and the P10-I2A acceptance list. Session 139 would have violated them while the session said READY | all three amended in place; the runtime contract now carries an explicit supersession table naming each |
| F-2 | HIGH | `CURRENT.md` / `INDEX.md` still on Session 137 | both updated |
| F-3 | MEDIUM | "single-instance beta target is recorded in §10.5 and §10.1.1" — no document records that; the phrase existed only in the new file | reworded as this contract's own decision, resting on §10.1.1's actual Post-B deferral |
| F-4 | MEDIUM | the review section recorded a review that had not run | replaced with this record |
| F-5 | MEDIUM | Profile stage 1 completed at registration → `PENDING -> COMPLETED`, which §7 forbids *without exception*, forcing the implementer to make an architecture call | §8 now locks registration *before* the admission lease, with the first stage born `ACTIVE`; §7 unchanged |
| F-6 | MEDIUM | the existing status region is `aria-atomic="true"` over everything, so §8.3.5 and §8.3.6 could not both hold without a DOM change nobody authorized | §8.3.5 now specifies the target DOM: stage list as ordinary content, live region narrowed to a single-sentence child keeping all three attributes |
| F-7 | LOW | the retained `What this analysis includes` list duplicates three stage labels verbatim | §8.3.1 makes the two mutually exclusive in one view |
| F-8 | LOW | `crypto.randomUUID()` is undefined outside a secure context; a throw could break the existing duplicate-submit block | §8 adds the no-token path: no run, canonical indeterminate state, form behavior unchanged |
| F-9 | LOW | the 55.6 s measurement attributed to "Session 133/134" | attributed to Session 133's measurement, Session 134's selection |
| F-10 | LOW | p10 §8.2 does not name `WebPresentationLocalizer` | citation softened to §8.2's ownership rule |

The stage sets, the state machine, the transport choice and the ownership split
did not change in the correction pass.

**Round 2 (closure verification): PASS WITH FINDINGS** — BLOCKER 0, HIGH 0,
MEDIUM 0, LOW 2. All ten round-1 findings verified **CLOSED**, none partial. The
reviewer separately re-confirmed that §27.1's `aria-atomic` sentence and p10
§8.3.5's target DOM block agree, that the F-5 chain leaves no case where an
implementer must choose, and that scope is still docs + worklog only.

Two new LOW findings from the correction pass, both closed:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| N-1 | LOW | the prohibition lists drifted on one item — ui-system §27.1 dropped `checkmark`, design-principles and the P10-I2A list kept it, while p10 §8.3.3 allowed an unnamed decorative glyph. An implementer rendering ✓ for `COMPLETED` would have violated one of them | p10 §8.3.3 made the single owner and authorizes the checkmark for `COMPLETED`, on the ground that the old prohibition meant a checkmark *not backed by server state*; the other two lists now defer to it |
| N-2 | LOW | §16's "only the analysis path registers" rationale went stale once F-5 moved registration before the admission lease — an admission-rejected run now also holds a slot briefly | §16 rewritten to say so and to rest the bound on §12's cap |

No third round was run at that point: the remaining verdict carried no BLOCKER,
HIGH or MEDIUM, and both LOW fixes were one-paragraph clarifications inside
sections the reviewer had already verified.

---

## Architecture-Owner Precision Correction

The architecture owner accepted the overall architecture — synchronous POST
unchanged, polling preferred, process-local ephemeral registry, percentage/ETA
prohibited, stage sets unchanged — and returned four precision findings. None
reopened a decision; all four are closed.

### AO-F-1 — transport / state-store reasoning: **CLOSED**

The owner was right that the round-1 table committed a category error. Surviving
an off-process move is a property of the **state/store**, not of the transport,
and the selected polling implementation is itself process-local and loses
progress on restart. That row is removed, and §10.1 now states the distinction
explicitly and credits no transport with future distributed survivability.

The `SignalR: package required = yes` cell was also **not source-proven, and was
wrong on the server side**. Verified: `src/GitHubProof.Web/GitHubProof.Web.csproj`
declares **no `PackageReference` at all** and runs on the
`Microsoft.NET.Sdk.Web` shared framework, which carries server-side hub support.
The claim is narrowed to the cost that *is* verified — a JS client library that
would have to be vendored into `wwwroot/js` (the repository has no
`package.json` and no node toolchain, and `wwwroot/js` is three hand-written
vanilla files), plus a hub as a second routing mechanism.

Polling stays selected, now grounded only in current facts: 3–4 transitions per
run, the POST already delivering every ending, one small read-only GET, no held
second connection, no reconnect protocol, plain request/response testing, no
client runtime, negligible push-latency value.

### AO-F-2 — HTTP cache semantics: **CLOSED**

New §10.5. Every response carries `Cache-Control: no-store`, on the
`known:true` and `known:false` paths alike. `ETag`, `Last-Modified`, conditional
requests and `304` are prohibited and added to §17.

Verified while closing it: `Cache-Control`, `CacheControl`, `ResponseCache` and
`no-store` appear **nowhere** in the codebase, so there is no repository-standard
equivalent to defer to. §10.5 says so rather than implying an existing
convention.

A stale progress snapshot would be strictly worse than no progress: the visitor
cannot distinguish a frozen snapshot from a stalled analysis.

### AO-F-3 — bounded runtime values: **CLOSED**, no owner input needed

All six locked exactly in §13/§13.1; Session 139 selects none.

| Value | Locked |
|---|---|
| Poll interval | 2000 ms constant, first poll immediate, no backoff |
| Initial `known:false` retry limit | 5 consecutive before any known response |
| Consecutive transport-failure stop limit | 3 |
| Registry absolute TTL | 10 minutes |
| Registry maximum entries | 64 |
| Configuration | **none of the six is configurable** |

Rationale is recorded per value. The configuration decision is the one that
needed an explicit argument: R and U earned knobs because they were selected
from measurement and are deployment-tuning levers. These six are not — exceeding
the cap or the TTL degrades only the progress enhancement and never an analysis,
so no operator ever needs to change one under load. All six are constants owned
by `GitHubProof.Web`; promoting any to configuration later is an owner decision.

No value required owner input, so no STOP was raised.

### AO-F-4 — atomic snapshot semantics: **CLOSED**

New §7.1. A reader must never observe an intermediate transition state. One
transition publishes one coherent snapshot; the invariant at every observable
instant of a live run is that `completed` is exactly the stages preceding
`active`, with no gap and no overlap. `completed` containing stage N while
`active` is still stage N, and a `null` `active` between two non-terminal
stages, are both prohibited by name.

The registry exposes immutable snapshots replaced atomically; a published
snapshot is never mutated in place and one transition is never two writes. No
concrete synchronization primitive is prescribed. Terminal completion is the one
authorized `null` active — `completed` = every stage, published before the entry
is released.

### Final closure review

A **final forked cold-context review** verified all five focus areas against
source and returned **PASS WITH FINDINGS** — BLOCKER 0, HIGH 0, MEDIUM 5, LOW 2 (**seven** findings).
Areas 1–4 PASS; area 5 (Session 139 readiness) FAIL against the strict
"zero decisions" bar.

The reviewer's own summary line read `MEDIUM 4 · LOW 2`, which totals six and
contradicts its own itemised list. Reconciled against that list, which is the
record of account: R-1..R-5 are MEDIUM and R-6..R-7 are LOW, so the count is
**seven**. The header was an arithmetic slip; no finding was missing. All seven
closed:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| R-1 | MEDIUM | the Decision Options narrative in this file still credited polling with surviving a future off-process move — the exact category error the correction above declares closed | clause removed and replaced with the state/store distinction |
| R-2 | MEDIUM | `CURRENT.md` and this file described the poll interval as a **floor**, which is precisely the phrasing that authorizes the backoff §17 prohibits — and `CURRENT.md` is the first thing Session 139 reads | both now say constant 2 s; `INDEX.md` corrected too |
| R-3 | MEDIUM | the 10-minute TTL was justified as being "above the two-minute per-request transport timeout" — a non-sequitur: that timeout bounds one of a dense profile's 662 requests, not the run, and `ProfileExecutionDeadlineOptions.Timeout` defaults to null so nothing caps a run by default | justification rewritten on the real basis, with the benign over-run consequence stated |
| R-4 | MEDIUM | the terminal snapshot was **optional** (`may publish`), so an implementer had to choose between "all completed, no active" and leaving the last stage frozen as `ACTIVE` — visitor-visible for up to one poll, and p10 §8.3.6 had no rule for the null-active announcement | now **must** publish; p10 §8.3.6 gains one line: the live region does not change at terminal |
| R-5 | MEDIUM | §10.4 called the response shape exhaustive, but §8's "malformed token is rejected" path had neither a status code nor a body — and the choice changes client accounting, since a non-200 counts against the failure limit of 3 while a `known:false` counts against the retry limit of 5 | **HTTP 400, empty body**, with the accounting consequence stated; the two JSON bodies remain the exhaustive JSON set |
| R-6 | LOW | §5.1 called all three pipeline types "concrete Application types". Verified: `ProfilePatternPromotionPolicy` and `RepresentativeRepositorySelectionPolicy` are `GitHubProof.Domain.Profile`; only the composer is Application. §3 rests entirely on a layer boundary, so this understated deferred decision 2's cost | corrected in §5.1 and in `INDEX.md`; §18 was already right |
| R-7 | LOW | §13.1's "the poll interval alongside them" placed a client value on the server registry type | rewritten to name each constant's home |

R-4 and R-5 are the two that would genuinely have stopped a Session 139
implementer under the stop-on-ambiguity mandate. Neither changed the stage sets,
the state machine, the transport selection, the ownership split or any of the
six locked values.

The reviewer also independently confirmed the two factual claims this session
rests on: `GitHubProof.Web.csproj` carries **no `PackageReference`**, there is no
`package.json`/lockfile/`node_modules` anywhere, `wwwroot/js/` is three
hand-written files, `Program.cs:111` is the only `Map*` call, and
`Cache-Control`/`CacheControl`/`ResponseCache`/`no-store`/`ETag`/`Last-Modified`
appear **nowhere** in `src/` or `tests/`. No cross-document contradiction was
introduced by any of the four amendments.

### Closure review (final)

A **final closure review** ran in a forked cold context over the corrected tree,
re-deriving every load-bearing claim from source rather than trusting earlier
rounds. **PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 3, LOW 4 (seven).**
Questions 1–10 PASS; 11 and 12 FAIL, each on one item. It found **no false
factual claim** in the contract. All seven closed:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| C-A | MEDIUM | two incompatible renderings for a `known:false` arriving **after** a known response but before the run ends: §9 said keep the last snapshot, §12 said degrade to the indeterminate state — and since p10 §8.3.1 makes the stage list and the description list mutually exclusive, degrading *means* hiding the stage list, i.e. the reset §9 forbids. §15 had no row for it. Reachable: no deadline is configured, so nothing caps a run below the 10-minute TTL | new §15 row locks it — stop polling, keep the last snapshot, **do not** hide the stage list; §12's two clauses reworded; the indeterminate fallback is now reserved for the never-known case |
| C-B | MEDIUM | both amended product documents still carried a top-of-file Session 101 `Amended` banner asserting the indeterminate-only position, hundreds of lines above the §27.1 / §7.3 amendments that reverse it — falsifying the contract's own "every place is amended" claim | both banners annotated with a forward pointer; both added to the supersession table |
| C-C | MEDIUM | this file and `CURRENT.md` told Session 139 that the `aria-atomic="true"` assertion encoded the pre-138 shape and could be changed. **Wrong and dangerous:** it is a plain file-level substring check on `_GitHubInput.cshtml`, and p10 §8.3.5 *keeps* all three attributes on the narrowed region, so it passes unchanged — it is the only automated guard that the live region keeps `aria-atomic` | both files corrected: exactly two assertions are superseded (`setInterval`, `fetch(`); `aria-atomic`, `EventSource` and `WebSocket` must stay |
| C-D | LOW | R-6 residue — `CURRENT.md` still said the five-stage mock needs "an Application progress seam", understating the cost in the file Session 139 reads first | corrected to name the two Domain policy seams as well |
| C-E | LOW | §15's bare `§11.1` reference resolves inside this contract to §11 *Percentage and ETA*, which has no §11.1 | qualified as `public-profile-runtime-collection.md` §11.1, matching §14 |
| C-F | LOW | `github-proof-milestone-b-p7-ux-working-direction.md` §22 still sanctioned `SSE gerektiğinde analysis progress / streaming` — a fifth document, and a stale **permission** rather than a stale prohibition, so the one place SSE could be justified from | line marked SUPERSEDED with a §22 note; document added to the supersession table |
| C-G | LOW | §13's first-poll rationale claimed it makes stage 1 observable as `ACTIVE`. Not true in either flow: `GitHubEntryClassifier` already parses and discards, so stage 1's condition is effectively met at registration | rationale restated (target kind + first wall-clock stage at ~0 s); §4 now records stage 1 as instantaneous for the same reason as stages 3–4, with padding/delay/animation prohibited |

Only C-A touched the closure bar. Nothing in this round changed the stage sets,
the state machine, the transport selection, the endpoint semantics, the six
locked values or the ownership split.

### Ratification round

**PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 4, LOW 4 (eight).** C-A..C-G
all verified CLOSED; the supersession table verified complete against an
independent `docs/` sweep; scope and the four-round finding-count record
verified consistent. All eight closed.

Four were **reconciliation debt the C-A/C-G fixes themselves created** — the
recurring lesson of this session, which is that a correction has to be swept for
every place that restated the thing it corrected:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| P-1 | MEDIUM | §2 still promised the p10 §8.3 indeterminate experience for restart and second-instance, which is exactly what C-A stopped §12 from saying | §2 now splits the never-known case from the snapshot-shown case and defers to §15 |
| P-4 | MEDIUM | the Selected Architecture summary still read "lost on restart, which degrades to the indeterminate state and nothing more" | rewritten to the §15 rule |
| P-5 | LOW | §8 justified §7 compliance by claiming the client observes stage 1 `ACTIVE` — which §4, after the C-G fix, says will not happen | rationale moved onto the sequence the **server publishes**, where §7 actually binds |
| P-6 | LOW | the new §15 rationale rested on the stage-list/description-list exclusivity, but the description list is a docs-only MAY never delivered in Web (verified: no string, no `WebTextKey`) | rationale moved onto the real basis — a stage list with no server state behind it is client-invented state, which §7 forbids |

One was a **real copy defect this session introduced**:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| P-3 | MEDIUM | §8.3.2 requires one label to serve all three states, but every locked **Turkish** label was present-continuous, so `GitHub hedefi doğrulanıyor · Tamamlandı` says "is being validated · Completed". Turkish `-iyor` is a finite verb; the English gerund phrases carry no tense and were fine. TR is a shipped culture and the copy is locked, so Session 139 could not have fixed it | all seven TR labels relocked as verbal nouns — `GitHub hedefinin doğrulanması`, `Herkese açık GitHub verilerinin okunması`, `İzlerin analiz edilmesi`, `Repository Proof'un hazırlanması`, `GitHub profilinin doğrulanması`, `Herkese açık repository'lerin okunması`, `Repository kanıtlarının ve tekrar eden izlerin incelenmesi`. EN unchanged, and §8.3.2 records why the two columns now diverge |

One was a **genuine gap that left Session 139 a choice**:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| P-2 | MEDIUM | no owner said what the live region carries between submit and the first known snapshot (up to ~10 s under the retry limit), or in the never-known fallback. §8.3.6 only covered the active-stage case, and the existing test would not have caught the regression | §8.3.6 now locks it: the canonical indeterminate sentence until the first snapshot and in fallback, then the active stage label — exactly today's behavior, preserved |

Two were record accuracy:

| ID | Sev | Finding | Closure |
|---|---|---|---|
| P-7 | LOW | `CURRENT.md` still said "all four documents", never mentioning the fifth | corrected to seven locations across five documents |
| P-8 | LOW | the session header and Ownership Decision table still said four | both corrected; the table now lists all seven rows |

Nothing in this round changed the stage sets, the state machine, the transport
selection, the endpoint semantics, the six locked values or the ownership split.
The only user-visible change is the Turkish label column.

### Closure confirmation round

**PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 1, LOW 5 (six).** P-1..P-8 all
verified CLOSED. P-3's Turkish was checked independently rather than accepted:
all seven labels are grammatical verbal nouns, each reads correctly against
`Bekliyor` / `Sürüyor` / `Tamamlandı`, the `Repository Proof'un` and
`repository'lerin` apostrophe/suffix handling matches the existing convention in
`WebPresentationLocalizer.cs`, the EN column is intact, and §8.3.2's
one-label-serves-three-states rule now holds in **both** columns — which it did
not before P-3.

| ID | Sev | Finding | Closure |
|---|---|---|---|
| Q-1 | MEDIUM | the last ownerless decision. **Three** elements carry `data-analysis-form`: the landing input, plus the Profile drill-down forms in `_ProfileRepositoryEvidence.cshtml:66` and `_ProfileStartingPoint.cshtml:88`, which post a hidden `GitHubTarget` to `/Index` and **do start real Repository Proof runs**. `analysis-progress.js` binds to all three via `querySelectorAll`, and only the landing has a `data-analysis-status` region. No owner said whether a drill-down run gets a token — so Session 139 would have had to choose | contract §8 locks it: **only the landing form carries a token**; the drill-down forms emit none, register no run and keep today's duplicate-submit behavior, because they have no status region and p10 §8.6.5 already rules out per-row visible processing. §8.6.5's "that script is not modified" sentence scoped to say Session 139 may modify it for the landing form only |
| Q-2 | LOW | ui-system §27.1's surviving-prohibition list still omitted the checkmark disambiguation the other two lists carry | one sentence added, deferring to p10 §8.3.3 as sole owner |
| Q-3 | LOW | §4 said "no label appears here" 22 lines above an illustrative quoted label | reworded to "no label is *locked* here" |
| Q-4 | LOW | §8.3.1's exclusivity rationale said "three of its lines are also stage labels" — true only in EN after the P-3 relock | scoped to English, with the rule restated so it still binds in both languages |
| Q-5 | LOW | `CURRENT.md` stopped at the "final closure review" and recorded neither the ratification round nor the TR relock | both recorded |
| Q-6 | LOW | `INDEX.md`'s taxonomy read 4 + 3 + 1 = 8 but miscounted: record accuracy was 2, and the genuine gap (P-2) was uncategorised | corrected to 4 reconciliation + 1 gap + 2 record + 1 copy defect |

Five of the six were record and wording hygiene with no effect on what Session
139 builds. Only Q-1 touched the closure bar, and it is the same class of miss as
the round-1 BLOCKER: a place the delivered Web already used the mechanism this
contract was reasoning about.

---

## Session 139

**READY.** Boundary: Web only. May implement exactly the stage identities,
order, states, endpoint, transport, run identity, registry fields, copy and
accessibility rules the two contracts lock — nothing else.

Session 139 must also update `tests/GitHubProof.Web.Tests/AnalysisProgressContractTests.cs`.
Exactly **two** assertions are superseded: the `DoesNotContain` exclusions for
`setInterval` and `fetch(` in `analysis-progress.js`, which encode the pre-138
prohibition. Changing those two is authorized and is not a regression.

Everything else in that file must **stay**:

- the `EventSource` and `WebSocket` exclusions — §17 still prohibits both;
- `Assert.Contains("aria-atomic=\"true\"", ...)`. This is a plain file-level
  substring check on `_GitHubInput.cshtml`, not a whole-region assertion, and
  p10 §8.3.5 **keeps all three attributes** on the narrowed live region, so it
  passes unchanged under the new DOM. It is the only automated guard that the
  live region keeps `aria-atomic`, and weakening it would silently drop an
  accessibility guarantee this contract still requires.

**No Milestone B completion is claimed.**

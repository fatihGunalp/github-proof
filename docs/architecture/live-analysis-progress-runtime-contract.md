# Live Analysis Progress — Runtime Contract

**Status:** Canonical. Locked by Session 138.

**Owns:** the runtime semantics of user-visible live analysis progress for both
GitHub Proof analysis flows — run identity, server state, transport, request
lifecycle, terminal delivery, refresh/reconnect behavior, security and update
policy.

**Does not own:** any user-facing copy, stage label, visual shape or
announcement wording. Those stay owned by
`docs/product/p10-presentation-language-contract.md` §8.3, which this contract
is the runtime counterpart of. Where §8.3 says *what the visitor reads*, this
document says *what the server may prove*.

**Does not own:** evidence, analysis, eligibility, pattern, representative,
composition, report or availability semantics. Nothing in this contract may be
read by an evaluator, policy, composer, projector or report section, and nothing
here changes what any analysis produces.

**Supersedes**, together with p10 §8.3.1–§8.3.6, the pre-Session-138
"no live stage state" position wherever it was stated. Every place is amended in
place rather than left to drift:

| Document | Section | Status |
|---|---|---|
| `p10-presentation-language-contract.md` | §8.3 | amended — §8.3.1 quotes and supersedes the old text |
| `github-proof-ui-system-light-dark-v1.md` | §27 Analysis Progress | amended — §27.1 |
| `github-proof-design-principles-v1.md` | §7.3 Analysis Progress | amended — Session 138 note; its own `runtime telemetry açıkça kanıtlamadıkça` condition is now met |
| `p10-web-profile-flow-implementation-plan.md` | P10-I2A acceptance list | annotated — it records a delivered shape and stays accurate as such |
| `github-proof-ui-system-light-dark-v1.md` | top-of-file Session 101 `Amended` banner | annotated — it stated the indeterminate-only position hundreds of lines above §27.1 |
| `github-proof-design-principles-v1.md` | top-of-file Session 101 `Amended` banner | annotated — same reason, above §7.3 |
| `github-proof-milestone-b-p7-ux-working-direction.md` | §22 technology direction | annotated — it sanctioned `SSE gerektiğinde analysis progress / streaming`, a stale **permission** rather than a stale prohibition, now prohibited by §17 |

None of those documents gains runtime semantics from this one, and this one
gains no copy or visual shape from them.

`public-profile-runtime-collection.md` §10.1.1 ("User-visible processing and
capacity state") states the Milestone B *requirement* that an admitted
long-running profile analysis must not present an unexplained blank wait. This
contract is one bounded way that requirement is met for the *processing* state.
It changes nothing about capacity, admission, budget or deadline behavior, and
it does not close that requirement's capacity-state half.

---

## 1. Problem

Both analysis flows are today a single HTTP POST that performs the whole
analysis and whose response **is** the result page:

~~~text
POST /            -> IndexModel.OnPostAsync   -> repository result page
POST /profile     -> ProfileModel.OnPostAsync -> profile result page
~~~

A browser cannot render intermediate state from a navigation it is still
waiting on. Live progress therefore cannot come from the analysis request. It
can only come from a **second, read-only channel** observed while that request
is still in flight.

Measured runtime makes this worth solving for Profile: Session 133 measured the
dense reference profile at about 55.6 seconds under the configuration Session
134 then selected as the production candidate. That is a long, currently
unexplained wait.

## 2. Locked shape

~~~text
POST (unchanged)          runs the analysis and returns the terminal page
progress registry         in-memory, process-local, ephemeral
GET /analysis-progress    read-only observation of the in-flight run
~~~

The analysis is **not** detached from its request. No run creation endpoint, no
result retrieval endpoint, no background job, no queue and no persistence are
introduced. Result delivery, failure delivery and every existing status code
stay exactly as they are today.

Live progress is a **strictly optional enhancement over the canonical
indeterminate state**. When progress is unavailable and **nothing has yet been
known** — process restart, a second application instance, a blocked request, a
client without JavaScript — the experience is exactly the one p10 §8.3 already
describes, with no error surface of its own.

Once a snapshot **has** been shown, §15 governs instead: the client keeps that
snapshot rather than reverting to the indeterminate state, because reverting
would erase information that was true when reported. The two cases are
deliberately different and are never collapsed.

## 3. Stage truthfulness rule

A user-visible live stage may exist only where the current source has a real
bounded phase whose **start and successful completion are both observable at an
authorized publication point**. A stage may not be created from a phase that is
merely named in code, and a phase that cannot prove completion may not receive a
completed state.

Every authorized publication point is inside `GitHubProof.Web`. This contract
authorizes **no** Domain, Application or Infrastructure change. That boundary is
what fixes the stage sets below: they are exactly the phases the Web layer can
already prove.

## 4. Repository Proof stages

Four stages, in this fixed order. Publication points are all in
`LiveRepositoryReportService` / `IndexModel`. The EN/TR label for every stage
identity in §4 and §5 is locked by
`p10-presentation-language-contract.md` §8.3.2; no label is *locked* here, and
any label quoted below is illustrative only.

| # | Stage identity | Completed when |
|---|---|---|
| 1 | `repository-target-validated` | `GitHubRepositoryUrl.Parse` has accepted the classified entry value |
| 2 | `repository-public-data-read` | `IGitHubRepositorySnapshotCollector.CollectAsync` has returned a successful snapshot |
| 3 | `repository-traces-analyzed` | `RepositoryAnalysisRunner.Analyze` has returned |
| 4 | `repository-proof-prepared` | `RepositoryReportProjector.Project` has returned |

Stage 2 covers the entire provider phase and carries effectively all of this
flow's wall-clock. The metadata-versus-remaining-surface boundary is internal to
`GitHubRepositorySnapshotOrchestrator` and is deliberately **not** exposed: it is
not observable at the Web boundary, and reaching it would require an
Infrastructure change this contract does not authorize.

Stages 3 and 4 are synchronous CPU phases that typically complete in
milliseconds. They are truthful, and a poll will often never observe them
active. That is acceptable (§7); fabricating a visible duration for them is not.

Stage 1 is in the same category, and more sharply so: `GitHubEntryClassifier`
already parses the value to accept it and discards the result, so by the moment
the run is registered the completion condition is effectively met and the
publication point is a re-parse. `Validating the GitHub target — Completed` is a
true statement, and no poll is expected to observe it `ACTIVE`. Profile stage 1
adds only an in-memory lease acquisition. Neither may be padded, delayed or
animated to make it visible.

## 5. Profile Proof stages

Three stages, in this fixed order. Publication points are all in
`LiveProfileReportService` / `ProfileModel`.

| # | Stage identity | Completed when |
|---|---|---|
| 1 | `profile-target-validated` | the entry value re-classified as a profile, `GitHubProfileHandle.Parse` accepted it, and the admission lease was granted |
| 2 | `profile-public-surface-read` | `IGitHubProfileSnapshotCollector.CollectAsync` has returned a successful snapshot (profile metadata **and** the public repository manifest) |
| 3 | `profile-evidence-examined` | `ProfilePipeline.RunAsync` has returned a composed result |

Stage 3 spans repository admission, every admitted repository's collection and
analysis, cross-repository evidence composition, pattern promotion,
representative selection, profile composition and report projection. It carries
roughly all of this flow's wall-clock.

### 5.1 Why Profile has three stages and not five

`ProfilePipeline.RunAsync` is one opaque interval at the Web boundary. Its
internal P5/P6/P7 boundaries are driven by `CrossRepositoryEvidenceComposer`
(Application) plus `ProfilePatternPromotionPolicy` and
`RepresentativeRepositorySelectionPolicy` (**Domain**) — one Application type and
two Domain policies, none with an interface seam, reached through no observable
callback. Splitting stage 3 would therefore require a progress seam **inside**
`ProfilePipeline`, the single Application composition path, which today carries
no operational concern at all — and seams on two Domain policies, which is a
heavier constraint than an Application-only change; the existing Session 120/131 telemetry
was deliberately built as Web-owned decorators precisely to keep it that way.

A separate `profile-identity-read` stage, completed at the moment profile
availability resolves, is technically reachable through the existing
`GitHubProfileSnapshotProgress` carrier, but only by extending that
Infrastructure type with a publication point it does not have.

Both are therefore **deferred owner decisions**, recorded here and not
authorized. Neither may be implemented as an inference, an assumption or a
timer.

## 6. Rejected stage and progress candidates

Recorded so they are not reintroduced.

| Candidate | Verdict | Reason |
|---|---|---|
| `Recurring traces identified` as a live stage | REJECTED | its start and completion are not observable (§5.1), and "identified" would also claim an outcome a valid empty run never produces |
| Per-repository stage or event | REJECTED | repositories run concurrently at R > 1, so any per-repository sequence is fiction; also prohibited by the coarse-grained rule (§13) |
| `N of M repositories examined` | REJECTED | no truthful denominator exists: discovered is not admitted, and the admitted total is knowable up front only on the concurrent path, not the sequential one |
| Bare growing "N repositories examined so far" | REJECTED | counts only admitted repositories, so it can never reconcile with the repository count the delivered report shows; reads as a completeness claim it cannot support |
| Percentage complete | PROHIBITED | see §11 |
| Estimated time remaining | PROHIBITED | see §11 |
| Repository metadata as its own stage | REJECTED | internal to the snapshot orchestrator; not observable at an authorized publication point |
| Report projection as its own Repository stage | REJECTED | a projection detail, not a user-meaningful phase; folded into `repository-proof-prepared` |
| Exposing `ProfileExecutionTelemetry` counters | REJECTED | it is completion/observation telemetry for operators, not user progress, and §9/§9.1 of the runtime collection contract keep it out of presentation |

## 7. Stage state machine

Exactly three states:

~~~text
PENDING     the stage has not started
ACTIVE      the stage has started and has not completed
COMPLETED   the stage completed successfully
~~~

Allowed transition, per stage:

~~~text
PENDING -> ACTIVE -> COMPLETED
~~~

Prohibited, without exception:

- any backward transition, including `COMPLETED -> ACTIVE`;
- `PENDING -> COMPLETED` without passing through `ACTIVE`;
- two stages `ACTIVE` at once;
- a stage reaching `ACTIVE` or `COMPLETED` before its predecessor is
  `COMPLETED`;
- any state advanced by the client rather than reported by the server.

There is deliberately **no** `FAILED`, `CANCELLED` or `SKIPPED` state. A run
that ends without reaching a stage simply leaves that stage `PENDING`, and the
terminal outcome is delivered by the POST response (§9). Adding a failure state
would duplicate, in a second channel, truth that the result page already owns.

The client renders exactly the last snapshot the server reported. It never
advances a stage on its own, never runs a stage timer, never interpolates
between snapshots and never animates a stage it has not been told is active.

### 7.1 Observable atomicity

A reader must **never** observe an intermediate transition state. Advancing
from stage N to stage N+1 publishes **one coherent snapshot**:

~~~text
before:   completed = [ ..., ]              active = stage-N
after:    completed = [ ..., stage-N ]      active = stage-N+1
~~~

Externally observable intermediate snapshots are prohibited, including:

- `completed` already containing stage N while `active` is still stage N;
- `active` being `null` between two non-terminal stages;
- `completed` and `active` disagreeing about which stage is next.

The invariant, at every observable instant of a live run: `completed` is exactly
the stages preceding `active` in the fixed order, with no gap and no overlap.

The registry therefore exposes **immutable, coherent snapshots**: a transition
publishes a new snapshot value in a single atomic replacement, and a reader
takes the current snapshot once and serializes that instance. A published
snapshot is never mutated in place, and one transition is never two writes. No
particular synchronization primitive is prescribed — only that atomic
replacement and non-mutation hold.

**Terminal completion is the one authorized `null` active, and it is
mandatory.** When the final stage completes, the run **must** publish

~~~text
completed = every stage        active = null
~~~

before the entry is released (§9). This is a coherent terminal snapshot, not an
intermediate one, and a reader that sees it is seeing the truth. It is required
rather than optional so that the last stage is never left frozen as `ACTIVE` in
the final poll before navigation — an implementer must not have to choose
between the two renderings.

A reader may or may not observe this snapshot: the entry is released immediately
after, and the POST response usually replaces the page first. Both outcomes are
correct, and the client treats a later `known:false` exactly as §9 requires.

## 8. Run identity

Live progress requires correlating the in-flight POST with the observation
channel. **Web owns this identity**, because progress is a Web presentation
concern and no other layer needs, produces or consumes it.

Locked rules:

- The correlation token is generated **by the client** before submit
  (`crypto.randomUUID()`) and travels as a hidden field in the same form body.
- **Only the landing form carries a token.** `_GitHubInput.cshtml` is the one
  form in scope. Three elements carry `data-analysis-form` today — the landing
  input and the two Profile drill-down forms in
  `_ProfileRepositoryEvidence.cshtml` and `_ProfileStartingPoint.cshtml`, which
  post a hidden `GitHubTarget` to `/Index` and do start real Repository Proof
  runs — and `analysis-progress.js` binds to all three. The drill-down forms
  **deliberately stay progress-free**: they emit no token, register no run, and
  keep exactly today's duplicate-submit behavior. They have no status region and
  no stage-list markup, and p10 §8.6.5 explicitly rules out per-row visible
  processing, so giving them progress would mean inventing a per-row surface
  this contract does not authorize. Session 139 must not add one.
  The landing's preserve-method redirect re-posts that body, so the token
  survives the `/` to `/profile` hop with no additional mechanism.
- The server accepts only a canonical UUID form and rejects anything else
  without allocating registry state.
- It is **not** an analysis identifier. It must never be reused as, derived
  from, or written into `RepositoryAnalysis.AnalysisId`, a
  `RepositoryAnalysisReference`, a profile identity, an evidence key, a
  telemetry key or any report field, and it must never reach Domain,
  Application or Infrastructure.
- A run is registered only by the page model that performs the analysis:
  `IndexModel` for repository targets, `ProfileModel` for profile targets. The
  observation endpoint **never** creates a registry entry.
- **Registration creates the run with its first stage already `ACTIVE`.** That
  is the registry's initial snapshot, not a transition, so §7 stays intact: §7
  constrains the sequence the **server publishes**, and that sequence is still
  `ACTIVE` then `COMPLETED` in order, with `PENDING` as the pre-registration
  state the page renders. Whether a poll happens to observe stage 1 `ACTIVE` is
  irrelevant to §7 and, per §4, is not expected.
- Registration happens at the earliest point the flow is committed to analyzing:
  for repository targets, once `GitHubEntryClassifier` has resolved a repository
  route; for profile targets, once `ProfileModel` has re-classified the value as
  a profile — **before** the admission lease, so that stage 1 can complete on the
  lease being granted. An admission rejection releases the entry on the same
  `finally` path as any other ending (§9) and produces its existing page.
- If the client cannot produce a token — `crypto.randomUUID()` is unavailable
  outside a secure context — the form submits without one, no run is registered,
  and the visitor gets the canonical indeterminate state. Token generation must
  never be able to throw out of the submit handler: the existing duplicate-submit
  block and serialization behavior are unchanged in that case.
- A registration for a token that is already live is refused. The analysis then
  proceeds normally with no progress; a duplicate token may never displace or
  join an existing run.

## 9. Terminal delivery and failure

**The progress channel never reports a terminal outcome.** Every ending —
success, `Partial`, invalid target, not found, rate limited, forbidden,
provider failure, visitor/same-profile/global capacity rejection, budget
rejection, deadline expiry, thrown fault — is delivered exactly as it is today,
as the POST response that replaces the page. When that response arrives, the
progress UI ceases to exist along with the page that held it.

Consequences, all locked:

- No stage becomes `FAILED`.
- Completed stages are not preserved across the navigation, and no stage history
  is persisted anywhere.
- The registry entry is released in a `finally` on every path, including thrown
  faults and cancellation.
- A poll for a released or never-registered token answers "unknown". The client
  must not render "unknown" as an error, a failure or a reset — it stops
  polling and keeps its last snapshot until navigation replaces the page.
- A provider, capacity, budget or deadline outcome must never appear in the
  progress channel in any form. It cannot become evidence, a negative finding, a
  missing skill or a statement about the developer, because the channel carries
  no such vocabulary at all.

### 9.1 Partial is not failure

A `Partial` profile composition is a **terminal success**: `HasProfile` is true,
the report is projected and the result page renders with its existing Partial
notice. Stage 3 therefore reaches `COMPLETED` normally.

**Stage completion describes a phase finishing. It never describes the
completeness of the result.** The progress channel and the progress UI must
never carry the words partial, incomplete, complete, failed or any other
completeness vocabulary. Result completeness stays owned by the report surface.

## 10. Transport

**Selected: client polling of a read-only JSON endpoint.**

### 10.1 What the comparison is and is not about

Transport is **not** what determines off-process or multi-instance
survivability. That is a property of the state/store or message architecture.
The polling implementation selected here is itself process-local and loses
progress on restart or on a second instance (§12), exactly as an SSE
implementation over the same registry would. No transport is credited with
future distributed survivability, and none is penalised for lacking it.

The comparison below is therefore only about cost and fit **against the current
runtime**.

### 10.2 Comparison

| | SSE | Polling | SignalR |
|---|---|---|---|
| Transitions it must carry | 3–4 per run | 3–4 per run | 3–4 per run |
| Value of push latency at that frequency | negligible | negligible | negligible |
| Held second connection per run | yes, for the whole run | no | yes |
| Reconnect protocol | required | none | required |
| Routing surface added | a streaming endpoint | one read-only GET | a hub — a second routing mechanism beside `MapRazorPages()` |
| Client runtime added | none | none | **a vendored JS client library** |
| Testing | flush loop, connection lifetime, buffering | plain request/response | hub harness |

### 10.3 Why polling

Grounded only in verified current facts:

- a run publishes **three or four transitions** in up to about a minute, so
  push latency buys almost nothing;
- the synchronous POST stays canonical and already delivers every terminal
  outcome, so the channel carries no result, no failure and no ending;
- it adds **one small read-only GET** to a routing surface that is
  `MapRazorPages()` and nothing else;
- it holds **no second connection** for the duration of a ~55 s profile run;
- it needs **no reconnect protocol** — an unanswered poll is just the next
  poll;
- it is testable as plain request/response, with no connection lifetime,
  flush behavior or proxy buffering to prove.

On SignalR specifically, the verified cost is **not** a server NuGet package:
`src/GitHubProof.Web/GitHubProof.Web.csproj` declares no `PackageReference` at
all and runs on the `Microsoft.NET.Sdk.Web` shared framework, which carries the
server-side hub support. The verified cost is (a) a **client runtime library
that would have to be vendored into `wwwroot/js`** — the repository has no
`package.json` and no node toolchain anywhere, and `wwwroot/js` is three
hand-written vanilla files, so this would be a new kind of dependency for the
project — and (b) a hub endpoint, which is a second routing mechanism. Neither
buys anything at three transitions per run.

### 10.4 Endpoint

~~~text
GET /analysis-progress?run=<token>
~~~

- Implemented as a Razor Page with a GET handler returning JSON. The
  application's entire routing surface is `MapRazorPages()`; introducing a
  second routing mechanism for one endpoint is not authorized.
- **GET only, read-only, no side effects.** It never registers a run, never
  touches the admission guard, never touches the budget circuit, never starts,
  resumes, extends or cancels any analysis, and never writes to the registry.
- Response shape, exhaustive:

~~~json
{ "known": true, "target": "repository", "active": "repository-public-data-read", "completed": ["repository-target-validated"] }
{ "known": false }
~~~

- `target` is `repository` or `profile`. `active` is one stage identity or
  `null`. `completed` is the ordered list of completed stage identities.
- The response carries **no** user-facing prose. Both stage lists are rendered
  server-side into the landing page, fully localized and in `PENDING` state,
  which keeps every string under p10 §8.2's whole-Web presentation ownership and
  keeps the endpoint free of copy.
- A **missing, malformed or non-canonical-UUID `run` value** is answered with
  **HTTP 400 and an empty body** — never `{ "known": false }`. The distinction
  is deliberate: `known:false` means "no such live run", which is an ordinary
  and expected state, whereas a malformed token means the client sent something
  it should never send. A 400 counts against the §13 transport-failure stop
  limit, not the `known:false` retry limit, so a broken client stops after 3
  attempts instead of 5. The two bodies above therefore remain the exhaustive
  set of **JSON** responses; 400 carries no body at all.
- `known:false` before any known response has been seen is the ordinary
  redirect/registration race, bounded by §13.

### 10.5 Cache policy

The response is ephemeral runtime state. A stale snapshot served as current
progress would be exactly the fake progress this contract exists to prevent —
and it would be worse than no progress, because the visitor cannot tell a
frozen snapshot from a stalled analysis.

**Every response from this endpoint must carry `Cache-Control: no-store`**, on
the `known:true` and `known:false` paths alike. There is no prior convention to
follow: `Cache-Control`, `CacheControl`, `ResponseCache` and `no-store` appear
nowhere in the codebase today, so this is a new rule rather than an application
of an existing one.

`ETag`, `Last-Modified`, `If-None-Match`, `304` and any other conditional or
revalidation semantics are **prohibited** on this endpoint. Progress is not a
resource with versions; a conditional request would add a second correctness
surface for a payload of one enum and one short list.

## 11. Percentage and ETA

**Percentage: PROHIBITED. ETA: PROHIBITED.**

No progress bar fill fraction, no `17% / 50% / 90%`, no remaining-time estimate,
no elapsed-time-derived completion figure, and no denominator of any kind may be
rendered or transported.

The evidence is direct. Repository execution cost is highly non-uniform:
Session 133 measured the same dense profile at R3 `112.209 s`, R4 `70.888 s` and
R5 `55.557 s` while the request count stayed constant at 662, so wall-clock
tracks scheduling, not work units. Stage count is not time. Repository count is
not a valid denominator (§6). Nothing in the current runtime provides a
monotonic completion metric against a provable denominator, so any figure would
be an invention.

The indeterminate activity indicator that already exists is retained. It means
exactly **"analysis is still active"** and never "this fraction is done". It
must have no determinate fill state, and it respects `prefers-reduced-motion` as
it does today.

## 12. Server state

One process-local singleton registry. Per live run, exactly:

| Field | Why it is needed | Notes |
|---|---|---|
| correlation token | the registry key | canonical UUID form only |
| target kind | selects which stage list the client reveals | the client cannot know it — classification is server-side |
| current stage | the one `ACTIVE` stage identity | from the fixed set in §4/§5 |
| completed stages | the ordered completed identities | derivable from stage order; stored or derived, not extended |
| last-updated instant | TTL eviction only | never transported, never rendered |

**Explicitly not stored and not transported:** the submitted target string, the
login, any repository name, any URL, availability status or reason, any count,
any provider detail, any error or exception text, any timing or duration, any
per-stage timestamp, the analysis result, and any evidence. Per-stage timestamps
are excluded deliberately: they have no consumer and would invite exactly the
duration math §11 prohibits.

Lifetime and limits:

- Memory only. Not persisted, not shared between instances, not durable.
- Released in a `finally` when the run ends (§9). The **absolute TTL is
  10 minutes**, measured from the last update, and is a backstop only: in normal
  operation the `finally` always wins. If it ever evicts a live run, the poll
  answers `known:false` and the client stops polling per §9, keeping its last
  snapshot — it does not fall back to the indeterminate state, because something
  was already known. It is chosen far above every observed run
  — the dense profile measured about 56 s — and small enough that an entry
  leaked by an anomaly cannot linger. It is deliberately **not** justified by any
  per-request transport timeout: that timeout bounds one of a dense profile's
  hundreds of requests, not the run, and `ProfileExecutionDeadlineOptions.Timeout`
  defaults to null, so no execution deadline caps a run in the default
  configuration and Repository runs have neither a deadline nor an admission
  guard. If a run ever outlives the TTL, the consequence is benign and already
  covered: the poll answers `known:false`, the client stops polling per §9, and
  the analysis continues untouched.
- The **maximum is 64 concurrent entries**. Profile global concurrency is 1
  today, and Repository Proof has no admission guard, so the cap exists for
  concurrent repository runs on a single-instance beta; each entry is a handful
  of fields, so 64 is trivially bounded. Exceeding it refuses **registration
  only**: that analysis runs normally with no progress. Registry pressure may
  never fail, delay, degrade or alter an analysis.
- A stored snapshot is immutable and replaced atomically (§7.1).
- **What is lost on restart or on a second instance:** the run's progress state,
  and nothing else. The poll answers `known:false` — the client falls back to
  the indeterminate state if nothing was ever known, and otherwise stops polling
  and keeps its last snapshot (§15) — and the analysis and its result are
  unaffected.
  Accepting that loss is **this contract's own decision**, taken because
  `public-profile-runtime-collection.md` §10.1.1 defers durable jobs, durable
  queues and distributed workers past Milestone B, and because a durable run
  store is exactly the deferred mechanism. No other document records a
  single-instance beta target, and this one does not create one.

## 13. Update and overhead policy

Every value below is **exact and locked**. Session 139 selects none of them.

| Value | Locked | Why this number |
|---|---|---|
| Poll interval | **2000 ms**, constant | a run has 3–4 transitions; at 2 s a ~55 s dense profile costs ~28 GETs, each an O(1) read. No backoff, no acceleration, no adaptive interval — an interval derived from elapsed time would be inferring progress from time |
| First poll | immediately after submit, then every 2000 ms | so the target kind, and the first stage that genuinely occupies wall-clock, appear at ~0 s instead of ~2 s. It is **not** justified by making stage 1 observable as `ACTIVE` — see §4 |
| Initial `known:false` retry limit | **5** consecutive, before any known response | covers the `/` → 307 → `/profile` re-post and registration; classification is synchronous, so ~10 s is already generous. On the 5th, stop silently — no error, no retry storm |
| Consecutive transport-failure stop limit | **3** | non-200, network error or malformed JSON. After 3 in a row, stop and keep the last snapshot. A fourth attempt would not tell the visitor anything the POST is not about to tell them |
| Registry absolute TTL | **10 minutes** (§12) | backstop only |
| Registry maximum entries | **64** (§12) | single-instance beta bound |

- Polling also stops on `known:false` **after** a known response (the run ended)
  and when the page is replaced.
- Polling pauses while the document is hidden and resumes on visibility. The
  interval never compounds while hidden.
- No elapsed-time value may drive the poll interval, the stage display, or any
  inference about how far along a run is.
- Progress publication is **coarse-grained**: at most one write per stage
  transition, so at most four writes per run. No per-repository, per-collector
  or per-provider-request publication is authorized (§6).
- The progress mechanism emits **nothing** to logs, metrics or
  `ProfileExecutionTelemetry`, and adds no telemetry cardinality.
- It must not measurably worsen analysis runtime. It introduces no lock the
  analysis path contends on beyond a single bounded registry write per stage.

### 13.1 Configuration ownership

**None of the six values above is configurable.** All are constants owned by
`GitHubProof.Web` — the poll interval, the initial `known:false` retry limit and
the transport-failure stop limit as constants in `analysis-progress.js`, and the
TTL and entry cap as constants on the registry type. No options class, no
configuration section, no key and no default binding is authorized for them.

This is deliberate. R and U earned configuration because they were selected from
measurement and are deployment-tuning knobs. These six are not: exceeding the
registry cap or the TTL degrades **only** the progress enhancement and never an
analysis, so there is no operational situation in which an operator must change
one under load. Promoting any of them to configuration later is an owner
decision, not an implementation one.

## 14. Cancellation

**Explicit cancellation: NOT AUTHORIZED. No cancel control may be rendered.**

What already exists and is unchanged: abandoning the request — refresh, back,
tab close, navigation — aborts the connection, and ASP.NET Core surfaces that as
the handler's `CancellationToken`. `public-profile-runtime-collection.md` §11.1
governs what happens next, and it stays exactly as it is: the cancellation
propagates, and no analysis, pipeline result, partial profile or failure profile
is fabricated from it.

An explicit cancel button would require a **write** channel able to abort an
in-flight run from a different request. That is new runtime semantics: a
cancellation registry, a mutating endpoint, an abuse surface where any token
holder can terminate a run, and an interaction with the bounded repository
worker pool. It is recorded here as a future capability requiring its own owner
decision. Rendering a control that does not reliably do this would be a fake
affordance and is prohibited.

## 15. Refresh, navigation and reconnect

| Situation | Behavior | Change required |
|---|---|---|
| refresh while active | the connection aborts, server work cancels per `public-profile-runtime-collection.md` §11.1; the fresh GET shows the landing page | none |
| refresh of `/profile` | `OnGet` already redirects to `/`; a GET can never start an analysis | none |
| browser back / BFCache restore | the existing `pageshow` reset runs; it must additionally reset every stage to `PENDING` and re-hide the region | client reset extended |
| tab close | connection aborts, work cancels | none |
| duplicate submit, same tab | already blocked by form state | none |
| duplicate submit, another tab, same profile | the admission guard answers `SameProfileBusy` (409) with the existing page | none |
| progress transport disconnect | the client stops polling and keeps its last snapshot; no error, no reset | per §9 |
| progress unknown from the start | bounded retries, then silent fallback to the indeterminate state | per §10 |
| `known:false` **after** a known response, run still active — TTL eviction, an instance flip | the client stops polling and **keeps its last snapshot**. No reset, no error, and the stage list is **not** hidden: the stages already reported `COMPLETED` were true when reported, and the run is still working | per §9 |

The last two rows are deliberately different. Falling back to the canonical
indeterminate state means hiding the stage list, because a stage list with no
server state behind it would be client-invented state — exactly what §7 forbids.
That is correct only when **nothing** was ever known. Once a snapshot has been
shown, the stages already reported are server-reported facts, so hiding them
would erase true information and read as a reset, which §9 forbids.

A progress poll may never start, resume or duplicate an analysis. This is
structural, not a rule the endpoint has to remember: the endpoint has no
registration or analysis path at all.

## 16. Security and isolation

- The endpoint requires no authentication, because it exposes nothing that
  needs any. Its entire vocabulary is a target kind and stage identities drawn
  from the fixed sets in §4 and §5.
- A guessed or shared token therefore reveals only that *some* analysis of a
  given kind is at a given stage. No profile, repository, URL, login, count,
  provider state or evidence is reachable through it (§12).
- Token entropy is a full random UUID; malformed tokens are rejected without
  allocation; unknown tokens produce `known:false` and create nothing.
- Enumeration cannot grow memory. Only a page model on the analysis path
  registers — never the observation endpoint — and since §8 places registration
  before the admission lease, a run rejected at admission also holds a slot
  briefly and is released on the same `finally` path as any other ending (§9).
  The bound is §12's cap, which no request can raise.
- No run state is shared between visitors, and no listing, enumeration or
  "current run" endpoint is authorized.
- GitHub Proof analyzes public targets only; this contract does not assume,
  introduce or depend on authentication.

## 17. Non-goals

Not authorized by this contract, in any form:

- background jobs, queues, workers, schedulers or any detachment of analysis
  from its request;
- persistence, caching or reuse of run state or analysis results;
- SSE, WebSocket, SignalR, long-polling or HTTP streaming;
- a run-creation endpoint, a result-retrieval endpoint or any second POST;
- percentages, ETAs, denominators, counts or durations (§11);
- `ETag`, `Last-Modified`, conditional requests or any cacheable progress
  response (§10.5);
- adaptive, backing-off or elapsed-time-derived poll intervals (§13);
- configuration for any of the six locked runtime values (§13.1);
- a cancel control (§14);
- a modal or full-screen overlay (p10 §8.3);
- per-repository or per-request events (§6, §13);
- new stage identities beyond §4 and §5;
- any Domain, Application or Infrastructure change (§3);
- any change to result delivery, failure surfaces, status codes, admission,
  budget protection, the execution deadline or `ProfileExecutionTelemetry`.

## 18. Deferred owner decisions

Recorded, not authorized:

1. A `profile-identity-read` stage, completed when profile availability
   resolves. Requires a publication point on `GitHubProfileSnapshotProgress`
   (Infrastructure).
2. Splitting `profile-evidence-examined` into repository-evidence and
   recurring-trace phases. Requires a progress seam inside `ProfilePipeline`
   (Application), and interface seams for the composer and the two policies.
3. Explicit user cancellation (§14).
4. Any behavior that must survive a process restart or a second application
   instance — that is where a durable run store begins, and
   `public-profile-runtime-collection.md` §10.1.1 defers durable jobs, durable
   queues and distributed workers past Milestone B.

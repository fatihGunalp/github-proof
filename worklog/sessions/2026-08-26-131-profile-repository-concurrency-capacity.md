# Profile Repository Concurrency and Capacity Decision

**Session:** 131
**Date:** 2026-08-26
**Route:** Opus / high
**Result:** **R2-PERFORMANCE-INSUFFICIENT** (architecture-owner verdict).

Two layers, and they disagree:

~~~text
TECHNICAL : R2-U2-PASS
PRODUCT   : PERFORMANCE-INSUFFICIENT
OVERALL   : PERFORMANCE-INSUFFICIENT
~~~

Bounded repository concurrency R=2 cuts dense-profile wall clock almost exactly
in half (254.634 s → 127.667 s, 1.99×) with an **identical §6 profile-level
fingerprint**, deterministic per-repository parity proven separately by test,
and **exactly the same 662 requests**; U2/R2 runs two different profiles
simultaneously with isolated telemetry and no provider distress. All of that
holds.

**It is still not a product result.** The architecture owner's dense-profile
user-facing latency gate is **≤ 60 seconds**, and 127.667 s fails it. Being
49.9% faster than a 254-second baseline does not make a two-minute wait
acceptable. The seam ships **DEFAULT OFF (R=1)** and this session enables
nothing and approves nothing.

---

## Baseline

~~~text
Branch : session/131-profile-repository-concurrency-capacity
Base   : cc76c75  (== main == origin/main, verified before any mutation)
Pre-existing working tree: ?? artifacts/  (user-owned, untouched)
~~~

Session 130 is **integrated as `cc76c75`**.

---

## Goal

One bounded performance + capacity decision:

- **R** = repository concurrency inside one profile analysis
- **U** = simultaneously executing different profile analyses

Current production shape is **U1 / R1**. Answer whether **U1/R2** materially
reduces dense-profile wall clock with exact semantic parity, and whether
**U2/R2** is safe for the provider and for execution isolation. Stop at
U2/R2 — do not explore R3/U3.

---

## Context read

~~~text
AGENTS.md, CLAUDE.md
agents/02-context-loading.md, agents/04-development-workflow.md
docs/architecture/public-profile-runtime-collection.md
  §6 semantic-equivalence invariant
  §7 no hidden cap
  §8 request amplification vs latency  (concurrency precondition)
  §9 / §9.1 telemetry contract and prohibitions
  §10 / §10.1 execution guard + concurrent execution policy
  §10.4 / §10.5 provider protection, provisional concurrency numbers
  §11 / §11.1 / §11.3 failure, cancellation, budget rejection
~~~

---

## Canonical position established before implementing

**§8 authorizes this work, and its precondition is already met.** §8 says
"Bounded concurrency may reduce elapsed time later" but requires that
"unnecessary requests are removed first, and measured (§9), before any
concurrency work is considered." Session 122 audited exactly that and returned
**REST-R3 — no material redundancy**, proven removable set **0** on all three
measured shapes; §9 telemetry has measured the surface in Sessions 120 and
130. The precondition is satisfied, so bounded concurrency is now legitimately
on the table.

**§8 also fixes the shape of an acceptable implementation:** "broad
`Task.WhenAll` fan-out over repositories or collectors is **not** authorized."
So the seam must be explicitly bounded, not a fan-out.

**§10.5 makes U a configuration question, not a canonical one.** It records
"initial global Profile concurrency `1`, same-profile concurrency `1` and
queue `0` remain **provisional implementation recommendations**." Changing
them is changing provisional implementation values, not canonical policy.
§10.1 records the requirement for a bounded concurrent-execution policy and
explicitly does not authorize queueing or background-job infrastructure —
which matches this session's own prohibition.

**No canonical amendment is required for this session.**

---

## Pre-implementation findings

- `ProfileRepositoryAnalysisOrchestrator.AnalyzeAsync` is the sequential
  `foreach + await`. Result ordering today comes from manifest iteration
  order, so any concurrency must write results by index rather than append on
  completion.
- `AnalysisAdmissionPolicy.Evaluate` is **pure** — it reads only immutable
  `ProfileRepository` condition state and immutable policy fields, performs no
  I/O. Admission can therefore be resolved in a sequential first pass, leaving
  only the I/O-bound `Analyze` decisions to run concurrently. Admission
  outcomes and their order are then structurally identical to today.
- `ProfileExecutionTelemetryScope` and `ProfileGitHubBudgetScope` are both
  **AsyncLocal**, documented as flowing into every awaited call inside one
  execution while staying invisible to any other concurrent execution — the R
  and U isolation this session needs already exists by design.
- `ProfileExecutionTelemetry` is already thread-safe (`Interlocked`,
  `ConcurrentDictionary`, explicit gates); `ProfileGitHubBudgetState` uses
  `Volatile.Read`/`Interlocked.Exchange`.
- **U needs no code change at all.** `ProfileAdmissionOptions.GlobalConcurrency`
  and `SameProfileConcurrency` are already externally configurable under
  `ProfileGuardrails:Admission`.
- The orchestrator has exactly one production construction site
  (`ProfilePipelineServiceCollectionExtensions.cs:204`) and an optional
  trailing constructor parameter already, so a second optional parameter keeps
  every existing call site compiling.

---

## What was built

**U needed no code at all.** `ProfileAdmissionOptions.GlobalConcurrency` /
`SameProfileConcurrency` are already externally configurable under
`ProfileGuardrails:Admission`, and `ProfileExecutionAdmissionGuard` already
linearizes both decisions under one lock. U1 → U2 is a configuration change to
values §10.5 itself calls provisional.

**R needed one bounded seam**, in
`ProfileRepositoryAnalysisOrchestrator`:

- one new optional constructor argument, `maxRepositoryConcurrency`, defaulting
  to `DefaultMaxRepositoryConcurrency = 1`;
- **R=1 executes the original sequential method verbatim**, so the default path
  is not merely equivalent, it is the same code;
- R>1 runs a three-phase path:
  1. admission resolved **first, sequentially, in manifest order** —
     `AnalysisAdmissionPolicy.Evaluate` is pure (immutable repository condition
     state, immutable policy fields, no I/O), so hoisting it cannot change any
     admission outcome or its order;
  2. only the I/O-bound `Analyze` decisions run concurrently, through **a fixed
     pool of R workers pulling from one shared queue**, each writing to its
     **own pre-assigned index**. Deliberately not one task per repository:
     §8 prohibits "broad `Task.WhenAll` fan-out over repositories" by name, and
     a worker pool fans out over R — the bound itself — so no interpretation of
     that prohibition is required (see the closure review below, M-1);
  3. results assembled by index, so ordering follows the manifest and is
     independent of completion order. Workers pull in ascending order, so
     collection also *starts* in manifest order; nothing reads completion
     order.

Externally configurable through
`ProfileGuardrails:Collection:MaxRepositoryConcurrency`
(`ProfileCollectionOptions`), deliberately a **separate** options class from
the admission options so R and U cannot be confused for one another.
Repository Proof is untouched; no queue, no background job, no scheduler.

---

## Measurements — real provider, GraphQL DEFAULT OFF throughout

The numbers below are the **second** measurement round, re-run deliberately
after the closure review changed the concurrent path's shape (semaphore →
worker pool), so every figure describes **the code that would actually ship**
rather than an equivalent earlier shape. The first round produced the same
structure (2.04×, 662 requests, IDENTICAL parity) and is not reproduced here.

| Scenario | Profile | R | U | Elapsed | Requests | Rate-limit delta (core) | Semantic parity | Provider safety |
|---|---|---|---|---|---|---|---|---|
| S1 baseline | karpathy | 1 | 1 | **254.634 s** | 662 | 2884 → 2224 | baseline | clean |
| S2 | karpathy | 2 | 1 | **127.667 s** | 662 | 2223 → 1563 | **IDENTICAL** to S1 | clean |
| S3 | karpathy | 2 | 2 | **125.764 s** | 662 | 1562 → **773** | **IDENTICAL** to S1 | clean |
| S3 | torvalds | 2 | 2 | **29.909 s** | 130 | 1561 → 1286 | n/a (U2 partner) | clean |

Every scenario: `RepositoriesCompleted` equal to admitted,
`RepositoriesIncomplete = 0`, `UnrecordedRateLimitObservations = 0`, zero
GraphQL requests.

S3's two executions overlapped: **combined wall clock 125.765 s**, i.e. the
130-request profile completed entirely inside the dense profile's window and
cost no additional wall time.

**Q1 — does R2 materially reduce dense wall clock with exact semantics?**
Yes. **254.634 s → 127.667 s, a 1.99× reduction (−49.9%)**, with the live
semantic fingerprint IDENTICAL.

The mechanism is visible in the telemetry and is genuinely overlap, not a
measurement artifact: the *sum* of per-repository collection time stayed in the
same band while wall clock halved — same work, overlapped. Repository analysis
CPU stayed negligible across 54 repositories, confirming Session 120's finding
that this is I/O wait, not compute.

A second, independent confirmation that the overlap is real came from the
telemetry fix this session made: `PipelineOutsideRepositoriesMs` reports a
real duration (`53`) at R=1 and `undefined` at R=2, precisely because the
per-repository intervals stop being disjoint once they overlap.

**Request count did not change at all: 662 in every karpathy scenario**, with
an identical per-category breakdown. This is exactly §8's separation holding in
practice — concurrency changed *when* requests were issued, never *how many*.

**Q2 — can two different profiles execute simultaneously without corruption?**
Yes, and the isolation was already designed in rather than added here.
`ProfileExecutionTelemetryScope` and `ProfileGitHubBudgetScope` are both
`AsyncLocal`, documented as flowing into every awaited call inside one
execution while staying invisible to any other. Observed: the two executions
recorded **662** and **130** requests separately with no cross-contamination,
each produced its own `Complete`/`composed` outcome, each kept its own
discovered/admitted/completed counts, and the dense profile's fingerprint under
U2/R2 was IDENTICAL to its solo R1 run.

**Q3 — real provider pressure.**

| | S1 (U1/R1) | S2 (U1/R2) | S3 (U2/R2, both) |
|---|---|---|---|
| requests | 662 | 662 | 792 |
| elapsed | 254.634 s | 127.667 s | 125.765 s |
| request rate | ~2.6 /s (~156 /min) | ~5.2 /s (~311 /min) | ~6.3 /s (~378 /min) |
| max in-flight requests | 1 | 2 | 4 |
| `x-ratelimit-resource` | `core` | `core` | `core` |
| `x-ratelimit-limit` | 5000 | 5000 | 5000 |
| `x-ratelimit-remaining` | 2884 → 2224 | 2223 → 1563 | 1562 → **773** |
| `x-ratelimit-used` | 2116 → 2776 | 2777 → 3437 | 3438 → 4227 |
| `Retry-After` observed | none | none | none |
| primary limit event | none | none | none |
| secondary limit event | none | none | none |
| GraphQL requests | **0** | **0** | **0** |

`UnrecordedRateLimitObservations = 0` in every run, so no rate-limit context
was dropped.

**Q4 — is the primary budget sufficient?** For one round of these shapes, yes.
But this session consumed the budget faster than expected and the honest floor
is lower than a first reading suggested:

- this measurement round cost **2111** requests by observed `x-ratelimit-used`
  delta (4227 − 2116); the nominal expectation from the per-run counts is
  662 × 3 + 130 = 2116, and the 5-request gap between nominal and observed is
  recorded as-is rather than reconciled;
- it ran in a window that **already carried 2116 used** from the earlier round,
  so the lowest observed `x-ratelimit-remaining` across the session was
  **773 / 5000** — not comfortable headroom;
- one dense profile costs **662**, a medium **130**, a small **14**.

**A pre-flight `/rate_limit` reading is not a safe basis for that judgement,
and this session has direct evidence.** Immediately before this round,
`GET /rate_limit` reported `core: limit 5000, used 0, remaining 5000`. The very
first real response header in the run that followed reported
`remaining 2884, used 2116` — exactly the previous round's request count. The
two disagreed, and **the in-run response headers are the authoritative
number**: the earlier usage had not actually reset. No explanation for the
discrepancy is asserted here; the observation is recorded because it directly
affects the §10.4 reserve design, which is a Production Runtime Decisions
question.

**Q5 — secondary-limit compatibility.** No secondary rate limit and no
`Retry-After` occurred naturally at any point. The measured burst shape is
modest: **at most 4 concurrent requests** under U2/R2 (2 profiles × R2), and
~378 requests/minute at peak. Both are far below the concurrency and
per-minute pressure GitHub's secondary-limit guidance is concerned with. This
is an **observation across these runs, not a guarantee** — the Session 126
residual (a secondary limit identifiable only through an error message) is
untouched by this session and remains its own open item.

**Q6 — reserve/admission input for the Production Runtime Decisions session.**
Measured facts only; **no reserve floor is selected here**, and none is
justified by these measurements alone:

- one dense profile = **662** requests; medium = **130**; small = **14**;
- U2 admits two executions, so the worst *measured* pair cost **792**, and a
  worst *plausible* pair (two dense) would cost **~1324** inside one window;
- R does **not** change request cost at all, so raising R has **zero** effect
  on any reserve calculation — it only compresses the same cost into a shorter
  interval, which raises the per-minute rate — **156 → 311 /min** on the dense
  profile going from R1 to R2. (The ~378 /min peak belongs to the **U2 pair**
  and is a U effect, not R's.);
- therefore the reserve question is governed by **U and by profile shape**, not
  by R. That separation is the main input this session hands forward;
- and, from the `/rate_limit` discrepancy above, **the reserve circuit should
  read the live response headers it already observes rather than a pre-flight
  `/rate_limit` probe** — §10.4's `ProfileGitHubBudgetState` already works that
  way, and this session found a concrete reason not to change that.

---

## Semantic invariants

**Deterministic proof (canonical determinism — same captured inputs, same
versions).** `ProfileRepositoryConcurrencyTests` (12 tests) compares R1 against
R2 and R3 over a fixture containing every per-repository outcome the
orchestrator can produce — analyzed, analysis-failed, collection-failed,
collection-unavailable, not-admitted and admission-undetermined — and asserts
the flattened per-repository description (reference, outcome, admission
decision/reason, analysis status, normalized score, all 15 signals with status
and score, collection availability, failure reason) is equal.

**Live corroboration.** The fingerprint compared across S1/S2/S3 covers
analysis status, composition status, every repository's eligibility status,
admission decision/reason, archived/activity/independence, every repeated
pattern with its supporting and independent supporting repositories, every
representative repository with its pattern supports, every limitation and
warning with scope and affected repositories, every `WhyThisConclusion` entry
with its evidence references, and every `RepositoryDrillDown` row. IDENTICAL in
both comparisons.

**Ordering has two layers, and the session found the second one by test.**
`ProfileRepositoryManifest` already sorts by `ProfileRepositoryReference`
(owner, then name, ordinal), so manifest order is canonical rather than
input order; the orchestrator then preserves that index regardless of
completion order. Pinned by a test that makes the first repository finish last
and asserts the result order is unchanged while the recorded completion order
really did invert.

Also pinned: bounded-not-fan-out (24 admitted repositories at R=2 show exactly
2 in flight, never 24), R=1 never overlaps, R=2 genuinely does overlap,
cancellation propagates out of the concurrent path, per-repository failure
isolation survives concurrency, and R<1 is rejected.

**No cap, sampling, top-N, evidence omission or approximation was introduced**
(§7). Every admitted repository is still collected and analyzed in full; only
the scheduling changed.

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror   0 warnings, 0 errors
Domain          842/842
Application     274/274   (+12 concurrency tests)
Infrastructure  636/636
Web             560/560   (+7 wiring tests, +1 opt-in measurement harness)
TOTAL          2312/2312  (baseline 2292, +20)
git diff --check                              clean
~~~

The measurement harness is opt-in behind the Session 120 gate plus its own two
profile variables, and takes no network time when unset.

---

## Independent closure review

**PASS WITH FINDINGS** — 0 blocker, 4 MEDIUM, 4 LOW. Every finding was closed
in-session; the two that mattered are recorded here because both changed the
shipped code rather than only the record.

**M-1 (MEDIUM) — §8 fan-out prohibition was being interpreted by the
implementation in its own favour.** The first implementation created one task
per admitted repository and awaited `Task.WhenAll` over them, bounded by a
`SemaphoreSlim` permit — and defended that in a code comment ("a bounded
permit, never a fan-out") while the worklog asserted no canonical amendment was
needed. The reviewer's judgement, which is correct: the *intent* reading
supports it (provider pressure genuinely was bounded, and the measurement
proves it) but the *text* reading does not, because §8 prohibits
"broad `Task.WhenAll` fan-out over repositories" **by name**. Having the
implementation rule on the scope of a canonical prohibition is exactly what
`AGENTS.md` §6 forbids.

Closed by **changing the code rather than asking for a favourable reading**:
the concurrent path is now a fixed pool of R workers pulling from one shared
queue, so the fan-out is over R — the bound itself — and no interpretive
argument is required at all. The reviewer had also noted the worker pool is
strictly better on allocation for large profiles, so this cost nothing.

**M-2 (MEDIUM) — a telemetry field silently became meaningless under R>1.**
`PipelineWorkOutsideRepositoriesElapsed` is derived as
`PipelineElapsed − collectionTotal − analysisTotal`, and its own comment
justified the subtraction by the fact that "the Profile path introduces no
concurrency at all". This session made that premise conditionally false: under
R>1 the per-repository intervals overlap, their sum exceeds wall clock, and the
old clamp made the field read a flat `0` — which reads as "no pipeline work
outside repositories". Since §9.2 exists so production guard values are chosen
from measurement, a silently-wrong measurement is a real defect. Now returns
**null**, rendered `PipelineOutsideRepositoriesMs=undefined`. Visible working
in the final measurement: `53` at R=1, `undefined` at R=2.

**M-3, M-4 and the four LOW findings** were an evidence-strength overstatement
in this record ("byte-identical" for what is a profile-level fingerprint that
deliberately excludes the per-repository half proven by test), the missing
`CURRENT`/`INDEX` update, two stale comments, a fingerprint-scope comment, a
missing composition-root test (the wiring tests would all have stayed green if
the argument passing R into the orchestrator were deleted — now covered by two
behavioural tests), and two timing-brittle exact-equality assertions (now a
forced-overlap gate and an upper-bound assertion that cannot flake).

**The measurement was then re-run in full against the final worker-pool
shape**, so no number in this record describes code that was subsequently
changed.

---

## Stop rule

The technical concurrency experiment succeeded, but the product latency gate did
not.

- **TECHNICAL:** R2/U2 PASS for the measured scenarios.
- **SEMANTIC PARITY:** PASS.
- **PROVIDER OBSERVATION:** PASS for the measured dense + medium U2/R2 run.
- **PRODUCT LATENCY:** FAIL — the architecture-owner dense-profile gate is
  **<= 60 seconds**, while R2 measured **127.667 seconds**.

Therefore the overall Session 131 verdict is:

**R2-PERFORMANCE-INSUFFICIENT**

R2 is a valid and semantically safe concurrency seam, but it is not sufficient
to close the Milestone B user-facing latency problem.

Directional planning inference only — **NOT MEASURED, NOT A BENCHMARK, NOT AN
APPROVED CONFIGURATION** — from the near-linear R1 -> R2 observation:

~~~text
R3 ideal projection ~= 85 s
R4 ideal projection ~= 64 s
R5 ideal projection ~= 51 s
~~~

These projections do not authorize R3/R4/R5. Request count remained **662**, so
trying to reach about 60 seconds by concurrency alone would compress roughly
the same 662 REST requests into about one minute for one dense profile. With
concurrent dense users, provider request-rate protection therefore becomes a
first-class architecture question.

R3, R4, R5 and U3 were **not tested** and are **not approved**. No further live
measurement belongs to this session.

---

## Not done / not touched

- **No production enablement.** R ships **DEFAULT OFF (R=1)**; U defaults stay
  `GlobalConcurrency=1` / `SameProfileConcurrency=1`. No configuration file was
  changed.
- **No canonical change.** §8 already authorized bounded concurrency once
  unnecessary requests were removed and measured (Session 122 REST-R3 +
  Sessions 120/130 telemetry); §10.5 already calls the concurrency numbers
  provisional.
- **No reserve floor selected**, no execution deadline, no production runtime
  value.
- **No GraphQL work**: envelope stays default off, zero authoritative GraphQL
  requests in every scenario; no S4, no batching, no aliases, no
  DecisionBounded GraphQL, no Session 126 fix, no live A/B.
- **No queue, background job, scheduler or distributed coordination.**
- **Repository Proof untouched.**
- `artifacts/` untouched. No commit, no push, no deploy.

---

## Known risks

| Risk | State |
|---|---|
| Secondary rate limit carried as a message-only signal | Session 126 residual, **unchanged and untouched**. Not provoked here and not observed, but R/U raise request *rate*, so this stays the relevant unknown for any future R/U increase |
| U2 with **two dense** profiles | Not measured. The brief prohibited using two dense profiles as an artificial stress test, so the ~1324-request pair is a projection from measured per-profile cost, **not an observation** |
| Wall-clock figures | One measurement per scenario per round, on one network. The ~2× is large, reproduced across **both** measurement rounds, and structurally explained (I/O overlap, identical request count) — but it is not a production latency guarantee (§18.11 discipline) |
| Primary-budget headroom | Two measurement rounds in one window left the lowest observed `remaining` at **773/5000**. Measurement is expensive at these shapes; a future session doing the same should plan the window rather than assume a reset |
| `/rate_limit` pre-check reliability | Reported `5000/5000 used=0` while the very next in-run header reported `used=2116`. Unexplained, recorded, and directly relevant to the §10.4 reserve design |
| R>2 | Deliberately untested |

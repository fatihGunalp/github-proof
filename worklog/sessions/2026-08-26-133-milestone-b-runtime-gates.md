# Milestone B Runtime Gates — Dense Latency, Launch Burst, Capacity UX

**Session:** 133
**Date:** 2026-08-26
**Result:** PASS — all three bounded Session 133 runtime gates demonstrated.
R5/U1 is the first measured dense candidate at `<= 60 s` (`55.557 s`), with
662 requests, max 5 in-flight HTTP, semantic parity, no incomplete repository,
no `Retry-After` and no primary/provider protection event. Production
configuration remains unchanged. Independent closure review is **PASS WITH
FINDINGS** with **NONE** critical; R5 is **ACCEPTED** as the Milestone B
production-candidate repository concurrency and Session 133 closure is **PASS**.
**Milestone B closure is NOT CLAIMED.**
**Scope:** application / infrastructure / web / validation / worklog
**Model Route:** Sol / high
**Model Gate:** APPROVED — supplied task explicitly records `USER APPROVAL: devam et`

---

## Goal

Close, with the smallest safe implementation and evidence available, the three
Milestone B runtime gates: designated dense-profile latency `<= 60 s`,
controlled handling of 10/50/100 simultaneous arrivals, and explicit
processing/capacity presentation without changing profile truth.

---

## Starting State

~~~text
Branch:
session/133-milestone-b-runtime-gates

Base HEAD:
37b8ec2

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-26-131-profile-repository-concurrency-capacity.md
~~~

`main`, `origin/main` and `HEAD` were all verified at
`37b8ec2ec9eb039985b4024a8d45d404ce09aa4e` before the branch was created.
`artifacts/` is user-owned and out of scope.

---

## Model Route

~~~text
Selected:
Sol / high

Why:
The task combines bounded concurrency, provider-budget safety, deterministic
burst admission, live performance measurement and cross-layer Web behaviour.

User Approval:
devam et (recorded explicitly in the supplied task)
~~~

---

## Model Escalation

None.

---

## Context Read

- `AGENTS.md`
- `agents/01-model-routing.md`
- `agents/02-context-loading.md`
- `agents/03-project-contract.md`
- `agents/04-development-workflow.md`
- `agents/06-review-and-validation.md`
- `agents/07-worklog-integration.md`
- `worklog/CURRENT.md`
- `worklog/INDEX.md`
- `worklog/sessions/TEMPLATE.md`
- `worklog/sessions/2026-08-26-131-profile-repository-concurrency-capacity.md`
- `docs/architecture/public-profile-runtime-collection.md` — runtime,
  admission, telemetry, provider guard and GraphQL boundaries
- `docs/roadmap/deployment-milestones.md` — Milestone B and release gate
- `docs/roadmap/mvp-phases.md` — Milestone B clarification and P10 boundary

---

## Contract / Invariants Checked

- Dense production-candidate scenario must complete end to end in `<= 60 s`.
- 10/50/100 are arrival counts, not active-analysis concurrency requirements.
- Active profile execution remains bounded; excess arrivals receive an
  explicit retry/capacity outcome and never wait indefinitely.
- Live response headers, not a `/rate_limit` preflight, own primary-budget
  operational state; `Retry-After` and secondary protection remain bounded.
- Operational failures and capacity state never become evidence, warnings,
  limitations, scores, patterns or profile truth.
- No sampling, hidden top-N, evidence omission, approximate semantics, GraphQL
  shortcut, queue/cache/auth/distributed infrastructure or new dependency.

---

## Changes

Created:

- `ProfileLaunchBurstTests.cs`: deterministic 10/50/100 simultaneous-arrival
  harness with one bounded fake downstream analysis, exact accepted/capacity
  counts and finite completion assertions.
- `ProfileMilestoneBRuntimeGateHarness.cs`: opt-in R3 -> R4 -> R5 production
  composition measurement that stops at the first `<= 60 s` candidate, uses a
  caller-supplied production budget floor on the real metadata response header,
  records max in-flight HTTP / reset / `Retry-After` / primary-limit state, and
  keeps GraphQL default-off.
  During the live run its single-`core`-window reporting assumption failed on
  two distinct reset windows; the test-only formatter now preserves every
  bounded core window and a regression test locks that shape.

Modified:

- `tests/GitHubProof.Application.Tests/Profile/ProfileRepositoryConcurrencyTests.cs`:
  the captured-input parity theory now also runs `R4` and `R5` directly against
  the sequential baseline, and its per-repository fingerprint was widened to the
  full semantic surface (see Validation). This file is the repository-level
  semantic anchor the R5 candidate rests on.
- `tests/GitHubProof.Web.Tests/ProfileRuntimeBenchmarkHarness.cs`: its existing
  production-composition factory can now receive the same external budget-floor
  configuration and an observation-only test handler.
- `tests/GitHubProof.Web.Tests/ProfileConcurrencyMeasurementHarness.cs`: exposed
  the existing profile-level fingerprint internally so the new harness reuses it
  instead of duplicating semantic parity logic.

Three of the changes above and inside the two created files are
review-driven proof-strengthening rather than original implementation; they are
itemized in `## Independent Review` below.

No production source, canonical docs, dependency or production configuration
changed. R remains default `1`; U remains default `1`. Every Session 133 change
is confined to `tests/` and `worklog/`.

---

## Decisions

- Existing fail-fast admission is the smallest correct launch-burst mechanism;
  no queue or background infrastructure is needed for Milestone B.
- Production capacity is tested at U=1: one admitted analysis, all excess
  arrivals explicitly `GlobalCapacityBusy`. Existing Web mapping remains `503`
  with retry copy; same-profile remains distinct at `409`.
- Existing submit UI already exposes accepted/processing state, while typed Web
  paths already distinguish capacity, provider/budget interruption, deadline,
  provider rate-limit and terminal success/failure. No presentation change was
  justified.
- R5 is the proposed Milestone B production-candidate repository concurrency:
  it is the first measured candidate meeting latency, parity, provider-safety
  and request-count gates. Production configuration is deliberately unchanged.
- R3 and R4 remain measured failures of the `<= 60 s` product gate even though
  both completed safely and with semantic parity.

---

## Validation

~~~text
Targeted Web gate set
PASS — 61/61
      launch burst, admission, ProfileModel capacity mapping,
      processing contract, provider budget protection, R wiring

Pre-live launch/runtime gate harness set
PASS — 5/5

Authenticated dense gate
PASS — 1/1; R3 -> R4 -> R5 stopped at first <=60 candidate (R5)

Post-live multi-window regression validation
PASS — 2/2 targeted tests

FINAL AUTHORITATIVE CURRENT-TREE VALIDATION

dotnet build GitHubProof.slnx -warnaserror
PASS — 0 warnings, 0 errors

dotnet test GitHubProof.slnx --no-build
PASS — Domain 842/842
       Application 276/276
       Infrastructure 636/636
       Web 568/568
       TOTAL 2322/2322

git diff --check
PASS — no whitespace errors (Git emitted only existing LF->CRLF notices)

HISTORICAL — superseded, retained for traceability only

dotnet test GitHubProof.slnx --no-build --no-restore
(intermediate run, before the live reporter fix and before the review-driven
R4/R5 parity coverage was added — NOT the final state)
PASS — Domain 842/842
       Application 274/274
       Infrastructure 636/636
       Web 565/565
       TOTAL 2317/2317
~~~

The 2317 -> 2322 delta is fully accounted for: `Application 274 -> 276` is the
two review-driven `R4`/`R5` parity cases, and `Web 565 -> 568` is the three
tests added after the live multi-window reporter fix.

Burst outcomes, deterministic fake downstream only:

| Arrivals | Accepted | Capacity | Max active analyses | Crash | Unbounded wait |
|---:|---:|---:|---:|---:|---:|
| 10 | 1 | 9 | 1 | 0 | 0 |
| 50 | 1 | 49 | 1 | 0 | 0 |
| 100 | 1 | 99 | 1 | 0 | 0 |

Repository concurrency semantic parity is covered deterministically against
captured inputs, and the final direct coverage is:

~~~text
R1  sequential baseline (the comparison target)
R2
R3
R4
R5
~~~

Each concurrent degree is compared to the `R1` sequential run over a mixed
outcome set (analyzed / analysis-failed / collection-failed / unavailable /
not-admitted / undetermined). The per-repository fingerprint surface now
includes:

- admission decision / reason
- analysis status
- earned / applicable / normalized score
- signal status / score / evidence keys
- limitation keys
- warning codes
- category status / score / maximum / normalized
- collection availability
- failure reason

The full suite passed. Provider budget/header isolation, post-metadata rejection
before manifest, reset capture, Web operational mapping and
failure-not-profile-truth tests all passed.

Authenticated production-composition measurement, U=1 and GraphQL default-off:

| R | Wall clock | Requests | Max HTTP | Core remaining windows | Reset windows (UTC) | Retry-After | Primary event | Incomplete | Live parity |
|---:|---:|---:|---:|---|---|---:|---:|---:|---|
| 3 | 112.209 s | 662 | 3 | 4277→3616 (min 3616); 4965→4965 | 19:57:57; 19:58:15 | 0 | 0 | 0 | PASS |
| 4 | 70.888 s | 662 | 4 | 3615→2955 (min 2955); 4962→4962 | 19:57:57; 19:58:15 | 0 | 0 | 0 | PASS |
| 5 | **55.557 s** | **662** | 5 | 2954→2294 (min 2294); 4961→4961 | 19:57:57; 19:58:15 | 0 | 0 | 0 | **PASS** |

Every run composed successfully, emitted zero GraphQL requests and kept the
physical request count exactly equal to Session 131's 662-request baseline.
R4 and R5 produced the same live profile-level fingerprint as R3; the
captured-input parity tests anchor repository-level semantics to the sequential
baseline for R2, R3, R4 and R5 alike. No sampling, hidden cap, omission or
approximation ran.

One preliminary R3 provider execution completed before the first harness
attempt exposed the test-only `SingleOrDefault(core)` reporting defect. Its
decision telemetry was not emitted and it is not used as a candidate result;
the full locked sequence above was rerun after the bounded multi-window fix.

---

## Supporting Evidence — architecture-owner manual R5 Web smoke

A real Web/manual Profile run was performed by the architecture owner with R5.
Observed telemetry:

~~~text
RepositoriesCompleted=57
RepositoriesIncomplete=0

DiscoveryMs=2317
PipelineMs=51845

RepositoryCollection:
count=57
totalMs=248388
minMs=3555
maxMs=6159

RepositoryAnalysis:
count=57
totalMs=47

Rate limit:
core#1 min remaining=3620
core#2 remaining=4968
UnrecordedRateLimitObservations=0

Status=Complete
Outcome=composed
RepositoryOutcomes={}

Observed request/rate-limit samples:
793
~~~

Interpretation:

- 57/57 repositories completed;
- no incomplete repository;
- no sampling / top-N indication;
- complete composed result;
- provider budget remained above the `1500` safety floor;
- behaviour is consistent with R5 bounded repository concurrency;
- this is **supporting evidence, NOT a replacement for the `55.557 s`
  benchmark**;
- it is a **different profile shape** from the 662-request dense benchmark, so
  its 793 observed samples are not comparable to the 662-request baseline and no
  latency claim is derived from it.

---

## Independent Review

An independent closure review was run over the Session 133 diff, the measured
evidence and the final tree.

Findings raised and the minimal fixes applied for them — all test/harness-only
proof-strengthening, no behavioural change to the runtime under test:

- **A — launch-burst synchronization.** The burst test lacked a true all-ready
  synchronization point, so late scheduling could have degraded it into
  staggered arrivals. A readiness barrier now holds every arrival behind one
  release signal, and the admission-decision completion is separately bounded,
  so the simultaneous-arrival claim is actually proven.
- **B — R4/R5 repository semantic parity.** Direct captured-input parity
  coverage stopped at R3, leaving the proposed R5 candidate anchored only
  indirectly. `R4` and `R5` are now compared to the sequential baseline
  directly, and the fingerprint surface was widened to the full list recorded in
  Validation.
- **C — provider-protection signal timing.** The runtime-gate observer only
  invalidated a candidate at run-end evaluation. It now cancels the in-flight
  run immediately on a provider-protection response, so a future live candidate
  is invalidated at the moment of the signal rather than after further provider
  work.

Production source and production configuration were unchanged by these fixes;
the whole Session 133 working set remains `tests/` and `worklog/` only.

Review outcome:

~~~text
Independent review:
PASS WITH FINDINGS

Critical findings:
NONE

R5 production candidate:
ACCEPT

Session 133 closure:
PASS

Milestone B closure:
NOT CLAIMED
~~~

The non-blocking findings from that review were record-accuracy items against
this worklog and `CURRENT.md`; they are the subject of the worklog-only
correction pass that produced this revision.

---

## Problems

Problem:
The first authenticated R3 run completed provider work but failed while
formatting telemetry because the harness assumed exactly one `core` rate-limit
window. GitHub responses produced two reset-window buckets.

Cause:
Test-only use of `SingleOrDefault(window.Resource == "core")` contradicted the
existing telemetry contract, whose key includes both resource and reset instant.

Resolution:
The harness now reports all bounded core windows and a targeted regression test
pins the two-window shape. The locked sequence was rerun from R3 and passed.

---

## Not Done

- Production configuration was not modified; R remains default 1 until the
  owner applies the accepted R5 candidate through deployment configuration.
- Milestone B itself is **not** closed. The independent review closed Session
  133 and accepted R5 as a candidate; the milestone exit still requires the
  production-candidate configuration to be applied and validated.
- No commit, push, merge or deploy was performed.

---

## Known Risks

- R5 has 4.443 seconds of observed margin on one network/provider run. It is a
  measured product candidate, not a universal latency guarantee.
- The known message-only secondary-limit residual from Session 126 remains
  unchanged, and Session 133 does not move that product/runtime position. The
  test-only runtime-gate observer parses no provider error text; it
  conservatively treats either a `Retry-After` header, OR a `403`/`429` that it
  does not classify as primary exhaustion, as a secondary/protection event and
  invalidates the candidate. That is a deliberately broad, test-only
  invalidation rule for benchmark safety — it is **not** a canonical GitHub
  secondary-limit discriminator and must not be read as one.
- Two `core` reset windows were observed in every candidate and are recorded
  without interpretation. Live metadata budget protection still used the first
  real response header; no `/rate_limit` preflight was used.

---

## Working Set / Final Git State

~~~text
Modified:
- tests/GitHubProof.Application.Tests/Profile/ProfileRepositoryConcurrencyTests.cs
- tests/GitHubProof.Web.Tests/ProfileConcurrencyMeasurementHarness.cs
- tests/GitHubProof.Web.Tests/ProfileRuntimeBenchmarkHarness.cs
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- tests/GitHubProof.Web.Tests/ProfileLaunchBurstTests.cs
- tests/GitHubProof.Web.Tests/ProfileMilestoneBRuntimeGateHarness.cs
- worklog/sessions/2026-08-26-133-milestone-b-runtime-gates.md

Pre-existing unrelated:
- artifacts/
~~~

---

## Next

The independent closure review is complete and Session 133 is closed. The next
meaningful session applies and validates the production-candidate runtime
configuration:

~~~text
R = 5
U = 1
GraphQL = default OFF
~~~

and then performs the required production-candidate smoke / Milestone B exit
validation. The provider floor is not changed without a separate owner decision,
and Milestone B is not claimed closed before that exit validation passes.

---

## Git

~~~text
Suggested commit:
test(profile): close Milestone B runtime gates with R5 evidence

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Goal tamamlandı veya Result doğru final state'e getirildi
[x] Targeted validation çalıştı
[x] Gerekliyse broader validation çalıştı
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Kullanıcıya kısa final rapor verildi
~~~

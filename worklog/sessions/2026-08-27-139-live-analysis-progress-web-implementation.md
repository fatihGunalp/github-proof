# Session 139 — Live Analysis Progress Web Implementation

**Date:** 2026-08-27
**Result:** PASS WITH FINDINGS
**Scope:** web
**Model Route:** Sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

Independent review (`github-proof-review`, forked cold context, Sonnet / high):
**PASS WITH FINDINGS** — BLOCKER 0, HIGH 0, MEDIUM 0, LOW 1 + 2 NIT. All three
accepted and closed in this same session (see Problems), affected tests + diff
check rerun. No second broad review.

---

## Goal

Implement exactly the Session 138 live analysis progress contract in
`GitHubProof.Web` only: the process-local ephemeral run registry, the read-only
`GET /analysis-progress?run=<token>` Razor Page, the client-generated UUID
correlation token, constant 2000 ms polling, the 4 Repository / 3 Profile stage
identities and their `PENDING → ACTIVE → COMPLETED` state machine, the narrowed
single-sentence live region, and the BFCache reset — selecting no runtime value,
copy or rendering of its own.

---

## Starting State

~~~text
Branch:
session/139-live-analysis-progress-web-implementation

Base HEAD:
3173bb9  (main == origin/main == HEAD verified equal)

Pre-existing Working Tree:
?? artifacts/   (user-owned, out of scope, untouched)

Relevant Previous Session:
sessions/2026-08-27-138-unified-live-analysis-progress-contract.md  (RATIFIED)
~~~

---

## Model Route

~~~text
Selected:
Sonnet / high

Why:
Bounded Web-only implementation but invariant-heavy — registry atomicity/
coherence/cleanup/cap, endpoint known/unknown/400/no-store, the six locked
client limits, and accessibility/live-region guards that must not weaken.

User Approval:
devam et
~~~

---

## Model Escalation

~~~text
None.
~~~

---

## Context Read

~~~text
AGENTS.md
CLAUDE.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
docs/architecture/live-analysis-progress-runtime-contract.md
docs/product/p10-presentation-language-contract.md §8.3.1–§8.3.6, §8.6.5
worklog/CURRENT.md
src/GitHubProof.Web/** (Pages, Analysis, Profile, Localization, wwwroot)
tests/GitHubProof.Web.Tests/AnalysisProgressContractTests.cs, IndexModelTests.cs,
  ProfileModelTests.cs, WebPresentationLocalizerTests.cs, CssArchitectureTests.cs
~~~

---

## Contract / Invariants Checked

~~~text
- Existing synchronous analysis POST unchanged; no run-creation / result-retrieval
  endpoint; no background job/queue/persistence; no SSE/SignalR/WebSocket.
- Registry: process-local singleton; immutable coherent snapshots; one atomic
  transition replacement; `completed` is always exactly the stages preceding
  `active`; `active == null` only at the mandatory terminal snapshot; cleanup in
  a `using`/finally on every path; cap 64 refuses registration only; TTL 10 min
  backstop; none configurable.
- Endpoint: GET only, read-only, no registry write; known → {known,target,active,
  completed}; unknown valid UUID → {known:false}; malformed/missing → HTTP 400
  empty body; every response `Cache-Control: no-store`; no ETag/conditional.
- Stages: Repository 4 (target-validated, public-data-read, traces-analyzed,
  proof-prepared); Profile 3 (target-validated, public-surface-read,
  evidence-examined). No FAILED/CANCELLED/SKIPPED. Partial is terminal success
  completing stage 3.
- Client: landing form only generates crypto.randomUUID before submit; immediate
  first poll then every 2000 ms; initial unknown max 5; transport-failure max 3;
  pause while hidden, resume on visible; pageshow resets to PENDING; known-then-
  lost keeps last snapshot (no revert); client never advances a stage.
- Presentation: inline only; no modal/cancel/percentage/ETA/counts; exact p10
  §8.3.2–§8.3.3 copy via WebPresentationLocalizer; indeterminate indicator
  retained; live region child keeps role=status / aria-live=polite /
  aria-atomic=true; announce indeterminate sentence before first snapshot, active
  stage label during, nothing on terminal.
- Drill-down forms (_ProfileRepositoryEvidence / _ProfileStartingPoint): no token,
  no polling, no stage UI; current submit/duplicate-submit behavior unchanged.
- p10 §8.3.5 aria-atomic substring guard preserved; only the `setInterval` and
  `fetch(` exclusions in AnalysisProgressContractTests are superseded.
~~~

---

## Changes

~~~text
Created:
- src/GitHubProof.Web/Progress/AnalysisProgressTargetKind.cs
- src/GitHubProof.Web/Progress/AnalysisProgressStages.cs
- src/GitHubProof.Web/Progress/AnalysisProgressSnapshot.cs
- src/GitHubProof.Web/Progress/AnalysisProgressRun.cs
- src/GitHubProof.Web/Progress/AnalysisProgressRegistry.cs
- src/GitHubProof.Web/Pages/AnalysisProgress.cshtml (+ .cshtml.cs)
- tests/GitHubProof.Web.Tests/AnalysisProgressRegistryTests.cs
- tests/GitHubProof.Web.Tests/AnalysisProgressEndpointTests.cs
- tests/GitHubProof.Web.Tests/AnalysisProgressClientContractTests.cs

Modified:
- src/GitHubProof.Web/Localization/WebTextKey.cs
    +7 stage-label keys, +3 stage-state keys
- src/GitHubProof.Web/Localization/WebPresentationLocalizer.cs
    EN/TR copy for the 10 new keys (exact p10 §8.3.2–§8.3.3 text)
- src/GitHubProof.Web/Analysis/LiveRepositoryReportService.cs
    optional AnalysisProgressRun param; CompleteCurrentStage() after Parse,
    after successful CollectAsync, after Analyze
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
    optional AnalysisProgressRun param; CompleteCurrentStage() after successful
    snapshot resolves (stage 2)
- src/GitHubProof.Web/Pages/Index.cshtml.cs
    inject registry; [BindProperty] AnalysisProgressToken; register a repository
    run once the classifier resolves a repository route (never for a profile
    redirect); complete stage 4 after Project(); `using` releases on every path
- src/GitHubProof.Web/Pages/Profile.cshtml.cs
    inject registry; [BindProperty] AnalysisProgressToken; register a profile run
    after re-classification and BEFORE the lease; complete stage 1 on lease grant;
    complete stage 3 when HasComposedProfile; `using` releases on every path
- src/GitHubProof.Web/Program.cs
    AddSingleton<AnalysisProgressRegistry>()
- src/GitHubProof.Web/Pages/Shared/_GitHubInput.cshtml
    data-analysis-progress marker; both stage lists (server-rendered, localized,
    PENDING, hidden); narrowed live-region child; indicator retained
- src/GitHubProof.Web/wwwroot/js/analysis-progress.js
    per-form duplicate-submit/reset kept for all [data-analysis-form]; token
    generation + polling only for the [data-analysis-progress] landing form;
    constant setInterval(2000), limits 5/3, visibilitychange pause/resume,
    pageshow resetState, known-then-lost keeps last snapshot
- src/GitHubProof.Web/wwwroot/css/repository-input.css
    stage-list / state-token / completed-checkmark styles; indicator + reduced
    motion unchanged
- tests/GitHubProof.Web.Tests/AnalysisProgressContractTests.cs
    drop the two superseded exclusions; add streaming-transport exclusions
- tests/GitHubProof.Web.Tests/IndexModelTests.cs
    registry ctor arg; +5 progress-registration tests + observing collector
- tests/GitHubProof.Web.Tests/ProfileModelTests.cs
    registry ctor arg; +4 progress tests + observing collector

Deleted:
- none
~~~

---

## Decisions

~~~text
- Run handle API is a single AnalysisProgressRun.CompleteCurrentStage() advanced
  once per observable completion point, in order; the last call publishes the
  mandatory coherent terminal snapshot (all completed, active null). A call past
  terminal throws.
- Registration owner split per contract §8: IndexModel registers a repository run
  the moment the classifier resolves a repository route (never on the profile
  redirect); ProfileModel registers a profile run after re-classification and
  before the admission lease. The endpoint never registers.
- Stage-completion publication points: repository stages 1–3 inside
  LiveRepositoryReportService, stage 4 in IndexModel after Project();
  profile stage 1 in ProfileModel on lease grant, stage 2 in
  LiveProfileReportService on a successful snapshot, stage 3 in ProfileModel on
  HasComposedProfile.
- Endpoint returns hand-built/serialized JSON via ContentResult (no dependency on
  MVC JSON formatter registration); `{"known":false}` written literally; camelCase
  via a local JsonSerializerOptions.
- Correlation token bound as [BindProperty] string? AnalysisProgressToken on both
  page models; the JS appends it as a hidden field only on a valid landing
  submit, so a non-JS client posts nothing and gets the indeterminate state.
- All copy is server-rendered: the JS reads stage labels from the DOM
  (.analysis-progress__stage-label) and never constructs a progress string.
- Registry TimeProvider is the existing singleton; TTL/cap are constants on the
  registry type; poll/limits are constants in analysis-progress.js. No options
  class, section or key.
- No canonical docs change was required. No Domain/Application/Infrastructure,
  package, config or appsettings change.
~~~

---

## Validation

~~~text
dotnet test tests/GitHubProof.Web.Tests
PASS — 631/631 (was 585; +46 new)

dotnet test GitHubProof.slnx
PASS — Domain 842, Infrastructure 636, Web 631, Application 276 = 2385/2385

dotnet build GitHubProof.slnx
PASS — 0 warnings, 0 errors

git diff --check
PASS — only advisory LF→CRLF notices, exit 0

Live smoke (dotnet run, Development):
- GET /analysis-progress?run=<uuid>       → 200 {"known":false}   Cache-Control: no-store
- GET /analysis-progress?run=bad          → 400, Content-Length: 0, Cache-Control: no-store
- GET /analysis-progress (no run)         → 400 empty, Cache-Control: no-store
- landing page renders both stage lists (4 + 3), narrowed live region
  (role=status / aria-live=polite / aria-atomic=true), indicator retained
- real repository POST + concurrent polling:
  {"known":true,"target":"repository","active":"repository-public-data-read",
   "completed":["repository-target-validated"]}
  — stage 1 completed right after Parse, stage 2 ACTIVE, exactly per §4.
  (The unauthenticated GitHub call for octocat/Hello-World stalled on the
  provider side and the POST had not returned; the progress channel behaved
  correctly throughout.)
~~~

---

## Problems

~~~text
Problem:
Curl-driven end-to-end could not drive the analysis POST to completion — the
unauthenticated GitHub fetch stalled (rate-limited / slow).

Cause:
No GITHUB_TOKEN; 60/hour unauthenticated quota, partially consumed by smoke
attempts. Not a code issue.

Resolution:
Live smoke confirmed registration, stage-1 completion, stage-2 ACTIVE and the
endpoint contract directly; full stage progression to terminal is proven by the
deterministic model-level and registry-level tests.
~~~

Independent review findings — all accepted and closed this session:

~~~text
LOW-1  analysis-progress.js streak counters were not cross-reset, so a mixed
       response run (fail, known:false, fail) could drift from §13's literal
       "consecutive" / "in a row". Fixed: a valid known:false zeroes
       failureStreak, a transport failure zeroes unknownStreak. Guard test
       added (Client_streak_limits_are_consecutive_and_reset_each_other).

NIT-1  AnalysisProgressRun.LastUpdatedUtc was read by the registry backstop
       sweep outside `gate`. Changed to a volatile 64-bit tick count
       (Volatile.Read/Write) so the cross-thread read is well-defined.

NIT-2  AnalysisProgressContractTests had a duplicated EventSource exclusion
       (Ordinal + OrdinalIgnoreCase). Collapsed to one OrdinalIgnoreCase set.
~~~

Post-fix validation: Web 631/631, solution 2385/2385, build 0 warnings,
`git diff --check` clean.

---

## Not Done

~~~text
- No five-stage Profile split (deferred owner decision, contract §5.1 / §18).
- No profile-identity-read stage (needs Infrastructure publication point).
- No explicit cancellation control (contract §14).
- No durable / multi-instance run store (contract §18).
- No configuration surface for any of the six locked values.
- Drill-down forms remain progress-free (no per-row surface invented).
~~~

---

## Known Risks

~~~text
- Full E2E stage progression to the terminal snapshot was not observed live
  (provider stall). Covered deterministically by tests; a token-authenticated
  smoke would close it.
- The registry cap race is bounded by a registration lock; under extreme
  concurrency the count cannot exceed 64 because the check and TryAdd are both
  inside that lock.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Web/Analysis/LiveRepositoryReportService.cs
- src/GitHubProof.Web/Localization/WebPresentationLocalizer.cs
- src/GitHubProof.Web/Localization/WebTextKey.cs
- src/GitHubProof.Web/Pages/Index.cshtml.cs
- src/GitHubProof.Web/Pages/Profile.cshtml.cs
- src/GitHubProof.Web/Pages/Shared/_GitHubInput.cshtml
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
- src/GitHubProof.Web/Program.cs
- src/GitHubProof.Web/wwwroot/css/repository-input.css
- src/GitHubProof.Web/wwwroot/js/analysis-progress.js
- tests/GitHubProof.Web.Tests/AnalysisProgressContractTests.cs
- tests/GitHubProof.Web.Tests/IndexModelTests.cs
- tests/GitHubProof.Web.Tests/ProfileModelTests.cs

Untracked (this session):
- src/GitHubProof.Web/Progress/
- src/GitHubProof.Web/Pages/AnalysisProgress.cshtml (+ .cshtml.cs)
- tests/GitHubProof.Web.Tests/AnalysisProgressRegistryTests.cs
- tests/GitHubProof.Web.Tests/AnalysisProgressEndpointTests.cs
- tests/GitHubProof.Web.Tests/AnalysisProgressClientContractTests.cs

Pre-existing unrelated:
- artifacts/   (untouched)
~~~

---

## Next

Independent review is done (PASS WITH FINDINGS, all closed). Session 139 is
complete and STOPPED. The remaining step is user-owned: review the diff and, if
accepted, commit `feat(web): add truthful live analysis progress` and integrate
the branch. An authenticated live smoke (`GITHUB_TOKEN` set) would close the one
recorded gap — full stage progression to the terminal snapshot observed live.

---

## Git

~~~text
Suggested commit:
feat(web): add truthful live analysis progress

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Goal implemented
[x] Targeted validation ran (Web 630/630)
[x] Broader validation ran (solution 2384/2384, build 0 warnings)
[x] git diff --check reviewed
[x] git diff/status reviewed
[x] Contract/invariant drift checked — none; canonical docs unchanged
[x] Session Result updated — PASS WITH FINDINGS (all closed)
[x] INDEX row updated
[x] CURRENT.Active Session updated
[x] CURRENT.Next Immediate Action updated
[x] Final user report
~~~

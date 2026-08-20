# Session 094 — P10-I1 Live Profile Request Boundary

**Type:** implementation (Web)
**Branch:** session/094-p10-i1-live-profile-request-boundary
**Session Base:** 77f6f16 (docs(profile): close P10 implementation contract) — `main`, verified clean working tree.

---

## 1. Goal

Implement P10-I1 — Live Profile Request Boundary, exactly as locked in
`docs/product/p10-web-profile-flow-implementation-plan.md` §P10-I1 (Session
093). Three bounded responsibilities:

1. Add the missing `IGitHubProfileSnapshotCollector` Web composition-root
   registration to `Program.cs`, following the existing
   `IGitHubRepositorySnapshotCollector` registration pattern exactly.
2. Add the smallest Web service/result pair executing raw profile identity
   → `IGitHubProfileSnapshotCollector.CollectAsync` →
   `GitHubProfileSnapshotResult` → `GitHubProfileSnapshot` →
   `ProfilePipeline.RunAsync` → `ProfilePipelineResult` → `ProfileReport` on
   composed success — the profile counterpart of `LiveRepositoryReportService`
   / `LiveRepositoryAnalysisResult`.
3. Targeted Web tests proving DI resolution, the success path, typed
   collector-level failure (no pipeline invocation), preserved
   pipeline Failed/Composed invariants, and no Web-side re-implementation of
   `GitHubProfileHandle` parsing.

No pipeline stage (`ProfilePipeline`, `ProfileComposer`,
`ProfileReportProjector`), no Infrastructure collector/orchestrator/handle,
and no P10 routing/presentation slice (I2-I6) is in scope.

## 2. Context Read (minimum-context)

- `worklog/CURRENT.md` — confirmed P10 contract/planning CLOSED (Session
  093), P10-I1 is the exact next action, no active-session collision.
- `docs/product/p10-web-profile-flow-implementation-plan.md` §P10-I1 — the
  locked three-part scope, the exact flow diagram, and the explicit
  "do not modify" list.
- Actual code (read, not modified, to confirm the precedent and the exact
  seams): `src/GitHubProof.Web/Program.cs`,
  `src/GitHubProof.Web/Analysis/LiveRepositoryReportService.cs` +
  `LiveRepositoryAnalysisResult.cs`,
  `src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs`,
  `src/GitHubProof.Infrastructure/GitHub/Profile/IGitHubProfileSnapshotCollector.cs`,
  `GitHubProfileSnapshotResult.cs`, `GitHubProfileSnapshotOrchestrator.cs`,
  `GitHubProfileHandle.cs`, `src/GitHubProof.Application/Profile/ProfilePipeline.cs`,
  `ProfilePipelineResult.cs`, `ProfileRepositoryAnalysisOrchestrator.cs`,
  `src/GitHubProof.Application/Profile/Report/ProfileReport.cs`.
- Existing test precedent: `tests/GitHubProof.Web.Tests/LiveRepositoryReportServiceTests.cs`,
  `IndexModelTests.cs`, `HomeCompositionTests.cs`,
  `ProfilePipelineServiceCollectionExtensionsTests.cs`,
  `tests/GitHubProof.Application.Tests/Profile/ProfilePipelineTests.cs` +
  `ProfileCompositionTestSupport.cs` (construction reference only — not
  reused directly; `GitHubProof.Web.Tests` does not reference
  `GitHubProof.Application.Tests`).

No canonical `docs/architecture/` or `docs/product/` file was read beyond
what P10-I1's own locked section already cited; no full repository or full
session history load was performed.

## 3. Implementation

**`src/GitHubProof.Web/Program.cs`** (additive only, 21 lines): one new
`builder.Services.AddScoped<IGitHubProfileSnapshotCollector>(...)`
registration placed after `AddProfilePipeline()`, reusing the same named
`"GitHubProof.GitHub"` `HttpClient` and the same registered `TimeProvider`
singleton, constructing via
`GitHubProfileSnapshotOrchestrator.CreateDefault(httpClient, timeProvider)`
— line-for-line the same pattern as the pre-existing
`IGitHubRepositorySnapshotCollector` registration immediately above it. One
new `builder.Services.AddScoped<LiveProfileReportService>();`. The
pre-existing `IGitHubRepositorySnapshotCollector` registration and
`AddProfilePipeline()` call are untouched.

**`src/GitHubProof.Web/Profile/LiveProfileAnalysisResult.cs`** (new): the
profile counterpart of `LiveRepositoryAnalysisResult`, same invariant shape
— `PipelineResult is null` XOR `!ProfileAvailability.IsAvailable`, enforced
in the constructor. Carries the real `ProfilePipelineResult?` (Failed or
Composed, exactly as `ProfilePipeline.RunAsync` produced it) rather than
re-encoding its own success/failure taxonomy, so a Failed composition and a
collector-level unavailable/rate-limited failure remain two distinguishable,
typed outcomes.

**`src/GitHubProof.Web/Profile/LiveProfileReportService.cs`** (new): takes
`IGitHubProfileSnapshotCollector` and `ProfilePipeline` (both already
production types — no new abstraction). `AnalyzeAsync(profileIdentity,
version, cancellationToken)` forwards the raw string straight to
`snapshotCollector.CollectAsync(profileIdentity, cancellationToken)` — it
parses nothing itself; `GitHubProfileHandle.Parse` stays exclusively owned
by the Infrastructure orchestrator, called internally by `CollectAsync`. On
an unsuccessful collection it returns immediately with
`PipelineResult = null` and the collector's own `DataAvailability` (typed
`RateLimited`/`Forbidden`/`Unavailable`/... preserved verbatim, never
collapsed into a generic claim) — `ProfilePipeline.RunAsync` is never
called. On a successful collection it calls
`profilePipeline.RunAsync(snapshot, version, cancellationToken)` with the
real production signature and wraps the resulting `ProfilePipelineResult`
(Failed or Composed) unchanged.

## 4. Tests

**`tests/GitHubProof.Web.Tests/LiveProfileReportServiceTests.cs`** (new, 7
tests): success through a real `ProfilePipeline` (constructed from real
Domain/Application policy objects over a zero-repository snapshot, so no
production algorithm is stubbed) producing a 9-section `ProfileReport`;
collector-level `Unavailable` and `RateLimited` failures preserved typed and
distinct, pipeline never reached (proved by a snapshot source/analysis
runner that throws if ever invoked); invalid raw identity propagates the
*same* `ArgumentException` a collector that calls the real
`GitHubProfileHandle.Parse` throws, with `ParamName == "value"` — proving
`LiveProfileReportService` adds no parsing of its own; direct invariant
tests on `LiveProfileAnalysisResult` proving a Failed `ProfilePipelineResult`
is carried through unchanged (no `Report`, `HasProfile == false`) and that
the XOR constructor guard rejects a mismatched pairing either direction.

**`tests/GitHubProof.Web.Tests/ProfileCompositionRootTests.cs`** (new, 4
tests): a `ServiceCollection` replicating `Program.cs`'s exact registration
pattern resolves `IGitHubProfileSnapshotCollector` to the real
`GitHubProfileSnapshotOrchestrator` type; the repository collector
registration resolves unchanged alongside it in the same container (proving
neither replaces the other); `LiveProfileReportService` resolves from the
same composition; a source-text assertion on the actual `Program.cs` file
confirms the registration literally exists there, follows the repository
collector's pattern, and reuses the same named HttpClient at least three
times (one `AddHttpClient` + one `CreateClient` per collector).

## 5. Validation

```text
dotnet build GitHubProof.slnx           -> 0 Warning / 0 Error
dotnet test  (narrow: new 2 files)      -> 11/11 PASS
dotnet test  tests/GitHubProof.Web.Tests -> 83/83 PASS (72 baseline + 11 new)
dotnet test  GitHubProof.slnx (full)    -> 1386/1386 PASS
  (Application 246, Domain 798, Infrastructure 259, Web 83)
git diff --check                        -> clean
```

Working-tree scope (`git status --short`), final — including the same-session
worklog integration pass (§6-§7 below: `CURRENT.md`/`INDEX.md` update and this
session file's own creation):

```text
 M src/GitHubProof.Web/Program.cs
 M worklog/CURRENT.md
 M worklog/INDEX.md
?? src/GitHubProof.Web/Profile/LiveProfileAnalysisResult.cs
?? src/GitHubProof.Web/Profile/LiveProfileReportService.cs
?? tests/GitHubProof.Web.Tests/LiveProfileReportServiceTests.cs
?? tests/GitHubProof.Web.Tests/ProfileCompositionRootTests.cs
?? worklog/sessions/2026-08-20-094-p10-i1-live-profile-request-boundary.md
```

The three `worklog/` entries are this session's own operational-memory
handoff, not production scope. No canonical `docs/` change; no new NuGet
package; no `ProfilePipeline`/`ProfileComposer`/`ProfileReportProjector`/
`IGitHubProfileSnapshotCollector`/`GitHubProfileHandle` change.

## 6. Independent Review

`github-proof-review` (forked, Sonnet high): **PASS WITH FINDINGS** — 1
MEDIUM, non-blocking: this worklog (`CURRENT.md` Active Session + Session
094 file) was not yet written at review time. No scope, contract-drift,
layering, or determinism issue found. Verdict text: "implementasyonun
kendisi kilitli P10-I1 kontratını birebir karşılıyor ve tam doğrulanmış
durumda." Addressed by this same session (this file + `CURRENT.md` +
`INDEX.md` update) — no code change required for the finding.

## 7. Remaining Risk / Next Action

- P10-I1 is COMPLETE. `IGitHubProfileSnapshotCollector` resolves from the
  real Web DI composition; no Web page consumes `LiveProfileReportService`
  yet (that is P10-I2's responsibility, unstarted).
- The pre-P10 production-capacity gap (no deterministic GitHub API request
  upper bound for profile analysis — repositories analyzed individually,
  some collectors paginate until GitHub has no next page) remains explicitly
  out of scope for this slice, per the task's operational note; unchanged by
  this session.
- Exact next action: **P10-I2 — Profile Entry + Routing**
  (`docs/product/p10-web-profile-flow-implementation-plan.md` §P10-I2).
- Commit/push NOT performed (user-owned, per workflow).

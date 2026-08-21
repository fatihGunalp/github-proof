# Session 097 — Profile Runtime Collection Implementation

**Date:** 2026-08-21
**Result:** PASS WITH FINDINGS — implementation + automated proofs PASS; controlled live smoke FAILED; Profile runtime readiness BLOCKED
**Scope:** infrastructure / web / application tests
**Model Route:** Claude / Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## 1. Goal

Implement the contract locked by Session 096
(`docs/architecture/public-profile-runtime-collection.md`): the smallest
production-safe Profile V1 runtime composition that issues zero GitHub requests
for `SourceFiles` and `DependencyDeclarations`, preserves the exact current
Profile V1 semantic result, leaves Repository Proof unchanged, and adds bounded
per-profile runtime telemetry.

The internal execution guard is deliberately **not** implemented.

## 2. Starting State

~~~text
Branch:
session/097-profile-runtime-collection-implementation

Base HEAD:
715c55a — matches the expected SHA given in the handoff

Pre-existing Working Tree:
CLEAN

Relevant Previous Session:
worklog/sessions/2026-08-20-096-profile-runtime-cost-contract.md
~~~

## 3. Implementation Architecture

### 3.1 Profile-specific collection composition (Infrastructure)

`GitHubRepositorySnapshotOrchestrator` gained a Profile-specific construction
path, and the shared assembly around it was refactored so the two enrichment
collectors can be absent:

- added `CreateProfileComposition(HttpClient, TimeProvider?)` — the same assembly
  with the two enrichment collectors omitted;
- added a second public constructor taking the nine required collectors;
- **changed** the two enrichment collector fields to nullable, where null means
  the surface is omitted, never that collection failed;
- **changed** `CollectAsync` to skip the two collector calls and pass `null` for
  their collections when they are absent;
- **changed** `CollectAvailability` to accept nullable collections and contribute
  no entry for an absent one.

So the source change is not purely additive. What *is* preserved exactly is the
Repository Proof contract: the eleven-collector constructor keeps its non-null
parameter contract, `CreateDefault` still builds the full composition, and on
that path both collectors are non-null, so every branch above takes the same
route it took before. Repository Proof's observable semantics are unchanged, and
that is what the tests prove — the existing orchestrator tests are additive only,
and the composition tests assert the full composition still collects both
enrichment surfaces and still emits both availability entries.

When omitted:

- the collector is never reached, so no request is issued;
- `RepositorySnapshot` receives `null` for both collections and normalizes them
  to the same empty collections the isolation-test baseline uses;
- **no availability entry is contributed at all** — the omission is absent, never
  `NotRequested`, `Unavailable` or `Failed`;
- no collection warning, limitation or status change is produced.

### 3.2 Web composition

`AddProfilePipeline` now owns the whole profile runtime boundary:

- it registers a profile-specific named GitHub client,
  `"GitHubProof.GitHub.Profile"`, configured identically to the Repository Proof
  client (same `BaseAddress`, same 2-minute timeout, same optional
  `GITHUB_TOKEN` bearer) plus the request-counting handler;
- it builds `IProfileRepositorySnapshotSource` from
  `CreateProfileComposition`, so the profile path **no longer resolves the
  caller's `IGitHubRepositorySnapshotCollector` at all**;
- `Program.cs`'s profile snapshot collector registration now takes its client
  from the same profile factory helper, so profile-metadata and
  repository-manifest requests are counted too.

Repository Proof's registration, its `"GitHubProof.GitHub"` client and its
construction path are untouched, and the telemetry handler is registered only on
the profile client.

### 3.3 Telemetry (Web, `Profile/Runtime/`)

- `ProfileExecutionTelemetry` — per-execution counters (`Interlocked`), a
  bounded category dictionary, duration, status and outcome, plus one structured
  `ToLogMessage()` line.
- `ProfileExecutionTelemetryScope` — an `AsyncLocal` ambient current-execution
  holder. This is the deliberate answer to handler pooling: `IHttpClientFactory`
  reuses `DelegatingHandler` instances across DI scopes and requests, so a
  handler can never safely hold a per-request object, while an `AsyncLocal`
  value flows into every awaited HTTP call of one execution and is invisible to
  any other.
- `ProfileGitHubRequestCategory` — a closed 17-value vocabulary derived from the
  API route shape only. No repository name, owner, login, path or SHA can ever
  become a category, and an unrecognized value is folded into `other`.
- `ProfileGitHubRequestTelemetryHandler` — counts the request. It reads no
  header, touches no body and modifies no message.
- `TelemetryProfileRepositorySnapshotSource` / `TelemetryRepositoryAnalysisRunner`
  — observation-only wrappers supplying the repository counters. The orchestrator
  calls the snapshot source once per **admitted** repository and the analysis
  runner only for a repository whose collection succeeded, so invocation counts
  are exactly "admitted" and "completed". Admission is never recomputed in Web
  and `ProfileRepositoryAnalysisOrchestrator` is unchanged, per Session 096 §14.2.
- `LiveProfileReportService` creates one telemetry instance per `AnalyzeAsync`
  call, opens the ambient scope, records discovered/status/outcome and logs the
  completion line in a `finally`.

Observable per execution: `DurationMs`, `GitHubRequests`,
`RepositoriesDiscovered`, `RepositoriesAdmitted`, `RepositoriesCompleted`,
`RepositoriesIncomplete`, `Status`, `Outcome`, `RepositoryOutcomes`,
`RequestCategories`.

`RepositoriesIncomplete` is derived (`Admitted - Completed`), so the two can
never disagree. `RepositoryOutcomes` (added closing review finding F2) counts
*why* admitted repositories could not complete, over a closed vocabulary of the
canonical `DataAvailabilityStatus` names plus `AnalysisFailed` and `Other`, so a
rate limit stays distinguishable from a forbidden repository and from an
ordinary failure.

## 4. Removed Request Surface

Verified theoretical ceiling removed from the Profile path, per admitted
repository:

~~~text
SourceFiles           2 tree-resolution + up to 32 content   = up to 34
DependencyDeclarations                  up to 32 content     = up to 32
                                                               ---------
                                                               up to 66
~~~

Measured on the deterministic fixture in
`ProfileRepositoryCollectionCompositionTests`: exactly 4 requests removed
(1 source-content, 1 manifest-content, and the source collector's duplicate
`git/commits` + `git/trees` pair), with the profile request set a strict subset
of the full one.

## 5. Changes

~~~text
Created:
- src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetry.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetryScope.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileGitHubRequestCategory.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileGitHubRequestTelemetryHandler.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileRuntimeTelemetryDecorators.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/FakeGitHubApiHandler.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/ProfileRepositoryCollectionCompositionTests.cs
- tests/GitHubProof.Application.Tests/Profile/ProfileEnrichmentOmissionEquivalenceTests.cs
- tests/GitHubProof.Web.Tests/ProfileExecutionTelemetryTests.cs
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTelemetryTests.cs
- worklog/sessions/2026-08-21-097-profile-runtime-collection-implementation.md

Modified after the first independent review (F2):
- src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetry.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileRuntimeTelemetryDecorators.cs
- tests/GitHubProof.Web.Tests/ProfileExecutionTelemetryTests.cs
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTelemetryTests.cs

Modified after the second independent review (F1/F2/F3):
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileRuntimeTelemetryDecorators.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs (comment only)
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTelemetryTests.cs
- worklog/CURRENT.md, worklog/INDEX.md and this file (F3 wording)

Modified:
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
- src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
- src/GitHubProof.Web/Program.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositorySnapshotOrchestratorTests.cs
- tests/GitHubProof.Web.Tests/ProfileCompositionRootTests.cs
- tests/GitHubProof.Web.Tests/ProfilePipelineServiceCollectionExtensionsTests.cs
- worklog/CURRENT.md
- worklog/INDEX.md
~~~

Unchanged, as required: `ProfileRepositoryAnalysisOrchestrator`,
`CrossRepositoryEvidenceComposer`, `ProfileComposer`, `ProfileReportProjector`,
every Domain policy, the 15 signal evaluators, eligibility rules, pattern rules,
representative selection, P10 presentation, and both existing enrichment
isolation test classes.

## 6. Two Superseded Test Assumptions

Two pre-existing Web tests encoded P10-I1 decisions this session deliberately
supersedes. Both were updated rather than weakened, and their real invariants
were preserved:

1. `ProfileCompositionRootTests` asserted that both collectors reuse the same
   `"GitHubProof.GitHub"` client. Session 097's §E request-counting boundary
   gives the profile path its own instrumented client. The test now asserts the
   separation explicitly: Repository Proof still registers and looks up its own
   client exactly twice, the profile collector resolves through the profile
   factory helper, and the telemetry handler appears only in the profile wiring.
2. `AddProfilePipeline_ResolvesTheProductionSnapshotSourceAdapter` asserted the
   resolved source type is exactly the production adapter. It now asserts the
   observation-only wrapper delegates to that exact production adapter, which
   preserves the invariant it exists for — Web contributes no analysis decision.

Four further tests were added around them, including one proving the profile
pipeline no longer resolves the Repository Proof collector at all.


## 6b. Independent Review

`github-proof-review`, run after implementation and the full test pass. It
re-ran `dotnet build` and `dotnet test` itself in Release, confirmed
1484/1484 at that point, verified all twelve §14 acceptance criteria, and
confirmed the two existing isolation test classes were untouched.

**Verdict: PASS WITH FINDINGS — 0 BLOCKER, 0 HIGH, 2 MEDIUM, 3 LOW.**
Both MEDIUMs are closed. The three LOWs are recorded, not fixed, to avoid
broadening this session's scope.

**F1 — MEDIUM, valid, fixed.** The session file claimed `worklog/CURRENT.md`
and `worklog/INDEX.md` had been updated while `git status` showed both
unchanged, so `CURRENT.md` still declared Session 096 active on the 096 branch
and "implementation NOT STARTED". The finding was correct: the worklog step had
not run yet when that text was written. Both files are now genuinely updated —
Active Session, Active Branch, Session Base, the Git section, the phase-state
block and the Next Action.

**F2 — MEDIUM, valid, fixed.** §9 requires rate-limit / forbidden / failed to
stay observable "when applicable". The first implementation met that only at
the profile-collection level; when the profile snapshot succeeded but individual
repositories were rate-limited, telemetry showed only `RepositoriesIncomplete=N`.
Since the next step is exactly the live run where a rate limit is most likely,
and §9.2 exists so guard values come from that measurement, this was closed
rather than deferred: `TelemetryProfileRepositorySnapshotSource` now records the
typed `DataAvailability.Status` of an unsuccessful collection and
`TelemetryRepositoryAnalysisRunner` records `AnalysisFailed` on a non-cancellation
throw (recorded, then rethrown unchanged — the orchestrator still sees the same
failure and produces the same typed result). Three tests were added.

**LOW findings, recorded and deliberately not fixed:**

- **F3** — the new public nine-collector constructor would let a caller build an
  omitting composition and register it as the Repository Proof collector, which
  would silently violate §3.1. No live defect (`Program.cs` uses `CreateDefault`,
  and a test asserts that), and the constructor must stay public because there is
  no `InternalsVisibleTo`.
- **F4** — the `repository-contents` category merges enrichment traffic with
  project-file traffic, so a future accidental `contents/` enrichment request on
  the profile path would not stand out in the category split; only the
  Infrastructure fixture test would catch it.
- **F5** — `HttpClient`'s own 2-minute transport timeout surfaces as
  `TaskCanceledException` and is therefore labelled `Outcome=cancelled`, merging
  transport timeout with caller cancellation in the operational label. Behavior
  is unaffected (it is rethrown either way); this is telemetry wording only.

**Review note, not a finding:** the profile path now opens a second connection
pool for `api.github.com`. The transport contract is identical and there is no
semantic effect, but it is a deliberate operational change worth recording.

## 6c. Second Review Pass (ChatGPT)

A second ChatGPT independent review accepted the core implementation and raised
three further MEDIUM findings. All three are closed. Behavior outside telemetry
classification is unchanged: every exception still propagates exactly as before,
and no execution guard was implemented.

**F1 — transport timeout must not be labelled caller cancellation. Valid, fixed.**
`LiveProfileReportService` caught every `OperationCanceledException` and recorded
`Outcome = Status = cancelled`, which merged caller/request cancellation with an
OCE raised while the supplied token was *not* cancelled — notably `HttpClient`'s
own transport timeout. This is the same distinction §10 principle 2 and §11.1
keep sharp, inverted. The catch is now split: a cancelled caller token records
`cancelled`, and any other OCE records the existing `Faulted` operational label.
Both rethrow unchanged and neither invents a status value. (This also closes the
first review's LOW F5, which had flagged the same conflation.)

**F2 — a thrown repository collection recorded no reason. Valid, fixed.**
`TelemetryProfileRepositorySnapshotSource` recorded a `RepositoryOutcome` only
when the inner source *returned* an unsuccessful result. When it *threw*, the
orchestrator still converted it into its existing collection-failed path, but the
execution was left with an unexplained `RepositoriesIncomplete`. The wrapper now
records the same closed `Failed` outcome for any non-caller-cancellation throw
and rethrows unchanged, so the orchestrator receives the identical exception and
produces the identical semantic result. Caller cancellation is rethrown without
being relabelled a repository failure, and a non-caller OCE (a transport timeout)
is recorded as `Failed` rather than as cancellation.

The same reasoning was applied to `TelemetryRepositoryAnalysisRunner`, whose
previous unconditional `catch (OperationCanceledException) { throw; }` left the
symmetric hole: `IRepositoryAnalysisRunner.Analyze` takes no `CancellationToken`
and observes none, so nothing it throws can be caller cancellation, and any throw
— OCE included — is now recorded as `AnalysisFailed`.

**F3 — worklog claim was factually too strong. Valid, fixed.**
The wording "gained two additive members only / and nothing else changed in it"
did not match the diff. §3.1 above, `worklog/CURRENT.md` and the
`worklog/INDEX.md` row now state accurately that a Profile-specific factory and
nine-collector construction path were *added* while the shared assembly was
*refactored* — nullable collector fields, changed `CollectAsync` and changed
`CollectAvailability` — and that what is preserved exactly is the Repository
Proof contract, proven by test rather than by the shape of the diff. The
`CreateProfileComposition` comment's "Nothing else differs" was likewise narrowed
to "Nothing else about the collected result differs". The Repository Proof
preservation claim itself was not weakened.

Four tests were added for F1 and F2:

~~~text
AnalyzeAsync_WithANonCallerCancellation_DoesNotLabelItCancelled
AnalyzeAsync_WhenCollectionThrows_RecordsFaultedAndRethrows
AnalyzeAsync_WhenRepositoryCollectionThrows_RecordsFailedWithoutChangingSemantics
AnalyzeAsync_WhenRepositoryCollectionThrowsNonCallerCancellation_RecordsFailed
~~~

The third asserts the whole F2 contract in one place: admitted 1, completed 0,
incomplete 1, `RepositoryOutcomes["Failed"] = 1`, and — unchanged semantics — the
pipeline still composed a profile from the orchestrator's existing
collection-failed result.

Remaining open from the first review: **F3** (the public nine-collector
constructor could in principle build an omitting Repository Proof composition)
and **F4** (`repository-contents` merges enrichment with project-file traffic).
Both are LOW and deliberately unfixed.

## 7. Cancellation and Guard

- External/request cancellation is unchanged: `OperationCanceledException`
  propagates out of `LiveProfileReportService`; telemetry records it in a
  `catch`-and-rethrow and a `finally`, and nothing is swallowed or converted. No
  `RepositoryAnalysis`, `ProfilePipelineResult`, partial profile or failure
  profile is fabricated (contract §11.1). Proven by
  `AnalyzeAsync_WhenCancelled_RecordsCancellationAndRethrows`.
- The internal execution guard is **NOT IMPLEMENTED**. No timeout value was
  chosen, no new `AnalysisStatus` or `DataAvailabilityStatus` was created
  (contract §11.2, §10 principle 7 — values come after measurement).

## 8. Validation

~~~text
dotnet build GitHubProof.slnx
PASS — 0 Uyarı / 0 Hata

dotnet test GitHubProof.slnx --no-restore
PASS — 1491/1491 (final, after both review correction passes)

  Domain          798/798
  Application     251/251   (+5 new: profile-level equivalence)
  Infrastructure  270/270   (+11 new: 9 composition/zero-request, 2 orchestrator)
  Web             172/172   (+27 new: 10 telemetry unit, 11 live telemetry,
                             6 composition-root / DI)

The first independent review re-ran build and test itself in Release at the
pre-correction state and confirmed 1484/1484 with 0 warnings.

Targeted runs (all PASS before the full run):
  ProfileRepositoryCollectionCompositionTests            9/9
  GitHubRepositorySnapshotOrchestratorTests             17/17
  ProfileEnrichmentOmissionEquivalenceTests               5/5
  ProfileExecutionTelemetryTests                        10/10
  LiveProfileReportServiceTelemetryTests                11/11
  GitHubProof.Web.Tests (full)                        172/172

Existing enrichment isolation tests
PASS — unchanged and green

git diff --check (incl. untracked, via git add -N then reset)
PASS — no whitespace error

Controlled live GitHub smoke (user-run)
FAILED — see §8b. Automated validation above is unaffected and still PASS.
~~~

## 8b. Controlled Live Smoke — FAILED

The user ran the controlled live smoke. It did **not** produce a Profile Proof
result.

~~~text
Input:            fatihGunalp
Runtime:          > 3 minutes, no Profile Proof result
Observed sends:   at least 842 outbound GitHub GET send events before abort
Dominant pattern: repeated commit pagination for repository id 1079771485
                  (fatihGunalp/fatih-azure-sdk-for-net)
Telemetry line:   not produced — the process was terminated before
                  LiveProfileReportService's finally block ran
Process state:    stopped
~~~

### Rate-limit reading is NOT a contradiction

The later GitHub reading was `core: limit=5000, used=47, remaining=4953,
reset=1787271349` (2026-08-21 03:15:49 +03:00). That counter was read **after
the reset boundary**, so `used=47` belongs to the new primary-rate-limit window
and must not be compared with the 842 earlier send events. Request amplification
and primary-quota consumption are separate facts and this run does not put them
in conflict. Nothing here indicates a telemetry counting defect.

### Blocker identified: unbounded required-surface pagination

The live run did not disprove the Session 097 optimization — it exposed a
different, pre-existing blocker. A read-only pagination audit over the six
paginated surfaces found:

~~~text
commits        unbounded pagination; CommitHistory and CommitDistribution both
               require IsComplete BEFORE their ordinary decision paths, so no
               early stop is available under current semantics
issues         maximum decision becomes irreversible at >= 2 qualifying issues
pull requests  maximum decision becomes irreversible at >= 2 merged PRs
releases       maximum decision becomes irreversible at the first published
               release with notes
tags           current ReleaseUsage semantics can be decided from one page
contributors   no current production consumer was found
~~~

Commits remain the dominant blocker: an exact non-Strong `CommitDistribution`
result can require the full history under the current contract, because its
`activeDayCount` has no bounded exact substitute.

The audit was read-only. No file was changed by it, and it authorizes no
implementation.

## 9. Problems

~~~text
BLOCKING — Profile runtime readiness is BLOCKED.

Blocker:
unbounded required-surface pagination (see §8b)

The blocker is not a defect in this session's implementation. The distinction
that holds across this file and CURRENT.md is:

  Implementation / automated proofs   PASS WITH FINDINGS
  Controlled live smoke              FAILED
  Profile runtime readiness          BLOCKED
  Blocker                            unbounded required-surface pagination

Every Session 097 deliverable still stands: the two enrichment surfaces are
provably omitted from the Profile composition, Repository Proof is unchanged,
both semantic-equivalence proofs pass, the telemetry foundation is in place and
the cancellation corrections are retained. What the live run exposed is a
pre-existing surface this optimization never touched — commits, issues, pull
requests, releases, tags and contributors all paginate without bound, and
commits cannot be stopped early under the current evaluator contract.
~~~

Non-blocking note from implementation: the contents collector legitimately
issues its own git/commits and git/trees requests, so a naive "no /git/trees/
requests" assertion would have been the exact brittle test the task warned
against. The zero-request proof is therefore exact request-count and
set-difference based, run against a deterministic GitHub fixture with both
compositions.

## 10. Not Done

~~~text
- Internal execution guard (no mechanism, no numeric value).
- Bounded concurrent-profile-execution policy.
- Any concurrency, fan-out, caching, persistence or pagination change.
- A successful §9 completion-telemetry measurement. The controlled live smoke
  WAS run and it FAILED: the process was terminated before the completion line
  was emitted, so no measurement was obtained (§8b).
~~~

## 11. Known Risks

~~~text
- Profile runtime readiness is BLOCKED. The removed enrichment surfaces are
  proven removed, but a real profile run still cannot finish because the
  required surfaces themselves paginate without bound.
- The per-profile request cost is still unmeasured: the live run aborted before
  any telemetry line was produced, so no §9 measurement exists yet and no §10
  guard numeric value can be chosen.
- The telemetry log line is Information-level and unconditional. If a future
  deployment runs at scale it may deserve a sampling or level decision; that is
  out of scope here.
~~~

## 12. Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
- src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
- src/GitHubProof.Web/Program.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositorySnapshotOrchestratorTests.cs
- tests/GitHubProof.Web.Tests/ProfileCompositionRootTests.cs
- tests/GitHubProof.Web.Tests/ProfilePipelineServiceCollectionExtensionsTests.cs
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Web/Profile/Runtime/ (5 files)
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/FakeGitHubApiHandler.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/ProfileRepositoryCollectionCompositionTests.cs
- tests/GitHubProof.Application.Tests/Profile/ProfileEnrichmentOmissionEquivalenceTests.cs
- tests/GitHubProof.Web.Tests/ProfileExecutionTelemetryTests.cs
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTelemetryTests.cs
- worklog/sessions/2026-08-21-097-profile-runtime-collection-implementation.md

Modified after the independent review (F2):
- src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetry.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileRuntimeTelemetryDecorators.cs
- tests/GitHubProof.Web.Tests/ProfileExecutionTelemetryTests.cs
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTelemetryTests.cs

Pre-existing unrelated:
- NONE
~~~

## 13. Next

~~~text
Session 098 — Profile Decision-Sufficient Pagination Contract
(bounded CONTRACT AMENDMENT / DECISION session — NOT implementation)
~~~

Session 098 must decide how Profile Proof may stop required-surface pagination
**only** when the existing V1 semantic decision has already become
mathematically irreversible, without pretending the underlying collection is
complete.

Locked constraints for that session:

~~~text
- no fake IsComplete = true
- no arbitrary hard page cap
- no sampling and no approximation
- no concurrency-as-cost-fix
- Repository Proof unchanged
- fork semantics are a separate product decision and are NOT changed by 097
- contributors omission requires an explicit amendment of Session 096 §5.4
- commits need a contract change before any implementation
~~~

Open contract questions it has to answer, at minimum: what `IsComplete` means
when collection stopped because the decision was already pinned; how that state
avoids today's `isComplete=false` -> CollectionWarning -> limitation ->
`AnalysisStatus.Partial` chain; and what the evaluators' evidence values should
report when the observed counts are bounded by decision rather than by the
repository.

## 14. Git

~~~text
Suggested commit:
feat(profile): omit non-scoring enrichment from profile collection

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

## 15. Completion Checklist

~~~text
[x] Zero source/dependency requests on the Profile path (proven)
[x] Repository Proof composition and behavior unchanged (proven)
[x] Repository-level semantic equivalence (proven)
[x] Profile-level semantic equivalence through ProfileReport (proven)
[x] Omission produces no warning / limitation / status change (proven)
[x] Existing isolation tests unchanged and green
[x] Telemetry with §9 fields and §9.1 prohibitions (proven)
[x] Cancellation preserved, never converted (proven)
[x] No repository cap, ranking, sampling, fan-out, cache or persistence
[x] Execution guard NOT implemented, no numeric value chosen
[x] Full build + test PASS, 0 warnings
[x] git diff --check PASS
[x] Independent review run; both MEDIUM findings closed, 3 LOW recorded
[x] Second review pass: 3 further MEDIUM findings closed (2 LOW still open)
[x] Session Result / INDEX / CURRENT updated (verified against git status)
[x] Controlled live GitHub smoke — RUN by the user; FAILED (see §8b)
[ ] Profile runtime readiness — BLOCKED on Session 098's contract amendment
~~~

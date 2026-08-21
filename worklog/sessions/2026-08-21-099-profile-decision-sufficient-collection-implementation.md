# Session 099 — Profile Decision-Sufficient Collection Implementation

**Date:** 2026-08-21
**Result:** SESSION 099 RUNTIME EXIT GATE PASSED — automated implementation and controlled live validation COMPLETE
**Scope:** domain / infrastructure / web / tests
**Model Route:** Claude / Opus / xhigh
**Model Gate:** APPROVED — user wrote `devam et`

---

## 1. Goal

Implement the contract locked by Session 098: a Profile V1 runtime path that
skips known forks before deep collection, stops paginated surfaces only when the
authorized V1 decision is already mathematically irreversible, never fakes
enumeration completeness, and leaves Repository Proof full-enumeration behavior
untouched.

## 2. Starting State

~~~text
Branch:
session/099-profile-decision-sufficient-collection-implementation

Base HEAD:
7fadd11 — matches the expected SHA given in the handoff

Pre-existing Working Tree:
CLEAN

Relevant Previous Session:
worklog/sessions/2026-08-21-098-profile-decision-sufficient-pagination-contract.md
~~~

## 3. Implementation

### 3.1 `CollectionObservationState` (Domain)

A closed three-value enum (`Enumerated` / `DecisionBounded` / `Incomplete`) plus
an optional trailing `RepositoryCollection<T>` constructor parameter, so every
pre-existing caller keeps meaning exactly what it meant:
`isComplete: true` → `Enumerated`, `isComplete: false` → `Incomplete`.

The invariants are enforced **structurally**, not by convention:

~~~text
(state == Enumerated) != isComplete              -> ArgumentException
DecisionBounded with a non-null TotalCount       -> ArgumentException
Enumerated with a total != Items.Count           -> ArgumentException
undefined state                                  -> ArgumentOutOfRangeException
~~~

So no code path can fabricate `IsComplete = true`, and no bounded collection can
claim a total. `RepositoryCollection<T>.DecisionBounded(items)` is the single
construction path a stopping collector uses.

### 3.2 The decision-stop seam

`RepositoryCollectionDecisionBoundary` (Domain, new
`Analysis/Collection/`) exposes one predicate per surface and **delegates every
threshold to the canonical evaluator that owns it**:

~~~text
AreCommitDecisionsIrreversible   -> CommitHistorySignalEvaluator.IsStrongThresholdReached
                                    + CommitDistributionSignalEvaluator.Observe
                                    + CommitDistributionSignalEvaluator.IsStrongThresholdReached
IsIssueDecisionIrreversible      -> IssueUsageSignalEvaluator.IsTerminalThresholdReached
IsPullRequestDecisionIrreversible-> PullRequestUsageSignalEvaluator.IsTerminalThresholdReached
IsReleaseDecisionIrreversible    -> ReleaseUsageSignalEvaluator.IsTerminalThresholdReached
IsTagDecisionIrreversible        -> ReleaseUsageSignalEvaluator.IsTagContributionPinned
~~~

The numbers 10, 8, 14, 2, 2 and 1 exist in exactly one place each — the
evaluator — and were extracted into named constants there. **No collector
contains a threshold.** Two delegate types (`CommitCollectionDecisionBoundary`,
`CountCollectionDecisionBoundary`) carry the boundary into Infrastructure as an
optional constructor parameter; a composition that supplies none enumerates
fully. That is how Repository Proof keeps its behavior without any flag.

### 3.3 Commit signals

Both evaluators now admit a terminal Strong result from a `DecisionBounded`
collection and keep rejecting an `Incomplete` one:

~~~text
CommitHistory        observed count >= 10                       -> Strong / 7
CommitDistribution   active days >= 8 AND span >= 14 days       -> Strong / 8
~~~

Below those thresholds a bounded collection still yields `UnavailableEvidence`,
because the exact count matters there and no provider field can establish it.

Evidence for a bounded collection uses lower-bound vocabulary
(`observedCountAtLeast`, `activeDaysAtLeast`, `spanDaysAtLeast`,
`firstObservedCommitAt`, `observation=decision-bounded`) and never claims the
observed oldest commit is the repository's first. `Enumerated` and `Incomplete`
evidence are byte-identical to before.

### 3.4 Per-surface stop behavior

~~~text
commits        stop when count >= 10 AND active days >= 8 AND span >= 14 days
issues         stop when >= 2 qualifying issues observed
pull requests  stop when >= 2 merged pull requests observed
releases       stop when the first published release with notes is observed
tags           stop when >= 1 tag is observed
~~~

A stop only produces `DecisionBounded` when a `rel="next"` page actually exists;
if provider enumeration ends on that same page the result is `Enumerated`. A
stop condition that is never reached enumerates every page, exactly as today.
No page cap, no sampling, no approximation, no `rel="last"`.

Every intentional stop returns `DataAvailability.Available` and an **empty**
warning list, so `limitation.snapshot.collection-partial` is not emitted and
`RepositoryAnalysis.Status` is not downgraded by the bound. A transport failure
still produces `Incomplete` with its existing warning.

### 3.5 Contributors

Removed from the Profile composition entirely: the profile constructor now takes
eight collectors, `contributorCollector` is nullable, an omitted composition
never reaches it, contributes no availability entry and produces no warning.
Repository Proof's `CreateDefault` still collects contributors unchanged, and
`RepositorySnapshot.Contributors` was not removed.

### 3.6 Fork Policy B

`EligibilityPolicy` gained a `ForkAdmission` rule bound immutably to its version,
exactly as `CurrentActivityWindow` already is, plus a new canonical `V2`. Stage A
consults it:

~~~text
Empty = Present                      -> Skip / EmptyRepository   (unchanged, resolves first)
ForkAdmission = Skip AND Fork Present-> Skip / ForkDetected      (new)
Empty = Absent                       -> Analyze
otherwise                            -> Undetermined
~~~

`AnalysisAdmissionPolicy` takes an `EligibilityPolicy` and nothing else — there is
deliberately no version-only constructor, so a canonical version can never be
paired with fork behavior it does not own.

`AnalysisAdmissionResult` accepts `ForkDetected` as a Stage-A reason. That is a
required consequence of eligibility §6.3 — `ForkDetected` was already a canonical
reason identity under §15.2; only the stage that may report it changed.

`ProfileRepositoryAnalysisOrchestrator`, `CrossRepositoryEvidenceComposer`,
`ProfileComposer` and `ProfileReportProjector` are **untouched**: the orchestrator
simply never calls the snapshot source for a `Skip` decision.

### 3.7 Versions

~~~text
EligibilityPolicy.V2         "2.0.0"        major: a previously allowed
                                            contribution is withdrawn.
                                            CurrentActivityWindow unchanged at 365 days.
AnalyzerVersion              1.1.0 -> 1.2.0 snapshot normalization + evidence values
RuleSetVersion               2026.08.15 -> 2026.08.21   evaluator semantics
process.commit-history       Version 1 -> 2
process.commit-distribution  Version 1 -> 2
ReportSchemaVersion          1              unchanged
ProfileAnalyzerVersion       V1             deliberately NOT bumped
~~~

`RepositoryAnalysisComposer`'s canonical signal contract table was updated to the
two new signal versions so the metadata check still binds.

## 4. Changed Files

~~~text
Production — created:
- src/GitHubProof.Domain/Analysis/Enums/CollectionObservationState.cs
- src/GitHubProof.Domain/Analysis/Collection/RepositoryCollectionDecisionBoundary.cs
- src/GitHubProof.Domain/Profile/Enums/ForkAdmissionRule.cs

Production — modified:
- src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
- src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
- src/GitHubProof.Domain/Analysis/Signals/CommitHistorySignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/CommitDistributionSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/IssueUsageSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/PullRequestUsageSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/ReleaseUsageSignalEvaluator.cs
- src/GitHubProof.Domain/Profile/AnalysisAdmissionPolicy.cs
- src/GitHubProof.Domain/Profile/Models/AnalysisAdmissionResult.cs
- src/GitHubProof.Domain/Profile/ValueObjects/EligibilityPolicy.cs
- src/GitHubProof.Application/Analysis/RepositoryAnalysisComposer.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryCommitCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryIssueCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryPullRequestCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryReleaseCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTagCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
- src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
- src/GitHubProof.Web/Program.cs

Tests — created:
- tests/GitHubProof.Domain.Tests/Analysis/Models/CollectionObservationStateTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/CommitSignalDecisionBoundedTests.cs
- tests/GitHubProof.Domain.Tests/Profile/ForkAdmissionPolicyTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/DecisionBoundedCollectionTests.cs
- tests/GitHubProof.Application.Tests/Profile/ForkPolicyAndDecisionBoundedProfileTests.cs

Additional files touched by the round-3 corrections:
- tests/GitHubProof.Application.Tests/Profile/ProfileAnalysisTestSupport.cs
  (added the non-canonical TestEligibilityPolicy)
- tests/GitHubProof.Application.Tests/Profile/CrossRepositoryEvidenceComposerTests.cs
- tests/GitHubProof.Application.Tests/Profile/CrossRepositoryLanguageEvidenceTests.cs
- tests/GitHubProof.Application.Tests/Profile/ProfileRepositoryAnalysisOrchestratorTests.cs
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTests.cs
- tests/GitHubProof.Web.Tests/ProfileModelTests.cs
  (all six migrated from the removed version-only constructor)

Tests — modified (all truthfully updated, none weakened):
- tests/GitHubProof.Domain.Tests/Analysis/Signals/CommitHistorySignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/CommitDistributionSignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Profile/AnalysisAdmissionPolicyTests.cs
- tests/GitHubProof.Domain.Tests/Profile/AnalysisAdmissionResultTests.cs
- tests/GitHubProof.Application.Tests/Analysis/RepositoryAnalysisComposerTests.cs
- tests/GitHubProof.Application.Tests/Analysis/Presentation/RepositoryReportProjectorTests.cs
- tests/GitHubProof.Application.Tests/Profile/ProfileCompositionTestSupport.cs
- tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationHarness.cs
- tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationRiskTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositorySnapshotOrchestratorTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/FakeGitHubApiHandler.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/ProfileRepositoryCollectionCompositionTests.cs
- tests/GitHubProof.Web.Tests/LiveProfileReportServiceTelemetryTests.cs
- tests/GitHubProof.Web.Tests/ProfilePipelineServiceCollectionExtensionsTests.cs
~~~

No canonical `docs/` file was changed: Session 098 already locked every rule this
session implements.

### 4.1 Superseded test expectations, updated not weakened

~~~text
AnalysisAdmissionResultTests   ForkDetected moved from the "rejected at Stage A"
                               theory into a new explicit theory that accepts all
                               three canonical admission reasons.
Commit signal Definition tests version literal 1 -> 2.
Composer / projector fixtures  the two commit signals' SignalSpec version 1 -> 2.
Orchestrator profile tests     the profile call list no longer contains
                               "contributors"; renamed accordingly.
Composition tests              contributors added to the removed-request set and
                               the delta became 5; a new test proves the full
                               composition still collects them.
ProfileValidationHarness       pinned to EligibilityPolicy.V2 — P9 validates
                               production truth, and the mostly-forks scenario
                               now asserts NonContributing / ForkDetected / no
                               AnalysisReference while the fork stays visible.
~~~

## 5. Validation

~~~text
dotnet build GitHubProof.slnx -c Release --no-restore
PASS — 0 Uyarı / 0 Hata

dotnet test GitHubProof.slnx -c Release --no-restore
PASS — 1552/1552 (final, after all three review rounds)

  Domain          834   (+36)
  Application     257   (+6)
  Infrastructure  288   (+18)
  Web             173   (+1)

git diff --check (incl. untracked, via git add -N then reset)
PASS — no whitespace error

No GitHub network request was made at any point.
~~~

## 6. Independent Review

`github-proof-review`, run twice.

**Round 1: BLOCKED — 1 BLOCKER, 1 HIGH, 1 MEDIUM, 2 LOW.**

~~~text
F1 BLOCKER  GitHubRepositorySnapshotOrchestrator.MapCollection dropped
            ObservationState, so a DecisionBounded issues / pull-requests / tags
            collection silently became Incomplete in the snapshot. The three
            evaluators' new lower-bound evidence branches were therefore
            unreachable dead code, and a deliberate bound was labelled a failure.
            The finding was correct and was the most important outcome of the
            review: the collector-level tests could not see it.
            FIX: MapCollection and both RepositorySnapshot normalizers now carry
            the state, plus two snapshot-level regression tests —
            DecisionBoundedCollections_KeepThatStateInTheSnapshot (all five
            surfaces) and IncompleteCollections_AreNeverPromotedToDecisionBounded.

F2 HIGH     The P9 validation harness ran EligibilityPolicy.V1 while production
            registers V2, so the mostly-forks scenario validated superseded fork
            semantics while claiming to be "the exact same production wiring".
            FIX: harness pinned to V2; the scenario now asserts the Session 098
            §4.5 truth and the fork's continued visibility.

F3 MEDIUM   Worklog not yet updated — closed by this file.
F4 LOW      A test name claimed "every purpose" where §22.1.1 keeps
            ProfileContext Allowed. Renamed.
F5 LOW      Dangling formatting in the fake GitHub handler. Fixed.
~~~

**Round 2: PASS WITH FINDINGS — 0 BLOCKER, 0 HIGH, 1 MEDIUM (F3, this file),
2 LOW.** Both new LOWs were fixed rather than accepted:

~~~text
N1 LOW      The ReleaseUsage Enumerated evidence value had its `tags=` field
            shifted one position by the bounded/enumerated split. §15.5 says
            Enumerated evidence changes in no way at all, so the value is now
            composed field-by-field and an Enumerated evaluation produces the
            byte-identical string it always did.
N2 LOW      A boundary test name was broader than the single direction it
            proved. Renamed, with a pointer to the test that proves the other
            direction.
~~~

Round 2 also re-confirmed clean: no fake `IsComplete=true`; `DecisionBounded`
never treated as failure; `Incomplete` never promoted; no exact total from
bounded evidence; no threshold duplicated in Infrastructure; no early-stop leak
into Repository Proof; no contributor request on the Profile path; no fork deep
collection; `TemplateDerived` unchanged; telemetry semantics untouched;
`ProfileAnalyzerVersion` not bumped; no page cap, sampling, hidden top-N or
concurrency; no weakened test.

**Round 3 (ChatGPT): BLOCKED — 0 BLOCKER, 1 HIGH, 2 MEDIUM, 2 LOW.** All five
were valid and all five are closed.

~~~text
F1 HIGH     AnalysisAdmissionPolicy still had a version-only constructor that
            defaulted to ForkAdmissionRule.Analyze, so a caller could pair
            EligibilityPolicyVersion "2.0.0" with V1 fork behavior — provenance
            claiming V2 while executing Analyze. That contradicted the
            version-ownership invariant EligibilityPolicy itself enforces.
            FIX: the version-only constructor is GONE. EligibilityPolicy is the
            only construction input, so the pairing is unconstructible rather
            than merely discouraged — EligibilityPolicy.Create already refuses
            to rebind a canonical version to a different rule. Every call site
            was migrated (production DI plus seven test sites); the Application
            tests gained a non-canonical `TestEligibilityPolicy` carrying the
            existing `TestPolicyVersion`, so they keep exercising Analyze
            exactly as before. Regression test
            NoAdmissionPolicyCanCarryV2ProvenanceWithAnalyzeSemantics proves the
            rebind is rejected, that a V2 policy evaluates a fork to Skip, and —
            by reflection over the constructor surface — that no overload can
            accept a bare version at all.

F2 MEDIUM   An Enumerated collection could still be constructed with an explicit
            TotalCount that disagreed with Items.Count, which is structurally
            impossible under the locked meaning of Enumerated.
            FIX: a third constructor invariant rejects it. Incomplete may still
            carry a provider-known total >= Items.Count, and DecisionBounded
            still rejects any total. Verified no production path breaks:
            MergeFileTree's Math.Max always resolves to files.Count, and both
            RepositorySnapshot normalizers pass normalized.Length.

F3 MEDIUM   The pinned-commit CollectAtCommit warning listed contributors among
            the sections "collected from live GitHub state" — a false factual
            claim on the Profile composition, where the contributor collector is
            null.
            FIX: the live-section list is composed from what the composition
            actually collected. Repository Proof's wording is byte-identical;
            the Profile wording simply ends at "releases, and tags"; the
            omission itself is still described nowhere and carries no warning of
            its own. Two tests cover both compositions.

F4 LOW      The Profile DI comment still said two surfaces were omitted.
            Corrected to three, and the CreateProfileComposition comment was
            tightened the same way.

F5 LOW      An admission-reason test paired RequiredInputUnavailable with Skip,
            normalizing a non-canonical pair as canonical.
            FIX: the theory now carries the decision alongside each reason
            (EmptyRepository -> Skip, ForkDetected -> Skip,
            RequiredInputUnavailable -> Undetermined), plus a new test proving
            the canonical producer only ever emits those three pairs. Production
            pairing permissiveness was not broadened.
~~~

**Round 4 (verification): PASS WITH FINDINGS — 0 BLOCKER, 0 HIGH, 1 MEDIUM
(worklog synchronisation, closed by this update), 1 LOW.** The reviewer
independently re-ran Release build and test and confirmed 1552/1552, verified
all five round-3 fixes against the code, and re-checked the full sixteen-item
risk list with no drift. The LOW was a duplicated comment block on
`TestPolicyVersion`, since removed.

## 7. Repository Proof Preservation

~~~text
CreateDefault still builds the full eleven-collector set, including Contributors,
SourceFiles and DependencyDeclarations, and supplies NO decision boundary, so its
paginated collectors still enumerate every page. Its collections can only be
Enumerated or Incomplete — DecisionBounded is unreachable there.
Proof: Commits_WithoutABoundary_EnumeratesEveryPage,
FullComposition_StillCollectsBothEnrichmentSurfaces,
ProfileComposition_IssuesNoContributorRequestAtAll (full arm),
and the untouched Repository Proof test suites.

The only Repository Proof-visible change is the explicitly versioned evaluator
metadata Session 098 required: the two commit signals' Version and the
Analyzer/RuleSet versions. Collection semantics, thresholds, scores, evidence
keys and Enumerated evidence values are unchanged.
~~~

## 8. Problems

~~~text
None outstanding.

Both substantive defects were found by review rather than by the test suite, and
both are now guarded structurally rather than by convention:

- the dropped ObservationState (round 1) cannot recur because the
  RepositoryCollection constructor rejects every impossible state combination,
  including the mismatched Enumerated total added in round 3;
- the version/behaviour mismatch (round 3) cannot recur because
  AnalysisAdmissionPolicy has no constructor that accepts a bare version.
~~~

## 9. Not Done

~~~text
- Execution guard: still NOT IMPLEMENTED, numeric values remain UNSELECTED.
- Bounded concurrent-profile-execution policy: not implemented.
- No target request count or duration was promised or documented; the previous
  measurements (~5m31s / no result, and >3m / >=842 sends / no result) are
  historical failure baselines only.
~~~

## 10. Known Risks

~~~text
- The primary and secondary real-profile runs completed; their measurements are
  validation observations, not universal performance targets or an SLA.
- A very large NON-fork repository whose commit distribution never pins still
  enumerates in full by design. The execution guard remains the only outer limit
  and still has no value.
- The Profile path now runs EligibilityPolicy.V2, which withdraws fork
  contributions. Any consumer comparing a pre-Session-099 profile result with a
  new one will see fork-derived traceability disappear — an expected eligibility
  semantic change, not a regression.
~~~

## 11. Live State

~~~text
Automated implementation status:
COMPLETE — build PASS (0 warning / 0 error), 1552/1552 tests PASS,
independent review PASS WITH FINDINGS with every code finding closed

Controlled live smoke:
PASS — primary exit-gate profile `fatihGunalp`; browser result rendered and
HTTP POST `/Profile` returned 200

Profile runtime readiness:
COMPLETE
~~~

## 12. Next

The Session 099 runtime exit gate is passed. P10-I3 — Meaning / Direction /
Starting Point — is the next slice. Execution-guard numeric values remain
unselected.

### 12.1 Controlled live validation

Primary `fatihGunalp` measurement (canonical exit gate):

~~~text
DurationMs=19523
GitHubRequests=63
RepositoriesDiscovered=5
RepositoriesAdmitted=4
RepositoriesCompleted=4
RepositoriesIncomplete=0
Status=Complete
Outcome=composed
RepositoryOutcomes={}
RequestCategories=profile-metadata=1, profile-repositories=1,
repository-commits=4, repository-contents=13, repository-git-commit=4,
repository-git-reference=4, repository-git-tree=4, repository-issues=4,
repository-languages=4, repository-license=4, repository-metadata=4,
repository-pull-requests=4, repository-readme=4, repository-releases=4,
repository-tags=4
ContributorDeepRequests=0
HistoricalBlockerForkDeepRequests=0
TokenLeakCheck=0
~~

Secondary `torvalds` validation also passed: DurationMs=61438,
GitHubRequests=136, RepositoriesDiscovered=12, RepositoriesAdmitted=9,
RepositoriesCompleted=9, RepositoriesIncomplete=0, Status=Complete,
Outcome=composed, RepositoryOutcomes={}. Its `other=26` classification is
recorded only as an observation/follow-up candidate, not a blocker or target.

Neither run establishes a universal request-count target or latency SLA.

## 13. Git

~~~text
Suggested commit:
feat(profile): implement decision-sufficient collection and fork policy

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

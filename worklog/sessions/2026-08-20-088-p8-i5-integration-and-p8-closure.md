# P8-I5 — Integration and P8 Closure

**Date:** 2026-08-20
**Result:** PASS, then PASS WITH FINDINGS (1 MEDIUM, corrected in-slice — see
"Post-Review Bounded Correction" below), then external closure audit PASS
(see "External Closure Audit" below). P8-I5 slice complete. **P8 COMPLETE.**
Additive Application/Infrastructure/Web implementation; no canonical docs
changed by the implementation slice itself (the closure audit's one
required correction — a docs-only roadmap status sync — is recorded
separately below); no existing Domain/Application file's behavior changed
beyond the finding's own correction (two `ValueObjects` files gained an
additive `V1` static member each; one `Infrastructure` `.csproj` gained a
`ProjectReference` to `Application`).
**Scope:** Single Application-level pipeline wiring P3 → P8-I1 → P5 → P6 →
P7 → P8-I3 → P8-I4 into one running composition path; production `V1`
runtime version values; the smallest production `IProfileRepositorySnapshotSource`
adapter; minimal Web DI wiring.
**Model Route:** sonnet / high (INVARIANT_HEAVY — bounded integration of
already-locked contracts, but same-run reference-identity trace and
Complete/Partial/Failed accounting both had to survive the wiring intact)
**Model Gate:** APPROVED — user wrote `devam et`

Per the user's explicit task instructions, no `/github-proof-review` was run
for this slice.

---

## Verified Baseline

```text
branch (at gate)  main
working tree      clean
HEAD              b2b8624  feat(profile): project typed public evidence report
origin/main       b2b8624  (identical)

P8-I1  COMPLETE
P8-I2  COMPLETE
P8-I3  COMPLETE
P8-I4  COMPLETE
```

Branched as `session/088-p8-i5-integration-and-p8-closure` from `main` @
`b2b8624`. Per the task instructions, this session performed no broad
repository/worklog research and did not reopen P8 semantics — the task
itself carried the complete integration order, the runtime-version
decisions left to implementation, and the exact required test list. Context
read was narrowly targeted at the existing P3/P5/P6/P7/P8-I1/P8-I3/P8-I4
production types themselves (constructors, method signatures, already-owned
invariants) rather than their canonical docs, which this slice does not
reinterpret.

---

## What Was Built

```text
add  src/GitHubProof.Application/Profile/ProfilePipeline.cs
add  src/GitHubProof.Application/Profile/ProfilePipelineResult.cs
add  src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositorySnapshotSource.cs
add  src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
mod  src/GitHubProof.Domain/Profile/ValueObjects/ObservationRuleVersion.cs   (+ V1 = "1.0.0")
mod  src/GitHubProof.Domain/Profile/ValueObjects/PatternRuleVersion.cs      (+ V1 = "1.0.0")
mod  src/GitHubProof.Infrastructure/GitHubProof.Infrastructure.csproj      (+ ProjectReference -> Application)
mod  src/GitHubProof.Web/Program.cs                                        (+ AddProfilePipeline() call)

add  tests/GitHubProof.Application.Tests/Profile/ProfilePipelineTests.cs
add  tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GitHubProfileRepositorySnapshotSourceTests.cs
add  tests/GitHubProof.Web.Tests/ProfilePipelineServiceCollectionExtensionsTests.cs
```

### `ProfilePipeline` (Application)

The single Application-level composition path:

```text
GitHubProfileSnapshot
  -> ProfileRepositoryAnalysisOrchestrator.AnalyzeAsync            (P3)
  -> RepositoryEligibilityPolicy.Evaluate (per repository)         (P8-I1)
  -> CrossRepositoryEvidenceComposer.Compose(orchestration, ...)   (P5)
  -> ProfilePatternPromotionPolicy.Promote(evidence)                (P6)
  -> RepresentativeRepositorySelectionPolicy.Select(patternSet)     (P7)
  -> ProfileComposer.Compose(ProfileCompositionInput)               (P8-I3)
  -> ProfileReportProjector.Project(profile) [only if composed]     (P8-I4)
```

No stage's algorithm is reimplemented. Every constructor input is the real
production type; `ProfilePipeline` only sequences them and passes each
stage's own output object — never a recomposed equivalent — into the next,
which is what lets `ProfileComposer`'s own reference-identity coherence
checks (`PatternSetTracesToCrossRepositoryEvidence`,
`RepresentativeSelectionTracesToPatternSet`) succeed on a correctly-wired
run. Final repository eligibility is evaluated per repository directly
against `snapshot.CapturedAt` (never a system clock); the injected
`TimeProvider` is used only for the profile's audit-only `StartedAt`/
`CompletedAt`, which `DeveloperPublicEvidenceProfile` already documents as
excluded from semantic equality.

### `ProfilePipelineResult` (Application)

The pipeline's own outcome wrapper. **As corrected post-review** (see "Post-Review
Bounded Correction" below for the finding this replaced): the public
`Failed(composition)` factory accepts only a `Failed` composition and always
produces `Report = null`; the internal `Composed(composition, report)`
factory — usable only from within `GitHubProof.Application`, i.e. only
`ProfilePipeline` itself — accepts only a composed (non-`Failed`) composition
paired with its report. No public API accepts a `ProfileCompositionResult`
and a `ProfileReport` together, so a composed profile and report can no
longer be paired arbitrarily by an external caller. `ProfileComposer` itself
remains the sole owner of the Composed/Failed decision (already exhaustively
tested in P8-I3's `ProfileComposerTests`); this wrapper's only job is to
route that decision faithfully — including never turning a `Failed` into a
`Partial` — from within a closed construction boundary.

### `GitHubProfileRepositorySnapshotSource` (Infrastructure) — production adapter

Per the "PRODUCTION ADAPTER CHECK": `IProfileRepositorySnapshotSource` had
one existing usage (`ProfileRepositoryAnalysisOrchestrator`) and no
production implementation. The existing single-repository GitHub snapshot
collection path (`IGitHubRepositorySnapshotCollector` /
`GitHubRepositorySnapshotOrchestrator`, already used by
`LiveRepositoryReportService`) already collects exactly the `RepositorySnapshot`
shape this interface needs. The adapter performs no collection of its own —
it translates `ProfileRepositoryReference.Url` into the collector's URL
parameter and `GitHubRepositorySnapshotResult` into `ProfileRepositorySnapshotResult`,
preserving availability, failure and cancellation exactly as the wrapped
collector already reports them. No new collection semantics were
introduced; no semantic contract change was required, so this did not need
to stop.

This required one mechanical, additive `ProjectReference` from
`GitHubProof.Infrastructure` to `GitHubProof.Application` (the Infrastructure
project previously depended only on Domain): `IProfileRepositorySnapshotSource`
and `ProfileRepositorySnapshotResult` are Application types, and no existing
Infrastructure file needed to reference Application before this adapter. No
cycle: Application depends only on Domain.

### `AddProfilePipeline` (Web) — DI/wiring only

Registers the production `V1` runtime version values and the full pipeline
object graph (`AnalysisAdmissionPolicy`, `ProfileRepositoryAnalysisOrchestrator`,
`RepositoryEligibilityPolicy`, `RepositoryContributionPolicy`,
`CrossRepositoryEvidenceComposer`, `ProfilePatternPromotionPolicy`,
`RepresentativeRepositorySelectionPolicy`, `ProfileComposer`,
`ProfileReportProjector`, `ProfilePipeline`) behind the existing
`IGitHubRepositorySnapshotCollector`/`RepositoryAnalysisRunner`/`TimeProvider`
registrations `Program.cs` already had. It makes no eligibility, composition,
pattern-promotion, representative-selection or report decision of its own —
every one of those stays owned by the Application/Domain types it resolves.
No profile page/UI was added in this slice, per scope.

### Production `V1` runtime versions established

```text
EligibilityPolicyVersion                 EligibilityPolicy.V1               (Session 084, reused)
ProfileAnalyzerVersion                   ProfileAnalyzerVersion.V1          (Session 085, reused)
RepresentativeRepositoryRuleVersion      RepresentativeRepositoryRuleVersion.V1  (Session 075, reused)
ObservationRuleVersion                   ObservationRuleVersion.V1 = "1.0.0"     (NEW, this session)
PatternRuleVersion                       PatternRuleVersion.V1 = "1.0.0"         (NEW, this session)
```

`ObservationRuleVersion.V1`/`PatternRuleVersion.V1` follow the exact
precedent `RepresentativeRepositoryRuleVersion.V1` set in Session 075: a
`public static readonly` `"1.0.0"` instance on the value object itself, nothing
scattered. Canonical docs were checked for a conflicting concrete literal for
either version (`grep` across `docs/` and `worklog/CURRENT.md`); none exists —
every existing reference to these two identities is structural
(`ObservationRuleVersion`/`PatternRuleVersion` as a type), never a bound
version string, so establishing `"1.0.0"` here does not contradict anything
already locked. No stop condition was hit.

---

## Complete / Partial / Failed Behavior

- **Complete**: proven end to end — two independent repositories with real,
  promotable language evidence produce a `Complete` profile, a promoted
  `TechnicalFocus` pattern, a representative-repository selection, and a
  full report, with the `Unsupported` `ProblemAreaPatterns` section and the
  `Deferred` `RoleEvidenceAlignment` section both present without forcing
  `Partial` (`ProfilePipelineTests.RunAsync_CompleteFlow_ProducesCoherentProfileAndReport`).
- **Partial**: an unavailable/rate-limited repository collection still
  produces a `Partial` profile with the canonical `RepositoryEvidenceRateLimited`
  + `RepositoryEligibilityUndetermined` warnings, the `PartialEvidenceSurface`
  limitation, and a full report whose `Warnings` match the profile's
  (`RunAsync_UnavailableCollection_ProducesPartialWithLimitationsWarningsAndReport`).
- **Failed**: a correctly-wired pipeline structurally cannot produce
  incoherence (eligibility is evaluated 1:1 over `orchestration.Results`,
  which itself is already exactly-one-per-repository by construction;
  pattern set/selection are always fed forward by the same reference), so
  `ProfileComposer`'s own `Composed`/`Failed` decision — already exhaustively
  covered by P8-I3's `ProfileComposerTests` coherence suite — is out of this
  slice's reach to re-trigger honestly through `RunAsync`. What this slice
  owns and tests directly is the pipeline's own Failed-routing guard:
  `ProfilePipelineResult.Failed(...)` accepts only a `Failed` composition and
  always produces `Report = null`
  (`PipelineResult_Failed_HasNoProfileAndNoReport`,
  `PipelineResult_Failed_RejectsAComposedCompositionResult`), and no public
  API can pair an arbitrary composition with an arbitrary report at all
  (`PipelineResult_PublicApi_CannotConstructAnArbitraryComposedProfileAndReportPair`
  — the post-review correction; see "Post-Review Bounded Correction" below).
- **Sparse-but-resolved stays Complete**: a single, fully-resolved repository
  with no promotable pattern remains `Complete`, carrying only the
  `LimitedIndependentEvidenceSurface` limitation
  (`RunAsync_SparseSingleRepository_RemainsCompleteWithReport`).

---

## Determinism and Traceability

- `EligibilityPolicy.V1`'s activity cutoff is anchored to `snapshot.CapturedAt`,
  never the injected `TimeProvider`: a repository pushed 400 days before
  `CapturedAt` classifies `Historical` even when the pipeline's own clock is
  set so that using it instead would misclassify the same repository
  `Active` (`RunAsync_ActivityClassification_UsesSnapshotCapturedAt_NeverTheSystemClock` —
  empirically verified to fail when the guard is removed, then restored).
- Two independent runs over the same fixed snapshot and a fresh deterministic
  analysis-ID factory each produce a semantically identical profile and
  report — eligibility statuses, limitation/warning codes, promoted pattern
  identities, the representative-repository selection, report section
  availability, conclusion pattern keys and repository drill-down entries all
  match, and the deterministic analysis IDs themselves match
  (`RunAsync_SameFixedInputAndDeterministicIds_IsReplayDeterministic`).
- Repository order in the input snapshot does not alter the semantic result
  (`RunAsync_RepositoryOrderInSnapshot_DoesNotAlterSemanticResult`) — this is
  also structurally guaranteed one layer down by `ProfileRepositoryManifest`'s
  own canonical Owner/Name sort, exercised here end to end through the real
  pipeline rather than only at the Domain unit level.
- The trace `Report.WhyThisConclusion` → `Pattern` (same reference) →
  `SupportingEvidence` (`ProfileEvidenceReference`) → `RepositoryAnalysisReference`
  survives into the report unbroken, verified against a real
  `RepositoryAnalysis.AnalysisId` produced by the actual orchestrator run
  (covered inside the Complete-flow test).

---

## Validation

```text
Initial slice:
targeted (ProfilePipelineTests)                                    7/7
targeted (GitHubProfileRepositorySnapshotSourceTests)               4/4
targeted (ProfilePipelineServiceCollectionExtensionsTests)          4/4
Application (full)                                                158/158  (151 baseline + 7 new)
Domain (full, unchanged)                                           798/798
Infrastructure (full)                                              259/259 (255 baseline + 4 new)
Web (full)                                                          72/72  (68 baseline + 4 new)
Full solution suite                                              1287/1287 (1272 baseline + 15 new)
Full solution build (GitHubProof.slnx)                        0 warning / 0 error
git diff --check                                                        clean

Post-review bounded correction:
targeted (ProfilePipelineTests)                                    9/9
Application (full)                                                160/160
Full solution build (GitHubProof.slnx)                        0 warning / 0 error
Full solution suite                                              1289/1289
git diff --check                                                        clean
```

One guard was empirically isolated by temporarily removing it and confirming
the corresponding test fails, then restoring the fix and re-confirming green:
`RunAsync_ActivityClassification_UsesSnapshotCapturedAt_NeverTheSystemClock`
against a temporary `snapshot.CapturedAt` → `timeProvider.GetUtcNow()`
substitution in `ProfilePipeline.EvaluateEligibility`.

---

## Post-Review Bounded Correction

An external reviewer flagged 1 MEDIUM finding against the initial slice:
`ProfilePipelineResult.Create(ProfileCompositionResult, ProfileReport?)` was
`public` and accepted the composition and the report as two independent
parameters. Its null/shape guards proved only "a composed result has a
report and a failed one does not" — they proved nothing about *provenance*,
so an external caller could legally call `Create(profileA.Composition,
profileB.Report)` and get back a `ProfilePipelineResult` pairing one
profile's composition with a different profile's report. `ProfilePipeline.RunAsync`
itself only ever passed the correct same-run pair, so this was a public-API
exposure risk, not a live production defect.

**Correction applied** (`src/GitHubProof.Application/Profile/ProfilePipelineResult.cs`):

- `Create` removed. Replaced with two narrower factories:
  - `public static Failed(ProfileCompositionResult composition)` — accepts
    only a `Failed` composition (throws `ArgumentException` otherwise) and
    always produces `Report = null`. This is the only public construction
    path, and it carries no `ProfileReport` parameter at all, so it cannot be
    used to attach one.
  - `internal static Composed(ProfileCompositionResult composition, ProfileReport report)` —
    accepts only a composed (non-`Failed`) composition; visible solely inside
    `GitHubProof.Application`, so only `ProfilePipeline` — the one place that
    actually produced both from the same run — may pair them.
- `ProfilePipeline.RunAsync` updated mechanically to call `Failed(...)` or
  `Composed(...)` instead of `Create(...)`; no other pipeline semantics
  changed (same stage order, same reference-identity trace, same
  Complete/Partial/Failed accounting).
- No duplicate report/profile semantic-comparison logic was added, no
  `Profile` back-reference was added to `ProfileReport`, and the P8-I4 report
  contract was not touched.

**Tests changed** (`tests/GitHubProof.Application.Tests/Profile/ProfilePipelineTests.cs`):

- Removed: `PipelineResult_NeverAttachesAReportToAFailedComposition` (used
  the now-removed `Create` API).
- Added: `PipelineResult_Failed_HasNoProfileAndNoReport` — `Failed(...)` on a
  `Failed` composition yields `Status = Failed`, `Profile = null`,
  `Report = null`.
- Added: `PipelineResult_Failed_RejectsAComposedCompositionResult` —
  `Failed(...)` throws `ArgumentException` when given a composed
  (non-`Failed`) composition.
- Added: `PipelineResult_PublicApi_CannotConstructAnArbitraryComposedProfileAndReportPair` —
  reflects over every public constructor/static factory on
  `ProfilePipelineResult` and asserts none accepts both a
  `ProfileCompositionResult` and a `ProfileReport`; separately asserts the
  `Failed` factory's parameter list contains no `ProfileReport` parameter.
  Empirically verified: temporarily reintroducing the old public `Create`
  method made this test fail with the exact expected message, confirming the
  guard is real; the temporary method was then removed and the suite
  re-confirmed green.
- All Complete/Partial/determinism/traceability tests from the initial slice
  remain unchanged and green.

Net Application test count: 160 (158 prior + 3 new − 1 removed).

---

## External Closure Audit

A separate, independent closure audit reviewed the corrected implementation
against seven criteria: scope completeness, evidence integrity, determinism,
boundary review (no score/seniority/hiring/capability/AI-judgment claims),
API integrity, test coverage adequacy, and documentation consistency.

**Verdict: PASS**, with exactly one required, non-blocking correction:
`docs/roadmap/mvp-phases.md` still carried stale `P8 implementation: READY` /
`READY (not started)` status lines (three locations) that contradicted the
worklog's `COMPLETE` claim. No engineering gap was found; no architectural
decision was reopened.

That correction has been applied in this same docs-only pass: the three
stale `READY`/`READY (not started)` P8 status references in
`docs/roadmap/mvp-phases.md` now read `COMPLETE`, matching the implementation
state, with no surrounding prose rewritten. No source, test, or Domain/
Application/Infrastructure/Web file was touched by this pass.

---

## Remaining Gap

None found. No canonical docs contradiction was encountered; none was
changed. `RoleEvidenceAlignment` remains DEFERRED per the standing P7
decision (Session 076/077) — unaffected by this slice, correctly Deferred
end to end through the real pipeline. `ProblemArea` remains Unsupported per
the standing P8-1 decision — unaffected by this slice, correctly Unsupported
end to end through the real pipeline.

---

## P8 Closure

```text
P8-I1 Final Repository Eligibility Runtime:  COMPLETE
P8-I2 Profile Root Foundation:               COMPLETE
P8-I3 Deterministic Profile Composer:        COMPLETE
P8-I4 Typed Profile Report and Projector:    COMPLETE
P8-I5 Integration and P8 Closure:            COMPLETE

P8 — Deterministic Profile Composer and Report Contract:  COMPLETE
```

The 1 MEDIUM finding against the initial slice was corrected and validated
(build 0/0, full suite 1289/1289, targeted guard test empirically verified),
and the external closure audit passed. P8 is unconditionally closed.

Not marked complete by this session (explicitly out of scope): P9 — Public
Profile Evidence Validation, and Milestone B.

## Exact Next Action

```text
P9 — Public Profile Evidence Validation
```

Git transition (branch → `main`) for this session is user-owned; working
tree state at session end must be read directly from Git, not from this
file.

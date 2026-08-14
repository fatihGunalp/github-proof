# ScoreBreakdown Domain Model

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain
**Model Route:** Terra / high
**Model Gate:** APPROVED - user wrote `devam et`

---

## Goal

Implement the contract-derived `ScoreBreakdown` domain model and focused invariant tests.

## Starting State

~~~text
Branch: main
Base HEAD: c3a5192
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-003-category-result.md
~~~

## Context Read

~~~text
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
worklog/CURRENT.md
docs/architecture/analysis-domain-model.md
docs/product/scoring-and-report-contract.md
src/GitHubProof.Domain/Analysis/Models/CategoryResult.cs
src/GitHubProof.Domain/Analysis/Models/SignalResult.cs
~~~

## Contract / Invariants Checked

- Overall calculation is RawApplicableScoreNormalization.
- Only scored signals that opt into overall scoring contribute to numerator and denominator.
- Unscored and overall-excluded signals stay visible as distinct exclusions.
- Exact halves round upward.
- A zero applicable maximum produces neither a normalized score nor a score label.

## Changes

Created:

- `ScoreCalculationMethod` enum with the contract-locked raw applicable-score normalization method.
- `ScoreBreakdown` aggregate with raw, applicable-maximum, normalized-score, label, and exclusion views.
- Four focused invariant tests.

Modified:

- Worklog current state and index.

## Decisions

- Overall scoring is derived from category-owned signal results, never from category percentages.
- A signal excluded from overall remains visible in `ExcludedFromOverallScoreSignals`; unscored statuses remain visible in `ExcludedSignals`.
- A zero applicable maximum yields null normalized score and null label rather than a fabricated score.
- Score labels use the canonical Turkish report-contract labels.

## Validation

~~~text
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore
PASS - 17 passed, 0 failed, 0 skipped

dotnet test GitHubProof.slnx --no-restore
PASS - 17 passed, 0 failed, 0 skipped

dotnet build GitHubProof.slnx --no-restore
PASS - 0 warnings, 0 errors

dotnet format GitHubProof.slnx --verify-no-changes --no-restore
PASS

git diff --check
PASS
~~~

## Problems

None.

## Next

Prepare the next domain-model task and run its mandatory model gate before implementation.

## Git

~~~text
Suggested commit: feat(domain): add score breakdown model
Commit: PENDING - user-owned unless explicitly requested
Push: PENDING - user-owned unless explicitly requested
~~~

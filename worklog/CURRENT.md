# GitHub Proof — Current Development State

**Last Updated:** 2026-08-15
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

```text
Product Stage: MVP Implementation
Delivery Phase: Phase 1 — Analysis Model / Domain Foundation
Focus: AnalysisWarning domain model is implemented and verified.
```

---

## Active Session

```text
NONE
```

## Model Gate State

```text
NONE
```

## Current Task

```text
No active coding task.
```

## In Progress

```text
None.
```

## Current Problem

```text
None.
```

## Next Immediate Action

```text
Prepare the RepositoryAnalysis domain-model task and run its mandatory model gate before implementation.
```

## Working Set

```text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Domain/Analysis/Models/AnalysisWarning.cs
- tests/GitHubProof.Domain.Tests/Analysis/Models/AnalysisWarningTests.cs
- worklog/sessions/2026-08-15-008-analysis-warning.md
```

## Repository State

```text
Branch: main
Last verified HEAD: 050e496
Last verified working tree: completed Session 008 changes listed above
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 008 — AnalysisWarning domain model: PASS

- Added an immutable warning model with severity, message, and optional related references.
- Preserved warning visibility without requiring links that may not exist for every warning.
- Added four focused invariant tests.
```

## Last Validation

```text
dotnet test GitHubProof.slnx --no-restore: PASS — 38 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: only completed Session 008 changes
```

## Critical Invariants

```text
15 MVP signals; 4 categories.
Category-internal maximum: 91.
Overall-included maximum: 86.
Overall method: RawApplicableScoreNormalization.

Missing => Score 0 and denominator included.
NotApplicable / UnavailableEvidence / RequiresHumanReview
=> Score null and excluded from numerator and denominator.

portfolio.presentation-completeness is category-included and overall-excluded.
```

## Do Not Reinvestigate

```text
MVP signal count, category count, nullable score semantics, overall method,
and presentation-completeness exclusion are contract-locked in docs/.
```

## Known Problems

```text
None currently confirmed.
```

## Git Rule

```text
Codex may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
```

# GitHub Proof — Current Development State

**Last Updated:** 2026-08-15
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

```text
Product Stage: MVP Implementation
Delivery Phase: Phase 3 — Rule-Based Signal Engine
Focus: Three deterministic evaluator slices are complete; Phase 3 Signal Engine remains in progress.
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
Prepare the next bounded RepositoryUnderstandability evaluator task and run the mandatory model gate.
```

## Working Set

```text
Session 022 evaluator, tests, and worklog changes are uncommitted.
```

## Repository State

```text
Branch: main
Last verified HEAD: b38f8a9
Last verified working tree: Session 022 changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 022 — Description + License Signal Evaluators: PASS

- Added only `repository.description.present` and `repository.license.present` evaluators.
- Reused ISignalEvaluator and normalized RepositorySnapshot evidence without a generic engine.
- Preserved Missing versus UnavailableEvidence semantics with observable evidence.
- Used corrected LicenseFile evidence for deterministic Present, Partial, Missing, and UnavailableEvidence license outcomes.
- Kept signal versions, scoring contracts, and provider isolation unchanged.
- Full automated suite, build, format, and diff validation passed.
```

## Last Validation

```text
focused Description + License evaluator tests: PASS — 27 passed
README evaluator regression: PASS — 11 passed
RepositorySnapshot LicenseFile regression: PASS — 11 passed
Infrastructure orchestration regression: PASS — 10 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 87 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 79 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 166 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 022 changes only
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

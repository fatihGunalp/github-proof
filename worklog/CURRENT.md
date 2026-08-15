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
Focus: Four deterministic evaluator slices are complete; Usage or Demo is the next bounded Phase 3 signal.
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
None currently confirmed.
```

## Next Immediate Action

```text
Session 027 — `documentation.usage-or-demo`.
```

## Working Set

```text
Session 026 Domain evaluator, focused tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: ad5d0da
Last verified working tree: Session 026 changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 026 — Installation Guide Signal Evaluator Retry: PASS

- Added the canonical Version 1 `documentation.installation-guide` production evaluator.
- Evaluated exact ATX/Setext sections, actionable steps, and six Strong feature families deterministically.
- Kept Strong evidence source-local and docs ordering Ordinal.
- Preserved positive results from incomplete evidence while requiring complete corpus for Partial/Missing.
- Distinguished UnavailableEvidence and bounded RequiresHumanReview from missing evidence.
- Kept provider isolation, scoring, signal Version, and existing evaluator contracts unchanged.
```

## Last Validation

```text
focused InstallationGuide evaluator tests: PASS — 54 passed
README/Description/License evaluator regression: PASS — 38 passed
RepositorySnapshot regression: PASS — 13 passed
documentation collector + contents + orchestrator regression: PASS — 28 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 143 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 231 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 026 changes only
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

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
Focus: Phase 3 is in progress; License signal contract correction is required before Session 020 can continue.
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
BLOCKED — the snapshot does not preserve unrecognized license-file evidence independently from generic FileTree entries.
```

## Next Immediate Action

```text
Resolve the License evidence contract in canonical docs before running a new model gate for the resumed task.
```

## Working Set

```text
Session 020 worklog updates only; no production or test code was changed.
```

## Repository State

```text
Branch: main
Last verified HEAD: fe47257
Last verified working tree: Session 020 worklog updates only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 019 — Signal Engine Foundation + README Exists: PASS

- Added the minimal `ISignalEvaluator` Domain boundary.
- Implemented only `repository.readme.exists` from normalized RepositorySnapshot data.
- Preserved Missing versus UnavailableEvidence semantics with observable evidence.
- Added deterministic TimeProvider-based evaluation and canonical Present limitation.
- Full automated suite, build, format, and diff validation passed.
```

## Last Validation

```text
focused README evaluator tests: PASS — 11 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 57 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 76 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 133 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 019 changes only
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

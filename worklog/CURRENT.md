# GitHub Proof — Current Development State

**Last Updated:** 2026-08-15
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

```text
Product Stage: MVP Implementation
Delivery Phase: Phase 2 — GitHub Data Collection
Focus: RepositorySnapshot now carries tag evidence independently; snapshot orchestration is the next bounded Phase 2 task.
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
Run a new model gate for Session 017 — RepositorySnapshot Orchestration.
```

## Working Set

```text
Session 016 implementation and worklog updates are uncommitted.
```

## Repository State

```text
Branch: main
Last verified HEAD: d5424b8
Last verified working tree: Session 016 changes only; no pre-existing unrelated changes
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 016 — Tags Snapshot Contract Alignment: PASS

- Added Tags as a separate canonical RepositorySnapshot section and Phase 2 collection item.
- Added provider-independent `RepositoryTag` and an immutable Tags snapshot collection.
- Preserved independent Releases and Tags availability without changing release-usage signal semantics or version.
```

## Last Validation

```text
focused RepositorySnapshot tests: PASS — 8 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 46 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 68 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 114 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 016 changes only
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

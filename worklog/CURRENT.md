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
Focus: License evidence snapshot contract aligned; Phase 3 Signal Engine remains in progress.
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
Retry the bounded Description + License signal evaluator task using the corrected RepositorySnapshot license evidence contract.
```

## Working Set

```text
Session 021 snapshot contract, mapping, tests, docs, and worklog changes are uncommitted.
```

## Repository State

```text
Branch: main
Last verified HEAD: dce9f4b
Last verified working tree: Session 021 changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 021 — License Evidence Snapshot Alignment: PASS

- Added provider-independent `RepositoryFile? LicenseFile` to RepositorySnapshot.
- Preserved constructor source compatibility with a nullable trailing parameter.
- Mapped the existing descriptive LicenseFile into the snapshot without data loss.
- Verified recognized, unrecognized-file, absent, and unavailable license states remain distinct.
- Updated the canonical analysis domain model without changing signal semantics or version.
- Full automated suite, build, format, and diff validation passed.
```

## Last Validation

```text
focused RepositorySnapshot tests: PASS — 11 passed
focused descriptive/orchestrator tests: PASS — 16 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 60 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 79 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 139 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 021 changes only
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

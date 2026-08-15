# GitHub Proof — Current Development State

**Last Updated:** 2026-08-15
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

```text
Product Stage: MVP Implementation
Delivery Phase: Phase 2 — GitHub Data Collection COMPLETE
Focus: Phase 2 exit criterion passed; Phase 3 Signal Engine is next.
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
Prepare Phase 3 — Signal Engine first bounded task and run mandatory model gate.
```

## Working Set

```text
Session 018 opt-in live validation harness and worklog updates are uncommitted.
```

## Repository State

```text
Branch: main
Last verified HEAD: 25cec67
Last verified working tree: Session 018 validation-only changes; no pre-existing unrelated changes
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 018 — Phase 2 Final Integration Validation: PASS

- Full automated suite, build, format, and diff validation passed.
- Two bounded public repositories produced coherent live RepositorySnapshot values with independently verified head SHAs.
- A real GitHub 404 produced explicit unavailable metadata and no fake snapshot.
- Phase 2 Exit Criterion: PASS.
```

## Last Validation

```text
live integration: PASS — 2 snapshots + 1 not-found case
focused orchestration tests: PASS — 7 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 46 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 76 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 122 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 018 validation-only changes
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

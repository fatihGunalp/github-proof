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
Focus: Issue and pull-request collection is implemented; remaining Phase 2 collectors are pending.
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
Run a new model gate before the next bounded Phase 2 data-collection slice.
```

## Working Set

```text
Session 013 implementation and worklog updates are uncommitted.
```

## Repository State

```text
Branch: main
Last verified HEAD: badc250
Last verified working tree: Session 013 changes present; no pre-existing unrelated changes
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 013 — Issues and pull requests collection: PASS

- Added normalized issue and pull-request collectors with Link-header pagination.
- Excluded pull-request-shaped entries from issue data and preserved first-observed records by number.
- Kept unavailable, partial, rate-limited, forbidden, and not-applicable data explicit through availability and completeness state.
- Added sixteen focused issue and pull-request collection tests.
```

## Last Validation

```text
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 43 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 86 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 013 changes only
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

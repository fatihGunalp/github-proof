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
Focus: Releases, tags, and contributors collection is implemented; remaining Phase 2 collectors are pending.
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
Session 014 implementation and worklog updates are uncommitted.
```

## Repository State

```text
Branch: main
Last verified HEAD: 6b3d5a1
Last verified working tree: Session 014 changes present; no pre-existing unrelated changes
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 014 — Releases, tags, and contributors collection: PASS

- Added independent normalized release, tag, and contributor collectors with Link-header pagination.
- Preserved release notes, published/draft state, tags independent from releases, and contributors without anonymous identity expansion.
- Preserved usable data when later pages fail and retained each collection's own availability/completeness state.
- Added sixteen focused release, tag, and contributor collection tests.
```

## Last Validation

```text
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 59 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 102 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 014 changes only
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

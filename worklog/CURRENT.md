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
Focus: All 15 signals, Application orchestration, and the first frozen validation run are complete; Phase 3 final exit validation is next.
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
No active task.
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
Start Session 038 — Phase 3 Final Integration / Exit Validation after its mandatory model gate.
```

## Working Set

```text
Session 037 frozen manifest, 10-repository live artifacts, and deterministic analyzer coverage.
```

## Repository State

```text
Branch: main
Last verified HEAD: c7c2bb6
Last verified working tree: CLEAN before Session 037
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 037 — Validation Repository Set + Determinism: PASS

- Froze 10 distinct public validation repositories at real default/head commit SHAs.
- Ran the opt-in collection → snapshot → 15-signal pipeline successfully for all 10 repositories.
- Proved two fixed-time analyses of the same snapshot have zero semantic differences.
- Added per-repository validation-only artifacts while leaving all human review statuses `Pending`.
```

## Last Validation

```text
Repository baseline: PASS — main at c7c2bb6, clean before Session 037
Manifest/schema tests: PASS — 2 passed
Opt-in live validation: PASS — 10 repositories, 150 signals, 0 semantic differences
Domain tests: PASS — 420 passed
Application tests: PASS — 10 passed
Infrastructure tests: PASS — 90 passed
Solution tests: PASS — 520 passed
Solution build: PASS — 0 warnings, 0 errors
dotnet format verify: PASS
git diff --check: PASS
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
Repository sections outside default-branch contents remain live and non-atomic with the frozen head SHA; validation artifacts expose this Phase 2 limitation.
```

## Git Rule

```text
Codex may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
```

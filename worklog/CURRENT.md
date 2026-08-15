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
Focus: Ten production evaluators are complete; test-structure and continuous-integration signals are next.
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
Implement `validation.test-structure` and `validation.continuous-integration` in the next bounded session.
```

## Working Set

```text
Session 032 canonical clarification, three Domain evaluators, focused tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: 86dee23
Last verified working tree: CLEAN before Session 032
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 032 — Issue + Pull Request + Release Signals: PASS

- Confirmed normalized issue/PR separation, merge state, release/draft/notes data, tags, completeness, and independent availability.
- Clarified qualifying issue, release-note presence, and terminal incomplete-evidence semantics for Version 1.
- Added deterministic issue, pull-request, and release usage Domain evaluators.
- Covered all score branches, Issues-disabled, separate Releases/Tags failures, terminal positive evidence, ordering, and determinism.
- Production evaluators are now 10 / 15.
```

## Last Validation

```text
Repository baseline: PASS — main at 86dee23, clean before Session 032
Contract/data audit: PASS — issue, PR, release, tag, completeness, and availability data sufficient
Focused Issue + PR + Release tests: PASS — 46 passed
All evaluator regression: PASS — 261 passed
Domain tests: PASS — 312 passed
Infrastructure tests: PASS — 88 passed
Solution tests: PASS — 400 passed
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
None currently confirmed.
```

## Git Rule

```text
Codex may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
```

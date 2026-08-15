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
Focus: Seven production evaluators are complete; issue, pull-request, and release usage signals are next.
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
Implement `process.issue-usage`, `process.pull-request-usage`, and `process.release-usage` in the next bounded session.
```

## Working Set

```text
Session 031 canonical clarification, two Domain evaluators, focused tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: 17c74aa
Last verified working tree: CLEAN before Session 031
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 031 — Commit History + Commit Distribution: PASS

- Audited default-branch commit collection, pagination, UTC timestamps, metadata age, availability, and duplicate normalization.
- Clarified complete-only Version 1 scoring, snapshot-relative repository age, UTC active days, and elapsed first-last span.
- Added deterministic `process.commit-history` and `process.commit-distribution` Domain evaluators.
- Covered every score boundary, exact 14-day edges, incomplete/unavailable evidence, UTC day grouping, unsorted inputs, and deterministic output.
- Production evaluators are now 7 / 15.
```

## Last Validation

```text
Repository baseline: PASS — main at 17c74aa, clean before Session 031
Contract/data audit: PASS — required snapshot data and deterministic semantics confirmed
Focused Commit History + Distribution tests: PASS — 50 passed
All evaluator regression: PASS — 215 passed
Domain tests: PASS — 266 passed
Infrastructure tests: PASS — 88 passed
Solution tests: PASS — 354 passed
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

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
Focus: Usage/Demo evaluator implementation is blocked by a non-exhaustive canonical status contract.
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
Complete-corpus high-confidence visual/demo-only evidence has no canonical Usage/Demo status.
```

## Next Immediate Action

```text
Define the canonical status/score for high-confidence visual/demo evidence without clear usage, then retry the evaluator.
```

## Working Set

```text
Session 029 worklog changes only; production, test, and canonical docs are unchanged.
```

## Repository State

```text
Branch: main
Last verified HEAD: 9b1bf85
Last verified working tree: CLEAN before Session 029
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 029 — Usage/Demo Signal Evaluator: BLOCKED — CONTRACT CONTRADICTION

- Verified the clean main baseline at 9b1bf85 and reloaded the canonical Version 1 contract.
- Found that high-confidence screenshot/video/live-demo evidence without clear usage matches none of the six statuses.
- Confirmed the product boundary treats those artifacts as valid positive Usage/Demo evidence.
- Stopped before production/test implementation rather than choosing an undocumented status or score.
```

## Last Validation

```text
Repository baseline: PASS — main at 9b1bf85, clean before Session 029
Canonical status exhaustiveness review: BLOCKED — visual/demo-only high-confidence state is unclassified
Cross-document signal-boundary review: PASS
Production/test scope review: PASS — no src/** or tests/** changes
dotnet test/build/format: NOT RUN — implementation stopped before code changes
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
`documentation.usage-or-demo` Version 1 does not define a status for complete-corpus high-confidence visual/demo evidence when clear usage is absent.
```

## Git Rule

```text
Codex may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
```

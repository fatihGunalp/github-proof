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
Focus: All 15 production signals and Application orchestration are available; validation-set and cross-run determinism work is next.
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
Start Session 037 — Validation Repository Set + Signal Determinism after its mandatory model gate.
```

## Working Set

```text
Session 036 Application orchestration, derived presentation completeness, and focused tests.
```

## Repository State

```text
Branch: main
Last verified HEAD: 715114a
Last verified working tree: CLEAN before Session 036
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 036 — Application Signal Orchestration + Presentation Completeness: PASS

- Added deterministic Application orchestration for the exact 14 independent production evaluators.
- Added the derived `portfolio.presentation-completeness` Domain evaluator over five upstream signal results.
- Enforced missing, duplicate, unknown, and returned-key contract guards without reflection or a generic registry.
- Production signal output is now 15 / 15 in canonical order; the composite remains excluded from overall scoring.
```

## Last Validation

```text
Repository baseline: PASS — main at 715114a, clean before Session 036
Focused Presentation Completeness tests: PASS — 13 passed
Focused Application orchestration tests: PASS — 8 passed
All evaluator regression: PASS — 369 passed
Domain tests: PASS — 420 passed
Application tests: PASS — 8 passed
Infrastructure tests: PASS — 88 passed
Solution tests: PASS — 516 passed
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

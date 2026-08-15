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
Focus: Fourteen production evaluators are complete; Application orchestration and the derived presentation-completeness signal are next.
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
Session 036 — Application Signal Orchestration + `portfolio.presentation-completeness`.
```

## Working Set

```text
Session 035 canonical Homepage alignment, two Domain evaluators, focused tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: 14f4d48
Last verified working tree: CLEAN before Session 035
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 035 — Homepage Contract Fix + Portfolio Evaluators: PASS

- Closed the Homepage/Live Demo Version 1 semantic and availability-precedence gaps.
- Added deterministic Topics and Homepage/Live Demo Domain evaluators.
- Covered every score/status branch, URL/link exclusions, malformed ambiguity, availability, and determinism.
- Production evaluators are now 14 / 15.
```

## Last Validation

```text
Repository baseline: PASS — main at 14f4d48, clean before Session 035
Focused Topics + Homepage tests: PASS — 49 passed
All evaluator regression: PASS — 356 passed
Domain tests: PASS — 407 passed
Infrastructure tests: PASS — 88 passed
Solution tests: PASS — 495 passed
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

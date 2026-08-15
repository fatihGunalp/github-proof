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
Focus: Twelve production evaluators are complete; the three portfolio signals are next.
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
Implement `portfolio.topics`, `portfolio.homepage-or-live-demo`, and `portfolio.presentation-completeness` in the next bounded session.
```

## Working Set

```text
Session 033 canonical clarification, two Domain evaluators, focused tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: 63f3467
Last verified working tree: CLEAN before Session 033
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 033 — Test Structure + Continuous Integration: PASS

- Confirmed file-tree/project-content/workflow content, completeness, and availability evidence is sufficient.
- Clarified exact test indicators, evidence combinations, bounded run-command families, and YAML ambiguity semantics.
- Added deterministic Test Structure and Continuous Integration Domain evaluators.
- Covered every status/score branch, false positives, dependency evidence, malformed workflow content, incomplete evidence, ordering, and determinism.
- Production evaluators are now 12 / 15.
```

## Last Validation

```text
Repository baseline: PASS — main at 63f3467, clean before Session 033
Contract/data audit: PASS — test/project/workflow content and completeness data sufficient
Focused Test Structure + CI tests: PASS — 46 passed
All evaluator regression: PASS — 307 passed
Domain tests: PASS — 358 passed
Infrastructure tests: PASS — 88 passed
Solution tests: PASS — 446 passed
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

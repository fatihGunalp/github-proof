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
Focus: Four deterministic evaluators are complete; Usage/Demo implementation is blocked on canonical corpus and semantic rules.
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
`documentation.usage-or-demo` Version 1 is not executable: corpus scope conflicts and usage/demo/visual/link classification rules are underdefined.
```

## Next Immediate Action

```text
Define the canonical Version 1 Usage/Demo corpus and deterministic semantic contract before evaluator implementation.
```

## Working Set

```text
Session 027 worklog changes only; production and test code are unchanged.
```

## Repository State

```text
Branch: main
Last verified HEAD: 30640fc
Last verified working tree: Session 027 worklog changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 027 — Usage/Demo Signal Contract Audit: BLOCKED

- Confirmed normalized README content, homepage metadata, Markdown syntax, and availability are carried in the snapshot.
- Found a canonical corpus contradiction between README-only RequiredData and README-or-documentation evidence scope.
- Confirmed usage command, request/response, code/output, screenshot, video, demo, brittle-link, and Strong aggregation rules are not executable.
- Confirmed this signal has no bounded RequiresHumanReview triggers or complete status precedence.
- Made no production, test, canonical docs, snapshot, collector, or dependency changes.
```

## Last Validation

```text
Repository baseline: PASS — main at 30640fc, clean before Session 027
Raw data representation audit: PASS
Canonical corpus consistency audit: BLOCKED
Semantic determinism audit: BLOCKED
Production tests/build/format: NOT RUN — audit-only session with no production/test changes
git diff --check: PASS
git status --short: Session 027 worklog changes only
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
Usage/Demo corpus scope and deterministic evidence/status rules require canonical clarification before implementation.
```

## Git Rule

```text
Codex may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
```

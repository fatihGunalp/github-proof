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
Focus: Five production evaluators are complete; process commit-history and commit-distribution signals are next.
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
Implement `process.commit-history` and `process.commit-distribution` in the next bounded session.
```

## Working Set

```text
Session 030 canonical Partial fix, Domain evaluator, focused tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: 5060f1c
Last verified working tree: CLEAN before Session 030
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 030 — Usage/Demo Contract Fix + Evaluator: PASS

- Closed the single exhaustive-status gap by classifying complete-corpus high-confidence visual/demo-only evidence as Partial / 4.
- Added the deterministic Version 1 `documentation.usage-or-demo` evaluator.
- Covered exact usage, screenshot, video, live-demo, weak evidence, source-local, availability, ambiguity, and determinism rules.
- Preserved Version 1, all existing scores, status precedence, provider isolation, and dependency boundaries.
- Production evaluators are now 5 / 15.
```

## Last Validation

```text
Repository baseline: PASS — main at 5060f1c, clean before Session 030
Contract exhaustiveness: PASS — 7 / 7 valid states classified
Focused Usage/Demo tests: PASS — 73 passed
Existing evaluator regression: PASS — 92 passed
Documentation Infrastructure regression: PASS — 28 passed
Domain tests: PASS — 216 passed
Infrastructure tests: PASS — 88 passed
Solution tests: PASS — 304 passed
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

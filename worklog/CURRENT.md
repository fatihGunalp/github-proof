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
Focus: Four production evaluators are complete; the Usage/Demo Version 1 contract is executable and evaluator implementation is next.
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
Session 029 — Usage/Demo Signal Evaluator.
```

## Working Set

```text
Session 028 canonical docs and worklog changes only; production and test code are unchanged.
```

## Repository State

```text
Branch: main
Last verified HEAD: 7641cc6
Last verified working tree: Session 028 changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 028 — Usage/Demo Semantic Contract: PASS

- Locked README + bounded docs Markdown corpus and conditional homepage corroboration.
- Defined exact usage/demo headings, section boundaries, clear usage examples, and false positives.
- Defined deterministic screenshot, video, live-demo, brittle-link, and evidence-distinctness rules.
- Locked Strong/Present/Partial/Missing/UnavailableEvidence/RequiresHumanReview precedence.
- Confirmed existing snapshot evidence is sufficient; no production model or collector change is required.
- Preserved Version 1, scores, overall inclusion, and remote-health limitations.
```

## Last Validation

```text
Repository baseline: PASS — main at 7641cc6, clean before Session 028
Canonical contradiction review: PASS
Contract review matrix: PASS — 18/18 YES
Snapshot sufficiency: PASS — no data contract change required
Canonical consistency review: PASS
Production/test scope review: PASS — no src/** or tests/** changes
dotnet test/build/format: NOT RUN — docs-only session
git diff --check: PASS
git status --short: Session 028 canonical docs and worklog changes only
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

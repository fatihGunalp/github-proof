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
Focus: Three deterministic evaluator slices are complete; Installation Guide semantic contract is executable and snapshot alignment is next.
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
Installation Guide semantic contract is now executable; documentation evidence is not yet available in RepositorySnapshot.
```

## Next Immediate Action

```text
Session 025 — Documentation Evidence Snapshot Alignment: define bounded collection limits, then carry normalized docs/**/*.md content and completeness/availability into RepositorySnapshot.
```

## Working Set

```text
Session 024 canonical docs and worklog changes only; production and test code are unchanged.
```

## Repository State

```text
Branch: main
Last verified HEAD: 7401793
Last verified working tree: Session 024 canonical docs and worklog changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 024 — Installation Guide Semantic Contract: PASS

- Defined the bounded README plus exact docs/**/*.md documentation corpus.
- Locked README/docs fallback, source-local evaluation, exact heading forms, and section boundaries.
- Defined actionable steps and all six Strong feature detectors with distinct evidence counting.
- Bounded Strong, Present, Partial, Missing, UnavailableEvidence, and RequiresHumanReview behavior.
- Preserved Version 1, all scores, 15-signal/4-category contracts, and provider isolation.
- Added high-level future snapshot evidence semantics without choosing C# fields or arbitrary collection limits.
```

## Last Validation

```text
Contract review matrix: PASS — all 18 checks YES
Canonical contradiction review: PASS
Versioning review: PASS — Version 1 remains the first executable pre-publication contract
Docs consistency review: PASS — canonical and example Missing evidence use the complete corpus rule
Production code changed: NONE
Tests changed: NONE
dotnet test/build/format: NOT RUN — docs-only session
git diff --check: PASS
git status --short: Session 024 canonical docs and worklog changes only
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
RepositorySnapshot and Infrastructure do not yet carry the canonical docs/**/*.md content corpus or its completeness/availability semantics.
Exact deterministic file-count/content-size collection bounds must be selected before production collection implementation.
```

## Git Rule

```text
Codex may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
```

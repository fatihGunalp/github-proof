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
Focus: Three deterministic evaluator slices are complete; bounded documentation evidence is aligned and Installation Guide evaluator retry is next.
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
None.
```

## Next Immediate Action

```text
Session 026 — Installation Guide Signal Evaluator Retry.
```

## Working Set

```text
Session 025 Domain, Infrastructure, canonical docs, tests, and worklog changes only.
```

## Repository State

```text
Branch: main
Last verified HEAD: 8c12f28
Last verified working tree: Session 025 changes only
Pre-existing unrelated changes: NONE
```

## Last Completed Work

```text
Session 025 — Documentation Evidence Snapshot Alignment: PASS

- Added provider-independent RepositorySnapshot.DocumentationMarkdown evidence with source-compatible unknown/incomplete defaults.
- Collected exact docs/**/*.md paths at the resolved head SHA in deterministic Ordinal order.
- Enforced 16-file, 128 KiB per-file, and 512 KiB corpus limits before and after content decoding.
- Preserved complete empty, bounded partial, tree-truncated, and API failure semantics separately.
- Added and propagated the dedicated documentation-markdown availability key and deterministic warnings.
- Kept README, LicenseFile, evaluator, score, and signal Version 1 semantics unchanged.
```

## Last Validation

```text
focused RepositorySnapshot tests: PASS — 13 passed
focused documentation Markdown collector tests: PASS — 9 passed
existing contents collector regression: PASS — 9 passed
orchestrator focused/regression: PASS — 10 passed
README/Description/License evaluator regression: PASS — 38 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 89 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 177 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
git status --short: Session 025 changes only
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

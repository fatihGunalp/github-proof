# Deterministic RepositoryAnalysis Composer

**Date:** 2026-08-15
**Result:** PASS
**Scope:** application / domain integrity / scoring / ranking / tests / docs
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Compose a locale-independent, deterministic `RepositoryAnalysis` from one `RepositorySnapshot` and the canonical 15 semantic `SignalResult` values without changing Phase 3 signal or scoring semantics.

## Starting State

~~~text
Branch: main
Base HEAD: 512bc61
Pre-existing Working Tree: CLEAN
Relevant Previous Session: Session 043 — Language-Neutral Signal Presentation Boundary (PASS)
~~~

## Context Read

~~~text
AGENTS.md
agents/01-model-routing.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/product/report-information-architecture.md
docs/product/scoring-and-report-contract.md
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md — canonical metadata/contract lock
~~~

## Contract / Invariants

- Exactly 15 canonical signal keys in canonical order and four existing categories.
- RawApplicableScoreNormalization remains unchanged.
- Missing is scored zero and remains in the applicable denominator.
- NotApplicable, UnavailableEvidence, and RequiresHumanReview remain unscored and excluded.
- portfolio.presentation-completeness remains category-included and overall-excluded.
- Strength/gap/action selection is deterministic, capped at three, and evidence-backed.
- Composer accepts no locale and uses no presentation localizer.
- Repository literals and EvidenceItem semantics remain unchanged.
- Missing, duplicate, unknown, metadata-mismatched, or integrity-broken inputs fail fast.
- No renderer, Web UI, LLM, validation-011, new evaluator, new dependency, MediatR, or CQRS.

## Changes

~~~text
- Added RepositoryAnalysisComposer with explicit analysis id/version/timestamps and no locale input.
- Validated exact canonical 15 signal keys, metadata, and output order; unknown, duplicate, missing, or mismatched inputs fail fast.
- Composed four canonical categories and the unchanged RawApplicableScoreNormalization breakdown.
- Added evidence-backed deterministic strength, critical-gap, and improvement-action selection with a maximum of three each.
- Kept NotApplicable, UnavailableEvidence, and RequiresHumanReview out of gaps/actions and emitted stable warning/limitation keys.
- Kept portfolio.presentation-completeness in its category and out of overall score; skipped its overlapping derived action.
- Added RepositoryAnalysis graph validation for highlight/action signal and evidence references.
- Enforced contiguous action priorities and unique action presentation keys.
- Migrated ScoreBreakdown user-facing Turkish Label to locale-neutral LabelKey.
- Updated canonical report/scoring/domain docs and added an exact Session 045 localized projection handoff.
~~~

## Validation

~~~text
dotnet build GitHubProof.slnx --no-restore -v:minimal: PASS (0 warnings, 0 errors)
Composer targeted tests: PASS (13/13)
Updated domain model targeted tests: PASS (18/18)
Domain tests: PASS (422/422)
Application tests: PASS (31/31)
Infrastructure tests: PASS (101/101)
Total: PASS (554/554)
dotnet format --verify-no-changes: PASS
git diff --check: PASS
~~~

## Not Done

- Markdown renderer
- HTML/Web UI
- Locale-specific report generation
- validation-011
- Commit or push

## Next

Session 045 should project one existing RepositoryAnalysis into a shared localized RepositoryReport contract with EN default/fallback and TR support. It must localize section/score/action/warning/limitation/disclaimer copy without rerunning collection, evaluators, composer, scoring, or ranking. Markdown and Web UI remain consumers of that shared projection.

## Git

~~~text
Suggested commit: feat(analysis): compose deterministic repository analysis
Commit: PENDING — user-owned
Push: PENDING — user-owned
~~~

# Report Information Architecture

**Date:** 2026-08-15
**Result:** DOCS
**Scope:** product / report / architecture
**Model Route:** Sol / medium
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Use Turkey research only to calibrate the initial user pain, then define a global, evidence-first and language-neutral Phase 4 report information architecture; identify reusable domain fields and missing report contracts; and leave an exact bounded implementation handoff for Session 043 without changing production code or canonical scoring.

## Starting State

~~~text
Branch: main
Base HEAD: d3e9146
Pre-existing Working Tree: CLEAN
Relevant Previous Session: Session 041 — Phase 3 Final Exit Validation (PASS)
~~~

## Model Route

~~~text
Selected: Sol / medium
Why: The task defines a cross-cutting report and product boundary with multiple valid information-architecture choices while preserving locked domain and scoring invariants.
User Approval: devam et

Follow-up contract decision:
Terra / medium
Why: A bounded documentation-only extension established the global TR/EN localization boundary and revised the implementation handoff.
User Approval: devam et
~~~

## Model Escalation

~~~text
None.
~~~

## Context Read

~~~text
AGENTS.md
agents/01-model-routing.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/product/scoring-and-report-contract.md
docs/product/sample-analysis-report.md
docs/product/mvp-product-contract.md
docs/product/report-information-architecture.md
docs/architecture/analysis-domain-model.md
docs/roadmap/mvp-phases.md — Phase 4
existing RepositoryAnalysis aggregate and application signal orchestration
~~~

## Contract / Invariants Checked

- The 15 canonical signals and four canonical categories remain unchanged.
- `RawApplicableScoreNormalization` remains unchanged.
- `portfolio.presentation-completeness` remains category-included and overall-excluded.
- `Missing` remains scored zero and denominator-included.
- `NotApplicable`, `UnavailableEvidence`, and `RequiresHumanReview` remain null-scored and calculation-excluded.
- The report exposes at most three strengths, three critical gaps, and three improvement actions.
- Web and Markdown must represent the same `RepositoryAnalysis` meaning.
- No LLM produces report truth, selection, or scoring.
- No seniority, employability, authorship/originality, AI-use, or overall developer-quality inference is introduced.
- Domain/report truth is locale independent; display copy is not domain truth.
- A locale switch does not recollect, re-evaluate, rescore, or reselect report content.
- English is the global default/fallback and Turkish is an MVP launch locale.
- `validation-011` remains outside Phase 4 scope.

## Changes

~~~text
Created:
- docs/product/report-information-architecture.md
- worklog/sessions/2026-08-15-042-report-information-architecture.md

Modified:
- docs/product/scoring-and-report-contract.md
- docs/product/sample-analysis-report.md
- docs/product/mvp-product-contract.md
- docs/roadmap/mvp-phases.md
- worklog/CURRENT.md
- worklog/INDEX.md

Deleted:
- None
~~~

## Decisions

- The Phase 4 report is evidence-first: identity → proves → weakens → actions → uncertainty → categories/secondary score → signal detail → limitations/version/boundary.
- Overall and category scores remain available but are secondary diagnostic summaries.
- Strong evidence and critical gaps use deterministic, status-aware selection with canonical signal-order tie-breaks.
- `NotApplicable`, `UnavailableEvidence`, and `RequiresHumanReview` are explicit uncertainty, not critical gaps.
- “Collaboration” is not introduced as a fifth or replacement category. Issue and pull-request results may use collaboration language as a subtheme of `DevelopmentProcessVisibility`.
- Existing `RepositoryAnalysis` fields are sufficient for report meaning; the missing foundation is deterministic application-layer composition and shared report projection/presentation policy.
- Session 043 is bounded to `RepositoryAnalysis` composition and targeted tests. Markdown and Web renderers are deferred.
- GitHub Proof is global. Turkish calibration research does not create Turkey-specific report semantics.
- Existing Turkish strings in evaluator definitions/results are a confirmed localization-contract drift; Session 043 must separate semantic result data from presentation copy without changing evaluator rule behavior.
- `en` is the MVP global default/fallback; `tr` is an MVP launch locale. Locale is resolved after analysis, at the projection/rendering boundary.

## Validation

~~~text
Canonical report contract review: PASS
Domain/report field mapping review: PASS
Scoring and uncertainty drift review: PASS
Documentation cross-reference review: PASS
Localization boundary and global-product review: PASS — semantic analysis data remains locale independent; existing hard-coded Turkish copy is recorded as implementation drift
git diff --check: PASS
git status/diff scope review: PASS — docs and worklog only; no production code changes
Build/tests: NOT RUN — documentation and worklog only; production code unchanged
~~~

## Problems

~~~text
None.
~~~

## Not Done

- No production code or tests changed.
- No report composer, projection, Markdown renderer, or Web UI implemented.
- No evaluator or canonical scoring rule changed.
- No new dependency, MediatR, or CQRS introduced.
- No validation-011 work.
- No commit or push.

## Known Risks

- Report display names and action copy still need a single deterministic presentation catalog.
- Evidence-reference integrity is not yet enforced by the existing aggregate constructors.
- `AnalysisHighlight.ScoreImpact` has no necessary user-facing role in the evidence-first report and needs a deliberate implementation treatment.
- Shared renderer projection is not yet implemented, so Web/Markdown semantic parity is a future Phase 4 obligation.
- Current evaluator definitions and results contain hard-coded Turkish copy; this must be refactored before a single RepositoryAnalysis can be truthfully rendered in TR and EN.

## Working Set / Final Git State

~~~text
Modified:
- docs/product/scoring-and-report-contract.md
- docs/product/sample-analysis-report.md
- docs/product/mvp-product-contract.md
- docs/roadmap/mvp-phases.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- docs/product/report-information-architecture.md
- worklog/sessions/2026-08-15-042-report-information-architecture.md

Pre-existing unrelated:
- None
~~~

## Next

Session 043 — implement the language-neutral report contract plus deterministic `RepositoryAnalysis` composition from `RepositorySnapshot` and the canonical 15 semantic signal results, with targeted application/domain tests and no full renderer work.

## Git

~~~text
Suggested commit:
docs(report): define global evidence-first report architecture

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

## Completion Checklist

~~~text
[x] Goal tamamlandı veya Result doğru final state'e getirildi
[x] Targeted validation çalıştı
[x] Gerekliyse broader validation çalıştı
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Kullanıcıya kısa final rapor verildi
~~~

# Language-Neutral Signal Presentation Boundary

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain / application / localization / tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Separate locale-independent signal analysis truth from localized user-facing copy across all 15 canonical signals without changing Phase 3 scoring, applicability, evidence, ordering, determinism, or acceptance semantics; provide deterministic EN/TR presentation with English fallback and leave the RepositoryAnalysis composer for Session 044.

## Starting State

~~~text
Branch: main
Base HEAD: 0709411
Pre-existing Working Tree: CLEAN
Relevant Previous Session: Session 042 — Report Information Architecture (DOCS)
~~~

## Model Route

~~~text
Selected: Sol / high
Why: This is a cross-cutting domain/application contract migration across all validated evaluators with high regression cost and locale-independent determinism requirements.
User Approval: devam et
~~~

## Model Escalation

~~~text
None.
~~~

## Context Read

~~~text
AGENTS.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/architecture/analysis-domain-model.md
docs/product/report-information-architecture.md
~~~

## Contract / Invariants Checked

- Canonical 15 signal keys, status values, maximum scores, scoring rules, applicability, evidence semantics, and canonical order must not change.
- `RawApplicableScoreNormalization` must not change.
- `portfolio.presentation-completeness` remains category-included and overall-excluded.
- Locale switching must not rerun analysis or change score, status, evidence, selection, or ranking.
- English is the default/fallback locale and Turkish is an MVP launch locale.
- Repository literals are never translated.
- No LLM translation, new dependency, MediatR, or CQRS.
- RepositoryAnalysis composition, report aggregation, Markdown rendering, Web UI, and validation-011 are outside this session.

## Changes

~~~text
Domain:
- SignalDefinition now carries only semantic identity/configuration plus deterministic versioned applicability/evaluation/evidence rule keys.
- SignalResult now carries only key/version/category/status/score/max/overall-inclusion/evidence/evaluated-at truth.
- CategoryResult, AnalysisHighlight, ImprovementAction, AnalysisWarning, and RepositoryAnalysis no longer embed user-facing prose; stable presentation/limitation keys are used where identity is required.

Evaluators:
- All 15 canonical evaluators migrated to the semantic SignalDefinition/SignalResult contracts.
- Signal keys, versions, categories, maximum scores, score/status/applicability logic, evidence creation, evaluated-at behavior, canonical order, and overall inclusion flags were preserved.

Application presentation:
- Added SignalPresentation and SignalPresentationLocalizer.
- Added source-controlled neutral English and Turkish .resx catalogs.
- Unknown locale falls back to English; tr and tr-* select Turkish.
- EvidenceItem instances and repository literals are passed through unchanged.

Docs/worklog:
- Updated domain, scoring/report, and report IA contracts.
- Replaced the outdated Session 043 composer handoff with the exact Session 044 handoff.
~~~

## Decisions

~~~text
- Name, Description, ExplanationTemplate, RecommendationTemplate, Explanation, Recommendation, and prose Limitations are presentation copy, not analysis truth.
- Applicability/evaluation/evidence rule semantics remain evaluator code plus versioned semantic rule keys; they are not translated prose.
- EvidenceItem semantics remain unchanged. Presentation treats evidence as opaque analysis output and never translates repository literals.
- English is the neutral default/fallback; Turkish is the first additional launch locale.
- UnavailableEvidence and NotApplicable do not receive missing/remediation recommendations.
~~~

## Validation

~~~text
dotnet test GitHubProof.slnx --no-restore: PASS
- Domain: 417/417
- Application: 18/18
- Infrastructure: 101/101
- Total: 536/536

git diff --check: PASS
15 canonical SignalDefinition instances: PASS
portfolio.presentation-completeness IncludeInOverallScore=false: PASS
Frozen validation artifacts: untouched
validation-011: NOT RUN — intentionally out of scope
~~~

## Problems

~~~text
The initial mechanical evaluator migration temporarily introduced mojibake in source literals. The encoding damage was detected before completion, reversed, and regression tests returned to green. No corrupted literal remains in the final diff.
~~~

## Not Done

- RepositoryAnalysis composer
- Report aggregation
- Markdown renderer
- Web UI
- validation-011
- Commit or push

## Known Risks

~~~text
- SignalPresentation is intentionally signal-level only; report section/action/warning localization expands with the Session 044 report projection.
- EvidenceItem.Notes remains part of the unchanged evidence contract and is passed through verbatim; repository-originated literal values are never translated.
- This is an intentional breaking source-contract migration for in-repository consumers; all current consumers and tests were migrated together.
~~~

## Working Set / Final Git State

~~~text
Base HEAD: 0709411
Branch: main
Pre-existing working tree: CLEAN
Final working tree: Session 043 source, tests, docs, and worklog changes only
Commit/push: NOT RUN
~~~

## Next

Session 044 — build the locale-independent RepositoryAnalysis composer and shared report projection contract using the exact handoff in docs/product/report-information-architecture.md.

## Git

~~~text
Suggested commit:
refactor(analysis): separate semantic results from localized presentation

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

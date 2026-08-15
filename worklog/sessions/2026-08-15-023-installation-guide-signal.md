# Installation Guide Signal

**Date:** 2026-08-15
**Result:** BLOCKED
**Scope:** domain contract audit, with implementation only if the contract is sufficient
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Audit the canonical and implemented evidence contracts, then implement only the deterministic `documentation.installation-guide` evaluator if README/docs content availability and actionable-step semantics are sufficient for reliable classification.

## Starting State

~~~text
Branch: main
Base HEAD: 78fb3fd
Pre-existing Working Tree: CLEAN
Relevant Previous Sessions: 019 (PASS), 021 (PASS), 022 (PASS)
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
worklog/INDEX.md
worklog/sessions/2026-08-15-022-description-license-signal-evaluators.md
docs/architecture/mvp-signal-definitions.md — Installation Guide section only
docs/architecture/analysis-domain-model.md — RepositorySnapshot, RepositoryFile, and DataAvailability
docs/product/scoring-and-report-contract.md — status, recommendation, and missing/unavailable semantics
docs/product/analysis-categories-and-signal-boundaries.md — Installation Guide boundary
docs/roadmap/mvp-phases.md — example Installation Guide result
ISignalEvaluator and ReadmeExistsSignalEvaluator
RepositorySnapshot, RepositoryReadme, RepositoryFile, and DataAvailability
SignalDefinition, SignalResult, SignalStatus, EvidenceItem, and EvidenceCollection
GitHubRepositoryDescriptiveDataCollector
GitHubRepositoryContentsCollector and GitHubRepositoryContentsCollection
GitHubRepositorySnapshotOrchestrator
relevant contents/orchestration tests
Session 023 task contract
~~~

## Contract Audit

### Required data

- Canonical RequiredData includes README content, indexable Markdown files under `docs` when needed, and content availability.
- Canonical Missing requires all required content to have been successfully inspected.
- Canonical evidence boundary says installation steps may exist in README or indexable documentation.

### Current snapshot

- `RepositoryReadme` carries a normalized path and complete decoded content; `readme` availability is recorded separately.
- Generic `FileTree` entries are normalized with `Content = null`.
- The contents collector fetches content only for selected project/dependency files and workflow files; the orchestrator also overlays the normalized license file.
- Indexable `docs/**/*.md` content is not selected, fetched, or represented as a dedicated snapshot collection.
- There is no docs-content availability key that distinguishes fully inspected, partially collected, unavailable, rate-limited, or not-requested documentation content.

### Data contract gap

- When README evidence alone yields a positive deterministic result, docs fallback need not change that positive conclusion.
- When README contains no installation evidence, README inspection alone cannot prove canonical absence because installation evidence may exist in `docs` Markdown.
- File paths without content cannot establish absence, actionability, or Strong features.
- Returning Missing in that state would convert uncollected evidence into missing evidence; returning UnavailableEvidence for every such repository would still leave the canonical required-data collection contract unimplemented.

### Semantic contract gap

- Canonical text requires at least one "actionable step" for Present but does not lock whether this means a command, fenced code block, numbered/list step, imperative instruction, or another syntax.
- The six Strong feature names are listed, but deterministic recognition boundaries for prerequisites, dependency installation, configuration, build, run, and migration/preparation are not defined.
- Multiple reasonable parsers would therefore produce different Present/Partial/Strong outcomes for the same Markdown.
- Arbitrary keyword counting would affect score/status and violate the deterministic versioned contract.

## Minimum Correction Needed

1. Canonically define which `docs` Markdown paths are indexable and when README evidence may short-circuit docs inspection.
2. Extend the normalized snapshot/collection boundary to carry those Markdown contents plus explicit complete/partial/unavailable availability, or choose and document an equivalent provider-independent representation.
3. Canonically define the syntactic criteria for an actionable step and the deterministic recognition rules/distinctness boundary for each Strong feature.
4. Define when unresolved text becomes `RequiresHumanReview` instead of Partial, Missing, or Present.

These are product/domain/integration decisions and were not made silently in this evaluator session.

## Changes

- Updated worklog only to record the mandatory audit and blocker.
- No production code, tests, canonical docs, or dependencies were changed.

## Validation

~~~text
Repository baseline: PASS — main at 78fb3fd, clean before Session 023
Canonical/data contract comparison: BLOCKED — gaps confirmed
Production tests/build/format: NOT RUN — implementation gate failed before code changes
~~~

## Not Done

- `documentation.installation-guide` evaluator implementation.
- Markdown parser/helper implementation.
- Evaluator tests or regression runs.
- Infrastructure workaround, filesystem/network reads, or new dependency.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 78fb3fd
Pre-existing unrelated changes: NONE
Final working tree: Session 023 worklog changes only
~~~

## Next

After canonical decisions are made, run a new mandatory model gate for the smallest snapshot/contract alignment task before retrying this evaluator.

## Git

~~~text
Suggested commit: docs(worklog): record installation guide contract gaps
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

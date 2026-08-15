# Installation Guide Semantic Contract

**Date:** 2026-08-15
**Result:** PASS
**Scope:** canonical product/domain/architecture documentation only
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Close the Session 023 data and semantic contract gaps by defining an executable, deterministic MVP v1 contract for `documentation.installation-guide`, without changing production or test code.

## Starting State

~~~text
Branch: main
Base HEAD: 7401793
Pre-existing Working Tree: CLEAN
Relevant Previous Session: 023 (BLOCKED)
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
worklog/sessions/2026-08-15-023-installation-guide-signal.md
docs/architecture/mvp-signal-definitions.md — common contract, Version 1 rule, Installation Guide, and Contract Lock v1
docs/architecture/analysis-domain-model.md — RepositorySnapshot, DataAvailability, EvidenceCollection, and signal versioning
docs/product/scoring-and-report-contract.md — status, evidence, recommendation, version, and Missing/Unavailable semantics
docs/product/sample-analysis-report.md — Installation Guide consistency example
docs/product/analysis-categories-and-signal-boundaries.md — Installation Guide evidence boundary
docs/roadmap/mvp-phases.md — Phase 2 bounds and example Installation Guide result
RepositorySnapshot and RepositorySnapshotData
GitHubRepositoryContentsCollector
Session 024 task contract
~~~

## Contradiction and Versioning Audit

- Proposed rules preserve the 15-signal, 4-category, integer score, nullable score, and RawApplicableScoreNormalization contracts.
- Missing still requires successfully inspected evidence; incomplete or unavailable corpus remains UnavailableEvidence.
- Reliable positive evidence may be used from a partial snapshot without converting unavailable data into missing evidence.
- README or indexable documentation evidence remains within the existing category boundary.
- Domain remains provider-independent and does not depend on HTTP, GitHub DTOs, or a Markdown renderer package.
- Existing versioning rules explicitly define the pre-publication contract lock as the first executable `Version: 1`; no version bump is required.
- No canonical contradiction was found.

## Decisions Locked

- Corpus: normalized README plus regular blobs under exact, recursive `docs/**/*.md`; extension matching is case-insensitive and path order is Ordinal.
- Fallback: README is primary, but docs are required whenever README cannot establish the exact result; a reliable Strong or Present may short-circuit incomplete evidence as defined.
- Evaluation is source-local; Strong features are never combined across files.
- ATX and Setext headings use exact Turkish/English candidate matching without fuzzy semantics.
- Section bodies end at the next same-or-higher heading.
- Actionable evidence is limited to non-empty fenced blocks, non-empty ordered steps, or the locked English/Turkish imperative set.
- All six Strong feature detectors and distinct evidence counting are explicit; at least three distinct features produce Strong.
- Partial and Missing require complete corpus evidence; unavailable/partial data cannot produce a lower final result unless a reliable Strong/Present already exists.
- RequiresHumanReview is limited to observable, bounded structural ambiguity and is not a generic uncertainty bucket.
- Full source bodies are not copied into signal evidence.
- Version remains 1 and all scores remain unchanged.
- Future snapshot semantics require docs collection availability, completeness, paths, contents, and content availability.
- No existing exact collection bound was found; a deterministic file-count/content-size policy is mandatory before Session 025 production collection.

## Contract Review Matrix

~~~text
Documentation corpus defined: YES
README/docs fallback defined: YES
Source-local evaluation defined: YES
Exact heading rules defined: YES
Section boundary defined: YES
Actionable step defined: YES
Partial rule executable: YES
Present rule executable: YES
Six Strong feature detectors defined: YES
Distinct feature counting defined: YES
Missing completeness rule defined: YES
UnavailableEvidence rule defined: YES
RequiresHumanReview bounded: YES
Evidence requirements defined: YES
Recommendations defined: YES
Future snapshot data requirement defined: YES
Scoring unchanged: YES
Signal Version decision explicit: YES
~~~

## Changes

- Expanded only the canonical Installation Guide section into the executable MVP v1 contract.
- Added high-level provider-independent installation documentation corpus semantics to the domain model document without choosing production types.
- Aligned README-only Missing examples in the scoring contract, sample report, and roadmap with the complete corpus rule.
- Updated worklog for Session 024.
- Production code, tests, and dependencies were not changed.

## Validation

~~~text
Contract review matrix: PASS — 18/18 YES
Canonical contradiction review: PASS
Versioning review: PASS — Version 1 retained
Docs consistency review: PASS
Production/test scope review: PASS — no src/** or tests/** changes
dotnet test/build/format: NOT RUN — docs-only session
git diff --check: PASS
~~~

## Not Done

- Documentation evidence snapshot types or fields.
- Docs Markdown collection and collection-bound implementation.
- InstallationGuideSignalEvaluator or evaluator tests.
- Any dependency, generic Markdown framework, Signal Engine runner, or unrelated evaluator.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 7401793
Pre-existing unrelated changes: NONE
Final working tree: Session 024 canonical docs and worklog changes only
~~~

## Next

Session 025 — Documentation Evidence Snapshot Alignment. First lock deterministic file-count/content-size bounds, then represent and collect the canonical docs Markdown corpus provider-independently.

## Git

~~~text
Suggested commit: docs(domain): define installation guide evaluation contract
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

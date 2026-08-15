# Test Structure + Continuous Integration

**Date:** 2026-08-15
**Result:** PASS
**Scope:** Version 1 test-structure and CI executable semantics, Domain evaluators, and focused regression tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement deterministic `validation.test-structure` and `validation.continuous-integration` evaluators using normalized file-tree, project-content, and workflow evidence.

## Starting State

~~~text
Branch: main
Base HEAD: 63f3467
Pre-existing Working Tree: CLEAN
Production evaluators: 10 / 15
~~~

## Context Read

~~~text
AGENTS.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/architecture/mvp-signal-definitions.md — Test Structure, Continuous Integration, and Contract Lock v1
docs/product/analysis-categories-and-signal-boundaries.md — validation evidence boundaries
docs/architecture/analysis-domain-model.md — RepositorySnapshot evidence and Domain isolation
docs/product/scoring-and-report-contract.md — nullable status and human-review semantics
RepositoryFile, RepositoryCollection, DataAvailability, SignalResult, and ISignalEvaluator contracts
Relevant repository-contents collector and snapshot orchestration contracts
Session 033 task contract
~~~

## Plan

1. Audit file-tree/project-content/workflow evidence, completeness, and availability.
2. Add minimum executable Version 1 rules for test evidence and bounded workflow command interpretation.
3. Implement both Domain evaluators and focused tests.
4. Run focused, evaluator regression, Domain, Infrastructure, solution, build, format, and diff validation.
5. Review scope and close the session worklog.

## Audit Decision

- File tree paths, directory markers, and selected project/dependency contents are present in snapshot `FileTree`; selected content overlays preserve the original path.
- `file-tree` and `project-files` availability plus `FileTree.IsComplete` jointly preserve required test-evidence completeness.
- Workflow paths, contents, availability, and collection completeness are present independently in snapshot `Workflows`.
- No Infrastructure or snapshot data gap exists.
- Existing test and CI language is product-aligned but not executable enough for deterministic Version 1 classification.
- Exact path segments, bounded dependency declarations, reliable `.spec.`/`.test.` patterns, evidence-combination rules, and conservative `run:` command families provide a minimum closed contract without generic filename/YAML/NLP heuristics.

## Constraints

- Preserve Version 1 scores, statuses, category, and overall inclusion.
- Missing requires complete available evidence.
- Do not infer test success, CI run state, repository quality, or generic keyword meaning.
- No external YAML/Markdown parser, package, generic engine, registry, Infrastructure, snapshot, orchestration, report, Web/API, or other evaluator change.
- MediatR and CQRS remain absent.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: COMPLETE — canonical alignment, two evaluators, focused tests, full regression validation, and worklog closeout passed.
Next: Implement topics, homepage/live-demo, and presentation-completeness signals in the next bounded session.
~~~

## Canonical Clarification

- Test directory evidence uses only exact case-insensitive `tests`, `test`, and `__tests__` path segments; generic filename keyword matches are forbidden.
- Explicit `.Tests.csproj` / `.Test.csproj`, locked framework configs, Maven test-scope dependencies, Gradle test configurations, and exact `.spec.` / `.test.` markers form the closed Version 1 evidence set.
- Strong requires an executable combination representing multiple evidence types or organized evidence; a project/framework alone is Present, a reliable file alone is Partial, and an exact directory name alone is RequiresHumanReview.
- Test Structure produces no terminal score from incomplete file/project evidence.
- CI interprets only inline or block YAML `run:` scalars and a locked set of build/test/deploy command families; names, comments, environment text, and `uses:` are not command evidence.
- Build plus test is corpus-level Strong, deploy-only is Partial 3, and otherwise unknown workflow purpose is Partial 2.
- Bounded run-scalar ambiguity produces RequiresHumanReview only on a complete available corpus; incomplete corpus remains UnavailableEvidence unless observed build plus test already fixes terminal Strong.

## Implementation

- Added `TestStructureSignalEvaluator` using exact path segments, explicit project/config markers, BCL XML inspection for Maven test scope, and bounded Gradle dependency declarations.
- Test evidence is grouped by type with deterministic Ordinal representative paths; no file contents are copied into `SignalResult`.
- Added `ContinuousIntegrationSignalEvaluator` with a small run-scalar extractor and exact command-family classifier rather than a generic YAML/NLP framework.
- CI evidence records workflow counts, recognized purpose counts, ambiguity/content state, completeness, availability, and deterministic representative paths.
- Both evaluators reuse `ISignalEvaluator`, consume Domain snapshot data only, read `TimeProvider` once, and use culture-independent comparisons/output.
- Recommendations and limitations explicitly avoid claiming test success, CI run success, activity, safety, or repository quality.

## Test Coverage

- Test Structure: Missing, unavailable file tree/project content, incomplete tree, both test-project suffixes, pytest/Jest/Vitest configs, Maven/Gradle dependency evidence, all exact test directory segments, spec/test files, organized/multiple Strong paths, bounded directory ambiguity, and false-positive filenames.
- CI: empty corpus, build+test in one/separate workflows, build-only, test-only block scalar, deploy-only, unknown purpose, unavailable/incomplete content, terminal incomplete Strong, malformed quoted run scalar, null content, and comment/name/env/uses false positives.
- Both definitions, evidence fields, ordering independence, deterministic state, limitations, recommendations, and one clock read per evaluation.

## Validation

~~~text
focused Test Structure + CI evaluator tests: PASS — 46 passed
all evaluator regression: PASS — 307 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 358 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 446 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Architecture and Scope Review

~~~text
ISignalEvaluator reused: YES
Provider/HTTP/GitHub DTO dependency in Domain: NO
Generic signal engine or evaluator registry: NO
Generic YAML/NLP framework: NO
External YAML/Markdown parser: NONE
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
Infrastructure/snapshot/orchestration/scoring/report change: NO
Production evaluators after PASS: 12 / 15
~~~

## Problems

None.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 63f3467
Pre-existing unrelated changes: NONE

Modified:
- docs/architecture/mvp-signal-definitions.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Domain/Analysis/Signals/TestStructureSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/ContinuousIntegrationSignalEvaluator.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/TestStructureSignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/ContinuousIntegrationSignalEvaluatorTests.cs
- worklog/sessions/2026-08-15-033-test-structure-and-ci.md
~~~

## Next

Implement portfolio topics, homepage/live-demo, and presentation-completeness signals in the next bounded session.

## Git

~~~text
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

Suggested commit: `feat(domain): add validation signal evaluators`

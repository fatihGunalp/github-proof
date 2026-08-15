# Documentation Evidence Snapshot Alignment

**Date:** 2026-08-15
**Result:** PASS
**Scope:** canonical collection bounds, Domain snapshot, Infrastructure collection, orchestration, and focused regression tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Collect the canonical bounded `docs/**/*.md` corpus at the resolved default-branch head and carry it into `RepositorySnapshot` with provider-independent complete, partial, and unavailable semantics. Do not implement the Installation Guide evaluator.

## Starting State

~~~text
Branch: main
Base HEAD: 8c12f28
Pre-existing Working Tree: CLEAN
Relevant Previous Sessions: 023 (BLOCKED), 024 (PASS)
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
worklog/sessions/2026-08-15-024-installation-guide-semantic-contract.md
docs/architecture/mvp-signal-definitions.md — Installation Guide corpus and bound contract
docs/architecture/analysis-domain-model.md — RepositorySnapshot and documentation corpus evidence
docs/roadmap/mvp-phases.md — Phase 2 collection and Phase 3 signal engine
RepositorySnapshot, RepositorySnapshotData, RepositoryFile, RepositoryCollection, and DataAvailability
GitHubRepositoryContentsCollector and GitHubRepositoryContentsCollection
GitHubRepositorySnapshotOrchestrator
relevant Domain and Infrastructure tests
Session 025 task contract
~~~

## Existing Collection Audit

- File tree resolution is pinned through default-branch ref, commit SHA, and tree SHA.
- Selected project/workflow content requests already use the same resolved head SHA through `?ref=<head-sha>`.
- Selected-file failures preserve earlier successful files, mark collections incomplete, and retain the first actual failure availability.
- Tree truncation preserves usable entries while making dependent selections incomplete.
- RepositoryFile already carries nullable tree-reported Size.
- No existing file-count, per-file decoded-size, or total decoded-size bound existed for docs content.
- Existing RepositoryCollection + IsComplete + collection DataAvailability + warnings are sufficient; no per-item availability model is needed.

## Changes

- Added optional trailing `RepositorySnapshot.DocumentationMarkdown` as `RepositoryCollection<RepositoryFile>`.
- A missing constructor argument defaults to empty/incomplete with unknown TotalCount, distinct from explicit complete empty evidence.
- Added `DocumentationMarkdownFiles` and `DocumentationMarkdownAvailability` to the Infrastructure contents result.
- Added dedicated `documentation-markdown` availability through collector and orchestrator.
- Selected only regular blobs under exact recursive `docs/` with case-insensitive `.md` extension.
- Sorted candidates with StringComparer.Ordinal before content requests.
- Limited selection to the first 16 candidates, 131072 decoded UTF-8 bytes per file, and 524288 decoded UTF-8 bytes total.
- Rejected unknown/oversize declared sizes before fetch and revalidated decoded bytes after fetch.
- Continued after total-budget skips so smaller later candidates remain deterministically eligible.
- Preserved accepted earlier files and real RateLimited, Forbidden, or Failed status after content failure.
- Kept bound/truncation partial collections Available while IsComplete is false.
- Recorded numeric bounds and complete-empty/partial/unavailable semantics in canonical architecture docs.
- Added focused Domain, collector, and orchestrator coverage without implementing an evaluator.

## Decisions

- Reuse RepositoryFile and RepositoryCollection; do not introduce a generic documentation abstraction.
- README remains independent in RepositorySnapshot.Readme; docs/README.md belongs to DocumentationMarkdown.
- Incomplete collection and unavailable API evidence remain separate concepts.
- Incomplete collection TotalCount remains null, matching existing selected-file semantics.
- Documentation content uses the already resolved default-branch head SHA; no extra tree/ref request is introduced.
- Signal semantics, scores, and documentation.installation-guide Version 1 are unchanged.
- No third-party dependency, MediatR, CQRS, generic Markdown engine, or evaluator registry is introduced.

## Validation

~~~text
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
~~~

The first infrastructure-focused run overlapped another dotnet build and hit a transient shared output file lock. The same focused command was rerun sequentially and passed 9/9; no implementation change was required for that environment-only failure.

## Not Done

- InstallationGuideSignalEvaluator or actionable/Strong Markdown parsing.
- Other signal evaluators, generic Signal Engine runner, scoring aggregation, or reports.
- External Markdown parser or any third-party dependency.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 8c12f28
Pre-existing unrelated changes: NONE
Final working tree: Session 025 changes only
~~~

## Next

Session 026 — Installation Guide Signal Evaluator Retry.

## Git

~~~text
Suggested commit: feat(infrastructure): collect bounded documentation markdown evidence
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

# Repository Contents and Workflow Files

**Date:** 2026-08-15
**Result:** PASS
**Scope:** infrastructure
**Model Route:** Terra / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Collect the default-branch file tree, relevant project/dependency file contents, and GitHub Actions workflow contents while preserving branch consistency, partial data, completeness, and availability.

## Starting State

~~~text
Branch: main
Base HEAD: ef33bf0
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-014-releases-tags-contributors-collection.md
~~~

## Context Read

~~~text
AGENTS.md
agents/01-model-routing.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
worklog/CURRENT.md
worklog/INDEX.md
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md
docs/roadmap/mvp-phases.md
src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryMetadataCollection.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryCommitCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryDescriptiveDataCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubApiAvailability.cs
src/GitHubProof.Infrastructure/GitHub/GitHubPagination.cs
~~~

## Contract / Invariants Checked

- Test structure requires file tree, project/dependency files, test-path evidence, and availability; Infrastructure does not evaluate it.
- Continuous integration requires `.github/workflows` discovery and YAML contents; workflow run state is out of scope.
- The default branch is resolved once to a head commit SHA; tree and selected content requests use that immutable SHA.
- A truncated recursive tree preserves returned entries but is not represented as complete.
- Workflow absence in a complete tree differs from unavailable workflow content.
- Empty textual file content differs from unavailable or unsupported content.

## Changes

- Added a contents collection result and collector using Git reference, commit, recursive tree, and selected content endpoints.
- Reused existing availability mapping and `RepositoryCollection` completeness behavior.
- Added nine focused tests for tree normalization, truncation, path preservation, content selection, workflow partial failure, empty repository, and errors.

## Decisions

- No new dependency: existing `HttpClient` and JSON support are sufficient.
- Relevant textual content is limited to workflow YAML and known project/dependency files required by the test-structure contract.
- File paths retain repository `/` semantics and use ordinal path identity; Windows path rules are not applied.
- `prerelease` or workflow semantics are not inferred; raw textual content only is collected.

## Validation

~~~text
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore
PASS — 68 passed, 0 failed, 0 skipped

dotnet test GitHubProof.slnx --no-restore
PASS — 111 passed, 0 failed, 0 skipped

dotnet build GitHubProof.slnx --no-restore
PASS — 0 warnings, 0 errors

dotnet format GitHubProof.slnx --verify-no-changes --no-restore
PASS

git diff --check
PASS
~~~

## Problems

None.

## Not Done

- Signal evaluation, scoring, snapshot orchestration, and other Phase 2 collectors.

## Working Set / Final Git State

~~~text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContentsCollection.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContentsCollector.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositoryContentsCollectorTests.cs
- worklog/sessions/2026-08-15-015-repository-contents-workflows.md

Pre-existing unrelated:
- NONE
~~~

## Next

Run a new model gate before the next bounded Phase 2 data-collection slice.

## Git

~~~text
Suggested commit: feat(infrastructure): add repository contents and workflow collection
Commit: PENDING — user-owned unless explicitly requested
Push: PENDING — user-owned unless explicitly requested
~~~

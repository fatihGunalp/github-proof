# Releases, Tags, and Contributors Collection

**Date:** 2026-08-15
**Result:** PASS
**Scope:** infrastructure
**Model Route:** Terra / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Collect normalized public GitHub releases, tags, and contributors through all available pages while preserving independent availability and completeness state.

## Starting State

~~~text
Branch: main
Base HEAD: 6b3d5a1
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-013-issues-pull-requests-collection.md
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
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryIssueCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryPullRequestCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubPagination.cs
src/GitHubProof.Infrastructure/GitHub/GitHubApiAvailability.cs
~~~

## Contract / Invariants Checked

- Release Usage requires releases, tags, published/draft state, and availability; it does not score Infrastructure output.
- `RepositoryRelease` already carries the canonical normalized release fields, and `RepositoryContributor` already carries login plus contribution count.
- Tags remain an independent collection so release/tag mixed availability cannot be collapsed.
- Empty successful collections are available data, not missing evidence.
- Contributor endpoint `204` means an empty repository and therefore successful empty contributor data.
- Anonymous contributors are intentionally not requested: the official endpoint only includes them when `anon` is explicitly enabled, while the canonical representation is login-based.

## Changes

- Added independent release, tag, and contributor collection result types and collectors.
- Reused shared GitHub availability mapping and Link-header pagination without adding a generic collection framework.
- Added a small test-only support helper and sixteen focused tests for normalization, pagination, partial data, empty collections, and failures.

## Decisions

- No new dependency: the existing `HttpClient` / `System.Net.Http.Json` boundary is sufficient.
- Release duplicate identity is tag name; tag duplicate identity is tag name; contributor duplicate identity is login.
- Release notes are preserved as normalized text, but whether they are explanatory remains evaluator work.
- `prerelease` is not collected because the canonical signal only requires published and draft state.

## Validation

~~~text
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore
PASS — 59 passed, 0 failed, 0 skipped

dotnet test GitHubProof.slnx --no-restore
PASS — 102 passed, 0 failed, 0 skipped

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

- Signal evaluation, scoring, snapshot orchestration, file tree, workflows, and other Phase 2 collectors.

## Working Set / Final Git State

~~~text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContributorCollection.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContributorCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryReleaseCollection.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryReleaseCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTag.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTagCollection.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTagCollector.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubCollectionTestSupport.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositoryContributorCollectorTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositoryReleaseCollectorTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositoryTagCollectorTests.cs
- worklog/sessions/2026-08-15-014-releases-tags-contributors-collection.md

Pre-existing unrelated:
- NONE
~~~

## Next

Run a new model gate before the next bounded Phase 2 data-collection slice.

## Git

~~~text
Suggested commit: feat(infrastructure): add release tag and contributor collection
Commit: PENDING — user-owned unless explicitly requested
Push: PENDING — user-owned unless explicitly requested
~~~

# Issues and Pull Requests Collection

**Date:** 2026-08-15
**Result:** PASS
**Scope:** infrastructure
**Model Route:** Terra / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Collect normalized public GitHub issue and pull-request data through all available pages, while preserving partial data, completeness state, and distinct availability semantics.

## Starting State

~~~text
Branch: main
Base HEAD: badc250
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-012-commit-collection.md
~~~

## Context Read

~~~text
AGENTS.md
agents/01-model-routing.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
worklog/CURRENT.md
worklog/INDEX.md
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md
docs/roadmap/mvp-phases.md
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryCommitCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryCommitCollection.cs
src/GitHubProof.Infrastructure/GitHub/GitHubApiAvailability.cs
~~~

## Contract / Invariants Checked

- The issue signal requires issue-list data, explicit pull-request separation, open/closed counts, and availability.
- The pull-request signal requires open/closed pull requests, merge status, repository ownership/fork information, and availability.
- GitHub issue responses may contain pull requests; these must never be represented as issues.
- Collection failures remain availability/completeness facts, not missing evidence or evaluator output.
- Domain repository issue and pull-request types require fields this slice intentionally does not collect. Infrastructure therefore uses small normalized representations instead of synthetic domain values.

## Changes

- Added normalized issue and pull-request models, collection results, and collectors.
- Added a small internal Link-header pagination helper and moved the existing commit collector to use it.
- Added sixteen focused tests for paging, PR exclusion, duplicate handling, merge state, partial collection, and failure semantics.

## Decisions

- No new dependency: the existing `HttpClient` / `System.Net.Http.Json` boundary is sufficient.
- Request `state=all` and `per_page=100` so the normalized data contains both open and closed records.
- Preserve first-observed records by GitHub number when pages overlap.

## Validation

~~~text
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore
PASS — 43 passed, 0 failed, 0 skipped

dotnet test GitHubProof.slnx --no-restore
PASS — 86 passed, 0 failed, 0 skipped

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

- Signal evaluation, scoring, snapshot orchestration, releases, contributors, file tree, workflows, and all other Phase 2 collectors.

## Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryCommitCollector.cs
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Infrastructure/GitHub/GitHubPagination.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryIssue.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryIssueCollection.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryIssueCollector.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryPullRequest.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryPullRequestCollection.cs
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryPullRequestCollector.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositoryIssueCollectorTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositoryPullRequestCollectorTests.cs
- worklog/sessions/2026-08-15-013-issues-pull-requests-collection.md

Pre-existing unrelated:
- NONE
~~~

## Next

Run a new model gate before the next bounded Phase 2 data-collection slice.

## Git

~~~text
Suggested commit: feat(infrastructure): add issue and pull request collection
Commit: PENDING — user-owned unless explicitly requested
Push: PENDING — user-owned unless explicitly requested
~~~

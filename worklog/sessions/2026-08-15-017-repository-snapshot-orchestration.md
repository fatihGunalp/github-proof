# RepositorySnapshot Orchestration

**Date:** 2026-08-15
**Result:** PASS
**Scope:** infrastructure
**Model Route:** Sol / medium
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Compose the existing GitHub collectors into a metadata-first, sequential, testable pipeline that maps normalized observable data into the canonical Domain `RepositorySnapshot`.

## Starting State

~~~text
Branch: main
Base HEAD: 07a2e62
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-016-tags-snapshot-contract-alignment.md
~~~

## Context Read

~~~text
AGENTS.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
worklog/CURRENT.md
worklog/INDEX.md
Session 017 task contract
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md
docs/roadmap/mvp-phases.md
src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
src/GitHubProof.Domain/Analysis/ValueObjects/RepositoryIdentity.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepository*Collector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepository*Collection.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryUrl.cs
tests/GitHubProof.Infrastructure.Tests/GitHub collector tests
~~~

## Contract / Invariants Checked

- Metadata is collected first; metadata failure must not create a fake snapshot.
- Existing collector availability and completeness semantics must be preserved.
- Releases and Tags remain independent sections and availability states.
- Domain remains isolated from GitHub, HTTP, and JSON types.
- Orchestration is sequential and explicit; no Signal Engine behavior is introduced.

## Changes

- Added an explicit Infrastructure orchestration service and fatal metadata result contract.
- Added small collector interfaces for constructor injection and dependency-free orchestration tests.
- Added metadata-first sequential collection and explicit mapping to every canonical RepositorySnapshot section.
- Merged selected project/dependency contents and license file observations into FileTree without losing tree completeness.
- Added real title/body normalization required to map Issue and Pull Request collector results into Domain types.
- Aggregated collector availability and warnings while preserving independent Releases and Tags states.
- Added seven focused orchestration test cases covering full, partial, archived, fatal metadata, and availability behavior.

## Decisions

- Dependency baseline contains no MediatR/CQRS package and will remain unchanged.
- Infrastructure is the smallest correct layer because orchestration depends directly on GitHub-specific collectors and URL parsing.
- Metadata failure returns no fake snapshot and retains the metadata `DataAvailability` state without a new product enum.
- `CapturedAt` is read once from `TimeProvider`; `HeadCommitSha` comes only from the contents collector's resolved ref.
- Live GitHub sections are not claimed to be fully atomic with the contents SHA; one technical warning records this limitation when a head SHA exists.
- No collector state is translated into SignalStatus or missing evidence.

## Validation

~~~text
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore --filter FullyQualifiedName~GitHubRepositorySnapshotOrchestratorTests
PASS — 7 passed, 0 failed, 0 skipped

dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore
PASS — 75 passed, 0 failed, 0 skipped

dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore
PASS — 46 passed, 0 failed, 0 skipped

Application tests
N/A — no Application test project exists

dotnet test GitHubProof.slnx --no-restore
PASS — 121 passed, 0 failed, 0 skipped

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

- Real GitHub integration validation, reserved for Session 018.
- Signal evaluation, scoring, reporting, API, UI, and persistence.

## Working Set / Final Git State

~~~text
Modified:
- existing GitHub collector classes (interface implementation only)
- GitHubRepositoryIssue.cs
- GitHubRepositoryIssueCollector.cs
- GitHubRepositoryPullRequest.cs
- GitHubRepositoryPullRequestCollector.cs
- Issue and pull-request collector tests
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- GitHubRepositorySnapshotOrchestrator.cs
- GitHubRepositorySnapshotResult.cs
- IGitHubRepositoryCollectors.cs
- GitHubRepositorySnapshotOrchestratorTests.cs
- worklog/sessions/2026-08-15-017-repository-snapshot-orchestration.md

Pre-existing unrelated:
- NONE
~~~

## Next

Run a new model gate for Session 018 — Phase 2 integration and validation.

## Git

~~~text
Suggested commit: feat(infrastructure): orchestrate repository snapshot collection
Commit: PENDING — user-owned unless explicitly requested
Push: PENDING — user-owned unless explicitly requested
~~~

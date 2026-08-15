# Tags Snapshot Contract Alignment

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain / canonical architecture
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Align the canonical `RepositorySnapshot` contract with the existing `process.release-usage` required data so tag evidence and tag availability can be carried independently from releases.

## Starting State

~~~text
Branch: main
Base HEAD: d5424b8
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-015-repository-contents-workflows.md
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
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md
docs/roadmap/mvp-phases.md
src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
src/GitHubProof.Domain/Analysis/ValueObjects/DataAvailability.cs
src/GitHubProof.Domain/Analysis/Enums/DataAvailabilityStatus.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTagCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTagCollection.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTag.cs
tests/GitHubProof.Domain.Tests/Analysis/Models/RepositorySnapshotTests.cs
~~~

## Contract / Invariants Checked

- Tags are a required input for `process.release-usage` and remain distinct from releases.
- Signal rules, maximum score, status semantics, and version do not change.
- Tag availability must remain independently observable from release availability.
- Domain types remain provider-independent and immutable.

## Changes

- Added a separate `Tags` section to the canonical RepositorySnapshot schema.
- Added Tags to the Phase 2 collection list while leaving the release-usage signal contract unchanged.
- Added the provider-independent `RepositoryTag` Domain model.
- Added an optional Tags collection to `RepositorySnapshot`, defaulting to a complete empty collection for existing callers.
- Added focused tests for normalization, immutability, empty state, completeness, and release/tag availability independence.

## Decisions

- The approved contract alignment adds a separate Tags snapshot section.
- The existing free-form availability data key contract already supports the `tags` key; no enum or status change is required.
- `RepositoryTag` contains only `Name`; provider-specific fields are not needed by the canonical signal contract.
- The optional constructor parameter is appended to preserve existing source call compatibility.
- No orchestration or collector redesign is included in this session.
- No new third-party dependency will be introduced.

## Validation

~~~text
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore --filter FullyQualifiedName~RepositorySnapshotTests
PASS — 8 passed, 0 failed, 0 skipped

dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore
PASS — 46 passed, 0 failed, 0 skipped

dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore
PASS — 68 passed, 0 failed, 0 skipped

dotnet test GitHubProof.slnx --no-restore
PASS — 114 passed, 0 failed, 0 skipped

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

- RepositorySnapshot orchestration and collector coordination.
- Signal evaluation, scoring, and reporting.

## Working Set / Final Git State

~~~text
Modified:
- docs/architecture/analysis-domain-model.md
- docs/roadmap/mvp-phases.md
- src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
- src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
- tests/GitHubProof.Domain.Tests/Analysis/Models/RepositorySnapshotTests.cs
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- worklog/sessions/2026-08-15-016-tags-snapshot-contract-alignment.md

Pre-existing unrelated:
- NONE
~~~

## Next

Run a new model gate for Session 017 — RepositorySnapshot Orchestration.

## Git

~~~text
Suggested commit: fix(domain): align repository snapshot with tag evidence contract
Commit: PENDING — user-owned unless explicitly requested
Push: PENDING — user-owned unless explicitly requested
~~~

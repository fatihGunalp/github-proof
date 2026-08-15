# License Evidence Snapshot Alignment

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain, infrastructure, docs
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Preserve the already collected, provider-independent `LicenseFile` evidence losslessly across the RepositorySnapshot normalization boundary without implementing a signal evaluator.

## Starting State

~~~text
Branch: main
Base HEAD: dce9f4b
Pre-existing Working Tree: CLEAN
Local main: 2 commits ahead of origin/main
Relevant Previous Session: worklog/sessions/2026-08-15-020-simple-repository-evidence-signals.md
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
worklog/INDEX.md
worklog/sessions/2026-08-15-020-simple-repository-evidence-signals.md
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md — License section
docs/roadmap/mvp-phases.md — Phase 2 and Phase 3
RepositorySnapshot and RepositorySnapshotData
GitHubRepositoryDescriptiveDataCollection
GitHubRepositoryDescriptiveDataCollector
GitHubRepositorySnapshotOrchestrator
related Domain and Infrastructure tests
Session 021 task contract
~~~

## Contract Audit

- Existing RepositoryFile is provider-independent and already preserves normalized path, directory state, size, and content.
- No new LicenseEvidence abstraction is necessary for this bounded correction.
- A nullable trailing RepositoryFile parameter preserves all existing RepositorySnapshot constructor call sites while representing explicit absence naturally.
- Existing descriptive collection already distinguishes recognized metadata, detected file evidence, and no license response; only the snapshot mapping lost the dedicated file semantic.
- Existing license availability remains the authority for distinguishing successful absence from unavailable data.

## Changes

- Added nullable `RepositoryFile? LicenseFile` to RepositorySnapshot.
- Added an optional trailing constructor parameter so existing callers remain source-compatible.
- Mapped `descriptive.LicenseFile` directly in GitHubRepositorySnapshotOrchestrator.
- Retained the existing FileTree merge behavior for backward compatibility.
- Added Domain tests for recognized license plus file, unrecognized file without metadata, and the default no-evidence state.
- Added orchestrator tests for recognized, unrecognized-file, successful absence, and unavailable license states.
- Updated the canonical RepositorySnapshot contract and license evidence semantics in analysis-domain-model.md.

## Decisions

- Prefer nullable `RepositoryFile? LicenseFile` on RepositorySnapshot if the existing provider-independent type is sufficient.
- Preserve existing license availability semantics.
- Do not add evaluator logic, file-name heuristics, or a new dependency.
- `RepositoryLicense` continues to mean recognized license metadata only.
- Signal semantics, signal version, and scoring are unchanged.

## Validation

~~~text
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore --filter FullyQualifiedName~RepositorySnapshotTests: PASS — 11 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore --filter descriptive/orchestrator: PASS — 16 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 60 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 79 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 139 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Problems

None.

## Not Done

- Description or License signal evaluators.
- Other Signal Engine, scoring, report, UI, persistence, or live-validation work.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: dce9f4b
Pre-existing unrelated changes: NONE
Final working tree: Session 021 production, test, docs, and worklog changes only
~~~

## Next

Run a new mandatory model gate for a bounded retry of the Description + License evaluator task.

## Git

~~~text
Suggested commit: fix(domain): preserve license file evidence in repository snapshot
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

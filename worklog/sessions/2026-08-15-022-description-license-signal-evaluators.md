# Description and License Signal Evaluators

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain
**Model Route:** Terra / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement only the deterministic `repository.description.present` and `repository.license.present` evaluators using the corrected RepositorySnapshot license evidence contract.

## Starting State

~~~text
Branch: main
Base HEAD: b38f8a9
Pre-existing Working Tree: CLEAN
Relevant Previous Sessions: 019 (PASS), 020 (BLOCKED), 021 (PASS)
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
worklog/sessions/2026-08-15-019-signal-engine-readme-foundation.md
worklog/sessions/2026-08-15-020-simple-repository-evidence-signals.md
worklog/sessions/2026-08-15-021-license-evidence-snapshot-alignment.md
docs/architecture/mvp-signal-definitions.md — Description and License sections
docs/architecture/analysis-domain-model.md
docs/product/analysis-categories-and-signal-boundaries.md
docs/product/scoring-and-report-contract.md
ISignalEvaluator and ReadmeExistsSignalEvaluator
RepositorySnapshot, RepositoryMetadata, RepositoryLicense, RepositoryFile, DataAvailability
SignalDefinition, SignalResult, EvidenceItem, and EvidenceCollection
relevant Domain and Infrastructure tests
Session 022 task contract
~~~

## Contract Audit

- Description evaluation is deterministic from metadata availability and normalized RepositoryMetadata.Description.
- License evaluation is deterministic from license availability, recognized RepositoryLicense metadata, and the explicit normalized LicenseFile evidence added in Session 021.
- Availability is evaluated before description or license absence, so unavailable/failed/rate-limited data never becomes Missing.
- Existing SignalResult invariants support all required scores and recommendation obligations without modification.
- No unresolved contradiction remains after the Session 021 contract alignment.

## Changes

- Added DescriptionPresentSignalEvaluator for only `repository.description.present`.
- Added LicensePresentSignalEvaluator for only `repository.license.present`.
- Both evaluators reuse ISignalEvaluator, TimeProvider, EvidenceCollection, and SignalResult.
- Added focused tests for definitions, exact statuses/scores, evidence, recommendations, limitations, availability states, UTC evaluation, and deterministic results.
- Added a stale/partial license-evidence test proving unavailable availability takes precedence.

## Decisions

- Reuse ISignalEvaluator and explicit Domain evaluators only.
- No new dependency, registry, generic engine, or evaluator-side license heuristic.
- Description and License availability take precedence over absence inference.
- Description: Available + non-empty normalized description => Present/4; Available + absent => Missing/0; otherwise UnavailableEvidence/null.
- License: recognized metadata => Present/3; only LicenseFile => Partial/1; neither with Available => Missing/0; otherwise UnavailableEvidence/null.
- Present and Partial license results retain the legal-scope limitation without making legal-compliance claims.

## Validation

~~~text
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore --filter Description/License evaluators: PASS — 27 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore --filter ReadmeExistsSignalEvaluatorTests: PASS — 11 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore --filter RepositorySnapshotTests: PASS — 11 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore --filter GitHubRepositorySnapshotOrchestratorTests: PASS — 10 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 87 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 79 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 166 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Problems

- The initial focused Description test had an incorrect hard-coded character count (31 instead of 33). The expectation was corrected; evaluator behavior was unchanged.

## Not Done

- The other 12 signal evaluators and all evaluator orchestration.
- Scoring aggregation, report, UI, persistence, and live calibration.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: b38f8a9
Pre-existing unrelated changes: NONE
Final working tree: Session 022 production, test, and worklog changes only
~~~

## Next

Run a new mandatory model gate before selecting the next bounded RepositoryUnderstandability evaluator task.

## Git

~~~text
Suggested commit: feat(domain): add description and license signal evaluators
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

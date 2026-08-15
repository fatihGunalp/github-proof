# Signal Engine Foundation and README Exists Signal

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain
**Model Route:** Sol / medium
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Create the smallest deterministic Signal Engine production slice from normalized RepositorySnapshot evidence to SignalResult, implementing only `repository.readme.exists`.

## Starting State

~~~text
Branch: main
Base HEAD: b7a69eb
origin/main: b7a69eb
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-018-phase-2-integration-validation.md
~~~

## Context Read

~~~text
AGENTS.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
worklog/CURRENT.md
worklog/INDEX.md
Session 019 task contract
~~~

## Contract Audit

- Signal evaluation is a Domain business rule because it maps normalized domain evidence to canonical SignalResult semantics without transport or provider knowledge.
- Existing SignalDefinition, SignalResult, EvidenceCollection, EvidenceItem, SignalStatus, RepositorySnapshot, and DataAvailability contracts support this slice without modification.
- RepositorySnapshot exposes one normalized RepositoryReadme value, so ambiguous multiple README candidates are not representable. No speculative RequiresHumanReview path was added.
- Empty-repository handling remains an analysis-orchestration concern. This evaluator does not invent a signal-level NotApplicable policy.
- Any missing or non-Available README availability state maps to UnavailableEvidence, never Missing.

## Changes

- Added the minimal ISignalEvaluator boundary with Definition and Evaluate members.
- Added ReadmeExistsSignalEvaluator for only `repository.readme.exists`.
- Added observable README evidence for Present, Missing, and UnavailableEvidence results.
- Injected TimeProvider and read it once per evaluation for deterministic EvaluatedAt behavior.
- Added focused tests for the canonical definition, Present, Missing, every non-Available status, absent availability, evidence details, limitation text, and deterministic output.

## Decisions

- No new third-party dependency will be introduced.
- Only the README Exists evaluator is in scope.
- The evaluator lives in Domain and accepts RepositorySnapshot directly; GitHub HTTP and provider DTOs remain outside the boundary.
- Available README data with a normalized README produces Present and 5/5.
- Available README data without a normalized README produces Missing and 0/5 with inspection evidence and a recommendation.
- Unavailable README data produces UnavailableEvidence with null score and no README improvement recommendation.
- README presence carries the explicit limitation that existence does not prove sufficient, correct, or current content.

## Validation

~~~text
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore --filter FullyQualifiedName~ReadmeExistsSignalEvaluatorTests: PASS — 11 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 57 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 76 passed
Application tests: N/A — no Application test project
dotnet test GitHubProof.slnx --no-restore: PASS — 133 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Problems

- The first determinism assertion compared record collection references and produced a false negative. It was corrected to compare the complete domain state field by field; production behavior was unchanged.

## Not Done

- The other 14 signal evaluators.
- Category and overall scoring, analysis orchestration, reporting, and calibration.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: b7a69eb
Pre-existing unrelated changes: NONE
Final working tree: Session 019 production, test, and worklog changes only
~~~

## Next

Run a new mandatory model gate before selecting the next bounded Phase 3 signal-engine task.

## Git

~~~text
Suggested commit: feat(domain): add signal evaluator foundation and readme signal
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

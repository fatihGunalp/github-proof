# ImprovementPlan Domain Model

**Date:** 2026-08-15
**Result:** PASS
**Scope:** domain
**Model Route:** Terra / medium
**Model Gate:** APPROVED - user wrote `devam`

---

## Goal

Implement the contract-derived `ImprovementPlan` domain model and focused invariant tests.

## Starting State

~~~text
Branch: main
Base HEAD: e7ab44b
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-006-improvement-action.md
~~~

## Context Read

~~~text
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
worklog/CURRENT.md
docs/architecture/analysis-domain-model.md
docs/product/scoring-and-report-contract.md
src/GitHubProof.Domain/Analysis/Models/ImprovementAction.cs
src/GitHubProof.Domain/Analysis/ValueObjects/AnalysisVersion.cs
~~~

## Contract / Invariants Checked

- The plan owns the report-level maximum of three actions.
- Actions remain traceable through their own signal and evidence references.
- GeneratedAt is UTC and rule-set version is required.
- No undocumented priority ordering or minimum-action rule is added.

## Changes

Created:

- `ImprovementPlan` aggregate with action limit, generation time, and rule-set version.
- Four focused invariant tests.

Modified:

- Worklog current state and index.

## Decisions

- The aggregate enforces the report contract's maximum of three actions.
- Plans may be empty when no applicable action is available; no undocumented minimum is imposed.
- Priority is carried by each action without plan-level ordering rules.

## Validation

~~~text
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore
PASS - 34 passed, 0 failed, 0 skipped

dotnet test GitHubProof.slnx --no-restore
PASS - 34 passed, 0 failed, 0 skipped

dotnet build GitHubProof.slnx --no-restore
PASS - 0 warnings, 0 errors

dotnet format GitHubProof.slnx --verify-no-changes --no-restore
PASS

git diff --check
PASS
~~~

## Problems

None.

## Next

Prepare the AnalysisWarning domain-model task and run its mandatory model gate before implementation.

## Git

~~~text
Suggested commit: feat(domain): add improvement plan model
Commit: PENDING - user-owned unless explicitly requested
Push: PENDING - user-owned unless explicitly requested
~~~

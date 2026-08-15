# Issue + Pull Request + Release Signals

**Date:** 2026-08-15
**Result:** PASS
**Scope:** Version 1 issue, pull-request, and release canonical alignment, Domain evaluators, and focused regression tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement deterministic `process.issue-usage`, `process.pull-request-usage`, and `process.release-usage` evaluators over normalized repository snapshot evidence.

## Starting State

~~~text
Branch: main
Base HEAD: 86dee23
Pre-existing Working Tree: CLEAN
Production evaluators: 7 / 15
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
docs/architecture/mvp-signal-definitions.md — Issue, Pull Request, Release, and Contract Lock Version 1
docs/architecture/analysis-domain-model.md — normalized snapshot and UTC invariants
docs/product/analysis-categories-and-signal-boundaries.md — process evidence boundaries
docs/product/scoring-and-report-contract.md — nullable status semantics
Repository issue, pull-request, release, tag, collection, and availability Domain contracts
Relevant GitHub collectors and snapshot orchestration contracts
Session 032 task contract
~~~

## Plan

1. Audit normalized issue, PR, release/tag data, completeness, and independent availability semantics.
2. Add only the minimum executable Version 1 clarifications for issue qualification, release notes, and terminal incomplete evidence.
3. Implement all three Domain evaluators and focused boundary tests.
4. Run focused, evaluator regression, Domain, Infrastructure, solution, build, format, and diff validation.
5. Review scope and close the session worklog.

## Audit Decision

- Issues and pull requests are independently collected; the issue collector filters PR records before snapshot mapping.
- Issue open/closed state, PR open/closed/merged state, and all collection completeness flags survive normalization.
- Issues-disabled is preserved as `issues = NotApplicable`.
- Releases and tags are independently collected, paged, normalized, mapped, and exposed with separate availability records.
- Release records preserve published/draft state and normalized optional notes.
- No snapshot or Infrastructure data gap exists.
- Version 1 qualifying issue means any normalized non-PR issue record; title/body quality is outside the signal boundary.
- Version 1 release-note presence means a normalized non-empty `Notes` value; NLP or quality inference is outside scope.
- Incomplete evidence may preserve only a terminal positive result that no unseen item or independent source can improve.

## Constraints

- Preserve all Version 1 thresholds, scores, statuses, categories, and overall inclusion.
- Preserve Issues-disabled as the only automatic issue `NotApplicable` path.
- Missing requires complete and available required evidence.
- Do not merge Releases and Tags availability or infer repository type.
- No Infrastructure, snapshot, orchestration, scoring, report, Web/API, or other evaluator changes.
- No package, generic signal engine, registry, MediatR, or CQRS.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: COMPLETE — canonical alignment, three evaluators, focused tests, full regression validation, and worklog closeout passed.
Next: Implement test-structure and continuous-integration signals in the next bounded session.
~~~

## Canonical Clarification

- A Version 1 qualifying issue is every normalized `RepositoryIssue`; provider normalization already excludes pull requests and content quality is not scored.
- Two observed issues preserve terminal Present and two observed merged PRs preserve terminal Strong even if later pagination fails; smaller incomplete sets remain UnavailableEvidence because unseen records can change the result.
- A release note is present when a published release has a normalized non-empty `Notes` value; length, language, and NLP quality are not evaluated.
- Published release with notes is terminal Present even when other release/tag pages are unavailable.
- Published without notes requires complete available Releases but does not require Tags; a visible tag requires complete available Releases but additional Tags cannot change its Partial result.
- Draft-only and Missing require both Releases and Tags to be complete and available.

## Implementation

- Added `IssueUsageSignalEvaluator` with Issues-disabled NotApplicable and the exact 0, 1, and 2+ matrix.
- Added `PullRequestUsageSignalEvaluator` with merged precedence and exact Missing, Partial, Present, and Strong branches.
- Added `ReleaseUsageSignalEvaluator` with published notes, published without notes, tag-only, draft-only, Missing, and independent availability behavior.
- Each evaluator reuses `ISignalEvaluator`, reads `TimeProvider` once, consumes only normalized Domain data, and generates ordering/culture-independent evidence.
- Evidence records all requested counts, completeness flags, and independent availability states without copying titles, bodies, or lists.
- Recommendations and limitations preserve canonical evidence boundaries and do not infer management quality, code review, teamwork, stability, or maintenance.

## Test Coverage

- Issue: disabled, 0, 1, 2, open/closed mix, PR separation, unavailable, incomplete, terminal incomplete Present, and small-repository behavior.
- Pull Request: 0, open, closed-unmerged, one/two merged, mixed state, unavailable, incomplete, terminal incomplete Strong, and single-owner behavior.
- Release: published with notes, null/empty/whitespace notes, tag-only, draft-only, none, independent Releases/Tags failures, incomplete terminal evidence, and ordering.
- Exact definition fields, evidence, recommendations, limitations, deterministic output, and one clock read per evaluation.

## Validation

~~~text
focused Issue/PR/Release evaluator tests: PASS — 46 passed
all evaluator regression: PASS — 261 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 312 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 400 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Architecture and Scope Review

~~~text
ISignalEvaluator reused: YES
Provider/HTTP/GitHub DTO dependency in Domain: NO
Generic signal engine or evaluator registry: NO
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
Infrastructure/snapshot/orchestration/scoring/report change: NO
Production evaluators after PASS: 10 / 15
~~~

## Problems

None.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 86dee23
Pre-existing unrelated changes: NONE

Modified:
- docs/architecture/mvp-signal-definitions.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Domain/Analysis/Signals/IssueUsageSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/PullRequestUsageSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/ReleaseUsageSignalEvaluator.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/IssueUsageSignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/PullRequestUsageSignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/ReleaseUsageSignalEvaluatorTests.cs
- worklog/sessions/2026-08-15-032-issue-pr-release-signals.md
~~~

## Next

Implement `validation.test-structure` and `validation.continuous-integration` in the next bounded session.

## Git

~~~text
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

Suggested commit: `feat(domain): add collaboration and release signal evaluators`

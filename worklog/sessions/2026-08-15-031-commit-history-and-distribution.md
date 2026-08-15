# Commit History + Commit Distribution

**Date:** 2026-08-15
**Result:** PASS
**Scope:** Version 1 commit-history and commit-distribution canonical alignment, Domain evaluators, and focused regression tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement deterministic `process.commit-history` and `process.commit-distribution` evaluators over the normalized repository snapshot without leaking provider concerns into Domain.

## Starting State

~~~text
Branch: main
Base HEAD: 17c74aa
Pre-existing Working Tree: CLEAN
Production evaluators: 5 / 15
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
docs/architecture/mvp-signal-definitions.md — Commit History and Commit Distribution Version 1
docs/architecture/analysis-domain-model.md — RepositorySnapshot, UTC, and availability invariants
docs/product/scoring-and-report-contract.md — nullable status semantics
Repository snapshot and commit collection Domain contracts
GitHub commit collector, pagination, normalization, and orchestration contracts
Session 031 task contract
~~~

## Plan

1. Audit commit collection completeness, availability, normalization, and temporal contracts.
2. Add only the minimum executable clarification needed for deterministic Version 1 behavior.
3. Implement both Domain evaluators and focused boundary tests.
4. Run focused, regression, full solution, formatting, and diff validation.
5. Review scope and close the session worklog.

## Audit Decision

- The collector follows default-branch pagination, removes duplicate SHA values before snapshot construction, and normalizes committer timestamps to UTC.
- A failed later page preserves observed items but marks the commit collection incomplete and `commits` unavailable.
- Version 1 therefore does not infer a monotonic terminal score from incomplete pagination; incomplete commit data remains `UnavailableEvidence`.
- Repository age is measured at snapshot capture time so identical snapshot input produces identical score state.
- Active days use UTC calendar dates; first-last span uses elapsed UTC time.
- The evaluator consumes the normalized snapshot collection as-is and adds no second duplicate-removal rule.

## Constraints

- Preserve all Version 1 thresholds, statuses, scores, category, and overall inclusion.
- Missing requires complete, available evidence.
- No Infrastructure, collector, snapshot, scoring, orchestration, report, Web/API, or other signal changes.
- No new package, generic rules framework, MediatR, or CQRS.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: COMPLETE — canonical alignment, both evaluators, focused tests, full regression validation, and worklog closeout passed.
Next: Implement issue, pull-request, and release usage signals in the next bounded session.
~~~

## Canonical Clarification

- Numeric and Missing results require `commits = Available` plus a complete commit collection.
- Incomplete pagination remains `UnavailableEvidence` even when observed values cross a positive threshold; Version 1 makes no monotonic terminal inference.
- Commit count uses normalized snapshot items; evaluator input order does not matter and no secondary SHA dedupe is introduced.
- Repository age is elapsed UTC time from metadata `CreatedAt` to snapshot `CapturedAt`; exactly 14 days is applicable.
- Active days are distinct UTC calendar dates; first-last span is elapsed time between earliest and latest normalized UTC timestamps; exactly 14 days satisfies Strong.
- A `CreatedAt` later than `CapturedAt` is inconsistent required data and produces `UnavailableEvidence`.

## Implementation

- Added `CommitHistorySignalEvaluator` with the complete Version 1 0, 1, 2, 3–4, 5–6, 7–9, and 10+ status/score matrix.
- Added `CommitDistributionSignalEvaluator` with repository-age applicability, all active-day thresholds, and the eight-day plus fourteen-day Strong conjunction.
- Both evaluators reuse `ISignalEvaluator`, consume only normalized Domain snapshot data, read `TimeProvider` once, and produce culture-independent deterministic evidence.
- History evidence records observed count, completeness, availability, and default branch.
- Distribution evidence records repository age, active-day count, first/last UTC commit timestamps, span, completeness, and metadata/commit availability without copying commit messages.
- Recommendations and limitations preserve the canonical boundaries; imported-repository possibility is limitation-only and never changes score.

## Test Coverage

- Commit History: 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, and greater-than-10 boundaries.
- Commit Distribution: repository age below/exactly 14 days; 0, 1, 2, 3, 4, 5, 6, 7, and 8 active days; short/equal-14-day spans.
- Metadata and commit availability states, incomplete pagination, observed-positive partial data, same UTC day grouping, unsorted inputs, normalized duplicate semantics, invalid age, evidence fields, and deterministic output.

## Validation

~~~text
focused two-evaluator tests: PASS — 50 passed
all evaluator regression: PASS — 215 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 266 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 354 passed
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
Production evaluators after PASS: 7 / 15
~~~

## Problems

None. The first focused run exposed two test-only record-array reference equality assertions; field-level deterministic state assertions replaced them and all gates passed.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 17c74aa
Pre-existing unrelated changes: NONE

Modified:
- docs/architecture/mvp-signal-definitions.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Domain/Analysis/Signals/CommitHistorySignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/CommitDistributionSignalEvaluator.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/CommitHistorySignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/CommitDistributionSignalEvaluatorTests.cs
- worklog/sessions/2026-08-15-031-commit-history-and-distribution.md
~~~

## Next

Implement issue, pull-request, and release signals in a subsequent bounded session.

## Git

~~~text
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

Suggested commit: `feat(domain): add commit process signal evaluators`

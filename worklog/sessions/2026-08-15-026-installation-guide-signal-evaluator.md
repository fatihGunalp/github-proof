# Installation Guide Signal Evaluator Retry

**Date:** 2026-08-15
**Result:** PASS
**Scope:** `documentation.installation-guide` production evaluator and focused regression tests only
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement the deterministic Version 1 `documentation.installation-guide` evaluator against normalized README and bounded `docs/**/*.md` snapshot evidence, preserving unavailable, incomplete, missing, and bounded ambiguity semantics.

## Starting State

~~~text
Branch: main
Base HEAD: ad5d0da
Pre-existing Working Tree: CLEAN
Relevant Previous Sessions: 023 (BLOCKED), 024 (PASS), 025 (PASS)
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
worklog/sessions/2026-08-15-023-installation-guide-signal.md
worklog/sessions/2026-08-15-024-installation-guide-semantic-contract.md
worklog/sessions/2026-08-15-025-documentation-evidence-snapshot-alignment.md
docs/architecture/mvp-signal-definitions.md — Installation Guide section
docs/architecture/analysis-domain-model.md — RepositorySnapshot, DocumentationMarkdown, and DataAvailability
docs/product/scoring-and-report-contract.md — status, evidence, recommendation, and limitation semantics
docs/product/analysis-categories-and-signal-boundaries.md — Installation Guide boundary
Session 026 task contract
~~~

## Plan

1. Audit the existing evaluator and normalized snapshot patterns.
2. Implement only `InstallationGuideSignalEvaluator` with small private deterministic helpers.
3. Add focused tests for headings, actionability, Strong features, corpus availability, ambiguity, and determinism.
4. Run focused and full regression validation, review scope, and finalize the worklog.

## Constraints

- No canonical contract, signal score, or Version change.
- No provider, HTTP, GitHub DTO, or Infrastructure dependency in Domain.
- No third-party Markdown package or generic evaluator framework.
- No other signal evaluator, runner, registry, scoring aggregation, or report work.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: COMPLETE — evaluator, tests, regression validation, and worklog closeout passed.
Next: Session 027 — documentation.usage-or-demo.
~~~

## Implementation

- Added the production `InstallationGuideSignalEvaluator` as an `ISignalEvaluator` using only normalized Domain snapshot data.
- Kept the canonical signal key, RepositoryUnderstandability category, maximum score 8, Version 1, and overall-score inclusion unchanged.
- Parsed exact case-insensitive ATX and Setext installation headings while excluding fenced-code content and respecting same-or-higher section boundaries.
- Detected only canonical actionable evidence: non-empty closed fences, non-empty ordered steps, and locked English/Turkish imperatives.
- Detected prerequisites, dependency installation, configuration, build, run, and migration/preparation with canonical feature priority and per-evidence identity de-duplication.
- Kept Strong feature aggregation within one coherent source section and sorted docs inputs with `StringComparer.Ordinal`.
- Preserved reliable Strong/Present evidence from incomplete collections while requiring complete README + docs corpus for Partial or Missing.
- Distinguished unavailable/incomplete corpus from missing evidence and limited human review to observable bounded heading/fence ambiguity.
- Produced concise deterministic evidence, recommendations, and canonical positive-result limitations without copying source bodies.
- Reused `TimeProvider` and read it once per evaluation.

## Test Coverage

- All ten canonical headings, case-insensitive ATX, Setext, and trailing ATX decoration.
- Same-or-higher section boundaries and lower-level subsection inclusion.
- Body keyword, badge/URL, installer identifier, non-canonical headings, and typo false positives.
- Non-empty/empty fences, ordered steps/artifacts, English/Turkish imperatives, and explanatory prose.
- Two versus three Strong features, all six feature families, distinct identity de-duplication, and source-local aggregation.
- Strong, Present, Partial, Missing, UnavailableEvidence, and RequiresHumanReview status selection.
- Incomplete/unavailable corpus positive preservation, lower-result exclusion, deterministic time, and Ordinal docs ordering.

## Validation

~~~text
focused InstallationGuide evaluator tests: PASS — 54 passed
existing README/Description/License evaluator regression: PASS — 38 passed
RepositorySnapshot regression: PASS — 13 passed
documentation collector + contents + orchestrator regression: PASS — 28 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 143 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 231 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Architecture and Scope Review

~~~text
ISignalEvaluator reused: YES
Provider/HTTP/GitHub DTO dependency in Domain: NO
Generic Markdown or rule framework: NO
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
Canonical docs changed: NO
Other evaluators or collection behavior changed: NO
~~~

## Working Set / Final Git State

~~~text
Branch: main
HEAD: ad5d0da
Pre-existing unrelated changes: NONE
Final working tree: Session 026 Domain evaluator, focused tests, and worklog changes only
~~~

## Next

Session 027 — implement `documentation.usage-or-demo` only after its mandatory model gate and contract audit.

## Git

~~~text
Suggested commit: feat(domain): add installation guide signal evaluator
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

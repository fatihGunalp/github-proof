# Usage/Demo Signal Evaluator

**Date:** 2026-08-15
**Result:** BLOCKED — CONTRACT CONTRADICTION
**Scope:** `documentation.usage-or-demo` production evaluator and focused regression tests only
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement the deterministic Version 1 `documentation.usage-or-demo` evaluator against normalized README, bounded `docs/**/*.md`, and conditional homepage corroboration evidence without changing the canonical contract or snapshot model.

## Starting State

~~~text
Branch: main
Base HEAD: 9b1bf85
Pre-existing Working Tree: CLEAN
Relevant Previous Sessions: 027 (BLOCKED), 028 (PASS)
Production evaluators: 4 / 15
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
worklog/sessions/2026-08-15-027-usage-demo-contract-audit.md
worklog/sessions/2026-08-15-028-usage-demo-semantic-contract.md
docs/architecture/mvp-signal-definitions.md — Usage/Demo Version 1
docs/architecture/analysis-domain-model.md — README, DocumentationMarkdown, Metadata.Homepage, availability, and completeness
docs/product/scoring-and-report-contract.md
Session 029 task contract
~~~

## Plan

1. Inspect existing evaluator and snapshot-test patterns.
2. Implement only `UsageDemoSignalEvaluator` with small private deterministic helpers.
3. Add focused tests for detection, status precedence, availability, source locality, and determinism.
4. Run focused and full regression validation, review scope, and finalize the worklog.

## Constraints

- No canonical contract, signal score, Version, snapshot, collector, or orchestrator change.
- No provider, HTTP, GitHub DTO, or Infrastructure dependency in Domain.
- No third-party Markdown package or generic evaluator/rule framework.
- No other signal evaluator, runner, registry, scoring aggregation, report, Web/API, or live validation work.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: BLOCKED — complete-corpus high-confidence visual/demo-only evidence has no canonical status.
Next: Amend the canonical Version 1 status rules, then retry the evaluator in a new session.
~~~

## Contract Contradiction

The canonical contract cannot classify this valid, fully available snapshot state:

~~~text
README + DocumentationMarkdown corpus: Available + Complete
Clear usage example count: 0
High-confidence visual/demo evidence count: >= 1
Bounded weak evidence count: 0
Malformed ambiguity: NONE
~~~

The locked rules conflict for that state:

- `Strong` requires clear usage plus distinct high-confidence demo/visual evidence in the same source, so it does not apply.
- `Present` requires at least one clear usage example, so it does not apply.
- `Partial` requires that only bounded weak presentation evidence exists; high-confidence screenshot, video, or corroborated live-demo evidence is explicitly outside the two allowed weak candidates.
- `Missing` requires that no clear usage, high-confidence demo/visual, or bounded weak evidence exists, so it does not apply.
- `UnavailableEvidence` does not apply because the required corpus is complete and available.
- `RequiresHumanReview` is limited to malformed Markdown ambiguity, which is absent.

The cross-document product boundary also lists screenshots, videos, and live demos as valid positive Usage/Demo evidence. Therefore ignoring high-confidence visual/demo-only evidence or treating it as Missing would contradict the product boundary as well as the signal's Missing rule.

Implementing the evaluator would require a new product/domain decision, most plausibly whether high-confidence visual/demo-only evidence is `Partial / 4` or another explicitly defined status. No such decision was made silently.

## Changes

~~~text
Created:
- worklog/sessions/2026-08-15-029-usage-demo-signal-evaluator.md

Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Production code: NONE
Test code: NONE
Canonical docs: NONE
Dependencies: NONE
~~~

## Validation

~~~text
Repository baseline: PASS — main at 9b1bf85, clean before Session 029
Canonical Usage/Demo status exhaustiveness review: BLOCKED — high-confidence visual/demo-only state is unclassified
Cross-document signal-boundary review: PASS — screenshots, videos, and live demos are valid positive evidence
Production/test scope review: PASS — no src/** or tests/** changes
dotnet test/build/format: NOT RUN — implementation was correctly stopped before code changes
git diff --check: PASS
~~~

## Problems

~~~text
Problem:
The canonical Version 1 status rules are not exhaustive for complete-corpus high-confidence visual/demo-only evidence.

Cause:
Partial is restricted to two bounded weak candidates, while Missing explicitly excludes any high-confidence visual/demo evidence.

Resolution:
BLOCKED — canonical product/domain contract decision required before implementation.
~~~

## Not Done

- `UsageDemoSignalEvaluator` was not created.
- Focused or regression tests were not created or run.
- No snapshot, collector, evaluator, scoring, dependency, or canonical docs changes were made.

## Known Risks

- Choosing a status in code before the canonical contract is amended would make deterministic behavior implementation-defined and could change scoring semantics silently.

## Working Set / Final Git State

~~~text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- worklog/sessions/2026-08-15-029-usage-demo-signal-evaluator.md

Pre-existing unrelated: NONE
~~~

## Next

Define the canonical status and score behavior for complete-corpus high-confidence screenshot/video/live-demo evidence without a clear usage example, then retry the Usage/Demo evaluator.

## Git

~~~text
Suggested commit: docs(worklog): record usage demo evaluator contract contradiction
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

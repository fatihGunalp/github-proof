# Homepage Contract Fix + Portfolio Evaluators

**Date:** 2026-08-15
**Result:** PASS
**Scope:** minimum Homepage/Live Demo Version 1 contract fix, Topics and Homepage Domain evaluators, and focused regression tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Close the Session 034 Homepage/Live Demo contract gap with the minimum canonical clarification, then implement `portfolio.topics` and `portfolio.homepage-or-live-demo` against normalized Domain snapshot evidence.

## Starting State

```text
Branch: main
Base HEAD: 14f4d48
Pre-existing Working Tree: CLEAN
Active Session: NONE
Production evaluators: 12 / 15
```

## Context Read

```text
AGENTS.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
worklog/sessions/2026-08-15-028-usage-demo-semantic-contract.md
worklog/sessions/2026-08-15-030-usage-demo-contract-fix-and-evaluator.md
worklog/sessions/2026-08-15-034-topics-and-homepage-signals.md
docs/architecture/mvp-signal-definitions.md — Topics and Homepage/Live Demo
docs/architecture/analysis-domain-model.md — normalized metadata, README, and availability evidence
docs/product/analysis-categories-and-signal-boundaries.md — Portfolio evidence boundary
docs/product/scoring-and-report-contract.md — status and nullable-score semantics
docs/decisions/001-portfolio-presentation-category.md — Portfolio category decision
Session 035 task contract
```

## Plan

1. Reconfirm the Session 034 gaps against the authorized Homepage contract fix.
2. Apply only the minimum executable Version 1 clarification to the canonical signal definition.
3. Implement Topics and Homepage evaluators with focused status, parsing, availability, and determinism tests.
4. Run focused and full validation, review scope, and close the session worklog.

## Constraints

- Preserve signal Version, scores, category, and overall inclusion.
- Do not implement `portfolio.presentation-completeness`, Application orchestration, registry, report, Web/API, or Infrastructure changes.
- Do not perform remote HTTP/DNS/404 validation.
- Do not add dependencies, a generic parser/rules framework, MediatR, or CQRS.
- Commit and push remain user-owned.

## Checkpoint

```text
Status: COMPLETE — canonical fix, two Domain evaluators, focused tests, and full regression validation passed.
Next: Session 036 — Application Signal Orchestration + portfolio.presentation-completeness.
```

## Contract Fix

- Reconfirmed all four Session 034 gaps, changed the primary Homepage/Live Demo definition, and aligned only the directly conflicting product-boundary and ADR wording.
- Defined meaningful metadata homepage as an absolute HTTP/HTTPS URL outside the same GitHub repository URL/subtree and explicit documentation-only host/path markers.
- Reused Usage/Demo Version 1 URL normalization and exact English/Turkish demo-intent labels.
- Limited README parsing to inline Markdown and HTML anchors outside fenced code; generic links, badges, images, and target-only keywords do not become candidates.
- Defined bounded ambiguous intent through exact label tokens and mapped it to `Partial / 2`.
- Limited `RequiresHumanReview` to relevant malformed Markdown/HTML/URL structures that block exact classification.
- Locked terminal `Present / 5`, metadata-unavailable, README-unavailable, `Present / 4`, human-review, Partial, and Missing precedence.
- Explicitly prohibited use of stale snapshot values when the corresponding availability is not `Available`.
- Preserved scores, `Version: 1`, category, overall inclusion, and the no-remote-health rule.

## Implementation

- Added `TopicsSignalEvaluator` using normalized lowercase distinct topics and Ordinal evidence ordering.
- Topics maps `>=3`, `2`, `1`, and `0` to the canonical Strong/Present/Missing scores and never treats unavailable metadata as Missing.
- Added `HomepageLiveDemoSignalEvaluator` using only `RepositorySnapshot`, metadata, README content, repository identity, and availability.
- Meaningful metadata homepage is terminal 5-point evidence even when README is unavailable.
- Metadata unavailable prevents a README 4-point result because a missing homepage could change the exact score.
- README exact demo intent produces 4; bounded near-intent produces 2; generic links and fenced examples are ignored.
- Relevant malformed metadata/README candidates produce bounded human review, while complete no-candidate evidence produces Missing.
- Evidence contains only normalized URLs, concise labels/counts, source locations, and availability notes; no README body is copied.
- Both evaluators read `TimeProvider` once and produce deterministic evidence ordering.

## Test Coverage

- Topics: every score threshold, unavailable statuses, missing availability, whitespace/case duplicate normalization, concise evidence, input-order determinism, and definition fields.
- Homepage: terminal metadata homepage, URL normalization, same-repository management URLs, exact documentation markers, every canonical demo label, Markdown/HTML target normalization, ambiguous intent, Missing, both availability directions, generic/badge/fenced exclusions, malformed metadata/README, remote-health absence, determinism, and definition fields.

## Validation

```text
Repository baseline: PASS — main at 14f4d48, clean before Session 035
focused Topics + Homepage evaluator tests: PASS — 49 passed
all evaluator regression: PASS — 356 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 407 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 495 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
```

## Architecture and Scope Review

```text
ISignalEvaluator reused: YES
Provider HTTP/JSON/GitHub DTO dependency in Domain: NO
Remote URL health validation: NO
Generic Markdown/rules framework: NO
Infrastructure/snapshot/orchestration change: NO
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
portfolio.presentation-completeness: NOT IMPLEMENTED
Production evaluators after PASS: 14 / 15
```

## Problems

None.

## Working Set / Final Git State

```text
Branch: main
HEAD: 14f4d48
Pre-existing unrelated changes: NONE

Modified:
- docs/architecture/mvp-signal-definitions.md
- docs/product/analysis-categories-and-signal-boundaries.md
- docs/decisions/001-portfolio-presentation-category.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Domain/Analysis/Signals/TopicsSignalEvaluator.cs
- src/GitHubProof.Domain/Analysis/Signals/HomepageLiveDemoSignalEvaluator.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/TopicsSignalEvaluatorTests.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/HomepageLiveDemoSignalEvaluatorTests.cs
- worklog/sessions/2026-08-15-035-homepage-contract-and-portfolio-signals.md
```

## Next

Session 036 — implement Application Signal Orchestration and the derived `portfolio.presentation-completeness` signal after its mandatory model gate.

## Git

```text
Suggested commit: feat(domain): add portfolio metadata signal evaluators
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
```

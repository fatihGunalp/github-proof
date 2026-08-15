# Portfolio Topics + Homepage Signals

**Date:** 2026-08-15
**Result:** BLOCKED — CONTRACT GAP
**Scope:** `portfolio.topics` and `portfolio.homepage-or-live-demo` canonical data and semantic audit only
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement the two independent Portfolio Presentation evaluators using normalized snapshot evidence while preserving deterministic status, availability, and provider-isolation contracts.

## Starting State

```text
Branch: main
Base HEAD: d82884d
Pre-existing Working Tree: CLEAN
Active Session: NONE
Production evaluators: 12 / 15
```

## Context Read

```text
AGENTS.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
worklog/INDEX.md
docs/architecture/mvp-signal-definitions.md — Topics and Homepage/Live Demo
docs/architecture/analysis-domain-model.md — snapshot and availability evidence
docs/product/scoring-and-report-contract.md — nullable status semantics
docs/product/analysis-categories-and-signal-boundaries.md — Portfolio evidence boundary
docs/decisions/001-portfolio-presentation-category.md
Sessions 028 and 030 — live-demo/link consistency
Session 034 task contract
```

## Plan

1. Audit normalized topics, homepage, README, and independent availability evidence.
2. Confirm that “meaningful homepage”, explicit README demo link, Partial, and human-review rules are executable without arbitrary heuristics.
3. If the canonical contract is sufficient, implement both evaluators and focused tests; otherwise stop at the documented contract gap.
4. Run proportional validation, review scope, and close the session worklog.

## Constraints

- Do not implement `portfolio.presentation-completeness` or signal orchestration.
- Do not add Infrastructure changes unless a real normalized-data gap is confirmed.
- Do not add remote URL health checks, third-party packages, generic parsing/rules frameworks, MediatR, or CQRS.
- Do not invent a product heuristic where multiple reasonable interpretations remain.
- Commit and push remain user-owned.

## Checkpoint

```text
Status: BLOCKED — Homepage/Live Demo Version 1 semantics are not deterministic or exhaustive.
Next: lock the Homepage/Live Demo semantic and availability-precedence rules in canonical docs, then retry both evaluators.
```

## Outcome

```text
BLOCKED — CONTRACT GAP
Topics data and semantics: SUFFICIENT
Homepage/Live Demo raw data: SUFFICIENT
Homepage/Live Demo semantic contract: NOT EXECUTABLE
Production evaluators remain: 12 / 15
```

The Homepage/Live Demo evaluator cannot be implemented without choosing among multiple reasonable classifications for the same normalized snapshot. Per the task contract, no arbitrary URL, label, or availability heuristic was introduced. Topics was not implemented separately because the bounded session goal requires both independent evaluators and explicitly stops on a multi-interpretation product gap.

## Topics Audit

- `RepositorySnapshot.Topics` carries the required provider-independent list.
- Snapshot and GitHub metadata normalization both remove null/blank entries, trim values, and deduplicate with `OrdinalIgnoreCase`.
- `metadata` availability atomically distinguishes a successfully inspected empty topic list from unavailable metadata.
- Counts `0`, `1`, `2`, and `>=3` therefore map deterministically to the canonical statuses and scores.
- An evaluator can sort concise topic evidence with `StringComparer.Ordinal` so input ordering does not affect output domain state.
- No Infrastructure, snapshot, collector, dependency, or canonical change is required for `portfolio.topics`.

## Homepage Data Audit

- `RepositoryMetadata.Homepage` preserves the normalized optional metadata value.
- `RepositoryReadme` carries the full provider-independent README path and content.
- `metadata` and `readme` availability are recorded independently.
- Existing Usage/Demo Version 1 rules define exact demo-intent labels, absolute HTTP/HTTPS URL normalization, fenced-content exclusion, and no remote health check.
- These raw fields are sufficient once Homepage/Live Demo's own classification and precedence rules are locked.
- No Infrastructure or snapshot representation gap was found.

## Blocking Semantic Gaps

### Meaningful metadata homepage

The canonical contract awards `Present / 5` for a “meaningful” homepage but does not define an observable boundary. Multiple reasonable implementations remain, including:

- any non-empty homepage string;
- any syntactically valid absolute HTTP/HTTPS URL;
- an external URL after excluding the repository/source/issues links;
- only a URL whose target type can be classified as product, documentation, demo, example, or project page.

The current docs do not select one of these rules or define how malformed, relative, self-referential, GitHub-hosted, or generic website values are classified.

### README evidence boundary

The purpose and evidence-type sections accept documentation, published examples, and product/project pages, while the scoring table awards the 4-point branch only to an “explicit demo link.” Session 028/030 provides a deterministic exact-label rule for demo intent, but it does not decide how this separate signal scores an explicit README documentation/product link.

### Partial versus RequiresHumanReview

The same scoring table says both:

```text
link purpose cannot be determined -> Partial / 2
link meaning is ambiguous          -> RequiresHumanReview / null
```

No bounded observable distinction or precedence separates these states. A generic or context-poor README link can reasonably satisfy either rule.

### Availability and terminal-result precedence

The table states that unavailable README **or** metadata produces `UnavailableEvidence`, but it does not define terminal-positive exceptions:

- README unavailable + meaningful metadata homepage: `Present / 5` may be monotonic maximum evidence, or the literal unavailable rule may win.
- Metadata unavailable + explicit README demo: `Present / 4` may be preserved, or unavailable metadata may win because a homepage could raise the result to 5.

The Usage/Demo signal's precedence cannot silently be applied because that contract explicitly says homepage is corroboration-only and has its own corpus rules. Homepage/Live Demo has no equivalent Version 1 precedence section.

### Human-review triggers

The general scoring contract defines the meaning of `RequiresHumanReview` but this signal does not bound it to malformed syntax, conflicting labels, or another observable state. Using it as a general semantic uncertainty bucket would conflict with deterministic scoring and overlap `Partial / 2`.

## Minimum Canonical Correction

1. Define exactly what makes `RepositoryMetadata.Homepage` meaningful, including accepted URL syntax and excluded targets.
2. Define the supported README link syntax, exact eligible intent labels/contexts, and treatment of explicit docs/product/project-page links.
3. Separate `Partial / 2` from bounded `RequiresHumanReview / null` with observable triggers and precedence.
4. Define metadata/README availability precedence, including whether terminal `Present / 5` survives unavailable README and whether `Present / 4` survives unavailable metadata.
5. Preserve the existing no-HTTP/DNS/404 rule and state the minimum deterministic evidence fields.

## Changes

```text
Created:
- worklog/sessions/2026-08-15-034-topics-and-homepage-signals.md

Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Production code: NONE
Test code: NONE
Canonical docs: NONE
Dependencies: NONE
```

## Validation

```text
Repository baseline: PASS — main at d82884d, clean before Session 034
Topics data/semantic audit: PASS — executable without contract or data changes
Homepage raw-data audit: PASS — metadata, README, and independent availability are sufficient
Homepage semantic audit: BLOCKED — multiple reasonable classifications remain
Production test/build/format: NOT RUN — implementation correctly stopped before code changes
git diff --check: PASS
```

## Architecture and Scope Review

```text
ISignalEvaluator implementation: NOT CREATED
Provider/HTTP/GitHub DTO dependency in Domain: NONE
Remote URL health check: NOT INTRODUCED
Generic parsing/rules framework: NOT INTRODUCED
Infrastructure/snapshot/collector change: NONE
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
portfolio.presentation-completeness: NOT IMPLEMENTED
```

## Working Set / Final Git State

```text
Branch: main
HEAD: d82884d
Pre-existing unrelated changes: NONE
Final working tree: Session 034 worklog changes only
```

## Next

Create a bounded canonical contract-alignment session for `portfolio.homepage-or-live-demo`, then retry Topics + Homepage evaluators. Application orchestration and `portfolio.presentation-completeness` remain deferred until the two independent Portfolio evaluators pass.

## Git

```text
Suggested commit: docs(worklog): record homepage signal contract gaps
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
```

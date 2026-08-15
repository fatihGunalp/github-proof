# Usage/Demo Signal Contract Audit

**Date:** 2026-08-15
**Result:** BLOCKED
**Scope:** canonical data and semantic contract audit only
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Determine whether `documentation.usage-or-demo` Version 1 can be implemented deterministically from the current canonical contract and normalized `RepositorySnapshot` evidence, without inventing heuristics or changing production/test code.

## Starting State

~~~text
Branch: main
Base HEAD: 30640fc
Pre-existing Working Tree: CLEAN
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
docs/architecture/mvp-signal-definitions.md — Usage/Demo, Homepage/Live Demo, and Contract Lock v1
docs/architecture/analysis-domain-model.md — RepositorySnapshot, RepositoryMetadata, RepositoryReadme, DocumentationMarkdown, and DataAvailability
docs/product/analysis-categories-and-signal-boundaries.md — Usage/Demo evidence boundary
docs/product/scoring-and-report-contract.md — status, evidence, recommendation, and uncertainty semantics
RepositorySnapshot and RepositorySnapshotData
DataAvailability
GitHubRepositoryMetadataCollector
GitHubRepositoryDescriptiveDataCollector
GitHubRepositorySnapshotOrchestrator
Session 027 task contract
~~~

## Plan

1. Extract the canonical Usage/Demo data, scoring, and evidence contract.
2. Verify whether README, homepage, links, images, code blocks, and availability are represented and collected.
3. Test each semantic term for a single executable deterministic interpretation.
4. Record PASS or the exact blocking contract gaps; do not implement an evaluator or modify canonical docs.

## Constraints

- No production or test code changes.
- No canonical docs, snapshot, collector, signal Version, or score changes.
- No dependency, Markdown parser/helper, evaluator, or generic Signal Engine work.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: Audit complete — canonical Usage/Demo contract is not deterministic/executable.
Next: Lock the corpus and semantic rules in canonical docs before evaluator implementation.
~~~

## Outcome

~~~text
BLOCKED — SEMANTIC CONTRACT GAP
Additional canonical data-scope contradiction: README-only RequiredData versus README-or-documentation evidence boundary.
Current snapshot representation gap: NONE confirmed for the raw README/homepage/docs evidence already collected.
~~~

The evaluator cannot be implemented without choosing among multiple reasonable classifications for the same README. No arbitrary keyword list, URL heuristic, or provider-specific workaround was introduced.

## Data Contract Audit

### README

PASS for raw representation and collection.

- `RepositoryReadme` carries normalized path and decoded full content.
- The collector distinguishes a successful GitHub 404 absence from unavailable, rate-limited, forbidden, failed, or not-requested evidence through the `readme` availability key.
- Markdown links, image references, and fenced code blocks remain observable in the raw README content and do not require pre-parsed provider fields.

### Repository homepage

PASS for raw representation and collection.

- `RepositoryMetadata.Homepage` carries the normalized optional homepage string.
- The `metadata` availability key establishes whether null homepage means inspected absence or unavailable metadata.
- No separate homepage availability key is required while homepage is part of the atomic metadata response.

### Markdown links, images, and code blocks

PASS for syntax availability in README; semantic classification remains blocked.

- Raw README content is sufficient to parse Markdown links, image references, and fenced code blocks deterministically once parsing rules are locked.
- The snapshot does not need provider-specific link/image/code-block DTOs.
- External target reachability is not collected, and Contract Lock v1 states that demo/homepage reachability is not mandatory.

### Documentation corpus scope

BLOCKED canonical scope contradiction.

- `mvp-signal-definitions.md` lists README, homepage, Markdown links, image references, code blocks, and availability as RequiredData; it does not name documentation files.
- `analysis-categories-and-signal-boundaries.md` says evidence may exist in README **or documentation**.
- The snapshot happens to carry bounded `DocumentationMarkdown` with `documentation-markdown` availability, but that collection was canonically defined for Installation Guide and does not by itself decide whether Usage/Demo must inspect it.
- Until the Usage/Demo corpus is locked, Missing cannot prove that all required content was inspected.

### Availability conclusion

- `readme`, `metadata`, and `documentation-markdown` statuses can distinguish available evidence from collection failure.
- Missing versus UnavailableEvidence is executable only after the required corpus and partial-positive precedence are defined.
- If `brittle link` is intended to mean unreachable/broken, the current snapshot has no URL reachability observation; if it is structural, the structural rule is not defined.

## Semantic Contract Audit

### Usage example

BLOCKED.

- The contract does not say whether every fenced block counts or how a usage code block differs from installation, configuration, build, or unrelated source code.
- A usage command has no locked command family, heading/section context, imperative rule, or required target/argument rule.
- Sample request/response, code example, and example output have no supported syntax, pairing, marker, or section-boundary rules.
- “Açık” and “bağlama uygun” require semantic judgment unless observable deterministic criteria are added.

### Demo, visual, and video

BLOCKED.

- A screenshot is not distinguished from a logo, badge, architecture diagram, icon, decorative image, or external tracking image.
- Screenshot detection has no allowed Markdown syntax, extension, alt-text, path, or surrounding-section rule.
- Video links have no host, URL, file-extension, label, or embed-pattern contract.
- A homepage is not defined as always being a live demo; the separate homepage signal also permits documentation and product pages.
- A generic documentation link is not defined as demo evidence for this signal.

### Partial and brittle links

BLOCKED.

- “Belirsiz ekran görüntüsü” has no deterministic ambiguity boundary.
- “Kırılgan bağlantı” has no structural definition.
- Contract Lock v1 says reachability is not mandatory, so Partial cannot silently be based on live HTTP verification.

### Strong

BLOCKED.

- “Usage example + demo or visual” does not define evidence distinctness, source locality, section coherence, aggregation across README/docs/homepage, or duplicate link/image handling.
- The same Markdown can reasonably produce Strong, Present, Partial, or Missing under different plausible implementations.

### RequiresHumanReview

BLOCKED.

- The general status exists, but this signal has no bounded observable triggers or precedence relative to Partial and UnavailableEvidence.
- It cannot be used as a generic uncertainty bucket without weakening deterministic scoring.

## Minimum Canonical Correction

1. Lock the exact Usage/Demo corpus: README only or README plus a precisely bounded documentation set; define availability/completeness and when homepage/README/docs may establish a terminal positive result.
2. Define executable Markdown rules for usage commands, code examples, request/response pairs, and example output, including section boundaries and false positives such as installation/build/configuration snippets.
3. Define deterministic screenshot, visual, video, demo, homepage, generic documentation-link, and brittle-link recognition without mandatory network validation, or explicitly add a normalized reachability contract.
4. Define source-local/distinct evidence aggregation and exact Strong, Present, Partial, Missing, UnavailableEvidence, and bounded RequiresHumanReview precedence.

## Changes

- Updated worklog only to record the audit and blocker.
- Production code changed: NONE.
- Tests changed: NONE.
- Canonical docs changed: NONE.
- Dependencies changed: NONE.

## Validation

~~~text
Repository baseline: PASS — main at 30640fc, clean before Session 027
Raw data representation audit: PASS — README, homepage, Markdown syntax, and availability are carried
Canonical corpus consistency audit: BLOCKED — README-only versus README-or-documentation contradiction
Semantic determinism audit: BLOCKED — executable classification rules are absent
Production tests/build/format: NOT RUN — audit-only session with no production/test changes
git diff --check: PASS
~~~

## Architecture and Dependency Review

~~~text
Production evaluator: NOT CREATED
Markdown parser/helper: NOT CREATED
Snapshot/collector change: NONE
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
~~~

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 30640fc
Pre-existing unrelated changes: NONE
Final working tree: Session 027 worklog changes only
~~~

## Git

~~~text
Suggested commit: docs(worklog): record usage demo contract gaps
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

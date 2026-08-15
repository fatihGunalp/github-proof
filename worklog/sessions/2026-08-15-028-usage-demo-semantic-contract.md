# Usage/Demo Semantic Contract

**Date:** 2026-08-15
**Result:** PASS
**Scope:** canonical product/domain/architecture documentation only
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Resolve the Session 027 corpus and semantic gaps by making `documentation.usage-or-demo` Version 1 deterministic and executable, without changing production/test code, scores, or signal Version.

## Starting State

~~~text
Branch: main
Base HEAD: 7641cc6
Pre-existing Working Tree: CLEAN
Production evaluators: 4 / 15
Relevant Previous Session: 027 (BLOCKED)
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
worklog/sessions/2026-08-15-027-usage-demo-contract-audit.md
docs/architecture/mvp-signal-definitions.md — Usage/Demo and Contract Lock v1
docs/architecture/analysis-domain-model.md — README, DocumentationMarkdown, Metadata.Homepage, and availability/completeness
docs/product/analysis-categories-and-signal-boundaries.md — Usage/Demo evidence boundary
docs/product/scoring-and-report-contract.md — status, evidence, recommendation, and unavailable semantics
Session 028 task contract
~~~

## Plan

1. Audit the proposed corpus, evidence, scoring, and precedence rules against canonical invariants.
2. If no contradiction exists, lock the smallest executable Usage/Demo Version 1 contract in canonical docs.
3. Confirm the existing normalized snapshot remains sufficient and that no production/test change is needed.
4. Run the docs-only consistency matrix and Git validation, then close the session.

## Constraints

- No production or test code changes.
- No score, signal Version, category, or overall-score inclusion changes.
- No snapshot, collector, evaluator, dependency, or generic Markdown framework changes.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: COMPLETE — contradiction audit, canonical contract lock, consistency review, and Git validation passed.
Next: Session 029 — Usage/Demo Signal Evaluator.
~~~

## Contradiction Audit

- The proposed rules preserve 15 signals, 4 categories, integer scores, nullable unscored statuses, and `RawApplicableScoreNormalization`.
- Strong 7, Present 5, Partial 4, Missing 0, UnavailableEvidence null, and RequiresHumanReview null remain unchanged.
- `IncludeInOverallScore: true` and `Version: 1` remain unchanged.
- Missing still requires complete inspected evidence; incomplete evidence is not converted into a zero score.
- Strong is observable terminal maximum evidence, so it may survive unavailable unrelated corpus parts without creating false absence.
- Source-local Strong prevents cross-file Frankenstein evidence and remains within the README-or-documentation product boundary.
- Homepage is limited to corroboration and does not overlap by itself with the separate homepage/live-demo signal.
- No mandatory remote link-health check is introduced, matching Contract Lock v1.
- The normalized snapshot already carries every required raw input and availability/completeness state.
- No canonical contradiction was found.

## Decisions Locked

- Corpus is normalized README plus bounded canonical `docs/**/*.md`; homepage is conditional live-demo corroboration only.
- README is primary; docs are fallback/secondary; Markdown evidence remains source-local.
- Exact Turkish/English ATX and Setext usage/example and demo/visual headings are locked with same-or-higher section boundaries.
- Clear usage is limited to meaningful fenced examples, non-installation-only ordered steps, exact command families, sample request/response, and sample output labels.
- Arbitrary code, dependency/version/config/install-only snippets, badges, generic prose, images, and links do not become clear usage.
- Screenshot evidence uses exact section context or locked positive markers, with exclusion markers overriding high confidence.
- Conflicting positive/exclusion image markers are the bounded ambiguous screenshot candidate; generic images are not Partial.
- Video host/extension and section/label rules are explicit; generic video-host links are not evidence.
- Homepage is never a standalone Usage/Demo signal; normalized URL equality corroborates only an explicit demo-intent link.
- Brittle link means structurally uncorroborated demo intent, not a broken-link claim; no HTTP/DNS/404 validation occurs.
- Strong requires distinct usage + high-confidence visual/demo in one source and is terminal.
- Present, Partial, and Missing require complete Markdown corpus; incomplete evidence that can change the score produces UnavailableEvidence.
- RequiresHumanReview is limited to observable malformed Markdown that blocks exact classification.
- Evidence, recommendation, limitation, status precedence, score, and Version rules are explicit.

## Data Contract Result

~~~text
README: SUFFICIENT
DocumentationMarkdown: SUFFICIENT
RepositoryMetadata.Homepage: SUFFICIENT
readme / documentation-markdown / metadata availability: SUFFICIENT
Separate parsed link/image/code-block fields: NOT REQUIRED
Remote link-health field: NOT REQUIRED
NO DATA CONTRACT CHANGE REQUIRED
~~~

## Contract Review Matrix

~~~text
Corpus defined: YES
Usage headings: YES
Clear usage example: YES
False-positive boundary: YES
Screenshot/visual: YES
Video: YES
Homepage/live demo: YES
Brittle link: YES
Strong: YES
Present: YES
Partial: YES
Missing: YES
UnavailableEvidence: YES
RequiresHumanReview: YES
Status precedence: YES
Evidence: YES
Limitation: YES
Snapshot sufficiency: YES

Result: PASS — 18/18 YES
~~~

## Changes

- Expanded only the canonical Usage/Demo signal into an executable MVP Version 1 contract.
- Added a concise provider-independent Usage/Demo corpus note to the domain model document.
- Updated worklog for Session 028.
- Production code, tests, snapshot types, collectors, scores, and dependencies were not changed.

## Validation

~~~text
Repository baseline: PASS — main at 7641cc6, clean before Session 028
Canonical contradiction review: PASS
Contract review matrix: PASS — 18/18 YES
Snapshot sufficiency: PASS — no data contract change required
Canonical consistency review: PASS
Production/test scope review: PASS — no src/** or tests/** changes
dotnet test/build/format: NOT RUN — docs-only session
git diff --check: PASS
~~~

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 7641cc6
Pre-existing unrelated changes: NONE
Final working tree: Session 028 canonical docs and worklog changes only
~~~

## Next

Session 029 — implement only the `documentation.usage-or-demo` production evaluator after its mandatory model gate.

## Git

~~~text
Suggested commit: docs(domain): define usage demo evaluation contract
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~

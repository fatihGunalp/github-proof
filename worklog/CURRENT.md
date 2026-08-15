# GitHub Proof — Current Development State

**Last Updated:** 2026-08-16
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

~~~text
Product Stage: MVP Implementation
Delivery Phase: Phase 4 — Report Generation COMPLETE
Next Delivery Phase: Phase 5 — Minimal Web UI
Focus: Session 046 completed deterministic Markdown rendering and Phase 4 exit validation.
~~~

---

## Active Session

~~~text
NONE
~~~

## Model Gate State

~~~text
NONE
~~~

## Current Task

~~~text
NONE
~~~

## In Progress

~~~text
NONE
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Start Phase 5 / Session 047 — Minimal Web UI foundation using RepositoryReport as the canonical presentation input.
~~~

## Working Set

~~~text
Phase 3 and Phase 4 are complete.
Sessions 042–046 established evidence-first report architecture, language-neutral semantic truth, deterministic RepositoryAnalysis composition, localized RepositoryReport projection, and deterministic Markdown rendering.
Session 039 and validation-011 remain exploratory, blocked/non-gating, and outside the current MVP delivery path.
~~~

## Repository State

~~~text
Branch: main
Session 046 validated baseline: edeff9b
Live HEAD / commit / push state is intentionally not duplicated here; use Git directly for live repository state.
Session 046 working tree passed final validation before user-owned commit/push.
~~~

## Last Completed Work

~~~text
Session 046 — Deterministic Markdown Report + Phase 4 Exit: PASS

- Added RepositoryReportMarkdownRenderer.
- Renderer accepts only RepositoryReport.
- Preserved exact evidence-first section order.
- Preserved max-three strength/gap/action ordering.
- Preserved Missing vs explicit uncertainty semantics.
- Kept score secondary.
- Preserved repository/evidence literals.
- Added platform-independent deterministic LF output.
- Kept versioned product-boundary disclaimer mandatory.
- Closed Phase 4 and handed typed RepositoryReport to Phase 5 Web UI.
~~~

## Last Validation

~~~text
Markdown renderer targeted tests: PASS — 10/10
Domain tests: PASS — 422/422
Application tests: PASS — 49/49
Infrastructure tests: PASS — 101/101
Total: PASS — 572/572
Forced rebuild: PASS — 0 warnings, 0 errors
dotnet format --verify-no-changes: PASS
git diff --check: PASS
~~~

## Critical Invariants

~~~text
15 MVP signals; 4 categories.
Category-internal maximum: 91.
Overall-included maximum: 86.
Overall method: RawApplicableScoreNormalization.

Missing => Score 0 and denominator included.

NotApplicable / UnavailableEvidence / RequiresHumanReview
=> Score null and excluded from numerator and denominator.

portfolio.presentation-completeness is category-included and overall-excluded.

RepositoryAnalysis is locale-independent analysis truth.
RepositoryReport is localized presentation projection.
RepositoryReportMarkdownRenderer renders presentation truth; it does not recreate analysis truth.

Locale switching must not rerun collection/evaluation/composition/scoring/ranking.
Markdown renderer must not localize or parse analysis.
Web UI must consume RepositoryReport directly rather than parse Markdown.
~~~

## Do Not Reinvestigate

~~~text
MVP signal count, category count, nullable score semantics, overall method,
presentation-completeness exclusion, evidence-first report order,
semantic/presentation localization boundary, and Phase 4 Markdown boundary
are contract-locked in docs/.
~~~

## Known Problems

~~~text
- README.rst and README.adoc documentation detection gaps.
- Cargo dependency installation instruction detection.
- Explicit README demo-link detection.
- CI wrapper commands such as tox and dotnet-coverage.
- Reusable workflow resolution.
- Derived presentation completeness inheriting upstream misses.
- Broken-link/reachability semantics remain outside MVP v1.
- Non-GitHub-Actions CI is a future consideration.
- Historical-ref snapshots intentionally keep metadata, languages, commits, issues, pull requests, releases, tags, and contributors live/non-atomic and disclose that boundary in warnings.
~~~

## Git Rule

~~~text
Development tooling may edit, test, review diff/status, and update worklog.
Commit and push remain user-owned unless explicitly requested.
~~~
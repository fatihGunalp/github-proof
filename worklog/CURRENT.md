# GitHub Proof — Current Development State

**Last Updated:** 2026-08-16
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

~~~text
Product Stage: MVP Implementation
Delivery Phase: Phase 5 — Minimal Web UI IN_PROGRESS
Next Delivery Phase: Phase 6 — MVP Validation
Focus: Session 047 — Web Foundation + Semantic UI System.
~~~

---

## Active Session

~~~text
Session 047 — Web Foundation + Semantic UI System
worklog/sessions/2026-08-16-047-web-foundation-semantic-ui-system.md
Result: IN_PROGRESS
Base HEAD: 89e7be3
Execution: PowerShell / manual
~~~

## Model Gate State

~~~text
Manual PowerShell workflow selected by the user after routing discussion.
~~~

## Current Task

~~~text
Establish the Phase 5 Web foundation and semantic Light/Dark UI system while preserving RepositoryReport as the canonical presentation input.
~~~

## In Progress

~~~text
Session 047 is active.

Completed checkpoint:
- Minimal GitHubProof.Web Razor Pages host.
- GitHubProof.Web.Tests project.
- Semantic Light / Dark / System token architecture.
- Generic Button and Input primitives.
- CSS semantic-token architecture tests.
- Canonical repository-URL-only RepositoryInput.
- Minimal evidence-first Home composition.
- Foundation checkpoint pushed at 227c84f / fb1f388.

Step 5.1 completed:
- Hero vertical position and visual weight refined.
- Content width and reading rhythm refined.
- Typography hierarchy strengthened.
- RepositoryInput promoted as the primary interaction surface.
- Disabled Analyze state remains honest but visually legible.
- Supporting metadata hierarchy refined.
- Mobile spacing and stacking refined.
- Explicit public GitHub repository boundary preserved.
- Dark browser review: PASS.
- Light browser review: PASS.
- Light/Dark hierarchy equivalence: PASS.

Live repository analysis remains intentionally unwired.
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Session 047 / next step — implement StatusBadge and the seven canonical evidence-state semantics:

Strong
Present
Partial
Missing
NotApplicable
UnavailableEvidence
RequiresHumanReview

Preserve:
- text/icon + color semantics,
- Missing != UnavailableEvidence,
- theme equivalence,
- semantic-token-only styling.

Do not wire live repository analysis yet.
~~~

## Working Set

~~~text
Phase 3 and Phase 4 are complete.
Phase 5 / Session 047 is active.
Sessions 042–046 established evidence-first report architecture, language-neutral semantic truth, deterministic RepositoryAnalysis composition, localized RepositoryReport projection, and deterministic Markdown rendering.
The Phase 5 UI foundation is guided by github-proof-design-principles-v1.md and github-proof-ui-system-light-dark-v1.md without overriding canonical product/report contracts.
Session 039 and validation-011 remain exploratory, blocked/non-gating, and outside the current MVP delivery path.
~~~

## Repository State

~~~text
Branch: main
Session 047 base HEAD: 89e7be3
Pre-existing working tree at session start: CLEAN
Live HEAD / commit / push state after session start should be read from Git directly.
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
Session 047 checkpoint validation:

GitHubProof.Web.Tests: PASS — 7/7
Full solution build: PASS
Full solution tests: PASS
dotnet format --verify-no-changes: PASS
CSS semantic-token boundary: PASS
Canonical repository URL scope guard: PASS
Username discovery not introduced: PASS
Browser smoke check: PASS
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
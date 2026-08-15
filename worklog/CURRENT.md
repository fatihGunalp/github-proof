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

Completed:
- Minimal GitHubProof.Web Razor Pages host.
- Semantic Light / Dark / System token architecture.
- Generic Button and Input primitives.
- Canonical repository-URL-only Home.
- Home visual refinement accepted in Dark and Light.
- StatusBadge typed presentation mapping implemented.
- StatusBadge consumes SignalPresentation.Status and localized StatusLabel.
- All seven canonical SignalStatus values have explicit non-color visual cues.
- Missing and UnavailableEvidence remain visually and semantically distinct.
- StatusBadge consumes semantic status tokens only.
- CSS primitive-token architecture guard now covers all consumer stylesheets.

Live repository analysis remains intentionally unwired.
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Build the first typed RepositoryReport report shell.

The Web UI must consume RepositoryReport directly and preserve:
identity -> proves -> weakens -> do first -> uncertainty -> categories/secondary score -> signal details -> disclaimer.

Do not parse Markdown.
Do not recreate analysis truth in Web.
Do not wire live GitHub analysis yet.
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
Session 047 / Step 6:

GitHubProof.Web.Tests: PASS
GitHubProof.Web build: PASS
Seven canonical SignalStatus values: PASS
Typed StatusBadge mapping: PASS
Localized StatusLabel preservation: PASS
Non-color status cues: PASS
Missing != UnavailableEvidence: PASS
CSS semantic-token boundary: PASS
Primitive-token consumer guard: PASS
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
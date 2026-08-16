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
Focus: Session 049 — Evidence-first Product UI + Progressive Disclosure / visual refinement.
~~~

## Active Session

~~~text
worklog/sessions/2026-08-16-049-evidence-first-product-ui.md

Session 049 — Evidence-first Product UI + Progressive Disclosure
Result: IN_PROGRESS
Base HEAD: ae2bb94
~~~

## Model Gate State

~~~text
APPROVED

Session:
049

Model:
Terra

Reasoning:
medium

Execution:
macOS Terminal / manual

Implementation Agent:
NONE
~~~

## Current Task

~~~text
Implement the user-approved modern card-based RepositoryReport composition.

Target:
- compact repository hero,
- four-card decision viewport,
- collapsed secondary inspection layers,
- responsive Light/Dark presentation.

Preserve typed RepositoryReport and all analysis/scoring semantics.
~~~

## In Progress

~~~text
Session 049 remains IN_PROGRESS.

The visual target is now locked to the approved modern report mockup.

Current production checkpoint:
522852d

Validated presentation foundation:
- evidence-first report hierarchy,
- progressive disclosure,
- semantic status icons,
- curated Lucide UI icon foundation,
- compact repository identity,
- compact uncertainty/category/signal/disclaimer layers,
- Light/Dark-compatible styling.

Next implementation begins from component boundaries.
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Session 049 / Step 27:

Split the report presentation into bounded reusable Razor components before
implementing the approved four-card decision viewport.

Do not change:
- analysis semantics,
- scoring,
- localization semantics,
- RepositoryReport truth,
- canonical signal/category contracts.
~~~

## Working Set

~~~text
Active Session:
worklog/sessions/2026-08-16-049-evidence-first-product-ui.md

Locked implementation target:
modern four-card GitHub Proof report mockup

Next primary surfaces:
- src/GitHubProof.Web/Pages/Shared/_RepositoryReport.cshtml
- new bounded report partials under Pages/Shared/
- src/GitHubProof.Web/wwwroot/css/report.css
- tests/GitHubProof.Web.Tests/

Current checkpoint production changes are committed.
~~~

## Repository State

~~~text
Branch: main
Session 049 base HEAD: ae2bb94
Pre-existing working tree at session start: CLEAN

Latest production checkpoint:
522852d

Checkpoint:
feat(web): refine evidence-first report experience

Session status:
IN_PROGRESS

Execution:
macOS Terminal / manual

Checkpoint push and final origin/main synchronization are verified by the
Step 26 terminal command after the worklog commit.
~~~

## Last Completed Work

~~~text
Session 048 — Live Repository Analysis Vertical Slice: PASS

Delivered:
- real repository URL → GitHub → analysis → RepositoryReport vertical slice,
- canonical production analyzer factory,
- RepositoryAnalysisRunner,
- canonical GitHub snapshot composition seam,
- testable live report orchestration,
- real Home POST,
- typed report rendering,
- bounded invalid/unavailable repository feedback,
- one compact Light/Dark theme button.

Browser acceptance:
PASS

Validated with:
https://github.com/fatihGunalp/claude-code-dotnet-sdk-skill

Invalid URL behavior:
PASS
~~~

## Last Validation

~~~text
Session 048 final validation:

Full solution build:
PASS

Full solution tests:
PASS

dotnet format --verify-no-changes:
PASS

git diff --check:
PASS

Canonical repository URL boundary:
PASS

Canonical 15-signal analysis:
PASS

Typed RepositoryReport:
PASS

Markdown in Web flow:
NONE

Live browser analysis:
PASS

Malformed URL browser feedback:
PASS

Single Light/Dark theme button:
PASS

Theme persistence:
PASS

Phase 5:
IN_PROGRESS
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

Standing workflow:
- do not push intermediate session work,
- push when the active session is finalized,
- session-final push occurs only after final validation and bounded diff review.
~~~

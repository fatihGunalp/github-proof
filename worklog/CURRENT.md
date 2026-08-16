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
Focus: No active session. Next: Session 049 — Evidence-first Product UI + Progressive Disclosure / visual refinement.
~~~

## Active Session

~~~text
NONE

Last completed:
Session 048 — Live Repository Analysis Vertical Slice — PASS
~~~

## Model Gate State

~~~text
No active model gate.

Last completed session:
Session 048

Model:
Sol

Reasoning:
medium

Execution:
PowerShell / manual
~~~

## Current Task

~~~text
No active implementation session.

Session 048 completed the first real production vertical slice:

Repository URL
→ live GitHub collection
→ canonical analysis
→ RepositoryAnalysis
→ localized RepositoryReport
→ typed Web report.

Next planned work:
Session 049 — Evidence-first Product UI + Progressive Disclosure / visual refinement.
~~~

## In Progress

~~~text
No active session.

Session 048 completed:
- canonical production analyzer composition,
- RepositorySnapshot → RepositoryAnalysis execution boundary,
- canonical Infrastructure snapshot composition,
- live Web orchestration,
- canonical repository URL validation reuse,
- real Home POST analysis,
- typed RepositoryReport rendering,
- bounded repository failure states,
- real browser analysis validation,
- malformed URL browser validation,
- compact Light/Dark theme interaction,
- theme persistence.

Phase 5 remains IN_PROGRESS.
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Initialize Session 049 separately.

Planned focus:
Evidence-first Product UI + Progressive Disclosure / visual refinement.

Preserve the live architecture completed in Session 048.

Do not recreate:
- repository collection,
- signal analysis,
- RepositoryAnalysis composition,
- RepositoryReport projection.

Session 049 should refine hierarchy, interaction, disclosure, and product polish around the now-working live vertical slice.
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

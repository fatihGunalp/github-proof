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
Focus: No active session. Next: Session 048 — Live Repository Analysis Vertical Slice.
~~~

## Active Session

~~~text
NONE

Last completed:
Session 047 — Web Foundation + Semantic UI System — PASS
~~~

## Model Gate State

~~~text
Manual PowerShell workflow selected by the user after routing discussion.
~~~

## Current Task

~~~text
No active implementation session.

Session 047 completed the Phase 5 Web foundation and typed RepositoryReport presentation surface.

Next:
Session 048 — Live Repository Analysis Vertical Slice.

The next vertical slice should connect the canonical repository URL input to the existing analysis pipeline and typed RepositoryReport presentation.

Do not parse Markdown.
Do not redefine analysis or scoring truth.
~~~

## In Progress

~~~text
No active session.

Session 047 completed:

- Minimal ASP.NET Core Razor Pages Web host.
- Semantic Light / Dark / System token architecture.
- Canonical repository-URL-only Home.
- Minimal Home visual hierarchy.
- Canonical seven-state StatusBadge semantics.
- Typed RepositoryReport report shell.
- Exact canonical evidence-first section ordering.
- Maximum-three first-reading strengths, gaps, and actions.
- Uncertainty before secondary score.
- Progressive SignalDetails disclosure using native details/summary.
- Visible disclosure affordance without replacing native semantics.
- Focus-visible disclosure treatment.
- Quieter evidence metadata hierarchy.
- Stronger decision / uncertainty section rhythm without card containers.
- Development-only typed RepositoryReport preview.
- English and Turkish preview fixtures.
- Dark and Light desktop browser review.
- Dark and Light narrow/mobile browser review.
- Home remained unchanged throughout report work.

Live repository analysis remains intentionally unwired.
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Initialize Session 048 — Live Repository Analysis Vertical Slice.

Before implementation:
1. run the model gate,
2. load the minimum required context,
3. initialize Session 048 worklog state,
4. define the vertical-slice boundary,
5. preserve repository URL as the canonical MVP input,
6. preserve RepositoryReport as the Web presentation input.

Expected direction:

Repository URL
→ existing repository collection / analysis pipeline
→ RepositoryAnalysis
→ localized RepositoryReport
→ existing Web report presentation

Do not parse Markdown.
Do not introduce username discovery.
Do not invent fake analysis progress.
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
Session 047 — Web Foundation + Semantic UI System: PASS

Delivered:
- Razor Pages Web foundation.
- Light / Dark / System semantic design-token architecture.
- Repository-URL-only Home.
- Canonical seven-state StatusBadge.
- Typed RepositoryReport presentation.
- Evidence-first report hierarchy.
- Progressive SignalDetails.
- Development-only typed report preview.
- Responsive presentation foundation.
- Native disclosure and focus-visible accessibility foundation.
- Dark / Light desktop and narrow/mobile browser review.
- Presentation hardening without card soup, gauges, Markdown parsing, or product-scope expansion.

Session 047 intentionally did not wire live GitHub analysis.
~~~

## Last Validation

~~~text
Session 047 final validation:

GitHubProof.Web.Tests:
PASS — 32/32

GitHubProof.Web build:
PASS

Full solution build:
PASS

Full solution tests:
PASS

dotnet format --verify-no-changes:
PASS

CSS semantic-token boundary:
PASS

Home:
UNCHANGED

RepositoryReport direct typed input:
PASS

Canonical evidence-first section order:
PASS

Seven SignalStatus semantics:
PASS

Uncertainty before secondary score:
PASS

SignalDetails native details/summary:
PASS

Disclosure affordance:
PASS

Focus-visible foundation:
PASS

Markdown parsing in Web:
NONE

Browser visual review:
- Dark desktop: PASS
- Light desktop: PASS
- Dark narrow/mobile: PASS
- Light narrow/mobile: PASS

Live repository analysis:
NOT WIRED — intentionally deferred to Session 048.
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

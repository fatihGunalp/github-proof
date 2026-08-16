# GitHub Proof — Current Development State

**Last Updated:** 2026-08-16
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

~~~text
Product Stage: MVP Implementation / Product Scope Expansion
Repository Analysis Capability: preserved
Phase 5 Web UI: verified checkpoint preserved
Next Workstream: Public Profile Evidence — Product Contract

Focus:
Define the profile-level public evidence product boundary before additional
repository-report visual refinement or profile-level implementation.
~~~

## Active Session

~~~text
NONE
~~~

Session 049 was finalized as `PARTIAL` at its verified repository-report UI
checkpoint.

## Model Gate State

~~~text
NONE

Next independent task requires a new model-routing decision before execution.

Execution mode:
macOS Terminal / manual

Implementation Agent:
NONE
~~~

## Current Task

~~~text
NONE — Session 049 is finalized.

Next planned independent task:

Public Profile Evidence — Product Contract

Do not resume Overall Score Card refinement before the profile-level product
boundary and information architecture implications are defined.
~~~

## In Progress

~~~text
None.

Preserved repository-report production checkpoint:
40cab55

Repository hero:
PASS / LOCKED
~~~

## Current Problem

~~~text
None confirmed.
~~~

## Next Immediate Action

~~~text
Start a new independent product/domain session for:

Public Profile Evidence — Product Contract

The session must define, before production implementation:

- what a Developer Public Evidence Profile represents,
- what it explicitly does not represent,
- how public repositories contribute evidence,
- how repository-level evidence is reused,
- what cross-repository patterns may be inferred,
- traceability from profile claim to repository evidence,
- prohibited seniority/employability/capability conclusions,
- recruiter-facing evidence boundaries.

Do not modify production code before this contract is locked.
~~~

## Working Set

~~~text
NONE — no active development session.

Preserved production checkpoint:
40cab55 — feat(web): implement modern report composition

Preserved visual decision:
Repository hero — PASS / LOCKED

Next task has not started.
~~~

## Repository State

~~~text
Branch:
main

Last verified pre-closure HEAD:
7e3e5d1

Session 049 base HEAD:
ae2bb94

Session 049 production checkpoint:
40cab55

Session 049 final result:
PARTIAL

Pre-existing working tree at Session 049 start:
CLEAN

Closure reason:
Product direction expanded from single-repository presentation refinement to a
new profile-level Public Profile Evidence product/domain capability.

Production code changed during closure:
NO

Execution:
macOS Terminal / manual

Implementation Agent:
NONE
~~~

## Last Completed Work

~~~text
Session 049 — Evidence-first Product UI + Progressive Disclosure

Final result:
PARTIAL

Verified production checkpoint:
40cab55 — feat(web): implement modern report composition

Delivered and preserved:
- bounded RepositoryReport Razor partial composition,
- four-card first-reading foundation,
- score / strength / weakness / action card foundations,
- progressive secondary inspection layers,
- Light/Dark-compatible presentation,
- repository hero visual acceptance.

Repository hero:
PASS / LOCKED

Remaining Session 049 visual refinement was intentionally deferred because the
product direction expanded to profile-level Public Profile Evidence analysis.
~~~

## Last Validation

~~~text
Session 049 repository hero checkpoint:

GitHubProof.Application.Tests:
54 / 54 PASS

GitHubProof.Web.Tests:
68 / 68 PASS

git diff --check:
PASS

Browser visual acceptance:
Repository hero PASS / LOCKED

Analysis/scoring contract:
UNCHANGED
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

# GitHub Proof — Current Development State

**Last Updated:** 2026-08-16
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

Product Stage:
Stage 6 — Profile P2

Public Profile Evidence V1:
P0 — CONTRACT LOCKED / REVISION 2
P1 — CONTRACT LOCKED
P2 — IN_PROGRESS

Production code:
NOT IN SCOPE

## Active Session

worklog/sessions/2026-08-16-057-repository-eligibility-contract.md

Session 057 — Repository Eligibility Contract
Result: IN_PROGRESS
Base HEAD: 4c93400

## Model Gate State

~~~text
BYPASSED BY EXPLICIT USER INSTRUCTION

The user explicitly requested that model-routing gates not be used for this
workstream.

Execution:
ChatGPT-guided / macOS Terminal / manual

Implementation Agent:
NONE
~~~

## Current Task

Define and contract-lock repository eligibility semantics for
Public Profile Evidence V1.

Goal:
determine whether and how each public repository may contribute to
profile-level evidence without introducing repository/developer quality
judgments.

## In Progress

Session-final validation / commit / push / independent remote verification.

No further Stage 5 semantic changes are planned unless final validation finds
a concrete defect.

## Current Problem

None currently blocking.

Stage 5 remote checkpoint:
VERIFIED

Stage 6 contract work may proceed.

## Next Immediate Action

Session 057 / Step 6.1

Extract and classify:

- eligibility dimensions
- eligibility statuses
- eligibility reasons
- contribution semantics
- independence semantics

Do not write production code.

## Working Set

Stage 5 final checkpoint:

- docs/architecture/public-profile-evidence-domain-model.md
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-16-056-resume-public-profile-domain-contract.md

Production code/tests:
UNCHANGED

## Repository State

Branch:
main

Stage 6 base HEAD:
4c93400

Previous completed stage:
Stage 5 — Profile P1 Domain Model Contract

Stage 5 commit:
4c934006556bfa9a4caf7a802f9d5270cea512b6

Remote verification:
PASS

Public Profile Evidence V1:
P0 — CONTRACT LOCKED / REVISION 2
P1 — CONTRACT LOCKED
P2 — IN_PROGRESS

Session 057:
IN_PROGRESS

Production code/tests:
UNCHANGED

## Last Completed Work

Stage 4 — Canonical Roadmap V2 Alignment

Result:
PASS / REMOTE VERIFIED

Checkpoint:
17db53d

Bounded dependency clarification:
993bfcc

Remote verification:
PASS

Canonical execution order and Dependency Gates are aligned.

## Last Validation

origin/main:
993bfccd1b630ffa26958aafffb85e20474b4dcb

Independent remote verification:
PASS

Verified:

- Stage 4 canonical roadmap complete
- bounded dependency clarification present
- J1/J2 independence preserved
- Dependency Gates authoritative
- Stage 5 canonical next stage preserved
- product contracts unchanged
- production code/tests unchanged

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

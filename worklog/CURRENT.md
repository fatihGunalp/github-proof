# GitHub Proof — Current Development State

**Last Updated:** 2026-08-16
**Source of Truth:** `docs/`
**Operational Instructions:** `AGENTS.md` + `agents/`

> This file is an operational snapshot, not a source of product or domain truth.

---

## Current Phase

Product roadmap:
Stage 6 — Profile P2 — COMPLETE / REMOTE VERIFIED

Next product stage:
Stage 7 — Public Profile Evidence Foundation P3–P9 — NOT STARTED

Active work:
Session 058 — Claude Code Orchestration Foundation

Public Profile Evidence V1:
P0 — CONTRACT LOCKED / REVISION 2
P1 — CONTRACT LOCKED
P2 — CONTRACT LOCKED

Production code/tests:
UNCHANGED

## Active Session

worklog/sessions/2026-08-16-058-claude-code-orchestration-foundation.md

Session 058 — Claude Code Orchestration Foundation
Result: DOCS
Base HEAD: d059d0f229b6ded90efcbd1dac2002b11b0543e5

## Model Gate State

~~~text
BYPASSED BY EXPLICIT USER INSTRUCTION

Execution:
ChatGPT-guided / macOS Terminal / manual

Implementation Agent:
NONE
~~~

## Collaboration Workflow

ChatGPT → Claude collaboration:

ChatGPT analysis / decision
→ worklog/session handoff
→ Claude implementation
→ Claude forked-context independent review
→ worklog/session result
→ next ChatGPT analysis.

This is operational workflow only; canonical product/domain truth remains in
`docs/`.

## Current Task

Finalize the Session 058 Claude Code orchestration checkpoint.

Claude foundation:
COMPLETE

Artifacts:

- CLAUDE.md
- .claude/skills/github-proof-dev/SKILL.md
- .claude/skills/github-proof-review/SKILL.md
- .claude/skills/github-proof-finalize/SKILL.md

Product/domain contracts:
UNCHANGED

Production code/tests:
UNCHANGED

Stage 7:
NOT STARTED

## In Progress

Session-final validation / commit / push / independent remote verification.

No further Claude orchestration semantic work is planned unless final validation
finds a concrete defect.

## Current Problem

None currently blocking.

Stage 6:
COMPLETE / REMOTE VERIFIED

Checkpoint:
d059d0f229b6ded90efcbd1dac2002b11b0543e5

## Next Immediate Action

Validate the exact Session 058 final scope.

If PASS:
stage → cached validation → commit → push → independent GitHub remote verification.

Do not start Stage 7 before Session 058 is remote verified.

## Working Set

Expected Session 058 scope begins with:

- CLAUDE.md
- .claude/skills/github-proof-dev/SKILL.md
- .claude/skills/github-proof-review/SKILL.md
- .claude/skills/github-proof-finalize/SKILL.md
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-16-058-claude-code-orchestration-foundation.md

Product/domain docs:
UNCHANGED

Production code/tests:
UNCHANGED

## Repository State

Branch:
main

Session 058 base HEAD:
d059d0f229b6ded90efcbd1dac2002b11b0543e5

origin/main at session start:
d059d0f229b6ded90efcbd1dac2002b11b0543e5

Previous completed stage:
Stage 6 — Profile P2

Stage 6 remote verification:
PASS

Stage 7:
NOT STARTED

## Last Completed Work

Stage 6 — Repository Eligibility Contract

Result:
DOCS / REMOTE VERIFIED

Checkpoint:
d059d0f229b6ded90efcbd1dac2002b11b0543e5

Canonical contract:
docs/architecture/public-profile-evidence-repository-eligibility.md

## Last Validation

Independent GitHub remote verification:

origin/main:
d059d0f229b6ded90efcbd1dac2002b11b0543e5

Result:
PASS

Verified:

- exact Stage 6 four-file checkpoint
- P2 CONTRACT LOCKED / Version 1
- Session 057 DOCS / semantic COMPLETE
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

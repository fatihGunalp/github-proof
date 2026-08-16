# Session 058 — Claude Code Orchestration Foundation

**Date:** 2026-08-16
**Result:** DOCS
**Scope:** developer tooling / Claude Code project instructions / skills
**Product Stage:** UNCHANGED — Stage 7 NOT STARTED
**Base HEAD:** d059d0f229b6ded90efcbd1dac2002b11b0543e5
**Execution:** ChatGPT-guided / macOS Terminal / manual
**Implementation Agent:** NONE

## Goal

Create a production-grade Claude Code orchestration layer for GitHub Proof
without duplicating or weakening existing canonical product/domain contracts
or reusable repository workflow knowledge.

Target structure:

~~~text
CLAUDE.md

.claude/
└── skills/
    ├── github-proof-dev/
    │   └── SKILL.md
    ├── github-proof-review/
    │   └── SKILL.md
    └── github-proof-finalize/
        └── SKILL.md
~~~

## Architectural Intent

The Claude layer must know how to find and apply GitHub Proof truth.

It must not become a second copy of GitHub Proof truth.

Canonical product/domain/architecture truth remains in:

~~~text
docs/
~~~

Operational state remains in:

~~~text
worklog/CURRENT.md
~~~

Reusable workflow guidance remains in:

~~~text
agents/
~~~

## Required Boundaries

1. Product/domain/architecture contracts remain unchanged.
2. Production code and tests remain unchanged.
3. Stage 7 does not begin during this session.
4. `CLAUDE.md` remains a small bootstrap layer.
5. Existing `agents/` playbooks are referenced, not duplicated.
6. `worklog/CURRENT.md` may be the first operational read but is not product truth.
7. Claude Code must not execute the Codex-specific Luna/Terra/Sol model-routing
   and `devam et` gate from `agents/01-model-routing.md`.
8. This Claude-specific routing exception must not alter Codex behavior.
9. Development, independent review and finalization remain separate concerns.
10. Finalization must remain explicit/manual and bounded by current authorization.
11. No unnecessary scripts or reference-document abstractions are introduced in V1.
12. Existing working-tree and Git safety rules remain preserved.

## Planned Artifacts

### CLAUDE.md

Small repository bootstrap covering:

- authority and source-of-truth boundaries,
- operational-state loading,
- Claude-specific model-routing exception,
- skill routing,
- contract safety,
- Git safety.

### github-proof-dev

Primary development orchestrator:

- read CURRENT first,
- classify task,
- detect scope collision,
- load minimum relevant context,
- use existing workflow playbooks,
- perform bounded implementation,
- perform required validation,
- stop before unauthorized finalization.

### github-proof-review

Independent verification capability:

- inspect bounded changed scope,
- compare against canonical contracts,
- validate contract drift and tests,
- report findings without automatically fixing them.

### github-proof-finalize

Manual side-effect boundary:

- explicit invocation only,
- inspect CURRENT and active session,
- validate expected scope,
- perform final Git checks,
- commit/push only when authorized,
- verify remote state when finalization includes push.

## Non-Goals

This session does not:

- redefine GitHub Proof product semantics,
- change Public Profile Evidence P0/P1/P2,
- implement Profile P3–P9,
- modify source code,
- modify tests,
- add model-specific product behavior,
- create a second project-contract or context-map copy,
- start Stage 7.

## Starting State

~~~text
Branch:
main

Base HEAD:
d059d0f229b6ded90efcbd1dac2002b11b0543e5

Base Remote:
origin/main = d059d0f229b6ded90efcbd1dac2002b11b0543e5

Working Tree:
CLEAN

Previous Product Stage:
Stage 6 — Profile P2

Previous Result:
REMOTE VERIFIED

Next Product Stage:
Stage 7 — NOT STARTED
~~~

## Next

Review the minimum existing repository workflow sources needed to design the
Claude bootstrap and three skills without duplication.


## Final Result

Result:

DOCS

Claude Code orchestration foundation:

COMPLETE

Created:

- CLAUDE.md
- .claude/skills/github-proof-dev/SKILL.md
- .claude/skills/github-proof-review/SKILL.md
- .claude/skills/github-proof-finalize/SKILL.md

Locked orchestration decisions:

- CLAUDE.md is a small bootstrap, not a product/domain truth source.
- docs/ remains canonical product/domain/architecture truth.
- worklog/CURRENT.md is the first operational read for meaningful repository work,
  but remains operational state only.
- Claude Code does not execute the Codex-specific Luna/Terra/Sol + `devam et`
  model-routing gate.
- Claude communicates with the user in Turkish by default.
- github-proof-dev performs bounded development orchestration and required
  self-validation.
- github-proof-review is an independent, non-mutating-by-default review capability.
- github-proof-finalize is manual-only through `disable-model-invocation: true`.
- commit and push remain separate explicit side effects.
- existing agents/ playbooks are reused rather than duplicated.
- no model override is embedded in the skills.
- no extra scripts/reference abstraction was introduced in V1.
- product/domain contracts remain unchanged.
- production code/tests remain unchanged.
- Stage 7 was not started during Session 058.

Validation:

PASS — Claude bootstrap and three skills exist.
PASS — model-routing isolation.
PASS — canonical product/scoring truth not duplicated into skills.
PASS — dev/review/finalize responsibility boundaries.
PASS — finalization manual and authorization-bounded.
PASS — product/domain/code/tests unchanged.
PASS — git diff --check.

Next:

session-final git scope validation
→ stage/cached validation
→ commit
→ push
→ independent GitHub remote verification
→ only then Stage 7.


## Token and Handoff Optimization

The initial Claude foundation was compacted before push.

Collaboration model:

ChatGPT analysis / decision
→ worklog/session handoff
→ Claude implementation
→ forked-context Claude independent review
→ worklog/session result
→ next ChatGPT analysis.

Optimization decisions:

- repeated workflow prose was removed from skills,
- existing agents/ playbooks remain authoritative reusable workflow knowledge,
- review uses `context: fork` for isolated second-pass verification,
- github-proof-dev consumes ChatGPT worklog handoff instead of restarting
  analysis when canonical contracts already support it,
- Claude leaves concise result/validation/review/next-action state for the next
  ChatGPT session,
- finalization remains manual-only,
- no scripts or duplicate reference docs were added.

The optimization changes orchestration only.
Product/domain contracts and production code/tests remain unchanged.


## Claude Native Model / Effort Routing

Before push, Session 058 also added a Claude-native cost/capability gate.

Routing:

- MECHANICAL → sonnet / low
- BOUNDED → sonnet / medium
- INVARIANT_HEAVY → sonnet / high
- COMPLEX → opus / high
- HIGH_RISK → opus / xhigh

Rules:

- use Claude Code model-family aliases rather than hard-coded model versions,
- do not copy the Codex Luna/Terra/Sol gate,
- route before broad context loading or implementation,
- after recommendation, stop until the user selects model/effort and writes
  `devam et`,
- do not re-run ChatGPT analysis when the worklog handoff already bounds the
  implementation decision,
- do not embed fixed model/effort overrides in skills,
- `max` is outside normal routing and requires exceptional justification.

Purpose:

reduce unnecessary reasoning/token spend while preserving stronger models for
tasks whose implementation risk actually requires them.

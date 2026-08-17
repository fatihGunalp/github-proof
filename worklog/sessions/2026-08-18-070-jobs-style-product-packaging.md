# Session 070 — Jobs-Style Product Packaging

Date: 2026-08-18
Result: PASS
Scope: product experience / packaging audit + bounded experience contract
Model Route: Claude Opus / high
Model Gate: APPROVED

## Goal

Audit the currently delivered GitHub Proof experience against a human-first
product discipline, and determine what the product should communicate before
any visual redesign is attempted.

Guiding principle:

    HUMAN → PROBLEM → EXPERIENCE → TECHNOLOGY

not:

    TECHNOLOGY → FEATURES → PRODUCT → USER

Stage 1 is analysis only. No canonical product documentation, architecture
contract, production code, UI code or test is modified.

## Starting State

Branch:
session/070-jobs-style-product-packaging

Session Base:
635e014c37b7852bd4641413a7216f33523e8fb8

Invariants verified before any mutation:

    branch          = session/070-jobs-style-product-packaging   OK
    HEAD            = 635e014                                    OK
    merge-base main = 635e014                                    OK
    origin/main     = 635e014                                    OK
    working tree    = clean                                      OK

Previous session:
069 — Product Positioning Alignment — COMPLETE / REMOTE VERIFIED at 635e014

## Preserved Product Truth

Session 069 locked the product positioning. This session does not redefine it.

    Repository Evidence
    != Human Capability
    != Human Authorship
    != Human Understanding

- Evidence, not verdicts / Kanıt sunar, hüküm vermez.
- "Proof" means observable and traceable repository evidence only.
- Evidence gap != skill gap.
- The existing repository score remains and is not a developer score.
- Evidence-backed human verification topics remain a FUTURE capability.

Canonical owner: `docs/product/product-positioning.md`.

## Stage 1 Deliverable

SESSION 070 — JOBS-STYLE PRODUCT EXPERIENCE AUDIT, then STOP.

The audit separates the currently implemented experience from the documented
future experience, and inspects the actual Web/report surface rather than
assuming the documentation is implemented.

## Stage 1 Result

Executive verdict:

    STRONG PRODUCT / WEAK PACKAGING

The evidence engine, the traceability chain and the epistemic boundaries are
sound and contract-locked. The delivered experience presents that work in
analyzer vocabulary and gives the score more visual weight than the locked
report contract intends, so the user carries interpretation burden the product
should absorb.

No packaging decision is locked by this session. The audit proposes candidate
positioning, hierarchy and message decisions for a separate Stage 2 approval.

## Step 2 — Future P10 Experience Principles

Step 1 was accepted. Step 2 decides the product experience truth that
**Profile P10 — Web Profile Flow** must implement later. It does not redesign,
repair or plan work on the current UI; the current UI is used only as evidence.

Nothing below is canonically locked. These are decisions proposed for review.

### Human problem

Public GitHub carries useful evidence, but reading it costs too much manual
interpretation and easily produces unsupported conclusions.

GitHub Proof does not remove uncertainty. It reduces interpretation burden,
makes observable evidence understandable, shows what each statement rests on,
and states where public GitHub evidence stops.

### Three core experience questions

    1. WHAT CAN WE SEE?
    2. WHAT IS IT BASED ON?
    3. WHERE DOES THE EVIDENCE STOP?

Semantic questions, not UI copy.

### Information hierarchy

    LEVEL 1 — UNDERSTAND
    identity/context, plain-language evidence summary, strongest observable
    evidence, important public evidence gaps, concise evidence boundary

    LEVEL 2 — TRACE
    evidence references, observable sources, evidence detail, availability and
    uncertainty explanation, Visible Evidence Score, category breakdown

    LEVEL 3 — INSPECT
    the 15 individual signals, exact statuses, analyzer/ruleset/schema
    versions, snapshot commit, methodology, Markdown output

TRACE is deliberately not called VERIFY, to keep it separate from the future
human-verification capability.

### First meaningful result

Decision: **option C** — one plain-language sentence plus up to three evidence
statements.

It must be a projection of already-approved deterministic truth: the existing
deterministic strong-evidence and critical-gap selection under
`docs/product/scoring-and-report-contract.md` §11 and §12. It creates no new
signal, no new score, no new inference, and no skill, authorship or
human-understanding claim.

A bounded presentation contract is required before P10 implements it. The
composition rule must be owned by the presentation catalog, never by Razor or
any other UI code.

### Score rule

Confirmed **SECONDARY**. Scoring semantics are untouched: 15 signals, 4
categories, weights, normalization, status semantics and `RuleSetVersion` all
remain as locked. This is a presentation-priority decision only.

The score belongs in LEVEL 2 and must be preceded by the plain-language
evidence summary, the strongest observable evidence and the important evidence
gaps.

### Shared truth versus contextual emphasis

One canonical evidence truth for every user. No separate score, no separate
analysis truth, no hidden recruiter engine. Only emphasis and disclosure order
may differ between repository owner, experienced developer and
recruiter/reviewer.

Universal LEVEL 1 truth carries no user-specific action; improvement guidance
is contextual and belongs to the repository-owner context.

### Proposed future contract

`docs/product/product-experience-principles.md` is recommended as a separate
bounded contract. It is **not created in this session**.

It would own experience principles, information hierarchy, progressive
disclosure, first-result semantics, score presentation priority, shared
truth versus contextual emphasis, and the say-no rules.

It would not own scoring semantics, signal semantics, architecture, visual
design, component design, exact copy or implementation, and it would not
re-own the repository report section contract held by
`docs/product/report-information-architecture.md`.

### Current UI status

Current UI findings from Step 1 were reclassified as future P10 constraints
only. No fix, no implementation task and no UI recommendation was produced.

## Step 3 — Product Experience Principles Contract

Step 2 was reviewed and **accepted with minor refinements**. The refinements
are carried into the contract:

1. The Before → After transformation no longer uses absolute
   uncertainty-removal language. GitHub Proof does not eliminate uncertainty;
   the user still interprets, but now on top of evidence and its stated limits.
2. LEVEL 1 carries an explicit boundary that missing public evidence does not
   prove missing implementation, knowledge, skill or capability.
3. LEVEL 3 does not claim that nothing is hidden. Canonical evidence truth may
   be inspected at technical depth, internal implementation detail that is not
   product truth stays internal, and prohibited human inferences stay
   prohibited at every level.
4. The First Meaningful Result is not defined as "what the repository does not
   show". It states what is publicly evidenced and where important public
   evidence is missing, unavailable or insufficient, without converting
   evidence absence into implementation or capability absence.
5. Exact templates and slot mappings are deliberately not locked. If exact
   deterministic projection rules are required before P10, they need a
   separately approved bounded presentation contract or an extension of an
   existing one.
6. The score decision locks priority only. No component, chart or visual form
   is prescribed, and the fate of the current visual representation is left to
   P10.

### Created

    docs/product/product-experience-principles.md
    PRODUCT EXPERIENCE LOCKED, version 1

### Locked experience decisions

- Human-first model: HUMAN → PROBLEM → EXPERIENCE → TECHNOLOGY.
- Three core experience questions: what can we see, what is it based on, where
  does the evidence stop.
- Experience architecture: **UNDERSTAND / TRACE / INSPECT**. TRACE means
  evidence traceability only and is not future human verification.
- **First Meaningful Result = C**: one concise plain-language synthesis plus up
  to three evidence statements, as a presentation projection of already-approved
  deterministic truth. No new signal, no new score, no new inference, no LLM
  generation, and no semantic mapping invented in the view layer.
- **Visible Evidence Score = SECONDARY**. Scoring semantics unchanged; evidence
  understanding precedes score interpretation.
- One canonical evidence truth; contextual emphasis only, with no separate
  score, evidence engine, candidate truth or recruiter truth.
- Eight principle-level say-no rules.

### Ownership boundary

Session 070 owns the future-P10 experience truth. It does not own P10
implementation.

Exact template rules, exact copy, localization integration, visual hierarchy
implementation, component and responsive behaviour, accessibility states and
runtime UI validation remain **unresolved and P10-owned**.

The contract defers scoring, signal, repository-report-section, positioning,
profile, job, architecture, visual design and roadmap ownership to their
existing canonical owners, and does not duplicate
`docs/product/report-information-architecture.md`.

### Mutation scope

Only `docs/product/product-experience-principles.md` was created. All other
canonical product, architecture and roadmap documents remain byte-identical,
and no production code, UI code or test was touched.

## Step 3 Review — PASS WITH ONE BOUNDED CONTRACT CLARIFICATION

A potential ownership tension was identified: the experience contract states
that contextual user actions do not belong to the universal Level 1 evidence
truth, while the existing repository-specific report and design guidance
historically allows prioritized improvement actions in the repository report's
first-reading experience.

Clarified **only inside** `docs/product/product-experience-principles.md`, as
two new subsections under §8.

`§8.1 Universal Evidence Layer / Contextual Action Layer` separates the two:

- the universal evidence layer is shared across audiences, carries canonical
  evidence understanding, and must be understandable without any action being
  offered;
- the contextual action layer may follow or accompany the shared evidence truth
  when the user context makes it relevant, the repository owner may receive
  improvement guidance, the recruiter/reviewer must never be forced into
  repository-owner actions, and it creates no separate analysis truth.

Locked cross-surface rule:

    contextual actions != universal evidence truth

`§8.2 Ownership Boundary for Prioritized Actions` records that this document
owns only that cross-surface rule. The existing repository-report
prioritized-action presentation, including its position in the first-reading
layer, remains owned by `report-information-architecture.md`,
`scoring-and-report-contract.md` and `github-proof-design-principles-v1.md`.
Those contracts are not overridden or redefined, no change to current
repository report behaviour is required, and reconciliation of the concrete
surface with these cross-surface principles belongs to P10.

This is an ownership clarification, not a UI decision. No existing locked
document was modified.

## Explicit Non-Displacement

This packaging work does not displace the technical roadmap.

Session 070 does not replace or reorder the technical roadmap. The Step 2
decisions target **Profile P10 — Web Profile Flow**, which is far downstream of
the current position and is not brought forward by this session.

After Session 070 closes, development returns to the recorded evidence pipeline
position, before P7:

- Stage 7 — Public Profile Evidence Foundation
- P7 — Representative Repository Selection — NOT STARTED
- ProblemArea — taxonomy, promotion and repository evidence remain deferred
- `ProducedWorkEvidence` — unresolved
- `RepositoryProblemAreaEvidence` — blocked

## Final Pass

`docs/product/product-experience-principles.md` — Product Experience Contract
V1 received **FINAL PASS**.

Approved decisions:

- Experience architecture **UNDERSTAND / TRACE / INSPECT**.
- **First Meaningful Result = C** — one concise plain-language synthesis plus up
  to three evidence statements, as a presentation projection of already-approved
  deterministic truth.
- **Visible Evidence Score = SECONDARY**, with scoring semantics unchanged.
- Universal evidence truth versus contextual action boundary, including the
  cross-surface rule `contextual actions != universal evidence truth` and the
  ownership boundary leaving repository-report prioritized-action presentation
  with its existing report and design contracts.

Scope confirmations:

- Session 070 does **not** implement or advance P10.
- No UI implementation was performed.
- The technical roadmap was **not** reordered.
- After Session 070 integration, development returns to the recorded pre-P7
  evidence pipeline position.

Session 070 mutation set:

    docs/product/product-experience-principles.md   (new)
    worklog/CURRENT.md
    worklog/INDEX.md
    worklog/sessions/2026-08-18-070-jobs-style-product-packaging.md   (new)

All other canonical product, architecture and roadmap documents remain
byte-identical. No production code, UI code or test was modified.

## Current Status

READY_FOR_COMMIT

Not yet committed. Not yet integrated into `main`. Not yet remote verified.

## Next Action

User-owned commit of the Session 070 mutation set from their own terminal.

Deferred, each requiring separate approval: the bounded presentation contract
for exact deterministic projection rules, if required before P10; the Step 1
current-UI findings, which remain future P10 constraints and not Session 070
work; Profile P10 itself.

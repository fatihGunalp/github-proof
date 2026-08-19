# Public Profile Evidence V1 — Representative Repository Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Profile Phase:** P7 — Representative Repository and Role Evidence Alignment
(Representative Repository sub-decision only)
**Roadmap Stage:** Stage 7 — Public Profile Evidence Foundation
**Locked:** 2026-08-18 / Session 074

## 1. Purpose

This contract defines deterministically:

> For a specific canonical `RepeatedProfilePattern`, which repository should a
> reviewer inspect first, and why?

It resolves the representative-repository selection rule left open by
`docs/architecture/public-profile-evidence-domain-model.md` §11 and owned by
§18 P7, and it composes only over one coherent `ProfilePatternSet` already
produced under the pattern truth locked by
`docs/architecture/public-profile-evidence-patterns.md`.

It does not alter P1 result ownership, P2 eligibility semantics, P5
observation semantics or P6 pattern promotion semantics.

It does not define Role Evidence Alignment. See §14.

## 2. Canonical Boundary

`RepresentativeRepository` is an explanation/navigation result.

It is not:

- best repository,
- repository quality,
- repository ranking,
- strongest developer work,
- popularity ranking,
- capability evidence,
- seniority,
- employability,
- hire/no-hire.

Its epistemic strength may never exceed the pattern evidence that selected it.

## 3. Selection Scope — V1

The canonical V1 selection unit is:

> one representative repository per `RepeatedProfilePattern`.

Every canonical `RepeatedProfilePattern` that exists receives exactly one
representative-repository selection. That selection materializes as exactly
one `RepresentativePatternSupport`; how those supports are grouped into
`RepresentativeRepository` objects is a separate result-shape concern
governed by §3.4.

### 3.1 Excluded From V1

Two related shapes are explicitly **not** the V1 selection unit:

- a globally bounded profile representative set,
- a minimum repository set computed for maximum pattern coverage.

A global bounded set may exist later only as a presentation or deduplication
projection computed **over already completed, pattern-specific selections**
produced by this contract. It must not become an independent selection
mechanism, and it must not feed back into which repository this contract
selects for a given pattern.

Minimum-set/maximum-coverage selection is out of scope for V1 in full. It is
not partially adopted through comparison, tie-break or grouping rules.

### 3.2 No Global K

This contract never introduces a global top-N, top-3 or top-K representative
list. Each pattern's selection is independent of how many patterns exist or
how many repositories participate across the profile.

### 3.3 Empty ProfilePatternSet

`docs/architecture/public-profile-evidence-patterns.md` §6 already locks that
an empty pattern collection is not a claim: it means only that the contracted
repetition condition was not met on the observed evidence surface, and it is
not negative developer evidence. `ProfilePatternSet.Patterns` may legitimately
be empty (`Count == 0`).

This contract inherits that boundary without weakening it:

    zero RepeatedProfilePattern values
    →
    zero RepresentativeRepository values

A `ProfilePatternSet` with no patterns produces a valid
`RepresentativeRepositorySelection` whose `RepresentativeRepositories`
collection is empty. This is not a failure, not negative evidence, not
insufficient capability, and never a reason to fabricate a representative
repository. `PatternRuleVersion`, `ObservationRuleVersion`,
`EligibilityPolicyVersion`, `IsEvidenceSurfaceComplete` and
`RepresentativeRepositoryRuleVersion` are still recorded on the result exactly
as for a non-empty selection (§11) — only the repository list is empty.

When the set is non-empty, §3's one-selection-per-pattern rule applies to
every pattern the set contains: no pattern may be silently skipped, and no
pattern may receive more than one selection.

### 3.4 Grouped Result Cardinality

§3's canonical rule — *one representative repository selection per
`RepeatedProfilePattern`* — is a rule about **selection**, not about how many
`RepresentativeRepository` objects the result contains. The two are different
counts because §8 permits the same repository to independently win several
patterns, and §11 groups those wins under a single `RepresentativeRepository`.

The exact object model, for a source `ProfilePatternSet` containing `N`
patterns:

- `N == 0` → zero `RepresentativeRepository` objects (§3.3),
- every existing `RepeatedProfilePattern` → exactly one
  `RepresentativePatternSupport`,
- each `RepresentativePatternSupport` belongs to the repository that
  independently won that pattern under §4–§7,
- when the same `ProfileRepositoryReference` wins several patterns, those
  supports are grouped under one `RepresentativeRepository`.

Therefore, for `N > 0`:

    1 <= RepresentativeRepositories.Count <= N

    sum(RepresentativeRepository.PatternSupports.Count) == N

The lower bound is reached when one repository wins every pattern; the upper
bound when every pattern is won by a different repository.

`RepresentativeRepositories` must contain each selected
`ProfileRepositoryReference` exactly once after grouping: two entries may
never carry the same repository reference. `RepresentativeRepository.PatternSupports`
is always one or more — a `RepresentativeRepository` never exists without at
least one represented pattern.

Grouping is a result-shape concern only. It is performed after every
pattern's independent selection has completed, and it must never influence
which repository wins a pattern (§8).

## 4. Candidate Repositories

For one `RepeatedProfilePattern`:

    CandidateRepositories
    =
    RepeatedProfilePattern.IndependentSupportingRepositories

Only `IndependentSupportingRepositories` participate. `SupportingRepositories`
that are not independent supporters remain visible as pattern context but are
never selection candidates.

Because `IndependentSupportingRepositories` is itself constrained by the
locked P2/P5 `IndependentRepetition` contribution decision
(`docs/architecture/public-profile-evidence-repository-eligibility.md`):

- a repository excluded from `IndependentRepetition` because it is a fork or
  template-derived lineage can never become a representative candidate for
  that pattern,
- a repository whose independence is `Undetermined` is never optimistically
  promoted into the candidate set,
- an archived repository is **not** automatically excluded. Archived state
  blocks `CurrentFocus`, not `IndependentRepetition`. An archived repository
  that remains a valid independent supporter of the pattern stays a valid
  candidate.

This contract performs no independence re-evaluation. It consumes the
candidate set exactly as P5/P6 already produced it.

## 5. Comparison Rule

Candidates for the same pattern are compared only using evidence belonging to
that pattern.

Define:

    PatternSupportObservationCount(candidate, pattern)
    =
    number of distinct pattern.SupportingObservations for which candidate
    contains a positive supporting ProfileEvidenceReference

A higher `PatternSupportObservationCount` sorts first.

### 5.1 Semantic Strength

`PatternSupportObservationCount` means only:

> this repository exposes more of the structured evidence that forms this
> specific pattern.

It is not:

- a score,
- a quality signal,
- a strength value,
- a confidence value,
- capability evidence.

It has no meaning outside the one pattern it was computed for, and it is never
summed, averaged or compared across patterns.

## 6. Tie-Break

When two or more candidates share the same maximum
`PatternSupportObservationCount` for a pattern, resolve the tie using the
existing canonical `ProfileRepositoryReference` ordering:

1. `Owner`, ordinal comparison,
2. `Name`, ordinal comparison.

No other heuristic — stars, recency, size, activity, popularity or any hidden
weighting — may participate in the tie-break. No new heuristic may be
introduced without a new `RepresentativeRepositoryRuleVersion`.

### 6.1 Canonical Repository Identity Precondition

`ProfileRepositoryReference.CompareTo` compares `Owner` then `Name` only,
while the record's own equality also distinguishes references whose `Url`
differs. Two candidate references could therefore, in principle, be unequal
as values yet compare equal under §6 — leaving the winner undetermined.

Representative selection requires this precondition on the union of every
pattern's candidate set within one selection execution — not merely per
pattern, since winners from different patterns are compared to each other
during grouping (§3.4) and result ordering (§13):

> Within one selection execution, for any two candidate
> `ProfileRepositoryReference` values, `CompareTo` returns `0` **if and only
> if** the two values are equal.

Canonical P3→P6 input already satisfies this. The canonical GitHub profile
collector deduplicates repositories by `FullName` and constructs each
repository `Url` canonically from that same owner and name, so two candidates
sharing `Owner` and `Name` are necessarily the same value. Under canonical
input the precondition therefore holds trivially and §6 resolves every tie by
`Owner` then `Name`, exactly as locked — this contract does not change normal
winner semantics in any way.

If a non-canonical input ever violates the precondition — two unequal
candidate references for the same `Owner`/`Name` — the selection execution
must **reject that input deterministically**. It must not:

- depend on candidate enumeration order,
- silently pick one of the colliding references,
- fall back to `Url` or any other field as an additional winner heuristic,
- invent a new ranking or evidence heuristic of any kind.

Rejection is a structural input-validity failure, not a developer-evidence
statement: it says the supplied candidate set was not canonical, and it
carries no meaning about any repository or developer. This is deliberately
distinct from `ProfileRepositoryReferenceOrder`, whose `Url` fallback exists
to keep profile-surface *presentation ordering* independent of execution
order; that fallback is a display-ordering concern and must never be
borrowed as a representative-selection tie-break.

## 7. Exactly One Selection

Selection produces exactly one winning candidate repository per pattern: the
candidate with the highest `PatternSupportObservationCount`, tie-broken per
§6. That win materializes as exactly one `RepresentativePatternSupport`; it
does not by itself imply one `RepresentativeRepository` object, because a
repository winning several patterns is grouped into a single
`RepresentativeRepository` (§3.4). This applies to every pattern in the source
`ProfilePatternSet`; when that set is empty, there is nothing to apply it to
and no selection is produced (§3.3).

## 8. Cross-Pattern Independence

Each pattern's selection is computed independently of every other pattern.

The same repository may independently become the representative of multiple
patterns. That is a valid and expected outcome:

    Pattern A independently selects repo-x.
    Pattern B independently selects repo-x.

is correct.

A repository must never win a pattern's selection because it supports more
total profile patterns, more total observations across patterns, or a wider
overall evidence breadth. That is prohibited:

    repo-x supports more total patterns than repo-y,
    therefore repo-x is more representative of pattern A.

is incorrect regardless of how the comparison is expressed.

No set-cover, greedy coverage, global breadth score or hidden cross-pattern
weighting may participate in §5 or §6. Selection for pattern A must not
observe, count or rank evidence belonging to pattern B.

## 9. Incomplete and Undetermined Evidence

Known positive structured evidence may still produce a representative
selection when the overall evidence surface is incomplete. Incomplete evidence
must not become negative developer evidence and must not block a selection
that otherwise satisfies §4–§7.

Undetermined evidence:

- is not positive support,
- is not negative support,
- is not a candidate through optimistic fallback.

### 9.1 Coherent Source Set

Representative-repository selection is one deterministic, in-execution
composition over exactly one coherent `ProfilePatternSet` instance, not a bare
collection of `RepeatedProfilePattern` values assembled ad hoc. `ProfilePatternSet`
is the existing canonical container that already carries, for its entire
contained pattern collection:

- `RuleVersion` (`PatternRuleVersion`),
- `ObservationRuleVersion`,
- `PolicyVersion` (`EligibilityPolicyVersion`),
- `IsEvidenceSurfaceComplete`.

`ProfilePatternPromotionPolicy.Promote` already propagates
`IsEvidenceSurfaceComplete` from `CrossRepositoryEvidence` onto the
`ProfilePatternSet` it produces. This contract consumes that existing value —
it does not compute, re-derive or invent a new completeness signal.

This contract defines no persisted or cross-process identity for a
`ProfilePatternSet` instance. No such identity is canonically defined upstream
(unlike, for example, `RepositoryAnalysisReference`, which already carries a
stable `Guid`). V1 needs only the single supplied instance for the duration of
one selection execution; it does not introduce a
`ProfilePatternSetReference`-style identity type to name that instance beyond
its own lifetime. If exact persisted/cross-process `ProfilePatternSet`
identity is ever required — for example, to let a later report reference a
completed profile analysis across a process boundary — that identity belongs
to a separate, later contract. It is explicitly deferred here, not invented.

### 9.2 Completeness Preservation

A representative-repository selection result must preserve
`ProfilePatternSet.IsEvidenceSurfaceComplete` directly on the
representative-selection result boundary: one `IsEvidenceSurfaceComplete`
value carried alongside the full selection output produced from that one
`ProfilePatternSet` instance (§11 `RepresentativeRepositorySelection`).

`ProfilePatternSet.IsEvidenceSurfaceComplete` is not interchangeable with, and
must not be replaced by,
`ObservedProfileEvidence.UndeterminedEvidenceRepositoryCount`. The two are
different semantics at different levels:

- `IsEvidenceSurfaceComplete` states whether the entire evidence surface the
  `ProfilePatternSet` was built from is complete,
- `UndeterminedEvidenceRepositoryCount` is observation-scoped and may differ
  between the `SupportingObservations` of one pattern (for example, the
  `documentation` pattern's two mapped signals).

Downstream presentation must not claim more certainty than
`IsEvidenceSurfaceComplete` and the individual observations' own counts
support. This contract does not collapse per-observation
`UndeterminedEvidenceRepositoryCount` values into a new pattern-level or
representative-level number; each remains readable from its own
`ObservedProfileEvidence` via the traceability in §10–§11.

## 10. Traceability

`RepresentativePatternSupport` supports two distinct traces. They answer
different questions, serve different purposes, and must never be conflated or
substituted for one another.

**Canonical Pattern Trace** — identity / canonical pattern context only,
never a source of representative-specific evidence (§10.1):

    RepresentativeRepositorySelection
            ↓
    RepresentativeRepository
            ↓
    RepresentativePatternSupport
            ↓
    Pattern
            ↓
    exact canonical RepeatedProfilePattern (within the source ProfilePatternSet)

**Representative Support Evidence Trace** — answers exactly *"why was this
repository selected for this pattern?"*, and is the only trace that may be
used to explain or present that selection (§10.2):

    RepresentativeRepositorySelection
            ↓
    RepresentativeRepository
            ↓
    RepresentativePatternSupport
            ↓
    scoped supporting ObservationKey values
            ↓
    scoped ProfileEvidenceReference values
            ↓
    RepositoryAnalysisReference[]
            ↓
    selected-repository evidence

The Canonical Pattern Trace exists so a result can prove exactly which
`RepeatedProfilePattern` instance backs a support. `Pattern` may legitimately
expose the complete canonical pattern object — `SupportingObservations`,
`SupportingRepositories`, `IndependentSupportingRepositories` and all of
their evidence, for every supporting repository — because it is a reference
to the one real existing instance (§9.1), not a filtered copy. That
completeness is inherent to holding a genuine reference and is not itself a
violation.

But the Canonical Pattern Trace is **never** the representative-specific
evidence path. Evidence reached through `Pattern` is canonical pattern
context. It must never be interpreted, enumerated or presented as
representative-specific support evidence for **any** repository — including
the selected repository itself. Reading
`Pattern.SupportingObservations` and filtering it down to the selected
repository is equally prohibited: the result may coincide, but it is the
wrong trace, and it silently bypasses the scoping §10.2 guarantees.

The Representative Support Evidence Trace is the sole source of
representative-specific evidence: the repository- and pattern-scoped subset
already computed for §5's comparison and carried on
`RepresentativePatternSupport` itself (§11). For the question *"why was this
repository selected for this pattern?"*, that trace is the only admissible
answer.

### 10.1 Exact Pattern Identity (Canonical Pattern Trace)

`RepeatedProfilePattern` truth is not duplicated by this contract.

`Family` + `PatternKey` is the canonical semantic identity of a pattern within
the versioned pattern taxonomy locked by
`docs/architecture/public-profile-evidence-patterns.md` — it names *which*
pattern (for example, `EngineeringPractice` / `documentation`) under a given
`PatternRuleVersion`.

Within one coherent `ProfilePatternSet`, `(Family, PatternKey)` is sufficient
to locate the unique `RepeatedProfilePattern` instance: `ProfilePatternSet`'s
own construction invariant already rejects a duplicate `(Family, PatternKey)`
pair within one set and already enforces a single consistent `RuleVersion` and
`ObservationRuleVersion` across every pattern it contains.

`(Family, PatternKey)` is not, by itself, an identity for a specific analysis
execution or for a persisted `ProfilePatternSet` instance. Two different
selection executions — even under identical rule versions — may each hold
their own `ProfilePatternSet` instance containing a pattern with the same
`(Family, PatternKey)` but different supporting evidence (for example, a
later GitHub state). This contract does not introduce an execution or
persistence identity to disambiguate those instances (§9.1).

Instead, exact traceability is structural, not a later lookup: a
`RepresentativePatternSupport` holds an in-memory canonical reference to the
exact `RepeatedProfilePattern` instance it was produced from — the same
instance already present in the one `ProfilePatternSet` supplied to that
selection execution (§11). This is composition over existing canonical
pattern truth, not a new identity: it is not a
`ProfilePatternSetReference`-style persistence identity (§9.1), not a
duplicate or recomputed copy of the pattern, and not resolved by matching
`(Family, PatternKey)` after the fact — the reference is established once,
directly, when the support is constructed. `PatternFamily` and `PatternKey`
remain useful convenience projections of that referenced pattern's own
`Family`/`PatternKey`, but they are not themselves the traceability
mechanism, and must never be used to match a pattern across two different
executions or sets.

### 10.2 Evidence Scope (Representative Support Evidence Trace)

The Representative Support Evidence Trace (§10) must carry only evidence
belonging to:

- the selected repository,
- the exact pattern it represents.

It must not attach unrelated evidence from the same repository, including
evidence supporting a different pattern the same repository also represents.
Each `RepresentativePatternSupport` is scoped to exactly one pattern.

This scoping is delivered by the scoped
`ObservationKey`/`ProfileEvidenceReference` fields on
`RepresentativePatternSupport`, which are the sole representative-specific
evidence source. The separate Canonical Pattern Trace (§10, §10.1) is not a
second path to the same answer: `Pattern` exists for identity and canonical
pattern context only, and a consumer must never use it to read, enumerate or
present representative-specific evidence for any repository — the selected
repository included (§10).

## 11. Contract Shape

### RepresentativeRepositorySelection

This is the representative-selection result boundary: the single coherent
output of running §3–§8, in one execution, over one `ProfilePatternSet`
instance (§9.1).

It carries no stable or persisted reference back to that `ProfilePatternSet`
instance — no such identity is canonically defined upstream, and this
contract does not invent one (§9.1). Instead it records the source set's
semantic context directly, as plain values copied at selection time:

- `PatternRuleVersion` — carried directly from the source
  `ProfilePatternSet.RuleVersion` (§12.1).
- `ObservationRuleVersion` — carried directly from the source
  `ProfilePatternSet.ObservationRuleVersion` (§12.1).
- `EligibilityPolicyVersion` — carried directly from the source
  `ProfilePatternSet.PolicyVersion` (§12.1).
- `IsEvidenceSurfaceComplete` — carried directly from
  `ProfilePatternSet.IsEvidenceSurfaceComplete` (§9.2); no new completeness
  value is computed.
- `RuleVersion` — the `RepresentativeRepositoryRuleVersion` that produced this
  selection.
- `RepresentativeRepositories` — **zero or more** `RepresentativeRepository`,
  after grouping per §3.4. Zero when the source `ProfilePatternSet` contains
  zero patterns; this is a valid result, not a failure (§3.3). Every entry
  holds a distinct `ProfileRepositoryReference` (§3.4). The other five fields
  above are always recorded, even when this collection is empty.

### RepresentativeRepository

- `Repository` — the selected `ProfileRepositoryReference`.
- `PatternSupports` — one or more `RepresentativePatternSupport`, one per
  pattern this repository was independently selected for.

A `RepresentativeRepository` groups every pattern for which the same
repository was independently selected. It does not itself perform selection
across patterns; each grouped `RepresentativePatternSupport` was produced by
an independent §5–§7 run for its own pattern. It carries no version or
completeness field of its own — those live once, at the
`RepresentativeRepositorySelection` boundary, per §9.2 and §12.1.

### RepresentativePatternSupport

Canonical Pattern Trace field:

- `Pattern` — an in-memory canonical reference to the exact
  `RepeatedProfilePattern` instance this support was produced from: the same
  instance already present in the one `ProfilePatternSet` supplied to this
  selection execution (§10, §10.1). This is composition over existing pattern
  truth, not a copy, not a re-derivation and not a new persistence identity
  (§9.1). It exists for identity and canonical pattern context only — it is
  never a representative-specific evidence path, for any repository,
  including the selected one (§10, §10.2).
- `PatternFamily`, `PatternKey` — convenience projections of `Pattern.Family`
  and `Pattern.PatternKey` (§10.1). They are not themselves the traceability
  mechanism and must never be used to match a pattern across two different
  executions or sets.

Representative Support Evidence Trace fields:

- supporting `ProfileEvidenceReference` values — scoped exactly to this
  repository and this pattern (§10.2), ordered by `EvidenceKey`, ordinal
  (§13.1).
- supporting observation identities — the `ObservationKey` values counted in
  §5 for this repository and this pattern (§10, §10.2), in the same canonical
  order. This is a projection of the scoped evidence references above, never
  an independently mutable second truth (§13.1).

This pair is the sole source for evidence explaining why this repository was
selected; `Pattern` above must never substitute for it, not even for the
selected repository (§10).

### 11.1 Prohibited Fields

The following must never appear on `RepresentativeRepositorySelection`,
`RepresentativeRepository` or `RepresentativePatternSupport`:

- Rank
- Score
- Quality
- Strength
- Confidence
- Capability
- Seniority
- Employability

## 12. Version Ownership

`RepresentativeRepositoryRuleVersion` identifies the selection rules defined
here.

It is a dedicated version identity. It must not reuse `PatternRuleVersion`,
`ObservationRuleVersion` or `EligibilityPolicyVersion`.

### 12.1 ProfilePatternSet as the Coherent Anchor

`RepeatedProfilePattern` itself owns only `RuleVersion` (`PatternRuleVersion`)
and `ObservationRuleVersion`. It does not own `EligibilityPolicyVersion` and
it does not own a completeness flag. `EligibilityPolicyVersion` and
`IsEvidenceSurfaceComplete` exist only on the containing `ProfilePatternSet`
(§9.1).

Consequently, representative-repository selection must consume one coherent
`ProfilePatternSet` as its input — never a loose collection of
`RepeatedProfilePattern` values gathered from possibly different sets. A
`RepresentativeRepositorySelection` result must record:

- the consumed `ProfilePatternSet.RuleVersion` (`PatternRuleVersion`),
- `ProfilePatternSet.ObservationRuleVersion`,
- `ProfilePatternSet.PolicyVersion` (`EligibilityPolicyVersion`),
- `ProfilePatternSet.IsEvidenceSurfaceComplete` (§9.2),

directly, as plain values on `RepresentativeRepositorySelection` (§11) — not
through any stable or persisted reference to the source `ProfilePatternSet`
instance, since no such identity is canonically defined upstream (§9.1). A
selection execution must reject an attempt to mix patterns drawn from two
different `ProfilePatternSet` instances or two different version
combinations.

A new `RepresentativeRepositoryRuleVersion` is required to change:

- candidate rules (§4),
- representative count per pattern, or the grouped result cardinality it
  produces (§3, §3.4, §7),
- the pattern-support comparison rule (§5),
- the tie-break rule (§6) or its canonical identity precondition and
  rejection behavior (§6.1),
- any grouping or deduplication semantics (§3.4, §11),
- the evidence carried by the representative result (§10),
- the canonical result ordering (§13),
- the scoped support evidence ordering or projection rule (§13.1).

## 13. Determinism, Ordering and Locale

For the same:

- source `ProfilePatternSet` (and, transitively, the `CrossRepositoryEvidence`
  it was promoted from),
- `ObservationRuleVersion`,
- `EligibilityPolicyVersion`,
- `PatternRuleVersion`,
- `RepresentativeRepositoryRuleVersion`,

the selected representative set must be semantically identical.

Canonical ordering:

1. pattern family,
2. pattern key, ordinal,
3. within a pattern's candidate comparison: `PatternSupportObservationCount`
   descending, then `ProfileRepositoryReference` ordering (§6),
4. `RepresentativeRepository` results by `ProfileRepositoryReference`
   ordering,
5. `PatternSupports` within one `RepresentativeRepository` by pattern family,
   then pattern key, ordinal,
6. within one `RepresentativePatternSupport`, the scoped supporting evidence
   by `ObservationKey`, ordinal (§13.1).

Input enumeration order and execution order must not change the result. No
system clock participates. Locale may change presentation only; it must not
change a selected repository, a pattern key, a count or a rule version.

### 13.1 Scoped Support Evidence Ordering and Projection

The Representative Support Evidence Trace fields on
`RepresentativePatternSupport` (§10.2, §11) are also ordered, so input
enumeration order cannot change them either.

Two upstream invariants already make this exact:
`ObservedProfileEvidence` requires every `ProfileEvidenceReference` it holds
to satisfy `EvidenceKey == ObservationKey`, and permits at most one evidence
reference per repository per observation. Consequently, for one selected
repository and one pattern, each counted observation contributes exactly one
scoped `ProfileEvidenceReference`, and that reference's `EvidenceKey` is the
observation's `ObservationKey`.

Therefore:

- the scoped `ProfileEvidenceReference` values are ordered by their
  `EvidenceKey`, ordinal;
- the supporting `ObservationKey` identities follow that same canonical
  order — they are the same key sequence, not an independently sorted list.

`SupportingObservationKeys` is a **projection**, never a second mutable
truth. Implementations must take one of these two shapes:

- **A (preferred)** — derive the `ObservationKey` sequence from the scoped
  `ProfileEvidenceReference` values, so the two cannot diverge by
  construction; or
- **B** — store both, and validate at construction that they are exactly
  one-to-one and identically ordered, rejecting any input where they are not.

Neither shape introduces a score, weight or count beyond
`PatternSupportObservationCount` as already defined in §5.

## 14. Role Evidence Alignment Boundary

Session 074 does not define `RoleEvidenceAlignment` semantics.

This contract must not infer, imply or encode any role mapping, including but
not limited to:

    C# → .NET Developer
    JavaScript → Frontend Developer
    Python → AI/ML Developer

`RepresentativeRepository` remains job-agnostic and role-agnostic. Role
Evidence Alignment belongs to a later P7 decision session and may reference
`RepresentativeRepository` results, but this contract carries no dependency in
the other direction.

## 15. LLM Boundary

No LLM participates in representative-repository selection.

An LLM may narrate an existing canonical `RepresentativeRepository` result. It
may not create, upgrade, reinterpret or override one.

## 16. Prohibited Semantics

Representative-repository production must never emit:

- developer capability score,
- candidate score,
- seniority,
- employability,
- hire/no-hire,
- candidate ranking,
- repository quality ranking,
- popularity-derived developer-quality evidence,
- years-of-professional-experience inference,
- pattern or repository strength/confidence values,
- hidden weighting of any kind,
- cross-pattern breadth advantage (§8).

## 17. P7 Dependency and Scope

This contract requires no additional P2 runtime and no new P5/P6 evidence
kind. It composes only over one coherent `ProfilePatternSet` — the existing
canonical container for `RepeatedProfilePattern` results already locked by
`docs/architecture/public-profile-evidence-patterns.md` — and introduces no
new upstream evidence production.

`docs/architecture/public-profile-evidence-domain-model.md` §18 assigns
representative-repository selection rules and Role Evidence Alignment jointly
to P7. This contract discharges only the representative-repository half. Role
taxonomy, generic role evidence definitions and role evidence alignment rules
remain unstarted and unscoped by this document.

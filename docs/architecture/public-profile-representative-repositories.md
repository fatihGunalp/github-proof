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
`RepresentativeRepository` selection.

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

## 7. Exactly One Selection

Selection produces exactly one primary `RepresentativeRepository` candidate
per pattern: the candidate with the highest `PatternSupportObservationCount`,
tie-broken per §6. This applies to every pattern in the source
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

Every `RepresentativeRepositorySelection` result must support this traversal:

    RepresentativeRepositorySelection
            ↓
    RepresentativeRepository
            ↓
    RepresentativePatternSupport
            ↓
    exact RepeatedProfilePattern within the source ProfilePatternSet
            ↓
    SupportingObservations (ObservedProfileEvidence[])
            ↓
    ProfileEvidenceReference[]
            ↓
    RepositoryAnalysisReference[]
            ↓
    repository-level evidence

### 10.1 Exact Pattern Identity

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

### 10.2 Evidence Scope

A representative result must carry only evidence belonging to:

- the selected repository,
- the exact pattern it represents.

It must not attach unrelated evidence from the same repository, including
evidence supporting a different pattern the same repository also represents.
Each `RepresentativePatternSupport` is scoped to exactly one pattern.

This scoping constrains *evidence enumeration*, not the traceability
reference itself. `RepresentativePatternSupport.Pattern` (§11) necessarily
points at the full `RepeatedProfilePattern` instance — including its
`SupportingObservations` for every supporting repository, not only the
selected one — because that is the only structural way to guarantee exact
traceability (§10.1). A consumer must use `Pattern` only to verify or
navigate pattern identity, never to enumerate or present evidence for a
repository other than the one this `RepresentativePatternSupport` was
produced for. The scoped `ObservationKey`/`ProfileEvidenceReference` fields on
`RepresentativePatternSupport` remain the sole source for evidence belonging
to the selected repository; `Pattern` is not a second, wider evidence path
around that scoping.

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
  one produced per `RepeatedProfilePattern` instance actually present in that
  one `ProfilePatternSet` instance (§10.1, §3.3). Zero when the source
  `ProfilePatternSet` contains zero patterns; this is a valid result, not a
  failure (§3.3). The other five fields above are always recorded, even when
  this collection is empty.

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

- `Pattern` — an in-memory canonical reference to the exact
  `RepeatedProfilePattern` instance this support was produced from: the same
  instance already present in the one `ProfilePatternSet` supplied to this
  selection execution (§10.1). This is composition over existing pattern
  truth, not a copy, not a re-derivation and not a new persistence identity
  (§9.1). It exists for identity/traceability verification only — it is not
  an evidence-enumeration path and must not be used to read or present
  evidence for any repository other than the one this
  `RepresentativePatternSupport` was produced for (§10.2).
- `PatternFamily`, `PatternKey` — convenience projections of `Pattern.Family`
  and `Pattern.PatternKey` (§10.1). They are not themselves the traceability
  mechanism and must never be used to match a pattern across two different
  executions or sets.
- supporting observation identities — the `ObservationKey` values counted in
  §5 for this repository and this pattern.
- supporting `ProfileEvidenceReference` values — scoped exactly to this
  repository and this pattern, per §10.2.

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
- representative count per pattern (§3, §7),
- the pattern-support comparison rule (§5),
- the tie-break rule (§6),
- any grouping or deduplication semantics (§11),
- the evidence carried by the representative result (§10).

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
   then pattern key, ordinal.

Input enumeration order and execution order must not change the result. No
system clock participates. Locale may change presentation only; it must not
change a selected repository, a pattern key, a count or a rule version.

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

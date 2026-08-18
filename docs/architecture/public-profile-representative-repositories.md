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
§18 P7, and it composes only over pattern truth already locked by
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
tie-broken per §6.

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

A `RepresentativeRepository` result must preserve enough completeness
information that downstream presentation cannot claim more certainty than the
underlying pattern evidence supports.

This contract does not compute or materialize a new pattern-level
completeness number. `RepeatedProfilePattern` itself carries no single
completeness field; completeness lives on each of its `SupportingObservations`
(`ObservedProfileEvidence.EligibleRepositoryCount` and
`.UndeterminedEvidenceRepositoryCount`). `EligibleRepositoryCount` is uniform
across every observation of one `CrossRepositoryEvidence` by that model's own
construction invariant, so it may be read directly from any supporting
observation. `UndeterminedEvidenceRepositoryCount` is observation-scoped and
may differ between the `SupportingObservations` of one pattern (for example,
the `documentation` pattern's two mapped signals) — this contract does not
collapse those per-observation counts into one new pattern-level value. A
`RepresentativeRepository` result satisfies this section by remaining
traceable to its pattern's `SupportingObservations` per §10–§11, and reading
each observation's own counts from there rather than inventing a new field.

## 10. Traceability

Every `RepresentativeRepository` result must support this traversal:

    RepresentativeRepository
            ↓
    RepresentativePatternSupport
            ↓
    RepeatedProfilePattern
            ↓
    ObservedProfileEvidence[]
            ↓
    ProfileEvidenceReference[]
            ↓
    RepositoryAnalysisReference[]
            ↓
    repository-level evidence

A representative result must carry only evidence belonging to:

- the selected repository,
- the exact pattern it represents.

It must not attach unrelated evidence from the same repository, including
evidence supporting a different pattern the same repository also represents.
Each `RepresentativePatternSupport` is scoped to exactly one pattern.

## 11. Contract Shape

### RepresentativeRepository

- `Repository` — the selected `ProfileRepositoryReference`.
- `PatternSupports` — one or more `RepresentativePatternSupport`, one per
  pattern this repository was independently selected for.
- `RuleVersion` — the `RepresentativeRepositoryRuleVersion` that produced the
  selection.
- no separate completeness field; completeness is read from each
  `RepresentativePatternSupport`'s referenced `SupportingObservations` per §9.

A `RepresentativeRepository` groups every pattern for which the same
repository was independently selected. It does not itself perform selection
across patterns; each grouped `RepresentativePatternSupport` was produced by
an independent §5–§7 run for its own pattern.

### RepresentativePatternSupport

- `PatternFamily` — the represented pattern's family.
- `PatternKey` — the represented pattern's key.
- supporting observation identities — the `ObservationKey` values counted in
  §5 for this repository and this pattern.
- supporting `ProfileEvidenceReference` values — scoped exactly to this
  repository and this pattern, per §10.

### 11.1 Prohibited Fields

The following must never appear on `RepresentativeRepository` or
`RepresentativePatternSupport`:

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

A `RepresentativeRepository` result must record the consumed
`PatternRuleVersion` (and, transitively through the pattern, its
`ObservationRuleVersion` and `EligibilityPolicyVersion`) and must reject
inputs produced under mixed versions, exactly as `RepeatedProfilePattern`
does for its own inputs.

A new `RepresentativeRepositoryRuleVersion` is required to change:

- candidate rules (§4),
- representative count per pattern (§3, §7),
- the pattern-support comparison rule (§5),
- the tie-break rule (§6),
- any grouping or deduplication semantics (§11),
- the evidence carried by the representative result (§10).

## 13. Determinism, Ordering and Locale

For the same:

- `CrossRepositoryEvidence`,
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
kind. It composes only over `RepeatedProfilePattern` results already locked by
`docs/architecture/public-profile-evidence-patterns.md`.

`docs/architecture/public-profile-evidence-domain-model.md` §18 assigns
representative-repository selection rules and Role Evidence Alignment jointly
to P7. This contract discharges only the representative-repository half. Role
taxonomy, generic role evidence definitions and role evidence alignment rules
remain unstarted and unscoped by this document.

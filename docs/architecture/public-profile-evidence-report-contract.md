# Public Profile Evidence V1 — Typed Profile Report Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Profile Phase:** P8 — Deterministic Profile Composer and Report Contract
(report sub-contract)
**Roadmap Stage:** Stage 7 — Public Profile Evidence Foundation
**Locked:** 2026-08-19 / Session 079
**Amended:** 2026-08-19 / Session 080 — §6.5 only: records that the
previously open ProblemArea presentation decision is now locked in
`docs/product/public-profile-evidence-product-contract.md` §24 (default
Milestone B report does not render an `Unsupported` ProblemArea section),
and adds the structured-truth-is-never-dropped and never-a-negative-finding
rules. No section set, availability state, ordering rule or prohibited
semantic changed
**Amended:** 2026-08-28 / Session 141, recorded by Session 142 — §6.5 only.
Separates **canonical omission** (a section leaving the typed report — still
prohibited, now stated explicitly and in more detail than before) from
**presentation omission** (a rendering declining to draw the visual container
of a section that has no user-facing content — now permitted, per section,
only under an explicit product-contract sanction). The single sentence
"Omission is never permitted for a `Supported` section on the grounds that its
content is empty" is replaced by that distinction; its intent is preserved and
the five `Supported + empty` sections that report a real analytical result
about the developer's visible work are explicitly unaffected. `Limitations` in
its empty state is the one `Supported` section sanctioned, and a sanction table
is added. **No section set, availability state, ordering rule, evidence rule or
prohibited semantic changed**, and no `ProfileReport` field, type or projector
behavior changed
**Production Code:** NOT IN SCOPE

## 1. Purpose

This contract defines deterministically:

> How canonical `DeveloperPublicEvidenceProfile` truth is projected into a
> typed profile report, what that report may express, and in what order.

It resolves the profile report DTO shape, section availability semantics and
canonical report ordering that
`docs/product/public-profile-evidence-product-contract.md` §27 deliberately
left open, and it presents the canonical root locked by
`docs/architecture/public-profile-evidence-domain-model.md` §14.

It does not alter P1 result ownership, P2 eligibility semantics, P5
observation semantics, P6 pattern promotion semantics or P7
representative-repository selection semantics.

It defines presentation projection only. It defines no evidence rule.

## 2. Canonical Boundary

`ProfileReport` is a deterministic typed presentation DTO.

It is not:

- a domain aggregate,
- a second canonical truth store,
- an inference engine,
- a scoring surface.

Canonical flow:

    DeveloperPublicEvidenceProfile
            ↓
    ProfileReportProjector
            ↓
    ProfileReport

The projection direction is one-way. No report result may flow back into
canonical profile truth.

Its epistemic strength may never exceed the profile truth it was projected
from.

## 3. What the Report May Do

The report may:

1. translate,
2. organize,
3. explain.

Translating means rendering canonical semantic identities into localized
display text. Organizing means grouping, ordering and progressively
disclosing existing truth. Explaining means exposing the evidence trace that
already exists in canonical truth.

## 4. What the Report May Not Do

The report and its projector must not:

- create a new TechnicalFocus result,
- create a new EngineeringPractice result,
- infer a ProblemArea result,
- infer a RoleEvidenceAlignment result,
- rerun or re-derive repository eligibility,
- reselect or rerank RepresentativeRepository,
- calculate any score,
- infer seniority,
- infer capability,
- infer employability,
- infer job fit,
- produce hire/no-hire,
- use locale to change semantic truth,
- treat LLM narration as canonical truth.

Therefore:

    Presentation
    !=
    Inference Engine

    Locale Change
    !=
    Truth Change

## 5. Canonical Report Sections

The typed report contract provides semantic sections for exactly:

1. PublicProfileSummary
2. TechnicalFocusAreas
3. EngineeringPracticePatterns
4. ProblemAreaPatterns
5. RepresentativeRepositories
6. WhyThisConclusion / ConclusionEvidence
7. Limitations
8. RepositoryDrillDown
9. RoleEvidenceAlignment — conditional extension

The semantic responsibility of each section is locked by this contract.

Exact code naming may follow existing repository naming conventions. Naming
freedom does not extend to semantic responsibility, section set or ordering.

## 6. Section Availability Semantics

Every report section carries an explicit availability state. The canonical
states are:

- Supported
- Unsupported
- Deferred

Section content emptiness is an independent dimension. Availability and
emptiness must not be collapsed into one flag, and an empty content
collection must never be rendered as though it were a negative finding.

### 6.1 Supported

The capability exists in this product version and ran.

`Supported` with empty content means the capability ran and produced no
result under its versioned rules.

Example:

    TechnicalFocusAreas
    Supported + empty

means only:

> No repeated TechnicalFocus pattern was promoted under the current
> versioned promotion rules.

It does not mean:

> This developer has no technical focus.

### 6.2 Unsupported

The capability does not exist in this product version.

Canonical V1 case:

    ProblemAreaPatterns
    Unsupported + empty

`ProblemArea` has no taxonomy, no promotion rule and no production code path
in V1 (`docs/architecture/public-profile-evidence-patterns.md` §9).

`Unsupported + empty` does not mean:

> No problem area was found.

The product does not produce this inference at all. Presenting the empty
result as a finding would convert a capability boundary into an apparent
evidence absence.

### 6.3 Deferred

The capability is contracted but explicitly postponed for this version.

Canonical V1 case:

    RoleEvidenceAlignment
    Deferred + empty

RoleEvidenceAlignment is explicitly deferred for Profile V1 / Milestone B
(`docs/architecture/public-profile-evidence-domain-model.md` §12).

`Deferred + empty` does not mean:

> No role match.

### 6.4 Required Distinctions

    Unsupported
    !=
    Evidence Absence

    Deferred
    !=
    Negative Result

    Supported + empty
    !=
    Negative Developer Evidence

### 6.5 Default Presentation of Non-Supported Sections

Default presentation may omit an `Unsupported` or `Deferred` section rather
than rendering an empty-result card that a reader would reasonably interpret
as a finding — but only where the product contract sanctions that omission.

This is a presentation decision taken over an explicit capability state. It
is not the hiding of canonical evidence: the capability state remains part of
the typed report contract and remains available to any consumer that chooses
to surface it.

#### Canonical Omission vs Presentation Omission

**Amended 2026-08-28 / Session 141.** This subsection previously read, in
full:

> Omission is never permitted for a `Supported` section on the grounds that
> its content is empty, because its emptiness is a real analytical result.

That sentence conflated two different things, and the sentence is now replaced
by the explicit distinction below. Its protective intent is preserved
completely: nothing that follows permits a canonical section, availability
state or item to be dropped, and no analytical result may be erased.

**Canonical omission — PROHIBITED.**

    A `Supported` section may never be removed from the canonical report.

- The section stays in the typed `ProfileReport` at its canonical position
  (§7).
- Its `Availability` stays `Supported` and may not be downgraded, recomputed
  or swapped for another state because it happened to be empty.
- Its `Items` collection is carried as-is, including as `[]`.
- No data is lost, merged, reordered or substituted.
- Every structured/API consumer continues to see the section exactly as
  composed.

**Presentation omission — PERMITTED, bounded.**

    Web presentation may omit the visual container of a `Supported` section
    when that section has no user-facing content, and this is not a canonical
    omission.

- It removes a **container** — a heading, a card, a disclosure control — and
  nothing else.
- It requires the section to have no user-facing content in that render;
  a section with items always renders.
- It requires an explicit product-contract sanction naming that section, in
  the same way the `Unsupported`/`Deferred` omissions above do. The sanction
  is not general: it is granted per section, and it must state why the empty
  container communicates less accurately than its absence.
- It may never be used to avoid reporting a real analytical result about the
  developer's visible work. Where a `Supported + empty` section *is* such a
  result — no recurring technical direction, no recurring working method, no
  selected starting point, no recurring conclusion, no inspectable repository
  — the section must keep rendering its neutral empty copy. Those five
  sections are unaffected by this amendment.

The distinction is therefore between **what the report contains** and **what
one rendering of it chooses to show**. This contract governs the first and is
unchanged by Session 141; it governs the second only to the extent of setting
the boundary above.

#### Sanctioned Presentation Omissions

| Section | Availability | Sanction |
|---|---|---|
| `ProblemAreaPatterns` | `Unsupported` | `docs/product/public-profile-evidence-product-contract.md` §24 |
| `RoleEvidenceAlignment` | `Deferred` | `docs/product/public-profile-evidence-product-contract.md` §24 |
| `Limitations` | `Supported`, `Items = []` only | `docs/product/session-141-human-language-copy/10-limits-warnings-empty-states.md` §7, recorded in `docs/product/p10-presentation-language-contract.md` §8.5.1 |

`Limitations` is the only `Supported` section carrying this sanction. Its empty
state is the **absence of a caveat**, not the absence of a finding about the
developer's work: a heading that promises a constraint and then withdraws it
communicates less accurately than no section at all. A non-empty `Limitations`
section always renders. Any future request to omit another `Supported` section
requires its own product-level sanction and its own entry in this table.

#### Product-Level Sanction

Section presence is a product decision owned by
`docs/product/public-profile-evidence-product-contract.md` §24. This contract
governs how a section is typed, ordered and marked — not whether the product
is willing to drop it.

Both non-supported V1 sections are now sanctioned for omission:

- `RoleEvidenceAlignment` (Deferred): product contract §24 states the section
  is empty or not shown and that the report remains complete and valid
  without it.
- `ProblemAreaPatterns` (Unsupported): product contract §24 locks that the
  default Milestone B user-facing report does not render this section while
  its availability is `Unsupported`.

One `Supported` section is additionally sanctioned for **presentation**
omission in its empty state only — `Limitations`, under Session 141; see the
sanction table above. That sanction is product-owned in the same way, and it
removes a container, never a section.

#### Structured Truth Is Never Dropped

Omission is a rendering decision in the default user-facing report only. The
typed report and any structured/API surface must still carry:

    ProblemAreaPatterns
    Availability = Unsupported
    Items = []

and

    RoleEvidenceAlignment
    Availability = Deferred
    Items = []

and, for the Session 141 presentation sanction:

    Limitations
    Availability = Supported
    Items = []

A consumer reading the structured report must still be able to see that the
product version does not produce ProblemArea, that RoleEvidenceAlignment is
deferred, and that `Limitations` was composed and recorded no limitation.
Presentation may decline to render a section; it may never delete the
capability state.

`Unsupported` and `Deferred` must not be collapsed into one state, in the
typed contract or in any presentation, because they answer different
questions: the product does not have this capability, versus the product has
contracted this capability and postponed it.

#### Never a Negative Finding

Whether rendered or omitted, `Unsupported + empty` and `Deferred + empty`
must never be presented as:

- "No problem areas found",
- "No role match",
- an empty result card a reader would read as a finding,
- any negative developer conclusion.

This restates §6.2, §6.3 and §6.4 at the presentation boundary.

## 7. Canonical Report Ordering

The semantic report section order is fixed and locale-independent:

1. PublicProfileSummary
2. TechnicalFocusAreas
3. EngineeringPracticePatterns
4. ProblemAreaPatterns
5. RepresentativeRepositories
6. WhyThisConclusion
7. Limitations
8. RepositoryDrillDown
9. RoleEvidenceAlignment — conditional extension

Locale may change labels and text.

Locale may not change semantic ordering.

Omitting a section's presentation under §6.5 — whether an `Unsupported` /
`Deferred` section or the sanctioned empty `Supported` one — does not reorder
the remaining sections; the surviving sections keep their relative canonical
order, and the canonical report itself still carries every section at its
canonical position.

### 7.1 Item Ordering

Typed report item ordering must also be deterministic.

Canonical items must not be ordered by:

- localized display text,
- API response order,
- collection insertion order,
- asynchronous completion order,
- current wall-clock time,
- any hidden quality or ranking score.

Where upstream canonical ordering already exists — pattern ordering in
`docs/architecture/public-profile-evidence-patterns.md` §12,
representative-repository ordering in
`docs/architecture/public-profile-representative-repositories.md`,
limitation and warning ordering in
`docs/architecture/public-profile-evidence-domain-model.md` §13 — the report
preserves it rather than defining a competing order.

Where a presentation ordering key is genuinely required and no upstream order
exists, it must be a stable canonical semantic identity or key.

## 8. PublicProfileSummary Contract

PublicProfileSummary may compress existing profile truth.

It may not create new claims.

Required invariant:

> Every semantic conclusion expressed by the summary must be traceable to a
> report section, and through it to profile and repository evidence.

Prohibited summary inflation includes:

- expert,
- strong engineer,
- senior,
- backend developer,
- highly skilled,
- good candidate.

Such statements are prohibited unless a future separate canonical contract
explicitly establishes that truth. Compressing existing evidence is
permitted; upgrading its epistemic strength while compressing it is not.

## 9. TechnicalFocus and EngineeringPractice Report Items

Typed report items must preserve enough structured information to explain the
profile claim, including where applicable:

- canonical pattern key/reference,
- localized display label,
- independent supporting repository count,
- supporting repository references,
- representative repository reference.

The canonical pattern key is identity. The localized display label is
presentation.

Therefore:

    localized label
    !=
    semantic truth

Two report items are the same item when their canonical pattern references
are equal, regardless of locale.

## 10. RepresentativeRepositories Report

Representative-repository presentation must preserve P7 semantics.

`RepresentativeRepository` means:

> the repository selected to represent a repeated profile pattern.

It does not mean:

- best repository,
- strongest repository,
- highest-quality repository,
- portfolio rank,
- repository ranking.

The report must not introduce those interpretations through labels, ordering,
emphasis or summary wording.

The report presents the P7 selection. It does not reselect or rerank it.

## 11. WhyThisConclusion

The report must preserve an evidence-first trace:

    Profile conclusion
            ↓
    Repeated pattern
            ↓
    Supporting repositories
            ↓
    Repository analysis
            ↓
    Concrete evidence

A reader must be able to understand why a conclusion exists.

Evidence must be visible. Internal algorithm and selection mechanics do not
have to be exposed in full; exposing the evidence chain is the requirement,
not exposing the implementation.

## 12. RepositoryDrillDown

RepositoryDrillDown connects profile truth back to existing repository-level
truth.

It must not duplicate complete RepositoryAnalysis into a second
profile-owned truth store.

It must preserve references sufficient to navigate:

    profile claim
    → repository
    → repository analysis
    → evidence

## 13. Warnings Presentation

`Warnings[]` is canonical profile truth
(`docs/architecture/public-profile-evidence-domain-model.md` §13).

It is not a mandatory main narrative report section, and it is deliberately
absent from the canonical section list in §5.

Warnings may be surfaced as analysis status, notice or progressive-disclosure
presentation.

Example: a `Partial` profile may display a concise notice that some public
repository evidence could not be evaluated.

The structured warning truth remains in `DeveloperPublicEvidenceProfile`
regardless of how, or whether, presentation surfaces it. Presentation choice
never deletes canonical truth.

## 14. Truth and Presentation Boundary

There is one canonical structured truth:

    DeveloperPublicEvidenceProfile

Developer View, Recruiter View, locale, UI, report format and optional
narration all consume that same truth.

    Developer View
            ↘
    DeveloperPublicEvidenceProfile
            ↗
    Recruiter View

They must not produce different semantic profiles.

A view may show less. It may not show something else.

## 15. LLM Boundary

Optional LLM use is permitted only after deterministic structured truth
exists:

    Evidence
    ↓
    Deterministic Structured Truth
    ↓
    optional LLM narration

This is prohibited:

    Evidence
    ↓
    LLM inference
    ↓
    Canonical Profile Truth

An LLM may narrate an existing report. It may not create, upgrade,
reinterpret or override profile truth, and it may not fill an evidence gap.

Therefore:

    LLM Narration
    !=
    Canonical Truth Generation

## 16. Prohibited Semantics

Report production must never emit:

- developer capability score,
- profile score,
- repository quality score,
- confidence score,
- candidate score,
- seniority,
- employability,
- hire/no-hire,
- candidate ranking,
- job fit,
- years-of-professional-experience inference,
- inferred role,
- authorship or originality inference,
- unsupported framework/category mapping.

The absence of public evidence must never be rendered as a negative developer
claim.

## 17. Determinism and Locale

For the same:

- `DeveloperPublicEvidenceProfile`,
- this report contract version,

the projected report must be semantically identical, including section set,
section availability states and ordering.

Locale may change only rendered text.

No system clock participates in projection.

Input enumeration order and execution order must not change the result.

## 18. Version Ownership

This contract introduces no new runtime rule-version identity.

The report produces no canonical truth, so it owns no evidence rule that
would require its own version. Report determinism derives entirely from the
consumed `DeveloperPublicEvidenceProfile` — which already carries
`AnalyzerVersion` and the individual upstream rule/policy versions — together
with this fixed contract.

A new version of this contract is required to change:

- the canonical section set,
- any section's semantic responsibility,
- the availability state set,
- the canonical section ordering,
- the summary contract,
- any prohibited-semantics boundary.

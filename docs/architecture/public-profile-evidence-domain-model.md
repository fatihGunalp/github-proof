# GitHub Proof — Public Profile Evidence Domain Model Contract

**Status:** CONTRACT LOCKED
**Contract Scope:** Public Profile Evidence V1 / P1
**Product Contract:** Public Profile Evidence V1 — Revision 2
**Repository Dependency:** RepositoryAnalysis preserved
**Downstream Consumer:** Job Evidence Alignment V1
**Session:** 056
**Date:** 2026-08-16
**Amended:** 2026-08-19 / Session 077 — RoleEvidenceAlignment
requirement/deferral alignment only (§12 optional/conditional production,
§14 Analyzer Version Ownership, §16 determinism wording, §18 P7 ownership);
no result-shape, invariant, ownership boundary or evidence semantics changed
beyond clarifying that RoleEvidenceAlignment is optional and currently
DEFERRED under the Session 076 decision
**Amendment History (cont.):**
- 2026-08-19 / Session 078 — P8-1 decision-lock clarification only (§13:
  locks the profile-level `AnalysisStatus` semantic values Complete/Partial/
  Failed, distinct from repository-level `RepositoryAnalysis.AnalysisStatus`,
  and locks that Limitation/Warning presence or count does not by itself
  determine Partial; §14 Result Completion Boundary: locks that an internal
  provenance/version-coherence failure produces AnalysisStatus.Failed rather
  than being normalized to Partial). No existing result shape, invariant,
  ownership boundary or prohibited-semantics rule was weakened, removed or
  reinterpreted; every addition is a direct transcription of a locked P8-1
  handoff decision
  (see `worklog/sessions/2026-08-19-078-p8-1-profile-composition-decision-lock.md`).
  Several other P8-1 exact-shape questions (SnapshotReference runtime
  representation, ProfileLimitation/ProfileWarning exact taxonomy,
  DeveloperPublicEvidenceProfile exact runtime shape, exact
  version-coherence validation algorithm) remain intentionally open and are
  not decided by this amendment.
- 2026-08-19 / Session 079 — P8-1 exact contract work. **This amendment does
  NOT close P8-1**; independent review established that four semantic
  decisions remain unresolved. In particular: the emission condition for
  `LimitedIndependentEvidenceSurface` and the canonical V1 `ProfileWarning`
  identity set are explicitly NOT locked (§13 "Deliberately Not Locked in
  V1"), the exact `SnapshotIdentity` input set remains partial (§6) and the
  version-lineage "compatible" definition remains partial (§14). Do not read
  the entries below as closing those. [Superseded by the Session 080 entry
  below: all four were subsequently locked and the "Deliberately Not Locked
  in V1" subsections this entry points at no longer exist.] Locks the exact
  semantics that were
  resolved: §6 `SnapshotReference`
  (ProfileIdentity / CapturedAt / deterministically derived SnapshotIdentity,
  prohibited derivation inputs, provenance-dimension separation, no snapshot
  duplication); §8 cross-reference to the final eligibility result consumed
  by composition; §13 structured `ProfileLimitation` / `ProfileWarning`
  identity, the canonical V1 limitation taxonomy, the warning promotion
  boundary, collection independence, ordering/deduplication, and the
  `AnalysisStatus` ownership boundary (a valid root carries `Complete` or
  `Partial` only; `Failed` belongs to the composition-result boundary); §14
  member semantics, extended root exclusions and the audit-timestamp
  boundary; §15 snapshot-traceability cross-reference. No existing invariant,
  ownership boundary, evidence rule or prohibited-semantics rule was weakened
  or removed. The Session 078 §13 text stating that `Failed` is a value of the
  root `AnalysisStatus` field is superseded by the §13 ownership boundary
  added here; the three completeness states themselves are unchanged. See
  `worklog/sessions/2026-08-19-079-p8-1-exact-contract-closure.md`.
- 2026-08-19 / Session 080 — P8-1 remaining semantic decisions. Closes three
  of the four gaps the Session 079 entry recorded as open: §6 locks that
  `CapturedAt` does not participate in `SnapshotIdentity` and that identity
  is derived from the canonical normalized snapshot semantic representation
  rather than a composer-owned field inventory; §13 locks the
  `LimitedIndependentEvidenceSurface` emission condition (below the
  already-locked P6 repetition minimum, with empty patterns explicitly not
  qualifying) and the canonical V1 `ProfileWarning` identity set
  (`RepositoryEvidenceUnavailable`, `RepositoryEvidenceRateLimited`,
  `RepositoryAnalysisFailed`, `RepositoryEligibilityUndetermined`); §14 locks
  what version-lineage "compatible" means (lineage coherence, not equal
  version strings) and restates the Failed-not-Partial boundary. The §13,
  §6 and §14 "Deliberately Not Locked in V1" notes are removed accordingly.
  No new threshold, identity or rule was invented beyond these locked
  decisions, and no existing invariant was weakened. **P8-1 is still not
  closed by this amendment.** See
  `worklog/sessions/2026-08-19-080-p8-1-remaining-semantic-decisions.md`.
- 2026-08-19 / Session 081 — P8-1 independent-review corrections. Adds the
  locked profile-level evidence-surface accounting rule: §13 defines
  "expected evidence surface" (supported + applicable obligations already
  imposed by the existing canonical owners; Unsupported/Deferred capabilities
  are not obligations), separates resolved deterministic terminal outcomes
  (`Analyze`/`Skip`/`Allowed`/`NotAllowed`/`NotApplicable`) from unresolved
  `Undetermined`, redefines Complete/Partial/Failed on that accounting, and
  locks `ProfilePatternSet.IsEvidenceSurfaceComplete` as a consumed upstream
  input (false → Partial; true alone does not establish Complete). §13 also
  adds the deterministic `ProfileWarning` identity mapping, the
  one-event-many-identities rule, and enumerated canonical ordering keys for
  `Limitations[]`/`Warnings[]`. §6 clarifies that the `CapturedAt` exclusion
  is by meaning rather than field name. §14 and §16 correct the statement
  that snapshot capture time is audit-only: it is a declared semantic input
  to temporal eligibility, so `SnapshotIdentity` plus rule versions alone is
  not a safe cache/idempotency key. No new capability, warning identity or
  limitation identity was created; no previously locked decision was
  reopened. **P8-1 is still not closed by this amendment.** See
  `worklog/sessions/2026-08-19-081-p8-1-review-corrections.md`.
- 2026-08-19 / Session 082 — P8-1 exit gate corrections (editorial precision
  only, no semantic change): §13 states that resolved/unresolved is a total
  partition of an evaluated obligation and that a future contract version
  introducing a new outcome value must declare which side it falls on; §13
  names the canonical repository comparer for limitation/warning ordering
  (`ProfileRepositoryReference`, Owner then Name, ordinal) and forbids
  substituting a presentation-layer or application-internal ordering; §13
  enumerates the V1 scope values (`Profile`, `Repository`), derived from the
  already-locked V1 identities without adding any identity or capability; §6
  clarifies that "inputs that can materially affect profile analysis" is a
  floor rather than a licence to hand-pick a subset. The P8-1 exit gate was
  then run and passed with no BLOCKER, HIGH or MEDIUM finding. **P8-1 is
  COMPLETE as of this amendment.** See
  `worklog/sessions/2026-08-19-082-p8-1-exit-gate-finalization.md`.

## 1. Purpose

This contract defines the canonical Domain semantics and boundaries for
Public Profile Evidence V1.

Its responsibility is to make profile-level public GitHub evidence
representable at codeable semantic precision without turning that evidence
into developer capability truth.

Production implementation is not defined by this document.

## 2. Analysis Boundary

Public Profile Evidence analyzes a normalized public GitHub evidence surface.

The analysis unit is:

    public GitHub profile evidence

It is not:

    Developer
    DeveloperCapabilityProfile
    Resume
    WorkExperienceProfile
    EmployabilityProfile

Therefore:

    GitHubProfileSnapshot != Developer

and:

    DeveloperPublicEvidenceProfile != DeveloperCapabilityProfile

Public GitHub evidence cannot establish the complete real-world capability,
career history or private work of a person.

## 3. Canonical Epistemic Model

Public Profile Evidence V1 distinguishes two canonical profile-level
epistemic layers:

    Observed Evidence
            ↓
    Repeated Pattern
            ↓
    DeveloperPublicEvidenceProfile

Observed evidence and repeated pattern are not interchangeable.

A quantified cross-repository observation does not automatically become a
repeated pattern.

Pattern promotion requires a separate deterministic and versioned rule.

## 4. Bounded Inference Boundary

EvidenceBackedProfileInference and ProfileInferenceAssessment are not part of
the canonical Public Profile Evidence V1 root.

They are deferred future extension concepts.

This deferral is not a prohibition.

If bounded inference is introduced later, it requires a separate deterministic
and versioned contract and must preserve all existing product safety
boundaries.

The V1 profile foundation must remain useful without converting repeated
public evidence into developer-experience or capability claims.

## 5. Job-Agnostic Boundary

DeveloperPublicEvidenceProfile is reusable and job-agnostic.

It must not contain:

- JobRequirementProfile,
- JobEvidenceAlignment,
- job-specific requirement importance,
- job-specific representative repository selection,
- job-specific evidence visibility improvement actions.

RoleEvidenceAlignment, when present, refers only to a generic versioned role
evidence definition.

Therefore:

    RoleEvidenceAlignment != JobEvidenceAlignment

## 6. Identity and Snapshot Layer

### GitHubProfileIdentity

GitHubProfileIdentity represents the stable public profile identity required
by profile analysis.

It must:

- be non-empty,
- support deterministic equality,
- remain independent from localized presentation,
- contain no capability or profile-pattern interpretation.

It must not model GitHub API DTO shape.

### GitHubProfileSnapshot

GitHubProfileSnapshot represents normalized observable public profile state at
a defined collection point.

It may contain:

- GitHubProfileIdentity,
- normalized public repository manifest,
- collection availability state,
- audit capture time,
- collection limitations required by downstream analysis.

It must not contain:

- developer capability,
- repeated-pattern interpretation,
- job-specific state,
- recruiter prose.

A snapshot is an observable evidence surface, not a person model.

### ProfileRepository

ProfileRepository represents one normalized repository manifest entry inside a
GitHubProfileSnapshot.

It carries only the repository context required for:

- deterministic ordering,
- eligibility assessment,
- RepositoryAnalysis orchestration,
- profile-level traceability.

It is not:

- RepositoryAnalysis,
- RepositorySnapshot duplication,
- an eligibility decision,
- a capability claim.

### SnapshotReference

SnapshotReference is the stable provenance link from a
DeveloperPublicEvidenceProfile to the exact GitHubProfileSnapshot that
composition consumed.

Every DeveloperPublicEvidenceProfile must carry one.

Its canonical semantic content is:

    SnapshotReference
    ├── ProfileIdentity
    ├── CapturedAt
    └── SnapshotIdentity

It answers exactly three questions: whose public profile data this was, when
that data was captured, and precisely which normalized snapshot produced the
profile.

The last two questions are answered by two different members and must not be
merged:

    CapturedAt
    =
    when was it observed?

    SnapshotIdentity
    =
    what canonical normalized semantic snapshot was observed?

#### SnapshotIdentity

SnapshotIdentity must be derived deterministically from the canonical
normalized GitHubProfileSnapshot semantic representation.

It must cover the normalized snapshot inputs that can materially affect
profile analysis, so that a materially different input surface yields a
different identity.

That is a floor, not a selection licence. It states the minimum the identity
must distinguish; it does not authorise a composer to pick which fields of
the canonical normalized snapshot representation qualify as "materially
affecting". Deciding that subset by hand is exactly the parallel field
inventory prohibited below.

SnapshotIdentity must not depend on:

- raw GitHub API ordering,
- collection insertion order,
- asynchronous completion order,
- locale,
- presentation text,
- UI state,
- current wall-clock time,
- random GUID generation,
- AnalyzerVersion,
- any downstream rule or policy version.

The purpose of SnapshotIdentity is deterministic input provenance. It is not
cryptographic attestation and not legal proof, and no contract may treat it
as either.

##### CapturedAt Does Not Participate

`CapturedAt` must not participate in SnapshotIdentity.

The two members answer different questions, and collapsing observation time
into semantic identity would make two observations of an unchanged public
profile look like two different snapshots.

Therefore:

    same normalized semantic snapshot
    observed at two different times
    =>
    same SnapshotIdentity
    different CapturedAt

`CapturedAt` remains a required member of SnapshotReference and remains
available to every consumer that needs observation time — including the
eligibility activity rules, which use `SnapshotCapturedAt` rather than a
system clock. This rule constrains identity derivation only.

The exclusion is by meaning, not by field name. Because identity is derived
from the canonical normalized snapshot semantic representation, and that
representation itself carries an audit capture time, the observation-time
value must be excluded there too. Any field expressing "when this snapshot was
observed" — whatever it is called on the snapshot, the reference or any
intermediate normalized form — is outside SnapshotIdentity.

Therefore:

    observation time, under any field name
    !=
    part of SnapshotIdentity

##### Canonical Snapshot Contract Owns the Semantic Content

SnapshotIdentity is derived from the canonical normalized
GitHubProfileSnapshot semantic representation that the snapshot contract
already owns.

Profile composition must not build a second, manually maintained field
inventory for identity derivation while that canonical semantic snapshot
content exists. A parallel inventory would drift from the snapshot contract
and would silently change identity semantics without a contract change.

Therefore:

    identity derivation input
    =
    canonical normalized snapshot semantic representation

    identity derivation input
    !=
    a hand-maintained field list owned by the composer

#### Provenance Dimension Separation

SnapshotIdentity is one provenance dimension. It does not absorb the others.

Profile provenance is explained jointly by:

    SnapshotIdentity
    +
    AnalyzerVersion
    +
    individual rule/policy versions

This separation is required so the system can distinguish:

    input changed
    !=
    analysis rules changed

A single combined identity that conflates observed input with the rules
applied to it would destroy that distinction and is therefore prohibited.

#### No Snapshot Duplication

SnapshotReference exists so that provenance does not require copying the
snapshot.

The complete GitHubProfileSnapshot must not be duplicated into
DeveloperPublicEvidenceProfile merely to provide provenance.

Therefore:

    stable snapshot provenance
    !=
    embedded snapshot copy

## 7. Reference Layer

Profile analysis must prefer references over embedding or recreating complete
repository-analysis objects.

### ProfileRepositoryReference

ProfileRepositoryReference identifies a repository within the profile evidence
surface.

It must support deterministic equality and ordering.

It contains no developer interpretation.

### RepositoryAnalysisReference

RepositoryAnalysisReference identifies the exact repository-level analysis
consumed by profile analysis.

Its purpose is:

    profile-level result
            ↓
    exact RepositoryAnalysis

The reference must carry enough stable identity to prevent a profile claim
from silently pointing to a different repository-analysis result.

It must not duplicate RepositoryAnalysis.

### ProfileEvidenceReference

ProfileEvidenceReference is the canonical traceability pointer from
profile-level evidence to repository-level evidence.

Required direction:

    profile observation
            ↓
    ProfileEvidenceReference
            ↓
    RepositoryAnalysisReference
            ↓
    repository-level evidence

It may reference stable repository-analysis evidence identities where required.

It must not:

- copy localized explanation text,
- recreate repository signal evaluation,
- manufacture a second repository evidence model.

## 8. Repository Eligibility Result

RepositoryEligibilityResult represents the profile-analysis result of applying
a versioned repository-eligibility policy to one ProfileRepository.

Its semantic purpose is limited to:

> Whether and how this repository may contribute to profile-level evidence.

RepositoryEligibilityResult is not:

- developer quality,
- repository quality,
- repository score,
- capability evidence by itself.

P1 requires the result to remain traceable to:

- ProfileRepositoryReference,
- eligibility policy/version identity,
- structured eligibility state/reasons defined by P2.

P1 does not define the eligibility algorithm.

The final eligibility result that profile composition consumes — its
purpose-specific decision surface, its deterministically derived overall
status and its required provenance — is locked by
`docs/architecture/public-profile-evidence-repository-eligibility.md`, which
also records that the narrower runtime `RepositoryContributionAssessment` is
not that final result. Composition consumes the final result; it does not
substitute the narrower one.

P2 owns:

- exact eligibility statuses,
- exact reason taxonomy,
- fork contribution rules,
- template contribution rules,
- archived/historical rules,
- activity rules,
- insufficient-evidence rules,
- independent-repository determination,
- eligibility policy versioning.

## 9. Observation Layer

### ObservedProfileEvidence

ObservedProfileEvidence represents one deterministic and quantified
profile-level observation derived from repository-level evidence.

Examples of semantic observations include:

    C#/.NET evidence appears in N eligible repositories.

    Automated-test evidence appears in N eligible repositories.

    CI evidence appears in N eligible repositories.

An observation may contain:

- stable observation identity/key,
- observation kind,
- quantified value,
- contributing repository references,
- supporting ProfileEvidenceReference values,
- producing rule/version identity.

It must not contain:

- developer capability language,
- practical-experience claims,
- seniority,
- employability,
- hiring judgment,
- localized recruiter prose.

ObservedProfileEvidence is evidence truth.

It is not automatically a repeated pattern.

### CrossRepositoryEvidence

CrossRepositoryEvidence is the canonical container for deterministic
cross-repository observations.

Conceptual shape:

    CrossRepositoryEvidence
    └── ObservedProfileEvidence[]

It answers:

> What quantified observations are visible across the eligible public
> repository evidence surface?

It does not answer:

> What does this prove about the person's capability?

It also does not automatically answer:

> Which observations qualify as repeated patterns?

Pattern promotion belongs to a separate deterministic and versioned rule.

Therefore:

    observation count
    !=
    repeated pattern

## 10. Pattern Layer

### RepeatedProfilePattern

RepeatedProfilePattern is the canonical profile-level pattern truth.

It represents a deterministic and versioned pattern promoted from one or more
ObservedProfileEvidence results.

Required traceability:

    RepeatedProfilePattern
            ↓
    ObservedProfileEvidence[]
            ↓
    ProfileEvidenceReference[]
            ↓
    RepositoryAnalysisReference[]
            ↓
    repository-level evidence

A repeated pattern may describe what is repeatedly visible in the public
portfolio.

It must not become:

- developer capability truth,
- mastery,
- seniority,
- employability,
- professional-experience duration,
- hiring judgment.

### Pattern Families

Public Profile Evidence V1 requires three bounded pattern families:

- TechnicalFocus
- EngineeringPractice
- ProblemArea

These families classify repeated public evidence.

They do not create separate independent copies of the same pattern truth.

Therefore the canonical ownership remains:

    RepeatedProfilePattern

TechnicalFocusArea, EngineeringPracticePattern and ProblemAreaPattern may be
implemented as typed semantic projections or compositions over the canonical
RepeatedProfilePattern contract.

They must not become parallel stores containing conflicting pattern truth.

### TechnicalFocus

TechnicalFocus represents repeated visible technical-area evidence.

It does not mean:

> The developer is proficient in this technology.

Exact technical taxonomy and promotion rules are deferred.

### EngineeringPractice

EngineeringPractice represents repeated visible engineering-practice evidence.

It does not mean:

> The developer is good at this engineering practice.

Exact taxonomy and promotion rules are deferred.

### ProblemArea

ProblemArea represents repeated visible evidence around a bounded
problem/application area.

It does not mean:

> The developer can generally solve this class of problem.

Exact taxonomy and promotion rules are deferred.

### Pattern Promotion Boundary

Observed evidence cannot silently promote itself into a pattern.

Pattern production requires a deterministic and versioned promotion rule.

P1 defines:

- valid pattern semantics,
- traceability requirements,
- allowed pattern-family boundary,
- root ownership.

Later profile phases define:

- exact taxonomy,
- exact repetition thresholds,
- exact promotion conditions,
- any pattern-strength semantics.

Therefore:

    repeated observation
    !=
    automatically accepted repeated pattern

## 11. Representative Repository

RepresentativeRepository is a profile-level explanation-support result.

It identifies a repository because that repository materially supports one or
more canonical RepeatedProfilePattern results.

Required semantic direction:

    RepresentativeRepository
            ↓
    RepeatedProfilePattern[]
            ↓
    profile observations
            ↓
    repository-level evidence

RepresentativeRepository must carry enough structured references to answer:

> Why is this repository representative of this public profile evidence?

It must not mean:

- best repository,
- highest-quality repository,
- strongest developer work,
- most popular repository,
- repository ranking.

Stars, forks or popularity cannot silently become developer-quality evidence.

Exact representative-repository selection rules belong to a later profile
phase.

## 12. Role Evidence Alignment

RoleEvidenceAlignment represents bounded overlap between canonical public
profile evidence and a generic, versioned technical-role evidence definition.

It may reference:

- RepeatedProfilePattern results,
- RepresentativeRepository results,
- structured limitations relevant to the mapping.

It remains job-agnostic.

Therefore:

    RoleEvidenceAlignment
    !=
    JobEvidenceAlignment

and:

    RoleEvidenceAlignment
    !=
    JobFit

RoleEvidenceAlignment must not produce:

- employability judgment,
- seniority assignment,
- candidate ranking,
- hire/no-hire,
- candidate score.

No downstream result may increase the epistemic strength of its supporting
profile evidence.

Role taxonomy and role-alignment production rules belong to a later profile
phase.

### Optional / Conditional Production

RoleEvidenceAlignment production is optional and conditional, not a
mandatory stage of profile composition.

A completed DeveloperPublicEvidenceProfile for V1 may validly contain zero
RoleEvidenceAlignment results. As of Session 076/077, RoleEvidenceAlignment
is explicitly DEFERRED — NOT_READY_WITH_CURRENT_EVIDENCE for Profile V1 /
Milestone B: the current canonical public evidence surface is insufficient
for a useful, safe and deterministic generic technical-role alignment
without semantic inflation
(`worklog/sessions/2026-08-19-076-role-evidence-alignment-feasibility.md`).
This deferral is not a permanent prohibition and may be reopened if the
canonical evidence surface later becomes sufficient.

Therefore:

    zero RoleEvidenceAlignment results
    !=
    negative developer evidence

    zero RoleEvidenceAlignment results
    !=
    incomplete or invalid DeveloperPublicEvidenceProfile

A composer (P8) may produce a valid DeveloperPublicEvidenceProfile without
producing any RoleEvidenceAlignment. Role definition and role-alignment rule
version identities are required only when this optional capability is
actually produced; their absence does not block composition.

## 13. Limitations and Warnings

ProfileLimitation and ProfileWarning are structured semantic results, not
free text. Their canonical identity and their equality, ordering and
deduplication are defined by stable semantic identity — never by a localized
message (see "Structured Identity, Ordering and Deduplication" below).

### ProfileLimitation

ProfileLimitation represents a known semantic boundary on what the
DeveloperPublicEvidenceProfile can support.

It answers:

> Given the evidence surface actually available to us, how far may this
> result safely be interpreted?

A ProfileLimitation describes the evidence surface.

It is not a negative developer conclusion.

#### Canonical V1 Limitation Taxonomy

The minimum canonical V1 ProfileLimitation identities are exactly:

- LimitedIndependentEvidenceSurface
- PartialEvidenceSurface

`LimitedIndependentEvidenceSurface` expresses that the independent evidence
surface available for profile-level repetition was narrow enough to bound how
far the result may be interpreted.

`PartialEvidenceSurface` expresses that part of the expected evidence surface
could not be evaluated, bounding interpretation of the produced result.

Additional limitation identities must not be introduced speculatively. A new
identity requires a demonstrated distinct semantic boundary that neither
existing identity already expresses, and it is a versioned contract change.

#### LimitedIndependentEvidenceSurface Emission Condition

Emit `LimitedIndependentEvidenceSurface` when the count of canonical
repositories whose `IndependentRepetition` contribution decision is `Allowed`
is below the canonical minimum repository count required for repetition
promotion.

That minimum is not a second threshold owned here. It is the repetition
threshold already locked by
`docs/architecture/public-profile-evidence-patterns.md` §5, which for the
current V1 `PatternRuleVersion` is 2. A separate limitation threshold must
not be introduced.

Therefore:

    0 or 1 repositories with IndependentRepetition = Allowed
    =>
    emit LimitedIndependentEvidenceSurface

    2 or more
    =>
    do not emit it on repository-count grounds

##### Empty Patterns Are Not This Limitation

When two or more repositories have `IndependentRepetition = Allowed` but no
pattern was promoted, `LimitedIndependentEvidenceSurface` must not be emitted
merely because the pattern collections are empty.

Therefore:

    empty pattern result
    !=
    limited independent evidence surface

An evidence surface that was wide enough to support repetition, but on which
no repetition happened to occur, is a real analytical result on an adequate
surface — not a bounded surface. Emitting the limitation there would convert
an ordinary empty result into an apparent evidence deficiency, which the
absence boundary in `public-profile-evidence-patterns.md` §6 already
prohibits.

`PartialEvidenceSurface` needs no separate emission rule: its condition
follows directly from the `Partial` definition below — part of the expected
evidence surface could not be evaluated.

#### Explicit Limitation Exclusions

The following are product capability states, not limitations and not
warnings:

- ProblemArea being unsupported in V1
  (`docs/architecture/public-profile-evidence-patterns.md` §9),
- RoleEvidenceAlignment being deferred in V1 (§12).

Neither may be emitted as a ProfileLimitation or a ProfileWarning. Doing so
would present a bounded product capability boundary as if it were a property
of the observed evidence surface.

An empty TechnicalFocus or EngineeringPractice result does not by itself
create a limitation.

Therefore:

    no promoted pattern
    !=
    limitation

    no promoted pattern
    !=
    no repository-level evidence

A limitation is emitted only when the evidence surface itself bounds safe
interpretation — not merely because a versioned promotion rule produced an
empty result on a fully evaluated surface.

### ProfileWarning

ProfileWarning represents a noteworthy analysis or data condition requiring
attention.

It answers:

> What unusual, unavailable, incomplete or uncertain processing/data
> condition occurred during this analysis?

A warning does not automatically invalidate the complete profile result.

ProfileWarning must not become capability, seniority or employability
judgment.

#### Canonical V1 Warning Taxonomy

The canonical V1 ProfileWarning identities are exactly:

- RepositoryEvidenceUnavailable
- RepositoryEvidenceRateLimited
- RepositoryAnalysisFailed
- RepositoryEligibilityUndetermined

No additional V1 warning identity may be introduced without a versioned
change to this contract.

Each identity carries the structured scope and affected canonical identities
required by "Structured Identity, Ordering and Deduplication" below, so that
a warning always states which repositories or canonical results it concerns.

None of these four identities is a statement about the developer. Each
describes a condition that occurred while collecting or evaluating evidence.

#### Deterministic Identity Mapping

Each identity has one canonical triggering condition. An implementation does
not choose between them by judgement.

##### RepositoryEvidenceRateLimited

Emit when evidence required by profile analysis could not be evaluated because
the canonical upstream condition is `RateLimited`.

##### RepositoryEvidenceUnavailable

Emit when required profile evidence could not be evaluated because of an
availability condition **other than** `RateLimited`.

This identity is not a generic duplicate of `RepositoryEvidenceRateLimited`.
When the upstream condition is specifically rate limiting, the rate-limited
identity is the correct one and this one must not be substituted for it.

##### RepositoryAnalysisFailed

Emit when a repository analysis that was required or attempted failed
technically, and that failure affects the expected evidence surface (see
"Expected Evidence Surface" below).

A technical failure is not evidence absence — this is the same boundary the
eligibility contract already draws
(`public-profile-evidence-repository-eligibility.md` §8 "Failed").

##### RepositoryEligibilityUndetermined

Emit when a material admission decision or a purpose-specific final
eligibility decision required by the profile remains `Undetermined`
(`public-profile-evidence-repository-eligibility.md` §6.4, §10, §31.2).

#### One Event May Produce Several Identities

A single underlying event may legitimately produce more than one warning
identity when those identities represent genuinely different semantic facts.

For example, a rate-limited collection that also leaves an eligibility
decision undecided produces both:

    RepositoryEvidenceRateLimited
    +
    RepositoryEligibilityUndetermined

because "the data could not be fetched" and "the eligibility question has no
answer" are different facts about the analysis.

This never licenses duplication. The deduplication rule in "Structured
Identity, Ordering and Deduplication" still applies in full: the same semantic
warning identity must not appear twice for the same canonical scope.

#### Warnings Describe Conditions, Not Completeness

Warnings state which conditions and reasons occurred. `AnalysisStatus` is
derived independently, from the evidence-surface accounting defined under
"AnalysisStatus" below.

Therefore:

    Warnings.Count > 0
    !=
    Partial

A profile may carry warnings and still be `Complete` when every supported and
applicable obligation was resolved.

#### Promotion Boundary

ProfileWarning is not a container for every repository-level warning
observed anywhere in the analysis.

A condition is promoted to profile-level ProfileWarning only when it
materially affects at least one of:

- profile analysis completeness,
- profile provenance,
- safe interpretation of the profile result.

Repository-level warnings that do not meet this threshold remain
repository-level truth, reachable through the repository analysis references
the profile already carries. They are not copied upward.

Therefore:

    repository-level warning
    !=
    automatically a ProfileWarning

### Limitation versus Warning

The distinction is:

    ProfileLimitation
    =
    boundary on what evidence truth can support

    ProfileWarning
    =
    noteworthy analysis/data condition requiring attention

Neither concept may be used as a hidden negative developer score.

#### Independent Collections

Limitations and Warnings are independent canonical collections.

There is no required one-to-one relation between them. A warning may exist
with no corresponding limitation, and a limitation may exist with no
corresponding warning.

Therefore:

    ProfileWarning
    !=
    cause of exactly one ProfileLimitation

Neither collection may be derived mechanically from the other.

### Structured Identity, Ordering and Deduplication

Every ProfileLimitation and ProfileWarning must carry stable structured
identity sufficient to determine equality without consulting presentation
text. That identity is composed of:

- a stable semantic code,
- the scope the condition applies to,
- the affected canonical identities, where the condition is scoped to
  specific repositories, patterns or other canonical results.

#### V1 Scope Values

The canonical V1 scope values are exactly:

    Profile
    Repository

This follows from the V1 identities already locked above, and introduces no
new capability: all four V1 ProfileWarning identities describe a condition
affecting one or more specific repositories and are therefore `Repository`
scoped, while both V1 ProfileLimitation identities describe a boundary on the
profile-level evidence surface as a whole and are therefore `Profile` scoped.

A `Repository` scoped entry must carry its affected repository identities. A
`Profile` scoped entry has no affected-identity list to carry, and its absence
is not a missing value.

Adding a scope value is a versioned change to this contract.

Localized message text is presentation produced from that structured
identity. It is never the source of truth, never the equality key and never
the ordering key.

Deduplication uses the structured identity. Two entries carrying the same
semantic code, scope and affected canonical identities are one condition,
regardless of how their localized text renders.

Canonical ordering of both collections must be deterministic and derived from
that same stable structured identity. Ordering must not depend on:

- localized display text,
- API response order,
- collection insertion order,
- asynchronous completion order,
- current wall-clock time,
- any hidden score or ranking.

#### Canonical Ordering Keys

Each collection is ordered independently, by these keys in this order:

1. semantic code, ordinal,
2. scope, ordinal,
3. affected canonical identities, each compared by its own canonical ordering
   — for repositories, the canonical `ProfileRepositoryReference` ordering,
   Owner then Name, ordinal, the same ordering the representative-repository
   contract uses for its tie-break
   (`public-profile-representative-repositories.md` §6.1); for any other
   affected result, that result's own canonical identity ordering —
   compared element by element in that canonical order.

No presentation-layer or application-internal ordering may be substituted
for the canonical ordering here.

Because deduplication already guarantees that no two entries in a collection
share the same semantic code, scope and affected canonical identities, these
three keys are jointly total: no tie can survive them, and no further
tie-breaker is required.

This mirrors the ordering discipline the pattern contract applies to pattern
collections (`public-profile-evidence-patterns.md` §12); it does not modify
that contract.

### Limitation and Warning Do Not Determine Completeness by Count

The presence of a ProfileLimitation does not, by itself, mean the profile
AnalysisStatus (see "AnalysisStatus" below) is Partial.

The presence of a ProfileWarning does not, by itself, mean AnalysisStatus is
Partial.

AnalysisStatus is determined only by the evidence-surface accounting defined
under "AnalysisStatus" below — whether every supported and applicable
obligation was resolved — not by how many limitations or warnings were
recorded alongside the result.

Therefore:

    limitation count
    !=
    completeness determination

    warning count
    !=
    completeness determination

    Warnings.Count > 0
    !=
    Partial

A sparse but fully resolved profile may carry limitations and warnings and
still be Complete. A profile with an unresolved obligation is Partial
regardless of how few or many warnings accompany it.

### AnalysisStatus (Profile-Level Analysis Completeness)

AnalysisStatus represents processing completeness, not evidence quantity.

It is distinct from repository-level `RepositoryAnalysis.AnalysisStatus`
(`docs/architecture/analysis-domain-model.md` §15), which describes one
repository's own analysis outcome and is a different, narrower-scoped result.

The canonical V1 profile-level analysis completeness semantics are:

    Complete
    Partial
    Failed

#### Expected Evidence Surface

The **expected evidence surface** is:

> the set of supported and applicable V1 profile-analysis obligations that the
> canonical contracts require to be evaluated for the current canonical
> repository universe.

Three words in that definition carry weight:

- **supported** — the capability exists in this product version;
- **applicable** — the obligation has semantic applicability for the
  repository or result being evaluated;
- **required by the canonical contracts** — the obligation is one the existing
  canonical owners already impose. This contract introduces no new capability
  list; the obligations are those already defined by the eligibility contract
  (`public-profile-evidence-repository-eligibility.md`), the pattern contract
  (`public-profile-evidence-patterns.md`) and the representative-repository
  contract (`public-profile-representative-repositories.md`).

##### Unsupported and Deferred Are Not Obligations

A capability that is Unsupported or Deferred in this product version is not an
expected evidence obligation, and its absence is therefore never a completeness
deficit.

Therefore:

    ProblemArea Unsupported in V1
    !=
    Partial

    RoleEvidenceAlignment Deferred in V1
    !=
    Partial

An obligation that is `NotApplicable` for a given repository is likewise not
an unmet obligation for that repository.

#### Evidence-Surface Accounting

Completeness is decided by accounting over that expected evidence surface, not
by counting limitations, warnings or results.

##### Resolved

An obligation is **resolved** when evaluating it produced a deterministic
terminal outcome. The deterministic terminal outcomes are:

    Analyze
    Skip
    Allowed
    NotAllowed
    NotApplicable

Resolved and unresolved are a total partition of an evaluated obligation:
every outcome that is not `Undetermined` is resolved. Should a future
contract version introduce a new outcome value, that version must state
which side of this partition it falls on; an implementation may not decide
it.

A negative outcome is a resolved outcome. `NotAllowed` means the contract
deterministically decided the repository may not contribute for that purpose —
it does not mean the obligation went unevaluated.

Therefore:

    negative contribution decision
    !=
    incomplete evidence

##### Unresolved

An obligation is **unresolved** when it should have been evaluated but the
required input was unavailable or insufficient, so the outcome remains
`Undetermined`.

`Undetermined` is a deterministic *record of a gap*, not a deterministic
answer to the question the obligation asks. The eligibility contract
deliberately preserves it rather than collapsing it into `NotAllowed`
(`public-profile-evidence-repository-eligibility.md` §10, §14.1, §31.2), and
profile-level accounting must honour that distinction rather than re-erasing
it.

Therefore:

    Undetermined
    !=
    evaluated successfully

    Undetermined
    !=
    automatically Failed

#### Complete

Every supported and applicable obligation in the expected evidence surface was
resolved.

A sparse profile — for example, one eligible repository, successfully
analyzed, producing no repeated pattern — is validly Complete, because
sparseness is a property of the evidence, not of the accounting.

Therefore:

    sparse evidence
    !=
    incomplete analysis

#### Partial

At least one supported and applicable obligation was unresolved, and the
remaining truth is still coherent enough to produce a valid
`DeveloperPublicEvidenceProfile`.

Examples: some eligible repositories could not be retrieved or analyzed; a
purpose-specific eligibility decision material to the profile remains
`Undetermined`; upstream pattern completeness reports an incomplete evidence
surface (see "Upstream Completeness" below).

#### Failed

Coherent canonical profile truth cannot be safely composed. This includes the
case where an internal semantic invariant or provenance/version/
repository-universe coherence chain is broken (§14 Result Completion Boundary,
§14 "What 'Compatible' Means"). A profile must not be forced into existence
over such an input set, and the outcome is Failed rather than a normalized
Partial.

Failed is about trustworthiness of composition, not about quantity of
evidence.

Therefore:

    Failed
    !=
    missing evidence

#### Human-Language Invariant

    "I checked and reached a deterministic answer"
    => resolved

    "I needed to check but could not determine the answer"
    => partial evidence surface

    "I cannot trust that these truth pieces belong together"
    => failed composition

#### Upstream Completeness

Profile composition must not silently recalculate canonical upstream
completeness. Where an upstream result already owns a canonical completeness
signal, that signal is consumed.

In particular, `ProfilePatternSet.IsEvidenceSurfaceComplete` — propagated from
`CrossRepositoryEvidence` by pattern promotion and carried onto the
representative-selection result
(`public-profile-representative-repositories.md` §9.1, §9.2, §12.1) — remains
a canonical input to profile completeness.

If that signal is `false` and a valid coherent profile can still be produced:

    AnalysisStatus
    =
    Partial

The converse does not hold. A `true` pattern-completeness signal alone does
not establish overall `Complete`: it speaks only for the evidence surface the
`ProfilePatternSet` was built from. Every other supported and applicable
profile evidence obligation must also be resolved before the profile is
`Complete`.

Therefore:

    IsEvidenceSurfaceComplete = true
    !=
    AnalysisStatus = Complete

### AnalysisStatus Ownership Boundary

The three completeness states above do not all belong to the same result.

The `AnalysisStatus` field carried by a valid `DeveloperPublicEvidenceProfile`
root (§14) is exactly:

    Complete
    Partial

`Failed` is not a state of a valid `DeveloperPublicEvidenceProfile`. A failed
composition does not produce a canonical profile root at all. `Failed` belongs
to the composition/orchestration result boundary that wraps composition:

    ProfileCompositionResult
    ├── Status = Complete | Partial | Failed
    └── Profile = present only when Status is Complete or Partial

Therefore this shape is prohibited:

    DeveloperPublicEvidenceProfile
    ├── AnalysisStatus = Failed
    └── partially valid canonical truth

A `DeveloperPublicEvidenceProfile` instance that exists is canonical truth
that may be safely consumed. There is no partially-valid profile root, and no
consumer may be required to check whether the root it received is real truth
or a failure placeholder.

This contract locks the ownership boundary and the state sets. It does not
prescribe the exact implementation type, naming or serialization shape of the
composition-result wrapper; that follows existing repository naming
conventions at implementation time.

## 14. DeveloperPublicEvidenceProfile Aggregate Root

DeveloperPublicEvidenceProfile is the immutable canonical result root of one
Public Profile Evidence analysis.

It represents:

    profile-level public evidence truth

It does not represent:

    Developer
    DeveloperCapabilityProfile
    Resume
    WorkExperienceProfile
    EmployabilityProfile

### Canonical Root Shape

Conceptual V1 shape:

    DeveloperPublicEvidenceProfile
    ├── ProfileIdentity
    ├── SnapshotReference
    ├── AnalysisStatus          // Complete | Partial only
    ├── AnalyzerVersion
    ├── RepositoryEligibilityResults[]
    ├── RepositoryAnalysisReferences[]
    ├── CrossRepositoryEvidence
    ├── RepeatedProfilePatterns[]
    ├── RepresentativeRepositories[]
    ├── RoleEvidenceAlignments[]
    ├── Limitations[]
    ├── Warnings[]
    ├── StartedAt
    └── CompletedAt

This shape defines semantic ownership.

It does not require every implementation type to embed complete child objects.

Reference-based composition should be preferred where it prevents duplication.

#### Member Semantics

- `ProfileIdentity` reuses the existing canonical profile identity model
  (§6). It is not redefined here.
- `SnapshotReference` follows §6 "SnapshotReference".
- `AnalysisStatus` is `Complete` or `Partial` only. `Failed` never appears on
  a valid root; see §13 "AnalysisStatus Ownership Boundary".
- `RepositoryEligibilityResults[]` carries the final eligibility truth
  defined by
  `docs/architecture/public-profile-evidence-repository-eligibility.md`, not
  a narrower intermediate assessment; see §8.
- `RepositoryAnalysisReferences[]` references existing repository-level
  truth. It does not duplicate RepositoryAnalysis into the profile aggregate.
- `CrossRepositoryEvidence` remains canonical because it is part of the
  evidence trace from repository analysis into profile patterns.
- `RepeatedProfilePatterns[]` is the one canonical store of repeated pattern
  truth; TechnicalFocus, EngineeringPractice and ProblemArea are
  deterministic typed projections over it, never parallel canonical stores.
- `RepresentativeRepositories[]` carries the already-determined P7 selection
  truth. Composition must not rerank or reselect it.
- `RoleEvidenceAlignments[]` remains structurally available and is validly
  empty in Profile V1 under the Session 076/077 deferral; see §12.
- `Limitations[]` and `Warnings[]` follow the structured contracts in §13.
- `StartedAt` and `CompletedAt` are audit data only.

### Root Ownership Rules

The root owns one canonical representation of each semantic truth.

It must not maintain conflicting duplicate stores for:

- repository analysis truth,
- observed profile evidence,
- repeated profile patterns,
- representative repository meaning,
- role evidence alignment.

RepositoryAnalysis remains repository-level atomic truth.

The profile root references RepositoryAnalysis; it does not recreate it.

CrossRepositoryEvidence owns the canonical observation collection.

RepeatedProfilePatterns owns the canonical pattern collection.

Typed technical-focus, engineering-practice and problem-area views must derive
from or compose that canonical pattern truth rather than becoming competing
truth stores.

### Root Exclusions

DeveloperPublicEvidenceProfile must not contain:

- EvidenceBackedProfileInference,
- ProfileInferenceAssessment,
- JobRequirementProfile,
- JobEvidenceAlignment,
- job-specific repository selection,
- job-specific evidence visibility actions,
- developer aggregate score,
- profile score,
- repository quality score,
- confidence score,
- candidate score,
- candidate quality,
- seniority,
- employability,
- hire/no-hire,
- job fit,
- candidate ranking,
- years-of-professional-experience inference,
- inferred role,
- unsupported framework/category mapping,
- localized labels,
- localized recruiter prose,
- generated summary text,
- UI state,
- LLM narration.

A failed composition must also be represented outside this root; see §13
"AnalysisStatus Ownership Boundary".

### Audit Timestamp Boundary

StartedAt and CompletedAt are audit data only.

They must not influence semantic analysis, and they must not participate in
deterministic semantic equality of the profile result. Two profile results are
not made semantically different merely by when composition started or
completed.

#### SnapshotCapturedAt Is Not Audit-Only

`CapturedAt` is excluded from `SnapshotIdentity` (§6), but it is **not** an
audit-only value at the profile level. It is a semantic input to temporal
eligibility: the eligibility contract anchors its activity cutoff on
`SnapshotCapturedAt`
(`public-profile-evidence-repository-eligibility.md` §19.2, §19.3.1).

Consequently, two observations carrying the same `SnapshotIdentity` and the
same rule versions may still produce different `ActivityClassification`
results, and therefore different `CurrentFocus` contribution decisions, when
their `CapturedAt` values fall on opposite sides of an activity cutoff.

Therefore:

    same SnapshotIdentity
    + same rule versions
    !=
    guaranteed identical profile result

##### Not a Safe Cache Key on Its Own

`SnapshotIdentity` alone must not be used as a cache or idempotency key for a
complete profile result. Any such key must also account for the temporal
inputs that participate in eligibility, at minimum `CapturedAt`.

This does not weaken determinism. Determinism is unchanged: the same
normalized inputs — including the same `CapturedAt` — plus the same rule
versions still produce the same canonical result. What this rule rejects is
the narrower and unsafe assumption that identity plus versions alone fully
determine the result.

### Result Completion Boundary

A completed DeveloperPublicEvidenceProfile must represent one coherent
analysis execution over one normalized profile evidence surface and one
identified set of semantic rule versions.

Results from different analysis executions or incompatible rule versions must
not be silently merged into one canonical root.

When composition detects such an incoherence — a repository analysis result
belonging to a different snapshot chain, an incompatible repository universe,
or a downstream result whose consumed rule/version lineage does not match its
upstream source — this is an internal provenance/version-coherence failure,
not a partial-evidence condition. It must not be normalized to Partial. A
profile must not be forced into existence over an incoherent input set; the
composition outcome is Failed and no valid profile root is produced at all
(see §13 "AnalysisStatus Ownership Boundary"). External evidence loss
(unavailable or unretrievable repository data) remains a Partial condition
and is unaffected by this rule.

### Analyzer Version Ownership

AnalyzerVersion identifies the versioned profile-analysis orchestration and
aggregate semantic contract that produced this DeveloperPublicEvidenceProfile.

AnalyzerVersion does not replace or subsume the individual version identities
owned by:

- repository eligibility policy,
- observation rules,
- pattern promotion rules,
- representative repository rules,
- generic role evidence definitions or alignment rules, when
  RoleEvidenceAlignment is actually produced (§12, §18 P7).

The profile result must make it possible to verify that it was produced from
one coherent and compatible set of rule versions.

AnalyzerVersion and the individual rule-version identities therefore have
distinct semantic ownership.

P1 locks this version ownership and coherence requirement without prescribing
the exact implementation type, serialization shape or storage representation.

#### What "Compatible" Means

Compatible does not mean that version strings are equal.

Different semantic layers legitimately carry different version numbers, and
requiring them to match would be a category error: `EligibilityPolicyVersion`,
`ObservationRuleVersion`, `PatternRuleVersion`,
`RepresentativeRepositoryRuleVersion` and `AnalyzerVersion` version different
things and advance independently.

Compatibility is a property of the analysis lineage, not of the version
literals. It requires all of:

1. **Same snapshot provenance chain.** Every composed result must trace to
   the same `SnapshotIdentity` (§6). Results derived from one snapshot must
   not be composed with results derived from another.
2. **Same canonical repository universe.** Snapshot, eligibility, repository
   analysis and downstream profile results must be explicable over one
   repository universe. A repository present in one layer and silently
   absent from another is incoherence, not an omission to tolerate.
3. **Explicit upstream/downstream version provenance.** Every downstream
   result must record the upstream rule/policy versions it actually
   consumed, so the lineage can be verified rather than assumed.
4. **No silent cross-run mixing.** Results produced by different analysis
   executions must not be merged into one root, even when their version
   literals happen to agree.
5. **Independently visible version dimensions.** Each version dimension
   remains separately inspectable. A single combined or flattened version
   token that hides which layer produced which result does not satisfy this
   requirement.

Therefore:

    equal version strings
    !=
    compatible lineage

    different version numbers across layers
    !=
    incompatible lineage

#### Failure Boundary

If the required snapshot provenance, repository-universe coherence or version
provenance cannot be proven, composition does not degrade gracefully.

    profile composition
    =
    Failed

    not
    Partial

This is the same boundary stated in §14 "Result Completion Boundary" and §13
"AnalysisStatus Ownership Boundary": unproven coherence is an internal
failure, and no valid profile root is produced. It must not be reported as an
evidence-availability condition.

## 15. Aggregate Invariants

DeveloperPublicEvidenceProfile must satisfy the following invariants.

### Identity

- ProfileIdentity must be present.
- SnapshotReference must identify the analyzed profile snapshot.
- Repository references must belong to that profile evidence surface.

### Snapshot Traceability

SnapshotReference must provide stable and deterministic traceability to the
exact normalized GitHubProfileSnapshot consumed by this profile analysis.

Its semantic identity and equality must be sufficient to distinguish that
snapshot from another normalized profile snapshot.

A DeveloperPublicEvidenceProfile must not silently rebind to or be interpreted
as having been produced from a different GitHubProfileSnapshot.

The canonical semantic content satisfying this invariant — ProfileIdentity,
CapturedAt and a deterministically derived SnapshotIdentity, together with the
prohibited derivation inputs and the provenance-dimension separation — is
defined once in §6 "SnapshotReference" and is not restated here.

This contract locks the traceability requirement and that semantic content
without prescribing the exact implementation type or storage representation.

### Repository Analysis Integrity

Every RepositoryAnalysisReference must identify an exact repository-level
analysis result.

Profile analysis must not silently substitute another RepositoryAnalysis result
for the same repository.

### Observation Integrity

Every ObservedProfileEvidence result must be traceable to at least one
ProfileEvidenceReference.

Observation values must be produced from normalized structured evidence.

Localized presentation text cannot be observation truth.

### Pattern Integrity

Every RepeatedProfilePattern must reference its supporting
ObservedProfileEvidence results.

A pattern cannot exist only because a label, README keyword or presentation
string exists.

Pattern promotion must identify the deterministic rule/version that produced
the pattern.

### Representative Repository Integrity

Every RepresentativeRepository must reference at least one canonical
RepeatedProfilePattern that explains its representative role.

Representative repository output must not create repository ranking semantics.

### Role Evidence Alignment Integrity

Every RoleEvidenceAlignment must be derived only from canonical
job-agnostic profile evidence and a versioned generic role evidence
definition.

It must not contain job-posting-specific truth.

### Limitation and Warning Integrity

ProfileLimitation and ProfileWarning values must describe evidence or analysis
conditions.

They cannot encode hidden capability, seniority, employability or hiring
judgments.

## 16. Determinism and Ordering

For the same:

- normalized GitHubProfileSnapshot,
- RepositoryAnalysis inputs,
- repository-eligibility policy version,
- observation rule versions,
- pattern rule versions,
- representative-repository rule versions,
- role evidence definition/rule versions, when RoleEvidenceAlignment is
  actually produced/applicable,

the canonical semantic result must be reproducible. When
RoleEvidenceAlignment is not produced (as in Profile V1 / Milestone B under
the Session 076/077 deferral), reproducibility of the profile result does not
depend on a role evidence definition/rule version, since none was applied.
This does not weaken determinism for any result that is produced: every
result that is produced, including RoleEvidenceAlignment if and when
produced, remains fully reproducible under its own rule versions.

### Deterministic Ordering

Collections exposed by canonical Domain results must use defined deterministic
ordering.

Ordering must not depend on:

- API response order,
- hash-map iteration order,
- localized labels,
- UI sorting state,
- current wall-clock time.

When a semantic ordering key is equal, a stable deterministic identity must be
used as the tie-breaker.

### Time Boundary

StartedAt and CompletedAt are audit metadata.

They may describe when analysis occurred.

They must not independently change evidence, pattern or alignment semantics.

No hidden current-time lookup may change a canonical result.

The snapshot capture timestamp is a different case and is not audit-only. It
is a declared semantic input to temporal eligibility
(`public-profile-evidence-repository-eligibility.md` §19.2, §19.3.1; §14
"SnapshotCapturedAt Is Not Audit-Only"). Its influence on a canonical result
is contracted and deterministic, not a hidden clock read, and it remains
excluded from `SnapshotIdentity` (§6).

### Locale Boundary

Canonical semantic truth is locale-independent.

Changing locale may change presentation only.

It must not rerun or alter:

- repository eligibility,
- observations,
- repeated patterns,
- representative repository selection,
- role evidence alignment.

## 17. Immutability

Canonical Domain result objects should be immutable after valid construction.

Required collections must not expose mutable internal state.

Construction must reject structurally invalid states where the invariant can
be enforced without requiring deferred production algorithms.

P1 contracts valid semantic shape and invariants.

It does not place deferred P2/P5/P6/P7 production thresholds inside
constructors.

## 18. Deferred Rule Ownership

P1 defines semantic result shape, ownership, traceability and invariants.

It does not define all production algorithms.

### P2 — Repository Eligibility

P2 owns:

- exact repository eligibility statuses,
- fork/template/archive/activity contribution rules,
- independent repository determination,
- eligibility reasons,
- eligibility policy versioning.

### P5 — Cross-Repository Evidence

P5 owns:

- deterministic observation aggregation,
- quantified cross-repository evidence production,
- repository/evidence contribution accounting,
- observation rule versioning.

P5 does not automatically promote observations into patterns.

### P6 — Pattern Production

P6 owns:

- technical-focus taxonomy,
- engineering-practice taxonomy,
- problem-area taxonomy,
- repeated-pattern promotion rules,
- pattern rule versioning,
- pattern-strength semantics if introduced.

P6 must not create developer capability truth.

These rules are locked by:

docs/architecture/public-profile-evidence-patterns.md

### P7 — Representative Repository and Role Evidence Alignment

P7 owns:

- representative repository selection rules — COMPLETE for P7 V1
  (`docs/architecture/public-profile-representative-repositories.md`),
- generic role taxonomy,
- generic role evidence definitions,
- role evidence alignment rules,
- related rule/version identities.

The last four items — generic role taxonomy, generic role evidence
definitions, role evidence alignment rules and their related rule/version
identities — are not required for P7 V1 completion. As of Session 076/077,
RoleEvidenceAlignment is explicitly DEFERRED — NOT_READY_WITH_CURRENT_EVIDENCE
for current Profile V1 / Milestone B: the current canonical public evidence
surface is insufficient for a useful, safe and deterministic generic
technical-role alignment without semantic inflation
(`worklog/sessions/2026-08-19-076-role-evidence-alignment-feasibility.md`).
This deferral is not a permanent prohibition; P7 V1 is
COMPLETE WITH EXPLICIT DEFERRAL on this basis.

If RoleEvidenceAlignment feasibility is reopened later, a separate
deterministic and versioned contract must define generic role taxonomy,
generic role evidence definitions and role evidence alignment rules before
any production implementation — exactly as P7 already required for the
representative-repository half. That future ownership remains P7's; it must
not be silently absorbed into Job Evidence Alignment, whose
`JobEvidenceAlignment` concept is job-specific by definition and remains
distinct from the generic, job-agnostic `RoleEvidenceAlignment` (§5, §12).

P7 must remain job-agnostic. This applies to RoleEvidenceAlignment if and
whenever it is reintroduced, exactly as it applies today.

Job-specific repository selection belongs to Job Evidence Alignment.

### Future Bounded Inference Extension

EvidenceBackedProfileInference and ProfileInferenceAssessment are deferred.

If introduced later, a separate deterministic contract must define:

- allowed inference kinds,
- allowed inference subjects,
- necessary and sufficient support conditions,
- versioning,
- traceability,
- false-positive safety boundaries.

No bounded inference production is authorized by P1.

## 19. Prohibited Semantics

Public Profile Evidence Domain must not produce or store:

- developer capability score,
- candidate score,
- seniority assignment,
- employability judgment,
- hire/no-hire,
- hiring recommendation,
- candidate ranking,
- years-of-professional-experience inference,
- mastery/expert assertion,
- authorship/originality inference,
- private-work assumptions,
- job fit,
- job suitability percentage.

The absence of public evidence must not become a negative developer claim.

Therefore:

    public evidence not found
    !=
    developer does not know the technology

and:

    sparse public GitHub
    !=
    weak developer

## 20. Framework and Provider Boundary

The profile Domain must not know:

- GitHub REST client implementation,
- HTTP,
- JSON serialization details,
- Entity Framework Core,
- PostgreSQL,
- ASP.NET Core,
- Blazor,
- Markdown rendering,
- LLM provider,
- localized UI copy,
- presentation state.

Provider-specific collection details belong outside Domain.

LLM may narrate canonical structured truth later.

LLM must not create, upgrade or override canonical profile evidence semantics.

## 21. Canonical Semantic Flow

The V1 semantic dependency direction is:

    GitHubProfileSnapshot
            ↓
    RepositoryEligibilityResult[]
            ↓
    RepositoryAnalysisReference[]
            ↓
    ObservedProfileEvidence[]
            ↓
    CrossRepositoryEvidence
            ↓
    RepeatedProfilePattern[]
            ↓
    RepresentativeRepository[]
            ↓
    RoleEvidenceAlignment[] (optional — may be empty)
            ↓
    DeveloperPublicEvidenceProfile

The arrows express semantic dependency, not a mandatory gate: when
RoleEvidenceAlignment is produced, it depends on RepresentativeRepository
and upstream pattern truth exactly in this order; when it is not produced
(as in Profile V1 / Milestone B under the Session 076/077 deferral), the
flow proceeds directly from RepresentativeRepository[] to
DeveloperPublicEvidenceProfile without it.

They do not require deep object embedding.

Reference-based composition is preferred where it preserves one canonical
truth and avoids duplication.

## 22. P1 Contract Result

This document defines the canonical candidate P1 domain model for:

    Public Profile Evidence V1

Current status:

    CONTRACT LOCKED

only after:

- cross-contract validation against Public Profile Evidence V1 Revision 2,
- validation against Job Evidence Alignment V1 boundaries,
- P1/P2 ownership validation,
- traceability validation,
- prohibited-semantics validation,
- deterministic/locale/versioning validation.

Production code remains out of scope until P1 and P2 are contract-locked.

# GitHub Proof — Public Profile Evidence Domain Model Contract

**Status:** CONTRACT LOCKED
**Contract Scope:** Public Profile Evidence V1 / P1
**Product Contract:** Public Profile Evidence V1 — Revision 2
**Repository Dependency:** RepositoryAnalysis preserved
**Downstream Consumer:** Job Evidence Alignment V1
**Session:** 056
**Date:** 2026-08-16

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

## 13. Limitations and Warnings

### ProfileLimitation

ProfileLimitation represents a known semantic boundary on what the
DeveloperPublicEvidenceProfile can support.

Examples may include:

- sparse eligible public evidence surface,
- insufficient repository coverage for a profile claim,
- unavailable required evidence,
- profile collection constraints.

A ProfileLimitation describes the evidence surface.

It is not a negative developer conclusion.

### ProfileWarning

ProfileWarning represents a noteworthy analysis or data condition requiring
attention.

It may identify conditions such as:

- incomplete collection,
- inconsistent source state,
- recoverable analysis concerns,
- conditions requiring human review.

A warning does not automatically invalidate the complete profile result.

ProfileWarning must not become capability, seniority or employability
judgment.

### Limitation versus Warning

The distinction is:

    ProfileLimitation
    =
    boundary on what evidence truth can support

    ProfileWarning
    =
    noteworthy analysis/data condition requiring attention

Neither concept may be used as a hidden negative developer score.

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
    ├── AnalysisStatus
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
- candidate score,
- seniority,
- employability,
- hire/no-hire,
- candidate ranking,
- years-of-professional-experience inference,
- localized recruiter prose.

### Result Completion Boundary

A completed DeveloperPublicEvidenceProfile must represent one coherent
analysis execution over one normalized profile evidence surface and one
identified set of semantic rule versions.

Results from different analysis executions or incompatible rule versions must
not be silently merged into one canonical root.

### Analyzer Version Ownership

AnalyzerVersion identifies the versioned profile-analysis orchestration and
aggregate semantic contract that produced this DeveloperPublicEvidenceProfile.

AnalyzerVersion does not replace or subsume the individual version identities
owned by:

- repository eligibility policy,
- observation rules,
- pattern promotion rules,
- representative repository rules,
- generic role evidence definitions or alignment rules.

The profile result must make it possible to verify that it was produced from
one coherent and compatible set of rule versions.

AnalyzerVersion and the individual rule-version identities therefore have
distinct semantic ownership.

P1 locks this version ownership and coherence requirement without prescribing
the exact implementation type, serialization shape or storage representation.

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

P1 locks this traceability requirement without prescribing the exact
implementation type or storage representation.

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
- role evidence definition/rule versions,

the canonical semantic result must be reproducible.

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

StartedAt, CompletedAt and snapshot capture timestamps are audit metadata.

They may describe when analysis occurred.

They must not independently change evidence, pattern or alignment semantics.

No hidden current-time lookup may change a canonical result.

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

### P7 — Representative Repository and Role Evidence Alignment

P7 owns:

- representative repository selection rules,
- generic role taxonomy,
- generic role evidence definitions,
- role evidence alignment rules,
- related rule/version identities.

P7 must remain job-agnostic.

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
    RoleEvidenceAlignment[]
            ↓
    DeveloperPublicEvidenceProfile

The arrows express semantic dependency.

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

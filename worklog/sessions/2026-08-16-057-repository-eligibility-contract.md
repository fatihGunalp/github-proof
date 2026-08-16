# Session 057 — Repository Eligibility Contract

**Date:** 2026-08-16
**Result:** DOCS
**Profile Phase:** P2 — Repository Eligibility Contract
**Roadmap Stage:** Stage 6 — Profile P2
**Scope:** domain/product contract / eligibility semantics / docs
**Base HEAD:** 4c93400
**Execution:** ChatGPT-guided / macOS Terminal / manual
**Implementation Agent:** NONE

## Goal

Contract-lock the Public Profile Evidence V1 repository eligibility semantics
before profile-level production implementation begins.

P2 must define which public repositories may contribute to which
profile-level evidence operations and why.

Production code is not in scope.

## Canonical Parent

docs/architecture/public-profile-evidence-domain-model.md

Status:
CONTRACT LOCKED

Profile Phase:
P1 — COMPLETE

## Core Question

Repository eligibility answers:

> Whether and how this repository may contribute to profile-level evidence.

It does not answer:

> Is this a good repository?

or:

> Is this a good developer?

## Required Repository Conditions

P2 must explicitly contract at least:

- original repository,
- fork,
- template,
- archived repository,
- empty repository,
- insufficient-evidence repository,
- active repository,
- historical repository,
- independent repository semantics.

## Non-Negotiable Boundaries

RepositoryEligibilityResult != repository quality

RepositoryEligibilityResult != developer quality

Eligibility must not become:

- developer score
- capability score
- seniority
- employability
- hire/no-hire
- candidate ranking

Forks/templates must not inflate repeated evidence.

Historical/archived repositories may remain evidence without silently becoming
current-practice claims.

## Next

Step 6.1 — extract and classify exact eligibility dimensions, statuses,
reasons and contribution semantics before drafting the canonical P2 contract.

## Pre-P2 Bounded P1 Clarification

Before P2 semantic work begins, the locked P1 contract received one bounded
clarification.

Canonical document:

docs/architecture/public-profile-evidence-domain-model.md

### SnapshotReference

Clarified semantic ownership:

- SnapshotReference provides stable/deterministic traceability to the exact
  normalized GitHubProfileSnapshot consumed by profile analysis.
- profile result cannot silently rebind to another snapshot.
- semantic identity/equality is required.
- exact implementation/storage shape remains unlocked.

### AnalyzerVersion

Clarified semantic ownership:

- AnalyzerVersion versions profile-analysis orchestration / aggregate semantic
  contract.
- it does not replace individual eligibility, observation, pattern,
  representative-repository or role rule versions.
- one DeveloperPublicEvidenceProfile must be verifiably produced from one
  coherent and compatible rule-version set.
- exact implementation/serialization/storage shape remains unlocked.

### Preserved P1 Decisions

UNCHANGED:

- bounded inference remains DEFERRED,
- DeveloperPublicEvidenceProfile remains job-agnostic,
- RoleEvidenceAlignment != JobEvidenceAlignment,
- no developer/candidate score,
- no seniority,
- no employability,
- no hire/no-hire,
- no candidate ranking.

### Validation

Result:

PASS

Production code/tests:

UNCHANGED

P1 status:

CONTRACT LOCKED

P2 semantic work:

NOT STARTED YET

## Pre-P2 Clarification Remote Verification

Commit:

a4e5f9321b238318b5db6ed52826712f4a25d929

Parent:

4c934006556bfa9a4caf7a802f9d5270cea512b6

Independent GitHub remote verification:

PASS

Verified:

- exact four-file checkpoint scope,
- P1 canonical delta is additive +37 / -0,
- SnapshotReference exact normalized snapshot traceability is present,
- AnalyzerVersion and individual rule-version ownership remain distinct,
- bounded inference remains DEFERRED,
- DeveloperPublicEvidenceProfile remains job-agnostic,
- RoleEvidenceAlignment != JobEvidenceAlignment,
- prohibited score/seniority/employability/hiring semantics remain excluded.

P2 semantic work may now begin.

## Step 6.1 — Repository Eligibility Semantic Extraction

### Core Decision

Repository conditions and repository eligibility outcome are different
semantic concepts.

The following values must NOT become one mutually-exclusive flat enum:

- Original
- Fork
- Template
- Archived
- Empty
- InsufficientEvidence
- Active
- Historical

A repository may legitimately satisfy more than one condition at the same
time.

Example:

Original
+
Archived
+
Historical

is semantically valid.

### A. Provenance / Origin Dimension

Describes where the repository comes from.

Candidate conditions:

- Original
- Fork
- TemplateDerived

This dimension does not itself determine overall eligibility.

Fork or template-derived state may restrict contribution semantics but must
not silently create authorship/originality conclusions.

### B. Lifecycle Dimension

Describes temporal/public lifecycle context.

Candidate conditions:

- Active
- Historical
- Archived

Archived is not equivalent to invalid evidence.

Historical evidence may remain usable while being inappropriate for
current-focus claims unless later rules explicitly allow that contribution.

Exact lifecycle thresholds remain a P2 rule decision.

### C. Evidence-Surface Dimension

Describes whether the repository exposes enough observable material for
profile-level evidence contribution.

Candidate conditions:

- Empty
- InsufficientEvidence
- EvidenceAvailable

Important:

InsufficientEvidence
!=
Missing

Missing remains repository-level evidence/signal semantics.

InsufficientEvidence is a profile-level repository contribution assessment.

Neither state is negative developer evidence.

### D. Independence Dimension

Profile repetition requires explicit independence semantics.

P2 must determine whether a repository contribution is:

- independent for repetition purposes,
- non-independent for repetition purposes,
- or not reliably determinable.

Exact enum names are not yet locked.

Critical invariant:

Fork/template relationships must not inflate independent repetition counts.

Independence is about contribution counting.

It is not authorship truth.

### E. Contribution Semantics

Eligibility must answer more than:

eligible / not eligible

The result must express whether and how a repository may contribute.

At minimum P2 must distinguish contribution capability for:

- profile context,
- quantified observations,
- independent repetition,
- current-focus evidence,
- historical evidence.

A repository may therefore be usable for one contribution purpose while
restricted for another.

Example semantic direction:

Archived historical repository
→ may contribute historical evidence
→ may be restricted from current-focus contribution

Fork
→ may remain visible profile context
→ must not automatically increase independent repetition

### F. Eligibility Assessment versus Repository Facts

Repository facts/conditions are inputs.

RepositoryEligibilityResult is the deterministic policy result produced from
those inputs.

Required conceptual direction:

ProfileRepository
+
normalized repository conditions/evidence
+
EligibilityPolicyVersion
↓
RepositoryEligibilityResult

RepositoryEligibilityResult must expose:

- repository reference,
- evaluated condition facts needed for explanation,
- contribution permissions/restrictions,
- structured reason(s),
- eligibility policy version.

Exact implementation shape is not locked by Step 6.1.

### G. Reason Semantics

Eligibility reasons explain why a contribution is permitted, restricted or
excluded.

Reasons must be:

- deterministic,
- structured,
- locale-independent,
- presentation-neutral,
- traceable to evaluated repository conditions.

Reason text is not canonical truth.

Localized explanation belongs to presentation.

### H. Safety Boundaries

RepositoryEligibilityResult
!=
repository quality

RepositoryEligibilityResult
!=
developer quality

Eligibility cannot produce:

- developer score,
- capability score,
- seniority,
- employability,
- hire/no-hire,
- candidate ranking,
- authorship/originality inference.

Public evidence absence or restriction cannot become a negative developer
claim.

## Step 6.1 Result

PASS

Key decision:

Repository eligibility is a multi-dimensional contribution policy result,
not a flat repository-type enum.

Next:

Step 6.1B — validate dimension separation, Missing boundary, independence
semantics and contribution-purpose model.

## Step 6.2 — Eligibility Status / Contribution / Reason Model

### 1. Purpose-Specific Contribution Decision

Eligibility is evaluated per contribution purpose.

Canonical contribution decision semantics:

- Allowed
- NotAllowed
- NotApplicable
- Undetermined

Definitions:

Allowed
=
the repository may contribute for that exact purpose under the evaluated
eligibility policy.

NotAllowed
=
the purpose is applicable, but deterministic policy prevents this repository
from contributing for that purpose.

NotApplicable
=
the contribution purpose has no semantic applicability for the evaluated
repository condition.

Undetermined
=
required observable input is unavailable or insufficient to make that exact
policy decision reliably.

Undetermined must not silently become Allowed or NotAllowed.

### 2. Canonical Contribution Purposes

P2 locks these semantic purposes:

#### ProfileContext

Whether the repository may remain visible as part of the public profile
evidence surface.

ProfileContext is not itself evidence for a profile-level claim.

#### ProfileObservation

Whether repository-level structured evidence may contribute to quantified
profile observations.

#### IndependentRepetition

Whether the repository may count as an independent supporting repository when
a repetition rule is evaluated.

IndependentRepetition does not assert authorship.

#### CurrentFocus

Whether the repository may support a claim about currently visible technical
focus under the versioned lifecycle/activity policy.

#### HistoricalEvidence

Whether the repository may support historical public-evidence context.

### 3. Overall Eligibility Status

RepositoryEligibilityResult exposes one derived overall eligibility status.

Canonical statuses:

- Contributing
- Restricted
- NonContributing
- Undetermined

The overall status is a deterministic summary of purpose-specific contribution
decisions.

It is not a second independent source of truth.

#### Contributing

At least one evidence-bearing contribution purpose is Allowed and no applicable
evidence-bearing purpose is policy-restricted or undetermined.

NotApplicable purposes do not prevent Contributing.

#### Restricted

At least one evidence-bearing contribution purpose is Allowed, while at least
one other applicable evidence-bearing purpose is NotAllowed.

Example direction:

Archived historical repository
→ HistoricalEvidence may be Allowed
→ CurrentFocus may be NotAllowed
→ overall status may therefore be Restricted

#### NonContributing

No evidence-bearing contribution purpose is Allowed and all required decisions
are determinable.

ProfileContext may still be Allowed.

Therefore:

ProfileContext Allowed
+
all evidence-bearing purposes NotAllowed/NotApplicable
→ NonContributing

This is not negative developer evidence.

#### Undetermined

Required observable repository facts are unavailable such that a reliable
canonical eligibility result cannot be completed.

Undetermined is an evidence limitation.

It is not repository quality or developer quality.

### 4. Evidence-Bearing versus Context-Only Purpose

Evidence-bearing purposes are:

- ProfileObservation
- IndependentRepetition
- CurrentFocus
- HistoricalEvidence

ProfileContext is context-only.

Therefore profile visibility cannot accidentally promote a repository into
profile evidence.

### 5. Structured Eligibility Reasons

Eligibility decisions require structured reason identities.

Reasons attach to:

- the overall RepositoryEligibilityResult where relevant,
- and/or the exact purpose-specific contribution decision they explain.

Reason identities must be:

- deterministic,
- version-compatible,
- locale-independent,
- presentation-neutral,
- explainable from observable repository conditions.

Canonical reason families:

- Provenance
- Lifecycle
- EvidenceSurface
- Independence
- InputAvailability
- Policy

Examples of reason semantics that P2 must support:

- ForkDetected
- TemplateDerived
- Archived
- Historical
- EmptyRepository
- InsufficientEvidence
- NonIndependentContribution
- IndependenceUndetermined
- RequiredInputUnavailable

Exact final reason-code set will be frozen in the canonical P2 contract.

### 6. Reason versus Condition

A repository condition is an observed/normalized fact.

An eligibility reason explains a policy decision derived from facts.

Therefore:

Fork
!=
ForkDetected reason

Archived
!=
Archived reason

The first belongs to repository condition input.

The second belongs to eligibility decision explanation.

### 7. Missing Boundary

Repository-level signal:

Missing

must never be converted directly into:

RepositoryEligibilityStatus.NonContributing

A repository can contain one or more Missing repository signals while still
providing sufficient evidence for other profile-level contribution purposes.

InsufficientEvidence is assessed at the repository/profile contribution
boundary under P2 policy.

### 8. Deterministic Derivation

Required semantic direction:

Repository conditions
+
observable repository evidence availability
+
EligibilityPolicyVersion
↓
purpose-specific ContributionDecision[]
↓
structured EligibilityReason[]
↓
derived RepositoryEligibilityStatus

No localized prose participates in this decision.

No LLM participates in this decision.

### Step 6.2 Result

PASS

Locked semantic model:

RepositoryEligibilityResult
=
derived overall status
+
purpose-specific contribution decisions
+
structured reasons
+
policy version
+
traceability

Exact implementation type shape remains deferred to implementation.

Next:

Step 6.2B — validate status derivation, context/evidence separation,
Undetermined semantics and reason ownership.

### 9. Overall Status Deterministic Precedence

Overall RepositoryEligibilityStatus is derived only from evidence-bearing
contribution purposes.

ProfileContext does not participate in overall status derivation.

Canonical precedence:

1. Undetermined
2. Restricted
3. Contributing
4. NonContributing

#### Rule 1 — Undetermined

If any applicable evidence-bearing contribution decision is Undetermined:

overall status
=
Undetermined

This rule has highest precedence.

Therefore:

Allowed
+
Undetermined
→
Undetermined

and:

Allowed
+
NotAllowed
+
Undetermined
→
Undetermined

The purpose-specific decisions remain available, so known Allowed or
NotAllowed facts are not lost.

The overall summary must not hide unresolved evidence limitations.

#### Rule 2 — Restricted

If no evidence-bearing decision is Undetermined,
at least one is Allowed,
and at least one applicable evidence-bearing decision is NotAllowed:

overall status
=
Restricted

#### Rule 3 — Contributing

If no evidence-bearing decision is Undetermined,
at least one evidence-bearing decision is Allowed,
and no applicable evidence-bearing decision is NotAllowed:

overall status
=
Contributing

NotApplicable decisions are ignored for this derivation.

#### Rule 4 — NonContributing

If no evidence-bearing decision is Undetermined
and no evidence-bearing decision is Allowed:

overall status
=
NonContributing

This includes cases where evidence-bearing purposes are deterministically
NotAllowed and/or NotApplicable.

NonContributing remains a repository contribution result only.

It is not negative developer evidence.

### Determinism Check

For one fixed set of purpose-specific contribution decisions, exactly one
overall RepositoryEligibilityStatus must be derivable.

No status tie or implicit fallback is permitted.

## Step 6.2C — Analysis Admission versus Final Eligibility Boundary

### Problem

ProfileRepository exists before RepositoryAnalysis.

Therefore P2 cannot require repository-analysis evidence to decide whether the
repository analyzer should run.

At the same time, InsufficientEvidence cannot be inferred reliably from one or
more repository-level Missing signal results.

### Canonical Two-Stage Policy Evaluation

P2 uses two deterministic evaluation stages under the same
EligibilityPolicyVersion.

#### Stage A — Analysis Admission

Input:

ProfileRepository
+
normalized manifest conditions
+
EligibilityPolicyVersion

Output:

AnalysisAdmissionDecision

Canonical decisions:

- Analyze
- Skip
- Undetermined

AnalysisAdmissionDecision answers only:

> Should repository analysis be attempted for this profile repository?

It is not RepositoryEligibilityResult.

It does not determine developer quality, repository quality or final
profile-evidence contribution.

### Admission Rules

#### Empty Repository

If normalized ProfileRepository state deterministically identifies the
repository as empty:

AnalysisAdmissionDecision
=
Skip

Reason:
EmptyRepository

The final repository eligibility result may then be completed without running
RepositoryAnalysis.

#### Non-Empty Repository

Original, fork, template-derived, archived, active or historical state does not
by itself require Skip.

If the normalized repository manifest contains the required information for
repository analysis orchestration:

AnalysisAdmissionDecision
=
Analyze

Fork/template/archive restrictions belong to final contribution decisions.

#### Required Admission Input Unavailable

If required normalized manifest input is unavailable and admission cannot be
decided reliably:

AnalysisAdmissionDecision
=
Undetermined

No implicit Analyze or Skip fallback is permitted.

### Stage B — Final Repository Eligibility

Final RepositoryEligibilityResult may consume:

- ProfileRepositoryReference,
- normalized repository conditions,
- AnalysisAdmissionDecision,
- RepositoryAnalysisReference when analysis was performed,
- repository AnalysisStatus,
- EligibilityPolicyVersion.

Required direction:

ProfileRepository
↓
AnalysisAdmissionDecision
↓
RepositoryAnalysis when Analyze
↓
final purpose-specific contribution decisions
↓
RepositoryEligibilityResult

### RepositoryAnalysis Status Mapping Boundary

Existing repository-level AnalysisStatus semantics are preserved.

#### Completed

Repository analysis is available for downstream profile evidence evaluation.

Completed does not mean every signal is present.

Individual Missing signals remain repository-level signal truth.

#### Partial

Meaningful partial RepositoryAnalysis exists.

Partial analysis may contribute only through evidence actually available.

Unavailable portions cannot be inferred.

Partial does not automatically mean InsufficientEvidence.

#### InsufficientData

RepositoryAnalysis could not produce enough evidence for a reliable repository
analysis.

At the P2 boundary:

AnalysisStatus.InsufficientData
→
EvidenceSurface condition:
InsufficientEvidence

This is profile contribution limitation only.

It is not negative developer evidence.

#### Failed

Technical failure is not InsufficientEvidence.

At the P2 boundary:

AnalysisStatus.Failed
→
required evidence-bearing contribution decisions:
Undetermined

Reason family:
InputAvailability

A technical failure must not be converted into evidence absence.

### Missing Signal Boundary

SignalStatus.Missing may occur inside an otherwise Completed or Partial
RepositoryAnalysis.

Therefore:

SignalStatus.Missing
!=
InsufficientEvidence

and:

SignalStatus.Missing
!=
RepositoryEligibilityStatus.NonContributing

P2 must never count Missing signal quantity as an implicit repository
eligibility threshold.

### Empty versus InsufficientEvidence

Empty is known from normalized repository condition input before analysis.

InsufficientEvidence is a final evidence-surface condition established through
the repository-analysis result boundary.

Therefore:

Empty
!=
InsufficientEvidence

although both may result in NonContributing for evidence-bearing profile
purposes.

Their reasons and traceability remain distinct.

### Canonical Flow

GitHubProfileSnapshot
↓
ProfileRepository[]
↓
AnalysisAdmissionDecision[]
↓
Analyze candidates only
↓
RepositoryAnalysis[]
↓
RepositoryEligibilityResult[]
↓
profile observations / repetition / patterns

### Ownership

AnalysisAdmissionDecision is a P2 policy intermediate.

RepositoryEligibilityResult remains the canonical P1 profile-level repository
eligibility result.

P2 does not alter RepositoryAnalysis or repository-level SignalStatus
semantics.

### Step 6.2C Result

PASS

Dependency-order conflict resolved without redefining RepositoryAnalysis.

Next:

Step 6.3 — draft the canonical P2 repository eligibility contract.


## Final Result

Result:

DOCS

Canonical contract:

docs/architecture/public-profile-evidence-repository-eligibility.md

Status:

CONTRACT LOCKED

Version:

1

Stage 6 semantic result:

COMPLETE

Locked decisions include:

- repository conditions are multi-dimensional, not one flat type enum,
- analysis admission and final eligibility are separate deterministic stages,
- Missing != InsufficientEvidence != technical failure,
- contribution is purpose-specific,
- overall eligibility status is deterministically derived,
- fork/template lineage cannot inflate independent repetition,
- archived/historical evidence remains usable within bounded purposes,
- current-focus classification is owned by EligibilityPolicyVersion,
- no hidden system clock or unversioned activity threshold,
- V1 reason identities are closed and version-owned,
- eligibility cannot increase evidence epistemic strength,
- eligibility remains job-agnostic,
- no developer score, seniority, employability, hiring, ranking or authorship
  inference semantics.

Production code/tests:

UNCHANGED

Final locked-contract validation:

PASS

Next:

session-final git validation → commit → push → independent GitHub remote
verification.

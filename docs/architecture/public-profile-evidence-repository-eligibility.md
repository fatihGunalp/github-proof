# Public Profile Evidence V1 — Repository Eligibility Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Profile Phase:** P2 — Repository Eligibility Contract
**Roadmap Stage:** Stage 6 — Profile P2
**Locked:** 2026-08-16 / Session 057
**Amended:** 2026-08-19 / Session 079 — added §31 locking which eligibility
result profile composition (P8) consumes, that Undetermined must survive to
composition, that the derived overall status is not a second truth, the
required provenance, and that the narrower runtime
`RepositoryContributionAssessment` is not the final result. No eligibility
rule, status value, contribution purpose, reason identity, policy semantic or
derivation rule was added, removed or changed
**Amended:** 2026-08-19 / Session 080 — added §19.3.1 locking the Profile V1
`CurrentActivityWindow` at exactly 365 days as a recency boundary (repository
age irrelevant), applied through the unchanged §19.2 activity reference, and
updated the §31.5 prerequisite note accordingly. No other lifecycle,
independence, provenance, contribution or derivation rule changed; §19.3
policy-version ownership is unchanged and still requires a new
`EligibilityPolicyVersion` to alter the value
**Amended:** 2026-08-21 / Session 098 — **Profile V1 eligibility semantics
change.** Fork lineage now requires `AnalysisAdmissionDecision = Skip` with
reason `ForkDetected` (§6.3, new §19.3.2, new §22.1.1, §25 Fork and the §28
"Fork + Active + EvidenceAvailable" example). A known fork
therefore contributes no `ProfileObservation`, no `IndependentRepetition`, no
`CurrentFocus` and no `HistoricalEvidence`, and its overall status is
`NonContributing`. This is a product/eligibility decision, **not** a
performance optimization: Profile V1 has no attribution contract able to prove
which fork changes belong to the profile owner, so a fork must not be used as
developer evidence. It requires a new `EligibilityPolicyVersion` under the
unchanged §19.3 ownership rule. `TemplateDerived`, archive, activity,
independence, reason identities, status derivation and every other rule are
unchanged
**Production Code:** NOT IN SCOPE

## 1. Purpose

This contract defines deterministically:

> Whether and how one public repository may contribute to profile-level
> evidence.

It refines the RepositoryEligibilityResult boundary locked by:

- docs/product/public-profile-evidence-product-contract.md
- docs/architecture/public-profile-evidence-domain-model.md
- docs/roadmap/mvp-phases.md

Repository eligibility is contribution policy.

Its canonical semantic question is:

> Whether and how this repository may contribute to profile-level evidence.

RepositoryEligibilityResult remains the canonical P1 profile-level repository
eligibility result.

P2 refines its statuses, reasons, contribution rules and deterministic policy.
It does not replace the P1 ownership boundary.

It is not repository quality.

It is not developer quality.

## 2. Canonical Boundary

RepositoryEligibilityResult
!=
RepositoryAnalysis

RepositoryEligibilityResult
!=
repository score

RepositoryEligibilityResult
!=
developer capability

RepositoryEligibilityResult
!=
developer score

RepositoryEligibilityResult
!=
seniority

RepositoryEligibilityResult
!=
employability

RepositoryEligibilityResult
!=
hire/no-hire

RepositoryEligibilityResult
!=
candidate ranking

RepositoryEligibilityResult
!=
authorship/originality inference

A restriction or absence of public evidence must not become a negative
developer claim.

## 3. Repository Conditions versus Eligibility Result

Repository conditions are normalized observable inputs.

RepositoryEligibilityResult is the versioned deterministic policy result
derived from those inputs.

The following repository conditions do not form one mutually-exclusive enum:

- Original
- Fork
- TemplateDerived
- Archived
- Empty
- InsufficientEvidence
- Active
- Historical

A repository may satisfy multiple conditions simultaneously.

Example:

Original
+
Archived
+
Historical

is valid.

## 4. Condition Dimensions

### 4.1 Provenance / Origin

Canonical normalized conditions:

- Original
- Fork
- TemplateDerived

Provenance does not by itself determine overall eligibility.

Fork/template state must not silently establish:

- authorship,
- originality,
- independent technical production.

### 4.2 Lifecycle

Canonical normalized conditions:

- Active
- Historical
- Archived

Archived does not mean invalid evidence.

Historical evidence may remain usable while being restricted from
current-focus contribution.

Exact lifecycle/activity thresholds belong to the versioned eligibility policy.

### 4.3 Evidence Surface

Canonical evidence-surface conditions include:

- Empty
- InsufficientEvidence
- EvidenceAvailable

Empty
!=
InsufficientEvidence

Empty is determinable from normalized pre-analysis repository state.

InsufficientEvidence is established at the repository-analysis boundary.

Neither state is negative developer evidence.

### 4.4 Independence

Independence exists only for profile repetition contribution.

A repository contribution must be deterministically classified as:

- independent for repetition,
- non-independent for repetition,
- or undetermined.

Independence does not assert authorship.

Fork/template relationships must not inflate independent repetition counts.

## 5. Missing Boundary

Repository-level SignalStatus.Missing remains repository-analysis truth.

Therefore:

SignalStatus.Missing
!=
InsufficientEvidence

SignalStatus.Missing
!=
RepositoryEligibilityStatus.NonContributing

A repository may contain Missing signals and still provide reliable evidence
for other profile-level purposes.

P2 must never derive repository eligibility from a count of Missing signals.

## 6. Two-Stage Eligibility Evaluation

Eligibility uses two deterministic stages under one EligibilityPolicyVersion.

### 6.1 Stage A — Analysis Admission

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

> Should repository analysis be attempted for this ProfileRepository?

It is a P2 policy intermediate.

It is not RepositoryEligibilityResult.

### 6.2 Empty Repository Admission

When normalized repository state deterministically identifies an empty
repository:

AnalysisAdmissionDecision
=
Skip

Reason:
EmptyRepository

RepositoryAnalysis is not required to establish this condition.

### 6.3 Fork Admission (Session 098)

When normalized manifest provenance deterministically identifies a fork:

AnalysisAdmissionDecision
=
Skip

Reason:
ForkDetected

`RepositoryAnalysis` is not required to establish this condition, exactly as
§6.2 already holds for an empty repository. The fork condition is known from
the profile repository manifest before any repository collection begins.

The rule exists because Profile V1 owns **no attribution contract**. It cannot
prove which changes inside a fork were authored by the profile owner, so it
must not consume fork content as developer evidence. This deliberately accepts
a possible false negative — the owner may have meaningful work inside a fork —
in preference to a claim the contract cannot support.

Reactivating fork evidence requires a new, explicit attribution surface capable
of proving owner-authored contribution. No such surface may be inferred today.

This rule belongs to the `EligibilityPolicyVersion` that carries it (§19.3).

### 6.3.1 Other Non-Empty Repository Admission

Original, template-derived, archived, active or historical state does not by
itself require Skip.

When required orchestration input is available:

AnalysisAdmissionDecision
=
Analyze

Contribution restrictions are evaluated later.

### 6.4 Admission Input Unavailable

When required normalized manifest input is unavailable and admission cannot be
determined reliably:

AnalysisAdmissionDecision
=
Undetermined

No implicit Analyze or Skip fallback is permitted.

## 7. Stage B — Final Repository Eligibility

Final RepositoryEligibilityResult may consume:

- ProfileRepositoryReference,
- normalized repository conditions,
- AnalysisAdmissionDecision,
- RepositoryAnalysisReference when analysis was performed,
- RepositoryAnalysis.AnalysisStatus when available,
- EligibilityPolicyVersion.

Canonical direction:

ProfileRepository
↓
AnalysisAdmissionDecision
↓
RepositoryAnalysis when Analyze
↓
purpose-specific contribution decisions
↓
RepositoryEligibilityResult

## 8. RepositoryAnalysis Status Boundary

Existing repository-analysis semantics are preserved.

### Completed

Completed RepositoryAnalysis is available for downstream profile evidence.

Completed does not mean every repository signal is present.

### Partial

Partial RepositoryAnalysis may contribute only through evidence actually
available.

Unavailable portions must not be inferred.

Partial does not automatically mean InsufficientEvidence.

### InsufficientData

RepositoryAnalysis.AnalysisStatus.InsufficientData maps to the profile
evidence-surface condition:

InsufficientEvidence

This is a contribution limitation only.

It is not negative developer evidence.

### Failed

RepositoryAnalysis.AnalysisStatus.Failed is a technical failure.

Failed
!=
InsufficientEvidence

Required evidence-bearing contribution decisions become Undetermined when the
technical failure prevents a reliable decision.

Technical failure must not be converted into evidence absence.

## 9. Canonical Evaluation Flow

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

P2 does not alter RepositoryAnalysis or repository-level SignalStatus
semantics.

## 10. Contribution Decision

Repository eligibility is purpose-specific.

Canonical ContributionDecision values:

- Allowed
- NotAllowed
- NotApplicable
- Undetermined

### Allowed

The repository may contribute for that exact purpose under the evaluated
EligibilityPolicyVersion.

### NotAllowed

The contribution purpose is applicable, but deterministic policy prevents the
repository from contributing for that purpose.

### NotApplicable

The contribution purpose has no semantic applicability for the evaluated
repository condition.

### Undetermined

Required observable input is unavailable or insufficient to decide that exact
contribution reliably.

Undetermined must not silently become Allowed or NotAllowed.

## 11. Canonical Contribution Purposes

### ProfileContext

Whether the repository may remain visible as part of the public profile
evidence surface.

ProfileContext is context-only.

ProfileContext is not itself evidence for a profile-level claim.

### ProfileObservation

Whether available structured repository evidence may contribute to quantified
profile observations.

### IndependentRepetition

Whether the repository may count as an independent supporting repository for a
profile repetition rule.

IndependentRepetition does not assert authorship.

### CurrentFocus

Whether the repository may support a claim about currently visible technical
focus under the versioned lifecycle/activity policy.

### HistoricalEvidence

Whether the repository may support historical public-evidence context.

## 12. Evidence-Bearing Purposes

Evidence-bearing contribution purposes are:

- ProfileObservation
- IndependentRepetition
- CurrentFocus
- HistoricalEvidence

ProfileContext is excluded from overall evidence contribution derivation.

Therefore repository visibility cannot accidentally promote a repository into
profile-level evidence.

## 13. Repository Eligibility Status

RepositoryEligibilityResult exposes one derived overall status.

Canonical RepositoryEligibilityStatus values:

- Contributing
- Restricted
- NonContributing
- Undetermined

The overall status is derived from purpose-specific contribution decisions.

It is not an independent source of truth.

## 14. Deterministic Overall Status Derivation

Only evidence-bearing contribution purposes participate.

Canonical precedence:

1. Undetermined
2. Restricted
3. Contributing
4. NonContributing

### 14.1 Undetermined

If any applicable evidence-bearing contribution decision is Undetermined:

RepositoryEligibilityStatus
=
Undetermined

This rule has highest precedence.

Allowed
+
Undetermined
→
Undetermined

Allowed
+
NotAllowed
+
Undetermined
→
Undetermined

Known purpose-specific decisions remain visible.

The overall result must not hide unresolved evidence limitations.

### 14.2 Restricted

When:

- no evidence-bearing decision is Undetermined,
- at least one evidence-bearing decision is Allowed,
- at least one applicable evidence-bearing decision is NotAllowed,

then:

RepositoryEligibilityStatus
=
Restricted

Example:

HistoricalEvidence
=
Allowed

CurrentFocus
=
NotAllowed

→
Restricted

### 14.3 Contributing

When:

- no evidence-bearing decision is Undetermined,
- at least one evidence-bearing decision is Allowed,
- no applicable evidence-bearing decision is NotAllowed,

then:

RepositoryEligibilityStatus
=
Contributing

NotApplicable decisions are ignored for this derivation.

### 14.4 NonContributing

When:

- no evidence-bearing decision is Undetermined,
- no evidence-bearing decision is Allowed,

then:

RepositoryEligibilityStatus
=
NonContributing

Evidence-bearing purposes may be deterministically NotAllowed and/or
NotApplicable.

ProfileContext may still be Allowed.

NonContributing is only a repository contribution result.

It is not negative developer evidence.

### 14.5 Status Uniqueness

For one fixed set of purpose-specific contribution decisions, exactly one
RepositoryEligibilityStatus must be derivable.

No status tie is permitted.

No implicit fallback is permitted.

## 15. Structured Eligibility Reasons

Eligibility reasons explain why a contribution decision or overall result was
produced.

Canonical reason identities must be:

- deterministic,
- locale-independent,
- presentation-neutral,
- version-compatible,
- traceable to observable repository conditions.

Localized reason prose is not canonical truth.

### 15.1 Reason Families

Canonical reason families:

- Provenance
- Lifecycle
- EvidenceSurface
- Independence
- InputAvailability
- Policy

### 15.2 Canonical V1 Reason Identities

The canonical V1 EligibilityReason identities are exactly:

- ForkDetected
- TemplateDerived
- Archived
- Historical
- EmptyRepository
- InsufficientEvidence
- NonIndependentContribution
- IndependenceUndetermined
- RequiredInputUnavailable

No additional V1 reason identity may be introduced without a new eligibility
contract/rule version.

These are semantic reason identities.

A reason identity may carry deterministic structured references to the
repository condition or required input that caused the decision.

Presentation wording may vary by locale without changing the canonical result.

## 16. Condition versus Reason

Repository condition
!=
EligibilityReason

A repository condition is normalized observable input.

An EligibilityReason explains a deterministic policy decision derived from
input facts.

Therefore:

Fork condition
!=
ForkDetected reason

Archived condition
!=
Archived reason

The distinction prevents observed repository facts from becoming policy
conclusions by themselves.

## 17. Result Traceability

RepositoryEligibilityResult must remain traceable to:

- ProfileRepositoryReference,
- AnalysisAdmissionDecision,
- RepositoryAnalysisReference when analysis occurred,
- normalized conditions relevant to the decision,
- purpose-specific ContributionDecision values,
- structured EligibilityReason identities,
- EligibilityPolicyVersion.

A result must not silently point to a different repository or a different
RepositoryAnalysis result.

## 18. Locale and LLM Boundary

No localized prose participates in eligibility evaluation.

No LLM participates in eligibility evaluation.

Changing locale must not change:

- AnalysisAdmissionDecision,
- ContributionDecision,
- RepositoryEligibilityStatus,
- EligibilityReason identity,
- independence determination,
- EligibilityPolicyVersion.

Presentation may translate canonical reason identities after the deterministic
result exists.

## 19. V1 Lifecycle and Independence Rules

### 19.1 Activity Classification

Activity classification and archive state are separate dimensions.

Canonical activity classification:

- Active
- Historical
- Undetermined

Archived is an independent repository condition.

Therefore a repository may be:

Active + Archived

or:

Historical + Archived

even though archive state later restricts CurrentFocus contribution.

### 19.2 Activity Reference

Activity classification must use:

- the normalized profile SnapshotCapturedAt,
- a normalized repository source-control activity timestamp,
- the exact CurrentActivityWindow belonging to the evaluated
  EligibilityPolicyVersion.

General metadata update timestamps must not silently substitute for
source-control activity.

No system clock may be read inside deterministic eligibility evaluation.

Required semantic direction:

ActivityCutoff
=
SnapshotCapturedAt
-
CurrentActivityWindow

If:

RepositoryActivityTimestamp >= ActivityCutoff

then:

ActivityClassification
=
Active

If:

RepositoryActivityTimestamp < ActivityCutoff

then:

ActivityClassification
=
Historical

If the required repository activity timestamp is unavailable:

ActivityClassification
=
Undetermined

### 19.3 CurrentActivityWindow Ownership

P2 does not invent an unversioned universal duration.

Every usable EligibilityPolicyVersion must uniquely identify one immutable
policy definition containing an exact positive CurrentActivityWindow.

Changing CurrentActivityWindow requires a new EligibilityPolicyVersion.

### 19.3.2 Fork Admission Ownership (Session 098)

The §6.3 fork admission rule belongs to the same immutable policy definition.

The Session 098 change — fork lineage requiring `Skip` with reason
`ForkDetected` — withdraws contributions a previous version allowed, so it is a
breaking eligibility semantic change and **requires a new
`EligibilityPolicyVersion`**. It may not be applied under the version that
carried the previous rule.

The concrete version identity is fixed by the implementation session under the
existing semantic-versioning convention already used by
`EligibilityPolicyVersion`, `ObservationRuleVersion`, `PatternRuleVersion` and
`RepresentativeRepositoryRuleVersion`; a withdrawal of previously allowed
contribution is a **major** increase. No parallel version identity may be
invented for it.

An implementation must not:

- choose a hidden default window,
- change the window without changing policy version,
- derive the window from locale,
- derive the window from current execution time.

Therefore identical normalized inputs plus the same complete policy version
produce the same activity classification.

### 19.3.1 Profile V1 CurrentActivityWindow Value

For Profile V1:

CurrentActivityWindow
=
exactly 365 days

This value belongs immutably to the `EligibilityPolicyVersion` that carries
it. Changing it requires a new `EligibilityPolicyVersion`, exactly as §19.3
already requires.

#### It Is a Recency Boundary

The window is a recency boundary on observed source-control activity.

It is not a requirement that a repository have 365 days of history, and it is
not a minimum age, maturity or duration expectation of any kind.

Therefore:

repository age
=
irrelevant to ActivityClassification

A repository created two weeks before the snapshot, with source-control
activity inside the window, is `Active`. A repository with ten years of
history whose last source-control activity precedes the cutoff is
`Historical`.

#### Application

Using the §19.2 activity reference unchanged:

ActivityCutoff
=
SnapshotCapturedAt
-
365 days

RepositoryActivityTimestamp >= ActivityCutoff
→
ActivityClassification
=
Active

RepositoryActivityTimestamp < ActivityCutoff
→
ActivityClassification
=
Historical

Required source-control activity timestamp unavailable
→
ActivityClassification
=
Undetermined

#### Preserved Boundaries

Locking the value changes nothing else in §19.2:

- general metadata update timestamps must not substitute for source-control
  activity,
- no system clock participates; the cutoff is anchored to
  `SnapshotCapturedAt`,
- an unavailable activity timestamp yields `Undetermined`, never an
  optimistic `Active`.

Archived remains an independent repository condition (§19.1). A repository
may be `Active + Archived`, and §19.4 may still make `CurrentFocus =
NotAllowed` on archive grounds regardless of activity classification.

### 19.4 Archive Override

Archived repository state does not erase historical evidence.

However:

Archived
→
CurrentFocus
=
NotAllowed

when the archive state is deterministically known.

Reason:
Archived

HistoricalEvidence may still be evaluated from available evidence.

### 19.5 V1 Independence Classification

Canonical IndependenceClassification values:

- Independent
- NonIndependent
- Undetermined

V1 independence means:

> lineage independence for repetition counting within the observable normalized
> GitHub evidence surface.

It does not mean:

- authorship,
- originality,
- independent professional work,
- independent problem solving.

### 19.6 Independent

A repository is Independent for V1 repetition counting when normalized
provenance information deterministically establishes that it is:

- not a fork,
- not template-derived,

and the required provenance information is available.

This classification is bounded to observable repository lineage metadata.

It does not prove authorship.

### 19.7 NonIndependent

A repository is NonIndependent for V1 repetition counting when it is
deterministically known to be:

- a fork,
- or template-derived.

Therefore:

Fork
→
IndependentRepetition
=
NotAllowed

TemplateDerived
→
IndependentRepetition
=
NotAllowed

This prevents inherited repository lineage from inflating repeated profile
evidence.

### 19.8 Independence Undetermined

If required provenance/lineage input is unavailable:

IndependenceClassification
=
Undetermined

and:

IndependentRepetition
=
Undetermined

Reason:
IndependenceUndetermined

No optimistic independence fallback is permitted.

### 19.9 No Content-Similarity Inference in P2 V1

P2 V1 does not infer repository independence from:

- source-code similarity,
- README similarity,
- naming similarity,
- commit-author heuristics,
- LLM judgment.

Such mechanisms would require a separate future versioned evidence contract.

V1 independence remains intentionally bounded to normalized observable lineage
facts.

## 20. Condition Precedence Principles

Repository conditions combine rather than replace one another.

Final contribution decisions must apply all relevant restrictions.

Canonical principles:

1. technical/input Undetermined must not be hidden by another permissive
   condition;
2. Empty prevents evidence-bearing contribution;
3. InsufficientEvidence prevents evidence-bearing contribution;
4. Fork/TemplateDerived prevent IndependentRepetition;
5. Archived prevents CurrentFocus;
6. Historical activity prevents CurrentFocus;
7. Active activity does not override Fork, TemplateDerived, Archived, Empty,
   InsufficientEvidence or Undetermined restrictions;
8. ProfileContext remains separate from evidence-bearing purposes.

A permissive condition cannot erase a stricter applicable safety boundary.

## 21. V1 Exact Contribution Rules

V1 final eligibility is derived in two layers:

1. establish the evidence-surface baseline;
2. apply provenance, independence and lifecycle restrictions.

Repository conditions must not be collapsed into one repository-type switch.

## 22. Evidence-Surface Baseline Matrix

### 22.1 Empty Repository

When:

AnalysisAdmissionDecision
=
Skip

and reason:
EmptyRepository

the canonical decisions are:

| Purpose | Decision |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | NotAllowed |
| IndependentRepetition | NotAllowed |
| CurrentFocus | NotAllowed |
| HistoricalEvidence | NotAllowed |

Overall status:

NonContributing

Reason:
EmptyRepository

Empty repository visibility remains profile context only.

### 22.1.1 Known Fork (Session 098)

When:

AnalysisAdmissionDecision
=
Skip

and reason:
ForkDetected

the canonical decisions are:

| Purpose | Decision |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | NotAllowed |
| IndependentRepetition | NotAllowed |
| CurrentFocus | NotAllowed |
| HistoricalEvidence | NotAllowed |

Overall status:

NonContributing

Reason:
ForkDetected

The fork remains visible in the profile repository universe and its observable
fork condition remains visible; only its evidence-bearing contribution is
withheld.

This is not negative developer evidence, not a quality judgment and not a claim
that the repository contains no work.

Because deep collection of a fork is not a V1 evidence obligation, a failure to
collect one can no longer make the profile `Partial`.

### 22.2 Completed RepositoryAnalysis

When:

AnalysisAdmissionDecision
=
Analyze

and:

RepositoryAnalysis.AnalysisStatus
=
Completed

then the repository has an EvidenceAvailable evidence-surface baseline.

Baseline decisions:

| Purpose | Baseline |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | Allowed |
| IndependentRepetition | resolve from IndependenceClassification |
| CurrentFocus | resolve from lifecycle/archive rules |
| HistoricalEvidence | Allowed |

Allowed means only that available structured repository evidence may
contribute.

It does not mean every repository signal is positive or present.

### 22.3 Partial RepositoryAnalysis

When:

RepositoryAnalysis.AnalysisStatus
=
Partial

the repository also has an EvidenceAvailable evidence-surface baseline because
meaningful partial analysis exists.

Baseline decisions:

| Purpose | Baseline |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | Allowed for actually available structured evidence only |
| IndependentRepetition | resolve from IndependenceClassification |
| CurrentFocus | resolve from lifecycle/archive rules |
| HistoricalEvidence | Allowed for actually available structured evidence only |

Unavailable repository evidence cannot contribute.

No unavailable fact may be inferred from the partial result.

### 22.4 InsufficientData RepositoryAnalysis

When:

RepositoryAnalysis.AnalysisStatus
=
InsufficientData

the canonical evidence-surface condition is:

InsufficientEvidence

Canonical decisions:

| Purpose | Decision |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | NotAllowed |
| IndependentRepetition | NotAllowed |
| CurrentFocus | NotAllowed |
| HistoricalEvidence | NotAllowed |

Overall status:

NonContributing

Reason:
InsufficientEvidence

This is not negative developer evidence.

### 22.5 Failed RepositoryAnalysis

When:

RepositoryAnalysis.AnalysisStatus
=
Failed

repository-analysis-dependent evidence cannot be treated as available.

Canonical baseline:

| Purpose | Baseline |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | Undetermined |
| IndependentRepetition | resolve deterministic manifest-only restriction first; otherwise Undetermined |
| CurrentFocus | resolve deterministic manifest-only restriction first; otherwise Undetermined |
| HistoricalEvidence | Undetermined |

Examples:

Known Fork
+
RepositoryAnalysis Failed
→
IndependentRepetition
=
NotAllowed

(Session 098: under the current policy version a known fork never reaches a
RepositoryAnalysis at all — admission is `Skip` per §6.3, so §22.1.1 applies
instead. This example remains valid only for a policy version that admitted
forks.)

Known Archived
+
RepositoryAnalysis Failed
→
CurrentFocus
=
NotAllowed

Known non-fork/non-template provenance
+
RepositoryAnalysis Failed
→
IndependentRepetition
=
Undetermined

because repository evidence contribution itself could not be established.

Known Archived
+
RepositoryAnalysis Failed
does not make HistoricalEvidence available.

HistoricalEvidence remains:
Undetermined

A technical failure is not evidence absence.

Required unresolved decisions use an InputAvailability reason such as:

RequiredInputUnavailable

Overall status is derived only after these purpose-specific decisions are
resolved.

### 22.6 Admission Undetermined

When:

AnalysisAdmissionDecision
=
Undetermined

repository analysis has not been reliably admitted.

Canonical baseline:

| Purpose | Baseline |
|---|---|
| ProfileContext | Allowed |
| ProfileObservation | Undetermined |
| IndependentRepetition | resolve deterministic manifest-only restriction first; otherwise Undetermined |
| CurrentFocus | resolve deterministic manifest-only restriction first; otherwise Undetermined |
| HistoricalEvidence | Undetermined |

Examples:

Known Fork
+
Admission Undetermined
→
IndependentRepetition
=
NotAllowed

(Session 098: a deterministically known fork resolves to `Skip` per §6.3 before
this baseline is reached, so under the current policy version this example is
unreachable. It remains valid only for a policy version that admitted forks.)

Known Archived
+
Admission Undetermined
→
CurrentFocus
=
NotAllowed

Otherwise unresolved evidence-bearing decisions remain Undetermined.

The exact structured InputAvailability reason must identify the missing
admission input.

Overall status is derived after purpose-specific resolution.

## 23. Independence Contribution Rules

These rules apply after an EvidenceAvailable baseline exists.

### Independent

IndependenceClassification
=
Independent

→

IndependentRepetition
=
Allowed

### NonIndependent

IndependenceClassification
=
NonIndependent

→

IndependentRepetition
=
NotAllowed

Required reason:

- ForkDetected when fork lineage establishes the restriction,
- TemplateDerived when template lineage establishes the restriction,
- or both reasons when both normalized conditions apply.

### Undetermined

IndependenceClassification
=
Undetermined

→

IndependentRepetition
=
Undetermined

Reason:
IndependenceUndetermined

### Important

IndependentRepetition
=
Allowed

means only that the repository may count as one independent repository for
profile repetition rules.

It does not prove authorship or originality.

## 24. Lifecycle / CurrentFocus Rules

These rules apply after an EvidenceAvailable baseline exists.

### Archived

If:

Archived
=
true

then:

CurrentFocus
=
NotAllowed

Reason:
Archived

This rule is sufficient even if activity classification is otherwise
Undetermined.

Archive state already establishes the V1 current-focus restriction.

### Historical

If repository activity classification is:

Historical

then:

CurrentFocus
=
NotAllowed

Reason:
Historical

### Active

If:

- Archived = false,
- ActivityClassification = Active,

then:

CurrentFocus
=
Allowed

subject to all stronger evidence-surface restrictions already applied.

### Activity Undetermined

If:

- Archived = false,
- ActivityClassification = Undetermined,

then:

CurrentFocus
=
Undetermined

Reason:
RequiredInputUnavailable

No optimistic Active fallback is permitted.

## 25. Provenance Contribution Rules

### Original

Original provenance introduces no additional V1 contribution restriction.

It does not by itself prove authorship.

### Fork

Fork condition (amended by Session 098):

- does not remove ProfileContext,
- **removes ProfileObservation**,
- **removes CurrentFocus**,
- **removes HistoricalEvidence**,
- prevents IndependentRepetition.

The mechanism is admission: a known fork is `Skip` with reason `ForkDetected`
(§6.3, §22.1.1), so no fork-derived evidence is produced at all.

A fork must not be narrated as original production, and its withheld
contribution must not be narrated as absence of work.

Where fork-derived evidence exists in an earlier `EligibilityPolicyVersion`, it
must retain fork provenance in traceability. Under the current version no such
evidence is produced.

### TemplateDerived

TemplateDerived condition:

- does not remove ProfileContext,
- does not automatically remove ProfileObservation,
- does not automatically remove HistoricalEvidence,
- prevents IndependentRepetition.

Template-derived evidence must retain template provenance in traceability.

Template use must not be narrated as proof of independent architecture,
authorship or original problem solving.

## 26. Historical Evidence Rule

HistoricalEvidence is evidence-surface based, not mutually exclusive with
CurrentFocus.

Therefore an EvidenceAvailable repository may contribute historical public
evidence even when it is also Active.

Archived or Historical lifecycle state does not invalidate
HistoricalEvidence.

This purpose describes usable public evidence context.

It is not a claim about professional experience duration.

## 27. Purpose-Level Resolution Precedence

Each contribution purpose is resolved independently before overall status is
derived.

Canonical resolution principles:

1. a deterministic evidence-surface exclusion such as Empty or
   InsufficientEvidence produces NotAllowed for evidence-bearing purposes;
2. a technical inability to determine an otherwise required decision produces
   Undetermined;
3. a deterministic purpose-specific restriction may resolve the purpose to
   NotAllowed even when another input irrelevant to that already-sufficient
   restriction is unavailable;
4. a permissive condition never overrides NotAllowed or Undetermined;
5. Allowed is emitted only when all inputs required for that purpose are
   sufficiently known.

Examples:

Archived = true
+
ActivityClassification = Undetermined
→
CurrentFocus = NotAllowed

because archive state alone is sufficient for the restriction.

Fork = true
+
other lineage input unavailable
→
IndependentRepetition = NotAllowed

because known fork lineage alone is sufficient for the restriction.

Archived = false
+
ActivityClassification = Undetermined
→
CurrentFocus = Undetermined

because no sufficient deterministic restriction or permission exists.

## 28. Overall Status Examples

### Original + Active + EvidenceAvailable + Independent

ProfileObservation:
Allowed

IndependentRepetition:
Allowed

CurrentFocus:
Allowed

HistoricalEvidence:
Allowed

Overall:
Contributing

### Fork + Active + EvidenceAvailable

Amended by Session 098. A known fork never reaches this evidence state, because
admission is `Skip` before any repository collection:

ProfileObservation:
NotAllowed (ForkDetected)

IndependentRepetition:
NotAllowed (ForkDetected)

CurrentFocus:
NotAllowed (ForkDetected)

HistoricalEvidence:
NotAllowed (ForkDetected)

Overall:
NonContributing

Active lifecycle state does not override the provenance restriction (§20
principle 7). The result does not imply authorship, and it is not a statement
that the fork contains no work.

### Original + Archived + Historical + EvidenceAvailable + Independent

ProfileObservation:
Allowed

IndependentRepetition:
Allowed

CurrentFocus:
NotAllowed

HistoricalEvidence:
Allowed

Overall:
Restricted

### Empty

All evidence-bearing purposes:
NotAllowed

Overall:
NonContributing

### InsufficientEvidence

All evidence-bearing purposes:
NotAllowed

Overall:
NonContributing

### RepositoryAnalysis Failed

Evidence-bearing purposes:
Undetermined

Overall:
Undetermined

## 29. Epistemic Strength Boundary

Repository eligibility controls whether and how existing public repository
evidence may contribute.

It must not increase the epistemic strength of that evidence.

Therefore:

ContributionDecision.Allowed

means only:

> this existing evidence may participate in this contracted profile purpose.

It does not mean:

- stronger repository evidence,
- stronger authorship evidence,
- stronger capability evidence,
- practical-experience evidence,
- professional-experience evidence.

Required invariant:

eligibility output epistemic strength
<=
supporting repository evidence epistemic strength

A contribution policy may restrict evidence usage.

It may not upgrade weak, partial or limited evidence into stronger truth.

RepositoryAnalysis.Partial evidence remains partial evidence after eligibility.

Repository-level uncertainty remains uncertainty after eligibility.

## 30. Job-Agnostic Boundary

Repository eligibility is reusable profile-foundation truth.

P2 evaluation must not consume:

- a job posting,
- JobRequirementProfile,
- JobEvidenceAlignment,
- job-specific requirement importance,
- job-specific repository relevance,
- recruiter preference,
- hiring context.

Therefore:

RepositoryEligibilityResult
=
job-agnostic profile repository contribution truth

Job context must not change:

- AnalysisAdmissionDecision,
- ContributionDecision,
- RepositoryEligibilityStatus,
- EligibilityReason,
- IndependenceClassification,
- ActivityClassification.

A later Job Evidence Alignment consumer may select or compare contracted profile
evidence for one job.

It must not retroactively change repository eligibility or increase the
underlying profile evidence strength.

## 31. Final Eligibility Result Consumed by Profile Composition

This section locks which result profile composition (P8) consumes. It
introduces no new eligibility rule, status, purpose, reason identity or
policy semantics; every rule it refers to is already defined above.

### 31.1 The Final Result

The result profile composition consumes is the RepositoryEligibilityResult
defined by this contract, carrying all of:

- the purpose-specific ContributionDecision values (§10) for every canonical
  contribution purpose (§11): ProfileContext, ProfileObservation,
  IndependentRepetition, CurrentFocus, HistoricalEvidence,
- the overall RepositoryEligibilityStatus (§13) deterministically derived
  from those decisions by §14,
- the traceability required by §17.

Its canonical semantic question is unchanged:

> What may this repository safely contribute evidence toward?

It is not:

> Is this a good repository?

### 31.2 Undetermined Must Survive

The Undetermined ContributionDecision (§10) and the Undetermined
RepositoryEligibilityStatus (§13, §14.1) must reach profile composition
intact.

An implementation must not collapse Undetermined into NotAllowed, into
false, or into any absent/default value on the way to composition.

Therefore:

insufficient evidence to decide safely
!=
decided as not allowed

Erasing that distinction would convert a known gap in observability into an
apparent deterministic exclusion, which §14.1 already prohibits at the
derivation level and which is equally prohibited at the consumption level.

### 31.3 Derived Status Is Not a Second Truth

The overall RepositoryEligibilityStatus remains derived output, exactly as
§13 states.

It must not be stored, transported or recomputed in a way that lets it
contradict the purpose-specific decisions it was derived from. For one fixed
set of purpose-specific decisions, §14.5 already requires exactly one
derivable status; no consumer may present or persist a status that §14 would
not produce from those same decisions.

Therefore:

overall status
!=
independent eligibility truth

### 31.4 Required Provenance

The final result must preserve:

- canonical repository identity, via ProfileRepositoryReference (§17),
- eligibility policy/version provenance, via EligibilityPolicyVersion (§17,
  §19.3),
- deterministic structured reason/trace information sufficient to explain why
  each purpose-specific decision was made, via the EligibilityReason
  identities locked in §15.

Reason and trace information is explanatory only. It must not introduce
repository quality, developer capability, seniority, employability or ranking
semantics — §2 and §29 already prohibit these, and no reason, trace or
presentation of a reason may reintroduce them.

### 31.5 RepositoryContributionAssessment Is Not the Final Result

The narrower runtime type currently present in the codebase as
`RepositoryContributionAssessment` is not the final RepositoryEligibilityResult
defined here. By its own definition it does not fully represent CurrentFocus
and HistoricalEvidence and derives no overall RepositoryEligibilityStatus.

Profile composition must consume the final result defined by this contract.

An implementation must not substitute the narrower assessment, and must not
silently treat an unevaluated CurrentFocus or HistoricalEvidence purpose as
though it had been decided.

Closing that gap is implementation work governed by this contract. It is not
a further semantic decision, because every rule required to produce the final
result — the purposes (§11), the decision values (§10), the baselines
(§22), the independence rules (§23), the lifecycle/CurrentFocus rules (§24),
the provenance rules (§25), the HistoricalEvidence rule (§26), the
resolution precedence (§27) and the deterministic overall derivation (§14) —
is already locked above.

The `CurrentActivityWindow` prerequisite required by §19.3 is now satisfied:
§19.3.1 locks the Profile V1 value at exactly 365 days, so
`ActivityClassification` and therefore `CurrentFocus` are computable without
an implementation selecting a window value.

### 31.6 Human-Language Invariant

A repository does not have one universal include/exclude answer.

GitHub Proof records what the repository may safely be used to support.

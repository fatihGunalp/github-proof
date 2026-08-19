# P8 — Implementation Planning

**Date:** 2026-08-19
**Result:** DOCS — executable P8 implementation plan recorded. No code written.
P8 implementation remains READY / NOT STARTED.
**Scope:** planning only — no src/, no tests/, no canonical semantic change
**Model Route:** opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Verified Baseline

```text
branch (at gate)  main
working tree      clean
HEAD              89361e6  docs(profile): close P8-1 composition and report contracts
main              89361e6  (identical)

worklog/CURRENT.md
  P8-1 — Exact Contract Closure:  COMPLETE
  P8 implementation:              READY
  Active Session:                 NONE

src/    untouched
tests/  untouched
```

Sessions 078–082 (the P8-1 contract chain) were committed and merged by the
user into `main` as `89361e6`. This session branches from that commit as
`session/083-p8-implementation-planning`.

Next session number verified from `worklog/sessions/`: 082 is the latest, so
this is 083.

---

## Code Inspection — What Actually Exists

Read for planning purposes only; nothing was modified.

### Domain — `src/GitHubProof.Domain/Profile/`

```text
Policies      AnalysisAdmissionPolicy, RepositoryContributionPolicy,
              ProfilePatternPromotionPolicy,
              RepresentativeRepositorySelectionPolicy
Models        GitHubProfileSnapshot, ProfileRepository,
              ProfileRepositoryManifest, GitHubProfileMetadata,
              AnalysisAdmissionResult, ContributionAssessment,
              RepositoryContributionAssessment, ObservedProfileEvidence,
              CrossRepositoryEvidence, RepeatedProfilePattern,
              ProfilePatternSet, RepresentativePatternSupport,
              RepresentativeRepository, RepresentativeRepositorySelection
ValueObjects  GitHubProfileIdentity, ProfileRepositoryReference,
              RepositoryAnalysisReference, ProfileEvidenceReference,
              EligibilityPolicyVersion, ObservationRuleVersion,
              PatternRuleVersion, RepresentativeRepositoryRuleVersion
```

### Application — `src/GitHubProof.Application/Profile/`

```text
ProfileRepositoryAnalysisOrchestrator, ProfileAnalysisOrchestrationResult,
ProfileRepositoryAnalysisResult, ProfileRepositorySnapshotResult,
IProfileRepositorySnapshotSource, CrossRepositoryEvidenceComposer,
ProfileRepositoryReferenceOrder
```

### Findings That Materially Shape the Plan

**F-A — `ProfileRepository` already carries the inputs `CurrentFocus` needs.**
It exposes `Archived` (`RepositoryConditionState`) and `LastPushedAt`
(`DateTimeOffset?`), plus `CreatedAt`. `LastPushedAt` is a source-control push
timestamp, which is what eligibility §19.2 requires and is distinct from a
generic metadata update timestamp. No collection-layer change is needed to
evaluate `ActivityClassification`.

**F-B — `EligibilityPolicyVersion` is a bare string wrapper.** It carries no
policy definition. Eligibility §19.3 requires every usable
`EligibilityPolicyVersion` to uniquely identify one immutable policy
definition containing an exact positive `CurrentActivityWindow`, and §19.3.1
locks that value at 365 days for Profile V1. Nothing in the codebase binds a
version to a window today. Creating that binding is real P8-I1 work.

**F-C — the narrow eligibility type is load-bearing inside P5.**
`CrossRepositoryEvidenceComposer` constructs `RepositoryContributionPolicy`
and calls `Evaluate(...)` per repository, and `CrossRepositoryEvidence` stores
the resulting `RepositoryContributionAssessment` values. So introducing the
final `RepositoryEligibilityResult` is not an isolated addition — it has a P5
blast radius, with existing Domain and Application tests over it.

**F-D — `IsEvidenceSurfaceComplete` currently means manifest completeness
only.** `CrossRepositoryEvidenceComposer` passes
`orchestration.IsRepositoryManifestComplete` into `CrossRepositoryEvidence`,
which `ProfilePatternPromotionPolicy` propagates to `ProfilePatternSet` and
`RepresentativeRepositorySelectionPolicy` carries onto the selection. This is
exactly the consumed upstream signal domain model §13 "Upstream Completeness"
describes — and it confirms the contract's asymmetry: `false` → Partial, but
`true` alone cannot establish Complete, because it says nothing about
unresolved per-repository obligations.

**F-E — an ordering comparer already exists at two levels.**
`ProfileRepositoryReference.CompareTo` (Owner, Name) is the canonical Domain
ordering; `ProfileRepositoryReferenceOrder` in Application adds a `Url`
fallback and is display/ordering-internal. Domain model §13 "Canonical
Ordering Keys" names the canonical one and forbids substituting the
application-internal ordering — so limitation/warning ordering must use
`ProfileRepositoryReference`'s own ordering, not
`ProfileRepositoryReferenceOrder`.

---

## Slice Plan

### P8-I1 — Final Repository Eligibility Runtime

**Responsibility.** Produce the final `RepositoryEligibilityResult` that
composition consumes, per eligibility §31: all five purpose-specific
`ContributionDecision` values (`ProfileContext`, `ProfileObservation`,
`IndependentRepetition`, `CurrentFocus`, `HistoricalEvidence`), the derived
overall `RepositoryEligibilityStatus`, and §17/§31.4 traceability. Add the
`EligibilityPolicyVersion` → policy definition binding carrying
`CurrentActivityWindow` = 365 days (§19.3, §19.3.1), and the
`ActivityClassification` evaluation anchored on `SnapshotCapturedAt` (§19.2).

**Likely files/types.**

```text
add     Domain/Profile/ValueObjects/EligibilityPolicy.cs
          (or equivalent: version + immutable CurrentActivityWindow)
add     Domain/Profile/Enums/RepositoryEligibilityStatus.cs
add     Domain/Profile/Enums/ContributionPurpose.cs
add     Domain/Profile/Enums/ActivityClassification.cs
add     Domain/Profile/Models/RepositoryEligibilityResult.cs
add     Domain/Profile/RepositoryEligibilityPolicy.cs
          (Stage B completion: lifecycle/archive/historical + overall status)
change  Domain/Profile/RepositoryContributionPolicy.cs      (see F-C)
change  Domain/Profile/Models/RepositoryContributionAssessment.cs (see F-C)
change  Application/Profile/CrossRepositoryEvidenceComposer.cs (see F-C)
change  Domain/Profile/Models/CrossRepositoryEvidence.cs      (see F-C)
```

**Dependency.** None. This is the first slice.

**Tests required.** Domain: the §24 lifecycle branches (Archived, Historical,
Active, activity Undetermined) including `Active + Archived`; §19.2 cutoff
boundary at exactly `SnapshotCapturedAt − 365d` on both sides; missing
activity timestamp → `Undetermined` with no optimistic `Active`; repository
age irrelevance (new repo Active, old repo Historical); §14 overall-status
derivation across all four values including the §14.1 Undetermined precedence
and §14.5 uniqueness; §26 HistoricalEvidence not mutually exclusive with
CurrentFocus; §22 baselines for Empty / Completed / Partial / InsufficientData
/ Failed / admission-Undetermined; §31.2 `Undetermined` survival. Application:
full regression of the existing `CrossRepositoryEvidenceComposer` and
`CrossRepositoryLanguageEvidence` suites.

**Invariants protected.** `Undetermined` never collapses to `NotAllowed`
(§31.2); overall status never contradicts its purpose decisions (§31.3);
no system clock (§19.2); metadata timestamps never substitute for
source-control activity; reason/trace introduces no quality, capability,
seniority, employability or ranking semantics (§31.4, §2, §29); eligibility is
not repository quality.

**Out of scope.** Any profile root type; any composition; any report; changing
P5 observation semantics or P6 promotion rules; changing the collection layer.

**Exit criteria.** All five purposes and the derived status produced from a
single policy version; the 365-day window bound immutably to that version;
full solution build 0 warning / 0 error; **P5 behaviour provably unchanged** —
existing Domain and Application profile suites pass without semantic edits to
their expectations.

---

### P8-I2 — Profile Root Foundation

**Responsibility.** The Domain vocabulary the composer will assemble:
`SnapshotReference` (§6), the profile-level analysis status and its ownership
boundary (§13), structured `ProfileLimitation` / `ProfileWarning` with their
V1 taxonomies, scope values, identity, deduplication and ordering (§13), and
the `DeveloperPublicEvidenceProfile` root plus the composition-result wrapper
that owns `Failed` (§13, §14).

**Likely files/types.**

```text
add  Domain/Profile/ValueObjects/SnapshotIdentity.cs
add  Domain/Profile/ValueObjects/SnapshotReference.cs
add  Domain/Profile/ValueObjects/ProfileAnalyzerVersion.cs
add  Domain/Profile/Enums/ProfileAnalysisStatus.cs        (Complete | Partial)
add  Domain/Profile/Enums/ProfileConditionScope.cs        (Profile | Repository)
add  Domain/Profile/Enums/ProfileLimitationCode.cs        (2 identities)
add  Domain/Profile/Enums/ProfileWarningCode.cs           (4 identities)
add  Domain/Profile/Models/ProfileLimitation.cs
add  Domain/Profile/Models/ProfileWarning.cs
add  Domain/Profile/Models/DeveloperPublicEvidenceProfile.cs
add  Domain/Profile/Models/ProfileCompositionResult.cs    (owns Failed)
```

**Dependency.** Needs `RepositoryEligibilityResult` from P8-I1 for the root's
`RepositoryEligibilityResults[]` member. **No other hard dependency** — the
`SnapshotReference` / limitation / warning work is independent of I1 and could
run in parallel if the team wants throughput.

**Tests required.** `CapturedAt` excluded from `SnapshotIdentity`, including
the equality consequence (same normalized snapshot, two capture times → same
identity, different `CapturedAt`); identity independent of enumeration order,
locale and repeated construction; a valid root rejects
`ProfileAnalysisStatus.Failed` by construction; root construction rejects an
incoherent input set; `StartedAt`/`CompletedAt` excluded from semantic
equality; limitation/warning deduplication by code + scope + affected
identities; canonical ordering by the §13 key order using
`ProfileRepositoryReference`'s own ordering (F-E); reflection guard against
prohibited root fields (score, seniority, employability, job fit, inferred
role, localized labels, generated summary text, UI state), mirroring the
existing `RepresentativeRepositoryContractInvariantTests` pattern.

**Invariants protected.** `Failed` never on a valid root; empty collections
never negative; audit timestamps never semantic; identity never derived from a
composer-owned field inventory; the four warning and two limitation identities
exactly as locked.

**Out of scope.** Composition logic; report projection; emitting any
limitation or warning (that is I3); persistence.

**Exit criteria.** Root and wrapper constructible and invariant-guarded;
prohibited-field guard passing; full build clean.

---

### P8-I3 — Deterministic Profile Composer

**Responsibility.** Assemble the canonical root from P3–P7 results: verify
version-lineage compatibility (§14), perform the evidence-surface accounting
that decides `Complete` vs `Partial` and consumes
`ProfilePatternSet.IsEvidenceSurfaceComplete` (§13), emit limitations and
warnings per their locked emission conditions and identity mapping, and return
`Failed` with no root when coherence cannot be proven.

**Likely files/types.**

```text
add  Application/Profile/ProfileComposer.cs
add  Application/Profile/ProfileCompositionInput.cs   (if a bundle is needed)
```

**Dependency.** P8-I1 and P8-I2.

**Tests required.** Complete on a sparse but fully resolved profile (one
repository, no repeated pattern); `NotAllowed` counted as resolved, not as a
completeness deficit; `Undetermined` producing Partial; `IsEvidenceSurfaceComplete
= false` → Partial; `IsEvidenceSurfaceComplete = true` with an unresolved
obligation still → Partial (the locked asymmetry); Unsupported ProblemArea and
Deferred RoleEvidenceAlignment never producing Partial;
`LimitedIndependentEvidenceSurface` emitted at 0 and 1 independent
repositories and not at 2+; 2+ independent repositories with no promoted
pattern producing no limitation; each of the four warning identities from its
canonical triggering condition; one event producing two identities where the
facts differ, without duplicating an identity for the same scope; each of the
five §14 compatibility conditions failing → `Failed` and no root; input-order
invariance of the whole composition.

**Invariants protected.** Warning count never determines status; incoherence
→ Failed not Partial; P7 selection never re-ranked; patterns never
recomputed; no score, seniority, employability or job-fit anywhere.

**Out of scope.** Any report or presentation; LLM; persistence; UI.

**Exit criteria.** Composition deterministic and input-order invariant; every
locked emission and accounting rule covered by a test; full build clean.

---

### P8-I4 — Typed Profile Report and Projector

**Responsibility.** Project the canonical root into the typed `ProfileReport`
per the report contract: nine semantic sections in the locked
locale-independent order, `Supported` / `Unsupported` / `Deferred`
availability independent of emptiness, deterministic item ordering preserving
upstream canonical orders, and the summary compress-don't-inflate rule.

**Likely files/types.**

```text
add  Application/Profile/Report/ProfileReport.cs
add  Application/Profile/Report/ProfileReportSection.cs
add  Application/Profile/Report/ProfileReportSectionAvailability.cs
add  Application/Profile/Report/ProfileReportProjector.cs
add  Application/Profile/Report/<per-section item DTOs>
```

**Dependency.** P8-I3.

**Tests required.** Section set and order fixed and locale-independent;
`Supported + empty` distinct from `Unsupported + empty` distinct from
`Deferred + empty`; ProblemArea `Unsupported` with `Items = []` retained in
structured truth while the default report does not render it; omission never
reordering the surviving sections; RoleEvidenceAlignment `Deferred` retained
and never collapsed with `Unsupported`; item ordering preserving pattern,
representative-repository, limitation and warning orders; a projection
introducing no claim absent from the root; locale change altering text only.

**Invariants protected.** Presentation is not an inference engine; no
reselection or re-ranking of representative repositories; no score, seniority,
capability, employability or job-fit; summary traceable to sections; LLM
excluded from truth generation.

**Out of scope.** Web/UI rendering; Markdown or PDF output; localization
resource files; LLM narration.

**Exit criteria.** Projection deterministic for a fixed root; every
availability and ordering rule covered; full build clean.

---

### P8-I5 — Integration and P8 Closure

**Responsibility.** Wire the composer into the existing profile orchestration
path, choose and record the production runtime version values, and validate
end-to-end determinism over the profile flow.

**Likely files/types.**

```text
change  Application/Profile/* composition root wiring
change  Web composition root / DI registration (no analysis logic in Web)
add     end-to-end determinism tests
```

**Dependency.** P8-I4.

**Tests required.** End-to-end replay determinism (same snapshot + same rule
versions → identical canonical result and identical report); a `Failed`
composition surfacing as a composition result without a root; a Partial
profile rendering its concise notice without the report inventing a claim.

**Invariants protected.** Web produces no analysis truth; locale changes
rerun nothing; the whole chain remains traceable from conclusion back to
repository evidence.

**Out of scope.** Milestone B release; P9 validation profile set; P10 web
profile flow; persistence; authentication.

**Exit criteria.** Full solution build 0 warning / 0 error; all suites green;
P9 able to start against a real composed profile.

---

## Dependency Order

```text
P8-I1 ──┐
        ├──> P8-I2 ──> P8-I3 ──> P8-I4 ──> P8-I5
        │
        └── (I2's SnapshotReference / limitation / warning work has no
             hard dependency on I1 and may run in parallel; only the root's
             RepositoryEligibilityResults[] member needs I1)
```

## Regrouping Assessment

**The five-slice grouping holds.** The real code structure does not require
mechanical regrouping, and no canonical semantics are affected by keeping it.
Two scheduling facts are worth recording:

1. **P8-I1 carries a P5 regression surface** (F-C) that the slice boundary
   does not make obvious. Its exit criteria therefore include proving P5
   behaviour unchanged, not merely that the new eligibility result is correct.
2. **P8-I2 is only partially dependent on P8-I1** — the identity, limitation
   and warning work is independent. Recorded as an option, not a
   recommendation to restructure.

---

## Technical Choices Deliberately Left Open by P8-1

These are naming, typing, versioning and mechanism choices. **None is a
product or domain semantic, and none may be turned into one during
implementation.**

1. **Composition-result wrapper** — concrete type name and shape of the result
   that owns `Failed` and holds an optional profile. Semantics locked (§13);
   naming free.
2. **Report DTO code naming** — type and member names for the typed report and
   its sections. Semantic responsibility, section set and order locked (report
   contract §5, §7); naming free.
3. **`SnapshotIdentity` derivation mechanism** — how the identity is computed
   from the canonical normalized snapshot semantic representation. The
   derivation boundary, prohibited inputs and the ban on a composer-owned field
   inventory are locked (§6); the mechanism is free.
4. **Profile `AnalyzerVersion` first runtime value** — precedent:
   `RepresentativeRepositoryRuleVersion.V1 = "1.0.0"` (Session 075).
5. **`EligibilityPolicy` binding shape** (new, surfaced by F-B) — how a policy
   version is bound to its immutable `CurrentActivityWindow`. The value (365
   days), its immutability and the requirement that changing it needs a new
   policy version are locked (§19.3, §19.3.1); the binding type is free.
6. **How the final eligibility result relates to the existing narrow
   assessment** (new, surfaced by F-C) — extend the existing type, or add a
   final result alongside it. Either satisfies §31.5 provided composition
   consumes the final result and the narrow one is never substituted for it.
   Both options carry different P5 regression cost; this is an engineering
   trade-off, not a semantic one.

Items 5 and 6 are newly identified by this session's code inspection. They are
recorded as open technical choices, **not** as new semantic decisions, and
neither reopens P8-1.

---

## Blockers

None. No canonical semantic gap was found during planning, and every input
P8-I1 needs already exists in the collected snapshot (F-A).

---

## Validation

- No `src/` or `tests/` file was read-modified; `git status --short` shows only
  the new worklog session file plus `CURRENT.md` / `INDEX.md`.
- `git diff --check` clean.
- Canonical `docs/` unchanged by this session — planning records no contract
  amendment.

## Next Action

```text
P8-I1 — Final Repository Eligibility Runtime
```

First concrete step: bind `EligibilityPolicyVersion` to an immutable policy
definition carrying `CurrentActivityWindow` = 365 days (F-B), then complete
Stage B with `CurrentFocus` / `HistoricalEvidence` and the derived overall
status. P8-I1 is a separate meaningful session and must be routed through its
own model/effort gate.

P8 implementation state after this session: **READY / NOT STARTED**.

No commit, no push — user-owned per CLAUDE.md.

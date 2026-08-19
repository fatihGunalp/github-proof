# P8-I3 — Deterministic Profile Composer

**Date:** 2026-08-19
**Result:** PASS — P8-I3 slice complete. Additive Application implementation; no
canonical docs changed; no existing `src/`/`tests/` file modified.
**Scope:** Application composer assembling `DeveloperPublicEvidenceProfile`
from already-produced P3–P8-I2 results, with coherence verification,
Complete/Partial accounting, and limitation/warning emission
**Model Route:** sonnet / high (INVARIANT_HEAVY)
**Model Gate:** APPROVED — user wrote `devam et`

---

## Verified Baseline

```text
branch (at gate)  main
working tree      clean
HEAD              db84146  feat(profile): establish profile root foundation
origin/main       db84146  (identical)

worklog/CURRENT.md
  P8-I1 Final Repository Eligibility Runtime:  COMPLETE
  P8-I2 Profile Root Foundation:               COMPLETE
  P8-I3 Deterministic Profile Composer:        NOT STARTED (this session)
  Active Session:                              NONE
```

Branched as `session/086-p8-i3-deterministic-profile-composer` from `main` @
`db84146`. Next session number verified from `worklog/sessions/`: 085 was
latest, so this is 086.

---

## Context Read

```text
CLAUDE.md
worklog/CURRENT.md
worklog/sessions/2026-08-19-083-p8-implementation-planning.md (P8-I3 slice)
worklog/sessions/2026-08-19-084-p8-i1-final-repository-eligibility-runtime.md
worklog/sessions/2026-08-19-085-p8-i2-profile-root-foundation.md
docs/architecture/public-profile-evidence-domain-model.md §6-§14
Full existing Domain/Profile and Application/Profile source (all models,
policies, value objects, and the two existing Application composers
CrossRepositoryEvidenceComposer/ProfileRepositoryAnalysisOrchestrator) and
their test fixtures (ProfileContributionTestSupport, ProfileAnalysisTestSupport,
DeveloperPublicEvidenceProfileTests)
```

---

## What Was Built

Two new Application files, additive only:

```text
add  src/GitHubProof.Application/Profile/ProfileCompositionInput.cs
add  src/GitHubProof.Application/Profile/ProfileComposer.cs
add  tests/GitHubProof.Application.Tests/Profile/ProfileCompositionTestSupport.cs
add  tests/GitHubProof.Application.Tests/Profile/ProfileComposerTests.cs
```

**`ProfileCompositionInput`** bundles the already-produced P3–P8-I2 results
composition needs: `ProfileAnalysisOrchestrationResult` (snapshot + per-repository
`ProfileRepositoryAnalysisResult` — the source of `Outcome`/`CollectionAvailability`
warning granularity that `RepositoryEligibilityResult` alone cannot carry, since
`EligibilityReason` collapses rate-limited/unavailable/failed into one
`RequiredInputUnavailable` value), `RepositoryEligibilityResults[]` (P8-I1),
`CrossRepositoryEvidence` (P5), `ProfilePatternSet` (P6),
`RepresentativeRepositorySelection` (P7), `AnalyzerVersion`, `StartedAt`/
`CompletedAt`. It performs no semantic validation itself — only null checks.

**`ProfileComposer.Compose(ProfileCompositionInput)`** returns
`ProfileCompositionResult`:

1. **Coherence verification** (the responsibility P8-I2's independent review
   explicitly deferred to this slice): repository-universe equality across
   `orchestration.Results` / `RepositoryEligibilityResults` /
   `CrossRepositoryEvidence.RepositoryContributions`; per-repository
   admission/contribution **value** equality between layers (not reference
   equality — `RepositoryContributionAssessment` has no `Equals` override and
   the existing architecture computes it independently in two places, so two
   coherent runs are value-equal but reference-distinct by design); a
   "Canonical Observation Trace" check that every `ObservedProfileEvidence` a
   promoted pattern cites is, by reference (`ReferenceEqualityComparer`),
   actually one of `CrossRepositoryEvidence.Observations`; and a "Canonical
   Pattern Trace" check that every `RepeatedProfilePattern` a representative
   selection cites is, by reference, actually one of `ProfilePatternSet.Patterns`.
   Any incoherence, or an `ArgumentException` surfacing from
   `DeveloperPublicEvidenceProfile`'s own version-coherence guard, is caught
   and mapped to `ProfileCompositionResult.Failed()` — never normalized to
   Partial.
2. **Evidence-surface accounting**: `Partial` when
   `PatternSet.IsEvidenceSurfaceComplete` is `false`, or when any
   `RepositoryEligibilityResult.Status == Undetermined`; `Complete` otherwise.
   `IsEvidenceSurfaceComplete = true` alone never establishes `Complete` (the
   locked asymmetry); a `NotAllowed` repository is resolved, not a deficit.
3. **Limitations**: `LimitedIndependentEvidenceSurface` when the count of
   `IndependentRepetition.IsAllowed` repositories is below
   `RepeatedProfilePattern.MinimumIndependentSupportingRepositories` (reused,
   not a new threshold); `PartialEvidenceSurface` exactly when status is
   `Partial`. Both `Profile` scoped, per §13.
4. **Warnings**: `RepositoryEligibilityUndetermined` from
   `RepositoryEligibilityResult.Status == Undetermined` (one pass, independent
   of outcome); `RepositoryEvidenceRateLimited` / `RepositoryEvidenceUnavailable`
   / `RepositoryAnalysisFailed` from `ProfileRepositoryAnalysisResult.Outcome`
   + `CollectionAvailability.Status` + `Analysis.Status` (a second, independent
   pass) — reproducing the contract's own worked example
   (rate-limited collection + undetermined eligibility → two identities, never
   duplicated for the same repository).

---

## Technical Choices Made

1. **Exceptions as the Failed boundary, deliberately.** `DeveloperPublicEvidenceProfile`'s
   constructor already enforces the version-coherence half of §14 by throwing
   `ArgumentException`. Rather than duplicating that logic, `Compose` performs
   its own additional coherence checks first (repository universe,
   value-divergence, observation/pattern traceability) and then lets
   construction run; any `ArgumentException` — from a pre-check gap or from
   the constructor itself — is caught and mapped to `Failed()`. This is not
   exceptions-as-control-flow for an ordinary branch: `ProfileCompositionResult`
   already exists specifically to represent "coherent truth could not be
   composed," so converting a thrown invariant violation into that exact
   outcome is the boundary the type was built for.
2. **Value equality, not reference equality, for contribution coherence.**
   `RepositoryEligibilityPolicy` and `CrossRepositoryEvidenceComposer` each
   independently call `RepositoryContributionPolicy.Evaluate` today (no
   shared-instance wiring exists yet — that is P8-I5's integration
   responsibility). A coherent composition therefore produces value-equal but
   reference-distinct `RepositoryContributionAssessment` objects for the same
   repository. `ContributionsMatch` compares `Reference`, `PolicyVersion`,
   `Independence`, `AnalysisReference`, and both `ContributionAssessment`
   values (`Decision` + `Reasons`) instead. Reference equality would have
   produced a permanent false-Failed on every correct input.
3. **Reference equality, deliberately, for the Pattern/Observation traces.**
   `RepresentativePatternSupport.Pattern` and each pattern's
   `SupportingObservations` are, by the existing `ProfilePatternPromotionPolicy`/
   `RepresentativeRepositorySelectionPolicy` implementations, literal in-memory
   references into the given `CrossRepositoryEvidence`/`ProfilePatternSet` —
   the representative-repository contract calls this the "Canonical Pattern
   Trace" explicitly. Reference equality (`ReferenceEqualityComparer`) is the
   precise, contract-grounded way to prove "this was actually produced from
   the given input," not an arbitrary strictness choice.
4. **No new runtime check for §14 compatibility conditions 1 and 5**
   ("same snapshot provenance chain" / "independently visible version
   dimensions"). No type in the current pipeline
   (`RepositoryEligibilityResult`, `CrossRepositoryEvidence`,
   `ProfilePatternSet`, `RepresentativeRepositorySelection`) carries a
   `SnapshotIdentity` at all, so condition 1 has no field to check against; in
   this type system it is subsumed by the repository-universe check (condition
   2). Condition 5 is a structural guarantee of the type design (each version
   dimension already lives in its own field) rather than a runtime-failable
   condition. Documented rather than invented as a new check — confirmed by
   independent review as a genuine type-system limitation, not a shortcut.
5. **`ProfileCompositionInput` carries `ProfileAnalysisOrchestrationResult`
   rather than a narrower bundle.** Warning derivation needs
   `ProfileRepositoryAnalysisResult.Outcome`/`CollectionAvailability`, which
   exists only at that layer (P8 contracts never encode rate-limited vs.
   unavailable vs. failed below it). Reusing the existing orchestration result
   avoided inventing a parallel per-repository warning-source type.

---

## Validation

```text
build     GitHubProof.slnx — 0 warning / 0 error
tests     Domain          798 / 798   (unchanged)
          Infrastructure  255 / 255   (unchanged)
          Application     123 / 123   (101 baseline + 22 new)
          Web              68 /  68   (unchanged)
          total          1244 / 1244
git       diff --check clean; no existing src/ or tests/ file modified
```

22 new `ProfileComposerTests` cover: Complete on a sparse fully-resolved
profile; a `NotAllowed` (forked) repository resolved and not a deficit; two
independent repositories with no promoted pattern producing no limitation;
`LimitedIndependentEvidenceSurface` at 0 and 1 independent repositories;
`IndependenceClassification.Undetermined` producing `Partial` +
`RepositoryEligibilityUndetermined`; `IsEvidenceSurfaceComplete = false`
forcing `Partial` even with every repository resolved; `IsEvidenceSurfaceComplete
= true` with an unresolved repository still `Partial` (the locked asymmetry);
each of the four `ProfileWarning` identities from its own canonical
triggering condition, including the rate-limited case producing two
identities at once (the contract's own worked example); `AdmissionUndetermined`
producing only `RepositoryEligibilityUndetermined` (no evidence-availability
warning, since no collection was attempted); a skipped empty repository
producing no warning at all; six coherence-failure scenarios returning
`Failed` with no root — a missing eligibility result, a `CrossRepositoryEvidence`
built over a different repository universe, a value-divergent
admission/contribution for the same repository, a pattern set promoted from a
different (value-equal, reference-distinct) `CrossRepositoryEvidence`, a
representative selection referencing a foreign pattern instance, a version
mismatch between the pattern set and the representative selection, and a
duplicate eligibility reference; and input-order invariance of the whole
composition.

---

## Independent Review

Three rounds via the repository's `github-proof-review` workflow, against the
bounded diff (4 new files, all under `src/GitHubProof.Application/Profile/**`
and `tests/GitHubProof.Application.Tests/Profile/**`):

**Round 1: PASS WITH FINDINGS (1 HIGH).** The composer verified
`RepresentativeRepositorySelection → ProfilePatternSet` traceability (the
"Canonical Pattern Trace") but not the layer below it:
`ProfilePatternSet.Patterns[].SupportingObservations → CrossRepositoryEvidence.Observations`.
Concrete false-negative: a `ProfilePatternSet` promoted from a *different*
(value-equal but reference-distinct) `CrossRepositoryEvidence` — entirely
plausible since `PatternRuleVersion`/`ObservationRuleVersion`/`PolicyVersion`
are stable literals that any two coherent runs share — would pass every
existing check and compose successfully. Exactly the "no silent cross-run
mixing... even when version literals happen to agree" boundary §14 locks.

Fixed by adding `PatternSetTracesToCrossRepositoryEvidence`, the same
reference-equality technique one layer earlier, plus a dedicated test.

**Round 2: PASS WITH FINDINGS (1 MEDIUM).** The round-1 fix's own test used a
hand-built `ObservedProfileEvidence` never drawn from `evidence.Observations`,
so the *new* check caught it for an unintended reason, leaving
`RepresentativeSelectionTracesToPatternSet` (the pre-existing check) with zero
test coverage — a verification-integrity regression, confirmed empirically by
disabling that check and observing all 123 tests still pass.

Fixed by rebuilding that test on the real `ProfilePatternPromotionPolicy`,
promoting twice from the same evidence to get value-equal-but-reference-distinct
`RepeatedProfilePattern` instances, isolating the two checks. Confirmed
empirically both directions (disable each check independently, observe
exactly one test fails, restore, re-verify green).

**Round 3: PASS.** 0 BLOCKER/HIGH/MEDIUM/LOW. Independently repeated the
round-2 isolation experiment and reached the same result; confirmed scope
unchanged (still exactly the 4 files above); confirmed the round-1 fix's
dedicated test is untouched and still isolated.

No finding required a new product/domain semantic decision. No canonical docs
changed by this session or by any review round.

---

## Out of Scope — Confirmed Absent

No report projection, no UI, no persistence, no P9/P10, no orchestration
wiring into the existing profile analysis pipeline (that is P8-I5), no
representative-repository reranking, no pattern recomputation, no capability/
seniority/employability/job-fit semantics anywhere in `ProfileComposer` or its
tests.

## Known Risks

`ProfileComposer` is not yet wired into any running Application composition
path — `CrossRepositoryEvidenceComposer` and `RepositoryEligibilityPolicy`
still run as two independent evaluations of `RepositoryContributionPolicy`
today, which is exactly why value-based (not reference-based) coherence
checking was required in this slice. P8-I5 may choose to share a single
computed `RepositoryContributionAssessment` between them; if it does, the
value-based check here remains correct (a stronger reference-equal case still
satisfies value equality) and needs no revisit.

---

## Working Set / Final Git State

```text
Untracked: src/GitHubProof.Application/Profile/ProfileComposer.cs
           src/GitHubProof.Application/Profile/ProfileCompositionInput.cs
           tests/GitHubProof.Application.Tests/Profile/ProfileComposerTests.cs
           tests/GitHubProof.Application.Tests/Profile/ProfileCompositionTestSupport.cs
Pre-existing unrelated: none
```

No `docs/` file changed. No existing `src/`/`tests/` file modified.

---

## Next

```text
P8-I4 — Typed Profile Report and Projector
```

Depends on P8-I3 (this slice). Full slice plan:
`worklog/sessions/2026-08-19-083-p8-implementation-planning.md`. P8-I4 is a
separate meaningful session and must be routed through its own model/effort
gate.

---

## Git

```text
Suggested commit:
feat(profile): implement deterministic profile composer

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
```

Claude did not commit, push, merge, rebase, tag, release or deploy.

---

## Completion Checklist

```text
[x] Goal tamamlandı: ProfileComposer implementasyonu, independent review PASS
[x] Targeted validation çalıştı (ProfileComposerTests 22/22, filtered runs)
[x] Broader validation çalıştı (full build + full suite 1244/1244)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi — none found
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Kullanıcıya kısa final rapor verildi (Türkçe)
```

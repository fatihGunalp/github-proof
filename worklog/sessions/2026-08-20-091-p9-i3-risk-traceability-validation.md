# P9-I3 — Profile Risk and Traceability Validation

**Date:** 2026-08-20
**Result:** PASS — no canonical production contract violation found across
any of the 11 canonical scenarios; all 9 required risks falsified against
real production output
**Scope:** tests/-only — new
`tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationRiskTests.cs`;
no `src/`, no canonical `docs/` changed
**Model Route:** sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement `p9-implementation-plan.md` §8 P9-I3: run the P9-I2 harness and
its 11 concrete fixtures through the real `ProfilePipeline` and validate,
against real production output only, that the 9 required risks
(`docs/product/p9-profile-validation-contract.md` §3) are falsified, that
every meaningful claim is traceable, that prohibited profile-level
inferences remain structurally absent, and that the `ProblemAreaPatterns`/
`RoleEvidenceAlignment` capability boundary holds. This slice does not own
final determinism or P9 exit closure (P9-I4).

---

## Context Read

```text
Operational:
CLAUDE.md
worklog/CURRENT.md
agents/02-context-loading.md, 04, 06, 07

Canonical:
docs/product/p9-profile-validation-contract.md (full — §2-§8 especially)
docs/product/p9-implementation-plan.md — §8 P9-I3 only

Code (read to derive correct expected production behavior per scenario;
none reimplemented):
src/GitHubProof.Application/Profile/ProfileComposer.cs (full —
  BuildLimitations/BuildWarnings/DetermineAnalysisStatus exact conditions)
src/GitHubProof.Domain/Profile/RepositoryContributionPolicy.cs (full —
  ClassifyLineage: fork/template -> NonIndependent, independent of Archived)
src/GitHubProof.Domain/Profile/RepositoryEligibilityPolicy.cs (full —
  ResolveCurrentFocus: Archived restricts only CurrentFocus, never
  ProfileObservation/IndependentRepetition/HistoricalEvidence)
src/GitHubProof.Domain/Profile/Enums/ObservedProfileEvidenceKind.cs
src/GitHubProof.Domain/Profile/Enums/EligibilityReason.cs
src/GitHubProof.Domain/Profile/Enums/ProfileWarningCode.cs
src/GitHubProof.Domain/Profile/Models/RoleEvidenceAlignment.cs

Existing P9-I2 harness (reused, not duplicated):
tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationScenario.cs
tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationScenarioFixtures.cs
tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationScenarioFixturesTests.cs
  (AllScenarios TheoryData reused via MemberType)
tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationHarness.cs
tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationTraceability.cs
tests/GitHubProof.Application.Tests/Profile/Validation/ProfileValidationCapabilityBoundary.cs
```

Git state verified before mutation: branch
`session/091-p9-i3-risk-traceability-validation`, HEAD = `c195dc9` (matches
the expected Session 091 base — P9-I2's merge commit), working tree clean.

---

## Model Route

```text
Selected:
sonnet / high

Why:
Writing per-scenario assertions that must read real production semantics
(exact BuildLimitations/BuildWarnings/lineage-classification conditions)
precisely enough to state a correct expectation — a wrong expectation would
either mask a real defect or falsely report one. The task's STOP protocol
for a proven contract violation raised the cost of a wrong read further.
```

---

## Work Performed

Added `ProfileValidationRiskTests.cs` under
`tests/GitHubProof.Application.Tests/Profile/Validation/`. No existing
harness file was modified; nothing was duplicated — every test runs the
existing `ProfileValidationScenarioFixtures.Build(...)` fixture through the
existing `ProfileValidationHarness.RunAsync(...)` (the real
`ProfilePipeline`), and reuses the existing `ProfileValidationTraceability`/
`ProfileValidationCapabilityBoundary` helpers unchanged.

### Per-scenario risk assertions (11 `[Fact]` methods, one per scenario)

Each method is named and commented with the contract §3 risk(s) it
falsifies, reading only structured production output (patterns,
observations, eligibility results, limitations, warnings, report
sections):

- **DotNetHeavy / FrontendHeavy (R1, R4):** the minority-language repository
  (1 independent repo, below the 2-repository promotion threshold) is never
  promoted; the majority pattern's supporting observations are all
  `ObservedProfileEvidenceKind.RepositoryLanguageEvidence` (quantified
  structured evidence), never README-text-derived.
- **MixedFullStack (R1, R5):** both technical areas promote independently
  (2 independent repositories each); their `IndependentSupportingRepositories`
  sets are disjoint — no single repository backs, and so cannot dominate,
  more than one area.
- **AiMlHeavy (R1, R4):** the promoted pattern key is the raw language
  literal `"Python"`, never an invented `"AI/ML"` label — confirmed
  contractually correct (`ProfilePatternPromotionPolicy`: "no grouping,
  aliasing or framework inference"), and directly demonstrates the
  documented F2 production-wiring input from Session 090 as an actual,
  observed behavior rather than a hypothesis.
- **FewRepositories (R5, R7, R9):** both of the 2 repositories count as
  independent support (`IndependentSupportingRepositoryCount == 2`, no
  single repository stands in alone); at exactly the promotion threshold no
  `LimitedIndependentEvidenceSurface` limitation is manufactured (the
  surface is sufficient, not "limited"); `AllMeaningfulClaimsAreTraceable`
  holds.
- **ManyRepositories (R5, R8):** the one genuinely unavailable
  (rate-limited) repository is marked `RepositoryEligibilityStatus.Undetermined`
  and carries exactly its two canonical warnings
  (`RepositoryEvidenceRateLimited`, `RepositoryEligibilityUndetermined`) —
  never silently dropped, never merged as either positive or negative
  evidence — while the two resolved technical areas (C#, TypeScript) still
  promote correctly from their own available repositories, undisturbed.
- **MostlyForks (R2, R9):** 2 of 3 repositories are forks, leaving only 1
  independent repository — below threshold — so **no** `TechnicalFocus`
  pattern is promoted at all; forks did not inflate repetition to reach the
  minimum. `LimitedIndependentEvidenceSurface` is correctly present. With no
  meaningful claims produced, traceability holds vacuously (no orphan claim
  leaked).
- **MostlyArchived (R6, R8):** all 3 repositories (1 active + 2 archived)
  support the promoted pattern (`IndependentSupportingRepositoryCount == 3`)
  — archived repositories are never excluded from independent evidence.
  For the 2 archived repositories, `HistoricalEvidence.IsAllowed` and
  `IndependentRepetition.IsAllowed` stay `true`; only `CurrentFocus.IsAllowed`
  is `false`, with reason `Archived` — archived restricts current-focus
  only, never "failure"/"low quality". The profile stays `Complete` with no
  warnings — archived is never confused with unavailable.
- **StudentTutorialHeavy (R1, R3, R4):** 2 of 3 repositories are
  template-derived, leaving 1 independent repository — below threshold —
  so no `TechnicalFocus`/`EngineeringPractice` pattern is promoted from mere
  structural template repetition; `LimitedIndependentEvidenceSurface` is
  correctly present.
- **Polyglot (R1, R5):** all 3 independent technical areas (C#, TypeScript,
  Python) promote with `IndependentSupportingRepositoryCount == 2` each,
  and no repository is shared across two areas — none dominates.
- **SparsePublicProfile (R7, R8, R9):** the one fully-resolved repository
  keeps the profile `Complete` (never demoted to `Partial`, never given any
  negative-judgment marker — none exists on the type) with exactly
  `LimitedIndependentEvidenceSurface` and **no** warnings — proving "sparse
  but resolved" is structurally distinct from "unavailable" (the
  `ManyRepositories` case). No pattern is falsely promoted from one
  repository; traceability holds.

### Cross-scenario checks (`[Theory]` over all 11 identities, reusing
`ProfileValidationScenarioFixturesTests.AllScenarios`)

- `EveryScenario_AllMeaningfulClaimsAreTraceable` —
  `ProfileValidationTraceability.AllMeaningfulClaimsAreTraceable` holds for
  every one of the 11 real fixtures' real pipeline output (not just the one
  ad-hoc fixture P9-I2's self-proof used).
- `EveryScenario_ProblemAreaStaysUnsupportedAndRoleEvidenceStaysDeferred` —
  `ProfileValidationCapabilityBoundary.HoldsFor` holds for every scenario:
  `ProblemAreaPatterns` stays `Unsupported + []`, `RoleEvidenceAlignment`
  stays `Deferred + []` in every composition, confirming
  `Unsupported != Evidence Absence`, `Deferred != Negative Result` across
  the full scenario matrix, not just one fixture.

### Prohibited-inference check

`ProfileAndReportTypes_ExposeNoProhibitedInferenceProperty`: a bounded,
reflection-based structural scan (the same technique already established by
`ProfilePipelineTests.PipelineResult_PublicApi_...`) over the 14 canonical
profile/report types, asserting no public property name resembles a
prohibited claim (score, rank, junior/senior, employability, hire,
capability, confidence, salary, years-of-experience, authorship,
originality, AI-written, security, production-ready, architecture-quality,
talent, intelligence, quality, strength). No NLP/LLM/string-heuristic
scanner was built — the report contract already states the product surface
carries no free-form inference text to scan (report contract §2: "It
defines presentation projection only"), so the correct bounded check is
over the type shape itself, not generated text.

`Supported + empty is not interpreted as negative developer evidence` was
verified directly inline wherever a scenario legitimately produces an empty
`TechnicalFocusAreas`/`EngineeringPracticePatterns` section (MostlyForks,
StudentTutorialHeavy, SparsePublicProfile): each asserts
`Availability == Supported` alongside the empty `Items`, matching report
contract §6.1 exactly.

### Two test-authoring corrections applied before finalizing (not production
defects)

While first running the new tests, two assertions were found to be
incorrect in the test itself, not in production behavior, and were
corrected in place before this session's final validation run:

1. `MostlyForks`'s fork-repository filter used
   `Reference.Name.Contains("fork")`, which also matched the *original*
   repository's name (`mostly-forks-original`, since the fixture's own
   scenario prefix contains "fork"). Corrected to the unambiguous
   `Contains("-fork-")` (and, for consistency, `StudentTutorialHeavy`'s
   template filter to `Contains("-template-")`).
2. `ManyRepositories`'s expected warning-code order literal
   (`[RepositoryEligibilityUndetermined, RepositoryEvidenceRateLimited]`)
   did not match `ProfileWarningCode`'s actual declared enum order
   (`RepositoryEvidenceRateLimited = 2` before
   `RepositoryEligibilityUndetermined = 4`); corrected to match.

Both were caught and fixed by running the tests themselves — no production
code was touched to make either pass.

**No canonical production contract violation was found in any of the 11
scenarios.** Every required risk was falsified against the real,
unmodified `ProfilePipeline`; production code was not touched.

---

## Result Summary

```text
Scenarios executed: 11 / 11
Required risks covered: 9 / 9
  R1 false technical focus              — DotNetHeavy, FrontendHeavy,
                                           MixedFullStack, AiMlHeavy,
                                           StudentTutorialHeavy, Polyglot
  R2 fork inflation                     — MostlyForks
  R3 template inflation                 — StudentTutorialHeavy
  R4 README keyword inflation           — DotNetHeavy, FrontendHeavy,
                                           AiMlHeavy, StudentTutorialHeavy
  R5 single-repository dominance        — MixedFullStack, FewRepositories,
                                           ManyRepositories, Polyglot
  R6 archived-repository distortion     — MostlyArchived
  R7 sparse-profile negative judgment   — FewRepositories,
                                           SparsePublicProfile
  R8 missing/unavailable data confusion — ManyRepositories, MostlyArchived,
                                           SparsePublicProfile
  R9 broken evidence traceability       — FewRepositories, MostlyForks,
                                           SparsePublicProfile (+ all 11 via
                                           the cross-scenario theory)

Traceability failures: 0
Prohibited-inference failures: 0
Unsupported-claim failures: 0 (ProblemAreaPatterns/RoleEvidenceAlignment
  boundary held on all 11 scenarios)
Proven production defect: NONE
```

---

## Validation

```bash
dotnet build tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj
dotnet test tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj \
  --filter "FullyQualifiedName~ProfileValidationRiskTests"
dotnet test tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj
dotnet build GitHubProof.slnx
dotnet test GitHubProof.slnx
git diff --check
git status --short
git diff --stat
```

Results:
- Targeted (`ProfileValidationRiskTests`): 34/34 PASS (11 per-scenario risk
  facts + 11 traceability theory cases + 11 capability-boundary theory
  cases + 1 reflection fact).
- `GitHubProof.Application.Tests`: 224/224 PASS (190 prior + 34 new; no
  test removed or modified elsewhere).
- Full solution (`GitHubProof.slnx`): 1353/1353 PASS (Application 224,
  Domain 798, Infrastructure 259, Web 72) — 0 build warning, 0 error.
- `git diff --check`: clean.
- `git status --short`: 1 untracked file only
  (`ProfileValidationRiskTests.cs`) — no modified file at all this slice.
- `git diff --stat`: empty (the sole change is a new, untracked file).

---

## Independent Review

Run via `github-proof-review`. **Verdict: PASS** — 0 BLOCKER, 0 HIGH,
0 MEDIUM, 0 LOW. Independently re-ran `dotnet test`
(`--filter ProfileValidationRiskTests`: 34/34) and the full solution
(1353/1353, 0 warning), confirmed `git diff c195dc9 HEAD --stat` is empty
(all changes are uncommitted working-tree state, matching "Commit: NOT
CREATED"), confirmed no `src/`/canonical `docs/` change, cross-checked all
11 scenario → risk mappings against contract §3 verbatim (no invented
mapping), verified `MinimumIndependentSupportingRepositories = 2` and the
`ProfileWarningCode` enum order independently against source, and confirmed
the `MostlyForks`/`StudentTutorialHeavy` fixture naming collision that
prompted the pre-finalization test fix was a genuine test-authoring bug, not
a production defect. No contract/invariant drift, no layer-boundary
violation, no unnecessary abstraction or dependency added; determinism
correctly left to P9-I4.

---

## Preservation

```text
Production code changed: NO
Production semantics changed: NO
P9-I4 started: NO
P10 started: NO
Commit: NOT CREATED
Push: NOT PERFORMED
```

---

## Next

If PASS: `P9-I4 — Determinism + Exit Validation` is the only authorized
next implementation slice.

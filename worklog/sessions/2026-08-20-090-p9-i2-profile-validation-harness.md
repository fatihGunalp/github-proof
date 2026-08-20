# P9-I2 — Profile Validation Fixtures / Harness

**Date:** 2026-08-20
**Result:** PASS (Round 2 independent review: PASS WITH FINDINGS, 1 LOW,
non-blocking, no correction needed) — F1 resolved with 11 concrete
deterministic scenario fixtures (this continuation); F2 remains accepted
non-blocking, unchanged, documented as a P9-I3 input — see Independent
Review. Bounded,
deterministic, network-free harness added under
`tests/GitHubProof.Application.Tests/Profile/Validation/`, running the
canonical P9 scenarios' concrete fixtures through the real production
`ProfilePipeline`; `CreatePipeline`/`AssertSemanticallyEquivalent` extracted
from `ProfilePipelineTests` into the shared `ProfileCompositionTestSupport`
so no second implementation of either exists
**Scope:** tests/-only — no `src/`, no canonical `docs/` changed
**Model Route:** sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement `p9-implementation-plan.md` §8 P9-I2: build validation
infrastructure — not risk assertions (P9-I3) or exit proof (P9-I4) — capable
of constructing and executing all 11 canonical P9 scenarios
(`docs/product/p9-profile-validation-contract.md` §2) through the real
`ProfilePipeline`, with reusable traceability (§6), unsupported/deferred (§7)
and determinism (§8.1) helpers, proven by a bounded self-test rather than
left as unused scaffolding.

---

## Context Read

```text
Operational:
CLAUDE.md
worklog/CURRENT.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md

Canonical:
docs/product/p9-profile-validation-contract.md (full — §2-§10 especially)
docs/product/p9-implementation-plan.md — §8 P9-I2 only

Code (real production types the harness must drive, read only, not
reimplemented):
src/GitHubProof.Application/Profile/ProfilePipeline.cs
src/GitHubProof.Application/Profile/ProfilePipelineResult.cs
src/GitHubProof.Domain/Profile/ProfilePatternPromotionPolicy.cs
src/GitHubProof.Domain/Profile/RepositoryContributionPolicy.cs
src/GitHubProof.Domain/Profile/RepositoryEligibilityPolicy.cs
src/GitHubProof.Domain/Profile/Models/ProfileRepository.cs
src/GitHubProof.Domain/Profile/Models/DeveloperPublicEvidenceProfile.cs
src/GitHubProof.Domain/Profile/Models/CrossRepositoryEvidence.cs
src/GitHubProof.Domain/Profile/Models/ObservedProfileEvidence.cs
src/GitHubProof.Domain/Profile/Models/RepeatedProfilePattern.cs
src/GitHubProof.Domain/Profile/Models/RepresentativeRepository.cs
src/GitHubProof.Domain/Profile/Models/RepresentativePatternSupport.cs
src/GitHubProof.Domain/Profile/Models/RepositoryEligibilityResult.cs
src/GitHubProof.Domain/Profile/ValueObjects/ProfileEvidenceReference.cs
src/GitHubProof.Application/Profile/Report/ProfileReport.cs
src/GitHubProof.Application/Profile/Report/ProfileReportProjector.cs
src/GitHubProof.Application/Profile/Report/PatternReportItem.cs
src/GitHubProof.Application/Profile/Report/ConclusionEvidenceReportItem.cs
src/GitHubProof.Application/Profile/Report/RepositoryDrillDownReportItem.cs
docs/architecture/public-profile-evidence-report-contract.md — §5-§7 only
  (report section set/availability, cross-checked while reading the
  ProfileReport model above)

Existing test support (reused, not duplicated):
tests/GitHubProof.Application.Tests/Profile/ProfileAnalysisTestSupport.cs
tests/GitHubProof.Application.Tests/Profile/ProfileCompositionTestSupport.cs
tests/GitHubProof.Application.Tests/Profile/ProfilePipelineTests.cs
tests/GitHubProof.Application.Tests/Profile/Report/ProfileReportTestSupport.cs
```

`docs/product/validation-repository-set.md` / `samples/validation/` were not
re-read this session — their reuse boundary was already locked by P9-I1
contract §9 and did not need re-derivation; this slice does not touch them.

Git state verified before mutation: branch
`session/090-p9-i2-profile-validation-harness`, HEAD =
`4a1bc5a` (matches the expected Session 090 base, session 089's docs commit),
working tree clean.

---

## Model Route

```text
Selected:
sonnet / high

Why:
Real test/harness code against production types, where a wrong turn (a
second traceability model, a stubbed pipeline stage, an independently
drifting determinism check) would silently violate the just-locked P9-I1
contract. Required reading ~15 production types precisely to derive a
traceability check that matches ProfileComposer's own construction-time
invariants rather than inventing a new one.
```

---

## Work Performed

### 1. Extraction (no behavior change)

Moved `CreatePipeline(...)` and `AssertSemanticallyEquivalent(...)` from
`ProfilePipelineTests.cs` (private) into `ProfileCompositionTestSupport.cs`
(public), so both `ProfilePipelineTests` and the new harness use the exact
same real-`ProfilePipeline` construction and the exact same P9 §8.1
determinism comparison — never two independently maintained copies.
`ProfilePipelineTests.cs` now calls both unqualified via its existing
`using static ProfileCompositionTestSupport;`. No test assertion changed; all
9 pre-existing `ProfilePipelineTests` facts still pass unmodified.

### 2. New harness (`tests/GitHubProof.Application.Tests/Profile/Validation/`)

- `ProfileValidationScenario.cs` — the 11 canonical scenario identities as an
  enum (contract §2), labels only.
- `ProfileValidationScenarioCatalog.cs` — data-only dictionary mapping each
  identity to its composition specification (verbatim from contract §2),
  proving representability without claiming validation.
- `ProfileValidationHarness.RunAsync(...)` — a one-line named entry point
  onto `ProfileCompositionTestSupport.CreatePipeline(...).RunAsync(...)`, so
  a scenario run and a P8 integration-test run are provably the same
  production wiring. No new result type: it returns the real
  `ProfilePipelineResult` as-is (Status, Profile, Report all already carry
  everything §10 requires — eligibility, limitations, warnings, patterns,
  representative repositories, section availability, WhyThisConclusion,
  drill-down).
- `ProfileValidationTraceability` — the single §6.1/§6.2 PASS/FAIL check for
  the four meaningful claim surfaces (TechnicalFocusAreas,
  EngineeringPracticePatterns, RepresentativeRepositories,
  WhyThisConclusion), implemented with the exact same reference-identity
  technique (`ReferenceEqualityComparer.Instance`) `ProfileComposer` and
  `ProfileReportProjector` already use in production — not a second trace
  model.
- `ProfileValidationCapabilityBoundary` — the §7 check that
  `ProblemAreaPatterns` stays `Unsupported + []` and `RoleEvidenceAlignment`
  stays `Deferred + []`.
- `ProfileValidationHarnessTests.cs` — the bounded self-proof (see below).

### 3. Fixtures

No new fixture abstraction layer was introduced. `CreateRepository`
(fork/template/archived/empty parameters) plus `CreateSource`/
`CreateLanguageEvidenceSnapshot`/`CreateUnavailable` (already in
`ProfileAnalysisTestSupport`/`ProfileCompositionTestSupport`) are already
general enough to compose any of the 11 scenarios (many/few repositories,
mostly forks, mostly archived, sparse profile, multi-language polyglot,
etc.) — confirmed by inspection of
`RepositoryContributionPolicy.ClassifyLineage` (fork/template →
`IndependenceClassification.NonIndependent`, which is exactly what a
"mostly forks"/"mostly archived" scenario needs to falsify against in
P9-I3). Building a second builder layer on top would have duplicated
existing, already-correct construction rather than adding capability, so
none was added — `ProfileValidationHarnessTests`'s one fixture helper
(`TwoIndependentCSharpRepositories`) demonstrates the pattern P9-I3 will
repeat per scenario.

### 4. Minimum Slice Proof

`ProfileValidationHarnessTests.cs` proves, against a fixed two-independent-
repository C# composition run through the real pipeline:

1. `RunAsync_...ExecutesThroughRealPipelineAndExposesFullInspectionSurface`
   — proof items 1+2 (real pipeline run; full inspection surface: status,
   eligibility, limitations, warnings, patterns, representative
   repositories, 9 report sections, WhyThisConclusion, drill-down all
   present).
2. `Traceability_OnARealPipelineResult_HoldsForEveryMeaningfulClaim` — proof
   item 3.
3. `Traceability_RejectsAClaimNotBelongingToTheProfile` — the helper is not
   vacuously true: a claim from an independent pipeline run fails against a
   different profile.
4. `CapabilityBoundary_OnARealPipelineResult_...` — proof item 4.
5. `Determinism_TwoIndependentRunsOverTheSameFixedInput_...` — proof item 5.
6. `RepositoryOrder_DoesNotAlterTheSemanticResult` — proof item 6.
7. `ScenarioCatalog_ContainsAllElevenCanonicalIdentities` — representability
   coverage (11/11).

Item 7 (normal tests remain network-free) is a structural fact: the harness
and its fixtures only ever construct in-memory
`FakeSnapshotSource`/`GitHubProfileSnapshot` objects; no `HttpClient` or
`IGitHubClient` is referenced anywhere in the new files.

No production code changed. No canonical `docs/` changed.

### 5. F1 correction (same session, post-review continuation)

Independent review Finding F1 (below) held that contract §10 item 1 requires
11 concrete deterministic scenario fixtures, not only a representable
fixture model plus a textual catalog. Resolved by adding
`ProfileValidationScenarioFixtures.cs`: one `Build(ProfileValidationScenario)`
switch expression with 11 private builder methods (`DotNetHeavy`,
`FrontendHeavy`, `MixedFullStack`, `AiMlHeavy`, `FewRepositories`,
`ManyRepositories`, `MostlyForks`, `MostlyArchived`,
`StudentTutorialHeavy`, `Polyglot`, `SparsePublicProfile`), each returning a
fresh, deterministic `(GitHubProfileSnapshot, FakeSnapshotSource)` pair built
entirely from the existing `CreateRepository`/`CreateSource`/
`CreateLanguageEvidenceSnapshot`/`CreateReadmeEvidenceSnapshot`/
`CreateUnavailable` building blocks — no new production-semantics
reimplementation, only composition data. Three small shared builders
(`LanguageMajority`, `MultiLanguageRepositories`, `RepositoriesWithLanguages`)
factor out the repeated "N independent repositories across M technical
areas" shape so the 11 scenarios do not each hand-roll snapshot/source
wiring:

- `DotNetHeavy`/`FrontendHeavy`/`AiMlHeavy`: 3 repositories, 2 sharing the
  scenario's dominant language, 1 minority-language repository.
- `MixedFullStack`: 2 independent areas (C#, TypeScript), 2 repositories
  each.
- `Polyglot`: 3 independent areas (C#, TypeScript, Python), 2 repositories
  each, none dominant.
- `FewRepositories`: exactly 2 independent repositories (the
  `RepeatedProfilePattern.MinimumIndependentSupportingRepositories` floor).
- `ManyRepositories`: 5 repositories, 4 collected across 2 languages plus 1
  genuinely `CreateUnavailable(DataAvailabilityStatus.RateLimited)`
  collection outcome.
- `MostlyForks`/`MostlyArchived`: 3 repositories, 2 with `Fork`/`Archived =
  Present` respectively, 1 without.
- `StudentTutorialHeavy`: 3 repositories, 2 with `TemplateDerived = Present`
  (the closest existing production lineage concept to tutorial/boilerplate
  repetition), README-only evidence.
- `SparsePublicProfile`: 1 repository, README-only evidence — the smallest
  possible eligible+collectible surface.

`ProfileValidationScenarioFixturesTests.cs` proves, per scenario (via
`[Theory]`/`TheoryData<ProfileValidationScenario>` over all 11 identities):

1. `FixtureInventory_CoversAllElevenCanonicalScenarios` — `Build(...)`
   handles every one of the 11 enum values without throwing and each
   produces at least one repository.
2. `Fixture_IsConstructedDeterministically` — two independent `Build(...)`
   calls for the same scenario normalize to the same
   `SnapshotIdentity.FromSemanticContent(...)` digest. This reuses the
   production determinism mechanism itself
   (`docs/architecture/public-profile-evidence-domain-model.md` §6) rather
   than a second, independently-defined comparison.
3. `Fixture_ExecutesThroughRealPipelineWithoutNetworkAccess` — the fixture
   runs through `ProfileValidationHarness.RunAsync` (the real
   `ProfilePipeline`) without throwing, and the result's
   `HasProfile`/`Report` pairing invariant holds. No risk, pattern or claim
   content is asserted — only structural well-formedness, so this does not
   claim any P9 risk is validated.

`ProfileValidationScenario` was widened from `internal` to `public` (the
only accessibility change in this correction): xUnit's `[Theory]`/
`TheoryData<T>` require the data type to be at least as accessible as the
public test methods consuming it. No other type's accessibility changed.

---

## Scenario Readiness

```text
Scenario fixtures ready: 11 / 11
P9 risk scenarios validated: 0 / 11
```

All 11 fixtures are concrete, deterministic (proven by matching
`SnapshotIdentity`), and execute through the real `ProfilePipeline` without
network access. None of the 9 required P9 risks
(`docs/product/p9-profile-validation-contract.md` §3) has been asserted
against any scenario's output — that is P9-I3's responsibility, not claimed
complete here.

---

## Validation

```bash
dotnet build tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj
dotnet test tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj \
  --filter "FullyQualifiedName~Profile.Validation"
dotnet test tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj
dotnet build GitHubProof.slnx
dotnet test GitHubProof.slnx
git diff --check
git status --short
git diff --stat
```

Results (F1-correction re-run):
- Targeted (`Profile.Validation`, all 3 test files): 30/30 PASS, covering
  all 11 scenarios × 2 theories (determinism, real-pipeline execution) plus
  the inventory/catalog/harness-self-proof facts.
- `GitHubProof.Application.Tests`: 190/190 PASS (167 prior + 23 new: 1
  inventory fact + 11 determinism theory cases + 11 execution theory cases;
  no test removed).
- Full solution (`GitHubProof.slnx`): 1319/1319 PASS (Application 190,
  Domain 798, Infrastructure 259, Web 72) — 0 build warning, 0 error.
- `git diff --check`: clean.
- `git status --short`: 4 modified (`ProfileCompositionTestSupport.cs`,
  `ProfilePipelineTests.cs`, `worklog/CURRENT.md`, `worklog/INDEX.md`), 2
  untracked (`tests/.../Profile/Validation/` — now 6 files — and this
  session file).
- `git diff --stat`: 117 insertions / 82 deletions across the 4 modified
  files (the new/untracked `Validation/` directory does not appear in
  `git diff --stat` against `HEAD`). No `src/`, no canonical `docs/`
  changed.

Proof
```text
real ProfilePipeline used: YES
network-free: YES
traceability helper proven: YES (positive + negative case)
Unsupported/Deferred helper proven: YES
determinism helper proven: YES
fixture inventory: 11 / 11
fixture determinism (SnapshotIdentity match): 11 / 11
fixture real-pipeline execution (no exception, well-formed result): 11 / 11
```

---

## Independent Review

### Round 1 (initial P9-I2 delivery)

Verdict: **PASS WITH FINDINGS** — 0 BLOCKER, 0 HIGH, 2 MEDIUM, 0 LOW.
Independently re-verified `dotnet build`/`dotnet test` on `GitHubProof.slnx`
(1296/1296, 0 warning) and `git diff --check`/`git status --short`, matching
this file's own validation section; confirmed `src/` and canonical `docs/`
untouched; confirmed the `CreatePipeline`/`AssertSemanticallyEquivalent`
extraction is behavior-preserving; `ProfileValidationTraceability`
cross-checked directly against `ProfileComposer`'s and
`ProfileReportProjector`'s own reference-identity invariants; the
11-identity enum/catalog cross-checked 11/11 against contract §2.

- **F1 (MEDIUM) — §10 literal-fixture vs. delivered representability.**
  Contract §10 item 1 reads as requiring 11 concrete executable fixtures;
  the initial delivery provided one concrete reusable fixture pattern
  (`TwoIndependentCSharpRepositories`) plus an 11-identity textual
  composition catalog only.
- **F2 (MEDIUM) — `AI/ML-heavy` is not distinguishable via current
  production wiring.** `ProfilePatternPromotionPolicy.PromoteTechnicalFocus`
  (Domain) promotes `TechnicalFocus` only from raw
  `RepositoryLanguageEvidence` (the GitHub-reported language literal); it
  never reads `DependencySemanticCategoryEvidence`, so a "Python + ML
  libraries" composition and a plain "Python" composition produce the
  identical generic `"Python"` pattern under today's production pipeline.

### F1 resolution (this continuation)

Per explicit user instruction, F1 only was resolved — not by documentation,
but by adding `ProfileValidationScenarioFixtures.cs` (11 concrete
deterministic fixture builders) and
`ProfileValidationScenarioFixturesTests.cs` (proving inventory 11/11,
determinism 11/11 via `SnapshotIdentity`, real-pipeline execution 11/11 —
see "F1 correction" above). No P9-I3 risk assertion was added; no production
code changed.

F2 is unchanged and intentionally left as an explicit, documented P9-I3
input (`AiMlHeavy()` in `ProfileValidationScenarioFixtures.cs` carries this
same note inline) — not resolved, not silently dropped, per this
continuation's explicit instruction.

### Round 2 (F1-correction re-review)

Run via `github-proof-review` after the F1 correction. Verdict: **PASS WITH
FINDINGS** — 0 BLOCKER, 0 HIGH, 0 MEDIUM, 1 LOW (non-blocking). F1 confirmed
resolved (11 concrete fixtures, contract §2 cross-checked 11/11, traceability/
capability-boundary logic re-verified against `ProfileComposer`'s own
mechanism). F2 confirmed intentionally untouched and still documented
(inline in `AiMlHeavy()` and in this file). Independently re-verified
`dotnet build`/`dotnet test` on `GitHubProof.slnx` (1319/1319, 0 warning),
`git diff --check`, and that `src/`/canonical `docs/` remain untouched.

**LOW finding (accepted, no correction needed):** a pre-existing, Session
089-authored wording tension between
`docs/product/p9-profile-validation-contract.md` §10 items 2-3 (risk-check
hooks, prohibited-claim scan, labeled there as P9-I2 handoff items) and
`docs/product/p9-implementation-plan.md` §8, which assigns risk-coverage and
the broader prohibited-inference scan to P9-I3 explicitly. This session
followed the more detailed, explicit P9-I2 scope in the implementation plan
(items 1/4/5/6: fixtures, traceability, unsupported/deferred, determinism)
and built no risk-check/prohibited-claim-scan hook, per this continuation's
own explicit instruction not to add P9-I3 risk assertions. Not a defect in
this diff — a pre-existing cross-document wording gap, worth resolving
before P9-I3 starts but not blocking this slice. No code or contract text
changed in response; carried forward as a P9-I3 handoff note.

---

## Preservation

```text
Production code changed: NO
Production semantics changed: NO
P9-I3 started: NO
P10 started: NO
Commit: NOT CREATED
Push: NOT PERFORMED
```

---

## Next

`P9-I3 — Profile Risk and Traceability Validation` is the only authorized
next implementation slice. Before or at its start, the Round 2 LOW finding
(the §10 vs. implementation-plan §8 wording tension over which document owns
"P9-I2 risk-check hooks"/"prohibited-claim scan") should be resolved so
P9-I3 does not inherit an ambiguous handoff — but this is a documentation
clarification, not a blocker to starting P9-I3 itself.

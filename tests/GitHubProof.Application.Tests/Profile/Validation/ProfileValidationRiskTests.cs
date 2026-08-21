using System.Reflection;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Profile.Validation;

// P9-I3 — Profile Risk and Traceability Validation
// (docs/product/p9-profile-validation-contract.md §3-§7;
// docs/product/p9-implementation-plan.md §8 P9-I3).
//
// Runs each of the 11 concrete P9-I2 fixtures
// (ProfileValidationScenarioFixtures) through the real ProfilePipeline
// (ProfileValidationHarness) and asserts that scenario's mapped required
// risk(s) are falsified against the real production output — never against
// a reimplementation. Every assertion reads only structured production
// output already produced by the unmodified pipeline (patterns,
// observations, eligibility results, limitations, warnings, report
// sections); no new inference, threshold or taxonomy is introduced here.
//
// This file does not own final determinism or P9 exit closure — that is
// P9-I4. A scenario proving a canonical contract violation is not
// "corrected" here: production code is not touched by this file.
public sealed class ProfileValidationRiskTests
{
    // ---------------------------------------------------------- R1/R4: DotNetHeavy

    [Fact]
    public async Task DotNetHeavy_PromotesOnlyTheMajorityLanguage_FromStructuredEvidenceNotReadmeText()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.DotNetHeavy);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // R1 (false technical focus): the minority language (1 independent
        // repository, below the promotion threshold) must not be promoted.
        Assert.DoesNotContain(
            result.Profile!.RepeatedProfilePatterns,
            pattern => pattern.PatternKey == "JavaScript");

        // R4 (README keyword inflation): the promoted majority pattern must
        // rest on quantified RepositoryLanguageEvidence, never on README
        // text presence.
        RepeatedProfilePattern pattern = Assert.Single(
            result.Profile.RepeatedProfilePatterns,
            p => p.Family == ProfilePatternFamily.TechnicalFocus);
        Assert.Equal("C#", pattern.PatternKey);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
        Assert.All(
            pattern.SupportingObservations,
            observation => Assert.Equal(
                ObservedProfileEvidenceKind.RepositoryLanguageEvidence,
                observation.Kind));
    }

    // ------------------------------------------------------ R1/R4: FrontendHeavy

    [Fact]
    public async Task FrontendHeavy_PromotesOnlyTheMajorityLanguage_FromStructuredEvidenceNotReadmeText()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.FrontendHeavy);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Assert.DoesNotContain(
            result.Profile!.RepeatedProfilePatterns,
            pattern => pattern.PatternKey == "Python");

        RepeatedProfilePattern pattern = Assert.Single(
            result.Profile.RepeatedProfilePatterns,
            p => p.Family == ProfilePatternFamily.TechnicalFocus);
        Assert.Equal("TypeScript", pattern.PatternKey);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
        Assert.All(
            pattern.SupportingObservations,
            observation => Assert.Equal(
                ObservedProfileEvidenceKind.RepositoryLanguageEvidence,
                observation.Kind));
    }

    // ------------------------------------------------------- R1/R5: MixedFullStack

    [Fact]
    public async Task MixedFullStack_PromotesBothAreasIndependently_WithNoSharedDominatingRepository()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.MixedFullStack);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        RepeatedProfilePattern backend = Assert.Single(
            result.Profile!.RepeatedProfilePatterns,
            p => p.PatternKey == "C#");
        RepeatedProfilePattern frontend = Assert.Single(
            result.Profile.RepeatedProfilePatterns,
            p => p.PatternKey == "TypeScript");

        // R1: each independent technical area is promoted on its own
        // repetition evidence (2 independent repositories each).
        Assert.Equal(2, backend.IndependentSupportingRepositoryCount);
        Assert.Equal(2, frontend.IndependentSupportingRepositoryCount);

        // R5 (single-repository dominance): no repository backs both areas —
        // one repository never inflates or dominates a second, unrelated
        // claim.
        Assert.Empty(
            backend.IndependentSupportingRepositories.Intersect(
                frontend.IndependentSupportingRepositories));
    }

    // ------------------------------------------------------------ R1/R4: AiMlHeavy

    [Fact]
    public async Task AiMlHeavy_PromotesOnlyTheRawLanguageLiteral_NeverAnInventedFrameworkLabel()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.AiMlHeavy);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Assert.DoesNotContain(
            result.Profile!.RepeatedProfilePatterns,
            pattern => pattern.PatternKey == "Go");

        RepeatedProfilePattern pattern = Assert.Single(
            result.Profile.RepeatedProfilePatterns,
            p => p.Family == ProfilePatternFamily.TechnicalFocus);

        // R1/R4, and the documented F2 production-wiring input (P9-I2
        // session 090): the pattern key is the raw GitHub language literal
        // "Python" — never an invented "AI/ML" or "Machine Learning" label.
        // This is the contractually correct behavior
        // (ProfilePatternPromotionPolicy: "no grouping, aliasing or
        // framework inference"), not a defect — it is exactly what falsifies
        // false technical focus here: the system does not claim a
        // capability/domain it cannot structurally support.
        Assert.Equal("Python", pattern.PatternKey);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
    }

    // --------------------------------------------------- R5/R7/R9: FewRepositories

    [Fact]
    public async Task FewRepositories_BothRepositoriesCountIndependently_NoLimitationAtTheThreshold()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.FewRepositories);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        RepeatedProfilePattern pattern = Assert.Single(result.Profile!.RepeatedProfilePatterns);

        // R5 (single-repository dominance): both of the two repositories
        // count as independent support — neither is a lone repository
        // standing in for the pattern.
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);

        // Exactly at RepeatedProfilePattern.MinimumIndependentSupportingRepositories,
        // the evidence surface is sufficient, not "limited" — the profile
        // must not manufacture a negative-judgment limitation merely
        // because the repository count is small (R7).
        Assert.Empty(result.Profile.Limitations);

        // R9 (broken evidence traceability).
        Assert.True(
            ProfileValidationTraceability.AllMeaningfulClaimsAreTraceable(
                result.Profile,
                result.Report!));
    }

    // -------------------------------------------------------- R5/R8: ManyRepositories

    [Fact]
    public async Task ManyRepositories_UnavailableRepositoryIsMarkedNotSilentlyAbsorbed_OtherPatternsUnaffected()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.ManyRepositories);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // R8 (missing/unavailable data confusion): the unresolved repository
        // is explicitly marked Undetermined and produces its own warnings —
        // it is not silently dropped, and it is not treated as either
        // positive or negative evidence.
        Assert.Equal(ProfileCompositionStatus.Partial, result.Status);
        Assert.Contains(
            ProfileLimitationCode.PartialEvidenceSurface,
            result.Profile!.Limitations.Select(limitation => limitation.Code));

        RepositoryEligibilityResult unresolved = Assert.Single(
            result.Profile.RepositoryEligibilityResults,
            r => r.Status == RepositoryEligibilityStatus.Undetermined);
        Assert.Equal(
            [ProfileWarningCode.RepositoryEvidenceRateLimited, ProfileWarningCode.RepositoryEligibilityUndetermined],
            result.Profile.Warnings
                .Where(warning => warning.AffectedRepositories.Contains(unresolved.Reference))
                .Select(warning => warning.Code)
                .Order());

        // R5 (single-repository dominance): the two resolved technical areas
        // still promote correctly from their own available repositories,
        // undisturbed by the unavailable one.
        RepeatedProfilePattern csharp = Assert.Single(
            result.Profile.RepeatedProfilePatterns,
            p => p.PatternKey == "C#");
        RepeatedProfilePattern typescript = Assert.Single(
            result.Profile.RepeatedProfilePatterns,
            p => p.PatternKey == "TypeScript");
        Assert.Equal(2, csharp.IndependentSupportingRepositoryCount);
        Assert.Equal(2, typescript.IndependentSupportingRepositoryCount);
        Assert.DoesNotContain(unresolved.Reference, csharp.IndependentSupportingRepositories);
        Assert.DoesNotContain(unresolved.Reference, typescript.IndependentSupportingRepositories);
    }

    // ------------------------------------------------------------- R2/R9: MostlyForks

    [Fact]
    public async Task MostlyForks_ForksDoNotInflateIndependentRepetition_NoFalsePatternPromoted()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.MostlyForks);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // R2 (fork inflation): 2 of the 3 repositories are forks, so only 1
        // repository counts as independent support — below the 2-repository
        // minimum — so no TechnicalFocus pattern is promoted at all. Forks
        // did not inflate repetition to reach the threshold.
        Assert.Empty(result.Profile!.RepeatedProfilePatterns);
        Assert.Equal(
            ProfileReportSectionAvailability.Supported,
            result.Report!.TechnicalFocusAreas.Availability);
        Assert.Empty(result.Report.TechnicalFocusAreas.Items);

        Assert.Contains(
            ProfileLimitationCode.LimitedIndependentEvidenceSurface,
            result.Profile.Limitations.Select(limitation => limitation.Code));

        // Session 098 §4.5 / eligibility §6.3, §22.1.1: under the production
        // policy a known fork is skipped at admission, so it contributes no
        // evidence-bearing purpose at all and its overall status is
        // NonContributing. It still appears in the repository universe, so its
        // withheld contribution is never narrated as an absence of work.
        Assert.All(
            result.Profile.RepositoryEligibilityResults.Where(
                r => r.Reference.Name.Contains("-fork-")),
            r =>
            {
                Assert.False(r.IndependentRepetition.IsAllowed);
                Assert.Contains(EligibilityReason.ForkDetected, r.IndependentRepetition.Reasons);
                Assert.False(r.ProfileObservation.IsAllowed);
                Assert.False(r.CurrentFocus.IsAllowed);
                Assert.False(r.HistoricalEvidence.IsAllowed);
                Assert.Equal(RepositoryEligibilityStatus.NonContributing, r.Status);
                Assert.Null(r.AnalysisReference);
            });

        Assert.Equal(3, result.Profile.RepositoryEligibilityResults.Count);
        Assert.Equal(
            2,
            result.Report.RepositoryDrillDown.Items.Count(
                item => item.Repository.Name.Contains("-fork-")));

        // R9: with no meaningful claims produced, the trace check holds
        // vacuously true — no orphan claim leaked around the fork exclusion.
        Assert.True(
            ProfileValidationTraceability.AllMeaningfulClaimsAreTraceable(
                result.Profile,
                result.Report));
    }

    // -------------------------------------------------------- R6/R8: MostlyArchived

    [Fact]
    public async Task MostlyArchived_ArchivedRepositoriesStillContributeEvidence_OnlyCurrentFocusIsRestricted()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.MostlyArchived);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // R6 (archived-repository distortion): archived repositories are not
        // silently excluded from independent evidence — all 3 (1 active + 2
        // archived) support the promoted pattern.
        RepeatedProfilePattern pattern = Assert.Single(result.Profile!.RepeatedProfilePatterns);
        Assert.Equal("C#", pattern.PatternKey);
        Assert.Equal(3, pattern.IndependentSupportingRepositoryCount);

        IEnumerable<RepositoryEligibilityResult> archivedRepositories =
            result.Profile.RepositoryEligibilityResults.Where(
                r => r.Archived == RepositoryConditionState.Present);
        Assert.Equal(2, archivedRepositories.Count());
        Assert.All(
            archivedRepositories,
            r =>
            {
                // Historical evidence and independent repetition remain
                // valid — archived is never "failure" or "low quality".
                Assert.True(r.HistoricalEvidence.IsAllowed);
                Assert.True(r.IndependentRepetition.IsAllowed);
                // Current-focus contribution alone is restricted, and only
                // for the documented Archived reason.
                Assert.False(r.CurrentFocus.IsAllowed);
                Assert.Contains(EligibilityReason.Archived, r.CurrentFocus.Reasons);
            });

        // R8: archived repositories were fully collected and analyzed —
        // "archived" is never confused with "unavailable"; the profile
        // remains Complete, with no rate-limit/unavailable/failed warning
        // for any of them.
        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.Empty(result.Profile.Warnings);
    }

    // ------------------------------------------------ R1/R3/R4: StudentTutorialHeavy

    [Fact]
    public async Task StudentTutorialHeavy_TemplateDerivedRepositoriesDoNotInflateIndependentRepetition()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.StudentTutorialHeavy);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // R3 (template inflation) / R1 (false technical focus): 2 of the 3
        // repositories are template-derived, so only 1 counts as
        // independent — below threshold — so no TechnicalFocus or
        // EngineeringPractice pattern is promoted from mere structural
        // repetition.
        Assert.Empty(result.Profile!.RepeatedProfilePatterns);
        Assert.Equal(
            ProfileReportSectionAvailability.Supported,
            result.Report!.TechnicalFocusAreas.Availability);
        Assert.Equal(
            ProfileReportSectionAvailability.Supported,
            result.Report.EngineeringPracticePatterns.Availability);
        Assert.Empty(result.Report.TechnicalFocusAreas.Items);
        Assert.Empty(result.Report.EngineeringPracticePatterns.Items);

        Assert.Contains(
            ProfileLimitationCode.LimitedIndependentEvidenceSurface,
            result.Profile.Limitations.Select(limitation => limitation.Code));

        // R4: no README-derived keyword evidence was substituted for
        // structured repetition — confirmed by the absence of any promoted
        // pattern above rather than a weaker text-based one.
        Assert.All(
            result.Profile.RepositoryEligibilityResults.Where(
                r => r.Reference.Name.Contains("-template-")),
            r =>
            {
                Assert.False(r.IndependentRepetition.IsAllowed);
                Assert.Contains(EligibilityReason.TemplateDerived, r.IndependentRepetition.Reasons);
            });
    }

    // ------------------------------------------------------------- R1/R5: Polyglot

    [Fact]
    public async Task Polyglot_PromotesEachAreaIndependently_WithNoAreaDominatingTheOthers()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.Polyglot);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Dictionary<string, RepeatedProfilePattern> byLanguage = result.Profile!.RepeatedProfilePatterns
            .Where(p => p.Family == ProfilePatternFamily.TechnicalFocus)
            .ToDictionary(p => p.PatternKey);

        Assert.Equal(["C#", "Python", "TypeScript"], byLanguage.Keys.Order());

        // R1/R5: three independent technical areas, each backed by exactly
        // two repositories — none dominant over the others, and none
        // sharing a repository with another area.
        Assert.All(byLanguage.Values, p => Assert.Equal(2, p.IndependentSupportingRepositoryCount));

        HashSet<ProfileRepositoryReference> seen = [];
        foreach (RepeatedProfilePattern pattern in byLanguage.Values)
        {
            Assert.All(
                pattern.IndependentSupportingRepositories,
                repository => Assert.True(seen.Add(repository)));
        }
    }

    // ------------------------------------------------------ R7/R8/R9: SparsePublicProfile

    [Fact]
    public async Task SparsePublicProfile_RemainsCompleteWithALimitation_NeverConfusedWithUnavailableOrNegativeJudgment()
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(ProfileValidationScenario.SparsePublicProfile);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // R7 (sparse-profile negative judgment): the one available
        // repository is fully resolved, so the profile stays Complete — a
        // sparse-but-resolved surface is never demoted to Partial or given
        // any negative-judgment marker (none exists on the type).
        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.Equal(
            [ProfileLimitationCode.LimitedIndependentEvidenceSurface],
            result.Profile!.Limitations.Select(limitation => limitation.Code));

        // R8: "sparse" (resolved, just small) is structurally distinct from
        // "unavailable" (unresolved) — no warning of any kind is produced
        // here, unlike the ManyRepositories/rate-limited case.
        Assert.Empty(result.Profile.Warnings);

        // No pattern is falsely promoted from a single repository.
        Assert.Empty(result.Profile.RepeatedProfilePatterns);
        Assert.Equal(
            ProfileReportSectionAvailability.Supported,
            result.Report!.TechnicalFocusAreas.Availability);
        Assert.Empty(result.Report.TechnicalFocusAreas.Items);

        // R9.
        Assert.True(
            ProfileValidationTraceability.AllMeaningfulClaimsAreTraceable(
                result.Profile,
                result.Report));
    }

    // --------------------------------------------------------- cross-scenario

    [Theory]
    [MemberData(
        nameof(ProfileValidationScenarioFixturesTests.AllScenarios),
        MemberType = typeof(ProfileValidationScenarioFixturesTests))]
    public async Task EveryScenario_AllMeaningfulClaimsAreTraceable(ProfileValidationScenario scenario)
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(scenario);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Assert.True(result.HasProfile);
        Assert.True(
            ProfileValidationTraceability.AllMeaningfulClaimsAreTraceable(
                result.Profile!,
                result.Report!));
    }

    [Theory]
    [MemberData(
        nameof(ProfileValidationScenarioFixturesTests.AllScenarios),
        MemberType = typeof(ProfileValidationScenarioFixturesTests))]
    public async Task EveryScenario_ProblemAreaStaysUnsupportedAndRoleEvidenceStaysDeferred(
        ProfileValidationScenario scenario)
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(scenario);

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Assert.True(result.HasProfile);
        Assert.True(ProfileValidationCapabilityBoundary.HoldsFor(result.Report!));
    }

    // ---------------------------------------------------- prohibited inference

    // Contract §5 prohibited profile claims / report contract §16 prohibited
    // semantics. The product surface carries no free-form inference text to
    // scan (report contract §2: "It defines presentation projection only. It
    // defines no evidence rule."), so the bounded, correct check is
    // structural: none of the canonical profile/report types may expose a
    // property whose name denotes a prohibited claim. This reuses the exact
    // reflection technique already established by
    // ProfilePipelineTests.PipelineResult_PublicApi_...; it is not an
    // NLP/string-heuristic scanner over generated text, because no such text
    // exists in this product surface.
    [Fact]
    public void ProfileAndReportTypes_ExposeNoProhibitedInferenceProperty()
    {
        string[] prohibitedNameFragments =
        [
            "Score", "Rank", "Junior", "Senior", "Employab", "Hire", "Capability",
            "Confidence", "Salary", "YearsOfExperience", "Authorship", "Original",
            "AiWritten", "Security", "ProductionReady", "ArchitectureQuality",
            "Talent", "Intelligence", "Quality", "Strength"
        ];

        Type[] canonicalTypes =
        [
            typeof(DeveloperPublicEvidenceProfile),
            typeof(RepositoryEligibilityResult),
            typeof(ProfileLimitation),
            typeof(ProfileWarning),
            typeof(RepeatedProfilePattern),
            typeof(ObservedProfileEvidence),
            typeof(RepresentativeRepository),
            typeof(RepresentativePatternSupport),
            typeof(RoleEvidenceAlignment),
            typeof(ProfileReport),
            typeof(PatternReportItem),
            typeof(ConclusionEvidenceReportItem),
            typeof(RepositoryDrillDownReportItem),
            typeof(ProfileSummaryFact)
        ];

        foreach (Type type in canonicalTypes)
        {
            foreach (PropertyInfo property in type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.False(
                    prohibitedNameFragments.Any(fragment => property.Name.Contains(
                        fragment,
                        StringComparison.OrdinalIgnoreCase)),
                    $"'{type.Name}.{property.Name}' resembles a prohibited profile-level claim.");
            }
        }
    }
}

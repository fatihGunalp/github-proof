using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile.Validation;

// P9-I2 minimum slice proof. This file does not validate any of the 11
// canonical scenarios (docs/product/p9-profile-validation-contract.md §2) —
// that is P9-I3's responsibility. It proves the harness itself works: a
// deterministic fixed composition runs through the real ProfilePipeline, the
// result exposes the full semantic inspection surface, and the traceability /
// capability-boundary / determinism helpers hold (and actually discriminate)
// on that real output.
public sealed class ProfileValidationHarnessTests
{
    [Fact]
    public void ScenarioCatalog_ContainsAllElevenCanonicalIdentities()
    {
        ProfileValidationScenario[] allIdentities = Enum.GetValues<ProfileValidationScenario>();

        Assert.Equal(11, allIdentities.Length);
        Assert.Equal(
            allIdentities.OrderBy(identity => identity),
            ProfileValidationScenarioCatalog.CompositionSpecifications.Keys.OrderBy(identity => identity));
        Assert.All(
            ProfileValidationScenarioCatalog.CompositionSpecifications.Values,
            specification => Assert.False(string.IsNullOrWhiteSpace(specification)));
    }

    [Fact]
    public async Task RunAsync_TwoIndependentLanguageHeavyRepositories_ExecutesThroughRealPipelineAndExposesFullInspectionSurface()
    {
        (GitHubProfileSnapshot snapshot, FakeSnapshotSource source) = TwoIndependentCSharpRepositories();

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // Proof items 1/2: a deterministic fixed scenario ran through the
        // real ProfilePipeline and the result exposes every semantic
        // inspection surface locked by contract §8.1 (status, eligibility,
        // limitations, warnings, promoted patterns, representative
        // repositories, report section availability, WhyThisConclusion,
        // repository drill-down).
        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.NotNull(result.Profile);
        Assert.NotNull(result.Report);
        Assert.NotEmpty(result.Profile!.RepositoryEligibilityResults);
        Assert.NotNull(result.Profile.Limitations);
        Assert.NotNull(result.Profile.Warnings);
        Assert.NotEmpty(result.Profile.RepeatedProfilePatterns);
        Assert.NotEmpty(result.Profile.RepresentativeRepositories);
        Assert.Equal(9, result.Report!.Sections.Count);
        Assert.NotEmpty(result.Report.TechnicalFocusAreas.Items);
        Assert.NotEmpty(result.Report.WhyThisConclusion.Items);
        Assert.NotEmpty(result.Report.RepositoryDrillDown.Items);
    }

    [Fact]
    public async Task Traceability_OnARealPipelineResult_HoldsForEveryMeaningfulClaim()
    {
        (GitHubProfileSnapshot snapshot, FakeSnapshotSource source) = TwoIndependentCSharpRepositories();

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Assert.True(
            ProfileValidationTraceability.AllMeaningfulClaimsAreTraceable(
                result.Profile!,
                result.Report!));
    }

    [Fact]
    public async Task Traceability_RejectsAClaimNotBelongingToTheProfile()
    {
        // The helper must actually discriminate, not be vacuously true: a
        // claim produced by an independent pipeline run (same shape,
        // different pattern/observation instances) fails §6.1's
        // reference-identity requirement against a different profile.
        (GitHubProfileSnapshot firstSnapshot, FakeSnapshotSource firstSource) =
            TwoIndependentCSharpRepositories();
        (GitHubProfileSnapshot secondSnapshot, FakeSnapshotSource secondSource) =
            TwoIndependentCSharpRepositories("gamma", "delta");

        ProfilePipelineResult first = await ProfileValidationHarness.RunAsync(firstSource, firstSnapshot);
        ProfilePipelineResult second = await ProfileValidationHarness.RunAsync(secondSource, secondSnapshot);

        PatternReportItem foreignItem = Assert.Single(second.Report!.TechnicalFocusAreas.Items);

        Assert.False(ProfileValidationTraceability.IsTraceable(first.Profile!, foreignItem));
    }

    [Fact]
    public async Task CapabilityBoundary_OnARealPipelineResult_ProblemAreaUnsupportedAndRoleEvidenceDeferred()
    {
        (GitHubProfileSnapshot snapshot, FakeSnapshotSource source) = TwoIndependentCSharpRepositories();

        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        Assert.True(ProfileValidationCapabilityBoundary.HoldsFor(result.Report!));
    }

    [Fact]
    public async Task Determinism_TwoIndependentRunsOverTheSameFixedInput_AreSemanticallyEquivalent()
    {
        (GitHubProfileSnapshot firstSnapshot, FakeSnapshotSource firstSource) =
            TwoIndependentCSharpRepositories();
        (GitHubProfileSnapshot secondSnapshot, FakeSnapshotSource secondSource) =
            TwoIndependentCSharpRepositories();

        ProfilePipelineResult first = await ProfileValidationHarness.RunAsync(firstSource, firstSnapshot);
        ProfilePipelineResult second = await ProfileValidationHarness.RunAsync(secondSource, secondSnapshot);

        AssertSemanticallyEquivalent(first, second);
    }

    [Fact]
    public async Task RepositoryOrder_DoesNotAlterTheSemanticResult()
    {
        (GitHubProfileSnapshot forwardSnapshot, FakeSnapshotSource forwardSource) =
            TwoIndependentCSharpRepositories();
        (GitHubProfileSnapshot reversedSnapshot, FakeSnapshotSource reversedSource) =
            TwoIndependentCSharpRepositories(reversed: true);

        ProfilePipelineResult forward = await ProfileValidationHarness.RunAsync(forwardSource, forwardSnapshot);
        ProfilePipelineResult reversed = await ProfileValidationHarness.RunAsync(reversedSource, reversedSnapshot);

        AssertSemanticallyEquivalent(forward, reversed);
    }

    // A minimal, deterministic, network-free two-repository composition
    // (Fork/TemplateDerived/Archived all Absent, real language evidence),
    // sufficient to promote one TechnicalFocus pattern and one representative
    // repository through the real pipeline. Reused by every proof test above
    // so this file defines exactly one fixture shape rather than one per
    // test.
    private static (GitHubProfileSnapshot Snapshot, FakeSnapshotSource Source) TwoIndependentCSharpRepositories(
        string first = "alpha",
        string second = "beta",
        bool reversed = false)
    {
        ProfileRepository firstRepository = CreateRepository(first);
        ProfileRepository secondRepository = CreateRepository(second);

        IReadOnlyList<ProfileRepository> repositories = reversed
            ? [secondRepository, firstRepository]
            : [firstRepository, secondRepository];

        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(repositories);

        FakeSnapshotSource source = CreateSource(
            ($"octocat/{first}", () => CreateCollected(
                CreateLanguageEvidenceSnapshot(first, new Dictionary<string, long> { ["C#"] = 100 }))),
            ($"octocat/{second}", () => CreateCollected(
                CreateLanguageEvidenceSnapshot(second, new Dictionary<string, long> { ["C#"] = 100 }))));

        return (snapshot, source);
    }
}

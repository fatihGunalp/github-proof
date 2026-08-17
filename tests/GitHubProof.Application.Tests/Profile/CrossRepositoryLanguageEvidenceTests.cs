using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

public sealed class CrossRepositoryLanguageEvidenceTests
{
    private static readonly ObservationRuleVersion TestRuleVersion =
        new("test-observation-rules");

    [Fact]
    public async Task Compose_EmitsOnlyDominantLanguageEvidence()
    {
        CrossRepositoryEvidence evidence = await ComposeAsync(
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["C#"] = 9000, ["Shell"] = 120 })));

        ObservedProfileEvidence language = Assert.Single(LanguageObservations(evidence));

        Assert.Equal("language.C#", language.ObservationKey);
        Assert.Equal(
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence,
            language.Kind);
        Assert.Equal(1, language.SupportingRepositoryCount);
        Assert.Equal(0, language.UndeterminedEvidenceRepositoryCount);

        ProfileEvidenceReference reference = Assert.Single(language.EvidenceReferences);

        Assert.Equal("language.C#", reference.EvidenceKey);
        Assert.Equal(
            new[] { ProfileEvidenceKeys.LanguagesDataKey },
            reference.EvidenceKeys);
        Assert.Equal("octocat/alpha", reference.Repository.FullName);
    }

    [Fact]
    public async Task Compose_WhenByteCountsTieExactly_TreatsEveryTiedLanguageAsDominant()
    {
        CrossRepositoryEvidence evidence = await ComposeAsync(
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["C#"] = 4000, ["F#"] = 4000 })));

        Assert.Equal(
            new[] { "language.C#", "language.F#" },
            LanguageObservations(evidence)
                .Select(observation => observation.ObservationKey));
    }

    [Fact]
    public async Task Compose_WhenLanguagesAreEmptyOrZero_ProducesNoLanguageEvidence()
    {
        CrossRepositoryEvidence evidence = await ComposeAsync(
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long>())),
            ("beta", CreateLanguageEvidenceSnapshot(
                "beta",
                new Dictionary<string, long> { ["Text"] = 0 })));

        Assert.Empty(LanguageObservations(evidence));
    }

    [Fact]
    public async Task Compose_WhenLanguageDataIsUnavailable_IsUndeterminedRatherThanAbsent()
    {
        CrossRepositoryEvidence evidence = await ComposeAsync(
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["C#"] = 500 })),
            ("beta", CreateReadmeEvidenceSnapshot("beta")));

        ObservedProfileEvidence language = Assert.Single(LanguageObservations(evidence));

        Assert.Equal(2, evidence.EligibleRepositoryCount);
        Assert.Equal(1, language.SupportingRepositoryCount);
        Assert.Equal(1, language.UndeterminedEvidenceRepositoryCount);
        Assert.Equal(2, language.EligibleRepositoryCount);
    }

    [Fact]
    public async Task Compose_ForkedRepository_DoesNotInflateIndependentLanguageEvidence()
    {
        CrossRepositoryEvidence evidence = await ComposeAsync(
            [
                CreateProfileRepository("alpha"),
                CreateProfileRepository(
                    "forked",
                    fork: RepositoryConditionState.Present)
            ],
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["C#"] = 500 })),
            ("forked", CreateLanguageEvidenceSnapshot(
                "forked",
                new Dictionary<string, long> { ["C#"] = 700 })));

        ObservedProfileEvidence language = Assert.Single(LanguageObservations(evidence));

        Assert.Equal(2, language.SupportingRepositoryCount);
        Assert.Equal(1, language.IndependentSupportingRepositoryCount);
    }

    [Fact]
    public async Task Compose_LanguageEvidence_IsIndependentOfInputOrder()
    {
        CrossRepositoryEvidence first = await ComposeAsync(
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["Python"] = 10, ["C#"] = 900 })),
            ("beta", CreateLanguageEvidenceSnapshot(
                "beta",
                new Dictionary<string, long> { ["C#"] = 900, ["Python"] = 10 })));

        CrossRepositoryEvidence second = await ComposeAsync(
            ("beta", CreateLanguageEvidenceSnapshot(
                "beta",
                new Dictionary<string, long> { ["Python"] = 10, ["C#"] = 900 })),
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["C#"] = 900, ["Python"] = 10 })));

        Assert.Equal(Describe(first), Describe(second));
    }

    [Fact]
    public async Task ComposedLanguageEvidence_PromotesIntoTechnicalFocus()
    {
        CrossRepositoryEvidence evidence = await ComposeAsync(
            ("alpha", CreateLanguageEvidenceSnapshot(
                "alpha",
                new Dictionary<string, long> { ["C#"] = 900, ["Shell"] = 10 })),
            ("beta", CreateLanguageEvidenceSnapshot(
                "beta",
                new Dictionary<string, long> { ["C#"] = 400 })));

        ProfilePatternSet patterns =
            new ProfilePatternPromotionPolicy(new PatternRuleVersion("test-pattern-rules"))
                .Promote(evidence);

        RepeatedProfilePattern pattern = Assert.Single(patterns.Patterns);

        Assert.Equal(ProfilePatternFamily.TechnicalFocus, pattern.Family);
        Assert.Equal("C#", pattern.PatternKey);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            pattern.SupportingRepositories.Select(repository => repository.FullName));
        Assert.Equal(
            "language.C#",
            Assert.Single(pattern.SupportingObservations).ObservationKey);
        Assert.Equal(evidence.RuleVersion, patterns.ObservationRuleVersion);
        Assert.Equal(evidence.PolicyVersion, patterns.PolicyVersion);
    }

    private static IEnumerable<ObservedProfileEvidence> LanguageObservations(
        CrossRepositoryEvidence evidence) =>
        evidence.Observations.Where(observation =>
            observation.Kind
                == ObservedProfileEvidenceKind.RepositoryLanguageEvidence);

    private static IEnumerable<string> Describe(CrossRepositoryEvidence evidence) =>
        LanguageObservations(evidence).Select(observation =>
            $"{observation.ObservationKey}:{observation.SupportingRepositoryCount}"
            + $":{observation.IndependentSupportingRepositoryCount}"
            + $":{string.Join(",", observation.ContributingRepositories.Select(repository => repository.FullName))}");

    private static Task<CrossRepositoryEvidence> ComposeAsync(
        params (string Name, Domain.Analysis.Models.RepositorySnapshot Snapshot)[] repositories) =>
        ComposeAsync(
            repositories.Select(repository =>
                CreateProfileRepository(repository.Name)).ToArray(),
            repositories);

    private static async Task<CrossRepositoryEvidence> ComposeAsync(
        IReadOnlyList<ProfileRepository> profileRepositories,
        params (string Name, Domain.Analysis.Models.RepositorySnapshot Snapshot)[] repositories)
    {
        ProfileAnalysisOrchestrationResult orchestration =
            await new ProfileRepositoryAnalysisOrchestrator(
                    CreateSource(repositories
                        .Select(repository => (
                            $"octocat/{repository.Name}",
                            (Func<ProfileRepositorySnapshotResult>)(() =>
                                CreateCollected(repository.Snapshot))))
                        .ToArray()),
                    CreateAnalysisRunner(),
                    new AnalysisAdmissionPolicy(TestPolicyVersion),
                    new SequentialAnalysisIdFactory().Next)
                .AnalyzeAsync(
                    CreateProfileSnapshot(profileRepositories),
                    TestAnalysisVersion);

        return new CrossRepositoryEvidenceComposer(
                new RepositoryContributionPolicy(TestPolicyVersion))
            .Compose(orchestration, TestRuleVersion);
    }
}

using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ProfilePatternPromotionPolicyTests
{
    [Fact]
    public void Promote_RepeatedPracticeEvidence_ProducesOneCanonicalPattern()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [
                CreateObservation(
                    TestStructureSignalEvaluator.SignalKey,
                    [alpha, beta],
                    eligibleRepositoryCount: 2)
            ]));

        RepeatedProfilePattern pattern = Assert.Single(patterns.Patterns);

        Assert.Equal(ProfilePatternFamily.EngineeringPractice, pattern.Family);
        Assert.Equal(
            ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
            pattern.PatternKey);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            pattern.SupportingRepositories.Select(repository => repository.FullName));
        Assert.Equal(TestPatternRuleVersion, pattern.RuleVersion);
        Assert.Equal(RuleVersion, pattern.ObservationRuleVersion);
    }

    [Fact]
    public void Promote_SingleRepositoryEvidence_DoesNotEstablishAPattern()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha],
            [
                CreateObservation(
                    TestStructureSignalEvaluator.SignalKey,
                    [alpha],
                    eligibleRepositoryCount: 1)
            ]));

        Assert.Empty(patterns.Patterns);
    }

    [Fact]
    public void Promote_ForkAndTemplateLineage_CannotInflateRepetition()
    {
        RepositoryContributionAssessment original = CreateContribution("alpha");
        RepositoryContributionAssessment forked = CreateContribution(
            "forked",
            fork: RepositoryConditionState.Present);
        RepositoryContributionAssessment templated = CreateContribution(
            "templated",
            templateDerived: RepositoryConditionState.Present);

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [original, forked, templated],
            [
                CreateObservation(
                    TestStructureSignalEvaluator.SignalKey,
                    [original, forked, templated],
                    eligibleRepositoryCount: 3)
            ]));

        Assert.Empty(patterns.Patterns);
    }

    [Fact]
    public void Promote_UndeterminedIndependence_DoesNotBlockACertainPattern()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment unknown = CreateContribution(
            "unknown-lineage",
            fork: RepositoryConditionState.Undetermined);

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta, unknown],
            [
                CreateObservation(
                    TestStructureSignalEvaluator.SignalKey,
                    [alpha, beta, unknown],
                    eligibleRepositoryCount: 3)
            ]));

        RepeatedProfilePattern pattern = Assert.Single(patterns.Patterns);

        Assert.Equal(3, pattern.SupportingRepositoryCount);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
        Assert.DoesNotContain(
            unknown.Reference,
            pattern.IndependentSupportingRepositories);
    }

    [Fact]
    public void Promote_DocumentationPractice_UnionsBothMappedSignals()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [
                CreateObservation(
                    InstallationGuideSignalEvaluator.SignalKey,
                    [alpha],
                    eligibleRepositoryCount: 2),
                CreateObservation(
                    UsageDemoSignalEvaluator.SignalKey,
                    [beta],
                    eligibleRepositoryCount: 2)
            ]));

        RepeatedProfilePattern pattern = Assert.Single(patterns.Patterns);

        Assert.Equal(
            ProfilePatternPromotionPolicy.DocumentationPatternKey,
            pattern.PatternKey);
        Assert.Equal(2, pattern.SupportingObservations.Count);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
    }

    [Fact]
    public void Promote_OneRepositorySupportingBothDocumentationSignals_IsCountedOnce()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha],
            [
                CreateObservation(
                    InstallationGuideSignalEvaluator.SignalKey,
                    [alpha],
                    eligibleRepositoryCount: 1),
                CreateObservation(
                    UsageDemoSignalEvaluator.SignalKey,
                    [alpha],
                    eligibleRepositoryCount: 1)
            ]));

        Assert.Empty(patterns.Patterns);
    }

    [Theory]
    [InlineData("process.commit-history")]
    [InlineData("process.commit-distribution")]
    [InlineData("repository.readme.exists")]
    [InlineData("repository.description.present")]
    [InlineData("repository.license.present")]
    [InlineData("portfolio.topics")]
    [InlineData("portfolio.homepage-or-live-demo")]
    [InlineData("portfolio.presentation-completeness")]
    public void Promote_ExcludedSignals_ProduceNoPractice(string signalKey)
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [CreateObservation(signalKey, [alpha, beta], eligibleRepositoryCount: 2)]));

        Assert.Empty(patterns.Patterns);
    }

    [Fact]
    public void Promote_RepeatedDominantLanguage_ProducesTechnicalFocus()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [CreateLanguageObservation("C#", [alpha, beta], eligibleRepositoryCount: 2)]));

        RepeatedProfilePattern pattern = Assert.Single(patterns.Patterns);

        Assert.Equal(ProfilePatternFamily.TechnicalFocus, pattern.Family);
        Assert.Equal("C#", pattern.PatternKey);
        Assert.Equal(2, pattern.IndependentSupportingRepositoryCount);
    }

    [Fact]
    public void Promote_DifferentLanguages_AreNeverGroupedTogether()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [
                CreateLanguageObservation("C#", [alpha, beta], eligibleRepositoryCount: 2),
                CreateLanguageObservation("F#", [alpha, beta], eligibleRepositoryCount: 2)
            ]));

        Assert.Equal(
            new[] { "C#", "F#" },
            patterns.TechnicalFocusPatterns.Select(pattern => pattern.PatternKey));
    }

    [Fact]
    public void Promote_NeverProducesProblemAreaPatterns()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [
                CreateObservation(
                    TestStructureSignalEvaluator.SignalKey,
                    [alpha, beta],
                    eligibleRepositoryCount: 2),
                CreateLanguageObservation("C#", [alpha, beta], eligibleRepositoryCount: 2),
                CreateObservation(
                    "portfolio.topics",
                    [alpha, beta],
                    eligibleRepositoryCount: 2)
            ]));

        Assert.Empty(patterns.ProblemAreaPatterns);
        Assert.NotEmpty(patterns.Patterns);
    }

    [Fact]
    public void Promote_OrdersPatternsByFamilyThenKey()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [
                CreateObservation(
                    ReleaseUsageSignalEvaluator.SignalKey,
                    [alpha, beta],
                    eligibleRepositoryCount: 2),
                CreateObservation(
                    ContinuousIntegrationSignalEvaluator.SignalKey,
                    [alpha, beta],
                    eligibleRepositoryCount: 2),
                CreateLanguageObservation("C#", [alpha, beta], eligibleRepositoryCount: 2)
            ]));

        Assert.Equal(
            new[]
            {
                "C#",
                ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey,
                ProfilePatternPromotionPolicy.ReleaseUsagePatternKey
            },
            patterns.Patterns.Select(pattern => pattern.PatternKey));
    }

    [Fact]
    public void Promote_CarriesVersionAndEvidenceSurfaceCoherence()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment restricted =
            CreateRestrictedContribution("empty");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta, restricted],
            [
                CreateObservation(
                    TestStructureSignalEvaluator.SignalKey,
                    [alpha, beta],
                    eligibleRepositoryCount: 2)
            ],
            isEvidenceSurfaceComplete: false));

        Assert.Equal(TestPatternRuleVersion, patterns.RuleVersion);
        Assert.Equal(RuleVersion, patterns.ObservationRuleVersion);
        Assert.Equal(PolicyVersion, patterns.PolicyVersion);
        Assert.False(patterns.IsEvidenceSurfaceComplete);
    }

    [Fact]
    public void Promote_TechnicalFocusAndPracticeEvidence_RemainTraceable()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        ProfilePatternSet patterns = Promote(CreateEvidence(
            [alpha, beta],
            [CreateLanguageObservation("C#", [alpha, beta], eligibleRepositoryCount: 2)]));

        RepeatedProfilePattern pattern = Assert.Single(patterns.Patterns);
        ObservedProfileEvidence observation = Assert.Single(
            pattern.SupportingObservations);

        Assert.All(
            observation.EvidenceReferences,
            reference => Assert.Equal(
                observation.ObservationKey,
                reference.EvidenceKey));
        Assert.Equal(
            alpha.AnalysisReference,
            observation.EvidenceReferences
                .Single(reference => reference.Repository == alpha.Reference)
                .Analysis);
    }

    [Fact]
    public void Promote_WhenEvidenceIsMissing_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(() => Promote(null!));

        Assert.Throws<ArgumentNullException>(
            () => new ProfilePatternPromotionPolicy(null!));
    }

    private static ProfilePatternSet Promote(CrossRepositoryEvidence evidence) =>
        new ProfilePatternPromotionPolicy(TestPatternRuleVersion).Promote(evidence);
}

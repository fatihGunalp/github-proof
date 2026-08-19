using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepresentativeRepositoryTests
{
    [Fact]
    public void Constructor_GroupsSupportsOrderedByFamilyThenPatternKey()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        RepresentativePatternSupport focus = CreateSupport(
            CreatePattern(
                ProfilePatternFamily.TechnicalFocus,
                "C#",
                [alpha, beta]),
            alpha);
        RepresentativePatternSupport practiceB = CreateSupport(
            CreatePattern(
                ProfilePatternFamily.EngineeringPractice,
                "issue-usage",
                [alpha, beta]),
            alpha);
        RepresentativePatternSupport practiceA = CreateSupport(
            CreatePattern(
                ProfilePatternFamily.EngineeringPractice,
                "automated-testing",
                [alpha, beta]),
            alpha);

        RepresentativeRepository representative = new(
            alpha.Reference,
            [focus, practiceB, practiceA]);

        Assert.Equal(
            new[] { "C#", "automated-testing", "issue-usage" },
            representative.PatternSupports.Select(support => support.PatternKey));
    }

    [Fact]
    public void Constructor_WhenNoPatternSupportIsGiven_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");

        Assert.Throws<ArgumentException>(
            () => new RepresentativeRepository(alpha.Reference, []));
    }

    [Fact]
    public void Constructor_WhenTheSamePatternIsRepeated_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [alpha, beta]);

        Assert.Throws<ArgumentException>(() => new RepresentativeRepository(
            alpha.Reference,
            [
                CreateSupport(pattern, alpha),
                CreateSupport(pattern, alpha)
            ]));
    }

    [Fact]
    public void Constructor_WhenASupportBelongsToADifferentRepository_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [alpha, beta]);

        RepresentativePatternSupport betaSupport = CreateSupport(pattern, beta);

        Assert.Throws<ArgumentException>(
            () => new RepresentativeRepository(alpha.Reference, [betaSupport]));
    }

    [Fact]
    public void Constructor_WhenRepositoryOrSupportsAreMissing_FailsFast()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepresentativePatternSupport support = CreateSupport(
            CreatePattern(
                ProfilePatternFamily.EngineeringPractice,
                "automated-testing",
                [alpha, beta]),
            alpha);

        Assert.Throws<ArgumentNullException>(
            () => new RepresentativeRepository(null!, [support]));
        Assert.Throws<ArgumentNullException>(
            () => new RepresentativeRepository(alpha.Reference, null!));
    }

    private static RepresentativePatternSupport CreateSupport(
        RepeatedProfilePattern pattern,
        RepositoryContributionAssessment winner) =>
        new(
            pattern,
            winner.Reference,
            ScopedEvidenceFor(pattern, winner.Reference));

    private static RepeatedProfilePattern CreatePattern(
        ProfilePatternFamily family,
        string patternKey,
        IReadOnlyList<RepositoryContributionAssessment> supporters) =>
        new(
            family,
            patternKey,
            [CreateObservation("validation.test-structure", supporters, supporters.Count)],
            supporters.Select(contribution => contribution.Reference),
            TestPatternRuleVersion);
}

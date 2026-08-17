using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ProfilePatternSetTests
{
    private const string SignalKey = "validation.test-structure";

    [Fact]
    public void Constructor_OrdersPatternsAndExposesFamilyProjections()
    {
        RepeatedProfilePattern practice = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing");
        RepeatedProfilePattern focus = CreatePattern(
            ProfilePatternFamily.TechnicalFocus,
            "C#");

        ProfilePatternSet set = CreateSet([practice, focus]);

        Assert.Equal(
            new[] { "C#", "automated-testing" },
            set.Patterns.Select(pattern => pattern.PatternKey));
        Assert.Equal(
            new[] { "C#" },
            set.TechnicalFocusPatterns.Select(pattern => pattern.PatternKey));
        Assert.Equal(
            new[] { "automated-testing" },
            set.EngineeringPracticePatterns.Select(pattern => pattern.PatternKey));
        Assert.Empty(set.ProblemAreaPatterns);
    }

    [Fact]
    public void Constructor_WhenOnePatternIdentityRepeats_Rejects()
    {
        Assert.Throws<ArgumentException>(() => CreateSet(
        [
            CreatePattern(ProfilePatternFamily.EngineeringPractice, "automated-testing"),
            CreatePattern(ProfilePatternFamily.EngineeringPractice, "automated-testing")
        ]));
    }

    [Fact]
    public void Constructor_WhenPatternRuleVersionsDiffer_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern foreign = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha.Reference, beta.Reference],
            new PatternRuleVersion("other-pattern-rules"));

        Assert.Throws<ArgumentException>(() => CreateSet([foreign]));
    }

    [Fact]
    public void Constructor_WhenObservationRuleVersionsDiffer_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        Assert.Throws<ArgumentException>(() => new ProfilePatternSet(
            [CreatePattern(ProfilePatternFamily.EngineeringPractice, "automated-testing")],
            TestPatternRuleVersion,
            new ObservationRuleVersion("other-observation-rules"),
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_WhenVersionsAreMissing_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(() => new ProfilePatternSet(
            [],
            null!,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));

        Assert.Throws<ArgumentNullException>(() => new ProfilePatternSet(
            [],
            TestPatternRuleVersion,
            RuleVersion,
            null!,
            isEvidenceSurfaceComplete: true));
    }

    private static ProfilePatternSet CreateSet(
        IEnumerable<RepeatedProfilePattern> patterns) =>
        new(
            patterns,
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true);

    private static RepeatedProfilePattern CreatePattern(
        ProfilePatternFamily family,
        string patternKey)
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        return new RepeatedProfilePattern(
            family,
            patternKey,
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion);
    }
}

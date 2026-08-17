using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepeatedProfilePatternTests
{
    private const string SignalKey = "validation.test-structure";

    [Fact]
    public void Constructor_OrdersObservationsAndRepositoriesDeterministically()
    {
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment alpha = CreateContribution("alpha");

        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "documentation",
            [
                CreateObservation("documentation.usage-or-demo", [beta], 2),
                CreateObservation("documentation.installation-guide", [alpha], 2)
            ],
            [beta.Reference, alpha.Reference],
            TestPatternRuleVersion);

        Assert.Equal(
            new[] { "documentation.installation-guide", "documentation.usage-or-demo" },
            pattern.SupportingObservations.Select(
                observation => observation.ObservationKey));
        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            pattern.SupportingRepositories.Select(repository => repository.FullName));
        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            pattern.IndependentSupportingRepositories.Select(
                repository => repository.FullName));
    }

    [Fact]
    public void Constructor_WhenOnlyOneIndependentRepositorySupports_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        Assert.Throws<ArgumentException>(() => new RepeatedProfilePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha.Reference],
            TestPatternRuleVersion));
    }

    [Fact]
    public void Constructor_WhenIndependentRepositoryDoesNotSupport_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment outsider = CreateContribution("outsider");

        Assert.Throws<ArgumentException>(() => new RepeatedProfilePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 3)],
            [alpha.Reference, outsider.Reference],
            TestPatternRuleVersion));
    }

    [Fact]
    public void Constructor_WhenObservationsMixRuleVersions_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        ObservedProfileEvidence foreign = new(
            "documentation.usage-or-demo",
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            [
                CreateEvidenceReference(alpha, "documentation.usage-or-demo"),
                CreateEvidenceReference(beta, "documentation.usage-or-demo")
            ],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            new ObservationRuleVersion("other-observation-rules"));

        Assert.Throws<ArgumentException>(() => new RepeatedProfilePattern(
            ProfilePatternFamily.EngineeringPractice,
            "documentation",
            [
                CreateObservation("documentation.installation-guide", [alpha, beta], 2),
                foreign
            ],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion));
    }

    [Fact]
    public void Constructor_WhenNoObservationSupportsThePattern_Rejects()
    {
        Assert.Throws<ArgumentException>(() => new RepeatedProfilePattern(
            ProfilePatternFamily.TechnicalFocus,
            "C#",
            [],
            [],
            TestPatternRuleVersion));
    }

    [Fact]
    public void Constructor_WhenIndependentRepositoryRepeats_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        Assert.Throws<ArgumentException>(() => new RepeatedProfilePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha.Reference, alpha.Reference],
            TestPatternRuleVersion));
    }
}

using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepresentativeRepositorySelectionTests
{
    [Fact]
    public void Constructor_ZeroRepresentativeRepositoriesIsAValidResult()
    {
        RepresentativeRepositorySelection selection = new(
            [],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: false,
            TestRepresentativeRepositoryRuleVersion);

        Assert.Empty(selection.RepresentativeRepositories);
        Assert.Equal(TestPatternRuleVersion, selection.PatternRuleVersion);
        Assert.Equal(RuleVersion, selection.ObservationRuleVersion);
        Assert.Equal(PolicyVersion, selection.EligibilityPolicyVersion);
        Assert.False(selection.IsEvidenceSurfaceComplete);
        Assert.Equal(TestRepresentativeRepositoryRuleVersion, selection.RuleVersion);
    }

    [Fact]
    public void Constructor_OrdersRepresentativesByProfileRepositoryReference()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        RepresentativeRepository betaRepresentative = CreateRepresentative(
            "pattern-a", beta, alpha, beta);
        RepresentativeRepository alphaRepresentative = CreateRepresentative(
            "pattern-b", alpha, alpha, beta);

        RepresentativeRepositorySelection selection = new(
            [betaRepresentative, alphaRepresentative],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion);

        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            selection.RepresentativeRepositories.Select(
                representative => representative.Repository.FullName));
    }

    [Fact]
    public void Constructor_WhenTheSameRepositoryAppearsTwice_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");

        RepresentativeRepository first = CreateRepresentative(
            "automated-testing", alpha, alpha, beta);
        RepresentativeRepository second = CreateRepresentative(
            "automated-testing", alpha, alpha, beta);

        Assert.Throws<ArgumentException>(() => new RepresentativeRepositorySelection(
            [first, second],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
    }

    [Fact]
    public void Constructor_WhenTheSamePatternIsRepresentedUnderTwoDifferentRepositories_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion);

        RepresentativeRepository alphaRepresentative = new(
            alpha.Reference,
            [
                new RepresentativePatternSupport(
                    pattern,
                    alpha.Reference,
                    ScopedEvidenceFor(pattern, alpha.Reference))
            ]);
        RepresentativeRepository betaRepresentative = new(
            beta.Reference,
            [
                new RepresentativePatternSupport(
                    pattern,
                    beta.Reference,
                    ScopedEvidenceFor(pattern, beta.Reference))
            ]);

        Assert.Throws<ArgumentException>(() => new RepresentativeRepositorySelection(
            [alphaRepresentative, betaRepresentative],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
    }

    [Fact]
    public void Constructor_WhenAPatternWasProducedUnderADifferentPatternRuleVersion_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        PatternRuleVersion foreignPatternRuleVersion = new("other-pattern-rules");
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha.Reference, beta.Reference],
            foreignPatternRuleVersion);

        RepresentativeRepository representative = new(
            alpha.Reference,
            [
                new RepresentativePatternSupport(
                    pattern,
                    alpha.Reference,
                    ScopedEvidenceFor(pattern, alpha.Reference))
            ]);

        Assert.Throws<ArgumentException>(() => new RepresentativeRepositorySelection(
            [representative],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
    }

    [Fact]
    public void Constructor_WhenAPatternConsumedADifferentObservationRuleVersion_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        ObservationRuleVersion foreignObservationRuleVersion =
            new("other-observation-rules");
        ObservedProfileEvidence observation = new(
            "validation.test-structure",
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            [
                CreateEvidenceReference(alpha, "validation.test-structure"),
                CreateEvidenceReference(beta, "validation.test-structure")
            ],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            foreignObservationRuleVersion);
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [observation],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion);

        RepresentativeRepository representative = new(
            alpha.Reference,
            [
                new RepresentativePatternSupport(
                    pattern,
                    alpha.Reference,
                    ScopedEvidenceFor(pattern, alpha.Reference))
            ]);

        Assert.Throws<ArgumentException>(() => new RepresentativeRepositorySelection(
            [representative],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
    }

    [Fact]
    public void Constructor_WhenTwoRepresentativesCollideNonCanonically_Rejects()
    {
        ProfileRepositoryReference canonical = new(
            "octocat", "alpha", new Uri("https://github.com/octocat/alpha"));
        ProfileRepositoryReference mirrored = new(
            "octocat", "alpha", new Uri("https://mirror.example.com/octocat/alpha"));
        RepositoryContributionAssessment beta = CreateContribution("beta");

        RepresentativeRepository canonicalRepresentative = CreateRawRepresentative(
            "pattern-a", canonical, beta);
        RepresentativeRepository mirroredRepresentative = CreateRawRepresentative(
            "pattern-b", mirrored, beta);

        Assert.Throws<ArgumentException>(() => new RepresentativeRepositorySelection(
            [canonicalRepresentative, mirroredRepresentative],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
    }

    [Fact]
    public void Constructor_WhenAVersionOrTheRepositoryListIsMissing_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(() => new RepresentativeRepositorySelection(
            null!,
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
        Assert.Throws<ArgumentNullException>(() => new RepresentativeRepositorySelection(
            [],
            null!,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            TestRepresentativeRepositoryRuleVersion));
        Assert.Throws<ArgumentNullException>(() => new RepresentativeRepositorySelection(
            [],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true,
            null!));
    }

    private static RepresentativeRepository CreateRepresentative(
        string patternKey,
        RepositoryContributionAssessment winner,
        RepositoryContributionAssessment alpha,
        RepositoryContributionAssessment beta)
    {
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            patternKey,
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion);

        return new RepresentativeRepository(
            winner.Reference,
            [
                new RepresentativePatternSupport(
                    pattern,
                    winner.Reference,
                    ScopedEvidenceFor(pattern, winner.Reference))
            ]);
    }

    private static RepresentativeRepository CreateRawRepresentative(
        string patternKey,
        ProfileRepositoryReference primary,
        RepositoryContributionAssessment secondarySupporter)
    {
        const string signalKey = "validation.test-structure";
        ObservedProfileEvidence observation = new(
            signalKey,
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            [
                new ProfileEvidenceReference(
                    primary,
                    CreateAnalysisReference(primary.Name, primary.Owner),
                    signalKey,
                    null),
                CreateEvidenceReference(secondarySupporter, signalKey)
            ],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            RuleVersion);

        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            patternKey,
            [observation],
            [primary, secondarySupporter.Reference],
            TestPatternRuleVersion);

        return new RepresentativeRepository(
            primary,
            [
                new RepresentativePatternSupport(
                    pattern,
                    primary,
                    ScopedEvidenceFor(pattern, primary))
            ]);
    }
}

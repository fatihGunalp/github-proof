using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class CrossRepositoryEvidenceTests
{
    private const string SignalKey = "repository.readme.exists";

    [Fact]
    public void Constructor_OrdersContributionsAndObservationsIndependentlyOfInputOrder()
    {
        RepositoryContributionAssessment beta = Contributing("beta");
        RepositoryContributionAssessment alpha = Contributing("alpha");

        CrossRepositoryEvidence evidence = new(
            [beta, alpha],
            [
                Observation("z.signal", [beta, alpha], eligibleRepositoryCount: 2),
                Observation("a.signal", [alpha], eligibleRepositoryCount: 2)
            ],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true);

        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            evidence.RepositoryContributions.Select(
                contribution => contribution.Reference.FullName));
        Assert.Equal(
            new[] { "a.signal", "z.signal" },
            evidence.Observations.Select(observation => observation.ObservationKey));
        Assert.Equal(2, evidence.EligibleRepositoryCount);
    }

    [Fact]
    public void Constructor_WhenObservationUsesANonContributingRepository_Rejects()
    {
        RepositoryContributionAssessment contributing = Contributing("alpha");
        RepositoryContributionAssessment alsoContributing = Contributing("gamma");
        RepositoryContributionAssessment restricted = Restricted("beta");

        Assert.Throws<ArgumentException>(() => new CrossRepositoryEvidence(
            [contributing, alsoContributing, restricted],
            [Observation(SignalKey, [contributing, restricted], eligibleRepositoryCount: 2)],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_WhenObservationPointsToAnotherAnalysis_Rejects()
    {
        RepositoryContributionAssessment contributing = Contributing("alpha");
        ProfileEvidenceReference foreign = new(
            contributing.Reference,
            new RepositoryAnalysisReference(
                Guid.Parse("04800000-0000-0000-0000-000000000099"),
                CreateAnalysisReference("alpha").Repository,
                TestAnalysisVersion),
            SignalKey,
            ["evidence-key"]);

        Assert.Throws<ArgumentException>(() => new CrossRepositoryEvidence(
            [contributing],
            [
                new ObservedProfileEvidence(
                    SignalKey,
                    ObservedProfileEvidenceKind.RepositorySignalEvidence,
                    [foreign],
                    independentSupportingRepositoryCount: 1,
                    eligibleRepositoryCount: 1,
                    undeterminedEvidenceRepositoryCount: 0,
                    RuleVersion)
            ],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_WhenIndependentCountDisagreesWithContributionDecisions_Rejects()
    {
        RepositoryContributionAssessment forked = Contributing(
            "forked",
            fork: RepositoryConditionState.Present);

        Assert.Throws<ArgumentException>(() => new CrossRepositoryEvidence(
            [forked],
            [
                new ObservedProfileEvidence(
                    SignalKey,
                    ObservedProfileEvidenceKind.RepositorySignalEvidence,
                    [Evidence(forked, SignalKey)],
                    independentSupportingRepositoryCount: 1,
                    eligibleRepositoryCount: 1,
                    undeterminedEvidenceRepositoryCount: 0,
                    RuleVersion)
            ],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_WhenObservationUsesADifferentEligibleDenominator_Rejects()
    {
        RepositoryContributionAssessment contributing = Contributing("alpha");

        Assert.Throws<ArgumentException>(() => new CrossRepositoryEvidence(
            [contributing],
            [Observation(SignalKey, [contributing], eligibleRepositoryCount: 5)],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_WhenAContributionUsesAnotherPolicyVersion_Rejects()
    {
        ProfileRepository repository = CreateProfileRepository("alpha");
        RepositoryContributionAssessment foreign = new RepositoryContributionPolicy(
                new EligibilityPolicyVersion("other-policy"))
            .Evaluate(
                repository,
                new AnalysisAdmissionResult(
                    repository.Reference,
                    AnalysisAdmissionDecision.Analyze,
                    reason: null,
                    new EligibilityPolicyVersion("other-policy")),
                AnalysisStatus.Completed,
                CreateAnalysisReference("alpha"));

        Assert.Throws<ArgumentException>(() => new CrossRepositoryEvidence(
            [foreign],
            [],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_WhenContributionsUseDifferentAnalysisVersions_Rejects()
    {
        ProfileRepository repository = CreateProfileRepository("beta");
        RepositoryContributionAssessment other = new RepositoryContributionPolicy(
                PolicyVersion)
            .Evaluate(
                repository,
                CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
                AnalysisStatus.Completed,
                new RepositoryAnalysisReference(
                    Guid.Parse("04800000-0000-0000-0000-000000000002"),
                    CreateAnalysisReference("beta").Repository,
                    new AnalysisVersion("other-analyzer", "other-rules", 1)));

        Assert.Throws<ArgumentException>(() => new CrossRepositoryEvidence(
            [Contributing("alpha"), other],
            [],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true));
    }

    [Fact]
    public void Constructor_ReportsUndeterminedContributionsWithoutTreatingThemAsAbsence()
    {
        RepositoryContributionAssessment contributing = Contributing("alpha");
        RepositoryContributionAssessment unresolved = Unresolved("beta");

        CrossRepositoryEvidence evidence = new(
            [contributing, unresolved],
            [Observation(SignalKey, [contributing], eligibleRepositoryCount: 1)],
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: false);

        Assert.Equal(1, evidence.EligibleRepositoryCount);
        Assert.Equal(1, evidence.UndeterminedContributionRepositoryCount);
        Assert.False(evidence.IsEvidenceSurfaceComplete);
    }

    private static RepositoryContributionAssessment Contributing(
        string name,
        RepositoryConditionState fork = RepositoryConditionState.Absent)
    {
        ProfileRepository repository = CreateProfileRepository(name, fork);

        return new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference(name));
    }

    private static RepositoryContributionAssessment Restricted(string name)
    {
        ProfileRepository repository = CreateProfileRepository(
            name,
            empty: RepositoryConditionState.Present);

        return new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Skip),
            analysisStatus: null,
            analysisReference: null);
    }

    private static RepositoryContributionAssessment Unresolved(string name)
    {
        ProfileRepository repository = CreateProfileRepository(name);

        return new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Failed,
            analysisReference: null);
    }

    private static ObservedProfileEvidence Observation(
        string observationKey,
        IReadOnlyList<RepositoryContributionAssessment> contributions,
        int eligibleRepositoryCount) =>
        new(
            observationKey,
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            contributions.Select(contribution =>
                Evidence(contribution, observationKey)),
            contributions.Count(contribution =>
                contribution.IndependentRepetition.IsAllowed),
            eligibleRepositoryCount,
            undeterminedEvidenceRepositoryCount: 0,
            RuleVersion);

    private static ProfileEvidenceReference Evidence(
        RepositoryContributionAssessment contribution,
        string signalKey) =>
        new(
            contribution.Reference,
            contribution.AnalysisReference
                ?? CreateAnalysisReference(contribution.Reference.Name),
            signalKey,
            ["evidence-key"]);
}

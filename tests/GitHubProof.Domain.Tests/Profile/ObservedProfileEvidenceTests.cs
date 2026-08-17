using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ObservedProfileEvidenceTests
{
    private const string SignalKey = "repository.readme.exists";

    [Fact]
    public void Constructor_OrdersEvidenceReferencesDeterministically()
    {
        ObservedProfileEvidence observation = Create(
            [Evidence("beta"), Evidence("alpha")],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 3,
            undeterminedEvidenceRepositoryCount: 1);

        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            observation.ContributingRepositories.Select(
                repository => repository.FullName));
        Assert.Equal(2, observation.SupportingRepositoryCount);
    }

    [Fact]
    public void Constructor_WhenObservationHasNoSupportingEvidence_Rejects()
    {
        Assert.Throws<ArgumentException>(() => Create(
            [],
            independentSupportingRepositoryCount: 0,
            eligibleRepositoryCount: 1,
            undeterminedEvidenceRepositoryCount: 0));
    }

    [Fact]
    public void Constructor_WhenEvidenceBelongsToAnotherSignal_Rejects()
    {
        Assert.Throws<ArgumentException>(() => Create(
            [
                new ProfileEvidenceReference(
                    CreateReference("alpha"),
                    CreateAnalysisReference("alpha"),
                    "another.signal",
                    ["evidence-key"])
            ],
            independentSupportingRepositoryCount: 0,
            eligibleRepositoryCount: 1,
            undeterminedEvidenceRepositoryCount: 0));
    }

    [Fact]
    public void Constructor_WhenOneRepositorySupportsTwice_Rejects()
    {
        Assert.Throws<ArgumentException>(() => Create(
            [Evidence("alpha"), Evidence("alpha")],
            independentSupportingRepositoryCount: 1,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0));
    }

    [Fact]
    public void Constructor_WhenIndependentSupportExceedsSupport_Rejects()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            [Evidence("alpha")],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0));
    }

    [Fact]
    public void Constructor_WhenCountedRepositoriesExceedTheEligibleSurface_Rejects()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            [Evidence("alpha"), Evidence("beta")],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 1));
    }

    private static ObservedProfileEvidence Create(
        IEnumerable<ProfileEvidenceReference> evidenceReferences,
        int independentSupportingRepositoryCount,
        int eligibleRepositoryCount,
        int undeterminedEvidenceRepositoryCount) =>
        new(
            SignalKey,
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            evidenceReferences,
            independentSupportingRepositoryCount,
            eligibleRepositoryCount,
            undeterminedEvidenceRepositoryCount,
            RuleVersion);

    private static ProfileEvidenceReference Evidence(string name) =>
        new(
            CreateReference(name),
            CreateAnalysisReference(name),
            SignalKey,
            ["evidence-key"]);
}

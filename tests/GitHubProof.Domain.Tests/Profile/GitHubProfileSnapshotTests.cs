using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class GitHubProfileSnapshotTests
{
    [Fact]
    public void Constructor_NormalizesCaptureTimeAndCollectionWarnings()
    {
        DateTimeOffset localTime = new(
            2026,
            8,
            15,
            12,
            30,
            0,
            TimeSpan.FromHours(3));

        GitHubProfileSnapshot snapshot = new(
            CreateIdentity(),
            localTime,
            new GitHubProfileMetadata(" The Octocat ", localTime, publicRepositoryCount: 8),
            repositories: null,
            availability:
            [
                DataAvailability.Available("profile", localTime)
            ],
            collectionWarnings: [" partial manifest ", "partial manifest", "  "]);

        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), snapshot.CapturedAt);
        Assert.Equal("The Octocat", snapshot.Metadata!.DisplayName);
        Assert.Equal(["partial manifest"], snapshot.CollectionWarnings);
        Assert.Empty(snapshot.Repositories.Repositories);
    }

    [Fact]
    public void Constructor_RejectsDuplicateAvailabilityKeys()
    {
        Assert.Throws<ArgumentException>(() => new GitHubProfileSnapshot(
            CreateIdentity(),
            DateTimeOffset.Parse("2026-08-15T09:30:00Z"),
            metadata: null,
            repositories: null,
            availability:
            [
                DataAvailability.Available("profile", DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
                DataAvailability.Unavailable("profile", DataAvailabilityStatus.Failed, "duplicate")
            ],
            collectionWarnings: null));
    }

    [Fact]
    public void FindAvailability_ReturnsRequestedDataKeyOnly()
    {
        GitHubProfileSnapshot snapshot = new(
            CreateIdentity(),
            DateTimeOffset.Parse("2026-08-15T09:30:00Z"),
            metadata: null,
            repositories: null,
            availability:
            [
                DataAvailability.Available("profile", DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
                DataAvailability.Unavailable(
                    "profile-repositories",
                    DataAvailabilityStatus.RateLimited,
                    "GitHub rate limit reached.")
            ],
            collectionWarnings: null);

        Assert.Equal(
            DataAvailabilityStatus.RateLimited,
            snapshot.FindAvailability("profile-repositories")!.Status);
        Assert.Null(snapshot.FindAvailability("commits"));
    }

    [Fact]
    public void Constructor_RequiresIdentity()
    {
        Assert.Throws<ArgumentNullException>(() => new GitHubProfileSnapshot(
            identity: null!,
            DateTimeOffset.Parse("2026-08-15T09:30:00Z"),
            metadata: null,
            repositories: null,
            availability: null,
            collectionWarnings: null));
    }

    private static GitHubProfileIdentity CreateIdentity() =>
        new("octocat", new Uri("https://github.com/octocat"));
}

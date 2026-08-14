using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class RepositorySnapshotTests
{
    [Fact]
    public void Constructor_NormalizesSnapshotDataAndPreservesCollectionState()
    {
        DateTimeOffset localTime = new(
            2026,
            8,
            15,
            12,
            30,
            0,
            TimeSpan.FromHours(3));

        RepositorySnapshot snapshot = CreateSnapshot(
            capturedAt: localTime,
            headCommitSha: " abc123 ",
            metadata: new RepositoryMetadata(
                " description ",
                " https://example.test/demo ",
                localTime,
                issuesEnabled: true),
            commits: new RepositoryCollection<RepositoryCommit>(
                [new RepositoryCommit(" abc123 ", localTime, " initial ", null)],
                totalCount: 42,
                isComplete: false),
            topics: [" dotnet ", "DOTNET", " github-api "],
            languages: new Dictionary<string, long>
            {
                [" C# "] = 1200
            },
            availability:
            [
                DataAvailability.Available("commits", localTime)
            ],
            collectionWarnings: [" partial history ", "partial history"]);

        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), snapshot.CapturedAt);
        Assert.Equal("abc123", snapshot.HeadCommitSha);
        Assert.Equal("description", snapshot.Metadata!.Description);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), snapshot.Metadata.CreatedAt);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), snapshot.Commits.Items[0].CommittedAt);
        Assert.Equal(42, snapshot.Commits.TotalCount);
        Assert.False(snapshot.Commits.IsComplete);
        Assert.Equal(["dotnet", "github-api"], snapshot.Topics);
        Assert.Equal(1200, snapshot.Languages["c#"]);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), snapshot.Availability[0].RetrievedAt);
        Assert.Equal(["partial history"], snapshot.CollectionWarnings);
    }

    [Fact]
    public void Constructor_AllowsPartialSnapshotWithoutTurningMissingDataIntoEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            headCommitSha: null,
            metadata: null,
            commits: null,
            availability:
            [
                DataAvailability.Unavailable(
                    "commits",
                    DataAvailabilityStatus.RateLimited,
                    "GitHub rate limit reached.")
            ]);

        Assert.Null(snapshot.HeadCommitSha);
        Assert.Null(snapshot.Metadata);
        Assert.Empty(snapshot.Commits.Items);
        Assert.Equal(
            DataAvailabilityStatus.RateLimited,
            snapshot.FindAvailability(" commits ")!.Status);
    }

    [Fact]
    public void Constructor_RejectsDuplicateAvailabilityKeys()
    {
        DataAvailability first = DataAvailability.Unavailable(
            "commits",
            DataAvailabilityStatus.Failed,
            "First failure.");
        DataAvailability second = DataAvailability.Unavailable(
            "commits",
            DataAvailabilityStatus.RateLimited,
            "Second failure.");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateSnapshot(availability: [first, second]));

        Assert.Equal("availability", exception.ParamName);
    }

    [Fact]
    public void RepositoryCollection_RejectsTotalBelowCollectedItemCount()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RepositoryCollection<string>(["one", "two"], totalCount: 1));

        Assert.Equal("totalCount", exception.ParamName);
    }

    [Fact]
    public void PullRequest_RejectsMergedButOpenState()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new RepositoryPullRequest(
                1,
                "Feature",
                isClosed: false,
                isMerged: true,
                DateTimeOffset.UtcNow));

        Assert.Equal("isClosed", exception.ParamName);
    }

    private static RepositorySnapshot CreateSnapshot(
        DateTimeOffset? capturedAt = null,
        string? headCommitSha = "abc123",
        RepositoryMetadata? metadata = null,
        RepositoryCollection<RepositoryCommit>? commits = null,
        IEnumerable<string>? topics = null,
        IReadOnlyDictionary<string, long>? languages = null,
        IEnumerable<DataAvailability>? availability = null,
        IEnumerable<string>? collectionWarnings = null)
    {
        return new RepositorySnapshot(
            new RepositoryIdentity(
                "owner",
                "repository",
                new Uri("https://github.com/owner/repository"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            capturedAt ?? DateTimeOffset.UtcNow,
            headCommitSha,
            metadata,
            readme: null,
            fileTree: null,
            commits,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics,
            license: null,
            languages,
            contributors: null,
            availability,
            collectionWarnings);
    }
}

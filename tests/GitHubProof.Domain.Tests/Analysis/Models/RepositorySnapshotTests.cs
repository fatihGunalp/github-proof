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
    public void Constructor_NormalizesTagsAndPreservesCollectionState()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            tags: new RepositoryCollection<RepositoryTag>(
                [new RepositoryTag(" v1.0.0 ")],
                totalCount: 3,
                isComplete: false));

        RepositoryTag tag = Assert.Single(snapshot.Tags.Items);

        Assert.Equal("v1.0.0", tag.Name);
        Assert.Equal(3, snapshot.Tags.TotalCount);
        Assert.False(snapshot.Tags.IsComplete);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<RepositoryTag>)snapshot.Tags.Items).Add(new RepositoryTag("v2.0.0")));
    }

    [Fact]
    public void Constructor_AllowsEmptyTagsCollection()
    {
        RepositorySnapshot snapshot = CreateSnapshot(tags: null);

        Assert.Empty(snapshot.Tags.Items);
        Assert.Equal(0, snapshot.Tags.TotalCount);
        Assert.True(snapshot.Tags.IsComplete);
    }

    [Fact]
    public void Constructor_PreservesTagAvailabilityIndependentlyFromReleases()
    {
        DateTimeOffset retrievedAt = new(2026, 8, 15, 9, 30, 0, TimeSpan.Zero);
        RepositorySnapshot snapshot = CreateSnapshot(
            availability:
            [
                DataAvailability.Available("releases", retrievedAt),
                DataAvailability.Unavailable(
                    "tags",
                    DataAvailabilityStatus.RateLimited,
                    "Tag collection was rate limited.")
            ]);

        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("releases")!.Status);
        Assert.Equal(
            DataAvailabilityStatus.RateLimited,
            snapshot.FindAvailability("tags")!.Status);
    }

    [Fact]
    public void Constructor_PreservesRecognizedLicenseAndLicenseFileIndependently()
    {
        RepositoryLicense license = new(
            "mit",
            "MIT License",
            "MIT",
            "LICENSE");
        RepositoryFile licenseFile = new(
            " LICENSE ",
            isDirectory: false,
            size: 1080,
            content: "license text");

        RepositorySnapshot snapshot = CreateSnapshot(
            license: license,
            licenseFile: licenseFile);

        RepositoryFile actualLicenseFile = Assert.IsType<RepositoryFile>(snapshot.LicenseFile);

        Assert.Same(license, snapshot.License);
        Assert.Same(licenseFile, actualLicenseFile);
        Assert.Equal("LICENSE", actualLicenseFile.Path);
        Assert.Equal(1080, actualLicenseFile.Size);
        Assert.Equal("license text", actualLicenseFile.Content);
    }

    [Fact]
    public void Constructor_PreservesUnrecognizedLicenseFileWithoutCreatingLicenseMetadata()
    {
        RepositoryFile licenseFile = new(
            " LICENSE.custom ",
            isDirectory: false,
            size: 42,
            content: null);

        RepositorySnapshot snapshot = CreateSnapshot(
            license: null,
            licenseFile: licenseFile);

        RepositoryFile actualLicenseFile = Assert.IsType<RepositoryFile>(snapshot.LicenseFile);

        Assert.Null(snapshot.License);
        Assert.Same(licenseFile, actualLicenseFile);
        Assert.Equal("LICENSE.custom", actualLicenseFile.Path);
    }

    [Fact]
    public void Constructor_DefaultsToNoLicenseFileEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot();

        Assert.Null(snapshot.License);
        Assert.Null(snapshot.LicenseFile);
        Assert.Empty(snapshot.Tags.Items);
        Assert.Empty(snapshot.Releases.Items);
        Assert.Null(snapshot.Readme);
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
        IEnumerable<string>? collectionWarnings = null,
        RepositoryCollection<RepositoryTag>? tags = null,
        RepositoryLicense? license = null,
        RepositoryFile? licenseFile = null)
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
            license,
            languages,
            contributors: null,
            availability,
            collectionWarnings,
            tags,
            licenseFile);
    }
}

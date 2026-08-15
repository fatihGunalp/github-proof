using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class ReleaseUsageSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset ReleasesRetrievedAt = CapturedAt.AddMinutes(1);
    private static readonly DateTimeOffset TagsRetrievedAt = CapturedAt.AddMinutes(2);
    private static readonly DateTimeOffset EvaluatedAt = CapturedAt.AddMinutes(3);

    [Fact]
    public void Definition_MatchesCanonicalReleaseUsageContract()
    {
        SignalDefinition definition = new ReleaseUsageSignalEvaluator().Definition;

        Assert.Equal("process.release-usage", definition.Key);
        Assert.Equal(AnalysisCategory.DevelopmentProcessVisibility, definition.Category);
        Assert.Equal(6, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["releases", "tags"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WithPublishedReleaseAndNotes_ReturnsPresentSix()
    {
        SignalResult result = EvaluateAvailable(
            [Published("v1.0.0", "Changes")],
            tags: []);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(6, result.Score);
        Assert.Null(result.Recommendation);
        Assert.Single(result.Limitations);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Contains("published=1", evidence.Value);
        Assert.Contains("publishedWithNotes=1", evidence.Value);
        Assert.Contains("draft=0", evidence.Value);
        Assert.Contains("tags=0", evidence.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Evaluate_WithPublishedReleaseWithoutNormalizedNotes_ReturnsPartialFour(
        string? notes)
    {
        SignalResult result = EvaluateAvailable(
            [Published("v1.0.0", notes)],
            tags: []);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
        Assert.NotNull(result.Recommendation);
        Assert.Contains("publishedWithNotes=0", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithTagOnly_ReturnsPartialThree()
    {
        SignalResult result = EvaluateAvailable(
            releases: [],
            tags: [new RepositoryTag("v1.0.0")]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(3, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithDraftOnly_ReturnsPartialTwo()
    {
        SignalResult result = EvaluateAvailable(
            [Draft("v1.0.0")],
            tags: []);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithNoReleaseOrTag_ReturnsMissing()
    {
        SignalResult result = EvaluateAvailable(releases: [], tags: []);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithPublishedNotesAndUnavailableTags_PreservesTerminalPresent()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Published("v1.0.0", "Changes")],
            releasesComplete: true,
            tags: [],
            tagsComplete: false,
            DataAvailability.Available("releases", ReleasesRetrievedAt),
            DataAvailability.Unavailable(
                "tags",
                DataAvailabilityStatus.RateLimited,
                "Tags failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(6, result.Score);
        Assert.Contains("tagsAvailability=RateLimited", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithPublishedReleaseWithoutNotes_DoesNotRequireTags()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Published("v1.0.0", notes: null)],
            releasesComplete: true,
            tags: [],
            tagsComplete: false,
            DataAvailability.Available("releases", ReleasesRetrievedAt),
            DataAvailability.Unavailable(
                "tags",
                DataAvailabilityStatus.Failed,
                "Tags failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Fact]
    public void Evaluate_WithObservedTagAndCompleteReleases_PreservesTagPartialWhenTagPagingFails()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            releases: [],
            releasesComplete: true,
            tags: [new RepositoryTag("v1.0.0")],
            tagsComplete: false,
            DataAvailability.Available("releases", ReleasesRetrievedAt),
            DataAvailability.Unavailable(
                "tags",
                DataAvailabilityStatus.RateLimited,
                "Later tag page failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(3, result.Score);
    }

    [Fact]
    public void Evaluate_WithUnavailableReleasesAndObservedTag_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            releases: [],
            releasesComplete: false,
            tags: [new RepositoryTag("v1.0.0")],
            tagsComplete: true,
            DataAvailability.Unavailable(
                "releases",
                DataAvailabilityStatus.RateLimited,
                "Releases failed."),
            DataAvailability.Available("tags", TagsRetrievedAt));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenTagsAreUnavailableAndNoTerminalEvidenceExists_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            releases: [],
            releasesComplete: true,
            tags: [],
            tagsComplete: false,
            DataAvailability.Available("releases", ReleasesRetrievedAt),
            DataAvailability.Unavailable(
                "tags",
                DataAvailabilityStatus.Forbidden,
                "Tags failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenReleasePagingIsIncompleteButNotesAreObserved_PreservesTerminalPresent()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Published("v1.0.0", "Changes")],
            releasesComplete: false,
            tags: [],
            tagsComplete: false,
            DataAvailability.Unavailable(
                "releases",
                DataAvailabilityStatus.RateLimited,
                "Later release page failed."),
            DataAvailability.Unavailable(
                "tags",
                DataAvailabilityStatus.Failed,
                "Tags failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(6, result.Score);
    }

    [Fact]
    public void Evaluate_WhenReleasePagingIsIncompleteWithoutObservedNotes_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Published("v1.0.0", notes: null)],
            releasesComplete: false,
            tags: [],
            tagsComplete: true,
            DataAvailability.Unavailable(
                "releases",
                DataAvailabilityStatus.RateLimited,
                "Later release page failed."),
            DataAvailability.Available("tags", TagsRetrievedAt));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_InputOrderDoesNotAffectResultAndTimeIsReadOnce()
    {
        RepositoryRelease[] releases =
        [
            Draft("v2.0.0"),
            Published("v1.0.0", "Changes")
        ];
        RepositoryTag[] tags =
        [new("v2.0.0"), new("v1.0.0")];
        RepositorySnapshot firstSnapshot = CreateAvailableSnapshot(releases, tags);
        RepositorySnapshot secondSnapshot = CreateAvailableSnapshot(
            releases.Reverse(),
            tags.Reverse());
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new ReleaseUsageSignalEvaluator(firstTimeProvider).Evaluate(firstSnapshot);
        SignalResult second = new ReleaseUsageSignalEvaluator(secondTimeProvider).Evaluate(secondSnapshot);

        AssertEquivalent(first, second);
        Assert.Equal(TagsRetrievedAt, first.Evidence[0].ObservedAt);
        Assert.Equal(1, firstTimeProvider.CallCount);
        Assert.Equal(1, secondTimeProvider.CallCount);
    }

    private static SignalResult EvaluateAvailable(
        IEnumerable<RepositoryRelease> releases,
        IEnumerable<RepositoryTag> tags) =>
        Evaluate(CreateAvailableSnapshot(releases, tags));

    private static SignalResult Evaluate(RepositorySnapshot snapshot) =>
        new ReleaseUsageSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

    private static RepositorySnapshot CreateAvailableSnapshot(
        IEnumerable<RepositoryRelease> releases,
        IEnumerable<RepositoryTag> tags) =>
        CreateSnapshot(
            releases,
            releasesComplete: true,
            tags,
            tagsComplete: true,
            DataAvailability.Available("releases", ReleasesRetrievedAt),
            DataAvailability.Available("tags", TagsRetrievedAt));

    private static RepositoryRelease Published(string tag, string? notes) =>
        new(
            tag,
            hasPublishedRelease: true,
            isDraft: false,
            name: null,
            notes,
            CapturedAt.AddDays(-1));

    private static RepositoryRelease Draft(string tag) =>
        new(
            tag,
            hasPublishedRelease: false,
            isDraft: true,
            name: null,
            notes: null,
            publishedAt: null);

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<RepositoryRelease> releases,
        bool releasesComplete,
        IEnumerable<RepositoryTag> tags,
        bool tagsComplete,
        DataAvailability releasesAvailability,
        DataAvailability tagsAvailability)
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
            CapturedAt,
            headCommitSha: "abc123",
            new RepositoryMetadata(null, null, CapturedAt.AddDays(-30), issuesEnabled: true),
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            new RepositoryCollection<RepositoryRelease>(releases, isComplete: releasesComplete),
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            [releasesAvailability, tagsAvailability],
            collectionWarnings: null,
            new RepositoryCollection<RepositoryTag>(tags, isComplete: tagsComplete));
    }

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(first.Explanation, second.Explanation);
        Assert.Equal(first.Recommendation, second.Recommendation);
        Assert.Equal(first.Limitations, second.Limitations);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
    }

    private sealed class CountingTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public int CallCount { get; private set; }

        public override DateTimeOffset GetUtcNow()
        {
            CallCount++;
            return utcNow;
        }
    }
}

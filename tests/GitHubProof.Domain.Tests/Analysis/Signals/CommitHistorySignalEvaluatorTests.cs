using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class CommitHistorySignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalCommitHistoryContract()
    {
        SignalDefinition definition = new CommitHistorySignalEvaluator().Definition;

        Assert.Equal("process.commit-history", definition.Key);
        Assert.Equal(AnalysisCategory.DevelopmentProcessVisibility, definition.Category);
        Assert.Equal(7, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["commits"], definition.RequiredDataKeys);
    }

    [Theory]
    [InlineData(10, SignalStatus.Strong, 7)]
    [InlineData(9, SignalStatus.Present, 6)]
    [InlineData(7, SignalStatus.Present, 6)]
    [InlineData(6, SignalStatus.Present, 5)]
    [InlineData(5, SignalStatus.Present, 5)]
    [InlineData(4, SignalStatus.Present, 4)]
    [InlineData(3, SignalStatus.Present, 4)]
    [InlineData(2, SignalStatus.Partial, 2)]
    [InlineData(1, SignalStatus.Partial, 1)]
    [InlineData(0, SignalStatus.Missing, 0)]
    [InlineData(11, SignalStatus.Strong, 7)]
    public void Evaluate_WithCompleteAvailableCommits_AppliesExactThresholds(
        int commitCount,
        SignalStatus expectedStatus,
        int expectedScore)
    {
        CommitHistorySignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult result = evaluator.Evaluate(CreateSnapshot(
            CreateCommits(commitCount),
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt)));

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedScore, result.Score);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("RepositoryCommits", evidence.Source);
        Assert.Contains($"observedCount={commitCount.ToString(System.Globalization.CultureInfo.InvariantCulture)}", evidence.Value);
        Assert.Contains("complete=True", evidence.Value);
        Assert.Contains("availability=Available", evidence.Value);
        Assert.Equal("main", evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.NotApplicable)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenCommitsAreUnavailable_DoesNotInferMissingOrPositiveScore(
        DataAvailabilityStatus status)
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            CreateCommits(12),
            isComplete: true,
            DataAvailability.Unavailable("commits", status, "Collection failed."));

        SignalResult result = new CommitHistorySignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("availability=", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WhenAvailabilityIsNotRecorded_ReturnsUnavailableEvidence()
    {
        SignalResult result = new CommitHistorySignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(CreateSnapshot(
                CreateCommits(0),
                isComplete: true,
                commitsAvailability: null));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenPaginationIsIncomplete_DoesNotUseObservedStrongThreshold()
    {
        SignalResult result = new CommitHistorySignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(CreateSnapshot(
                CreateCommits(12),
                isComplete: false,
                DataAvailability.Available("commits", RetrievedAt)));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("koleksiyon tam: False", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_UsesNormalizedSnapshotItemsWithoutSecondaryShaDeduplication()
    {
        RepositoryCommit[] commits =
        [
            new("same-sha", CapturedAt.AddDays(-2), null, null),
            new("same-sha", CapturedAt.AddDays(-1), null, null)
        ];

        SignalResult result = new CommitHistorySignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(CreateSnapshot(
                commits,
                isComplete: true,
                DataAvailability.Available("commits", RetrievedAt)));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
        Assert.Contains("observedCount=2", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_ReadsTimeProviderOnceAndIsDeterministic()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            CreateCommits(5).Reverse(),
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt));
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new CommitHistorySignalEvaluator(firstTimeProvider).Evaluate(snapshot);
        SignalResult second = new CommitHistorySignalEvaluator(secondTimeProvider).Evaluate(snapshot);

        AssertEquivalent(first, second);
        Assert.Equal(EvaluatedAt, first.EvaluatedAt);
        Assert.Equal(1, firstTimeProvider.CallCount);
        Assert.Equal(1, secondTimeProvider.CallCount);
    }

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.SignalVersion, second.SignalVersion);
        Assert.Equal(first.Category, second.Category);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.MaximumScore, second.MaximumScore);
        Assert.Equal(first.IncludeInOverallScore, second.IncludeInOverallScore);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
    }

    private static IReadOnlyList<RepositoryCommit> CreateCommits(int count)
    {
        return Enumerable.Range(1, count)
            .Select(index => new RepositoryCommit(
                $"sha-{index}",
                CapturedAt.AddHours(-index),
                null,
                null))
            .ToArray();
    }

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<RepositoryCommit> commits,
        bool isComplete,
        DataAvailability? commitsAvailability)
    {
        List<DataAvailability> availability =
        [DataAvailability.Available("metadata", RetrievedAt)];

        if (commitsAvailability is not null)
        {
            availability.Add(commitsAvailability);
        }

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
            new RepositoryCollection<RepositoryCommit>(commits, isComplete: isComplete),
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability,
            collectionWarnings: null);
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

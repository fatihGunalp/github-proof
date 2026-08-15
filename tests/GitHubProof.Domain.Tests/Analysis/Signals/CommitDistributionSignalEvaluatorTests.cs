using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class CommitDistributionSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalCommitDistributionContract()
    {
        SignalDefinition definition = new CommitDistributionSignalEvaluator().Definition;

        Assert.Equal("process.commit-distribution", definition.Key);
        Assert.Equal("Commitlerin Zamana Yayılımı", definition.Name);
        Assert.Equal(AnalysisCategory.DevelopmentProcessVisibility, definition.Category);
        Assert.Equal(8, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["commits", "metadata"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenRepositoryIsYoungerThanFourteenDays_ReturnsNotApplicableBeforeCommitAvailability()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            createdAt: CapturedAt - TimeSpan.FromDays(14) + TimeSpan.FromTicks(1),
            commits: [],
            isComplete: false,
            DataAvailability.Unavailable(
                "commits",
                DataAvailabilityStatus.Failed,
                "Collection failed."));

        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        Assert.Equal(SignalStatus.NotApplicable, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
        Assert.Contains("14", result.Explanation);
        Assert.Contains("activeDays=not-evaluated", Assert.Single(result.Evidence).Value);
        Assert.Contains("firstCommitAt=not-evaluated", result.Evidence[0].Value);
        Assert.Contains("lastCommitAt=not-evaluated", result.Evidence[0].Value);
    }

    [Fact]
    public void Evaluate_WhenRepositoryAgeIsExactlyFourteenDays_IsApplicable()
    {
        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(CreateSnapshot(
                createdAt: CapturedAt - TimeSpan.FromDays(14),
                commits: [],
                isComplete: true,
                DataAvailability.Available("commits", RetrievedAt)));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Theory]
    [InlineData(7, SignalStatus.Present, 7)]
    [InlineData(6, SignalStatus.Present, 7)]
    [InlineData(5, SignalStatus.Present, 6)]
    [InlineData(4, SignalStatus.Present, 5)]
    [InlineData(3, SignalStatus.Partial, 4)]
    [InlineData(2, SignalStatus.Partial, 2)]
    [InlineData(1, SignalStatus.Partial, 1)]
    [InlineData(0, SignalStatus.Missing, 0)]
    public void Evaluate_WithCompleteAvailableCommits_AppliesActiveDayThresholds(
        int activeDays,
        SignalStatus expectedStatus,
        int expectedScore)
    {
        SignalResult result = Evaluate(CreateSequentialCommits(activeDays));

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedScore, result.Score);
        Assert.Equal(expectedStatus is SignalStatus.Partial or SignalStatus.Missing,
            result.Recommendation is not null);
        Assert.Equal(2, result.Limitations.Count);
    }

    [Fact]
    public void Evaluate_WithEightActiveDaysAndExactFourteenDaySpan_ReturnsStrong()
    {
        DateTimeOffset first = CapturedAt.AddDays(-20);
        RepositoryCommit[] commits =
        [
            CreateCommit(8, first.AddDays(14)),
            CreateCommit(3, first.AddDays(2)),
            CreateCommit(1, first),
            CreateCommit(6, first.AddDays(5)),
            CreateCommit(4, first.AddDays(3)),
            CreateCommit(2, first.AddDays(1)),
            CreateCommit(7, first.AddDays(6)),
            CreateCommit(5, first.AddDays(4))
        ];

        SignalResult result = Evaluate(commits);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
        Assert.Null(result.Recommendation);
        Assert.Contains("activeDays=8", Assert.Single(result.Evidence).Value);
        Assert.Contains("firstCommitAt=", result.Evidence[0].Value);
        Assert.Contains("lastCommitAt=", result.Evidence[0].Value);
        Assert.Contains("spanDays=14", result.Evidence[0].Value);
    }

    [Fact]
    public void Evaluate_WithEightActiveDaysButShortSpan_ReturnsPresentSeven()
    {
        SignalResult result = Evaluate(CreateSequentialCommits(8));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(7, result.Score);
    }

    [Fact]
    public void Evaluate_CountsDistinctUtcCalendarDays()
    {
        RepositoryCommit[] commits =
        [
            new("sha-1", new DateTimeOffset(2026, 8, 1, 23, 30, 0, TimeSpan.FromHours(2)), null, null),
            new("sha-2", DateTimeOffset.Parse("2026-08-01T22:30:00Z"), null, null)
        ];

        SignalResult result = Evaluate(commits);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(1, result.Score);
        Assert.Contains("activeDays=1", Assert.Single(result.Evidence).Value);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenCommitsAreUnavailableForApplicableRepository_ReturnsUnavailableEvidence(
        DataAvailabilityStatus status)
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            CapturedAt.AddDays(-30),
            CreateSequentialCommits(8),
            isComplete: true,
            DataAvailability.Unavailable("commits", status, "Collection failed."));

        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenPaginationIsIncomplete_DoesNotUseObservedStrongEvidence()
    {
        DateTimeOffset first = CapturedAt.AddDays(-20);
        IReadOnlyList<RepositoryCommit> commits = Enumerable.Range(0, 8)
            .Select(index => CreateCommit(index + 1, first.AddDays(index == 7 ? 14 : index)))
            .ToArray();
        RepositorySnapshot snapshot = CreateSnapshot(
            CapturedAt.AddDays(-30),
            commits,
            isComplete: false,
            DataAvailability.Available("commits", RetrievedAt));

        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("commitsComplete=False", Assert.Single(result.Evidence).Value);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenMetadataIsUnavailable_ReturnsUnavailableEvidence(
        DataAvailabilityStatus status)
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            CapturedAt.AddDays(-30),
            CreateSequentialCommits(8),
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt),
            DataAvailability.Unavailable("metadata", status, "Metadata failed."));

        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenMetadataValueIsAbsentDespiteAvailability_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            createdAt: null,
            CreateSequentialCommits(8),
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt));

        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenCreatedAtIsAfterCapturedAt_ReturnsUnavailableEvidence()
    {
        SignalResult result = new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(CreateSnapshot(
                createdAt: CapturedAt.AddTicks(1),
                CreateSequentialCommits(1),
                isComplete: true,
                DataAvailability.Available("commits", RetrievedAt)));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_UsesSnapshotCapturedAtRatherThanEvaluationTimeForRepositoryAge()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            createdAt: CapturedAt - TimeSpan.FromDays(14) + TimeSpan.FromHours(1),
            commits: [],
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt));
        CountingTimeProvider timeProvider = new(EvaluatedAt.AddYears(5));

        SignalResult result = new CommitDistributionSignalEvaluator(timeProvider).Evaluate(snapshot);

        Assert.Equal(SignalStatus.NotApplicable, result.Status);
        Assert.Equal(1, timeProvider.CallCount);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentOrderIndependentState()
    {
        IReadOnlyList<RepositoryCommit> commits = CreateSequentialCommits(5);
        RepositorySnapshot firstSnapshot = CreateSnapshot(
            CapturedAt.AddDays(-30),
            commits,
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt));
        RepositorySnapshot secondSnapshot = CreateSnapshot(
            CapturedAt.AddDays(-30),
            commits.Reverse(),
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt));
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new CommitDistributionSignalEvaluator(firstTimeProvider).Evaluate(firstSnapshot);
        SignalResult second = new CommitDistributionSignalEvaluator(secondTimeProvider).Evaluate(secondSnapshot);

        AssertEquivalent(first, second);
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
        Assert.Equal(first.Explanation, second.Explanation);
        Assert.Equal(first.Recommendation, second.Recommendation);
        Assert.Equal(first.Limitations, second.Limitations);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
    }

    private static SignalResult Evaluate(IEnumerable<RepositoryCommit> commits)
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            CapturedAt.AddDays(-30),
            commits,
            isComplete: true,
            DataAvailability.Available("commits", RetrievedAt));

        return new CommitDistributionSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
    }

    private static IReadOnlyList<RepositoryCommit> CreateSequentialCommits(int activeDays)
    {
        return Enumerable.Range(0, activeDays)
            .Select(index => CreateCommit(index + 1, CapturedAt.AddDays(-(index + 1))))
            .ToArray();
    }

    private static RepositoryCommit CreateCommit(int index, DateTimeOffset committedAt)
    {
        return new RepositoryCommit($"sha-{index}", committedAt, null, null);
    }

    private static RepositorySnapshot CreateSnapshot(
        DateTimeOffset? createdAt,
        IEnumerable<RepositoryCommit> commits,
        bool isComplete,
        DataAvailability commitsAvailability,
        DataAvailability? metadataAvailability = null)
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
            createdAt is null
                ? null
                : new RepositoryMetadata(null, null, createdAt.Value, issuesEnabled: true),
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
            [
                metadataAvailability ?? DataAvailability.Available("metadata", RetrievedAt),
                commitsAvailability
            ],
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

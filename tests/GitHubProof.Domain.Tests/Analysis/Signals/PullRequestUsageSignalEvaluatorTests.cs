using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class PullRequestUsageSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt = CapturedAt.AddMinutes(1);
    private static readonly DateTimeOffset EvaluatedAt = CapturedAt.AddMinutes(2);

    [Fact]
    public void Definition_MatchesCanonicalPullRequestUsageContract()
    {
        SignalDefinition definition = new PullRequestUsageSignalEvaluator().Definition;

        Assert.Equal("process.pull-request-usage", definition.Key);
        Assert.Equal(AnalysisCategory.DevelopmentProcessVisibility, definition.Category);
        Assert.Equal(6, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["pull-requests"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WithNoPullRequests_ReturnsMissing()
    {
        SignalResult result = Evaluate(CreateSnapshot(
            pullRequests: [],
            isComplete: true,
            DataAvailability.Available("pull-requests", RetrievedAt)));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithOneOpenPullRequest_ReturnsPartialTwo()
    {
        SignalResult result = EvaluateAvailable(
            [CreatePullRequest(1, isClosed: false, isMerged: false)]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
        Assert.Contains("open=1", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithOneClosedUnmergedPullRequest_ReturnsPartialTwo()
    {
        SignalResult result = EvaluateAvailable(
            [CreatePullRequest(1, isClosed: true, isMerged: false)]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
        Assert.Contains("closedUnmerged=1", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithOneMergedPullRequest_ReturnsPresentFour()
    {
        SignalResult result = EvaluateAvailable(
            [CreatePullRequest(1, isClosed: true, isMerged: true)]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithTwoMergedPullRequests_ReturnsStrongSix()
    {
        SignalResult result = EvaluateAvailable(
        [
            CreatePullRequest(1, isClosed: true, isMerged: true),
            CreatePullRequest(2, isClosed: true, isMerged: true)
        ]);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(6, result.Score);
    }

    [Fact]
    public void Evaluate_WithMergedAndUnmergedCombination_UsesMergedThreshold()
    {
        SignalResult result = EvaluateAvailable(
        [
            CreatePullRequest(1, isClosed: false, isMerged: false),
            CreatePullRequest(2, isClosed: true, isMerged: false),
            CreatePullRequest(3, isClosed: true, isMerged: true)
        ]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Contains("total=3", evidence.Value);
        Assert.Contains("merged=1", evidence.Value);
        Assert.Contains("open=1", evidence.Value);
        Assert.Contains("closedUnmerged=1", evidence.Value);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenExactResultDependsOnUnavailablePullRequests_ReturnsUnavailableEvidence(
        DataAvailabilityStatus status)
    {
        SignalResult result = Evaluate(CreateSnapshot(
            [CreatePullRequest(1, isClosed: true, isMerged: true)],
            isComplete: false,
            DataAvailability.Unavailable("pull-requests", status, "Collection failed.")));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenIncompleteCollectionHasTwoMergedPullRequests_PreservesTerminalStrong()
    {
        RepositoryPullRequest[] pullRequests =
        [
            CreatePullRequest(1, isClosed: true, isMerged: true),
            CreatePullRequest(2, isClosed: true, isMerged: true)
        ];
        SignalResult result = Evaluate(CreateSnapshot(
            pullRequests,
            isComplete: false,
            DataAvailability.Unavailable(
                "pull-requests",
                DataAvailabilityStatus.RateLimited,
                "Later page failed.")));

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(6, result.Score);
        Assert.Contains("complete=False", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_SmallSingleOwnerRepositoryWithNoPullRequests_IsMissing()
    {
        SignalResult result = EvaluateAvailable([]);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.NotEqual(SignalStatus.NotApplicable, result.Status);
    }

    [Fact]
    public void Evaluate_InputOrderDoesNotAffectResultAndTimeIsReadOnce()
    {
        RepositoryPullRequest[] pullRequests =
        [
            CreatePullRequest(1, isClosed: true, isMerged: false),
            CreatePullRequest(2, isClosed: false, isMerged: false),
            CreatePullRequest(3, isClosed: true, isMerged: true)
        ];
        RepositorySnapshot firstSnapshot = CreateSnapshot(
            pullRequests,
            isComplete: true,
            DataAvailability.Available("pull-requests", RetrievedAt));
        RepositorySnapshot secondSnapshot = CreateSnapshot(
            pullRequests.Reverse(),
            isComplete: true,
            DataAvailability.Available("pull-requests", RetrievedAt));
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new PullRequestUsageSignalEvaluator(firstTimeProvider).Evaluate(firstSnapshot);
        SignalResult second = new PullRequestUsageSignalEvaluator(secondTimeProvider).Evaluate(secondSnapshot);

        AssertEquivalent(first, second);
        Assert.Equal(1, firstTimeProvider.CallCount);
        Assert.Equal(1, secondTimeProvider.CallCount);
    }

    private static SignalResult EvaluateAvailable(IEnumerable<RepositoryPullRequest> pullRequests) =>
        Evaluate(CreateSnapshot(
            pullRequests,
            isComplete: true,
            DataAvailability.Available("pull-requests", RetrievedAt)));

    private static SignalResult Evaluate(RepositorySnapshot snapshot) =>
        new PullRequestUsageSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

    private static RepositoryPullRequest CreatePullRequest(
        int number,
        bool isClosed,
        bool isMerged) =>
        new(number, $"Pull request {number}", isClosed, isMerged, CapturedAt.AddDays(-number));

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<RepositoryPullRequest> pullRequests,
        bool isComplete,
        DataAvailability availability)
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
            new RepositoryCollection<RepositoryPullRequest>(pullRequests, isComplete: isComplete),
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            [availability],
            collectionWarnings: null);
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

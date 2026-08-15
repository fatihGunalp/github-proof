using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class IssueUsageSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt = CapturedAt.AddMinutes(1);
    private static readonly DateTimeOffset EvaluatedAt = CapturedAt.AddMinutes(2);

    [Fact]
    public void Definition_MatchesCanonicalIssueUsageContract()
    {
        SignalDefinition definition = new IssueUsageSignalEvaluator().Definition;

        Assert.Equal("process.issue-usage", definition.Key);
        Assert.Equal(AnalysisCategory.DevelopmentProcessVisibility, definition.Category);
        Assert.Equal(5, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["issues"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenIssuesAreDisabled_ReturnsNotApplicable()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            issues: [],
            issuesComplete: false,
            DataAvailability.Unavailable(
                "issues",
                DataAvailabilityStatus.NotApplicable,
                "GitHub Issues is disabled."),
            issuesEnabled: false);

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.NotApplicable, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Theory]
    [InlineData(0, SignalStatus.Missing, 0)]
    [InlineData(1, SignalStatus.Partial, 2)]
    [InlineData(2, SignalStatus.Present, 5)]
    public void Evaluate_WithCompleteAvailableIssues_AppliesExactThresholds(
        int issueCount,
        SignalStatus expectedStatus,
        int expectedScore)
    {
        SignalResult result = Evaluate(CreateSnapshot(
            CreateIssues(issueCount),
            issuesComplete: true,
            DataAvailability.Available("issues", RetrievedAt)));

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedScore, result.Score);
        Assert.Equal(expectedStatus is SignalStatus.Partial or SignalStatus.Missing,
            result.Recommendation is not null);
        Assert.Single(result.Limitations);
    }

    [Fact]
    public void Evaluate_CountsOpenAndClosedIssuesWithoutInspectingContentQuality()
    {
        RepositoryIssue[] issues =
        [
            new(1, "Short", body: null, isClosed: false, CapturedAt.AddDays(-2)),
            new(2, "Another", "Body", isClosed: true, CapturedAt.AddDays(-1))
        ];

        SignalResult result = Evaluate(CreateSnapshot(
            issues,
            issuesComplete: true,
            DataAvailability.Available("issues", RetrievedAt)));

        Assert.Equal(SignalStatus.Present, result.Status);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Contains("qualifying=2", evidence.Value);
        Assert.Contains("open=1", evidence.Value);
        Assert.Contains("closed=1", evidence.Value);
        Assert.Contains("complete=True", evidence.Value);
        Assert.Contains("availability=Available", evidence.Value);
    }

    [Fact]
    public void Evaluate_DoesNotCountPullRequestsAsIssues()
    {
        RepositoryPullRequest[] pullRequests =
        [new(10, "PR", isClosed: true, isMerged: true, CapturedAt)];
        RepositorySnapshot snapshot = CreateSnapshot(
            issues: [],
            issuesComplete: true,
            DataAvailability.Available("issues", RetrievedAt),
            pullRequests);

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.Contains("qualifying=0", Assert.Single(result.Evidence).Value);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenExactResultDependsOnUnavailableIssues_ReturnsUnavailableEvidence(
        DataAvailabilityStatus status)
    {
        SignalResult result = Evaluate(CreateSnapshot(
            CreateIssues(1),
            issuesComplete: false,
            DataAvailability.Unavailable("issues", status, "Collection failed.")));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenIncompleteCollectionAlreadyHasTwoIssues_PreservesTerminalPresent()
    {
        SignalResult result = Evaluate(CreateSnapshot(
            CreateIssues(2),
            issuesComplete: false,
            DataAvailability.Unavailable(
                "issues",
                DataAvailabilityStatus.RateLimited,
                "Later page failed.")));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(5, result.Score);
    }

    [Fact]
    public void Evaluate_SmallRepositoryWithNoIssues_IsMissingRatherThanNotApplicable()
    {
        SignalResult result = Evaluate(CreateSnapshot(
            issues: [],
            issuesComplete: true,
            DataAvailability.Available("issues", RetrievedAt)));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.NotEqual(SignalStatus.NotApplicable, result.Status);
    }

    [Fact]
    public void Evaluate_ReadsTimeProviderOnceAndProducesDeterministicState()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            CreateIssues(2).Reverse(),
            issuesComplete: true,
            DataAvailability.Available("issues", RetrievedAt));
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new IssueUsageSignalEvaluator(firstTimeProvider).Evaluate(snapshot);
        SignalResult second = new IssueUsageSignalEvaluator(secondTimeProvider).Evaluate(snapshot);

        AssertEquivalent(first, second);
        Assert.Equal(1, firstTimeProvider.CallCount);
        Assert.Equal(1, secondTimeProvider.CallCount);
    }

    private static SignalResult Evaluate(RepositorySnapshot snapshot) =>
        new IssueUsageSignalEvaluator(new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

    private static IReadOnlyList<RepositoryIssue> CreateIssues(int count) =>
        Enumerable.Range(1, count)
            .Select(number => new RepositoryIssue(
                number,
                $"Issue {number}",
                body: null,
                isClosed: number % 2 == 0,
                CapturedAt.AddDays(-number)))
            .ToArray();

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<RepositoryIssue> issues,
        bool issuesComplete,
        DataAvailability issuesAvailability,
        IEnumerable<RepositoryPullRequest>? pullRequests = null,
        bool issuesEnabled = true)
    {
        return SnapshotFactory.Create(
            issues: new RepositoryCollection<RepositoryIssue>(issues, isComplete: issuesComplete),
            pullRequests: new RepositoryCollection<RepositoryPullRequest>(pullRequests),
            issuesEnabled,
            availability:
            [
                DataAvailability.Available("metadata", RetrievedAt),
                issuesAvailability
            ]);
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

    private static class SnapshotFactory
    {
        public static RepositorySnapshot Create(
            RepositoryCollection<RepositoryIssue>? issues = null,
            RepositoryCollection<RepositoryPullRequest>? pullRequests = null,
            bool issuesEnabled = true,
            IEnumerable<DataAvailability>? availability = null)
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
                new RepositoryMetadata(null, null, CapturedAt.AddDays(-30), issuesEnabled),
                readme: null,
                fileTree: null,
                commits: null,
                issues,
                pullRequests,
                releases: null,
                workflows: null,
                topics: null,
                license: null,
                languages: null,
                contributors: null,
                availability,
                collectionWarnings: null);
        }
    }
}

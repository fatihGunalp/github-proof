using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class TopicsSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalTopicsContract()
    {
        SignalDefinition definition = new TopicsSignalEvaluator().Definition;

        Assert.Equal("portfolio.topics", definition.Key);
        Assert.Equal("GitHub Topics", definition.Name);
        Assert.Equal(AnalysisCategory.PortfolioPresentation, definition.Category);
        Assert.Equal(4, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["metadata"], definition.RequiredDataKeys);
    }

    [Theory]
    [InlineData(0, SignalStatus.Missing, 0)]
    [InlineData(1, SignalStatus.Present, 2)]
    [InlineData(2, SignalStatus.Present, 3)]
    [InlineData(3, SignalStatus.Strong, 4)]
    [InlineData(5, SignalStatus.Strong, 4)]
    public void Evaluate_MapsDistinctTopicCountToCanonicalResult(
        int topicCount,
        SignalStatus expectedStatus,
        int expectedScore)
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        TopicsSignalEvaluator evaluator = new(timeProvider);
        string[] topics = Enumerable.Range(1, topicCount)
            .Select(index => $"topic-{index}")
            .ToArray();

        SignalResult result = evaluator.Evaluate(CreateSnapshot(
            topics,
            DataAvailability.Available("metadata", RetrievedAt)));

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedScore, result.Score);
        Assert.Equal(EvaluatedAt, result.EvaluatedAt);
        Assert.Equal(1, timeProvider.CallCount);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("RepositoryTopics", evidence.Source);
        Assert.Equal(topicCount.ToString(System.Globalization.CultureInfo.InvariantCulture), evidence.Value);
        Assert.Equal("topics", evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);

        if (expectedStatus == SignalStatus.Missing)
        {
            Assert.NotNull(result.Recommendation);
            Assert.Empty(result.Limitations);
        }
        else
        {
            Assert.Null(result.Recommendation);
            Assert.Single(result.Limitations);
        }
    }

    [Fact]
    public void Evaluate_NormalizesCaseWhitespaceDuplicatesAndEvidenceOrder()
    {
        TopicsSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            [" DotNet ", "dotnet", " ZETA", "api", "API "],
            DataAvailability.Available("metadata", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(4, result.Score);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("3", evidence.Value);
        Assert.Contains("api, dotnet, zeta", evidence.Notes);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.NotApplicable)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenMetadataUnavailable_DoesNotInferMissing(
        DataAvailabilityStatus status)
    {
        TopicsSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult result = evaluator.Evaluate(CreateSnapshot(
            ["untrusted-topic"],
            DataAvailability.Unavailable("metadata", status, "Collection failed.")));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("MetadataAvailability", evidence.Source);
        Assert.Equal("unavailable", evidence.Value);
        Assert.Contains(status.ToString(), evidence.Notes);
    }

    [Fact]
    public void Evaluate_WhenMetadataAvailabilityMissing_ReturnsUnavailableEvidence()
    {
        TopicsSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult result = evaluator.Evaluate(CreateSnapshot(["topic"], availability: null));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("kaydedilmemiş", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_WithReorderedEquivalentTopics_ProducesEquivalentDomainState()
    {
        TopicsSignalEvaluator firstEvaluator = new(new CountingTimeProvider(EvaluatedAt));
        TopicsSignalEvaluator secondEvaluator = new(new CountingTimeProvider(EvaluatedAt));
        DataAvailability availability = DataAvailability.Available("metadata", RetrievedAt);

        SignalResult first = firstEvaluator.Evaluate(CreateSnapshot(
            ["Zeta", "alpha", "BETA", " beta "],
            availability));
        SignalResult second = secondEvaluator.Evaluate(CreateSnapshot(
            ["beta", "BETA", "alpha", "Zeta"],
            availability));

        AssertEquivalent(first, second);
    }

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<string> topics,
        DataAvailability? availability) =>
        new(
            CreateIdentity(),
            CapturedAt,
            headCommitSha: "abc123",
            metadata: new RepositoryMetadata(null, null, CapturedAt, issuesEnabled: true),
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics,
            license: null,
            languages: null,
            contributors: null,
            availability is null ? null : [availability],
            collectionWarnings: null);

    private static RepositoryIdentity CreateIdentity() =>
        new(
            "owner",
            "repository",
            new Uri("https://github.com/owner/repository"),
            "main",
            isPublic: true,
            isArchived: false,
            isFork: false,
            isTemplate: false,
            primaryLanguage: "C#");

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.SignalVersion, second.SignalVersion);
        Assert.Equal(first.Category, second.Category);
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

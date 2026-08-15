using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class ReadmeExistsSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalReadmeSignalContract()
    {
        SignalDefinition definition = new ReadmeExistsSignalEvaluator().Definition;

        Assert.Equal("repository.readme.exists", definition.Key);
        Assert.Equal(AnalysisCategory.RepositoryUnderstandability, definition.Category);
        Assert.Equal(1, definition.Version);
        Assert.Equal(5, definition.MaximumScore);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["file-tree", "readme"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenReadmeIsAvailableAndFound_ReturnsPresentWithObservableEvidence()
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        ReadmeExistsSignalEvaluator evaluator = new(timeProvider);
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryReadme("README.md", "# GitHub Proof"),
            DataAvailability.Available("readme", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(5, result.Score);
        Assert.Equal(5, result.MaximumScore);
        Assert.Equal(EvaluatedAt, result.EvaluatedAt);
        Assert.Equal(1, timeProvider.CallCount);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("repository.readme.exists", evidence.Key);
        Assert.Equal("README", evidence.Source);
        Assert.Equal("true", evidence.Value);
        Assert.Equal("README.md", evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);
        Assert.Equal(EvidenceConfidence.High, evidence.Confidence);
    }

    [Fact]
    public void Evaluate_WhenReadmeIsAvailableAndAbsent_ReturnsMissingWithInspectionEvidence()
    {
        ReadmeExistsSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            readme: null,
            DataAvailability.Available("readme", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("false", evidence.Value);
        Assert.Null(evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);
        Assert.Contains("başarıyla incelendi", evidence.Notes);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.NotApplicable)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenReadmeDataIsUnavailable_DoesNotInferMissing(
        DataAvailabilityStatus availabilityStatus)
    {
        ReadmeExistsSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            readme: null,
            DataAvailability.Unavailable(
                "readme",
                availabilityStatus,
                "Collection could not complete."));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("unavailable", evidence.Value);
        Assert.Equal(CapturedAt, evidence.ObservedAt);
        Assert.Contains(availabilityStatus.ToString(), evidence.Notes);
    }

    [Fact]
    public void Evaluate_WhenReadmeAvailabilityIsNotRecorded_ReturnsUnavailableEvidence()
    {
        ReadmeExistsSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult result = evaluator.Evaluate(CreateSnapshot(readme: null, availability: null));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Equal("unavailable", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentDomainState()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryReadme("docs/README.md", "# Docs"),
            DataAvailability.Available("readme", RetrievedAt));
        ReadmeExistsSignalEvaluator first = new(new CountingTimeProvider(EvaluatedAt));
        ReadmeExistsSignalEvaluator second = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult firstResult = first.Evaluate(snapshot);
        SignalResult secondResult = second.Evaluate(snapshot);

        Assert.Equal(firstResult.SignalKey, secondResult.SignalKey);
        Assert.Equal(firstResult.SignalVersion, secondResult.SignalVersion);
        Assert.Equal(firstResult.Category, secondResult.Category);
        Assert.Equal(firstResult.Status, secondResult.Status);
        Assert.Equal(firstResult.Score, secondResult.Score);
        Assert.Equal(firstResult.MaximumScore, secondResult.MaximumScore);
        Assert.Equal(firstResult.IncludeInOverallScore, secondResult.IncludeInOverallScore);
        Assert.Equal(firstResult.Evidence, secondResult.Evidence);
        Assert.Equal(firstResult.EvaluatedAt, secondResult.EvaluatedAt);
    }

    private static RepositorySnapshot CreateSnapshot(
        RepositoryReadme? readme,
        DataAvailability? availability)
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
            metadata: null,
            readme,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability is null ? null : [availability],
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

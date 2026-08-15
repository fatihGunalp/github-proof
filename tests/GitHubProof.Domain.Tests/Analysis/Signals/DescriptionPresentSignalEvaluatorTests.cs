using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class DescriptionPresentSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalDescriptionSignalContract()
    {
        SignalDefinition definition = new DescriptionPresentSignalEvaluator().Definition;

        Assert.Equal("repository.description.present", definition.Key);
        Assert.Equal(AnalysisCategory.RepositoryUnderstandability, definition.Category);
        Assert.Equal(4, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["metadata"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenMetadataHasDescription_ReturnsPresentWithCharacterCountEvidence()
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        DescriptionPresentSignalEvaluator evaluator = new(timeProvider);
        RepositorySnapshot snapshot = CreateSnapshot(
            "A concise repository description.",
            DataAvailability.Available("metadata", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Equal(EvaluatedAt, result.EvaluatedAt);
        Assert.Equal(1, timeProvider.CallCount);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("RepositoryMetadata", evidence.Source);
        Assert.Equal("33", evidence.Value);
        Assert.Equal("description", evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);
        Assert.Contains("33 karakterlik", evidence.Notes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Evaluate_WhenMetadataDescriptionIsAbsent_ReturnsMissingWithInspectionEvidence(
        string? description)
    {
        DescriptionPresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            description,
            DataAvailability.Available("metadata", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("false", evidence.Value);
        Assert.Equal("description", evidence.Location);
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
    public void Evaluate_WhenMetadataIsUnavailable_DoesNotInferMissing(
        DataAvailabilityStatus availabilityStatus)
    {
        DescriptionPresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            "Description should not be trusted.",
            DataAvailability.Unavailable(
                "metadata",
                availabilityStatus,
                "Collection could not complete."));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("MetadataAvailability", evidence.Source);
        Assert.Equal("unavailable", evidence.Value);
        Assert.Null(evidence.Location);
        Assert.Equal(CapturedAt, evidence.ObservedAt);
        Assert.Contains(availabilityStatus.ToString(), evidence.Notes);
    }

    [Fact]
    public void Evaluate_WhenMetadataAvailabilityIsNotRecorded_ReturnsUnavailableEvidence()
    {
        DescriptionPresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult result = evaluator.Evaluate(CreateSnapshot("Description", availability: null));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Equal("unavailable", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentDomainState()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "Description",
            DataAvailability.Available("metadata", RetrievedAt));

        SignalResult first = new DescriptionPresentSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
        SignalResult second = new DescriptionPresentSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        AssertEquivalent(first, second);
    }

    private static RepositorySnapshot CreateSnapshot(
        string? description,
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
            new RepositoryMetadata(description, null, CapturedAt, issuesEnabled: true),
            readme: null,
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

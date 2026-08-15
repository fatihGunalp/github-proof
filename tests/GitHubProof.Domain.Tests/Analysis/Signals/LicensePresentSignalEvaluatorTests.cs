using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class LicensePresentSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalLicenseSignalContract()
    {
        SignalDefinition definition = new LicensePresentSignalEvaluator().Definition;

        Assert.Equal("repository.license.present", definition.Key);
        Assert.Equal("Lisans", definition.Name);
        Assert.Equal(AnalysisCategory.RepositoryUnderstandability, definition.Category);
        Assert.Equal(3, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["license"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenRecognizedLicenseExists_ReturnsPresent()
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        LicensePresentSignalEvaluator evaluator = new(timeProvider);
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryLicense("mit", "MIT License", "MIT", "LICENSE"),
            licenseFile: null,
            DataAvailability.Available("license", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(3, result.Score);
        Assert.Null(result.Recommendation);
        Assert.Equal(EvaluatedAt, result.EvaluatedAt);
        Assert.Equal(1, timeProvider.CallCount);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("RepositoryLicense", evidence.Source);
        Assert.Equal("MIT", evidence.Value);
        Assert.Equal("LICENSE", evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);
        Assert.Single(result.Limitations);
        Assert.Contains("yasal uygunluğun", result.Limitations[0]);
        Assert.DoesNotContain("yasal olarak uygundur", result.Limitations[0]);
    }

    [Fact]
    public void Evaluate_WhenRecognizedLicenseAndFileExist_RecognizedLicenseTakesPrecedence()
    {
        LicensePresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryLicense("mit", "MIT License", "MIT", path: null),
            new RepositoryFile("LICENSE", false, 1080, "license text"),
            DataAvailability.Available("license", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(3, result.Score);
        Assert.Equal("RepositoryLicense", Assert.Single(result.Evidence).Source);
    }

    [Fact]
    public void Evaluate_WhenOnlyNormalizedLicenseFileExists_ReturnsPartial()
    {
        LicensePresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            license: null,
            new RepositoryFile("LICENSE.custom", false, 42, "custom license text"),
            DataAvailability.Available("license", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(1, result.Score);
        Assert.NotNull(result.Recommendation);
        Assert.DoesNotContain("dosyayı ekleyin", result.Recommendation);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("LicenseFile", evidence.Source);
        Assert.Equal("unrecognized-file", evidence.Value);
        Assert.Equal("LICENSE.custom", evidence.Location);
        Assert.Equal(RetrievedAt, evidence.ObservedAt);
        Assert.Contains("tanınmış lisans metadata'sı yok", evidence.Notes);
    }

    [Fact]
    public void Evaluate_WhenNoLicenseEvidenceExistsAfterSuccessfulInspection_ReturnsMissing()
    {
        LicensePresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            license: null,
            licenseFile: null,
            DataAvailability.Available("license", RetrievedAt));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.NotNull(result.Recommendation);
        Assert.Contains("açık kaynak lisansı ekleyin", result.Recommendation);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("License", evidence.Source);
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
    public void Evaluate_WhenLicenseDataIsUnavailable_DoesNotInferMissing(
        DataAvailabilityStatus availabilityStatus)
    {
        LicensePresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            license: null,
            licenseFile: null,
            DataAvailability.Unavailable(
                "license",
                availabilityStatus,
                "Collection could not complete."));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
        Assert.DoesNotContain("açık kaynak lisansı ekleyin", result.Explanation);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("LicenseAvailability", evidence.Source);
        Assert.Equal("unavailable", evidence.Value);
        Assert.Null(evidence.Location);
        Assert.Equal(CapturedAt, evidence.ObservedAt);
        Assert.Contains(availabilityStatus.ToString(), evidence.Notes);
    }

    [Fact]
    public void Evaluate_WhenUnavailableDataCarriesStaleLicenseEvidence_ReturnsUnavailableEvidence()
    {
        LicensePresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryLicense("mit", "MIT License", "MIT", "LICENSE"),
            new RepositoryFile("LICENSE", false, 1080, null),
            DataAvailability.Unavailable(
                "license",
                DataAvailabilityStatus.Failed,
                "Collection could not complete."));

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("LicenseAvailability", evidence.Source);
        Assert.Equal("unavailable", evidence.Value);
        Assert.Null(evidence.Location);
    }

    [Fact]
    public void Evaluate_WhenLicenseAvailabilityIsNotRecorded_ReturnsUnavailableEvidence()
    {
        LicensePresentSignalEvaluator evaluator = new(new CountingTimeProvider(EvaluatedAt));

        SignalResult result = evaluator.Evaluate(CreateSnapshot(
            license: null,
            licenseFile: null,
            availability: null));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
        Assert.Equal("unavailable", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentDomainState()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            license: null,
            new RepositoryFile("LICENSE.custom", false, 42, null),
            DataAvailability.Available("license", RetrievedAt));

        SignalResult first = new LicensePresentSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
        SignalResult second = new LicensePresentSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        AssertEquivalent(first, second);
    }

    private static RepositorySnapshot CreateSnapshot(
        RepositoryLicense? license,
        RepositoryFile? licenseFile,
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
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license,
            languages: null,
            contributors: null,
            availability is null ? null : [availability],
            collectionWarnings: null,
            licenseFile: licenseFile);
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

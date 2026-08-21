using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I1: the profile counterpart of LiveRepositoryReportServiceTests. These
// tests prove the typed request boundary from a raw profile identity string,
// through the real (pre-existing, unmodified) IGitHubProfileSnapshotCollector
// contract and ProfilePipeline, to a ProfileReport or a typed failure —
// without the Web layer parsing profile identity or inventing any new
// profile-analysis truth.
public sealed class LiveProfileReportServiceTests
{
    private const string ProfileIdentity = "octocat";

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-20T00:10:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public async Task AnalyzeAsync_WithAvailableProfile_ProducesComposedReportThroughTheRealPipeline()
    {
        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        LiveProfileReportService service =
            CreateService(collector);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(
                ProfileIdentity,
                Version);

        Assert.True(result.IsSuccessful);
        Assert.True(result.ProfileAvailability.IsAvailable);
        Assert.Equal(1, collector.CallCount);
        Assert.Equal(ProfileIdentity, collector.LastProfileIdentity);

        ProfilePipelineResult pipelineResult = result.PipelineResult!;

        Assert.True(pipelineResult.HasProfile);

        ProfileReport report = Assert.IsType<ProfileReport>(pipelineResult.Report);

        Assert.Equal(9, report.Sections.Count);
        Assert.Equal(
            ProfileReportSectionAvailability.Supported,
            report.PublicProfileSummary.Availability);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenProfileIsUnavailable_PreservesTypedFailureAndSkipsThePipeline()
    {
        DataAvailability unavailable =
            DataAvailability.Unavailable(
                "profile",
                DataAvailabilityStatus.Unavailable,
                "GitHub profile was not found.");

        FakeProfileSnapshotCollector collector =
            new(new GitHubProfileSnapshotResult(
                snapshot: null,
                unavailable));

        LiveProfileReportService service =
            CreateService(collector);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(
                ProfileIdentity,
                Version);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.PipelineResult);
        Assert.Same(unavailable, result.ProfileAvailability);
        Assert.Equal(1, collector.CallCount);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenProfileIsRateLimited_PreservesRateLimitedStatusRatherThanACollapsedNegativeClaim()
    {
        DataAvailability rateLimited =
            DataAvailability.Unavailable(
                "profile",
                DataAvailabilityStatus.RateLimited,
                "GitHub's API rate limit was reached.");

        FakeProfileSnapshotCollector collector =
            new(new GitHubProfileSnapshotResult(
                snapshot: null,
                rateLimited));

        LiveProfileReportService service =
            CreateService(collector);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(
                ProfileIdentity,
                Version);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            DataAvailabilityStatus.RateLimited,
            result.ProfileAvailability.Status);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenProfileIdentityIsInvalid_PropagatesTheCollectorsOwnValidationWithoutReimplementingIt()
    {
        // This collector reproduces exactly what the real
        // GitHubProfileSnapshotOrchestrator does before any HTTP call:
        // GitHubProfileHandle.Parse(profile). LiveProfileReportService itself
        // must add no parsing/validation of its own — it just forwards the
        // raw string — so the exact same ArgumentException the Infrastructure
        // parser throws propagates unaltered, and the pipeline is never
        // reached.
        ParsingProfileSnapshotCollector collector = new();

        LiveProfileReportService service =
            CreateService(collector);

        ArgumentException exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.AnalyzeAsync(
                    "   ",
                    Version));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void LiveProfileAnalysisResult_PreservesAFailedPipelineResultWithoutReinterpretingIt()
    {
        // A Failed ProfilePipelineResult (ProfileComposer could not prove
        // same-run coherence) must be carried through unchanged: still no
        // Report, still Failed status — the Web wrapper adds no report and
        // invents no success.
        ProfilePipelineResult failed =
            ProfilePipelineResult.Failed(ProfileCompositionResult.Failed());

        LiveProfileAnalysisResult result =
            new(
                failed,
                DataAvailability.Available("profile", CapturedAt));

        Assert.True(result.IsSuccessful);
        Assert.Same(failed, result.PipelineResult);
        Assert.False(result.PipelineResult!.HasProfile);
        Assert.Null(result.PipelineResult.Report);
    }

    [Fact]
    public void LiveProfileAnalysisResult_RejectsAPipelineResultPairedWithUnavailableMetadata()
    {
        ProfilePipelineResult composed =
            ProfilePipelineResult.Failed(ProfileCompositionResult.Failed());

        Assert.Throws<ArgumentException>(() =>
            new LiveProfileAnalysisResult(
                composed,
                DataAvailability.Unavailable(
                    "profile",
                    DataAvailabilityStatus.Unavailable,
                    "unused")));
    }

    [Fact]
    public void LiveProfileAnalysisResult_RejectsAvailableMetadataWithNoPipelineResult()
    {
        Assert.Throws<ArgumentException>(() =>
            new LiveProfileAnalysisResult(
                pipelineResult: null,
                DataAvailability.Available("profile", CapturedAt)));
    }

    private static LiveProfileReportService CreateService(
        IGitHubProfileSnapshotCollector collector) =>
        new(collector, CreateRealPipeline());

    private static ProfilePipeline CreateRealPipeline() =>
        new(
            new ProfileRepositoryAnalysisOrchestrator(
                new NeverCalledSnapshotSource(),
                new NeverCalledAnalysisRunner(),
                new AnalysisAdmissionPolicy(EligibilityPolicy.V1)),
            new RepositoryEligibilityPolicy(EligibilityPolicy.V1),
            new CrossRepositoryEvidenceComposer(
                new RepositoryContributionPolicy(EligibilityPolicy.V1.Version)),
            new ProfilePatternPromotionPolicy(PatternRuleVersion.V1),
            new RepresentativeRepositorySelectionPolicy(RepresentativeRepositoryRuleVersion.V1),
            new ProfileComposer(),
            new ProfileReportProjector(),
            ObservationRuleVersion.V1,
            ProfileAnalyzerVersion.V1);

    private static GitHubProfileSnapshotResult CreateSuccessfulSnapshotResult()
    {
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                ProfileIdentity,
                new Uri($"https://github.com/{ProfileIdentity}")),
            CapturedAt,
            metadata: null,
            repositories: null,
            availability: null,
            collectionWarnings: null);

        return new GitHubProfileSnapshotResult(
            snapshot,
            DataAvailability.Available("profile", CapturedAt));
    }

    private sealed class FakeProfileSnapshotCollector(
        GitHubProfileSnapshotResult result)
        : IGitHubProfileSnapshotCollector
    {
        public int CallCount { get; private set; }

        public string? LastProfileIdentity { get; private set; }

        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CallCount++;
            LastProfileIdentity = profile;

            return Task.FromResult(result);
        }
    }

    // Mirrors GitHubProfileSnapshotOrchestrator.CollectAsync's own first
    // step, so the "no reimplemented parsing" test exercises the real
    // validation rule rather than a Web-invented substitute.
    private sealed class ParsingProfileSnapshotCollector : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default)
        {
            GitHubProfileHandle.Parse(profile);

            throw new InvalidOperationException(
                "This collector must fail parsing before returning any result.");
        }
    }

    private sealed class NeverCalledSnapshotSource : IProfileRepositorySnapshotSource
    {
        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "No profile repository exists in these tests' zero-repository "
                + "snapshots, so this source must never be invoked.");
    }

    private sealed class NeverCalledAnalysisRunner : IRepositoryAnalysisRunner
    {
        public RepositoryAnalysis Analyze(
            RepositorySnapshot snapshot,
            AnalysisVersion version,
            Guid analysisId) =>
            throw new InvalidOperationException(
                "No repository is ever admitted in these tests' zero-repository "
                + "snapshots, so this runner must never be invoked.");
    }
}

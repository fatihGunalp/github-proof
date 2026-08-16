using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Analysis;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class LiveRepositoryReportServiceTests
{
    private const string RepositoryUrl =
        "https://github.com/owner/repository";

    private static readonly Guid AnalysisId =
        Guid.Parse("04800000-0000-0000-0000-000000000002");

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-16T00:10:00Z");

    private static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-16T00:15:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public async Task AnalyzeAsync_WithAvailableRepository_ProducesCanonicalAnalysis()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        LiveRepositoryReportService service =
            CreateService(collector);

        LiveRepositoryAnalysisResult result =
            await service.AnalyzeAsync(
                $"  {RepositoryUrl}  ",
                Version,
                AnalysisId);

        RepositoryAnalysis analysis =
            Assert.IsType<RepositoryAnalysis>(result.Analysis);

        Assert.True(result.IsSuccessful);
        Assert.True(result.MetadataAvailability.IsAvailable);
        Assert.Equal(1, collector.CallCount);
        Assert.Equal(RepositoryUrl, collector.LastRepositoryUrl);
        Assert.Equal(AnalysisId, analysis.AnalysisId);
        Assert.Equal(Version, analysis.Version);
        Assert.Equal(15, analysis.SignalResults.Count);
        Assert.Equal(4, analysis.CategoryResults.Count);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenRepositoryMetadataIsUnavailable_PreservesFailure()
    {
        DataAvailability unavailable =
            DataAvailability.Unavailable(
                "metadata",
                DataAvailabilityStatus.Unavailable,
                "Repository metadata unavailable.");

        FakeSnapshotCollector collector =
            new(new GitHubRepositorySnapshotResult(
                snapshot: null,
                unavailable));

        LiveRepositoryReportService service =
            CreateService(collector);

        LiveRepositoryAnalysisResult result =
            await service.AnalyzeAsync(
                RepositoryUrl,
                Version,
                AnalysisId);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.Analysis);
        Assert.Same(unavailable, result.MetadataAvailability);
        Assert.Equal(1, collector.CallCount);
    }

    [Fact]
    public async Task Project_WithTwoLocales_DoesNotRerunCollectionOrAnalysis()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        LiveRepositoryReportService service =
            CreateService(collector);

        LiveRepositoryAnalysisResult result =
            await service.AnalyzeAsync(
                RepositoryUrl,
                Version,
                AnalysisId);

        RepositoryAnalysis analysis =
            Assert.IsType<RepositoryAnalysis>(result.Analysis);

        RepositoryReport english =
            service.Project(analysis, "en");

        RepositoryReport turkish =
            service.Project(analysis, "tr");

        Assert.Equal(1, collector.CallCount);

        Assert.Equal("en", english.Locale);
        Assert.Equal("tr", turkish.Locale);

        Assert.Equal(
            english.Identity.AnalysisId,
            turkish.Identity.AnalysisId);

        Assert.Equal(
            english.Identity.FullName,
            turkish.Identity.FullName);

        Assert.Equal(
            english.SignalDetails.Items.Select(item => item.SignalKey),
            turkish.SignalDetails.Items.Select(item => item.SignalKey));

        Assert.Equal(
            english.SignalDetails.Items.Select(item => item.Status),
            turkish.SignalDetails.Items.Select(item => item.Status));

        Assert.Equal(
            english.SignalDetails.Items.Select(item => item.Score),
            turkish.SignalDetails.Items.Select(item => item.Score));

        Assert.NotEqual(
            english.Identity.SectionTitle,
            turkish.Identity.SectionTitle);
    }

    [Fact]
    public void AnalyzeAsync_HasNoLocaleParameter()
    {
        System.Reflection.MethodInfo method =
            typeof(LiveRepositoryReportService)
                .GetMethod(
                    nameof(LiveRepositoryReportService.AnalyzeAsync))!;

        Assert.DoesNotContain(
            method.GetParameters(),
            parameter =>
                parameter.Name!.Contains(
                    "locale",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenRepositoryUrlIsNotCanonical_FailsBeforeCollection()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        LiveRepositoryReportService service =
            CreateService(collector);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnalyzeAsync(
                "https://example.com/owner/repository",
                Version,
                AnalysisId));

        Assert.Equal(0, collector.CallCount);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenRepositoryUrlIsWhitespace_FailsBeforeCollection()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        LiveRepositoryReportService service =
            CreateService(collector);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnalyzeAsync(
                "   ",
                Version,
                AnalysisId));

        Assert.Equal(0, collector.CallCount);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenAnalysisIdIsEmpty_FailsBeforeCollection()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        LiveRepositoryReportService service =
            CreateService(collector);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnalyzeAsync(
                RepositoryUrl,
                Version,
                Guid.Empty));

        Assert.Equal(0, collector.CallCount);
    }

    private static LiveRepositoryReportService CreateService(
        IGitHubRepositorySnapshotCollector collector) =>
        new(
            collector,
            new RepositoryAnalysisRunner(
                new FixedTimeProvider(EvaluationTime)),
            new RepositoryReportProjector());

    private static GitHubRepositorySnapshotResult
        CreateSuccessfulSnapshotResult()
    {
        RepositorySnapshot snapshot =
            new(
                new RepositoryIdentity(
                    "owner",
                    "repository",
                    new Uri(RepositoryUrl),
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
                license: null,
                languages: null,
                contributors: null,
                availability: null,
                collectionWarnings: null);

        return new GitHubRepositorySnapshotResult(
            snapshot,
            DataAvailability.Available(
                "metadata",
                CapturedAt));
    }

    private sealed class FakeSnapshotCollector(
        GitHubRepositorySnapshotResult result)
        : IGitHubRepositorySnapshotCollector
    {
        public int CallCount { get; private set; }

        public string? LastRepositoryUrl { get; private set; }

        public Task<GitHubRepositorySnapshotResult> CollectAsync(
            string repositoryUrl,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CallCount++;
            LastRepositoryUrl = repositoryUrl;

            return Task.FromResult(result);
        }
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

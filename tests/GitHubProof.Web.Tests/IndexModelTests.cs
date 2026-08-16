using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Analysis;
using GitHubProof.Web.Pages;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class IndexModelTests
{
    private const string RepositoryUrl =
        "https://github.com/owner/repository";

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-16T00:20:00Z");

    private static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-16T00:25:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public async Task OnPostAsync_WithAvailableRepository_RendersTypedReport()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        IndexModel model =
            CreateModel(collector);

        model.RepositoryUrl = RepositoryUrl;

        object action =
            await model.OnPostAsync(
                CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.NotNull(model.Report);
        Assert.Null(model.ErrorMessage);
        Assert.Equal(1, collector.CallCount);
        Assert.Equal(
            "owner/repository",
            model.Report!.Identity.FullName);
        Assert.Equal(
            15,
            model.Report.SignalDetails.Items.Count);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidRepositoryUrl_ShowsInputErrorWithoutCollection()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        IndexModel model =
            CreateModel(collector);

        model.RepositoryUrl =
            "https://example.com/owner/repository";

        object action =
            await model.OnPostAsync(
                CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Null(model.Report);
        Assert.NotNull(model.ErrorMessage);
        Assert.Contains(
            "canonical public GitHub repository URL",
            model.ErrorMessage!,
            StringComparison.Ordinal);
        Assert.Equal(0, collector.CallCount);
    }

    [Fact]
    public async Task OnPostAsync_WithUnavailableRepository_ShowsBoundedFailure()
    {
        DataAvailability unavailable =
            DataAvailability.Unavailable(
                "metadata",
                DataAvailabilityStatus.Unavailable,
                "Repository was not found.");

        FakeSnapshotCollector collector =
            new(
                new GitHubRepositorySnapshotResult(
                    snapshot: null,
                    unavailable));

        IndexModel model =
            CreateModel(collector);

        model.RepositoryUrl = RepositoryUrl;

        object action =
            await model.OnPostAsync(
                CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Null(model.Report);
        Assert.NotNull(model.ErrorMessage);
        Assert.DoesNotContain(
            unavailable.Reason!,
            model.ErrorMessage!,
            StringComparison.Ordinal);
        Assert.Equal(1, collector.CallCount);
    }

    [Fact]
    public async Task OnPostAsync_WhenRateLimited_ExplainsOptionalTokenPath()
    {
        DataAvailability unavailable =
            DataAvailability.Unavailable(
                "metadata",
                DataAvailabilityStatus.RateLimited,
                "GitHub API rate limit.");

        FakeSnapshotCollector collector =
            new(
                new GitHubRepositorySnapshotResult(
                    snapshot: null,
                    unavailable));

        IndexModel model =
            CreateModel(collector);

        model.RepositoryUrl = RepositoryUrl;

        object action =
            await model.OnPostAsync(
                CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Null(model.Report);
        Assert.Contains(
            "GITHUB_TOKEN",
            model.ErrorMessage!,
            StringComparison.Ordinal);
        Assert.Equal(1, collector.CallCount);
    }

    private static IndexModel CreateModel(
        IGitHubRepositorySnapshotCollector collector)
    {
        LiveRepositoryReportService service =
            new(
                collector,
                new RepositoryAnalysisRunner(
                    new FixedTimeProvider(
                        EvaluationTime)),
                new RepositoryReportProjector());

        return new IndexModel(
            service,
            Version);
    }

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

        public Task<GitHubRepositorySnapshotResult> CollectAsync(
            string repositoryUrl,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;

            return Task.FromResult(result);
        }
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            utcNow;
    }
}

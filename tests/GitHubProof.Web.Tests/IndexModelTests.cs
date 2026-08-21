using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Analysis;
using GitHubProof.Web.Localization;
using GitHubProof.Web.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
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
    public async Task OnPostAsync_WithCanonicalRepositoryUrl_RunsRepositoryProofUnchanged()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        IndexModel model =
            CreateModel(collector);

        model.GitHubTarget = RepositoryUrl;

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
    public async Task OnPostAsync_Projects_existing_repository_analysis_in_resolved_request_culture()
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());
        IndexModel model = CreateModel(collector);
        DefaultHttpContext context = new();
        context.Features.Set<IRequestCultureFeature>(
            new RequestCultureFeature(
                new RequestCulture(WebCulture.Turkish),
                provider: null));
        model.PageContext = new PageContext { HttpContext = context };
        model.GitHubTarget = RepositoryUrl;

        await model.OnPostAsync(CancellationToken.None);

        Assert.Equal(1, collector.CallCount);
        Assert.NotNull(model.Report);
        Assert.Equal(WebCulture.Turkish, model.Report!.Locale);
        Assert.Contains(
            "Önce",
            model.Report.DoFirst.Title,
            StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("octocat")]
    [InlineData("https://github.com/octocat")]
    public async Task OnPostAsync_WithProfileShape_RedirectsToProfileProofPreservingPost(
        string input)
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        IndexModel model =
            CreateModel(collector);

        model.GitHubTarget = input;

        object action =
            await model.OnPostAsync(
                CancellationToken.None);

        RedirectToPageResult redirect =
            Assert.IsType<RedirectToPageResult>(action);

        Assert.Equal("/Profile", redirect.PageName);

        // Preserve-method (307): the identity travels in the re-posted form
        // body, and profile analysis therefore stays a POST. A GET of
        // /profile must never be able to start a live collection.
        Assert.True(redirect.PreserveMethod);
        Assert.DoesNotContain(
            redirect.RouteValues ?? new Microsoft.AspNetCore.Routing.RouteValueDictionary(),
            entry => entry.Value is string value && value == input);

        // The two semantics stay separate: the repository collector is never
        // touched for a profile-shaped entry.
        Assert.Equal(0, collector.CallCount);
        Assert.Null(model.Report);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://github.com/owner/repository/tree/main")]
    // Non-canonical shapes must stay on the unified entry rather than being
    // pushed into either flow.
    [InlineData("owner/repo")]
    [InlineData("github.com/owner/repo")]
    [InlineData("github.com/octocat")]
    [InlineData("https://example.com/owner/repository")]
    [InlineData("https://example.com/octocat")]
    [InlineData("https://github.com/owner/repository?tab=readme")]
    [InlineData("not a login")]
    public async Task OnPostAsync_WithUnresolvableInput_StaysOnEntryWithValidationErrorAndDoesNotGuess(
        string input)
    {
        FakeSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        IndexModel model =
            CreateModel(collector);

        model.GitHubTarget = input;

        object action =
            await model.OnPostAsync(
                CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Null(model.Report);
        Assert.Equal(0, collector.CallCount);

        // One entry-level message listing the accepted shapes — not a
        // flow-specific error the user cannot act on.
        Assert.NotNull(model.ErrorMessage);
        Assert.Contains("username", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("profile URL", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("repository URL", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);

        // The submitted value is kept so the user can correct it in place.
        Assert.Equal(input, model.GitHubTarget);
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

        model.GitHubTarget = RepositoryUrl;

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

        model.GitHubTarget = RepositoryUrl;

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
            Version,
            new WebPresentationLocalizer());
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

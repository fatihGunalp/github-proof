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
using GitHubProof.Web.Localization;
using GitHubProof.Web.Pages;
using GitHubProof.Web.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I2 Profile Proof route boundary. These tests prove the route reaches the
// existing P10-I1 LiveProfileReportService and preserves its typed outcomes;
// they intentionally assert nothing about report-section presentation, which
// is P10-I3..I5.
public sealed class ProfileModelTests
{
    private const string Identity = "octocat";

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-20T00:10:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public void OnGet_PerformsNoLiveProfileAnalysisAtAll()
    {
        // The correction that matters: profile analysis fans out across the
        // account's repositories and consumes rate limit, so a refresh, a
        // browser back/forward, a prefetch, a crawler or a link-preview fetch
        // of /profile must never start one. GET carries no analysis path.
        ThrowingProfileSnapshotCollector collector = new();

        ProfileModel model = CreateModel(collector);

        // Even with an identity already bound, GET must not analyze.
        model.GitHubTarget = Identity;

        object action = model.OnGet();

        RedirectToPageResult redirect =
            Assert.IsType<RedirectToPageResult>(action);

        Assert.Equal("/Index", redirect.PageName);
        Assert.Equal(0, collector.CallCount);
        Assert.False(model.HasComposedProfile);
        Assert.Null(model.Identity);
    }

    [Fact]
    public void OnGet_HasNoAsynchronousAnalysisOverload()
    {
        // Structural guard: if a GET handler that can await the profile
        // service is ever reintroduced, this fails rather than silently
        // restoring the rate-limit exposure.
        System.Reflection.MethodInfo[] getHandlers = typeof(ProfileModel)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(method => method.Name.StartsWith("OnGet", StringComparison.Ordinal))
            .ToArray();

        System.Reflection.MethodInfo handler = Assert.Single(getHandlers);

        Assert.Empty(handler.GetParameters());
        Assert.Equal(typeof(IActionResult), handler.ReturnType);
    }

    [Fact]
    public async Task OnPostAsync_WithAvailableProfile_ReachesLiveProfileReportService()
    {
        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = Identity;

        object action = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Equal(1, collector.CallCount);
        Assert.Equal(Identity, collector.LastProfileIdentity);
        Assert.Equal(Identity, model.Identity);
        Assert.True(model.HasComposedProfile);
        Assert.Null(model.ErrorMessage);
        Assert.NotNull(model.Report);
    }

    [Fact]
    public async Task OnPostAsync_WithAvailableProfile_ExposesTheExactTypedProfileReport()
    {
        // P10-I3: ProfileModel must expose the already-projected typed
        // ProfileReport itself — not a re-derived value, not raw profile
        // evidence. Asserting the canonical nine-section shape (report
        // contract §5/§7) is the strongest available proof of that without
        // reaching into ProfilePipelineResult's internal construction path.
        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = Identity;

        await model.OnPostAsync(CancellationToken.None);

        ProfileReport report = model.Report
            ?? throw new InvalidOperationException(
                "Expected a composed profile to expose a typed report.");

        Assert.Equal(9, report.Sections.Count);
        Assert.Equal(
            ProfileReportSectionKey.PublicProfileSummary,
            report.Sections[0].SectionKey);
        Assert.Equal(
            ProfileReportSectionKey.TechnicalFocusAreas,
            report.Sections[1].SectionKey);
        Assert.Equal(
            ProfileReportSectionKey.EngineeringPracticePatterns,
            report.Sections[2].SectionKey);
        Assert.Equal(
            ProfileReportSectionKey.RepresentativeRepositories,
            report.Sections[4].SectionKey);
        Assert.Equal(8, report.PublicProfileSummary.Items.Count);
    }

    [Fact]
    public async Task ProfilePipelineResult_Composed_CannotCarryANullReport()
    {
        // The state ProfileModel defensively guards against with
        // InvalidOperationException — HasProfile == true with Report == null
        // — cannot be produced through the pipeline's own construction path:
        // ProfilePipelineResult.Composed (internal, invoked only by
        // ProfilePipeline from the same run that produced the composition) is
        // guarded by ArgumentNullException.ThrowIfNull(report). There is no
        // InternalsVisibleTo from Application to this test assembly and no
        // public seam that could hand ProfileModel a composed-but-reportless
        // result, so that branch is provably unreachable in production; this
        // test proves the invariant at its one source of truth via reflection
        // instead of altering Application internals to make it reachable.
        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());
        LiveProfileReportService service =
            new(collector, CreateRealPipeline());

        LiveProfileAnalysisResult result = await service.AnalyzeAsync(
            Identity,
            Version,
            CancellationToken.None);

        ProfilePipelineResult pipelineResult = result.PipelineResult
            ?? throw new InvalidOperationException(
                "Expected a successful result to carry a pipeline result.");

        Assert.True(pipelineResult.HasProfile);

        System.Reflection.MethodInfo composedFactory = typeof(ProfilePipelineResult)
            .GetMethod(
                "Composed",
                System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "ProfilePipelineResult.Composed was not found by reflection.");

        System.Reflection.TargetInvocationException thrown =
            Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                composedFactory.Invoke(
                    null,
                    [pipelineResult.Composition, null]));

        Assert.IsType<ArgumentNullException>(thrown.InnerException);
    }

    [Fact]
    public async Task OnPostAsync_WithCanonicalProfileUrl_PassesItThroughUnparsed()
    {
        // The Web layer must not derive its own identity: the raw value
        // reaches the collector, whose own GitHubProfileHandle.Parse owns it.
        const string profileUrl = "https://github.com/octocat";

        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());

        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = profileUrl;

        await model.OnPostAsync(CancellationToken.None);

        Assert.Equal(profileUrl, collector.LastProfileIdentity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://github.com/owner/repository")]
    [InlineData("owner/repo")]
    [InlineData("github.com/owner/repo")]
    [InlineData("not a login")]
    public async Task OnPostAsync_WithNonProfileShape_RedirectsToEntryWithoutAnalyzing(string target)
    {
        // A direct POST here must not be able to push a repository-shaped or
        // non-canonical value into the profile flow.
        ThrowingProfileSnapshotCollector collector = new();

        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = target;

        object action = await model.OnPostAsync(CancellationToken.None);

        RedirectToPageResult redirect =
            Assert.IsType<RedirectToPageResult>(action);

        Assert.Equal("/Index", redirect.PageName);
        Assert.Equal(0, collector.CallCount);
        Assert.False(model.HasComposedProfile);
    }

    [Fact]
    public async Task OnPostAsync_WhenRateLimited_PreservesRateLimitedRatherThanANegativeClaim()
    {
        FakeProfileSnapshotCollector collector =
            new(new GitHubProfileSnapshotResult(
                snapshot: null,
                DataAvailability.Unavailable(
                    "profile",
                    DataAvailabilityStatus.RateLimited,
                    "GitHub API rate limit.")));

        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = Identity;

        await model.OnPostAsync(CancellationToken.None);

        Assert.False(model.HasComposedProfile);
        Assert.Contains(
            "GITHUB_TOKEN",
            model.ErrorMessage!,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnPostAsync_WhenRateLimited_Uses_resolved_Turkish_Web_copy()
    {
        FakeProfileSnapshotCollector collector =
            new(new GitHubProfileSnapshotResult(
                snapshot: null,
                DataAvailability.Unavailable(
                    "profile",
                    DataAvailabilityStatus.RateLimited,
                    "GitHub API rate limit.")));
        ProfileModel model = CreateModel(collector);
        DefaultHttpContext context = new();
        context.Features.Set<IRequestCultureFeature>(
            new RequestCultureFeature(
                new RequestCulture(WebCulture.Turkish),
                provider: null));
        model.PageContext = new PageContext { HttpContext = context };
        model.GitHubTarget = Identity;

        await model.OnPostAsync(CancellationToken.None);

        Assert.Contains("hız sınırına", model.ErrorMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnPostAsync_WhenProfileUnavailable_ShowsBoundedFailure()
    {
        DataAvailability unavailable =
            DataAvailability.Unavailable(
                "profile",
                DataAvailabilityStatus.Unavailable,
                "Profile was not found.");

        FakeProfileSnapshotCollector collector =
            new(new GitHubProfileSnapshotResult(
                snapshot: null,
                unavailable));

        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = Identity;

        await model.OnPostAsync(CancellationToken.None);

        Assert.False(model.HasComposedProfile);
        Assert.NotNull(model.ErrorMessage);
        Assert.DoesNotContain(
            unavailable.Reason!,
            model.ErrorMessage!,
            StringComparison.Ordinal);
    }

    private static ProfileModel CreateModel(
        IGitHubProfileSnapshotCollector collector) =>
        new(
            new LiveProfileReportService(collector, CreateRealPipeline()),
            Version,
            new WebPresentationLocalizer());

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
            new Application.Profile.Report.ProfileReportProjector(),
            ObservationRuleVersion.V1,
            ProfileAnalyzerVersion.V1);

    private static GitHubProfileSnapshotResult CreateSuccessfulSnapshotResult()
    {
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                Identity,
                new Uri($"https://github.com/{Identity}")),
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

    // Fails loudly if any code path reaches live collection when it must not.
    private sealed class ThrowingProfileSnapshotCollector : IGitHubProfileSnapshotCollector
    {
        public int CallCount => 0;

        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "Live profile collection must not be reachable from this path.");
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

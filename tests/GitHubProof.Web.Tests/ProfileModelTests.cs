using System.Net;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Localization;
using GitHubProof.Web.Pages;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
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
        Assert.False(model.IsPartialProfile);
        Assert.Null(model.ErrorTitle);
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
        Assert.Equal(
            "Profile evidence is unavailable",
            model.ErrorTitle);
        Assert.Contains("temporarily limited", model.ErrorMessage!, StringComparison.Ordinal);
        Assert.DoesNotContain("GITHUB_TOKEN", model.ErrorMessage!, StringComparison.Ordinal);
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

        Assert.Equal("Profil kanıtına erişilemiyor", model.ErrorTitle);
        Assert.Contains("geçici olarak sınırladı", model.ErrorMessage!, StringComparison.Ordinal);
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
        Assert.Equal("Profile evidence is unavailable", model.ErrorTitle);
        Assert.NotNull(model.ErrorMessage);
        Assert.DoesNotContain(
            unavailable.Reason!,
            model.ErrorMessage!,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnPostAsync_WhenExecutionDeadlineExpires_ReturnsGatewayTimeoutWithoutEvidence()
    {
        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult());
        ProfileModel model = new(
            new LiveProfileReportService(
                collector,
                CreateRealPipeline(),
                deadlineFactory: new ExpiredDeadlineFactory()),
            Version,
            new WebPresentationLocalizer(),
            DisabledProfileExecutionAdmissionGuard.Instance)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            },
            GitHubTarget = Identity
        };

        object action = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, model.Response.StatusCode);
        Assert.False(model.HasComposedProfile);
        Assert.Equal("Profile analysis took too long", model.ErrorTitle);
        Assert.Null(model.Report);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenDeadlineExpiresAfterAvailability_DoesNotRenderPartialOrAReport()
    {
        DeadlineAfterAvailabilityFactory deadlineFactory = new();
        DataAvailability profileAvailability =
            DataAvailability.Available("profile", CapturedAt);
        ProfileModel model = new(
            new LiveProfileReportService(
                new AvailabilityThenDeadlineCollector(
                    profileAvailability,
                    deadlineFactory),
                CreateRealPipeline(),
                deadlineFactory: deadlineFactory),
            Version,
            new WebPresentationLocalizer(),
            DisabledProfileExecutionAdmissionGuard.Instance)
        {
            PageContext = CreatePageContext(),
            GitHubTarget = Identity
        };

        await model.OnPostAsync(CancellationToken.None);

        Assert.Equal(StatusCodes.Status504GatewayTimeout, model.Response.StatusCode);
        Assert.False(model.HasComposedProfile);
        Assert.False(model.IsPartialProfile);
        Assert.Null(model.Report);
        Assert.Equal("Profile analysis took too long", model.ErrorTitle);
    }

    [Fact]
    public async Task OnPostAsync_WhenSameNormalizedProfileIsActive_ReturnsConflictWithoutAnalysis()
    {
        ProfileExecutionAdmissionGuard guard = CreateAdmissionGuard(
            new ProfileAdmissionOptions
            {
                GlobalConcurrency = 1,
                SameProfileConcurrency = 1
            });
        using ProfileAdmissionLease active = guard.TryAcquire("octocat", null);
        ThrowingProfileSnapshotCollector collector = new();
        ProfileModel model = CreateModel(collector, guard);
        model.PageContext = CreatePageContext();
        model.GitHubTarget = "https://github.com/OCTOCAT";

        object action = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Equal(StatusCodes.Status409Conflict, model.Response.StatusCode);
        Assert.Equal(0, collector.CallCount);
        Assert.False(model.HasComposedProfile);
        Assert.Null(model.Report);
        Assert.Equal("This profile is already being analyzed", model.ErrorTitle);
        Assert.Contains("still in progress", model.ErrorMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnPostAsync_WhenGlobalProfileCapacityIsFull_ReturnsServiceUnavailableWithoutAnalysis()
    {
        ProfileExecutionAdmissionGuard guard = CreateAdmissionGuard(
            new ProfileAdmissionOptions
            {
                GlobalConcurrency = 1,
                SameProfileConcurrency = 1
            });
        using ProfileAdmissionLease active = guard.TryAcquire("hubot", null);
        ThrowingProfileSnapshotCollector collector = new();
        ProfileModel model = CreateModel(collector, guard);
        model.PageContext = CreatePageContext();
        model.GitHubTarget = Identity;

        object action = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, model.Response.StatusCode);
        Assert.Equal(0, collector.CallCount);
        Assert.False(model.HasComposedProfile);
        Assert.Null(model.Report);
        Assert.Equal("Profile analysis is temporarily busy", model.ErrorTitle);
        Assert.Contains("cannot start another", model.ErrorMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnPostAsync_WhenVisitorLimitRejects_ReturnsTooManyRequestsWithBoundedRetry()
    {
        ProfileExecutionAdmissionGuard guard = CreateAdmissionGuard(
            new ProfileAdmissionOptions
            {
                GlobalConcurrency = 1,
                SameProfileConcurrency = 1,
                VisitorPermitLimit = 1,
                VisitorWindow = TimeSpan.FromMinutes(1)
            });
        IPAddress visitor = IPAddress.Parse("192.0.2.44");
        using (ProfileAdmissionLease first = guard.TryAcquire("hubot", visitor))
        {
            Assert.True(first.IsAdmitted);
        }

        ThrowingProfileSnapshotCollector collector = new();
        ProfileModel model = CreateModel(collector, guard);
        model.PageContext = CreatePageContext(visitor);
        model.GitHubTarget = Identity;

        object action = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Equal(StatusCodes.Status429TooManyRequests, model.Response.StatusCode);
        Assert.Equal("60", model.Response.Headers.RetryAfter);
        Assert.Equal(0, collector.CallCount);
        Assert.False(model.HasComposedProfile);
        Assert.Null(model.Report);
        Assert.Equal("Profile requests are temporarily limited", model.ErrorTitle);
        Assert.Contains("could not start", model.ErrorMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnPostAsync_WhenBudgetProtectionRejects_ReturnsServiceUnavailableWithoutPipeline()
    {
        DataAvailability profileAvailability =
            DataAvailability.Available("profile", CapturedAt);
        ProfileModel model = new(
            new LiveProfileReportService(
                new BudgetRejectingProfileSnapshotCollector(profileAvailability),
                CreateRealPipeline()),
            Version,
            new WebPresentationLocalizer(),
            DisabledProfileExecutionAdmissionGuard.Instance)
        {
            PageContext = CreatePageContext(),
            GitHubTarget = Identity
        };

        object action = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(action);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, model.Response.StatusCode);
        Assert.False(model.HasComposedProfile);
        Assert.Null(model.Report);
        Assert.Equal("Profile analysis is temporarily unavailable", model.ErrorTitle);
        Assert.NotNull(model.ErrorMessage);
        Assert.DoesNotContain("remaining", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("floor", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnPostAsync_WithRealPartialProfile_KeepsReportAndCanonicalLimitationsVisibleToPresentation()
    {
        FakeProfileSnapshotCollector collector =
            new(CreateSuccessfulSnapshotResult(isRepositoryManifestComplete: false));
        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = Identity;

        await model.OnPostAsync(CancellationToken.None);

        Assert.True(model.HasComposedProfile);
        Assert.True(model.IsPartialProfile);
        Assert.NotNull(model.Report);
        Assert.Contains(
            model.Report.Limitations.Items,
            limitation => limitation.Code == ProfileLimitationCode.PartialEvidenceSurface);
        Assert.Null(model.ErrorTitle);
        Assert.Null(model.ErrorMessage);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.NotApplicable, "This profile is not supported")]
    [InlineData(DataAvailabilityStatus.Forbidden, "Profile evidence is unavailable")]
    [InlineData(DataAvailabilityStatus.Unavailable, "Profile evidence is unavailable")]
    [InlineData(DataAvailabilityStatus.Failed, "Profile evidence is unavailable")]
    [InlineData(DataAvailabilityStatus.NotRequested, "Profile evidence is unavailable")]
    public async Task OnPostAsync_WithTypedProfileAvailability_UsesBoundedCopyWithoutRawReason(
        DataAvailabilityStatus status,
        string expectedTitle)
    {
        const string rawReason = "provider-secret-diagnostic";
        FakeProfileSnapshotCollector collector = new(
            new GitHubProfileSnapshotResult(
                snapshot: null,
                DataAvailability.Unavailable("profile", status, rawReason)));
        ProfileModel model = CreateModel(collector);
        model.GitHubTarget = Identity;

        await model.OnPostAsync(CancellationToken.None);

        Assert.False(model.HasComposedProfile);
        Assert.False(model.IsPartialProfile);
        Assert.Null(model.Report);
        Assert.Equal(expectedTitle, model.ErrorTitle);
        Assert.DoesNotContain(rawReason, model.ErrorMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public void TerminalPipelineFailure_UsesCompositionFailurePresentation_NotOperationalGuardCopy()
    {
        ProfileModel model = CreateModel(new ThrowingProfileSnapshotCollector());
        ProfilePipelineResult failed = ProfilePipelineResult.Failed(
            ProfileCompositionResult.Failed());

        System.Reflection.MethodInfo applyPipelineResult = typeof(ProfileModel)
            .GetMethod(
                "ApplyPipelineResult",
                System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                "ProfileModel.ApplyPipelineResult was not found.");

        applyPipelineResult.Invoke(model, [failed]);

        Assert.False(model.HasComposedProfile);
        Assert.False(model.IsPartialProfile);
        Assert.Null(model.Report);
        Assert.Equal("The profile view could not be completed", model.ErrorTitle);
        Assert.Contains("compose a profile", model.ErrorMessage!, StringComparison.Ordinal);
        Assert.DoesNotContain("too long", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("temporarily busy", model.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    private static ProfileModel CreateModel(
        IGitHubProfileSnapshotCollector collector,
        IProfileExecutionAdmissionGuard? admissionGuard = null) =>
        new(
            new LiveProfileReportService(collector, CreateRealPipeline()),
            Version,
            new WebPresentationLocalizer(),
            admissionGuard ?? DisabledProfileExecutionAdmissionGuard.Instance);

    private static ProfileExecutionAdmissionGuard CreateAdmissionGuard(
        ProfileAdmissionOptions options) =>
        new(Options.Create(options), TimeProvider.System);

    private static PageContext CreatePageContext(IPAddress? visitor = null)
    {
        DefaultHttpContext context = new();
        context.Connection.RemoteIpAddress = visitor;
        return new PageContext { HttpContext = context };
    }

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

    private static GitHubProfileSnapshotResult CreateSuccessfulSnapshotResult(
        bool isRepositoryManifestComplete = true)
    {
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                Identity,
                new Uri($"https://github.com/{Identity}")),
            CapturedAt,
            metadata: null,
            repositories: new ProfileRepositoryManifest(
                isComplete: isRepositoryManifestComplete),
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
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CallCount++;
            LastProfileIdentity = profile;

            progress?.ResolveProfileAvailability(result.ProfileAvailability);

            return Task.FromResult(result);
        }
    }

    // Fails loudly if any code path reaches live collection when it must not.
    private sealed class ThrowingProfileSnapshotCollector : IGitHubProfileSnapshotCollector
    {
        public int CallCount => 0;

        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null) =>
            throw new InvalidOperationException(
                "Live profile collection must not be reachable from this path.");
    }

    private sealed class BudgetRejectingProfileSnapshotCollector(
        DataAvailability profileAvailability)
        : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null)
        {
            progress?.ResolveProfileAvailability(profileAvailability);
            throw new GitHubProfileBudgetProtectionRejectedException();
        }
    }

    private sealed class AvailabilityThenDeadlineCollector(
        DataAvailability profileAvailability,
        DeadlineAfterAvailabilityFactory deadlineFactory)
        : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null)
        {
            bool published = progress?.ResolveProfileAvailability(profileAvailability)
                ?? false;
            Assert.True(published);

            deadlineFactory.CancelCurrent();
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException(
                "The execution token should have been cancelled.");
        }
    }

    private sealed class DeadlineAfterAvailabilityFactory
        : IProfileExecutionDeadlineFactory
    {
        private TriggerableDeadline? current;

        public IProfileExecutionDeadline Create(
            CancellationToken callerCancellationToken)
        {
            current = new TriggerableDeadline(callerCancellationToken);
            return current;
        }

        public void CancelCurrent() =>
            (current ?? throw new InvalidOperationException(
                "The execution deadline has not been created."))
            .Cancel();
    }

    private sealed class TriggerableDeadline : IProfileExecutionDeadline
    {
        private readonly CancellationTokenSource deadline = new();
        private readonly CancellationTokenSource execution;
        private bool expired;

        public TriggerableDeadline(CancellationToken callerCancellationToken)
        {
            execution = CancellationTokenSource.CreateLinkedTokenSource(
                callerCancellationToken,
                deadline.Token);
        }

        public CancellationToken ExecutionToken => execution.Token;

        public bool IsExpired => expired;

        public bool TryComplete() => !expired;

        public void Cancel()
        {
            expired = true;
            deadline.Cancel();
        }

        public void Dispose()
        {
            execution.Dispose();
            deadline.Dispose();
        }
    }

    private sealed class ExpiredDeadlineFactory : IProfileExecutionDeadlineFactory
    {
        public IProfileExecutionDeadline Create(
            CancellationToken callerCancellationToken) =>
            new ExpiredDeadline(callerCancellationToken);
    }

    private sealed class ExpiredDeadline : IProfileExecutionDeadline
    {
        private readonly CancellationTokenSource deadline = new();
        private readonly CancellationTokenSource execution;

        public ExpiredDeadline(CancellationToken callerCancellationToken)
        {
            execution = CancellationTokenSource.CreateLinkedTokenSource(
                callerCancellationToken,
                deadline.Token);
            deadline.Cancel();
        }

        public CancellationToken ExecutionToken => execution.Token;

        public bool IsExpired => true;

        public bool TryComplete() => false;

        public void Dispose()
        {
            execution.Dispose();
            deadline.Dispose();
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

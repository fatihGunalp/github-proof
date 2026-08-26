using System.Net;
using System.Net.Http.Headers;
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
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 120 — minimum runtime instrumentation
// (docs/architecture/public-profile-runtime-collection.md §9 / §9.1).
//
// Everything proven here is about OBSERVATION. The instrumentation exists so a
// later controlled benchmark can state profile size -> request amplification ->
// pipeline cost -> wall-clock time. No value added in this slice is read by an
// evaluator, policy, composer, projector, admission guard or budget decision,
// and none of it changes a single analysis outcome.
public sealed class ProfileRuntimeInstrumentationTests
{
    private const string ProfileIdentity = "octocat";

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-24T00:10:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    // ------------------------------------------------- 1. behavioral neutrality

    // The load-bearing neutrality proof: the identical profile input, run once
    // through the instrumented wiring AddProfilePipeline produces and once
    // through the bare production components, must yield the same profile truth
    // and the same report shape.
    [Fact]
    public async Task Instrumentation_DoesNotChangeProfileTruthOrReportShape()
    {
        ProfilePipelineResult instrumented = await RunPipelineAsync(instrumented: true);
        ProfilePipelineResult bare = await RunPipelineAsync(instrumented: false);

        Assert.True(instrumented.HasProfile);
        Assert.True(bare.HasProfile);

        Assert.Equal(
            bare.Profile!.AnalysisStatus,
            instrumented.Profile!.AnalysisStatus);
        Assert.Equal(
            LimitationCodes(bare.Profile),
            LimitationCodes(instrumented.Profile));
        Assert.Equal(
            WarningCodes(bare.Profile),
            WarningCodes(instrumented.Profile));
        Assert.Equal(
            EligibilityStatuses(bare.Profile),
            EligibilityStatuses(instrumented.Profile));
        Assert.Equal(
            ReportShape(bare.Report!),
            ReportShape(instrumented.Report!));
    }

    // Repository outcome classification is operational truth the benchmark will
    // read; the timing work above it must not have disturbed it.
    [Fact]
    public async Task Instrumentation_LeavesOperationalFailureClassificationUnchanged()
    {
        LiveProfileReportService service = CreateService();

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(3, telemetry.RepositoriesDiscovered);
        Assert.Equal(2, telemetry.RepositoriesAdmitted);
        Assert.Equal(1, telemetry.RepositoriesCompleted);
        Assert.Equal(1, telemetry.RepositoriesIncomplete);
        Assert.Equal(
            1,
            telemetry.RepositoryOutcomes[DataAvailabilityStatus.Failed.ToString()]);
        Assert.All(
            telemetry.RepositoryOutcomes.Keys,
            key => Assert.Contains(key, ProfileRepositoryOutcome.All));
    }

    // ----------------------------------------------------- 2. timing structure

    [Fact]
    public async Task Timing_IsStructurallyValidAndNonNegative()
    {
        LiveProfileReportService service = CreateService();

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;
        ProfileDurationDistribution collection = telemetry.RepositoryCollection;
        ProfileDurationDistribution analysis = telemetry.RepositoryAnalysis;

        // Session 131: this runs at the default repository concurrency (R=1),
        // where the per-repository intervals cannot overlap, so the derived
        // remainder is still a real duration. It is null only under R>1.
        TimeSpan outsideRepositories = Assert.NotNull(
            telemetry.PipelineWorkOutsideRepositoriesElapsed);

        foreach (TimeSpan value in new[]
        {
            telemetry.Duration,
            telemetry.DiscoveryElapsed,
            telemetry.PipelineElapsed,
            outsideRepositories,
            collection.Total,
            collection.Minimum,
            collection.Maximum,
            analysis.Total,
            analysis.Minimum,
            analysis.Maximum
        })
        {
            Assert.True(value >= TimeSpan.Zero);
        }

        Assert.True(collection.Minimum <= collection.Maximum);
        Assert.True(analysis.Minimum <= analysis.Maximum);
        Assert.True(collection.Total >= collection.Maximum);
        Assert.True(analysis.Total >= analysis.Maximum);
    }

    // The sample counts must equal the number of times the orchestrator actually
    // reached each seam: two admitted repositories collected, one of which went
    // on to be analysed.
    [Fact]
    public async Task Timing_SamplesEveryRepositorySeamExactlyOnce()
    {
        LiveProfileReportService service = CreateService();

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(
            telemetry.RepositoriesAdmitted,
            telemetry.RepositoryCollection.Count);
        Assert.Equal(
            telemetry.RepositoriesCompleted,
            telemetry.RepositoryAnalysis.Count);
    }

    // The derived remainder is a subtraction, not an estimate: the two
    // aggregated intervals are disjoint and contained in the pipeline interval,
    // so the identity below must hold exactly whenever the remainder is
    // positive.
    [Fact]
    public void PipelineRemainder_IsAnExactSubtractionOfContainedIntervals()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordPipelineElapsed(TimeSpan.FromMilliseconds(100));
        telemetry.RecordRepositoryCollectionElapsed(TimeSpan.FromMilliseconds(40));
        telemetry.RecordRepositoryCollectionElapsed(TimeSpan.FromMilliseconds(20));
        telemetry.RecordRepositoryAnalysisElapsed(TimeSpan.FromMilliseconds(15));

        Assert.Equal(
            TimeSpan.FromMilliseconds(25),
            telemetry.PipelineWorkOutsideRepositoriesElapsed);
    }

    // Mutation-oriented: when the sampled intervals exceed the pipeline
    // interval the remainder is not a duration at all, and must not be reported
    // as one.
    //
    // Session 131 changed this from a clamp-to-zero to null. Under repository
    // concurrency R > 1 the per-repository intervals overlap and their sum
    // routinely exceeds wall clock, so a clamped "0 ms" would have been a
    // routine, silent, and wrong reading of "no pipeline work outside
    // repositories" — precisely the measurement §9.2 exists to keep honest.
    [Fact]
    public void PipelineRemainder_IsUndefinedWhenIntervalsExceedThePipeline()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordPipelineElapsed(TimeSpan.FromMilliseconds(10));
        telemetry.RecordRepositoryCollectionElapsed(TimeSpan.FromMilliseconds(999));

        Assert.Null(telemetry.PipelineWorkOutsideRepositoriesElapsed);
        Assert.Contains(
            "PipelineOutsideRepositoriesMs=undefined",
            telemetry.ToLogMessage(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DurationDistribution_IsEmptyBeforeAnySample()
    {
        ProfileExecutionTelemetry telemetry = new();

        Assert.Equal(ProfileDurationDistribution.Empty, telemetry.RepositoryCollection);
        Assert.Equal(ProfileDurationDistribution.Empty, telemetry.RepositoryAnalysis);
        Assert.Equal(TimeSpan.Zero, telemetry.DiscoveryElapsed);
        Assert.Equal(TimeSpan.Zero, telemetry.PipelineElapsed);
    }

    // Discovery is recorded in a finally, so a profile whose metadata is
    // unavailable still reports the wall-clock its discovery consumed and never
    // reaches the pipeline interval at all.
    [Fact]
    public async Task Timing_RecordsDiscoveryEvenWhenTheProfileIsUnavailable()
    {
        LiveProfileReportService service = new(
            new StubProfileCollector(
                new GitHubProfileSnapshotResult(
                    snapshot: null,
                    DataAvailability.Unavailable(
                        "profile",
                        DataAvailabilityStatus.RateLimited,
                        "GitHub rate limit was reached."))),
            CreatePipeline(new StubRepositorySnapshotSource(), instrumented: true));

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.False(result.IsSuccessful);
        Assert.True(telemetry.DiscoveryElapsed >= TimeSpan.Zero);
        Assert.Equal(TimeSpan.Zero, telemetry.PipelineElapsed);
        Assert.Equal(0, telemetry.RepositoryCollection.Count);
    }

    // ------------------------------------------- 3. rate-limit OBSERVATION only

    // The stub sends no resource and no reset header, so every observation
    // belongs to one bucket and the sequence is a valid comparison.
    [Fact]
    public async Task RateLimit_RecordsFirstLastAndMinimumAcrossEveryCategory()
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            await SendAsync("users/octocat", remaining: "4900");
            await SendAsync("repos/octocat/hello-world", remaining: "4300");
            await SendAsync("repos/octocat/hello-world/commits", remaining: "4700");
        }

        ProfileRateLimitWindow window = Assert.Single(telemetry.RateLimitWindows);

        Assert.Equal(ProfileRateLimitResource.Unspecified, window.Resource);
        Assert.Null(window.ResetAt);
        Assert.Equal(4900, window.FirstRemaining);
        Assert.Equal(4700, window.LastRemaining);
        Assert.Equal(4300, window.MinimumRemaining);
        Assert.Equal(3, window.SampleCount);
        Assert.Equal(0, telemetry.UnrecordedRateLimitObservations);
    }

    // The widened OBSERVATION must not widen the proactive budget DECISION: it
    // still reads the profile-metadata response only
    // (public-profile-runtime-collection.md §10.4 / §11.3).
    [Fact]
    public async Task RateLimit_ObservationDoesNotWidenTheBudgetDecisionSurface()
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            await SendAsync("repos/octocat/hello-world", remaining: "11");
            await SendAsync("repos/octocat/hello-world/commits", remaining: "12");
        }

        Assert.Null(budgetState.MetadataObservation);
        Assert.Equal(
            11,
            Assert.Single(telemetry.RateLimitWindows).MinimumRemaining);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            await SendAsync("users/octocat", remaining: "13");
        }

        Assert.Equal(13, budgetState.MetadataObservation!.Remaining);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    [InlineData("-1")]
    [InlineData("9999999999999999999999")]
    public async Task RateLimit_AbsentOrMalformedHeaderChangesNothing(string? remaining)
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            HttpStatusCode status = await SendAsync("users/octocat", remaining);

            Assert.Equal(HttpStatusCode.OK, status);
        }

        Assert.Empty(telemetry.RateLimitWindows);
        Assert.Null(budgetState.MetadataObservation);
        Assert.Equal(1, telemetry.GitHubRequests);
    }

    // A negative value can never reach the recorder through the handler, but the
    // recorder itself must still refuse it rather than corrupt the minimum.
    [Fact]
    public void RateLimit_RecorderRejectsANegativeSample()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRateLimitObservation(
            resource: null,
            remaining: -5,
            limit: null,
            used: null,
            resetAt: null);

        Assert.Empty(telemetry.RateLimitWindows);
    }

    [Fact]
    public void RateLimit_IsUnobservedWhenNoResponseCarriedTheHeader()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.Complete(
            TimeSpan.FromMilliseconds(1),
            "Complete",
            ProfileExecutionOutcome.Composed);

        Assert.Empty(telemetry.RateLimitWindows);
        Assert.Contains(
            "RateLimitBuckets={unobserved}",
            telemetry.ToLogMessage(),
            StringComparison.Ordinal);
    }

    // ------------------------------------------------- 4. request-count fidelity

    [Fact]
    public async Task RequestCount_StillMatchesActualHandlerInvocations()
    {
        ProfileExecutionTelemetry telemetry = new();
        CountingInnerHandler inner = new();
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            foreach (string path in new[]
            {
                "users/octocat",
                "users/octocat/repos",
                "repos/octocat/hello-world",
                "repos/octocat/hello-world/contents/src/App.csproj",
                "repos/octocat/hello-world/contents/.github/workflows/ci.yml"
            })
            {
                using HttpResponseMessage response = await httpClient.GetAsync(path);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        Assert.Equal(inner.SendCount, telemetry.GitHubRequests);
        Assert.Equal(5, telemetry.GitHubRequests);

        // I-5 stays narrowed: contents amplification is answered by the single
        // existing category, and no new vocabulary was introduced for it.
        Assert.Equal(
            2,
            telemetry.RequestCategories[ProfileGitHubRequestCategory.RepositoryContents]);
    }

    [Fact]
    public async Task RequestCategoryVocabulary_RemainsClosed()
    {
        LiveProfileReportService service = CreateService();

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.All(
            telemetry.RequestCategories.Keys,
            key => Assert.Contains(key, ProfileGitHubRequestCategory.All));
        // Session 124 (S1): exactly one new member, the single GraphQL HTTP
        // category authorized by §18.9. The vocabulary is still closed.
        Assert.Equal(18, ProfileGitHubRequestCategory.All.Count);
    }

    // ------------------------------------------------------------- 5. privacy

    // §9.1: the whole rendered telemetry surface must be free of any repository,
    // owner, path, credential or content substring.
    [Fact]
    public async Task Telemetry_NeverCarriesIdentityOrContent()
    {
        const string token = "ghp_thisIsASecretTokenValue";

        ProfileExecutionTelemetry telemetry = new();
        CountingInnerHandler inner = new();
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                "repos/some-owner/some-private-name/contents/src/Secret.cs");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        telemetry.RecordRepositoryCollectionElapsed(TimeSpan.FromMilliseconds(7));
        telemetry.RecordRepositoryAnalysisElapsed(TimeSpan.FromMilliseconds(3));
        telemetry.RecordDiscoveryElapsed(TimeSpan.FromMilliseconds(11));
        telemetry.RecordPipelineElapsed(TimeSpan.FromMilliseconds(40));
        telemetry.Complete(
            TimeSpan.FromMilliseconds(60),
            "Complete",
            ProfileExecutionOutcome.Composed);

        string message = telemetry.ToLogMessage();

        foreach (string forbidden in new[]
        {
            token,
            "Bearer",
            "Authorization",
            "some-owner",
            "some-private-name",
            "Secret.cs",
            "src/",
            "octocat",
            "https://",
            "api.github.com"
        })
        {
            Assert.DoesNotContain(forbidden, message, StringComparison.OrdinalIgnoreCase);
        }
    }

    // The new timing values are rendered as durations only. A future change that
    // keyed a duration by repository would fail here.
    [Fact]
    public async Task LogMessage_RendersTheNewOperationalValues()
    {
        LiveProfileReportService service = CreateService();

        await service.AnalyzeAsync(ProfileIdentity, Version);

        string message = service.LastExecutionTelemetry!.ToLogMessage();

        foreach (string token in new[]
        {
            "DiscoveryMs=",
            "PipelineMs=",
            "PipelineOutsideRepositoriesMs=",
            "RepositoryCollection={count=2,",
            "RepositoryAnalysis={count=1,",
            "RateLimitBuckets={",
            "UnrecordedRateLimitObservations=0"
        })
        {
            Assert.Contains(token, message, StringComparison.Ordinal);
        }
    }

    // ------------------------------------ 6. cancellation / deadline unchanged

    [Fact]
    public async Task CallerCancellation_StillPropagatesUnchanged()
    {
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        LiveProfileReportService service = CreateService();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AnalyzeAsync(ProfileIdentity, Version, cancellation.Token));

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(ProfileExecutionOutcome.Cancelled, telemetry.Outcome);
        Assert.Equal(ProfileExecutionOutcome.Cancelled, telemetry.Status);
    }

    // The instrumentation must not have introduced a second definition of "how
    // long the execution took": Duration still comes from the outer stopwatch
    // and still bounds every interval measured inside it.
    [Fact]
    public async Task TotalDuration_StillEnclosesTheMeasuredIntervals()
    {
        LiveProfileReportService service = CreateService();

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.True(telemetry.Duration >= telemetry.DiscoveryElapsed);
        Assert.True(telemetry.Duration >= telemetry.PipelineElapsed);
        Assert.True(
            telemetry.PipelineElapsed
                >= telemetry.RepositoryCollection.Total + telemetry.RepositoryAnalysis.Total);
    }

    // ------------------------------------------------------------- test support

    private static async Task<ProfilePipelineResult> RunPipelineAsync(bool instrumented)
    {
        LiveProfileReportService service = new(
            new StubProfileCollector(CreateProfileSnapshotResult()),
            CreatePipeline(new StubRepositorySnapshotSource(), instrumented));

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        return result.PipelineResult!;
    }

    private static IReadOnlyList<string> LimitationCodes(
        Domain.Profile.Models.DeveloperPublicEvidenceProfile profile) =>
        [.. profile.Limitations.Select(limitation => $"{limitation.Code}/{limitation.Scope}")];

    private static IReadOnlyList<string> WarningCodes(
        Domain.Profile.Models.DeveloperPublicEvidenceProfile profile) =>
        [.. profile.Warnings.Select(warning => $"{warning.Code}/{warning.Scope}")];

    private static IReadOnlyList<string> EligibilityStatuses(
        Domain.Profile.Models.DeveloperPublicEvidenceProfile profile) =>
        [
            .. profile.RepositoryEligibilityResults.Select(
                result => $"{result.Reference.FullName}={result.Status}")
        ];

    // All nine canonical sections, by key, availability and item count, plus the
    // warnings the report carries for presentation.
    private static IReadOnlyList<string> ReportShape(ProfileReport report) =>
        [
            Shape(report.PublicProfileSummary),
            Shape(report.TechnicalFocusAreas),
            Shape(report.EngineeringPracticePatterns),
            Shape(report.ProblemAreaPatterns),
            Shape(report.RepresentativeRepositories),
            Shape(report.WhyThisConclusion),
            Shape(report.Limitations),
            Shape(report.RepositoryDrillDown),
            Shape(report.RoleEvidenceAlignment),
            $"warnings/{report.Warnings.Count}"
        ];

    private static string Shape<TItem>(ProfileReportSection<TItem> section) =>
        $"{section.SectionKey}/{section.Availability}/{section.Items.Count}";

    private static LiveProfileReportService CreateService() =>
        new(
            new StubProfileCollector(CreateProfileSnapshotResult()),
            CreatePipeline(new StubRepositorySnapshotSource(), instrumented: true));

    // instrumented: the two observation-only wrappers AddProfilePipeline adds.
    // bare: the same production components with those wrappers removed, so any
    // behavioural difference would have to come from them.
    //
    // The eligibility policy is pinned to V1 here, not the V2 production
    // registers: the neutrality proof is an A/B comparison, so both arms must
    // differ ONLY by the wrappers, and the fixture is shared with the sibling
    // telemetry tests. This is deliberately not a claim about production wiring.
    private static ProfilePipeline CreatePipeline(
        IProfileRepositorySnapshotSource source,
        bool instrumented) =>
        new(
            new ProfileRepositoryAnalysisOrchestrator(
                instrumented
                    ? new TelemetryProfileRepositorySnapshotSource(source)
                    : source,
                instrumented
                    ? new TelemetryRepositoryAnalysisRunner(new RepositoryAnalysisRunner())
                    : new RepositoryAnalysisRunner(),
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

    private static async Task<HttpStatusCode> SendAsync(
        string path,
        string? remaining)
    {
        ProfileGitHubRequestTelemetryHandler handler = new()
        {
            InnerHandler = new CountingInnerHandler(remaining)
        };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using HttpResponseMessage response = await httpClient.GetAsync(path);

        return response.StatusCode;
    }

    private static GitHubProfileSnapshotResult CreateProfileSnapshotResult()
    {
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                ProfileIdentity,
                new Uri($"https://github.com/{ProfileIdentity}")),
            CapturedAt,
            metadata: null,
            new ProfileRepositoryManifest(
                [
                    CreateRepository("analyzed"),
                    CreateRepository("unavailable"),
                    CreateRepository("empty", RepositoryConditionState.Present)
                ]),
            [DataAvailability.Available("profile-metadata", CapturedAt)],
            collectionWarnings: null);

        return new GitHubProfileSnapshotResult(
            snapshot,
            DataAvailability.Available("profile", CapturedAt));
    }

    private static ProfileRepository CreateRepository(
        string name,
        RepositoryConditionState empty = RepositoryConditionState.Absent) =>
        new(
            new ProfileRepositoryReference(
                "octocat",
                name,
                new Uri($"https://github.com/octocat/{name}")),
            defaultBranch: "main",
            primaryLanguage: "C#",
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            empty,
            CapturedAt.AddDays(-10),
            CapturedAt.AddYears(-2));

    private sealed class CountingInnerHandler(string? remaining = null) : HttpMessageHandler
    {
        private int sendCount;

        public int SendCount => Volatile.Read(ref sendCount);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref sendCount);

            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };

            if (remaining is not null)
            {
                response.Headers.TryAddWithoutValidation(
                    "X-RateLimit-Remaining",
                    remaining);
            }

            return Task.FromResult(response);
        }
    }

    private sealed class StubProfileCollector(GitHubProfileSnapshotResult result)
        : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress?.ResolveProfileAvailability(result.ProfileAvailability);

            return Task.FromResult(result);
        }
    }

    private sealed class StubRepositorySnapshotSource : IProfileRepositorySnapshotSource
    {
        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                reference.Name == "unavailable"
                    ? new ProfileRepositorySnapshotResult(
                        snapshot: null,
                        DataAvailability.Unavailable(
                            "metadata",
                            DataAvailabilityStatus.Failed,
                            "GitHub could not be reached."))
                    : new ProfileRepositorySnapshotResult(
                        CreateRepositorySnapshot(reference),
                        DataAvailability.Available("metadata", CapturedAt)));
        }

        private static RepositorySnapshot CreateRepositorySnapshot(
            ProfileRepositoryReference reference) =>
            new(
                new RepositoryIdentity(
                    reference.Owner,
                    reference.Name,
                    reference.Url,
                    "main",
                    isPublic: true,
                    isArchived: false,
                    isFork: false,
                    isTemplate: false,
                    primaryLanguage: "C#"),
                CapturedAt,
                headCommitSha: "abc123",
                metadata: null,
                new RepositoryReadme("README.md", "# Project\n\nOverview."),
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
                [DataAvailability.Available("readme", CapturedAt)],
                collectionWarnings: null);
    }
}

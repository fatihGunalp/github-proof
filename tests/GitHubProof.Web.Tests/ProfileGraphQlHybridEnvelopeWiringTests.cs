using System.Net;
using System.Text;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 127 — S3 wiring proofs for
// docs/architecture/public-profile-runtime-collection.md §§18-19.
//
// Three properties matter: the authoritative envelope is OFF unless
// configuration says otherwise; OFF means the Profile path is exactly what it is
// today; and ON produces the same ProfileReport while issuing strictly fewer
// provider requests.
public sealed class ProfileGraphQlHybridEnvelopeWiringTests
{
    private const string RepositoryUrl = "https://github.com/octocat/hello-world";
    private const string MetadataRequest = "/repos/octocat/hello-world";
    private const string LanguagesRequest = "/repos/octocat/hello-world/languages";

    // Session 129. The two identity requests an accepted envelope also
    // replaces, and the recursive tree request it does NOT.
    private const string GitRefRequest =
        "/repos/octocat/hello-world/git/ref/heads/main";

    private const string GitCommitRequest =
        "/repos/octocat/hello-world/git/commits/" + FixtureHandler.HeadCommitSha;

    // This handler records AbsolutePath, so the `?recursive=1` query the
    // collector sends is not part of the recorded value.
    private const string RecursiveTreeRequest =
        "/repos/octocat/hello-world/git/trees/" + FixtureHandler.RootTreeSha;

    // ------------------------------------------------------- 1. default is off

    [Fact]
    public void RepositoryEnvelope_IsDisabledByDefault()
    {
        using ServiceProvider provider = BuildProvider(out _);

        Assert.False(provider
            .GetRequiredService<IOptions<ProfileGraphQlHybridOptions>>()
            .Value
            .RepositoryEnvelopeEnabled);
    }

    [Fact]
    public void RepositoryEnvelope_StaysDisabledWhenConfigurationSaysNothingAboutIt()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProfileExecutionDeadline:Timeout"] = "00:03:00"
            })
            .Build();
        using ServiceProvider provider = BuildProvider(out _, configuration);

        Assert.False(provider
            .GetRequiredService<IOptions<ProfileGraphQlHybridOptions>>()
            .Value
            .RepositoryEnvelopeEnabled);
    }

    // Default OFF is not "an envelope that declines"; it is no envelope at all,
    // and the exact existing request graph.
    [Fact]
    public async Task DefaultOff_IssuesZeroGraphQlRequestsAndKeepsBothRestRequests()
    {
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler);
        using IServiceScope scope = provider.CreateScope();

        ProfileRepositorySnapshotResult result = await scope.ServiceProvider
            .GetRequiredService<IProfileRepositorySnapshotSource>()
            .CollectAsync(Reference());

        Assert.All(
            handler.Paths,
            path => Assert.DoesNotContain("/graphql", path, StringComparison.Ordinal));
        Assert.Equal(1, handler.Count(MetadataRequest));
        Assert.Equal(1, handler.Count(LanguagesRequest));

        // Session 129: off, the identity chain is the REST one, in full.
        Assert.Equal(1, handler.Count(GitRefRequest));
        Assert.Equal(1, handler.Count(GitCommitRequest));
        Assert.Equal(1, handler.Count(RecursiveTreeRequest));

        Assert.True(result.IsSuccessful);
        Assert.Equal(
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 },
            result.Snapshot!.Languages);
    }

    // ------------------------------- 2. enabled replaces the two REST requests

    [Fact]
    public async Task Enabled_ServesTheEnvelopeAndSkipsBothRestRequests()
    {
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler, EnvelopeEnabled());
        using IServiceScope scope = provider.CreateScope();

        ProfileRepositorySnapshotResult result = await scope.ServiceProvider
            .GetRequiredService<IProfileRepositorySnapshotSource>()
            .CollectAsync(Reference());

        Assert.Equal(1, handler.Count("/graphql"));
        Assert.Equal(0, handler.Count(MetadataRequest));
        Assert.Equal(0, handler.Count(LanguagesRequest));

        // Session 129: the two identity GETs are gone as well, and the
        // recursive tree GET is NOT — it runs from the envelope tree OID.
        Assert.Equal(0, handler.Count(GitRefRequest));
        Assert.Equal(0, handler.Count(GitCommitRequest));
        Assert.Equal(1, handler.Count(RecursiveTreeRequest));

        Assert.True(result.IsSuccessful);

        // The snapshot is anchored to the commit the ENVELOPE named.
        Assert.Equal(FixtureHandler.HeadCommitSha, result.Snapshot!.HeadCommitSha);
        Assert.Equal(
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 },
            result.Snapshot.Languages);
        Assert.Equal("hello-world", result.Snapshot.Repository.Name);
    }

    // ------------------------------- the shadow/envelope precedence rule

    // With the envelope on, the languages shadow is not constructed at all — so
    // the same repository can never cost a second GraphQL request for an
    // observation the envelope already made authoritative.
    [Fact]
    public async Task EnabledTogetherWithTheShadow_StillIssuesExactlyOnePost()
    {
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler, BothEnabled());
        using IServiceScope scope = provider.CreateScope();

        await scope.ServiceProvider
            .GetRequiredService<IProfileRepositorySnapshotSource>()
            .CollectAsync(Reference());

        Assert.Equal(1, handler.Count("/graphql"));
        Assert.Equal(0, handler.Count(LanguagesRequest));
    }

    // The shadow alone still behaves exactly as Session 125 wired it, so the
    // precedence rule above is a precedence rule and not a removal.
    [Fact]
    public async Task TheShadowAlone_StillObservesAndLeavesRestAuthoritative()
    {
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler, ShadowEnabled());
        using IServiceScope scope = provider.CreateScope();

        await scope.ServiceProvider
            .GetRequiredService<IProfileRepositorySnapshotSource>()
            .CollectAsync(Reference());

        Assert.Equal(1, handler.Count("/graphql"));
        Assert.Equal(1, handler.Count(MetadataRequest));
        Assert.Equal(1, handler.Count(LanguagesRequest));
    }

    // --------------------------------- 18/19. the Profile result cannot drift

    // The headline claim of S3, proven end to end: the same profile input with
    // the envelope OFF and with an ACCEPTED envelope produces the same
    // ProfileReport, status, warnings and limitations — while issuing one fewer
    // provider request.
    [Fact]
    public async Task EnvelopeOffAndAccepted_ProduceTheIdenticalProfileReport()
    {
        ProfileRun off = await RunProfileAsync(configuration: null);
        ProfileRun on = await RunProfileAsync(EnvelopeEnabled());

        // Not vacuous: the OFF run issued no POST and kept both REST requests;
        // the ON run issued one POST and kept neither.
        Assert.Equal(0, off.GraphQlRequests);
        Assert.Equal(1, on.GraphQlRequests);
        Assert.Equal(1, off.Count(MetadataRequest));
        Assert.Equal(1, off.Count(LanguagesRequest));
        Assert.Equal(0, on.Count(MetadataRequest));
        Assert.Equal(0, on.Count(LanguagesRequest));

        // Session 129: three fewer physical provider requests for the accepted
        // repository, and an identical report. Request counts only (§18.11).
        Assert.Equal(1, off.Count(GitRefRequest));
        Assert.Equal(1, off.Count(GitCommitRequest));
        Assert.Equal(0, on.Count(GitRefRequest));
        Assert.Equal(0, on.Count(GitCommitRequest));
        Assert.Equal(1, off.Count(RecursiveTreeRequest));
        Assert.Equal(1, on.Count(RecursiveTreeRequest));

        Assert.Equal(off.Result.Status, on.Result.Status);
        Assert.True(off.Result.HasProfile);
        Assert.True(on.Result.HasProfile);
        Assert.Equal(
            off.Result.Profile!.AnalysisStatus,
            on.Result.Profile!.AnalysisStatus);
        Assert.Equal(FingerprintReport(off.Result), FingerprintReport(on.Result));
    }

    // Fallback parity: an errored or incomplete candidate still yields the
    // REST-only result, and the run pays the two REST requests it always did.
    [Theory]
    [InlineData(GraphQlEnvelopeMode.Errored)]
    [InlineData(GraphQlEnvelopeMode.IncompleteLanguages)]
    [InlineData(GraphQlEnvelopeMode.Malformed)]
    [InlineData(GraphQlEnvelopeMode.EnvelopeFailure)]
    public async Task EveryFallback_ProducesTheIdenticalProfileReport(GraphQlEnvelopeMode mode)
    {
        ProfileRun off = await RunProfileAsync(configuration: null);
        ProfileRun fallback = await RunProfileAsync(EnvelopeEnabled(), mode);

        Assert.Equal(1, fallback.GraphQlRequests);
        Assert.Equal(1, fallback.Count(MetadataRequest));
        Assert.Equal(1, fallback.Count(LanguagesRequest));
        Assert.Equal(1, fallback.Count(GitRefRequest));
        Assert.Equal(1, fallback.Count(GitCommitRequest));

        Assert.Equal(off.Result.Status, fallback.Result.Status);
        Assert.Equal(FingerprintReport(off.Result), FingerprintReport(fallback.Result));
    }

    // ------------------------ 24. one physical POST, one category count

    [Fact]
    public async Task TheEnvelopePost_CountsOnceInTheExistingGraphQlCategory()
    {
        ProfileExecutionTelemetry telemetry = new();
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler, EnvelopeEnabled());
        using IServiceScope scope = provider.CreateScope();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            await scope.ServiceProvider
                .GetRequiredService<IProfileRepositorySnapshotSource>()
                .CollectAsync(Reference());
        }

        // One POST served THREE surfaces and is still counted exactly once.
        Assert.Equal(1, handler.Count("/graphql"));
        Assert.Equal(1, telemetry.RequestCategories[ProfileGitHubRequestCategory.GraphQl]);
        Assert.Equal(handler.Paths.Count, telemetry.GitHubRequests);
        Assert.All(
            telemetry.RequestCategories.Keys,
            key => Assert.Contains(key, ProfileGitHubRequestCategory.All));
    }

    // ------------------------------- 23. the two budgets stay separate

    // A GraphQL rate-limit header on the envelope response reaches no REST
    // budget state. The §10.4 / §11.3 decision still reads the REST
    // profile-metadata response and nothing else.
    [Fact]
    public async Task AGraphQlRateLimitHeader_NeverReachesTheRestBudgetDecision()
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        using ServiceProvider provider = BuildProvider(
            out FixtureHandler handler,
            EnvelopeEnabled(),
            GraphQlEnvelopeMode.Accepted,
            graphQlRateLimitHeaders: true);
        using IServiceScope scope = provider.CreateScope();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            await scope.ServiceProvider
                .GetRequiredService<IProfileRepositorySnapshotSource>()
                .CollectAsync(Reference());
        }

        // The repository path never observes the REST metadata budget: only the
        // profile-metadata request does, and this run never issued one.
        Assert.Null(budgetState.MetadataObservation);
        Assert.Equal(1, handler.Count("/graphql"));
        Assert.Equal(1, telemetry.RequestCategories[ProfileGitHubRequestCategory.GraphQl]);
    }

    // --------------------------- 25. bounded envelope telemetry (§18.9)

    [Fact]
    public void Telemetry_RecordsEnvelopeOutcomesWithoutTouchingTheRequestCount()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordGitHubRequest(ProfileGitHubRequestCategory.GraphQl);
        telemetry.RecordRepositoryEnvelope(served: 7, fallback: 4, disabledForRun: true);

        // Eleven envelope outcomes, still one physical request. Envelope counts
        // are not HTTP request counts and are never added to them.
        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Equal(7, telemetry.RepositoryEnvelopeServed);
        Assert.Equal(4, telemetry.RepositoryEnvelopeFallback);
        Assert.True(telemetry.GraphQlDisabledForRun);
    }

    [Fact]
    public void Telemetry_RendersNoEnvelopeSectionWhenTheEnvelopeNeverRan()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordGitHubRequest(ProfileGitHubRequestCategory.RepositoryLanguages);
        telemetry.Complete(TimeSpan.FromMilliseconds(1), "Complete", ProfileExecutionOutcome.Composed);

        // Default-off executions log exactly what they logged before S3.
        Assert.False(telemetry.HasRepositoryEnvelopeObservations);
        Assert.DoesNotContain("RepositoryEnvelope", telemetry.ToLogMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void Telemetry_EnvelopeSectionCarriesOnlyCountsAndTheDisableFlag()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRepositoryEnvelope(2, 1, disabledForRun: true);
        telemetry.Complete(TimeSpan.FromMilliseconds(1), "Complete", ProfileExecutionOutcome.Composed);

        string log = telemetry.ToLogMessage();

        Assert.Contains(
            "RepositoryEnvelope={Served=2;Fallback=1;GraphQlDisabledForRun=True}",
            log,
            StringComparison.Ordinal);

        // No identity, topic, language, payload or error text can reach it — the
        // recorder accepts nothing but two integers and a boolean.
        Assert.All(
            typeof(ProfileExecutionTelemetry)
                .GetMethod(nameof(ProfileExecutionTelemetry.RecordRepositoryEnvelope))!
                .GetParameters()
                .Select(parameter => parameter.ParameterType),
            type => Assert.Contains(type, new[] { typeof(int), typeof(bool) }));

        foreach (string forbidden in
            new[] { "octocat", "hello-world", "C#", "JavaScript", "dotnet", "query", "repository(" })
        {
            Assert.DoesNotContain(forbidden, log, StringComparison.OrdinalIgnoreCase);
        }
    }

    // The run-scoped disable is SHARED, so it must not make the other side's
    // section appear. With the envelope enabled the shadow is never constructed,
    // and an execution that disables GraphQL must not log a LanguagesShadow
    // section for observations that never happened.
    [Fact]
    public void Telemetry_LogsNoShadowSectionWhenOnlyTheEnvelopeRan()
    {
        ProfileExecutionTelemetry telemetry = new();

        // Exactly what LiveProfileReportService records for an envelope-only run
        // whose first repository failed at envelope level.
        telemetry.RecordLanguagesShadow(0, 0, 0, disabledForRun: true);
        telemetry.RecordRepositoryEnvelope(0, 3, disabledForRun: true);
        telemetry.Complete(TimeSpan.FromMilliseconds(1), "Complete", ProfileExecutionOutcome.Composed);

        string log = telemetry.ToLogMessage();

        Assert.DoesNotContain("LanguagesShadow", log, StringComparison.Ordinal);
        Assert.Contains(
            "RepositoryEnvelope={Served=0;Fallback=3;GraphQlDisabledForRun=True}",
            log,
            StringComparison.Ordinal);

        // The flag itself is still true: it is the one §18.6 disable, and only
        // the SECTION is attributed to the side that actually observed.
        Assert.True(telemetry.GraphQlDisabledForRun);
    }

    // And the mirror image: a shadow-only run logs no envelope section.
    [Fact]
    public void Telemetry_LogsNoEnvelopeSectionWhenOnlyTheShadowRan()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordLanguagesShadow(0, 0, 2, disabledForRun: true);
        telemetry.RecordRepositoryEnvelope(0, 0, disabledForRun: true);
        telemetry.Complete(TimeSpan.FromMilliseconds(1), "Complete", ProfileExecutionOutcome.Composed);

        string log = telemetry.ToLogMessage();

        Assert.DoesNotContain("RepositoryEnvelope", log, StringComparison.Ordinal);
        Assert.Contains(
            "LanguagesShadow={Match=0;Mismatch=0;NotCompared=2;GraphQlDisabledForRun=True}",
            log,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Telemetry_IgnoresANegativeEnvelopeCount()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRepositoryEnvelope(-1, 0, disabledForRun: false);

        Assert.False(telemetry.HasRepositoryEnvelopeObservations);
    }

    // Both recorders read the ONE §18.6 run-scoped disable, so neither may clear
    // what the other saw — whichever order the execution records them in.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Telemetry_KeepsTheDisableFlagWhicheverRecorderSeesItFirst(bool envelopeFirst)
    {
        ProfileExecutionTelemetry telemetry = new();

        if (envelopeFirst)
        {
            telemetry.RecordRepositoryEnvelope(0, 1, disabledForRun: true);
            telemetry.RecordLanguagesShadow(0, 0, 0, disabledForRun: false);
        }
        else
        {
            telemetry.RecordLanguagesShadow(0, 0, 0, disabledForRun: false);
            telemetry.RecordRepositoryEnvelope(0, 1, disabledForRun: true);
        }

        Assert.True(telemetry.GraphQlDisabledForRun);
    }

    // -------------------------------------------------------- test support

    public enum GraphQlEnvelopeMode
    {
        Accepted,
        Errored,
        IncompleteLanguages,
        Malformed,
        EnvelopeFailure
    }

    private static ProfileRepositoryReference Reference() =>
        new("octocat", "hello-world", new Uri(RepositoryUrl));

    private static async Task<ProfileRun> RunProfileAsync(
        IConfiguration? configuration,
        GraphQlEnvelopeMode mode = GraphQlEnvelopeMode.Accepted)
    {
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler, configuration, mode);
        using IServiceScope scope = provider.CreateScope();

        HttpClient client = scope.ServiceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName);

        GitHubProfileSnapshotResult profile = await GitHubProfileSnapshotOrchestrator
            .CreateDefault(client, TimeProvider.System)
            .CollectAsync("octocat");

        Assert.NotNull(profile.Snapshot);

        ProfilePipelineResult result = await scope.ServiceProvider
            .GetRequiredService<ProfilePipeline>()
            .RunAsync(profile.Snapshot!, new AnalysisVersion("test-analyzer", "test-rules", 1));

        return new ProfileRun(result, handler.Paths);
    }

    private sealed record ProfileRun(
        ProfilePipelineResult Result,
        IReadOnlyList<string> Paths)
    {
        public int GraphQlRequests => Count("/graphql");

        public int Count(string path) =>
            Paths.Count(request => request == path);
    }

    // Every section of the report, by key, availability and item count, plus the
    // profile-level warning and limitation codes. Nothing here is derived from
    // wall-clock time, so two runs are directly comparable (§19.2).
    private static string FingerprintReport(ProfilePipelineResult result)
    {
        ProfileReport report = result.Report!;

        return string.Join(
            "\n",
            $"status={result.Status}",
            $"analysis={result.Profile!.AnalysisStatus}",
            Section("summary", report.PublicProfileSummary.Availability, report.PublicProfileSummary.Items.Count),
            Section("focus", report.TechnicalFocusAreas.Availability, report.TechnicalFocusAreas.Items.Count),
            Section("practice", report.EngineeringPracticePatterns.Availability, report.EngineeringPracticePatterns.Items.Count),
            Section("problem", report.ProblemAreaPatterns.Availability, report.ProblemAreaPatterns.Items.Count),
            Section("representatives", report.RepresentativeRepositories.Availability, report.RepresentativeRepositories.Items.Count),
            Section("why", report.WhyThisConclusion.Availability, report.WhyThisConclusion.Items.Count),
            Section("limitations", report.Limitations.Availability, report.Limitations.Items.Count),
            Section("drilldown", report.RepositoryDrillDown.Availability, report.RepositoryDrillDown.Items.Count),
            "warnings=" + string.Join("|", report.Warnings.Select(warning => warning.Code)),
            "limitationCodes=" + string.Join(
                "|",
                report.Limitations.Items.Select(limitation => limitation.Code)),
            "representativeNames=" + string.Join(
                "|",
                report.RepresentativeRepositories.Items.Select(item => item.Repository.FullName)),
            "summaryFacts=" + string.Join(
                "|",
                report.PublicProfileSummary.Items.Select(fact =>
                    $"{fact.ReferencedSection}:{fact.ReferencedSectionAvailability}:{fact.ItemCount}")));
    }

    private static string Section(
        string name,
        ProfileReportSectionAvailability availability,
        int items) =>
        $"{name}={availability}:{items}";

    private static IConfiguration EnvelopeEnabled() =>
        Configure(envelope: true, shadow: false);

    private static IConfiguration ShadowEnabled() =>
        Configure(envelope: false, shadow: true);

    private static IConfiguration BothEnabled() =>
        Configure(envelope: true, shadow: true);

    private static IConfiguration Configure(bool envelope, bool shadow) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ProfileGraphQlHybridOptions.ConfigurationSectionName}:"
                    + nameof(ProfileGraphQlHybridOptions.RepositoryEnvelopeEnabled)] =
                    envelope ? "true" : "false",
                [$"{ProfileGraphQlShadowOptions.ConfigurationSectionName}:"
                    + nameof(ProfileGraphQlShadowOptions.LanguagesShadowEnabled)] =
                    shadow ? "true" : "false"
            })
            .Build();

    private static ServiceProvider BuildProvider(
        out FixtureHandler handler,
        IConfiguration? configuration = null,
        GraphQlEnvelopeMode mode = GraphQlEnvelopeMode.Accepted,
        bool graphQlRateLimitHeaders = false)
    {
        FixtureHandler fixture = new(mode, graphQlRateLimitHeaders);
        ServiceCollection services = new();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddScoped<IGitHubRepositorySnapshotCollector, NeverCalledSnapshotCollector>();
        services.AddProfilePipeline(configuration);
        services
            .AddHttpClient(ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => fixture);

        handler = fixture;

        return services.BuildServiceProvider(validateScopes: true);
    }

    // The smallest GitHub the Profile path needs: available profile metadata and
    // manifest, available repository metadata and languages, an empty git
    // repository so the contents collector short-circuits, and deterministic
    // empty list endpoints.
    private sealed class FixtureHandler(
        GraphQlEnvelopeMode mode,
        bool graphQlRateLimitHeaders) : HttpMessageHandler
    {
        private readonly List<string> paths = [];

        public IReadOnlyList<string> Paths
        {
            get
            {
                lock (paths)
                {
                    return [.. paths];
                }
            }
        }

        public int Count(string path) => Paths.Count(request => request == path);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path = request.RequestUri!.AbsolutePath;

            lock (paths)
            {
                paths.Add(path);
            }

            return Task.FromResult(Route(path));
        }

        private HttpResponseMessage Route(string path)
        {
            const string prefix = "/repos/octocat/hello-world";

            if (path == "/graphql")
            {
                HttpResponseMessage response = mode == GraphQlEnvelopeMode.EnvelopeFailure
                    ? Json("{ }", HttpStatusCode.BadGateway)
                    : Json(GraphQlBody());

                if (graphQlRateLimitHeaders)
                {
                    response.Headers.TryAddWithoutValidation("x-ratelimit-remaining", "4321");
                    response.Headers.TryAddWithoutValidation("x-ratelimit-limit", "5000");
                    response.Headers.TryAddWithoutValidation("x-ratelimit-resource", "graphql");
                }

                return response;
            }

            if (path == "/users/octocat")
            {
                return Json("""
                    {
                      "login": "octocat",
                      "type": "User",
                      "name": "The Octocat",
                      "created_at": "2020-01-01T00:00:00Z",
                      "public_repos": 1,
                      "avatar_url": "https://avatars.githubusercontent.com/u/1"
                    }
                    """);
            }

            if (path == "/users/octocat/repos")
            {
                return Json("""
                    [
                      {
                        "name": "hello-world",
                        "owner": { "login": "octocat" },
                        "private": false,
                        "fork": false,
                        "is_template": false,
                        "archived": false,
                        "size": 120,
                        "language": "C#",
                        "default_branch": "main",
                        "pushed_at": "2026-08-01T00:00:00Z",
                        "created_at": "2024-01-01T00:00:00Z",
                        "description": "A deterministic fixture repository."
                      }
                    ]
                    """);
            }

            if (path == prefix)
            {
                return Json("""
                    {
                      "name": "hello-world",
                      "owner": { "login": "octocat" },
                      "private": false,
                      "archived": false,
                      "fork": false,
                      "is_template": false,
                      "language": "C#",
                      "default_branch": "main",
                      "description": "A deterministic fixture repository.",
                      "created_at": "2024-01-01T00:00:00Z",
                      "has_issues": true,
                      "topics": ["dotnet"]
                    }
                    """);
            }

            if (path == $"{prefix}/languages")
            {
                return Json("""{ "C#": 1200, "JavaScript": 300 }""");
            }

            // Session 129. The fixture repository is NON-EMPTY, and its REST
            // identity chain carries exactly the two SHAs the GraphQL envelope
            // carries — which is what makes an off/on comparison a mapping
            // proof rather than a coincidence (§19.17).
            if (path == $"{prefix}/git/ref/heads/main")
            {
                return Json($$"""{ "object": { "sha": "{{HeadCommitSha}}" } }""");
            }

            if (path == $"{prefix}/git/commits/{HeadCommitSha}")
            {
                return Json($$"""{ "tree": { "sha": "{{RootTreeSha}}" } }""");
            }

            if (path == $"{prefix}/git/trees/{RootTreeSha}")
            {
                return Json("""
                    {
                      "truncated": false,
                      "tree": [
                        { "path": "README.md", "type": "blob", "mode": "100644", "size": 42 }
                      ]
                    }
                    """);
            }

            if (path.EndsWith("/commits", StringComparison.Ordinal)
                || path.EndsWith("/issues", StringComparison.Ordinal)
                || path.EndsWith("/pulls", StringComparison.Ordinal)
                || path.EndsWith("/releases", StringComparison.Ordinal)
                || path.EndsWith("/tags", StringComparison.Ordinal))
            {
                return Json("[]");
            }

            return Json("""{ "message": "Not Found" }""", HttpStatusCode.NotFound);
        }

        // The accepted body mirrors the REST metadata and languages responses
        // above field for field. The fallback bodies differ from it in exactly
        // one way each, so a test names the reason it fell back.
        // The one head commit and root tree this fixture repository has.
        public const string HeadCommitSha = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b";
        public const string RootTreeSha = "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a";

        private string GraphQlBody() => mode switch
        {
            GraphQlEnvelopeMode.Malformed => """{ "unexpected": true }""",

            GraphQlEnvelopeMode.Errored => """
                {
                  "data": { "repository": null },
                  "errors": [ { "message": "Something went wrong." } ]
                }
                """,

            GraphQlEnvelopeMode.IncompleteLanguages => IncompleteLanguagesBody,

            _ => AcceptedBody
        };

        // The languages connection is the SECOND one in the envelope, so the
        // last complete pageInfo is the one to flip. Built by position because
        // both connections carry the same line.
        private static readonly string IncompleteLanguagesBody = Flip(AcceptedBody);

        private static string Flip(string body)
        {
            const string complete = """{ "hasNextPage": false }""";
            int index = body.LastIndexOf(complete, StringComparison.Ordinal);

            return body[..index]
                + """{ "hasNextPage": true }"""
                + body[(index + complete.Length)..];
        }

        private const string AcceptedBody = """
            {
              "data": {
                "repository": {
                  "name": "hello-world",
                  "owner": { "login": "octocat" },
                  "isPrivate": false,
                  "isArchived": false,
                  "isFork": false,
                  "isTemplate": false,
                  "description": "A deterministic fixture repository.",
                  "homepageUrl": null,
                  "createdAt": "2024-01-01T00:00:00Z",
                  "hasIssuesEnabled": true,
                  "primaryLanguage": { "name": "C#" },
                  "defaultBranchRef": {
                    "name": "main",
                    "target": {
                      "oid": "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b",
                      "tree": { "oid": "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a" }
                    }
                  },
                  "repositoryTopics": {
                    "pageInfo": { "hasNextPage": false },
                    "nodes": [ { "topic": { "name": "dotnet" } } ]
                  },
                  "languages": {
                    "pageInfo": { "hasNextPage": false },
                    "edges": [
                      { "size": 1200, "node": { "name": "C#" } },
                      { "size": 300, "node": { "name": "JavaScript" } }
                    ]
                  }
                }
              }
            }
            """;

        private static HttpResponseMessage Json(
            string content,
            HttpStatusCode statusCode = HttpStatusCode.OK) =>
            new(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
    }

    private sealed class NeverCalledSnapshotCollector : IGitHubRepositorySnapshotCollector
    {
        public Task<GitHubRepositorySnapshotResult> CollectAsync(
            string repositoryUrl,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "The Repository Proof collector must never be reached from the profile path.");
    }
}

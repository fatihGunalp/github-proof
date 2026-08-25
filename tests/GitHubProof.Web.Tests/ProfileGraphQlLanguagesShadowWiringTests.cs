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

// Session 125 — S2 wiring proofs for
// docs/architecture/public-profile-runtime-collection.md §§18-19.
//
// Two properties matter here and nothing else: the shadow is OFF unless
// configuration says otherwise, and OFF means the Profile path is exactly what
// it is today — not "a shadow that decides to do nothing", but no shadow at all.
public sealed class ProfileGraphQlLanguagesShadowWiringTests
{
    private const string RepositoryUrl = "https://github.com/octocat/hello-world";

    // ------------------------------------------------------- A. default is off

    [Fact]
    public void LanguagesShadow_IsDisabledByDefault()
    {
        using ServiceProvider provider = BuildProvider(out _);

        ProfileGraphQlShadowOptions options = provider
            .GetRequiredService<IOptions<ProfileGraphQlShadowOptions>>()
            .Value;

        Assert.False(options.LanguagesShadowEnabled);
    }

    [Fact]
    public void LanguagesShadow_StaysDisabledWhenConfigurationSaysNothingAboutIt()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProfileExecutionDeadline:Timeout"] = "00:03:00"
            })
            .Build();
        using ServiceProvider provider = BuildProvider(out _, configuration);

        Assert.False(provider
            .GetRequiredService<IOptions<ProfileGraphQlShadowOptions>>()
            .Value
            .LanguagesShadowEnabled);
    }

    // ------------------------------------------- B. default-off issues nothing

    [Fact]
    public async Task DefaultOff_IssuesZeroGraphQlRequestsOnTheProductionPath()
    {
        using ServiceProvider provider = BuildProvider(out FixtureHandler handler);
        using IServiceScope scope = provider.CreateScope();

        IProfileRepositorySnapshotSource source =
            scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>();

        await source.CollectAsync(Reference());

        Assert.NotEmpty(handler.Paths);
        Assert.All(
            handler.Paths,
            path => Assert.DoesNotContain("/graphql", path, StringComparison.Ordinal));

        // The REST languages request still happened, so the absence above is
        // about the shadow, not about a collection that never ran.
        Assert.Contains(
            handler.Paths,
            path => path.EndsWith("/languages", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DefaultOff_LeavesTheCollectedLanguagesUntouched()
    {
        using ServiceProvider provider = BuildProvider(out _);
        using IServiceScope scope = provider.CreateScope();

        ProfileRepositorySnapshotResult result = await scope.ServiceProvider
            .GetRequiredService<IProfileRepositorySnapshotSource>()
            .CollectAsync(Reference());

        Assert.True(result.IsSuccessful);
        Assert.Equal(
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 },
            result.Snapshot!.Languages);
    }

    // ------------------------------------- enabled wires the real shadow in

    [Fact]
    public async Task Enabled_RunsTheShadowAndStillReturnsTheRestLanguages()
    {
        using ServiceProvider provider = BuildProvider(
            out FixtureHandler handler,
            ShadowEnabled());
        using IServiceScope scope = provider.CreateScope();

        ProfileRepositorySnapshotResult result = await scope.ServiceProvider
            .GetRequiredService<IProfileRepositorySnapshotSource>()
            .CollectAsync(Reference());

        Assert.Contains(
            handler.Paths,
            path => path.EndsWith("/graphql", StringComparison.Ordinal));

        // The observation ran; the answer is still REST's.
        Assert.True(result.IsSuccessful);
        Assert.Equal(
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 },
            result.Snapshot!.Languages);
    }

    // ------------------------------- P/Q. the Profile result cannot drift

    // The headline claim of S2, proven end to end rather than inferred: the same
    // profile input through the real pipeline with the shadow OFF and with the
    // shadow ON produces the same ProfileReport, the same status, the same
    // warnings and the same limitations. The shadow observed and changed nothing.
    [Fact]
    public async Task ShadowOffAndOn_ProduceTheIdenticalProfileReport()
    {
        ProfileRun off = await RunProfileAsync(configuration: null);
        ProfileRun on = await RunProfileAsync(ShadowEnabled());

        // Not a vacuous comparison: the OFF run issued no GraphQL request and
        // the ON run really did observe the surface.
        Assert.Equal(0, off.GraphQlRequests);
        Assert.Equal(1, on.GraphQlRequests);

        Assert.Equal(off.Result.Status, on.Result.Status);
        Assert.True(off.Result.HasProfile);
        Assert.True(on.Result.HasProfile);
        Assert.Equal(
            off.Result.Profile!.AnalysisStatus,
            on.Result.Profile!.AnalysisStatus);
        Assert.Equal(FingerprintReport(off.Result), FingerprintReport(on.Result));
    }

    // A mismatching shadow is still an observation. The fixture below serves a
    // GraphQL map that disagrees with REST on purpose; the profile result must
    // be the same as if no shadow had run at all.
    [Fact]
    public async Task AMismatchingShadow_ProducesTheIdenticalProfileReport()
    {
        ProfileRun off = await RunProfileAsync(configuration: null);
        ProfileRun mismatching = await RunProfileAsync(
            ShadowEnabled(),
            disagreeingLanguages: true);

        Assert.Equal(1, mismatching.GraphQlRequests);
        Assert.Equal(off.Result.Status, mismatching.Result.Status);
        Assert.Equal(
            FingerprintReport(off.Result),
            FingerprintReport(mismatching.Result));
    }

    // --------------------------------- L. one physical POST, one category count

    // Two GraphQL pages are two physical POSTs and therefore two `graphql`
    // requests — never one per language and never one per surface (§18.9
    // counting rule).
    [Fact]
    public async Task EachGraphQlPage_CountsOnceInTheExistingGraphQlCategory()
    {
        ProfileExecutionTelemetry telemetry = new();
        using ServiceProvider provider = BuildProvider(
            out FixtureHandler handler,
            ShadowEnabled(),
            paginateLanguages: true);
        using IServiceScope scope = provider.CreateScope();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            await scope.ServiceProvider
                .GetRequiredService<IProfileRepositorySnapshotSource>()
                .CollectAsync(Reference());
        }

        int graphQlPosts = handler.Paths.Count(path =>
            path.EndsWith("/graphql", StringComparison.Ordinal));

        Assert.Equal(2, graphQlPosts);
        Assert.Equal(
            graphQlPosts,
            telemetry.RequestCategories[ProfileGitHubRequestCategory.GraphQl]);
        Assert.Equal(handler.Paths.Count, telemetry.GitHubRequests);
        Assert.All(
            telemetry.RequestCategories.Keys,
            key => Assert.Contains(key, ProfileGitHubRequestCategory.All));
    }

    // ------------------------------- F-2. bounded shadow telemetry (§18.9)

    [Fact]
    public void Telemetry_RecordsShadowOutcomesWithoutTouchingTheRequestCount()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordGitHubRequest(ProfileGitHubRequestCategory.GraphQl);
        telemetry.RecordLanguagesShadow(
            match: 3,
            mismatch: 2,
            notCompared: 5,
            disabledForRun: true);

        // Ten shadow outcomes, still one physical request. Shadow counts are not
        // HTTP request counts and are never added to them.
        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Equal(3, telemetry.LanguagesShadowMatch);
        Assert.Equal(2, telemetry.LanguagesShadowMismatch);
        Assert.Equal(5, telemetry.LanguagesShadowNotCompared);
        Assert.True(telemetry.GraphQlDisabledForRun);
    }

    [Fact]
    public void Telemetry_RendersNoShadowSectionWhenTheShadowNeverRan()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordGitHubRequest(ProfileGitHubRequestCategory.RepositoryLanguages);
        telemetry.Complete(TimeSpan.FromMilliseconds(1), "Complete", ProfileExecutionOutcome.Composed);

        // Default-off executions log exactly what they logged before S2.
        Assert.False(telemetry.HasLanguagesShadowObservations);
        Assert.DoesNotContain("LanguagesShadow", telemetry.ToLogMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void Telemetry_ShadowSectionCarriesOnlyCountsAndTheDisableFlag()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordLanguagesShadow(1, 1, 1, disabledForRun: true);
        telemetry.Complete(TimeSpan.FromMilliseconds(1), "Complete", ProfileExecutionOutcome.Composed);

        string log = telemetry.ToLogMessage();

        Assert.Contains(
            "LanguagesShadow={Match=1;Mismatch=1;NotCompared=1;GraphQlDisabledForRun=True}",
            log,
            StringComparison.Ordinal);

        // No identity, no language, no payload, no error text can reach it —
        // the recorder accepts nothing but three integers and a boolean.
        Assert.All(
            typeof(ProfileExecutionTelemetry)
                .GetMethod(nameof(ProfileExecutionTelemetry.RecordLanguagesShadow))!
                .GetParameters()
                .Select(parameter => parameter.ParameterType),
            type => Assert.Contains(type, new[] { typeof(int), typeof(bool) }));

        foreach (string forbidden in new[] { "octocat", "hello-world", "C#", "JavaScript", "query", "repository(" })
        {
            Assert.DoesNotContain(forbidden, log, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Telemetry_IgnoresANegativeShadowCount()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordLanguagesShadow(-1, 0, 0, disabledForRun: false);

        Assert.False(telemetry.HasLanguagesShadowObservations);
    }

    // -------------------------------------------------------- test support

    private static ProfileRepositoryReference Reference() =>
        new("octocat", "hello-world", new Uri(RepositoryUrl));

    private static async Task<ProfileRun> RunProfileAsync(
        IConfiguration? configuration,
        bool disagreeingLanguages = false)
    {
        using ServiceProvider provider = BuildProvider(
            out FixtureHandler handler,
            configuration,
            disagreeingLanguages: disagreeingLanguages);
        using IServiceScope scope = provider.CreateScope();

        HttpClient client = scope.ServiceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName);

        GitHubProfileSnapshotResult profile =
            await GitHubProfileSnapshotOrchestrator
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
        public int GraphQlRequests => Paths.Count(path =>
            path.EndsWith("/graphql", StringComparison.Ordinal));
    }

    // Every section of the report, by key, availability and item count, plus the
    // profile-level warning and limitation codes. Nothing here is derived from
    // wall-clock time, so two runs are directly comparable.
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

    private static IConfiguration ShadowEnabled() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ProfileGraphQlShadowOptions.ConfigurationSectionName}:"
                    + nameof(ProfileGraphQlShadowOptions.LanguagesShadowEnabled)] = "true"
            })
            .Build();

    private static ServiceProvider BuildProvider(
        out FixtureHandler handler,
        IConfiguration? configuration = null,
        bool paginateLanguages = false,
        bool disagreeingLanguages = false)
    {
        FixtureHandler fixture = new(paginateLanguages, disagreeingLanguages);
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

    // The smallest GitHub the descriptive collector needs to reach `languages`
    // successfully: available metadata, an empty git repository so the contents
    // collector short-circuits, and deterministic empty list endpoints.
    private sealed class FixtureHandler(
        bool paginateLanguages,
        bool disagreeingLanguages = false) : HttpMessageHandler
    {
        private readonly List<string> paths = [];
        private int graphQlCalls;

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
                return Json(GraphQlPage(Interlocked.Increment(ref graphQlCalls)));
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
                      "created_at": "2024-01-01T00:00:00Z",
                      "has_issues": true,
                      "topics": []
                    }
                    """);
            }

            if (path == $"{prefix}/languages")
            {
                return Json("""{ "C#": 1200, "JavaScript": 300 }""");
            }

            if (path.StartsWith($"{prefix}/git/ref/", StringComparison.Ordinal))
            {
                return Json(
                    """{ "message": "Git Repository is empty." }""",
                    HttpStatusCode.Conflict);
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

        // Page 1 carries C# and promises another page; page 2 carries JavaScript
        // and ends the connection. Together they are the complete REST map.
        private string GraphQlPage(int call) =>
            disagreeingLanguages
                ? """
                  {
                    "data": { "repository": { "languages": {
                      "pageInfo": { "hasNextPage": false, "endCursor": null },
                      "edges": [ { "size": 999999, "node": { "name": "Rust" } } ]
                    } } }
                  }
                  """
                : !paginateLanguages
                ? """
                  {
                    "data": { "repository": { "languages": {
                      "pageInfo": { "hasNextPage": false, "endCursor": null },
                      "edges": [
                        { "size": 1200, "node": { "name": "C#" } },
                        { "size": 300, "node": { "name": "JavaScript" } }
                      ]
                    } } }
                  }
                  """
                : call == 1
                    ? """
                      {
                        "data": { "repository": { "languages": {
                          "pageInfo": { "hasNextPage": true, "endCursor": "cursor-1" },
                          "edges": [ { "size": 1200, "node": { "name": "C#" } } ]
                        } } }
                      }
                      """
                    : """
                      {
                        "data": { "repository": { "languages": {
                          "pageInfo": { "hasNextPage": false, "endCursor": null },
                          "edges": [ { "size": 300, "node": { "name": "JavaScript" } } ]
                        } } }
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

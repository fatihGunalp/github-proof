using System.Net;
using System.Reflection;
using System.Text;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 124 — S1 wiring proofs for
// docs/architecture/public-profile-runtime-collection.md §§18-19.
//
// S1's defining property is what does NOT happen: the GraphQL transport is
// registered and resolvable, and no collector, no snapshot source and no
// production collection path uses it. These tests exist so that property cannot
// regress silently into S2.
public sealed class ProfileGraphQlTransportWiringTests
{
    private const string RepositoryUrl = "https://github.com/octocat/hello-world";

    // ------------------------------------------------------- registration only

    [Fact]
    public void AddProfilePipeline_ResolvesTheProductionGraphQlTransport()
    {
        using ServiceProvider provider = BuildProvider(out _);
        using IServiceScope scope = provider.CreateScope();

        IGitHubGraphQlTransport transport =
            scope.ServiceProvider.GetRequiredService<IGitHubGraphQlTransport>();

        Assert.IsType<GitHubGraphQlTransport>(transport);
    }

    // The transport shares the existing profile named client rather than adding
    // a second GitHub client or a second credential source.
    [Fact]
    public async Task GraphQlTransport_UsesTheExistingProfileNamedClient()
    {
        using ServiceProvider provider = BuildProvider(out RecordingHandler handler);
        using IServiceScope scope = provider.CreateScope();

        IGitHubGraphQlTransport transport =
            scope.ServiceProvider.GetRequiredService<IGitHubGraphQlTransport>();

        await transport.SendAsync(new GitHubGraphQlRequest("query { viewer { login } }"));

        Assert.Equal(
            "https://api.github.com/graphql",
            Assert.Single(handler.RequestPaths, path => path.EndsWith("/graphql", StringComparison.Ordinal)));
    }

    // ------------------------------------- K. no collector uses GraphQL in S1

    // Behavioural: the production-wired profile snapshot source starts on REST
    // and never touches /graphql. Metadata answers 404 so the collection stops
    // immediately — the point is which endpoint was addressed, not how far the
    // collection got.
    [Fact]
    public async Task ProfileSnapshotSource_IssuesRestRequestsAndNeverGraphQl()
    {
        using ServiceProvider provider = BuildProvider(out RecordingHandler handler);
        using IServiceScope scope = provider.CreateScope();

        IProfileRepositorySnapshotSource source =
            scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>();

        await source.CollectAsync(
            new ProfileRepositoryReference("octocat", "hello-world", new Uri(RepositoryUrl)));

        Assert.NotEmpty(handler.RequestPaths);
        Assert.All(
            handler.RequestPaths,
            path => Assert.DoesNotContain("/graphql", path, StringComparison.Ordinal));
        Assert.Contains(
            handler.RequestPaths,
            path => path.Contains("/repos/octocat/hello-world", StringComparison.Ordinal));
    }

    [Fact]
    public void ProfileSnapshotSource_StillResolvesTheRestComposition()
    {
        using ServiceProvider provider = BuildProvider(out _);
        using IServiceScope scope = provider.CreateScope();

        TelemetryProfileRepositorySnapshotSource recorder =
            Assert.IsType<TelemetryProfileRepositorySnapshotSource>(
                scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>());

        Assert.Equal(
            "GitHubProof.Infrastructure.GitHub.Profile.GitHubProfileRepositorySnapshotSource",
            recorder.Inner.GetType().FullName);
    }

    // Authoritative DI proof: the transport registration is replaced with one
    // that throws, and the whole production graph is then resolved AND
    // exercised. If any component took the dependency — by constructor
    // injection, or by calling GetRequiredService inside a registration factory
    // — construction fails here. Together with the behavioural test above (zero
    // /graphql requests at invocation time), this is what the "nothing uses it"
    // claim rests on.
    [Fact]
    public async Task ProductionPipelineGraph_NeverResolvesTheGraphQlTransport()
    {
        ServiceCollection services = new();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddScoped<IGitHubRepositorySnapshotCollector, NeverCalledSnapshotCollector>();
        services.AddProfilePipeline();
        services
            .AddHttpClient(ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new RecordingHandler());

        // Last registration wins, so the whole graph now sees a poisoned
        // transport. Resolving it is exactly the failure this test looks for.
        services.AddScoped<IGitHubGraphQlTransport>(
            _ => throw new InvalidOperationException(
                "The production Profile graph must not resolve the GraphQL transport in S1."));

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ProfilePipeline>());

        IProfileRepositorySnapshotSource source =
            scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>();

        await source.CollectAsync(
            new ProfileRepositoryReference("octocat", "hello-world", new Uri(RepositoryUrl)));
    }

    // Supplementary only, and deliberately narrow: this scans constructor
    // parameters of Infrastructure types for the exact transport types. It does
    // not — and is not claimed to — rule out a service-locator call, a
    // Lazy<>/Func<>/IEnumerable<> wrapper, property injection, or a dependency
    // taken outside this assembly. Those are covered by the two tests above.
    [Fact]
    public void NoInfrastructureConstructorTakesTheGraphQlTransport()
    {
        Type[] dependents = typeof(GitHubGraphQlTransport).Assembly
            .GetTypes()
            .Where(type => type != typeof(GitHubGraphQlTransport))
            .Where(type => type
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(constructor => constructor.GetParameters())
                .Any(parameter =>
                    parameter.ParameterType == typeof(IGitHubGraphQlTransport)
                    || parameter.ParameterType == typeof(GitHubGraphQlTransport)))
            .ToArray();

        Assert.Empty(dependents);
    }

    // ------------------------------------- L. Repository Proof stays untouched

    // AddProfilePipeline configures only its own named client. The Repository
    // Proof client name is left exactly as Program.cs configures it — here,
    // unconfigured, because this provider never registers it (§3.1).
    [Fact]
    public void AddProfilePipeline_LeavesTheRepositoryProofClientUnconfigured()
    {
        using ServiceProvider provider = BuildProvider(out _);
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

        Assert.Null(factory.CreateClient("GitHubProof.GitHub").BaseAddress);
        Assert.Equal(
            new Uri("https://api.github.com/"),
            factory
                .CreateClient(ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName)
                .BaseAddress);
    }

    [Fact]
    public void RepositoryProofComposition_TakesNoGraphQlDependency()
    {
        GitHubRepositorySnapshotOrchestrator orchestrator =
            GitHubRepositorySnapshotOrchestrator.CreateDefault(
                new HttpClient { BaseAddress = new Uri("https://api.github.com/") });

        Assert.All(
            orchestrator.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(field => field.FieldType),
            fieldType => Assert.NotEqual(typeof(IGitHubGraphQlTransport), fieldType));
    }

    // ------------------------------------------------ C. one POST, one count

    [Fact]
    public async Task OneGraphQlPost_CountsOnceInTotalAndOnceInTheGraphQlCategory()
    {
        ProfileExecutionTelemetry telemetry = new();
        CountingHandler inner = new(GraphQlBody("""
            {
              "data": {
                "repository": { "nameWithOwner": "octocat/hello-world" },
                "languages": { "edges": [] },
                "commits": { "nodes": [] },
                "releases": { "nodes": [] },
                "tags": { "nodes": [] }
              }
            }
            """));
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        GitHubGraphQlTransport transport = new(client);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            GitHubGraphQlResponse response =
                await transport.SendAsync(new GitHubGraphQlRequest("query { repository { id } }"));

            Assert.Equal(GitHubGraphQlOutcome.Success, response.Outcome);
        }

        // One physical POST serving five fields is still one request and one
        // category increment — never once per surface (§18.9 counting rule).
        Assert.Equal(1, inner.SendCount);
        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Equal(
            1,
            telemetry.RequestCategories[ProfileGitHubRequestCategory.GraphQl]);
        Assert.Equal(
            ProfileGitHubRequestCategory.GraphQl,
            Assert.Single(telemetry.RequestCategories.Keys));
    }

    [Fact]
    public void GraphQlEndpoint_ClassifiesIntoExactlyOneClosedCategory()
    {
        Assert.Equal(
            ProfileGitHubRequestCategory.GraphQl,
            ProfileGitHubRequestCategory.Classify(new Uri("https://api.github.com/graphql")));
        Assert.Equal(
            ProfileGitHubRequestCategory.GraphQl,
            ProfileGitHubRequestCategory.Classify(new Uri("graphql", UriKind.Relative)));

        // Nothing below the endpoint may widen the vocabulary.
        Assert.Equal(
            ProfileGitHubRequestCategory.Other,
            ProfileGitHubRequestCategory.Classify(
                new Uri("https://api.github.com/graphql/octocat")));

        Assert.Contains(
            ProfileGitHubRequestCategory.GraphQl,
            ProfileGitHubRequestCategory.All);
        Assert.Equal(
            ProfileGitHubRequestCategory.All.Count,
            ProfileGitHubRequestCategory.All.Distinct(StringComparer.Ordinal).Count());
    }

    // ---------------------------------------- E. no GraphQL text in telemetry

    // Meaningful because the telemetry surface under test is NOT empty: the
    // response carries rate-limit headers, so a real rate-limit bucket is
    // recorded and rendered alongside the category counters. The sentinels live
    // in the query, the payload and the error message — the three body-derived
    // sources §18.9 forbids — and none may appear anywhere in that rendered
    // surface, in any category key, or in any rate-limit observation.
    [Fact]
    public async Task Telemetry_CarriesNoGraphQlQueryErrorTextOrPayload()
    {
        const string querySentinel = "SENTINELQUERYsecretRepositoryName";
        const string payloadSentinel = "SENTINELPAYLOADoctocatSecretApp";
        const string errorSentinel = "SENTINELERRORcouldNotResolve";

        ProfileExecutionTelemetry telemetry = new();
        CountingHandler inner = new(
            GraphQlBody($$"""
                {
                  "data": { "repository": { "nameWithOwner": "{{payloadSentinel}}" } },
                  "errors": [ { "message": "{{errorSentinel}}", "path": ["repository"] } ]
                }
                """),
            remaining: "4987",
            resource: "graphql");
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        GitHubGraphQlTransport transport = new(client);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            GitHubGraphQlResponse response = await transport.SendAsync(
                new GitHubGraphQlRequest($"query {{ {querySentinel} {{ id }} }}"));

            Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        }

        telemetry.Complete(
            TimeSpan.FromMilliseconds(1),
            "Complete",
            ProfileExecutionOutcome.Composed);

        string log = telemetry.ToLogMessage();

        // The surface really did record something — one request, one graphql
        // category and one rate-limit bucket — so the assertions below are not
        // vacuous.
        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Contains("graphql", log, StringComparison.Ordinal);
        Assert.NotEmpty(telemetry.RateLimitWindows);

        foreach (string sentinel in new[] { querySentinel, payloadSentinel, errorSentinel })
        {
            Assert.DoesNotContain(sentinel, log, StringComparison.Ordinal);
            Assert.All(
                telemetry.RequestCategories.Keys,
                key => Assert.DoesNotContain(sentinel, key, StringComparison.Ordinal));
            Assert.All(
                telemetry.RateLimitWindows,
                window => Assert.DoesNotContain(
                    sentinel,
                    window.ToString(),
                    StringComparison.Ordinal));
        }

        // The vocabulary stayed closed: nothing body-derived became a key.
        Assert.All(
            telemetry.RequestCategories.Keys,
            key => Assert.Contains(key, ProfileGitHubRequestCategory.All));
    }

    // ------------------------------- M. the budget decision stays REST-only

    [Fact]
    public async Task GraphQlResponse_DoesNotFeedTheProfileMetadataBudgetDecision()
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        CountingHandler inner = new(
            GraphQlBody("""{ "data": { "viewer": {} } }"""),
            remaining: "12");
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        GitHubGraphQlTransport transport = new(client);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            await transport.SendAsync(new GitHubGraphQlRequest("query { viewer { login } }"));
        }

        // The GraphQL response carried a well-formed remaining header and is
        // still not the profile-metadata observation the §10.4 / §11.3 decision
        // reads. The two provider budgets stay separate (§18.9).
        Assert.Null(budgetState.MetadataObservation);
        Assert.Equal(1, telemetry.GitHubRequests);
    }

    [Fact]
    public async Task RestProfileMetadataResponse_StillFeedsTheBudgetDecision()
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        CountingHandler inner = new(
            GraphQlBody("""{ "login": "octocat" }"""),
            remaining: "12");
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            using HttpResponseMessage response = await client.GetAsync("users/octocat");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        Assert.Equal(12, budgetState.MetadataObservation!.Remaining);
    }

    // ------------------------------------------------------------ test support

    private static ServiceProvider BuildProvider(out RecordingHandler handler)
    {
        RecordingHandler recording = new();
        ServiceCollection services = new();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddScoped<IGitHubRepositorySnapshotCollector, NeverCalledSnapshotCollector>();

        services.AddProfilePipeline();
        services
            .AddHttpClient(ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => recording);

        handler = recording;

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static HttpResponseMessage GraphQlBody(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly List<string> requestPaths = [];

        public IReadOnlyList<string> RequestPaths
        {
            get
            {
                lock (requestPaths)
                {
                    return [.. requestPaths];
                }
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            lock (requestPaths)
            {
                requestPaths.Add(request.RequestUri!.AbsoluteUri);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(
                    """{ "message": "Not Found" }""",
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }

    private sealed class CountingHandler(
        HttpResponseMessage response,
        string? remaining = null,
        string? resource = null) : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;

            if (remaining is not null)
            {
                response.Headers.TryAddWithoutValidation(
                    "X-RateLimit-Remaining",
                    remaining);
            }

            if (resource is not null)
            {
                response.Headers.TryAddWithoutValidation(
                    "X-RateLimit-Resource",
                    resource);
            }

            return Task.FromResult(response);
        }
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

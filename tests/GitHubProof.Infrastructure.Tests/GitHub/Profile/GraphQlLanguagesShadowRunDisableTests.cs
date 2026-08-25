using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 125 — §18.6 run-scoped disable, §18.9 bounded shadow telemetry, and
// the two decorator branches the first independent review found untested.
//
// §18.6 exists so a systemic GraphQL failure cannot be multiplied by the
// admitted repository count. These tests drive several repositories through one
// execution scope and count the physical POSTs.
public sealed class GraphQlLanguagesShadowRunDisableTests
{
    private const string Owner = "octocat";

    // ------------------------------------------- §18.6 disable for the run

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]   // EnvelopeFailure
    [InlineData(HttpStatusCode.Forbidden)]      // EnvelopeFailure
    [InlineData(HttpStatusCode.BadGateway)]     // EnvelopeFailure
    public async Task AnEnvelopeFailure_DisablesTheShadowForTheRestOfTheExecution(
        HttpStatusCode statusCode)
    {
        ShadowFixture fixture = new(("""{ "message": "no" }""", statusCode));
        GraphQlLanguagesShadowState state = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            for (int repository = 0; repository < 4; repository++)
            {
                RepositorySnapshot snapshot = await fixture.CollectAsync($"repo-{repository}");

                // 3. REST still supplies every repository, disabled or not.
                Assert.Equal(RestLanguages, snapshot.Languages);
            }
        }

        // 1. The first repository paid exactly one POST.
        // 2. The three that followed paid none.
        Assert.Equal(1, fixture.GraphQlRequests);
        Assert.True(state.IsDisabledForRun);
        Assert.Equal(4, state.NotCompared);
        Assert.Equal(0, state.Match);
        Assert.Equal(0, state.Mismatch);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{ "data": null }""")]
    public async Task AMalformedEnvelope_DisablesTheShadowForTheRestOfTheExecution(string body)
    {
        ShadowFixture fixture = new((body, HttpStatusCode.OK));
        GraphQlLanguagesShadowState state = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            await fixture.CollectAsync("repo-0");
            await fixture.CollectAsync("repo-1");
        }

        Assert.Equal(1, fixture.GraphQlRequests);
        Assert.True(state.IsDisabledForRun);
    }

    // G-1: a client-side transport timeout is an envelope-level failure, so it
    // disables the run exactly like a non-200 status does. Without this, a
    // systemic timeout would be the most expensive shape of the multiplication
    // §18.6 forbids — one full timeout per admitted repository.
    [Fact]
    public async Task AClientTransportTimeout_DisablesTheShadowForTheRestOfTheExecution()
    {
        ShadowFixture fixture = new(
            (MatchingLanguages(), HttpStatusCode.OK),
            timeoutOnGraphQl: true);
        GraphQlLanguagesShadowState state = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            for (int repository = 0; repository < 3; repository++)
            {
                RepositorySnapshot snapshot = await fixture.CollectAsync($"repo-{repository}");

                Assert.Equal(RestLanguages, snapshot.Languages);
            }
        }

        // 2. The timeout became NotCompared and set the disable.
        // 3. The two repositories that followed issued zero GraphQL POSTs.
        Assert.Equal(1, fixture.GraphQlRequests);
        Assert.True(state.IsDisabledForRun);
        Assert.Equal(3, state.NotCompared);
        Assert.Equal(0, state.Match);
        Assert.Equal(0, state.Mismatch);
    }

    // 4. A new execution may attempt GraphQL again after a timeout disable.
    [Fact]
    public async Task AFollowingExecution_MayAttemptGraphQlAgainAfterATimeout()
    {
        ShadowFixture fixture = new(
            (MatchingLanguages(), HttpStatusCode.OK),
            timeoutOnGraphQl: true,
            timeoutCalls: 1);
        GraphQlLanguagesShadowState first = new();
        GraphQlLanguagesShadowState second = new();

        using (GraphQlLanguagesShadowScope.Begin(first))
        {
            await fixture.CollectAsync("repo-0");
            await fixture.CollectAsync("repo-1");
        }

        using (GraphQlLanguagesShadowScope.Begin(second))
        {
            await fixture.CollectAsync("repo-2");
        }

        Assert.True(first.IsDisabledForRun);
        Assert.False(second.IsDisabledForRun);
        Assert.Equal(2, fixture.GraphQlRequests);
        Assert.Equal(1, second.Match);
    }

    // DataWithErrors deliberately does NOT disable the run. §18.4 makes that
    // response unusable for the repository it belongs to, and it stays a
    // per-repository abstention.
    //
    // Session 125 (L-3) pins both shapes the owner ruled on. The second is a
    // null `data` alongside `errors` — a bounded structural fact, and the shape
    // a top-level failure takes, but not proof of a provider-wide fault: an
    // ordinary top-level query error looks identical. Neither disables the run,
    // and a later repository in the same execution still attempts GraphQL.
    [Theory]
    // partial payload alongside errors
    [InlineData("""
        {
          "data": { "repository": { "languages": {
            "pageInfo": { "hasNextPage": false, "endCursor": null },
            "edges": [ { "size": 1200, "node": { "name": "C#" } } ]
          } } },
          "errors": [ { "message": "partial" } ]
        }
        """)]
    // null data alongside errors — the owner-decided envelope
    [InlineData("""
        {
          "data": null,
          "errors": [
            { "type": "RATE_LIMITED", "path": ["repository"], "message": "API rate limit exceeded." }
          ]
        }
        """)]
    public async Task ADataWithErrorsEnvelope_AbstainsWithoutDisablingTheRun(string body)
    {
        ShadowFixture fixture = new((body, HttpStatusCode.OK));
        GraphQlLanguagesShadowState state = new();
        RepositorySnapshot first;
        RepositorySnapshot second;

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            first = await fixture.CollectAsync("repo-0");

            // One repository, one abstention — and no disable from it.
            Assert.Equal(1, state.NotCompared);
            Assert.False(state.IsDisabledForRun);

            second = await fixture.CollectAsync("repo-1");
        }

        // The later repository still attempted, because nothing said the
        // provider is down.
        Assert.Equal(2, fixture.GraphQlRequests);
        Assert.Equal(2, state.NotCompared);
        Assert.Equal(0, state.Match);
        Assert.Equal(0, state.Mismatch);
        Assert.False(state.IsDisabledForRun);

        // REST stayed authoritative for both, and no GraphQL data was consumed.
        Assert.Equal(RestLanguages, first.Languages);
        Assert.Equal(RestLanguages, second.Languages);
    }

    // 4. The disable belongs to one execution. A second scope starts clean and
    // may use GraphQL again — nothing is persisted, shared or remembered.
    [Fact]
    public async Task AFollowingExecution_MayUseGraphQlAgain()
    {
        ShadowFixture fixture = new(
            ("""{ "message": "no" }""", HttpStatusCode.Forbidden),
            (MatchingLanguages(), HttpStatusCode.OK));
        GraphQlLanguagesShadowState first = new();
        GraphQlLanguagesShadowState second = new();

        using (GraphQlLanguagesShadowScope.Begin(first))
        {
            await fixture.CollectAsync("repo-0");
            await fixture.CollectAsync("repo-1");
        }

        using (GraphQlLanguagesShadowScope.Begin(second))
        {
            await fixture.CollectAsync("repo-2");
        }

        Assert.True(first.IsDisabledForRun);
        Assert.False(second.IsDisabledForRun);

        // One POST in the first execution, one in the second.
        Assert.Equal(2, fixture.GraphQlRequests);
        Assert.Equal(1, second.Match);
    }

    [Fact]
    public void TheDisableDoesNotSurviveItsScope()
    {
        GraphQlLanguagesShadowState state = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            Assert.Same(state, GraphQlLanguagesShadowScope.Current);
            state.DisableForRun();
        }

        Assert.Null(GraphQlLanguagesShadowScope.Current);
    }

    // ------------------------------------------- §18.9 bounded observation

    [Fact]
    public async Task AMatchIncrementsOnlyMatch()
    {
        ShadowFixture fixture = new((MatchingLanguages(), HttpStatusCode.OK));
        GraphQlLanguagesShadowState state = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            await fixture.CollectAsync("repo-0");
        }

        Assert.Equal(1, state.Match);
        Assert.Equal(0, state.Mismatch);
        Assert.Equal(0, state.NotCompared);
        Assert.False(state.IsDisabledForRun);
    }

    [Fact]
    public async Task AMismatchIncrementsOnlyMismatch()
    {
        ShadowFixture fixture = new(("""
            {
              "data": { "repository": { "languages": {
                "pageInfo": { "hasNextPage": false, "endCursor": null },
                "edges": [ { "size": 999999, "node": { "name": "Rust" } } ]
              } } }
            }
            """, HttpStatusCode.OK));
        GraphQlLanguagesShadowState state = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            RepositorySnapshot snapshot = await fixture.CollectAsync("repo-0");

            // A mismatch is operationally visible and evidentially invisible.
            Assert.Equal(RestLanguages, snapshot.Languages);
        }

        Assert.Equal(0, state.Match);
        Assert.Equal(1, state.Mismatch);
        Assert.Equal(0, state.NotCompared);
    }

    // The counters carry no identity and no content — only three integers and a
    // boolean exist to carry anything at all.
    [Fact]
    public void TheShadowStateExposesOnlyBoundedCounters()
    {
        Assert.All(
            typeof(GraphQlLanguagesShadowState)
                .GetProperties()
                .Select(property => property.PropertyType),
            type => Assert.Contains(type, new[] { typeof(int), typeof(bool) }));
    }

    // ---------------------------------- F-3 A. decorator cancellation branch

    // The wrapper must let caller cancellation through: it is not swallowed by
    // the shadow exception isolation, does not become NotCompared and does not
    // trigger the §18.6 disable.
    [Fact]
    public async Task CallerCancellation_PropagatesAndDoesNotDisableTheRun()
    {
        ShadowFixture fixture = new((MatchingLanguages(), HttpStatusCode.OK));
        GraphQlLanguagesShadowState state = new();
        using CancellationTokenSource cancellation = new();

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => fixture.CollectAsync("repo-0", cancellation, cancelOnGraphQl: true));
        }

        Assert.False(state.IsDisabledForRun);
        Assert.Equal(0, state.NotCompared);
        Assert.Equal(0, state.Match);
        Assert.Equal(0, state.Mismatch);
    }

    // ------------------------------- F-3 B. REST languages unavailable branch

    // When the authoritative REST languages surface is not Available there is
    // nothing to compare against, so the shadow is not invoked at all — no POST,
    // and the REST failure keeps its own status, reason and RetrievedAt.
    [Fact]
    public async Task WhenRestLanguagesIsUnavailable_TheShadowIsNotInvokedAtAll()
    {
        ShadowFixture fixture = new((MatchingLanguages(), HttpStatusCode.OK), languagesFail: true);
        GraphQlLanguagesShadowState state = new();
        RepositorySnapshot withShadow;
        RepositorySnapshot withoutShadow;

        using (GraphQlLanguagesShadowScope.Begin(state))
        {
            withShadow = await fixture.CollectAsync("repo-0");
        }

        ShadowFixture bare = new((MatchingLanguages(), HttpStatusCode.OK), languagesFail: true, withShadow: false);
        withoutShadow = await bare.CollectAsync("repo-0");

        Assert.Equal(0, fixture.GraphQlRequests);
        Assert.Equal(0, state.Match + state.Mismatch + state.NotCompared);
        Assert.False(state.IsDisabledForRun);

        DataAvailability shadowed = withShadow.FindAvailability("languages")!;
        DataAvailability plain = withoutShadow.FindAvailability("languages")!;

        Assert.False(shadowed.IsAvailable);
        Assert.Equal(plain.Status, shadowed.Status);
        Assert.Equal(plain.Reason, shadowed.Reason);
        Assert.Empty(withShadow.Languages);
    }

    // ------------------------------------------------------------ fixtures

    private static readonly Dictionary<string, long> RestLanguages =
        new(StringComparer.OrdinalIgnoreCase) { ["C#"] = 1200, ["JavaScript"] = 300 };

    private static string MatchingLanguages() => """
        {
          "data": { "repository": { "languages": {
            "pageInfo": { "hasNextPage": false, "endCursor": null },
            "edges": [
              { "size": 1200, "node": { "name": "C#" } },
              { "size": 300, "node": { "name": "JavaScript" } }
            ]
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

    private sealed class ShadowFixture
    {
        private readonly Func<int, (string Body, HttpStatusCode Status)> graphQl;
        private readonly bool languagesFail;
        private readonly bool withShadow;
        private readonly Handler handler;
        private readonly HttpClient client;

        // A fresh response per call: an HttpResponseMessage is disposed by the
        // transport, so handing the same instance back twice would fail for a
        // reason that has nothing to do with what is under test.
        public ShadowFixture(
            (string Body, HttpStatusCode Status) first,
            (string Body, HttpStatusCode Status)? later = null,
            bool languagesFail = false,
            bool withShadow = true,
            bool timeoutOnGraphQl = false,
            int timeoutCalls = int.MaxValue)
        {
            TimeoutOnGraphQl = timeoutOnGraphQl;
            TimeoutCalls = timeoutCalls;
            graphQl = call => call == 1 || later is null ? first : later.Value;
            this.languagesFail = languagesFail;
            this.withShadow = withShadow;
            handler = new Handler(this);
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
        }

        public int GraphQlRequests => handler.GraphQlRequests;

        internal bool TimeoutOnGraphQl { get; }

        internal int TimeoutCalls { get; }

        public async Task<RepositorySnapshot> CollectAsync(
            string name,
            CancellationTokenSource? cancellation = null,
            bool cancelOnGraphQl = false)
        {
            handler.CancelOnGraphQl = cancelOnGraphQl ? cancellation : null;

            GitHubRepositorySnapshotResult result = await GitHubRepositorySnapshotOrchestrator
                .CreateProfileComposition(
                    client,
                    TimeProvider.System,
                    withShadow ? new GitHubGraphQlLanguagesShadow(new GitHubGraphQlTransport(client)) : null)
                .CollectAsync(
                    $"https://github.com/{Owner}/{name}",
                    cancellation?.Token ?? CancellationToken.None);

            Assert.True(result.IsSuccessful);

            return result.Snapshot!;
        }

        private sealed class Handler(ShadowFixture fixture) : HttpMessageHandler
        {
            private int graphQlCalls;

            public int GraphQlRequests => graphQlCalls;

            public CancellationTokenSource? CancelOnGraphQl { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string path = request.RequestUri!.AbsolutePath;

                if (path == "/graphql")
                {
                    graphQlCalls++;

                    if (CancelOnGraphQl is not null)
                    {
                        CancelOnGraphQl.Cancel();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    // Exactly what HttpClient raises when its own Timeout
                    // elapses: a TaskCanceledException while the caller's token
                    // is still alive.
                    if (fixture.TimeoutOnGraphQl && graphQlCalls <= fixture.TimeoutCalls)
                    {
                        throw new TaskCanceledException("The request timed out.");
                    }

                    (string body, HttpStatusCode status) = fixture.graphQl(graphQlCalls);

                    return Task.FromResult(Json(body, status));
                }

                if (path.EndsWith("/languages", StringComparison.Ordinal))
                {
                    return Task.FromResult(fixture.languagesFail
                        ? Json("""{ "message": "boom" }""", HttpStatusCode.InternalServerError)
                        : Json("""{ "C#": 1200, "JavaScript": 300 }"""));
                }

                if (path.StartsWith("/repos/", StringComparison.Ordinal)
                    && path.Count(character => character == '/') == 3)
                {
                    return Task.FromResult(Json($$"""
                        {
                          "name": "{{path[(path.LastIndexOf('/') + 1)..]}}",
                          "owner": { "login": "{{Owner}}" },
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
                        """));
                }

                if (path.Contains("/git/ref/", StringComparison.Ordinal))
                {
                    return Task.FromResult(Json(
                        """{ "message": "Git Repository is empty." }""",
                        HttpStatusCode.Conflict));
                }

                if (path.EndsWith("/commits", StringComparison.Ordinal)
                    || path.EndsWith("/issues", StringComparison.Ordinal)
                    || path.EndsWith("/pulls", StringComparison.Ordinal)
                    || path.EndsWith("/releases", StringComparison.Ordinal)
                    || path.EndsWith("/tags", StringComparison.Ordinal))
                {
                    return Task.FromResult(Json("[]"));
                }

                return Task.FromResult(Json(
                    """{ "message": "Not Found" }""",
                    HttpStatusCode.NotFound));
            }
        }
    }
}

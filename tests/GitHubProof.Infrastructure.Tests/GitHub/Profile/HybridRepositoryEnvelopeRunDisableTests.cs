using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 127 — the §18.6 run-scoped disable, proven across SEVERAL
// repositories through the real Profile composition.
//
// The rule this file exists for is a cost rule with a correctness edge: a
// systemic GraphQL failure must cost ONE attempt for the whole execution, not
// one per admitted repository — and every repository must still be collected in
// full over REST while that is true.
public sealed class HybridRepositoryEnvelopeRunDisableTests
{
    private const string Owner = "octocat";

    // ------------------------------ envelope-level failures disable the run

    public static TheoryData<string, HttpStatusCode, string?, string?> ProviderWideCases =>
        new()
        {
            { "{ }", HttpStatusCode.Unauthorized, null, null },
            { "{ }", HttpStatusCode.Forbidden, null, null },
            { "{ }", HttpStatusCode.BadGateway, null, null },
            { "{ }", HttpStatusCode.GatewayTimeout, null, null },
            { "not json", HttpStatusCode.OK, null, null },
            { ErroredEnvelope, HttpStatusCode.OK, "x-ratelimit-remaining", "0" },
            { ErroredEnvelope, HttpStatusCode.OK, "Retry-After", "120" }
        };

    [Theory]
    [MemberData(nameof(ProviderWideCases))]
    public async Task AnEnvelopeLevelFailure_CostsOneAttemptForTheWholeExecution(
        string body,
        HttpStatusCode status,
        string? header,
        string? value)
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        using Fixture fixture = new(
            first: (body, status),
            later: (Envelope("second"), HttpStatusCode.OK),
            headers: header is null ? [] : [(header, value!)]);

        RepositorySnapshot first = await fixture.CollectAsync("first");
        RepositorySnapshot second = await fixture.CollectAsync("second");
        RepositorySnapshot third = await fixture.CollectAsync("third");

        // One POST for three repositories.
        Assert.Equal(1, fixture.GraphQlRequests);
        Assert.True(state.IsDisabledForRun);
        Assert.Equal(0, state.RepositoryEnvelopeServed);
        Assert.Equal(3, state.RepositoryEnvelopeFallback);

        // And every repository was still collected in full over REST.
        foreach (RepositorySnapshot snapshot in new[] { first, second, third })
        {
            Assert.Equal(
                new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 },
                snapshot.Languages);
            Assert.True(snapshot.FindAvailability("metadata")!.IsAvailable);
            Assert.True(snapshot.FindAvailability("languages")!.IsAvailable);
        }

        // Each of the three paid for both replaced REST requests, including the
        // two that never attempted GraphQL at all.
        foreach (string name in new[] { "first", "second", "third" })
        {
            Assert.Equal(1, fixture.CountExact($"/repos/{Owner}/{name}"));
            Assert.Equal(1, fixture.CountExact($"/repos/{Owner}/{name}/languages"));
        }
    }

    // ----------------------------- an abstention does NOT disable the run

    // §18.4 without a Session 126 provider-wide header: the repository falls
    // back, and the NEXT repository is still served. A per-repository
    // abstention that silently became a run-scoped disable would throw away
    // every later consolidation for no proven reason.
    [Fact]
    public async Task AnAbstention_LeavesLaterRepositoriesServable()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        using Fixture fixture = new(
            first: (ErroredEnvelope, HttpStatusCode.OK),
            later: (Envelope("second"), HttpStatusCode.OK));

        await fixture.CollectAsync("first");
        await fixture.CollectAsync("second");

        Assert.False(state.IsDisabledForRun);
        Assert.Equal(2, fixture.GraphQlRequests);
        Assert.Equal(1, state.RepositoryEnvelopeServed);
        Assert.Equal(1, state.RepositoryEnvelopeFallback);

        // The abstaining repository paid for both REST requests; the served one
        // paid for neither.
        Assert.Equal(1, fixture.CountExact($"/repos/{Owner}/first"));
        Assert.Equal(1, fixture.CountExact($"/repos/{Owner}/first/languages"));
        Assert.Equal(0, fixture.CountExact($"/repos/{Owner}/second"));
        Assert.Equal(0, fixture.CountExact($"/repos/{Owner}/second/languages"));
    }

    // ------------------------------------ the disable belongs to one execution

    [Fact]
    public async Task AFollowingExecution_MayUseGraphQlAgain()
    {
        using Fixture fixture = new(
            first: ("{ }", HttpStatusCode.BadGateway),
            later: (Envelope("second"), HttpStatusCode.OK));

        ProfileGraphQlExecutionState first = new();

        using (ProfileGraphQlExecutionScope.Begin(first))
        {
            await fixture.CollectAsync("first");
            await fixture.CollectAsync("second");
        }

        ProfileGraphQlExecutionState second = new();

        using (ProfileGraphQlExecutionScope.Begin(second))
        {
            await fixture.CollectAsync("second");
        }

        Assert.True(first.IsDisabledForRun);
        Assert.False(second.IsDisabledForRun);
        Assert.Equal(2, fixture.GraphQlRequests);
        Assert.Equal(1, second.RepositoryEnvelopeServed);
    }

    // With no ambient scope at all — a direct composition call outside a Profile
    // execution — nothing is recorded and nothing is disabled. The envelope
    // still works per repository, which keeps the state optional rather than
    // load-bearing.
    [Fact]
    public async Task WithoutAnExecutionScope_TheEnvelopeStillServesPerRepository()
    {
        using Fixture fixture = new(first: (Envelope("only"), HttpStatusCode.OK));

        RepositorySnapshot snapshot = await fixture.CollectAsync("only");

        Assert.Equal(1, fixture.GraphQlRequests);
        Assert.Equal(0, fixture.CountExact($"/repos/{Owner}/only"));
        Assert.Equal("only", snapshot.Repository.Name);
    }

    // ---------------------------------------------------------- test support

    private const string ErroredEnvelope = """
        {
          "data": { "repository": null },
          "errors": [ { "message": "Something went wrong." } ]
        }
        """;

    private static string Envelope(string name) => $$"""
        {
          "data": {
            "repository": {
              "name": "{{name}}",
              "owner": { "login": "{{Owner}}" },
              "isPrivate": false,
              "isArchived": false,
              "isFork": false,
              "isTemplate": false,
              "description": null,
              "homepageUrl": null,
              "createdAt": "2024-01-01T00:00:00Z",
              "hasIssuesEnabled": true,
              "primaryLanguage": { "name": "C#" },
              "defaultBranchRef": { "name": "main" },
              "repositoryTopics": {
                "pageInfo": { "hasNextPage": false },
                "nodes": []
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

    // A deterministic multi-repository GitHub, small on purpose: this file is
    // about the run-scoped disable, not about evidence shape.
    private sealed class Fixture : IDisposable
    {
        private readonly Func<int, (string Body, HttpStatusCode Status)> graphQl;
        private readonly (string Name, string Value)[] headers;
        private readonly Handler handler;
        private readonly HttpClient client;

        public Fixture(
            (string Body, HttpStatusCode Status) first,
            (string Body, HttpStatusCode Status)? later = null,
            (string Name, string Value)[]? headers = null)
        {
            graphQl = call => call == 1 || later is null ? first : later.Value;
            this.headers = headers ?? [];
            handler = new Handler(this);
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
        }

        public int GraphQlRequests => handler.GraphQlRequests;

        public int CountExact(string path) =>
            handler.RequestPaths.Count(request => request == path);

        public void Dispose() => client.Dispose();

        public async Task<RepositorySnapshot> CollectAsync(string name)
        {
            GitHubRepositorySnapshotResult result = await GitHubRepositorySnapshotOrchestrator
                .CreateProfileComposition(
                    client,
                    TimeProvider.System,
                    languagesShadow: null,
                    new GitHubGraphQlRepositoryEnvelopeSource(
                        new GitHubGraphQlTransport(client)))
                .CollectAsync($"https://github.com/{Owner}/{name}");

            Assert.True(result.IsSuccessful);

            return result.Snapshot!;
        }

        private sealed class Handler(Fixture fixture) : HttpMessageHandler
        {
            private readonly List<string> requestPaths = [];

            private int graphQlCalls;

            public int GraphQlRequests => graphQlCalls;

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
                cancellationToken.ThrowIfCancellationRequested();

                string path = request.RequestUri!.AbsolutePath;

                lock (requestPaths)
                {
                    requestPaths.Add(path);
                }

                if (path == "/graphql")
                {
                    graphQlCalls++;

                    (string body, HttpStatusCode status) = fixture.graphQl(graphQlCalls);
                    HttpResponseMessage response = Json(body, status);

                    foreach ((string name, string value) in fixture.headers)
                    {
                        response.Headers.TryAddWithoutValidation(name, value);
                    }

                    return Task.FromResult(response);
                }

                if (path.EndsWith("/languages", StringComparison.Ordinal))
                {
                    return Task.FromResult(Json("""{ "C#": 1200, "JavaScript": 300 }"""));
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

                // An empty repository, so the contents surface resolves without
                // needing a tree fixture. The adopted surfaces are unaffected.
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

            private static HttpResponseMessage Json(
                string content,
                HttpStatusCode status = HttpStatusCode.OK) =>
                new(status)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
        }
    }
}

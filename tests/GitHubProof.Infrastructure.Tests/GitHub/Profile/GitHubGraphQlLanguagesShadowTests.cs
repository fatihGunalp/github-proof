using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 125 — S2 of docs/architecture/public-profile-runtime-collection.md
// §§18-19: the first real REST <-> GraphQL parity observation.
//
// The shadow's whole job is to be right or to abstain. It may never guess, never
// consume a partial payload, and never turn structural ambiguity into a verdict
// — a wrong Match would be worse than no comparison at all.
public sealed class GitHubGraphQlLanguagesShadowTests
{
    private static readonly GitHubRepositoryUrl Repository =
        GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world");

    // --------------------------------------------------------- D. edge order

    [Fact]
    public async Task Compare_MatchesWhenTheSameMapArrivesInADifferentEdgeOrder()
    {
        RecordingTransport transport = new(Success("""
            [
              { "size": 300, "node": { "name": "JavaScript" } },
              { "size": 1200, "node": { "name": "C#" } }
            ]
            """));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.Match, outcome);
    }

    // The REST collector and the domain snapshot both key languages with
    // OrdinalIgnoreCase, so the shadow must agree with them rather than with
    // ordinal equality.
    [Fact]
    public async Task Compare_UsesTheSameCaseInsensitiveKeyingAsTheRestPath()
    {
        RecordingTransport transport = new(Success("""
            [ { "size": 1200, "node": { "name": "c#" } } ]
            """));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.Match, outcome);
    }

    // ------------------------------------------------------- E/F. mismatches

    [Fact]
    public async Task Compare_ReportsMismatchOnAByteDifference()
    {
        RecordingTransport transport = new(Success("""
            [ { "size": 1201, "node": { "name": "C#" } } ]
            """));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.Mismatch, outcome);
    }

    [Fact]
    public async Task Compare_ReportsMismatchWhenGraphQlIsMissingALanguage()
    {
        RecordingTransport transport = new(Success("""
            [ { "size": 1200, "node": { "name": "C#" } } ]
            """));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.Mismatch, outcome);
    }

    [Fact]
    public async Task Compare_ReportsMismatchWhenGraphQlCarriesAnExtraLanguage()
    {
        RecordingTransport transport = new(Success("""
            [
              { "size": 1200, "node": { "name": "C#" } },
              { "size": 300, "node": { "name": "JavaScript" } }
            ]
            """));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.Mismatch, outcome);
    }

    // ------------------------------------- G/H/I. never consume a bad envelope

    // §18.4: an HTTP 200 carrying `errors` is unresolved, not data. The partial
    // language map inside it must never be compared, because a comparison would
    // read it as an observation.
    [Fact]
    public async Task Compare_NeverConsumesAPartialPayloadFromAnErroredEnvelope()
    {
        RecordingTransport transport = new(Response(HttpStatusCode.OK, """
            {
              "data": { "repository": { "languages": {
                "pageInfo": { "hasNextPage": false, "endCursor": null },
                "edges": [ { "size": 1200, "node": { "name": "C#" } } ]
              } } },
              "errors": [ { "message": "partial" } ]
            }
            """));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        // Not a Match, even though the visible edges happen to agree.
        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task Compare_AbstainsOnAnEnvelopeFailure(HttpStatusCode statusCode)
    {
        RecordingTransport transport = new(Response(statusCode, """{ "message": "no" }"""));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{ "data": null }""")]
    [InlineData("""{ "data": { "repository": null } }""")]
    [InlineData("""{ "data": { "repository": { "languages": null } } }""")]
    [InlineData("""{ "data": { "repository": { "languages": { "edges": [] } } } }""")]
    public async Task Compare_AbstainsOnAMalformedOrUnreadableEnvelope(string body)
    {
        RecordingTransport transport = new(Response(HttpStatusCode.OK, body));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
    }

    // ------------------------------------------- structural ambiguity abstains

    // The REST collector throws on a duplicate language rather than letting one
    // win. The shadow must be no more permissive — and must not apply last-wins.
    [Theory]
    [InlineData("""[ { "size": 1200, "node": { "name": "C#" } }, { "size": 5, "node": { "name": "C#" } } ]""")]
    [InlineData("""[ { "size": 1200, "node": { "name": "C#" } }, { "size": 5, "node": { "name": "c#" } } ]""")]
    public async Task Compare_AbstainsOnADuplicateLanguageRatherThanPickingOne(string edges)
    {
        RecordingTransport transport = new(Success(edges));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
    }

    [Theory]
    [InlineData("""[ { "size": -1, "node": { "name": "C#" } } ]""")]
    [InlineData("""[ { "size": 1200, "node": { "name": "   " } } ]""")]
    [InlineData("""[ { "size": 1200, "node": {} } ]""")]
    [InlineData("""[ { "size": 1200 } ]""")]
    [InlineData("""[ { "node": { "name": "C#" } } ]""")]
    [InlineData("""[ { "size": "1200", "node": { "name": "C#" } } ]""")]
    public async Task Compare_AbstainsOnAStructurallyAmbiguousEdge(string edges)
    {
        RecordingTransport transport = new(Success(edges));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["C#"] = 1200 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
    }

    // ----------------------------------------------------------- J. pagination

    [Fact]
    public async Task Compare_FollowsEveryPageAndComparesTheCompleteMap()
    {
        Dictionary<string, long> rest = new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < 150; index++)
        {
            rest[$"Language{index}"] = index + 1;
        }

        RecordingTransport transport = new(
            Success(EdgesFor(0, 100), hasNextPage: true, endCursor: "cursor-100"),
            Success(EdgesFor(100, 150)));

        GraphQlLanguagesShadowOutcome outcome = await Compare(transport, rest);

        Assert.Equal(GraphQlLanguagesShadowOutcome.Match, outcome);
        Assert.Equal(2, transport.Requests.Count);

        // The first page asks for no cursor; the second asks for exactly the one
        // the first returned. No arbitrary cap, no sampling.
        Assert.Null(ReadVariable(transport.Requests[0], "after"));
        Assert.Equal("cursor-100", ReadVariable(transport.Requests[1], "after"));
        Assert.All(
            transport.Requests,
            request => Assert.Equal("100", ReadVariable(request, "first")));
    }

    // A connection that promises another page but hands back no usable cursor
    // cannot be enumerated. Abstaining beats looping, and beats comparing a
    // knowingly incomplete map.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Compare_AbstainsWhenAnotherPageIsPromisedWithoutACursor(string? endCursor)
    {
        RecordingTransport transport = new(
            Success(EdgesFor(0, 1), hasNextPage: true, endCursor: endCursor));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["Language0"] = 1 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
        Assert.Single(transport.Requests);
    }

    [Fact]
    public async Task Compare_AbstainsWhenTheCursorDoesNotAdvance()
    {
        RecordingTransport transport = new(
            Success(EdgesFor(0, 1), hasNextPage: true, endCursor: "same"),
            Success(EdgesFor(1, 2), hasNextPage: true, endCursor: "same"));

        GraphQlLanguagesShadowOutcome outcome = await Compare(
            transport,
            new Dictionary<string, long> { ["Language0"] = 1, ["Language1"] = 2 });

        Assert.Equal(GraphQlLanguagesShadowOutcome.NotCompared, outcome);
        Assert.Equal(2, transport.Requests.Count);
    }

    // ------------------------------------------------------ K. query shape

    [Fact]
    public async Task Compare_RequestsOnlyWhatLanguageParityNeeds()
    {
        RecordingTransport transport = new(Success(EdgesFor(0, 1)));

        await Compare(transport, new Dictionary<string, long> { ["Language0"] = 1 });

        string query = Assert.Single(transport.Requests).Query;

        // §18.12 forbids adopting exact-total provider fields; the query must not
        // even ask for one. §18.3 keeps `isEmpty` unadopted.
        Assert.DoesNotContain("totalCount", query, StringComparison.Ordinal);
        Assert.DoesNotContain("totalSize", query, StringComparison.Ordinal);
        Assert.DoesNotContain("isEmpty", query, StringComparison.Ordinal);

        // No surface this slice does not own.
        foreach (string unrelated in new[]
        {
            "defaultBranchRef", "topics", "repositoryTopics", "history", "commits",
            "pullRequests", "releases", "refs", "issues", "licenseInfo", "object"
        })
        {
            Assert.DoesNotContain(unrelated, query, StringComparison.Ordinal);
        }

        Assert.Contains("languages(", query, StringComparison.Ordinal);
        Assert.Contains("size", query, StringComparison.Ordinal);
    }

    // Repository identity is a variable, never interpolated into the operation
    // text, so the query is a constant across every repository.
    [Fact]
    public async Task Compare_CarriesRepositoryIdentityInVariablesOnly()
    {
        RecordingTransport transport = new(Success(EdgesFor(0, 1)));

        await Compare(transport, new Dictionary<string, long> { ["Language0"] = 1 });

        GitHubGraphQlRequest request = Assert.Single(transport.Requests);

        Assert.DoesNotContain("octocat", request.Query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hello-world", request.Query, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("octocat", ReadVariable(request, "owner"));
        Assert.Equal("hello-world", ReadVariable(request, "name"));
    }

    // ---------------------------------------------------------- M. cancellation

    [Fact]
    public async Task Compare_PropagatesCallerCancellation()
    {
        CancellingTransport transport = new();
        GitHubGraphQlLanguagesShadow shadow = new(transport);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => shadow.CompareAsync(
                Repository,
                new Dictionary<string, long>(),
                cancellation.Token));
    }

    // ------------------------------------------------ N. no domain status ever

    [Fact]
    public void ShadowOutcome_IsNotADomainStatus()
    {
        Assert.Equal(
            "GitHubProof.Infrastructure.GitHub.Profile",
            typeof(GraphQlLanguagesShadowOutcome).Namespace);
        Assert.Equal(
            [
                nameof(GraphQlLanguagesShadowOutcome.NotCompared),
                nameof(GraphQlLanguagesShadowOutcome.Match),
                nameof(GraphQlLanguagesShadowOutcome.Mismatch)
            ],
            Enum.GetNames<GraphQlLanguagesShadowOutcome>());

        // The seam returns the outcome and nothing else: no availability, no
        // reason, no evidence carrier that could grow into one (§18.7).
        Assert.Equal(
            typeof(Task<GraphQlLanguagesShadowOutcome>),
            typeof(IGitHubGraphQlLanguagesShadow)
                .GetMethod(nameof(IGitHubGraphQlLanguagesShadow.CompareAsync))!
                .ReturnType);
    }

    // -------------------------------------------------------- test support

    private static Task<GraphQlLanguagesShadowOutcome> Compare(
        IGitHubGraphQlTransport transport,
        IReadOnlyDictionary<string, long> restLanguages) =>
        new GitHubGraphQlLanguagesShadow(transport).CompareAsync(
            Repository,
            new ReadOnlyDictionary<string, long>(
                new Dictionary<string, long>(restLanguages, StringComparer.OrdinalIgnoreCase)));

    private static string EdgesFor(int start, int end) =>
        "[" + string.Join(
            ",",
            Enumerable
                .Range(start, end - start)
                .Select(index => $$"""{ "size": {{index + 1}}, "node": { "name": "Language{{index}}" } }"""))
        + "]";

    private static HttpResponseMessage Success(
        string edges,
        bool hasNextPage = false,
        string? endCursor = null) =>
        Response(HttpStatusCode.OK, $$"""
            {
              "data": { "repository": { "languages": {
                "pageInfo": {
                  "hasNextPage": {{(hasNextPage ? "true" : "false")}},
                  "endCursor": {{(endCursor is null ? "null" : $"\"{endCursor}\"")}}
                },
                "edges": {{edges}}
              } } }
            }
            """);

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body) =>
        new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string? ReadVariable(GitHubGraphQlRequest request, string name) =>
        request.Variables?[name]?.ToJsonString().Trim('"');

    // A transport stub rather than a fake HTTP handler: this suite is about the
    // shadow's reading and comparison rules, and S1 already proves the transport
    // itself.
    private sealed class RecordingTransport(params HttpResponseMessage[] responses)
        : IGitHubGraphQlTransport
    {
        private readonly GitHubGraphQlTransport transport = new(
            new HttpClient(new QueueHandler(responses))
            {
                BaseAddress = new Uri("https://api.github.com/")
            });

        public List<GitHubGraphQlRequest> Requests { get; } = [];

        public Task<GitHubGraphQlResponse> SendAsync(
            GitHubGraphQlRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            return transport.SendAsync(request, cancellationToken);
        }

        private sealed class QueueHandler(HttpResponseMessage[] responses) : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> queue = new(responses);

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(queue.Dequeue());
            }
        }
    }

    private sealed class CancellingTransport : IGitHubGraphQlTransport
    {
        public Task<GitHubGraphQlResponse> SendAsync(
            GitHubGraphQlRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException("The token should already be cancelled.");
        }
    }
}

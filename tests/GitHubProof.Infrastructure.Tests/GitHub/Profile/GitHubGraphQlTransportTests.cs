using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 124 — S1 of docs/architecture/public-profile-runtime-collection.md
// §§18-19.
//
// The load-bearing proof here is §18.4: an HTTP 200 carrying `errors` must be
// structurally distinguishable from ordinary success, and a body this transport
// cannot read must never become successful absence. Everything else exists so
// that distinction cannot be lost later.
public sealed class GitHubGraphQlTransportTests
{
    private static readonly GitHubGraphQlRequest Query = new("query { viewer { login } }");

    // ------------------------------------------------------------- A. request

    [Fact]
    public async Task Send_IssuesOnePostToGraphQlWithAJsonBody()
    {
        RecordingHandler handler = new(Json("""{ "data": { "viewer": {} } }"""));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        await transport.SendAsync(Query);

        Assert.Equal(1, handler.SendCount);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.github.com/graphql", handler.RequestUri!.AbsoluteUri);
        Assert.Equal("application/json", handler.ContentType);

        using JsonDocument body = JsonDocument.Parse(handler.Body!);

        Assert.Equal(
            Query.Query,
            body.RootElement.GetProperty("query").GetString());
    }

    [Fact]
    public async Task Send_CarriesVariablesAndLeavesTheCallersObjectReusable()
    {
        RecordingHandler handler = new(
            Json("""{ "data": { "viewer": {} } }"""),
            Json("""{ "data": { "viewer": {} } }"""));
        GitHubGraphQlTransport transport = new(CreateClient(handler));
        JsonObject variables = new() { ["login"] = "octocat" };
        GitHubGraphQlRequest request = new("query($login: String!) { user(login: $login) { id } }", variables);

        await transport.SendAsync(request);
        await transport.SendAsync(request);

        using JsonDocument body = JsonDocument.Parse(handler.Body!);

        Assert.Equal(
            "octocat",
            body.RootElement.GetProperty("variables").GetProperty("login").GetString());
    }

    // GraphQL is not a versioned REST route, so the REST collectors'
    // X-GitHub-Api-Version behaviour is deliberately not invented for it.
    [Fact]
    public async Task Send_DoesNotInventTheRestApiVersionHeader()
    {
        RecordingHandler handler = new(Json("""{ "data": { "viewer": {} } }"""));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        await transport.SendAsync(Query);

        Assert.False(handler.Headers!.Contains("X-GitHub-Api-Version"));
        Assert.Contains(
            handler.Headers.UserAgent,
            product => string.Equals(product.Product?.Name, "GitHubProof", StringComparison.Ordinal));
    }

    // ------------------------------------------------------ B. authentication

    // The transport creates no credential source of its own: whatever the
    // existing named client is configured with is what reaches GitHub.
    [Fact]
    public async Task Send_CarriesTheExistingClientAuthorizationHeader()
    {
        RecordingHandler handler = new(Json("""{ "data": { "viewer": {} } }"""));
        HttpClient client = CreateClient(handler);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "configured-value");
        GitHubGraphQlTransport transport = new(client);

        await transport.SendAsync(Query);

        Assert.Equal("Bearer", handler.Authorization!.Scheme);
        Assert.Equal("configured-value", handler.Authorization.Parameter);
    }

    [Fact]
    public async Task Send_SendsNoAuthorizationWhenTheClientHasNone()
    {
        RecordingHandler handler = new(Json("""{ "data": { "viewer": {} } }"""));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        await transport.SendAsync(Query);

        Assert.Null(handler.Authorization);
    }

    // --------------------------------------------- D. HTTP 200 + data + errors

    // §18.4, the blocking rule this whole slice exists for.
    [Fact]
    public async Task Send_DoesNotTreatHttp200WithErrorsAsOrdinarySuccess()
    {
        RecordingHandler handler = new(Json("""
            {
              "data": { "repository": null },
              "errors": [
                { "type": "NOT_FOUND", "path": ["repository"], "message": "Could not resolve to a Repository." }
              ]
            }
            """));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.False(response.IsSuccess);
        Assert.Equal(1, response.ErrorCount);
    }

    // E (transport half): errors are normalized to a count. No message, path or
    // extension survives anywhere in the response surface, so nothing downstream
    // can branch on wording and nothing exists to leak into telemetry (§18.9).
    //
    // The fixture deliberately carries a POPULATED `data` alongside the errored
    // envelope, so the walk below has real content to sift and the assertion is
    // not satisfied by an empty object. The sentinels live only in the error
    // message, the error path and an error extension.
    [Fact]
    public async Task Send_RetainsNoGraphQlErrorTextAnywhereInTheResponse()
    {
        const string messageSentinel = "SENTINEL-MESSAGE-could-not-resolve";
        const string pathSentinel = "SENTINEL-PATH-repositoryOwner";
        const string extensionSentinel = "SENTINEL-EXTENSION-NOT_FOUND";

        RecordingHandler handler = new(Json($$"""
            {
              "data": { "viewer": { "login": "benign-payload" } },
              "errors": [
                {
                  "message": "{{messageSentinel}}",
                  "path": ["{{pathSentinel}}"],
                  "extensions": { "code": "{{extensionSentinel}}" }
                }
              ]
            }
            """));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.Equal(1, response.ErrorCount);

        // The payload itself is retained for the future §18.5 isolation work, so
        // the walk must find the benign content — otherwise the test would pass
        // simply because nothing was kept at all.
        string surface = DescribeResponseSurface(response);

        Assert.Contains("benign-payload", surface, StringComparison.Ordinal);
        Assert.DoesNotContain(messageSentinel, surface, StringComparison.Ordinal);
        Assert.DoesNotContain(pathSentinel, surface, StringComparison.Ordinal);
        Assert.DoesNotContain(extensionSentinel, surface, StringComparison.Ordinal);

        // No normalized error representation exists to hold the text either: the
        // response exposes a count and nothing else about the errors.
        Assert.DoesNotContain(
            typeof(GitHubGraphQlResponse).GetProperties(),
            property => property.PropertyType != typeof(int)
                && property.Name.Contains("Error", StringComparison.Ordinal));
    }

    // Every public value the response exposes, plus the raw payload text,
    // flattened into one string. Nested records are walked one level, which
    // covers GitHubGraphQlRateLimit — the only nested type on the surface.
    private static string DescribeResponseSurface(GitHubGraphQlResponse response)
    {
        StringBuilder surface = new();

        surface.Append(response.Data?.GetRawText());

        foreach (PropertyInfo property in typeof(GitHubGraphQlResponse).GetProperties())
        {
            object? value = property.GetValue(response);

            surface.Append(property.Name).Append('=').Append(value).Append(';');

            if (value is null || value is JsonElement || value.GetType().IsPrimitive)
            {
                continue;
            }

            foreach (PropertyInfo nested in value.GetType().GetProperties())
            {
                surface.Append(nested.Name).Append('=').Append(nested.GetValue(value)).Append(';');
            }
        }

        return surface.ToString();
    }

    // F-3: System.Text.Json resolves a duplicate member name to the LAST
    // occurrence, so without an explicit guard an errored envelope could be read
    // as ordinary success. All three orderings must be Malformed.
    [Theory]
    // 1. non-empty errors followed by empty errors
    [InlineData("""{ "data": { "viewer": {} }, "errors": [ { "message": "boom" } ], "errors": [] }""")]
    // 2. empty errors followed by non-empty errors
    [InlineData("""{ "data": { "viewer": {} }, "errors": [], "errors": [ { "message": "boom" } ] }""")]
    // 3. duplicate data
    [InlineData("""{ "data": { "viewer": {} }, "data": { "viewer": {} } }""")]
    [InlineData("""{ "data": null, "data": { "viewer": {} } }""")]
    public async Task Send_TreatsADuplicateContractMemberAsMalformed(string body)
    {
        RecordingHandler handler = new(Json(body));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.Malformed, response.Outcome);
        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Equal(0, response.ErrorCount);
    }

    // A duplicate on a member this contract does not depend on is not the
    // transport's business, so the guard stays narrow.
    [Fact]
    public async Task Send_IgnoresDuplicatesOnMembersItDoesNotRead()
    {
        RecordingHandler handler = new(Json("""
            { "data": { "viewer": {} }, "extensions": { "a": 1 }, "extensions": { "b": 2 } }
            """));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.Success, response.Outcome);
    }

    [Fact]
    public async Task Send_TreatsAnEmptyErrorsArrayAsNoErrors()
    {
        RecordingHandler handler = new(Json("""{ "data": { "viewer": {} }, "errors": [] }"""));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.Success, response.Outcome);
        Assert.Equal(0, response.ErrorCount);
    }

    // ------------------------------------------------------ F. successful data

    [Fact]
    public async Task Send_ParsesSuccessfulDataWithoutProducingDomainEvidence()
    {
        RecordingHandler handler = new(Json("""
            { "data": { "repository": { "nameWithOwner": "octocat/hello-world" } } }
            """));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.Success, response.Outcome);
        Assert.True(response.IsSuccess);
        Assert.Equal(
            "octocat/hello-world",
            response.Data!.Value.GetProperty("repository").GetProperty("nameWithOwner").GetString());
    }

    // §18.7: the transport creates no availability and no domain status. Proven
    // structurally rather than by comment, so a later slice cannot quietly add
    // one here instead of at the surface adapter.
    [Fact]
    public void Response_ExposesNoDomainAvailabilityOrStatus()
    {
        Type[] exposed = typeof(GitHubGraphQlResponse)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => Nullable.GetUnderlyingType(property.PropertyType)
                ?? property.PropertyType)
            .ToArray();

        Assert.DoesNotContain(typeof(DataAvailability), exposed);
        Assert.DoesNotContain(
            exposed,
            type => type.Namespace?.StartsWith("GitHubProof.Domain", StringComparison.Ordinal) == true);
    }

    // ------------------------------------------------------------ G. rateLimit

    [Fact]
    public async Task Send_ParsesRateLimitAsBoundedStructuralValuesOnly()
    {
        RecordingHandler handler = new(Json("""
            {
              "data": {
                "rateLimit": {
                  "cost": 1,
                  "limit": 5000,
                  "remaining": 4987,
                  "used": 13,
                  "resetAt": "2026-08-25T12:00:00Z"
                }
              }
            }
            """));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.Success, response.Outcome);
        Assert.Equal(1, response.RateLimit!.Cost);
        Assert.Equal(5000, response.RateLimit.Limit);
        Assert.Equal(4987, response.RateLimit.Remaining);
        Assert.Equal(13, response.RateLimit.Used);
        Assert.Equal(
            DateTimeOffset.Parse("2026-08-25T12:00:00Z"),
            response.RateLimit.ResetAt);

        // The GraphQL observation is its own type carrying nothing but bounded
        // numbers and one instant, so it cannot be merged into, or mistaken for,
        // the REST core budget the §10.4 / §11.3 decision reads (§18.9).
        Assert.All(
            typeof(GitHubGraphQlRateLimit).GetProperties(),
            property => Assert.Contains(
                Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType,
                new[] { typeof(int), typeof(DateTimeOffset) }));
    }

    [Fact]
    public async Task Send_LeavesRateLimitUnobservedWhenTheQueryDidNotAskForIt()
    {
        RecordingHandler handler = new(Json("""{ "data": { "viewer": {} } }"""));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Null(response.RateLimit);
    }

    // ---------------------------------------------------- H. envelope failure

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Send_ReportsNonSuccessStatusAsEnvelopeFailure(HttpStatusCode statusCode)
    {
        RecordingHandler handler = new(Json("""{ "message": "no" }""", statusCode));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.EnvelopeFailure, response.Outcome);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Null(response.Data);
        Assert.Equal(0, response.ErrorCount);
    }

    [Fact]
    public async Task Send_ReportsATransportExceptionAsEnvelopeFailureWithoutAStatus()
    {
        ThrowingHandler handler = new(new HttpRequestException("connection reset"));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.EnvelopeFailure, response.Outcome);
        Assert.Null(response.StatusCode);
    }

    // ---------------------------------------------------------- I. malformed

    // The trap this guards: none of these may become "Success with no data",
    // which is exactly the successful-absence reading §18.4 forbids.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json at all")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{ "data": null }""")]
    [InlineData("""{ "data": "unexpected" }""")]
    [InlineData("""{ "data": { "viewer": {} }, "errors": "unexpected" }""")]
    public async Task Send_CannotTurnAnUnreadableBodyIntoSuccessfulAbsence(string body)
    {
        RecordingHandler handler = new(Json(body));
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        GitHubGraphQlResponse response = await transport.SendAsync(Query);

        Assert.Equal(GitHubGraphQlOutcome.Malformed, response.Outcome);
        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
    }

    // -------------------------------------------------------- J. cancellation

    // Caller cancellation is not a GraphQL failure (§11.1, §18.8): it propagates
    // rather than becoming an EnvelopeFailure result.
    [Fact]
    public async Task Send_PropagatesCallerCancellationInsteadOfReportingFailure()
    {
        BlockingHandler handler = new();
        GitHubGraphQlTransport transport = new(CreateClient(handler));
        using CancellationTokenSource cancellation = new();

        Task<GitHubGraphQlResponse> pending = transport.SendAsync(Query, cancellation.Token);

        await handler.Started;
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    // ------------------------------------------------------------- test support

    // The transport no longer defaults BaseAddress: it trusts the configured
    // named client it is given, so an isolated test client must set it here.
    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    private static HttpResponseMessage Json(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHandler(params HttpResponseMessage[] responses)
        : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public int SendCount { get; private set; }

        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? Body { get; private set; }

        public string? ContentType { get; private set; }

        public AuthenticationHeaderValue? Authorization { get; private set; }

        public HttpRequestHeaders? Headers { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            Headers = request.Headers;
            Authorization = request.Headers.Authorization;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return responses.Dequeue();
        }
    }

    private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(exception);
    }

    private sealed class BlockingHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource started =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Started => started.Task;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

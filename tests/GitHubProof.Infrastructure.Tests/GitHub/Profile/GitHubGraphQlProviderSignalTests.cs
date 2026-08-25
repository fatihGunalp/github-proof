using System.Net;
using System.Reflection;
using System.Text;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 126 — provider-wide classification of an HTTP 200 GraphQL envelope
// (docs/architecture/public-profile-runtime-collection.md §18.6, which already
// names a primary or secondary rate-limit response as envelope-level).
//
// The rule under test is narrow on purpose: a 200 carrying errors is
// provider-wide **only** when a response HEADER says so. Error text is never
// read, and the shape GitHub identifies only through that text is deliberately
// left unclassified rather than guessed.
public sealed class GitHubGraphQlProviderSignalTests
{
    private static readonly GitHubGraphQlRequest Query = new("query { viewer { login } }");

    // Sentinels live only in the error members, never in the payload, so an
    // assertion that they are absent cannot be confused with the payload this
    // transport legitimately retains for the future §18.5 isolation work.
    private const string ErroredEnvelope = """
        {
          "data": { "repository": null },
          "errors": [
            {
              "type": "SENTINELTYPErateLimited",
              "path": ["SENTINELPATHrepositoryOwner"],
              "message": "SENTINELMESSAGEapiRateLimitExceeded"
            }
          ]
        }
        """;

    // ------------------------------------------- 1. remaining == 0 classifies

    [Fact]
    public async Task Send_ClassifiesZeroRemainingAsAProviderWideCondition()
    {
        GitHubGraphQlResponse response = await SendAsync(
            ErroredEnvelope,
            ("x-ratelimit-remaining", "0"));

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.Equal(GitHubGraphQlProviderSignal.RateLimitExhausted, response.ProviderSignal);
        Assert.True(response.ReportsProviderWideCondition);
    }

    // ---------------------------------------------- 4. Retry-After classifies

    [Theory]
    [InlineData("60")]
    [InlineData("0")]
    [InlineData("Wed, 26 Aug 2026 12:00:00 GMT")]
    public async Task Send_ClassifiesAValidRetryAfterAsAProviderWideCondition(string retryAfter)
    {
        GitHubGraphQlResponse response = await SendAsync(
            ErroredEnvelope,
            ("Retry-After", retryAfter));

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.Equal(GitHubGraphQlProviderSignal.RetryAfterRequested, response.ProviderSignal);
    }

    // Exhaustion outranks a Retry-After when both are present, so the
    // classification is deterministic.
    [Fact]
    public async Task Send_PrefersExhaustionWhenBothHeadersArePresent()
    {
        GitHubGraphQlResponse response = await SendAsync(
            ErroredEnvelope,
            ("x-ratelimit-remaining", "0"),
            ("Retry-After", "60"));

        Assert.Equal(GitHubGraphQlProviderSignal.RateLimitExhausted, response.ProviderSignal);
    }

    // ------------------------------ 5/6. everything else stays per-repository

    // The residual shape this session cannot close: an errored 200 whose only
    // indication of a provider-wide condition would be the error message.
    [Fact]
    public async Task Send_LeavesAnErroredEnvelopeUnclassifiedWithoutAQualifyingHeader()
    {
        GitHubGraphQlResponse response = await SendAsync(
            ErroredEnvelope,
            ("x-ratelimit-remaining", "4987"),
            ("x-ratelimit-resource", "graphql"));

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
        Assert.False(response.ReportsProviderWideCondition);
    }

    // 6. A null `data` alongside errors is still not provider-wide on its own —
    // the Session 125 ruling is unchanged by this session.
    [Fact]
    public async Task Send_LeavesANullDataEnvelopeUnclassifiedWithoutAQualifyingHeader()
    {
        GitHubGraphQlResponse response = await SendAsync(
            """{ "data": null, "errors": [ { "message": "boom" } ] }""",
            ("x-ratelimit-remaining", "10"));

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.Null(response.Data);
        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
    }

    // A header that cannot be read is not a signal. Guessing here would be the
    // inference this session exists to avoid.
    [Theory]
    [InlineData("x-ratelimit-remaining", "")]
    [InlineData("x-ratelimit-remaining", "not-a-number")]
    [InlineData("x-ratelimit-remaining", "-1")]
    [InlineData("x-ratelimit-remaining", "1")]
    [InlineData("Retry-After", "later")]
    [InlineData("Retry-After", "-5")]
    [InlineData("x-ratelimit-resource", "graphql")]
    [InlineData("x-ratelimit-reset", "1900000000")]
    public async Task Send_TreatsAnUnreadableOrIrrelevantHeaderAsNoSignal(
        string name,
        string value)
    {
        GitHubGraphQlResponse response = await SendAsync(ErroredEnvelope, (name, value));

        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
    }

    // A header sent twice cannot be read as one value, so it is not a signal.
    // The claim is pinned here rather than only asserted in prose.
    [Theory]
    [InlineData("x-ratelimit-remaining", "0", "0")]
    [InlineData("x-ratelimit-remaining", "0", "5")]
    [InlineData("Retry-After", "60", "60")]
    public async Task Send_TreatsARepeatedHeaderAsNoSignal(
        string name,
        string first,
        string second)
    {
        GitHubGraphQlResponse response = await SendAsync(
            ErroredEnvelope,
            (name, first),
            (name, second));

        Assert.Equal(GitHubGraphQlOutcome.DataWithErrors, response.Outcome);
        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
    }

    // ------------------------------ 7. a successful response stays successful

    // The provider saying "zero points left" does not retroactively make a
    // complete, error-free observation a failure. §18.6 disables on failure;
    // pre-emptively disabling on success would be a policy this session does not
    // own, and the cost of not doing it is one request that then fails and
    // disables normally.
    [Fact]
    public async Task Send_DoesNotTurnASuccessfulEnvelopeIntoAFailureOnZeroRemaining()
    {
        GitHubGraphQlResponse response = await SendAsync(
            """{ "data": { "viewer": { "login": "octocat" } } }""",
            ("x-ratelimit-remaining", "0"),
            ("Retry-After", "60"));

        Assert.Equal(GitHubGraphQlOutcome.Success, response.Outcome);
        Assert.True(response.IsSuccess);
        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
        Assert.False(response.ReportsProviderWideCondition);
    }

    // ------------------------------- 8/9/10. existing outcomes are unchanged

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task Send_KeepsNonSuccessStatusesAsEnvelopeFailure(HttpStatusCode statusCode)
    {
        GitHubGraphQlResponse response = await SendAsync(
            """{ "message": "no" }""",
            statusCode,
            ("x-ratelimit-remaining", "0"));

        Assert.Equal(GitHubGraphQlOutcome.EnvelopeFailure, response.Outcome);
        Assert.Equal(statusCode, response.StatusCode);

        // The signal is not needed and not set: a non-200 already disables.
        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{ "data": null }""")]
    public async Task Send_KeepsMalformedBehaviourUnchanged(string body)
    {
        GitHubGraphQlResponse response = await SendAsync(
            body,
            ("x-ratelimit-remaining", "0"));

        Assert.Equal(GitHubGraphQlOutcome.Malformed, response.Outcome);
        Assert.Equal(GitHubGraphQlProviderSignal.None, response.ProviderSignal);
    }

    // -------------------------------------- 14. nothing readable is retained

    // The fixture body carries a message, a path and a type. None of them may
    // survive anywhere on the response, and the signal must come from the header
    // rather than from any of them.
    [Fact]
    public async Task Send_RetainsNoErrorContentWhileClassifying()
    {
        GitHubGraphQlResponse response = await SendAsync(
            ErroredEnvelope,
            ("x-ratelimit-remaining", "0"));

        string surface = string.Join(
            ";",
            typeof(GitHubGraphQlResponse)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => $"{property.Name}={property.GetValue(response)}"))
            + response.Data?.GetRawText();

        Assert.Equal(GitHubGraphQlProviderSignal.RateLimitExhausted, response.ProviderSignal);

        // The payload is retained on purpose, so the walk has real content to
        // sift and the assertions below are not vacuous.
        Assert.Contains("repository", surface, StringComparison.Ordinal);

        foreach (string sentinel in new[]
        {
            "SENTINELMESSAGEapiRateLimitExceeded",
            "SENTINELTYPErateLimited",
            "SENTINELPATHrepositoryOwner"
        })
        {
            Assert.DoesNotContain(sentinel, surface, StringComparison.Ordinal);
        }
    }

    // The classification is structural by construction: the signal type is a
    // closed three-member enum and carries no value of its own.
    [Fact]
    public void TheProviderSignalIsAClosedStructuralVocabulary()
    {
        Assert.Equal(
            [
                nameof(GitHubGraphQlProviderSignal.None),
                nameof(GitHubGraphQlProviderSignal.RateLimitExhausted),
                nameof(GitHubGraphQlProviderSignal.RetryAfterRequested)
            ],
            Enum.GetNames<GitHubGraphQlProviderSignal>());
        Assert.Equal(
            "GitHubProof.Infrastructure.GitHub.Profile",
            typeof(GitHubGraphQlProviderSignal).Namespace);
    }

    // ---------------------------------------------------- 11. cancellation

    [Fact]
    public async Task Send_StillPropagatesCallerCancellationRatherThanClassifyingIt()
    {
        BlockingHandler handler = new();
        GitHubGraphQlTransport transport = new(CreateClient(handler));
        using CancellationTokenSource cancellation = new();

        Task<GitHubGraphQlResponse> pending = transport.SendAsync(Query, cancellation.Token);

        await handler.Started;
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    // ------------------------------------------------------------ fixtures

    private static Task<GitHubGraphQlResponse> SendAsync(
        string body,
        params (string Name, string Value)[] headers) =>
        SendAsync(body, HttpStatusCode.OK, headers);

    private static async Task<GitHubGraphQlResponse> SendAsync(
        string body,
        HttpStatusCode statusCode,
        params (string Name, string Value)[] headers)
    {
        HeaderHandler handler = new(body, statusCode, headers);
        GitHubGraphQlTransport transport = new(CreateClient(handler));

        return await transport.SendAsync(Query);
    }

    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    private sealed class HeaderHandler(
        string body,
        HttpStatusCode statusCode,
        (string Name, string Value)[] headers) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            foreach ((string name, string value) in headers)
            {
                response.Headers.TryAddWithoutValidation(name, value);
            }

            return Task.FromResult(response);
        }
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

using System.Net;
using System.Text.Json;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// The transport-level result of one physical POST /graphql.
//
// It creates NO DataAvailability. §18.7 keeps availability with the collector
// that actually supplies a surface, so mapping this onto an availability status
// is the future surface adapter's job, not the transport's.
//
// `Data` is preserved verbatim for the future §18.5 isolation work, which needs
// the parsed payload to decide what may be retained. GraphQL errors are NOT
// preserved verbatim: they are normalized to a count. Error text, extensions
// and paths are forbidden in telemetry (§18.9), and §18.4 decisions must never
// be made from an error message's wording, so the transport deliberately keeps
// no free text to leak or to be tempted by.
public sealed record GitHubGraphQlResponse
{
    private GitHubGraphQlResponse(
        GitHubGraphQlOutcome outcome,
        JsonElement? data,
        int errorCount,
        GitHubGraphQlRateLimit? rateLimit,
        HttpStatusCode? statusCode,
        GitHubGraphQlProviderSignal providerSignal = GitHubGraphQlProviderSignal.None)
    {
        Outcome = outcome;
        Data = data;
        ErrorCount = errorCount;
        RateLimit = rateLimit;
        StatusCode = statusCode;
        ProviderSignal = providerSignal;
    }

    public GitHubGraphQlOutcome Outcome { get; }

    // Present only when the response body parsed and carried a `data` object.
    // A caller must still respect Outcome: data alongside DataWithErrors is
    // unresolved, never observed truth (§18.4).
    public JsonElement? Data { get; }

    // Structural only. Zero for every outcome except DataWithErrors.
    public int ErrorCount { get; }

    public GitHubGraphQlRateLimit? RateLimit { get; }

    // The provider status for an envelope failure. Null when no response was
    // produced at all.
    public HttpStatusCode? StatusCode { get; }

    // Session 126. Set only on a DataWithErrors envelope, and only from a
    // response header, never from error content. `None` everywhere else — in
    // particular a Success envelope always carries `None`, so a successful
    // response can never be turned into a failure by a header.
    public GitHubGraphQlProviderSignal ProviderSignal { get; }

    // §18.6 acts on this rather than on the outcome alone: an HTTP 200 carrying
    // errors is provider-wide only when the provider itself said so
    // structurally.
    public bool ReportsProviderWideCondition =>
        ProviderSignal != GitHubGraphQlProviderSignal.None;

    // Deliberately narrow: only this outcome permits reading an absence as an
    // observation. Callers should branch on this rather than on `Data is not
    // null`, which would readmit the exact §18.4 failure mode.
    public bool IsSuccess => Outcome == GitHubGraphQlOutcome.Success;

    internal static GitHubGraphQlResponse Success(
        JsonElement data,
        GitHubGraphQlRateLimit? rateLimit) =>
        new(
            GitHubGraphQlOutcome.Success,
            data,
            errorCount: 0,
            rateLimit,
            HttpStatusCode.OK);

    internal static GitHubGraphQlResponse DataWithErrors(
        JsonElement? data,
        int errorCount,
        GitHubGraphQlRateLimit? rateLimit,
        GitHubGraphQlProviderSignal providerSignal =
            GitHubGraphQlProviderSignal.None) =>
        new(
            GitHubGraphQlOutcome.DataWithErrors,
            data,
            errorCount,
            rateLimit,
            HttpStatusCode.OK,
            providerSignal);

    internal static GitHubGraphQlResponse EnvelopeFailure(HttpStatusCode? statusCode) =>
        new(
            GitHubGraphQlOutcome.EnvelopeFailure,
            data: null,
            errorCount: 0,
            rateLimit: null,
            statusCode);

    internal static GitHubGraphQlResponse Malformed(HttpStatusCode? statusCode) =>
        new(
            GitHubGraphQlOutcome.Malformed,
            data: null,
            errorCount: 0,
            rateLimit: null,
            statusCode);
}

using System.Globalization;

namespace GitHubProof.Web.Profile.Runtime;

// Counts GitHub requests issued inside one profile execution and records the
// metadata response's sanitized remaining/reset budget integers.
//
// It observes only the request route shape and the five explicitly named
// X-RateLimit response headers. It never reads/copies an Authorization header,
// never touches request/response bodies and never modifies the message — the
// GitHub transport contract is unchanged.
//
// Session 120 separates those headers by purpose:
//   OBSERVATION (§9 telemetry, every response)
//     Resource, Limit, Remaining, Used, Reset — enough context to tell one
//     rate-limit bucket from another, which is what B-RI needed and what a
//     bare Remaining sequence could not provide (§9.2: guard values come from
//     measurement, not guesses).
//   DECISION (unchanged)
//     Remaining + Reset, profile-metadata response only, recorded solely into
//     the separate ProfileGitHubBudgetState carrier.
// Enriching the observation deliberately does not enrich the decision.
//
// Registered only on the profile-specific named client, so Repository Proof
// traffic never passes through it at all.
public sealed class ProfileGitHubRequestTelemetryHandler : DelegatingHandler
{
    private const string RemainingHeader = "X-RateLimit-Remaining";
    private const string ResetHeader = "X-RateLimit-Reset";
    private const string LimitHeader = "X-RateLimit-Limit";
    private const string UsedHeader = "X-RateLimit-Used";
    private const string ResourceHeader = "X-RateLimit-Resource";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string category = ProfileGitHubRequestCategory.Classify(request.RequestUri);
        ProfileExecutionTelemetry? telemetry = ProfileExecutionTelemetryScope.Current;
        ProfileGitHubBudgetState? budgetState = ProfileGitHubBudgetScope.Current;
        telemetry?.RecordGitHubRequest(category);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        bool hasRemaining =
            TryReadNonNegativeInt32(response, RemainingHeader, out int remaining);
        DateTimeOffset? resetAt = TryReadResetAt(response);

        // Session 120 — B-RI. Every well-formed remaining value is OBSERVED
        // together with the context that identifies WHICH rate-limit bucket it
        // belongs to. Remaining alone could not distinguish a resource, a reset
        // window or a boundary crossing, which is why three benchmarks produced
        // a remaining delta that did not track the measured request count.
        //
        // Still telemetry: it changes no decision, and any missing or malformed
        // field simply yields less context rather than discarding the sample or
        // affecting execution.
        if (hasRemaining)
        {
            telemetry?.RecordRateLimitObservation(
                TryReadResource(response),
                remaining,
                TryReadNonNegativeInt32(response, LimitHeader, out int limit)
                    ? limit
                    : null,
                TryReadNonNegativeInt32(response, UsedHeader, out int used)
                    ? used
                    : null,
                resetAt);
        }

        // The proactive budget DECISION is deliberately left exactly as it was:
        // still the profile-metadata response only, still remaining + reset,
        // still its own separate carrier. Enriching the observation above must
        // not enrich or widen this. Whether this premise is suitable at all is
        // the open D-2 question; Session 120 does not answer it here.
        if (budgetState is not null
            && hasRemaining
            && string.Equals(
                category,
                ProfileGitHubRequestCategory.ProfileMetadata,
                StringComparison.Ordinal))
        {
            budgetState.RecordMetadataObservation(remaining, resetAt);
        }

        return response;
    }

    private static bool TryReadNonNegativeInt32(
        HttpResponseMessage response,
        string headerName,
        out int value)
    {
        value = default;

        return response.Headers.TryGetValues(headerName, out IEnumerable<string>? values)
            && values.Count() == 1
            && int.TryParse(
                values.Single(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out value)
            && value >= 0;
    }

    private static bool TryReadNonNegativeInt64(
        HttpResponseMessage response,
        string headerName,
        out long value)
    {
        value = default;

        return response.Headers.TryGetValues(headerName, out IEnumerable<string>? values)
            && values.Count() == 1
            && long.TryParse(
                values.Single(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out value)
            && value >= 0;
    }

    // Returned as-is for the telemetry layer to normalize: this handler makes no
    // claim about which resource names GitHub uses.
    private static string? TryReadResource(HttpResponseMessage response) =>
        response.Headers.TryGetValues(ResourceHeader, out IEnumerable<string>? values)
            && values.Count() == 1
                ? values.Single()
                : null;

    private static DateTimeOffset? TryReadResetAt(HttpResponseMessage response)
    {
        if (!TryReadNonNegativeInt64(
                response,
                ResetHeader,
                out long resetUnixSeconds)
            || resetUnixSeconds > DateTimeOffset.MaxValue.ToUnixTimeSeconds())
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(resetUnixSeconds);
    }
}

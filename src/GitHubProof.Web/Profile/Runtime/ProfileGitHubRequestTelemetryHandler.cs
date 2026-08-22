using System.Globalization;

namespace GitHubProof.Web.Profile.Runtime;

// Counts GitHub requests issued inside one profile execution and records the
// metadata response's sanitized remaining/reset budget integers.
//
// It observes only the request route shape and the two explicitly named
// X-RateLimit response headers. It never reads/copies an Authorization header,
// never logs the budget values, never touches request/response bodies and never
// modifies the message — the GitHub transport contract is unchanged.
//
// Registered only on the profile-specific named client, so Repository Proof
// traffic never passes through it at all.
public sealed class ProfileGitHubRequestTelemetryHandler : DelegatingHandler
{
    private const string RemainingHeader = "X-RateLimit-Remaining";
    private const string ResetHeader = "X-RateLimit-Reset";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string category = ProfileGitHubRequestCategory.Classify(request.RequestUri);
        ProfileExecutionTelemetry? telemetry = ProfileExecutionTelemetryScope.Current;
        ProfileGitHubBudgetState? budgetState = ProfileGitHubBudgetScope.Current;
        telemetry?.RecordGitHubRequest(category);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (budgetState is not null
            && string.Equals(
                category,
                ProfileGitHubRequestCategory.ProfileMetadata,
                StringComparison.Ordinal)
            && TryReadNonNegativeInt32(response, RemainingHeader, out int remaining))
        {
            DateTimeOffset? resetAt = TryReadResetAt(response);

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

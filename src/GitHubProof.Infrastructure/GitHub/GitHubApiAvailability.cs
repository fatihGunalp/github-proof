using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

internal static class GitHubApiAvailability
{
    public static DataAvailability FromFailureResponse(
        HttpResponseMessage response,
        string dataKey)
    {
        (DataAvailabilityStatus status, string reason) = response.StatusCode switch
        {
            HttpStatusCode.Forbidden when IsRateLimited(response) => (
                DataAvailabilityStatus.RateLimited,
                $"GitHub rate limit was reached while retrieving {dataKey} data."),
            HttpStatusCode.TooManyRequests => (
                DataAvailabilityStatus.RateLimited,
                $"GitHub rate limit was reached while retrieving {dataKey} data."),
            HttpStatusCode.Forbidden => (
                DataAvailabilityStatus.Forbidden,
                $"GitHub denied access to {dataKey} data."),
            HttpStatusCode.NotFound => (
                DataAvailabilityStatus.Unavailable,
                $"GitHub could not retrieve {dataKey} data."),
            _ => (
                DataAvailabilityStatus.Failed,
                $"GitHub returned HTTP {(int)response.StatusCode} ({response.StatusCode}) for {dataKey} data.")
        };

        return DataAvailability.Unavailable(dataKey, status, reason);
    }

    private static bool IsRateLimited(HttpResponseMessage response) =>
        response.Headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? values)
        && values.Any(value => string.Equals(value, "0", StringComparison.Ordinal));
}

namespace GitHubProof.Web.Profile.Runtime;

// Counts GitHub requests issued inside one profile execution.
//
// It observes only the request's route shape. It never reads, copies or logs a
// header (so GITHUB_TOKEN and the Authorization header can never reach
// telemetry), never touches the request or response body, and never modifies
// the message — the GitHub transport contract is exactly what it was.
//
// Registered only on the profile-specific named client, so Repository Proof
// traffic never passes through it at all.
public sealed class ProfileGitHubRequestTelemetryHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ProfileExecutionTelemetryScope.Current?.RecordGitHubRequest(
            ProfileGitHubRequestCategory.Classify(request.RequestUri));

        return base.SendAsync(request, cancellationToken);
    }
}

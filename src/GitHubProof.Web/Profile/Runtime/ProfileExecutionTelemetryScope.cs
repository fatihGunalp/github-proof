namespace GitHubProof.Web.Profile.Runtime;

// Carries the current execution's telemetry along the async flow of one profile
// analysis.
//
// It is deliberately AsyncLocal rather than a scoped or singleton service.
// IHttpClientFactory pools DelegatingHandler instances and reuses them across
// DI scopes and requests, so a handler can never safely hold a per-request
// object. An AsyncLocal value flows into every awaited HTTP call made inside
// the execution and is invisible to any other concurrent execution, which is
// exactly the isolation §9 requires.
//
// Outside a profile execution Current is null and every recorder no-ops, so
// Repository Proof traffic is never counted here.
public static class ProfileExecutionTelemetryScope
{
    private static readonly AsyncLocal<ProfileExecutionTelemetry?> CurrentTelemetry = new();

    public static ProfileExecutionTelemetry? Current => CurrentTelemetry.Value;

    public static IDisposable Begin(ProfileExecutionTelemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        ProfileExecutionTelemetry? previous = CurrentTelemetry.Value;
        CurrentTelemetry.Value = telemetry;

        return new Restore(previous);
    }

    private sealed class Restore(ProfileExecutionTelemetry? previous) : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            CurrentTelemetry.Value = previous;
        }
    }
}

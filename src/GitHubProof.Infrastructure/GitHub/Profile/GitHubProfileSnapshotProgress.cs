using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// Operational progress for one live Profile collection. This carrier exists
// only so the outer Web execution boundary can preserve the truthful profile
// availability if cancellation stops collection after metadata resolution.
// It is never added to a snapshot, warning, analysis or report.
public sealed class GitHubProfileSnapshotProgress : IDisposable
{
    private static readonly object UnresolvedState = new();
    private static readonly object ExecutionCancellationWonState = new();

    private readonly CancellationTokenRegistration cancellationRegistration;
    private object state = UnresolvedState;

    public GitHubProfileSnapshotProgress(
        CancellationToken executionCancellationToken)
    {
        cancellationRegistration = executionCancellationToken.Register(
            static progress =>
                ((GitHubProfileSnapshotProgress)progress!).PublishCancellation(),
            this);
    }

    public DataAvailability? ProfileAvailability =>
        Volatile.Read(ref state) is ResolvedState resolved
            ? resolved.Availability
            : null;

    public bool ExecutionCancellationWonBeforeResolution =>
        ReferenceEquals(
            Volatile.Read(ref state),
            ExecutionCancellationWonState);

    // Returns false only when execution cancellation already won the one
    // authoritative unresolved-state transition. The late value then never
    // becomes visible. A resolved value is embedded in the winning state object,
    // so cancellation cannot observe "resolved" before the value is published.
    public bool ResolveProfileAvailability(DataAvailability availability)
    {
        ArgumentNullException.ThrowIfNull(availability);

        object resolved = new ResolvedState(availability);
        object prior = Interlocked.CompareExchange(
            ref state,
            resolved,
            UnresolvedState);

        if (ReferenceEquals(prior, UnresolvedState))
        {
            return true;
        }

        if (ReferenceEquals(prior, ExecutionCancellationWonState))
        {
            return false;
        }

        throw new InvalidOperationException(
            "Profile availability can be resolved only once per collection.");
    }

    private void PublishCancellation()
    {
        Interlocked.CompareExchange(
            ref state,
            ExecutionCancellationWonState,
            UnresolvedState);
    }

    public void Dispose()
    {
        cancellationRegistration.Dispose();
    }

    private sealed record ResolvedState(DataAvailability Availability);
}

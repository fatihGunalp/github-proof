namespace GitHubProof.Web.Profile.Runtime;

public sealed record ProfileGitHubBudgetObservation(
    int Remaining,
    DateTimeOffset? ResetAt);

// Operational control state for one live Profile execution. This is separate
// from telemetry by design: telemetry observes execution and is never read to
// make a runtime decision, while this narrowly scoped carrier exists only for
// the Profile-only continuation guard.
public sealed class ProfileGitHubBudgetState
{
    private ProfileGitHubBudgetObservation? metadataObservation;

    public ProfileGitHubBudgetObservation? MetadataObservation =>
        Volatile.Read(ref metadataObservation);

    public void RecordMetadataObservation(
        int remaining,
        DateTimeOffset? resetAt) =>
        Interlocked.Exchange(
            ref metadataObservation,
            new ProfileGitHubBudgetObservation(remaining, resetAt));
}

public static class ProfileGitHubBudgetScope
{
    private static readonly AsyncLocal<ProfileGitHubBudgetState?> CurrentState = new();

    public static ProfileGitHubBudgetState? Current => CurrentState.Value;

    public static IDisposable Begin(ProfileGitHubBudgetState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        ProfileGitHubBudgetState? previous = CurrentState.Value;
        CurrentState.Value = state;
        return new Restore(previous);
    }

    private sealed class Restore(ProfileGitHubBudgetState? previous) : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            CurrentState.Value = previous;
        }
    }
}

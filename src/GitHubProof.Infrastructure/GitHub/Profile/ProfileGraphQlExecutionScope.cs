namespace GitHubProof.Infrastructure.GitHub.Profile;

// Operational state for GraphQL within ONE Profile execution
// (docs/architecture/public-profile-runtime-collection.md §18.6, §18.9).
//
// It is not evidence and it is not read by any evaluator, policy, composer,
// projector or report section. It carries two kinds of thing: whether §18.6 has
// disabled GraphQL for the rest of this execution, and bounded outcome counts.
// It never holds a repository identity, a language name, a byte count, query
// text, a payload or any error content.
//
// Session 127 renames the Session 125 type rather than adding a second one.
// §18.6 authorizes exactly one run-scoped disable, and once an authoritative
// repository envelope (S3) exists alongside the languages shadow (S2), a
// languages-specific name would describe only half of what the state governs.
// The disable semantics, the shadow counters and the single-execution lifetime
// are unchanged.
public sealed class ProfileGraphQlExecutionState
{
    private int disabledForRun;
    private int match;
    private int mismatch;
    private int notCompared;
    private int envelopeServed;
    private int envelopeFallback;

    // §18.6: set by an envelope-level failure and never cleared. The disable is
    // scoped to this state object, which lives for exactly one execution, so it
    // cannot be persisted, shared or remembered.
    public bool IsDisabledForRun => Volatile.Read(ref disabledForRun) != 0;

    public int Match => Volatile.Read(ref match);

    public int Mismatch => Volatile.Read(ref mismatch);

    public int NotCompared => Volatile.Read(ref notCompared);

    // Session 127 (S3). Two bounded structural counters for the authoritative
    // repository envelope. They are NOT HTTP request counts and must never be
    // summed with one (§18.9 counting rule): a served envelope is one accepted
    // candidate, and a fallback is one repository collected over REST instead.
    public int RepositoryEnvelopeServed => Volatile.Read(ref envelopeServed);

    public int RepositoryEnvelopeFallback => Volatile.Read(ref envelopeFallback);

    public int Observations => Match + Mismatch + NotCompared;

    public bool HasObservations => Observations > 0 || IsDisabledForRun;

    public void DisableForRun() => Interlocked.Exchange(ref disabledForRun, 1);

    public void RecordEnvelopeServed() => Interlocked.Increment(ref envelopeServed);

    public void RecordEnvelopeFallback() => Interlocked.Increment(ref envelopeFallback);

    public void Record(GraphQlLanguagesShadowOutcome outcome)
    {
        switch (outcome)
        {
            case GraphQlLanguagesShadowOutcome.Match:
                Interlocked.Increment(ref match);
                break;

            case GraphQlLanguagesShadowOutcome.Mismatch:
                Interlocked.Increment(ref mismatch);
                break;

            default:
                Interlocked.Increment(ref notCompared);
                break;
        }
    }
}

// The ambient execution scope, mirroring ProfileGitHubBudgetScope: the state
// belongs to one AnalyzeAsync call and is restored on dispose, so a second
// execution on the same DI scope — or on the same service instance — starts
// from a fresh state and may use GraphQL again (§18.6: "scoped to a single
// execution … not persisted, not shared between executions and not
// remembered").
public static class ProfileGraphQlExecutionScope
{
    private static readonly AsyncLocal<ProfileGraphQlExecutionState?> CurrentState = new();

    public static ProfileGraphQlExecutionState? Current => CurrentState.Value;

    public static IDisposable Begin(ProfileGraphQlExecutionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        ProfileGraphQlExecutionState? previous = CurrentState.Value;
        CurrentState.Value = state;

        return new Restore(previous);
    }

    private sealed class Restore(ProfileGraphQlExecutionState? previous) : IDisposable
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

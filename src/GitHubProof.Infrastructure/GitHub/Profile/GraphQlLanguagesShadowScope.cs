namespace GitHubProof.Infrastructure.GitHub.Profile;

// Operational state for the languages shadow within ONE Profile execution
// (docs/architecture/public-profile-runtime-collection.md §18.6, §18.9).
//
// It is not evidence and it is not read by any evaluator, policy, composer,
// projector or report section. It carries two things: whether §18.6 has
// disabled the shadow for the rest of this execution, and bounded outcome
// counts. It never holds a repository identity, a language name, a byte count,
// query text, a payload or any error content.
public sealed class GraphQlLanguagesShadowState
{
    private int disabledForRun;
    private int match;
    private int mismatch;
    private int notCompared;

    // §18.6: set by an envelope-level failure and never cleared. The disable is
    // scoped to this state object, which lives for exactly one execution, so it
    // cannot be persisted, shared or remembered.
    public bool IsDisabledForRun => Volatile.Read(ref disabledForRun) != 0;

    public int Match => Volatile.Read(ref match);

    public int Mismatch => Volatile.Read(ref mismatch);

    public int NotCompared => Volatile.Read(ref notCompared);

    public int Observations => Match + Mismatch + NotCompared;

    public bool HasObservations => Observations > 0 || IsDisabledForRun;

    public void DisableForRun() => Interlocked.Exchange(ref disabledForRun, 1);

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
public static class GraphQlLanguagesShadowScope
{
    private static readonly AsyncLocal<GraphQlLanguagesShadowState?> CurrentState = new();

    public static GraphQlLanguagesShadowState? Current => CurrentState.Value;

    public static IDisposable Begin(GraphQlLanguagesShadowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        GraphQlLanguagesShadowState? previous = CurrentState.Value;
        CurrentState.Value = state;

        return new Restore(previous);
    }

    private sealed class Restore(GraphQlLanguagesShadowState? previous) : IDisposable
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

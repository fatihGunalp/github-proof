using System.Collections.Concurrent;

namespace GitHubProof.Web.Progress;

// The process-local singleton run registry
// (live-analysis-progress-runtime-contract.md §12). In-memory only, not
// persisted, not shared between instances, not durable. It stores nothing
// beyond a correlation token, the target kind, the active/completed stage
// state and a last-updated instant; the submitted target string, any login,
// repository name, URL, availability, count, provider detail, error text,
// timing or evidence never enter it.
//
// Registration is performed only by a page model on the analysis path
// (IndexModel / ProfileModel); the observation endpoint never creates an entry.
// Registry pressure — the entry cap or the TTL — degrades only the progress
// enhancement and never an analysis (§12, §13.1).
public sealed class AnalysisProgressRegistry
{
    // Exact and locked (§13, §13.1). None configurable.
    public const int MaxEntries = 64;
    public static readonly TimeSpan AbsoluteTtl = TimeSpan.FromMinutes(10);

    private readonly TimeProvider clock;
    private readonly ConcurrentDictionary<Guid, AnalysisProgressRun> runs = new();
    private readonly object registrationGate = new();

    public AnalysisProgressRegistry(TimeProvider clock)
    {
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    // Parses the client-supplied token and registers a run with its first stage
    // already ACTIVE. Returns null — progress disabled, analysis unaffected —
    // when the token is missing or not a canonical UUID, when a run for that
    // token is already live, or when the entry cap is exhausted.
    public AnalysisProgressRun? TryBegin(
        string? correlationToken,
        AnalysisProgressTargetKind targetKind)
    {
        return Guid.TryParseExact(correlationToken, "D", out Guid token)
            ? TryBegin(token, targetKind)
            : null;
    }

    public AnalysisProgressRun? TryBegin(
        Guid correlationToken,
        AnalysisProgressTargetKind targetKind)
    {
        EvictExpired();

        lock (registrationGate)
        {
            if (runs.Count >= MaxEntries)
            {
                return null;
            }

            AnalysisProgressRun run = new(
                correlationToken,
                targetKind,
                this,
                clock);

            // A registration for a token that is already live is refused; a
            // duplicate token may never displace or join an existing run (§8).
            return runs.TryAdd(correlationToken, run)
                ? run
                : null;
        }
    }

    public bool TryGetSnapshot(
        Guid correlationToken,
        out AnalysisProgressSnapshot snapshot)
    {
        EvictExpired();

        if (runs.TryGetValue(correlationToken, out AnalysisProgressRun? run))
        {
            snapshot = run.CurrentSnapshot;
            return true;
        }

        snapshot = null!;
        return false;
    }

    // The number of live entries. Bounded by MaxEntries.
    public int Count => runs.Count;

    internal void Release(Guid correlationToken) =>
        runs.TryRemove(correlationToken, out _);

    // A backstop only (§12): in normal operation the page model's `using` always
    // wins. An entry idle past the absolute TTL is removed; the next poll then
    // answers `known:false` and the client stops polling per §9.
    private void EvictExpired()
    {
        DateTimeOffset now = clock.GetUtcNow();

        foreach (KeyValuePair<Guid, AnalysisProgressRun> entry in runs)
        {
            if (now - entry.Value.LastUpdatedUtc > AbsoluteTtl)
            {
                runs.TryRemove(entry.Key, out _);
            }
        }
    }
}

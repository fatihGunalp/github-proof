namespace GitHubProof.Web.Progress;

// A single live run's handle, held by the page model that performs the analysis
// (IndexModel or ProfileModel) and released in a `finally`/`using` on every
// path (live-analysis-progress-runtime-contract.md §9).
//
// The analysis path calls CompleteCurrentStage() once at each observable
// completion point, in order. Registration itself creates the run with its
// first stage already ACTIVE (§8): that is the initial snapshot, not a
// transition. When the final stage completes, the published snapshot is the
// mandatory coherent terminal one — every stage completed, no active stage
// (§7.1).
//
// The client never advances a stage; it renders exactly the last snapshot the
// server reported (§7).
public sealed class AnalysisProgressRun : IDisposable
{
    private readonly AnalysisProgressRegistry owner;
    private readonly TimeProvider clock;
    private readonly IReadOnlyList<string> stages;
    private readonly object gate = new();

    private int completedCount;
    private AnalysisProgressSnapshot snapshot;
    private bool released;
    private long lastUpdatedUtcTicks;

    internal AnalysisProgressRun(
        Guid token,
        AnalysisProgressTargetKind targetKind,
        AnalysisProgressRegistry owner,
        TimeProvider clock)
    {
        Token = token;
        TargetKind = targetKind;
        this.owner = owner;
        this.clock = clock;
        stages = AnalysisProgressStages.For(targetKind);
        completedCount = 0;
        snapshot = BuildSnapshot();
    }

    public Guid Token { get; }

    public AnalysisProgressTargetKind TargetKind { get; }

    // TTL eviction only (§12). Never transported, never rendered. Read by the
    // registry's backstop sweep without taking `gate`, so it is published
    // through a volatile 64-bit tick count rather than a struct field.
    internal DateTimeOffset LastUpdatedUtc =>
        new(Volatile.Read(ref lastUpdatedUtcTicks), TimeSpan.Zero);

    public AnalysisProgressSnapshot CurrentSnapshot
    {
        get
        {
            lock (gate)
            {
                return snapshot;
            }
        }
    }

    // The active stage has finished. One coherent snapshot is published: the
    // stage moves into `completed` and its successor becomes `active`, or — when
    // it was the last stage — the terminal snapshot is published (§7.1).
    public void CompleteCurrentStage()
    {
        lock (gate)
        {
            if (released)
            {
                return;
            }

            if (completedCount >= stages.Count)
            {
                throw new InvalidOperationException(
                    "Every analysis progress stage is already completed.");
            }

            completedCount++;
            snapshot = BuildSnapshot();
        }
    }

    private AnalysisProgressSnapshot BuildSnapshot()
    {
        Volatile.Write(ref lastUpdatedUtcTicks, clock.GetUtcNow().UtcTicks);

        string? active = completedCount < stages.Count
            ? stages[completedCount]
            : null;

        string[] completed = new string[completedCount];
        for (int index = 0; index < completedCount; index++)
        {
            completed[index] = stages[index];
        }

        return new AnalysisProgressSnapshot(TargetKind, active, completed);
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (released)
            {
                return;
            }

            released = true;
        }

        owner.Release(Token);
    }
}

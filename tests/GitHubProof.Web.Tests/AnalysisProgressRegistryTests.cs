using GitHubProof.Web.Progress;
using Xunit;

namespace GitHubProof.Web.Tests;

// live-analysis-progress-runtime-contract.md §7, §7.1, §8, §12, §13.
public sealed class AnalysisProgressRegistryTests
{
    private static AnalysisProgressRegistry NewRegistry(TimeProvider? clock = null) =>
        new(clock ?? TimeProvider.System);

    [Fact]
    public void Registration_creates_the_run_with_its_first_stage_already_active()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        Guid token = Guid.NewGuid();

        AnalysisProgressRun? run = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Repository);

        Assert.NotNull(run);
        AnalysisProgressSnapshot snapshot = run!.CurrentSnapshot;
        Assert.Equal(AnalysisProgressTargetKind.Repository, snapshot.TargetKind);
        Assert.Equal(AnalysisProgressStages.RepositoryTargetValidated, snapshot.ActiveStage);
        Assert.Empty(snapshot.CompletedStages);
    }

    [Fact]
    public void Repository_advances_through_exactly_four_stages_in_locked_order()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        AnalysisProgressRun run = registry.TryBegin(
            Guid.NewGuid(),
            AnalysisProgressTargetKind.Repository)!;

        string[] order =
        [
            AnalysisProgressStages.RepositoryTargetValidated,
            AnalysisProgressStages.RepositoryPublicDataRead,
            AnalysisProgressStages.RepositoryTracesAnalyzed,
            AnalysisProgressStages.RepositoryProofPrepared
        ];

        for (int completed = 0; completed < order.Length; completed++)
        {
            AnalysisProgressSnapshot snapshot = run.CurrentSnapshot;

            // The invariant, at every observable instant: `completed` is exactly
            // the stages preceding `active`, with no gap and no overlap (§7.1).
            Assert.Equal(order[..completed], snapshot.CompletedStages);
            Assert.Equal(order[completed], snapshot.ActiveStage);

            run.CompleteCurrentStage();
        }

        AnalysisProgressSnapshot terminal = run.CurrentSnapshot;
        Assert.Null(terminal.ActiveStage);
        Assert.Equal(order, terminal.CompletedStages);
    }

    [Fact]
    public void Profile_advances_through_exactly_three_stages_in_locked_order()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        AnalysisProgressRun run = registry.TryBegin(
            Guid.NewGuid(),
            AnalysisProgressTargetKind.Profile)!;

        string[] order =
        [
            AnalysisProgressStages.ProfileTargetValidated,
            AnalysisProgressStages.ProfilePublicSurfaceRead,
            AnalysisProgressStages.ProfileEvidenceExamined
        ];

        run.CompleteCurrentStage();
        Assert.Equal(AnalysisProgressStages.ProfilePublicSurfaceRead, run.CurrentSnapshot.ActiveStage);
        Assert.Equal(new[] { order[0] }, run.CurrentSnapshot.CompletedStages);

        run.CompleteCurrentStage();
        run.CompleteCurrentStage();

        AnalysisProgressSnapshot terminal = run.CurrentSnapshot;
        Assert.Null(terminal.ActiveStage);
        Assert.Equal(order, terminal.CompletedStages);
    }

    [Fact]
    public void Terminal_snapshot_is_all_completed_and_no_active_stage()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        AnalysisProgressRun run = registry.TryBegin(
            Guid.NewGuid(),
            AnalysisProgressTargetKind.Profile)!;

        run.CompleteCurrentStage();
        run.CompleteCurrentStage();
        run.CompleteCurrentStage();

        AnalysisProgressSnapshot terminal = run.CurrentSnapshot;
        Assert.Null(terminal.ActiveStage);
        Assert.Equal(
            AnalysisProgressStages.For(AnalysisProgressTargetKind.Profile),
            terminal.CompletedStages);
    }

    [Fact]
    public void Completing_a_stage_after_the_terminal_snapshot_throws()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        AnalysisProgressRun run = registry.TryBegin(
            Guid.NewGuid(),
            AnalysisProgressTargetKind.Profile)!;

        run.CompleteCurrentStage();
        run.CompleteCurrentStage();
        run.CompleteCurrentStage();

        Assert.Throws<InvalidOperationException>(run.CompleteCurrentStage);
    }

    [Fact]
    public void Published_snapshots_are_immutable_and_replaced_not_mutated()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        AnalysisProgressRun run = registry.TryBegin(
            Guid.NewGuid(),
            AnalysisProgressTargetKind.Repository)!;

        AnalysisProgressSnapshot first = run.CurrentSnapshot;
        run.CompleteCurrentStage();
        AnalysisProgressSnapshot second = run.CurrentSnapshot;

        Assert.NotSame(first, second);
        // The earlier snapshot instance was not mutated in place.
        Assert.Empty(first.CompletedStages);
        Assert.Single(second.CompletedStages);
    }

    [Fact]
    public void A_duplicate_live_token_is_refused_and_never_joins_the_existing_run()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        Guid token = Guid.NewGuid();

        AnalysisProgressRun first = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Repository)!;
        first.CompleteCurrentStage();

        AnalysisProgressRun? duplicate = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Profile);

        Assert.Null(duplicate);
        // The original run is untouched — still repository, still at stage 2.
        Assert.True(registry.TryGetSnapshot(token, out AnalysisProgressSnapshot snapshot));
        Assert.Equal(AnalysisProgressTargetKind.Repository, snapshot.TargetKind);
        Assert.Equal(AnalysisProgressStages.RepositoryPublicDataRead, snapshot.ActiveStage);
    }

    [Fact]
    public void The_entry_cap_refuses_registration_only_and_the_existing_entries_are_unaffected()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        List<AnalysisProgressRun> live = [];

        for (int index = 0; index < AnalysisProgressRegistry.MaxEntries; index++)
        {
            AnalysisProgressRun? run = registry.TryBegin(
                Guid.NewGuid(),
                AnalysisProgressTargetKind.Repository);
            Assert.NotNull(run);
            live.Add(run!);
        }

        Assert.Null(registry.TryBegin(Guid.NewGuid(), AnalysisProgressTargetKind.Repository));
        Assert.Equal(AnalysisProgressRegistry.MaxEntries, registry.Count);

        // Releasing one frees exactly one slot.
        live[0].Dispose();
        Assert.NotNull(registry.TryBegin(Guid.NewGuid(), AnalysisProgressTargetKind.Repository));
    }

    [Fact]
    public void An_entry_idle_past_the_ten_minute_ttl_is_evicted_and_polls_answer_unknown()
    {
        MutableTimeProvider clock = new(DateTimeOffset.Parse("2026-08-27T00:00:00Z"));
        AnalysisProgressRegistry registry = NewRegistry(clock);
        Guid token = Guid.NewGuid();

        registry.TryBegin(token, AnalysisProgressTargetKind.Profile);
        Assert.True(registry.TryGetSnapshot(token, out _));

        clock.Advance(AnalysisProgressRegistry.AbsoluteTtl + TimeSpan.FromSeconds(1));

        Assert.False(registry.TryGetSnapshot(token, out _));
    }

    [Fact]
    public void Disposing_a_run_releases_its_entry()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        Guid token = Guid.NewGuid();

        AnalysisProgressRun run = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Repository)!;
        Assert.True(registry.TryGetSnapshot(token, out _));

        run.Dispose();

        Assert.False(registry.TryGetSnapshot(token, out _));
        // Idempotent.
        run.Dispose();
    }

    [Fact]
    public void A_malformed_or_missing_token_string_registers_nothing()
    {
        AnalysisProgressRegistry registry = NewRegistry();

        Assert.Null(registry.TryBegin((string?)null, AnalysisProgressTargetKind.Repository));
        Assert.Null(registry.TryBegin("", AnalysisProgressTargetKind.Repository));
        Assert.Null(registry.TryBegin("not-a-uuid", AnalysisProgressTargetKind.Repository));
        Assert.Null(registry.TryBegin(
            "{" + Guid.NewGuid().ToString() + "}",
            AnalysisProgressTargetKind.Repository));
        Assert.Equal(0, registry.Count);
    }

    [Fact]
    public void A_canonical_uuid_string_registers_a_run()
    {
        AnalysisProgressRegistry registry = NewRegistry();
        string token = Guid.NewGuid().ToString("D");

        AnalysisProgressRun? run = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Profile);

        Assert.NotNull(run);
    }

    private sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan by) => now += by;
    }
}

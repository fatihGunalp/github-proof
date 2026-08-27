namespace GitHubProof.Web.Progress;

// One immutable, coherent view of a live run
// (live-analysis-progress-runtime-contract.md §7.1). A transition publishes a
// new snapshot value in a single atomic replacement; a reader takes the current
// value once and never observes an intermediate state.
//
// Invariant, at every observable instant: `CompletedStages` is exactly the
// stages preceding `ActiveStage` in the fixed order, with no gap and no
// overlap. `ActiveStage` is `null` only at the terminal snapshot, where
// `CompletedStages` is every stage.
public sealed record AnalysisProgressSnapshot(
    AnalysisProgressTargetKind TargetKind,
    string? ActiveStage,
    IReadOnlyList<string> CompletedStages);

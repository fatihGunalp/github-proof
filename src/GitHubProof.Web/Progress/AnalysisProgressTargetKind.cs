namespace GitHubProof.Web.Progress;

// Which of the two analysis flows a live run belongs to. The client cannot know
// this — classification is server-side — so it is the one thing the progress
// snapshot must carry beyond the stage state itself
// (live-analysis-progress-runtime-contract.md §12).
public enum AnalysisProgressTargetKind
{
    Repository,
    Profile
}

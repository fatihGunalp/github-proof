using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;

namespace GitHubProof.Domain.Analysis.Collection;

// The decision-stop seam contracted by
// docs/architecture/public-profile-runtime-collection.md §15.
//
// A collector may know only this: "after observing this normalized prefix, the
// supplied decision boundary says collection may stop." It never restates a
// signal threshold. Every predicate below delegates to the canonical evaluator
// that owns the threshold, so exactly one copy of each number exists.
//
// A composition that supplies no boundary enumerates fully — that is how
// Repository Proof keeps its behavior without any flag of its own.
public delegate bool CommitCollectionDecisionBoundary(
    IReadOnlyList<RepositoryCommit> observedCommits);

public delegate bool CountCollectionDecisionBoundary(int observedQualifyingCount);

public static class RepositoryCollectionDecisionBoundary
{
    // Commits are bound by BOTH commit signals together: stopping is only safe
    // once neither CommitHistory nor CommitDistribution can still change.
    public static bool AreCommitDecisionsIrreversible(
        IReadOnlyList<RepositoryCommit> observedCommits)
    {
        ArgumentNullException.ThrowIfNull(observedCommits);

        if (!CommitHistorySignalEvaluator.IsStrongThresholdReached(
                observedCommits.Count))
        {
            return false;
        }

        (int activeDayCount, TimeSpan commitSpan) =
            CommitDistributionSignalEvaluator.Observe(observedCommits);

        return CommitDistributionSignalEvaluator.IsStrongThresholdReached(
            activeDayCount,
            commitSpan);
    }

    public static bool IsIssueDecisionIrreversible(int observedQualifyingCount) =>
        IssueUsageSignalEvaluator.IsTerminalThresholdReached(observedQualifyingCount);

    public static bool IsPullRequestDecisionIrreversible(int observedMergedCount) =>
        PullRequestUsageSignalEvaluator.IsTerminalThresholdReached(observedMergedCount);

    public static bool IsReleaseDecisionIrreversible(
        int observedPublishedWithNotesCount) =>
        ReleaseUsageSignalEvaluator.IsTerminalThresholdReached(
            observedPublishedWithNotesCount);

    public static bool IsTagDecisionIrreversible(int observedTagCount) =>
        ReleaseUsageSignalEvaluator.IsTagContributionPinned(observedTagCount);
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

// docs/architecture/mvp-signal-definitions.md §7/§8 "Session 098 istisnası":
// a DecisionBounded collection may carry the terminal Strong result because the
// observed quantities are monotone lower bounds. An Incomplete collection with
// the identical observed prefix may not — that distinction is the whole point.
public sealed class CommitSignalDecisionBoundedTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");

    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");

    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    // ------------------------------------------------------- commit history

    [Fact]
    public void CommitHistory_DecisionBoundedAtTheStrongThreshold_IsTerminalStrong()
    {
        SignalResult result = EvaluateHistory(
            CreateSnapshot(CreateCommits(10), CollectionObservationState.DecisionBounded));

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(7, result.Score);
    }

    [Fact]
    public void CommitHistory_DecisionBoundedEvidence_UsesLowerBoundVocabulary()
    {
        SignalResult result = EvaluateHistory(
            CreateSnapshot(CreateCommits(12), CollectionObservationState.DecisionBounded));

        string value = Assert.Single(result.Evidence).Value;

        Assert.Contains("observedCountAtLeast=12", value, StringComparison.Ordinal);
        Assert.Contains("observation=decision-bounded", value, StringComparison.Ordinal);
        Assert.DoesNotContain("observedCount=", value, StringComparison.Ordinal);
        Assert.DoesNotContain("complete=", value, StringComparison.Ordinal);
    }

    // An identical observed prefix that stopped because something FAILED keeps
    // today's semantics exactly.
    [Fact]
    public void CommitHistory_IncompleteWithTheSamePrefix_IsStillUnavailableEvidence()
    {
        SignalResult result = EvaluateHistory(
            CreateSnapshot(CreateCommits(10), CollectionObservationState.Incomplete));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    // Below the Strong threshold the exact count still matters, so a bounded
    // collection cannot produce a terminal score.
    [Fact]
    public void CommitHistory_DecisionBoundedBelowTheThreshold_IsUnavailableEvidence()
    {
        SignalResult result = EvaluateHistory(
            CreateSnapshot(CreateCommits(9), CollectionObservationState.DecisionBounded));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void CommitHistory_EnumeratedBehavior_IsUnchanged()
    {
        SignalResult result = EvaluateHistory(
            CreateSnapshot(CreateCommits(10), CollectionObservationState.Enumerated));

        string value = Assert.Single(result.Evidence).Value;

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(7, result.Score);
        Assert.Contains("observedCount=10", value, StringComparison.Ordinal);
        Assert.Contains("complete=True", value, StringComparison.Ordinal);
        Assert.DoesNotContain("AtLeast", value, StringComparison.Ordinal);
    }

    // --------------------------------------------------- commit distribution

    [Fact]
    public void CommitDistribution_DecisionBoundedAtTheStrongThreshold_IsTerminalStrong()
    {
        SignalResult result = EvaluateDistribution(
            CreateSnapshot(
                CreateCommits(10),
                CollectionObservationState.DecisionBounded));

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void CommitDistribution_DecisionBoundedEvidence_UsesLowerBoundVocabulary()
    {
        SignalResult result = EvaluateDistribution(
            CreateSnapshot(
                CreateCommits(10),
                CollectionObservationState.DecisionBounded));

        string value = Assert.Single(result.Evidence).Value;

        Assert.Contains("activeDaysAtLeast=10", value, StringComparison.Ordinal);
        Assert.Contains("spanDaysAtLeast=", value, StringComparison.Ordinal);
        Assert.Contains("firstObservedCommitAt=", value, StringComparison.Ordinal);
        Assert.Contains("observation=decision-bounded", value, StringComparison.Ordinal);

        // Never claims the observed oldest commit is the repository's first.
        Assert.DoesNotContain("firstCommitAt=", value, StringComparison.Ordinal);
        Assert.DoesNotContain("commitsComplete=", value, StringComparison.Ordinal);
    }

    [Fact]
    public void CommitDistribution_IncompleteWithTheSamePrefix_IsStillUnavailableEvidence()
    {
        SignalResult result = EvaluateDistribution(
            CreateSnapshot(CreateCommits(10), CollectionObservationState.Incomplete));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    // Ten commits all on one day: the span and active-day thresholds are not
    // reached, so no terminal score is available from a bounded collection.
    [Fact]
    public void CommitDistribution_DecisionBoundedBelowTheThreshold_IsUnavailableEvidence()
    {
        RepositoryCommit[] sameDay =
        [
            .. Enumerable.Range(0, 10).Select(index => new RepositoryCommit(
                $"sha-{index}",
                CapturedAt.AddMinutes(-index),
                $"commit {index}",
                "octocat"))
        ];

        SignalResult result = EvaluateDistribution(
            CreateSnapshot(sameDay, CollectionObservationState.DecisionBounded));

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    // ------------------------------------------------------ boundary helpers

    // The count half of the combined boundary. The distribution half — count
    // reached but active days/span not — is proven in
    // DecisionBoundedCollectionTests.Commits_WhenTheConditionIsNeverReached_EnumeratesEveryPage.
    [Fact]
    public void CommitBoundary_IsNotIrreversibleBelowTheCommitCountThreshold()
    {
        Assert.False(
            RepositoryCollectionDecisionBoundaryProbe(CreateCommits(9)));
        Assert.True(
            RepositoryCollectionDecisionBoundaryProbe(CreateCommits(10)));
    }

    private static bool RepositoryCollectionDecisionBoundaryProbe(
        IReadOnlyList<RepositoryCommit> commits) =>
        GitHubProof.Domain.Analysis.Collection.RepositoryCollectionDecisionBoundary
            .AreCommitDecisionsIrreversible(commits);

    // ---------------------------------------------------------------- setup

    // One commit per day going back from the capture time, so ten commits give
    // ten distinct UTC days and a nine-day span... which is deliberately not
    // enough on its own; the helper below spreads them over more than 14 days.
    private static RepositoryCommit[] CreateCommits(int count) =>
        [
            .. Enumerable.Range(0, count).Select(index => new RepositoryCommit(
                $"sha-{index}",
                CapturedAt.AddDays(-index * 2),
                $"commit {index}",
                "octocat"))
        ];

    private static SignalResult EvaluateHistory(RepositorySnapshot snapshot) =>
        new CommitHistorySignalEvaluator(new FixedTimeProvider(EvaluatedAt))
            .Evaluate(snapshot);

    private static SignalResult EvaluateDistribution(RepositorySnapshot snapshot) =>
        new CommitDistributionSignalEvaluator(new FixedTimeProvider(EvaluatedAt))
            .Evaluate(snapshot);

    private static RepositorySnapshot CreateSnapshot(
        IReadOnlyList<RepositoryCommit> commits,
        CollectionObservationState observationState)
    {
        RepositoryCollection<RepositoryCommit> collection = observationState switch
        {
            CollectionObservationState.Enumerated =>
                new RepositoryCollection<RepositoryCommit>(commits),

            CollectionObservationState.DecisionBounded =>
                RepositoryCollection<RepositoryCommit>.DecisionBounded(commits),

            _ => new RepositoryCollection<RepositoryCommit>(commits, isComplete: false)
        };

        return new RepositorySnapshot(
            new RepositoryIdentity(
                "owner",
                "repository",
                new Uri("https://github.com/owner/repository"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            new RepositoryMetadata(null, null, CapturedAt.AddYears(-2), issuesEnabled: true),
            readme: null,
            fileTree: null,
            collection,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            [
                DataAvailability.Available("metadata", RetrievedAt),
                DataAvailability.Available("commits", RetrievedAt)
            ],
            collectionWarnings: null);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class PullRequestUsageSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.pull-request-usage";

    // The canonical terminal threshold. It lives here and nowhere else.
    public const int TerminalMergedPullRequestCount = 2;

    private const string PullRequestsDataKey = "pull-requests";
    private readonly TimeProvider _timeProvider;

    public PullRequestUsageSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.DevelopmentProcessVisibility,
        maximumScore: 6,
        requiredDataKeys: [PullRequestsDataKey],
        includeInOverallScore: true,
        version: 1);

    // Deterministic and side-effect-free. The observed merged count only
    // grows, and Strong/6 is this signal's maximum, so once this returns true
    // no further page can change the result.
    public static bool IsTerminalThresholdReached(int observedMergedCount) =>
        observedMergedCount >= TerminalMergedPullRequestCount;

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(PullRequestsDataKey);
        int mergedCount = snapshot.PullRequests.Items.Count(pullRequest => pullRequest.IsMerged);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (IsTerminalThresholdReached(mergedCount))
        {
            return CreateResult(
                SignalStatus.Strong,
                score: 6,
                evidence,
                evaluatedAt);
        }

        if (availability?.IsAvailable != true || !snapshot.PullRequests.IsComplete)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                evaluatedAt);
        }

        if (mergedCount == 1)
        {
            return CreateResult(
                SignalStatus.Present,
                score: 4,
                evidence,
                evaluatedAt);
        }

        if (snapshot.PullRequests.Items.Count > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 2,
                evidence,
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            evaluatedAt);
    }

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability)
    {
        int mergedCount = snapshot.PullRequests.Items.Count(pullRequest => pullRequest.IsMerged);
        int openCount = snapshot.PullRequests.Items.Count(pullRequest => !pullRequest.IsClosed);
        int closedUnmergedCount = snapshot.PullRequests.Items.Count(
            pullRequest => pullRequest.IsClosed && !pullRequest.IsMerged);
        string state = availability is null
            ? "not-recorded"
            : availability.Status.ToString();
        // A bounded observation reports lower bounds and never a total.
        string value = snapshot.PullRequests.IsDecisionBounded
            ? $"totalAtLeast={Format(snapshot.PullRequests.Items.Count)};mergedAtLeast={Format(mergedCount)};openAtLeast={Format(openCount)};closedUnmergedAtLeast={Format(closedUnmergedCount)};observation=decision-bounded;availability={state}"
            : $"total={Format(snapshot.PullRequests.Items.Count)};merged={Format(mergedCount)};open={Format(openCount)};closedUnmerged={Format(closedUnmergedCount)};complete={snapshot.PullRequests.IsComplete.ToString(CultureInfo.InvariantCulture)};availability={state}";

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            "RepositoryPullRequests",
            value,
            location: null,
            availability?.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            "Pull request durumları normalize merged ve closed alanlarından sayılmıştır."));

        return evidence;
    }

    private static string Format(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        EvidenceCollection evidence,
        DateTimeOffset evaluatedAt)
    {
        return new SignalResult(
            Definition.Key,
            Definition.Version,
            Definition.Category,
            status,
            score,
            Definition.MaximumScore,
            Definition.IncludeInOverallScore,
            evidence.Items,
            evaluatedAt);
    }
}

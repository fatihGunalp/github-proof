using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class PullRequestUsageSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.pull-request-usage";

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

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(PullRequestsDataKey);
        int mergedCount = snapshot.PullRequests.Items.Count(pullRequest => pullRequest.IsMerged);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (mergedCount >= 2)
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
        string value = $"total={Format(snapshot.PullRequests.Items.Count)};merged={Format(mergedCount)};open={Format(openCount)};closedUnmerged={Format(closedUnmergedCount)};complete={snapshot.PullRequests.IsComplete.ToString(CultureInfo.InvariantCulture)};availability={state}";

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

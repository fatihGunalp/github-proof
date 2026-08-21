using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class IssueUsageSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.issue-usage";

    // The canonical terminal threshold. It lives here and nowhere else.
    public const int TerminalQualifyingIssueCount = 2;

    private const string IssuesDataKey = "issues";
    private readonly TimeProvider _timeProvider;

    public IssueUsageSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.DevelopmentProcessVisibility,
        maximumScore: 5,
        requiredDataKeys: [IssuesDataKey],
        includeInOverallScore: true,
        version: 1);

    // Deterministic and side-effect-free. The observed qualifying count only
    // grows, and Present/5 is this signal's maximum, so once this returns true
    // no further page can change the result.
    public static bool IsTerminalThresholdReached(int observedQualifyingCount) =>
        observedQualifyingCount >= TerminalQualifyingIssueCount;

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(IssuesDataKey);
        int qualifyingCount = snapshot.Issues.Items.Count;
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (availability?.Status == DataAvailabilityStatus.NotApplicable)
        {
            return CreateResult(
                SignalStatus.NotApplicable,
                score: null,
                evidence,
                evaluatedAt);
        }

        if (IsTerminalThresholdReached(qualifyingCount))
        {
            return CreateResult(
                SignalStatus.Present,
                score: 5,
                evidence,
                evaluatedAt);
        }

        if (availability?.IsAvailable != true || !snapshot.Issues.IsComplete)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                evaluatedAt);
        }

        if (qualifyingCount == 1)
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
        int openCount = snapshot.Issues.Items.Count(issue => !issue.IsClosed);
        int closedCount = snapshot.Issues.Items.Count(issue => issue.IsClosed);
        string state = availability is null
            ? "not-recorded"
            : availability.Status.ToString();
        // A bounded observation reports lower bounds and never a total.
        string value = snapshot.Issues.IsDecisionBounded
            ? $"qualifyingAtLeast={Format(snapshot.Issues.Items.Count)};openAtLeast={Format(openCount)};closedAtLeast={Format(closedCount)};observation=decision-bounded;availability={state}"
            : $"qualifying={Format(snapshot.Issues.Items.Count)};open={Format(openCount)};closed={Format(closedCount)};complete={snapshot.Issues.IsComplete.ToString(CultureInfo.InvariantCulture)};availability={state}";

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            "RepositoryIssues",
            value,
            location: null,
            availability?.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            "Issue kayıtları provider normalizasyonunda pull requestlerden ayrıştırılmıştır."));

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

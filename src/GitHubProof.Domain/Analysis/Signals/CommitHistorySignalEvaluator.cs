using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class CommitHistorySignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.commit-history";

    private const string CommitsDataKey = "commits";
    private readonly TimeProvider _timeProvider;

    public CommitHistorySignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.DevelopmentProcessVisibility,
        maximumScore: 7,
        requiredDataKeys: [CommitsDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(CommitsDataKey);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (availability?.IsAvailable != true || !snapshot.Commits.IsComplete)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                evaluatedAt);
        }

        int commitCount = snapshot.Commits.Items.Count;
        (SignalStatus Status, int Score) classification = Classify(commitCount);

        return CreateResult(
            classification.Status,
            classification.Score,
            evidence,
            evaluatedAt);
    }

    private static (SignalStatus Status, int Score) Classify(int commitCount)
    {
        return commitCount switch
        {
            >= 10 => (SignalStatus.Strong, 7),
            >= 7 => (SignalStatus.Present, 6),
            >= 5 => (SignalStatus.Present, 5),
            >= 3 => (SignalStatus.Present, 4),
            2 => (SignalStatus.Partial, 2),
            1 => (SignalStatus.Partial, 1),
            _ => (SignalStatus.Missing, 0)
        };
    }

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability)
    {
        bool canInspect = availability?.IsAvailable == true
            && snapshot.Commits.IsComplete;
        string availabilityState = availability is null
            ? "not-recorded"
            : availability.Status.ToString();
        string observedCount = snapshot.Commits.Items.Count.ToString(CultureInfo.InvariantCulture);
        string value = $"observedCount={observedCount};complete={snapshot.Commits.IsComplete.ToString(CultureInfo.InvariantCulture)};availability={availabilityState}";
        string notes = canInspect
            ? $"Commit erişilebilirliği Available ve koleksiyon tam; {observedCount} normalize edilmiş commit bulundu."
            : CreateUnavailableNotes(snapshot, availability);

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            canInspect ? "RepositoryCommits" : "CommitAvailability",
            value,
            snapshot.Repository.DefaultBranch,
            availability?.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            notes));

        return evidence;
    }

    private static string CreateUnavailableNotes(
        RepositorySnapshot snapshot,
        DataAvailability? availability)
    {
        string availabilityState = availability is null
            ? "not-recorded"
            : availability.Status.ToString();

        return $"Commit erişilebilirliği: {availabilityState}; koleksiyon tam: {snapshot.Commits.IsComplete.ToString(CultureInfo.InvariantCulture)}. {availability?.Reason}";
    }

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

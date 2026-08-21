using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class CommitHistorySignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.commit-history";

    // The canonical Strong threshold. It lives here and nowhere else: a
    // collection boundary that wants to know whether further commits could
    // still change this signal must ask IsStrongThresholdReached rather than
    // restate the number.
    public const int StrongCommitCount = 10;

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
        version: 2);

    // Deterministic and side-effect-free. Observed commit count only grows as
    // more pages are read, so once this returns true the Strong classification
    // is irreversible and no further page can change the signal result.
    public static bool IsStrongThresholdReached(int observedCommitCount) =>
        observedCommitCount >= StrongCommitCount;

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(CommitsDataKey);
        int commitCount = snapshot.Commits.Items.Count;
        bool isAvailable = availability?.IsAvailable == true;

        // A deliberately bounded collection may still carry the terminal Strong
        // result, because the observed count is a lower bound and the
        // classification is monotone. An Incomplete collection may not: it
        // keeps today's semantics exactly.
        bool decisionBoundedStrong = isAvailable
            && snapshot.Commits.IsDecisionBounded
            && IsStrongThresholdReached(commitCount);

        if (!isAvailable || (!snapshot.Commits.IsComplete && !decisionBoundedStrong))
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                CollectEvidence(snapshot, availability, canInspect: false),
                evaluatedAt);
        }

        (SignalStatus Status, int Score) classification = decisionBoundedStrong
            ? (SignalStatus.Strong, Definition.MaximumScore)
            : Classify(commitCount);

        return CreateResult(
            classification.Status,
            classification.Score,
            CollectEvidence(snapshot, availability, canInspect: true),
            evaluatedAt);
    }

    private static (SignalStatus Status, int Score) Classify(int commitCount)
    {
        return commitCount switch
        {
            >= StrongCommitCount => (SignalStatus.Strong, 7),
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
        DataAvailability? availability,
        bool canInspect)
    {
        string availabilityState = availability is null
            ? "not-recorded"
            : availability.Status.ToString();
        string observedCount = snapshot.Commits.Items.Count.ToString(
            CultureInfo.InvariantCulture);
        bool isDecisionBounded = snapshot.Commits.IsDecisionBounded;

        // A bounded observation reports a lower bound and never a total. Only
        // an enumerated collection may state complete=True.
        string value = isDecisionBounded
            ? $"observedCountAtLeast={observedCount};observation=decision-bounded;availability={availabilityState}"
            : $"observedCount={observedCount};complete={snapshot.Commits.IsComplete.ToString(CultureInfo.InvariantCulture)};availability={availabilityState}";
        string notes = canInspect
            ? isDecisionBounded
                ? $"Commit erişilebilirliği Available ve koleksiyon karar-yeterli olarak sınırlandı; en az {observedCount} normalize edilmiş commit gözlendi. Bu bir alt sınırdır, repository toplamı değildir."
                : $"Commit erişilebilirliği Available ve koleksiyon tam; {observedCount} normalize edilmiş commit bulundu."
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

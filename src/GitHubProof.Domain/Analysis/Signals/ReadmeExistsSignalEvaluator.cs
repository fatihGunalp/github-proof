using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class ReadmeExistsSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "repository.readme.exists";

    private const string ReadmeDataKey = "readme";
    private readonly TimeProvider _timeProvider;

    public ReadmeExistsSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.RepositoryUnderstandability,
        maximumScore: 5,
        requiredDataKeys: ["file-tree", ReadmeDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(ReadmeDataKey);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (availability?.IsAvailable != true)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                evaluatedAt);
        }

        if (snapshot.Readme is not null)
        {
            return CreateResult(
                SignalStatus.Present,
                Definition.MaximumScore,
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
        bool isAvailable = availability?.IsAvailable == true;
        DateTimeOffset observedAt = availability?.RetrievedAt
            ?? snapshot.CapturedAt;
        string value = isAvailable
            ? (snapshot.Readme is null ? "false" : "true")
            : "unavailable";
        string notes = isAvailable
            ? snapshot.Readme is null
                ? "README verisi başarıyla incelendi ve README bulunamadı."
                : "Normalize edilmiş snapshot içinde GitHub tarafından tanınan README bulundu."
            : CreateUnavailableNotes(availability);

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            "README",
            value,
            snapshot.Readme?.Path,
            observedAt,
            EvidenceConfidence.High,
            notes));

        return evidence;
    }

    private static string CreateUnavailableNotes(DataAvailability? availability)
    {
        if (availability is null)
        {
            return "README veri erişilebilirliği snapshot içinde kaydedilmemiş.";
        }

        return $"README veri erişilebilirlik durumu: {availability.Status}. {availability.Reason}";
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

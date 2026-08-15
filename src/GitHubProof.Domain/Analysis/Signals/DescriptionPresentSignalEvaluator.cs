using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class DescriptionPresentSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "repository.description.present";

    private const string MetadataDataKey = "metadata";
    private const string Recommendation =
        "Repository'nin çözdüğü problemi tek cümlede açıklayan kısa bir GitHub description ekleyin.";
    private const string PresenceLimitation =
        "Description bulunması, projenin kapsamının veya değer önerisinin tamamen anlaşılır olduğunu kanıtlamaz.";

    private readonly TimeProvider _timeProvider;

    public DescriptionPresentSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Proje Açıklaması",
        AnalysisCategory.RepositoryUnderstandability,
        "Repository metadata içindeki kısa proje açıklamasının varlığını değerlendirir.",
        maximumScore: 4,
        requiredDataKeys: [MetadataDataKey],
        applicabilityRules: ["Bütün public repository'lerde uygulanır."],
        evaluationRules:
        [
            "Dolu description alanı Present sonucu ve tam puan üretir.",
            "Metadata başarıyla incelenip description bulunamadığında Missing sonucu üretilir.",
            "Metadata verisine erişilemediğinde UnavailableEvidence sonucu üretilir."
        ],
        evidenceRequirements:
        [
            "Repository metadata description alanının incelendiğine dair kanıt.",
            "Description bulunduğunda normalize edilmiş karakter sayısı."
        ],
        explanationTemplate: "Repository metadata description alanı ve veri erişilebilirliği deterministik olarak değerlendirilir.",
        recommendationTemplate: Recommendation,
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(MetadataDataKey);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (availability?.IsAvailable != true)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                "Repository metadata erişilebilir olmadığı için description değerlendirilemedi.",
                recommendation: null,
                limitations: null,
                evaluatedAt);
        }

        string? description = snapshot.Metadata?.Description;

        if (description is not null)
        {
            return CreateResult(
                SignalStatus.Present,
                Definition.MaximumScore,
                evidence,
                $"Repository metadata içerisinde {description.Length.ToString(CultureInfo.InvariantCulture)} karakterlik görünür bir description bulundu.",
                recommendation: null,
                limitations: [PresenceLimitation],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "Repository metadata başarıyla incelendi ancak description bulunamadı.",
            Recommendation,
            limitations: null,
            evaluatedAt);
    }

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability)
    {
        bool isAvailable = availability?.IsAvailable == true;
        DateTimeOffset observedAt = availability?.RetrievedAt
            ?? snapshot.CapturedAt;
        string? description = snapshot.Metadata?.Description;
        string value = isAvailable
            ? description?.Length.ToString(CultureInfo.InvariantCulture) ?? "false"
            : "unavailable";
        string source = isAvailable
            ? "RepositoryMetadata"
            : "MetadataAvailability";
        string? location = isAvailable ? "description" : null;
        string notes = isAvailable
            ? description is null
                ? "Repository metadata başarıyla incelendi ve description bulunamadı."
                : $"Repository description alanında {description.Length.ToString(CultureInfo.InvariantCulture)} karakterlik açıklama bulundu."
            : CreateUnavailableNotes(availability);

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            source,
            value,
            location,
            observedAt,
            EvidenceConfidence.High,
            notes));

        return evidence;
    }

    private static string CreateUnavailableNotes(DataAvailability? availability)
    {
        if (availability is null)
        {
            return "Metadata veri erişilebilirliği snapshot içinde kaydedilmemiş.";
        }

        return $"Metadata veri erişilebilirlik durumu: {availability.Status}. {availability.Reason}";
    }

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        EvidenceCollection evidence,
        string explanation,
        string? recommendation,
        IEnumerable<string>? limitations,
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
            explanation,
            recommendation,
            limitations,
            evaluatedAt);
    }
}

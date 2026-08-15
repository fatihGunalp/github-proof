using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class ReadmeExistsSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "repository.readme.exists";

    private const string ReadmeDataKey = "readme";
    private const string Recommendation =
        "Repository'nin amacını, kurulumunu ve kullanımını açıklayan bir README ekleyin.";
    private const string PresenceLimitation =
        "README'nin bulunması içeriğin yeterli, doğru veya güncel olduğunu kanıtlamaz.";

    private readonly TimeProvider _timeProvider;

    public ReadmeExistsSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "README Varlığı",
        AnalysisCategory.RepositoryUnderstandability,
        "GitHub tarafından README olarak tanınan bir dosyanın varlığını değerlendirir.",
        maximumScore: 5,
        requiredDataKeys: ["file-tree", ReadmeDataKey],
        applicabilityRules: ["Public ve boş olmayan repository'lerde uygulanır."],
        evaluationRules:
        [
            "README bulunduğunda Present sonucu ve tam puan üretilir.",
            "README verisi başarıyla incelenip dosya bulunamadığında Missing sonucu üretilir.",
            "README verisine erişilemediğinde UnavailableEvidence sonucu üretilir.",
            "Birden fazla belirsiz README adayı varsa insan incelemesi gerekir."
        ],
        evidenceRequirements:
        [
            "GitHub tarafından README olarak tanınan dosyanın bulunma durumu.",
            "README verisinin erişilebilirlik durumu."
        ],
        explanationTemplate: "README varlığı ve veri erişilebilirliği deterministik olarak değerlendirilir.",
        recommendationTemplate: Recommendation,
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
                "README verisi alınamadığı için README varlığı değerlendirilemedi.",
                recommendation: null,
                limitations: null,
                evaluatedAt);
        }

        if (snapshot.Readme is not null)
        {
            return CreateResult(
                SignalStatus.Present,
                Definition.MaximumScore,
                evidence,
                $"GitHub tarafından README olarak tanınan '{snapshot.Readme.Path}' dosyası bulundu.",
                recommendation: null,
                limitations: [PresenceLimitation],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "README verisi başarıyla incelendi ancak GitHub tarafından tanınan bir README bulunamadı.",
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

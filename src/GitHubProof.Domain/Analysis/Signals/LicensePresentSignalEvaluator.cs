using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class LicensePresentSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "repository.license.present";

    private const string LicenseDataKey = "license";
    private const string MissingRecommendation =
        "Projenin kullanım ve katkı koşullarını açıklayan uygun bir açık kaynak lisansı ekleyin.";
    private const string PartialRecommendation =
        "Mevcut lisans dosyasını yaygın ve açık biçimde tanınabilir bir açık kaynak lisans metniyle netleştirin.";
    private const string PresenceLimitation =
        "Lisans bulunması, yasal uygunluğun bütünüyle doğrulandığı anlamına gelmez.";

    private readonly TimeProvider _timeProvider;

    public LicensePresentSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Lisans",
        AnalysisCategory.RepositoryUnderstandability,
        "Repository'nin kullanım ve katkı koşullarına ilişkin görünür lisans kanıtını değerlendirir.",
        maximumScore: 3,
        requiredDataKeys: [LicenseDataKey],
        applicabilityRules: ["Bütün public repository'lerde uygulanır."],
        evaluationRules:
        [
            "Tanınmış lisans metadata'sı Present sonucu ve tam puan üretir.",
            "Tanınmış metadata olmadan normalize edilmiş lisans dosyası Partial sonucu üretir.",
            "Lisans verisi başarıyla incelenip kanıt bulunamadığında Missing sonucu üretilir.",
            "Lisans verisine erişilemediğinde UnavailableEvidence sonucu üretilir."
        ],
        evidenceRequirements:
        [
            "Tanınmış lisans metadata'sı veya normalize edilmiş lisans dosyası.",
            "Lisans verisinin erişilebilirlik durumu."
        ],
        explanationTemplate: "Lisans metadata'sı, normalize edilmiş lisans dosyası ve veri erişilebilirliği deterministik olarak değerlendirilir.",
        recommendationTemplate: MissingRecommendation,
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(LicenseDataKey);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability);

        if (availability?.IsAvailable != true)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                "Lisans verisi alınamadığı için lisans kanıtı değerlendirilemedi.",
                recommendation: null,
                limitations: null,
                evaluatedAt);
        }

        if (snapshot.License is not null)
        {
            return CreateResult(
                SignalStatus.Present,
                Definition.MaximumScore,
                evidence,
                $"Tanınmış '{snapshot.License.Name}' lisans metadata'sı bulundu.",
                recommendation: null,
                limitations: [PresenceLimitation],
                evaluatedAt);
        }

        if (snapshot.LicenseFile is not null)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 1,
                evidence,
                $"'{snapshot.LicenseFile.Path}' lisans dosyası bulundu ancak tanınmış lisans metadata'sı yok.",
                PartialRecommendation,
                limitations: [PresenceLimitation],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "Lisans verisi başarıyla incelendi ancak tanınmış lisans veya normalize edilmiş lisans dosyası bulunamadı.",
            MissingRecommendation,
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
            ? snapshot.License is not null
                ? snapshot.License.SpdxIdentifier ?? snapshot.License.Key
                : snapshot.LicenseFile is not null ? "unrecognized-file" : "false"
            : "unavailable";
        string source = snapshot.License is not null
            ? "RepositoryLicense"
            : snapshot.LicenseFile is not null ? "LicenseFile" : "License";
        string? location = snapshot.License?.Path ?? snapshot.LicenseFile?.Path;

        if (!isAvailable)
        {
            source = "LicenseAvailability";
            location = null;
        }

        string notes = isAvailable
            ? snapshot.License is not null
                ? $"Tanınmış lisans metadata'sı: {snapshot.License.Name}."
                : snapshot.LicenseFile is not null
                    ? "Normalize edilmiş lisans dosyası bulundu ancak tanınmış lisans metadata'sı yok."
                    : "Lisans verisi başarıyla incelendi ancak lisans kanıtı bulunamadı."
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
            return "Lisans veri erişilebilirliği snapshot içinde kaydedilmemiş.";
        }

        return $"Lisans veri erişilebilirlik durumu: {availability.Status}. {availability.Reason}";
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

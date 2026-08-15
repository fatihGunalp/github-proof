using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class ReleaseUsageSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.release-usage";

    private const string ReleasesDataKey = "releases";
    private const string TagsDataKey = "tags";
    private const string Recommendation =
        "Anlamlı teslim noktaları için version tag ve açıklayıcı GitHub Release oluşturun.";
    private const string Limitation =
        "Release varlığı semantic versioning, production kararlılığı veya aktif bakım kanıtı değildir.";

    private readonly TimeProvider _timeProvider;

    public ReleaseUsageSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Release Kullanımı",
        AnalysisCategory.DevelopmentProcessVisibility,
        "Published ve draft GitHub Releases ile tag kanıtlarının görünür kullanımını değerlendirir.",
        maximumScore: 6,
        requiredDataKeys: [ReleasesDataKey, TagsDataKey],
        applicabilityRules:
        [
            "Bütün public repository'lerde uygulanır.",
            "Repository türü veya amacı tahmin edilerek NotApplicable sonucu üretilmez."
        ],
        evaluationRules:
        [
            "Release note içeren published release Present sonucu ve tam puan üretir.",
            "Notes alanı olmayan published release Partial sonucu ve dört puan üretir.",
            "Published release yokken tag veya yalnız draft release bulunması ilgili Partial sonucunu üretir."
        ],
        evidenceRequirements:
        [
            "Published, notes içeren published, draft ve tag sayıları.",
            "Releases ve Tags koleksiyonlarının ayrı bütünlük ve erişilebilirlik durumları."
        ],
        explanationTemplate: "Normalize release/tag kanıtı ve bağımsız veri bütünlüğü deterministik olarak değerlendirilir.",
        recommendationTemplate: Recommendation,
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? releasesAvailability = snapshot.FindAvailability(ReleasesDataKey);
        DataAvailability? tagsAvailability = snapshot.FindAvailability(TagsDataKey);
        int publishedCount = snapshot.Releases.Items.Count(release => release.HasPublishedRelease);
        int publishedWithNotesCount = snapshot.Releases.Items.Count(
            release => release.HasPublishedRelease && release.Notes is not null);
        int draftCount = snapshot.Releases.Items.Count(release => release.IsDraft);
        int tagCount = snapshot.Tags.Items.Count;
        EvidenceCollection evidence = CollectEvidence(
            snapshot,
            releasesAvailability,
            tagsAvailability,
            publishedCount,
            publishedWithNotesCount,
            draftCount,
            tagCount);

        if (publishedWithNotesCount > 0)
        {
            return CreateResult(
                SignalStatus.Present,
                score: 6,
                evidence,
                $"Açıklayıcı note içeren {Format(publishedWithNotesCount)} published release bulundu.",
                recommendation: null,
                [Limitation],
                evaluatedAt);
        }

        bool releasesTrusted = releasesAvailability?.IsAvailable == true
            && snapshot.Releases.IsComplete;

        if (!releasesTrusted)
        {
            return CreateUnavailableResult(
                evidence,
                "Published release ve release-note sonucu için Releases verisi erişilebilir ve tam değildir.",
                evaluatedAt);
        }

        if (publishedCount > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 4,
                evidence,
                "Published release bulundu ancak normalize edilmiş açıklayıcı release note bulunamadı.",
                Recommendation,
                [Limitation],
                evaluatedAt);
        }

        if (tagCount > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 3,
                evidence,
                $"Published release bulunmadı ancak {Format(tagCount)} tag gözlendi.",
                Recommendation,
                [Limitation],
                evaluatedAt);
        }

        bool tagsTrusted = tagsAvailability?.IsAvailable == true
            && snapshot.Tags.IsComplete;

        if (!tagsTrusted)
        {
            return CreateUnavailableResult(
                evidence,
                "Tag bulunmadığı sonucunu doğrulamak için Tags verisi erişilebilir ve tam değildir.",
                evaluatedAt);
        }

        if (draftCount > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 2,
                evidence,
                "Published release veya tag bulunmadı; yalnız draft release kanıtı bulundu.",
                Recommendation,
                [Limitation],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "Releases ve Tags verisi tam olarak incelendi ancak release veya tag bulunamadı.",
            Recommendation,
            [Limitation],
            evaluatedAt);
    }

    private SignalResult CreateUnavailableResult(
        EvidenceCollection evidence,
        string explanation,
        DateTimeOffset evaluatedAt)
    {
        return CreateResult(
            SignalStatus.UnavailableEvidence,
            score: null,
            evidence,
            explanation,
            recommendation: null,
            limitations: null,
            evaluatedAt);
    }

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? releasesAvailability,
        DataAvailability? tagsAvailability,
        int publishedCount,
        int publishedWithNotesCount,
        int draftCount,
        int tagCount)
    {
        string releasesState = GetAvailabilityState(releasesAvailability);
        string tagsState = GetAvailabilityState(tagsAvailability);
        string value = $"published={Format(publishedCount)};publishedWithNotes={Format(publishedWithNotesCount)};draft={Format(draftCount)};tags={Format(tagCount)};releasesComplete={snapshot.Releases.IsComplete.ToString(CultureInfo.InvariantCulture)};tagsComplete={snapshot.Tags.IsComplete.ToString(CultureInfo.InvariantCulture)};releasesAvailability={releasesState};tagsAvailability={tagsState}";
        DateTimeOffset observedAt = GetObservedAt(
            snapshot.CapturedAt,
            releasesAvailability,
            tagsAvailability);

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            "RepositoryReleasesAndTags",
            value,
            location: null,
            observedAt,
            EvidenceConfidence.High,
            "Release note varlığı normalize edilmiş Notes alanından; Releases ve Tags durumları ayrı kaynaklardan değerlendirilmiştir."));

        return evidence;
    }

    private static DateTimeOffset GetObservedAt(
        DateTimeOffset fallback,
        DataAvailability? releasesAvailability,
        DataAvailability? tagsAvailability)
    {
        DateTimeOffset? releasesAt = releasesAvailability?.RetrievedAt;
        DateTimeOffset? tagsAt = tagsAvailability?.RetrievedAt;

        if (releasesAt is null)
        {
            return tagsAt ?? fallback;
        }

        if (tagsAt is null)
        {
            return releasesAt.Value;
        }

        return releasesAt > tagsAt ? releasesAt.Value : tagsAt.Value;
    }

    private static string GetAvailabilityState(DataAvailability? availability) =>
        availability is null ? "not-recorded" : availability.Status.ToString();

    private static string Format(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

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

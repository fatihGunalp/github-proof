using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class CommitHistorySignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.commit-history";

    private const string CommitsDataKey = "commits";
    private const string Recommendation =
        "Değişiklikleri amaçlarına göre küçük ve açıklanabilir commitler hâlinde kaydedin.";
    private const string Limitation =
        "Commit sayısı commit kalitesini, geliştirici yetkinliğini veya özgün üretimi kanıtlamaz.";

    private readonly TimeProvider _timeProvider;

    public CommitHistorySignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Commit Geçmişi",
        AnalysisCategory.DevelopmentProcessVisibility,
        "Default branch üzerindeki görünür commit geçmişinin büyüklüğünü değerlendirir.",
        maximumScore: 7,
        requiredDataKeys: [CommitsDataKey],
        applicabilityRules: ["Bütün public repository'lerde uygulanır."],
        evaluationRules:
        [
            "Tam ve erişilebilir commit koleksiyonu Version 1 commit sayısı eşiklerine göre puanlanır.",
            "Tam koleksiyonda commit bulunmaması Missing sonucu üretir.",
            "Erişilemeyen veya eksik sayfalanmış commit koleksiyonu UnavailableEvidence sonucu üretir."
        ],
        evidenceRequirements:
        [
            "Normalize edilmiş default branch commit sayısı.",
            "Commit erişilebilirliği ve koleksiyon bütünlüğü."
        ],
        explanationTemplate: "Default branch commit sayısı ve veri bütünlüğü deterministik olarak değerlendirilir.",
        recommendationTemplate: Recommendation,
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
                "Commit koleksiyonu erişilebilir ve tam olmadığı için commit geçmişi puanlanamadı.",
                recommendation: null,
                limitations: null,
                evaluatedAt);
        }

        int commitCount = snapshot.Commits.Items.Count;
        (SignalStatus Status, int Score) classification = Classify(commitCount);

        return CreateResult(
            classification.Status,
            classification.Score,
            evidence,
            CreateExplanation(commitCount, classification.Status),
            classification.Status is SignalStatus.Partial or SignalStatus.Missing
                ? Recommendation
                : null,
            [Limitation],
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

    private static string CreateExplanation(int commitCount, SignalStatus status)
    {
        string count = commitCount.ToString(CultureInfo.InvariantCulture);

        return status == SignalStatus.Missing
            ? "Default branch commit verisi tam olarak incelendi ancak commit bulunamadı."
            : $"Default branch üzerinde {count} normalize edilmiş commit bulundu; sonuç {status} olarak sınıflandırıldı.";
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

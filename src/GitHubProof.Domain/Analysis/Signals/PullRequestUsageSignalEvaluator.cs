using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class PullRequestUsageSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.pull-request-usage";

    private const string PullRequestsDataKey = "pull-requests";
    private const string Recommendation =
        "Önemli değişiklikleri ayrı branch ve pull request akışıyla görünür hâle getirin.";
    private const string Limitation =
        "Pull request varlığı gerçek code review veya ekip çalışmasını garanti etmez.";

    private readonly TimeProvider _timeProvider;

    public PullRequestUsageSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Pull Request Kullanımı",
        AnalysisCategory.DevelopmentProcessVisibility,
        "Açık, kapalı-unmerged ve merged pull request kayıtlarının görünür kullanımını değerlendirir.",
        maximumScore: 6,
        requiredDataKeys: [PullRequestsDataKey],
        applicabilityRules:
        [
            "Bütün public repository'lerde uygulanır.",
            "Repository büyüklüğü veya sahiplik yapısı NotApplicable sonucu üretmez."
        ],
        evaluationRules:
        [
            "İki veya daha fazla merged pull request Strong sonucu ve tam puan üretir.",
            "Tek merged pull request Present sonucu üretir.",
            "Merged olmayan en az bir pull request Partial sonucu üretir."
        ],
        evidenceRequirements:
        [
            "Toplam, merged, açık ve kapalı-unmerged pull request sayıları.",
            "Pull request koleksiyonu bütünlüğü ve erişilebilirliği."
        ],
        explanationTemplate: "Normalize pull request durumları ve veri bütünlüğü deterministik olarak değerlendirilir.",
        recommendationTemplate: Recommendation,
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
                $"Repository üzerinde {Format(mergedCount)} merged pull request bulundu.",
                recommendation: null,
                [Limitation],
                evaluatedAt);
        }

        if (availability?.IsAvailable != true || !snapshot.PullRequests.IsComplete)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                "Pull request koleksiyonu exact sonucu belirlemek için erişilebilir ve tam değildir.",
                recommendation: null,
                limitations: null,
                evaluatedAt);
        }

        if (mergedCount == 1)
        {
            return CreateResult(
                SignalStatus.Present,
                score: 4,
                evidence,
                "Repository üzerinde tam olarak bir merged pull request bulundu.",
                recommendation: null,
                [Limitation],
                evaluatedAt);
        }

        if (snapshot.PullRequests.Items.Count > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 2,
                evidence,
                "Merged pull request bulunmadı ancak açık veya kapalı-unmerged pull request kanıtı bulundu.",
                Recommendation,
                [Limitation],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "Pull request verisi tam olarak incelendi ancak pull request bulunamadı.",
            Recommendation,
            [Limitation],
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

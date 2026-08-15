using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class CommitDistributionSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "process.commit-distribution";

    private static readonly TimeSpan MinimumRepositoryAge = TimeSpan.FromDays(14);
    private static readonly TimeSpan StrongMinimumSpan = TimeSpan.FromDays(14);

    private const string CommitsDataKey = "commits";
    private const string MetadataDataKey = "metadata";
    private readonly TimeProvider _timeProvider;

    public CommitDistributionSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.DevelopmentProcessVisibility,
        maximumScore: 8,
        requiredDataKeys: [CommitsDataKey, MetadataDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? metadataAvailability = snapshot.FindAvailability(MetadataDataKey);

        if (metadataAvailability?.IsAvailable != true || snapshot.Metadata is null)
        {
            return CreateUnavailableResult(
                snapshot,
                metadataAvailability,
                evaluatedAt);
        }

        TimeSpan repositoryAge = snapshot.CapturedAt - snapshot.Metadata.CreatedAt;

        if (repositoryAge < TimeSpan.Zero)
        {
            return CreateUnavailableResult(
                snapshot,
                metadataAvailability,
                evaluatedAt);
        }

        if (repositoryAge < MinimumRepositoryAge)
        {
            EvidenceCollection evidence = CollectDistributionEvidence(
                snapshot,
                metadataAvailability,
                repositoryAge,
                activeDayCount: null,
                commitSpan: null);

            return CreateResult(
                SignalStatus.NotApplicable,
                score: null,
                evidence,
                evaluatedAt);
        }

        DataAvailability? commitsAvailability = snapshot.FindAvailability(CommitsDataKey);

        if (commitsAvailability?.IsAvailable != true || !snapshot.Commits.IsComplete)
        {
            return CreateUnavailableResult(
                snapshot,
                commitsAvailability,
                evaluatedAt,
                repositoryAge);
        }

        IReadOnlyList<RepositoryCommit> commits = snapshot.Commits.Items;
        int activeDayCount = commits
            .Select(commit => DateOnly.FromDateTime(commit.CommittedAt.UtcDateTime))
            .Distinct()
            .Count();
        TimeSpan commitSpan = commits.Count == 0
            ? TimeSpan.Zero
            : commits.Max(commit => commit.CommittedAt)
                - commits.Min(commit => commit.CommittedAt);
        (SignalStatus Status, int Score) classification = Classify(
            activeDayCount,
            commitSpan);
        EvidenceCollection resultEvidence = CollectDistributionEvidence(
            snapshot,
            commitsAvailability,
            repositoryAge,
            activeDayCount,
            commitSpan);

        return CreateResult(
            classification.Status,
            classification.Score,
            resultEvidence,
            evaluatedAt);
    }

    private static (SignalStatus Status, int Score) Classify(
        int activeDayCount,
        TimeSpan commitSpan)
    {
        if (activeDayCount >= 8 && commitSpan >= StrongMinimumSpan)
        {
            return (SignalStatus.Strong, 8);
        }

        return activeDayCount switch
        {
            >= 6 => (SignalStatus.Present, 7),
            5 => (SignalStatus.Present, 6),
            4 => (SignalStatus.Present, 5),
            3 => (SignalStatus.Partial, 4),
            2 => (SignalStatus.Partial, 2),
            1 => (SignalStatus.Partial, 1),
            _ => (SignalStatus.Missing, 0)
        };
    }

    private static EvidenceCollection CollectDistributionEvidence(
        RepositorySnapshot snapshot,
        DataAvailability availability,
        TimeSpan repositoryAge,
        int? activeDayCount,
        TimeSpan? commitSpan)
    {
        string activeDays = activeDayCount?.ToString(CultureInfo.InvariantCulture)
            ?? "not-evaluated";
        string spanDays = commitSpan is null
            ? "not-evaluated"
            : FormatDays(commitSpan.Value);
        DateTimeOffset? firstCommit = activeDayCount is null || snapshot.Commits.Items.Count == 0
            ? null
            : snapshot.Commits.Items.Min(commit => commit.CommittedAt);
        DateTimeOffset? lastCommit = activeDayCount is null || snapshot.Commits.Items.Count == 0
            ? null
            : snapshot.Commits.Items.Max(commit => commit.CommittedAt);
        string firstCommitAt = activeDayCount is null
            ? "not-evaluated"
            : FormatTimestamp(firstCommit);
        string lastCommitAt = activeDayCount is null
            ? "not-evaluated"
            : FormatTimestamp(lastCommit);
        string metadataState = GetAvailabilityState(snapshot.FindAvailability(MetadataDataKey));
        string commitsState = activeDayCount is null
            ? "not-evaluated"
            : GetAvailabilityState(snapshot.FindAvailability(CommitsDataKey));
        string value = $"repositoryAgeDays={FormatDays(repositoryAge)};activeDays={activeDays};firstCommitAt={firstCommitAt};lastCommitAt={lastCommitAt};spanDays={spanDays};commitsComplete={snapshot.Commits.IsComplete.ToString(CultureInfo.InvariantCulture)};metadataAvailability={metadataState};commitsAvailability={commitsState}";
        string notes = activeDayCount is null
            ? "Repository yaşı UTC snapshot zamanlarına göre hesaplandı; commit dağılımı uygulanabilirlik nedeniyle incelenmedi."
            : "Aktif günler UTC takvim günlerine, ilk-son aralığı normalize edilmiş UTC commit zamanlarına göre hesaplandı.";

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            "RepositoryCommitDistribution",
            value,
            snapshot.Repository.DefaultBranch,
            availability.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            notes));

        return evidence;
    }

    private static EvidenceCollection CollectUnavailableEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability,
        TimeSpan? repositoryAge)
    {
        string age = repositoryAge is null
            ? "unknown"
            : FormatDays(repositoryAge.Value);
        string availabilityState = GetAvailabilityState(availability);
        DateTimeOffset? observedFirstCommit = snapshot.Commits.Items.Count == 0
            ? null
            : snapshot.Commits.Items.Min(commit => commit.CommittedAt);
        DateTimeOffset? observedLastCommit = snapshot.Commits.Items.Count == 0
            ? null
            : snapshot.Commits.Items.Max(commit => commit.CommittedAt);
        string metadataState = GetAvailabilityState(snapshot.FindAvailability(MetadataDataKey));
        string commitsState = GetAvailabilityState(snapshot.FindAvailability(CommitsDataKey));
        string value = $"repositoryAgeDays={age};observedActiveDays=not-evaluated;observedFirstCommitAt={FormatTimestamp(observedFirstCommit)};observedLastCommitAt={FormatTimestamp(observedLastCommit)};observedSpanDays=not-evaluated;commitsComplete={snapshot.Commits.IsComplete.ToString(CultureInfo.InvariantCulture)};metadataAvailability={metadataState};commitsAvailability={commitsState}";

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            "CommitDistributionAvailability",
            value,
            snapshot.Repository.DefaultBranch,
            availability?.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            $"Gerekli veri erişilebilirliği: {availabilityState}. {availability?.Reason}"));

        return evidence;
    }

    private SignalResult CreateUnavailableResult(
        RepositorySnapshot snapshot,
        DataAvailability? availability,
        DateTimeOffset evaluatedAt,
        TimeSpan? repositoryAge = null)
    {
        return CreateResult(
            SignalStatus.UnavailableEvidence,
            score: null,
            CollectUnavailableEvidence(snapshot, availability, repositoryAge),
            evaluatedAt);
    }

    private static string FormatDays(TimeSpan duration)
    {
        return duration.TotalDays.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatTimestamp(DateTimeOffset? timestamp)
    {
        return timestamp?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
            ?? "none";
    }

    private static string GetAvailabilityState(DataAvailability? availability)
    {
        return availability is null
            ? "not-recorded"
            : availability.Status.ToString();
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

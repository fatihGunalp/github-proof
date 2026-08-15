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
    private readonly TimeProvider _timeProvider;

    public ReleaseUsageSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.DevelopmentProcessVisibility,
        maximumScore: 6,
        requiredDataKeys: [ReleasesDataKey, TagsDataKey],
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
                evaluatedAt);
        }

        bool releasesTrusted = releasesAvailability?.IsAvailable == true
            && snapshot.Releases.IsComplete;

        if (!releasesTrusted)
        {
            return CreateUnavailableResult(
                evidence,
                evaluatedAt);
        }

        if (publishedCount > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 4,
                evidence,
                evaluatedAt);
        }

        if (tagCount > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 3,
                evidence,
                evaluatedAt);
        }

        bool tagsTrusted = tagsAvailability?.IsAvailable == true
            && snapshot.Tags.IsComplete;

        if (!tagsTrusted)
        {
            return CreateUnavailableResult(
                evidence,
                evaluatedAt);
        }

        if (draftCount > 0)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 2,
                evidence,
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            evaluatedAt);
    }

    private SignalResult CreateUnavailableResult(
        EvidenceCollection evidence,
        DateTimeOffset evaluatedAt)
    {
        return CreateResult(
            SignalStatus.UnavailableEvidence,
            score: null,
            evidence,
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

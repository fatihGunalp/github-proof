using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class TopicsSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "portfolio.topics";

    private const string MetadataDataKey = "metadata";
    private readonly TimeProvider _timeProvider;

    public TopicsSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.PortfolioPresentation,
        maximumScore: 4,
        requiredDataKeys: [MetadataDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(MetadataDataKey);
        IReadOnlyList<string> topics = NormalizeTopics(snapshot.Topics);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability, topics);

        if (availability?.IsAvailable != true)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                evaluatedAt);
        }

        return topics.Count switch
        {
            >= 3 => CreateResult(
                SignalStatus.Strong,
                score: 4,
                evidence,
                evaluatedAt),
            2 => CreateResult(
                SignalStatus.Present,
                score: 3,
                evidence,
                evaluatedAt),
            1 => CreateResult(
                SignalStatus.Present,
                score: 2,
                evidence,
                evaluatedAt),
            _ => CreateResult(
                SignalStatus.Missing,
                score: 0,
                evidence,
                evaluatedAt)
        };
    }

    private static IReadOnlyList<string> NormalizeTopics(IEnumerable<string> topics) =>
        topics
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => topic.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(topic => topic, StringComparer.Ordinal)
            .ToArray();

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability,
        IReadOnlyList<string> topics)
    {
        bool isAvailable = availability?.IsAvailable == true;
        DateTimeOffset observedAt = availability?.RetrievedAt ?? snapshot.CapturedAt;
        string value = isAvailable ? FormatCount(topics.Count) : "unavailable";
        string notes = isAvailable
            ? topics.Count == 0
                ? "Metadata başarıyla incelendi; normalize topic count: 0."
                : $"Normalize topic count: {FormatCount(topics.Count)}; topics: {FormatTopics(topics)}."
            : CreateUnavailableNotes(availability);

        EvidenceCollection evidence = new();
        evidence.Add(new EvidenceItem(
            SignalKey,
            isAvailable ? "RepositoryTopics" : "MetadataAvailability",
            value,
            isAvailable ? "topics" : null,
            observedAt,
            EvidenceConfidence.High,
            notes));

        return evidence;
    }

    private static string FormatTopics(IReadOnlyList<string> topics)
    {
        const int maximumNames = 8;
        string values = string.Join(", ", topics.Take(maximumNames));

        return topics.Count > maximumNames
            ? $"{values}, …"
            : values;
    }

    private static string FormatCount(int count) =>
        count.ToString(CultureInfo.InvariantCulture);

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
        DateTimeOffset evaluatedAt) =>
        new(
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

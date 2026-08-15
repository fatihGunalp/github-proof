using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class PresentationCompletenessSignalEvaluator
{
    public const string SignalKey = "portfolio.presentation-completeness";
    public static IReadOnlyList<string> RequiredSignalKeys { get; } = Array.AsReadOnly(
    [
        DescriptionPresentSignalEvaluator.SignalKey,
        ReadmeExistsSignalEvaluator.SignalKey,
        UsageDemoSignalEvaluator.SignalKey,
        LicensePresentSignalEvaluator.SignalKey,
        TopicsSignalEvaluator.SignalKey
    ]);

    private readonly TimeProvider _timeProvider;

    public PresentationCompletenessSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.PortfolioPresentation,
        maximumScore: 5,
        requiredDataKeys: RequiredSignalKeys,
        includeInOverallScore: false,
        version: 1);

    public SignalResult Evaluate(IEnumerable<SignalResult> signalResults)
    {
        ArgumentNullException.ThrowIfNull(signalResults);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        SignalResult[] results = signalResults.ToArray();

        if (results.Any(result => result is null))
        {
            throw new ArgumentException(
                "Upstream signal results cannot contain null values.",
                nameof(signalResults));
        }

        if (results
            .GroupBy(result => result.SignalKey, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Upstream signal result keys must be unique.",
                nameof(signalResults));
        }

        Dictionary<string, SignalResult> byKey = results.ToDictionary(
            result => result.SignalKey,
            StringComparer.Ordinal);
        SignalResult[] required = RequiredSignalKeys
            .Select(key => byKey.TryGetValue(key, out SignalResult? result)
                ? result
                : throw new ArgumentException(
                    $"Required upstream signal result '{key}' is missing.",
                    nameof(signalResults)))
            .ToArray();

        SignalResult? unavailable = required.FirstOrDefault(result =>
            result.Status == SignalStatus.UnavailableEvidence);

        if (unavailable is not null)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                required,
                evaluatedAt);
        }

        SignalResult? humanReview = required.FirstOrDefault(result =>
            result.Status == SignalStatus.RequiresHumanReview);

        if (humanReview is not null)
        {
            return CreateResult(
                SignalStatus.RequiresHumanReview,
                score: null,
                required,
                evaluatedAt);
        }

        SignalResult? notApplicable = required.FirstOrDefault(result =>
            result.Status == SignalStatus.NotApplicable);

        if (notApplicable is not null)
        {
            throw new InvalidOperationException(
                $"Required presentation signal '{notApplicable.SignalKey}' cannot be NotApplicable in the fixed five-item MVP contract.");
        }

        int presentCount = required.Count(result => result.Status is
            SignalStatus.Strong or SignalStatus.Present or SignalStatus.Partial);

        return presentCount switch
        {
            5 => CreateScoredResult(SignalStatus.Strong, 5, required, evaluatedAt),
            4 => CreateScoredResult(SignalStatus.Present, 4, required, evaluatedAt),
            >= 1 => CreateScoredResult(SignalStatus.Partial, presentCount, required, evaluatedAt),
            _ => CreateScoredResult(SignalStatus.Missing, 0, required, evaluatedAt)
        };
    }

    private SignalResult CreateScoredResult(
        SignalStatus status,
        int score,
        IReadOnlyList<SignalResult> required,
        DateTimeOffset evaluatedAt)
    {
        return CreateResult(
            status,
            score,
            required,
            evaluatedAt);
    }

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        IReadOnlyList<SignalResult> required,
        DateTimeOffset evaluatedAt) =>
        new(
            Definition.Key,
            Definition.Version,
            Definition.Category,
            status,
            score,
            Definition.MaximumScore,
            Definition.IncludeInOverallScore,
            required.Select(CreateEvidence),
            evaluatedAt);

    private static EvidenceItem CreateEvidence(SignalResult result) =>
        new(
            SignalKey,
            "SignalResult",
            result.Status.ToString(),
            result.SignalKey,
            result.EvaluatedAt,
            result.Status == SignalStatus.RequiresHumanReview
                ? EvidenceConfidence.Low
                : EvidenceConfidence.High,
            $"Upstream signal status: {result.Status}; score: {result.Score?.ToString(CultureInfo.InvariantCulture) ?? "null"}.");
}

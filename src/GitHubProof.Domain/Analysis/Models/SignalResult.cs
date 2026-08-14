using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record SignalResult
{
    public SignalResult(
        string signalKey,
        int signalVersion,
        AnalysisCategory category,
        SignalStatus status,
        int? score,
        int maximumScore,
        bool includeInOverallScore,
        IEnumerable<EvidenceItem>? evidence,
        string explanation,
        string? recommendation,
        IEnumerable<string>? limitations,
        DateTimeOffset evaluatedAt)
    {
        SignalKey = RequireValue(signalKey, nameof(signalKey));

        if (signalVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(signalVersion),
                signalVersion,
                "Signal version must be greater than zero.");
        }

        SignalVersion = signalVersion;
        Category = category;
        Status = status;

        if (maximumScore <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumScore),
                maximumScore,
                "Maximum score must be greater than zero.");
        }

        MaximumScore = maximumScore;
        IncludeInOverallScore = includeInOverallScore;
        Score = score;

        Evidence = evidence?
            .Where(item => item is not null)
            .Distinct()
            .ToArray()
            ?? Array.Empty<EvidenceItem>();

        Explanation = RequireValue(
            explanation,
            nameof(explanation));

        Recommendation = NormalizeOptional(recommendation);

        Limitations = limitations?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? Array.Empty<string>();

        EvaluatedAt = evaluatedAt.ToUniversalTime();

        Validate();
    }

    public string SignalKey { get; }

    public int SignalVersion { get; }

    public AnalysisCategory Category { get; }

    public SignalStatus Status { get; }

    public int? Score { get; }

    public int MaximumScore { get; }

    public bool IncludeInOverallScore { get; }

    public IReadOnlyList<EvidenceItem> Evidence { get; }

    public string Explanation { get; }

    public string? Recommendation { get; }

    public IReadOnlyList<string> Limitations { get; }

    public DateTimeOffset EvaluatedAt { get; }

    public bool HasScore => Score.HasValue;

    public bool IsApplicable =>
        Status != SignalStatus.NotApplicable;

    public bool HasAvailableEvidence =>
        Status != SignalStatus.UnavailableEvidence;

    private void Validate()
    {
        if (Score is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Score),
                Score,
                "Score cannot be less than zero.");
        }

        if (Score > MaximumScore)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Score),
                Score,
                "Score cannot exceed maximum score.");
        }

        if (Status is SignalStatus.NotApplicable
            or SignalStatus.UnavailableEvidence
            or SignalStatus.RequiresHumanReview)
        {
            if (Score is not null)
            {
                throw new ArgumentException(
                    $"{Status} results cannot include a numeric score.",
                    nameof(Score));
            }
        }
        else if (Score is null)
        {
            throw new ArgumentException(
                $"{Status} results must include a numeric score.",
                nameof(Score));
        }

        if (Status is SignalStatus.Partial
            or SignalStatus.Missing
            or SignalStatus.RequiresHumanReview)
        {
            if (string.IsNullOrWhiteSpace(Recommendation))
            {
                throw new ArgumentException(
                    $"{Status} results must include a recommendation.",
                    nameof(Recommendation));
            }
        }

        if (Status == SignalStatus.Missing
            && Score != 0)
        {
            throw new ArgumentException(
                "Missing results must have a score of zero.",
                nameof(Score));
        }

        if (Status == SignalStatus.Missing
            && Evidence.Count == 0)
        {
            throw new ArgumentException(
                "Missing results must include evidence showing that the data was inspected.",
                nameof(Evidence));
        }
    }

    private static string RequireValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}

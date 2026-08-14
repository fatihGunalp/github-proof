using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record CategoryResult
{
    public CategoryResult(
        AnalysisCategory category,
        IEnumerable<SignalResult>? signalResults,
        string summary,
        IEnumerable<string>? limitations)
    {
        Category = category;
        SignalResults = NormalizeSignalResults(signalResults, category);
        Summary = RequireValue(summary, nameof(summary));
        Limitations = NormalizeValues(limitations);

        SignalResult[] scoredResults = SignalResults
            .Where(IsScored)
            .ToArray();

        Score = scoredResults.Sum(result => result.Score!.Value);
        MaximumScore = scoredResults.Sum(result => result.MaximumScore);
        NormalizedScore = MaximumScore == 0
            ? null
            : RoundHalfUp((decimal)Score / MaximumScore * 100);
        Status = DetermineStatus(SignalResults, scoredResults.Length);
    }

    public AnalysisCategory Category { get; }

    public int Score { get; }

    public int MaximumScore { get; }

    public int? NormalizedScore { get; }

    public CategoryStatus Status { get; }

    public IReadOnlyList<SignalResult> SignalResults { get; }

    public string Summary { get; }

    public IReadOnlyList<string> Limitations { get; }

    private static bool IsScored(SignalResult result) =>
        result.Status is SignalStatus.Strong
            or SignalStatus.Present
            or SignalStatus.Partial
            or SignalStatus.Missing;

    private static CategoryStatus DetermineStatus(
        IReadOnlyList<SignalResult> signalResults,
        int scoredResultCount)
    {
        if (scoredResultCount > 0)
        {
            return signalResults.Any(result => result.Status is SignalStatus.UnavailableEvidence
                or SignalStatus.RequiresHumanReview)
                ? CategoryStatus.Partial
                : CategoryStatus.Completed;
        }

        return signalResults.Any(result => result.Status is SignalStatus.UnavailableEvidence
            or SignalStatus.RequiresHumanReview)
            ? CategoryStatus.InsufficientData
            : CategoryStatus.NotEvaluated;
    }

    private static IReadOnlyList<SignalResult> NormalizeSignalResults(
        IEnumerable<SignalResult>? signalResults,
        AnalysisCategory category)
    {
        SignalResult[] normalized = signalResults?
            .Where(result => result is not null)
            .Distinct()
            .ToArray()
            ?? Array.Empty<SignalResult>();

        if (normalized.Any(result => result.Category != category))
        {
            throw new ArgumentException(
                "Every signal result must belong to the category.",
                nameof(signalResults));
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeValues(
        IEnumerable<string>? values) =>
        values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? Array.Empty<string>();

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

    private static int RoundHalfUp(decimal value) =>
        (int)Math.Floor(value + 0.5m);
}

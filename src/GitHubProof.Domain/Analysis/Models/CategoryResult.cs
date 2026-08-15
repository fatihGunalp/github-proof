using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record CategoryResult
{
    public CategoryResult(
        AnalysisCategory category,
        IEnumerable<SignalResult>? signalResults)
    {
        Category = category;
        SignalResults = NormalizeSignalResults(signalResults, category);

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

    private static int RoundHalfUp(decimal value) =>
        (int)Math.Floor(value + 0.5m);
}

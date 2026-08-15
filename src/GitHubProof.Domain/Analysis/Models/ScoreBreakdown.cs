using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record ScoreBreakdown
{
    public ScoreBreakdown(IEnumerable<CategoryResult>? categoryBreakdowns)
    {
        CategoryBreakdowns = NormalizeCategoryBreakdowns(categoryBreakdowns);

        SignalResult[] signalResults = CategoryBreakdowns
            .SelectMany(category => category.SignalResults)
            .ToArray();

        ExcludedSignals = signalResults
            .Where(result => !IsScored(result))
            .ToArray();

        ExcludedFromOverallScoreSignals = signalResults
            .Where(result => !result.IncludeInOverallScore)
            .ToArray();

        SignalResult[] includedSignals = signalResults
            .Where(result => result.IncludeInOverallScore && IsScored(result))
            .ToArray();

        EarnedScore = includedSignals.Sum(result => result.Score!.Value);
        ApplicableMaximumScore = includedSignals.Sum(result => result.MaximumScore);
        NormalizedScore = ApplicableMaximumScore == 0
            ? null
            : RoundHalfUp((decimal)EarnedScore / ApplicableMaximumScore * 100);
        LabelKey = NormalizedScore is null
            ? null
            : GetLabelKey(NormalizedScore.Value);
        CalculationMethod = ScoreCalculationMethod.RawApplicableScoreNormalization;
    }

    public int EarnedScore { get; }

    public int ApplicableMaximumScore { get; }

    public int? NormalizedScore { get; }

    public string? LabelKey { get; }

    public IReadOnlyList<CategoryResult> CategoryBreakdowns { get; }

    public IReadOnlyList<SignalResult> ExcludedSignals { get; }

    public IReadOnlyList<SignalResult> ExcludedFromOverallScoreSignals { get; }

    public ScoreCalculationMethod CalculationMethod { get; }

    private static bool IsScored(SignalResult result) =>
        result.Status is SignalStatus.Strong
            or SignalStatus.Present
            or SignalStatus.Partial
            or SignalStatus.Missing;

    private static IReadOnlyList<CategoryResult> NormalizeCategoryBreakdowns(
        IEnumerable<CategoryResult>? categoryBreakdowns)
    {
        CategoryResult[] normalized = categoryBreakdowns?
            .Where(category => category is not null)
            .ToArray()
            ?? Array.Empty<CategoryResult>();

        if (normalized
            .GroupBy(category => category.Category)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Only one result per analysis category is allowed.",
                nameof(categoryBreakdowns));
        }

        string[] duplicateSignalKeys = normalized
            .SelectMany(category => category.SignalResults)
            .GroupBy(signal => signal.SignalKey, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateSignalKeys.Length > 0)
        {
            throw new ArgumentException(
                "A signal result cannot appear in more than one category.",
                nameof(categoryBreakdowns));
        }

        return normalized;
    }

    private static int RoundHalfUp(decimal value) =>
        (int)Math.Floor(value + 0.5m);

    private static string GetLabelKey(int normalizedScore) => normalizedScore switch
    {
        <= 24 => "score.label.limited-visible-evidence",
        <= 49 => "score.label.basic-visible-evidence",
        <= 69 => "score.label.developing-visible-evidence",
        <= 84 => "score.label.strong-visible-evidence",
        _ => "score.label.very-strong-visible-evidence"
    };
}

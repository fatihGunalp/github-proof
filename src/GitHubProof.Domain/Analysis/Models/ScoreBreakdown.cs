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
        Label = NormalizedScore is null
            ? null
            : GetLabel(NormalizedScore.Value);
        CalculationMethod = ScoreCalculationMethod.RawApplicableScoreNormalization;
    }

    public int EarnedScore { get; }

    public int ApplicableMaximumScore { get; }

    public int? NormalizedScore { get; }

    public string? Label { get; }

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

    private static string GetLabel(int normalizedScore) => normalizedScore switch
    {
        <= 24 => "Sınırlı görünür kanıt",
        <= 49 => "Temel görünür kanıt",
        <= 69 => "Gelişen görünür kanıt",
        <= 84 => "Güçlü görünür kanıt",
        _ => "Çok güçlü görünür kanıt"
    };
}

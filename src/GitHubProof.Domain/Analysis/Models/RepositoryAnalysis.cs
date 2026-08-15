using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record RepositoryAnalysis
{
    public const int MaximumHighlightsPerType = 3;

    public RepositoryAnalysis(
        Guid analysisId,
        RepositoryIdentity repository,
        RepositorySnapshot snapshot,
        AnalysisStatus status,
        AnalysisVersion version,
        DateTimeOffset startedAt,
        DateTimeOffset? completedAt,
        IEnumerable<SignalResult>? signalResults,
        IEnumerable<CategoryResult>? categoryResults,
        ScoreBreakdown scoreBreakdown,
        IEnumerable<AnalysisHighlight>? strengths,
        IEnumerable<AnalysisHighlight>? criticalGaps,
        ImprovementPlan improvementPlan,
        IEnumerable<string>? limitationKeys,
        IEnumerable<AnalysisWarning>? warnings)
    {
        if (analysisId == Guid.Empty)
        {
            throw new ArgumentException(
                "Analysis ID cannot be empty.",
                nameof(analysisId));
        }

        AnalysisId = analysisId;
        Repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        Snapshot = snapshot
            ?? throw new ArgumentNullException(nameof(snapshot));

        if (Repository != Snapshot.Repository)
        {
            throw new ArgumentException(
                "Snapshot repository must match the analysis repository.",
                nameof(snapshot));
        }

        Status = status;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        StartedAt = startedAt.ToUniversalTime();
        CompletedAt = completedAt?.ToUniversalTime();

        if (CompletedAt < StartedAt)
        {
            throw new ArgumentException(
                "Completed time cannot be before started time.",
                nameof(completedAt));
        }

        SignalResults = NormalizeSignalResults(signalResults);
        CategoryResults = NormalizeCategoryResults(categoryResults);
        ScoreBreakdown = scoreBreakdown
            ?? throw new ArgumentNullException(nameof(scoreBreakdown));
        Strengths = NormalizeHighlights(strengths, HighlightType.Strength, nameof(strengths));
        CriticalGaps = NormalizeHighlights(
            criticalGaps,
            HighlightType.CriticalGap,
            nameof(criticalGaps));
        ImprovementPlan = improvementPlan
            ?? throw new ArgumentNullException(nameof(improvementPlan));
        LimitationKeys = NormalizeStrings(limitationKeys);
        Warnings = NormalizeWarnings(warnings);

        ValidateResultGraph();
    }

    public Guid AnalysisId { get; }

    public RepositoryIdentity Repository { get; }

    public RepositorySnapshot Snapshot { get; }

    public AnalysisStatus Status { get; }

    public AnalysisVersion Version { get; }

    public string AnalyzerVersion => Version.AnalyzerVersion;

    public string RuleSetVersion => Version.RuleSetVersion;

    public int ReportSchemaVersion => Version.ReportSchemaVersion;

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? CompletedAt { get; }

    public IReadOnlyList<SignalResult> SignalResults { get; }

    public IReadOnlyList<CategoryResult> CategoryResults { get; }

    public ScoreBreakdown ScoreBreakdown { get; }

    public IReadOnlyList<AnalysisHighlight> Strengths { get; }

    public IReadOnlyList<AnalysisHighlight> CriticalGaps { get; }

    public ImprovementPlan ImprovementPlan { get; }

    public IReadOnlyList<string> LimitationKeys { get; }

    public IReadOnlyList<AnalysisWarning> Warnings { get; }

    private void ValidateResultGraph()
    {
        SignalResult[] categorySignals = CategoryResults
            .SelectMany(category => category.SignalResults)
            .ToArray();

        if (!HaveSameValues(SignalResults, categorySignals))
        {
            throw new ArgumentException(
                "Signal results must match the results owned by category results.",
                nameof(SignalResults));
        }

        if (!HaveSameValues(
                CategoryResults,
                ScoreBreakdown.CategoryBreakdowns))
        {
            throw new ArgumentException(
                "Category results must match score breakdown categories.",
                nameof(ScoreBreakdown));
        }

        if (!string.Equals(
                RuleSetVersion,
                ImprovementPlan.RuleSetVersion,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Improvement plan rule set version must match the analysis version.",
                nameof(ImprovementPlan));
        }
    }

    private static IReadOnlyList<SignalResult> NormalizeSignalResults(
        IEnumerable<SignalResult>? results)
    {
        SignalResult[] normalized = results?
            .Where(result => result is not null)
            .ToArray()
            ?? Array.Empty<SignalResult>();

        if (normalized
            .GroupBy(result => result.SignalKey, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Signal result keys must be unique.",
                nameof(results));
        }

        return normalized;
    }

    private static IReadOnlyList<CategoryResult> NormalizeCategoryResults(
        IEnumerable<CategoryResult>? results)
    {
        CategoryResult[] normalized = results?
            .Where(result => result is not null)
            .ToArray()
            ?? Array.Empty<CategoryResult>();

        if (normalized
            .GroupBy(result => result.Category)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Category results must be unique by category.",
                nameof(results));
        }

        return normalized;
    }

    private static IReadOnlyList<AnalysisHighlight> NormalizeHighlights(
        IEnumerable<AnalysisHighlight>? highlights,
        HighlightType expectedType,
        string parameterName)
    {
        AnalysisHighlight[] normalized = highlights?
            .Where(highlight => highlight is not null)
            .Distinct()
            .ToArray()
            ?? Array.Empty<AnalysisHighlight>();

        if (normalized.Length > MaximumHighlightsPerType)
        {
            throw new ArgumentException(
                $"No more than {MaximumHighlightsPerType} highlights are allowed.",
                parameterName);
        }

        if (normalized.Any(highlight => highlight.Type != expectedType))
        {
            throw new ArgumentException(
                $"All highlights must have type {expectedType}.",
                parameterName);
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeStrings(
        IEnumerable<string>? values) =>
        values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? Array.Empty<string>();

    private static IReadOnlyList<AnalysisWarning> NormalizeWarnings(
        IEnumerable<AnalysisWarning>? warnings) =>
        warnings?
            .Where(warning => warning is not null)
            .Distinct()
            .ToArray()
            ?? Array.Empty<AnalysisWarning>();

    private static bool HaveSameValues<T>(
        IReadOnlyCollection<T> first,
        IReadOnlyCollection<T> second)
        where T : notnull =>
        first.Count == second.Count
        && new HashSet<T>(first).SetEquals(second);
}

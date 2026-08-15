using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record SignalDefinition
{
    public SignalDefinition(
        string key,
        AnalysisCategory category,
        int maximumScore,
        IEnumerable<string> requiredDataKeys,
        bool includeInOverallScore,
        int version)
    {
        Key = RequireValue(key, nameof(key));
        Category = category;

        if (maximumScore <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumScore),
                maximumScore,
                "Maximum score must be greater than zero.");
        }

        MaximumScore = maximumScore;

        RequiredDataKeys = NormalizeValues(
            requiredDataKeys,
            nameof(requiredDataKeys));

        IncludeInOverallScore = includeInOverallScore;

        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(version),
                version,
                "Signal version must be greater than zero.");
        }

        Version = version;
    }

    public string Key { get; }

    public AnalysisCategory Category { get; }

    public int MaximumScore { get; }

    public IReadOnlyList<string> RequiredDataKeys { get; }

    public bool IncludeInOverallScore { get; }

    public int Version { get; }

    public string ApplicabilityRuleKey => $"{Key}.applicability.v{Version}";

    public string EvaluationRuleKey => $"{Key}.evaluation.v{Version}";

    public string EvidenceRequirementKey => $"{Key}.evidence.v{Version}";

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

    private static IReadOnlyList<string> NormalizeValues(
        IEnumerable<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);

        string[] normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "Collection must contain at least one value.",
                parameterName);
        }

        return normalized;
    }
}

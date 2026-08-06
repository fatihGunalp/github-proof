using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record SignalDefinition
{
    public SignalDefinition(
        string key,
        string name,
        AnalysisCategory category,
        string description,
        int maximumScore,
        IEnumerable<string> requiredDataKeys,
        IEnumerable<string> applicabilityRules,
        IEnumerable<string> evaluationRules,
        IEnumerable<string> evidenceRequirements,
        string explanationTemplate,
        string recommendationTemplate,
        bool includeInOverallScore,
        int version)
    {
        Key = RequireValue(key, nameof(key));
        Name = RequireValue(name, nameof(name));
        Category = category;
        Description = RequireValue(description, nameof(description));

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

        ApplicabilityRules = NormalizeValues(
            applicabilityRules,
            nameof(applicabilityRules));

        EvaluationRules = NormalizeValues(
            evaluationRules,
            nameof(evaluationRules));

        EvidenceRequirements = NormalizeValues(
            evidenceRequirements,
            nameof(evidenceRequirements));

        ExplanationTemplate = RequireValue(
            explanationTemplate,
            nameof(explanationTemplate));

        RecommendationTemplate = RequireValue(
            recommendationTemplate,
            nameof(recommendationTemplate));

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

    public string Name { get; }

    public AnalysisCategory Category { get; }

    public string Description { get; }

    public int MaximumScore { get; }

    public IReadOnlyList<string> RequiredDataKeys { get; }

    public IReadOnlyList<string> ApplicabilityRules { get; }

    public IReadOnlyList<string> EvaluationRules { get; }

    public IReadOnlyList<string> EvidenceRequirements { get; }

    public string ExplanationTemplate { get; }

    public string RecommendationTemplate { get; }

    public bool IncludeInOverallScore { get; }

    public int Version { get; }

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

using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record ImprovementAction
{
    public ImprovementAction(
        int priority,
        string title,
        string reason,
        string expectedImpact,
        IEnumerable<string> relatedSignals,
        EffortLevel effort,
        IEnumerable<string> evidenceReferences)
    {
        Priority = priority;
        Title = RequireValue(title, nameof(title));
        Reason = RequireValue(reason, nameof(reason));
        ExpectedImpact = RequireValue(expectedImpact, nameof(expectedImpact));
        RelatedSignals = NormalizeReferences(relatedSignals, nameof(relatedSignals));
        Effort = effort;
        EvidenceReferences = NormalizeReferences(
            evidenceReferences,
            nameof(evidenceReferences));
    }

    public int Priority { get; }

    public string Title { get; }

    public string Reason { get; }

    public string ExpectedImpact { get; }

    public IReadOnlyList<string> RelatedSignals { get; }

    public EffortLevel Effort { get; }

    public IReadOnlyList<string> EvidenceReferences { get; }

    private static IReadOnlyList<string> NormalizeReferences(
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
}

using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record AnalysisHighlight
{
    public AnalysisHighlight(
        HighlightType type,
        string signalKey,
        string title,
        string summary,
        int scoreImpact,
        IEnumerable<string> evidenceReferences)
    {
        Type = type;
        SignalKey = RequireValue(signalKey, nameof(signalKey));
        Title = RequireValue(title, nameof(title));
        Summary = RequireValue(summary, nameof(summary));
        ScoreImpact = scoreImpact;
        EvidenceReferences = NormalizeEvidenceReferences(evidenceReferences);
    }

    public HighlightType Type { get; }

    public string SignalKey { get; }

    public string Title { get; }

    public string Summary { get; }

    public int ScoreImpact { get; }

    public IReadOnlyList<string> EvidenceReferences { get; }

    private static IReadOnlyList<string> NormalizeEvidenceReferences(
        IEnumerable<string> evidenceReferences)
    {
        ArgumentNullException.ThrowIfNull(evidenceReferences);

        string[] normalized = evidenceReferences
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference => reference.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "At least one evidence reference is required.",
                nameof(evidenceReferences));
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

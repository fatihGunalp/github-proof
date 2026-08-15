using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record AnalysisWarning
{
    public AnalysisWarning(
        string code,
        WarningSeverity severity,
        string presentationKey,
        IEnumerable<string>? relatedDataKeys,
        IEnumerable<string>? relatedSignals)
    {
        Code = RequireValue(code, nameof(code));
        Severity = severity;
        PresentationKey = RequireValue(presentationKey, nameof(presentationKey));
        RelatedDataKeys = NormalizeReferences(relatedDataKeys);
        RelatedSignals = NormalizeReferences(relatedSignals);
    }

    public string Code { get; }

    public WarningSeverity Severity { get; }

    public string PresentationKey { get; }

    public IReadOnlyList<string> RelatedDataKeys { get; }

    public IReadOnlyList<string> RelatedSignals { get; }

    private static IReadOnlyList<string> NormalizeReferences(
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
}

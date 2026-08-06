using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record EvidenceItem
{
    public EvidenceItem(
        string key,
        string source,
        string value,
        string? location,
        DateTimeOffset observedAt,
        EvidenceConfidence confidence,
        string? notes)
    {
        Key = RequireValue(key, nameof(key));
        Source = RequireValue(source, nameof(source));
        Value = RequireValue(value, nameof(value));
        Location = NormalizeOptional(location);
        ObservedAt = observedAt.ToUniversalTime();
        Confidence = confidence;
        Notes = NormalizeOptional(notes);
    }

    public string Key { get; }

    public string Source { get; }

    public string Value { get; }

    public string? Location { get; }

    public DateTimeOffset ObservedAt { get; }

    public EvidenceConfidence Confidence { get; }

    public string? Notes { get; }

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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}

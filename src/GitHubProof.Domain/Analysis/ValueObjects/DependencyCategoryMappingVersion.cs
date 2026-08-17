namespace GitHubProof.Domain.Analysis.ValueObjects;

// Owns package identity and alias mapping semantics only. It never governs
// produced-work detection, ProblemArea qualification or profile promotion.
public sealed record DependencyCategoryMappingVersion
{
    public DependencyCategoryMappingVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}

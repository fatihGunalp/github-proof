namespace GitHubProof.Domain.Analysis.ValueObjects;

// Owns dependency source-reference detection semantics only, as contracted by
// docs/architecture/repository-dependency-source-reference-detection.md §17:
// the supported ecosystem surface, the extension-to-ecosystem map, the
// qualifying syntactic forms, the lexical exclusion and desynchronization
// rules, the literal-only call rule, identity resolution and its tie-breakers,
// the ambiguity rule, the structurally unreferenceable classes, the observation
// payload and the deterministic ordering.
//
// It never governs collection or snapshot normalization, scoring, dependency
// category mapping, profile observation or profile pattern promotion.
public sealed record SourceReferenceRuleVersion
{
    // The first runtime detection semantics, established by Session 073 against
    // contract version 1 as amended.
    public static readonly SourceReferenceRuleVersion V1 = new("1.0.0");

    public SourceReferenceRuleVersion(string value)
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

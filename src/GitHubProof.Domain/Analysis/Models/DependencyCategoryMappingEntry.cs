using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

// One registry mapping, already reduced to lookup identity. Review rationale is
// non-semantic and is deliberately absent, so no runtime result can depend on
// it.
public sealed record DependencyCategoryMappingEntry
{
    public DependencyCategoryMappingEntry(
        DependencyEcosystem ecosystem,
        string lookupIdentity,
        DependencySemanticCategory category)
    {
        if (!Enum.IsDefined(ecosystem))
        {
            throw new ArgumentOutOfRangeException(
                nameof(ecosystem),
                ecosystem,
                "Dependency ecosystem is not defined.");
        }

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(
                nameof(category),
                category,
                "Dependency semantic category is not defined.");
        }

        if (string.IsNullOrWhiteSpace(lookupIdentity))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                nameof(lookupIdentity));
        }

        Ecosystem = ecosystem;
        LookupIdentity = lookupIdentity.Trim();
        Category = category;
    }

    public DependencyEcosystem Ecosystem { get; }

    public string LookupIdentity { get; }

    public DependencySemanticCategory Category { get; }
}

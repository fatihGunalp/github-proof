using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis;

// Typed, validated mapping registry. It knows nothing about JSON, files or
// embedded resources: a loader outside Domain supplies already parsed entries.
public sealed class DependencyCategoryRegistry
{
    private readonly IReadOnlyDictionary<(DependencyEcosystem Ecosystem, string Identity), DependencySemanticCategory> entriesByIdentity;

    public DependencyCategoryRegistry(
        DependencyCategoryMappingVersion mappingVersion,
        IEnumerable<DependencyCategoryMappingEntry> entries)
    {
        MappingVersion = mappingVersion
            ?? throw new ArgumentNullException(nameof(mappingVersion));
        ArgumentNullException.ThrowIfNull(entries);

        Dictionary<(DependencyEcosystem, string), DependencySemanticCategory> byIdentity = [];
        List<DependencyCategoryMappingEntry> normalized = [];

        foreach (DependencyCategoryMappingEntry entry in entries)
        {
            ArgumentNullException.ThrowIfNull(entry);

            if (!byIdentity.TryAdd(
                    (entry.Ecosystem, entry.LookupIdentity),
                    entry.Category))
            {
                throw new ArgumentException(
                    $"Duplicate mapping for {entry.Ecosystem} '{entry.LookupIdentity}'.",
                    nameof(entries));
            }

            normalized.Add(entry);
        }

        normalized.Sort(static (left, right) =>
        {
            int ecosystem = string.CompareOrdinal(
                left.Ecosystem.ToString(),
                right.Ecosystem.ToString());

            return ecosystem != 0
                ? ecosystem
                : string.CompareOrdinal(left.LookupIdentity, right.LookupIdentity);
        });

        Entries = normalized.AsReadOnly();
        entriesByIdentity = byIdentity;
    }

    public DependencyCategoryMappingVersion MappingVersion { get; }

    public IReadOnlyList<DependencyCategoryMappingEntry> Entries { get; }

    // An unmapped or unknown package resolves to nothing. Absence of a mapping
    // is never absence of the technology.
    public DependencySemanticCategory? Resolve(RepositoryDependencyDeclaration declaration)
    {
        ArgumentNullException.ThrowIfNull(declaration);

        string identity = DependencyIdentityNormalizer.Normalize(
            declaration.Ecosystem,
            declaration.PackageId);

        return entriesByIdentity.TryGetValue(
            (declaration.Ecosystem, identity),
            out DependencySemanticCategory category)
            ? category
            : null;
    }
}

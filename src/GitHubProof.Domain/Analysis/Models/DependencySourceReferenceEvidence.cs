using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

// One dependency's source-reference occurrence in one repository.
//
// Its maximum meaning is the locked syntactic claim of
// docs/architecture/repository-dependency-source-reference-detection.md §3: at
// the pinned commit, a collected source artefact contains a qualifying
// import-declaration form, outside comments and string literals, whose resolved
// reference identity is accepted by exactly this declaration.
//
// It is never usage, never runtime or production use, never produced work,
// never ProblemArea evidence and never a statement about the developer.
// Several locations never make the observation stronger: they are traceability,
// not weight.
public sealed class DependencySourceReferenceEvidence
{
    public DependencySourceReferenceEvidence(
        DependencyEcosystem ecosystem,
        RepositoryDependencyDeclaration declaration,
        string resolvedReferenceIdentity,
        IEnumerable<DependencySourceReferenceLocation> locations)
    {
        if (!Enum.IsDefined(ecosystem))
        {
            throw new ArgumentOutOfRangeException(
                nameof(ecosystem),
                ecosystem,
                "Dependency ecosystem is not defined.");
        }

        ArgumentNullException.ThrowIfNull(declaration);

        if (string.IsNullOrWhiteSpace(resolvedReferenceIdentity))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                nameof(resolvedReferenceIdentity));
        }

        ArgumentNullException.ThrowIfNull(locations);

        Ecosystem = ecosystem;
        Declaration = declaration;
        ResolvedReferenceIdentity = resolvedReferenceIdentity.Trim();
        Locations = NormalizeLocations(locations);
    }

    public DependencyEcosystem Ecosystem { get; }

    // The source declaration itself, never a rewritten identity.
    public RepositoryDependencyDeclaration Declaration { get; }

    // The reference identity the source actually carried, which is not the
    // declared package identity in every ecosystem.
    public string ResolvedReferenceIdentity { get; }

    public IReadOnlyList<DependencySourceReferenceLocation> Locations { get; }

    private static IReadOnlyList<DependencySourceReferenceLocation> NormalizeLocations(
        IEnumerable<DependencySourceReferenceLocation> locations)
    {
        List<DependencySourceReferenceLocation> normalized = locations
            .Distinct()
            .ToList();

        if (normalized.Any(location => location is null))
        {
            throw new ArgumentException(
                "Locations cannot contain null values.",
                nameof(locations));
        }

        if (normalized.Count == 0)
        {
            throw new ArgumentException(
                "A source reference observation requires at least one location.",
                nameof(locations));
        }

        normalized.Sort(CompareLocations);

        return new ReadOnlyCollection<DependencySourceReferenceLocation>(normalized);
    }

    private static int CompareLocations(
        DependencySourceReferenceLocation left,
        DependencySourceReferenceLocation right)
    {
        int path = string.CompareOrdinal(left.SourcePath, right.SourcePath);

        if (path != 0)
        {
            return path;
        }

        int line = left.Line.CompareTo(right.Line);

        return line != 0
            ? line
            : string.CompareOrdinal(left.Form.ToString(), right.Form.ToString());
    }
}

using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

// One semantic category occurrence for one repository. Supporting declarations
// exist for traceability only: several packages in one category never make the
// evidence stronger, and this is never ProblemArea evidence.
public sealed class DependencySemanticCategoryEvidence
{
    public DependencySemanticCategoryEvidence(
        DependencySemanticCategory category,
        IEnumerable<RepositoryDependencyDeclaration> supportingDeclarations)
    {
        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(
                nameof(category),
                category,
                "Dependency semantic category is not defined.");
        }

        ArgumentNullException.ThrowIfNull(supportingDeclarations);

        Category = category;
        SupportingDeclarations = NormalizeDeclarations(supportingDeclarations, category);
    }

    public DependencySemanticCategory Category { get; }

    public IReadOnlyList<RepositoryDependencyDeclaration> SupportingDeclarations { get; }

    private static IReadOnlyList<RepositoryDependencyDeclaration> NormalizeDeclarations(
        IEnumerable<RepositoryDependencyDeclaration> supportingDeclarations,
        DependencySemanticCategory category)
    {
        List<RepositoryDependencyDeclaration> normalized = supportingDeclarations
            .Distinct()
            .ToList();

        if (normalized.Any(declaration => declaration is null))
        {
            throw new ArgumentException(
                "Supporting declarations cannot contain null values.",
                nameof(supportingDeclarations));
        }

        if (normalized.Count == 0)
        {
            throw new ArgumentException(
                $"Category '{category}' requires at least one supporting declaration.",
                nameof(supportingDeclarations));
        }

        normalized.Sort(static (left, right) =>
        {
            int path = string.CompareOrdinal(left.ManifestPath, right.ManifestPath);

            if (path != 0)
            {
                return path;
            }

            int package = string.CompareOrdinal(left.PackageId, right.PackageId);

            return package != 0
                ? package
                : string.CompareOrdinal(
                    left.DeclaredVersion ?? string.Empty,
                    right.DeclaredVersion ?? string.Empty);
        });

        return new ReadOnlyCollection<RepositoryDependencyDeclaration>(normalized);
    }
}

using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Analysis.Models;

// Non-scoring repository result derived from one RepositoryAnalysis and one
// mapping registry version. It participates in no signal, category or score, and
// it is never ProblemArea evidence.
public sealed class RepositorySemanticEvidence
{
    public RepositorySemanticEvidence(
        RepositoryAnalysisReference analysis,
        DependencyCategoryMappingVersion mappingVersion,
        DataAvailability? dependencyManifestsAvailability,
        bool areDependencyDeclarationsComplete,
        IEnumerable<DependencySemanticCategoryEvidence> categories)
    {
        Analysis = analysis
            ?? throw new ArgumentNullException(nameof(analysis));
        MappingVersion = mappingVersion
            ?? throw new ArgumentNullException(nameof(mappingVersion));
        DependencyManifestsAvailability = dependencyManifestsAvailability;
        AreDependencyDeclarationsComplete = areDependencyDeclarationsComplete;
        Categories = NormalizeCategories(categories);
    }

    public RepositoryAnalysisReference Analysis { get; }

    public DependencyCategoryMappingVersion MappingVersion { get; }

    // Null when the analysed snapshot recorded no dependency-manifests
    // availability at all. Unavailable and unrecorded are both undetermined,
    // never absence.
    public DataAvailability? DependencyManifestsAvailability { get; }

    public bool AreDependencyDeclarationsComplete { get; }

    public IReadOnlyList<DependencySemanticCategoryEvidence> Categories { get; }

    // True only when the supported manifest surface was scanned completely.
    // An empty category list under this flag means no mapped declaration was
    // observed, never that the repository lacks those technologies.
    public bool IsCategorySurfaceComplete =>
        DependencyManifestsAvailability?.IsAvailable == true
        && AreDependencyDeclarationsComplete;

    private static IReadOnlyList<DependencySemanticCategoryEvidence> NormalizeCategories(
        IEnumerable<DependencySemanticCategoryEvidence> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        List<DependencySemanticCategoryEvidence> normalized = categories.ToList();
        HashSet<Enums.DependencySemanticCategory> observed = [];

        foreach (DependencySemanticCategoryEvidence category in normalized)
        {
            ArgumentNullException.ThrowIfNull(category);

            if (!observed.Add(category.Category))
            {
                throw new ArgumentException(
                    $"Category '{category.Category}' is observed more than once.",
                    nameof(categories));
            }
        }

        normalized.Sort(static (left, right) =>
            left.Category.CompareTo(right.Category));

        return new ReadOnlyCollection<DependencySemanticCategoryEvidence>(normalized);
    }
}

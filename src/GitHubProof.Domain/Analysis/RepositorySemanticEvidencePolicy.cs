using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Analysis;

// Derives RepositorySemanticEvidence deterministically from one
// RepositoryAnalysis and one mapping registry version. It reads declarations
// only: it never parses manifests, never scores and never produces ProblemArea
// evidence.
public sealed class RepositorySemanticEvidencePolicy
{
    private readonly DependencyCategoryRegistry registry;

    public RepositorySemanticEvidencePolicy(DependencyCategoryRegistry registry)
    {
        this.registry = registry
            ?? throw new ArgumentNullException(nameof(registry));
    }

    public RepositorySemanticEvidence Compose(RepositoryAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        Dictionary<DependencySemanticCategory, List<RepositoryDependencyDeclaration>> supporting = [];

        foreach (RepositoryDependencyDeclaration declaration in
            analysis.Snapshot.DependencyDeclarations.Items)
        {
            DependencySemanticCategory? category = registry.Resolve(declaration);

            if (category is null)
            {
                continue;
            }

            if (!supporting.TryGetValue(
                    category.Value,
                    out List<RepositoryDependencyDeclaration>? declarations))
            {
                declarations = [];
                supporting.Add(category.Value, declarations);
            }

            declarations.Add(declaration);
        }

        return new RepositorySemanticEvidence(
            new RepositoryAnalysisReference(
                analysis.AnalysisId,
                analysis.Repository,
                analysis.Version),
            registry.MappingVersion,
            analysis.Snapshot.FindAvailability(RepositoryDataKeys.DependencyManifests),
            analysis.Snapshot.DependencyDeclarations.IsComplete,
            supporting.Select(category => new DependencySemanticCategoryEvidence(
                category.Key,
                category.Value)));
    }
}

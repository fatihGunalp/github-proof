using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

// The dependency evidence surface carries its own availability and completeness
// state. It never contributes snapshot collection warnings, because those turn
// RepositoryAnalysis.Status into Partial.
public sealed record GitHubRepositoryDependencyManifestCollection(
    RepositoryCollection<RepositoryDependencyDeclaration> Declarations,
    DataAvailability Availability)
{
    public bool IsComplete => Declarations.IsComplete;
}

using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

// The bounded raw source evidence surface carries its own availability and
// completeness state. It never contributes snapshot collection warnings, because
// those turn RepositoryAnalysis.Status into Partial.
public sealed record GitHubRepositorySourceFileCollection(
    RepositoryCollection<RepositorySourceFile> SourceFiles,
    DataAvailability Availability)
{
    public bool IsComplete => SourceFiles.IsComplete;
}

using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryContributorCollection(
    RepositoryCollection<RepositoryContributor> Contributors,
    DataAvailability Availability,
    IReadOnlyList<string> CollectionWarnings)
{
    public bool IsComplete => Contributors.IsComplete;
}

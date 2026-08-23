using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed record GitHubProfileRepositoryCollection(
    ProfileRepositoryManifest Repositories,
    DataAvailability Availability,
    IReadOnlyList<string> CollectionWarnings,
    RepositoryPresentationMetadata RepositoryPresentationMetadata)
{
    public bool IsComplete => Repositories.IsComplete;
}

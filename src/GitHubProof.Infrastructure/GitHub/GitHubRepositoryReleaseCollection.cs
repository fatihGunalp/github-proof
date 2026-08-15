using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryReleaseCollection(
    RepositoryCollection<RepositoryRelease> Releases,
    DataAvailability Availability,
    IReadOnlyList<string> CollectionWarnings)
{
    public bool IsComplete => Releases.IsComplete;
}

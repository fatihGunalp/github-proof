using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryTagCollection(
    RepositoryCollection<GitHubRepositoryTag> Tags,
    DataAvailability Availability,
    IReadOnlyList<string> CollectionWarnings)
{
    public bool IsComplete => Tags.IsComplete;
}

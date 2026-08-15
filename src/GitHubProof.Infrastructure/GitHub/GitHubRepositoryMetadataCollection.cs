using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryMetadataCollection(
    RepositoryIdentity? Repository,
    RepositoryMetadata? Metadata,
    IReadOnlyList<string> Topics,
    RepositoryLicense? License,
    DataAvailability Availability)
{
    public bool IsAvailable => Availability.IsAvailable;
}

using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed record GitHubProfileMetadataCollection(
    GitHubProfileIdentity? Identity,
    GitHubProfileMetadata? Metadata,
    DataAvailability Availability)
{
    public bool IsAvailable => Availability.IsAvailable;
}

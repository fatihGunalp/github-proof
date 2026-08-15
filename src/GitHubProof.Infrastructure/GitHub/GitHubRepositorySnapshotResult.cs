using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositorySnapshotResult
{
    public GitHubRepositorySnapshotResult(
        RepositorySnapshot? snapshot,
        DataAvailability metadataAvailability)
    {
        Snapshot = snapshot;
        MetadataAvailability = metadataAvailability
            ?? throw new ArgumentNullException(nameof(metadataAvailability));

        if (snapshot is not null && !metadataAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "A repository snapshot requires available metadata.",
                nameof(metadataAvailability));
        }
    }

    public RepositorySnapshot? Snapshot { get; }

    public DataAvailability MetadataAvailability { get; }

    public bool IsSuccessful => Snapshot is not null;
}

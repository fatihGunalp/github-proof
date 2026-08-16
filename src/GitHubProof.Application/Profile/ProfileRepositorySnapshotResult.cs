using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Application.Profile;

public sealed record ProfileRepositorySnapshotResult
{
    public ProfileRepositorySnapshotResult(
        RepositorySnapshot? snapshot,
        DataAvailability availability)
    {
        Snapshot = snapshot;
        Availability = availability
            ?? throw new ArgumentNullException(nameof(availability));

        if (snapshot is not null && !availability.IsAvailable)
        {
            throw new ArgumentException(
                "A repository snapshot requires available repository metadata.",
                nameof(availability));
        }

        if (snapshot is null && availability.IsAvailable)
        {
            throw new ArgumentException(
                "Available repository metadata must produce a repository snapshot.",
                nameof(availability));
        }
    }

    public RepositorySnapshot? Snapshot { get; }

    public DataAvailability Availability { get; }

    public bool IsSuccessful => Snapshot is not null;
}

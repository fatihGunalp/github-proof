using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed record GitHubProfileSnapshotResult
{
    public GitHubProfileSnapshotResult(
        GitHubProfileSnapshot? snapshot,
        DataAvailability profileAvailability)
    {
        Snapshot = snapshot;
        ProfileAvailability = profileAvailability
            ?? throw new ArgumentNullException(nameof(profileAvailability));

        if (snapshot is not null && !profileAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "A profile snapshot requires available profile metadata.",
                nameof(profileAvailability));
        }
    }

    public GitHubProfileSnapshot? Snapshot { get; }

    public DataAvailability ProfileAvailability { get; }

    public bool IsSuccessful => Snapshot is not null;
}

using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// The production IProfileRepositorySnapshotSource adapter. It performs no
// collection of its own: it reuses the existing single-repository GitHub
// snapshot collection path (IGitHubRepositorySnapshotCollector) and adapts its
// result shape, preserving availability, failure and cancellation exactly as
// that path already reports them.
public sealed class GitHubProfileRepositorySnapshotSource : IProfileRepositorySnapshotSource
{
    private readonly IGitHubRepositorySnapshotCollector snapshotCollector;

    public GitHubProfileRepositorySnapshotSource(
        IGitHubRepositorySnapshotCollector snapshotCollector)
    {
        this.snapshotCollector = snapshotCollector
            ?? throw new ArgumentNullException(nameof(snapshotCollector));
    }

    public async Task<ProfileRepositorySnapshotResult> CollectAsync(
        ProfileRepositoryReference reference,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);

        GitHubRepositorySnapshotResult result = await snapshotCollector.CollectAsync(
            reference.Url.AbsoluteUri,
            cancellationToken);

        return new ProfileRepositorySnapshotResult(
            result.Snapshot,
            result.MetadataAvailability);
    }
}

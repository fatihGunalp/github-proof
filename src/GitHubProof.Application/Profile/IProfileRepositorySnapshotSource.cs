using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

public interface IProfileRepositorySnapshotSource
{
    Task<ProfileRepositorySnapshotResult> CollectAsync(
        ProfileRepositoryReference reference,
        CancellationToken cancellationToken = default);
}

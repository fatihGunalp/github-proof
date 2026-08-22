namespace GitHubProof.Infrastructure.GitHub.Profile;

public interface IGitHubProfileSnapshotCollector
{
    // A supplied progress carrier must be bound to the same execution
    // cancellation boundary represented by cancellationToken.
    Task<GitHubProfileSnapshotResult> CollectAsync(
        string profile,
        CancellationToken cancellationToken = default,
        GitHubProfileSnapshotProgress? progress = null);
}

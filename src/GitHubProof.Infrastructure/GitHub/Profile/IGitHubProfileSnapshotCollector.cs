namespace GitHubProof.Infrastructure.GitHub.Profile;

public interface IGitHubProfileSnapshotCollector
{
    Task<GitHubProfileSnapshotResult> CollectAsync(
        string profile,
        CancellationToken cancellationToken = default);
}

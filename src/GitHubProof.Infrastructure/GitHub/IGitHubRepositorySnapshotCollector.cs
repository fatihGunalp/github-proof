namespace GitHubProof.Infrastructure.GitHub;

public interface IGitHubRepositorySnapshotCollector
{
    Task<GitHubRepositorySnapshotResult> CollectAsync(
        string repositoryUrl,
        CancellationToken cancellationToken = default);
}

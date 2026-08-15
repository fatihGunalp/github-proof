namespace GitHubProof.Infrastructure.GitHub;

public interface IGitHubRepositoryMetadataCollector
{
    Task<GitHubRepositoryMetadataCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        CancellationToken cancellationToken = default);
}

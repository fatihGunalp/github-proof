namespace GitHubProof.Infrastructure.GitHub.Profile;

public interface IGitHubProfileRepositoryCollector
{
    Task<GitHubProfileRepositoryCollection> CollectAsync(
        GitHubProfileHandle profile,
        GitHubProfileMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

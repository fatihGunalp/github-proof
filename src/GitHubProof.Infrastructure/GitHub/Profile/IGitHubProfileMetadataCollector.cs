namespace GitHubProof.Infrastructure.GitHub.Profile;

public interface IGitHubProfileMetadataCollector
{
    Task<GitHubProfileMetadataCollection> CollectAsync(
        GitHubProfileHandle profile,
        CancellationToken cancellationToken = default);
}

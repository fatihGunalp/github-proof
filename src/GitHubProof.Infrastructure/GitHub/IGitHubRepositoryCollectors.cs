namespace GitHubProof.Infrastructure.GitHub;

public interface IGitHubRepositoryDescriptiveDataCollector
{
    Task<GitHubRepositoryDescriptiveDataCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        GitHubCommitReference? contentReference = null,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryCommitCollector
{
    Task<GitHubRepositoryCommitCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryIssueCollector
{
    Task<GitHubRepositoryIssueCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryPullRequestCollector
{
    Task<GitHubRepositoryPullRequestCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryReleaseCollector
{
    Task<GitHubRepositoryReleaseCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryTagCollector
{
    Task<GitHubRepositoryTagCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryContributorCollector
{
    Task<GitHubRepositoryContributorCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryDependencyManifestCollector
{
    Task<GitHubRepositoryDependencyManifestCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryContentsCollection contents,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositorySourceFileCollector
{
    Task<GitHubRepositorySourceFileCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryContentsCollection contents,
        CancellationToken cancellationToken = default);
}

public interface IGitHubRepositoryContentsCollector
{
    Task<GitHubRepositoryContentsCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        GitHubCommitReference? contentReference = null,
        CancellationToken cancellationToken = default);
}

using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed class GitHubProfileSnapshotOrchestrator : IGitHubProfileSnapshotCollector
{
    public static GitHubProfileSnapshotOrchestrator CreateDefault(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        return new GitHubProfileSnapshotOrchestrator(
            new GitHubProfileMetadataCollector(httpClient, timeProvider),
            new GitHubProfileRepositoryCollector(httpClient, timeProvider),
            timeProvider);
    }

    private readonly IGitHubProfileMetadataCollector metadataCollector;
    private readonly IGitHubProfileRepositoryCollector repositoryCollector;
    private readonly TimeProvider timeProvider;

    public GitHubProfileSnapshotOrchestrator(
        IGitHubProfileMetadataCollector metadataCollector,
        IGitHubProfileRepositoryCollector repositoryCollector,
        TimeProvider? timeProvider = null)
    {
        this.metadataCollector = metadataCollector
            ?? throw new ArgumentNullException(nameof(metadataCollector));
        this.repositoryCollector = repositoryCollector
            ?? throw new ArgumentNullException(nameof(repositoryCollector));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubProfileSnapshotResult> CollectAsync(
        string profile,
        CancellationToken cancellationToken = default)
    {
        GitHubProfileHandle handle = GitHubProfileHandle.Parse(profile);
        GitHubProfileMetadataCollection metadata = await metadataCollector.CollectAsync(
            handle,
            cancellationToken);

        if (!metadata.IsAvailable)
        {
            return new GitHubProfileSnapshotResult(
                snapshot: null,
                metadata.Availability);
        }

        if (metadata.Identity is null || metadata.Metadata is null)
        {
            throw new InvalidOperationException(
                "Available profile metadata must include profile identity and metadata.");
        }

        DateTimeOffset capturedAt = timeProvider.GetUtcNow();
        GitHubProfileRepositoryCollection repositories =
            await repositoryCollector.CollectAsync(
                handle,
                metadata,
                cancellationToken);

        GitHubProfileSnapshot snapshot = new(
            metadata.Identity,
            capturedAt,
            metadata.Metadata,
            repositories.Repositories,
            [metadata.Availability, repositories.Availability],
            CollectWarnings(metadata.Metadata, repositories));

        return new GitHubProfileSnapshotResult(snapshot, metadata.Availability);
    }

    private static IReadOnlyList<string> CollectWarnings(
        GitHubProfileMetadata metadata,
        GitHubProfileRepositoryCollection repositories)
    {
        List<string> warnings = [.. repositories.CollectionWarnings];

        if (repositories.IsComplete
            && repositories.Repositories.Repositories.Count
                != metadata.PublicRepositoryCount)
        {
            warnings.Add(
                $"GitHub reported {metadata.PublicRepositoryCount} public repositories, "
                + $"but {repositories.Repositories.Repositories.Count} were collected.");
        }

        return warnings;
    }
}

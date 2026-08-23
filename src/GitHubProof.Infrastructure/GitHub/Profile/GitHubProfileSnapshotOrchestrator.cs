using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed class GitHubProfileSnapshotOrchestrator : IGitHubProfileSnapshotCollector
{
    public static GitHubProfileSnapshotOrchestrator CreateDefault(
        HttpClient httpClient,
        TimeProvider? timeProvider = null,
        IGitHubProfileBudgetProtection? budgetProtection = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        return new GitHubProfileSnapshotOrchestrator(
            new GitHubProfileMetadataCollector(httpClient, timeProvider),
            new GitHubProfileRepositoryCollector(httpClient, timeProvider),
            timeProvider,
            budgetProtection);
    }

    private readonly IGitHubProfileMetadataCollector metadataCollector;
    private readonly IGitHubProfileRepositoryCollector repositoryCollector;
    private readonly TimeProvider timeProvider;
    private readonly IGitHubProfileBudgetProtection budgetProtection;

    public GitHubProfileSnapshotOrchestrator(
        IGitHubProfileMetadataCollector metadataCollector,
        IGitHubProfileRepositoryCollector repositoryCollector,
        TimeProvider? timeProvider = null,
        IGitHubProfileBudgetProtection? budgetProtection = null)
    {
        this.metadataCollector = metadataCollector
            ?? throw new ArgumentNullException(nameof(metadataCollector));
        this.repositoryCollector = repositoryCollector
            ?? throw new ArgumentNullException(nameof(repositoryCollector));
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.budgetProtection = budgetProtection
            ?? DisabledGitHubProfileBudgetProtection.Instance;
    }

    public async Task<GitHubProfileSnapshotResult> CollectAsync(
        string profile,
        CancellationToken cancellationToken = default,
        GitHubProfileSnapshotProgress? progress = null)
    {
        GitHubProfileHandle handle = GitHubProfileHandle.Parse(profile);
        GitHubProfileMetadataCollection metadata = await metadataCollector.CollectAsync(
            handle,
            cancellationToken);

        // Metadata is the authoritative point at which ProfileAvailability is
        // resolved. The progress carrier atomically arbitrates this publication
        // against execution cancellation: a late typed collector return cannot
        // overwrite cancellation that already won, while a winning publication
        // remains visible through any later cancellation.
        progress?.ResolveProfileAvailability(metadata.Availability);

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

        // This decision is deliberately after Available metadata publication
        // and before captured-at/manifest work. A rejection escapes as a typed
        // operational signal; no snapshot, collector availability, warning or
        // completed-so-far profile can be fabricated here.
        budgetProtection.EnsureCanContinueAfterAvailableMetadata();

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
            CollectWarnings(metadata.Metadata, repositories),
            repositories.RepositoryPresentationMetadata);

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

using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// The production IProfileRepositorySnapshotSource adapter performs no
// collection of its own: it reuses IGitHubRepositorySnapshotCollector as-is.
// These tests prove the adapter introduces no new collection semantics — it
// only translates the reference/result shape and preserves availability,
// failure and cancellation exactly as the wrapped collector reports them.
public sealed class GitHubProfileRepositorySnapshotSourceTests
{
    private static readonly ProfileRepositoryReference Reference =
        new("octocat", "hello-world", new Uri("https://github.com/octocat/hello-world"));

    [Fact]
    public async Task CollectAsync_RequestsTheReferencesCanonicalUrl()
    {
        FakeCollector collector = new(SuccessfulResult());

        await new GitHubProfileRepositorySnapshotSource(collector)
            .CollectAsync(Reference);

        Assert.Equal(
            "https://github.com/octocat/hello-world",
            collector.LastRepositoryUrl);
    }

    [Fact]
    public async Task CollectAsync_WhenCollectionSucceeds_PreservesSnapshotAndAvailability()
    {
        GitHubRepositorySnapshotResult successful = SuccessfulResult();
        FakeCollector collector = new(successful);

        ProfileRepositorySnapshotResult result = await new GitHubProfileRepositorySnapshotSource(collector)
            .CollectAsync(Reference);

        Assert.True(result.IsSuccessful);
        Assert.Same(successful.Snapshot, result.Snapshot);
        Assert.Same(successful.MetadataAvailability, result.Availability);
    }

    [Fact]
    public async Task CollectAsync_WhenCollectionIsUnavailable_PreservesFailure()
    {
        DataAvailability unavailable = DataAvailability.Unavailable(
            "metadata",
            DataAvailabilityStatus.RateLimited,
            "GitHub rate limit reached.");
        FakeCollector collector = new(new GitHubRepositorySnapshotResult(
            snapshot: null,
            unavailable));

        ProfileRepositorySnapshotResult result = await new GitHubProfileRepositorySnapshotSource(collector)
            .CollectAsync(Reference);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.Snapshot);
        Assert.Same(unavailable, result.Availability);
    }

    [Fact]
    public async Task CollectAsync_PropagatesCancellation()
    {
        FakeCollector collector = new(SuccessfulResult());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => new GitHubProfileRepositorySnapshotSource(collector)
                .CollectAsync(Reference, cancellation.Token));
    }

    private static GitHubRepositorySnapshotResult SuccessfulResult()
    {
        DateTimeOffset capturedAt = DateTimeOffset.Parse("2026-08-16T00:00:00Z");

        RepositorySnapshot snapshot = new(
            new RepositoryIdentity(
                Reference.Owner,
                Reference.Name,
                Reference.Url,
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            capturedAt,
            headCommitSha: "abc123",
            metadata: null,
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability: null,
            collectionWarnings: null);

        return new GitHubRepositorySnapshotResult(
            snapshot,
            DataAvailability.Available("metadata", capturedAt));
    }

    private sealed class FakeCollector(GitHubRepositorySnapshotResult result)
        : IGitHubRepositorySnapshotCollector
    {
        public string? LastRepositoryUrl { get; private set; }

        public Task<GitHubRepositorySnapshotResult> CollectAsync(
            string repositoryUrl,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastRepositoryUrl = repositoryUrl;

            return Task.FromResult(result);
        }
    }
}

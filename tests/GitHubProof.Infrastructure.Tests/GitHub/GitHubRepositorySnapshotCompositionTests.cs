using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositorySnapshotCompositionTests
{
    [Fact]
    public void CreateDefault_WithHttpClient_ReturnsCanonicalOrchestrator()
    {
        using HttpClient httpClient = new();

        GitHubRepositorySnapshotOrchestrator orchestrator =
            GitHubRepositorySnapshotOrchestrator.CreateDefault(httpClient);

        Assert.NotNull(orchestrator);
    }

    [Fact]
    public void CreateDefault_WithExplicitTimeProvider_ReturnsCanonicalOrchestrator()
    {
        using HttpClient httpClient = new();
        FixedTimeProvider timeProvider = new(
            DateTimeOffset.Parse("2026-08-16T00:00:00Z"));

        GitHubRepositorySnapshotOrchestrator orchestrator =
            GitHubRepositorySnapshotOrchestrator.CreateDefault(
                httpClient,
                timeProvider);

        Assert.NotNull(orchestrator);
    }

    [Fact]
    public void CreateDefault_WhenHttpClientIsNull_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(
            () => GitHubRepositorySnapshotOrchestrator.CreateDefault(null!));
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

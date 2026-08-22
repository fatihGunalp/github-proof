using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;
using static GitHubProof.Infrastructure.Tests.GitHub.Profile.GitHubProfileTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

public sealed class GitHubProfileSnapshotOrchestratorTests
{
    private const string ProfileJson = """
        {
          "login": "octocat",
          "type": "User",
          "name": "The Octocat",
          "created_at": "2011-01-25T18:44:36Z",
          "public_repos": 2
        }
        """;

    [Fact]
    public async Task CollectAsync_ComposesProfileSnapshotFromMetadataAndManifest()
    {
        GitHubProfileSnapshotResult result = await CreateOrchestrator(
                JsonResponse(ProfileJson),
                JsonResponse($"[{RepositoryJson("zeta")}, {RepositoryJson("alpha")}]"))
            .CollectAsync("https://github.com/octocat");

        GitHubProfileSnapshot snapshot = result.Snapshot!;

        Assert.True(result.IsSuccessful);
        Assert.Equal("octocat", snapshot.Identity.Login);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), snapshot.CapturedAt);
        Assert.Equal(2, snapshot.Metadata!.PublicRepositoryCount);
        Assert.Equal(
            ["octocat/alpha", "octocat/zeta"],
            snapshot.Repositories.Repositories.Select(
                repository => repository.Reference.FullName));
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("profile")!.Status);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("profile-repositories")!.Status);
        Assert.Empty(snapshot.CollectionWarnings);
    }

    [Fact]
    public async Task CollectAsync_ProducesTheSameSnapshotForTheSameObservableState()
    {
        GitHubProfileSnapshotResult first = await CreateOrchestrator(
                JsonResponse(ProfileJson),
                JsonResponse($"[{RepositoryJson("zeta")}, {RepositoryJson("alpha")}]"))
            .CollectAsync("octocat");
        GitHubProfileSnapshotResult second = await CreateOrchestrator(
                JsonResponse(ProfileJson),
                JsonResponse($"[{RepositoryJson("alpha")}, {RepositoryJson("zeta")}]"))
            .CollectAsync("https://github.com/octocat");

        Assert.Equal(Describe(first.Snapshot!), Describe(second.Snapshot!));
    }

    [Fact]
    public async Task CollectAsync_ReturnsNoSnapshotWhenProfileIsUnavailable()
    {
        GitHubProfileSnapshotResult result = await CreateOrchestrator(
                JsonResponse("{}", HttpStatusCode.NotFound))
            .CollectAsync("octocat");

        Assert.False(result.IsSuccessful);
        Assert.Null(result.Snapshot);
        Assert.Equal(
            DataAvailabilityStatus.Unavailable,
            result.ProfileAvailability.Status);
    }

    [Fact]
    public async Task CollectAsync_KeepsPartialManifestWithoutInventingRepositories()
    {
        HttpResponseMessage repositoryPage = JsonResponse(
            $"[{RepositoryJson("alpha")}]");
        repositoryPage.Headers.Add(
            "Link",
            "<https://api.github.com/users/octocat/repos?page=2>; rel=\"next\"");

        GitHubProfileSnapshotResult result = await CreateOrchestrator(
                JsonResponse(ProfileJson),
                repositoryPage,
                JsonResponse("{}", HttpStatusCode.InternalServerError))
            .CollectAsync("octocat");

        GitHubProfileSnapshot snapshot = result.Snapshot!;

        Assert.False(snapshot.Repositories.IsComplete);
        Assert.Single(snapshot.Repositories.Repositories);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            snapshot.FindAvailability("profile-repositories")!.Status);
        Assert.NotEmpty(snapshot.CollectionWarnings);
    }

    [Fact]
    public async Task CollectAsync_WarnsWhenCollectedManifestDivergesFromReportedCount()
    {
        GitHubProfileSnapshotResult result = await CreateOrchestrator(
                JsonResponse(ProfileJson),
                JsonResponse($"[{RepositoryJson("alpha")}]"))
            .CollectAsync("octocat");

        Assert.Contains(
            result.Snapshot!.CollectionWarnings,
            warning => warning.Contains("2 public repositories", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAsync_RejectsInvalidProfileIdentityBeforeCallingGitHub()
    {
        QueueHttpMessageHandler handler = new();
        GitHubProfileSnapshotOrchestrator orchestrator =
            GitHubProfileSnapshotOrchestrator.CreateDefault(
                CreateClient(handler),
                CreateFixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentException>(
            () => orchestrator.CollectAsync("octo cat"));
        Assert.Empty(handler.RequestUris);
    }

    [Fact]
    public async Task CollectAsync_PublishesAvailableMetadataBeforeManifestContentCompletes()
    {
        CancellationObservingStream manifestContent = new();
        using CancellationTokenSource execution = new();
        using GitHubProfileSnapshotProgress progress = new(execution.Token);
        GitHubProfileSnapshotOrchestrator orchestrator = CreateOrchestrator(
            JsonResponse(ProfileJson),
            BlockingJsonResponse(manifestContent));

        Task<GitHubProfileSnapshotResult> collection = orchestrator.CollectAsync(
            "octocat",
            execution.Token,
            progress);

        await manifestContent.TokenObserved.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(progress.ProfileAvailability!.IsAvailable);

        await execution.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => collection);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CollectAsync_WhenExecutionCancellationWins_DoesNotPublishLateMetadata(
        bool metadataIsAvailable)
    {
        using CancellationTokenSource execution = new();
        using GitHubProfileSnapshotProgress progress = new(execution.Token);
        GitHubProfileMetadataCollection metadata = metadataIsAvailable
            ? AvailableProfileMetadata(publicRepositoryCount: 0)
            : UnavailableProfileMetadata();
        GitHubProfileSnapshotOrchestrator orchestrator = new(
            new CallbackMetadataCollector((_, _) =>
            {
                execution.Cancel();
                return Task.FromResult(metadata);
            }),
            new CallbackRepositoryCollector((_, _, _) =>
                Task.FromResult(EmptyRepositoryCollection())));

        GitHubProfileSnapshotResult result = await orchestrator.CollectAsync(
            "octocat",
            execution.Token,
            progress);

        Assert.True(progress.ExecutionCancellationWonBeforeResolution);
        Assert.Null(progress.ProfileAvailability);
        Assert.Equal(metadata.Availability, result.ProfileAvailability);
    }

    [Fact]
    public async Task CollectAsync_WhenAvailableMetadataWins_PreservesItAfterImmediateCancellation()
    {
        using CancellationTokenSource execution = new();
        using GitHubProfileSnapshotProgress progress = new(execution.Token);
        GitHubProfileMetadataCollection metadata =
            AvailableProfileMetadata(publicRepositoryCount: 0);
        GitHubProfileSnapshotOrchestrator orchestrator = new(
            new CallbackMetadataCollector((_, _) => Task.FromResult(metadata)),
            new CallbackRepositoryCollector((_, _, _) =>
            {
                execution.Cancel();
                return Task.FromResult(EmptyRepositoryCollection());
            }));

        GitHubProfileSnapshotResult result = await orchestrator.CollectAsync(
            "octocat",
            execution.Token,
            progress);

        Assert.True(result.IsSuccessful);
        Assert.False(progress.ExecutionCancellationWonBeforeResolution);
        Assert.Same(metadata.Availability, progress.ProfileAvailability);
    }

    [Fact]
    public async Task CollectAsync_WhenUnavailableMetadataWins_LaterCancellationCannotEraseIt()
    {
        using CancellationTokenSource execution = new();
        using GitHubProfileSnapshotProgress progress = new(execution.Token);
        GitHubProfileMetadataCollection metadata = UnavailableProfileMetadata();
        GitHubProfileSnapshotOrchestrator orchestrator = new(
            new CallbackMetadataCollector((_, _) => Task.FromResult(metadata)),
            new CallbackRepositoryCollector((_, _, _) =>
                throw new InvalidOperationException(
                    "Repository collection must not run for unavailable metadata.")));

        GitHubProfileSnapshotResult result = await orchestrator.CollectAsync(
            "octocat",
            execution.Token,
            progress);
        execution.Cancel();

        Assert.False(result.IsSuccessful);
        Assert.False(progress.ExecutionCancellationWonBeforeResolution);
        Assert.Same(metadata.Availability, progress.ProfileAvailability);
    }

    private static GitHubProfileSnapshotOrchestrator CreateOrchestrator(
        params HttpResponseMessage[] responses) =>
        GitHubProfileSnapshotOrchestrator.CreateDefault(
            CreateClient(new QueueHttpMessageHandler(responses)),
            CreateFixedTimeProvider());

    private static string Describe(GitHubProfileSnapshot snapshot) =>
        string.Join(
            "|",
            snapshot.Identity.Login,
            snapshot.CapturedAt.ToString("O"),
            snapshot.Repositories.IsComplete,
            string.Join(
                ",",
                snapshot.Repositories.Repositories.Select(repository => string.Join(
                    ";",
                    repository.Reference.FullName,
                    repository.Fork,
                    repository.TemplateDerived,
                    repository.Archived,
                    repository.Empty,
                    repository.LastPushedAt?.ToString("O") ?? "-"))),
            string.Join(",", snapshot.Availability.Select(
                availability => $"{availability.DataKey}={availability.Status}")),
            string.Join(",", snapshot.CollectionWarnings));

    private static GitHubProfileRepositoryCollection EmptyRepositoryCollection() =>
        new(
            new ProfileRepositoryManifest(),
            DataAvailability.Available(
                "profile-repositories",
                DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
            Array.Empty<string>());

    private sealed class CallbackMetadataCollector(
        Func<GitHubProfileHandle, CancellationToken,
            Task<GitHubProfileMetadataCollection>> callback)
        : IGitHubProfileMetadataCollector
    {
        public Task<GitHubProfileMetadataCollection> CollectAsync(
            GitHubProfileHandle profile,
            CancellationToken cancellationToken = default) =>
            callback(profile, cancellationToken);
    }

    private sealed class CallbackRepositoryCollector(
        Func<GitHubProfileHandle, GitHubProfileMetadataCollection,
            CancellationToken, Task<GitHubProfileRepositoryCollection>> callback)
        : IGitHubProfileRepositoryCollector
    {
        public Task<GitHubProfileRepositoryCollection> CollectAsync(
            GitHubProfileHandle profile,
            GitHubProfileMetadataCollection metadata,
            CancellationToken cancellationToken = default) =>
            callback(profile, metadata, cancellationToken);
    }
}

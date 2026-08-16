using System.Net;
using GitHubProof.Domain.Analysis.Enums;
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
}

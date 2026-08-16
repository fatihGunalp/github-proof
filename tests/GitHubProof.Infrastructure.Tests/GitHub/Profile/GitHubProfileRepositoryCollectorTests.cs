using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;
using static GitHubProof.Infrastructure.Tests.GitHub.Profile.GitHubProfileTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

public sealed class GitHubProfileRepositoryCollectorTests
{
    [Fact]
    public async Task CollectAsync_RequestsDeterministicallyOrderedBoundedPages()
    {
        QueueHttpMessageHandler handler = new(JsonResponse(
            $"[{RepositoryJson("hello-world")}]"));

        await CreateCollector(handler).CollectAsync(ProfileHandle, AvailableProfileMetadata());

        string query = handler.RequestUris[0].Query;

        Assert.Equal("/users/octocat/repos", handler.RequestUris[0].AbsolutePath);
        Assert.Contains("type=owner", query);
        Assert.Contains("sort=full_name", query);
        Assert.Contains("direction=asc", query);
        Assert.Contains("per_page=100", query);
    }

    [Fact]
    public async Task CollectAsync_FollowsPaginationAndOrdersManifestDeterministically()
    {
        HttpResponseMessage firstPage = JsonResponse(
            $"[{RepositoryJson("zeta")}, {RepositoryJson("alpha")}]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/users/octocat/repos?page=2>; rel=\"next\"");
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(
                firstPage,
                JsonResponse($"[{RepositoryJson("alpha")}, {RepositoryJson("beta")}]")));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata(publicRepositoryCount: 3));

        Assert.True(result.IsComplete);
        Assert.Equal(
            ["octocat/alpha", "octocat/beta", "octocat/zeta"],
            result.Repositories.Repositories.Select(
                repository => repository.Reference.FullName));
        Assert.Equal(3, result.Repositories.TotalCount);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Empty(result.CollectionWarnings);
    }

    [Fact]
    public async Task CollectAsync_NormalizesProvenanceLifecycleAndEmptyConditions()
    {
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse($"""
                [
                  {RepositoryJson("forked", fork: true, archived: true)},
                  {RepositoryJson("templated", isTemplate: true)},
                  {RepositoryJson("empty", size: 0, pushedAt: null, language: null)}
                ]
                """)));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata(publicRepositoryCount: 3));

        ProfileRepository forked = Find(result, "forked");
        ProfileRepository templated = Find(result, "templated");
        ProfileRepository empty = Find(result, "empty");

        Assert.Equal(RepositoryConditionState.Present, forked.Fork);
        Assert.Equal(RepositoryConditionState.Present, forked.Archived);
        Assert.Equal(RepositoryConditionState.Absent, forked.Empty);
        Assert.Equal(RepositoryConditionState.Present, templated.TemplateDerived);
        Assert.Equal(RepositoryConditionState.Absent, templated.Archived);
        Assert.Equal(RepositoryConditionState.Present, empty.Empty);
        Assert.Null(empty.LastPushedAt);
        Assert.Null(empty.PrimaryLanguage);
        Assert.Equal(
            DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
            forked.LastPushedAt);
    }

    [Fact]
    public async Task CollectAsync_KeepsMissingProvenanceMetadataUndetermined()
    {
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse($"""
                [{RepositoryJson(
                    "hello-world",
                    fork: null,
                    isTemplate: null,
                    archived: null,
                    size: null)}]
                """)));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata());

        ProfileRepository repository = Find(result, "hello-world");

        Assert.Equal(RepositoryConditionState.Undetermined, repository.Fork);
        Assert.Equal(RepositoryConditionState.Undetermined, repository.TemplateDerived);
        Assert.Equal(RepositoryConditionState.Undetermined, repository.Archived);
        Assert.Equal(RepositoryConditionState.Undetermined, repository.Empty);
    }

    [Fact]
    public async Task CollectAsync_TreatsPushedRepositoryWithZeroSizeAsNotEmpty()
    {
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse($"""
                [{RepositoryJson("tiny", size: 0)}]
                """)));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata());

        Assert.Equal(RepositoryConditionState.Absent, Find(result, "tiny").Empty);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierPagesWhenRateLimitedDuringPagination()
    {
        HttpResponseMessage firstPage = JsonResponse($"[{RepositoryJson("alpha")}]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/users/octocat/repos?page=2>; rel=\"next\"");
        HttpResponseMessage secondPage = JsonResponse("{}", HttpStatusCode.Forbidden);
        secondPage.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(firstPage, secondPage));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata(publicRepositoryCount: 2));

        Assert.False(result.IsComplete);
        Assert.Null(result.Repositories.TotalCount);
        Assert.Equal(["octocat/alpha"], result.Repositories.Repositories.Select(
            repository => repository.Reference.FullName));
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
        Assert.NotEmpty(result.CollectionWarnings);
    }

    [Fact]
    public async Task CollectAsync_StopsAtConfiguredPageBoundAndReportsPartialManifest()
    {
        HttpResponseMessage firstPage = JsonResponse($"[{RepositoryJson("alpha")}]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/users/octocat/repos?page=2>; rel=\"next\"");
        GitHubProfileRepositoryCollector collector = new(
            CreateClient(new QueueHttpMessageHandler(firstPage)),
            CreateFixedTimeProvider(),
            maxPageCount: 1);

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata());

        Assert.False(result.IsComplete);
        Assert.Single(result.Repositories.Repositories);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Contains(
            result.CollectionWarnings,
            warning => warning.Contains("bound of 1 page(s)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestRepositoriesWhenProfileIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();

        GitHubProfileRepositoryCollection result = await CreateCollector(handler)
            .CollectAsync(ProfileHandle, UnavailableProfileMetadata());

        Assert.Empty(handler.RequestUris);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_ExcludesPrivateRepositoriesFromThePublicManifest()
    {
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse($"""
                [
                  {RepositoryJson("public-repo")},
                  {RepositoryJson("private-repo", isPrivate: true)}
                ]
                """)));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata());

        Assert.Equal(["octocat/public-repo"], result.Repositories.Repositories.Select(
            repository => repository.Reference.FullName));
    }

    [Fact]
    public async Task CollectAsync_FailsWhenRepositoryOwnerCannotBeNormalized()
    {
        GitHubProfileRepositoryCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse($"""
                [{RepositoryJson("hello-world", owner: null)}]
                """)));

        GitHubProfileRepositoryCollection result = await collector.CollectAsync(
            ProfileHandle,
            AvailableProfileMetadata());

        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_UsesCollectedIdentityRatherThanTheRequestedHandle()
    {
        QueueHttpMessageHandler handler = new(JsonResponse(
            $"[{RepositoryJson("hello-world")}]"));

        GitHubProfileRepositoryCollection result = await CreateCollector(handler)
            .CollectAsync(
                GitHubProfileHandle.Parse("OctoCat"),
                AvailableProfileMetadata());

        Assert.Equal("/users/octocat/repos", handler.RequestUris[0].AbsolutePath);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Single(result.Repositories.Repositories);
    }

    [Fact]
    public void Constructor_RejectsNonPositivePageBound()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GitHubProfileRepositoryCollector(
            CreateClient(new QueueHttpMessageHandler()),
            CreateFixedTimeProvider(),
            maxPageCount: 0));
    }

    private static ProfileRepository Find(
        GitHubProfileRepositoryCollection result,
        string name) =>
        result.Repositories.Repositories.Single(
            repository => repository.Reference.Name == name);

    private static GitHubProfileRepositoryCollector CreateCollector(
        HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());
}

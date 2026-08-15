using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryTagCollectorTests
{
    [Fact]
    public async Task CollectAsync_PreservesTagsWhenReleasesAreIndependentlyEmpty()
    {
        GitHubRepositoryReleaseCollector releaseCollector = new(
            CreateClient(new QueueHttpMessageHandler(JsonResponse("[]"))),
            CreateFixedTimeProvider());
        QueueHttpMessageHandler tagHandler = new(JsonResponse("""
            [{ "name": "v1.0.0", "commit": { "sha": "abc" } }]
            """));
        GitHubRepositoryTagCollector tagCollector = CreateCollector(tagHandler);

        GitHubRepositoryReleaseCollection releases = await releaseCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());
        GitHubRepositoryTagCollection tags = await tagCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Empty(releases.Releases.Items);
        Assert.Equal(DataAvailabilityStatus.Available, releases.Availability.Status);
        Assert.Equal(["v1.0.0"], tags.Tags.Items.Select(tag => tag.Name));
        Assert.Equal(DataAvailabilityStatus.Available, tags.Availability.Status);
        Assert.Contains("per_page=100", tagHandler.RequestUris[0].Query);
    }

    [Fact]
    public async Task CollectAsync_FollowsNextPageAndDeduplicatesTagNames()
    {
        HttpResponseMessage firstPage = JsonResponse("[{ \"name\": \"v2\" }, { \"name\": \"v1\" }]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/tags?per_page=100&page=2>; rel=\"next\"");
        GitHubRepositoryTagCollector collector = CreateCollector(new QueueHttpMessageHandler(
            firstPage,
            JsonResponse("[{ \"name\": \"v1\" }, { \"name\": \"v0\" }]")));

        GitHubRepositoryTagCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal(["v2", "v1", "v0"], result.Tags.Items.Select(tag => tag.Name));
        Assert.Equal(3, result.Tags.TotalCount);
    }

    [Fact]
    public async Task CollectAsync_RepresentsAnEmptySuccessfulTagListAsAvailableAndComplete()
    {
        GitHubRepositoryTagCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[]")));

        GitHubRepositoryTagCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.Tags.Items);
        Assert.Equal(0, result.Tags.TotalCount);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierPagesWhenForbiddenDuringPagination()
    {
        HttpResponseMessage firstPage = JsonResponse("[{ \"name\": \"v1\" }]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/tags?per_page=100&page=2>; rel=\"next\"");
        GitHubRepositoryTagCollector collector = CreateCollector(new QueueHttpMessageHandler(
            firstPage,
            new HttpResponseMessage(HttpStatusCode.Forbidden)));

        GitHubRepositoryTagCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal(["v1"], result.Tags.Items.Select(tag => tag.Name));
        Assert.Null(result.Tags.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Forbidden, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedAndNotFoundResponsesWithoutClaimingCompleteness()
    {
        GitHubRepositoryTagCollector malformedCollector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[{ \"name\": \" \" }]")));
        GitHubRepositoryTagCollection malformed = await malformedCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        GitHubRepositoryTagCollector notFoundCollector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound)));
        GitHubRepositoryTagCollection notFound = await notFoundCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.False(malformed.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, malformed.Availability.Status);
        Assert.False(notFound.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Unavailable, notFound.Availability.Status);
    }

    private static GitHubRepositoryTagCollector CreateCollector(HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());
}

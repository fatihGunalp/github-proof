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

    // Session 128. Git tag refs are case-sensitive, so a repository may carry both
    // `V1.0` and `v1.0` — but this collector deduplicates with
    // StringComparer.OrdinalIgnoreCase, so it reports ONE tag for the pair.
    //
    // Pinned here because the Session 128 parity proof turns on it: the deduped
    // count is what reaches `tagsAtLeast`, so which side of a page boundary the
    // second variant falls on changes a §15.5 evidence value. That makes the
    // observed tag count order-dependent, which is why tags is ORDER-BLOCKED
    // rather than ORDER-PASS.
    //
    // This test states current behaviour. It does not assert that the behaviour
    // is correct — whether the comparer should be Ordinal is an open question for
    // the architecture owner, because changing it changes existing REST results.
    [Fact]
    public async Task CollectAsync_TreatsCaseVariantTagNamesAsOneTag()
    {
        GitHubRepositoryTagCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("""
                [{ "name": "V1.0" }, { "name": "v1.0" }]
                """)));

        GitHubRepositoryTagCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(["V1.0"], result.Tags.Items.Select(tag => tag.Name));
    }

    // The other side of the same boundary: with the second variant on page two,
    // the collector observes the SAME two upstream tags and still reports one —
    // so the divergence is not about dedup being wrong, it is about WHICH tags
    // reach the prefix before the decision boundary is asked.
    [Fact]
    public async Task CollectAsync_DeduplicatesACaseVariantAcrossAPageBoundary()
    {
        HttpResponseMessage firstPage = JsonResponse("""
            [{ "name": "V1.0" }, { "name": "v2.0" }]
            """);
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/tags?per_page=100&page=2>; rel=\"next\"");
        GitHubRepositoryTagCollector collector = CreateCollector(new QueueHttpMessageHandler(
            firstPage,
            JsonResponse("""
                [{ "name": "v1.0" }]
                """)));

        GitHubRepositoryTagCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(["V1.0", "v2.0"], result.Tags.Items.Select(tag => tag.Name));
    }

    private static GitHubRepositoryTagCollector CreateCollector(HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());
}

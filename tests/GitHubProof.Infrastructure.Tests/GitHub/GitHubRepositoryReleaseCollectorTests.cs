using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryReleaseCollectorTests
{
    [Fact]
    public async Task CollectAsync_PreservesPublishedAndDraftReleaseObservations()
    {
        QueueHttpMessageHandler handler = new(JsonResponse("""
            [
              { "tag_name": "v1.0.0", "draft": false, "name": "Version 1", "body": "Release notes", "published_at": "2026-08-10T12:30:00+03:00" },
              { "tag_name": "v2.0.0", "draft": true, "name": null, "body": "", "published_at": null }
            ]
            """));
        GitHubRepositoryReleaseCollector collector = CreateCollector(handler);

        GitHubRepositoryReleaseCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal(["v1.0.0", "v2.0.0"], result.Releases.Items.Select(release => release.TagName));
        Assert.True(result.Releases.Items[0].HasPublishedRelease);
        Assert.False(result.Releases.Items[0].IsDraft);
        Assert.Equal("Release notes", result.Releases.Items[0].Notes);
        Assert.Equal(DateTimeOffset.Parse("2026-08-10T09:30:00Z"), result.Releases.Items[0].PublishedAt);
        Assert.False(result.Releases.Items[1].HasPublishedRelease);
        Assert.True(result.Releases.Items[1].IsDraft);
        Assert.Null(result.Releases.Items[1].Notes);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Contains("per_page=100", handler.RequestUris[0].Query);
    }

    [Fact]
    public async Task CollectAsync_FollowsNextPageAndDeduplicatesByTagName()
    {
        HttpResponseMessage firstPage = JsonResponse("""
            [{ "tag_name": "v2", "draft": false, "name": null, "body": null, "published_at": "2026-08-12T00:00:00Z" }]
            """);
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/releases?per_page=100&page=2>; rel=\"next\"");
        GitHubRepositoryReleaseCollector collector = CreateCollector(new QueueHttpMessageHandler(
            firstPage,
            JsonResponse("""
                [
                  { "tag_name": "v2", "draft": false, "name": null, "body": null, "published_at": "2026-08-12T00:00:00Z" },
                  { "tag_name": "v1", "draft": false, "name": null, "body": null, "published_at": "2026-08-11T00:00:00Z" }
                ]
                """)));

        GitHubRepositoryReleaseCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal(["v2", "v1"], result.Releases.Items.Select(release => release.TagName));
        Assert.Equal(2, result.Releases.TotalCount);
    }

    [Fact]
    public async Task CollectAsync_RepresentsAnEmptySuccessfulReleaseListAsAvailableAndComplete()
    {
        GitHubRepositoryReleaseCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[]")));

        GitHubRepositoryReleaseCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.Releases.Items);
        Assert.Equal(0, result.Releases.TotalCount);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierPagesWhenRateLimitedDuringPagination()
    {
        HttpResponseMessage firstPage = JsonResponse("""
            [{ "tag_name": "v1", "draft": false, "name": null, "body": null, "published_at": "2026-08-10T00:00:00Z" }]
            """);
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/releases?per_page=100&page=2>; rel=\"next\"");
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubRepositoryReleaseCollector collector = CreateCollector(
            new QueueHttpMessageHandler(firstPage, rateLimited));

        GitHubRepositoryReleaseCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal(["v1"], result.Releases.Items.Select(release => release.TagName));
        Assert.Null(result.Releases.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsRateLimitBeforeUsableDataAndMalformedResponsesToFailedCollection()
    {
        GitHubRepositoryReleaseCollector rateLimitedCollector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests)));
        GitHubRepositoryReleaseCollection rateLimited = await rateLimitedCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        GitHubRepositoryReleaseCollector malformedCollector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[{ \"tag_name\": \"v1\", \"draft\": null }]")));
        GitHubRepositoryReleaseCollection malformed = await malformedCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.False(rateLimited.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, rateLimited.Availability.Status);
        Assert.False(malformed.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, malformed.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsTransportFailureAndSkipsUnavailableMetadata()
    {
        GitHubRepositoryReleaseCollector transportCollector = CreateCollector(
            new ThrowingHttpMessageHandler(new HttpRequestException("Network unavailable")));
        GitHubRepositoryReleaseCollection transportFailure = await transportCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        QueueHttpMessageHandler handler = new();
        GitHubRepositoryReleaseCollector unavailableMetadataCollector = CreateCollector(handler);
        GitHubRepositoryReleaseCollection notRequested = await unavailableMetadataCollector.CollectAsync(
            RepositoryUrl,
            UnavailableMetadata());

        Assert.Equal(DataAvailabilityStatus.Failed, transportFailure.Availability.Status);
        Assert.Empty(handler.RequestUris);
        Assert.Equal(DataAvailabilityStatus.NotRequested, notRequested.Availability.Status);
    }

    private static GitHubRepositoryReleaseCollector CreateCollector(HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());
}

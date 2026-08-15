using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryContributorCollectorTests
{
    [Fact]
    public async Task CollectAsync_NormalizesLoginAndContributionsWithoutRequestingAnonymousContributors()
    {
        QueueHttpMessageHandler handler = new(JsonResponse("""
            [
              { "login": "octocat", "contributions": 12 },
              { "login": "hubot", "contributions": 0 }
            ]
            """));
        GitHubRepositoryContributorCollector collector = CreateCollector(handler);

        GitHubRepositoryContributorCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal(["octocat", "hubot"], result.Contributors.Items.Select(contributor => contributor.Login));
        Assert.Equal([12, 0], result.Contributors.Items.Select(contributor => contributor.Contributions));
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Contains("per_page=100", handler.RequestUris[0].Query);
        Assert.DoesNotContain("anon", handler.RequestUris[0].Query);
    }

    [Fact]
    public async Task CollectAsync_FollowsNextPageAndDeduplicatesContributorLogins()
    {
        HttpResponseMessage firstPage = JsonResponse("[{ \"login\": \"octocat\", \"contributions\": 12 }]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/contributors?per_page=100&page=2>; rel=\"next\"");
        GitHubRepositoryContributorCollector collector = CreateCollector(new QueueHttpMessageHandler(
            firstPage,
            JsonResponse("[{ \"login\": \"OCTOCAT\", \"contributions\": 12 }, { \"login\": \"hubot\", \"contributions\": 4 }]")));

        GitHubRepositoryContributorCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal(["octocat", "hubot"], result.Contributors.Items.Select(contributor => contributor.Login));
        Assert.Equal(2, result.Contributors.TotalCount);
    }

    [Fact]
    public async Task CollectAsync_RepresentsEmptyRepositoryNoContentAsAvailableEmptyCollection()
    {
        GitHubRepositoryContributorCollector collector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent)));

        GitHubRepositoryContributorCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.Contributors.Items);
        Assert.Equal(0, result.Contributors.TotalCount);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierPagesWhenRateLimitedDuringPagination()
    {
        HttpResponseMessage firstPage = JsonResponse("[{ \"login\": \"octocat\", \"contributions\": 12 }]");
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/contributors?per_page=100&page=2>; rel=\"next\"");
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubRepositoryContributorCollector collector = CreateCollector(
            new QueueHttpMessageHandler(firstPage, rateLimited));

        GitHubRepositoryContributorCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal(["octocat"], result.Contributors.Items.Select(contributor => contributor.Login));
        Assert.Null(result.Contributors.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedTransportAndUnavailableMetadataToTheirOwnStates()
    {
        GitHubRepositoryContributorCollector malformedCollector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[{ \"login\": null, \"contributions\": 1 }]")));
        GitHubRepositoryContributorCollection malformed = await malformedCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        GitHubRepositoryContributorCollector transportCollector = CreateCollector(
            new ThrowingHttpMessageHandler(new HttpRequestException("Network unavailable")));
        GitHubRepositoryContributorCollection transport = await transportCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        QueueHttpMessageHandler handler = new();
        GitHubRepositoryContributorCollector unavailableMetadataCollector = CreateCollector(handler);
        GitHubRepositoryContributorCollection notRequested = await unavailableMetadataCollector.CollectAsync(
            RepositoryUrl,
            UnavailableMetadata());

        Assert.Equal(DataAvailabilityStatus.Failed, malformed.Availability.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, transport.Availability.Status);
        Assert.Empty(handler.RequestUris);
        Assert.Equal(DataAvailabilityStatus.NotRequested, notRequested.Availability.Status);
    }

    private static GitHubRepositoryContributorCollector CreateCollector(HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());
}

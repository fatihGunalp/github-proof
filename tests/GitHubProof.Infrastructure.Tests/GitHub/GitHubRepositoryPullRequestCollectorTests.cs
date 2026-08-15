using System.Net;
using System.Text;
using System.Text.Json;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryPullRequestCollectorTests
{
    [Fact]
    public async Task CollectAsync_NormalizesOpenClosedAndMergedPullRequests()
    {
        QueueHttpMessageHandler handler = new(JsonResponse("""
            [
              { "number": 4, "title": " First pull request ", "state": "open", "created_at": "2026-08-10T12:30:00+03:00", "merged_at": null },
              { "number": 5, "title": "Second pull request", "state": "closed", "created_at": "2026-08-11T00:00:00Z", "merged_at": null },
              { "number": 6, "title": "Third pull request", "state": "closed", "created_at": "2026-08-12T00:00:00Z", "merged_at": "2026-08-13T00:00:00Z" }
            ]
            """));
        GitHubRepositoryPullRequestCollector collector = CreateCollector(handler);

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal([4, 5, 6], result.PullRequests.Items.Select(pullRequest => pullRequest.Number));
        Assert.False(result.PullRequests.Items[0].IsClosed);
        Assert.Equal("First pull request", result.PullRequests.Items[0].Title);
        Assert.False(result.PullRequests.Items[1].IsMerged);
        Assert.True(result.PullRequests.Items[2].IsClosed);
        Assert.True(result.PullRequests.Items[2].IsMerged);
        Assert.Equal(DateTimeOffset.Parse("2026-08-10T09:30:00Z"), result.PullRequests.Items[0].CreatedAt);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Contains("state=all", handler.RequestUris[0].Query);
        Assert.Contains("per_page=100", handler.RequestUris[0].Query);
    }

    [Fact]
    public async Task CollectAsync_FollowsNextPageAndDeduplicatesPullRequestNumbers()
    {
        HttpResponseMessage firstPage = JsonResponse(PullRequestsJson(
            (3, "closed", "2026-08-11T00:00:00Z", "2026-08-12T00:00:00Z"),
            (2, "open", "2026-08-10T00:00:00Z", null)));
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/pulls?state=all&per_page=100&page=2>; rel=\"next\"");
        QueueHttpMessageHandler handler = new(
            firstPage,
            JsonResponse(PullRequestsJson(
                (2, "open", "2026-08-10T00:00:00Z", null),
                (1, "closed", "2026-08-09T00:00:00Z", null))));
        GitHubRepositoryPullRequestCollector collector = CreateCollector(handler);

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal([3, 2, 1], result.PullRequests.Items.Select(pullRequest => pullRequest.Number));
        Assert.Equal(3, result.PullRequests.TotalCount);
        Assert.Equal(2, handler.RequestUris.Count);
    }

    [Fact]
    public async Task CollectAsync_RepresentsAnEmptySuccessfulPullRequestListAsAvailableAndComplete()
    {
        GitHubRepositoryPullRequestCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[]")));

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.PullRequests.Items);
        Assert.Equal(0, result.PullRequests.TotalCount);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsForbiddenBeforeUsableDataWithoutClaimingCompleteness()
    {
        GitHubRepositoryPullRequestCollector collector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)));

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.PullRequests.Items);
        Assert.Null(result.PullRequests.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Forbidden, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsNotFoundToUnavailableIncompleteCollection()
    {
        GitHubRepositoryPullRequestCollector collector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound)));

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Unavailable, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedResponseToFailedIncompleteCollection()
    {
        GitHubRepositoryPullRequestCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("""
                [{ "number": 1, "title": "Pull request", "state": "closed", "created_at": null, "merged_at": null }]
                """)));

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsTransportFailureToFailedIncompleteCollection()
    {
        GitHubRepositoryPullRequestCollector collector = CreateCollector(
            new ThrowingHttpMessageHandler(new HttpRequestException("Network unavailable")));

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestPullRequestsWhenMetadataIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();
        GitHubRepositoryPullRequestCollector collector = CreateCollector(handler);

        GitHubRepositoryPullRequestCollection result = await collector.CollectAsync(
            RepositoryUrl,
            UnavailableMetadata());

        Assert.Empty(handler.RequestUris);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.Availability.Status);
    }

    private static GitHubRepositoryUrl RepositoryUrl { get; } =
        GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world");

    private static GitHubRepositoryMetadataCollection AvailableMetadata() =>
        new(
            new RepositoryIdentity(
                "octocat", "hello-world", new Uri("https://github.com/octocat/hello-world"),
                "main", isPublic: true, isArchived: false, isFork: false, isTemplate: false,
                primaryLanguage: "C#"),
            new RepositoryMetadata(null, null, DateTimeOffset.Parse("2026-08-01T00:00:00Z"), issuesEnabled: true),
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Available("metadata", DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    private static GitHubRepositoryMetadataCollection UnavailableMetadata() =>
        new(null, null, Array.Empty<string>(), null, DataAvailability.Unavailable(
            "metadata", DataAvailabilityStatus.RateLimited, "GitHub rate limit reached."));

    private static GitHubRepositoryPullRequestCollector CreateCollector(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") },
            new FixedTimeProvider(DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    private static HttpResponseMessage JsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static string PullRequestsJson(params (int Number, string State, string CreatedAt, string? MergedAt)[] pullRequests) =>
        JsonSerializer.Serialize(pullRequests.Select(pullRequest => new
        {
            number = pullRequest.Number,
            title = $"Pull request {pullRequest.Number}",
            state = pullRequest.State,
            created_at = pullRequest.CreatedAt,
            merged_at = pullRequest.MergedAt
        }));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class QueueHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);
            return Task.FromResult(responses.Dequeue());
        }
    }

    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(exception);
    }
}

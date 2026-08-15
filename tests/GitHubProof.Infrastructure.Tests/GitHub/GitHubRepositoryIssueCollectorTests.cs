using System.Net;
using System.Text;
using System.Text.Json;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryIssueCollectorTests
{
    [Fact]
    public async Task CollectAsync_ExcludesPullRequestsAndNormalizesIssueData()
    {
        QueueHttpMessageHandler handler = new(JsonResponse("""
            [
              { "number": 7, "title": " First issue ", "body": " Details ", "state": "open", "created_at": "2026-08-10T12:30:00+03:00" },
              { "number": 8, "title": "Pull request", "state": "closed", "created_at": "2026-08-11T00:00:00Z", "pull_request": { "url": "https://api.github.com/repos/octocat/hello-world/pulls/8" } },
              { "number": 9, "title": "Second issue", "body": null, "state": "closed", "created_at": "2026-08-12T00:00:00Z" }
            ]
            """));
        GitHubRepositoryIssueCollector collector = CreateCollector(handler);

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal([7, 9], result.Issues.Items.Select(issue => issue.Number));
        Assert.False(result.Issues.Items[0].IsClosed);
        Assert.Equal("First issue", result.Issues.Items[0].Title);
        Assert.Equal("Details", result.Issues.Items[0].Body);
        Assert.True(result.Issues.Items[1].IsClosed);
        Assert.Equal(DateTimeOffset.Parse("2026-08-10T09:30:00Z"), result.Issues.Items[0].CreatedAt);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Contains("state=all", handler.RequestUris[0].Query);
        Assert.Contains("per_page=100", handler.RequestUris[0].Query);
    }

    [Fact]
    public async Task CollectAsync_FollowsNextPageAndDeduplicatesIssueNumbers()
    {
        HttpResponseMessage firstPage = JsonResponse(IssuesJson(
            (2, "open", "2026-08-11T00:00:00Z"),
            (1, "closed", "2026-08-10T00:00:00Z")));
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/issues?state=all&per_page=100&page=2>; rel=\"next\"");
        QueueHttpMessageHandler handler = new(
            firstPage,
            JsonResponse(IssuesJson(
                (1, "closed", "2026-08-10T00:00:00Z"),
                (3, "open", "2026-08-09T00:00:00Z"))));
        GitHubRepositoryIssueCollector collector = CreateCollector(handler);

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal([2, 1, 3], result.Issues.Items.Select(issue => issue.Number));
        Assert.Equal(3, result.Issues.TotalCount);
        Assert.Equal(2, handler.RequestUris.Count);
    }

    [Fact]
    public async Task CollectAsync_RepresentsAnEmptySuccessfulIssueListAsAvailableAndComplete()
    {
        GitHubRepositoryIssueCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[]")));

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.Issues.Items);
        Assert.Equal(0, result.Issues.TotalCount);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_RepresentsDisabledIssuesAsNotApplicableWithoutARequest()
    {
        QueueHttpMessageHandler handler = new();
        GitHubRepositoryIssueCollector collector = CreateCollector(handler);

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata(issuesEnabled: false));

        Assert.Empty(handler.RequestUris);
        Assert.Empty(result.Issues.Items);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.NotApplicable, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierPagesWhenRateLimitedDuringPagination()
    {
        HttpResponseMessage firstPage = JsonResponse(IssuesJson(
            (1, "open", "2026-08-10T00:00:00Z")));
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/issues?state=all&per_page=100&page=2>; rel=\"next\"");
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubRepositoryIssueCollector collector = CreateCollector(
            new QueueHttpMessageHandler(firstPage, rateLimited));

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal([1], result.Issues.Items.Select(issue => issue.Number));
        Assert.Null(result.Issues.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
        Assert.Single(result.CollectionWarnings);
    }

    [Fact]
    public async Task CollectAsync_MapsRateLimitBeforeUsableDataWithoutClaimingCompleteness()
    {
        GitHubRepositoryIssueCollector collector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(result.Issues.Items);
        Assert.Null(result.Issues.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedResponseToFailedIncompleteCollection()
    {
        GitHubRepositoryIssueCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("""
                [{ "number": 1, "title": "Issue", "state": "unexpected", "created_at": "2026-08-10T00:00:00Z" }]
                """)));

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestIssuesWhenMetadataIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();
        GitHubRepositoryIssueCollector collector = CreateCollector(handler);

        GitHubRepositoryIssueCollection result = await collector.CollectAsync(
            RepositoryUrl,
            UnavailableMetadata());

        Assert.Empty(handler.RequestUris);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.Availability.Status);
    }

    private static GitHubRepositoryUrl RepositoryUrl { get; } =
        GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world");

    private static GitHubRepositoryMetadataCollection AvailableMetadata(bool issuesEnabled = true) =>
        new(
            new RepositoryIdentity(
                "octocat", "hello-world", new Uri("https://github.com/octocat/hello-world"),
                "main", isPublic: true, isArchived: false, isFork: false, isTemplate: false,
                primaryLanguage: "C#"),
            new RepositoryMetadata(null, null, DateTimeOffset.Parse("2026-08-01T00:00:00Z"), issuesEnabled),
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Available("metadata", DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    private static GitHubRepositoryMetadataCollection UnavailableMetadata() =>
        new(null, null, Array.Empty<string>(), null, DataAvailability.Unavailable(
            "metadata", DataAvailabilityStatus.RateLimited, "GitHub rate limit reached."));

    private static GitHubRepositoryIssueCollector CreateCollector(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") },
            new FixedTimeProvider(DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    private static HttpResponseMessage JsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static string IssuesJson(params (int Number, string State, string CreatedAt)[] issues) =>
        JsonSerializer.Serialize(issues.Select(issue => new
        {
            number = issue.Number,
            title = $"Issue {issue.Number}",
            body = (string?)null,
            state = issue.State,
            created_at = issue.CreatedAt
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
}

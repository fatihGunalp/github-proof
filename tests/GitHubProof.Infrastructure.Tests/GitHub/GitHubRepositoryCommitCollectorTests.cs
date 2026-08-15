using System.Net;
using System.Text;
using System.Text.Json;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryCommitCollectorTests
{
    [Fact]
    public async Task CollectAsync_UsesMetadataDefaultBranchAndNormalizesCommitterTimestamp()
    {
        QueueHttpMessageHandler handler = new(JsonResponse(CommitsJson(
            ("sha-one", "2026-08-10T12:30:00+03:00"))));
        GitHubRepositoryCommitCollector collector = CreateCollector(handler);

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata(defaultBranch: "release/2026"));

        Assert.True(result.IsComplete);
        Assert.Equal(1, result.Commits.TotalCount);
        Assert.Equal("sha-one", result.Commits.Items[0].Sha);
        Assert.Equal(
            DateTimeOffset.Parse("2026-08-10T09:30:00Z"),
            result.Commits.Items[0].CommittedAt);
        Assert.Null(result.Commits.Items[0].Message);
        Assert.Null(result.Commits.Items[0].AuthorLogin);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Contains("sha=release%2F2026", handler.RequestUris[0].Query);
        Assert.Contains("per_page=100", handler.RequestUris[0].Query);
    }

    [Fact]
    public async Task CollectAsync_FollowsNextPageAndDeduplicatesCommitShas()
    {
        HttpResponseMessage firstPage = JsonResponse(CommitsJson(
            ("sha-two", "2026-08-11T00:00:00Z"),
            ("sha-one", "2026-08-10T00:00:00Z")));
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/commits?sha=main&per_page=100&page=2>; rel=\"next\"");
        QueueHttpMessageHandler handler = new(
            firstPage,
            JsonResponse(CommitsJson(
                ("sha-one", "2026-08-10T00:00:00Z"),
                ("sha-zero", "2026-08-09T00:00:00Z"))));
        GitHubRepositoryCommitCollector collector = CreateCollector(handler);

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.True(result.IsComplete);
        Assert.Equal(3, result.Commits.TotalCount);
        Assert.Equal(["sha-two", "sha-one", "sha-zero"], result.Commits.Items.Select(item => item.Sha));
        Assert.Equal(2, handler.RequestUris.Count);
        Assert.Equal("?sha=main&per_page=100&page=2", handler.RequestUris[1].Query);
    }

    [Fact]
    public async Task CollectAsync_RepresentsAnEmptySuccessfulHistoryAsAvailableAndComplete()
    {
        GitHubRepositoryCommitCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("[]")));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Empty(result.Commits.Items);
        Assert.Equal(0, result.Commits.TotalCount);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_RepresentsGitHubEmptyRepositoryConflictAsAvailableAndComplete()
    {
        GitHubRepositoryCommitCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse(
                "{ \"message\": \"Git Repository is empty.\" }",
                HttpStatusCode.Conflict)));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Empty(result.Commits.Items);
        Assert.True(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierPagesWhenRateLimitedDuringPagination()
    {
        HttpResponseMessage firstPage = JsonResponse(CommitsJson(
            ("sha-one", "2026-08-10T00:00:00Z")));
        firstPage.Headers.Add(
            "Link",
            "<https://api.github.com/repos/octocat/hello-world/commits?sha=main&per_page=100&page=2>; rel=\"next\"");
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubRepositoryCommitCollector collector = CreateCollector(
            new QueueHttpMessageHandler(firstPage, rateLimited));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(["sha-one"], result.Commits.Items.Select(item => item.Sha));
        Assert.Null(result.Commits.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
        Assert.Single(result.CollectionWarnings);
    }

    [Fact]
    public async Task CollectAsync_MapsFailureBeforeUsableDataWithoutClaimingCompleteness()
    {
        GitHubRepositoryCommitCollector collector = CreateCollector(
            new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Empty(result.Commits.Items);
        Assert.Null(result.Commits.TotalCount);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Forbidden, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedResponseToFailedIncompleteCollection()
    {
        GitHubRepositoryCommitCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse("""
                [
                  {
                    "sha": "sha-one",
                    "commit": { "committer": {} }
                  }
                ]
                """)));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsTransportFailureToFailedIncompleteCollection()
    {
        GitHubRepositoryCommitCollector collector = CreateCollector(
            new ThrowingHttpMessageHandler(new HttpRequestException("Network unavailable")));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestCommitsWhenMetadataIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();
        GitHubRepositoryCommitCollector collector = CreateCollector(handler);
        GitHubRepositoryMetadataCollection unavailableMetadata = new(
            Repository: null,
            Metadata: null,
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Unavailable(
                "metadata",
                DataAvailabilityStatus.RateLimited,
                "GitHub rate limit reached."));

        GitHubRepositoryCommitCollection result = await collector.CollectAsync(
            RepositoryUrl,
            unavailableMetadata);

        Assert.Empty(handler.RequestUris);
        Assert.False(result.IsComplete);
        Assert.All(
            [result.Availability],
            availability => Assert.Equal(DataAvailabilityStatus.NotRequested, availability.Status));
    }

    private static GitHubRepositoryUrl RepositoryUrl { get; } =
        GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world");

    private static GitHubRepositoryMetadataCollection AvailableMetadata(
        string defaultBranch = "main") =>
        new(
            new RepositoryIdentity(
                "octocat",
                "hello-world",
                new Uri("https://github.com/octocat/hello-world"),
                defaultBranch,
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            new RepositoryMetadata(
                description: null,
                homepage: null,
                DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
                issuesEnabled: true),
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Available(
                "metadata",
                DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    private static GitHubRepositoryCommitCollector CreateCollector(
        HttpMessageHandler handler)
    {
        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        return new GitHubRepositoryCommitCollector(
            client,
            new FixedTimeProvider(DateTimeOffset.Parse("2026-08-15T09:30:00Z")));
    }

    private static HttpResponseMessage JsonResponse(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static string CommitsJson(params (string Sha, string CommittedAt)[] commits) =>
        JsonSerializer.Serialize(commits.Select(commit => new
        {
            sha = commit.Sha,
            commit = new
            {
                committer = new
                {
                    date = commit.CommittedAt
                }
            }
        }));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class QueueHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);
            return Task.FromResult(responses.Dequeue());
        }
    }

    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(exception);
    }
}

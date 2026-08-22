using System.Net;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;
using static GitHubProof.Infrastructure.Tests.GitHub.Profile.GitHubProfileTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

public sealed class GitHubProfileMetadataCollectorTests
{
    [Fact]
    public async Task CollectAsync_NormalizesPublicUserProfile()
    {
        QueueHttpMessageHandler handler = new(JsonResponse("""
            {
              "login": "octocat",
              "type": "User",
              "name": "The Octocat",
              "created_at": "2011-01-25T18:44:36Z",
              "public_repos": 8
            }
            """));

        GitHubProfileMetadataCollection result = await CreateCollector(handler)
            .CollectAsync(ProfileHandle);

        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Equal("octocat", result.Identity!.Login);
        Assert.Equal(new Uri("https://github.com/octocat"), result.Identity.Url);
        Assert.Equal("The Octocat", result.Metadata!.DisplayName);
        Assert.Equal(
            DateTimeOffset.Parse("2011-01-25T18:44:36Z"),
            result.Metadata.CreatedAt);
        Assert.Equal(8, result.Metadata.PublicRepositoryCount);
        Assert.Equal("/users/octocat", handler.RequestUris[0].AbsolutePath);
    }

    [Fact]
    public async Task CollectAsync_TreatsOrganizationAccountsAsNotApplicable()
    {
        GitHubProfileMetadataCollection result = await CreateCollector(
                new QueueHttpMessageHandler(JsonResponse("""
                    {
                      "login": "github",
                      "type": "Organization",
                      "created_at": "2011-01-25T18:44:36Z",
                      "public_repos": 8
                    }
                    """)))
            .CollectAsync(ProfileHandle);

        Assert.Equal(DataAvailabilityStatus.NotApplicable, result.Availability.Status);
        Assert.Null(result.Identity);
        Assert.Null(result.Metadata);
    }

    [Fact]
    public async Task CollectAsync_ReportsUnavailableProfileForUnknownUser()
    {
        GitHubProfileMetadataCollection result = await CreateCollector(
                new QueueHttpMessageHandler(
                    JsonResponse("{}", HttpStatusCode.NotFound)))
            .CollectAsync(ProfileHandle);

        Assert.Equal(DataAvailabilityStatus.Unavailable, result.Availability.Status);
        Assert.False(result.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_ReportsRateLimitWithoutInventingProfileState()
    {
        HttpResponseMessage response = JsonResponse("{}", HttpStatusCode.Forbidden);
        response.Headers.Add("X-RateLimit-Remaining", "0");

        GitHubProfileMetadataCollection result = await CreateCollector(
                new QueueHttpMessageHandler(response))
            .CollectAsync(ProfileHandle);

        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
        Assert.Null(result.Identity);
    }

    [Fact]
    public async Task CollectAsync_FailsWhenRequiredProfileFieldIsMissing()
    {
        GitHubProfileMetadataCollection result = await CreateCollector(
                new QueueHttpMessageHandler(JsonResponse("""
                    { "login": "octocat", "type": "User", "public_repos": 8 }
                    """)))
            .CollectAsync(ProfileHandle);

        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
        Assert.Null(result.Metadata);
    }

    [Fact]
    public async Task CollectAsync_FailsWhenTransportFails()
    {
        GitHubProfileMetadataCollector collector = new(
            CreateClient(new ThrowingHttpMessageHandler(
                new HttpRequestException("connection reset"))),
            CreateFixedTimeProvider());

        GitHubProfileMetadataCollection result = await collector.CollectAsync(ProfileHandle);

        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_ResponseHeadersRead_ContentReadReceivesExecutionToken()
    {
        CancellationObservingStream contentStream = new();
        using CancellationTokenSource execution = new();
        Task<GitHubProfileMetadataCollection> collection =
            CreateCollector(new QueueHttpMessageHandler(
                    BlockingJsonResponse(contentStream)))
                .CollectAsync(ProfileHandle, execution.Token);

        CancellationToken observed = await contentStream.TokenObserved.WaitAsync(
            TimeSpan.FromSeconds(5));
        await execution.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => collection);
        Assert.Equal(execution.Token, observed);
    }

    private static GitHubProfileMetadataCollector CreateCollector(
        HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());
}

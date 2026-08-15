using System.Net;
using System.Net.Http.Headers;
using System.Text;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryMetadataCollectorTests
{
    [Fact]
    public async Task CollectAsync_NormalizesAProviderResponseIntoDomainValues()
    {
        StubHttpMessageHandler handler = new(_ => JsonResponse("""
            {
              "name": " Hello-World ",
              "owner": { "login": " OctoCat " },
              "private": false,
              "archived": true,
              "fork": false,
              "is_template": true,
              "language": " C# ",
              "default_branch": " main ",
              "description": " A sample repository. ",
              "homepage": " https://example.test/demo ",
              "created_at": "2026-08-01T12:00:00+03:00",
              "has_issues": true,
              "topics": [" dotnet ", "DOTNET", " github-api "],
              "license": {
                "key": "mit",
                "name": "MIT License",
                "spdx_id": "MIT"
              }
            }
            """));
        GitHubRepositoryMetadataCollector collector = CreateCollector(handler);

        GitHubRepositoryMetadataCollection result = await collector.CollectAsync(
            GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world"));

        Assert.True(result.IsAvailable);
        Assert.Equal("OctoCat", result.Repository!.Owner);
        Assert.Equal("Hello-World", result.Repository.Name);
        Assert.Equal("main", result.Repository.DefaultBranch);
        Assert.Equal(new Uri("https://github.com/OctoCat/Hello-World"), result.Repository.Url);
        Assert.True(result.Repository.IsArchived);
        Assert.True(result.Repository.IsTemplate);
        Assert.Equal("C#", result.Repository.PrimaryLanguage);
        Assert.Equal("A sample repository.", result.Metadata!.Description);
        Assert.Equal("https://example.test/demo", result.Metadata.Homepage);
        Assert.Equal(DateTimeOffset.Parse("2026-08-01T09:00:00Z"), result.Metadata.CreatedAt);
        Assert.Equal(["dotnet", "github-api"], result.Topics);
        Assert.Equal("MIT", result.License!.SpdxIdentifier);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            result.Availability.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), result.Availability.RetrievedAt);
        Assert.Equal("/repos/octocat/hello-world", handler.RequestUri!.AbsolutePath);
        Assert.Contains(
            handler.AcceptHeaders,
            header => header.MediaType == "application/vnd.github+json");
        Assert.Equal("2026-03-10", handler.ApiVersion);
        Assert.Equal("GitHubProof/0.1", handler.UserAgent);
    }

    [Fact]
    public async Task CollectAsync_MapsNotFoundToUnavailableDataInsteadOfMissingEvidence()
    {
        GitHubRepositoryMetadataCollector collector = CreateCollector(
            new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        GitHubRepositoryMetadataCollection result = await collector.CollectAsync(
            GitHubRepositoryUrl.Parse("https://github.com/octocat/missing"));

        Assert.False(result.IsAvailable);
        Assert.Null(result.Repository);
        Assert.Null(result.Metadata);
        Assert.Equal(DataAvailabilityStatus.Unavailable, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsRateLimitResponseToRateLimitedAvailability()
    {
        GitHubRepositoryMetadataCollector collector = CreateCollector(
            new StubHttpMessageHandler(_ =>
            {
                HttpResponseMessage response = new(HttpStatusCode.Forbidden);
                response.Headers.Add("X-RateLimit-Remaining", "0");
                return response;
            }));

        GitHubRepositoryMetadataCollection result = await collector.CollectAsync(
            GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world"));

        Assert.False(result.IsAvailable);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsPrivateRepositoryToOutOfScopeData()
    {
        GitHubRepositoryMetadataCollector collector = CreateCollector(
            new StubHttpMessageHandler(_ => JsonResponse("""
                {
                  "name": "private-repository",
                  "owner": { "login": "octocat" },
                  "private": true
                }
                """)));

        GitHubRepositoryMetadataCollection result = await collector.CollectAsync(
            GitHubRepositoryUrl.Parse("https://github.com/octocat/private-repository"));

        Assert.False(result.IsAvailable);
        Assert.Equal(DataAvailabilityStatus.NotApplicable, result.Availability.Status);
        Assert.Contains("outside the MVP scope", result.Availability.Reason);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedProviderDataToFailedAvailability()
    {
        GitHubRepositoryMetadataCollector collector = CreateCollector(
            new StubHttpMessageHandler(_ => JsonResponse("""
                {
                  "owner": { "login": "octocat" },
                  "private": false
                }
                """)));

        GitHubRepositoryMetadataCollection result = await collector.CollectAsync(
            GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world"));

        Assert.False(result.IsAvailable);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMissingCreationTimestampToFailedAvailability()
    {
        GitHubRepositoryMetadataCollector collector = CreateCollector(
            new StubHttpMessageHandler(_ => JsonResponse("""
                {
                  "name": "hello-world",
                  "owner": { "login": "octocat" },
                  "private": false,
                  "default_branch": "main"
                }
                """)));

        GitHubRepositoryMetadataCollection result = await collector.CollectAsync(
            GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world"));

        Assert.False(result.IsAvailable);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    private static GitHubRepositoryMetadataCollector CreateCollector(
        StubHttpMessageHandler handler)
    {
        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        return new GitHubRepositoryMetadataCollector(
            client,
            new FixedTimeProvider(DateTimeOffset.Parse("2026-08-15T09:30:00Z")));
    }

    private static HttpResponseMessage JsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public IReadOnlyList<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; private set; } =
            Array.Empty<MediaTypeWithQualityHeaderValue>();

        public string? ApiVersion { get; private set; }

        public string? UserAgent { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            AcceptHeaders = request.Headers.Accept.ToArray();
            ApiVersion = request.Headers.GetValues("X-GitHub-Api-Version").Single();
            UserAgent = request.Headers.UserAgent.ToString();

            return Task.FromResult(responseFactory(request));
        }
    }
}

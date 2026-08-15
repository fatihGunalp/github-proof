using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryDescriptiveDataCollectorTests
{
    private const string HistoricalCommitSha =
        "c3eea5bec401935206d4697d0c73af950514c1ac";

    [Fact]
    public async Task CollectAsync_NormalizesReadmeLicenseAndLanguages()
    {
        QueueHttpMessageHandler handler = new(
            JsonResponse("""
                {
                  "path": "README.md",
                  "content": "IyBIZWxsbyBXb3JsZA==",
                  "encoding": "base64"
                }
                """),
            JsonResponse("""
                {
                  "path": "LICENSE",
                  "size": 1080,
                  "license": {
                    "key": "mit",
                    "name": "MIT License",
                    "spdx_id": "MIT"
                  }
                }
                """),
            JsonResponse("""
                {
                  " C# ": 1200,
                  "F#": 300
                }
                """));
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal("README.md", result.Readme!.Path);
        Assert.Equal("# Hello World", result.Readme.Content);
        Assert.Equal("LICENSE", result.LicenseFile!.Path);
        Assert.Equal(1080, result.LicenseFile.Size);
        Assert.Equal("MIT", result.License!.SpdxIdentifier);
        Assert.Equal("LICENSE", result.License.Path);
        Assert.Equal(1200, result.Languages["c#"]);
        Assert.Equal(300, result.Languages["f#"]);
        Assert.All(
            result.Availability,
            availability => Assert.Equal(DataAvailabilityStatus.Available, availability.Status));
        Assert.All(
            result.Availability,
            availability => Assert.Equal(
                DateTimeOffset.Parse("2026-08-15T09:30:00Z"),
                availability.RetrievedAt));
        Assert.Equal(
            [
                "/repos/octocat/hello-world/readme",
                "/repos/octocat/hello-world/license",
                "/repos/octocat/hello-world/languages"
            ],
            handler.RequestPaths);
    }

    [Fact]
    public async Task CollectAsync_PinsReadmeAndLicenseToExplicitCommitButKeepsLanguagesLive()
    {
        QueueHttpMessageHandler handler = new(
            JsonResponse("""
                {
                  "path": "README.md",
                  "content": "IyBIaXN0b3JpY2Fs",
                  "encoding": "base64"
                }
                """),
            JsonResponse("""
                {
                  "path": "LICENSE",
                  "size": 1080,
                  "license": {
                    "key": "mit",
                    "name": "MIT License",
                    "spdx_id": "MIT"
                  }
                }
                """),
            JsonResponse("{ \"Go\": 1200 }"));
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata(),
            GitHubCommitReference.Parse(HistoricalCommitSha));

        Assert.Equal("# Historical", result.Readme!.Content);
        Assert.Equal("MIT", result.License!.SpdxIdentifier);
        Assert.Equal(1200, result.Languages["Go"]);
        Assert.Equal("?ref=" + HistoricalCommitSha, handler.RequestUris[0].Query);
        Assert.Equal("?ref=" + HistoricalCommitSha, handler.RequestUris[1].Query);
        Assert.Empty(handler.RequestUris[2].Query);
    }

    [Fact]
    public async Task CollectAsync_MapsAbsentReadmeAndLicenseToAvailableEmptyEvidence()
    {
        QueueHttpMessageHandler handler = new(
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.NotFound),
            JsonResponse("{}"));
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Null(result.Readme);
        Assert.Null(result.License);
        Assert.Null(result.LicenseFile);
        Assert.Empty(result.Languages);
        Assert.All(
            result.Availability,
            availability => Assert.Equal(DataAvailabilityStatus.Available, availability.Status));
    }

    [Fact]
    public async Task CollectAsync_PreservesDistinctRateLimitForbiddenAndUnexpectedFailures()
    {
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        QueueHttpMessageHandler handler = new(
            rateLimited,
            new HttpResponseMessage(HttpStatusCode.Forbidden),
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(DataAvailabilityStatus.RateLimited, result.FindAvailability("readme")!.Status);
        Assert.Equal(DataAvailabilityStatus.Forbidden, result.FindAvailability("license")!.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, result.FindAvailability("languages")!.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsMalformedReadmeToFailedAvailabilityWhileRetainingOtherData()
    {
        QueueHttpMessageHandler handler = new(
            JsonResponse("""
                {
                  "path": "README.md",
                  "content": "not-base64",
                  "encoding": "base64"
                }
                """),
            new HttpResponseMessage(HttpStatusCode.NotFound),
            JsonResponse("{ \"C#\": 1200 }"));
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Null(result.Readme);
        Assert.Equal(DataAvailabilityStatus.Failed, result.FindAvailability("readme")!.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.FindAvailability("license")!.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.FindAvailability("languages")!.Status);
        Assert.Equal(1200, result.Languages["C#"]);
    }

    [Fact]
    public async Task CollectAsync_MapsTransportFailureToFailedAvailability()
    {
        QueueHttpMessageHandler handler = new(
            new HttpRequestException("Network unavailable"),
            new HttpResponseMessage(HttpStatusCode.NotFound),
            JsonResponse("{}"));
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(DataAvailabilityStatus.Failed, result.FindAvailability("readme")!.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.FindAvailability("license")!.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.FindAvailability("languages")!.Status);
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestDescriptiveDataWhenMetadataIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();
        GitHubRepositoryDescriptiveDataCollector collector = CreateCollector(handler);
        GitHubRepositoryMetadataCollection unavailableMetadata = new(
            Repository: null,
            Metadata: null,
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Unavailable(
                "metadata",
                DataAvailabilityStatus.RateLimited,
                "GitHub rate limit reached."));

        GitHubRepositoryDescriptiveDataCollection result = await collector.CollectAsync(
            RepositoryUrl,
            unavailableMetadata);

        Assert.Empty(handler.RequestPaths);
        Assert.All(
            result.Availability,
            availability => Assert.Equal(DataAvailabilityStatus.NotRequested, availability.Status));
    }

    private static GitHubRepositoryUrl RepositoryUrl { get; } =
        GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world");

    private static GitHubRepositoryMetadataCollection AvailableMetadata() =>
        new(
            new RepositoryIdentity(
                "octocat",
                "hello-world",
                new Uri("https://github.com/octocat/hello-world"),
                "main",
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
            Topics: ["dotnet"],
            License: null,
            DataAvailability.Available(
                "metadata",
                DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    private static GitHubRepositoryDescriptiveDataCollector CreateCollector(
        QueueHttpMessageHandler handler)
    {
        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        return new GitHubRepositoryDescriptiveDataCollector(
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

    private sealed class QueueHttpMessageHandler(params object[] responses) : HttpMessageHandler
    {
        private readonly Queue<object> responses = new(responses);

        public List<string> RequestPaths { get; } = [];

        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);
            RequestPaths.Add(request.RequestUri!.AbsolutePath);

            return responses.Dequeue() switch
            {
                HttpResponseMessage response => Task.FromResult(response),
                Exception exception => Task.FromException<HttpResponseMessage>(exception),
                _ => throw new InvalidOperationException("Unsupported HTTP response fixture.")
            };
        }
    }
}

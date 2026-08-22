using System.Net;
using System.Net.Http.Headers;
using System.Text;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;

namespace GitHubProof.Infrastructure.Tests.GitHub;

internal static class GitHubCollectionTestSupport
{
    public static GitHubRepositoryUrl RepositoryUrl { get; } =
        GitHubRepositoryUrl.Parse("https://github.com/octocat/hello-world");

    public static GitHubRepositoryMetadataCollection AvailableMetadata() =>
        new(
            new RepositoryIdentity(
                "octocat", "hello-world", new Uri("https://github.com/octocat/hello-world"),
                "main", isPublic: true, isArchived: false, isFork: false, isTemplate: false,
                primaryLanguage: "C#"),
            new RepositoryMetadata(null, null, DateTimeOffset.Parse("2026-08-01T00:00:00Z"), issuesEnabled: true),
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Available("metadata", DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    public static GitHubRepositoryMetadataCollection UnavailableMetadata() =>
        new(null, null, Array.Empty<string>(), null, DataAvailability.Unavailable(
            "metadata", DataAvailabilityStatus.RateLimited, "GitHub rate limit reached."));

    public static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    public static TimeProvider CreateFixedTimeProvider() =>
        new FixedTimeProvider(DateTimeOffset.Parse("2026-08-15T09:30:00Z"));

    public static HttpResponseMessage JsonResponse(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    public static HttpResponseMessage BlockingJsonResponse(
        CancellationObservingStream stream)
    {
        StreamContent content = new(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        };
    }

    internal sealed class QueueHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
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

    internal sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(exception);
    }

    internal sealed class CancellationObservingStream : Stream
    {
        private readonly TaskCompletionSource<CancellationToken> tokenObserved =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<CancellationToken> TokenObserved => tokenObserved.Task;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException("Only asynchronous reads are supported.");

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) =>
            ObserveAndWaitAsync(cancellationToken);

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            new(ObserveAndWaitAsync(cancellationToken));

        private async Task<int> ObserveAndWaitAsync(
            CancellationToken cancellationToken)
        {
            tokenObserved.TrySetResult(cancellationToken);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

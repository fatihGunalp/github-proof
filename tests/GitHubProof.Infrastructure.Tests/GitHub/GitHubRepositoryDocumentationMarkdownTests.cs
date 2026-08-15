using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryDocumentationMarkdownTests
{
    private const int MaximumFileBytes = 131072;
    private const int MaximumCorpusBytes = 524288;

    [Fact]
    public async Task CollectAsync_SelectsEligiblePathsInOrdinalOrderAndPinsContentToHead()
    {
        QueueHttpMessageHandler handler = CreateHandler(
            TreeResponse(
            [
                TreeEntry("docs/setup.md", size: 5),
                TreeEntry("documentation/setup.md", size: 5),
                TreeEntry("docs/install/windows.md", size: 7),
                TreeEntry("Docs/setup.md", size: 5),
                TreeEntry("docs/README.MD", size: 6),
                TreeEntry("docs/folder.md", type: "tree", size: null),
                TreeEntry("docs/setup.txt", size: 5),
                TreeEntry("examples/docs/setup.md", size: 5)
            ]),
            ContentResponse("docs/README.MD", "readme"),
            ContentResponse("docs/install/windows.md", "windows"),
            ContentResponse("docs/setup.md", "setup"));

        GitHubRepositoryContentsCollection result = await CreateCollector(handler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal(
            ["docs/README.MD", "docs/install/windows.md", "docs/setup.md"],
            result.DocumentationMarkdownFiles.Items.Select(file => file.Path));
        Assert.Equal(["readme", "windows", "setup"],
            result.DocumentationMarkdownFiles.Items.Select(file => file.Content));
        Assert.True(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(3, result.DocumentationMarkdownFiles.TotalCount);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            result.DocumentationMarkdownAvailability.Status);
        Assert.Equal(6, handler.RequestUris.Count);
        Assert.All(handler.RequestUris.Skip(3), uri => Assert.Contains("ref=head-sha", uri.Query));
        Assert.Contains("/contents/docs/README.MD", handler.RequestUris[3].AbsolutePath);
        Assert.Contains("/contents/docs/install/windows.md", handler.RequestUris[4].AbsolutePath);
        Assert.Contains("/contents/docs/setup.md", handler.RequestUris[5].AbsolutePath);
    }

    [Fact]
    public async Task CollectAsync_DistinguishesCompleteEmptyFromTruncatedPartialCorpus()
    {
        QueueHttpMessageHandler completeHandler = CreateHandler(
            TreeResponse([TreeEntry("src/app.cs", size: 10)]));
        QueueHttpMessageHandler truncatedHandler = CreateHandler(
            TreeResponse([TreeEntry("docs/setup.md", size: 5)], truncated: true),
            ContentResponse("docs/setup.md", "setup"));

        GitHubRepositoryContentsCollection complete = await CreateCollector(completeHandler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());
        GitHubRepositoryContentsCollection truncated = await CreateCollector(truncatedHandler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(complete.DocumentationMarkdownFiles.Items);
        Assert.Equal(0, complete.DocumentationMarkdownFiles.TotalCount);
        Assert.True(complete.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, complete.DocumentationMarkdownAvailability.Status);
        Assert.Single(truncated.DocumentationMarkdownFiles.Items);
        Assert.Null(truncated.DocumentationMarkdownFiles.TotalCount);
        Assert.False(truncated.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, truncated.DocumentationMarkdownAvailability.Status);
        Assert.Contains(truncated.CollectionWarnings, warning =>
            warning.Contains("Documentation Markdown", StringComparison.Ordinal)
            && warning.Contains("truncated", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CollectAsync_LimitsSelectionToFirstSixteenOrdinalCandidates()
    {
        string[] paths = Enumerable.Range(0, 17)
            .Select(index => $"docs/{index:D2}.md")
            .Reverse()
            .ToArray();
        QueueHttpMessageHandler handler = CreateHandler(
            TreeResponse(paths.Select(path => TreeEntry(path, size: 1))),
            paths
                .OrderBy(path => path, StringComparer.Ordinal)
                .Take(16)
                .Select(path => ContentResponse(path, "x"))
                .ToArray());

        GitHubRepositoryContentsCollection result = await CreateCollector(handler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal(16, result.DocumentationMarkdownFiles.Items.Count);
        Assert.Equal(
            Enumerable.Range(0, 16).Select(index => $"docs/{index:D2}.md"),
            result.DocumentationMarkdownFiles.Items.Select(file => file.Path));
        Assert.Null(result.DocumentationMarkdownFiles.TotalCount);
        Assert.False(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(16, handler.RequestUris.Count - 3);
        Assert.Contains(result.CollectionWarnings, warning =>
            warning.Contains("maximum file count of 16", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAsync_AllowsExactPerFileLimitAndRejectsOversizeOrUnknownSizeBeforeFetch()
    {
        string exactContent = new('a', MaximumFileBytes);
        QueueHttpMessageHandler handler = CreateHandler(
            TreeResponse(
            [
                TreeEntry("docs/exact.md", size: MaximumFileBytes),
                TreeEntry("docs/oversize.md", size: MaximumFileBytes + 1L),
                TreeEntry("docs/unknown.md", size: null)
            ]),
            ContentResponse("docs/exact.md", exactContent));

        GitHubRepositoryContentsCollection result = await CreateCollector(handler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal("docs/exact.md", Assert.Single(result.DocumentationMarkdownFiles.Items).Path);
        Assert.Equal(MaximumFileBytes, Encoding.UTF8.GetByteCount(
            result.DocumentationMarkdownFiles.Items[0].Content!));
        Assert.False(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(1, handler.RequestUris.Count - 3);
        Assert.Contains(result.CollectionWarnings, warning => warning.Contains("per-file limit", StringComparison.Ordinal));
        Assert.Contains(result.CollectionWarnings, warning => warning.Contains("no declared size", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAsync_AllowsExactTotalCorpusLimit()
    {
        string content = new('a', MaximumFileBytes);
        string[] paths = ["docs/a.md", "docs/b.md", "docs/c.md", "docs/d.md"];
        QueueHttpMessageHandler handler = CreateHandler(
            TreeResponse(paths.Select(path => TreeEntry(path, size: MaximumFileBytes))),
            paths.Select(path => ContentResponse(path, content)).ToArray());

        GitHubRepositoryContentsCollection result = await CreateCollector(handler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.True(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(4, result.DocumentationMarkdownFiles.TotalCount);
        Assert.Equal(
            MaximumCorpusBytes,
            result.DocumentationMarkdownFiles.Items.Sum(file =>
                Encoding.UTF8.GetByteCount(file.Content!)));
    }

    [Fact]
    public async Task CollectAsync_SkipsCandidateBeyondRemainingBudgetAndAcceptsSmallerLaterCandidate()
    {
        (string Path, int Size)[] candidates =
        [
            ("docs/a.md", 120000),
            ("docs/b.md", 120000),
            ("docs/c.md", 120000),
            ("docs/d.md", 120000),
            ("docs/e.md", 50000),
            ("docs/f.md", 40000)
        ];
        string[] fetchedPaths =
        [
            "docs/a.md",
            "docs/b.md",
            "docs/c.md",
            "docs/d.md",
            "docs/f.md"
        ];
        QueueHttpMessageHandler handler = CreateHandler(
            TreeResponse(candidates.Select(candidate => TreeEntry(candidate.Path, size: candidate.Size))),
            fetchedPaths
                .Select(path => ContentResponse(
                    path,
                    new string('a', candidates.Single(candidate => candidate.Path == path).Size)))
                .ToArray());

        GitHubRepositoryContentsCollection result = await CreateCollector(handler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Equal(fetchedPaths, result.DocumentationMarkdownFiles.Items.Select(file => file.Path));
        Assert.Equal(520000, result.DocumentationMarkdownFiles.Items.Sum(file =>
            Encoding.UTF8.GetByteCount(file.Content!)));
        Assert.False(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(5, handler.RequestUris.Count - 3);
        Assert.Contains(result.CollectionWarnings, warning =>
            warning.Contains("docs/e.md", StringComparison.Ordinal)
            && warning.Contains("remaining corpus budget", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAsync_RejectsDecodedContentThatExceedsDeclaredLimits()
    {
        QueueHttpMessageHandler perFileHandler = CreateHandler(
            TreeResponse([TreeEntry("docs/large.md", size: 1)]),
            ContentResponse("docs/large.md", new string('a', MaximumFileBytes + 1)));
        string[] paths = ["docs/a.md", "docs/b.md", "docs/c.md", "docs/d.md", "docs/e.md"];
        QueueHttpMessageHandler corpusHandler = CreateHandler(
            TreeResponse(paths.Select(path => TreeEntry(path, size: 1))),
            paths.Select(path => ContentResponse(path, new string('a', 110000))).ToArray());

        GitHubRepositoryContentsCollection perFile = await CreateCollector(perFileHandler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());
        GitHubRepositoryContentsCollection corpus = await CreateCollector(corpusHandler)
            .CollectAsync(RepositoryUrl, AvailableMetadata());

        Assert.Empty(perFile.DocumentationMarkdownFiles.Items);
        Assert.False(perFile.DocumentationMarkdownFiles.IsComplete);
        Assert.Contains(perFile.CollectionWarnings, warning =>
            warning.Contains("decoded content exceeded the per-file limit", StringComparison.Ordinal));
        Assert.Equal(4, corpus.DocumentationMarkdownFiles.Items.Count);
        Assert.False(corpus.DocumentationMarkdownFiles.IsComplete);
        Assert.Contains(corpus.CollectionWarnings, warning =>
            warning.Contains("decoded content exceeded the remaining corpus budget", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAsync_PreservesCollectedFilesAndActualFailureAvailability()
    {
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubRepositoryContentsCollection rateLimitedResult = await CollectWithSecondFailure(rateLimited);
        GitHubRepositoryContentsCollection forbiddenResult = await CollectWithSecondFailure(
            new HttpResponseMessage(HttpStatusCode.Forbidden));
        GitHubRepositoryContentsCollection malformedResult = await CollectWithSecondFailure(
            JsonResponse("""
                { "type": "file", "path": "docs/b.md", "encoding": "base64", "content": "***" }
                """));

        AssertFailureResult(rateLimitedResult, DataAvailabilityStatus.RateLimited);
        AssertFailureResult(forbiddenResult, DataAvailabilityStatus.Forbidden);
        AssertFailureResult(malformedResult, DataAvailabilityStatus.Failed);
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestDocumentationWhenMetadataIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();

        GitHubRepositoryContentsCollection result = await CreateCollector(handler)
            .CollectAsync(RepositoryUrl, UnavailableMetadata());

        Assert.Empty(handler.RequestUris);
        Assert.Empty(result.DocumentationMarkdownFiles.Items);
        Assert.False(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Equal(
            DataAvailabilityStatus.NotRequested,
            result.DocumentationMarkdownAvailability.Status);
    }

    private static async Task<GitHubRepositoryContentsCollection> CollectWithSecondFailure(
        HttpResponseMessage failure)
    {
        QueueHttpMessageHandler handler = CreateHandler(
            TreeResponse(
            [
                TreeEntry("docs/a.md", size: 1),
                TreeEntry("docs/b.md", size: 1)
            ]),
            ContentResponse("docs/a.md", "a"),
            failure);

        return await CreateCollector(handler).CollectAsync(RepositoryUrl, AvailableMetadata());
    }

    private static void AssertFailureResult(
        GitHubRepositoryContentsCollection result,
        DataAvailabilityStatus expectedStatus)
    {
        Assert.Equal("docs/a.md", Assert.Single(result.DocumentationMarkdownFiles.Items).Path);
        Assert.False(result.DocumentationMarkdownFiles.IsComplete);
        Assert.Null(result.DocumentationMarkdownFiles.TotalCount);
        Assert.Equal(expectedStatus, result.DocumentationMarkdownAvailability.Status);
        Assert.Contains(result.CollectionWarnings, warning =>
            warning.Contains("collected files were preserved", StringComparison.Ordinal));
    }

    private static GitHubRepositoryContentsCollector CreateCollector(HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());

    private static QueueHttpMessageHandler CreateHandler(
        HttpResponseMessage tree,
        params HttpResponseMessage[] contents) =>
        new([ReferenceResponse(), CommitResponse(), tree, .. contents]);

    private static HttpResponseMessage ReferenceResponse() =>
        JsonResponse("{ \"object\": { \"sha\": \"head-sha\" } }");

    private static HttpResponseMessage CommitResponse() =>
        JsonResponse("{ \"tree\": { \"sha\": \"root-tree-sha\" } }");

    private static HttpResponseMessage TreeResponse(
        IEnumerable<string> entries,
        bool truncated = false) =>
        JsonResponse(
            $"{{ \"tree\": [{string.Join(',', entries)}], \"truncated\": {truncated.ToString().ToLowerInvariant()} }}");

    private static string TreeEntry(string path, string type = "blob", long? size = 1) =>
        size is null
            ? $"{{ \"path\": \"{path}\", \"type\": \"{type}\" }}"
            : $"{{ \"path\": \"{path}\", \"type\": \"{type}\", \"size\": {size} }}";

    private static HttpResponseMessage ContentResponse(string path, string content) =>
        JsonResponse(
            $"{{ \"type\": \"file\", \"path\": \"{path}\", \"encoding\": \"base64\", \"content\": \"{Convert.ToBase64String(Encoding.UTF8.GetBytes(content))}\" }}");
}

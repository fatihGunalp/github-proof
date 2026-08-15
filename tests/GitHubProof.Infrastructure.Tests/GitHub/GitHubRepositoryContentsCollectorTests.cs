using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryContentsCollectorTests
{
    private const string HistoricalCommitSha =
        "c3eea5bec401935206d4697d0c73af950514c1ac";

    [Fact]
    public async Task CollectAsync_PinsTreeAndSelectedContentsToTheResolvedDefaultBranchHead()
    {
        QueueHttpMessageHandler handler = new(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("""
                [
                  { "path": "tests", "type": "tree" },
                  { "path": "tests/unit.test.js", "type": "blob", "size": 10 },
                  { "path": "src/Foo.Tests/Foo.Tests.csproj", "type": "blob", "size": 22 },
                  { "path": "package/__tests__/component.spec.ts", "type": "blob", "size": 11 },
                  { "path": ".github/workflows/ci.yml", "type": "blob", "size": 18 },
                  { "path": ".github/workflows/release.yaml", "type": "blob", "size": 19 }
                ]
                """),
            ContentResponse("src/Foo.Tests/Foo.Tests.csproj", "<Project />"),
            ContentResponse(".github/workflows/ci.yml", "name: CI"),
            ContentResponse(".github/workflows/release.yaml", "name: Release"));
        GitHubRepositoryContentsCollector collector = CreateCollector(handler);

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal("head-sha", result.HeadCommitSha);
        Assert.True(result.FileTree.IsComplete);
        Assert.Equal(6, result.FileTree.TotalCount);
        Assert.Equal(".github/workflows/ci.yml", result.FileTree.Items[4].Path);
        Assert.DoesNotContain('\\', result.FileTree.Items[4].Path);
        Assert.Equal(["src/Foo.Tests/Foo.Tests.csproj"], result.ProjectFiles.Items.Select(file => file.Path));
        Assert.Equal("<Project />", result.ProjectFiles.Items[0].Content);
        Assert.Equal(
            [".github/workflows/ci.yml", ".github/workflows/release.yaml"],
            result.Workflows.Items.Select(file => file.Path));
        Assert.Equal("name: CI", result.Workflows.Items[0].Content);
        Assert.Equal(DataAvailabilityStatus.Available, result.FileTreeAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.ProjectFilesAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.WorkflowsAvailability.Status);
        Assert.Contains("/git/ref/heads/main", handler.RequestUris[0].AbsolutePath);
        Assert.Contains("/git/commits/head-sha", handler.RequestUris[1].AbsolutePath);
        Assert.Contains("/git/trees/root-tree-sha", handler.RequestUris[2].AbsolutePath);
        Assert.All(handler.RequestUris.Skip(3), uri => Assert.Contains("ref=head-sha", uri.Query));
    }

    [Fact]
    public async Task CollectAsync_PinsTreeAndSelectedContentsToExplicitCommitWithoutResolvingHead()
    {
        QueueHttpMessageHandler handler = new(
            CommitResponse("historical-root-tree"),
            TreeResponse("""
                [
                  { "path": "package.json", "type": "blob", "size": 12 },
                  { "path": "tests/app.test.js", "type": "blob", "size": 10 },
                  { "path": ".github/workflows/ci.yml", "type": "blob", "size": 18 },
                  { "path": "docs/setup.md", "type": "blob", "size": 20 }
                ]
                """),
            ContentResponse("package.json", "{}"),
            ContentResponse(".github/workflows/ci.yml", "run: npm test"),
            ContentResponse("docs/setup.md", "# Setup"));
        GitHubRepositoryContentsCollector collector = CreateCollector(handler);

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata(),
            GitHubCommitReference.Parse(HistoricalCommitSha));

        Assert.Equal(HistoricalCommitSha, result.HeadCommitSha);
        Assert.Contains("/git/commits/" + HistoricalCommitSha, handler.RequestUris[0].AbsolutePath);
        Assert.DoesNotContain(
            handler.RequestUris,
            uri => uri.AbsolutePath.Contains("/git/ref/heads/", StringComparison.Ordinal));
        Assert.All(
            handler.RequestUris.Skip(2),
            uri => Assert.Contains("ref=" + HistoricalCommitSha, uri.Query));
        Assert.Contains(result.FileTree.Items, file => file.Path == "tests/app.test.js");
        Assert.Equal("run: npm test", Assert.Single(result.Workflows.Items).Content);
        Assert.Equal("# Setup", Assert.Single(result.DocumentationMarkdownFiles.Items).Content);
    }

    [Fact]
    public async Task CollectAsync_RepresentsNonexistentExplicitCommitAsUnavailableWithoutHeadFallback()
    {
        QueueHttpMessageHandler handler = new(new HttpResponseMessage(HttpStatusCode.NotFound));
        GitHubRepositoryContentsCollector collector = CreateCollector(handler);

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata(),
            GitHubCommitReference.Parse(HistoricalCommitSha));

        Assert.Null(result.HeadCommitSha);
        Assert.Equal(DataAvailabilityStatus.Unavailable, result.FileTreeAvailability.Status);
        Assert.Single(handler.RequestUris);
        Assert.Contains("/git/commits/" + HistoricalCommitSha, handler.RequestUris[0].AbsolutePath);
        Assert.DoesNotContain("/git/ref/heads/", handler.RequestUris[0].AbsolutePath);
    }

    [Fact]
    public async Task CollectAsync_RepresentsNoWorkflowsInACompleteTreeAsAvailableEmptyEvidence()
    {
        QueueHttpMessageHandler handler = new(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("[{ \"path\": \"src/app.cs\", \"type\": \"blob\", \"size\": 10 }]"));
        GitHubRepositoryContentsCollector collector = CreateCollector(handler);

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.True(result.FileTree.IsComplete);
        Assert.Empty(result.Workflows.Items);
        Assert.Equal(0, result.Workflows.TotalCount);
        Assert.True(result.Workflows.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.WorkflowsAvailability.Status);
        Assert.Equal(3, handler.RequestUris.Count);
    }

    [Fact]
    public async Task CollectAsync_PreservesEarlierWorkflowContentsWhenLaterContentIsRateLimited()
    {
        HttpResponseMessage rateLimited = new(HttpStatusCode.Forbidden);
        rateLimited.Headers.Add("X-RateLimit-Remaining", "0");
        GitHubRepositoryContentsCollector collector = CreateCollector(new QueueHttpMessageHandler(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("""
                [
                  { "path": ".github/workflows/ci.yml", "type": "blob", "size": 8 },
                  { "path": ".github/workflows/verify.yaml", "type": "blob", "size": 12 }
                ]
                """),
            ContentResponse(".github/workflows/ci.yml", "name: CI"),
            rateLimited));

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal([".github/workflows/ci.yml"], result.Workflows.Items.Select(file => file.Path));
        Assert.Null(result.Workflows.TotalCount);
        Assert.False(result.Workflows.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.WorkflowsAvailability.Status);
        Assert.True(result.FileTree.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_PreservesTruncatedTreeEntriesWithoutClaimingCompleteSelections()
    {
        GitHubRepositoryContentsCollector collector = CreateCollector(new QueueHttpMessageHandler(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("[{ \"path\": \"tests\", \"type\": \"tree\" }]", truncated: true)));

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(["tests"], result.FileTree.Items.Select(file => file.Path));
        Assert.False(result.FileTree.IsComplete);
        Assert.Null(result.FileTree.TotalCount);
        Assert.False(result.ProjectFiles.IsComplete);
        Assert.False(result.Workflows.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.FileTreeAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.WorkflowsAvailability.Status);
        Assert.Contains(result.CollectionWarnings, warning => warning.Contains("truncated", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CollectAsync_DeduplicatesPathsAndPreservesEmptyTreeAsAvailable()
    {
        GitHubRepositoryContentsCollector duplicateCollector = CreateCollector(new QueueHttpMessageHandler(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("""
                [
                  { "path": "tests", "type": "tree" },
                  { "path": "tests", "type": "tree" }
                ]
                """)));
        GitHubRepositoryContentsCollection duplicate = await duplicateCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        GitHubRepositoryContentsCollector emptyCollector = CreateCollector(new QueueHttpMessageHandler(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("[]")));
        GitHubRepositoryContentsCollection empty = await emptyCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Equal(["tests"], duplicate.FileTree.Items.Select(file => file.Path));
        Assert.Equal(1, duplicate.FileTree.TotalCount);
        Assert.Empty(empty.FileTree.Items);
        Assert.True(empty.FileTree.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, empty.FileTreeAvailability.Status);
    }

    [Fact]
    public async Task CollectAsync_RepresentsGitHubEmptyRepositoryConflictAsAvailableEmptyCollections()
    {
        GitHubRepositoryContentsCollector collector = CreateCollector(
            new QueueHttpMessageHandler(JsonResponse(
                "{ \"message\": \"Git Repository is empty.\" }",
                HttpStatusCode.Conflict)));

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Null(result.HeadCommitSha);
        Assert.Empty(result.FileTree.Items);
        Assert.Empty(result.ProjectFiles.Items);
        Assert.Empty(result.Workflows.Items);
        Assert.True(result.FileTree.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Available, result.FileTreeAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.Available, result.WorkflowsAvailability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsRateLimitBeforeTreeDataAndMalformedTreeResponses()
    {
        HttpResponseMessage rateLimited = new(HttpStatusCode.TooManyRequests);
        GitHubRepositoryContentsCollector rateLimitedCollector = CreateCollector(
            new QueueHttpMessageHandler(rateLimited));
        GitHubRepositoryContentsCollection rateLimitedResult = await rateLimitedCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        GitHubRepositoryContentsCollector malformedCollector = CreateCollector(new QueueHttpMessageHandler(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("[{ \"path\": \"\", \"type\": \"blob\" }]")));
        GitHubRepositoryContentsCollection malformedResult = await malformedCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.False(rateLimitedResult.FileTree.IsComplete);
        Assert.Equal(DataAvailabilityStatus.RateLimited, rateLimitedResult.FileTreeAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.NotRequested, rateLimitedResult.WorkflowsAvailability.Status);
        Assert.False(malformedResult.FileTree.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, malformedResult.FileTreeAvailability.Status);
    }

    [Fact]
    public async Task CollectAsync_MapsUnsupportedContentAndTransportFailureWithoutPretendingContentIsEmpty()
    {
        GitHubRepositoryContentsCollector unsupportedContentCollector = CreateCollector(new QueueHttpMessageHandler(
            ReferenceResponse("head-sha"),
            CommitResponse("root-tree-sha"),
            TreeResponse("[{ \"path\": \".github/workflows/ci.yml\", \"type\": \"blob\", \"size\": 1048577 }]"),
            JsonResponse("""
                { "type": "file", "path": ".github/workflows/ci.yml", "encoding": "none", "content": "" }
                """)));
        GitHubRepositoryContentsCollection unsupportedContent = await unsupportedContentCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        GitHubRepositoryContentsCollector transportCollector = CreateCollector(
            new ThrowingHttpMessageHandler(new HttpRequestException("Network unavailable")));
        GitHubRepositoryContentsCollection transportFailure = await transportCollector.CollectAsync(
            RepositoryUrl,
            AvailableMetadata());

        Assert.Empty(unsupportedContent.Workflows.Items);
        Assert.False(unsupportedContent.Workflows.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, unsupportedContent.WorkflowsAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, transportFailure.FileTreeAvailability.Status);
    }

    [Fact]
    public async Task CollectAsync_DoesNotRequestContentsWhenMetadataIsUnavailable()
    {
        QueueHttpMessageHandler handler = new();
        GitHubRepositoryContentsCollector collector = CreateCollector(handler);

        GitHubRepositoryContentsCollection result = await collector.CollectAsync(
            RepositoryUrl,
            UnavailableMetadata());

        Assert.Empty(handler.RequestUris);
        Assert.False(result.FileTree.IsComplete);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.FileTreeAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.ProjectFilesAvailability.Status);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.WorkflowsAvailability.Status);
    }

    private static GitHubRepositoryContentsCollector CreateCollector(HttpMessageHandler handler) =>
        new(CreateClient(handler), CreateFixedTimeProvider());

    private static HttpResponseMessage ReferenceResponse(string headCommitSha) =>
        JsonResponse($"{{ \"object\": {{ \"sha\": \"{headCommitSha}\" }} }}");

    private static HttpResponseMessage CommitResponse(string rootTreeSha) =>
        JsonResponse($"{{ \"tree\": {{ \"sha\": \"{rootTreeSha}\" }} }}");

    private static HttpResponseMessage TreeResponse(string entries, bool truncated = false) =>
        JsonResponse($"{{ \"tree\": {entries}, \"truncated\": {truncated.ToString().ToLowerInvariant()} }}");

    private static HttpResponseMessage ContentResponse(string path, string content) =>
        JsonResponse(
            $"{{ \"type\": \"file\", \"path\": \"{path}\", \"encoding\": \"base64\", \"content\": \"{Convert.ToBase64String(Encoding.UTF8.GetBytes(content))}\" }}");
}

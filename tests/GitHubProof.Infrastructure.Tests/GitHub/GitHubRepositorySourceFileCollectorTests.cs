using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

// Locks the bounded raw source collection contract in
// docs/architecture/repository-source-evidence-collection.md.
public sealed class GitHubRepositorySourceFileCollectorTests
{
    [Theory]
    [InlineData("100644")]
    [InlineData("100755")]
    public async Task CollectAsync_RegularBlob_IsCollected(string mode)
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [Blob("src/Program.cs", 16, mode)],
                truncated: false,
                Content("src/Program.cs", "class Program { }"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        RepositorySourceFile file = Assert.Single(result.SourceFiles.Items);
        Assert.Equal("src/Program.cs", file.Path);
        Assert.Equal("class Program { }", file.Content);
        Assert.True(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
        Assert.Equal(3, handler.RequestUris.Count);
    }

    [Fact]
    public async Task CollectAsync_SymlinkWithSourceExtension_IsOutsideTheSupportedSurface()
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [Symlink("playground/linked.js", 14)],
                truncated: false);

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Empty(result.SourceFiles.Items);
        Assert.True(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
        Assert.Equal(2, handler.RequestUris.Count);
    }

    [Fact]
    public async Task CollectAsync_SubmoduleAndTreeEntries_AreOutsideTheSupportedSurface()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Submodule("vendored/module.cs"), TreeEntry("src"), Blob("src/App.cs", 4)],
            truncated: false,
            Content("src/App.cs", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal("src/App.cs", Assert.Single(result.SourceFiles.Items).Path);
        Assert.True(result.IsComplete);
    }

    [Theory]
    [InlineData("src/a.cs", true)]
    [InlineData("src/a.fs", true)]
    [InlineData("src/a.vb", true)]
    [InlineData("src/a.kt", true)]
    [InlineData("src/a.kts", true)]
    [InlineData("src/a.scala", true)]
    [InlineData("src/a.groovy", true)]
    [InlineData("src/a.d.ts", true)]
    [InlineData("src/a.vue", true)]
    [InlineData("src/a.svelte", true)]
    [InlineData("src/a.razor", true)]
    [InlineData("src/a.cshtml", true)]
    [InlineData("src/A.CS", true)]
    [InlineData("src/a.json", false)]
    [InlineData("src/a.yaml", false)]
    [InlineData("src/a.toml", false)]
    [InlineData("src/a.xml", false)]
    [InlineData("src/a.md", false)]
    [InlineData("src/a.c", false)]
    [InlineData("src/Makefile", false)]
    public async Task CollectAsync_AppliesTheGlobalExtensionAllowList(
        string path,
        bool isEligible)
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob(path, 4)],
            truncated: false,
            Content(path, "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal(isEligible ? 1 : 0, result.SourceFiles.Items.Count);

        // An unsupported extension is never negative evidence and never
        // incompleteness.
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_ExcludesExactGradleBuildFilenamesButNotAllKotlinScripts()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [
                Blob("build.gradle.kts", 4),
                Blob("settings.gradle.kts", 4),
                Blob("scripts/Release.kts", 4)
            ],
            truncated: false,
            Content("scripts/Release.kts", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal("scripts/Release.kts", Assert.Single(result.SourceFiles.Items).Path);
        Assert.True(result.IsComplete);
    }

    [Theory]
    [InlineData("node_modules/pkg/index.js", false)]
    [InlineData("src/bin/App.cs", false)]
    [InlineData("app/dist/bundle.js", false)]
    [InlineData("Server/obj/Generated.cs", false)]
    [InlineData("web/.next/page.js", false)]
    [InlineData("packages/app/src/index.ts", true)]
    [InlineData("src/App.cs", true)]
    public async Task CollectAsync_AppliesStructuralPathExclusions(string path, bool isEligible)
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob(path, 4)],
            truncated: false,
            Content(path, "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal(isEligible ? 1 : 0, result.SourceFiles.Items.Count);

        // Exclusion is collection budget policy, not incompleteness.
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_UsesADeterministicExtensionRoundRobinTotalOrder()
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [
                    Blob("src/c.cs", 4),
                    Blob("src/z.ts", 4),
                    Blob("src/a.cs", 4),
                    Blob("src/b.cs", 4)
                ],
                truncated: false,
                Content("src/a.cs", "code"),
                Content("src/z.ts", "code"),
                Content("src/b.cs", "code"),
                Content("src/c.cs", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal(
            ["src/a.cs", "src/z.ts", "src/b.cs", "src/c.cs"],
            ContentPaths(handler));
        Assert.Equal(4, result.SourceFiles.Items.Count);
    }

    [Fact]
    public async Task CollectAsync_OversizedCandidate_ConsumesNoRetrievalSlot()
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [
                    Blob("src/a.cs", GitHubRepositorySourceFileCollector.MaximumSourceFileBytes + 1),
                    Blob("src/b.cs", 4)
                ],
                truncated: false,
                Content("src/b.cs", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal(["src/b.cs"], ContentPaths(handler));
        Assert.Equal("src/b.cs", Assert.Single(result.SourceFiles.Items).Path);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_UnknownDeclaredSize_ConsumesNoRetrievalSlotAndIsIncomplete()
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [BlobWithoutSize("src/a.cs"), Blob("src/b.cs", 4)],
                truncated: false,
                Content("src/b.cs", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal(["src/b.cs"], ContentPaths(handler));
        Assert.Single(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_ProviderFailureMidWalk_StillConsumesItsRetrievalSlot()
    {
        object[] entries = [.. Enumerable.Range(1, 33).Select(index => Blob(SourcePath(index), 4))];
        HttpResponseMessage[] contents =
        [
            .. Enumerable.Range(1, 32).Select(index => index == 5
                ? JsonResponse("{}", HttpStatusCode.InternalServerError)
                : Content(SourcePath(index), "code"))
        ];
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(entries, truncated: false, contents);

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        // 32 retrieval attempts, one of which failed after the slot was consumed.
        Assert.Equal(32, ContentPaths(handler).Count);
        Assert.Equal(31, result.SourceFiles.Items.Count);
        Assert.DoesNotContain(SourcePath(33), ContentPaths(handler));
        Assert.DoesNotContain(
            result.SourceFiles.Items,
            file => file.Path == SourcePath(5));
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Failed, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_PreFetchRejectionMidWalk_LetsALaterCandidateUseTheLastSlot()
    {
        object[] entries =
        [
            .. Enumerable.Range(1, 33).Select(index => index == 5
                ? Blob(SourcePath(index), GitHubRepositorySourceFileCollector.MaximumSourceFileBytes + 1)
                : Blob(SourcePath(index), 4))
        ];
        HttpResponseMessage[] contents =
        [
            .. Enumerable.Range(1, 33)
                .Where(index => index != 5)
                .Select(index => Content(SourcePath(index), "code"))
        ];
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(entries, truncated: false, contents);

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Equal(32, ContentPaths(handler).Count);
        Assert.Contains(SourcePath(33), ContentPaths(handler));
        Assert.DoesNotContain(SourcePath(5), ContentPaths(handler));
        Assert.Equal(32, result.SourceFiles.Items.Count);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_CorpusSkip_ContinuesToALaterFittingCandidate()
    {
        const int large = 65536;
        object[] entries =
        [
            Blob("src/a1.cs", large),
            Blob("src/a2.cs", large),
            Blob("src/a3.cs", large),
            Blob("src/a4.cs", large),
            Blob("src/a5.cs", large),
            Blob("src/b.cs", 60000),
            Blob("src/x.cs", 60000),
            Blob("src/y.cs", 100)
        ];
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                entries,
                truncated: false,
                Content("src/a1.cs", Filler(large)),
                Content("src/a2.cs", Filler(large)),
                Content("src/a3.cs", Filler(large)),
                Content("src/a4.cs", Filler(large)),
                Content("src/a5.cs", Filler(large)),
                Content("src/b.cs", Filler(60000)),
                Content("src/y.cs", Filler(100)));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.DoesNotContain("src/x.cs", ContentPaths(handler));
        Assert.Contains("src/y.cs", ContentPaths(handler));
        Assert.Equal(7, result.SourceFiles.Items.Count);
        Assert.False(result.IsComplete);
        Assert.True(
            result.SourceFiles.Items.Sum(file => Encoding.UTF8.GetByteCount(file.Content))
                <= GitHubRepositorySourceFileCollector.MaximumSourceCorpusBytes);
    }

    [Fact]
    public async Task CollectAsync_StripsOneLeadingByteOrderMarkAndPreservesLineEndings()
    {
        byte[] payload = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes("a\r\nb\n")];
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", payload.Length)],
            truncated: false,
            RawContent("src/a.cs", "base64", payload));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        RepositorySourceFile file = Assert.Single(result.SourceFiles.Items);
        Assert.Equal("a\r\nb\n", file.Content);
        Assert.Equal(5, Encoding.UTF8.GetByteCount(file.Content));
    }

    [Fact]
    public async Task CollectAsync_DeclaredSizeAboveTheBoundIsRejectedEvenWhenTheBomWouldFit()
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [Blob("src/a.cs", GitHubRepositorySourceFileCollector.MaximumSourceFileBytes + 3)],
                truncated: false);

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Empty(ContentPaths(handler));
        Assert.Empty(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_BomBearingFileAtTheBound_IsAcceptedOnNormalizedBytes()
    {
        const int declared = GitHubRepositorySourceFileCollector.MaximumSourceFileBytes;
        byte[] payload = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(Filler(declared - 3))];
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", declared)],
            truncated: false,
            RawContent("src/a.cs", "base64", payload));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        RepositorySourceFile file = Assert.Single(result.SourceFiles.Items);
        Assert.Equal(declared - 3, Encoding.UTF8.GetByteCount(file.Content));
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_InvalidUtf8_ProducesNoArtefactAndStaysAvailable()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", 2)],
            truncated: false,
            RawContent("src/a.cs", "base64", [0xFF, 0xFE]));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Empty(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_NulByte_ProducesNoArtefactAndStaysAvailable()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", 3)],
            truncated: false,
            RawContent("src/a.cs", "base64", [0x61, 0x00, 0x62]));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Empty(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_UnsupportedProviderEncoding_ProducesNoArtefact()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", 4)],
            truncated: false,
            RawContent("src/a.cs", "none", Encoding.UTF8.GetBytes("code")));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Empty(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_TruncatedTree_IsPartialAndIncompleteRatherThanEmpty()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", 4)],
            truncated: true,
            Content("src/a.cs", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Single(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_RateLimitedContent_KeepsTheRealFailureStatus()
    {
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", 4), Blob("src/b.cs", 4)],
            truncated: false,
            Content("src/a.cs", "code"),
            RateLimitedResponse());

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        // Partial positive evidence is preserved beside the real failure status.
        Assert.Equal("src/a.cs", Assert.Single(result.SourceFiles.Items).Path);
        Assert.False(result.Availability.IsAvailable);
        Assert.Equal(DataAvailabilityStatus.RateLimited, result.Availability.Status);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_WhenTheTreeRequestFails_KeepsTheFailureAvailability()
    {
        QueueHttpMessageHandler handler = new(
            JsonResponse("{}", HttpStatusCode.NotFound));
        GitHubRepositorySourceFileCollector collector = new(
            CreateClient(handler),
            CreateFixedTimeProvider());

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Empty(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
        Assert.False(result.Availability.IsAvailable);
        Assert.Equal("source-files", result.Availability.DataKey);
    }

    [Fact]
    public async Task CollectAsync_WhenTheFileTreeIsUnavailable_IsNotRequested()
    {
        GitHubRepositoryContentsCollection contents = new(
            HeadCommitSha: null,
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            DataAvailability.Unavailable(
                "file-tree",
                DataAvailabilityStatus.RateLimited,
                "GitHub rate limit reached."),
            DataAvailability.Unavailable("project-files", DataAvailabilityStatus.NotRequested, "n/a"),
            DataAvailability.Unavailable("workflows", DataAvailabilityStatus.NotRequested, "n/a"),
            DataAvailability.Unavailable("documentation-markdown", DataAvailabilityStatus.NotRequested, "n/a"),
            Array.Empty<string>());
        QueueHttpMessageHandler handler = new();

        GitHubRepositorySourceFileCollection result = await new GitHubRepositorySourceFileCollector(
                CreateClient(handler),
                CreateFixedTimeProvider())
            .CollectAsync(RepositoryUrl, contents);

        Assert.Empty(handler.RequestUris);
        Assert.Empty(result.SourceFiles.Items);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.NotRequested, result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_WhenTheRepositoryIsEmpty_IsAvailableCompleteAndEmpty()
    {
        GitHubRepositoryContentsCollection contents = new(
            HeadCommitSha: null,
            new RepositoryCollection<RepositoryFile>(),
            new RepositoryCollection<RepositoryFile>(),
            new RepositoryCollection<RepositoryFile>(),
            new RepositoryCollection<RepositoryFile>(),
            DataAvailability.Available("file-tree", RetrievedAt),
            DataAvailability.Available("project-files", RetrievedAt),
            DataAvailability.Available("workflows", RetrievedAt),
            DataAvailability.Available("documentation-markdown", RetrievedAt),
            Array.Empty<string>());
        QueueHttpMessageHandler handler = new();

        GitHubRepositorySourceFileCollection result = await new GitHubRepositorySourceFileCollector(
                CreateClient(handler),
                CreateFixedTimeProvider())
            .CollectAsync(RepositoryUrl, contents);

        Assert.Empty(handler.RequestUris);
        Assert.Empty(result.SourceFiles.Items);
        Assert.True(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_PinsSourceContentToTheCollectedHeadCommit()
    {
        (GitHubRepositorySourceFileCollector collector, QueueHttpMessageHandler handler) =
            CreateCollector(
                [Blob("src/a.cs", 4)],
                truncated: false,
                Content("src/a.cs", "code"));

        await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Contains("git/commits/head-123", handler.RequestUris[0].ToString());
        Assert.EndsWith("?ref=head-123", handler.RequestUris[2].ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CollectAsync_IsNotGatedByDependencyEvidence()
    {
        // The collector reads no declaration, manifest availability or semantic
        // evidence; the contents surface below carries none of them.
        (GitHubRepositorySourceFileCollector collector, _) = CreateCollector(
            [Blob("src/a.cs", 4)],
            truncated: false,
            Content("src/a.cs", "code"));

        GitHubRepositorySourceFileCollection result =
            await collector.CollectAsync(RepositoryUrl, CreateContents());

        Assert.Single(result.SourceFiles.Items);
        Assert.True(result.Availability.IsAvailable);
    }

    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:30:00Z");

    private static (GitHubRepositorySourceFileCollector Collector, QueueHttpMessageHandler Handler)
        CreateCollector(
            object[] treeEntries,
            bool truncated,
            params HttpResponseMessage[] contentResponses)
    {
        QueueHttpMessageHandler handler = new(
        [
            CommitResponse(),
            TreeResponse(treeEntries, truncated),
            .. contentResponses
        ]);

        return (
            new GitHubRepositorySourceFileCollector(
                CreateClient(handler),
                CreateFixedTimeProvider()),
            handler);
    }

    private static GitHubRepositoryContentsCollection CreateContents() =>
        new(
            "head-123",
            new RepositoryCollection<RepositoryFile>(),
            RepositoryCollection<RepositoryFile>.Empty,
            RepositoryCollection<RepositoryFile>.Empty,
            RepositoryCollection<RepositoryFile>.Empty,
            DataAvailability.Available("file-tree", RetrievedAt),
            DataAvailability.Available("project-files", RetrievedAt),
            DataAvailability.Available("workflows", RetrievedAt),
            DataAvailability.Available("documentation-markdown", RetrievedAt),
            Array.Empty<string>());

    private static string SourcePath(int index) =>
        $"src/f{index:D2}.cs";

    private static string Filler(int length) => new('a', length);

    private static List<string> ContentPaths(QueueHttpMessageHandler handler) =>
    [
        .. handler.RequestUris
            .Where(uri => uri.AbsolutePath.Contains("/contents/", StringComparison.Ordinal))
            .Select(uri => Uri.UnescapeDataString(
                uri.AbsolutePath[(uri.AbsolutePath.IndexOf("/contents/", StringComparison.Ordinal) + 10)..]))
    ];

    private static object Blob(string path, long size, string mode = "100644") =>
        new { path, type = "blob", mode, size };

    private static object BlobWithoutSize(string path) =>
        new { path, type = "blob", mode = "100644" };

    private static object Symlink(string path, long size) =>
        new { path, type = "blob", mode = "120000", size };

    private static object Submodule(string path) =>
        new { path, type = "commit", mode = "160000" };

    private static object TreeEntry(string path) =>
        new { path, type = "tree", mode = "040000" };

    private static HttpResponseMessage CommitResponse() =>
        JsonResponse(JsonSerializer.Serialize(new { tree = new { sha = "tree-1" } }));

    private static HttpResponseMessage TreeResponse(object[] entries, bool truncated) =>
        JsonResponse(JsonSerializer.Serialize(new { sha = "tree-1", tree = entries, truncated }));

    private static HttpResponseMessage Content(string path, string content) =>
        RawContent(path, "base64", Encoding.UTF8.GetBytes(content));

    private static HttpResponseMessage RawContent(string path, string encoding, byte[] bytes) =>
        JsonResponse(JsonSerializer.Serialize(new
        {
            type = "file",
            path,
            encoding,
            content = Convert.ToBase64String(bytes)
        }));

    private static HttpResponseMessage RateLimitedResponse()
    {
        HttpResponseMessage response = JsonResponse("{}", HttpStatusCode.Forbidden);
        response.Headers.TryAddWithoutValidation("X-RateLimit-Remaining", "0");

        return response;
    }
}

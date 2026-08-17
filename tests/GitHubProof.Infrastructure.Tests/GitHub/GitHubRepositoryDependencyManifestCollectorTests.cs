using System.Net;
using System.Text;
using System.Text.Json;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using static GitHubProof.Infrastructure.Tests.GitHub.GitHubCollectionTestSupport;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryDependencyManifestCollectorTests
{
    [Theory]
    [InlineData(
        "src/App.csproj",
        DependencyEcosystem.DotNet,
        "<Project><ItemGroup><PackageReference Include=\"Serilog\" Version=\"3.1.1\" /></ItemGroup></Project>",
        "Serilog",
        "3.1.1")]
    [InlineData(
        "src/App.fsproj",
        DependencyEcosystem.DotNet,
        "<Project><ItemGroup><PackageReference Include=\"FSharp.Core\" Version=\"8.0.100\" /></ItemGroup></Project>",
        "FSharp.Core",
        "8.0.100")]
    [InlineData(
        "src/App.vbproj",
        DependencyEcosystem.DotNet,
        "<Project><ItemGroup><PackageReference Include=\"Newtonsoft.Json\"><Version>13.0.3</Version></PackageReference></ItemGroup></Project>",
        "Newtonsoft.Json",
        "13.0.3")]
    [InlineData(
        "Directory.Packages.props",
        DependencyEcosystem.DotNet,
        "<Project><ItemGroup><PackageVersion Include=\"xunit\" Version=\"2.9.0\" /></ItemGroup></Project>",
        "xunit",
        "2.9.0")]
    [InlineData(
        "build.gradle.kts",
        DependencyEcosystem.Java,
        "dependencies {\n  implementation(\"org.jetbrains.kotlin:kotlin-stdlib:1.9.22\")\n}",
        "org.jetbrains.kotlin:kotlin-stdlib",
        "1.9.22")]
    [InlineData(
        "requirements-dev.txt",
        DependencyEcosystem.Python,
        "pytest>=8.0\n",
        "pytest",
        ">=8.0")]
    [InlineData(
        "pom.xml",
        DependencyEcosystem.Java,
        "<project><dependencies><dependency><groupId>org.junit</groupId><artifactId>junit</artifactId><version>5.10.0</version></dependency></dependencies></project>",
        "org.junit:junit",
        "5.10.0")]
    [InlineData(
        "build.gradle",
        DependencyEcosystem.Java,
        "dependencies {\n  implementation 'com.google.guava:guava:33.0.0'\n}",
        "com.google.guava:guava",
        "33.0.0")]
    [InlineData(
        "package.json",
        DependencyEcosystem.JavaScript,
        "{\"dependencies\":{\"express\":\"^4.18.2\"}}",
        "express",
        "^4.18.2")]
    [InlineData(
        "composer.json",
        DependencyEcosystem.Php,
        "{\"require\":{\"laravel/framework\":\"^10.0\"}}",
        "laravel/framework",
        "^10.0")]
    [InlineData(
        "pyproject.toml",
        DependencyEcosystem.Python,
        "[project]\nname = \"demo\"\ndependencies = [\"requests>=2.31\"]\n",
        "requests",
        ">=2.31")]
    [InlineData(
        "requirements.txt",
        DependencyEcosystem.Python,
        "# comment\nflask==3.0.0\n",
        "flask",
        "==3.0.0")]
    [InlineData(
        "go.mod",
        DependencyEcosystem.Go,
        "module demo\n\nrequire (\n\tgithub.com/gin-gonic/gin v1.9.1 // indirect\n)\n",
        "github.com/gin-gonic/gin",
        "v1.9.1")]
    [InlineData(
        "Cargo.toml",
        DependencyEcosystem.Rust,
        "[dependencies]\nserde = { version = \"1.0.197\", features = [\"derive\"] }\n",
        "serde",
        "1.0.197")]
    [InlineData(
        "Gemfile",
        DependencyEcosystem.Ruby,
        "source 'https://rubygems.org'\ngem 'rails', '~> 7.1'\n",
        "rails",
        "~> 7.1")]
    [InlineData(
        "demo.gemspec",
        DependencyEcosystem.Ruby,
        "Gem::Specification.new do |spec|\n  spec.add_dependency 'rake', '>= 13.0'\nend",
        "rake",
        ">= 13.0")]
    public async Task CollectAsync_NormalizesEveryCoveredEcosystemIntoOneShape(
        string path,
        DependencyEcosystem expectedEcosystem,
        string content,
        string expectedPackageId,
        string expectedVersion)
    {
        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            [Manifest(path, content.Length)],
            ContentResponse(path, content));

        RepositoryDependencyDeclaration declaration =
            Assert.Single(result.Declarations.Items);

        Assert.Equal(expectedEcosystem, declaration.Ecosystem);
        Assert.Equal(path, declaration.ManifestPath);
        Assert.Equal(expectedPackageId, declaration.PackageId);
        Assert.Equal(expectedVersion, declaration.DeclaredVersion);
        Assert.True(result.IsComplete);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            result.Availability.Status);
    }

    [Fact]
    public async Task CollectAsync_WhenNoManifestExistsInACompleteTree_IsAvailableCompleteAndEmpty()
    {
        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            [new RepositoryFile("src/Program.cs", false, 40, content: null)]);

        Assert.Empty(result.Declarations.Items);
        Assert.True(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_WhenFileTreeIsTruncated_IsIncompleteRatherThanEmpty()
    {
        const string content = "{\"dependencies\":{\"express\":\"^4.18.2\"}}";

        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            new RepositoryCollection<RepositoryFile>(
                [Manifest("package.json", content.Length)],
                isComplete: false),
            ContentResponse("package.json", content));

        Assert.Single(result.Declarations.Items);
        Assert.False(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_WhenCandidateExceedsThePerFileBound_SkipsItAndStaysAvailable()
    {
        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
        [
            Manifest(
                "package.json",
                GitHubRepositoryDependencyManifestCollector.MaximumManifestFileBytes + 1)
        ]);

        Assert.Empty(result.Declarations.Items);
        Assert.False(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_WhenCandidateSizeIsUnknown_SkipsItAndStaysAvailable()
    {
        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            [new RepositoryFile("package.json", false, size: null, content: null)]);

        Assert.Empty(result.Declarations.Items);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CollectAsync_BeyondTheManifestCountBound_StopsDeterministicallyInOrdinalOrder()
    {
        const string content = "{\"dependencies\":{\"express\":\"1.0.0\"}}";
        RepositoryFile[] candidates = Enumerable
            .Range(0, GitHubRepositoryDependencyManifestCollector.MaximumManifestFiles + 4)
            .Select(index => Manifest($"packages/p{index:D2}/package.json", content.Length))
            .Reverse()
            .ToArray();
        HttpResponseMessage[] responses = candidates
            .Select(file => file.Path)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Take(GitHubRepositoryDependencyManifestCollector.MaximumManifestFiles)
            .Select(path => ContentResponse(path, content))
            .ToArray();

        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            candidates,
            responses);

        Assert.False(result.IsComplete);
        Assert.Equal(
            GitHubRepositoryDependencyManifestCollector.MaximumManifestFiles,
            result.Declarations.Items.Count);
        Assert.Equal(
            "packages/p00/package.json",
            result.Declarations.Items[0].ManifestPath);
    }

    [Fact]
    public async Task CollectAsync_WhenContentRequestFails_KeepsTheFailureAvailability()
    {
        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            [Manifest("package.json", 30)],
            new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        Assert.Empty(result.Declarations.Items);
        Assert.False(result.IsComplete);
        Assert.Equal(DataAvailabilityStatus.Forbidden, result.Availability.Status);
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
                DataAvailabilityStatus.Failed,
                "GitHub file tree request failed."),
            DataAvailability.Unavailable("project-files", DataAvailabilityStatus.NotRequested, "n/a"),
            DataAvailability.Unavailable("workflows", DataAvailabilityStatus.NotRequested, "n/a"),
            DataAvailability.Unavailable("documentation-markdown", DataAvailabilityStatus.NotRequested, "n/a"),
            Array.Empty<string>());

        GitHubRepositoryDependencyManifestCollection result =
            await CreateCollector([]).CollectAsync(RepositoryUrl, contents);

        Assert.Empty(result.Declarations.Items);
        Assert.False(result.IsComplete);
        Assert.Equal(
            DataAvailabilityStatus.NotRequested,
            result.Availability.Status);
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
            DataAvailability.Available("file-tree", DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
            DataAvailability.Available("project-files", DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
            DataAvailability.Available("workflows", DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
            DataAvailability.Available("documentation-markdown", DateTimeOffset.Parse("2026-08-15T09:30:00Z")),
            Array.Empty<string>());

        GitHubRepositoryDependencyManifestCollection result =
            await CreateCollector([]).CollectAsync(RepositoryUrl, contents);

        Assert.Empty(result.Declarations.Items);
        Assert.True(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_WhenManifestContentIsUnparseable_ProducesNoDeclaration()
    {
        const string content = "{ not json";

        GitHubRepositoryDependencyManifestCollection result = await CollectAsync(
            [Manifest("package.json", content.Length)],
            ContentResponse("package.json", content));

        Assert.Empty(result.Declarations.Items);
        Assert.True(result.IsComplete);
        Assert.True(result.Availability.IsAvailable);
    }

    [Fact]
    public async Task CollectAsync_PinsManifestContentToTheCollectedHeadCommit()
    {
        const string content = "{\"dependencies\":{\"express\":\"1.0.0\"}}";
        QueueHttpMessageHandler handler = new(ContentResponse("package.json", content));

        await new GitHubRepositoryDependencyManifestCollector(
                CreateClient(handler),
                CreateFixedTimeProvider())
            .CollectAsync(
                RepositoryUrl,
                CreateContents([Manifest("package.json", content.Length)]));

        Assert.Contains("ref=head-123", Assert.Single(handler.RequestUris).Query);
    }

    [Fact]
    public async Task CollectAsync_WhenContentWasAlreadyCollected_ReusesItWithoutAnotherRequest()
    {
        const string content = "<Project><ItemGroup><PackageReference Include=\"Serilog\" Version=\"3.1.1\" /></ItemGroup></Project>";
        QueueHttpMessageHandler handler = new();
        DateTimeOffset retrievedAt = DateTimeOffset.Parse("2026-08-15T09:30:00Z");
        GitHubRepositoryContentsCollection contents = new(
            "head-123",
            new RepositoryCollection<RepositoryFile>(
                [Manifest("src/App.csproj", content.Length)]),
            new RepositoryCollection<RepositoryFile>(
                [new RepositoryFile("src/App.csproj", false, content.Length, content)]),
            RepositoryCollection<RepositoryFile>.Empty,
            RepositoryCollection<RepositoryFile>.Empty,
            DataAvailability.Available("file-tree", retrievedAt),
            DataAvailability.Available("project-files", retrievedAt),
            DataAvailability.Available("workflows", retrievedAt),
            DataAvailability.Available("documentation-markdown", retrievedAt),
            Array.Empty<string>());

        GitHubRepositoryDependencyManifestCollection result =
            await new GitHubRepositoryDependencyManifestCollector(
                    CreateClient(handler),
                    CreateFixedTimeProvider())
                .CollectAsync(RepositoryUrl, contents);

        Assert.Empty(handler.RequestUris);
        Assert.Equal("Serilog", Assert.Single(result.Declarations.Items).PackageId);
        Assert.True(result.IsComplete);
    }

    private static Task<GitHubRepositoryDependencyManifestCollection> CollectAsync(
        IReadOnlyList<RepositoryFile> files,
        params HttpResponseMessage[] responses) =>
        CreateCollector(responses).CollectAsync(
            RepositoryUrl,
            CreateContents(files));

    private static Task<GitHubRepositoryDependencyManifestCollection> CollectAsync(
        RepositoryCollection<RepositoryFile> fileTree,
        params HttpResponseMessage[] responses) =>
        CreateCollector(responses).CollectAsync(
            RepositoryUrl,
            CreateContents(fileTree));

    private static GitHubRepositoryDependencyManifestCollector CreateCollector(
        HttpResponseMessage[] responses) =>
        new(
            CreateClient(new QueueHttpMessageHandler(responses)),
            CreateFixedTimeProvider());

    private static GitHubRepositoryContentsCollection CreateContents(
        IReadOnlyList<RepositoryFile> files) =>
        CreateContents(new RepositoryCollection<RepositoryFile>(files));

    private static GitHubRepositoryContentsCollection CreateContents(
        RepositoryCollection<RepositoryFile> fileTree)
    {
        DateTimeOffset retrievedAt = DateTimeOffset.Parse("2026-08-15T09:30:00Z");

        return new GitHubRepositoryContentsCollection(
            "head-123",
            fileTree,
            RepositoryCollection<RepositoryFile>.Empty,
            RepositoryCollection<RepositoryFile>.Empty,
            RepositoryCollection<RepositoryFile>.Empty,
            DataAvailability.Available("file-tree", retrievedAt),
            DataAvailability.Available("project-files", retrievedAt),
            DataAvailability.Available("workflows", retrievedAt),
            DataAvailability.Available("documentation-markdown", retrievedAt),
            Array.Empty<string>());
    }

    private static RepositoryFile Manifest(string path, int size) =>
        new(path, isDirectory: false, size, content: null);

    private static HttpResponseMessage ContentResponse(string path, string content) =>
        JsonResponse(JsonSerializer.Serialize(new
        {
            type = "file",
            path,
            encoding = "base64",
            content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content))
        }));
}

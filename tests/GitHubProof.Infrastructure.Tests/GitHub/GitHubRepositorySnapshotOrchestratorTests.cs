using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositorySnapshotOrchestratorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:30:00Z");

    [Fact]
    public async Task CollectAsync_MapsAllSectionsInDeterministicMetadataFirstOrder()
    {
        StubCollectors collectors = new();
        GitHubRepositorySnapshotOrchestrator orchestrator = CreateOrchestrator(collectors);

        GitHubRepositorySnapshotResult result = await orchestrator.CollectAsync(
            " https://github.com/octocat/hello-world ");

        RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
        Assert.True(result.IsSuccessful);
        Assert.Equal(DataAvailabilityStatus.Available, result.MetadataAvailability.Status);
        Assert.Equal(
            [
                "metadata",
                "contents",
                "descriptive",
                "commits",
                "issues",
                "pull-requests",
                "releases",
                "tags",
                "contributors"
            ],
            collectors.Calls);

        Assert.Equal("octocat", collectors.Repository!.Owner);
        Assert.Equal("hello-world", collectors.Repository.Name);
        Assert.Equal("octocat", snapshot.Repository.Owner);
        Assert.Equal("hello-world", snapshot.Repository.Name);
        Assert.Equal(new Uri("https://github.com/octocat/hello-world"), snapshot.Repository.Url);
        Assert.Equal("main", snapshot.Repository.DefaultBranch);
        Assert.True(snapshot.Repository.IsPublic);
        Assert.True(snapshot.Repository.IsArchived);
        Assert.True(snapshot.Repository.IsFork);
        Assert.False(snapshot.Repository.IsTemplate);
        Assert.Equal("C#", snapshot.Repository.PrimaryLanguage);
        Assert.Equal(CapturedAt, snapshot.CapturedAt);
        Assert.Equal("head-123", snapshot.HeadCommitSha);

        Assert.Equal("Repository description", snapshot.Metadata!.Description);
        Assert.Equal("README.md", snapshot.Readme!.Path);
        Assert.Equal("MIT", snapshot.License!.SpdxIdentifier);
        Assert.Equal("LICENSE", snapshot.LicenseFile!.Path);
        Assert.Equal(7, snapshot.LicenseFile.Size);
        Assert.Equal(1200, snapshot.Languages["C#"]);
        Assert.Equal(["dotnet", "github-api"], snapshot.Topics);

        RepositoryFile projectFile = Assert.Single(
            snapshot.FileTree.Items,
            file => file.Path == "src/App.csproj");
        Assert.Equal("<Project />", projectFile.Content);
        Assert.Equal(".github/workflows/ci.yml", snapshot.Workflows.Items[0].Path);
        Assert.Contains("dotnet test", snapshot.Workflows.Items[0].Content);

        Assert.Equal("commit-1", Assert.Single(snapshot.Commits.Items).Sha);
        Assert.Equal("Issue title", Assert.Single(snapshot.Issues.Items).Title);
        Assert.Equal("Issue body", snapshot.Issues.Items[0].Body);
        Assert.Equal("Pull request title", Assert.Single(snapshot.PullRequests.Items).Title);
        Assert.True(snapshot.PullRequests.Items[0].IsMerged);
        Assert.Equal("v1.0.0", Assert.Single(snapshot.Releases.Items).TagName);
        Assert.Equal("v1.0.0", Assert.Single(snapshot.Tags.Items).Name);
        Assert.Equal("octocat", Assert.Single(snapshot.Contributors.Items).Login);

        Assert.Equal(13, snapshot.Availability.Count);
        Assert.All(snapshot.Availability, availability =>
            Assert.Equal(DataAvailabilityStatus.Available, availability.Status));
        Assert.Single(snapshot.CollectionWarnings);
        Assert.Contains("may not be atomic", snapshot.CollectionWarnings[0]);
    }

    [Fact]
    public async Task CollectAsync_PreservesUnrecognizedLicenseFileWithoutLicenseMetadata()
    {
        StubCollectors collectors = new();
        RepositoryFile licenseFile = new(
            "LICENSE.custom",
            isDirectory: false,
            size: 42,
            content: "custom license text");
        collectors.Descriptive = collectors.Descriptive with
        {
            License = null,
            LicenseFile = licenseFile
        };

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");

        RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
        RepositoryFile actualLicenseFile = Assert.IsType<RepositoryFile>(snapshot.LicenseFile);

        Assert.Null(snapshot.License);
        Assert.Same(licenseFile, actualLicenseFile);
        Assert.Equal("LICENSE.custom", actualLicenseFile.Path);
        Assert.Equal(42, actualLicenseFile.Size);
        Assert.Equal("custom license text", actualLicenseFile.Content);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("license")!.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesNoLicenseEvidenceAfterSuccessfulInspection()
    {
        StubCollectors collectors = new();
        collectors.Descriptive = collectors.Descriptive with
        {
            License = null,
            LicenseFile = null
        };

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");

        RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
        Assert.Null(snapshot.License);
        Assert.Null(snapshot.LicenseFile);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("license")!.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesUnavailableLicenseStateWithoutEvidence()
    {
        StubCollectors collectors = new();
        collectors.Descriptive = new GitHubRepositoryDescriptiveDataCollection(
            collectors.Descriptive.Readme,
            License: null,
            LicenseFile: null,
            collectors.Descriptive.Languages,
            [
                Available("readme"),
                Unavailable("license", DataAvailabilityStatus.Forbidden, "License unavailable."),
                Available("languages")
            ]);

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");

        RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
        Assert.Null(snapshot.License);
        Assert.Null(snapshot.LicenseFile);
        Assert.Equal(
            DataAvailabilityStatus.Forbidden,
            snapshot.FindAvailability("license")!.Status);
    }

    [Fact]
    public async Task CollectAsync_PreservesPartialDataAvailabilityAndDeduplicatedWarnings()
    {
        StubCollectors collectors = new();
        collectors.Contents = collectors.Contents with
        {
            FileTree = new RepositoryCollection<RepositoryFile>(
                collectors.Contents.FileTree.Items,
                isComplete: false),
            Workflows = new RepositoryCollection<RepositoryFile>(
                collectors.Contents.Workflows.Items,
                isComplete: false),
            WorkflowsAvailability = Unavailable(
                "workflows",
                DataAvailabilityStatus.Failed,
                "One workflow could not be read."),
            CollectionWarnings = ["Tree truncated.", "Workflow content partial."]
        };
        collectors.Commits = new GitHubRepositoryCommitCollection(
            collectors.Commits.Commits,
            Unavailable("commits", DataAvailabilityStatus.RateLimited, "Rate limited."),
            ["Commit pagination incomplete."]);
        collectors.Issues = new GitHubRepositoryIssueCollection(
            new RepositoryCollection<GitHubRepositoryIssue>(),
            Available("issues"),
            Array.Empty<string>());
        collectors.Releases = new GitHubRepositoryReleaseCollection(
            new RepositoryCollection<RepositoryRelease>(),
            Available("releases"),
            Array.Empty<string>());
        collectors.Tags = new GitHubRepositoryTagCollection(
            collectors.Tags.Tags,
            Unavailable("tags", DataAvailabilityStatus.RateLimited, "Rate limited."),
            ["Commit pagination incomplete."]);
        collectors.Contributors = new GitHubRepositoryContributorCollection(
            new RepositoryCollection<RepositoryContributor>(isComplete: false),
            Unavailable("contributors", DataAvailabilityStatus.Failed, "Request failed."),
            ["Contributor collection incomplete."]);

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");

        RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
        Assert.Equal("commit-1", Assert.Single(snapshot.Commits.Items).Sha);
        Assert.Empty(snapshot.Issues.Items);
        Assert.Equal(0, snapshot.Issues.TotalCount);
        Assert.False(snapshot.FileTree.IsComplete);
        Assert.False(snapshot.Workflows.IsComplete);
        Assert.Contains("dotnet test", Assert.Single(snapshot.Workflows.Items).Content);
        Assert.Empty(snapshot.Releases.Items);
        Assert.Equal(DataAvailabilityStatus.Available, snapshot.FindAvailability("releases")!.Status);
        Assert.Equal("v1.0.0", Assert.Single(snapshot.Tags.Items).Name);
        Assert.Equal(DataAvailabilityStatus.RateLimited, snapshot.FindAvailability("tags")!.Status);
        Assert.Equal(DataAvailabilityStatus.RateLimited, snapshot.FindAvailability("commits")!.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, snapshot.FindAvailability("workflows")!.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, snapshot.FindAvailability("contributors")!.Status);
        Assert.Equal(5, snapshot.CollectionWarnings.Count);
        Assert.Single(snapshot.CollectionWarnings, warning =>
            warning == "Commit pagination incomplete.");
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Failed)]
    [InlineData(DataAvailabilityStatus.NotApplicable)]
    public async Task CollectAsync_MetadataFailureDoesNotCreateSnapshotOrInvokeOtherCollectors(
        DataAvailabilityStatus status)
    {
        StubCollectors collectors = new()
        {
            Metadata = new GitHubRepositoryMetadataCollection(
                Repository: null,
                Metadata: null,
                Topics: Array.Empty<string>(),
                License: null,
                Unavailable("metadata", status, "Metadata unavailable."))
        };

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");

        Assert.False(result.IsSuccessful);
        Assert.Null(result.Snapshot);
        Assert.Equal(status, result.MetadataAvailability.Status);
        Assert.Equal(["metadata"], collectors.Calls);
    }

    private static GitHubRepositorySnapshotOrchestrator CreateOrchestrator(
        StubCollectors collectors) =>
        new(
            collectors,
            collectors,
            collectors,
            collectors,
            collectors,
            collectors,
            collectors,
            collectors,
            collectors,
            new FixedTimeProvider(CapturedAt));

    private static DataAvailability Available(string dataKey) =>
        DataAvailability.Available(dataKey, CapturedAt);

    private static DataAvailability Unavailable(
        string dataKey,
        DataAvailabilityStatus status,
        string reason) =>
        DataAvailability.Unavailable(dataKey, status, reason);

    private sealed class StubCollectors :
        IGitHubRepositoryMetadataCollector,
        IGitHubRepositoryContentsCollector,
        IGitHubRepositoryDescriptiveDataCollector,
        IGitHubRepositoryCommitCollector,
        IGitHubRepositoryIssueCollector,
        IGitHubRepositoryPullRequestCollector,
        IGitHubRepositoryReleaseCollector,
        IGitHubRepositoryTagCollector,
        IGitHubRepositoryContributorCollector
    {
        public StubCollectors()
        {
            RepositoryIdentity identity = new(
                "octocat",
                "hello-world",
                new Uri("https://github.com/octocat/hello-world"),
                "main",
                isPublic: true,
                isArchived: true,
                isFork: true,
                isTemplate: false,
                primaryLanguage: "C#");
            Metadata = new GitHubRepositoryMetadataCollection(
                identity,
                new RepositoryMetadata(
                    "Repository description",
                    "https://example.test",
                    DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
                    issuesEnabled: true),
                ["dotnet", "github-api"],
                new RepositoryLicense("mit", "MIT License", "MIT", path: null),
                Available("metadata"));
            Contents = new GitHubRepositoryContentsCollection(
                "head-123",
                new RepositoryCollection<RepositoryFile>(
                [
                    new RepositoryFile("src/App.csproj", false, 11, content: null),
                    new RepositoryFile("LICENSE", false, 7, content: null),
                    new RepositoryFile(".github/workflows/ci.yml", false, 20, content: null)
                ]),
                new RepositoryCollection<RepositoryFile>(
                    [new RepositoryFile("src/App.csproj", false, 11, "<Project />")]),
                new RepositoryCollection<RepositoryFile>(
                    [new RepositoryFile(".github/workflows/ci.yml", false, 20, "run: dotnet test")]),
                Available("file-tree"),
                Available("project-files"),
                Available("workflows"),
                Array.Empty<string>());
            Descriptive = new GitHubRepositoryDescriptiveDataCollection(
                new RepositoryReadme("README.md", "# Hello"),
                new RepositoryLicense("mit", "MIT License", "MIT", "LICENSE"),
                new RepositoryFile("LICENSE", false, 7, content: null),
                new Dictionary<string, long> { ["C#"] = 1200 },
                [Available("readme"), Available("license"), Available("languages")]);
            Commits = new GitHubRepositoryCommitCollection(
                new RepositoryCollection<RepositoryCommit>(
                    [new RepositoryCommit("commit-1", CapturedAt, "Initial", "octocat")]),
                Available("commits"),
                Array.Empty<string>());
            Issues = new GitHubRepositoryIssueCollection(
                new RepositoryCollection<GitHubRepositoryIssue>(
                    [new GitHubRepositoryIssue(1, "Issue title", "Issue body", false, CapturedAt)]),
                Available("issues"),
                Array.Empty<string>());
            PullRequests = new GitHubRepositoryPullRequestCollection(
                new RepositoryCollection<GitHubRepositoryPullRequest>(
                [
                    new GitHubRepositoryPullRequest(
                        2,
                        "Pull request title",
                        isClosed: true,
                        isMerged: true,
                        CapturedAt)
                ]),
                Available("pull-requests"),
                Array.Empty<string>());
            Releases = new GitHubRepositoryReleaseCollection(
                new RepositoryCollection<RepositoryRelease>(
                [
                    new RepositoryRelease(
                        "v1.0.0",
                        hasPublishedRelease: true,
                        isDraft: false,
                        "Version 1",
                        "Release notes",
                        CapturedAt)
                ]),
                Available("releases"),
                Array.Empty<string>());
            Tags = new GitHubRepositoryTagCollection(
                new RepositoryCollection<GitHubRepositoryTag>(
                    [new GitHubRepositoryTag("v1.0.0")]),
                Available("tags"),
                Array.Empty<string>());
            Contributors = new GitHubRepositoryContributorCollection(
                new RepositoryCollection<RepositoryContributor>(
                    [new RepositoryContributor("octocat", 12)]),
                Available("contributors"),
                Array.Empty<string>());
        }

        public List<string> Calls { get; } = [];

        public GitHubRepositoryUrl? Repository { get; private set; }

        public GitHubRepositoryMetadataCollection Metadata { get; set; }

        public GitHubRepositoryContentsCollection Contents { get; set; }

        public GitHubRepositoryDescriptiveDataCollection Descriptive { get; set; }

        public GitHubRepositoryCommitCollection Commits { get; set; }

        public GitHubRepositoryIssueCollection Issues { get; set; }

        public GitHubRepositoryPullRequestCollection PullRequests { get; set; }

        public GitHubRepositoryReleaseCollection Releases { get; set; }

        public GitHubRepositoryTagCollection Tags { get; set; }

        public GitHubRepositoryContributorCollection Contributors { get; set; }

        Task<GitHubRepositoryMetadataCollection> IGitHubRepositoryMetadataCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            CancellationToken cancellationToken)
        {
            Record("metadata", repository);
            return Task.FromResult(Metadata);
        }

        Task<GitHubRepositoryContentsCollection> IGitHubRepositoryContentsCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("contents", repository);
            return Task.FromResult(Contents);
        }

        Task<GitHubRepositoryDescriptiveDataCollection> IGitHubRepositoryDescriptiveDataCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("descriptive", repository);
            return Task.FromResult(Descriptive);
        }

        Task<GitHubRepositoryCommitCollection> IGitHubRepositoryCommitCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("commits", repository);
            return Task.FromResult(Commits);
        }

        Task<GitHubRepositoryIssueCollection> IGitHubRepositoryIssueCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("issues", repository);
            return Task.FromResult(Issues);
        }

        Task<GitHubRepositoryPullRequestCollection> IGitHubRepositoryPullRequestCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("pull-requests", repository);
            return Task.FromResult(PullRequests);
        }

        Task<GitHubRepositoryReleaseCollection> IGitHubRepositoryReleaseCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("releases", repository);
            return Task.FromResult(Releases);
        }

        Task<GitHubRepositoryTagCollection> IGitHubRepositoryTagCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("tags", repository);
            return Task.FromResult(Tags);
        }

        Task<GitHubRepositoryContributorCollection> IGitHubRepositoryContributorCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            CancellationToken cancellationToken)
        {
            Record("contributors", repository);
            return Task.FromResult(Contributors);
        }

        private void Record(string call, GitHubRepositoryUrl repository)
        {
            Calls.Add(call);
            Repository ??= repository;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

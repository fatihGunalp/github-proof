using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositorySnapshotOrchestratorTests
{
    private const string HistoricalCommitSha =
        "c3eea5bec401935206d4697d0c73af950514c1ac";
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
                "contributors",
                "dependency-manifests",
                "source-files"
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
        RepositoryFile documentationFile = Assert.Single(snapshot.DocumentationMarkdown.Items);
        Assert.Equal("docs/setup.md", documentationFile.Path);
        Assert.Equal(18, documentationFile.Size);
        Assert.Equal("# Installation", documentationFile.Content);
        Assert.True(snapshot.DocumentationMarkdown.IsComplete);

        Assert.Equal("commit-1", Assert.Single(snapshot.Commits.Items).Sha);
        Assert.Equal("Issue title", Assert.Single(snapshot.Issues.Items).Title);
        Assert.Equal("Issue body", snapshot.Issues.Items[0].Body);
        Assert.Equal("Pull request title", Assert.Single(snapshot.PullRequests.Items).Title);
        Assert.True(snapshot.PullRequests.Items[0].IsMerged);
        Assert.Equal("v1.0.0", Assert.Single(snapshot.Releases.Items).TagName);
        Assert.Equal("v1.0.0", Assert.Single(snapshot.Tags.Items).Name);
        Assert.Equal("octocat", Assert.Single(snapshot.Contributors.Items).Login);

        Assert.Equal(16, snapshot.Availability.Count);
        Assert.All(snapshot.Availability, availability =>
            Assert.Equal(DataAvailabilityStatus.Available, availability.Status));
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("documentation-markdown")!.Status);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("dependency-manifests")!.Status);
        Assert.Equal(
            "Serilog",
            Assert.Single(snapshot.DependencyDeclarations.Items).PackageId);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            snapshot.FindAvailability("source-files")!.Status);
        Assert.Equal(
            "src/Program.cs",
            Assert.Single(snapshot.SourceFiles.Items).Path);
        Assert.Single(snapshot.CollectionWarnings);
        Assert.Contains("may not be atomic", snapshot.CollectionWarnings[0]);
        Assert.Null(collectors.ContentsReference);
        Assert.Null(collectors.DescriptiveReference);
    }

    [Fact]
    public async Task CollectAtCommitAsync_PinsContentCollectorsAndKeepsLiveSectionsUnchanged()
    {
        StubCollectors collectors = new();
        collectors.Contents = collectors.Contents with
        {
            HeadCommitSha = HistoricalCommitSha
        };

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAtCommitAsync(
                "https://github.com/octocat/hello-world",
                HistoricalCommitSha);

        RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);

        Assert.Equal(HistoricalCommitSha, snapshot.HeadCommitSha);
        Assert.Equal(HistoricalCommitSha, collectors.ContentsReference!.CommitSha);
        Assert.Equal(HistoricalCommitSha, collectors.DescriptiveReference!.CommitSha);
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
                "contributors",
                "dependency-manifests",
                "source-files"
            ],
            collectors.Calls);
        Assert.Contains(
            snapshot.CollectionWarnings,
            warning => warning.Contains("requested commit SHA", StringComparison.Ordinal)
                && warning.Contains("live GitHub state", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CollectAtCommitAsync_FailsFastWhenExplicitCommitCannotBeCollected()
    {
        StubCollectors collectors = new();
        collectors.Contents = collectors.Contents with
        {
            HeadCommitSha = null,
            FileTreeAvailability = Unavailable(
                "file-tree",
                DataAvailabilityStatus.Unavailable,
                "Commit not found.")
        };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateOrchestrator(collectors).CollectAtCommitAsync(
                "https://github.com/octocat/hello-world",
                HistoricalCommitSha));

        Assert.Contains("will not fall back", exception.Message, StringComparison.Ordinal);
        Assert.Equal(["metadata", "contents"], collectors.Calls);
        Assert.Null(collectors.DescriptiveReference);
    }

    [Fact]
    public async Task CollectAtCommitAsync_RejectsInvalidCommitBeforeCallingGitHubCollectors()
    {
        StubCollectors collectors = new();

        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateOrchestrator(collectors).CollectAtCommitAsync(
                "https://github.com/octocat/hello-world",
                "not-a-full-commit-sha"));

        Assert.Empty(collectors.Calls);
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
            DocumentationMarkdownFiles = new RepositoryCollection<RepositoryFile>(
                collectors.Contents.DocumentationMarkdownFiles.Items,
                isComplete: false),
            WorkflowsAvailability = Unavailable(
                "workflows",
                DataAvailabilityStatus.Failed,
                "One workflow could not be read."),
            DocumentationMarkdownAvailability = Unavailable(
                "documentation-markdown",
                DataAvailabilityStatus.RateLimited,
                "Documentation content was rate limited."),
            CollectionWarnings =
            [
                "Tree truncated.",
                "Workflow content partial.",
                "Documentation content partial."
            ]
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
        Assert.False(snapshot.DocumentationMarkdown.IsComplete);
        Assert.Equal(
            "# Installation",
            Assert.Single(snapshot.DocumentationMarkdown.Items).Content);
        Assert.Empty(snapshot.Releases.Items);
        Assert.Equal(DataAvailabilityStatus.Available, snapshot.FindAvailability("releases")!.Status);
        Assert.Equal("v1.0.0", Assert.Single(snapshot.Tags.Items).Name);
        Assert.Equal(DataAvailabilityStatus.RateLimited, snapshot.FindAvailability("tags")!.Status);
        Assert.Equal(DataAvailabilityStatus.RateLimited, snapshot.FindAvailability("commits")!.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, snapshot.FindAvailability("workflows")!.Status);
        Assert.Equal(
            DataAvailabilityStatus.RateLimited,
            snapshot.FindAvailability("documentation-markdown")!.Status);
        Assert.Equal(DataAvailabilityStatus.Failed, snapshot.FindAvailability("contributors")!.Status);
        Assert.Equal(6, snapshot.CollectionWarnings.Count);
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

    [Fact]
    public async Task CollectAsync_DependencyEvidence_LeavesFileTreeAndCollectionWarningsUnchanged()
    {
        StubCollectors collectors = new()
        {
            DependencyManifests = new GitHubRepositoryDependencyManifestCollection(
                new RepositoryCollection<RepositoryDependencyDeclaration>(isComplete: false),
                Unavailable(
                    "dependency-manifests",
                    DataAvailabilityStatus.Failed,
                    "GitHub dependency manifest request failed."))
        };

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");
        RepositorySnapshot snapshot = result.Snapshot!;

        Assert.Equal(
            collectors.Contents.FileTree.Items.Select(file => file.Path),
            snapshot.FileTree.Items.Select(file => file.Path));
        Assert.Equal(
            "<Project />",
            snapshot.FileTree.Items.Single(file => file.Path == "src/App.csproj").Content);
        Assert.Single(snapshot.CollectionWarnings);
        Assert.DoesNotContain(
            snapshot.CollectionWarnings,
            warning => warning.Contains("dependency", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(snapshot.DependencyDeclarations.Items);
        Assert.False(snapshot.DependencyDeclarations.IsComplete);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            snapshot.FindAvailability("dependency-manifests")!.Status);
    }

    [Fact]
    public async Task CollectAsync_SourceEvidence_ContributesAvailabilityOnly()
    {
        StubCollectors collectors = new()
        {
            SourceFiles = new GitHubRepositorySourceFileCollection(
                new RepositoryCollection<RepositorySourceFile>(
                    [new RepositorySourceFile("src/Program.cs", "SOURCE-CONTENT")],
                    isComplete: false),
                Unavailable(
                    "source-files",
                    DataAvailabilityStatus.Failed,
                    "GitHub source content request failed."))
        };

        GitHubRepositorySnapshotResult result = await CreateOrchestrator(collectors)
            .CollectAsync("https://github.com/octocat/hello-world");
        RepositorySnapshot snapshot = result.Snapshot!;

        // The file tree keeps exactly the paths and values the contents
        // collector produced; source content is never overlaid into it.
        Assert.Equal(
            collectors.Contents.FileTree.Items.Select(file => file.Path),
            snapshot.FileTree.Items.Select(file => file.Path));
        Assert.Equal(
            "<Project />",
            snapshot.FileTree.Items.Single(file => file.Path == "src/App.csproj").Content);
        Assert.DoesNotContain(
            snapshot.FileTree.Items,
            file => file.Content == "SOURCE-CONTENT");

        // Source failures never reach the snapshot collection warnings, which
        // would turn RepositoryAnalysis.Status into Partial.
        Assert.Single(snapshot.CollectionWarnings);
        Assert.DoesNotContain(
            snapshot.CollectionWarnings,
            warning => warning.Contains("source", StringComparison.OrdinalIgnoreCase));

        // Partial positive artefacts are preserved beside the real failure.
        Assert.Equal(
            "src/Program.cs",
            Assert.Single(snapshot.SourceFiles.Items).Path);
        Assert.False(snapshot.SourceFiles.IsComplete);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            snapshot.FindAvailability("source-files")!.Status);
    }

    // public-profile-runtime-collection.md §5, §14.3: the Profile V1 composition
    // never reaches the omitted collectors at all, so no request attributable to
    // them can be issued, and it leaves the collection order of every required
    // surface unchanged. Session 099 added Contributors to that omission list.
    [Fact]
    public async Task ProfileComposition_NeverInvokesTheOmittedCollectors()
    {
        StubCollectors collectors = new();
        GitHubRepositorySnapshotOrchestrator orchestrator =
            CreateProfileOrchestrator(collectors);

        GitHubRepositorySnapshotResult result = await orchestrator.CollectAsync(
            "https://github.com/octocat/hello-world");

        Assert.True(result.IsSuccessful);
        Assert.Equal(
            [
                "metadata",
                "contents",
                "descriptive",
                "commits",
                "issues",
                "pull-requests",
                "releases",
                "tags"
            ],
            collectors.Calls);
    }

    [Fact]
    public async Task ProfileComposition_LeavesEveryOmittedSurfaceAbsent()
    {
        StubCollectors collectors = new();
        GitHubRepositorySnapshotResult result =
            await CreateProfileOrchestrator(collectors).CollectAsync(
                "https://github.com/octocat/hello-world");

        RepositorySnapshot snapshot = result.Snapshot!;

        Assert.Empty(snapshot.SourceFiles.Items);
        Assert.Empty(snapshot.DependencyDeclarations.Items);
        Assert.Empty(snapshot.Contributors.Items);
        Assert.Null(snapshot.FindAvailability("source-files"));
        Assert.Null(snapshot.FindAvailability("dependency-manifests"));
        Assert.Null(snapshot.FindAvailability("contributors"));
    }

    // Session 099 regression guard: the mapped surfaces must carry the
    // observation state through to the snapshot. Dropping it would silently
    // relabel a deliberate DecisionBounded stop as an Incomplete failure and
    // make the evaluators' lower-bound evidence unreachable.
    [Fact]
    public async Task DecisionBoundedCollections_KeepThatStateInTheSnapshot()
    {
        StubCollectors collectors = new();

        collectors.Commits = collectors.Commits with
        {
            Commits = RepositoryCollection<RepositoryCommit>.DecisionBounded(
                collectors.Commits.Commits.Items)
        };
        collectors.Issues = collectors.Issues with
        {
            Issues = RepositoryCollection<GitHubRepositoryIssue>.DecisionBounded(
                collectors.Issues.Issues.Items)
        };
        collectors.PullRequests = collectors.PullRequests with
        {
            PullRequests =
                RepositoryCollection<GitHubRepositoryPullRequest>.DecisionBounded(
                    collectors.PullRequests.PullRequests.Items)
        };
        collectors.Releases = collectors.Releases with
        {
            Releases = RepositoryCollection<RepositoryRelease>.DecisionBounded(
                collectors.Releases.Releases.Items)
        };
        collectors.Tags = collectors.Tags with
        {
            Tags = RepositoryCollection<GitHubRepositoryTag>.DecisionBounded(
                collectors.Tags.Tags.Items)
        };

        GitHubRepositorySnapshotResult result =
            await CreateOrchestrator(collectors).CollectAsync(
                "https://github.com/octocat/hello-world");

        RepositorySnapshot snapshot = result.Snapshot!;

        Assert.True(snapshot.Commits.IsDecisionBounded);
        Assert.True(snapshot.Issues.IsDecisionBounded);
        Assert.True(snapshot.PullRequests.IsDecisionBounded);
        Assert.True(snapshot.Releases.IsDecisionBounded);
        Assert.True(snapshot.Tags.IsDecisionBounded);

        // A deliberate bound is never a total and never a failure.
        Assert.Null(snapshot.Issues.TotalCount);
        Assert.Null(snapshot.PullRequests.TotalCount);
        Assert.Null(snapshot.Tags.TotalCount);
    }

    // An ordinary Incomplete collection must NOT be promoted to DecisionBounded
    // anywhere along the mapping path.
    [Fact]
    public async Task IncompleteCollections_AreNeverPromotedToDecisionBounded()
    {
        StubCollectors collectors = new();

        collectors.Issues = collectors.Issues with
        {
            Issues = new RepositoryCollection<GitHubRepositoryIssue>(
                collectors.Issues.Issues.Items,
                isComplete: false)
        };

        GitHubRepositorySnapshotResult result =
            await CreateOrchestrator(collectors).CollectAsync(
                "https://github.com/octocat/hello-world");

        Assert.Equal(
            CollectionObservationState.Incomplete,
            result.Snapshot!.Issues.ObservationState);
        Assert.False(result.Snapshot.Issues.IsDecisionBounded);
    }

    // The pinned-commit warning names the sections that were actually read from
    // live state. A composition that omits contributors must not claim they
    // were, and the omission itself stays neutral: no warning of its own.
    [Fact]
    public async Task ProfileComposition_PinnedCommitWarning_DoesNotClaimContributorsWereCollected()
    {
        StubCollectors collectors = new();
        collectors.Contents = collectors.Contents with
        {
            HeadCommitSha = HistoricalCommitSha
        };

        GitHubRepositorySnapshotResult result =
            await CreateProfileOrchestrator(collectors).CollectAtCommitAsync(
                "https://github.com/octocat/hello-world",
                HistoricalCommitSha);

        string warning = Assert.Single(
            result.Snapshot!.CollectionWarnings,
            text => text.Contains("requested commit SHA", StringComparison.Ordinal));

        Assert.DoesNotContain("contributors", warning, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "releases, and tags were collected from live GitHub state",
            warning,
            StringComparison.Ordinal);
    }

    // Repository Proof's wording is unchanged, contributors included.
    [Fact]
    public async Task FullComposition_PinnedCommitWarning_StillNamesContributors()
    {
        StubCollectors collectors = new();
        collectors.Contents = collectors.Contents with
        {
            HeadCommitSha = HistoricalCommitSha
        };

        GitHubRepositorySnapshotResult result =
            await CreateOrchestrator(collectors).CollectAtCommitAsync(
                "https://github.com/octocat/hello-world",
                HistoricalCommitSha);

        string warning = Assert.Single(
            result.Snapshot!.CollectionWarnings,
            text => text.Contains("requested commit SHA", StringComparison.Ordinal));

        Assert.Contains(
            "releases, tags, and contributors were collected from live GitHub state",
            warning,
            StringComparison.Ordinal);
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
            collectors,
            collectors,
            new FixedTimeProvider(CapturedAt));

    // Session 099: the Profile composition constructor now takes eight
    // collectors — Contributors joined the omitted surfaces.
    private static GitHubRepositorySnapshotOrchestrator CreateProfileOrchestrator(
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
        IGitHubRepositoryContributorCollector,
        IGitHubRepositoryDependencyManifestCollector,
        IGitHubRepositorySourceFileCollector
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
                    new RepositoryFile(".github/workflows/ci.yml", false, 20, content: null),
                    new RepositoryFile("docs/setup.md", false, 18, content: null)
                ]),
                new RepositoryCollection<RepositoryFile>(
                    [new RepositoryFile("src/App.csproj", false, 11, "<Project />")]),
                new RepositoryCollection<RepositoryFile>(
                    [new RepositoryFile(".github/workflows/ci.yml", false, 20, "run: dotnet test")]),
                new RepositoryCollection<RepositoryFile>(
                    [new RepositoryFile("docs/setup.md", false, 18, "# Installation")]),
                Available("file-tree"),
                Available("project-files"),
                Available("workflows"),
                Available("documentation-markdown"),
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
            DependencyManifests = new GitHubRepositoryDependencyManifestCollection(
                new RepositoryCollection<RepositoryDependencyDeclaration>(
                [
                    new RepositoryDependencyDeclaration(
                        DependencyEcosystem.DotNet,
                        "src/App.csproj",
                        "Serilog",
                        "3.1.1")
                ]),
                Available("dependency-manifests"));
            SourceFiles = new GitHubRepositorySourceFileCollection(
                new RepositoryCollection<RepositorySourceFile>(
                    [new RepositorySourceFile("src/Program.cs", "class Program { }")]),
                Available("source-files"));
        }

        public List<string> Calls { get; } = [];

        public GitHubRepositoryUrl? Repository { get; private set; }

        public GitHubCommitReference? ContentsReference { get; private set; }

        public GitHubCommitReference? DescriptiveReference { get; private set; }

        public GitHubRepositoryMetadataCollection Metadata { get; set; }

        public GitHubRepositoryContentsCollection Contents { get; set; }

        public GitHubRepositoryDescriptiveDataCollection Descriptive { get; set; }

        public GitHubRepositoryCommitCollection Commits { get; set; }

        public GitHubRepositoryIssueCollection Issues { get; set; }

        public GitHubRepositoryPullRequestCollection PullRequests { get; set; }

        public GitHubRepositoryReleaseCollection Releases { get; set; }

        public GitHubRepositoryTagCollection Tags { get; set; }

        public GitHubRepositoryContributorCollection Contributors { get; set; }

        public GitHubRepositoryDependencyManifestCollection DependencyManifests { get; set; }

        public GitHubRepositorySourceFileCollection SourceFiles { get; set; }

        Task<GitHubRepositoryDependencyManifestCollection> IGitHubRepositoryDependencyManifestCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryContentsCollection contents,
            CancellationToken cancellationToken)
        {
            Record("dependency-manifests", repository);
            return Task.FromResult(DependencyManifests);
        }

        Task<GitHubRepositorySourceFileCollection> IGitHubRepositorySourceFileCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryContentsCollection contents,
            CancellationToken cancellationToken)
        {
            Record("source-files", repository);
            return Task.FromResult(SourceFiles);
        }

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
            GitHubCommitReference? contentReference,
            CancellationToken cancellationToken)
        {
            Record("contents", repository);
            ContentsReference = contentReference;
            return Task.FromResult(Contents);
        }

        Task<GitHubRepositoryDescriptiveDataCollection> IGitHubRepositoryDescriptiveDataCollector.CollectAsync(
            GitHubRepositoryUrl repository,
            GitHubRepositoryMetadataCollection metadata,
            GitHubCommitReference? contentReference,
            CancellationToken cancellationToken)
        {
            Record("descriptive", repository);
            DescriptiveReference = contentReference;
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

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class TestStructureSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset FileTreeRetrievedAt = CapturedAt.AddMinutes(1);
    private static readonly DateTimeOffset ProjectFilesRetrievedAt = CapturedAt.AddMinutes(2);
    private static readonly DateTimeOffset EvaluatedAt = CapturedAt.AddMinutes(3);

    [Fact]
    public void Definition_MatchesCanonicalTestStructureContract()
    {
        SignalDefinition definition = new TestStructureSignalEvaluator().Definition;

        Assert.Equal("validation.test-structure", definition.Key);
        Assert.Equal(AnalysisCategory.ValidationAndUsability, definition.Category);
        Assert.Equal(10, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["file-tree", "project-files"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WithNoEvidenceAndCompleteCorpus_ReturnsMissing()
    {
        SignalResult result = EvaluateAvailable(
            [File("src/Program.cs")]);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenRequiredEvidenceIsUnavailable_ReturnsUnavailableEvidence(
        DataAvailabilityStatus status)
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [File("tests/sample.spec.js")],
            fileTreeComplete: true,
            DataAvailability.Unavailable("file-tree", status, "Tree failed."),
            DataAvailability.Available("project-files", ProjectFilesRetrievedAt));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenFileTreeIsIncomplete_ReturnsUnavailableEvidenceWithoutTerminalInference()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [File("tests/Unit.Tests.csproj"), File("tests/sample.spec.js")],
            fileTreeComplete: false,
            DataAvailability.Available("file-tree", FileTreeRetrievedAt),
            DataAvailability.Available("project-files", ProjectFilesRetrievedAt));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenProjectFileEvidenceIsUnavailable_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [File("Unit.Tests.csproj")],
            fileTreeComplete: true,
            DataAvailability.Available("file-tree", FileTreeRetrievedAt),
            DataAvailability.Unavailable(
                "project-files",
                DataAvailabilityStatus.Failed,
                "Project content failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Theory]
    [InlineData("Unit.Tests.csproj")]
    [InlineData("Unit.Test.csproj")]
    public void Evaluate_WithExplicitTestProject_ReturnsPresentEight(string path)
    {
        SignalResult result = EvaluateAvailable([File(path, "<Project />")]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
        Assert.Null(result.Recommendation);
        Assert.Contains(result.Evidence, item =>
            item.Source == "TestProject" && item.Location == path);
    }

    [Theory]
    [InlineData("pytest.ini")]
    [InlineData("jest.config.js")]
    [InlineData("vitest.config.ts")]
    public void Evaluate_WithFrameworkConfig_ReturnsPresentEight(string path)
    {
        SignalResult result = EvaluateAvailable([File(path, "config")]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
        Assert.Contains(result.Evidence, item => item.Source == "TestFramework");
    }

    [Fact]
    public void Evaluate_WithPomTestDependency_ReturnsPresentEight()
    {
        const string pom = """
            <project>
              <dependencies>
                <dependency>
                  <groupId>org.junit.jupiter</groupId>
                  <artifactId>junit-jupiter</artifactId>
                  <scope>test</scope>
                </dependency>
              </dependencies>
            </project>
            """;

        SignalResult result = EvaluateAvailable([File("pom.xml", pom)]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Theory]
    [InlineData("build.gradle", "testImplementation 'org.junit.jupiter:junit-jupiter:5.10.0'")]
    [InlineData("build.gradle.kts", "testRuntimeOnly(\"org.junit.platform:junit-platform-launcher\")")]
    public void Evaluate_WithGradleTestDependency_ReturnsPresentEight(
        string path,
        string content)
    {
        SignalResult result = EvaluateAvailable([File(path, content)]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Theory]
    [InlineData("tests/helpers.cs")]
    [InlineData("test/helpers.py")]
    [InlineData("src/__tests__/helpers.js")]
    public void Evaluate_WithExactTestDirectoryOnly_ReturnsRequiresHumanReview(string path)
    {
        SignalResult result = EvaluateAvailable([File(path)]);

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.NotNull(result.Recommendation);
        Assert.Contains(result.Evidence, item => item.Source == "TestDirectory");
    }

    [Theory]
    [InlineData("sample.spec.js")]
    [InlineData("sample.test.ts")]
    public void Evaluate_WithLimitedReliableFileOnly_ReturnsPartialFour(string path)
    {
        SignalResult result = EvaluateAvailable([File(path)]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithOrganizedTestDirectoryAndReliableFile_ReturnsStrongTen()
    {
        SignalResult result = EvaluateAvailable([File("tests/sample.spec.js")]);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(10, result.Score);
    }

    [Fact]
    public void Evaluate_WithSpecAndTestFileTypes_ReturnsStrongTen()
    {
        SignalResult result = EvaluateAvailable(
        [
            File("specs/sample.spec.js"),
            File("checks/sample.test.js")
        ]);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(10, result.Score);
    }

    [Fact]
    public void Evaluate_WithTwoExplicitTestProjects_ReturnsStrongTen()
    {
        SignalResult result = EvaluateAvailable(
        [
            File("Unit.Tests.csproj", "<Project />"),
            File("Integration.Tests.csproj", "<Project />")
        ]);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(10, result.Score);
    }

    [Fact]
    public void Evaluate_IgnoresGenericFilenameKeywordFalsePositives()
    {
        SignalResult result = EvaluateAvailable(
        [
            File("src/contest.js"),
            File("src/latest.ts"),
            File("docs/testing-notes.md"),
            File("src/testimonial.cs")
        ]);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Evaluate_InputOrderDoesNotAffectResultAndTimeIsReadOnce()
    {
        RepositoryFile[] files =
        [
            File("tests/sample.spec.js"),
            File("tests/Unit.Tests.csproj", "<Project />")
        ];
        RepositorySnapshot firstSnapshot = CreateAvailableSnapshot(files);
        RepositorySnapshot secondSnapshot = CreateAvailableSnapshot(files.Reverse());
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new TestStructureSignalEvaluator(firstTimeProvider).Evaluate(firstSnapshot);
        SignalResult second = new TestStructureSignalEvaluator(secondTimeProvider).Evaluate(secondSnapshot);

        AssertEquivalent(first, second);
        Assert.Equal(1, firstTimeProvider.CallCount);
        Assert.Equal(1, secondTimeProvider.CallCount);
    }

    private static SignalResult EvaluateAvailable(IEnumerable<RepositoryFile> files) =>
        Evaluate(CreateAvailableSnapshot(files));

    private static SignalResult Evaluate(RepositorySnapshot snapshot) =>
        new TestStructureSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

    private static RepositorySnapshot CreateAvailableSnapshot(IEnumerable<RepositoryFile> files) =>
        CreateSnapshot(
            files,
            fileTreeComplete: true,
            DataAvailability.Available("file-tree", FileTreeRetrievedAt),
            DataAvailability.Available("project-files", ProjectFilesRetrievedAt));

    private static RepositoryFile File(string path, string? content = null) =>
        new(path, isDirectory: false, size: content?.Length, content);

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<RepositoryFile> files,
        bool fileTreeComplete,
        DataAvailability fileTreeAvailability,
        DataAvailability projectFilesAvailability)
    {
        return new RepositorySnapshot(
            new RepositoryIdentity(
                "owner",
                "repository",
                new Uri("https://github.com/owner/repository"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            new RepositoryMetadata(null, null, CapturedAt.AddDays(-30), issuesEnabled: true),
            readme: null,
            new RepositoryCollection<RepositoryFile>(files, isComplete: fileTreeComplete),
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            [fileTreeAvailability, projectFilesAvailability],
            collectionWarnings: null);
    }

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(first.Explanation, second.Explanation);
        Assert.Equal(first.Recommendation, second.Recommendation);
        Assert.Equal(first.Limitations, second.Limitations);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
    }

    private sealed class CountingTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public int CallCount { get; private set; }

        public override DateTimeOffset GetUtcNow()
        {
            CallCount++;
            return utcNow;
        }
    }
}

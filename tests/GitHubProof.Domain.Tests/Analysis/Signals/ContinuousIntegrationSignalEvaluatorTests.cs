using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class ContinuousIntegrationSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt = CapturedAt.AddMinutes(1);
    private static readonly DateTimeOffset EvaluatedAt = CapturedAt.AddMinutes(2);

    [Fact]
    public void Definition_MatchesCanonicalContinuousIntegrationContract()
    {
        SignalDefinition definition = new ContinuousIntegrationSignalEvaluator().Definition;

        Assert.Equal("validation.continuous-integration", definition.Key);
        Assert.Equal(AnalysisCategory.ValidationAndUsability, definition.Category);
        Assert.Equal(8, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["workflows"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WithZeroCompleteWorkflows_ReturnsMissing()
    {
        SignalResult result = EvaluateAvailable([]);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Evaluate_WithBuildAndTestCommands_ReturnsStrongEight()
    {
        SignalResult result = EvaluateAvailable(
        [
            Workflow(".github/workflows/ci.yml", """
                name: CI
                jobs:
                  validate:
                    steps:
                      - run: dotnet build --no-restore
                      - run: dotnet test --no-build
                """)
        ]);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
        Assert.Contains(result.Evidence, item => item.Source == "WorkflowBuild");
        Assert.Contains(result.Evidence, item => item.Source == "WorkflowTest");
    }

    [Fact]
    public void Evaluate_WithBuildAndTestAcrossSeparateWorkflows_ReturnsStrongEight()
    {
        SignalResult result = EvaluateAvailable(
        [
            Workflow(".github/workflows/build.yml", "jobs:\n  build:\n    steps:\n      - run: npm run build"),
            Workflow(".github/workflows/test.yml", "jobs:\n  test:\n    steps:\n      - run: npm test")
        ]);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void Evaluate_WithBuildOnly_ReturnsPresentSix()
    {
        SignalResult result = EvaluateAvailable(
            [Workflow(".github/workflows/build.yml", "jobs:\n  build:\n    steps:\n      - run: dotnet build")]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(6, result.Score);
    }

    [Fact]
    public void Evaluate_WithTestOnlyBlockScalar_ReturnsPresentSix()
    {
        SignalResult result = EvaluateAvailable(
        [
            Workflow(".github/workflows/test.yml", """
                jobs:
                  test:
                    steps:
                      - run: |
                          dotnet test --no-restore
                """)
        ]);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(6, result.Score);
    }

    [Fact]
    public void Evaluate_WithDeployOnly_ReturnsPartialThree()
    {
        SignalResult result = EvaluateAvailable(
            [Workflow(".github/workflows/deploy.yml", "jobs:\n  deploy:\n    steps:\n      - run: npm run deploy")]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(3, result.Score);
    }

    [Fact]
    public void Evaluate_WithWorkflowOfUnknownPurpose_ReturnsPartialTwo()
    {
        SignalResult result = EvaluateAvailable(
            [Workflow(".github/workflows/labels.yml", "jobs:\n  label:\n    steps:\n      - uses: actions/labeler@v6")]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
    }

    [Theory]
    [InlineData(DataAvailabilityStatus.Unavailable)]
    [InlineData(DataAvailabilityStatus.NotRequested)]
    [InlineData(DataAvailabilityStatus.RateLimited)]
    [InlineData(DataAvailabilityStatus.Forbidden)]
    [InlineData(DataAvailabilityStatus.Failed)]
    public void Evaluate_WhenWorkflowEvidenceIsUnavailable_ReturnsUnavailableEvidence(
        DataAvailabilityStatus status)
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Workflow(".github/workflows/ci.yml", "jobs:\n  build:\n    steps:\n      - run: dotnet build")],
            isComplete: false,
            DataAvailability.Unavailable("workflows", status, "Workflow collection failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenIncompleteCorpusHasBuildOnly_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Workflow(".github/workflows/ci.yml", "jobs:\n  build:\n    steps:\n      - run: dotnet build")],
            isComplete: false,
            DataAvailability.Available("workflows", RetrievedAt));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WhenIncompleteCorpusAlreadyHasBuildAndTest_PreservesTerminalStrong()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            [Workflow(".github/workflows/ci.yml", "jobs:\n  ci:\n    steps:\n      - run: dotnet build && dotnet test")],
            isComplete: false,
            DataAvailability.Unavailable(
                "workflows",
                DataAvailabilityStatus.RateLimited,
                "Later workflow content failed."));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void Evaluate_WithUnclosedQuotedRunScalar_ReturnsRequiresHumanReview()
    {
        SignalResult result = EvaluateAvailable(
            [Workflow(".github/workflows/ci.yml", "jobs:\n  ci:\n    steps:\n      - run: \"dotnet test")]);

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.Contains(result.Evidence, item => item.Source == "WorkflowAmbiguity");
    }

    [Fact]
    public void Evaluate_WithNullWorkflowContent_ReturnsUnavailableEvidence()
    {
        SignalResult result = EvaluateAvailable(
            [Workflow(".github/workflows/ci.yml", content: null)]);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_CommentsNamesAndUsesDoNotCreateBuildOrTestEvidence()
    {
        SignalResult result = EvaluateAvailable(
        [
            Workflow(".github/workflows/metadata.yml", """
                name: Build and Test
                # run: dotnet build && dotnet test
                env:
                  TEST_COMMAND: dotnet test
                jobs:
                  metadata:
                    steps:
                      - name: Run tests
                        uses: actions/checkout@v4
                      - run: echo "dotnet build and dotnet test"
                """)
        ]);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
        Assert.DoesNotContain(result.Evidence, item => item.Source == "WorkflowBuild");
        Assert.DoesNotContain(result.Evidence, item => item.Source == "WorkflowTest");
    }

    [Fact]
    public void Evaluate_InputOrderDoesNotAffectResultAndTimeIsReadOnce()
    {
        RepositoryFile[] workflows =
        [
            Workflow(".github/workflows/test.yml", "jobs:\n  test:\n    steps:\n      - run: dotnet test"),
            Workflow(".github/workflows/build.yml", "jobs:\n  build:\n    steps:\n      - run: dotnet build")
        ];
        RepositorySnapshot firstSnapshot = CreateAvailableSnapshot(workflows);
        RepositorySnapshot secondSnapshot = CreateAvailableSnapshot(workflows.Reverse());
        CountingTimeProvider firstTimeProvider = new(EvaluatedAt);
        CountingTimeProvider secondTimeProvider = new(EvaluatedAt);

        SignalResult first = new ContinuousIntegrationSignalEvaluator(firstTimeProvider).Evaluate(firstSnapshot);
        SignalResult second = new ContinuousIntegrationSignalEvaluator(secondTimeProvider).Evaluate(secondSnapshot);

        AssertEquivalent(first, second);
        Assert.Equal(1, firstTimeProvider.CallCount);
        Assert.Equal(1, secondTimeProvider.CallCount);
    }

    private static SignalResult EvaluateAvailable(IEnumerable<RepositoryFile> workflows) =>
        Evaluate(CreateAvailableSnapshot(workflows));

    private static SignalResult Evaluate(RepositorySnapshot snapshot) =>
        new ContinuousIntegrationSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

    private static RepositorySnapshot CreateAvailableSnapshot(
        IEnumerable<RepositoryFile> workflows) =>
        CreateSnapshot(
            workflows,
            isComplete: true,
            DataAvailability.Available("workflows", RetrievedAt));

    private static RepositoryFile Workflow(string path, string? content) =>
        new(path, isDirectory: false, size: content?.Length, content);

    private static RepositorySnapshot CreateSnapshot(
        IEnumerable<RepositoryFile> workflows,
        bool isComplete,
        DataAvailability availability)
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
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            new RepositoryCollection<RepositoryFile>(workflows, isComplete: isComplete),
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            [availability],
            collectionWarnings: null);
    }

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.Evidence, second.Evidence);
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

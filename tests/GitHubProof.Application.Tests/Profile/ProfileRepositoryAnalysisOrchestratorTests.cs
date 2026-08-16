using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Enums;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

public sealed class ProfileRepositoryAnalysisOrchestratorTests
{
    [Fact]
    public async Task AnalyzeAsync_AdmittedRepository_ReusesTheExistingAnalysisPipeline()
    {
        RepositorySnapshot collected = CreateRepositorySnapshot("hello-world");
        FakeSnapshotSource source = CreateSource(
            ("octocat/hello-world", () => CreateCollected(collected)));
        SequentialAnalysisIdFactory ids = new();

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source, ids.Next).AnalyzeAsync(
                CreateProfileSnapshot([CreateProfileRepository("hello-world")]),
                TestAnalysisVersion);

        ProfileRepositoryAnalysisResult repository = Assert.Single(result.Results);

        Assert.Equal(ProfileRepositoryAnalysisOutcome.Analyzed, repository.Outcome);
        Assert.Equal(
            AnalysisAdmissionDecision.Analyze,
            repository.Admission.Decision);

        RepositoryAnalysis analysis = Assert.IsType<RepositoryAnalysis>(
            repository.Analysis);

        Assert.Same(collected, analysis.Snapshot);
        Assert.Equal(15, analysis.SignalResults.Count);
        Assert.Equal(
            RepositoryAnalysisComposer.CanonicalSignalKeys,
            analysis.SignalResults.Select(signal => signal.SignalKey));
        Assert.Equal(TestAnalysisVersion, analysis.Version);
        Assert.Equal(TestPolicyVersion, result.PolicyVersion);
        Assert.Equal(
            Guid.Parse("04800000-0000-0000-0000-000000000001"),
            analysis.AnalysisId);
    }

    [Fact]
    public async Task AnalyzeAsync_SkippedRepository_IsNeitherCollectedNorAnalyzed()
    {
        FakeSnapshotSource source = CreateSource();

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source).AnalyzeAsync(
                CreateProfileSnapshot(
                [
                    CreateProfileRepository(
                        "empty-repository",
                        RepositoryConditionState.Present)
                ]),
                TestAnalysisVersion);

        ProfileRepositoryAnalysisResult repository = Assert.Single(result.Results);

        Assert.Equal(ProfileRepositoryAnalysisOutcome.NotAdmitted, repository.Outcome);
        Assert.Equal(AnalysisAdmissionDecision.Skip, repository.Admission.Decision);
        Assert.Equal(
            AnalysisAdmissionReason.EmptyRepository,
            repository.Admission.Reason);
        Assert.Null(repository.Analysis);
        Assert.Empty(source.RequestedRepositories);
    }

    [Fact]
    public async Task AnalyzeAsync_UndeterminedAdmission_IsNotPromotedToAnalyze()
    {
        FakeSnapshotSource source = CreateSource();

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source).AnalyzeAsync(
                CreateProfileSnapshot(
                [
                    CreateProfileRepository(
                        "unknown-state",
                        RepositoryConditionState.Undetermined)
                ]),
                TestAnalysisVersion);

        ProfileRepositoryAnalysisResult repository = Assert.Single(result.Results);

        Assert.Equal(
            ProfileRepositoryAnalysisOutcome.AdmissionUndetermined,
            repository.Outcome);
        Assert.Equal(
            AnalysisAdmissionReason.RequiredInputUnavailable,
            repository.Admission.Reason);
        Assert.Null(repository.Analysis);
        Assert.Empty(source.RequestedRepositories);
    }

    [Fact]
    public async Task AnalyzeAsync_PartialFailure_KeepsSuccessfulRepositoryAnalyses()
    {
        FakeSnapshotSource source = CreateSource(
            ("octocat/analyzed",
                () => CreateCollected(CreateRepositorySnapshot("analyzed"))),
            ("octocat/analysis-fails",
                () => CreateCollected(CreateRepositorySnapshot("analysis-fails"))),
            ("octocat/collection-fails",
                () => throw new HttpRequestException("GitHub is unreachable.")),
            ("octocat/forbidden", () => CreateUnavailable()));

        ProfileAnalysisOrchestrationResult result =
            await new ProfileRepositoryAnalysisOrchestrator(
                    source,
                    new FailingAnalysisRunner(
                        CreateAnalysisRunner(),
                        "analysis-fails"),
                    new AnalysisAdmissionPolicy(TestPolicyVersion))
                .AnalyzeAsync(
                    CreateProfileSnapshot(
                    [
                        CreateProfileRepository("analyzed"),
                        CreateProfileRepository("analysis-fails"),
                        CreateProfileRepository("collection-fails"),
                        CreateProfileRepository("forbidden"),
                        CreateProfileRepository(
                            "skipped",
                            RepositoryConditionState.Present)
                    ]),
                    TestAnalysisVersion);

        Assert.Equal(
            new[]
            {
                ProfileRepositoryAnalysisOutcome.AnalysisFailed,
                ProfileRepositoryAnalysisOutcome.Analyzed,
                ProfileRepositoryAnalysisOutcome.CollectionFailed,
                ProfileRepositoryAnalysisOutcome.CollectionUnavailable,
                ProfileRepositoryAnalysisOutcome.NotAdmitted
            },
            result.Results.Select(repository => repository.Outcome));

        ProfileRepositoryAnalysisResult analyzed = FindResult(result, "analyzed");
        Assert.NotNull(analyzed.Analysis);

        ProfileRepositoryAnalysisResult analysisFailed =
            FindResult(result, "analysis-fails");
        Assert.Null(analysisFailed.Analysis);
        Assert.NotNull(analysisFailed.FailureReason);

        ProfileRepositoryAnalysisResult collectionFailed =
            FindResult(result, "collection-fails");
        Assert.Null(collectionFailed.CollectionAvailability);
        Assert.NotNull(collectionFailed.FailureReason);

        ProfileRepositoryAnalysisResult forbidden = FindResult(result, "forbidden");
        Assert.Equal(
            DataAvailabilityStatus.Forbidden,
            forbidden.CollectionAvailability?.Status);
        Assert.Null(forbidden.FailureReason);
    }

    [Fact]
    public async Task AnalyzeAsync_OrdersResultsDeterministicallyAcrossOwnersAndNames()
    {
        FakeSnapshotSource source = CreateSource(
            ("alpha/zeta",
                () => CreateCollected(CreateRepositorySnapshot("zeta", "alpha"))),
            ("octocat/alpha",
                () => CreateCollected(CreateRepositorySnapshot("alpha"))),
            ("octocat/beta",
                () => CreateCollected(CreateRepositorySnapshot("beta"))));

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source).AnalyzeAsync(
                CreateProfileSnapshot(
                [
                    CreateProfileRepository("beta"),
                    CreateProfileRepository("zeta", owner: "alpha"),
                    CreateProfileRepository("alpha")
                ]),
                TestAnalysisVersion);

        Assert.Equal(
            new[] { "alpha/zeta", "octocat/alpha", "octocat/beta" },
            result.Results.Select(repository => repository.Reference.FullName));
    }

    [Fact]
    public async Task AnalyzeAsync_WithSameInputs_ProducesTheSameSemanticResult()
    {
        static FakeSnapshotSource Source() =>
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateRepositorySnapshot("alpha"))),
                ("octocat/beta", () => CreateUnavailable()));

        static GitHubProfileSnapshot Snapshot() =>
            CreateProfileSnapshot(
            [
                CreateProfileRepository("alpha"),
                CreateProfileRepository("beta"),
                CreateProfileRepository(
                    "gamma",
                    RepositoryConditionState.Undetermined)
            ]);

        ProfileAnalysisOrchestrationResult first =
            await CreateOrchestrator(Source()).AnalyzeAsync(
                Snapshot(),
                TestAnalysisVersion);

        ProfileAnalysisOrchestrationResult second =
            await CreateOrchestrator(Source()).AnalyzeAsync(
                Snapshot(),
                TestAnalysisVersion);

        Assert.Equal(Describe(first), Describe(second));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenCollectedIdentityWasRenamed_KeepsProfileTraceability()
    {
        FakeSnapshotSource source = CreateSource(
            ("octocat/hello-world",
                () => CreateCollected(
                    CreateRepositorySnapshot("hello-universe", "octocat-renamed"))));

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source).AnalyzeAsync(
                CreateProfileSnapshot([CreateProfileRepository("hello-world")]),
                TestAnalysisVersion);

        ProfileRepositoryAnalysisResult repository = Assert.Single(result.Results);

        Assert.Equal(ProfileRepositoryAnalysisOutcome.Analyzed, repository.Outcome);
        Assert.Equal("octocat/hello-world", repository.Reference.FullName);
        Assert.Equal(
            "octocat-renamed/hello-universe",
            repository.Analysis?.Repository.FullName);
    }

    [Fact]
    public async Task AnalyzeAsync_IncompleteManifest_IsReportedAndNotTreatedAsComplete()
    {
        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha",
                () => CreateCollected(CreateRepositorySnapshot("alpha"))));

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source).AnalyzeAsync(
                CreateProfileSnapshot(
                    [CreateProfileRepository("alpha")],
                    isComplete: false),
                TestAnalysisVersion);

        Assert.False(result.IsRepositoryManifestComplete);
        Assert.Single(result.Results);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenSnapshotIsNull_FailsFast()
    {
        ProfileRepositoryAnalysisOrchestrator orchestrator =
            CreateOrchestrator(CreateSource());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => orchestrator.AnalyzeAsync(null!, TestAnalysisVersion));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenVersionIsNull_FailsFast()
    {
        ProfileRepositoryAnalysisOrchestrator orchestrator =
            CreateOrchestrator(CreateSource());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => orchestrator.AnalyzeAsync(
                CreateProfileSnapshot([CreateProfileRepository("alpha")]),
                null!));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenCancelled_DoesNotSwallowCancellation()
    {
        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha",
                () => CreateCollected(CreateRepositorySnapshot("alpha"))));
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => CreateOrchestrator(source).AnalyzeAsync(
                CreateProfileSnapshot([CreateProfileRepository("alpha")]),
                TestAnalysisVersion,
                cancellation.Token));

        Assert.Empty(source.RequestedRepositories);
    }

    [Fact]
    public void Constructor_WhenDependenciesAreMissing_FailsFast()
    {
        FakeSnapshotSource source = CreateSource();
        AnalysisAdmissionPolicy policy = new(TestPolicyVersion);

        Assert.Throws<ArgumentNullException>(
            () => new ProfileRepositoryAnalysisOrchestrator(
                null!,
                CreateAnalysisRunner(),
                policy));

        Assert.Throws<ArgumentNullException>(
            () => new ProfileRepositoryAnalysisOrchestrator(
                source,
                null!,
                policy));

        Assert.Throws<ArgumentNullException>(
            () => new ProfileRepositoryAnalysisOrchestrator(
                source,
                CreateAnalysisRunner(),
                null!));
    }

    private static FakeSnapshotSource CreateSource(
        params (string FullName, Func<ProfileRepositorySnapshotResult> Response)[] responses) =>
        new(responses.ToDictionary(
            response => response.FullName,
            response => response.Response,
            StringComparer.Ordinal));

    private static ProfileRepositoryAnalysisOrchestrator CreateOrchestrator(
        FakeSnapshotSource source,
        Func<Guid>? analysisIdFactory = null) =>
        new(
            source,
            CreateAnalysisRunner(),
            new AnalysisAdmissionPolicy(TestPolicyVersion),
            analysisIdFactory);

    private static ProfileRepositoryAnalysisResult FindResult(
        ProfileAnalysisOrchestrationResult result,
        string name) =>
        result.Results.Single(repository => string.Equals(
            repository.Reference.Name,
            name,
            StringComparison.Ordinal));

    private static IEnumerable<string> Describe(
        ProfileAnalysisOrchestrationResult result) =>
        result.Results.Select(repository =>
            $"{repository.Reference.FullName}:{repository.Outcome}:{repository.Admission.Reason}");
}

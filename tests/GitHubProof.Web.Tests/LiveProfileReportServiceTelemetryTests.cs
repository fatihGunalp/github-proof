using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Xunit;

namespace GitHubProof.Web.Tests;

// End-to-end §9 telemetry through the real LiveProfileReportService and the
// real ProfilePipeline, wired exactly as AddProfilePipeline wires it: the two
// observation-only wrappers supply the repository counters, and
// ProfileRepositoryAnalysisOrchestrator is untouched.
public sealed class LiveProfileReportServiceTelemetryTests
{
    private const string ProfileIdentity = "octocat";

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-21T00:10:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    // analyzed: admitted and analyzed successfully
    // unavailable: admitted, collection could not complete
    // empty: discovered but never admitted (AnalysisAdmissionPolicy skips it)
    [Fact]
    public async Task AnalyzeAsync_RecordsDiscoveredAdmittedCompletedAndIncomplete()
    {
        LiveProfileReportService service = CreateService(out _);

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(3, telemetry.RepositoriesDiscovered);
        Assert.Equal(2, telemetry.RepositoriesAdmitted);
        Assert.Equal(1, telemetry.RepositoriesCompleted);
        Assert.Equal(1, telemetry.RepositoriesIncomplete);
    }

    // §9: an admitted repository that could not complete must stay
    // distinguishable by its typed reason, not collapse into a bare count.
    [Fact]
    public async Task AnalyzeAsync_RecordsWhyAnAdmittedRepositoryCouldNotComplete()
    {
        LiveProfileReportService service = CreateService(out _);

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(
            1,
            telemetry.RepositoryOutcomes[DataAvailabilityStatus.Failed.ToString()]);
        Assert.All(
            telemetry.RepositoryOutcomes.Keys,
            key => Assert.Contains(key, ProfileRepositoryOutcome.All));
    }

    // Session 099: the profile composition no longer collects contributors, so
    // that request category can never appear in a profile execution. The
    // load-bearing zero-request proof is the Infrastructure composition test;
    // this is the runtime regression guard beside it.
    [Fact]
    public async Task AnalyzeAsync_NeverRecordsAContributorRequestCategory()
    {
        LiveProfileReportService service = CreateService(out _);

        await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.DoesNotContain(
            ProfileGitHubRequestCategory.RepositoryContributors,
            service.LastExecutionTelemetry!.RequestCategories.Keys);
    }

    [Fact]
    public async Task AnalyzeAsync_RecordsDurationStatusAndOutcome()
    {
        LiveProfileReportService service = CreateService(out _);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.True(result.IsSuccessful);
        Assert.Equal(ProfileExecutionOutcome.Composed, telemetry.Outcome);
        Assert.Equal(
            result.PipelineResult!.Profile!.AnalysisStatus.ToString(),
            telemetry.Status);
        Assert.True(telemetry.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task AnalyzeAsync_WithUnavailableProfile_RecordsTheTypedCollectionOutcome()
    {
        LiveProfileReportService service = new(
            new StubProfileCollector(
                new GitHubProfileSnapshotResult(
                    snapshot: null,
                    DataAvailability.Unavailable(
                        "profile",
                        DataAvailabilityStatus.RateLimited,
                        "GitHub rate limit was reached."))),
            CreatePipeline(new StubRepositorySnapshotSource()));

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.False(result.IsSuccessful);
        Assert.Equal(
            ProfileExecutionOutcome.CollectionUnavailable,
            telemetry.Outcome);
        Assert.Equal(
            DataAvailabilityStatus.RateLimited.ToString(),
            telemetry.Status);
        Assert.Equal(0, telemetry.RepositoriesDiscovered);
    }

    // §11.1: cancellation is recorded and rethrown. No RepositoryAnalysis,
    // ProfilePipelineResult, partial profile or failure profile is fabricated to
    // convert it, and the exception is never swallowed.
    [Fact]
    public async Task AnalyzeAsync_WhenCancelled_RecordsCancellationAndRethrows()
    {
        LiveProfileReportService service = CreateService(out _);

        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AnalyzeAsync(ProfileIdentity, Version, cancellation.Token));

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(ProfileExecutionOutcome.Cancelled, telemetry.Outcome);
        Assert.Equal(ProfileExecutionOutcome.Cancelled, telemetry.Status);
    }

    // An OperationCanceledException raised while the caller's token was NOT
    // cancelled is not caller cancellation — HttpClient's transport timeout
    // surfaces exactly this way. It must not be labelled `cancelled`, and it
    // must still propagate unchanged.
    [Fact]
    public async Task AnalyzeAsync_WithANonCallerCancellation_DoesNotLabelItCancelled()
    {
        LiveProfileReportService service = new(
            new ThrowingProfileCollector(
                new TaskCanceledException(
                    "The request was canceled due to the configured HttpClient.Timeout.")),
            CreatePipeline(new StubRepositorySnapshotSource()));

        using CancellationTokenSource cancellation = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AnalyzeAsync(ProfileIdentity, Version, cancellation.Token));

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.False(cancellation.IsCancellationRequested);
        Assert.NotEqual(ProfileExecutionOutcome.Cancelled, telemetry.Outcome);
        Assert.NotEqual(ProfileExecutionOutcome.Cancelled, telemetry.Status);
        Assert.Equal(ProfileExecutionOutcome.Faulted, telemetry.Outcome);
        Assert.Equal(ProfileExecutionOutcome.Faulted, telemetry.Status);
    }

    // An ordinary failure still propagates and is still recorded as a fault.
    [Fact]
    public async Task AnalyzeAsync_WhenCollectionThrows_RecordsFaultedAndRethrows()
    {
        LiveProfileReportService service = new(
            new ThrowingProfileCollector(
                new InvalidOperationException("GitHub could not be reached.")),
            CreatePipeline(new StubRepositorySnapshotSource()));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AnalyzeAsync(ProfileIdentity, Version));

        Assert.Equal(
            ProfileExecutionOutcome.Faulted,
            service.LastExecutionTelemetry!.Outcome);
    }

    // §9 / review F2: a thrown repository collection must still explain itself.
    // The orchestrator's own collection-failed handling is unchanged, so the
    // profile still composes exactly as it did before.
    [Fact]
    public async Task AnalyzeAsync_WhenRepositoryCollectionThrows_RecordsFailedWithoutChangingSemantics()
    {
        LiveProfileReportService service = new(
            new StubProfileCollector(CreateSingleRepositoryProfileSnapshotResult()),
            CreatePipeline(
                new ThrowingRepositorySnapshotSource(
                    new InvalidOperationException("GitHub could not be reached."))));

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(1, telemetry.RepositoriesAdmitted);
        Assert.Equal(0, telemetry.RepositoriesCompleted);
        Assert.Equal(1, telemetry.RepositoriesIncomplete);
        Assert.Equal(
            1,
            telemetry.RepositoryOutcomes[DataAvailabilityStatus.Failed.ToString()]);

        // Unchanged semantics: the orchestrator still converted the throw into
        // its existing collection-failed result and the profile still composed.
        Assert.True(result.IsSuccessful);
        Assert.True(result.PipelineResult!.HasProfile);
    }

    // A non-caller OperationCanceledException from repository collection is
    // likewise not caller cancellation, so it is recorded as Failed.
    [Fact]
    public async Task AnalyzeAsync_WhenRepositoryCollectionThrowsNonCallerCancellation_RecordsFailed()
    {
        LiveProfileReportService service = new(
            new StubProfileCollector(CreateSingleRepositoryProfileSnapshotResult()),
            CreatePipeline(
                new ThrowingRepositorySnapshotSource(
                    new TaskCanceledException(
                        "The request was canceled due to the configured HttpClient.Timeout."))));

        await service.AnalyzeAsync(ProfileIdentity, Version);

        ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry!;

        Assert.Equal(
            1,
            telemetry.RepositoryOutcomes[DataAvailabilityStatus.Failed.ToString()]);
        Assert.DoesNotContain(
            ProfileExecutionOutcome.Cancelled,
            telemetry.RepositoryOutcomes.Keys);
    }

    // The ambient scope must not leak past one execution.
    [Fact]
    public async Task AnalyzeAsync_LeavesNoAmbientTelemetryBehind()
    {
        LiveProfileReportService service = CreateService(out _);

        await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Null(ProfileExecutionTelemetryScope.Current);
    }

    // Two executions never share an instance, so counters cannot accumulate
    // across requests.
    [Fact]
    public async Task AnalyzeAsync_UsesAFreshTelemetryInstancePerExecution()
    {
        LiveProfileReportService service = CreateService(out _);

        await service.AnalyzeAsync(ProfileIdentity, Version);
        ProfileExecutionTelemetry first = service.LastExecutionTelemetry!;

        await service.AnalyzeAsync(ProfileIdentity, Version);
        ProfileExecutionTelemetry second = service.LastExecutionTelemetry!;

        Assert.NotSame(first, second);
        Assert.Equal(first.RepositoriesAdmitted, second.RepositoriesAdmitted);
    }

    private static LiveProfileReportService CreateService(
        out StubRepositorySnapshotSource source)
    {
        source = new StubRepositorySnapshotSource();

        return new LiveProfileReportService(
            new StubProfileCollector(CreateProfileSnapshotResult()),
            CreatePipeline(source));
    }

    // The exact wiring AddProfilePipeline produces: the production orchestrator,
    // with the two observation-only wrappers around the source and the runner.
    private static ProfilePipeline CreatePipeline(
        IProfileRepositorySnapshotSource source) =>
        new(
            new ProfileRepositoryAnalysisOrchestrator(
                new TelemetryProfileRepositorySnapshotSource(source),
                new TelemetryRepositoryAnalysisRunner(new RepositoryAnalysisRunner()),
                new AnalysisAdmissionPolicy(EligibilityPolicy.V1)),
            new RepositoryEligibilityPolicy(EligibilityPolicy.V1),
            new CrossRepositoryEvidenceComposer(
                new RepositoryContributionPolicy(EligibilityPolicy.V1.Version)),
            new ProfilePatternPromotionPolicy(PatternRuleVersion.V1),
            new RepresentativeRepositorySelectionPolicy(RepresentativeRepositoryRuleVersion.V1),
            new ProfileComposer(),
            new ProfileReportProjector(),
            ObservationRuleVersion.V1,
            ProfileAnalyzerVersion.V1);

    private static GitHubProfileSnapshotResult CreateProfileSnapshotResult()
    {
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                ProfileIdentity,
                new Uri($"https://github.com/{ProfileIdentity}")),
            CapturedAt,
            metadata: null,
            new ProfileRepositoryManifest(
                [
                    CreateRepository("analyzed"),
                    CreateRepository("unavailable"),
                    CreateRepository("empty", RepositoryConditionState.Present)
                ]),
            [DataAvailability.Available("profile-metadata", CapturedAt)],
            collectionWarnings: null);

        return new GitHubProfileSnapshotResult(
            snapshot,
            DataAvailability.Available("profile", CapturedAt));
    }

    private static ProfileRepository CreateRepository(
        string name,
        RepositoryConditionState empty = RepositoryConditionState.Absent) =>
        new(
            new ProfileRepositoryReference(
                "octocat",
                name,
                new Uri($"https://github.com/octocat/{name}")),
            defaultBranch: "main",
            primaryLanguage: "C#",
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            empty,
            CapturedAt.AddDays(-10),
            CapturedAt.AddYears(-2));

    private static GitHubProfileSnapshotResult CreateSingleRepositoryProfileSnapshotResult()
    {
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                ProfileIdentity,
                new Uri($"https://github.com/{ProfileIdentity}")),
            CapturedAt,
            metadata: null,
            new ProfileRepositoryManifest([CreateRepository("throwing")]),
            [DataAvailability.Available("profile-metadata", CapturedAt)],
            collectionWarnings: null);

        return new GitHubProfileSnapshotResult(
            snapshot,
            DataAvailability.Available("profile", CapturedAt));
    }

    private sealed class ThrowingProfileCollector(Exception exception)
        : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default) =>
            Task.FromException<GitHubProfileSnapshotResult>(exception);
    }

    private sealed class ThrowingRepositorySnapshotSource(Exception exception)
        : IProfileRepositorySnapshotSource
    {
        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default) =>
            Task.FromException<ProfileRepositorySnapshotResult>(exception);
    }

    private sealed class StubProfileCollector(GitHubProfileSnapshotResult result)
        : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(result);
        }
    }

    private sealed class StubRepositorySnapshotSource : IProfileRepositorySnapshotSource
    {
        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                reference.Name == "unavailable"
                    ? new ProfileRepositorySnapshotResult(
                        snapshot: null,
                        DataAvailability.Unavailable(
                            "metadata",
                            DataAvailabilityStatus.Failed,
                            "GitHub could not be reached."))
                    : new ProfileRepositorySnapshotResult(
                        CreateRepositorySnapshot(reference),
                        DataAvailability.Available("metadata", CapturedAt)));
        }

        private static RepositorySnapshot CreateRepositorySnapshot(
            ProfileRepositoryReference reference) =>
            new(
                new RepositoryIdentity(
                    reference.Owner,
                    reference.Name,
                    reference.Url,
                    "main",
                    isPublic: true,
                    isArchived: false,
                    isFork: false,
                    isTemplate: false,
                    primaryLanguage: "C#"),
                CapturedAt,
                headCommitSha: "abc123",
                metadata: null,
                new RepositoryReadme("README.md", "# Project\n\nOverview."),
                fileTree: null,
                commits: null,
                issues: null,
                pullRequests: null,
                releases: null,
                workflows: null,
                topics: null,
                license: null,
                languages: null,
                contributors: null,
                [DataAvailability.Available("readme", CapturedAt)],
                collectionWarnings: null);
    }
}

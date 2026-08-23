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

public sealed class ProfileExecutionBoundaryTests
{
    private const string ProfileIdentity = "octocat";

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-22T09:00:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public void Result_AcceptsExactlyTheDeadlineBeforeAvailabilityShape()
    {
        LiveProfileAnalysisResult result =
            LiveProfileAnalysisResult.DeadlineExpired(profileAvailability: null);

        Assert.Null(result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            LiveProfileAnalysisResult.ExecutionDataKey,
            result.ExecutionAvailability.DataKey);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            result.ExecutionAvailability.Status);
        Assert.Equal(
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            result.ExecutionAvailability.Reason);
        Assert.Null(result.ExecutionAvailability.RetrievedAt);
    }

    [Fact]
    public void Result_AcceptsExactlyTheDeadlineAfterAvailabilityShape()
    {
        DataAvailability available = ProfileAvailable();

        LiveProfileAnalysisResult result =
            LiveProfileAnalysisResult.DeadlineExpired(available);

        Assert.Same(available, result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            result.ExecutionAvailability.Status);
    }

    [Fact]
    public void Result_AcceptsExactlyTheProfileUnavailableShape()
    {
        DataAvailability unavailable = DataAvailability.Unavailable(
            "profile",
            DataAvailabilityStatus.RateLimited,
            "rate limited");

        LiveProfileAnalysisResult result =
            LiveProfileAnalysisResult.ProfileUnavailable(unavailable);

        Assert.Same(unavailable, result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            DataAvailabilityStatus.NotRequested,
            result.ExecutionAvailability.Status);
        Assert.Equal(
            LiveProfileAnalysisResult.ProfileUnavailableReason,
            result.ExecutionAvailability.Reason);
        Assert.Null(result.ExecutionAvailability.RetrievedAt);
    }

    [Fact]
    public void Result_AcceptsFailedPipelineAsAnAvailableTerminalExecution()
    {
        ProfilePipelineResult failed =
            ProfilePipelineResult.Failed(ProfileCompositionResult.Failed());

        LiveProfileAnalysisResult result =
            LiveProfileAnalysisResult.Completed(
                ProfileAvailable(),
                failed,
                CapturedAt);

        Assert.Same(failed, result.PipelineResult);
        Assert.Equal(
            DataAvailabilityStatus.Available,
            result.ExecutionAvailability.Status);
        Assert.Null(result.ExecutionAvailability.Reason);
        Assert.Equal(CapturedAt, result.ExecutionAvailability.RetrievedAt);
    }

    [Fact]
    public void Result_RejectsEveryNonCanonicalStatusReasonAndNullabilityShape()
    {
        ProfilePipelineResult failed =
            ProfilePipelineResult.Failed(ProfileCompositionResult.Failed());
        DataAvailability available = ProfileAvailable();
        DataAvailability unavailable = DataAvailability.Unavailable(
            "profile",
            DataAvailabilityStatus.Unavailable,
            "not found");

        Assert.Throws<ArgumentException>(() => new LiveProfileAnalysisResult(
            profileAvailability: null,
            DataAvailability.Unavailable(
                LiveProfileAnalysisResult.ExecutionDataKey,
                DataAvailabilityStatus.NotRequested,
                LiveProfileAnalysisResult.ProfileUnavailableReason),
            pipelineResult: null));

        Assert.Throws<ArgumentException>(() => new LiveProfileAnalysisResult(
            unavailable,
            DataAvailability.Unavailable(
                LiveProfileAnalysisResult.ExecutionDataKey,
                DataAvailabilityStatus.Failed,
                LiveProfileAnalysisResult.DeadlineExpiredReason),
            pipelineResult: null));

        Assert.Throws<ArgumentException>(() => new LiveProfileAnalysisResult(
            available,
            DataAvailability.Unavailable(
                LiveProfileAnalysisResult.ExecutionDataKey,
                DataAvailabilityStatus.Failed,
                "another-reason"),
            pipelineResult: null));

        Assert.Throws<ArgumentException>(() => new LiveProfileAnalysisResult(
            available,
            DataAvailability.Available("wrong-key", CapturedAt),
            failed));

        Assert.Throws<ArgumentException>(() => new LiveProfileAnalysisResult(
            available,
            DataAvailability.Available(
                LiveProfileAnalysisResult.ExecutionDataKey,
                CapturedAt),
            pipelineResult: null));
    }

    [Fact]
    public async Task AnalyzeAsync_UsesTerminalUtcInstantForExecutionAvailability()
    {
        DateTimeOffset terminalInstant =
            DateTimeOffset.Parse("2026-08-22T15:30:00+03:00");
        StubCollector collector = new(CreateSnapshotResult());
        LiveProfileReportService service = new(
            collector,
            CreatePipeline(new NeverCalledSnapshotSource()),
            timeProvider: new FixedTimeProvider(terminalInstant));

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Equal(
            terminalInstant.ToUniversalTime(),
            result.ExecutionAvailability.RetrievedAt);
        Assert.Equal(TimeSpan.Zero, result.ExecutionAvailability.RetrievedAt!.Value.Offset);
        Assert.Same(
            collector.Result.ProfileAvailability,
            result.ProfileAvailability);
    }

    [Fact]
    public async Task AnalyzeAsync_DeadlineRemainsActiveThroughTerminalResultCreation()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        StubCollector collector = new(CreateSnapshotResult());
        LiveProfileReportService service = new(
            collector,
            CreatePipeline(new NeverCalledSnapshotSource()),
            deadlineFactory: deadlineFactory,
            timeProvider: new ExpiringTimeProvider(deadlineFactory));

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Null(result.PipelineResult);
        Assert.True(result.ProfileAvailability!.IsAvailable);
        Assert.Equal(
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            result.ExecutionAvailability.Reason);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenDeadlineExpiresBeforeMetadata_ReturnsNullAvailability()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        CallbackCollector collector = new((token, _) =>
        {
            deadlineFactory.Expire();
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException("unreachable");
        });
        LiveProfileReportService service = CreateService(collector, deadlineFactory);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Null(result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            result.ExecutionAvailability.Reason);
        Assert.Equal(
            ProfileExecutionOutcome.DeadlineExpired,
            service.LastExecutionTelemetry!.Outcome);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AnalyzeAsync_WhenDeadlineWinsBeforeRealOrchestratorPublication_IgnoresLateTypedMetadata(
        bool metadataIsAvailable)
    {
        ControlledDeadlineFactory deadlineFactory = new();
        GitHubProfileMetadataCollection metadata = metadataIsAvailable
            ? AvailableMetadataCollection()
            : UnavailableMetadataCollection();
        GitHubProfileSnapshotOrchestrator collector = new(
            new CallbackMetadataCollector((_, _) =>
            {
                deadlineFactory.Expire();
                return Task.FromResult(metadata);
            }),
            new CallbackRepositoryCollector((_, _, _) =>
                Task.FromResult(EmptyRepositoryCollection())));
        LiveProfileReportService service = CreateService(
            collector,
            deadlineFactory);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Null(result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            result.ExecutionAvailability.Reason);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenRealAvailablePublicationWins_PreservesItAfterImmediateDeadline()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        GitHubProfileMetadataCollection metadata = AvailableMetadataCollection();
        GitHubProfileSnapshotOrchestrator collector = new(
            new CallbackMetadataCollector((_, _) => Task.FromResult(metadata)),
            new CallbackRepositoryCollector((_, _, _) =>
            {
                deadlineFactory.Expire();
                return Task.FromResult(EmptyRepositoryCollection());
            }));
        LiveProfileReportService service = CreateService(
            collector,
            deadlineFactory);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Same(metadata.Availability, result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            result.ExecutionAvailability.Reason);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenRealUnavailablePublicationWins_LaterDeadlineCannotOverrideIt()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        GitHubProfileMetadataCollection metadata = UnavailableMetadataCollection();
        GitHubProfileSnapshotOrchestrator orchestrator = new(
            new CallbackMetadataCollector((_, _) => Task.FromResult(metadata)),
            new CallbackRepositoryCollector((_, _, _) =>
                throw new InvalidOperationException(
                    "Repository collection must not run for unavailable metadata.")));
        ExpireAfterCollectionCollector collector = new(
            orchestrator,
            deadlineFactory);
        LiveProfileReportService service = CreateService(
            collector,
            deadlineFactory);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Same(metadata.Availability, result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            DataAvailabilityStatus.NotRequested,
            result.ExecutionAvailability.Status);
        Assert.Equal(
            LiveProfileAnalysisResult.ProfileUnavailableReason,
            result.ExecutionAvailability.Reason);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenDeadlineExpiresAfterMetadata_PreservesAvailabilityAndReturnsNoPartialPipeline()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        DataAvailability available = ProfileAvailable();
        CallbackCollector collector = new((token, progress) =>
        {
            progress!.ResolveProfileAvailability(available);
            deadlineFactory.Expire();
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException("unreachable");
        });
        LiveProfileReportService service = CreateService(collector, deadlineFactory);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.Same(available, result.ProfileAvailability);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            result.ExecutionAvailability.Status);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenDeadlineExpiresDuringPipeline_DiscardsCompletedSoFarResult()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        StubCollector collector = new(CreateSnapshotResult(withRepository: true));
        ProfilePipeline pipeline = CreatePipeline(
            new DeadlineSnapshotSource(deadlineFactory));
        LiveProfileReportService service = new(
            collector,
            pipeline,
            deadlineFactory: deadlineFactory);

        LiveProfileAnalysisResult result =
            await service.AnalyzeAsync(ProfileIdentity, Version);

        Assert.True(result.ProfileAvailability!.IsAvailable);
        Assert.Null(result.PipelineResult);
        Assert.Equal(
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            result.ExecutionAvailability.Reason);
    }

    [Fact]
    public async Task AnalyzeAsync_CallerCancellationPropagates()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        using CancellationTokenSource caller = new();
        CallbackCollector collector = new((token, _) =>
        {
            caller.Cancel();
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException("unreachable");
        });
        LiveProfileReportService service = CreateService(collector, deadlineFactory);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.AnalyzeAsync(ProfileIdentity, Version, caller.Token));
    }

    [Fact]
    public async Task AnalyzeAsync_CallerCancellationWinsOverlapWithDeadline()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        using CancellationTokenSource caller = new();
        CallbackCollector collector = new((token, _) =>
        {
            deadlineFactory.Expire();
            caller.Cancel();
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException("unreachable");
        });
        LiveProfileReportService service = CreateService(collector, deadlineFactory);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.AnalyzeAsync(ProfileIdentity, Version, caller.Token));
    }

    [Fact]
    public async Task AnalyzeAsync_TransportCancellationIsNotClassifiedAsDeadline()
    {
        ControlledDeadlineFactory deadlineFactory = new();
        TaskCanceledException transportTimeout = new("transport timeout");
        CallbackCollector collector = new((_, _) =>
            Task.FromException<GitHubProfileSnapshotResult>(transportTimeout));
        LiveProfileReportService service = CreateService(collector, deadlineFactory);

        OperationCanceledException thrown =
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.AnalyzeAsync(ProfileIdentity, Version));

        Assert.Same(transportTimeout, thrown);
        Assert.False(deadlineFactory.Current!.IsExpired);
    }

    private static LiveProfileReportService CreateService(
        IGitHubProfileSnapshotCollector collector,
        IProfileExecutionDeadlineFactory deadlineFactory) =>
        new(
            collector,
            CreatePipeline(new NeverCalledSnapshotSource()),
            deadlineFactory: deadlineFactory);

    private static ProfilePipeline CreatePipeline(
        IProfileRepositorySnapshotSource source) =>
        new(
            new ProfileRepositoryAnalysisOrchestrator(
                source,
                new RepositoryAnalysisRunner(),
                new AnalysisAdmissionPolicy(EligibilityPolicy.V1)),
            new RepositoryEligibilityPolicy(EligibilityPolicy.V1),
            new CrossRepositoryEvidenceComposer(
                new RepositoryContributionPolicy(EligibilityPolicy.V1.Version)),
            new ProfilePatternPromotionPolicy(PatternRuleVersion.V1),
            new RepresentativeRepositorySelectionPolicy(
                RepresentativeRepositoryRuleVersion.V1),
            new ProfileComposer(),
            new ProfileReportProjector(),
            ObservationRuleVersion.V1,
            ProfileAnalyzerVersion.V1);

    private static GitHubProfileSnapshotResult CreateSnapshotResult(
        bool withRepository = false)
    {
        ProfileRepositoryManifest repositories = withRepository
            ? new ProfileRepositoryManifest([CreateRepository()])
            : new ProfileRepositoryManifest();
        GitHubProfileSnapshot snapshot = new(
            new GitHubProfileIdentity(
                ProfileIdentity,
                new Uri($"https://github.com/{ProfileIdentity}")),
            CapturedAt,
            metadata: null,
            repositories,
            availability: null,
            collectionWarnings: null);

        return new GitHubProfileSnapshotResult(snapshot, ProfileAvailable());
    }

    private static ProfileRepository CreateRepository() =>
        new(
            new ProfileRepositoryReference(
                ProfileIdentity,
                "repo",
                new Uri($"https://github.com/{ProfileIdentity}/repo")),
            "main",
            "C#",
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            CapturedAt,
            CapturedAt.AddYears(-1));

    private static DataAvailability ProfileAvailable() =>
        DataAvailability.Available("profile", CapturedAt);

    private static GitHubProfileMetadataCollection AvailableMetadataCollection() =>
        new(
            new GitHubProfileIdentity(
                ProfileIdentity,
                new Uri($"https://github.com/{ProfileIdentity}")),
            new GitHubProfileMetadata(
                "The Octocat",
                CapturedAt.AddYears(-10),
                publicRepositoryCount: 0,
                avatarUrl: new Uri("https://avatars.githubusercontent.com/u/1")),
            ProfileAvailable());

    private static GitHubProfileMetadataCollection UnavailableMetadataCollection() =>
        new(
            Identity: null,
            Metadata: null,
            DataAvailability.Unavailable(
                "profile",
                DataAvailabilityStatus.RateLimited,
                "rate limited"));

    private static GitHubProfileRepositoryCollection EmptyRepositoryCollection() =>
        new(
            new ProfileRepositoryManifest(),
            DataAvailability.Available("profile-repositories", CapturedAt),
            Array.Empty<string>(),
            RepositoryPresentationMetadata.Empty);

    private sealed class StubCollector(GitHubProfileSnapshotResult result)
        : IGitHubProfileSnapshotCollector
    {
        public GitHubProfileSnapshotResult Result { get; } = result;

        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.ResolveProfileAvailability(Result.ProfileAvailability);
            return Task.FromResult(Result);
        }
    }

    private sealed class CallbackCollector(
        Func<CancellationToken, GitHubProfileSnapshotProgress?,
            Task<GitHubProfileSnapshotResult>> callback)
        : IGitHubProfileSnapshotCollector
    {
        public Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null) =>
            callback(cancellationToken, progress);
    }

    private sealed class CallbackMetadataCollector(
        Func<GitHubProfileHandle, CancellationToken,
            Task<GitHubProfileMetadataCollection>> callback)
        : IGitHubProfileMetadataCollector
    {
        public Task<GitHubProfileMetadataCollection> CollectAsync(
            GitHubProfileHandle profile,
            CancellationToken cancellationToken = default) =>
            callback(profile, cancellationToken);
    }

    private sealed class CallbackRepositoryCollector(
        Func<GitHubProfileHandle, GitHubProfileMetadataCollection,
            CancellationToken, Task<GitHubProfileRepositoryCollection>> callback)
        : IGitHubProfileRepositoryCollector
    {
        public Task<GitHubProfileRepositoryCollection> CollectAsync(
            GitHubProfileHandle profile,
            GitHubProfileMetadataCollection metadata,
            CancellationToken cancellationToken = default) =>
            callback(profile, metadata, cancellationToken);
    }

    private sealed class ExpireAfterCollectionCollector(
        IGitHubProfileSnapshotCollector inner,
        ControlledDeadlineFactory deadlineFactory)
        : IGitHubProfileSnapshotCollector
    {
        public async Task<GitHubProfileSnapshotResult> CollectAsync(
            string profile,
            CancellationToken cancellationToken = default,
            GitHubProfileSnapshotProgress? progress = null)
        {
            GitHubProfileSnapshotResult result = await inner.CollectAsync(
                profile,
                cancellationToken,
                progress);

            deadlineFactory.Expire();
            return result;
        }
    }

    private sealed class DeadlineSnapshotSource(
        ControlledDeadlineFactory deadlineFactory)
        : IProfileRepositorySnapshotSource
    {
        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            deadlineFactory.Expire();
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("unreachable");
        }
    }

    private sealed class NeverCalledSnapshotSource : IProfileRepositorySnapshotSource
    {
        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No repository should be collected.");
    }

    private sealed class ControlledDeadlineFactory : IProfileExecutionDeadlineFactory
    {
        public ControlledDeadline? Current { get; private set; }

        public IProfileExecutionDeadline Create(
            CancellationToken callerCancellationToken)
        {
            Current = new ControlledDeadline(callerCancellationToken);
            return Current;
        }

        public void Expire() =>
            (Current ?? throw new InvalidOperationException("No deadline exists."))
                .Expire();
    }

    private sealed class ControlledDeadline : IProfileExecutionDeadline
    {
        private readonly CancellationTokenSource deadline = new();
        private readonly CancellationTokenSource execution;

        public ControlledDeadline(CancellationToken callerCancellationToken)
        {
            execution = CancellationTokenSource.CreateLinkedTokenSource(
                callerCancellationToken,
                deadline.Token);
        }

        public CancellationToken ExecutionToken => execution.Token;

        public bool IsExpired => deadline.IsCancellationRequested;

        public bool TryComplete() => !IsExpired;

        public void Expire() => deadline.Cancel();

        public void Dispose()
        {
            execution.Dispose();
            deadline.Dispose();
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class ExpiringTimeProvider(
        ControlledDeadlineFactory deadlineFactory) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            deadlineFactory.Expire();
            return CapturedAt;
        }
    }
}

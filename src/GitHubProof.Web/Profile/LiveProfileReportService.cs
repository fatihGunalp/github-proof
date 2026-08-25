using System.Diagnostics;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Logging.Abstractions;

namespace GitHubProof.Web.Profile;

// The profile counterpart of LiveRepositoryReportService: the smallest Web
// boundary from a raw profile identity string to a ProfileReport (or a typed
// failure), through the existing production collector and ProfilePipeline.
// It parses nothing itself — GitHubProfileHandle.Parse stays owned by
// IGitHubProfileSnapshotCollector's own implementation — and it reimplements
// no eligibility, composition or report decision of its own.
//
// Session 097 additionally makes one profile execution observable
// (docs/architecture/public-profile-runtime-collection.md §9). The telemetry is
// operational only: it is written after the analysis result already exists,
// never read by it, and never returned to the caller as part of the result.
public sealed class LiveProfileReportService
{
    private readonly IGitHubProfileSnapshotCollector snapshotCollector;
    private readonly ProfilePipeline profilePipeline;
    private readonly ILogger<LiveProfileReportService> logger;
    private readonly IProfileExecutionDeadlineFactory deadlineFactory;
    private readonly TimeProvider timeProvider;

    public LiveProfileReportService(
        IGitHubProfileSnapshotCollector snapshotCollector,
        ProfilePipeline profilePipeline,
        ILogger<LiveProfileReportService>? logger = null,
        IProfileExecutionDeadlineFactory? deadlineFactory = null,
        TimeProvider? timeProvider = null)
    {
        this.snapshotCollector = snapshotCollector
            ?? throw new ArgumentNullException(nameof(snapshotCollector));
        this.profilePipeline = profilePipeline
            ?? throw new ArgumentNullException(nameof(profilePipeline));
        this.logger = logger ?? NullLogger<LiveProfileReportService>.Instance;
        this.deadlineFactory = deadlineFactory
            ?? DisabledProfileExecutionDeadlineFactory.Instance;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    // Exposed so a controlled measurement run can read the last execution's
    // operational counters without parsing the log line. It is never evidence
    // and never part of LiveProfileAnalysisResult.
    public ProfileExecutionTelemetry? LastExecutionTelemetry { get; private set; }

    public async Task<LiveProfileAnalysisResult> AnalyzeAsync(
        string profileIdentity,
        AnalysisVersion version,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(version);

        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        LastExecutionTelemetry = telemetry;

        long startedAt = Stopwatch.GetTimestamp();
        string outcome = ProfileExecutionOutcome.Faulted;
        string status = ProfileExecutionOutcome.NotRecorded;

        // The ambient scope is what lets the pooled HTTP telemetry handler and
        // the observation-only wrappers find THIS execution's counters.
        using IDisposable scope = ProfileExecutionTelemetryScope.Begin(telemetry);
        using IDisposable budgetScope = ProfileGitHubBudgetScope.Begin(budgetState);
        using IProfileExecutionDeadline deadline =
            deadlineFactory.Create(cancellationToken);
        using GitHubProfileSnapshotProgress progress =
            new(deadline.ExecutionToken);

        try
        {
            // Session 120: the discovery interval is exactly the live collector
            // call — profile metadata plus the public repository manifest. It is
            // recorded in a finally so an aborted discovery still reports the
            // wall-clock it consumed.
            long discoveryStartedAt = Stopwatch.GetTimestamp();
            GitHubProfileSnapshotResult snapshotResult;

            try
            {
                snapshotResult = await snapshotCollector.CollectAsync(
                    profileIdentity,
                    deadline.ExecutionToken,
                    progress);
            }
            finally
            {
                telemetry.RecordDiscoveryElapsed(
                    Stopwatch.GetElapsedTime(discoveryStartedAt));
            }

            // The progress carrier, not a late collector return, owns the
            // availability-vs-cancellation ordering. Caller cancellation is
            // still checked first and wins every overlap.
            cancellationToken.ThrowIfCancellationRequested();
            if (deadline.IsExpired
                && progress.ExecutionCancellationWonBeforeResolution)
            {
                outcome = ProfileExecutionOutcome.DeadlineExpired;
                status = LiveProfileAnalysisResult.DeadlineExpiredReason;
                return CreateDeadlineResult(
                    profileAvailability: null,
                    callerCancellationToken: cancellationToken);
            }

            DataAvailability resolvedProfileAvailability =
                progress.ProfileAvailability
                ?? throw new InvalidOperationException(
                    "A completed Profile collector call must resolve profile availability.");

            if (resolvedProfileAvailability != snapshotResult.ProfileAvailability)
            {
                throw new InvalidOperationException(
                    "The Profile collector result must match the availability "
                    + "published at the operational progress boundary.");
            }

            if (!snapshotResult.IsSuccessful)
            {
                // Caller cancellation owns every overlap race, even if a lower
                // layer returned a typed provider result instead of observing
                // the linked token.
                cancellationToken.ThrowIfCancellationRequested();

                if (resolvedProfileAvailability.IsAvailable)
                {
                    throw new InvalidOperationException(
                        "Available profile metadata must produce a profile snapshot.");
                }

                outcome = ProfileExecutionOutcome.CollectionUnavailable;
                status = resolvedProfileAvailability.Status.ToString();

                LiveProfileAnalysisResult unavailable =
                    LiveProfileAnalysisResult.ProfileUnavailable(
                        resolvedProfileAvailability);

                cancellationToken.ThrowIfCancellationRequested();
                return unavailable;
            }

            GitHubProfileSnapshot snapshot = snapshotResult.Snapshot
                ?? throw new InvalidOperationException(
                    "A successful profile snapshot result must contain a snapshot.");

            cancellationToken.ThrowIfCancellationRequested();
            if (deadline.IsExpired)
            {
                outcome = ProfileExecutionOutcome.DeadlineExpired;
                status = LiveProfileAnalysisResult.DeadlineExpiredReason;
                return CreateDeadlineResult(
                    progress.ProfileAvailability,
                    cancellationToken);
            }

            telemetry.RecordRepositoriesDiscovered(
                snapshot.Repositories.Repositories.Count);

            // The pipeline interval encloses every per-repository collection and
            // analysis sample the two decorators record, which is what makes
            // ProfileExecutionTelemetry.PipelineWorkOutsideRepositoriesElapsed a
            // sound subtraction rather than an estimate.
            long pipelineStartedAt = Stopwatch.GetTimestamp();
            ProfilePipelineResult pipelineResult;

            try
            {
                pipelineResult = await profilePipeline.RunAsync(
                    snapshot,
                    version,
                    deadline.ExecutionToken);
            }
            finally
            {
                telemetry.RecordPipelineElapsed(
                    Stopwatch.GetElapsedTime(pipelineStartedAt));
            }

            // The internal deadline remains active through creation of the
            // terminal outer result. Caller cancellation is checked first so
            // it deterministically wins an overlap with deadline expiry.
            cancellationToken.ThrowIfCancellationRequested();
            if (deadline.IsExpired)
            {
                outcome = ProfileExecutionOutcome.DeadlineExpired;
                status = LiveProfileAnalysisResult.DeadlineExpiredReason;
                return CreateDeadlineResult(
                    progress.ProfileAvailability,
                    cancellationToken);
            }

            outcome = pipelineResult.HasProfile
                ? ProfileExecutionOutcome.Composed
                : ProfileExecutionOutcome.CompositionFailed;
            status = pipelineResult.HasProfile
                ? pipelineResult.Profile!.AnalysisStatus.ToString()
                : pipelineResult.Status.ToString();

            LiveProfileAnalysisResult completed = LiveProfileAnalysisResult.Completed(
                resolvedProfileAvailability,
                pipelineResult,
                timeProvider.GetUtcNow());

            cancellationToken.ThrowIfCancellationRequested();
            if (!deadline.TryComplete())
            {
                outcome = ProfileExecutionOutcome.DeadlineExpired;
                status = LiveProfileAnalysisResult.DeadlineExpiredReason;
                return CreateDeadlineResult(
                    progress.ProfileAvailability,
                    cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return completed;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Recorded, then rethrown unchanged: caller cancellation is never
            // converted into a fabricated analysis, pipeline result, partial
            // profile or failure profile
            // (public-profile-runtime-collection.md §11.1).
            outcome = ProfileExecutionOutcome.Cancelled;
            status = ProfileExecutionOutcome.Cancelled;
            throw;
        }
        catch (GitHubProfileBudgetProtectionRejectedException)
        {
            // Caller cancellation and the internal deadline retain precedence
            // over a budget decision that races immediately after metadata.
            cancellationToken.ThrowIfCancellationRequested();

            DataAvailability profileAvailability = progress.ProfileAvailability
                ?? throw new InvalidOperationException(
                    "Budget protection can reject only after profile availability resolves.");

            if (!profileAvailability.IsAvailable)
            {
                throw new InvalidOperationException(
                    "Budget protection can reject only Available profile metadata.");
            }

            outcome = ProfileExecutionOutcome.BudgetProtectionRejected;
            status = LiveProfileAnalysisResult.BudgetProtectionRejectedReason;

            LiveProfileAnalysisResult rejected =
                LiveProfileAnalysisResult.BudgetProtectionRejected(
                    profileAvailability);

            cancellationToken.ThrowIfCancellationRequested();

            // Budget rejection is a terminal live-execution result too. Race it
            // through the same owned completion boundary as successful
            // execution, so a deadline that wins here discards the candidate.
            if (!deadline.TryComplete())
            {
                outcome = ProfileExecutionOutcome.DeadlineExpired;
                status = LiveProfileAnalysisResult.DeadlineExpiredReason;
                return CreateDeadlineResult(
                    progress.ProfileAvailability,
                    cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return rejected;
        }
        catch (OperationCanceledException) when (deadline.IsExpired)
        {
            // This source is owned by the Profile execution boundary. No
            // completed-so-far pipeline result or Partial profile is returned.
            outcome = ProfileExecutionOutcome.DeadlineExpired;
            status = LiveProfileAnalysisResult.DeadlineExpiredReason;

            return CreateDeadlineResult(
                progress.ProfileAvailability,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException raised while the supplied token was
            // NOT cancelled is not caller cancellation — HttpClient's own
            // transport timeout surfaces this way. Labelling it "cancelled"
            // would blur exactly the distinction §10 principle 2 and §11.1 keep
            // sharp, so it is recorded as an ordinary operational fault. The
            // exception itself still propagates unchanged, and no new
            // AnalysisStatus or DataAvailabilityStatus is invented.
            outcome = ProfileExecutionOutcome.Faulted;
            status = ProfileExecutionOutcome.Faulted;
            throw;
        }
        finally
        {
            telemetry.Complete(
                Stopwatch.GetElapsedTime(startedAt),
                status,
                outcome);

            logger.LogInformation("{ProfileExecutionTelemetry}", telemetry.ToLogMessage());
        }
    }

    private static LiveProfileAnalysisResult CreateDeadlineResult(
        DataAvailability? profileAvailability,
        CancellationToken callerCancellationToken)
    {
        LiveProfileAnalysisResult result =
            LiveProfileAnalysisResult.DeadlineExpired(profileAvailability);

        // The final check is deliberately after result construction: caller
        // cancellation wins every overlap that exists before this execution
        // outcome reaches its terminal return point.
        callerCancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}

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

    public LiveProfileReportService(
        IGitHubProfileSnapshotCollector snapshotCollector,
        ProfilePipeline profilePipeline,
        ILogger<LiveProfileReportService>? logger = null)
    {
        this.snapshotCollector = snapshotCollector
            ?? throw new ArgumentNullException(nameof(snapshotCollector));
        this.profilePipeline = profilePipeline
            ?? throw new ArgumentNullException(nameof(profilePipeline));
        this.logger = logger ?? NullLogger<LiveProfileReportService>.Instance;
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
        LastExecutionTelemetry = telemetry;

        long startedAt = Stopwatch.GetTimestamp();
        string outcome = ProfileExecutionOutcome.Faulted;
        string status = ProfileExecutionOutcome.NotRecorded;

        // The ambient scope is what lets the pooled HTTP telemetry handler and
        // the observation-only wrappers find THIS execution's counters.
        using IDisposable scope = ProfileExecutionTelemetryScope.Begin(telemetry);

        try
        {
            GitHubProfileSnapshotResult snapshotResult =
                await snapshotCollector.CollectAsync(
                    profileIdentity,
                    cancellationToken);

            if (!snapshotResult.IsSuccessful)
            {
                if (snapshotResult.ProfileAvailability.IsAvailable)
                {
                    throw new InvalidOperationException(
                        "Available profile metadata must produce a profile snapshot.");
                }

                outcome = ProfileExecutionOutcome.CollectionUnavailable;
                status = snapshotResult.ProfileAvailability.Status.ToString();

                return new LiveProfileAnalysisResult(
                    pipelineResult: null,
                    snapshotResult.ProfileAvailability);
            }

            GitHubProfileSnapshot snapshot = snapshotResult.Snapshot
                ?? throw new InvalidOperationException(
                    "A successful profile snapshot result must contain a snapshot.");

            telemetry.RecordRepositoriesDiscovered(
                snapshot.Repositories.Repositories.Count);

            ProfilePipelineResult pipelineResult = await profilePipeline.RunAsync(
                snapshot,
                version,
                cancellationToken);

            outcome = pipelineResult.HasProfile
                ? ProfileExecutionOutcome.Composed
                : ProfileExecutionOutcome.CompositionFailed;
            status = pipelineResult.HasProfile
                ? pipelineResult.Profile!.AnalysisStatus.ToString()
                : pipelineResult.Status.ToString();

            return new LiveProfileAnalysisResult(
                pipelineResult,
                snapshotResult.ProfileAvailability);
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
}

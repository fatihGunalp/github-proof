using System.Diagnostics;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Web.Profile.Runtime;

// Observation-only wrappers used to obtain the §9 repository counters without
// modifying ProfileRepositoryAnalysisOrchestrator, which Session 096 §14.2
// keeps unchanged, and without recomputing admission anywhere in Web.
//
// Neither wrapper makes, changes, delays, reorders or suppresses any decision:
// each increments a counter and returns the inner component's result unchanged.
// Both expose Inner so the composition-root tests can still prove the exact
// production type is the one doing the work.

// The orchestrator calls the snapshot source exactly once per repository it
// admits for analysis, so an invocation count IS the admitted count. Admission
// itself stays owned by AnalysisAdmissionPolicy.
public sealed class TelemetryProfileRepositorySnapshotSource(
    IProfileRepositorySnapshotSource inner)
    : IProfileRepositorySnapshotSource
{
    private readonly IProfileRepositorySnapshotSource inner = inner
        ?? throw new ArgumentNullException(nameof(inner));

    public IProfileRepositorySnapshotSource Inner => inner;

    public async Task<ProfileRepositorySnapshotResult> CollectAsync(
        ProfileRepositoryReference reference,
        CancellationToken cancellationToken = default)
    {
        ProfileExecutionTelemetry? telemetry = ProfileExecutionTelemetryScope.Current;

        telemetry?.RecordRepositoryAdmitted();

        ProfileRepositorySnapshotResult result;

        // Session 120. This is the only call site through which the orchestrator
        // reaches repository collection, so the sampled interval is exactly one
        // repository's collection. The sample is anonymous — a duration and
        // nothing else — and it is recorded even when the call throws, because
        // an aborted collection still consumed wall-clock.
        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            result = await inner.CollectAsync(reference, cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Caller cancellation is not a repository failure and is never
            // relabelled as one. The orchestrator's own rethrow contract owns it.
            throw;
        }
        catch
        {
            // A thrown collection becomes the orchestrator's existing
            // collection-failed path, which would otherwise leave this execution
            // with an unexplained incomplete repository. Recorded as the same
            // closed Failed outcome, then rethrown unchanged so the orchestrator
            // still sees the identical exception and produces the identical
            // semantic result. A non-caller OperationCanceledException — a
            // transport timeout, for instance — lands here too and is likewise
            // never labelled cancellation.
            telemetry?.RecordRepositoryOutcome(
                ProfileRepositoryOutcome.FromAvailabilityStatus(
                    DataAvailabilityStatus.Failed));
            throw;
        }
        finally
        {
            telemetry?.RecordRepositoryCollectionElapsed(
                Stopwatch.GetElapsedTime(startedAt));
        }

        // A rate limit must stay distinguishable from a forbidden repository and
        // from an ordinary failure (§9). Only the typed status is recorded —
        // never the reason text and never the repository identity.
        if (result is not null && !result.IsSuccessful)
        {
            telemetry?.RecordRepositoryOutcome(
                ProfileRepositoryOutcome.FromAvailabilityStatus(
                    result.Availability.Status));
        }

        return result!;
    }
}

// The orchestrator runs the analysis only for a repository whose collection
// succeeded, and treats a thrown analysis as not completed. A normal return is
// therefore exactly "this admitted repository's analysis completed".
public sealed class TelemetryRepositoryAnalysisRunner(
    IRepositoryAnalysisRunner inner)
    : IRepositoryAnalysisRunner
{
    private readonly IRepositoryAnalysisRunner inner = inner
        ?? throw new ArgumentNullException(nameof(inner));

    public IRepositoryAnalysisRunner Inner => inner;

    public RepositoryAnalysis Analyze(
        RepositorySnapshot snapshot,
        AnalysisVersion version,
        Guid analysisId)
    {
        RepositoryAnalysis analysis;

        // Session 120: the same anonymous sampling on the analysis side. Analyze
        // is synchronous, so this interval is pure CPU and never overlaps the
        // collection interval measured above.
        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            analysis = inner.Analyze(snapshot, version, analysisId);
        }
        catch
        {
            // Analyze takes no CancellationToken and observes none, so nothing
            // it throws can be caller cancellation — including an
            // OperationCanceledException, which here is an analysis fault like
            // any other. Recorded, then rethrown unchanged so the orchestrator
            // still sees the same failure and produces the same typed result.
            ProfileExecutionTelemetryScope.Current?.RecordRepositoryOutcome(
                ProfileRepositoryOutcome.AnalysisFailed);
            throw;
        }
        finally
        {
            ProfileExecutionTelemetryScope.Current?.RecordRepositoryAnalysisElapsed(
                Stopwatch.GetElapsedTime(startedAt));
        }

        ProfileExecutionTelemetryScope.Current?.RecordRepositoryCompleted();

        return analysis;
    }
}

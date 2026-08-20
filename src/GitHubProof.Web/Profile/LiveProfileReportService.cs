using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Infrastructure.GitHub.Profile;

namespace GitHubProof.Web.Profile;

// The profile counterpart of LiveRepositoryReportService: the smallest Web
// boundary from a raw profile identity string to a ProfileReport (or a typed
// failure), through the existing production collector and ProfilePipeline.
// It parses nothing itself — GitHubProfileHandle.Parse stays owned by
// IGitHubProfileSnapshotCollector's own implementation — and it reimplements
// no eligibility, composition or report decision of its own.
public sealed class LiveProfileReportService
{
    private readonly IGitHubProfileSnapshotCollector snapshotCollector;
    private readonly ProfilePipeline profilePipeline;

    public LiveProfileReportService(
        IGitHubProfileSnapshotCollector snapshotCollector,
        ProfilePipeline profilePipeline)
    {
        this.snapshotCollector = snapshotCollector
            ?? throw new ArgumentNullException(nameof(snapshotCollector));
        this.profilePipeline = profilePipeline
            ?? throw new ArgumentNullException(nameof(profilePipeline));
    }

    public async Task<LiveProfileAnalysisResult> AnalyzeAsync(
        string profileIdentity,
        AnalysisVersion version,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(version);

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

            return new LiveProfileAnalysisResult(
                pipelineResult: null,
                snapshotResult.ProfileAvailability);
        }

        GitHubProfileSnapshot snapshot = snapshotResult.Snapshot
            ?? throw new InvalidOperationException(
                "A successful profile snapshot result must contain a snapshot.");

        ProfilePipelineResult pipelineResult = await profilePipeline.RunAsync(
            snapshot,
            version,
            cancellationToken);

        return new LiveProfileAnalysisResult(
            pipelineResult,
            snapshotResult.ProfileAvailability);
    }
}

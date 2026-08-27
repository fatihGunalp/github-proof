using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Progress;

namespace GitHubProof.Web.Analysis;

public sealed class LiveRepositoryReportService
{
    private readonly IGitHubRepositorySnapshotCollector snapshotCollector;
    private readonly RepositoryAnalysisRunner analysisRunner;
    private readonly RepositoryReportProjector reportProjector;

    public LiveRepositoryReportService(
        IGitHubRepositorySnapshotCollector snapshotCollector,
        RepositoryAnalysisRunner analysisRunner,
        RepositoryReportProjector reportProjector)
    {
        this.snapshotCollector = snapshotCollector
            ?? throw new ArgumentNullException(nameof(snapshotCollector));
        this.analysisRunner = analysisRunner
            ?? throw new ArgumentNullException(nameof(analysisRunner));
        this.reportProjector = reportProjector
            ?? throw new ArgumentNullException(nameof(reportProjector));
    }

    // `progress`, when supplied by IndexModel, is advanced once at each
    // observable completion point in live-analysis-progress-runtime-contract.md
    // §4. It is a strictly optional Web presentation concern: a null handle
    // leaves this method's behavior, result and failure delivery untouched.
    public async Task<LiveRepositoryAnalysisResult> AnalyzeAsync(
        string repositoryUrl,
        AnalysisVersion version,
        Guid analysisId,
        CancellationToken cancellationToken = default,
        AnalysisProgressRun? progress = null)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            throw new ArgumentException(
                "Repository URL cannot be null, empty, or whitespace.",
                nameof(repositoryUrl));
        }

        ArgumentNullException.ThrowIfNull(version);

        if (analysisId == Guid.Empty)
        {
            throw new ArgumentException(
                "Analysis ID cannot be empty.",
                nameof(analysisId));
        }

        GitHubRepositoryUrl repository =
            GitHubRepositoryUrl.Parse(repositoryUrl);

        // Stage 1 `repository-target-validated`: Parse has accepted the
        // classified entry value (§4). By this point registration has already
        // marked it ACTIVE, and no poll is expected to observe it so.
        progress?.CompleteCurrentStage();

        GitHubRepositorySnapshotResult snapshotResult =
            await snapshotCollector.CollectAsync(
                repository.CanonicalUri.AbsoluteUri,
                cancellationToken);

        if (!snapshotResult.IsSuccessful)
        {
            if (snapshotResult.MetadataAvailability.IsAvailable)
            {
                throw new InvalidOperationException(
                    "Available repository metadata must produce a repository snapshot.");
            }

            return new LiveRepositoryAnalysisResult(
                analysis: null,
                snapshotResult.MetadataAvailability);
        }

        RepositorySnapshot snapshot = snapshotResult.Snapshot
            ?? throw new InvalidOperationException(
                "A successful repository snapshot result must contain a snapshot.");

        // Stage 2 `repository-public-data-read`: CollectAsync has returned a
        // successful snapshot (§4). This stage carries effectively all of this
        // flow's wall-clock.
        progress?.CompleteCurrentStage();

        RepositoryAnalysis analysis = analysisRunner.Analyze(
            snapshot,
            version,
            analysisId);

        // Stage 3 `repository-traces-analyzed`: the analysis run has returned
        // (§4). Stage 4 `repository-proof-prepared` is completed by IndexModel
        // once the projector has returned — that is the mandatory terminal
        // snapshot (§7.1).
        progress?.CompleteCurrentStage();

        return new LiveRepositoryAnalysisResult(
            analysis,
            snapshotResult.MetadataAvailability);
    }

    public RepositoryReport Project(
        RepositoryAnalysis analysis,
        string? locale)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return reportProjector.Project(analysis, locale);
    }
}

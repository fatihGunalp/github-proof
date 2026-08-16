using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;

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

    public async Task<LiveRepositoryAnalysisResult> AnalyzeAsync(
        string repositoryUrl,
        AnalysisVersion version,
        Guid analysisId,
        CancellationToken cancellationToken = default)
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

        RepositoryAnalysis analysis = analysisRunner.Analyze(
            snapshot,
            version,
            analysisId);

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

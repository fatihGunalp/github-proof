using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Application.Analysis;

public sealed class RepositoryAnalysisRunner
{
    private readonly RepositorySignalAnalyzer signalAnalyzer;
    private readonly RepositoryAnalysisComposer composer;
    private readonly TimeProvider timeProvider;

    public RepositoryAnalysisRunner(TimeProvider? timeProvider = null)
    {
        this.timeProvider = timeProvider ?? TimeProvider.System;
        signalAnalyzer = RepositorySignalAnalyzer.CreateDefault(this.timeProvider);
        composer = new RepositoryAnalysisComposer();
    }

    public RepositoryAnalysis Analyze(
        RepositorySnapshot snapshot,
        AnalysisVersion version,
        Guid analysisId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(version);

        if (analysisId == Guid.Empty)
        {
            throw new ArgumentException(
                "Analysis ID cannot be empty.",
                nameof(analysisId));
        }

        DateTimeOffset startedAt = timeProvider.GetUtcNow();
        IReadOnlyList<SignalResult> signalResults =
            signalAnalyzer.Analyze(snapshot);
        DateTimeOffset completedAt = timeProvider.GetUtcNow();

        return composer.Compose(
            snapshot,
            signalResults,
            analysisId,
            version,
            startedAt,
            completedAt);
    }
}

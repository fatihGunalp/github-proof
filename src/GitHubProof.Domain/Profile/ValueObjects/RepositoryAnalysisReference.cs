using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Profile.ValueObjects;

// Identifies the exact repository-level analysis consumed by profile analysis.
// It carries stable identity only; it never duplicates RepositoryAnalysis.
public sealed record RepositoryAnalysisReference
{
    public RepositoryAnalysisReference(
        Guid analysisId,
        RepositoryIdentity repository,
        AnalysisVersion version)
    {
        if (analysisId == Guid.Empty)
        {
            throw new ArgumentException(
                "Analysis ID cannot be empty.",
                nameof(analysisId));
        }

        AnalysisId = analysisId;
        Repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        Version = version
            ?? throw new ArgumentNullException(nameof(version));
    }

    public Guid AnalysisId { get; }

    public RepositoryIdentity Repository { get; }

    public AnalysisVersion Version { get; }
}

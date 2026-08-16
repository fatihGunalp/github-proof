using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Web.Analysis;

public sealed record LiveRepositoryAnalysisResult
{
    public LiveRepositoryAnalysisResult(
        RepositoryAnalysis? analysis,
        DataAvailability metadataAvailability)
    {
        Analysis = analysis;
        MetadataAvailability = metadataAvailability
            ?? throw new ArgumentNullException(nameof(metadataAvailability));

        if (analysis is not null && !metadataAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "A live repository analysis requires available repository metadata.",
                nameof(metadataAvailability));
        }

        if (analysis is null && metadataAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "Available repository metadata must produce a live repository analysis.",
                nameof(metadataAvailability));
        }
    }

    public RepositoryAnalysis? Analysis { get; }

    public DataAvailability MetadataAvailability { get; }

    public bool IsSuccessful => Analysis is not null;
}

using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Web.Profile;

public sealed record LiveProfileAnalysisResult
{
    public LiveProfileAnalysisResult(
        ProfilePipelineResult? pipelineResult,
        DataAvailability profileAvailability)
    {
        PipelineResult = pipelineResult;
        ProfileAvailability = profileAvailability
            ?? throw new ArgumentNullException(nameof(profileAvailability));

        if (pipelineResult is not null && !profileAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "A live profile pipeline result requires available profile metadata.",
                nameof(profileAvailability));
        }

        if (pipelineResult is null && profileAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "Available profile metadata must produce a live profile pipeline result.",
                nameof(profileAvailability));
        }
    }

    public ProfilePipelineResult? PipelineResult { get; }

    public DataAvailability ProfileAvailability { get; }

    public bool IsSuccessful => PipelineResult is not null;
}

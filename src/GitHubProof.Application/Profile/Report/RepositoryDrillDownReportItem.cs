using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile.Report;

// Connects profile truth back to existing repository-level truth by
// reference only: profile claim -> repository -> repository analysis
// (report contract §12). It never embeds or duplicates RepositoryAnalysis.
public sealed class RepositoryDrillDownReportItem
{
    public RepositoryDrillDownReportItem(
        ProfileRepositoryReference repository,
        RepositoryEligibilityStatus eligibilityStatus,
        RepositoryAnalysisReference? analysisReference)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        if (!Enum.IsDefined(eligibilityStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(eligibilityStatus),
                eligibilityStatus,
                "Repository eligibility status is not defined.");
        }

        EligibilityStatus = eligibilityStatus;
        AnalysisReference = analysisReference;
    }

    public ProfileRepositoryReference Repository { get; }

    public RepositoryEligibilityStatus EligibilityStatus { get; }

    public RepositoryAnalysisReference? AnalysisReference { get; }
}

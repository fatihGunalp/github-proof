namespace GitHubProof.Application.Profile.Report;

// Whether a report section's capability exists and ran in this product
// version (public-profile-evidence-report-contract.md §6). Independent of
// item emptiness: Supported + empty is a real analytical result, never a
// negative developer conclusion.
public enum ProfileReportSectionAvailability
{
    Supported = 1,
    Unsupported = 2,
    Deferred = 3
}

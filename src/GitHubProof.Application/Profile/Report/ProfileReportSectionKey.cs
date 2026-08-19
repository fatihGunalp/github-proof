namespace GitHubProof.Application.Profile.Report;

// The canonical V1 report section identities, in canonical order
// (public-profile-evidence-report-contract.md §5, §7). Locale never changes
// this set or this order.
public enum ProfileReportSectionKey
{
    PublicProfileSummary = 1,
    TechnicalFocusAreas = 2,
    EngineeringPracticePatterns = 3,
    ProblemAreaPatterns = 4,
    RepresentativeRepositories = 5,
    WhyThisConclusion = 6,
    Limitations = 7,
    RepositoryDrillDown = 8,
    RoleEvidenceAlignment = 9
}

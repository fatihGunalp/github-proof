using GitHubProof.Application.Profile.Report;

namespace GitHubProof.Application.Tests.Profile.Validation;

// The §7 Unsupported/Deferred acceptance check locked by
// docs/product/p9-profile-validation-contract.md §7. It asserts only the
// already-locked V1 boundary (report contract §6.2/§6.3):
//
//     ProblemAreaPatterns:      Availability = Unsupported, Items = []
//     RoleEvidenceAlignment:    Availability = Deferred,    Items = []
//
// It introduces no new capability state and does not activate either
// capability — it only proves the existing boundary held for one real
// pipeline run.
internal static class ProfileValidationCapabilityBoundary
{
    public static bool ProblemAreaIsUnsupportedAndEmpty(ProfileReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return report.ProblemAreaPatterns.Availability == ProfileReportSectionAvailability.Unsupported
            && report.ProblemAreaPatterns.Items.Count == 0;
    }

    public static bool RoleEvidenceAlignmentIsDeferredAndEmpty(ProfileReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return report.RoleEvidenceAlignment.Availability == ProfileReportSectionAvailability.Deferred
            && report.RoleEvidenceAlignment.Items.Count == 0;
    }

    public static bool HoldsFor(ProfileReport report) =>
        ProblemAreaIsUnsupportedAndEmpty(report)
        && RoleEvidenceAlignmentIsDeferredAndEmpty(report);
}

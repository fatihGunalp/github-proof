namespace GitHubProof.Domain.Profile.Enums;

// Processing completeness of a valid DeveloperPublicEvidenceProfile.
//
// Failed is deliberately absent: a failed composition produces no canonical
// profile root at all, so it belongs to the composition result boundary
// rather than to a profile that exists.
public enum ProfileAnalysisStatus
{
    Complete = 1,
    Partial = 2
}

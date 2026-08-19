namespace GitHubProof.Domain.Profile.Enums;

// The canonical V1 profile warning identities. A warning states which unusual,
// unavailable, incomplete or uncertain processing condition occurred; none of
// them is a statement about the developer.
//
// No identity may be added without a versioned contract change.
public enum ProfileWarningCode
{
    RepositoryEvidenceUnavailable = 1,
    RepositoryEvidenceRateLimited = 2,
    RepositoryAnalysisFailed = 3,
    RepositoryEligibilityUndetermined = 4
}

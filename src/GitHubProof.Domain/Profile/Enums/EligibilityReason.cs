namespace GitHubProof.Domain.Profile.Enums;

// The canonical V1 eligibility reason identities are closed by the locked
// repository eligibility contract; no identity may be added without a new
// contract version.
public enum EligibilityReason
{
    ForkDetected = 1,
    TemplateDerived = 2,
    Archived = 3,
    Historical = 4,
    EmptyRepository = 5,
    InsufficientEvidence = 6,
    NonIndependentContribution = 7,
    IndependenceUndetermined = 8,
    RequiredInputUnavailable = 9
}

namespace GitHubProof.Domain.Analysis.Enums;

public enum SignalStatus
{
    Strong = 1,
    Present = 2,
    Partial = 3,
    Missing = 4,
    NotApplicable = 5,
    UnavailableEvidence = 6,
    RequiresHumanReview = 7
}

namespace GitHubProof.Domain.Profile.Enums;

// The canonical V1 profile limitation identities. A limitation states how far
// the result may safely be interpreted given the evidence surface actually
// available; it is never a negative developer conclusion.
//
// No identity may be added without a versioned contract change.
public enum ProfileLimitationCode
{
    LimitedIndependentEvidenceSurface = 1,
    PartialEvidenceSurface = 2
}

namespace GitHubProof.Domain.Profile.Enums;

// Outcome of one profile composition attempt. Failed lives here, outside the
// canonical profile root, so no consumer ever has to check whether the root it
// received is real truth or a failure placeholder.
public enum ProfileCompositionStatus
{
    Complete = 1,
    Partial = 2,
    Failed = 3
}

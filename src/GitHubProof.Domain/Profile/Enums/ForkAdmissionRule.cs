namespace GitHubProof.Domain.Profile.Enums;

// How one eligibility policy version admits a repository whose normalized
// manifest provenance deterministically reports a fork.
//
// The rule belongs immutably to the EligibilityPolicyVersion that carries it,
// exactly as CurrentActivityWindow does
// (docs/architecture/public-profile-evidence-repository-eligibility.md §19.3,
// §19.3.2). Changing it requires a new version.
public enum ForkAdmissionRule
{
    // Policy 1.0.0 behavior: fork lineage alone does not require Skip, and the
    // restriction is applied later by IndependentRepetition only.
    Analyze = 1,

    // Policy 2.0.0 behavior (Session 098 §6.3): a known fork is skipped before
    // any repository collection, with reason ForkDetected. Profile V1 owns no
    // attribution contract able to prove which fork changes belong to the
    // profile owner, so a fork must not be used as developer evidence.
    Skip = 2
}

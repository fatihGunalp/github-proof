namespace GitHubProof.Domain.Profile.Enums;

// The one derived overall status of a RepositoryEligibilityResult. It is never
// an independent source of truth: it is derived from the purpose-specific
// contribution decisions and must never contradict them.
public enum RepositoryEligibilityStatus
{
    Undetermined = 0,
    Contributing = 1,
    Restricted = 2,
    NonContributing = 3
}

namespace GitHubProof.Domain.Profile.Enums;

// The canonical V1 scope of a profile limitation or warning. A Repository
// scoped condition carries the repositories it concerns; a Profile scoped
// condition bounds the profile-level evidence surface as a whole and carries
// none, and that absence is not a missing value.
public enum ProfileConditionScope
{
    Profile = 1,
    Repository = 2
}

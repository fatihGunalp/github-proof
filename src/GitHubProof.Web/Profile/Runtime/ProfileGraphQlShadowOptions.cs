namespace GitHubProof.Web.Profile.Runtime;

// The smallest configuration S2 needs, and deliberately not a feature-flag
// framework: one boolean, off by default
// (docs/architecture/public-profile-runtime-collection.md §§18-19).
//
// While it is false the Profile path is byte-for-byte the Session 099
// composition: no shadow is constructed, no GraphQL request is issued, and the
// request graph and result are exactly what they are today. Turning it on adds
// an observation only — REST stays authoritative for the languages surface, so
// this is not a switch between two behaviours.
public sealed class ProfileGraphQlShadowOptions
{
    public const string ConfigurationSectionName = "ProfileGraphQlShadow";

    public bool LanguagesShadowEnabled { get; set; }
}

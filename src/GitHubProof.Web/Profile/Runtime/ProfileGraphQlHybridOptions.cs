namespace GitHubProof.Web.Profile.Runtime;

// The smallest configuration S3 needs: one boolean, off by default
// (docs/architecture/public-profile-runtime-collection.md §§18-19, Session 127).
//
// It is a separate section from ProfileGraphQlShadow rather than a second flag
// on it, because the two are not variations of one feature. The shadow observes
// and changes nothing; this switch makes GraphQL AUTHORITATIVE for repository
// metadata, topics and languages. Merging them would also have renamed a
// configuration key already in use.
//
// While it is false the Profile path is exactly the Session 099 composition:
// no envelope source is constructed, no authoritative GraphQL request is issued,
// and the request graph and result are what they are today.
//
// It stays DEFAULT OFF and is not approved for production, beta, the §19.14
// live A/B validation or default-on use. The Session 126 residual — a secondary
// rate limit that GitHub identifies only through the error message — is still
// unresolved, and that blocker is unchanged by this slice.
public sealed class ProfileGraphQlHybridOptions
{
    public const string ConfigurationSectionName = "ProfileGraphQlHybrid";

    // When true, the authoritative repository envelope replaces the REST
    // repository metadata GET and the REST languages GET for every repository
    // whose candidate is accepted. It also SUPERSEDES the languages shadow: the
    // envelope already supplies that surface, so the shadow is not constructed
    // and no second GraphQL request per repository can be issued.
    public bool RepositoryEnvelopeEnabled { get; set; }
}

namespace GitHubProof.Infrastructure.GitHub.Profile;

// The result of one Profile-only languages shadow comparison
// (docs/architecture/public-profile-runtime-collection.md §§18-19, slice S2).
//
// This is NOT a domain status. It never becomes a DataAvailability, a signal
// status, a warning, a limitation or evidence of any kind: REST stays
// authoritative for the languages surface and this value is discarded by the
// production path (§18.1, §18.7).
public enum GraphQlLanguagesShadowOutcome
{
    // The comparison did not run, or could not be trusted: the shadow is
    // disabled for this call, the REST languages surface was not Available,
    // the GraphQL envelope was not a clean Success, or the GraphQL payload was
    // structurally ambiguous. Deliberately NOT a mismatch — an unavailable
    // comparison says nothing about parity.
    NotCompared,

    // The complete normalized GraphQL map equals the complete normalized REST
    // map.
    Match,

    // Both maps were read completely and they differ.
    Mismatch
}

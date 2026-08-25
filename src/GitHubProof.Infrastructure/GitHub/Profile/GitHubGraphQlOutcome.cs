namespace GitHubProof.Infrastructure.GitHub.Profile;

// The four response states
// docs/architecture/public-profile-runtime-collection.md §18.4 requires to be
// structurally distinguishable before any surface may be served over GraphQL.
//
// This is a TRANSPORT outcome, never a domain status. It is deliberately not
// mapped onto DataAvailability, AnalysisStatus or any signal status: §18.7
// leaves availability to the collector that actually supplies the surface, and
// forbids a GraphQL-specific evidence status.
public enum GitHubGraphQlOutcome
{
    // HTTP success, a `data` object, and no `errors`. The only state from which
    // an absence may later be read as an observation (§18.4).
    Success,

    // HTTP 200 carrying `data` AND a non-empty `errors` array. §18.4's blocking
    // case: this must never be represented as ordinary success, because an
    // absent or empty field here is unresolved, not observed-empty.
    DataWithErrors,

    // Non-success HTTP status, or the request never produced a response.
    // §18.6 calls this envelope-level.
    EnvelopeFailure,

    // A response body that could not be read as a GraphQL envelope at all:
    // unparsable, empty, or carrying neither usable `data` nor `errors`.
    // Kept separate from Success so a malformed body can never become
    // successful absence.
    Malformed
}

namespace GitHubProof.Infrastructure.GitHub.Profile;

// A bounded, transport-only classification of an HTTP 200 GraphQL envelope that
// nevertheless reports a provider-wide condition
// (docs/architecture/public-profile-runtime-collection.md §18.6, which already
// names "a GraphQL primary or secondary rate-limit response" as envelope-level).
//
// Every member is decided from a response HEADER, never from error text. No
// message, code, path or extension is read, and none is retained. This is not a
// domain status and never becomes one: it exists so §18.6 can act on the two
// conditions GitHub reports structurally.
public enum GitHubGraphQlProviderSignal
{
    // Nothing in the response headers reports a provider-wide condition. An
    // errored envelope with this signal stays a per-repository abstention.
    None,

    // `x-ratelimit-remaining: 0`. GitHub answers a GraphQL primary rate-limit
    // exhaustion with HTTP 200 and an error, and documents that further requests
    // should not be made until the window resets. Whether *this* error is the
    // rate-limit error cannot be proven without reading its text — but the
    // provider has said there are zero points left, which is enough on its own
    // to stop attempting GraphQL for the rest of the execution.
    RateLimitExhausted,

    // A present and parseable `Retry-After`. GitHub sends it with some secondary
    // rate-limit responses, including ones carried on HTTP 200. Its value is
    // deliberately not retained: this slice implements no retry or backoff, so
    // the bounded fact that the provider asked us to wait is all that is needed.
    RetryAfterRequested
}

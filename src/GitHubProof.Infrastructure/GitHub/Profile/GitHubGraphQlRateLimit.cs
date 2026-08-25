namespace GitHubProof.Infrastructure.GitHub.Profile;

// A bounded structural observation of GitHub's GraphQL `rateLimit` field.
//
// Numeric and time fields only — no query text, no identity, no payload. It
// describes the GraphQL provider resource and nothing else: §18.9 forbids
// summing, averaging or presenting GraphQL and REST rate-limit observations as
// one budget, and the §10.4 / §11.3 proactive budget decision continues to read
// the REST profile-metadata response only.
//
// Every field is nullable because the caller decides whether to ask for
// `rateLimit` at all, and a partial object is still a legitimate observation.
public sealed record GitHubGraphQlRateLimit(
    int? Cost,
    int? Limit,
    int? Remaining,
    int? Used,
    DateTimeOffset? ResetAt);

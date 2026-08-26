namespace GitHubProof.Infrastructure.GitHub.Profile;

// The Profile-only authoritative repository envelope seam (§§18-19, slice S3).
//
// Unlike the S2 shadow, this one SUPPLIES surfaces: when it returns an envelope,
// repository metadata, topics and languages come from GraphQL and the two REST
// requests that would have produced them are never issued.
//
// `null` is the only failure shape. It means "collect this repository through
// the existing REST composition", and it covers every §18.4/§18.5/§18.6 case
// alike — an errored envelope, a malformed body, a missing repository object, a
// missing or invalid adopted field, and an incomplete topics or languages
// connection. A caller therefore cannot accidentally act on a partial result,
// because no partial result exists to act on.
public interface IGitHubGraphQlRepositoryEnvelopeSource
{
    Task<GitHubGraphQlRepositoryEnvelope?> ResolveAsync(
        GitHubRepositoryUrl repository,
        CancellationToken cancellationToken = default);
}

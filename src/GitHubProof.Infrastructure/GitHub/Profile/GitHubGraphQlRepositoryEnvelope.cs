using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// One ACCEPTED authoritative repository envelope (Session 127, S3).
//
// It exists only when a single GraphQL response completely supplied all three
// adopted surfaces — repository metadata, topics and the complete language map
// — and every adopted field validated. There is no partial form: a candidate
// that fails any check produces no envelope at all, and the repository is
// collected through the proven REST composition instead (§18.5 case 3).
//
// `RetrievedAt` is the single instant §18.7 requires: captured once, after the
// body was read, parsed, validated and accepted, and shared by every surface
// this envelope supplies. Surfaces that stay on REST keep their own.
//
// Deliberately absent: license, default-branch head/tree identity, commits,
// pull requests, releases, tags, README and file contents. Those are §18.2 REST
// retentions or unadopted candidates, and this type carries no field for them
// so no code path can quietly start serving one.
public sealed record GitHubGraphQlRepositoryEnvelope(
    RepositoryIdentity Repository,
    RepositoryMetadata Metadata,
    IReadOnlyList<string> Topics,
    IReadOnlyDictionary<string, long> Languages,
    DateTimeOffset RetrievedAt);

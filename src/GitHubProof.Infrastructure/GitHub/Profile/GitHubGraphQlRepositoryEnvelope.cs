using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// One ACCEPTED authoritative repository envelope (Session 127, S3; extended by
// Session 129).
//
// It exists only when a single GraphQL response completely supplied every
// adopted surface — repository metadata, topics, the complete language map and
// the default-branch head/tree identity — and every adopted field validated.
// There is no partial form: a candidate that fails any check produces no
// envelope at all, and the repository is collected through the proven REST
// composition instead (§18.5 case 3).
//
// `HeadTree` is REQUIRED, which is the whole-envelope rule stated as a type.
// A repository whose `defaultBranchRef` is null — the shape an EMPTY repository
// answers with — cannot produce an envelope at all, so this type can never
// carry, imply or be read as successful emptiness. §18.13 leaves emptiness to
// the REST 409 rule, and the REST path is reached precisely because no envelope
// exists.
//
// `RetrievedAt` is the single instant §18.7 requires: captured once, after the
// body was read, parsed, validated and accepted, and shared by every EVIDENCE
// surface this envelope supplies. Surfaces that stay on REST keep their own.
// `HeadTree` is not among them: it is structural input to collection rather
// than an observed surface, and the recursive file tree it anchors keeps the
// REST collector's own `file-tree` instant.
//
// Deliberately absent: license, the recursive file tree, commits, pull
// requests, releases, tags, README and file contents. Those are §18.2 REST
// retentions or unadopted candidates, and this type carries no field for them
// so no code path can quietly start serving one.
public sealed record GitHubGraphQlRepositoryEnvelope(
    RepositoryIdentity Repository,
    RepositoryMetadata Metadata,
    IReadOnlyList<string> Topics,
    IReadOnlyDictionary<string, long> Languages,
    GitHubRepositoryHeadTreeIdentity HeadTree,
    DateTimeOffset RetrievedAt);

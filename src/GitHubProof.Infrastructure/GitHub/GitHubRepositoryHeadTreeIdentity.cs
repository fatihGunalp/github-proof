namespace GitHubProof.Infrastructure.GitHub;

// The default-branch head commit SHA and that commit's root tree SHA, already
// resolved for this repository by another transport.
//
// It exists so a composition that has already obtained both identities can hand
// them to the contents collector instead of making that collector issue its own
// `GET /git/ref/heads/{branch}` and `GET /git/commits/{sha}`. The recursive file
// tree itself is NOT part of this: it stays REST
// (docs/architecture/public-profile-runtime-collection.md §18.2, Session 129),
// because GraphQL exposes no recursive tree and no `truncated` equivalent, and
// `truncated` is what drives `FileTree.IsComplete`.
//
// The only composition that supplies one today is the Profile hybrid repository
// envelope; the Repository Proof composition supplies nothing and resolves both
// identities over REST exactly as it always has.
//
// Both members are the Git object identities Git itself defines — the object the
// default-branch ref points to, and the single root tree of that commit — so
// they are the same values the two REST responses carry, not a provider-computed
// restatement of them.
//
// Deliberately NOT carrying a `RetrievedAt`: these are structural INPUT to
// collection, not an observed evidence surface. The `file-tree` availability
// keeps the contents collector's own instant, because the recursive tree request
// is still the observation that produced it (§18.7).
public sealed record GitHubRepositoryHeadTreeIdentity(
    string HeadCommitSha,
    string RootTreeSha);

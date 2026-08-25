namespace GitHubProof.Infrastructure.GitHub.Profile;

// The Profile-only, read-only GraphQL transport seam
// (docs/architecture/public-profile-runtime-collection.md §§18-19, slice S1).
//
// It sends one physical POST /graphql and reports a structurally distinguishable
// transport outcome. It owns no surface, no collector, no availability and no
// evidence, and after S1 nothing in the production Profile collection path
// resolves it.
public interface IGitHubGraphQlTransport
{
    Task<GitHubGraphQlResponse> SendAsync(
        GitHubGraphQlRequest request,
        CancellationToken cancellationToken = default);
}

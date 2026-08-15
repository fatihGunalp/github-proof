using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryPullRequestCollection(
    RepositoryCollection<GitHubRepositoryPullRequest> PullRequests,
    DataAvailability Availability,
    IReadOnlyList<string> CollectionWarnings)
{
    public bool IsComplete => PullRequests.IsComplete;
}

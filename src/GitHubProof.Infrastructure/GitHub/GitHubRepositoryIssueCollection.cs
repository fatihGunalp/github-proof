using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryIssueCollection(
    RepositoryCollection<GitHubRepositoryIssue> Issues,
    DataAvailability Availability,
    IReadOnlyList<string> CollectionWarnings)
{
    public bool IsComplete => Issues.IsComplete;
}

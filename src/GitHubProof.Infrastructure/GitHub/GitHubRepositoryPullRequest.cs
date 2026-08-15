namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryPullRequest
{
    public GitHubRepositoryPullRequest(
        int number,
        bool isClosed,
        bool isMerged,
        DateTimeOffset createdAt)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Pull request number must be positive.");
        }

        if (isMerged && !isClosed)
        {
            throw new ArgumentException(
                "A merged pull request must be closed.",
                nameof(isClosed));
        }

        Number = number;
        IsClosed = isClosed;
        IsMerged = isMerged;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public int Number { get; }

    public bool IsClosed { get; }

    public bool IsMerged { get; }

    public DateTimeOffset CreatedAt { get; }
}

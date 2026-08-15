namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryIssue
{
    public GitHubRepositoryIssue(int number, bool isClosed, DateTimeOffset createdAt)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Issue number must be positive.");
        }

        Number = number;
        IsClosed = isClosed;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public int Number { get; }

    public bool IsClosed { get; }

    public DateTimeOffset CreatedAt { get; }
}

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryPullRequest
{
    public GitHubRepositoryPullRequest(
        int number,
        string title,
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
        Title = RequireValue(title, nameof(title));
        IsClosed = isClosed;
        IsMerged = isMerged;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public int Number { get; }

    public string Title { get; }

    public bool IsClosed { get; }

    public bool IsMerged { get; }

    public DateTimeOffset CreatedAt { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }
}

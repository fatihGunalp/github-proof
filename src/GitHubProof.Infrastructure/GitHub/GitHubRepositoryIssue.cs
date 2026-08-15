namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryIssue
{
    public GitHubRepositoryIssue(
        int number,
        string title,
        string? body,
        bool isClosed,
        DateTimeOffset createdAt)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Issue number must be positive.");
        }

        Number = number;
        Title = RequireValue(title, nameof(title));
        Body = string.IsNullOrWhiteSpace(body) ? null : body.Trim();
        IsClosed = isClosed;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public int Number { get; }

    public string Title { get; }

    public string? Body { get; }

    public bool IsClosed { get; }

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

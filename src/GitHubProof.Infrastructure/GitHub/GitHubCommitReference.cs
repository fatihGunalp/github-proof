namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubCommitReference
{
    private const int ShaLength = 40;

    private GitHubCommitReference(string commitSha)
    {
        CommitSha = commitSha;
    }

    public string CommitSha { get; }

    public static GitHubCommitReference Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "GitHub commit SHA cannot be null, empty, or whitespace.",
                nameof(value));
        }

        string normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length != ShaLength || normalized.Any(character => !IsHexDigit(character)))
        {
            throw new ArgumentException(
                "A full 40-character hexadecimal GitHub commit SHA is required.",
                nameof(value));
        }

        return new GitHubCommitReference(normalized);
    }

    public override string ToString() => CommitSha;

    private static bool IsHexDigit(char character) =>
        character is >= '0' and <= '9'
            or >= 'a' and <= 'f';
}

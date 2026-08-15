namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryTag
{
    public GitHubRepositoryTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tag name cannot be null, empty, or whitespace.", nameof(name));
        }

        Name = name.Trim();
    }

    public string Name { get; }
}

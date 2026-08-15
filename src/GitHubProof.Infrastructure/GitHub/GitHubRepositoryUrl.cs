namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryUrl
{
    private const string GitHubHost = "github.com";

    private GitHubRepositoryUrl(string owner, string name, Uri canonicalUri)
    {
        Owner = owner;
        Name = name;
        CanonicalUri = canonicalUri;
    }

    public string Owner { get; }

    public string Name { get; }

    public Uri CanonicalUri { get; }

    public static GitHubRepositoryUrl Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "GitHub repository URL cannot be null, empty, or whitespace.",
                nameof(value));
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(uri.Host, GitHubHost, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "A canonical HTTPS github.com repository URL is required.",
                nameof(value));
        }

        string[] segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 2
            || string.IsNullOrWhiteSpace(segments[0])
            || string.IsNullOrWhiteSpace(segments[1]))
        {
            throw new ArgumentException(
                "GitHub repository URL must identify exactly one owner and repository.",
                nameof(value));
        }

        string owner = segments[0];
        string name = segments[1];
        Uri canonicalUri = new(
            $"https://{GitHubHost}/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(name)}",
            UriKind.Absolute);

        return new GitHubRepositoryUrl(owner, name, canonicalUri);
    }
}

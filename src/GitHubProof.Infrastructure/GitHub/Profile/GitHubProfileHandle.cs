using System.Text.RegularExpressions;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed partial record GitHubProfileHandle
{
    private const string GitHubHost = "github.com";

    private GitHubProfileHandle(string login, Uri canonicalUri)
    {
        Login = login;
        CanonicalUri = canonicalUri;
    }

    public string Login { get; }

    public Uri CanonicalUri { get; }

    public static GitHubProfileHandle Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "GitHub profile identity cannot be null, empty, or whitespace.",
                nameof(value));
        }

        string candidate = value.Trim();
        string login = candidate.Contains("://", StringComparison.Ordinal)
            ? ParseProfileUrl(candidate)
            : candidate;

        if (!LoginPattern().IsMatch(login))
        {
            throw new ArgumentException(
                "A valid GitHub username is required.",
                nameof(value));
        }

        return new GitHubProfileHandle(
            login,
            new Uri($"https://{GitHubHost}/{login}", UriKind.Absolute));
    }

    private static string ParseProfileUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(uri.Host, GitHubHost, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "A canonical HTTPS github.com profile URL is required.",
                nameof(value));
        }

        string[] segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 1)
        {
            throw new ArgumentException(
                "GitHub profile URL must identify exactly one profile.",
                nameof(value));
        }

        return segments[0];
    }

    [GeneratedRegex(
        "^[A-Za-z0-9](?:[A-Za-z0-9]|-(?=[A-Za-z0-9])){0,38}$",
        RegexOptions.CultureInvariant)]
    private static partial Regex LoginPattern();
}

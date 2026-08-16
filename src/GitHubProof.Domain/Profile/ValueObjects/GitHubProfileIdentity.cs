namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record GitHubProfileIdentity
{
    public GitHubProfileIdentity(string login, Uri url)
    {
        Login = ProfileValue.RequireValue(login, nameof(login));
        Url = url ?? throw new ArgumentNullException(nameof(url));

        if (!Url.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "Profile URL must be absolute.",
                nameof(url));
        }

        if (!string.Equals(
                Url.Scheme,
                Uri.UriSchemeHttp,
                StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                Url.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Profile URL must use HTTP or HTTPS.",
                nameof(url));
        }
    }

    public string Login { get; }

    public Uri Url { get; }
}

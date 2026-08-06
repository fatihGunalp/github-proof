namespace GitHubProof.Domain.Analysis.ValueObjects;

public sealed record RepositoryIdentity
{
    public RepositoryIdentity(
        string owner,
        string name,
        Uri url,
        string defaultBranch,
        bool isPublic,
        bool isArchived,
        bool isFork,
        bool isTemplate,
        string? primaryLanguage)
    {
        Owner = RequireValue(owner, nameof(owner));
        Name = RequireValue(name, nameof(name));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        DefaultBranch = RequireValue(defaultBranch, nameof(defaultBranch));
        IsPublic = isPublic;
        IsArchived = isArchived;
        IsFork = isFork;
        IsTemplate = isTemplate;
        PrimaryLanguage = NormalizeOptional(primaryLanguage);

        if (!Url.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "Repository URL must be absolute.",
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
                "Repository URL must use HTTP or HTTPS.",
                nameof(url));
        }
    }

    public string Owner { get; }

    public string Name { get; }

    public Uri Url { get; }

    public string DefaultBranch { get; }

    public bool IsPublic { get; }

    public bool IsArchived { get; }

    public bool IsFork { get; }

    public bool IsTemplate { get; }

    public string? PrimaryLanguage { get; }

    public string FullName => $"{Owner}/{Name}";

    private static string RequireValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}

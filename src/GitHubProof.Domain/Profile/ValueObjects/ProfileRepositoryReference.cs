namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record ProfileRepositoryReference
    : IComparable<ProfileRepositoryReference>, ISnapshotSemanticComponent
{
    public ProfileRepositoryReference(string owner, string name, Uri url)
    {
        Owner = ProfileValue.RequireValue(owner, nameof(owner));
        Name = ProfileValue.RequireValue(name, nameof(name));
        Url = url ?? throw new ArgumentNullException(nameof(url));

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

    public string FullName => $"{Owner}/{Name}";

    public void WriteSemanticContent(SnapshotSemanticWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer
            .Write(nameof(Owner), Owner)
            .Write(nameof(Name), Name)
            .Write(nameof(Url), Url);
    }

    public int CompareTo(ProfileRepositoryReference? other)
    {
        if (other is null)
        {
            return 1;
        }

        int owner = string.CompareOrdinal(Owner, other.Owner);

        return owner != 0
            ? owner
            : string.CompareOrdinal(Name, other.Name);
    }
}

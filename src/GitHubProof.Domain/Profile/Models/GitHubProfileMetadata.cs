using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed record GitHubProfileMetadata : ISnapshotSemanticComponent
{
    public GitHubProfileMetadata(
        string? displayName,
        DateTimeOffset createdAt,
        int publicRepositoryCount,
        Uri avatarUrl)
    {
        DisplayName = ProfileValue.NormalizeOptional(displayName);
        CreatedAt = createdAt.ToUniversalTime();

        if (publicRepositoryCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(publicRepositoryCount),
                publicRepositoryCount,
                "Public repository count cannot be negative.");
        }

        PublicRepositoryCount = publicRepositoryCount;
        AvatarUrl = RequireAbsoluteHttpsUri(avatarUrl, nameof(avatarUrl));
    }

    public string? DisplayName { get; }

    public DateTimeOffset CreatedAt { get; }

    public int PublicRepositoryCount { get; }

    // The provider-returned canonical GitHub avatar image. Identity/profile
    // presentation metadata only: it carries no evidence, warning, limitation,
    // pattern or report semantics of its own.
    public Uri AvatarUrl { get; }

    public void WriteSemanticContent(SnapshotSemanticWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer
            .Write(nameof(DisplayName), DisplayName)
            .Write(nameof(CreatedAt), CreatedAt)
            .Write(nameof(PublicRepositoryCount), PublicRepositoryCount)
            .Write(nameof(AvatarUrl), AvatarUrl);
    }

    private static Uri RequireAbsoluteHttpsUri(Uri value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (!value.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "Avatar URL must be absolute.",
                parameterName);
        }

        if (!string.Equals(
                value.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Avatar URL must use HTTPS.",
                parameterName);
        }

        return value;
    }
}

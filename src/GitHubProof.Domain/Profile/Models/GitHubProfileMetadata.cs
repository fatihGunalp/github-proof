using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed record GitHubProfileMetadata : ISnapshotSemanticComponent
{
    public GitHubProfileMetadata(
        string? displayName,
        DateTimeOffset createdAt,
        int publicRepositoryCount)
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
    }

    public string? DisplayName { get; }

    public DateTimeOffset CreatedAt { get; }

    public int PublicRepositoryCount { get; }

    public void WriteSemanticContent(SnapshotSemanticWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer
            .Write(nameof(DisplayName), DisplayName)
            .Write(nameof(CreatedAt), CreatedAt)
            .Write(nameof(PublicRepositoryCount), PublicRepositoryCount);
    }
}

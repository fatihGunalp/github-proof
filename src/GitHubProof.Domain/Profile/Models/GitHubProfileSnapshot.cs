using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed record GitHubProfileSnapshot
{
    public GitHubProfileSnapshot(
        GitHubProfileIdentity identity,
        DateTimeOffset capturedAt,
        GitHubProfileMetadata? metadata,
        ProfileRepositoryManifest? repositories,
        IEnumerable<DataAvailability>? availability,
        IEnumerable<string>? collectionWarnings)
    {
        Identity = identity
            ?? throw new ArgumentNullException(nameof(identity));
        CapturedAt = capturedAt.ToUniversalTime();
        Metadata = metadata;
        Repositories = repositories ?? ProfileRepositoryManifest.Empty;
        Availability = NormalizeAvailability(availability);
        CollectionWarnings = NormalizeWarnings(collectionWarnings);
    }

    public GitHubProfileIdentity Identity { get; }

    public DateTimeOffset CapturedAt { get; }

    public GitHubProfileMetadata? Metadata { get; }

    public ProfileRepositoryManifest Repositories { get; }

    public IReadOnlyList<DataAvailability> Availability { get; }

    public IReadOnlyList<string> CollectionWarnings { get; }

    public DataAvailability? FindAvailability(string dataKey)
    {
        string normalizedKey = ProfileValue.RequireValue(
            dataKey,
            nameof(dataKey));

        return Availability.SingleOrDefault(item => string.Equals(
            item.DataKey,
            normalizedKey,
            StringComparison.Ordinal));
    }

    private static IReadOnlyList<DataAvailability> NormalizeAvailability(
        IEnumerable<DataAvailability>? availability)
    {
        if (availability is null)
        {
            return Array.Empty<DataAvailability>();
        }

        List<DataAvailability> normalized = [];
        HashSet<string> keys = new(StringComparer.Ordinal);

        foreach (DataAvailability item in availability)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!keys.Add(item.DataKey))
            {
                throw new ArgumentException(
                    $"Duplicate availability key '{item.DataKey}'.",
                    nameof(availability));
            }

            normalized.Add(item);
        }

        return normalized.AsReadOnly();
    }

    private static IReadOnlyList<string> NormalizeWarnings(
        IEnumerable<string>? warnings)
    {
        if (warnings is null)
        {
            return Array.Empty<string>();
        }

        return warnings
            .Where(warning => !string.IsNullOrWhiteSpace(warning))
            .Select(warning => warning.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

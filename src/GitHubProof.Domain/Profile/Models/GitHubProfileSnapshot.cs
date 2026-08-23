using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed record GitHubProfileSnapshot : ISnapshotSemanticComponent
{
    public GitHubProfileSnapshot(
        GitHubProfileIdentity identity,
        DateTimeOffset capturedAt,
        GitHubProfileMetadata? metadata,
        ProfileRepositoryManifest? repositories,
        IEnumerable<DataAvailability>? availability,
        IEnumerable<string>? collectionWarnings,
        RepositoryPresentationMetadata? repositoryPresentationMetadata = null)
    {
        Identity = identity
            ?? throw new ArgumentNullException(nameof(identity));
        CapturedAt = capturedAt.ToUniversalTime();
        Metadata = metadata;
        Repositories = repositories ?? ProfileRepositoryManifest.Empty;
        Availability = NormalizeAvailability(availability);
        CollectionWarnings = NormalizeWarnings(collectionWarnings);
        RepositoryPresentationMetadata = repositoryPresentationMetadata
            ?? RepositoryPresentationMetadata.Empty;
    }

    public GitHubProfileIdentity Identity { get; }

    public DateTimeOffset CapturedAt { get; }

    public GitHubProfileMetadata? Metadata { get; }

    public ProfileRepositoryManifest Repositories { get; }

    public IReadOnlyList<DataAvailability> Availability { get; }

    public IReadOnlyList<string> CollectionWarnings { get; }

    // Session 111 STEP 3: non-evidence repository presentation metadata
    // (currently Description only), looked up by canonical
    // ProfileRepositoryReference. Deliberately excluded from
    // WriteSemanticContent below — see SnapshotIdentityTests' IsDerived
    // allow-list.
    public RepositoryPresentationMetadata RepositoryPresentationMetadata { get; }

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

    // The canonical normalized semantic representation of this snapshot.
    //
    // CapturedAt is deliberately absent: it answers when this snapshot was
    // observed, not what was observed. The same exclusion applies to
    // DataAvailability.RetrievedAt below, which is observation time under a
    // different field name.
    //
    // Collections whose enumeration order carries no meaning are written in a
    // canonical order here, so insertion or API order never participates.
    public void WriteSemanticContent(SnapshotSemanticWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer
            .WriteComponent(nameof(Identity), Identity)
            .WriteComponent(nameof(Metadata), Metadata)
            .WriteComponent(nameof(Repositories), Repositories);

        int index = 0;

        foreach (DataAvailability availability in Availability
                     .OrderBy(item => item.DataKey, StringComparer.Ordinal))
        {
            writer
                .Write($"{nameof(Availability)}[{index}].{nameof(DataAvailability.DataKey)}", availability.DataKey)
                .Write($"{nameof(Availability)}[{index}].{nameof(DataAvailability.Status)}", availability.Status)
                .Write($"{nameof(Availability)}[{index}].{nameof(DataAvailability.Reason)}", availability.Reason);
            index++;
        }

        writer
            .Write($"{nameof(Availability)}.count", index)
            .WriteValues(
                nameof(CollectionWarnings),
                CollectionWarnings.Order(StringComparer.Ordinal));
    }

    // Provenance projection of this snapshot. The snapshot owns it, so no
    // consumer has to assemble provenance from a field list of its own.
    public SnapshotReference CreateReference() =>
        new(Identity, CapturedAt, SnapshotIdentity.FromSemanticContent(this));

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

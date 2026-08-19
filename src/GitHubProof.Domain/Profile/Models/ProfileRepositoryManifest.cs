using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed class ProfileRepositoryManifest : ISnapshotSemanticComponent
{
    public ProfileRepositoryManifest(
        IEnumerable<ProfileRepository>? repositories = null,
        int? totalCount = null,
        bool isComplete = true)
    {
        List<ProfileRepository> normalized = repositories?.ToList() ?? [];

        if (normalized.Any(repository => repository is null))
        {
            throw new ArgumentException(
                "Manifest entries cannot contain null values.",
                nameof(repositories));
        }

        HashSet<ProfileRepositoryReference> references = [];

        foreach (ProfileRepository repository in normalized)
        {
            if (!references.Add(repository.Reference))
            {
                throw new ArgumentException(
                    $"Duplicate repository '{repository.Reference.FullName}'.",
                    nameof(repositories));
            }
        }

        if (totalCount < normalized.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalCount),
                totalCount,
                "Total count cannot be less than the collected repository count.");
        }

        normalized.Sort(static (left, right) =>
            left.Reference.CompareTo(right.Reference));

        Repositories = new ReadOnlyCollection<ProfileRepository>(normalized);
        TotalCount = totalCount ?? (isComplete ? normalized.Count : null);
        IsComplete = isComplete;
    }

    public IReadOnlyList<ProfileRepository> Repositories { get; }

    public int? TotalCount { get; }

    public bool IsComplete { get; }

    public static ProfileRepositoryManifest Empty { get; } = new();

    // Repositories are already normalized into canonical Owner then Name order
    // by construction, so collection insertion order never participates.
    public void WriteSemanticContent(SnapshotSemanticWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer
            .WriteComponents(nameof(Repositories), Repositories)
            .Write(nameof(TotalCount), TotalCount)
            .Write(nameof(IsComplete), IsComplete);
    }
}

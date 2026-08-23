using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Session 111 STEP 3: repository identity/context metadata supplied by
// GitHub — currently the repository Description only. It is never evidence,
// never a pattern, never part of representative-repository selection and
// never a ProfileReport section. It exists solely so Web can look up a
// canonical repository's provider-returned Description for presentation,
// keyed by the same ProfileRepositoryReference identity used everywhere else.
// It is excluded from GitHubProfileSnapshot's semantic content: it carries no
// evidence/eligibility/determinism meaning of its own.
public sealed class RepositoryPresentationMetadata
{
    private readonly IReadOnlyDictionary<ProfileRepositoryReference, string?> descriptionsByRepository;

    public RepositoryPresentationMetadata(
        IEnumerable<KeyValuePair<ProfileRepositoryReference, string?>> descriptionsByRepository)
    {
        ArgumentNullException.ThrowIfNull(descriptionsByRepository);

        this.descriptionsByRepository = new ReadOnlyDictionary<ProfileRepositoryReference, string?>(
            descriptionsByRepository.ToDictionary(
                pair => pair.Key,
                pair => pair.Value));
    }

    public static RepositoryPresentationMetadata Empty { get; } =
        new(Array.Empty<KeyValuePair<ProfileRepositoryReference, string?>>());

    // Null both when the repository is unknown to this metadata set and when
    // GitHub itself returned no Description. The two cases are presented
    // identically: absence of a short description, never a claim about the
    // repository's quality, activity or documentation.
    public string? FindDescription(ProfileRepositoryReference repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return descriptionsByRepository.TryGetValue(repository, out string? description)
            ? description
            : null;
    }
}

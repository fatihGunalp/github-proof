using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// One repository's representative-selection win for one pattern. The
// repository must be one of Pattern.IndependentSupportingRepositories (§4):
// a repository outside that set can never become a support, enforced here so
// no caller can bypass §4 by constructing this type directly. Pattern is the
// Canonical Pattern Trace: an in-memory reference to the exact
// RepeatedProfilePattern instance, for identity and canonical pattern context
// only. It is never a representative-specific evidence path, not even for
// this repository, and it is never read here to reconstruct evidence.
// SupportingEvidenceReferences/SupportingObservationKeys are the
// Representative Support Evidence Trace: the sole source explaining why this
// repository was selected. That scoped evidence is computed once, during
// candidate evaluation, by the caller (RepresentativeRepositorySelectionPolicy)
// and carried here as-is — this constructor only normalizes and validates
// what it is given, ordered by EvidenceKey/ObservationKey ordinal.
public sealed class RepresentativePatternSupport
{
    public RepresentativePatternSupport(
        RepeatedProfilePattern pattern,
        ProfileRepositoryReference repository,
        IEnumerable<ProfileEvidenceReference> supportingEvidenceReferences)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        ArgumentNullException.ThrowIfNull(repository);

        if (!pattern.IndependentSupportingRepositories.Contains(repository))
        {
            throw new ArgumentException(
                $"Repository '{repository.FullName}' is not an independent supporting repository of pattern '{pattern.PatternKey}'.",
                nameof(repository));
        }

        PatternFamily = pattern.Family;
        PatternKey = pattern.PatternKey;
        SupportingEvidenceReferences = NormalizeSupportingEvidence(
            supportingEvidenceReferences,
            repository,
            pattern.PatternKey);
    }

    public RepeatedProfilePattern Pattern { get; }

    public ProfilePatternFamily PatternFamily { get; }

    public string PatternKey { get; }

    public IReadOnlyList<ProfileEvidenceReference> SupportingEvidenceReferences { get; }

    // A live projection, never a stored second source of truth: re-derived
    // from SupportingEvidenceReferences on every access, so it cannot be
    // mutated into divergence — even a caller holding the underlying array
    // (e.g. via a cast) only ever tampers with a disposable snapshot, and
    // the next read is unaffected and still correct.
    public IReadOnlyList<string> SupportingObservationKeys =>
        SupportingEvidenceReferences
            .Select(reference => reference.EvidenceKey)
            .ToArray();

    public int PatternSupportObservationCount => SupportingEvidenceReferences.Count;

    private static IReadOnlyList<ProfileEvidenceReference> NormalizeSupportingEvidence(
        IEnumerable<ProfileEvidenceReference> supportingEvidenceReferences,
        ProfileRepositoryReference repository,
        string patternKey)
    {
        ArgumentNullException.ThrowIfNull(supportingEvidenceReferences);

        List<ProfileEvidenceReference> normalized = supportingEvidenceReferences.ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException(
                $"Repository '{repository.FullName}' has no supporting evidence for pattern '{patternKey}'.",
                nameof(supportingEvidenceReferences));
        }

        foreach (ProfileEvidenceReference reference in normalized)
        {
            ArgumentNullException.ThrowIfNull(reference);

            if (reference.Repository != repository)
            {
                throw new ArgumentException(
                    $"Supporting evidence for pattern '{patternKey}' does not belong to repository '{repository.FullName}'.",
                    nameof(supportingEvidenceReferences));
            }
        }

        normalized.Sort(static (left, right) =>
            string.CompareOrdinal(left.EvidenceKey, right.EvidenceKey));

        return new ReadOnlyCollection<ProfileEvidenceReference>(normalized);
    }
}

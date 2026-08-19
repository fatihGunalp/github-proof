using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Groups every pattern for which the same repository was independently
// selected (grouping is a result-shape concern only; it never performs or
// influences selection across patterns). Carries no version or completeness
// field of its own: those live once, at RepresentativeRepositorySelection.
public sealed class RepresentativeRepository
{
    public RepresentativeRepository(
        ProfileRepositoryReference repository,
        IEnumerable<RepresentativePatternSupport> patternSupports)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        PatternSupports = NormalizePatternSupports(patternSupports, Repository);
    }

    public ProfileRepositoryReference Repository { get; }

    public IReadOnlyList<RepresentativePatternSupport> PatternSupports { get; }

    private static IReadOnlyList<RepresentativePatternSupport> NormalizePatternSupports(
        IEnumerable<RepresentativePatternSupport> patternSupports,
        ProfileRepositoryReference repository)
    {
        ArgumentNullException.ThrowIfNull(patternSupports);

        List<RepresentativePatternSupport> normalized = patternSupports.ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException(
                $"Repository '{repository.FullName}' requires at least one represented pattern.",
                nameof(patternSupports));
        }

        HashSet<(ProfilePatternFamily Family, string PatternKey)> identities = [];

        foreach (RepresentativePatternSupport support in normalized)
        {
            ArgumentNullException.ThrowIfNull(support);

            if (support.SupportingEvidenceReferences[0].Repository != repository)
            {
                throw new ArgumentException(
                    $"Pattern support for '{support.PatternKey}' does not belong to repository '{repository.FullName}'.",
                    nameof(patternSupports));
            }

            if (!identities.Add((support.PatternFamily, support.PatternKey)))
            {
                throw new ArgumentException(
                    $"Repository '{repository.FullName}' repeats pattern '{support.PatternKey}'.",
                    nameof(patternSupports));
            }
        }

        normalized.Sort(static (left, right) =>
        {
            int family = left.PatternFamily.CompareTo(right.PatternFamily);

            return family != 0
                ? family
                : string.CompareOrdinal(left.PatternKey, right.PatternKey);
        });

        return new ReadOnlyCollection<RepresentativePatternSupport>(normalized);
    }
}

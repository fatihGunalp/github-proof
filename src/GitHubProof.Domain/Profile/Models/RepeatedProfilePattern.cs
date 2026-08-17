using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Canonical profile-level pattern truth: evidence that repeats across two or
// more independent public repositories. It carries no strength, confidence,
// ranking or capability meaning.
public sealed class RepeatedProfilePattern
{
    public const int MinimumIndependentSupportingRepositories = 2;

    public RepeatedProfilePattern(
        ProfilePatternFamily family,
        string patternKey,
        IEnumerable<ObservedProfileEvidence> supportingObservations,
        IEnumerable<ProfileRepositoryReference> independentSupportingRepositories,
        PatternRuleVersion ruleVersion)
    {
        if (!Enum.IsDefined(family))
        {
            throw new ArgumentOutOfRangeException(
                nameof(family),
                family,
                "Profile pattern family is not defined.");
        }

        Family = family;
        PatternKey = ProfileValue.RequireValue(patternKey, nameof(patternKey));
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
        SupportingObservations = NormalizeObservations(
            supportingObservations,
            PatternKey);
        ObservationRuleVersion = SupportingObservations[0].RuleVersion;
        SupportingRepositories = NormalizeSupportingRepositories(
            SupportingObservations);
        IndependentSupportingRepositories = NormalizeIndependentRepositories(
            independentSupportingRepositories,
            SupportingRepositories,
            PatternKey);
    }

    public ProfilePatternFamily Family { get; }

    public string PatternKey { get; }

    public IReadOnlyList<ObservedProfileEvidence> SupportingObservations { get; }

    public IReadOnlyList<ProfileRepositoryReference> SupportingRepositories { get; }

    public IReadOnlyList<ProfileRepositoryReference> IndependentSupportingRepositories { get; }

    public PatternRuleVersion RuleVersion { get; }

    public ObservationRuleVersion ObservationRuleVersion { get; }

    public int SupportingRepositoryCount => SupportingRepositories.Count;

    public int IndependentSupportingRepositoryCount =>
        IndependentSupportingRepositories.Count;

    private static IReadOnlyList<ObservedProfileEvidence> NormalizeObservations(
        IEnumerable<ObservedProfileEvidence> supportingObservations,
        string patternKey)
    {
        ArgumentNullException.ThrowIfNull(supportingObservations);

        List<ObservedProfileEvidence> normalized = supportingObservations.ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException(
                $"Pattern '{patternKey}' requires at least one supporting observation.",
                nameof(supportingObservations));
        }

        HashSet<string> keys = new(StringComparer.Ordinal);
        ObservationRuleVersion ruleVersion = normalized[0]?.RuleVersion
            ?? throw new ArgumentException(
                "Supporting observations cannot contain null values.",
                nameof(supportingObservations));

        foreach (ObservedProfileEvidence observation in normalized)
        {
            ArgumentNullException.ThrowIfNull(observation);

            if (!keys.Add(observation.ObservationKey))
            {
                throw new ArgumentException(
                    $"Pattern '{patternKey}' repeats observation '{observation.ObservationKey}'.",
                    nameof(supportingObservations));
            }

            if (observation.RuleVersion != ruleVersion)
            {
                throw new ArgumentException(
                    $"Pattern '{patternKey}' mixes observation rule versions.",
                    nameof(supportingObservations));
            }
        }

        normalized.Sort(static (left, right) => string.CompareOrdinal(
            left.ObservationKey,
            right.ObservationKey));

        return new ReadOnlyCollection<ObservedProfileEvidence>(normalized);
    }

    private static IReadOnlyList<ProfileRepositoryReference> NormalizeSupportingRepositories(
        IReadOnlyList<ObservedProfileEvidence> observations)
    {
        List<ProfileRepositoryReference> repositories = [];
        HashSet<ProfileRepositoryReference> observed = [];

        foreach (ProfileRepositoryReference repository in observations
            .SelectMany(observation => observation.ContributingRepositories))
        {
            if (observed.Add(repository))
            {
                repositories.Add(repository);
            }
        }

        repositories.Sort(static (left, right) => left.CompareTo(right));

        return new ReadOnlyCollection<ProfileRepositoryReference>(repositories);
    }

    private static IReadOnlyList<ProfileRepositoryReference> NormalizeIndependentRepositories(
        IEnumerable<ProfileRepositoryReference> independentSupportingRepositories,
        IReadOnlyList<ProfileRepositoryReference> supportingRepositories,
        string patternKey)
    {
        ArgumentNullException.ThrowIfNull(independentSupportingRepositories);

        List<ProfileRepositoryReference> normalized =
            independentSupportingRepositories.ToList();
        HashSet<ProfileRepositoryReference> supporting = [.. supportingRepositories];
        HashSet<ProfileRepositoryReference> observed = [];

        foreach (ProfileRepositoryReference repository in normalized)
        {
            ArgumentNullException.ThrowIfNull(repository);

            if (!supporting.Contains(repository))
            {
                throw new ArgumentException(
                    $"Repository '{repository.FullName}' does not support pattern '{patternKey}'.",
                    nameof(independentSupportingRepositories));
            }

            if (!observed.Add(repository))
            {
                throw new ArgumentException(
                    $"Repository '{repository.FullName}' repeats in pattern '{patternKey}'.",
                    nameof(independentSupportingRepositories));
            }
        }

        if (normalized.Count < MinimumIndependentSupportingRepositories)
        {
            throw new ArgumentException(
                $"Pattern '{patternKey}' requires at least {MinimumIndependentSupportingRepositories} independent supporting repositories.",
                nameof(independentSupportingRepositories));
        }

        normalized.Sort(static (left, right) => left.CompareTo(right));

        return new ReadOnlyCollection<ProfileRepositoryReference>(normalized);
    }
}

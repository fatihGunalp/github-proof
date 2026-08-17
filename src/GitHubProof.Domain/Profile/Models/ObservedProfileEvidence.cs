using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// One deterministic quantified profile-level observation. It counts explicit
// supporting repository evidence only; it never asserts absence and carries no
// weighting, ranking or capability meaning.
public sealed class ObservedProfileEvidence
{
    public ObservedProfileEvidence(
        string observationKey,
        ObservedProfileEvidenceKind kind,
        IEnumerable<ProfileEvidenceReference> evidenceReferences,
        int independentSupportingRepositoryCount,
        int eligibleRepositoryCount,
        int undeterminedEvidenceRepositoryCount,
        ObservationRuleVersion ruleVersion)
    {
        ObservationKey = ProfileValue.RequireValue(
            observationKey,
            nameof(observationKey));

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "Observed profile evidence kind is not defined.");
        }

        Kind = kind;
        EvidenceReferences = NormalizeReferences(
            evidenceReferences,
            ObservationKey);
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));

        IndependentSupportingRepositoryCount = RequireNotNegative(
            independentSupportingRepositoryCount,
            nameof(independentSupportingRepositoryCount));
        EligibleRepositoryCount = RequireNotNegative(
            eligibleRepositoryCount,
            nameof(eligibleRepositoryCount));
        UndeterminedEvidenceRepositoryCount = RequireNotNegative(
            undeterminedEvidenceRepositoryCount,
            nameof(undeterminedEvidenceRepositoryCount));

        if (IndependentSupportingRepositoryCount > SupportingRepositoryCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(independentSupportingRepositoryCount),
                independentSupportingRepositoryCount,
                "Independent supporting repositories cannot exceed supporting repositories.");
        }

        if (SupportingRepositoryCount + UndeterminedEvidenceRepositoryCount
            > EligibleRepositoryCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(eligibleRepositoryCount),
                eligibleRepositoryCount,
                "Supporting and undetermined repositories cannot exceed the eligible repositories.");
        }
    }

    public string ObservationKey { get; }

    public ObservedProfileEvidenceKind Kind { get; }

    public IReadOnlyList<ProfileEvidenceReference> EvidenceReferences { get; }

    public ObservationRuleVersion RuleVersion { get; }

    public int SupportingRepositoryCount => EvidenceReferences.Count;

    public int IndependentSupportingRepositoryCount { get; }

    public int EligibleRepositoryCount { get; }

    public int UndeterminedEvidenceRepositoryCount { get; }

    public IEnumerable<ProfileRepositoryReference> ContributingRepositories =>
        EvidenceReferences.Select(reference => reference.Repository);

    private static IReadOnlyList<ProfileEvidenceReference> NormalizeReferences(
        IEnumerable<ProfileEvidenceReference> evidenceReferences,
        string observationKey)
    {
        ArgumentNullException.ThrowIfNull(evidenceReferences);

        List<ProfileEvidenceReference> normalized = evidenceReferences.ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException(
                "An observation requires at least one supporting evidence reference.",
                nameof(evidenceReferences));
        }

        HashSet<ProfileRepositoryReference> repositories = [];

        foreach (ProfileEvidenceReference reference in normalized)
        {
            ArgumentNullException.ThrowIfNull(reference);

            if (!string.Equals(
                    reference.EvidenceKey,
                    observationKey,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Evidence reference '{reference.EvidenceKey}' does not support observation '{observationKey}'.",
                    nameof(evidenceReferences));
            }

            if (!repositories.Add(reference.Repository))
            {
                throw new ArgumentException(
                    $"Repository '{reference.Repository.FullName}' supports observation '{observationKey}' more than once.",
                    nameof(evidenceReferences));
            }
        }

        normalized.Sort(static (left, right) =>
            left.Repository.CompareTo(right.Repository));

        return new ReadOnlyCollection<ProfileEvidenceReference>(normalized);
    }

    private static int RequireNotNegative(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Repository count cannot be negative.");
        }

        return value;
    }
}

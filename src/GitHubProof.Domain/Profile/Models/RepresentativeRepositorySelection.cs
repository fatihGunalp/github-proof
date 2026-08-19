using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// The representative-selection result boundary: the single coherent output of
// running selection over one ProfilePatternSet instance, in one execution. It
// carries no stable or persisted reference back to that ProfilePatternSet
// instance; instead it copies the source set's version and completeness
// context directly, as plain values, alongside its own dedicated
// RepresentativeRepositoryRuleVersion. RepresentativeRepositories is zero or
// more: zero patterns produce zero representatives, which is a valid result,
// not a failure. The constructor enforces, over the whole result: no pattern
// (Family+PatternKey) is represented under two different repositories; every
// referenced RepeatedProfilePattern shares this selection's own
// PatternRuleVersion and ObservationRuleVersion (§12.1 — never a mix of two
// ProfilePatternSet instances or version combinations); and no two
// repositories compare equal under ProfileRepositoryReference.CompareTo
// (Owner+Name) while being unequal as values (§6.1) — such a non-canonical
// collision is rejected deterministically rather than silently accepted.
public sealed class RepresentativeRepositorySelection
{
    public RepresentativeRepositorySelection(
        IEnumerable<RepresentativeRepository> representativeRepositories,
        PatternRuleVersion patternRuleVersion,
        ObservationRuleVersion observationRuleVersion,
        EligibilityPolicyVersion eligibilityPolicyVersion,
        bool isEvidenceSurfaceComplete,
        RepresentativeRepositoryRuleVersion ruleVersion)
    {
        PatternRuleVersion = patternRuleVersion
            ?? throw new ArgumentNullException(nameof(patternRuleVersion));
        ObservationRuleVersion = observationRuleVersion
            ?? throw new ArgumentNullException(nameof(observationRuleVersion));
        EligibilityPolicyVersion = eligibilityPolicyVersion
            ?? throw new ArgumentNullException(nameof(eligibilityPolicyVersion));
        IsEvidenceSurfaceComplete = isEvidenceSurfaceComplete;
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
        RepresentativeRepositories = NormalizeRepositories(
            representativeRepositories,
            PatternRuleVersion,
            ObservationRuleVersion);
    }

    public IReadOnlyList<RepresentativeRepository> RepresentativeRepositories { get; }

    public PatternRuleVersion PatternRuleVersion { get; }

    public ObservationRuleVersion ObservationRuleVersion { get; }

    public EligibilityPolicyVersion EligibilityPolicyVersion { get; }

    public bool IsEvidenceSurfaceComplete { get; }

    public RepresentativeRepositoryRuleVersion RuleVersion { get; }

    private static IReadOnlyList<RepresentativeRepository> NormalizeRepositories(
        IEnumerable<RepresentativeRepository> representativeRepositories,
        PatternRuleVersion patternRuleVersion,
        ObservationRuleVersion observationRuleVersion)
    {
        ArgumentNullException.ThrowIfNull(representativeRepositories);

        List<RepresentativeRepository> normalized = representativeRepositories.ToList();
        HashSet<ProfileRepositoryReference> observedRepositories = [];
        Dictionary<(string Owner, string Name), ProfileRepositoryReference> canonicalIdentities = [];
        HashSet<(ProfilePatternFamily Family, string PatternKey)> observedPatterns = [];

        foreach (RepresentativeRepository representative in normalized)
        {
            ArgumentNullException.ThrowIfNull(representative);

            if (!observedRepositories.Add(representative.Repository))
            {
                throw new ArgumentException(
                    $"Repository '{representative.Repository.FullName}' appears more than once after grouping.",
                    nameof(representativeRepositories));
            }

            (string Owner, string Name) identityKey =
                (representative.Repository.Owner, representative.Repository.Name);

            if (canonicalIdentities.TryGetValue(
                    identityKey,
                    out ProfileRepositoryReference? existing))
            {
                if (existing != representative.Repository)
                {
                    throw new ArgumentException(
                        $"Repository '{representative.Repository.FullName}' has two non-canonical references within one selection.",
                        nameof(representativeRepositories));
                }
            }
            else
            {
                canonicalIdentities[identityKey] = representative.Repository;
            }

            foreach (RepresentativePatternSupport support in representative.PatternSupports)
            {
                if (!observedPatterns.Add((support.PatternFamily, support.PatternKey)))
                {
                    throw new ArgumentException(
                        $"Pattern '{support.PatternKey}' is represented by more than one repository within one selection.",
                        nameof(representativeRepositories));
                }

                if (support.Pattern.RuleVersion != patternRuleVersion)
                {
                    throw new ArgumentException(
                        $"Pattern '{support.PatternKey}' was produced under a different pattern rule version than this selection.",
                        nameof(representativeRepositories));
                }

                if (support.Pattern.ObservationRuleVersion != observationRuleVersion)
                {
                    throw new ArgumentException(
                        $"Pattern '{support.PatternKey}' consumed a different observation rule version than this selection.",
                        nameof(representativeRepositories));
                }
            }
        }

        normalized.Sort(static (left, right) =>
            left.Repository.CompareTo(right.Repository));

        return new ReadOnlyCollection<RepresentativeRepository>(normalized);
    }
}

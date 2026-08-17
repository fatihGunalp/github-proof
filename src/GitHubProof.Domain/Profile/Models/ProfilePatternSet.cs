using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Canonical container for promoted profile patterns. The family views are
// projections over one canonical pattern collection, never parallel stores.
public sealed class ProfilePatternSet
{
    public ProfilePatternSet(
        IEnumerable<RepeatedProfilePattern> patterns,
        PatternRuleVersion ruleVersion,
        ObservationRuleVersion observationRuleVersion,
        EligibilityPolicyVersion policyVersion,
        bool isEvidenceSurfaceComplete)
    {
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
        ObservationRuleVersion = observationRuleVersion
            ?? throw new ArgumentNullException(nameof(observationRuleVersion));
        PolicyVersion = policyVersion
            ?? throw new ArgumentNullException(nameof(policyVersion));
        IsEvidenceSurfaceComplete = isEvidenceSurfaceComplete;
        Patterns = NormalizePatterns(
            patterns,
            ruleVersion,
            observationRuleVersion);
    }

    public IReadOnlyList<RepeatedProfilePattern> Patterns { get; }

    public PatternRuleVersion RuleVersion { get; }

    public ObservationRuleVersion ObservationRuleVersion { get; }

    public EligibilityPolicyVersion PolicyVersion { get; }

    // False when the underlying repository manifest was incomplete, so no
    // consumer may read this pattern set as an exhaustive evidence surface.
    public bool IsEvidenceSurfaceComplete { get; }

    public IEnumerable<RepeatedProfilePattern> TechnicalFocusPatterns =>
        Patterns.Where(pattern =>
            pattern.Family == ProfilePatternFamily.TechnicalFocus);

    public IEnumerable<RepeatedProfilePattern> EngineeringPracticePatterns =>
        Patterns.Where(pattern =>
            pattern.Family == ProfilePatternFamily.EngineeringPractice);

    public IEnumerable<RepeatedProfilePattern> ProblemAreaPatterns =>
        Patterns.Where(pattern =>
            pattern.Family == ProfilePatternFamily.ProblemArea);

    private static IReadOnlyList<RepeatedProfilePattern> NormalizePatterns(
        IEnumerable<RepeatedProfilePattern> patterns,
        PatternRuleVersion ruleVersion,
        ObservationRuleVersion observationRuleVersion)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        List<RepeatedProfilePattern> normalized = patterns.ToList();
        HashSet<(ProfilePatternFamily Family, string PatternKey)> identities = [];

        foreach (RepeatedProfilePattern pattern in normalized)
        {
            ArgumentNullException.ThrowIfNull(pattern);

            if (pattern.RuleVersion != ruleVersion)
            {
                throw new ArgumentException(
                    $"Pattern '{pattern.PatternKey}' was produced under a different pattern rule version.",
                    nameof(patterns));
            }

            if (pattern.ObservationRuleVersion != observationRuleVersion)
            {
                throw new ArgumentException(
                    $"Pattern '{pattern.PatternKey}' consumed a different observation rule version.",
                    nameof(patterns));
            }

            if (!identities.Add((pattern.Family, pattern.PatternKey)))
            {
                throw new ArgumentException(
                    $"Duplicate {pattern.Family} pattern '{pattern.PatternKey}'.",
                    nameof(patterns));
            }
        }

        normalized.Sort(static (left, right) =>
        {
            int family = left.Family.CompareTo(right.Family);

            return family != 0
                ? family
                : string.CompareOrdinal(left.PatternKey, right.PatternKey);
        });

        return new ReadOnlyCollection<RepeatedProfilePattern>(normalized);
    }
}

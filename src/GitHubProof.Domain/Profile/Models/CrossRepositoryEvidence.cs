using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Canonical container for deterministic cross-repository observations. It is
// structured aggregate evidence only: no pattern promotion, no ranking, no
// developer-level claim.
public sealed class CrossRepositoryEvidence
{
    public CrossRepositoryEvidence(
        IEnumerable<RepositoryContributionAssessment> repositoryContributions,
        IEnumerable<ObservedProfileEvidence> observations,
        ObservationRuleVersion ruleVersion,
        EligibilityPolicyVersion policyVersion,
        bool isEvidenceSurfaceComplete)
    {
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
        PolicyVersion = policyVersion
            ?? throw new ArgumentNullException(nameof(policyVersion));
        IsEvidenceSurfaceComplete = isEvidenceSurfaceComplete;
        RepositoryContributions = NormalizeContributions(
            repositoryContributions,
            policyVersion);
        Observations = NormalizeObservations(
            observations,
            RepositoryContributions,
            ruleVersion);
    }

    public IReadOnlyList<RepositoryContributionAssessment> RepositoryContributions { get; }

    public IReadOnlyList<ObservedProfileEvidence> Observations { get; }

    public ObservationRuleVersion RuleVersion { get; }

    public EligibilityPolicyVersion PolicyVersion { get; }

    // False when the underlying profile repository manifest was incomplete, so
    // no consumer may read these counts as an exhaustive evidence surface.
    public bool IsEvidenceSurfaceComplete { get; }

    public int EligibleRepositoryCount =>
        RepositoryContributions.Count(contribution =>
            contribution.ProfileObservation.IsAllowed);

    public int UndeterminedContributionRepositoryCount =>
        RepositoryContributions.Count(contribution =>
            contribution.ProfileObservation.Decision
                == ContributionDecision.Undetermined);

    private static IReadOnlyList<RepositoryContributionAssessment> NormalizeContributions(
        IEnumerable<RepositoryContributionAssessment> contributions,
        EligibilityPolicyVersion policyVersion)
    {
        ArgumentNullException.ThrowIfNull(contributions);

        List<RepositoryContributionAssessment> normalized = contributions.ToList();
        HashSet<ProfileRepositoryReference> references = [];
        AnalysisVersion? analysisVersion = null;

        foreach (RepositoryContributionAssessment contribution in normalized)
        {
            ArgumentNullException.ThrowIfNull(contribution);

            if (contribution.PolicyVersion != policyVersion)
            {
                throw new ArgumentException(
                    $"Repository '{contribution.Reference.FullName}' was assessed under a different eligibility policy version.",
                    nameof(contributions));
            }

            if (contribution.AnalysisReference is not null)
            {
                analysisVersion ??= contribution.AnalysisReference.Version;

                if (contribution.AnalysisReference.Version != analysisVersion)
                {
                    throw new ArgumentException(
                        $"Repository '{contribution.Reference.FullName}' was analyzed under a different analysis version.",
                        nameof(contributions));
                }
            }

            if (!references.Add(contribution.Reference))
            {
                throw new ArgumentException(
                    $"Duplicate repository contribution '{contribution.Reference.FullName}'.",
                    nameof(contributions));
            }
        }

        normalized.Sort(static (left, right) =>
            left.Reference.CompareTo(right.Reference));

        return new ReadOnlyCollection<RepositoryContributionAssessment>(normalized);
    }

    private static IReadOnlyList<ObservedProfileEvidence> NormalizeObservations(
        IEnumerable<ObservedProfileEvidence> observations,
        IReadOnlyList<RepositoryContributionAssessment> contributions,
        ObservationRuleVersion ruleVersion)
    {
        ArgumentNullException.ThrowIfNull(observations);

        List<ObservedProfileEvidence> normalized = observations.ToList();
        Dictionary<ProfileRepositoryReference, RepositoryContributionAssessment> byReference =
            contributions.ToDictionary(contribution => contribution.Reference);
        int eligibleCount = contributions.Count(contribution =>
            contribution.ProfileObservation.IsAllowed);
        HashSet<string> keys = new(StringComparer.Ordinal);

        foreach (ObservedProfileEvidence observation in normalized)
        {
            ArgumentNullException.ThrowIfNull(observation);

            if (observation.RuleVersion != ruleVersion)
            {
                throw new ArgumentException(
                    $"Observation '{observation.ObservationKey}' was produced under a different observation rule version.",
                    nameof(observations));
            }

            if (!keys.Add(observation.ObservationKey))
            {
                throw new ArgumentException(
                    $"Duplicate observation '{observation.ObservationKey}'.",
                    nameof(observations));
            }

            if (observation.EligibleRepositoryCount != eligibleCount)
            {
                throw new ArgumentException(
                    $"Observation '{observation.ObservationKey}' does not use the eligible repository count of this evidence surface.",
                    nameof(observations));
            }

            ValidateTraceability(observation, byReference);
        }

        normalized.Sort(static (left, right) => string.CompareOrdinal(
            left.ObservationKey,
            right.ObservationKey));

        return new ReadOnlyCollection<ObservedProfileEvidence>(normalized);
    }

    private static void ValidateTraceability(
        ObservedProfileEvidence observation,
        IReadOnlyDictionary<ProfileRepositoryReference, RepositoryContributionAssessment> byReference)
    {
        int independentCount = 0;

        foreach (ProfileEvidenceReference evidence in observation.EvidenceReferences)
        {
            if (!byReference.TryGetValue(
                    evidence.Repository,
                    out RepositoryContributionAssessment? contribution))
            {
                throw new ArgumentException(
                    $"Observation '{observation.ObservationKey}' references repository '{evidence.Repository.FullName}' outside this evidence surface.",
                    nameof(observation));
            }

            if (!contribution.ProfileObservation.IsAllowed)
            {
                throw new ArgumentException(
                    $"Repository '{evidence.Repository.FullName}' may not contribute to observation '{observation.ObservationKey}'.",
                    nameof(observation));
            }

            if (contribution.AnalysisReference != evidence.Analysis)
            {
                throw new ArgumentException(
                    $"Observation '{observation.ObservationKey}' references a different repository analysis for '{evidence.Repository.FullName}'.",
                    nameof(observation));
            }

            if (contribution.IndependentRepetition.IsAllowed)
            {
                independentCount++;
            }
        }

        if (observation.IndependentSupportingRepositoryCount != independentCount)
        {
            throw new ArgumentException(
                $"Observation '{observation.ObservationKey}' does not match the independent repetition decisions of its contributing repositories.",
                nameof(observation));
        }
    }
}

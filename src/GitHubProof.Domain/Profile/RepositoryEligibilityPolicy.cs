using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile;

// Completes Stage B of the locked repository eligibility contract. It reuses
// RepositoryContributionPolicy for the two purposes cross-repository evidence
// already depends on, and resolves the remaining purposes plus the derived
// overall status on top of that result.
//
// It answers only what a repository may safely contribute evidence toward. It
// is not repository quality and never increases the epistemic strength of the
// repository evidence it governs.
public sealed class RepositoryEligibilityPolicy
{
    private readonly RepositoryContributionPolicy contributionPolicy;

    public RepositoryEligibilityPolicy(EligibilityPolicy policy)
    {
        Policy = policy
            ?? throw new ArgumentNullException(nameof(policy));
        contributionPolicy = new RepositoryContributionPolicy(policy.Version);
    }

    public EligibilityPolicy Policy { get; }

    public RepositoryEligibilityResult Evaluate(
        ProfileRepository repository,
        AnalysisAdmissionResult admission,
        AnalysisStatus? analysisStatus,
        RepositoryAnalysisReference? analysisReference,
        DateTimeOffset snapshotCapturedAt)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(admission);

        RepositoryContributionAssessment contribution = contributionPolicy.Evaluate(
            repository,
            admission,
            analysisStatus,
            analysisReference);

        ActivityClassification activity = ClassifyActivity(
            repository,
            snapshotCapturedAt);

        return new RepositoryEligibilityResult(
            contribution,
            repository.Archived,
            activity,
            ResolveCurrentFocus(repository, contribution, activity),
            ResolveHistoricalEvidence(contribution));
    }

    // The cutoff is anchored to the snapshot capture time and the window owned
    // by this policy version. No system clock participates, and a missing
    // source-control activity timestamp never falls back to Active.
    private ActivityClassification ClassifyActivity(
        ProfileRepository repository,
        DateTimeOffset snapshotCapturedAt)
    {
        if (repository.LastPushedAt is not DateTimeOffset activityTimestamp)
        {
            return ActivityClassification.Undetermined;
        }

        return activityTimestamp >= Policy.ResolveActivityCutoff(snapshotCapturedAt)
            ? ActivityClassification.Active
            : ActivityClassification.Historical;
    }

    private static ContributionAssessment ResolveCurrentFocus(
        ProfileRepository repository,
        RepositoryContributionAssessment contribution,
        ActivityClassification activity)
    {
        ContributionAssessment observation = contribution.ProfileObservation;

        // A deterministic evidence-surface exclusion (empty or insufficient
        // evidence) restricts every evidence-bearing purpose under its own
        // reason.
        if (observation.Decision == ContributionDecision.NotAllowed)
        {
            return ContributionAssessment.NotAllowed([.. observation.Reasons]);
        }

        // Manifest-only lifecycle restrictions are sufficient on their own and
        // resolve first, whether or not the evidence surface was established.
        if (repository.Archived == RepositoryConditionState.Present)
        {
            return ContributionAssessment.NotAllowed(EligibilityReason.Archived);
        }

        if (activity == ActivityClassification.Historical)
        {
            return ContributionAssessment.NotAllowed(EligibilityReason.Historical);
        }

        // Repository evidence contribution itself could not be established, so
        // no permissive current-focus decision is available.
        if (observation.Decision != ContributionDecision.Allowed)
        {
            return ContributionAssessment.Undetermined(
                EligibilityReason.RequiredInputUnavailable);
        }

        // Allowed requires archive state and activity to be deterministically
        // known; an undetermined input never becomes an optimistic Active.
        return repository.Archived == RepositoryConditionState.Absent
            && activity == ActivityClassification.Active
                ? ContributionAssessment.Allowed()
                : ContributionAssessment.Undetermined(
                    EligibilityReason.RequiredInputUnavailable);
    }

    // Historical evidence follows the evidence-surface baseline only. Archived
    // or historical lifecycle state does not invalidate it, and it is not
    // mutually exclusive with current focus.
    private static ContributionAssessment ResolveHistoricalEvidence(
        RepositoryContributionAssessment contribution)
    {
        ContributionAssessment observation = contribution.ProfileObservation;

        return observation.Decision switch
        {
            ContributionDecision.Allowed =>
                ContributionAssessment.Allowed(),

            ContributionDecision.NotAllowed =>
                ContributionAssessment.NotAllowed([.. observation.Reasons]),

            // A technical failure is not evidence absence.
            _ => ContributionAssessment.Undetermined(
                EligibilityReason.RequiredInputUnavailable)
        };
    }
}

using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile;

// Stage A of the locked repository eligibility contract: whether repository
// analysis may be attempted for one profile repository. This is not
// RepositoryEligibilityResult and produces no contribution decision.
public sealed class AnalysisAdmissionPolicy
{
    // The eligibility policy is the ONLY construction input, deliberately.
    //
    // A version-only constructor would let a caller pair a canonical
    // EligibilityPolicyVersion with fork behavior that version does not own —
    // for example provenance "2.0.0" executing the V1 Analyze rule. Because
    // EligibilityPolicy itself refuses to rebind a canonical version to a
    // different rule, taking the policy makes that pairing unconstructible
    // rather than merely discouraged.
    public AnalysisAdmissionPolicy(EligibilityPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        PolicyVersion = policy.Version;
        ForkAdmission = policy.ForkAdmission;
    }

    public EligibilityPolicyVersion PolicyVersion { get; }

    public ForkAdmissionRule ForkAdmission { get; }

    public AnalysisAdmissionResult Evaluate(ProfileRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        // Empty resolves first, so an empty repository keeps reporting exactly
        // the reason it always reported.
        if (repository.Empty == RepositoryConditionState.Present)
        {
            return Create(
                repository,
                AnalysisAdmissionDecision.Skip,
                EligibilityReason.EmptyRepository);
        }

        // Deterministically known fork lineage is sufficient on its own under a
        // policy version whose fork admission rule is Skip. It resolves before
        // the empty-state Undetermined branch because the fork fact is already
        // known from the manifest and no repository collection could change it.
        if (ForkAdmission == ForkAdmissionRule.Skip
            && repository.Fork == RepositoryConditionState.Present)
        {
            return Create(
                repository,
                AnalysisAdmissionDecision.Skip,
                EligibilityReason.ForkDetected);
        }

        return repository.Empty switch
        {
            RepositoryConditionState.Absent => Create(
                repository,
                AnalysisAdmissionDecision.Analyze,
                reason: null),

            _ => Create(
                repository,
                AnalysisAdmissionDecision.Undetermined,
                EligibilityReason.RequiredInputUnavailable)
        };
    }

    private AnalysisAdmissionResult Create(
        ProfileRepository repository,
        AnalysisAdmissionDecision decision,
        EligibilityReason? reason) =>
        new(
            repository.Reference,
            decision,
            reason,
            PolicyVersion);
}

using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile;

// Stage A of the locked repository eligibility contract: whether repository
// analysis may be attempted for one profile repository. This is not
// RepositoryEligibilityResult and produces no contribution decision.
public sealed class AnalysisAdmissionPolicy
{
    public AnalysisAdmissionPolicy(EligibilityPolicyVersion policyVersion)
    {
        PolicyVersion = policyVersion
            ?? throw new ArgumentNullException(nameof(policyVersion));
    }

    public EligibilityPolicyVersion PolicyVersion { get; }

    public AnalysisAdmissionResult Evaluate(ProfileRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return repository.Empty switch
        {
            RepositoryConditionState.Present => Create(
                repository,
                AnalysisAdmissionDecision.Skip,
                AnalysisAdmissionReason.EmptyRepository),

            RepositoryConditionState.Absent => Create(
                repository,
                AnalysisAdmissionDecision.Analyze,
                reason: null),

            _ => Create(
                repository,
                AnalysisAdmissionDecision.Undetermined,
                AnalysisAdmissionReason.RequiredInputUnavailable)
        };
    }

    private AnalysisAdmissionResult Create(
        ProfileRepository repository,
        AnalysisAdmissionDecision decision,
        AnalysisAdmissionReason? reason) =>
        new(
            repository.Reference,
            decision,
            reason,
            PolicyVersion);
}

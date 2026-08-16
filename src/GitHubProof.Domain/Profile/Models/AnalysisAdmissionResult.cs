using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed record AnalysisAdmissionResult
{
    public AnalysisAdmissionResult(
        ProfileRepositoryReference reference,
        AnalysisAdmissionDecision decision,
        AnalysisAdmissionReason? reason,
        EligibilityPolicyVersion policyVersion)
    {
        Reference = reference
            ?? throw new ArgumentNullException(nameof(reference));

        if (!Enum.IsDefined(decision))
        {
            throw new ArgumentOutOfRangeException(
                nameof(decision),
                decision,
                "Analysis admission decision is not defined.");
        }

        if (reason is not null && !Enum.IsDefined(reason.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Analysis admission reason is not defined.");
        }

        if (decision == AnalysisAdmissionDecision.Analyze && reason is not null)
        {
            throw new ArgumentException(
                "An Analyze admission decision cannot carry a restriction reason.",
                nameof(reason));
        }

        if (decision != AnalysisAdmissionDecision.Analyze && reason is null)
        {
            throw new ArgumentException(
                "A Skip or Undetermined admission decision requires a reason.",
                nameof(reason));
        }

        Decision = decision;
        Reason = reason;
        PolicyVersion = policyVersion
            ?? throw new ArgumentNullException(nameof(policyVersion));
    }

    public ProfileRepositoryReference Reference { get; }

    public AnalysisAdmissionDecision Decision { get; }

    public AnalysisAdmissionReason? Reason { get; }

    public EligibilityPolicyVersion PolicyVersion { get; }
}

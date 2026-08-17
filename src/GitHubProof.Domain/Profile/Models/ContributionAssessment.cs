using GitHubProof.Domain.Profile.Enums;

namespace GitHubProof.Domain.Profile.Models;

// One purpose-specific contribution decision plus the structured reasons that
// produced it. Allowed never carries a restriction reason, and a restricted or
// unresolved decision can never be reason-less.
public sealed class ContributionAssessment
{
    public ContributionAssessment(
        ContributionDecision decision,
        IEnumerable<EligibilityReason>? reasons)
    {
        if (!Enum.IsDefined(decision))
        {
            throw new ArgumentOutOfRangeException(
                nameof(decision),
                decision,
                "Contribution decision is not defined.");
        }

        Decision = decision;
        Reasons = NormalizeReasons(reasons);

        if (decision == ContributionDecision.Allowed && Reasons.Count > 0)
        {
            throw new ArgumentException(
                "An Allowed contribution decision cannot carry restriction reasons.",
                nameof(reasons));
        }

        if (decision is ContributionDecision.NotAllowed or ContributionDecision.Undetermined
            && Reasons.Count == 0)
        {
            throw new ArgumentException(
                $"A {decision} contribution decision requires at least one reason.",
                nameof(reasons));
        }
    }

    public ContributionDecision Decision { get; }

    public IReadOnlyList<EligibilityReason> Reasons { get; }

    public bool IsAllowed => Decision == ContributionDecision.Allowed;

    public static ContributionAssessment Allowed() =>
        new(ContributionDecision.Allowed, reasons: null);

    public static ContributionAssessment NotAllowed(
        params EligibilityReason[] reasons) =>
        new(ContributionDecision.NotAllowed, reasons);

    public static ContributionAssessment Undetermined(
        params EligibilityReason[] reasons) =>
        new(ContributionDecision.Undetermined, reasons);

    private static IReadOnlyList<EligibilityReason> NormalizeReasons(
        IEnumerable<EligibilityReason>? reasons)
    {
        if (reasons is null)
        {
            return Array.Empty<EligibilityReason>();
        }

        EligibilityReason[] normalized = reasons
            .Distinct()
            .OrderBy(reason => reason)
            .ToArray();

        foreach (EligibilityReason reason in normalized)
        {
            if (!Enum.IsDefined(reason))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reasons),
                    reason,
                    "Eligibility reason is not defined.");
            }
        }

        return normalized;
    }
}

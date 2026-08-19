using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// The final repository eligibility result profile composition consumes. It
// answers only what this repository may safely contribute evidence toward; it
// is never repository quality, developer capability, seniority, employability
// or ranking.
//
// It composes the existing RepositoryContributionAssessment rather than
// replacing it: that bounded result stays the load-bearing input of the
// cross-repository evidence composer, and this type adds the remaining
// contribution purposes plus the derived overall status.
public sealed class RepositoryEligibilityResult
{
    private readonly Dictionary<ContributionPurpose, ContributionAssessment> assessments;

    public RepositoryEligibilityResult(
        RepositoryContributionAssessment contribution,
        RepositoryConditionState archived,
        ActivityClassification activity,
        ContributionAssessment currentFocus,
        ContributionAssessment historicalEvidence)
    {
        Contribution = contribution
            ?? throw new ArgumentNullException(nameof(contribution));

        if (!Enum.IsDefined(archived))
        {
            throw new ArgumentOutOfRangeException(
                nameof(archived),
                archived,
                "Repository condition state is not defined.");
        }

        if (!Enum.IsDefined(activity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(activity),
                activity,
                "Activity classification is not defined.");
        }

        Archived = archived;
        Activity = activity;
        CurrentFocus = currentFocus
            ?? throw new ArgumentNullException(nameof(currentFocus));
        HistoricalEvidence = historicalEvidence
            ?? throw new ArgumentNullException(nameof(historicalEvidence));

        // Every canonical baseline resolves the remaining evidence-bearing
        // purposes on top of an established evidence surface, so neither may be
        // allowed while the observation contribution itself is not. This keeps
        // a technical failure or an unresolved admission from being promoted
        // into available evidence.
        if (CurrentFocus.IsAllowed && !contribution.ProfileObservation.IsAllowed)
        {
            throw new ArgumentException(
                "Current focus cannot be allowed without an allowed observation contribution.",
                nameof(currentFocus));
        }

        if (HistoricalEvidence.IsAllowed && !contribution.ProfileObservation.IsAllowed)
        {
            throw new ArgumentException(
                "Historical evidence cannot be allowed without an allowed observation contribution.",
                nameof(historicalEvidence));
        }

        assessments = new Dictionary<ContributionPurpose, ContributionAssessment>
        {
            [ContributionPurpose.ProfileContext] = ProfileContext,
            [ContributionPurpose.ProfileObservation] = contribution.ProfileObservation,
            [ContributionPurpose.IndependentRepetition] = contribution.IndependentRepetition,
            [ContributionPurpose.CurrentFocus] = CurrentFocus,
            [ContributionPurpose.HistoricalEvidence] = HistoricalEvidence
        };

        Status = DeriveStatus(EvidenceBearingAssessments());
    }

    public RepositoryContributionAssessment Contribution { get; }

    public ProfileRepositoryReference Reference => Contribution.Reference;

    public EligibilityPolicyVersion PolicyVersion => Contribution.PolicyVersion;

    public AnalysisAdmissionResult Admission => Contribution.Admission;

    public IndependenceClassification Independence => Contribution.Independence;

    public RepositoryAnalysisReference? AnalysisReference => Contribution.AnalysisReference;

    // The normalized repository conditions that participate in the lifecycle
    // decisions, kept on the result so a restriction or an unresolved decision
    // stays traceable to the condition that produced it.
    public RepositoryConditionState Archived { get; }

    public ActivityClassification Activity { get; }

    // Every canonical evidence-surface baseline keeps a repository visible as
    // profile context, including empty, insufficient, failed and unadmitted
    // repositories, so this decision is not an evaluation input. Context
    // visibility alone never promotes a repository into profile-level
    // evidence: it is excluded from the derived status.
    public ContributionAssessment ProfileContext { get; } =
        ContributionAssessment.Allowed();

    public ContributionAssessment ProfileObservation => Contribution.ProfileObservation;

    public ContributionAssessment IndependentRepetition => Contribution.IndependentRepetition;

    public ContributionAssessment CurrentFocus { get; }

    public ContributionAssessment HistoricalEvidence { get; }

    // Derived from the purpose-specific decisions, never set independently.
    public RepositoryEligibilityStatus Status { get; }

    public ContributionAssessment For(ContributionPurpose purpose)
    {
        if (!assessments.TryGetValue(purpose, out ContributionAssessment? assessment))
        {
            throw new ArgumentOutOfRangeException(
                nameof(purpose),
                purpose,
                "Contribution purpose is not defined.");
        }

        return assessment;
    }

    // ProfileContext is context-only and is excluded, so repository visibility
    // can never promote a repository into profile-level evidence.
    private IEnumerable<ContributionAssessment> EvidenceBearingAssessments()
    {
        yield return ProfileObservation;
        yield return IndependentRepetition;
        yield return CurrentFocus;
        yield return HistoricalEvidence;
    }

    // Canonical precedence: Undetermined, Restricted, Contributing,
    // NonContributing. NotApplicable decisions are ignored, and exactly one
    // status is derivable for any fixed set of decisions.
    private static RepositoryEligibilityStatus DeriveStatus(
        IEnumerable<ContributionAssessment> evidenceBearing)
    {
        bool hasAllowed = false;
        bool hasNotAllowed = false;

        foreach (ContributionAssessment assessment in evidenceBearing)
        {
            switch (assessment.Decision)
            {
                // Highest precedence: an unresolved evidence limitation must
                // never be hidden by another permissive decision.
                case ContributionDecision.Undetermined:
                    return RepositoryEligibilityStatus.Undetermined;

                case ContributionDecision.Allowed:
                    hasAllowed = true;
                    break;

                case ContributionDecision.NotAllowed:
                    hasNotAllowed = true;
                    break;
            }
        }

        if (!hasAllowed)
        {
            return RepositoryEligibilityStatus.NonContributing;
        }

        return hasNotAllowed
            ? RepositoryEligibilityStatus.Restricted
            : RepositoryEligibilityStatus.Contributing;
    }
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile;

// Stage B of the locked repository eligibility contract, bounded to the two
// contribution purposes profile aggregation requires: ProfileObservation and
// IndependentRepetition. CurrentFocus and HistoricalEvidence are not evaluated,
// so no overall RepositoryEligibilityStatus is derived here.
public sealed class RepositoryContributionPolicy
{
    public RepositoryContributionPolicy(EligibilityPolicyVersion policyVersion)
    {
        PolicyVersion = policyVersion
            ?? throw new ArgumentNullException(nameof(policyVersion));
    }

    public EligibilityPolicyVersion PolicyVersion { get; }

    public RepositoryContributionAssessment Evaluate(
        ProfileRepository repository,
        AnalysisAdmissionResult admission,
        AnalysisStatus? analysisStatus,
        RepositoryAnalysisReference? analysisReference)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(admission);

        if (repository.Reference != admission.Reference)
        {
            throw new ArgumentException(
                "The admission result must belong to the evaluated repository.",
                nameof(admission));
        }

        if (admission.PolicyVersion != PolicyVersion)
        {
            throw new ArgumentException(
                "Admission and contribution evaluation must use one eligibility policy version.",
                nameof(admission));
        }

        if (admission.Decision != AnalysisAdmissionDecision.Analyze
            && (analysisStatus is not null || analysisReference is not null))
        {
            throw new ArgumentException(
                "Repository analysis input requires an Analyze admission decision.",
                nameof(analysisStatus));
        }

        if (analysisStatus is not null && !Enum.IsDefined(analysisStatus.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(analysisStatus),
                analysisStatus,
                "Analysis status is not defined.");
        }

        Lineage lineage = ClassifyLineage(repository);

        return admission.Decision switch
        {
            AnalysisAdmissionDecision.Skip => Restricted(
                admission,
                lineage,
                admission.Reason!.Value),

            AnalysisAdmissionDecision.Analyze => EvaluateAdmitted(
                admission,
                lineage,
                analysisStatus,
                analysisReference),

            _ => Unresolved(admission, lineage)
        };
    }

    private static Lineage ClassifyLineage(ProfileRepository repository)
    {
        List<EligibilityReason> reasons = [];

        if (repository.Fork == RepositoryConditionState.Present)
        {
            reasons.Add(EligibilityReason.ForkDetected);
        }

        if (repository.TemplateDerived == RepositoryConditionState.Present)
        {
            reasons.Add(EligibilityReason.TemplateDerived);
        }

        if (reasons.Count > 0)
        {
            return new Lineage(IndependenceClassification.NonIndependent, reasons);
        }

        return repository.Fork == RepositoryConditionState.Absent
            && repository.TemplateDerived == RepositoryConditionState.Absent
                ? new Lineage(IndependenceClassification.Independent, [])
                : new Lineage(IndependenceClassification.Undetermined, []);
    }

    private static RepositoryContributionAssessment EvaluateAdmitted(
        AnalysisAdmissionResult admission,
        Lineage lineage,
        AnalysisStatus? analysisStatus,
        RepositoryAnalysisReference? analysisReference)
    {
        // A missing analysis result is the same technical failure boundary as
        // AnalysisStatus.Failed: repository evidence contribution could not be
        // established, so it must not become evidence absence.
        if (analysisStatus is null or AnalysisStatus.Failed)
        {
            return Unresolved(admission, lineage);
        }

        if (analysisStatus == AnalysisStatus.InsufficientData)
        {
            return Restricted(
                admission,
                lineage,
                EligibilityReason.InsufficientEvidence);
        }

        return new RepositoryContributionAssessment(
            admission,
            lineage.Classification,
            ContributionAssessment.Allowed(),
            lineage.Classification switch
            {
                IndependenceClassification.Independent =>
                    ContributionAssessment.Allowed(),

                IndependenceClassification.NonIndependent =>
                    ContributionAssessment.NotAllowed([.. lineage.Reasons]),

                _ => ContributionAssessment.Undetermined(
                    EligibilityReason.IndependenceUndetermined)
            },
            analysisReference);
    }

    private static RepositoryContributionAssessment Restricted(
        AnalysisAdmissionResult admission,
        Lineage lineage,
        EligibilityReason reason) =>
        new(
            admission,
            lineage.Classification,
            ContributionAssessment.NotAllowed(reason),
            ContributionAssessment.NotAllowed(reason),
            analysisReference: null);

    // Repository evidence contribution could not be established. A
    // deterministic manifest-only lineage restriction still resolves
    // independent repetition; every other decision stays Undetermined.
    private static RepositoryContributionAssessment Unresolved(
        AnalysisAdmissionResult admission,
        Lineage lineage) =>
        new(
            admission,
            lineage.Classification,
            ContributionAssessment.Undetermined(
                EligibilityReason.RequiredInputUnavailable),
            lineage.Classification switch
            {
                IndependenceClassification.NonIndependent =>
                    ContributionAssessment.NotAllowed([.. lineage.Reasons]),

                IndependenceClassification.Undetermined =>
                    ContributionAssessment.Undetermined(
                        EligibilityReason.IndependenceUndetermined),

                _ => ContributionAssessment.Undetermined(
                    EligibilityReason.RequiredInputUnavailable)
            },
            analysisReference: null);

    private sealed record Lineage(
        IndependenceClassification Classification,
        IReadOnlyList<EligibilityReason> Reasons);
}

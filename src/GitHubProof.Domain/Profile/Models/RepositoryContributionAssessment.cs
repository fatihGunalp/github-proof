using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Bounded P2 Stage B result: the contribution purposes profile aggregation
// actually requires. It is deliberately not RepositoryEligibilityResult and
// derives no overall RepositoryEligibilityStatus, because CurrentFocus and
// HistoricalEvidence are not evaluated here.
public sealed class RepositoryContributionAssessment
{
    public RepositoryContributionAssessment(
        AnalysisAdmissionResult admission,
        IndependenceClassification independence,
        ContributionAssessment profileObservation,
        ContributionAssessment independentRepetition,
        RepositoryAnalysisReference? analysisReference)
    {
        Admission = admission
            ?? throw new ArgumentNullException(nameof(admission));

        if (!Enum.IsDefined(independence))
        {
            throw new ArgumentOutOfRangeException(
                nameof(independence),
                independence,
                "Independence classification is not defined.");
        }

        Independence = independence;
        ProfileObservation = profileObservation
            ?? throw new ArgumentNullException(nameof(profileObservation));
        IndependentRepetition = independentRepetition
            ?? throw new ArgumentNullException(nameof(independentRepetition));
        AnalysisReference = analysisReference;

        if (ProfileObservation.IsAllowed && analysisReference is null)
        {
            throw new ArgumentException(
                "An allowed observation contribution requires a repository analysis reference.",
                nameof(analysisReference));
        }

        if (IndependentRepetition.IsAllowed && !ProfileObservation.IsAllowed)
        {
            throw new ArgumentException(
                "Independent repetition cannot be allowed without an allowed observation contribution.",
                nameof(independentRepetition));
        }
    }

    public AnalysisAdmissionResult Admission { get; }

    public ProfileRepositoryReference Reference => Admission.Reference;

    public EligibilityPolicyVersion PolicyVersion => Admission.PolicyVersion;

    public IndependenceClassification Independence { get; }

    public ContributionAssessment ProfileObservation { get; }

    public ContributionAssessment IndependentRepetition { get; }

    public RepositoryAnalysisReference? AnalysisReference { get; }
}

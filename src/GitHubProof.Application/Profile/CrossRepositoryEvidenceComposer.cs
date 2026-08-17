using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// Aggregates repository-level evidence into deterministic cross-repository
// observations. It re-reads existing RepositoryAnalysis results; it never
// re-evaluates signals, recalculates repository score or weights evidence.
public sealed class CrossRepositoryEvidenceComposer
{
    private readonly RepositoryContributionPolicy contributionPolicy;

    public CrossRepositoryEvidenceComposer(
        RepositoryContributionPolicy contributionPolicy)
    {
        this.contributionPolicy = contributionPolicy
            ?? throw new ArgumentNullException(nameof(contributionPolicy));
    }

    public CrossRepositoryEvidence Compose(
        ProfileAnalysisOrchestrationResult orchestration,
        ObservationRuleVersion ruleVersion)
    {
        ArgumentNullException.ThrowIfNull(orchestration);
        ArgumentNullException.ThrowIfNull(ruleVersion);

        IReadOnlyDictionary<ProfileRepositoryReference, ProfileRepository> manifest =
            orchestration.Snapshot.Repositories.Repositories.ToDictionary(
                repository => repository.Reference);

        List<RepositoryContributionAssessment> contributions = [];
        Dictionary<string, SignalObservation> observations = new(StringComparer.Ordinal);

        foreach (ProfileRepositoryAnalysisResult result in orchestration.Results)
        {
            RepositoryAnalysis? analysis = result.Analysis;
            RepositoryAnalysisReference? analysisReference = analysis is null
                ? null
                : new RepositoryAnalysisReference(
                    analysis.AnalysisId,
                    analysis.Repository,
                    analysis.Version);
            RepositoryContributionAssessment contribution = contributionPolicy.Evaluate(
                manifest[result.Reference],
                result.Admission,
                analysis?.Status,
                analysisReference);

            contributions.Add(contribution);

            if (!contribution.ProfileObservation.IsAllowed || analysis is null)
            {
                continue;
            }

            CollectSignalEvidence(
                analysis,
                contribution,
                analysisReference!,
                observations);
        }

        int eligibleRepositoryCount = contributions.Count(contribution =>
            contribution.ProfileObservation.IsAllowed);

        return new CrossRepositoryEvidence(
            contributions,
            observations.Values
                .Where(observation => observation.HasSupport)
                .Select(observation => observation.Build(
                    eligibleRepositoryCount,
                    ruleVersion)),
            ruleVersion,
            contributionPolicy.PolicyVersion,
            orchestration.IsRepositoryManifestComplete);
    }

    private static void CollectSignalEvidence(
        RepositoryAnalysis analysis,
        RepositoryContributionAssessment contribution,
        RepositoryAnalysisReference analysisReference,
        Dictionary<string, SignalObservation> observations)
    {
        foreach (SignalResult signal in analysis.SignalResults)
        {
            if (!observations.TryGetValue(
                    signal.SignalKey,
                    out SignalObservation? observation))
            {
                observation = new SignalObservation(signal.SignalKey);
                observations.Add(signal.SignalKey, observation);
            }

            switch (signal.Status)
            {
                // Structured evidence for the signal exists in this repository.
                case SignalStatus.Strong:
                case SignalStatus.Present:
                case SignalStatus.Partial:
                    observation.AddSupport(
                        new ProfileEvidenceReference(
                            contribution.Reference,
                            analysisReference,
                            signal.SignalKey,
                            signal.Evidence.Select(evidence => evidence.Key)),
                        contribution.IndependentRepetition.IsAllowed);
                    break;

                // The repository could not establish the signal reliably. This
                // is not repository-level absence and never becomes profile
                // absence.
                case SignalStatus.UnavailableEvidence:
                case SignalStatus.RequiresHumanReview:
                    observation.AddUndetermined();
                    break;
            }
        }
    }

    private sealed class SignalObservation(string signalKey)
    {
        private readonly List<ProfileEvidenceReference> evidenceReferences = [];

        public void AddSupport(
            ProfileEvidenceReference reference,
            bool isIndependent)
        {
            evidenceReferences.Add(reference);

            if (isIndependent)
            {
                IndependentSupportingRepositoryCount++;
            }
        }

        public void AddUndetermined() => UndeterminedEvidenceRepositoryCount++;

        public int IndependentSupportingRepositoryCount { get; private set; }

        public int UndeterminedEvidenceRepositoryCount { get; private set; }

        public bool HasSupport => evidenceReferences.Count > 0;

        public ObservedProfileEvidence Build(
            int eligibleRepositoryCount,
            ObservationRuleVersion ruleVersion) =>
            new(
                signalKey,
                ObservedProfileEvidenceKind.RepositorySignalEvidence,
                evidenceReferences,
                IndependentSupportingRepositoryCount,
                eligibleRepositoryCount,
                UndeterminedEvidenceRepositoryCount,
                ruleVersion);
    }
}

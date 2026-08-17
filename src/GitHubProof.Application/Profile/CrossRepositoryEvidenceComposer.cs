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
        Dictionary<string, EvidenceObservation> observations = new(StringComparer.Ordinal);
        int undeterminedLanguageRepositoryCount = 0;

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

            if (!CollectLanguageEvidence(
                    analysis,
                    contribution,
                    analysisReference!,
                    observations))
            {
                undeterminedLanguageRepositoryCount++;
            }
        }

        int eligibleRepositoryCount = contributions.Count(contribution =>
            contribution.ProfileObservation.IsAllowed);

        return new CrossRepositoryEvidence(
            contributions,
            observations.Values
                .Where(observation => observation.HasSupport)
                .Select(observation => observation.Build(
                    eligibleRepositoryCount,
                    undeterminedLanguageRepositoryCount,
                    ruleVersion)),
            ruleVersion,
            contributionPolicy.PolicyVersion,
            orchestration.IsRepositoryManifestComplete);
    }

    private static void CollectSignalEvidence(
        RepositoryAnalysis analysis,
        RepositoryContributionAssessment contribution,
        RepositoryAnalysisReference analysisReference,
        Dictionary<string, EvidenceObservation> observations)
    {
        foreach (SignalResult signal in analysis.SignalResults)
        {
            EvidenceObservation observation = Resolve(
                observations,
                signal.SignalKey,
                ObservedProfileEvidenceKind.RepositorySignalEvidence);

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

    // Returns false when language evidence is undetermined for this repository,
    // which leaves every language observation undetermined for it rather than
    // asserting that no language evidence exists.
    private static bool CollectLanguageEvidence(
        RepositoryAnalysis analysis,
        RepositoryContributionAssessment contribution,
        RepositoryAnalysisReference analysisReference,
        Dictionary<string, EvidenceObservation> observations)
    {
        if (analysis.Snapshot
                .FindAvailability(ProfileEvidenceKeys.LanguagesDataKey)?
                .IsAvailable != true)
        {
            return false;
        }

        foreach (string language in FindDominantLanguages(analysis.Snapshot.Languages))
        {
            string observationKey =
                ProfileEvidenceKeys.CreateLanguageObservationKey(language);

            Resolve(
                    observations,
                    observationKey,
                    ObservedProfileEvidenceKind.RepositoryLanguageEvidence)
                .AddSupport(
                    new ProfileEvidenceReference(
                        contribution.Reference,
                        analysisReference,
                        observationKey,
                        [ProfileEvidenceKeys.LanguagesDataKey]),
                    contribution.IndependentRepetition.IsAllowed);
        }

        return true;
    }

    // Dominance requires a positive byte count and the repository maximum.
    // Every language tied at that exact maximum is dominant.
    private static IEnumerable<string> FindDominantLanguages(
        IReadOnlyDictionary<string, long> languages)
    {
        long dominantBytes = languages.Values.DefaultIfEmpty(0).Max();

        return dominantBytes <= 0
            ? []
            : languages
                .Where(language => language.Value == dominantBytes)
                .Select(language => language.Key);
    }

    private static EvidenceObservation Resolve(
        Dictionary<string, EvidenceObservation> observations,
        string observationKey,
        ObservedProfileEvidenceKind kind)
    {
        if (!observations.TryGetValue(
                observationKey,
                out EvidenceObservation? observation))
        {
            observation = new EvidenceObservation(observationKey, kind);
            observations.Add(observationKey, observation);
        }

        return observation;
    }

    private sealed class EvidenceObservation(
        string observationKey,
        ObservedProfileEvidenceKind kind)
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
            int undeterminedLanguageRepositoryCount,
            ObservationRuleVersion ruleVersion) =>
            new(
                observationKey,
                kind,
                evidenceReferences,
                IndependentSupportingRepositoryCount,
                eligibleRepositoryCount,
                UndeterminedEvidenceRepositoryCount
                    + (kind == ObservedProfileEvidenceKind.RepositoryLanguageEvidence
                        ? undeterminedLanguageRepositoryCount
                        : 0),
                ruleVersion);
    }
}

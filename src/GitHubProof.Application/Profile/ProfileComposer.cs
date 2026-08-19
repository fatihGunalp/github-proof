using System.Runtime.CompilerServices;
using GitHubProof.Application.Profile.Enums;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// Assembles the canonical DeveloperPublicEvidenceProfile from already-produced
// P3-P8 results. It never reranks representative repositories, never
// recomputes promoted patterns, and never invents capability, seniority,
// employability or job-fit semantics: it only verifies that the given results
// are coherent, accounts for evidence-surface completeness, and emits
// limitations/warnings under their locked identity mapping.
//
// When the given results cannot be proven coherent, composition returns
// Failed with no profile rather than normalizing the gap to Partial.
public sealed class ProfileComposer
{
    public ProfileCompositionResult Compose(ProfileCompositionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        try
        {
            if (!TryBuildEligibilityIndex(
                    input.RepositoryEligibilityResults,
                    out Dictionary<ProfileRepositoryReference, RepositoryEligibilityResult>
                        eligibilityByReference))
            {
                return ProfileCompositionResult.Failed();
            }

            if (!RepositoryUniverseIsCoherent(input, eligibilityByReference))
            {
                return ProfileCompositionResult.Failed();
            }

            if (!PatternSetTracesToCrossRepositoryEvidence(input))
            {
                return ProfileCompositionResult.Failed();
            }

            if (!RepresentativeSelectionTracesToPatternSet(input))
            {
                return ProfileCompositionResult.Failed();
            }

            ProfileAnalysisStatus status = DetermineAnalysisStatus(input);
            IReadOnlyList<ProfileLimitation> limitations = BuildLimitations(input, status);
            IReadOnlyList<ProfileWarning> warnings = BuildWarnings(
                input,
                eligibilityByReference);

            DeveloperPublicEvidenceProfile profile = new(
                input.Orchestration.Snapshot.CreateReference(),
                status,
                input.AnalyzerVersion,
                input.RepositoryEligibilityResults,
                input.CrossRepositoryEvidence,
                input.PatternSet,
                input.RepresentativeRepositorySelection,
                limitations,
                warnings,
                input.StartedAt,
                input.CompletedAt);

            return ProfileCompositionResult.Composed(profile);
        }
        catch (ArgumentException)
        {
            // Coherent canonical profile truth could not be composed. This is
            // the same boundary as an unproven §14 provenance/version/
            // repository-universe coherence chain: composition fails rather
            // than forcing a profile into existence over an incoherent input
            // set.
            return ProfileCompositionResult.Failed();
        }
    }

    // ------------------------------------------------------------ coherence

    private static bool TryBuildEligibilityIndex(
        IReadOnlyList<RepositoryEligibilityResult> results,
        out Dictionary<ProfileRepositoryReference, RepositoryEligibilityResult> index)
    {
        index = [];

        foreach (RepositoryEligibilityResult result in results)
        {
            if (!index.TryAdd(result.Reference, result))
            {
                return false;
            }
        }

        return true;
    }

    // §14 "Same canonical repository universe": a repository present in one
    // layer and silently absent from another is incoherence, not an
    // omission to tolerate. §14 "No silent cross-run mixing": the same
    // repository must carry the same admission and contribution decisions
    // across every layer that reports it, so two independently produced but
    // divergent views of one repository can never be silently merged.
    private static bool RepositoryUniverseIsCoherent(
        ProfileCompositionInput input,
        IReadOnlyDictionary<ProfileRepositoryReference, RepositoryEligibilityResult>
            eligibilityByReference)
    {
        Dictionary<ProfileRepositoryReference, RepositoryContributionAssessment>
            contributionsByReference = input.CrossRepositoryEvidence.RepositoryContributions
                .ToDictionary(contribution => contribution.Reference);

        HashSet<ProfileRepositoryReference> orchestrationUniverse = input.Orchestration.Results
            .Select(result => result.Reference)
            .ToHashSet();

        if (eligibilityByReference.Count != orchestrationUniverse.Count
            || contributionsByReference.Count != orchestrationUniverse.Count
            || !orchestrationUniverse.SetEquals(eligibilityByReference.Keys)
            || !orchestrationUniverse.SetEquals(contributionsByReference.Keys))
        {
            return false;
        }

        foreach (ProfileRepositoryAnalysisResult analysisResult in input.Orchestration.Results)
        {
            RepositoryEligibilityResult eligibility =
                eligibilityByReference[analysisResult.Reference];

            if (eligibility.Admission != analysisResult.Admission)
            {
                return false;
            }

            RepositoryContributionAssessment contribution =
                contributionsByReference[analysisResult.Reference];

            if (!ContributionsMatch(eligibility.Contribution, contribution))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContributionsMatch(
        RepositoryContributionAssessment left,
        RepositoryContributionAssessment right) =>
        left.Reference == right.Reference
        && left.PolicyVersion == right.PolicyVersion
        && left.Independence == right.Independence
        && left.AnalysisReference == right.AnalysisReference
        && AssessmentsMatch(left.ProfileObservation, right.ProfileObservation)
        && AssessmentsMatch(left.IndependentRepetition, right.IndependentRepetition);

    private static bool AssessmentsMatch(
        ContributionAssessment left,
        ContributionAssessment right) =>
        left.Decision == right.Decision
        && left.Reasons.SequenceEqual(right.Reasons);

    // ProfilePatternPromotionPolicy.Promote builds each pattern's
    // SupportingObservations directly from the given CrossRepositoryEvidence's
    // own ObservedProfileEvidence instances (same in-memory objects). A
    // pattern set carrying an observation absent from the given
    // CrossRepositoryEvidence was not actually promoted from it — the same
    // cross-run mixing §14 prohibits, one layer below the pattern trace.
    private static bool PatternSetTracesToCrossRepositoryEvidence(
        ProfileCompositionInput input)
    {
        HashSet<ObservedProfileEvidence> observations = new(
            input.CrossRepositoryEvidence.Observations,
            ReferenceEqualityComparer.Instance);

        return input.PatternSet.Patterns
            .SelectMany(pattern => pattern.SupportingObservations)
            .All(observations.Contains);
    }

    // The Canonical Pattern Trace is an in-memory reference to the exact
    // RepeatedProfilePattern instance a representative won
    // (public-profile-representative-repositories.md). A selection that
    // references a pattern absent from the given ProfilePatternSet was not
    // actually produced from it, which is exactly the cross-run mixing §14
    // prohibits.
    private static bool RepresentativeSelectionTracesToPatternSet(
        ProfileCompositionInput input)
    {
        HashSet<RepeatedProfilePattern> patterns = new(
            input.PatternSet.Patterns,
            ReferenceEqualityComparer.Instance);

        return input.RepresentativeRepositorySelection.RepresentativeRepositories
            .SelectMany(representative => representative.PatternSupports)
            .All(support => patterns.Contains(support.Pattern));
    }

    // ------------------------------------------------------ status/accounting

    // §13 "Evidence-Surface Accounting": Complete requires every supported and
    // applicable obligation to be resolved. RepositoryEligibilityStatus.
    // Undetermined already means at least one evidence-bearing purpose for
    // that repository stayed unresolved, and ProfilePatternSet.
    // IsEvidenceSurfaceComplete is consumed as-is per "Upstream Completeness"
    // rather than recalculated.
    private static ProfileAnalysisStatus DetermineAnalysisStatus(
        ProfileCompositionInput input)
    {
        if (!input.PatternSet.IsEvidenceSurfaceComplete)
        {
            return ProfileAnalysisStatus.Partial;
        }

        bool hasUnresolvedRepository = input.RepositoryEligibilityResults
            .Any(result => result.Status == RepositoryEligibilityStatus.Undetermined);

        return hasUnresolvedRepository
            ? ProfileAnalysisStatus.Partial
            : ProfileAnalysisStatus.Complete;
    }

    // ------------------------------------------------------------ limitations

    private static IReadOnlyList<ProfileLimitation> BuildLimitations(
        ProfileCompositionInput input,
        ProfileAnalysisStatus status)
    {
        List<ProfileLimitation> limitations = [];

        int independentRepetitionAllowedCount = input.RepositoryEligibilityResults
            .Count(result => result.IndependentRepetition.IsAllowed);

        if (independentRepetitionAllowedCount
            < RepeatedProfilePattern.MinimumIndependentSupportingRepositories)
        {
            limitations.Add(new ProfileLimitation(
                ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                ProfileConditionScope.Profile));
        }

        if (status == ProfileAnalysisStatus.Partial)
        {
            limitations.Add(new ProfileLimitation(
                ProfileLimitationCode.PartialEvidenceSurface,
                ProfileConditionScope.Profile));
        }

        return limitations;
    }

    // --------------------------------------------------------------- warnings

    // Deterministic identity mapping (§13): each repository is checked against
    // its own canonical triggering condition, independently for each of the
    // four identities, so one repository-level event may legitimately produce
    // more than one warning identity without ever duplicating the same
    // identity for the same repository.
    private static IReadOnlyList<ProfileWarning> BuildWarnings(
        ProfileCompositionInput input,
        IReadOnlyDictionary<ProfileRepositoryReference, RepositoryEligibilityResult>
            eligibilityByReference)
    {
        List<ProfileWarning> warnings = [];

        foreach (ProfileRepositoryAnalysisResult result in input.Orchestration.Results)
        {
            RepositoryEligibilityResult eligibility = eligibilityByReference[result.Reference];

            if (eligibility.Status == RepositoryEligibilityStatus.Undetermined)
            {
                warnings.Add(RepositoryWarning(
                    ProfileWarningCode.RepositoryEligibilityUndetermined,
                    result.Reference));
            }

            switch (result.Outcome)
            {
                case ProfileRepositoryAnalysisOutcome.CollectionUnavailable:
                    warnings.Add(RepositoryWarning(
                        result.CollectionAvailability!.Status
                            == DataAvailabilityStatus.RateLimited
                            ? ProfileWarningCode.RepositoryEvidenceRateLimited
                            : ProfileWarningCode.RepositoryEvidenceUnavailable,
                        result.Reference));
                    break;

                case ProfileRepositoryAnalysisOutcome.CollectionFailed:
                case ProfileRepositoryAnalysisOutcome.AnalysisFailed:
                    warnings.Add(RepositoryWarning(
                        ProfileWarningCode.RepositoryAnalysisFailed,
                        result.Reference));
                    break;

                case ProfileRepositoryAnalysisOutcome.Analyzed
                    when result.Analysis!.Status == AnalysisStatus.Failed:
                    warnings.Add(RepositoryWarning(
                        ProfileWarningCode.RepositoryAnalysisFailed,
                        result.Reference));
                    break;
            }
        }

        return warnings;
    }

    private static ProfileWarning RepositoryWarning(
        ProfileWarningCode code,
        ProfileRepositoryReference reference) =>
        new(code, ProfileConditionScope.Repository, [reference]);
}

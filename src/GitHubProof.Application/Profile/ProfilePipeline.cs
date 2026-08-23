using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// The single Application-level composition path from one GitHubProfileSnapshot
// to its DeveloperPublicEvidenceProfile and ProfileReport. It reimplements no
// existing algorithm: it only drives the already-produced P3-P8 stages in
// order, passing each stage's own output — by reference where canonical
// identity is required (CrossRepositoryEvidence, ProfilePatternSet) — into the
// next, so the same-run object trace those stages depend on is never broken
// by recomposing an equivalent object in between.
public sealed class ProfilePipeline
{
    private readonly ProfileRepositoryAnalysisOrchestrator analysisOrchestrator;
    private readonly RepositoryEligibilityPolicy eligibilityPolicy;
    private readonly CrossRepositoryEvidenceComposer evidenceComposer;
    private readonly ProfilePatternPromotionPolicy patternPromotionPolicy;
    private readonly RepresentativeRepositorySelectionPolicy representativeSelectionPolicy;
    private readonly ProfileComposer profileComposer;
    private readonly ProfileReportProjector reportProjector;
    private readonly ObservationRuleVersion observationRuleVersion;
    private readonly ProfileAnalyzerVersion analyzerVersion;
    private readonly TimeProvider timeProvider;

    public ProfilePipeline(
        ProfileRepositoryAnalysisOrchestrator analysisOrchestrator,
        RepositoryEligibilityPolicy eligibilityPolicy,
        CrossRepositoryEvidenceComposer evidenceComposer,
        ProfilePatternPromotionPolicy patternPromotionPolicy,
        RepresentativeRepositorySelectionPolicy representativeSelectionPolicy,
        ProfileComposer profileComposer,
        ProfileReportProjector reportProjector,
        ObservationRuleVersion observationRuleVersion,
        ProfileAnalyzerVersion analyzerVersion,
        TimeProvider? timeProvider = null)
    {
        this.analysisOrchestrator = analysisOrchestrator
            ?? throw new ArgumentNullException(nameof(analysisOrchestrator));
        this.eligibilityPolicy = eligibilityPolicy
            ?? throw new ArgumentNullException(nameof(eligibilityPolicy));
        this.evidenceComposer = evidenceComposer
            ?? throw new ArgumentNullException(nameof(evidenceComposer));
        this.patternPromotionPolicy = patternPromotionPolicy
            ?? throw new ArgumentNullException(nameof(patternPromotionPolicy));
        this.representativeSelectionPolicy = representativeSelectionPolicy
            ?? throw new ArgumentNullException(nameof(representativeSelectionPolicy));
        this.profileComposer = profileComposer
            ?? throw new ArgumentNullException(nameof(profileComposer));
        this.reportProjector = reportProjector
            ?? throw new ArgumentNullException(nameof(reportProjector));
        this.observationRuleVersion = observationRuleVersion
            ?? throw new ArgumentNullException(nameof(observationRuleVersion));
        this.analyzerVersion = analyzerVersion
            ?? throw new ArgumentNullException(nameof(analyzerVersion));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<ProfilePipelineResult> RunAsync(
        GitHubProfileSnapshot snapshot,
        AnalysisVersion analysisVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(analysisVersion);

        DateTimeOffset startedAt = timeProvider.GetUtcNow();

        // P3: repository admission + analysis.
        ProfileAnalysisOrchestrationResult orchestration = await analysisOrchestrator.AnalyzeAsync(
            snapshot,
            analysisVersion,
            cancellationToken);

        // Final repository eligibility (P8-I1), anchored to the snapshot's own
        // capture time — never a system clock.
        IReadOnlyList<RepositoryEligibilityResult> eligibilityResults =
            EvaluateEligibility(snapshot, orchestration);

        // P5: cross-repository evidence, fed by this same orchestration result.
        CrossRepositoryEvidence crossRepositoryEvidence = evidenceComposer.Compose(
            orchestration,
            observationRuleVersion);

        // P6: pattern promotion, fed by this same CrossRepositoryEvidence
        // instance (never a recomposed equivalent).
        ProfilePatternSet patternSet = patternPromotionPolicy.Promote(crossRepositoryEvidence);

        // P7: representative repository selection, fed by this same
        // ProfilePatternSet instance.
        RepresentativeRepositorySelection representativeSelection =
            representativeSelectionPolicy.Select(patternSet);

        DateTimeOffset completedAt = timeProvider.GetUtcNow();

        // P8-I3: composition over the exact same-run objects assembled above.
        ProfileCompositionInput input = new(
            orchestration,
            eligibilityResults,
            crossRepositoryEvidence,
            patternSet,
            representativeSelection,
            analyzerVersion,
            startedAt,
            completedAt);

        ProfileCompositionResult compositionResult = profileComposer.Compose(input);

        if (!compositionResult.HasProfile)
        {
            return ProfilePipelineResult.Failed(compositionResult);
        }

        // P8-I4: the report is projected only over a successfully composed
        // profile (Complete or Partial); a Failed composition never gets one.
        ProfileReport report = reportProjector.Project(compositionResult.Profile!);

        // Identity/presentation metadata carried straight from this same run's
        // snapshot — never through DeveloperPublicEvidenceProfile/ProfileReport,
        // which are evidence-report truth and already exclude it.
        return ProfilePipelineResult.Composed(
            compositionResult,
            report,
            snapshot.Metadata?.AvatarUrl,
            snapshot.RepositoryPresentationMetadata);
    }

    private IReadOnlyList<RepositoryEligibilityResult> EvaluateEligibility(
        GitHubProfileSnapshot snapshot,
        ProfileAnalysisOrchestrationResult orchestration)
    {
        IReadOnlyDictionary<ProfileRepositoryReference, ProfileRepository> repositoriesByReference =
            snapshot.Repositories.Repositories.ToDictionary(
                repository => repository.Reference);

        List<RepositoryEligibilityResult> results = [];

        foreach (ProfileRepositoryAnalysisResult result in orchestration.Results)
        {
            RepositoryAnalysisReference? analysisReference = result.Analysis is null
                ? null
                : new RepositoryAnalysisReference(
                    result.Analysis.AnalysisId,
                    result.Analysis.Repository,
                    result.Analysis.Version);

            results.Add(eligibilityPolicy.Evaluate(
                repositoriesByReference[result.Reference],
                result.Admission,
                result.Analysis?.Status,
                analysisReference,
                snapshot.CapturedAt));
        }

        return results;
    }
}

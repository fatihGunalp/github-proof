using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile;

public sealed class ProfileComposerTests
{
    // --------------------------------------------------------- Complete/Partial

    [Fact]
    public void TwoIndependentAnalyzedRepositories_IsComplete()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
        [
            (alpha, Analyzed(alpha)),
            (beta, Analyzed(beta))
        ]));

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.NotNull(result.Profile);
        Assert.Equal(ProfileAnalysisStatus.Complete, result.Profile!.AnalysisStatus);
        Assert.Empty(result.Profile.Limitations);
        Assert.Empty(result.Profile.Warnings);
        Assert.Empty(result.Profile.RoleEvidenceAlignments);
    }

    [Fact]
    public void SparseSingleRepository_IsCompleteWithLimitedIndependentEvidenceSurface()
    {
        ProfileRepository alpha = CreateRepository("alpha");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(alpha, Analyzed(alpha))]));

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.Equal(
            [ProfileLimitationCode.LimitedIndependentEvidenceSurface],
            result.Profile!.Limitations.Select(limitation => limitation.Code));
    }

    [Fact]
    public void ForkedRepository_IsResolvedNotAllowed_NotPartial()
    {
        ProfileRepository forked = CreateRepository(
            "forked",
            fork: RepositoryConditionState.Present);

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(forked, Analyzed(forked))]));

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);

        RepositoryEligibilityResult eligibility = result.Profile!.RepositoryEligibilityResults[0];

        Assert.True(eligibility.ProfileObservation.IsAllowed);
        Assert.False(eligibility.IndependentRepetition.IsAllowed);
        Assert.Equal(RepositoryEligibilityStatus.Restricted, eligibility.Status);
        Assert.Equal(
            [ProfileLimitationCode.LimitedIndependentEvidenceSurface],
            result.Profile.Limitations.Select(limitation => limitation.Code));
    }

    [Fact]
    public void TwoIndependentRepositoriesWithNoPromotedPattern_ProducesNoLimitation()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
        [
            (alpha, Analyzed(alpha)),
            (beta, Analyzed(beta))
        ]));

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.Empty(result.Profile!.RepeatedProfilePatterns);
        Assert.Empty(result.Profile.Limitations);
    }

    [Fact]
    public void IndependenceUndetermined_ProducesPartialAndEligibilityUndeterminedWarning()
    {
        ProfileRepository repository = CreateRepository(
            "undetermined-lineage",
            fork: RepositoryConditionState.Undetermined);

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(repository, Analyzed(repository))]));

        Assert.Equal(ProfileCompositionStatus.Partial, result.Status);
        Assert.Equal(
            RepositoryEligibilityStatus.Undetermined,
            result.Profile!.RepositoryEligibilityResults[0].Status);
        Assert.Equal(
            [ProfileWarningCode.RepositoryEligibilityUndetermined],
            result.Profile.Warnings.Select(warning => warning.Code));
        Assert.Equal(
            [
                ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                ProfileLimitationCode.PartialEvidenceSurface
            ],
            result.Profile.Limitations.Select(limitation => limitation.Code));
    }

    [Fact]
    public void EvidenceSurfaceIncomplete_ProducesPartial_EvenWithNoUnresolvedRepository()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [
                (alpha, Analyzed(alpha)),
                (beta, Analyzed(beta))
            ],
            isRepositoryManifestComplete: false));

        // The asymmetry §13 "Upstream Completeness" locks: IsEvidenceSurfaceComplete
        // = false forces Partial even though every repository here resolved cleanly.
        Assert.Equal(ProfileCompositionStatus.Partial, result.Status);
        Assert.DoesNotContain(
            result.Profile!.RepositoryEligibilityResults,
            eligibility => eligibility.Status == RepositoryEligibilityStatus.Undetermined);
        Assert.Contains(
            ProfileLimitationCode.PartialEvidenceSurface,
            result.Profile.Limitations.Select(limitation => limitation.Code));
    }

    [Fact]
    public void EvidenceSurfaceComplete_WithUnresolvedRepository_StaysPartial()
    {
        // IsEvidenceSurfaceComplete = true alone never establishes Complete: every
        // other supported and applicable obligation must also resolve.
        ProfileRepository repository = CreateRepository(
            "undetermined-lineage",
            fork: RepositoryConditionState.Undetermined);

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(repository, Analyzed(repository))],
            isRepositoryManifestComplete: true));

        Assert.Equal(ProfileCompositionStatus.Partial, result.Status);
    }

    // -------------------------------------------------------------- warnings

    [Fact]
    public void RateLimitedCollection_ProducesRateLimitedAndEligibilityUndetermined()
    {
        ProfileRepository repository = CreateRepository("rate-limited");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
        [
            (repository, CollectionUnavailable(repository, DataAvailabilityStatus.RateLimited))
        ]));

        Assert.Equal(ProfileCompositionStatus.Partial, result.Status);
        Assert.Equal(
            [
                ProfileWarningCode.RepositoryEvidenceRateLimited,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            result.Profile!.Warnings.Select(warning => warning.Code).Order());
    }

    [Fact]
    public void NonRateLimitedUnavailableCollection_ProducesEvidenceUnavailable()
    {
        ProfileRepository repository = CreateRepository("forbidden");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
        [
            (repository, CollectionUnavailable(repository, DataAvailabilityStatus.Forbidden))
        ]));

        Assert.Equal(
            [
                ProfileWarningCode.RepositoryEvidenceUnavailable,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            result.Profile!.Warnings.Select(warning => warning.Code).Order());
    }

    [Fact]
    public void CollectionFailure_ProducesAnalysisFailed()
    {
        ProfileRepository repository = CreateRepository("collection-failed");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(repository, CollectionFailed(repository))]));

        Assert.Equal(
            [
                ProfileWarningCode.RepositoryAnalysisFailed,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            result.Profile!.Warnings.Select(warning => warning.Code).Order());
    }

    [Fact]
    public void AnalysisFailure_ProducesAnalysisFailed()
    {
        ProfileRepository repository = CreateRepository("analysis-failed");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(repository, AnalysisFailed(repository))]));

        Assert.Equal(
            [
                ProfileWarningCode.RepositoryAnalysisFailed,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            result.Profile!.Warnings.Select(warning => warning.Code).Order());
    }

    [Fact]
    public void AnalyzedRepositoryWithFailedStatus_ProducesAnalysisFailed()
    {
        ProfileRepository repository = CreateRepository("analyzed-failed-status");

        ProfileCompositionResult result = Composer().Compose(CreateInput(
        [
            (repository, Analyzed(repository, status: AnalysisStatus.Failed))
        ]));

        Assert.Equal(
            [
                ProfileWarningCode.RepositoryAnalysisFailed,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            result.Profile!.Warnings.Select(warning => warning.Code).Order());
    }

    [Fact]
    public void AdmissionUndetermined_ProducesOnlyEligibilityUndetermined()
    {
        ProfileRepository repository = CreateRepository(
            "admission-undetermined",
            empty: RepositoryConditionState.Undetermined);

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(repository, AdmissionUndetermined(repository))]));

        Assert.Equal(
            [ProfileWarningCode.RepositoryEligibilityUndetermined],
            result.Profile!.Warnings.Select(warning => warning.Code));
    }

    [Fact]
    public void SkippedEmptyRepository_ProducesNoWarning()
    {
        ProfileRepository repository = CreateRepository(
            "empty",
            empty: RepositoryConditionState.Present);

        ProfileCompositionResult result = Composer().Compose(CreateInput(
            [(repository, NotAdmitted(repository))]));

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.Empty(result.Profile!.Warnings);
    }

    // ------------------------------------------------------------ coherence

    [Fact]
    public void EligibilityResultMissingForAnOrchestrationRepository_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfileAnalysisOrchestrationResult orchestration = CreateOrchestration(
            [alpha, beta],
            [Analyzed(alpha), Analyzed(beta)]);

        CrossRepositoryEvidence evidence = CreateEvidence(orchestration);

        List<RepositoryEligibilityResult> incompleteEligibility = CreateEligibilityResults(
                orchestration,
                new Dictionary<ProfileRepositoryReference, ProfileRepository>
                {
                    [alpha.Reference] = alpha,
                    [beta.Reference] = beta
                })
            .Where(eligibility => eligibility.Reference.Name == "alpha")
            .ToList();

        ProfilePatternSet patternSet = CreateEmptyPatternSet(evidence);

        ProfileCompositionInput input = new(
            orchestration,
            incompleteEligibility,
            evidence,
            patternSet,
            CreateEmptySelection(patternSet),
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void CrossRepositoryEvidenceFromADifferentRepositoryUniverse_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");
        ProfileRepository gamma = CreateRepository("gamma");

        ProfileAnalysisOrchestrationResult orchestration = CreateOrchestration(
            [alpha, beta],
            [Analyzed(alpha), Analyzed(beta)]);

        List<RepositoryEligibilityResult> eligibility = CreateEligibilityResults(
            orchestration,
            new Dictionary<ProfileRepositoryReference, ProfileRepository>
            {
                [alpha.Reference] = alpha,
                [beta.Reference] = beta
            });

        // A CrossRepositoryEvidence built over a different repository universe
        // (alpha + gamma instead of alpha + beta).
        ProfileAnalysisOrchestrationResult foreignOrchestration = CreateOrchestration(
            [alpha, gamma],
            [Analyzed(alpha), Analyzed(gamma)]);

        CrossRepositoryEvidence foreignEvidence = CreateEvidence(foreignOrchestration);
        ProfilePatternSet patternSet = CreateEmptyPatternSet(foreignEvidence);

        ProfileCompositionInput input = new(
            orchestration,
            eligibility,
            foreignEvidence,
            patternSet,
            CreateEmptySelection(patternSet),
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void EligibilityAndEvidenceDivergeOnTheSameRepository_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");

        // The orchestration and evidence agree the repository was analyzed
        // cleanly...
        ProfileAnalysisOrchestrationResult orchestration = CreateOrchestration(
            [alpha],
            [Analyzed(alpha)]);
        CrossRepositoryEvidence evidence = CreateEvidence(orchestration);

        // ...but eligibility is computed as if analysis had failed for the same
        // repository: a divergent, cross-run view of one repository.
        ProfileAnalysisOrchestrationResult divergentOrchestration = CreateOrchestration(
            [alpha],
            [AnalysisFailed(alpha)]);
        List<RepositoryEligibilityResult> divergentEligibility = CreateEligibilityResults(
            divergentOrchestration,
            new Dictionary<ProfileRepositoryReference, ProfileRepository>
            {
                [alpha.Reference] = alpha
            });

        ProfilePatternSet patternSet = CreateEmptyPatternSet(evidence);

        ProfileCompositionInput input = new(
            orchestration,
            divergentEligibility,
            evidence,
            patternSet,
            CreateEmptySelection(patternSet),
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void PatternSetPromotedFromADifferentCrossRepositoryEvidence_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfileAnalysisOrchestrationResult orchestration = CreateOrchestration(
            [alpha, beta],
            [
                AnalyzedWithLanguage(alpha, "C#"),
                AnalyzedWithLanguage(beta, "C#")
            ]);

        // The given CrossRepositoryEvidence...
        CrossRepositoryEvidence evidence = CreateEvidence(orchestration);
        List<RepositoryEligibilityResult> eligibility = CreateEligibilityResults(
            orchestration,
            new Dictionary<ProfileRepositoryReference, ProfileRepository>
            {
                [alpha.Reference] = alpha,
                [beta.Reference] = beta
            });

        // ...and a second, independently composed but value-equal
        // CrossRepositoryEvidence over the same repository universe and the
        // same rule/policy version literals. Promoting from the second and
        // presenting it as if it belonged to the first is exactly the "same
        // version literals, different run" case §14 prohibits.
        CrossRepositoryEvidence foreignEvidence = CreateEvidence(orchestration);
        ProfilePatternSet foreignPatternSet =
            new ProfilePatternPromotionPolicy(TestPatternRuleVersion).Promote(foreignEvidence);

        ProfileCompositionInput input = new(
            orchestration,
            eligibility,
            evidence,
            foreignPatternSet,
            CreateEmptySelection(foreignPatternSet),
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void RepresentativeSelectionVersionMismatch_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileCompositionInput baseline = CreateInput([(alpha, Analyzed(alpha))]);

        RepresentativeRepositorySelection mismatchedSelection = new(
            [],
            baseline.PatternSet.RuleVersion,
            new ObservationRuleVersion("a-different-observation-rule-version"),
            baseline.PatternSet.PolicyVersion,
            baseline.PatternSet.IsEvidenceSurfaceComplete,
            TestRepresentativeRepositoryRuleVersion);

        ProfileCompositionInput input = new(
            baseline.Orchestration,
            baseline.RepositoryEligibilityResults,
            baseline.CrossRepositoryEvidence,
            baseline.PatternSet,
            mismatchedSelection,
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void RepresentativeSelectionReferencesAForeignPattern_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfileAnalysisOrchestrationResult orchestration = CreateOrchestration(
            [alpha, beta],
            [
                AnalyzedWithLanguage(alpha, "C#"),
                AnalyzedWithLanguage(beta, "C#")
            ]);
        CrossRepositoryEvidence evidence = CreateEvidence(orchestration);
        List<RepositoryEligibilityResult> eligibility = CreateEligibilityResults(
            orchestration,
            new Dictionary<ProfileRepositoryReference, ProfileRepository>
            {
                [alpha.Reference] = alpha,
                [beta.Reference] = beta
            });

        // The real pattern set, promoted from the given evidence: its
        // SupportingObservations trace correctly to evidence.Observations, so
        // only RepresentativeSelectionTracesToPatternSet is exercised below —
        // PatternSetTracesToCrossRepositoryEvidence must not also trip here.
        ProfilePatternSet realPatternSet =
            new ProfilePatternPromotionPolicy(TestPatternRuleVersion).Promote(evidence);
        Assert.NotEmpty(realPatternSet.Patterns);

        // An independently promoted pattern set over the same evidence: its
        // patterns carry the identical (traceable) supporting observations
        // but are themselves fresh RepeatedProfilePattern instances, absent
        // from realPatternSet.Patterns by reference.
        ProfilePatternSet independentPatternSet =
            new ProfilePatternPromotionPolicy(TestPatternRuleVersion).Promote(evidence);
        RepresentativeRepositorySelection foreignSelection =
            new RepresentativeRepositorySelectionPolicy(TestRepresentativeRepositoryRuleVersion)
                .Select(independentPatternSet);

        ProfileCompositionInput input = new(
            orchestration,
            eligibility,
            evidence,
            realPatternSet,
            foreignSelection,
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void DuplicateEligibilityReference_Fails()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileCompositionInput baseline = CreateInput([(alpha, Analyzed(alpha))]);

        ProfileCompositionInput input = new(
            baseline.Orchestration,
            [.. baseline.RepositoryEligibilityResults, baseline.RepositoryEligibilityResults[0]],
            baseline.CrossRepositoryEvidence,
            baseline.PatternSet,
            baseline.RepresentativeRepositorySelection,
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);

        ProfileCompositionResult result = Composer().Compose(input);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
    }

    // ------------------------------------------------------------- ordering

    [Fact]
    public void Compose_IsInputOrderInvariant()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");
        ProfileRepository rateLimited = CreateRepository("rate-limited");

        (ProfileRepository, ProfileRepositoryAnalysisResult)[] forward =
        [
            (alpha, Analyzed(alpha)),
            (beta, Analyzed(beta)),
            (
                rateLimited,
                CollectionUnavailable(rateLimited, DataAvailabilityStatus.RateLimited))
        ];

        ProfileCompositionResult first = Composer().Compose(CreateInput(forward));
        ProfileCompositionResult second = Composer().Compose(
            CreateInput([.. forward.Reverse()]));

        Assert.Equal(ProfileCompositionStatus.Partial, first.Status);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(
            first.Profile!.RepositoryEligibilityResults.Select(r => r.Reference),
            second.Profile!.RepositoryEligibilityResults.Select(r => r.Reference));
        Assert.Equal(
            first.Profile.Warnings.Select(w => (w.Code, w.AffectedRepositories[0])),
            second.Profile.Warnings.Select(w => (w.Code, w.AffectedRepositories[0])));
        Assert.Equal(
            first.Profile.Limitations.Select(l => l.Code),
            second.Profile.Limitations.Select(l => l.Code));
    }

    // -------------------------------------------------------------- fixtures

    private static ProfileComposer Composer() => new();
}

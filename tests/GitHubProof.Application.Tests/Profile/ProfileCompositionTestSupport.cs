using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

internal static class ProfileCompositionTestSupport
{
    public static readonly EligibilityPolicy TestEligibilityPolicy =
        EligibilityPolicy.Create(TestPolicyVersion, TimeSpan.FromDays(365));

    public static readonly ObservationRuleVersion TestObservationRuleVersion =
        new("test-observation-rules");

    public static readonly PatternRuleVersion TestPatternRuleVersion =
        new("test-pattern-rules");

    public static readonly RepresentativeRepositoryRuleVersion
        TestRepresentativeRepositoryRuleVersion =
        new("test-representative-repository-rules");

    public static readonly DateTimeOffset StartedAt = CapturedAt.AddHours(1);

    public static readonly DateTimeOffset CompletedAt = StartedAt.AddMinutes(5);

    // Recent enough, relative to CapturedAt, to classify as Active under the
    // 365-day current activity window unless a scenario overrides it.
    public static ProfileRepository CreateRepository(
        string name,
        RepositoryConditionState fork = RepositoryConditionState.Absent,
        RepositoryConditionState templateDerived = RepositoryConditionState.Absent,
        RepositoryConditionState empty = RepositoryConditionState.Absent,
        RepositoryConditionState archived = RepositoryConditionState.Absent,
        DateTimeOffset? lastPushedAt = null,
        string owner = "octocat") =>
        new(
            CreateReference(name, owner),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork,
            templateDerived,
            archived,
            empty,
            lastPushedAt ?? CapturedAt.AddDays(-10),
            createdAt: CapturedAt.AddYears(-2));

    public static AnalysisAdmissionResult CreateAdmission(
        ProfileRepository repository,
        AnalysisAdmissionDecision decision,
        EligibilityReason? reason = null) =>
        new(
            repository.Reference,
            decision,
            reason ?? DefaultReason(decision),
            TestPolicyVersion);

    // A minimal but fully valid RepositoryAnalysis: no signals, no categories,
    // no highlights. Only Status varies across the scenarios that need it.
    public static RepositoryAnalysis CreateAnalysis(
        ProfileRepository repository,
        AnalysisStatus status = AnalysisStatus.Completed,
        RepositorySnapshot? snapshot = null)
    {
        snapshot ??= CreateRepositorySnapshot(
            repository.Reference.Name,
            repository.Reference.Owner);

        return new RepositoryAnalysis(
            Guid.NewGuid(),
            snapshot.Repository,
            snapshot,
            status,
            TestAnalysisVersion,
            CapturedAt,
            CapturedAt,
            signalResults: null,
            categoryResults: null,
            new ScoreBreakdown(null),
            strengths: null,
            criticalGaps: null,
            new ImprovementPlan([], CapturedAt, TestAnalysisVersion.RuleSetVersion),
            limitationKeys: null,
            warnings: null);
    }

    public static RepositoryAnalysisReference CreateAnalysisReference(
        RepositoryAnalysis analysis) =>
        new(analysis.AnalysisId, analysis.Repository, analysis.Version);

    public static ProfileRepositoryAnalysisResult Analyzed(
        ProfileRepository repository,
        AnalysisAdmissionResult? admission = null,
        AnalysisStatus status = AnalysisStatus.Completed) =>
        ProfileRepositoryAnalysisResult.Analyzed(
            admission ?? CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            CreateAnalysis(repository, status),
            DataAvailability.Available(MetadataDataKey, CapturedAt));

    // Carries real, promotable language evidence, so pattern promotion has
    // something to actually promote.
    public static ProfileRepositoryAnalysisResult AnalyzedWithLanguage(
        ProfileRepository repository,
        string language,
        AnalysisAdmissionResult? admission = null) =>
        ProfileRepositoryAnalysisResult.Analyzed(
            admission ?? CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            CreateAnalysis(
                repository,
                snapshot: CreateLanguageEvidenceSnapshot(
                    repository.Reference.Name,
                    new Dictionary<string, long> { [language] = 100 },
                    repository.Reference.Owner)),
            DataAvailability.Available(MetadataDataKey, CapturedAt));

    public static ProfileRepositoryAnalysisResult CollectionUnavailable(
        ProfileRepository repository,
        DataAvailabilityStatus status,
        AnalysisAdmissionResult? admission = null) =>
        ProfileRepositoryAnalysisResult.CollectionUnavailable(
            admission ?? CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            DataAvailability.Unavailable(
                MetadataDataKey,
                status,
                "Repository collection did not return available data."));

    public static ProfileRepositoryAnalysisResult CollectionFailed(
        ProfileRepository repository,
        AnalysisAdmissionResult? admission = null) =>
        ProfileRepositoryAnalysisResult.CollectionFailed(
            admission ?? CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            "Repository collection failed.");

    public static ProfileRepositoryAnalysisResult AnalysisFailed(
        ProfileRepository repository,
        AnalysisAdmissionResult? admission = null) =>
        ProfileRepositoryAnalysisResult.AnalysisFailed(
            admission ?? CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            DataAvailability.Available(MetadataDataKey, CapturedAt),
            "Repository analysis failed.");

    public static ProfileRepositoryAnalysisResult AdmissionUndetermined(
        ProfileRepository repository) =>
        ProfileRepositoryAnalysisResult.AdmissionUndetermined(
            CreateAdmission(repository, AnalysisAdmissionDecision.Undetermined));

    public static ProfileRepositoryAnalysisResult NotAdmitted(
        ProfileRepository repository) =>
        ProfileRepositoryAnalysisResult.NotAdmitted(
            CreateAdmission(repository, AnalysisAdmissionDecision.Skip));

    // -------------------------------------------------- pipeline construction

    public static ProfileAnalysisOrchestrationResult CreateOrchestration(
        IEnumerable<ProfileRepository> repositories,
        IEnumerable<ProfileRepositoryAnalysisResult> results,
        bool isRepositoryManifestComplete = true) =>
        new(
            CreateProfileSnapshot(repositories, isRepositoryManifestComplete),
            TestAnalysisVersion,
            TestPolicyVersion,
            results);

    public static CrossRepositoryEvidence CreateEvidence(
        ProfileAnalysisOrchestrationResult orchestration) =>
        new CrossRepositoryEvidenceComposer(new RepositoryContributionPolicy(TestPolicyVersion))
            .Compose(orchestration, TestObservationRuleVersion);

    public static List<RepositoryEligibilityResult> CreateEligibilityResults(
        ProfileAnalysisOrchestrationResult orchestration,
        IReadOnlyDictionary<ProfileRepositoryReference, ProfileRepository> repositoriesByReference,
        EligibilityPolicy? policy = null)
    {
        RepositoryEligibilityPolicy eligibilityPolicy = new(policy ?? TestEligibilityPolicy);
        List<RepositoryEligibilityResult> results = [];

        foreach (ProfileRepositoryAnalysisResult result in orchestration.Results)
        {
            RepositoryAnalysisReference? analysisReference = result.Analysis is null
                ? null
                : CreateAnalysisReference(result.Analysis);

            results.Add(eligibilityPolicy.Evaluate(
                repositoriesByReference[result.Reference],
                result.Admission,
                result.Analysis?.Status,
                analysisReference,
                orchestration.Snapshot.CapturedAt));
        }

        return results;
    }

    public static ProfilePatternSet CreateEmptyPatternSet(CrossRepositoryEvidence evidence) =>
        new(
            [],
            TestPatternRuleVersion,
            evidence.RuleVersion,
            evidence.PolicyVersion,
            evidence.IsEvidenceSurfaceComplete);

    public static RepresentativeRepositorySelection CreateEmptySelection(
        ProfilePatternSet patternSet) =>
        new(
            [],
            patternSet.RuleVersion,
            patternSet.ObservationRuleVersion,
            patternSet.PolicyVersion,
            patternSet.IsEvidenceSurfaceComplete,
            TestRepresentativeRepositoryRuleVersion);

    // Assembles a fully coherent ProfileCompositionInput from a set of
    // repository/result pairs, with an empty pattern set and selection. Tests
    // that need a real promoted pattern build PatternSet/Selection themselves
    // via ProfilePatternPromotionPolicy/RepresentativeRepositorySelectionPolicy
    // over the returned CrossRepositoryEvidence.
    public static ProfileCompositionInput CreateInput(
        IReadOnlyList<(ProfileRepository Repository, ProfileRepositoryAnalysisResult Result)> repositories,
        bool isRepositoryManifestComplete = true,
        ProfilePatternSet? patternSet = null,
        RepresentativeRepositorySelection? selection = null)
    {
        ProfileAnalysisOrchestrationResult orchestration = CreateOrchestration(
            repositories.Select(entry => entry.Repository),
            repositories.Select(entry => entry.Result),
            isRepositoryManifestComplete);

        CrossRepositoryEvidence evidence = CreateEvidence(orchestration);

        List<RepositoryEligibilityResult> eligibility = CreateEligibilityResults(
            orchestration,
            repositories.ToDictionary(
                entry => entry.Repository.Reference,
                entry => entry.Repository));

        ProfilePatternSet resolvedPatternSet = patternSet ?? CreateEmptyPatternSet(evidence);

        return new ProfileCompositionInput(
            orchestration,
            eligibility,
            evidence,
            resolvedPatternSet,
            selection ?? CreateEmptySelection(resolvedPatternSet),
            ProfileAnalyzerVersion.V1,
            StartedAt,
            CompletedAt);
    }

    // ------------------------------------------------- real pipeline execution
    //
    // Shared by ProfilePipelineTests and the P9-I2 profile validation harness
    // (tests/GitHubProof.Application.Tests/Profile/Validation/), so both
    // exercise the exact same real ProfilePipeline wiring rather than two
    // independent constructions of it.
    // policy defaults to V1 so every pre-Session-099 caller keeps exercising the
    // exact rules it always did; a fork-policy test passes V2 explicitly.
    public static ProfilePipeline CreatePipeline(
        FakeSnapshotSource source,
        Func<Guid>? analysisIdFactory = null,
        TimeProvider? timeProvider = null,
        EligibilityPolicy? policy = null)
    {
        policy ??= EligibilityPolicy.V1;

        ProfileRepositoryAnalysisOrchestrator analysisOrchestrator = new(
            source,
            CreateAnalysisRunner(),
            new AnalysisAdmissionPolicy(policy),
            analysisIdFactory);

        return new ProfilePipeline(
            analysisOrchestrator,
            new RepositoryEligibilityPolicy(policy),
            new CrossRepositoryEvidenceComposer(
                new RepositoryContributionPolicy(policy.Version)),
            new ProfilePatternPromotionPolicy(PatternRuleVersion.V1),
            new RepresentativeRepositorySelectionPolicy(RepresentativeRepositoryRuleVersion.V1),
            new ProfileComposer(),
            new ProfileReportProjector(),
            ObservationRuleVersion.V1,
            ProfileAnalyzerVersion.V1,
            timeProvider);
    }

    // The single semantic-equivalence comparison pattern for a ProfilePipeline
    // run, covering exactly the P9 validation contract §8.1 deterministic
    // comparison surface
    // (docs/product/p9-profile-validation-contract.md). Extracted from
    // ProfilePipelineTests so no second, independently-drifting determinism
    // check is defined for the same contract surface.
    public static void AssertSemanticallyEquivalent(
        ProfilePipelineResult first,
        ProfilePipelineResult second)
    {
        Assert.Equal(first.Status, second.Status);
        Assert.NotNull(first.Profile);
        Assert.NotNull(second.Profile);
        Assert.NotNull(first.Report);
        Assert.NotNull(second.Report);

        Assert.Equal(
            first.Profile!.RepositoryEligibilityResults.Select(r => (r.Reference, r.Status)),
            second.Profile!.RepositoryEligibilityResults.Select(r => (r.Reference, r.Status)));
        Assert.Equal(
            first.Profile.Limitations.Select(l => l.Code),
            second.Profile.Limitations.Select(l => l.Code));
        Assert.Equal(
            first.Profile.Warnings.Select(w => (w.Code, w.AffectedRepositories.Order())),
            second.Profile.Warnings.Select(w => (w.Code, w.AffectedRepositories.Order())));
        Assert.Equal(
            first.Profile.RepeatedProfilePatterns.Select(p => (p.Family, p.PatternKey)),
            second.Profile.RepeatedProfilePatterns.Select(p => (p.Family, p.PatternKey)));
        Assert.Equal(
            first.Profile.RepresentativeRepositories.Select(r => r.Repository),
            second.Profile.RepresentativeRepositories.Select(r => r.Repository));

        Assert.Equal(
            first.Report!.Sections.Select(s => (s.SectionKey, s.Availability)),
            second.Report!.Sections.Select(s => (s.SectionKey, s.Availability)));
        Assert.Equal(
            first.Report.WhyThisConclusion.Items.Select(item => item.Pattern.PatternKey),
            second.Report.WhyThisConclusion.Items.Select(item => item.Pattern.PatternKey));
        Assert.Equal(
            first.Report.RepositoryDrillDown.Items.Select(item => (item.Repository, item.EligibilityStatus)),
            second.Report.RepositoryDrillDown.Items.Select(item => (item.Repository, item.EligibilityStatus)));
    }

    private static EligibilityReason? DefaultReason(AnalysisAdmissionDecision decision) =>
        decision switch
        {
            AnalysisAdmissionDecision.Analyze => null,
            AnalysisAdmissionDecision.Skip => EligibilityReason.EmptyRepository,
            _ => EligibilityReason.RequiredInputUnavailable
        };
}

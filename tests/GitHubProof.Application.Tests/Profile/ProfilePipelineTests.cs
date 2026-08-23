using System.Reflection;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile;

// P8-I5 integration coverage: the single Application-level pipeline wiring
// ProfileRepositoryAnalysisOrchestrator (P3) -> RepositoryEligibilityPolicy
// (P8-I1) -> CrossRepositoryEvidenceComposer (P5) ->
// ProfilePatternPromotionPolicy (P6) -> RepresentativeRepositorySelectionPolicy
// (P7) -> ProfileComposer (P8-I3) -> ProfileReportProjector (P8-I4). Every
// stage here is the real production type — no algorithm is reimplemented or
// stubbed — so a regression in the wiring itself (a stage fed a recomposed
// object, a version literal left as a test double, a clock substituted for
// snapshot.CapturedAt) is what these tests are built to catch.
public sealed class ProfilePipelineTests
{
    [Fact]
    public async Task RunAsync_CompleteFlow_ProducesCoherentProfileAndReport()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([alpha, beta]);

        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha", () => CreateCollected(
                CreateLanguageEvidenceSnapshot("alpha", Language("C#")))),
            ("octocat/beta", () => CreateCollected(
                CreateLanguageEvidenceSnapshot("beta", Language("C#")))));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        // 1: complete flow -> profile + report.
        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.NotNull(result.Profile);
        Assert.NotNull(result.Report);

        // 2: a final RepositoryEligibilityResult for every repository.
        Assert.Equal(2, result.Profile!.RepositoryEligibilityResults.Count);
        Assert.Equal(
            [alpha.Reference, beta.Reference],
            result.Profile.RepositoryEligibilityResults.Select(r => r.Reference).Order());

        // 3/15: EligibilityPolicy.V1 is the production policy actually used.
        Assert.All(
            result.Profile.RepositoryEligibilityResults,
            eligibility => Assert.Equal(EligibilityPolicy.V1.Version, eligibility.PolicyVersion));

        // 15: production runtime versions were used end to end.
        Assert.Equal(ProfileAnalyzerVersion.V1, result.Profile.AnalyzerVersion);
        Assert.Equal(ObservationRuleVersion.V1, result.Profile.CrossRepositoryEvidence.RuleVersion);
        Assert.Equal(EligibilityPolicy.V1.Version, result.Profile.CrossRepositoryEvidence.PolicyVersion);

        // 5/6/7: the same-run evidence -> pattern -> representative-selection
        // trace composed successfully (ProfileComposer would have returned
        // Failed had the pipeline fed it a recomposed, non-identical object
        // at any stage).
        RepeatedProfilePattern pattern = Assert.Single(result.Profile.RepeatedProfilePatterns);
        Assert.Equal(ProfilePatternFamily.TechnicalFocus, pattern.Family);
        Assert.Equal("C#", pattern.PatternKey);
        Assert.Equal(PatternRuleVersion.V1, pattern.RuleVersion);

        RepresentativeRepository representative = Assert.Single(result.Profile.RepresentativeRepositories);
        Assert.Contains(representative.Repository, new[] { alpha.Reference, beta.Reference });

        // 11/12: an Unsupported/Deferred capability boundary never forces
        // Partial on its own.
        Assert.Equal(
            ProfileReportSectionAvailability.Unsupported,
            result.Report!.ProblemAreaPatterns.Availability);
        Assert.Empty(result.Report.ProblemAreaPatterns.Items);
        Assert.Equal(
            ProfileReportSectionAvailability.Deferred,
            result.Report.RoleEvidenceAlignment.Availability);
        Assert.Empty(result.Report.RoleEvidenceAlignment.Items);
        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);

        // 16: the report conclusion traces to the same pattern, its
        // observations' evidence references, and a real repository analysis.
        ConclusionEvidenceReportItem conclusion = Assert.Single(result.Report.WhyThisConclusion.Items);
        Assert.Same(pattern, conclusion.Pattern);
        Assert.NotEmpty(conclusion.SupportingEvidence);
        Assert.All(
            conclusion.SupportingEvidence,
            evidence =>
            {
                Assert.NotNull(evidence.Analysis);
                Assert.NotEqual(Guid.Empty, evidence.Analysis.AnalysisId);
            });
    }

    [Fact]
    public async Task RunAsync_SparseSingleRepository_RemainsCompleteWithReport()
    {
        // 10: a sparse but fully resolved single-repository input may remain
        // Complete (no promoted pattern requires two independent repositories).
        ProfileRepository alpha = CreateRepository("alpha");
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([alpha]);

        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha", () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.NotNull(result.Report);
        Assert.Empty(result.Profile!.RepeatedProfilePatterns);
        Assert.Equal(
            [ProfileLimitationCode.LimitedIndependentEvidenceSurface],
            result.Profile.Limitations.Select(limitation => limitation.Code));
    }

    [Fact]
    public async Task RunAsync_UnavailableCollection_ProducesPartialWithLimitationsWarningsAndReport()
    {
        // 9: Partial stays Partial, with canonical limitations/warnings
        // preserved and a report still produced.
        ProfileRepository repository = CreateRepository("rate-limited");
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([repository]);

        FakeSnapshotSource source = CreateSource(
            ("octocat/rate-limited", () => CreateUnavailable(DataAvailabilityStatus.RateLimited)));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        Assert.Equal(ProfileCompositionStatus.Partial, result.Status);
        Assert.NotNull(result.Profile);
        Assert.NotNull(result.Report);
        Assert.Equal(
            [
                ProfileWarningCode.RepositoryEvidenceRateLimited,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            result.Profile!.Warnings.Select(warning => warning.Code).Order());
        Assert.Contains(
            ProfileLimitationCode.PartialEvidenceSurface,
            result.Profile.Limitations.Select(limitation => limitation.Code));
        Assert.Equal(result.Profile.Warnings, result.Report!.Warnings);
    }

    [Fact]
    public async Task RunAsync_ActivityClassification_UsesSnapshotCapturedAt_NeverTheSystemClock()
    {
        // 3: the current-activity cutoff is anchored to snapshot.CapturedAt.
        // The injected TimeProvider clock is set so that, if it were used
        // instead, the same repository would misclassify as Active: only the
        // correct clock source keeps this Historical.
        DateTimeOffset lastPushedAt = CapturedAt.AddDays(-400);
        DateTimeOffset wrongClockNow = lastPushedAt.AddDays(100);

        ProfileRepository repository = CreateRepository(
            "historical",
            lastPushedAt: lastPushedAt);
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([repository]);

        FakeSnapshotSource source = CreateSource(
            ("octocat/historical", () => CreateCollected(CreateReadmeEvidenceSnapshot("historical"))));

        ProfilePipelineResult result = await CreatePipeline(
                source,
                timeProvider: new FixedTimeProvider(wrongClockNow))
            .RunAsync(snapshot, TestAnalysisVersion);

        RepositoryEligibilityResult eligibility = Assert.Single(
            result.Profile!.RepositoryEligibilityResults);

        Assert.Equal(ActivityClassification.Historical, eligibility.Activity);
        Assert.False(eligibility.CurrentFocus.IsAllowed);
    }

    [Fact]
    public async Task RunAsync_RepositoryOrderInSnapshot_DoesNotAlterSemanticResult()
    {
        // 14: input repository order must not alter the semantic result.
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfilePipelineResult forward = await CreatePipeline(TwoLanguageSource())
            .RunAsync(CreateProfileSnapshot([alpha, beta]), TestAnalysisVersion);
        ProfilePipelineResult reversed = await CreatePipeline(TwoLanguageSource())
            .RunAsync(CreateProfileSnapshot([beta, alpha]), TestAnalysisVersion);

        AssertSemanticallyEquivalent(forward, reversed);
    }

    [Fact]
    public async Task RunAsync_SameFixedInputAndDeterministicIds_IsReplayDeterministic()
    {
        // 4-7/13: the same orchestration feeds evidence, evidence feeds the
        // pattern set, the pattern set feeds the representative selection,
        // and two independent runs over the same fixed snapshot and
        // deterministic analysis IDs produce a semantically identical
        // profile and report.
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");

        ProfilePipelineResult first = await CreatePipeline(
                TwoLanguageSource(),
                new SequentialAnalysisIdFactory().Next)
            .RunAsync(CreateProfileSnapshot([alpha, beta]), TestAnalysisVersion);
        ProfilePipelineResult second = await CreatePipeline(
                TwoLanguageSource(),
                new SequentialAnalysisIdFactory().Next)
            .RunAsync(CreateProfileSnapshot([alpha, beta]), TestAnalysisVersion);

        AssertSemanticallyEquivalent(first, second);

        // Determinism extends to the analysis identity itself under a fixed
        // ID factory, not merely to the derived profile truth.
        Assert.Equal(
            first.Profile!.RepositoryEligibilityResults.Select(r => r.AnalysisReference?.AnalysisId),
            second.Profile!.RepositoryEligibilityResults.Select(r => r.AnalysisReference?.AnalysisId));
    }

    [Fact]
    public async Task RunAsync_WhenSnapshotCarriesAvatarMetadata_ExposesItOnTheComposedResult()
    {
        // Session 111 STEP 2: the avatar travels straight from this same run's
        // GitHubProfileSnapshot.Metadata to ProfilePipelineResult.AvatarUrl —
        // never through DeveloperPublicEvidenceProfile or ProfileReport, which
        // stay evidence-report truth only.
        Uri avatarUrl = new("https://avatars.githubusercontent.com/u/1");
        ProfileRepository alpha = CreateRepository("alpha");
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [alpha],
            metadata: new GitHubProfileMetadata(
                "The Octocat",
                CapturedAt.AddYears(-10),
                publicRepositoryCount: 1,
                avatarUrl));

        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha", () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        Assert.Equal(avatarUrl, result.AvatarUrl);
    }

    [Fact]
    public async Task RunAsync_WhenSnapshotCarriesNoMetadata_LeavesAvatarUrlNull()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([alpha]);

        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha", () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        Assert.Null(result.AvatarUrl);
    }

    [Fact]
    public async Task RunAsync_WhenSnapshotCarriesRepositoryDescriptions_ExposesThemKeyedByCanonicalReference()
    {
        // Session 111 STEP 3: repository Description travels straight from
        // this same run's GitHubProfileSnapshot.RepositoryPresentationMetadata
        // to ProfilePipelineResult.RepositoryPresentationMetadata — never
        // through DeveloperPublicEvidenceProfile or ProfileReport.
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");
        RepositoryPresentationMetadata descriptions = new(
            new Dictionary<ProfileRepositoryReference, string?>
            {
                [alpha.Reference] = "Alpha's short description.",
                [beta.Reference] = null
            });
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [alpha, beta],
            repositoryPresentationMetadata: descriptions);

        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha", () => CreateCollected(
                CreateLanguageEvidenceSnapshot("alpha", Language("C#")))),
            ("octocat/beta", () => CreateCollected(
                CreateLanguageEvidenceSnapshot("beta", Language("C#")))));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        Assert.Equal(
            "Alpha's short description.",
            result.RepositoryPresentationMetadata.FindDescription(alpha.Reference));
        Assert.Null(result.RepositoryPresentationMetadata.FindDescription(beta.Reference));
    }

    [Fact]
    public async Task RunAsync_WhenSnapshotCarriesNoRepositoryPresentationMetadata_LeavesItEmptyNotFailed()
    {
        ProfileRepository alpha = CreateRepository("alpha");
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([alpha]);

        FakeSnapshotSource source = CreateSource(
            ("octocat/alpha", () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))));

        ProfilePipelineResult result = await CreatePipeline(source).RunAsync(
            snapshot,
            TestAnalysisVersion);

        Assert.Equal(ProfileCompositionStatus.Complete, result.Status);
        Assert.Null(result.RepositoryPresentationMetadata.FindDescription(alpha.Reference));
    }

    [Fact]
    public async Task RunAsync_RepositoryDescriptionsDoNotChangeReportRepresentativeSelectionOrOrder()
    {
        // 5/6/7: adding repository Description must not perturb ProfileReport,
        // RepresentativeRepository or the representative-selection/report
        // order in any way — it is presentation metadata, never a ranking
        // input.
        ProfileRepository alpha = CreateRepository("alpha");
        ProfileRepository beta = CreateRepository("beta");
        RepositoryPresentationMetadata descriptions = new(
            new Dictionary<ProfileRepositoryReference, string?>
            {
                [alpha.Reference] = "Alpha's short description.",
                [beta.Reference] = "Beta's short description."
            });

        ProfilePipelineResult withoutDescriptions = await CreatePipeline(TwoLanguageSource())
            .RunAsync(CreateProfileSnapshot([alpha, beta]), TestAnalysisVersion);
        ProfilePipelineResult withDescriptions = await CreatePipeline(TwoLanguageSource())
            .RunAsync(
                CreateProfileSnapshot([alpha, beta], repositoryPresentationMetadata: descriptions),
                TestAnalysisVersion);

        AssertSemanticallyEquivalent(withoutDescriptions, withDescriptions);
        Assert.Equal(
            withoutDescriptions.Profile!.RepresentativeRepositories.Select(r => r.Repository),
            withDescriptions.Profile!.RepresentativeRepositories.Select(r => r.Repository));
    }

    [Fact]
    public void PipelineResult_Failed_HasNoProfileAndNoReport()
    {
        // 8: Failed composition -> no Profile, no Report at the integration
        // result boundary. Composed/Failed itself is ProfileComposer's
        // exhaustively-tested responsibility (ProfileComposerTests); this
        // isolates the pipeline's own Failed-routing.
        ProfileCompositionResult failed = ProfileCompositionResult.Failed();

        ProfilePipelineResult result = ProfilePipelineResult.Failed(failed);

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.False(result.HasProfile);
        Assert.Null(result.Profile);
        Assert.Null(result.Report);
    }

    [Fact]
    public void PipelineResult_Failed_RejectsAComposedCompositionResult()
    {
        // The public Failed factory accepts only a Failed composition — it
        // cannot be used to wrap a composed profile without a report either.
        ProfileRepository repository = CreateRepository("alpha");
        ProfileComposer composer = new();
        ProfileCompositionResult composed = composer.Compose(
            CreateInput([(repository, Analyzed(repository))]));

        Assert.Throws<ArgumentException>(() => ProfilePipelineResult.Failed(composed));
    }

    [Fact]
    public void PipelineResult_PublicApi_CannotConstructAnArbitraryComposedProfileAndReportPair()
    {
        // MEDIUM finding fix: pairing a ProfileCompositionResult with a
        // ProfileReport is a same-run provenance claim, not a value-shape
        // check, so no public constructor or factory may accept both —
        // otherwise an external caller could combine one profile's
        // composition with a different profile's report. Only
        // ProfilePipeline, via the internal Composed factory, may pair them.
        IEnumerable<MethodBase> publicConstructionMembers = typeof(ProfilePipelineResult)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.ReturnType == typeof(ProfilePipelineResult))
            .Cast<MethodBase>()
            .Concat(typeof(ProfilePipelineResult).GetConstructors(
                BindingFlags.Public | BindingFlags.Instance));

        Assert.All(
            publicConstructionMembers,
            member =>
            {
                ParameterInfo[] parameters = member.GetParameters();

                Assert.False(
                    parameters.Any(p => p.ParameterType == typeof(ProfileCompositionResult))
                        && parameters.Any(p => p.ParameterType == typeof(ProfileReport)),
                    $"'{member}' must not accept both a ProfileCompositionResult and a ProfileReport.");
            });

        // The one public factory that does accept a ProfileCompositionResult
        // (Failed) carries no ProfileReport parameter at all.
        MethodInfo failedFactory = typeof(ProfilePipelineResult)
            .GetMethod(nameof(ProfilePipelineResult.Failed))!;

        Assert.DoesNotContain(
            failedFactory.GetParameters(),
            parameter => parameter.ParameterType == typeof(ProfileReport));
    }

    // -------------------------------------------------------------- fixtures
    //
    // CreatePipeline and AssertSemanticallyEquivalent live in
    // ProfileCompositionTestSupport (shared with the P9-I2 profile validation
    // harness under Profile/Validation/), so this file and the harness never
    // define two independent copies of the same real-pipeline construction or
    // the same determinism comparison.

    private static FakeSnapshotSource TwoLanguageSource() => CreateSource(
        ("octocat/alpha", () => CreateCollected(
            CreateLanguageEvidenceSnapshot("alpha", Language("C#")))),
        ("octocat/beta", () => CreateCollected(
            CreateLanguageEvidenceSnapshot("beta", Language("C#")))));

    private static Dictionary<string, long> Language(string name) => new() { [name] = 100 };
}

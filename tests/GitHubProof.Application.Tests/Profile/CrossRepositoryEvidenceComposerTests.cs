using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

public sealed class CrossRepositoryEvidenceComposerTests
{
    private const string ReadmeSignalKey = "repository.readme.exists";

    private static readonly ObservationRuleVersion TestRuleVersion =
        new("test-observation-rules");

    [Fact]
    public async Task Compose_QuantifiesSupportingEvidenceWithFullTraceability()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [CreateProfileRepository("alpha"), CreateProfileRepository("beta")],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))),
                ("octocat/beta",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("beta")))));

        CrossRepositoryEvidence evidence = Compose(orchestration);
        ObservedProfileEvidence readme = Observation(evidence, ReadmeSignalKey);

        Assert.Equal(2, evidence.EligibleRepositoryCount);
        Assert.Equal(2, readme.SupportingRepositoryCount);
        Assert.Equal(2, readme.IndependentSupportingRepositoryCount);
        Assert.Equal(2, readme.EligibleRepositoryCount);
        Assert.Equal(
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            readme.Kind);
        Assert.Equal(
            new[] { "octocat/alpha", "octocat/beta" },
            readme.ContributingRepositories.Select(
                repository => repository.FullName));
        Assert.Equal(TestRuleVersion, readme.RuleVersion);
        Assert.Equal(TestPolicyVersion, evidence.PolicyVersion);
        Assert.True(evidence.IsEvidenceSurfaceComplete);

        foreach (ProfileEvidenceReference reference in readme.EvidenceReferences)
        {
            RepositoryAnalysis analysis = Analysis(orchestration, reference.Repository);

            Assert.Equal(analysis.AnalysisId, reference.Analysis.AnalysisId);
            Assert.Equal(analysis.Repository, reference.Analysis.Repository);
            Assert.Equal(ReadmeSignalKey, reference.SignalKey);
            Assert.Equal(
                analysis.SignalResults
                    .Single(signal => signal.SignalKey == ReadmeSignalKey)
                    .Evidence.Select(item => item.Key),
                reference.EvidenceKeys);
        }
    }

    [Fact]
    public async Task Compose_ForkedRepository_DoesNotInflateIndependentRepetition()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [
                CreateProfileRepository("alpha"),
                CreateProfileRepository(
                    "forked",
                    fork: RepositoryConditionState.Present)
            ],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))),
                ("octocat/forked",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("forked")))));

        CrossRepositoryEvidence evidence = Compose(orchestration);
        ObservedProfileEvidence readme = Observation(evidence, ReadmeSignalKey);
        RepositoryContributionAssessment forked = Contribution(evidence, "forked");

        Assert.Equal(2, readme.SupportingRepositoryCount);
        Assert.Equal(1, readme.IndependentSupportingRepositoryCount);
        Assert.True(forked.ProfileObservation.IsAllowed);
        Assert.Equal(
            ContributionDecision.NotAllowed,
            forked.IndependentRepetition.Decision);
        Assert.Equal(
            new[] { EligibilityReason.ForkDetected },
            forked.IndependentRepetition.Reasons);
    }

    [Fact]
    public async Task Compose_TemplateDerivedRepository_DoesNotInflateIndependentRepetition()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [
                CreateProfileRepository(
                    "templated",
                    templateDerived: RepositoryConditionState.Present)
            ],
            CreateSource(
                ("octocat/templated",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("templated")))));

        CrossRepositoryEvidence evidence = Compose(orchestration);

        Assert.Equal(
            0,
            Observation(evidence, ReadmeSignalKey).IndependentSupportingRepositoryCount);
        Assert.Equal(
            new[] { EligibilityReason.TemplateDerived },
            Contribution(evidence, "templated").IndependentRepetition.Reasons);
    }

    [Fact]
    public async Task Compose_InsufficientRepositoryEvidence_DoesNotContribute()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [CreateProfileRepository("sparse")],
            CreateSource(
                ("octocat/sparse",
                    () => CreateCollected(CreateRepositorySnapshot("sparse")))));

        CrossRepositoryEvidence evidence = Compose(orchestration);
        RepositoryContributionAssessment sparse = Contribution(evidence, "sparse");

        Assert.Equal(
            AnalysisStatus.InsufficientData,
            Analysis(orchestration, sparse.Reference).Status);
        Assert.Equal(
            ContributionDecision.NotAllowed,
            sparse.ProfileObservation.Decision);
        Assert.Equal(
            new[] { EligibilityReason.InsufficientEvidence },
            sparse.ProfileObservation.Reasons);
        Assert.Equal(0, evidence.EligibleRepositoryCount);
        Assert.Empty(evidence.Observations);
    }

    [Fact]
    public async Task Compose_TechnicalFailure_StaysUndeterminedAndKeepsOtherEvidence()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [
                CreateProfileRepository("alpha"),
                CreateProfileRepository("unreachable"),
                CreateProfileRepository("empty", RepositoryConditionState.Present)
            ],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))),
                ("octocat/unreachable",
                    () => throw new HttpRequestException("GitHub is unreachable."))));

        CrossRepositoryEvidence evidence = Compose(orchestration);
        RepositoryContributionAssessment unreachable =
            Contribution(evidence, "unreachable");
        RepositoryContributionAssessment empty = Contribution(evidence, "empty");

        Assert.Equal(
            ContributionDecision.Undetermined,
            unreachable.ProfileObservation.Decision);
        Assert.Equal(
            new[] { EligibilityReason.RequiredInputUnavailable },
            unreachable.ProfileObservation.Reasons);
        Assert.Equal(
            new[] { EligibilityReason.EmptyRepository },
            empty.ProfileObservation.Reasons);
        Assert.Equal(1, evidence.EligibleRepositoryCount);
        Assert.Equal(1, evidence.UndeterminedContributionRepositoryCount);
        Assert.Equal(
            1,
            Observation(evidence, ReadmeSignalKey).SupportingRepositoryCount);
    }

    [Fact]
    public async Task Compose_UnavailableSignalEvidence_IsUndeterminedRatherThanAbsent()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [CreateProfileRepository("alpha"), CreateProfileRepository("beta")],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))),
                ("octocat/beta",
                    () => CreateCollected(CreateLicenseEvidenceSnapshot("beta")))));

        CrossRepositoryEvidence evidence = Compose(orchestration);
        ObservedProfileEvidence readme = Observation(evidence, ReadmeSignalKey);

        Assert.Equal(
            SignalStatus.UnavailableEvidence,
            Analysis(orchestration, CreateReference("beta"))
                .SignalResults
                .Single(signal => signal.SignalKey == ReadmeSignalKey)
                .Status);
        Assert.Equal(2, evidence.EligibleRepositoryCount);
        Assert.Equal(1, readme.SupportingRepositoryCount);
        Assert.Equal(1, readme.UndeterminedEvidenceRepositoryCount);
        Assert.Equal(2, readme.EligibleRepositoryCount);
    }

    [Fact]
    public async Task Compose_EmitsObservationsOnlyForSignalsWithSupportingEvidence()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [CreateProfileRepository("alpha")],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha")))));

        CrossRepositoryEvidence evidence = Compose(orchestration);
        RepositoryAnalysis analysis = Analysis(orchestration, CreateReference("alpha"));

        Assert.Equal(
            analysis.SignalResults
                .Where(signal => signal.Status is SignalStatus.Strong
                    or SignalStatus.Present
                    or SignalStatus.Partial)
                .Select(signal => signal.SignalKey)
                .OrderBy(key => key, StringComparer.Ordinal),
            evidence.Observations.Select(observation => observation.ObservationKey));
    }

    [Fact]
    public async Task Compose_IncompleteManifest_MarksTheEvidenceSurfaceIncomplete()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [CreateProfileRepository("alpha")],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha")))),
            isRepositoryManifestComplete: false);

        Assert.False(Compose(orchestration).IsEvidenceSurfaceComplete);
    }

    [Fact]
    public async Task Compose_WithSameInputs_ProducesTheSameSemanticEvidence()
    {
        static Task<ProfileAnalysisOrchestrationResult> RunAsync(
            IEnumerable<ProfileRepository> repositories) =>
            OrchestrateAsync(
                repositories,
                CreateSource(
                    ("octocat/alpha",
                        () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha"))),
                    ("octocat/beta",
                        () => CreateCollected(CreateReadmeEvidenceSnapshot("beta"))),
                    ("octocat/gamma",
                        () => CreateCollected(CreateRepositorySnapshot("gamma")))));

        CrossRepositoryEvidence first = Compose(await RunAsync(
        [
            CreateProfileRepository("alpha"),
            CreateProfileRepository("beta"),
            CreateProfileRepository("gamma")
        ]));

        CrossRepositoryEvidence second = Compose(await RunAsync(
        [
            CreateProfileRepository("gamma"),
            CreateProfileRepository("beta"),
            CreateProfileRepository("alpha")
        ]));

        Assert.Equal(Describe(first), Describe(second));
    }

    [Fact]
    public async Task Compose_WhenPolicyVersionsDiffer_FailsFast()
    {
        ProfileAnalysisOrchestrationResult orchestration = await OrchestrateAsync(
            [CreateProfileRepository("alpha")],
            CreateSource(
                ("octocat/alpha",
                    () => CreateCollected(CreateReadmeEvidenceSnapshot("alpha")))));

        CrossRepositoryEvidenceComposer composer = new(
            new RepositoryContributionPolicy(
                new EligibilityPolicyVersion("other-policy")));

        Assert.Throws<ArgumentException>(
            () => composer.Compose(orchestration, TestRuleVersion));
    }

    [Fact]
    public void Compose_WhenRequiredInputIsMissing_FailsFast()
    {
        CrossRepositoryEvidenceComposer composer = CreateComposer();

        Assert.Throws<ArgumentNullException>(
            () => composer.Compose(null!, TestRuleVersion));

        Assert.Throws<ArgumentNullException>(
            () => new CrossRepositoryEvidenceComposer(null!));
    }

    private static CrossRepositoryEvidenceComposer CreateComposer() =>
        new(new RepositoryContributionPolicy(TestPolicyVersion));

    private static CrossRepositoryEvidence Compose(
        ProfileAnalysisOrchestrationResult orchestration) =>
        CreateComposer().Compose(orchestration, TestRuleVersion);

    private static async Task<ProfileAnalysisOrchestrationResult> OrchestrateAsync(
        IEnumerable<ProfileRepository> repositories,
        FakeSnapshotSource source,
        bool isRepositoryManifestComplete = true) =>
        await new ProfileRepositoryAnalysisOrchestrator(
                source,
                CreateAnalysisRunner(),
                new AnalysisAdmissionPolicy(TestPolicyVersion),
                new SequentialAnalysisIdFactory().Next)
            .AnalyzeAsync(
                CreateProfileSnapshot(repositories, isRepositoryManifestComplete),
                TestAnalysisVersion);

    private static ObservedProfileEvidence Observation(
        CrossRepositoryEvidence evidence,
        string observationKey) =>
        evidence.Observations.Single(observation => string.Equals(
            observation.ObservationKey,
            observationKey,
            StringComparison.Ordinal));

    private static RepositoryContributionAssessment Contribution(
        CrossRepositoryEvidence evidence,
        string name) =>
        evidence.RepositoryContributions.Single(contribution => string.Equals(
            contribution.Reference.Name,
            name,
            StringComparison.Ordinal));

    private static RepositoryAnalysis Analysis(
        ProfileAnalysisOrchestrationResult orchestration,
        ProfileRepositoryReference reference) =>
        orchestration.Results
            .Single(result => result.Reference == reference)
            .Analysis!;

    private static IEnumerable<string> Describe(CrossRepositoryEvidence evidence) =>
        evidence.Observations.Select(observation =>
            $"{observation.ObservationKey}:{observation.SupportingRepositoryCount}"
            + $":{observation.IndependentSupportingRepositoryCount}"
            + $":{observation.EligibleRepositoryCount}"
            + $":{observation.UndeterminedEvidenceRepositoryCount}"
            + $":{string.Join(",", observation.ContributingRepositories.Select(repository => repository.FullName))}");
}

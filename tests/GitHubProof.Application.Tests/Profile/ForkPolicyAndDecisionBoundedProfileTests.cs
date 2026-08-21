using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile;

// Session 099 end-to-end proofs for the two Session 098 decisions that reach the
// profile pipeline: Fork Policy B, and the analysis-neutrality of a
// DecisionBounded collection.
public sealed class ForkPolicyAndDecisionBoundedProfileTests
{
    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    private static readonly Guid AnalysisId =
        Guid.Parse("09900000-0000-0000-0000-000000000001");

    // ------------------------------------------------------------ fork policy

    // The whole point of admitting at Stage A: the expensive collection never
    // starts, so no fork repository GitHub request can be issued.
    [Fact]
    public async Task KnownFork_NeverInvokesTheRepositorySnapshotSource()
    {
        ProfileRepository fork = CreateRepository(
            "forked",
            fork: RepositoryConditionState.Present);
        ProfileRepository original = CreateRepository("original");

        FakeSnapshotSource source = CreateSource(original, fork);

        await CreatePipeline(source, policy: EligibilityPolicy.V2)
            .RunAsync(CreateProfileSnapshot([original, fork]), Version);

        Assert.Equal(["octocat/original"], source.RequestedRepositories);
    }

    [Fact]
    public async Task KnownFork_IsNonContributingAndCarriesNoAnalysisReference()
    {
        ProfileRepository fork = CreateRepository(
            "forked",
            fork: RepositoryConditionState.Present);
        ProfileRepository original = CreateRepository("original");

        ProfilePipelineResult result =
            await CreatePipeline(CreateSource(original, fork), policy: EligibilityPolicy.V2)
                .RunAsync(CreateProfileSnapshot([original, fork]), Version);

        RepositoryEligibilityResult forkResult = Assert.Single(
            result.Profile!.RepositoryEligibilityResults,
            eligibility => eligibility.Reference.Name == "forked");

        Assert.Equal(RepositoryEligibilityStatus.NonContributing, forkResult.Status);
        Assert.Null(forkResult.AnalysisReference);
        Assert.Contains(
            EligibilityReason.ForkDetected,
            forkResult.ProfileObservation.Reasons);
    }

    // The fork stays visible: it remains in the repository universe and keeps
    // its drill-down row, so its absence from evidence is never narrated as an
    // absence of work.
    [Fact]
    public async Task KnownFork_RemainsVisibleInTheProfileRepositoryUniverse()
    {
        ProfileRepository fork = CreateRepository(
            "forked",
            fork: RepositoryConditionState.Present);
        ProfileRepository original = CreateRepository("original");

        ProfilePipelineResult result =
            await CreatePipeline(CreateSource(original, fork), policy: EligibilityPolicy.V2)
                .RunAsync(CreateProfileSnapshot([original, fork]), Version);

        Assert.Equal(2, result.Profile!.RepositoryEligibilityResults.Count);
        Assert.Contains(
            result.Report!.RepositoryDrillDown.Items,
            item => item.Repository.Name == "forked");
    }

    // A fork that cannot be collected can no longer drag the whole profile to
    // Partial, because collecting it is not a V1 evidence obligation.
    [Fact]
    public async Task KnownFork_DoesNotForceThePartialProfileStatus()
    {
        ProfileRepository fork = CreateRepository(
            "forked",
            fork: RepositoryConditionState.Present);
        ProfileRepository[] originals =
            [CreateRepository("alpha"), CreateRepository("beta")];

        ProfilePipelineResult result = await CreatePipeline(
                CreateSource([.. originals, fork]),
                policy: EligibilityPolicy.V2)
            .RunAsync(CreateProfileSnapshot([.. originals, fork]), Version);

        Assert.Equal(ProfileAnalysisStatus.Complete, result.Profile!.AnalysisStatus);
    }

    // The narrow invariant Session 098 §17.8 locks: pattern identity and
    // representative selection are preserved, while fork-derived traceability
    // may intentionally disappear. Exact trace equality is NOT asserted.
    [Fact]
    public async Task ForkPolicy_PreservesPatternIdentityAndRepresentativeSelection()
    {
        ProfileRepository[] independents =
            [CreateRepository("alpha"), CreateRepository("beta")];
        ProfileRepository fork = CreateRepository(
            "forked",
            fork: RepositoryConditionState.Present);

        ProfilePipelineResult analysed = await CreatePipeline(
                CreateSource([.. independents, fork]),
                new SequentialAnalysisIdFactory().Next,
                policy: EligibilityPolicy.V1)
            .RunAsync(CreateProfileSnapshot([.. independents, fork]), Version);

        ProfilePipelineResult skipped = await CreatePipeline(
                CreateSource([.. independents, fork]),
                new SequentialAnalysisIdFactory().Next,
                policy: EligibilityPolicy.V2)
            .RunAsync(CreateProfileSnapshot([.. independents, fork]), Version);

        Assert.Equal(
            analysed.Profile!.RepeatedProfilePatterns
                .Select(pattern => (pattern.Family, pattern.PatternKey)),
            skipped.Profile!.RepeatedProfilePatterns
                .Select(pattern => (pattern.Family, pattern.PatternKey)));
        Assert.Equal(
            analysed.Profile.RepresentativeRepositories.Select(item => item.Repository),
            skipped.Profile.RepresentativeRepositories.Select(item => item.Repository));

        // The fork contributed observations under the old policy and none under
        // the new one. That difference is the expected eligibility change.
        Assert.NotEmpty(analysed.Profile.RepeatedProfilePatterns);
        Assert.True(
            analysed.Profile.CrossRepositoryEvidence.EligibleRepositoryCount
                > skipped.Profile.CrossRepositoryEvidence.EligibleRepositoryCount);
    }

    // -------------------------------------------- decision-bounded neutrality

    // A DecisionBounded collection alone produces no warning, no
    // collection-partial limitation and no status downgrade.
    [Fact]
    public void DecisionBoundedCommits_ProduceTheSameAnalysisAsAnEnumeratedCollection()
    {
        RepositoryAnalysis enumerated = Analyze(CreateCommitSnapshot(bounded: false));
        RepositoryAnalysis bounded = Analyze(CreateCommitSnapshot(bounded: true));

        // The claim is that the bound changes nothing, not that this minimal
        // fixture reaches Completed: the other surfaces are legitimately
        // unavailable in it, and both arms must agree on whatever that yields.
        Assert.Equal(enumerated.Status, bounded.Status);
        Assert.Equal(
            enumerated.ScoreBreakdown.EarnedScore,
            bounded.ScoreBreakdown.EarnedScore);
        Assert.Equal(enumerated.LimitationKeys, bounded.LimitationKeys);
        Assert.DoesNotContain(
            "limitation.snapshot.collection-partial",
            bounded.LimitationKeys);
        Assert.Equal(
            enumerated.Warnings.Select(warning => warning.Code),
            bounded.Warnings.Select(warning => warning.Code));
        Assert.Equal(
            enumerated.SignalResults.Select(signal =>
                $"{signal.SignalKey}:{signal.Status}:{signal.Score}"),
            bounded.SignalResults.Select(signal =>
                $"{signal.SignalKey}:{signal.Status}:{signal.Score}"));
    }

    // ---------------------------------------------------------------- setup

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(CapturedAt))
            .Analyze(snapshot, Version, AnalysisId);

    private static RepositorySnapshot CreateCommitSnapshot(bool bounded)
    {
        RepositoryCommit[] commits =
        [
            .. Enumerable.Range(0, 12).Select(index => new RepositoryCommit(
                $"sha-{index}",
                CapturedAt.AddDays(-index * 2),
                $"commit {index}",
                "octocat"))
        ];

        return new RepositorySnapshot(
            new RepositoryIdentity(
                "octocat",
                "alpha",
                new Uri("https://github.com/octocat/alpha"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            new RepositoryMetadata(null, null, CapturedAt.AddYears(-2), issuesEnabled: true),
            new RepositoryReadme("README.md", "# Project\n\nOverview."),
            fileTree: null,
            bounded
                ? RepositoryCollection<RepositoryCommit>.DecisionBounded(commits)
                : new RepositoryCollection<RepositoryCommit>(commits),
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            [
                DataAvailability.Available("metadata", CapturedAt),
                DataAvailability.Available("readme", CapturedAt),
                DataAvailability.Available("commits", CapturedAt)
            ],
            collectionWarnings: null);
    }

    private static FakeSnapshotSource CreateSource(params ProfileRepository[] repositories) =>
        new(repositories.ToDictionary(
            repository => repository.Reference.FullName,
            repository => new Func<ProfileRepositorySnapshotResult>(
                () => new ProfileRepositorySnapshotResult(
                    CreateLanguageEvidenceSnapshot(
                        repository.Reference.Name,
                        new Dictionary<string, long> { ["C#"] = 1200 }),
                    DataAvailability.Available("metadata", CapturedAt)))));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

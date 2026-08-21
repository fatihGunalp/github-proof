using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

// Fork Policy B, locked by Session 098 and implemented by Session 099:
// docs/architecture/public-profile-evidence-repository-eligibility.md §6.3,
// §19.3.2, §22.1.1, §25.
//
// This is an eligibility semantics change, not a performance optimization: a
// known fork contributes no evidence at all because Profile V1 owns no
// attribution contract able to prove which fork changes belong to the owner.
public sealed class ForkAdmissionPolicyTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-21T00:00:00Z");

    // ------------------------------------------------------------ admission

    [Fact]
    public void KnownFork_UnderTheCurrentPolicy_IsSkippedWithForkDetected()
    {
        AnalysisAdmissionResult result = new AnalysisAdmissionPolicy(EligibilityPolicy.V2)
            .Evaluate(CreateRepository(fork: RepositoryConditionState.Present));

        Assert.Equal(AnalysisAdmissionDecision.Skip, result.Decision);
        Assert.Equal(EligibilityReason.ForkDetected, result.Reason);
    }

    // The previous policy version keeps its own rule: it is immutable history.
    [Fact]
    public void KnownFork_UnderPolicyV1_IsStillAnalyzed()
    {
        AnalysisAdmissionResult result = new AnalysisAdmissionPolicy(EligibilityPolicy.V1)
            .Evaluate(CreateRepository(fork: RepositoryConditionState.Present));

        Assert.Equal(AnalysisAdmissionDecision.Analyze, result.Decision);
        Assert.Null(result.Reason);
    }

    // An empty repository keeps reporting exactly the reason it always did,
    // even when it is also a fork.
    [Fact]
    public void EmptyFork_StillReportsEmptyRepository()
    {
        AnalysisAdmissionResult result = new AnalysisAdmissionPolicy(EligibilityPolicy.V2)
            .Evaluate(CreateRepository(
                fork: RepositoryConditionState.Present,
                empty: RepositoryConditionState.Present));

        Assert.Equal(AnalysisAdmissionDecision.Skip, result.Decision);
        Assert.Equal(EligibilityReason.EmptyRepository, result.Reason);
    }

    [Fact]
    public void NonForkRepository_IsUnaffected()
    {
        AnalysisAdmissionResult result = new AnalysisAdmissionPolicy(EligibilityPolicy.V2)
            .Evaluate(CreateRepository(fork: RepositoryConditionState.Absent));

        Assert.Equal(AnalysisAdmissionDecision.Analyze, result.Decision);
    }

    // TemplateDerived semantics are explicitly NOT changed by Session 098/099.
    [Fact]
    public void TemplateDerivedRepository_IsStillAnalyzed()
    {
        AnalysisAdmissionResult result = new AnalysisAdmissionPolicy(EligibilityPolicy.V2)
            .Evaluate(CreateRepository(
                fork: RepositoryConditionState.Absent,
                templateDerived: RepositoryConditionState.Present));

        Assert.Equal(AnalysisAdmissionDecision.Analyze, result.Decision);
    }

    // ----------------------------------------------------------- eligibility

    // §22.1.1 keeps ProfileContext Allowed; only the four evidence-bearing
    // purposes are withheld.
    [Fact]
    public void SkippedFork_IsNonContributingWithEveryEvidencePurposeNotAllowed()
    {
        ProfileRepository repository = CreateRepository(
            fork: RepositoryConditionState.Present);
        AnalysisAdmissionResult admission =
            new AnalysisAdmissionPolicy(EligibilityPolicy.V2).Evaluate(repository);

        RepositoryEligibilityResult result =
            new RepositoryEligibilityPolicy(EligibilityPolicy.V2).Evaluate(
                repository,
                admission,
                analysisStatus: null,
                analysisReference: null,
                CapturedAt);

        Assert.Equal(RepositoryEligibilityStatus.NonContributing, result.Status);
        Assert.False(result.ProfileObservation.IsAllowed);
        Assert.False(result.IndependentRepetition.IsAllowed);
        Assert.False(result.CurrentFocus.IsAllowed);
        Assert.False(result.HistoricalEvidence.IsAllowed);

        // No analysis ran, so no analysis reference can exist.
        Assert.Null(result.AnalysisReference);

        // Not Undetermined: a resolved NotAllowed is a decided outcome and can
        // never force the profile to Partial.
        Assert.NotEqual(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    [Fact]
    public void SkippedFork_ReportsForkDetectedAsItsReason()
    {
        ProfileRepository repository = CreateRepository(
            fork: RepositoryConditionState.Present);
        AnalysisAdmissionResult admission =
            new AnalysisAdmissionPolicy(EligibilityPolicy.V2).Evaluate(repository);

        RepositoryEligibilityResult result =
            new RepositoryEligibilityPolicy(EligibilityPolicy.V2).Evaluate(
                repository,
                admission,
                analysisStatus: null,
                analysisReference: null,
                CapturedAt);

        Assert.Contains(EligibilityReason.ForkDetected, result.ProfileObservation.Reasons);
        Assert.Contains(EligibilityReason.ForkDetected, result.HistoricalEvidence.Reasons);
    }

    // -------------------------------------------------------- policy version

    [Fact]
    public void PolicyV1_RemainsImmutable()
    {
        Assert.Equal("1.0.0", EligibilityPolicy.V1.Version.Value);
        Assert.Equal(TimeSpan.FromDays(365), EligibilityPolicy.V1.CurrentActivityWindow);
        Assert.Equal(ForkAdmissionRule.Analyze, EligibilityPolicy.V1.ForkAdmission);
    }

    // The fork rule is a breaking change, so it belongs to a new major version.
    // The activity window is deliberately identical.
    [Fact]
    public void PolicyV2_IsTheNewMajorVersionWithTheSameActivityWindow()
    {
        Assert.Equal("2.0.0", EligibilityPolicy.V2.Version.Value);
        Assert.Equal(TimeSpan.FromDays(365), EligibilityPolicy.V2.CurrentActivityWindow);
        Assert.Equal(ForkAdmissionRule.Skip, EligibilityPolicy.V2.ForkAdmission);
    }

    // The version-ownership invariant, end to end: there is no construction
    // path that pairs the V2 provenance with V1 fork behavior. The admission
    // policy takes an EligibilityPolicy and nothing else, and EligibilityPolicy
    // refuses to mint a V2-versioned definition carrying Analyze.
    [Fact]
    public void NoAdmissionPolicyCanCarryV2ProvenanceWithAnalyzeSemantics()
    {
        // The only way to build one is from a policy, and the only policy whose
        // version is 2.0.0 is the canonical V2, which carries Skip.
        Assert.Throws<ArgumentException>(() => EligibilityPolicy.Create(
            EligibilityPolicy.V2.Version,
            TimeSpan.FromDays(365),
            ForkAdmissionRule.Analyze));

        AnalysisAdmissionPolicy resolved = new(
            EligibilityPolicy.Create(
                EligibilityPolicy.V2.Version,
                TimeSpan.FromDays(365),
                ForkAdmissionRule.Skip));

        Assert.Equal(EligibilityPolicy.V2.Version, resolved.PolicyVersion);
        Assert.Equal(ForkAdmissionRule.Skip, resolved.ForkAdmission);
        Assert.Equal(
            AnalysisAdmissionDecision.Skip,
            resolved.Evaluate(CreateRepository(fork: RepositoryConditionState.Present))
                .Decision);

        // And the constructor surface really is policy-only: no overload can
        // accept a bare version and invent a rule for it.
        Assert.Equal(
            [typeof(EligibilityPolicy)],
            typeof(AnalysisAdmissionPolicy)
                .GetConstructors()
                .Select(constructor =>
                    Assert.Single(constructor.GetParameters()).ParameterType));
    }

    [Fact]
    public void ACanonicalVersionCannotBeReboundToADifferentForkRule()
    {
        Assert.Throws<ArgumentException>(() => EligibilityPolicy.Create(
            EligibilityPolicy.V2.Version,
            TimeSpan.FromDays(365),
            ForkAdmissionRule.Analyze));
    }

    private static ProfileRepository CreateRepository(
        RepositoryConditionState fork,
        RepositoryConditionState templateDerived = RepositoryConditionState.Absent,
        RepositoryConditionState empty = RepositoryConditionState.Absent) =>
        new(
            new ProfileRepositoryReference(
                "octocat",
                "forked",
                new Uri("https://github.com/octocat/forked")),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork,
            templateDerived,
            RepositoryConditionState.Absent,
            empty,
            CapturedAt.AddDays(-10),
            CapturedAt.AddYears(-2));
}

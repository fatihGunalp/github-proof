using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.Models;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile.Validation;

// P9-I4 — Determinism + Exit Validation
// (docs/product/p9-profile-validation-contract.md §8;
// docs/product/p9-implementation-plan.md §8 P9-I4).
//
// This file adds only what P9-I3 explicitly left to P9-I4: determinism and
// repository-order invariance, proven across all 11 canonical scenario
// fixtures (P9-I2) through the real ProfilePipeline (the P9-I2 harness). It
// reuses the exact production determinism mechanism already established —
// ProfileCompositionTestSupport.AssertSemanticallyEquivalent, the same
// nine-field comparison surface contract §8.1 locks and
// ProfilePipelineTests already exercises on one fixed pair — rather than
// defining a second, independently-drifting comparison. It does not
// re-derive the other three P9 exit legs (prohibited inference, unsupported
// profile claim, evidence traceability): those are already fully proven,
// per scenario, by P9-I3's ProfileValidationRiskTests
// (EveryScenario_AllMeaningfulClaimsAreTraceable,
// EveryScenario_ProblemAreaStaysUnsupportedAndRoleEvidenceStaysDeferred,
// ProfileAndReportTypes_ExposeNoProhibitedInferenceProperty) and are reused
// by citation, not by a second implementation, in the P9 exit verdict.
public sealed class ProfileValidationDeterminismTests
{
    [Theory]
    [MemberData(
        nameof(ProfileValidationScenarioFixturesTests.AllScenarios),
        MemberType = typeof(ProfileValidationScenarioFixturesTests))]
    public async Task EveryScenario_IsReplayDeterministic_UnderFixedAnalysisIds(
        ProfileValidationScenario scenario)
    {
        // Two independent scenario builds (fresh snapshot + fresh source
        // each), run through two independent pipeline executions, each with
        // its own freshly-seeded deterministic analysis-ID factory —
        // mirroring ProfilePipelineTests.
        // RunAsync_SameFixedInputAndDeterministicIds_IsReplayDeterministic,
        // extended to all 11 scenarios instead of one ad-hoc fixture.
        (GitHubProfileSnapshot firstSnapshot, FakeSnapshotSource firstSource) =
            ProfileValidationScenarioFixtures.Build(scenario);
        (GitHubProfileSnapshot secondSnapshot, FakeSnapshotSource secondSource) =
            ProfileValidationScenarioFixtures.Build(scenario);

        ProfilePipelineResult first = await ProfileValidationHarness.RunAsync(
            firstSource,
            firstSnapshot,
            new SequentialAnalysisIdFactory().Next);
        ProfilePipelineResult second = await ProfileValidationHarness.RunAsync(
            secondSource,
            secondSnapshot,
            new SequentialAnalysisIdFactory().Next);

        // The contract §8.1 nine-field semantic surface.
        AssertSemanticallyEquivalent(first, second);

        // Determinism extends to the analysis identity itself under a fixed
        // ID factory (contract §8.2: AnalysisId values participate in
        // determinism only when fixed as input, as they are here).
        Assert.Equal(
            first.Profile!.RepositoryEligibilityResults.Select(
                r => r.AnalysisReference?.AnalysisId),
            second.Profile!.RepositoryEligibilityResults.Select(
                r => r.AnalysisReference?.AnalysisId));
    }

    [Theory]
    [MemberData(
        nameof(ProfileValidationScenarioFixturesTests.AllScenarios),
        MemberType = typeof(ProfileValidationScenarioFixturesTests))]
    public async Task EveryScenario_RepositoryInputOrderDoesNotAlterTheSemanticResult(
        ProfileValidationScenario scenario)
    {
        // Same technique as ProfilePipelineTests.
        // RunAsync_RepositoryOrderInSnapshot_DoesNotAlterSemanticResult,
        // applied to every scenario: a snapshot built from the same
        // repositories in reversed order must still produce a semantically
        // equivalent result.
        (GitHubProfileSnapshot forwardSnapshot, FakeSnapshotSource forwardSource) =
            ProfileValidationScenarioFixtures.Build(scenario);
        (GitHubProfileSnapshot reversedBaseSnapshot, FakeSnapshotSource reversedSource) =
            ProfileValidationScenarioFixtures.Build(scenario);

        GitHubProfileSnapshot reversedSnapshot = CreateProfileSnapshot(
            reversedBaseSnapshot.Repositories.Repositories.Reverse());

        ProfilePipelineResult forward = await ProfileValidationHarness.RunAsync(
            forwardSource,
            forwardSnapshot);
        ProfilePipelineResult reversed = await ProfileValidationHarness.RunAsync(
            reversedSource,
            reversedSnapshot);

        AssertSemanticallyEquivalent(forward, reversed);
    }
}

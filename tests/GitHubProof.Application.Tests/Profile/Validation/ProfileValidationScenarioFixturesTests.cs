using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Profile.Validation;

// Resolves independent-review Finding F1 (Session 090):
// docs/product/p9-profile-validation-contract.md §10 item 1 requires 11
// concrete deterministic scenario fixtures, not only 11 representable
// scenario identities. This file proves, for every one of the 11 canonical
// identities:
//
//   1. a concrete fixture exists (ProfileValidationScenarioFixtures.Build
//      handles it without throwing),
//   2. that fixture is constructed deterministically (two independent
//      builds of the same scenario produce the same canonical
//      SnapshotIdentity — the exact production determinism mechanism
//      SnapshotIdentity.FromSemanticContent already provides, reused here
//      rather than reimplemented),
//   3. that fixture executes through the real ProfilePipeline without any
//      network access (FakeSnapshotSource only).
//
// It does NOT validate any P9 risk (contract §3) for any scenario — that
// remains P9-I3's responsibility. No assertion here reads risk-relevant
// content (patterns, limitations, claims); only structural well-formedness
// of the pipeline result is checked.
public sealed class ProfileValidationScenarioFixturesTests
{
    public static TheoryData<ProfileValidationScenario> AllScenarios()
    {
        TheoryData<ProfileValidationScenario> data = [];

        foreach (ProfileValidationScenario scenario in Enum.GetValues<ProfileValidationScenario>())
        {
            data.Add(scenario);
        }

        return data;
    }

    [Fact]
    public void FixtureInventory_CoversAllElevenCanonicalScenarios()
    {
        ProfileValidationScenario[] allIdentities = Enum.GetValues<ProfileValidationScenario>();
        Assert.Equal(11, allIdentities.Length);

        foreach (ProfileValidationScenario scenario in allIdentities)
        {
            (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
                ProfileValidationScenarioFixtures.Build(scenario);

            Assert.NotEmpty(snapshot.Repositories.Repositories);
            Assert.NotNull(source);
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public void Fixture_IsConstructedDeterministically(ProfileValidationScenario scenario)
    {
        (GitHubProfileSnapshot first, _) = ProfileValidationScenarioFixtures.Build(scenario);
        (GitHubProfileSnapshot second, _) = ProfileValidationScenarioFixtures.Build(scenario);

        // Reuses the exact production determinism mechanism
        // (SnapshotIdentity.FromSemanticContent, docs/architecture/
        // public-profile-evidence-domain-model.md §6) rather than a second,
        // independently-defined comparison: two independent builds of the
        // same scenario must normalize to the same canonical semantic
        // digest.
        SnapshotIdentity firstIdentity = SnapshotIdentity.FromSemanticContent(first);
        SnapshotIdentity secondIdentity = SnapshotIdentity.FromSemanticContent(second);

        Assert.Equal(firstIdentity, secondIdentity);
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Fixture_ExecutesThroughRealPipelineWithoutNetworkAccess(
        ProfileValidationScenario scenario)
    {
        (GitHubProfileSnapshot snapshot, ProfileAnalysisTestSupport.FakeSnapshotSource source) =
            ProfileValidationScenarioFixtures.Build(scenario);

        // FakeSnapshotSource is the only IProfileRepositorySnapshotSource
        // this harness ever constructs — no HttpClient/IGitHubClient is
        // referenced anywhere in this file or in
        // ProfileValidationScenarioFixtures, so this run is structurally
        // network-free regardless of scenario.
        ProfilePipelineResult result = await ProfileValidationHarness.RunAsync(source, snapshot);

        // Structural well-formedness only (proof item 3): the real pipeline
        // ran to completion without throwing, and the composed/failed
        // result boundary (report contract's own invariant) held. No risk,
        // pattern or claim content is inspected here.
        Assert.Equal(result.HasProfile, result.Report is not null);

        if (result.HasProfile)
        {
            Assert.NotNull(result.Profile);
        }
    }
}

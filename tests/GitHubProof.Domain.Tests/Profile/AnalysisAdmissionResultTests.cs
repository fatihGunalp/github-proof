using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class AnalysisAdmissionResultTests
{
    private static readonly EligibilityPolicyVersion PolicyVersion =
        new("test-eligibility-policy");

    [Fact]
    public void Constructor_WhenAnalyzeCarriesReason_Rejects()
    {
        Assert.Throws<ArgumentException>(() => new AnalysisAdmissionResult(
            CreateReference(),
            AnalysisAdmissionDecision.Analyze,
            EligibilityReason.EmptyRepository,
            PolicyVersion));
    }

    [Theory]
    [InlineData(AnalysisAdmissionDecision.Skip)]
    [InlineData(AnalysisAdmissionDecision.Undetermined)]
    public void Constructor_WhenRestrictedDecisionHasNoReason_Rejects(
        AnalysisAdmissionDecision decision)
    {
        Assert.Throws<ArgumentException>(() => new AnalysisAdmissionResult(
            CreateReference(),
            decision,
            reason: null,
            PolicyVersion));
    }

    [Fact]
    public void Constructor_WhenDecisionIsUndefined_Rejects()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AnalysisAdmissionResult(
            CreateReference(),
            (AnalysisAdmissionDecision)99,
            EligibilityReason.EmptyRepository,
            PolicyVersion));
    }

    [Theory]
    [InlineData(EligibilityReason.Archived)]
    [InlineData(EligibilityReason.InsufficientEvidence)]
    public void Constructor_WhenReasonIsNotAnAdmissionReason_Rejects(
        EligibilityReason reason)
    {
        Assert.Throws<ArgumentException>(() => new AnalysisAdmissionResult(
            CreateReference(),
            AnalysisAdmissionDecision.Skip,
            reason,
            PolicyVersion));
    }

    // The canonical Stage A decision/reason pairs, each with the decision it
    // actually belongs to. Session 098 eligibility contract §6.3 added
    // ForkDetected to Skip: known fork lineage is a manifest-only fact that
    // resolves admission itself, exactly as EmptyRepository already did.
    // ForkDetected was already a canonical reason identity (§15.2); only the
    // stage that may report it changed. RequiredInputUnavailable belongs to
    // Undetermined, never to Skip.
    [Theory]
    [InlineData(AnalysisAdmissionDecision.Skip, EligibilityReason.EmptyRepository)]
    [InlineData(AnalysisAdmissionDecision.Skip, EligibilityReason.ForkDetected)]
    [InlineData(
        AnalysisAdmissionDecision.Undetermined,
        EligibilityReason.RequiredInputUnavailable)]
    public void Constructor_AcceptsEveryCanonicalAdmissionPair(
        AnalysisAdmissionDecision decision,
        EligibilityReason reason)
    {
        AnalysisAdmissionResult result = new(
            CreateReference(),
            decision,
            reason,
            PolicyVersion);

        Assert.Equal(decision, result.Decision);
        Assert.Equal(reason, result.Reason);
    }

    // AnalysisAdmissionPolicy is the canonical producer, and it only ever emits
    // the pairs above. The constructor stays permissive about pairing for
    // backward compatibility, which this test deliberately does not describe as
    // canonical.
    [Fact]
    public void ThePolicyOnlyEverProducesCanonicalPairs()
    {
        Assert.Equal(
            AnalysisAdmissionDecision.Skip,
            Produce(RepositoryConditionState.Present, RepositoryConditionState.Absent).Decision);
        Assert.Equal(
            EligibilityReason.EmptyRepository,
            Produce(RepositoryConditionState.Present, RepositoryConditionState.Absent).Reason);

        Assert.Equal(
            AnalysisAdmissionDecision.Skip,
            Produce(RepositoryConditionState.Absent, RepositoryConditionState.Present).Decision);
        Assert.Equal(
            EligibilityReason.ForkDetected,
            Produce(RepositoryConditionState.Absent, RepositoryConditionState.Present).Reason);

        Assert.Equal(
            AnalysisAdmissionDecision.Undetermined,
            Produce(RepositoryConditionState.Undetermined, RepositoryConditionState.Absent).Decision);
        Assert.Equal(
            EligibilityReason.RequiredInputUnavailable,
            Produce(RepositoryConditionState.Undetermined, RepositoryConditionState.Absent).Reason);
    }

    private static AnalysisAdmissionResult Produce(
        RepositoryConditionState empty,
        RepositoryConditionState fork) =>
        new AnalysisAdmissionPolicy(EligibilityPolicy.V2).Evaluate(
            new ProfileRepository(
                CreateReference(),
                defaultBranch: "main",
                primaryLanguage: "C#",
                fork,
                RepositoryConditionState.Absent,
                RepositoryConditionState.Absent,
                empty,
                DateTimeOffset.Parse("2026-08-11T00:00:00Z"),
                DateTimeOffset.Parse("2024-08-21T00:00:00Z")));

    [Fact]
    public void Constructor_WhenPolicyVersionIsNull_Rejects()
    {
        Assert.Throws<ArgumentNullException>(() => new AnalysisAdmissionResult(
            CreateReference(),
            AnalysisAdmissionDecision.Analyze,
            reason: null,
            policyVersion: null!));
    }

    private static ProfileRepositoryReference CreateReference() =>
        new(
            "octocat",
            "hello-world",
            new Uri("https://github.com/octocat/hello-world"));
}

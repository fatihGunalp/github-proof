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
    [InlineData(EligibilityReason.ForkDetected)]
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

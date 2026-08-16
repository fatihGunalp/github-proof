using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

public sealed class ProfileAnalysisOrchestrationResultTests
{
    [Fact]
    public void Constructor_OrdersResultsIndependentlyOfProductionOrder()
    {
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
        [
            CreateProfileRepository("beta"),
            CreateProfileRepository("alpha"),
            CreateProfileRepository("zeta", owner: "alpha")
        ]);

        ProfileAnalysisOrchestrationResult result = new(
            snapshot,
            TestAnalysisVersion,
            TestPolicyVersion,
            [
                Undetermined(CreateReference("beta")),
                Undetermined(CreateReference("zeta", "alpha")),
                Undetermined(CreateReference("alpha"))
            ]);

        Assert.Equal(
            new[] { "alpha/zeta", "octocat/alpha", "octocat/beta" },
            result.Results.Select(repository => repository.Reference.FullName));
    }

    [Fact]
    public void Constructor_WhenResultDoesNotBelongToTheSnapshot_Rejects()
    {
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [CreateProfileRepository("alpha")]);

        Assert.Throws<ArgumentException>(() => new ProfileAnalysisOrchestrationResult(
            snapshot,
            TestAnalysisVersion,
            TestPolicyVersion,
            [Undetermined(CreateReference("foreign"))]));
    }

    [Fact]
    public void Constructor_WhenResultsRepeatOneRepository_Rejects()
    {
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [CreateProfileRepository("alpha")]);

        Assert.Throws<ArgumentException>(() => new ProfileAnalysisOrchestrationResult(
            snapshot,
            TestAnalysisVersion,
            TestPolicyVersion,
            [
                Undetermined(CreateReference("alpha")),
                Undetermined(CreateReference("alpha"))
            ]));
    }

    [Fact]
    public void Constructor_ExposesManifestCompleteness()
    {
        ProfileAnalysisOrchestrationResult result = new(
            CreateProfileSnapshot(
                [CreateProfileRepository("alpha")],
                isComplete: false),
            TestAnalysisVersion,
            TestPolicyVersion,
            [Undetermined(CreateReference("alpha"))]);

        Assert.False(result.IsRepositoryManifestComplete);
        Assert.Single(result.Results);
    }

    [Fact]
    public void Constructor_WhenARepositoryResultIsMissing_Rejects()
    {
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
        [
            CreateProfileRepository("alpha"),
            CreateProfileRepository("beta")
        ]);

        Assert.Throws<ArgumentException>(() => new ProfileAnalysisOrchestrationResult(
            snapshot,
            TestAnalysisVersion,
            TestPolicyVersion,
            [Undetermined(CreateReference("alpha"))]));
    }

    [Fact]
    public void RepositoryResult_RejectsOutcomesThatContradictTheAdmissionDecision()
    {
        AnalysisAdmissionResult analyze = new(
            CreateReference("alpha"),
            AnalysisAdmissionDecision.Analyze,
            reason: null,
            TestPolicyVersion);

        Assert.Throws<ArgumentException>(
            () => ProfileRepositoryAnalysisResult.NotAdmitted(analyze));

        Assert.Throws<ArgumentException>(
            () => ProfileRepositoryAnalysisResult.AdmissionUndetermined(analyze));

        Assert.Throws<ArgumentException>(
            () => ProfileRepositoryAnalysisResult.Analyzed(
                analyze,
                CreateAnalysisRunner().Analyze(
                    CreateRepositorySnapshot("alpha"),
                    TestAnalysisVersion,
                    Guid.Parse("04800000-0000-0000-0000-000000000001")),
                CreateUnavailable().Availability));
    }

    [Fact]
    public void RepositoryResult_RequiresAFailureReasonForTechnicalFailures()
    {
        AnalysisAdmissionResult analyze = new(
            CreateReference("alpha"),
            AnalysisAdmissionDecision.Analyze,
            reason: null,
            TestPolicyVersion);

        Assert.Throws<ArgumentException>(
            () => ProfileRepositoryAnalysisResult.CollectionFailed(analyze, " "));

        Assert.Throws<ArgumentException>(
            () => ProfileRepositoryAnalysisResult.CollectionUnavailable(
                analyze,
                CreateCollected(CreateRepositorySnapshot("alpha")).Availability));
    }

    private static ProfileRepositoryAnalysisResult Undetermined(
        ProfileRepositoryReference reference) =>
        ProfileRepositoryAnalysisResult.AdmissionUndetermined(
            new AnalysisAdmissionResult(
                reference,
                AnalysisAdmissionDecision.Undetermined,
                AnalysisAdmissionReason.RequiredInputUnavailable,
                TestPolicyVersion));
}

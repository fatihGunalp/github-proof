using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class AnalysisAdmissionPolicyTests
{
    private static readonly EligibilityPolicyVersion PolicyVersion =
        new("test-eligibility-policy");

    [Fact]
    public void Evaluate_WhenRepositoryIsEmpty_SkipsWithEmptyRepositoryReason()
    {
        AnalysisAdmissionResult result = CreatePolicy().Evaluate(
            CreateRepository(empty: RepositoryConditionState.Present));

        Assert.Equal(AnalysisAdmissionDecision.Skip, result.Decision);
        Assert.Equal(EligibilityReason.EmptyRepository, result.Reason);
        Assert.Equal(PolicyVersion, result.PolicyVersion);
    }

    [Fact]
    public void Evaluate_WhenRepositoryIsNotEmpty_AdmitsAnalysisWithoutReason()
    {
        AnalysisAdmissionResult result = CreatePolicy().Evaluate(
            CreateRepository(empty: RepositoryConditionState.Absent));

        Assert.Equal(AnalysisAdmissionDecision.Analyze, result.Decision);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void Evaluate_WhenEmptyStateIsUndetermined_DoesNotFallBackToAnalyzeOrSkip()
    {
        AnalysisAdmissionResult result = CreatePolicy().Evaluate(
            CreateRepository(empty: RepositoryConditionState.Undetermined));

        Assert.Equal(AnalysisAdmissionDecision.Undetermined, result.Decision);
        Assert.Equal(
            EligibilityReason.RequiredInputUnavailable,
            result.Reason);
    }

    [Theory]
    [InlineData(RepositoryConditionState.Present, RepositoryConditionState.Absent, RepositoryConditionState.Absent)]
    [InlineData(RepositoryConditionState.Absent, RepositoryConditionState.Present, RepositoryConditionState.Absent)]
    [InlineData(RepositoryConditionState.Absent, RepositoryConditionState.Absent, RepositoryConditionState.Present)]
    [InlineData(RepositoryConditionState.Undetermined, RepositoryConditionState.Undetermined, RepositoryConditionState.Undetermined)]
    public void Evaluate_ProvenanceAndLifecycleConditions_DoNotRestrictAdmission(
        RepositoryConditionState fork,
        RepositoryConditionState templateDerived,
        RepositoryConditionState archived)
    {
        AnalysisAdmissionResult result = CreatePolicy().Evaluate(
            CreateRepository(
                empty: RepositoryConditionState.Absent,
                fork,
                templateDerived,
                archived));

        Assert.Equal(AnalysisAdmissionDecision.Analyze, result.Decision);
    }

    [Fact]
    public void Evaluate_KeepsResultTraceableToTheEvaluatedRepository()
    {
        ProfileRepository repository = CreateRepository(
            empty: RepositoryConditionState.Absent);

        AnalysisAdmissionResult result = CreatePolicy().Evaluate(repository);

        Assert.Equal(repository.Reference, result.Reference);
    }

    [Fact]
    public void Evaluate_WhenRepositoryIsNull_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(
            () => CreatePolicy().Evaluate(null!));
    }

    [Fact]
    public void Constructor_WhenPolicyVersionIsNull_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AnalysisAdmissionPolicy(null!));
    }

    private static AnalysisAdmissionPolicy CreatePolicy() => new(PolicyVersion);

    private static ProfileRepository CreateRepository(
        RepositoryConditionState empty,
        RepositoryConditionState fork = RepositoryConditionState.Absent,
        RepositoryConditionState templateDerived = RepositoryConditionState.Absent,
        RepositoryConditionState archived = RepositoryConditionState.Absent) =>
        new(
            new ProfileRepositoryReference(
                "octocat",
                "hello-world",
                new Uri("https://github.com/octocat/hello-world")),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork,
            templateDerived,
            archived,
            empty,
            lastPushedAt: null,
            createdAt: null);
}

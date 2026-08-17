using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Tests.Profile;

internal static class ProfileContributionTestSupport
{
    public static readonly EligibilityPolicyVersion PolicyVersion =
        new("test-eligibility-policy");

    public static readonly ObservationRuleVersion RuleVersion =
        new("test-observation-rules");

    public static readonly AnalysisVersion TestAnalysisVersion =
        new("test-analyzer", "test-rules", 1);

    public static ProfileRepositoryReference CreateReference(
        string name,
        string owner = "octocat") =>
        new(owner, name, new Uri($"https://github.com/{owner}/{name}"));

    public static ProfileRepository CreateProfileRepository(
        string name,
        RepositoryConditionState fork = RepositoryConditionState.Absent,
        RepositoryConditionState templateDerived = RepositoryConditionState.Absent,
        RepositoryConditionState empty = RepositoryConditionState.Absent,
        string owner = "octocat") =>
        new(
            CreateReference(name, owner),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork,
            templateDerived,
            archived: RepositoryConditionState.Absent,
            empty,
            lastPushedAt: null,
            createdAt: null);

    public static AnalysisAdmissionResult CreateAdmission(
        ProfileRepository repository,
        AnalysisAdmissionDecision decision,
        EligibilityReason? reason = null) =>
        new(
            repository.Reference,
            decision,
            reason ?? DefaultReason(decision),
            PolicyVersion);

    public static RepositoryAnalysisReference CreateAnalysisReference(
        string name,
        string owner = "octocat") =>
        new(
            Guid.Parse("04800000-0000-0000-0000-000000000001"),
            new RepositoryIdentity(
                owner,
                name,
                new Uri($"https://github.com/{owner}/{name}"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            TestAnalysisVersion);

    private static EligibilityReason? DefaultReason(
        AnalysisAdmissionDecision decision) =>
        decision switch
        {
            AnalysisAdmissionDecision.Analyze => null,
            AnalysisAdmissionDecision.Skip => EligibilityReason.EmptyRepository,
            _ => EligibilityReason.RequiredInputUnavailable
        };
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
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

    public static readonly PatternRuleVersion TestPatternRuleVersion =
        new("test-pattern-rules");

    public static readonly RepresentativeRepositoryRuleVersion
        TestRepresentativeRepositoryRuleVersion =
        new("test-representative-repository-rules");

    // Analyzed, evidence-bearing contribution. Lineage decides whether it can
    // also count as independent repetition.
    public static RepositoryContributionAssessment CreateContribution(
        string name,
        RepositoryConditionState fork = RepositoryConditionState.Absent,
        RepositoryConditionState templateDerived = RepositoryConditionState.Absent)
    {
        ProfileRepository repository = CreateProfileRepository(
            name,
            fork,
            templateDerived);

        return new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference(name));
    }

    public static RepositoryContributionAssessment CreateRestrictedContribution(
        string name)
    {
        ProfileRepository repository = CreateProfileRepository(
            name,
            empty: RepositoryConditionState.Present);

        return new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Skip),
            analysisStatus: null,
            analysisReference: null);
    }

    public static RepositoryContributionAssessment CreateUnresolvedContribution(
        string name)
    {
        ProfileRepository repository = CreateProfileRepository(name);

        return new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Failed,
            analysisReference: null);
    }

    // Mirrors RepresentativeRepositorySelectionPolicy's own candidate
    // evaluation, so tests can supply RepresentativePatternSupport's scoped
    // evidence explicitly instead of relying on the constructor to derive it.
    public static IReadOnlyList<ProfileEvidenceReference> ScopedEvidenceFor(
        RepeatedProfilePattern pattern,
        ProfileRepositoryReference repository) =>
        pattern.SupportingObservations
            .SelectMany(observation => observation.EvidenceReferences)
            .Where(reference => reference.Repository == repository)
            .ToArray();

    public static ProfileEvidenceReference CreateEvidenceReference(
        RepositoryContributionAssessment contribution,
        string evidenceKey) =>
        new(
            contribution.Reference,
            contribution.AnalysisReference
                ?? CreateAnalysisReference(contribution.Reference.Name),
            evidenceKey,
            ["evidence-key"]);

    public static ObservedProfileEvidence CreateObservation(
        string observationKey,
        IReadOnlyList<RepositoryContributionAssessment> contributions,
        int eligibleRepositoryCount,
        ObservedProfileEvidenceKind kind =
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
        int undeterminedEvidenceRepositoryCount = 0) =>
        new(
            observationKey,
            kind,
            contributions.Select(contribution =>
                CreateEvidenceReference(contribution, observationKey)),
            contributions.Count(contribution =>
                contribution.IndependentRepetition.IsAllowed),
            eligibleRepositoryCount,
            undeterminedEvidenceRepositoryCount,
            RuleVersion);

    public static ObservedProfileEvidence CreateLanguageObservation(
        string language,
        IReadOnlyList<RepositoryContributionAssessment> contributions,
        int eligibleRepositoryCount) =>
        CreateObservation(
            ProfileEvidenceKeys.CreateLanguageObservationKey(language),
            contributions,
            eligibleRepositoryCount,
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence);

    public static CrossRepositoryEvidence CreateEvidence(
        IReadOnlyList<RepositoryContributionAssessment> contributions,
        IEnumerable<ObservedProfileEvidence> observations,
        bool isEvidenceSurfaceComplete = true) =>
        new(
            contributions,
            observations,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete);

    private static EligibilityReason? DefaultReason(
        AnalysisAdmissionDecision decision) =>
        decision switch
        {
            AnalysisAdmissionDecision.Analyze => null,
            AnalysisAdmissionDecision.Skip => EligibilityReason.EmptyRepository,
            _ => EligibilityReason.RequiredInputUnavailable
        };
}

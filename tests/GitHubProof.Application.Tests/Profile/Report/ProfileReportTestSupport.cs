using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Tests.Profile.Report;

// Builds DeveloperPublicEvidenceProfile fixtures directly against the Domain
// constructors (mirroring GitHubProof.Domain.Tests.Profile.
// DeveloperPublicEvidenceProfileTests), so ProfileReportProjector tests can
// control pattern family/content/ordering precisely without depending on
// ProfileComposer's coherence rules or the real signal-evaluation pipeline.
internal static class ProfileReportTestSupport
{
    public static readonly EligibilityPolicyVersion PolicyVersion =
        new("test-eligibility-policy");

    public static readonly ObservationRuleVersion ObservationRuleVersion =
        new("test-observation-rules");

    public static readonly PatternRuleVersion PatternRuleVersion =
        new("test-pattern-rules");

    public static readonly RepresentativeRepositoryRuleVersion RepresentativeRuleVersion =
        new("test-representative-repository-rules");

    public static readonly AnalysisVersion TestAnalysisVersion =
        new("test-analyzer", "test-rules", 1);

    public static readonly DateTimeOffset StartedAt =
        new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    public static readonly DateTimeOffset CompletedAt = StartedAt.AddMinutes(2);

    private static readonly EligibilityPolicy Policy =
        EligibilityPolicy.Create(PolicyVersion, TimeSpan.FromDays(365));

    public static ProfileRepositoryReference CreateReference(
        string name,
        string owner = "octocat") =>
        new(owner, name, new Uri($"https://github.com/{owner}/{name}"));

    public static RepositoryAnalysisReference CreateAnalysisReference(
        string name,
        string owner = "octocat") =>
        new(
            Guid.NewGuid(),
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

    public static RepositoryEligibilityResult Eligibility(
        string name,
        EligibilityPolicy? policy = null)
    {
        EligibilityPolicy resolved = policy ?? Policy;

        ProfileRepository repository = new(
            CreateReference(name),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork: RepositoryConditionState.Absent,
            templateDerived: RepositoryConditionState.Absent,
            archived: RepositoryConditionState.Absent,
            empty: RepositoryConditionState.Absent,
            lastPushedAt: StartedAt.AddDays(-10),
            createdAt: StartedAt.AddYears(-2));

        return new RepositoryEligibilityPolicy(resolved).Evaluate(
            repository,
            new AnalysisAdmissionResult(
                repository.Reference,
                AnalysisAdmissionDecision.Analyze,
                reason: null,
                resolved.Version),
            AnalysisStatus.Completed,
            CreateAnalysisReference(name),
            StartedAt);
    }

    public static ObservedProfileEvidence CreateObservation(
        string observationKey,
        IReadOnlyList<string> repositoryNames,
        ObservedProfileEvidenceKind kind = ObservedProfileEvidenceKind.RepositorySignalEvidence) =>
        new(
            observationKey,
            kind,
            repositoryNames.Select(name => new ProfileEvidenceReference(
                CreateReference(name),
                CreateAnalysisReference(name),
                observationKey,
                [observationKey])),
            independentSupportingRepositoryCount: repositoryNames.Count,
            eligibleRepositoryCount: repositoryNames.Count,
            undeterminedEvidenceRepositoryCount: 0,
            ObservationRuleVersion);

    public static RepeatedProfilePattern CreatePattern(
        ProfilePatternFamily family,
        string patternKey,
        IReadOnlyList<string> independentRepositoryNames,
        ObservedProfileEvidenceKind kind = ObservedProfileEvidenceKind.RepositorySignalEvidence) =>
        new(
            family,
            patternKey,
            [CreateObservation($"{patternKey}-observation", independentRepositoryNames, kind)],
            independentRepositoryNames.Select(name => CreateReference(name)),
            PatternRuleVersion);

    public static ProfilePatternSet CreatePatternSet(
        IEnumerable<RepeatedProfilePattern> patterns,
        bool isEvidenceSurfaceComplete = true) =>
        new(patterns, PatternRuleVersion, ObservationRuleVersion, PolicyVersion, isEvidenceSurfaceComplete);

    public static RepresentativeRepositorySelection CreateSelection(
        ProfilePatternSet patternSet) =>
        new RepresentativeRepositorySelectionPolicy(RepresentativeRuleVersion).Select(patternSet);

    public static DeveloperPublicEvidenceProfile Build(
        ProfileAnalysisStatus status = ProfileAnalysisStatus.Complete,
        IReadOnlyList<RepositoryEligibilityResult>? eligibility = null,
        ProfilePatternSet? patternSet = null,
        RepresentativeRepositorySelection? selection = null,
        IReadOnlyList<ProfileLimitation>? limitations = null,
        IReadOnlyList<ProfileWarning>? warnings = null)
    {
        ProfilePatternSet resolvedPatternSet = patternSet ?? CreatePatternSet([]);

        return new DeveloperPublicEvidenceProfile(
            new SnapshotReference(
                new GitHubProfileIdentity(
                    "octocat",
                    new Uri("https://github.com/octocat")),
                StartedAt,
                Snapshot().CreateReference().SnapshotIdentity),
            status,
            ProfileAnalyzerVersion.V1,
            eligibility ?? [Eligibility("alpha")],
            new CrossRepositoryEvidence(
                [],
                [],
                ObservationRuleVersion,
                PolicyVersion,
                isEvidenceSurfaceComplete: true),
            resolvedPatternSet,
            selection ?? CreateSelection(resolvedPatternSet),
            limitations,
            warnings,
            StartedAt,
            CompletedAt);
    }

    private static GitHubProfileSnapshot Snapshot() =>
        new(
            new GitHubProfileIdentity("octocat", new Uri("https://github.com/octocat")),
            StartedAt,
            metadata: null,
            new ProfileRepositoryManifest(),
            availability: null,
            collectionWarnings: null);
}

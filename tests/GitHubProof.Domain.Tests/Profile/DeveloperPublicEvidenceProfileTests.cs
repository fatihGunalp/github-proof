using System.Reflection;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class DeveloperPublicEvidenceProfileTests
{
    private static readonly DateTimeOffset StartedAt =
        new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset CompletedAt = StartedAt.AddMinutes(2);

    // ------------------------------------------------------------ valid root

    [Fact]
    public void CompleteProfile_IsValid()
    {
        DeveloperPublicEvidenceProfile profile = Build();

        Assert.Equal(ProfileAnalysisStatus.Complete, profile.AnalysisStatus);
        Assert.Equal(ProfileAnalyzerVersion.V1, profile.AnalyzerVersion);
        Assert.NotNull(profile.SnapshotReference);
        Assert.Equal(
            profile.SnapshotReference.ProfileIdentity,
            profile.ProfileIdentity);
    }

    [Fact]
    public void PartialProfile_IsValid()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            status: ProfileAnalysisStatus.Partial);

        Assert.Equal(ProfileAnalysisStatus.Partial, profile.AnalysisStatus);
    }

    // Failed is not a value a valid root can carry at all.
    [Fact]
    public void FailedProfileStatus_IsImpossible()
    {
        Assert.Equal(
            [ProfileAnalysisStatus.Complete, ProfileAnalysisStatus.Partial],
            Enum.GetValues<ProfileAnalysisStatus>());

        Assert.DoesNotContain(
            "Failed",
            Enum.GetNames<ProfileAnalysisStatus>(),
            StringComparer.Ordinal);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Build(status: (ProfileAnalysisStatus)3));
    }

    [Fact]
    public void EmptyRoleEvidenceAlignments_IsValid()
    {
        DeveloperPublicEvidenceProfile profile = Build();

        Assert.Empty(profile.RoleEvidenceAlignments);
    }

    // The deferral is enforced at compile time: the placeholder cannot be
    // constructed, so the collection can only ever be empty.
    [Fact]
    public void RoleEvidenceAlignment_CannotBeConstructed()
    {
        Assert.Empty(typeof(RoleEvidenceAlignment).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));

        Assert.Empty(typeof(RoleEvidenceAlignment).GetProperties(
            BindingFlags.Public | BindingFlags.Instance));
    }

    [Fact]
    public void EmptyRepeatedPatterns_AreValidAndCarryNoNegativeMeaning()
    {
        DeveloperPublicEvidenceProfile profile = Build();

        Assert.Empty(profile.RepeatedProfilePatterns);
        Assert.Empty(profile.RepresentativeRepositories);
        Assert.Equal(ProfileAnalysisStatus.Complete, profile.AnalysisStatus);
    }

    // RepeatedProfilePatterns is the one canonical pattern store; the family
    // views are projections, never parallel stores on the root.
    [Fact]
    public void Root_ExposesNoParallelPatternFamilyStores()
    {
        string[] names = typeof(DeveloperPublicEvidenceProfile)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("TechnicalFocusAreas", names, StringComparer.Ordinal);
        Assert.DoesNotContain(
            "EngineeringPracticePatterns",
            names,
            StringComparer.Ordinal);
        Assert.DoesNotContain("ProblemAreaPatterns", names, StringComparer.Ordinal);
        Assert.Contains("RepeatedProfilePatterns", names, StringComparer.Ordinal);
    }

    [Fact]
    public void RepositoryAnalysisReferences_ComeFromTheEligibilitySurface()
    {
        DeveloperPublicEvidenceProfile profile = Build();

        Assert.Equal(
            profile.RepositoryEligibilityResults
                .Select(result => result.AnalysisReference)
                .OfType<RepositoryAnalysisReference>(),
            profile.RepositoryAnalysisReferences);
    }

    // ----------------------------------------------------------- timestamps

    [Fact]
    public void CompletedBeforeStarted_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Build(
            startedAt: CompletedAt,
            completedAt: StartedAt));
    }

    [Fact]
    public void EqualStartAndCompletion_IsAccepted()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            startedAt: StartedAt,
            completedAt: StartedAt);

        Assert.Equal(profile.StartedAt, profile.CompletedAt);
    }

    // Audit timestamps are not profile truth: they change nothing semantic.
    [Fact]
    public void AuditTimestamps_DoNotChangeSemanticContent()
    {
        DeveloperPublicEvidenceProfile early = Build();
        DeveloperPublicEvidenceProfile late = Build(
            startedAt: StartedAt.AddDays(30),
            completedAt: CompletedAt.AddDays(30));

        Assert.NotEqual(early.StartedAt, late.StartedAt);
        Assert.Equal(
            early.SnapshotReference.SnapshotIdentity,
            late.SnapshotReference.SnapshotIdentity);
        Assert.Equal(early.AnalysisStatus, late.AnalysisStatus);
        Assert.Equal(
            early.RepositoryEligibilityResults.Select(result => result.Reference),
            late.RepositoryEligibilityResults.Select(result => result.Reference));
    }

    // ------------------------------------------------------------ coherence

    [Fact]
    public void EligibilityResultFromAnotherPolicyVersion_IsRejected()
    {
        EligibilityPolicy other = EligibilityPolicy.Create(
            new EligibilityPolicyVersion("other-policy"),
            TimeSpan.FromDays(365));

        Assert.Throws<ArgumentException>(() => Build(
            eligibility: [Eligibility("alpha", other)]));
    }

    [Fact]
    public void PatternSetFromAnotherObservationRuleVersion_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Build(
            patternSet: new ProfilePatternSet(
                [],
                TestPatternRuleVersion,
                new ObservationRuleVersion("other-observation-rules"),
                PolicyVersion,
                isEvidenceSurfaceComplete: true)));
    }

    [Fact]
    public void SelectionFromAnotherPatternRuleVersion_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Build(
            selection: new RepresentativeRepositorySelection(
                [],
                new PatternRuleVersion("other-pattern-rules"),
                RuleVersion,
                PolicyVersion,
                isEvidenceSurfaceComplete: true,
                TestRepresentativeRepositoryRuleVersion)));
    }

    [Fact]
    public void DuplicateEligibilityResult_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Build(
            eligibility: [Eligibility("alpha"), Eligibility("alpha")]));
    }

    // ---------------------------------------------------------- normalization

    [Fact]
    public void EligibilityResults_AreOrderedByOwnerThenName()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            eligibility:
            [
                Eligibility("gamma"),
                Eligibility("alpha"),
                Eligibility("beta")
            ]);

        Assert.Equal(
            ["alpha", "beta", "gamma"],
            profile.RepositoryEligibilityResults.Select(
                result => result.Reference.Name));
    }

    [Fact]
    public void Conditions_AreDeduplicatedAndOrderedCanonically()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            warnings:
            [
                Warning(ProfileWarningCode.RepositoryAnalysisFailed, "beta"),
                Warning(ProfileWarningCode.RepositoryEvidenceUnavailable, "beta"),
                Warning(ProfileWarningCode.RepositoryAnalysisFailed, "beta"),
                Warning(ProfileWarningCode.RepositoryEvidenceUnavailable, "alpha")
            ],
            limitations:
            [
                new ProfileLimitation(
                    ProfileLimitationCode.PartialEvidenceSurface,
                    ProfileConditionScope.Profile),
                new ProfileLimitation(
                    ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                    ProfileConditionScope.Profile),
                new ProfileLimitation(
                    ProfileLimitationCode.PartialEvidenceSurface,
                    ProfileConditionScope.Profile)
            ]);

        // Code first, then scope, then affected identities.
        Assert.Equal(
            [
                (ProfileWarningCode.RepositoryEvidenceUnavailable, "alpha"),
                (ProfileWarningCode.RepositoryEvidenceUnavailable, "beta"),
                (ProfileWarningCode.RepositoryAnalysisFailed, "beta")
            ],
            profile.Warnings.Select(warning =>
                (warning.Code, warning.AffectedRepositories[0].Name)));

        Assert.Equal(
            [
                ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                ProfileLimitationCode.PartialEvidenceSurface
            ],
            profile.Limitations.Select(limitation => limitation.Code));
    }

    [Fact]
    public void Conditions_AreOrderedIndependentlyOfInputOrder()
    {
        ProfileWarning[] forward =
        [
            Warning(ProfileWarningCode.RepositoryEvidenceUnavailable, "alpha"),
            Warning(ProfileWarningCode.RepositoryAnalysisFailed, "beta")
        ];

        DeveloperPublicEvidenceProfile first = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            warnings: forward);

        DeveloperPublicEvidenceProfile second = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            warnings: [.. forward.Reverse()]);

        Assert.Equal(
            first.Warnings.Select(warning => warning.Code),
            second.Warnings.Select(warning => warning.Code));
    }

    [Fact]
    public void ConditionOutsideTheEvidenceSurface_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Build(
            eligibility: [Eligibility("alpha")],
            warnings:
            [
                Warning(ProfileWarningCode.RepositoryAnalysisFailed, "outside")
            ]));
    }

    [Fact]
    public void NullCollections_AreTreatedAsEmpty()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            limitations: null,
            warnings: null);

        Assert.Empty(profile.Limitations);
        Assert.Empty(profile.Warnings);
    }

    // -------------------------------------------------------- contract guard

    [Theory]
    [InlineData("Score")]
    [InlineData("Rank")]
    [InlineData("Quality")]
    [InlineData("Confidence")]
    [InlineData("Capability")]
    [InlineData("Seniority")]
    [InlineData("Employab")]
    [InlineData("JobFit")]
    [InlineData("Hire")]
    [InlineData("Reject")]
    [InlineData("Role")]
    [InlineData("Label")]
    [InlineData("Summary")]
    [InlineData("Narrat")]
    [InlineData("Display")]
    [InlineData("Prose")]
    [InlineData("Ui")]
    public void Root_DoesNotExposeProhibitedSemantics(string prohibited)
    {
        IEnumerable<string> names = typeof(DeveloperPublicEvidenceProfile)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            // RoleEvidenceAlignments is the contracted, deferred, always-empty
            // capability rather than an inferred role.
            .Where(name => name != nameof(
                DeveloperPublicEvidenceProfile.RoleEvidenceAlignments));

        Assert.DoesNotContain(
            names,
            name => name.Contains(prohibited, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Root_ExposesNoRawStringOrNumericSurface()
    {
        Assert.All(
            typeof(DeveloperPublicEvidenceProfile).GetProperties(
                BindingFlags.Public | BindingFlags.Instance),
            property => Assert.False(
                property.PropertyType == typeof(string)
                || property.PropertyType == typeof(int)
                || property.PropertyType == typeof(double)
                || property.PropertyType == typeof(decimal),
                $"{property.Name} exposes a bare score-shaped value."));
    }

    // -------------------------------------------------------------- fixtures

    private static readonly EligibilityPolicy Policy = EligibilityPolicy.Create(
        PolicyVersion,
        TimeSpan.FromDays(365));

    private static RepositoryEligibilityResult Eligibility(
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

    private static ProfileWarning Warning(ProfileWarningCode code, string repository) =>
        new(code, ProfileConditionScope.Repository, [CreateReference(repository)]);

    private static DeveloperPublicEvidenceProfile Build(
        ProfileAnalysisStatus status = ProfileAnalysisStatus.Complete,
        IReadOnlyList<RepositoryEligibilityResult>? eligibility = null,
        ProfilePatternSet? patternSet = null,
        RepresentativeRepositorySelection? selection = null,
        IReadOnlyList<ProfileLimitation>? limitations = null,
        IReadOnlyList<ProfileWarning>? warnings = null,
        DateTimeOffset? startedAt = null,
        DateTimeOffset? completedAt = null)
    {
        IReadOnlyList<RepositoryEligibilityResult> results =
            eligibility ?? [Eligibility("alpha")];

        return new DeveloperPublicEvidenceProfile(
            new SnapshotReference(
                new GitHubProfileIdentity(
                    "octocat",
                    new Uri("https://github.com/octocat")),
                StartedAt,
                Snapshot().CreateReference().SnapshotIdentity),
            status,
            ProfileAnalyzerVersion.V1,
            results,
            CreateEvidence([], []),
            patternSet ?? new ProfilePatternSet(
                [],
                TestPatternRuleVersion,
                RuleVersion,
                PolicyVersion,
                isEvidenceSurfaceComplete: true),
            selection ?? new RepresentativeRepositorySelection(
                [],
                TestPatternRuleVersion,
                RuleVersion,
                PolicyVersion,
                isEvidenceSurfaceComplete: true,
                TestRepresentativeRepositoryRuleVersion),
            limitations,
            warnings,
            startedAt ?? StartedAt,
            completedAt ?? CompletedAt);
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

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepositoryEligibilityPolicyTests
{
    private static readonly DateTimeOffset CapturedAt =
        new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    private static readonly EligibilityPolicy Policy = EligibilityPolicy.Create(
        PolicyVersion,
        TimeSpan.FromDays(365));

    private static DateTimeOffset Cutoff => CapturedAt.AddDays(-365);

    // ---------------------------------------------------------------- window

    [Fact]
    public void Activity_JustAboveCutoff_IsActive()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("above", lastPushedAt: Cutoff.AddTicks(1)));

        Assert.Equal(ActivityClassification.Active, result.Activity);
    }

    // The cutoff itself is inside the window: RepositoryActivityTimestamp >=
    // ActivityCutoff is Active.
    [Fact]
    public void Activity_ExactlyAtCutoff_IsActive()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("exact", lastPushedAt: Cutoff));

        Assert.Equal(ActivityClassification.Active, result.Activity);
    }

    [Fact]
    public void Activity_JustBelowCutoff_IsHistorical()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("below", lastPushedAt: Cutoff.AddTicks(-1)));

        Assert.Equal(ActivityClassification.Historical, result.Activity);
    }

    [Fact]
    public void Activity_MissingSourceControlTimestamp_IsUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("unknown", lastPushedAt: null));

        Assert.Equal(ActivityClassification.Undetermined, result.Activity);
    }

    // Repository age is irrelevant: a repository created days before the
    // snapshot with in-window activity is Active, and a decade-old repository
    // whose last push precedes the cutoff is Historical.
    [Fact]
    public void Activity_IgnoresRepositoryAge()
    {
        RepositoryEligibilityResult young = Evaluate(Repository(
            "young",
            lastPushedAt: CapturedAt.AddDays(-3),
            createdAt: CapturedAt.AddDays(-14)));

        RepositoryEligibilityResult old = Evaluate(Repository(
            "old",
            lastPushedAt: CapturedAt.AddYears(-4),
            createdAt: CapturedAt.AddYears(-10)));

        Assert.Equal(ActivityClassification.Active, young.Activity);
        Assert.Equal(ActivityClassification.Historical, old.Activity);
    }

    // A shorter window on a different policy version reclassifies the same
    // repository, so behaviour is owned by the policy rather than a constant.
    [Fact]
    public void Activity_FollowsTheWindowOwnedByThePolicyVersion()
    {
        ProfileRepository repository = Repository(
            "windowed",
            lastPushedAt: CapturedAt.AddDays(-200));

        RepositoryEligibilityResult wide = Evaluate(repository);
        RepositoryEligibilityResult narrow = Evaluate(
            repository,
            policy: EligibilityPolicy.Create(
                new EligibilityPolicyVersion("short-window"),
                TimeSpan.FromDays(90)));

        Assert.Equal(ActivityClassification.Active, wide.Activity);
        Assert.Equal(ActivityClassification.Historical, narrow.Activity);
    }

    [Fact]
    public void Activity_UsesNoSystemClock()
    {
        ProfileRepository repository = Repository(
            "clock",
            lastPushedAt: new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero));

        RepositoryEligibilityResult result = Evaluate(
            repository,
            capturedAt: new DateTimeOffset(2001, 3, 1, 0, 0, 0, TimeSpan.Zero));

        // Long past relative to now, but inside the window of its own snapshot.
        Assert.Equal(ActivityClassification.Active, result.Activity);
    }

    // ------------------------------------------------------------- lifecycle

    [Fact]
    public void CurrentFocus_Archived_IsNotAllowed()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "archived",
            lastPushedAt: CapturedAt.AddDays(-1),
            archived: RepositoryConditionState.Present));

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.Archived);
    }

    // Archive state alone is sufficient even when activity cannot be classified.
    [Fact]
    public void CurrentFocus_ArchivedWithUndeterminedActivity_IsNotAllowed()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "archived-unknown-activity",
            lastPushedAt: null,
            archived: RepositoryConditionState.Present));

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.Archived);
        Assert.Equal(ActivityClassification.Undetermined, result.Activity);
    }

    [Fact]
    public void CurrentFocus_Historical_IsNotAllowed()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "historical",
            lastPushedAt: Cutoff.AddDays(-1)));

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.Historical);
    }

    [Fact]
    public void CurrentFocus_ActiveAndNotArchived_IsAllowed()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "active",
            lastPushedAt: CapturedAt.AddDays(-1)));

        Assert.Equal(ContributionDecision.Allowed, result.CurrentFocus.Decision);
    }

    [Fact]
    public void CurrentFocus_ActivityUndetermined_IsUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "activity-unknown",
            lastPushedAt: null));

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.Undetermined,
            EligibilityReason.RequiredInputUnavailable);
    }

    // No optimistic Active fallback when archive state itself is unknown.
    [Fact]
    public void CurrentFocus_ArchiveStateUndetermined_IsUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "archive-unknown",
            lastPushedAt: CapturedAt.AddDays(-1),
            archived: RepositoryConditionState.Undetermined));

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.Undetermined,
            EligibilityReason.RequiredInputUnavailable);
    }

    // Active and Archived are independent dimensions; the combination is valid
    // and archive state still restricts current focus.
    [Fact]
    public void ActiveAndArchived_IsAValidCombination()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "active-archived",
            lastPushedAt: CapturedAt.AddDays(-1),
            archived: RepositoryConditionState.Present));

        Assert.Equal(ActivityClassification.Active, result.Activity);
        Assert.Equal(ContributionDecision.NotAllowed, result.CurrentFocus.Decision);
    }

    // --------------------------------------------------- historical evidence

    // Historical evidence is evidence-surface based and is not mutually
    // exclusive with current focus.
    [Fact]
    public void HistoricalEvidence_IsAllowedForAnActiveRepository()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "active-history",
            lastPushedAt: CapturedAt.AddDays(-1)));

        Assert.Equal(ContributionDecision.Allowed, result.CurrentFocus.Decision);
        Assert.Equal(ContributionDecision.Allowed, result.HistoricalEvidence.Decision);
    }

    [Fact]
    public void HistoricalEvidence_SurvivesArchivedAndHistoricalLifecycle()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "archived-historical",
            lastPushedAt: Cutoff.AddDays(-30),
            archived: RepositoryConditionState.Present));

        Assert.Equal(ContributionDecision.NotAllowed, result.CurrentFocus.Decision);
        Assert.Equal(ContributionDecision.Allowed, result.HistoricalEvidence.Decision);
    }

    // A technical failure is not evidence absence, so archive state does not
    // make historical evidence available.
    [Fact]
    public void HistoricalEvidence_ArchivedWithFailedAnalysis_StaysUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository(
                "archived-failed",
                lastPushedAt: Cutoff.AddDays(-30),
                archived: RepositoryConditionState.Present),
            analysisStatus: AnalysisStatus.Failed,
            analysisReference: null);

        Assert.Equal(
            ContributionDecision.Undetermined,
            result.HistoricalEvidence.Decision);
        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.Archived);
    }

    // ---------------------------------------------------- baseline behaviour

    [Fact]
    public void ProfileContext_RemainsAllowedForEveryBaseline()
    {
        Assert.All(
            new[]
            {
                Evaluate(Repository("completed", lastPushedAt: CapturedAt)),
                EvaluateEmpty("empty"),
                Evaluate(
                    Repository("insufficient", lastPushedAt: CapturedAt),
                    analysisStatus: AnalysisStatus.InsufficientData,
                    analysisReference: null),
                Evaluate(
                    Repository("failed", lastPushedAt: CapturedAt),
                    analysisStatus: AnalysisStatus.Failed,
                    analysisReference: null),
                EvaluateUnadmitted("unadmitted")
            },
            result => Assert.Equal(
                ContributionDecision.Allowed,
                result.ProfileContext.Decision));
    }

    [Fact]
    public void EmptyRepository_RestrictsEveryEvidenceBearingPurpose()
    {
        RepositoryEligibilityResult result = EvaluateEmpty("empty");

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.EmptyRepository);
        AssertDecision(
            result.HistoricalEvidence,
            ContributionDecision.NotAllowed,
            EligibilityReason.EmptyRepository);
        Assert.Equal(RepositoryEligibilityStatus.NonContributing, result.Status);
    }

    [Fact]
    public void InsufficientData_RestrictsEveryEvidenceBearingPurpose()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("insufficient", lastPushedAt: CapturedAt),
            analysisStatus: AnalysisStatus.InsufficientData,
            analysisReference: null);

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.InsufficientEvidence);
        AssertDecision(
            result.HistoricalEvidence,
            ContributionDecision.NotAllowed,
            EligibilityReason.InsufficientEvidence);
        Assert.Equal(RepositoryEligibilityStatus.NonContributing, result.Status);
    }

    // Admission was never resolved, so no evidence-bearing purpose may be
    // decided permissively, but a known manifest restriction still resolves.
    [Fact]
    public void AdmissionUndetermined_ResolvesManifestRestrictionsFirst()
    {
        RepositoryEligibilityResult result = EvaluateUnadmitted(
            "unadmitted-archived",
            archived: RepositoryConditionState.Present);

        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.NotAllowed,
            EligibilityReason.Archived);
        Assert.Equal(
            ContributionDecision.Undetermined,
            result.HistoricalEvidence.Decision);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    // Partial analysis still establishes an evidence-available baseline.
    [Fact]
    public void PartialAnalysis_KeepsTheEvidenceSurfaceAvailable()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("partial", lastPushedAt: CapturedAt.AddDays(-1)),
            analysisStatus: AnalysisStatus.Partial,
            analysisReference: CreateAnalysisReference("partial"));

        Assert.Equal(ContributionDecision.Allowed, result.ProfileObservation.Decision);
        Assert.Equal(ContributionDecision.Allowed, result.CurrentFocus.Decision);
        Assert.Equal(ContributionDecision.Allowed, result.HistoricalEvidence.Decision);
        Assert.Equal(RepositoryEligibilityStatus.Contributing, result.Status);
    }

    // No manifest-only restriction applies, so the unresolved evidence surface
    // leaves current focus Undetermined rather than allowed.
    [Fact]
    public void FailedAnalysis_WithoutManifestRestriction_LeavesCurrentFocusUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("failed-active", lastPushedAt: CapturedAt.AddDays(-1)),
            analysisStatus: AnalysisStatus.Failed,
            analysisReference: null);

        Assert.Equal(ActivityClassification.Active, result.Activity);
        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.Undetermined,
            EligibilityReason.RequiredInputUnavailable);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    [Fact]
    public void AdmissionUndetermined_WithoutManifestRestriction_LeavesCurrentFocusUndetermined()
    {
        RepositoryEligibilityResult result = EvaluateUnadmitted("unadmitted-active");

        Assert.Equal(ActivityClassification.Active, result.Activity);
        AssertDecision(
            result.CurrentFocus,
            ContributionDecision.Undetermined,
            EligibilityReason.RequiredInputUnavailable);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    [Fact]
    public void FailedAnalysis_WithKnownForkLineage_StillRestrictsRepetition()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository(
                "failed-fork",
                lastPushedAt: CapturedAt,
                fork: RepositoryConditionState.Present),
            analysisStatus: AnalysisStatus.Failed,
            analysisReference: null);

        Assert.Equal(
            ContributionDecision.NotAllowed,
            result.IndependentRepetition.Decision);
        Assert.Equal(
            ContributionDecision.Undetermined,
            result.ProfileObservation.Decision);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    // ------------------------------------------------------- purpose surface

    [Fact]
    public void EveryCanonicalPurposeIsRepresented()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("purposes", lastPushedAt: CapturedAt));

        foreach (ContributionPurpose purpose in Enum.GetValues<ContributionPurpose>())
        {
            Assert.NotNull(result.For(purpose));
        }

        Assert.Same(result.ProfileContext, result.For(ContributionPurpose.ProfileContext));
        Assert.Same(result.ProfileObservation, result.For(ContributionPurpose.ProfileObservation));
        Assert.Same(result.IndependentRepetition, result.For(ContributionPurpose.IndependentRepetition));
        Assert.Same(result.CurrentFocus, result.For(ContributionPurpose.CurrentFocus));
        Assert.Same(result.HistoricalEvidence, result.For(ContributionPurpose.HistoricalEvidence));
    }

    [Fact]
    public void Result_PreservesIdentityAndPolicyProvenance()
    {
        ProfileRepository repository = Repository("provenance", lastPushedAt: CapturedAt);
        RepositoryEligibilityResult result = Evaluate(repository);

        Assert.Equal(repository.Reference, result.Reference);
        Assert.Equal(PolicyVersion, result.PolicyVersion);
        Assert.NotNull(result.Admission);
        Assert.NotNull(result.AnalysisReference);
    }

    // The two inputs that can independently leave current focus Undetermined
    // stay distinguishable on the result.
    [Fact]
    public void Result_PreservesTheConditionsBehindALifecycleDecision()
    {
        RepositoryEligibilityResult unknownActivity = Evaluate(Repository(
            "trace-activity",
            lastPushedAt: null));

        RepositoryEligibilityResult unknownArchive = Evaluate(Repository(
            "trace-archive",
            lastPushedAt: CapturedAt.AddDays(-1),
            archived: RepositoryConditionState.Undetermined));

        Assert.Equal(
            ContributionDecision.Undetermined,
            unknownActivity.CurrentFocus.Decision);
        Assert.Equal(RepositoryConditionState.Absent, unknownActivity.Archived);
        Assert.Equal(ActivityClassification.Undetermined, unknownActivity.Activity);

        Assert.Equal(
            ContributionDecision.Undetermined,
            unknownArchive.CurrentFocus.Decision);
        Assert.Equal(RepositoryConditionState.Undetermined, unknownArchive.Archived);
        Assert.Equal(ActivityClassification.Active, unknownArchive.Activity);
    }

    // A technical failure or unresolved admission can never be promoted into
    // available evidence through the result surface.
    [Fact]
    public void Result_RejectsAllowingAPurposeWithoutAnAllowedObservation()
    {
        RepositoryContributionAssessment unresolved =
            CreateUnresolvedContribution("guarded");

        Assert.Throws<ArgumentException>(() => new RepositoryEligibilityResult(
            unresolved,
            RepositoryConditionState.Absent,
            ActivityClassification.Active,
            ContributionAssessment.Allowed(),
            ContributionAssessment.Undetermined(
                EligibilityReason.RequiredInputUnavailable)));

        Assert.Throws<ArgumentException>(() => new RepositoryEligibilityResult(
            unresolved,
            RepositoryConditionState.Absent,
            ActivityClassification.Active,
            ContributionAssessment.Undetermined(
                EligibilityReason.RequiredInputUnavailable),
            ContributionAssessment.Allowed()));
    }

    // ------------------------------------------------------------ derivation

    [Fact]
    public void Status_AllAllowed_IsContributing()
    {
        RepositoryEligibilityResult result = Evaluate(
            Repository("contributing", lastPushedAt: CapturedAt));

        Assert.Equal(RepositoryEligibilityStatus.Contributing, result.Status);
    }

    // Fork prevents independent repetition while other purposes stay allowed.
    [Fact]
    public void Status_ForkWithEvidence_IsRestricted()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "fork",
            lastPushedAt: CapturedAt,
            fork: RepositoryConditionState.Present));

        Assert.Equal(
            ContributionDecision.NotAllowed,
            result.IndependentRepetition.Decision);
        Assert.Equal(RepositoryEligibilityStatus.Restricted, result.Status);
    }

    [Fact]
    public void Status_ArchivedWithEvidence_IsRestricted()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "archived-restricted",
            lastPushedAt: CapturedAt,
            archived: RepositoryConditionState.Present));

        Assert.Equal(RepositoryEligibilityStatus.Restricted, result.Status);
    }

    // Undetermined has the highest precedence and is never hidden by an
    // allowed purpose.
    [Fact]
    public void Status_AllowedPlusUndetermined_IsUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "mixed-undetermined",
            lastPushedAt: null));

        Assert.Equal(ContributionDecision.Allowed, result.ProfileObservation.Decision);
        Assert.Equal(ContributionDecision.Undetermined, result.CurrentFocus.Decision);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    [Fact]
    public void Status_AllowedPlusNotAllowedPlusUndetermined_IsUndetermined()
    {
        RepositoryEligibilityResult result = Evaluate(Repository(
            "three-way",
            lastPushedAt: null,
            fork: RepositoryConditionState.Present));

        Assert.Equal(ContributionDecision.Allowed, result.ProfileObservation.Decision);
        Assert.Equal(ContributionDecision.NotAllowed, result.IndependentRepetition.Decision);
        Assert.Equal(ContributionDecision.Undetermined, result.CurrentFocus.Decision);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, result.Status);
    }

    // The derived status can never be set independently of its purposes.
    [Fact]
    public void Status_IsDerivedFromPurposeDecisionsAlone()
    {
        RepositoryContributionAssessment contribution = CreateContribution("derived");

        RepositoryEligibilityResult contributing = new(
            contribution,
            RepositoryConditionState.Absent,
            ActivityClassification.Active,
            ContributionAssessment.Allowed(),
            ContributionAssessment.Allowed());

        RepositoryEligibilityResult restricted = new(
            contribution,
            RepositoryConditionState.Absent,
            ActivityClassification.Historical,
            ContributionAssessment.NotAllowed(EligibilityReason.Historical),
            ContributionAssessment.Allowed());

        RepositoryEligibilityResult undetermined = new(
            contribution,
            RepositoryConditionState.Absent,
            ActivityClassification.Undetermined,
            ContributionAssessment.Undetermined(
                EligibilityReason.RequiredInputUnavailable),
            ContributionAssessment.Allowed());

        Assert.Equal(RepositoryEligibilityStatus.Contributing, contributing.Status);
        Assert.Equal(RepositoryEligibilityStatus.Restricted, restricted.Status);
        Assert.Equal(RepositoryEligibilityStatus.Undetermined, undetermined.Status);
    }

    // ProfileContext is context-only: it must never promote a repository into
    // profile-level evidence.
    [Fact]
    public void Status_ProfileContextAlone_DoesNotProduceContributing()
    {
        RepositoryEligibilityResult result = EvaluateEmpty("context-only");

        Assert.Equal(ContributionDecision.Allowed, result.ProfileContext.Decision);
        Assert.Equal(RepositoryEligibilityStatus.NonContributing, result.Status);
    }

    // ----------------------------------------------------------- determinism

    [Fact]
    public void Evaluate_IsDeterministicForTheSameInputs()
    {
        ProfileRepository repository = Repository(
            "deterministic",
            lastPushedAt: Cutoff.AddDays(3),
            archived: RepositoryConditionState.Absent,
            fork: RepositoryConditionState.Present);

        RepositoryEligibilityResult first = Evaluate(repository);
        RepositoryEligibilityResult second = Evaluate(repository);

        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Activity, second.Activity);

        foreach (ContributionPurpose purpose in Enum.GetValues<ContributionPurpose>())
        {
            Assert.Equal(first.For(purpose).Decision, second.For(purpose).Decision);
            Assert.Equal(first.For(purpose).Reasons, second.For(purpose).Reasons);
        }
    }

    [Fact]
    public void Evaluate_IsIndependentOfCaptureTimeOffsetRepresentation()
    {
        ProfileRepository repository = Repository(
            "offset",
            lastPushedAt: Cutoff.AddHours(1));

        RepositoryEligibilityResult utc = Evaluate(repository, capturedAt: CapturedAt);
        RepositoryEligibilityResult shifted = Evaluate(
            repository,
            capturedAt: CapturedAt.ToOffset(TimeSpan.FromHours(5)));

        Assert.Equal(utc.Activity, shifted.Activity);
        Assert.Equal(utc.Status, shifted.Status);
    }

    // ---------------------------------------------------------------- guards

    [Fact]
    public void Construction_RequiresPolicy() =>
        Assert.Throws<ArgumentNullException>(() =>
            new RepositoryEligibilityPolicy(policy: null!));

    [Fact]
    public void Evaluate_RequiresRepositoryAndAdmission()
    {
        RepositoryEligibilityPolicy policy = new(Policy);
        ProfileRepository repository = Repository("guard", lastPushedAt: CapturedAt);

        Assert.Throws<ArgumentNullException>(() => policy.Evaluate(
            repository: null!,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference("guard"),
            CapturedAt));

        Assert.Throws<ArgumentNullException>(() => policy.Evaluate(
            repository,
            admission: null!,
            AnalysisStatus.Completed,
            CreateAnalysisReference("guard"),
            CapturedAt));
    }

    // -------------------------------------------------------------- fixtures

    private static ProfileRepository Repository(
        string name,
        DateTimeOffset? lastPushedAt,
        RepositoryConditionState archived = RepositoryConditionState.Absent,
        RepositoryConditionState fork = RepositoryConditionState.Absent,
        RepositoryConditionState templateDerived = RepositoryConditionState.Absent,
        RepositoryConditionState empty = RepositoryConditionState.Absent,
        DateTimeOffset? createdAt = null) =>
        new(
            CreateReference(name),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork,
            templateDerived,
            archived,
            empty,
            lastPushedAt,
            createdAt);

    private static RepositoryEligibilityResult Evaluate(
        ProfileRepository repository,
        AnalysisStatus? analysisStatus = AnalysisStatus.Completed,
        RepositoryAnalysisReference? analysisReference = null,
        EligibilityPolicy? policy = null,
        DateTimeOffset? capturedAt = null)
    {
        EligibilityPolicy resolved = policy ?? Policy;

        // Admission and contribution evaluation must share one policy version.
        AnalysisAdmissionResult admission = new(
            repository.Reference,
            AnalysisAdmissionDecision.Analyze,
            reason: null,
            resolved.Version);

        return new RepositoryEligibilityPolicy(resolved).Evaluate(
            repository,
            admission,
            analysisStatus,
            analysisStatus == AnalysisStatus.Completed
                ? analysisReference ?? CreateAnalysisReference(repository.Reference.Name)
                : analysisReference,
            capturedAt ?? CapturedAt);
    }

    private static RepositoryEligibilityResult EvaluateEmpty(string name)
    {
        ProfileRepository repository = Repository(
            name,
            lastPushedAt: CapturedAt,
            empty: RepositoryConditionState.Present);

        return new RepositoryEligibilityPolicy(Policy).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Skip),
            analysisStatus: null,
            analysisReference: null,
            CapturedAt);
    }

    private static RepositoryEligibilityResult EvaluateUnadmitted(
        string name,
        RepositoryConditionState archived = RepositoryConditionState.Absent)
    {
        ProfileRepository repository = Repository(
            name,
            lastPushedAt: CapturedAt,
            archived: archived);

        return new RepositoryEligibilityPolicy(Policy).Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Undetermined),
            analysisStatus: null,
            analysisReference: null,
            CapturedAt);
    }

    private static void AssertDecision(
        ContributionAssessment assessment,
        ContributionDecision decision,
        params EligibilityReason[] reasons)
    {
        Assert.Equal(decision, assessment.Decision);
        Assert.Equal(reasons, assessment.Reasons);
    }
}

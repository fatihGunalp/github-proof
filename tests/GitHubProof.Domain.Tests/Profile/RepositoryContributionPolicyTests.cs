using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepositoryContributionPolicyTests
{
    [Fact]
    public void Evaluate_EmptyRepository_RestrictsEveryEvidenceBearingPurpose()
    {
        ProfileRepository repository = CreateProfileRepository(
            "empty",
            empty: RepositoryConditionState.Present);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Skip),
            analysisStatus: null,
            analysisReference: null);

        Assert.Equal(
            ContributionDecision.NotAllowed,
            assessment.ProfileObservation.Decision);
        Assert.Equal(
            [EligibilityReason.EmptyRepository],
            assessment.ProfileObservation.Reasons);
        Assert.Equal(
            ContributionDecision.NotAllowed,
            assessment.IndependentRepetition.Decision);
        Assert.Null(assessment.AnalysisReference);
    }

    [Fact]
    public void Evaluate_SkipReason_IsPropagatedInsteadOfAssumed()
    {
        ProfileRepository repository = CreateProfileRepository(
            "bounded",
            empty: RepositoryConditionState.Undetermined);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(
                repository,
                AnalysisAdmissionDecision.Skip,
                EligibilityReason.RequiredInputUnavailable),
            analysisStatus: null,
            analysisReference: null);

        Assert.Equal(
            [EligibilityReason.RequiredInputUnavailable],
            assessment.ProfileObservation.Reasons);
        Assert.Equal(
            [EligibilityReason.RequiredInputUnavailable],
            assessment.IndependentRepetition.Reasons);
    }

    [Theory]
    [InlineData(AnalysisStatus.Completed)]
    [InlineData(AnalysisStatus.Partial)]
    public void Evaluate_AnalyzedEvidenceSurface_AllowsObservationAndIndependentRepetition(
        AnalysisStatus status)
    {
        ProfileRepository repository = CreateProfileRepository("original");
        RepositoryAnalysisReference analysis = CreateAnalysisReference("original");

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            status,
            analysis);

        Assert.True(assessment.ProfileObservation.IsAllowed);
        Assert.True(assessment.IndependentRepetition.IsAllowed);
        Assert.Equal(IndependenceClassification.Independent, assessment.Independence);
        Assert.Equal(analysis, assessment.AnalysisReference);
    }

    [Fact]
    public void Evaluate_InsufficientData_IsNotTechnicalFailure()
    {
        ProfileRepository repository = CreateProfileRepository("sparse");

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.InsufficientData,
            CreateAnalysisReference("sparse"));

        Assert.Equal(
            ContributionDecision.NotAllowed,
            assessment.ProfileObservation.Decision);
        Assert.Equal(
            [EligibilityReason.InsufficientEvidence],
            assessment.ProfileObservation.Reasons);
        Assert.Equal(
            [EligibilityReason.InsufficientEvidence],
            assessment.IndependentRepetition.Reasons);
    }

    [Theory]
    [InlineData(AnalysisStatus.Failed)]
    [InlineData(null)]
    public void Evaluate_TechnicalFailure_LeavesObservationUndetermined(
        AnalysisStatus? status)
    {
        ProfileRepository repository = CreateProfileRepository("broken");

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            status,
            analysisReference: null);

        Assert.Equal(
            ContributionDecision.Undetermined,
            assessment.ProfileObservation.Decision);
        Assert.Equal(
            [EligibilityReason.RequiredInputUnavailable],
            assessment.ProfileObservation.Reasons);
        Assert.Equal(
            ContributionDecision.Undetermined,
            assessment.IndependentRepetition.Decision);
        Assert.Null(assessment.AnalysisReference);
    }

    [Fact]
    public void Evaluate_KnownForkWithTechnicalFailure_StillRestrictsRepetition()
    {
        ProfileRepository repository = CreateProfileRepository(
            "forked",
            fork: RepositoryConditionState.Present);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Failed,
            analysisReference: null);

        Assert.Equal(
            ContributionDecision.Undetermined,
            assessment.ProfileObservation.Decision);
        Assert.Equal(
            ContributionDecision.NotAllowed,
            assessment.IndependentRepetition.Decision);
        Assert.Equal(
            [EligibilityReason.ForkDetected],
            assessment.IndependentRepetition.Reasons);
    }

    [Fact]
    public void Evaluate_AdmissionUndetermined_DoesNotAllowAnyContribution()
    {
        ProfileRepository repository = CreateProfileRepository(
            "unknown",
            empty: RepositoryConditionState.Undetermined);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Undetermined),
            analysisStatus: null,
            analysisReference: null);

        Assert.Equal(
            ContributionDecision.Undetermined,
            assessment.ProfileObservation.Decision);
        Assert.Equal(
            ContributionDecision.Undetermined,
            assessment.IndependentRepetition.Decision);
    }

    [Theory]
    [InlineData(RepositoryConditionState.Present, RepositoryConditionState.Absent, EligibilityReason.ForkDetected)]
    [InlineData(RepositoryConditionState.Absent, RepositoryConditionState.Present, EligibilityReason.TemplateDerived)]
    public void Evaluate_InheritedLineage_PreventsIndependentRepetition(
        RepositoryConditionState fork,
        RepositoryConditionState templateDerived,
        EligibilityReason expectedReason)
    {
        ProfileRepository repository = CreateProfileRepository(
            "derived",
            fork,
            templateDerived);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference("derived"));

        Assert.True(assessment.ProfileObservation.IsAllowed);
        Assert.Equal(
            IndependenceClassification.NonIndependent,
            assessment.Independence);
        Assert.Equal(
            ContributionDecision.NotAllowed,
            assessment.IndependentRepetition.Decision);
        Assert.Equal([expectedReason], assessment.IndependentRepetition.Reasons);
    }

    [Fact]
    public void Evaluate_ForkAndTemplateLineage_ReportsBothReasons()
    {
        ProfileRepository repository = CreateProfileRepository(
            "derived",
            fork: RepositoryConditionState.Present,
            templateDerived: RepositoryConditionState.Present);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference("derived"));

        Assert.Equal(
            [EligibilityReason.ForkDetected, EligibilityReason.TemplateDerived],
            assessment.IndependentRepetition.Reasons);
    }

    [Fact]
    public void Evaluate_UnknownLineage_DoesNotAssumeIndependence()
    {
        ProfileRepository repository = CreateProfileRepository(
            "unknown-lineage",
            fork: RepositoryConditionState.Undetermined);

        RepositoryContributionAssessment assessment = Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference("unknown-lineage"));

        Assert.True(assessment.ProfileObservation.IsAllowed);
        Assert.Equal(
            IndependenceClassification.Undetermined,
            assessment.Independence);
        Assert.Equal(
            ContributionDecision.Undetermined,
            assessment.IndependentRepetition.Decision);
        Assert.Equal(
            [EligibilityReason.IndependenceUndetermined],
            assessment.IndependentRepetition.Reasons);
    }

    [Fact]
    public void Evaluate_WhenPolicyVersionsDiffer_Rejects()
    {
        ProfileRepository repository = CreateProfileRepository("original");
        AnalysisAdmissionResult admission = new(
            repository.Reference,
            AnalysisAdmissionDecision.Analyze,
            reason: null,
            new EligibilityPolicyVersion("other-policy"));

        Assert.Throws<ArgumentException>(() => Evaluate(
            repository,
            admission,
            AnalysisStatus.Completed,
            CreateAnalysisReference("original")));
    }

    [Fact]
    public void Evaluate_WhenAdmissionBelongsToAnotherRepository_Rejects()
    {
        ProfileRepository repository = CreateProfileRepository("original");
        ProfileRepository other = CreateProfileRepository("other");

        Assert.Throws<ArgumentException>(() => Evaluate(
            repository,
            CreateAdmission(other, AnalysisAdmissionDecision.Analyze),
            AnalysisStatus.Completed,
            CreateAnalysisReference("original")));
    }

    [Fact]
    public void Evaluate_WhenAnalysisInputArrivesWithoutAdmission_Rejects()
    {
        ProfileRepository repository = CreateProfileRepository(
            "empty",
            empty: RepositoryConditionState.Present);

        Assert.Throws<ArgumentException>(() => Evaluate(
            repository,
            CreateAdmission(repository, AnalysisAdmissionDecision.Skip),
            AnalysisStatus.Completed,
            CreateAnalysisReference("empty")));
    }

    private static RepositoryContributionAssessment Evaluate(
        ProfileRepository repository,
        AnalysisAdmissionResult admission,
        AnalysisStatus? analysisStatus,
        RepositoryAnalysisReference? analysisReference) =>
        new RepositoryContributionPolicy(PolicyVersion).Evaluate(
            repository,
            admission,
            analysisStatus,
            analysisReference);
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class ScoreBreakdownTests
{
    [Fact]
    public void Constructor_DerivesOverallScoreAndExcludesCompositeSignals()
    {
        SignalResult included = CreateSignal(
            "repository.readme.exists",
            SignalStatus.Strong,
            score: 5,
            maximumScore: 5);
        SignalResult composite = CreateSignal(
            "portfolio.presentation-completeness",
            SignalStatus.Present,
            score: 5,
            maximumScore: 5,
            includeInOverallScore: false,
            category: AnalysisCategory.PortfolioPresentation);

        ScoreBreakdown result = new(
        [
            CreateCategory(AnalysisCategory.RepositoryUnderstandability, [included]),
            CreateCategory(AnalysisCategory.PortfolioPresentation, [composite])
        ]);

        Assert.Equal(5, result.EarnedScore);
        Assert.Equal(5, result.ApplicableMaximumScore);
        Assert.Equal(100, result.NormalizedScore);
        Assert.Equal("score.label.very-strong-visible-evidence", result.LabelKey);
        Assert.Equal([composite], result.ExcludedFromOverallScoreSignals);
        Assert.Empty(result.ExcludedSignals);
        Assert.Equal(
            ScoreCalculationMethod.RawApplicableScoreNormalization,
            result.CalculationMethod);
    }

    [Fact]
    public void Constructor_ExcludesUnscoredSignalsAndRoundsHalfUp()
    {
        SignalResult scored = CreateSignal(
            "repository.description.present",
            SignalStatus.Present,
            score: 3,
            maximumScore: 8);
        SignalResult unavailable = CreateSignal(
            "documentation.installation-guide",
            SignalStatus.UnavailableEvidence,
            score: null,
            maximumScore: 8);

        ScoreBreakdown result = new(
        [CreateCategory(AnalysisCategory.RepositoryUnderstandability, [scored, unavailable])]);

        Assert.Equal(3, result.EarnedScore);
        Assert.Equal(8, result.ApplicableMaximumScore);
        Assert.Equal(38, result.NormalizedScore);
        Assert.Equal("score.label.basic-visible-evidence", result.LabelKey);
        Assert.Equal([unavailable], result.ExcludedSignals);
    }

    [Fact]
    public void Constructor_LeavesScoreAndLabelKeyEmptyWhenNoSignalIsApplicableToOverall()
    {
        SignalResult notApplicable = CreateSignal(
            "process.release-usage",
            SignalStatus.NotApplicable,
            score: null,
            maximumScore: 6,
            category: AnalysisCategory.DevelopmentProcessVisibility);
        SignalResult composite = CreateSignal(
            "portfolio.presentation-completeness",
            SignalStatus.Present,
            score: 5,
            maximumScore: 5,
            includeInOverallScore: false,
            category: AnalysisCategory.PortfolioPresentation);

        ScoreBreakdown result = new(
        [
            CreateCategory(AnalysisCategory.DevelopmentProcessVisibility, [notApplicable]),
            CreateCategory(AnalysisCategory.PortfolioPresentation, [composite])
        ]);

        Assert.Equal(0, result.EarnedScore);
        Assert.Equal(0, result.ApplicableMaximumScore);
        Assert.Null(result.NormalizedScore);
        Assert.Null(result.LabelKey);
        Assert.Equal([notApplicable], result.ExcludedSignals);
        Assert.Equal([composite], result.ExcludedFromOverallScoreSignals);
    }

    [Fact]
    public void Constructor_RejectsDuplicateCategories()
    {
        CategoryResult first = CreateCategory(
            AnalysisCategory.ValidationAndUsability,
            [CreateSignal("validation.test-structure", SignalStatus.Present, 5, 10, category: AnalysisCategory.ValidationAndUsability)]);
        CategoryResult second = CreateCategory(
            AnalysisCategory.ValidationAndUsability,
            [CreateSignal("validation.continuous-integration", SignalStatus.Present, 4, 8, category: AnalysisCategory.ValidationAndUsability)]);

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new ScoreBreakdown([first, second]));

        Assert.Equal("categoryBreakdowns", exception.ParamName);
    }

    private static CategoryResult CreateCategory(
        AnalysisCategory category,
        IEnumerable<SignalResult> signals) =>
        new(category, signals);

    private static SignalResult CreateSignal(
        string signalKey,
        SignalStatus status,
        int? score,
        int maximumScore,
        bool includeInOverallScore = true,
        AnalysisCategory category = AnalysisCategory.RepositoryUnderstandability) =>
        new(
            signalKey,
            signalVersion: 1,
            category,
            status,
            score,
            maximumScore,
            includeInOverallScore,
            evidence: status == SignalStatus.Missing ? [CreateEvidence()] : null,
            evaluatedAt: DateTimeOffset.UtcNow);

    private static EvidenceItem CreateEvidence() =>
        new(
            "evidence.key",
            "README.md",
            "observed",
            "README.md",
            DateTimeOffset.UtcNow,
            EvidenceConfidence.High,
            null);
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class CategoryResultTests
{
    [Fact]
    public void Constructor_DerivesScoreAndRoundsNormalizedScoreHalfUp()
    {
        CategoryResult result = CreateResult(
        [
            CreateSignal(SignalStatus.Strong, score: 3, maximumScore: 6),
            CreateSignal(SignalStatus.Missing, score: 0, maximumScore: 2)
        ]);

        Assert.Equal(3, result.Score);
        Assert.Equal(8, result.MaximumScore);
        Assert.Equal(38, result.NormalizedScore);
        Assert.Equal(CategoryStatus.Completed, result.Status);
    }

    [Fact]
    public void Constructor_IncludesCompositeSignalsInCategoryScore()
    {
        CategoryResult result = CreateResult(
        [
            CreateSignal(SignalStatus.Present, score: 3, maximumScore: 4, includeInOverallScore: false),
            CreateSignal(SignalStatus.Strong, score: 5, maximumScore: 5)
        ]);

        Assert.Equal(8, result.Score);
        Assert.Equal(9, result.MaximumScore);
        Assert.Equal(89, result.NormalizedScore);
    }

    [Fact]
    public void Constructor_ExcludesUnscoredResultsAndMarksPartialWhenSomeEvidenceIsUnavailable()
    {
        CategoryResult result = CreateResult(
        [
            CreateSignal(SignalStatus.Present, score: 4, maximumScore: 5),
            CreateSignal(SignalStatus.UnavailableEvidence, score: null, maximumScore: 8)
        ]);

        Assert.Equal(4, result.Score);
        Assert.Equal(5, result.MaximumScore);
        Assert.Equal(80, result.NormalizedScore);
        Assert.Equal(CategoryStatus.Partial, result.Status);
    }

    [Theory]
    [InlineData(SignalStatus.NotApplicable)]
    [InlineData(SignalStatus.UnavailableEvidence)]
    [InlineData(SignalStatus.RequiresHumanReview)]
    public void Constructor_DerivesStatusWhenNoSignalCanBeScored(SignalStatus status)
    {
        CategoryResult result = CreateResult(
        [CreateSignal(status, score: null, maximumScore: 5)]);

        Assert.Equal(0, result.Score);
        Assert.Equal(0, result.MaximumScore);
        Assert.Null(result.NormalizedScore);
        Assert.Equal(
            status == SignalStatus.NotApplicable
                ? CategoryStatus.NotEvaluated
                : CategoryStatus.InsufficientData,
            result.Status);
    }

    [Fact]
    public void Constructor_RejectsSignalFromAnotherCategory()
    {
        SignalResult signal = CreateSignal(
            SignalStatus.Present,
            score: 4,
            maximumScore: 5,
            category: AnalysisCategory.PortfolioPresentation);

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateResult([signal]));

        Assert.Equal("signalResults", exception.ParamName);
    }

    private static CategoryResult CreateResult(IEnumerable<SignalResult> signals) =>
        new(
            AnalysisCategory.RepositoryUnderstandability,
            signals,
            "Category summary.",
            ["Known limitation.", " Known limitation. "]);

    private static SignalResult CreateSignal(
        SignalStatus status,
        int? score,
        int maximumScore,
        bool includeInOverallScore = true,
        AnalysisCategory category = AnalysisCategory.RepositoryUnderstandability) =>
        new(
            $"signal.{Guid.NewGuid():N}",
            signalVersion: 1,
            category,
            status,
            score,
            maximumScore,
            includeInOverallScore,
            evidence: status == SignalStatus.Missing ? [CreateEvidence()] : null,
            explanation: "Signal explanation.",
            recommendation: status is SignalStatus.Partial
                or SignalStatus.Missing
                or SignalStatus.RequiresHumanReview
                ? "Do the next useful thing."
                : null,
            limitations: null,
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

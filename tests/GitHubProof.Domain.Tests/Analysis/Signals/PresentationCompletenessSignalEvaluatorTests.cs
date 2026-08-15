using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class PresentationCompletenessSignalEvaluatorTests
{
    private static readonly DateTimeOffset UpstreamEvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalPresentationCompletenessContract()
    {
        SignalDefinition definition = CreateEvaluator().Definition;

        Assert.Equal("portfolio.presentation-completeness", definition.Key);
        Assert.Equal("Repository Sunum Bütünlüğü", definition.Name);
        Assert.Equal(AnalysisCategory.PortfolioPresentation, definition.Category);
        Assert.Equal(5, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.False(definition.IncludeInOverallScore);
        Assert.Equal(
            PresentationCompletenessSignalEvaluator.RequiredSignalKeys,
            definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenAllFiveSignalsArePositive_ReturnsStrongFive()
    {
        SignalResult result = CreateEvaluator().Evaluate(CreateRequiredResults(
            SignalStatus.Strong,
            SignalStatus.Present,
            SignalStatus.Partial,
            SignalStatus.Present,
            SignalStatus.Strong));

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(5, result.Score);
        Assert.False(result.IncludeInOverallScore);
        Assert.Null(result.Recommendation);
        Assert.Equal(5, result.Evidence.Count);
        Assert.All(result.Evidence, evidence => Assert.Equal("SignalResult", evidence.Source));
    }

    [Fact]
    public void Evaluate_WhenFourSignalsArePositive_ReturnsPresentFour()
    {
        SignalResult result = CreateEvaluator().Evaluate(CreateResultsWithPositiveCount(4));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Null(result.Recommendation);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Evaluate_WhenOneToThreeSignalsArePositive_ReturnsPartial(int positiveCount)
    {
        SignalResult result = CreateEvaluator().Evaluate(
            CreateResultsWithPositiveCount(positiveCount));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(positiveCount, result.Score);
        Assert.NotNull(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenNoSignalIsPositive_ReturnsMissingZero()
    {
        SignalResult result = CreateEvaluator().Evaluate(CreateResultsWithPositiveCount(0));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.NotNull(result.Recommendation);
        Assert.Equal(5, result.Evidence.Count);
    }

    [Fact]
    public void Evaluate_WhenAnyRequiredSignalIsUnavailable_ReturnsUnavailableEvidence()
    {
        List<SignalResult> upstream = CreateResultsWithPositiveCount(4);
        upstream[0] = CreateUpstream(
            upstream[0].SignalKey,
            SignalStatus.UnavailableEvidence);
        upstream[1] = CreateUpstream(
            upstream[1].SignalKey,
            SignalStatus.RequiresHumanReview);

        SignalResult result = CreateEvaluator().Evaluate(upstream);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WhenRequiredSignalNeedsHumanReview_ReturnsRequiresHumanReview()
    {
        List<SignalResult> upstream = CreateResultsWithPositiveCount(4);
        upstream[2] = CreateUpstream(
            upstream[2].SignalKey,
            SignalStatus.RequiresHumanReview);

        SignalResult result = CreateEvaluator().Evaluate(upstream);

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.NotNull(result.Recommendation);
        Assert.Contains(result.Evidence, evidence =>
            evidence.Value == SignalStatus.RequiresHumanReview.ToString());
    }

    [Fact]
    public void Evaluate_WhenRequiredSignalIsNotApplicable_FailsFixedChecklistContract()
    {
        List<SignalResult> upstream = CreateResultsWithPositiveCount(4);
        upstream[3] = CreateUpstream(
            upstream[3].SignalKey,
            SignalStatus.NotApplicable);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => CreateEvaluator().Evaluate(upstream));

        Assert.Contains("cannot be NotApplicable", exception.Message);
    }

    [Fact]
    public void Evaluate_WhenRequiredResultIsMissing_FailsFast()
    {
        List<SignalResult> upstream = CreateResultsWithPositiveCount(5);
        upstream.RemoveAt(0);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CreateEvaluator().Evaluate(upstream));

        Assert.Contains("is missing", exception.Message);
    }

    [Fact]
    public void Evaluate_WhenResultKeyIsDuplicated_FailsFast()
    {
        List<SignalResult> upstream = CreateResultsWithPositiveCount(5);
        upstream.Add(upstream[0]);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CreateEvaluator().Evaluate(upstream));

        Assert.Contains("must be unique", exception.Message);
    }

    [Fact]
    public void Evaluate_UsesOnlyUpstreamResultsAndProducesDeterministicState()
    {
        IReadOnlyList<SignalResult> upstream = CreateRequiredResults(
            SignalStatus.Present,
            SignalStatus.Missing,
            SignalStatus.Partial,
            SignalStatus.Present,
            SignalStatus.Missing);

        SignalResult first = CreateEvaluator().Evaluate(upstream);
        SignalResult second = CreateEvaluator().Evaluate(upstream.Reverse());

        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.SignalVersion, second.SignalVersion);
        Assert.Equal(first.Category, second.Category);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.MaximumScore, second.MaximumScore);
        Assert.Equal(first.IncludeInOverallScore, second.IncludeInOverallScore);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(first.Explanation, second.Explanation);
        Assert.Equal(first.Recommendation, second.Recommendation);
        Assert.Equal(first.Limitations, second.Limitations);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
        Assert.Equal(
            PresentationCompletenessSignalEvaluator.RequiredSignalKeys,
            first.Evidence.Select(evidence => evidence.Location));
    }

    private static PresentationCompletenessSignalEvaluator CreateEvaluator() =>
        new(new FixedTimeProvider(EvaluatedAt));

    private static List<SignalResult> CreateResultsWithPositiveCount(int positiveCount) =>
        PresentationCompletenessSignalEvaluator.RequiredSignalKeys
            .Select((key, index) => CreateUpstream(
                key,
                index < positiveCount ? SignalStatus.Present : SignalStatus.Missing))
            .ToList();

    private static IReadOnlyList<SignalResult> CreateRequiredResults(
        params SignalStatus[] statuses)
    {
        Assert.Equal(
            PresentationCompletenessSignalEvaluator.RequiredSignalKeys.Count,
            statuses.Length);

        return PresentationCompletenessSignalEvaluator.RequiredSignalKeys
            .Select((key, index) => CreateUpstream(key, statuses[index]))
            .ToArray();
    }

    private static SignalResult CreateUpstream(string key, SignalStatus status)
    {
        int? score = status switch
        {
            SignalStatus.Strong or SignalStatus.Present or SignalStatus.Partial => 1,
            SignalStatus.Missing => 0,
            _ => null
        };

        return new SignalResult(
            key,
            signalVersion: 1,
            AnalysisCategory.RepositoryUnderstandability,
            status,
            score,
            maximumScore: 1,
            includeInOverallScore: true,
            evidence:
            [
                new EvidenceItem(
                    key,
                    "Test",
                    status.ToString(),
                    location: null,
                    UpstreamEvaluatedAt,
                    EvidenceConfidence.High,
                    notes: null)
            ],
            explanation: "Synthetic upstream result.",
            recommendation: status is SignalStatus.Partial
                or SignalStatus.Missing
                or SignalStatus.RequiresHumanReview
                ? "Resolve the synthetic condition."
                : null,
            limitations: null,
            UpstreamEvaluatedAt);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

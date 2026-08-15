using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis;

public sealed class RepositorySignalAnalyzerTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Analyze_RunsExactIndependentSetThenAppendsDerivedSignalInCanonicalOrder()
    {
        List<FakeSignalEvaluator> evaluators = CreateEvaluators();
        RepositorySignalAnalyzer analyzer = CreateAnalyzer(evaluators.AsEnumerable().Reverse());

        IReadOnlyList<SignalResult> results = analyzer.Analyze(CreateSnapshot());

        Assert.Equal(15, results.Count);
        Assert.Equal(
            RepositorySignalAnalyzer.CanonicalIndependentSignalKeys
                .Append(PresentationCompletenessSignalEvaluator.SignalKey),
            results.Select(result => result.SignalKey));
        Assert.Equal(15, results.Select(result => result.SignalKey).Distinct().Count());
        Assert.All(evaluators, evaluator => Assert.Equal(1, evaluator.CallCount));
        Assert.Equal(SignalStatus.Strong, results[^1].Status);
        Assert.Equal(5, results[^1].Score);
        Assert.False(results[^1].IncludeInOverallScore);
    }

    [Fact]
    public void Analyze_WithAllProductionEvaluators_ProducesEveryCanonicalSignalOnce()
    {
        FixedTimeProvider timeProvider = new(EvaluatedAt);
        ISignalEvaluator[] evaluators = CreateProductionEvaluators(timeProvider);
        RepositorySignalAnalyzer analyzer = new(
            evaluators.Reverse(),
            new PresentationCompletenessSignalEvaluator(timeProvider));

        IReadOnlyList<SignalResult> results = analyzer.Analyze(CreateSnapshot());

        Assert.Equal(15, results.Count);
        Assert.Equal(
            RepositorySignalAnalyzer.CanonicalIndependentSignalKeys
                .Append(PresentationCompletenessSignalEvaluator.SignalKey),
            results.Select(result => result.SignalKey));
        Assert.Equal(15, results.Select(result => result.SignalKey).Distinct().Count());
    }

    [Fact]
    public void Constructor_WhenEvaluatorKeyIsDuplicated_FailsFast()
    {
        List<FakeSignalEvaluator> evaluators = CreateEvaluators();
        evaluators.Add(new FakeSignalEvaluator(evaluators[0].Definition.Key));

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CreateAnalyzer(evaluators));

        Assert.Contains("Duplicate", exception.Message);
    }

    [Fact]
    public void Constructor_WhenExpectedEvaluatorIsMissing_FailsFast()
    {
        List<FakeSignalEvaluator> evaluators = CreateEvaluators();
        evaluators.RemoveAt(0);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CreateAnalyzer(evaluators));

        Assert.Contains("Missing", exception.Message);
    }

    [Fact]
    public void Constructor_WhenEvaluatorIsUnknown_FailsFast()
    {
        List<FakeSignalEvaluator> evaluators = CreateEvaluators();
        evaluators[^1] = new FakeSignalEvaluator("unknown.signal");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CreateAnalyzer(evaluators));

        Assert.Contains("Unknown", exception.Message);
    }

    [Fact]
    public void Analyze_WhenEvaluatorReturnsDifferentKey_FailsFast()
    {
        List<FakeSignalEvaluator> evaluators = CreateEvaluators();
        string expectedKey = evaluators[0].Definition.Key;
        evaluators[0] = new FakeSignalEvaluator(expectedKey, "different.result");
        RepositorySignalAnalyzer analyzer = CreateAnalyzer(evaluators);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => analyzer.Analyze(CreateSnapshot()));

        Assert.Contains(expectedKey, exception.Message);
        Assert.Contains("different.result", exception.Message);
    }

    [Fact]
    public void Analyze_WithSameSnapshotAndFixedResults_ProducesEquivalentState()
    {
        RepositorySnapshot snapshot = CreateSnapshot();
        RepositorySignalAnalyzer firstAnalyzer = CreateAnalyzer(CreateEvaluators().AsEnumerable().Reverse());
        RepositorySignalAnalyzer secondAnalyzer = CreateAnalyzer(CreateEvaluators());

        IReadOnlyList<SignalResult> first = firstAnalyzer.Analyze(snapshot);
        IReadOnlyList<SignalResult> second = secondAnalyzer.Analyze(snapshot);

        Assert.Equal(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            AssertEquivalent(first[index], second[index]);
        }
    }

    [Fact]
    public void CanonicalIndependentSet_ContainsEveryProductionEvaluatorExactlyOnce()
    {
        Assert.Equal(14, RepositorySignalAnalyzer.CanonicalIndependentSignalKeys.Count);
        Assert.Equal(
            14,
            RepositorySignalAnalyzer.CanonicalIndependentSignalKeys
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Equal(
            CreateEvaluators().Select(evaluator => evaluator.Definition.Key).ToHashSet(StringComparer.Ordinal),
            RepositorySignalAnalyzer.CanonicalIndependentSignalKeys.ToHashSet(StringComparer.Ordinal));
    }

    private static RepositorySignalAnalyzer CreateAnalyzer(
        IEnumerable<ISignalEvaluator> evaluators) =>
        new(
            evaluators,
            new PresentationCompletenessSignalEvaluator(
                new FixedTimeProvider(EvaluatedAt)));

    private static List<FakeSignalEvaluator> CreateEvaluators() =>
        RepositorySignalAnalyzer.CanonicalIndependentSignalKeys
            .Select(key => new FakeSignalEvaluator(key))
            .ToList();

    private static ISignalEvaluator[] CreateProductionEvaluators(TimeProvider timeProvider) =>
    [
        new ReadmeExistsSignalEvaluator(timeProvider),
        new DescriptionPresentSignalEvaluator(timeProvider),
        new InstallationGuideSignalEvaluator(timeProvider),
        new LicensePresentSignalEvaluator(timeProvider),
        new UsageDemoSignalEvaluator(timeProvider),
        new CommitHistorySignalEvaluator(timeProvider),
        new CommitDistributionSignalEvaluator(timeProvider),
        new IssueUsageSignalEvaluator(timeProvider),
        new PullRequestUsageSignalEvaluator(timeProvider),
        new ReleaseUsageSignalEvaluator(timeProvider),
        new TestStructureSignalEvaluator(timeProvider),
        new ContinuousIntegrationSignalEvaluator(timeProvider),
        new TopicsSignalEvaluator(timeProvider),
        new HomepageLiveDemoSignalEvaluator(timeProvider)
    ];

    private static RepositorySnapshot CreateSnapshot() =>
        new(
            new RepositoryIdentity(
                "owner",
                "repository",
                new Uri("https://github.com/owner/repository"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            metadata: null,
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability: null,
            collectionWarnings: null);

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
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
    }

    private sealed class FakeSignalEvaluator : ISignalEvaluator
    {
        private readonly string _resultKey;

        public FakeSignalEvaluator(string definitionKey, string? resultKey = null)
        {
            _resultKey = resultKey ?? definitionKey;
            Definition = new SignalDefinition(
                definitionKey,
                definitionKey,
                AnalysisCategory.RepositoryUnderstandability,
                "Synthetic evaluator.",
                maximumScore: 1,
                requiredDataKeys: ["synthetic"],
                applicabilityRules: ["Synthetic."],
                evaluationRules: ["Synthetic."],
                evidenceRequirements: ["Synthetic."],
                explanationTemplate: "Synthetic.",
                recommendationTemplate: "Synthetic.",
                includeInOverallScore: true,
                version: 1);
        }

        public int CallCount { get; private set; }

        public SignalDefinition Definition { get; }

        public SignalResult Evaluate(RepositorySnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            CallCount++;

            return new SignalResult(
                _resultKey,
                signalVersion: 1,
                AnalysisCategory.RepositoryUnderstandability,
                SignalStatus.Present,
                score: 1,
                maximumScore: 1,
                includeInOverallScore: true,
                evidence: null,
                explanation: "Synthetic result.",
                recommendation: null,
                limitations: null,
                EvaluatedAt);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

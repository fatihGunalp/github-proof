using System.Globalization;
using System.Text.Json;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis;

public sealed class RepositoryAnalysisComposerTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("042e8415-8f80-4a84-ad2e-044000000001");
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset StartedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");
    private static readonly DateTimeOffset CompletedAt =
        DateTimeOffset.Parse("2026-08-15T09:03:00Z");
    private static readonly AnalysisVersion Version = new("0.1.0", "2026.08.15", 1);

    private static readonly IReadOnlyList<SignalSpec> CanonicalSpecs =
    [
        new("repository.readme.exists", AnalysisCategory.RepositoryUnderstandability, 5, true),
        new("repository.description.present", AnalysisCategory.RepositoryUnderstandability, 4, true),
        new("documentation.installation-guide", AnalysisCategory.RepositoryUnderstandability, 8, true),
        new("repository.license.present", AnalysisCategory.RepositoryUnderstandability, 3, true),
        new("documentation.usage-or-demo", AnalysisCategory.RepositoryUnderstandability, 7, true),
        new("process.commit-history", AnalysisCategory.DevelopmentProcessVisibility, 7, true),
        new("process.commit-distribution", AnalysisCategory.DevelopmentProcessVisibility, 8, true),
        new("process.issue-usage", AnalysisCategory.DevelopmentProcessVisibility, 5, true),
        new("process.pull-request-usage", AnalysisCategory.DevelopmentProcessVisibility, 6, true),
        new("process.release-usage", AnalysisCategory.DevelopmentProcessVisibility, 6, true),
        new("validation.test-structure", AnalysisCategory.ValidationAndUsability, 10, true),
        new("validation.continuous-integration", AnalysisCategory.ValidationAndUsability, 8, true),
        new("portfolio.topics", AnalysisCategory.PortfolioPresentation, 4, true),
        new("portfolio.homepage-or-live-demo", AnalysisCategory.PortfolioPresentation, 5, true),
        new("portfolio.presentation-completeness", AnalysisCategory.PortfolioPresentation, 5, false)
    ];

    [Fact]
    public void Compose_ProducesCanonicalRepositoryAnalysisAndPreservesRepositoryLiterals()
    {
        RepositorySnapshot snapshot = CreateSnapshot();
        IReadOnlyList<SignalResult> results = CreateResults();

        RepositoryAnalysis analysis = Compose(snapshot, results.Reverse());

        Assert.Equal(15, analysis.SignalResults.Count);
        Assert.Equal(
            RepositoryAnalysisComposer.CanonicalSignalKeys,
            analysis.SignalResults.Select(result => result.SignalKey));
        Assert.Equal(
            [
                AnalysisCategory.RepositoryUnderstandability,
                AnalysisCategory.DevelopmentProcessVisibility,
                AnalysisCategory.ValidationAndUsability,
                AnalysisCategory.PortfolioPresentation
            ],
            analysis.CategoryResults.Select(result => result.Category));
        Assert.Equal("Örnek Repo", analysis.Repository.Name);
        Assert.Equal("src/Örnek.cs --flag", analysis.SignalResults[0].Evidence[0].Value);
        Assert.Equal("src/Örnek.cs", analysis.SignalResults[0].Evidence[0].Location);
        Assert.Equal(ScoreCalculationMethod.RawApplicableScoreNormalization, analysis.ScoreBreakdown.CalculationMethod);
    }

    [Fact]
    public void Compose_AggregatesCategoriesAndAppliesOverallDenominatorRules()
    {
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["repository.readme.exists"] = new(SignalStatus.Missing, 0),
            ["process.commit-distribution"] = new(SignalStatus.NotApplicable, null),
            ["validation.continuous-integration"] = new(SignalStatus.UnavailableEvidence, null),
            ["portfolio.homepage-or-live-demo"] = new(SignalStatus.RequiresHumanReview, null),
            ["portfolio.presentation-completeness"] = new(SignalStatus.Strong, 5)
        });

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), results);
        CategoryResult portfolio = Assert.Single(
            analysis.CategoryResults,
            category => category.Category == AnalysisCategory.PortfolioPresentation);

        Assert.Equal(60, analysis.ScoreBreakdown.EarnedScore);
        Assert.Equal(65, analysis.ScoreBreakdown.ApplicableMaximumScore);
        Assert.Equal(92, analysis.ScoreBreakdown.NormalizedScore);
        Assert.Equal("score.label.very-strong-visible-evidence", analysis.ScoreBreakdown.LabelKey);
        Assert.Equal(9, portfolio.MaximumScore);
        Assert.Equal(9, portfolio.Score);
        Assert.Contains(
            analysis.ScoreBreakdown.ExcludedFromOverallScoreSignals,
            result => result.SignalKey == "portfolio.presentation-completeness");
        Assert.Equal(
            [
                "process.commit-distribution",
                "validation.continuous-integration",
                "portfolio.homepage-or-live-demo"
            ],
            analysis.ScoreBreakdown.ExcludedSignals.Select(result => result.SignalKey));
    }

    [Fact]
    public void Compose_SelectsAtMostThreeEvidenceBackedStrengthsWithCanonicalTieBreak()
    {
        IReadOnlyList<SignalResult> results = CreateResults(
            overrides: new Dictionary<string, ResultState>
            {
                ["documentation.installation-guide"] = new(SignalStatus.Strong, 8),
                ["process.commit-distribution"] = new(SignalStatus.Strong, 8),
                ["validation.continuous-integration"] = new(SignalStatus.Strong, 8),
                ["portfolio.homepage-or-live-demo"] = new(SignalStatus.Strong, 5),
                ["validation.test-structure"] = new(SignalStatus.Strong, 10, HasEvidence: false)
            },
            defaultState: new ResultState(SignalStatus.Partial, 1));

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), results);

        Assert.Equal(
            [
                "documentation.installation-guide",
                "process.commit-distribution",
                "validation.continuous-integration"
            ],
            analysis.Strengths.Select(highlight => highlight.SignalKey));
        Assert.Equal(3, analysis.Strengths.Count);
        Assert.DoesNotContain(
            analysis.Strengths,
            highlight => highlight.SignalKey == "validation.test-structure");
    }

    [Fact]
    public void Compose_SelectsMissingGapsButNeverTreatsUncertaintyAsFailure()
    {
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["repository.readme.exists"] = new(SignalStatus.Missing, 0),
            ["process.issue-usage"] = new(SignalStatus.Missing, 0),
            ["validation.continuous-integration"] = new(SignalStatus.Missing, 0),
            ["portfolio.topics"] = new(SignalStatus.Missing, 0),
            ["validation.test-structure"] = new(SignalStatus.UnavailableEvidence, null),
            ["documentation.installation-guide"] = new(SignalStatus.RequiresHumanReview, null),
            ["process.commit-distribution"] = new(SignalStatus.NotApplicable, null)
        });

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), results);

        Assert.Equal(
            [
                "validation.continuous-integration",
                "repository.readme.exists",
                "process.issue-usage"
            ],
            analysis.CriticalGaps.Select(highlight => highlight.SignalKey));
        Assert.Equal(3, analysis.CriticalGaps.Count);
        Assert.DoesNotContain(
            analysis.CriticalGaps,
            highlight => highlight.SignalKey is
                "validation.test-structure"
                or "documentation.installation-guide"
                or "process.commit-distribution");
    }

    [Fact]
    public void Compose_FillsRemainingGapSlotsFromRankedPartialSignals()
    {
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["repository.readme.exists"] = new(SignalStatus.Missing, 0),
            ["process.commit-distribution"] = new(SignalStatus.Partial, 1),
            ["validation.test-structure"] = new(SignalStatus.Partial, 1),
            ["portfolio.topics"] = new(SignalStatus.Partial, 0)
        });

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), results);

        Assert.Equal(
            [
                "repository.readme.exists",
                "validation.test-structure",
                "process.commit-distribution"
            ],
            analysis.CriticalGaps.Select(highlight => highlight.SignalKey));
    }

    [Fact]
    public void Compose_CreatesStableNonOverlappingActionsOnlyForSelectedRealGaps()
    {
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["documentation.installation-guide"] = new(SignalStatus.Missing, 0),
            ["process.commit-distribution"] = new(SignalStatus.Missing, 0),
            ["validation.continuous-integration"] = new(SignalStatus.Missing, 0),
            ["portfolio.presentation-completeness"] = new(SignalStatus.Missing, 0)
        });

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), results);

        Assert.InRange(analysis.ImprovementPlan.Actions.Count, 0, 3);
        Assert.Equal(
            Enumerable.Range(1, analysis.ImprovementPlan.Actions.Count),
            analysis.ImprovementPlan.Actions.Select(action => action.Priority));
        Assert.Equal(
            analysis.ImprovementPlan.Actions.Count,
            analysis.ImprovementPlan.Actions.Select(action => action.PresentationKey).Distinct().Count());
        Assert.All(
            analysis.ImprovementPlan.Actions,
            action =>
            {
                string relatedSignal = Assert.Single(action.RelatedSignals);
                Assert.Contains(analysis.CriticalGaps, gap => gap.SignalKey == relatedSignal);
                Assert.Equal($"action.improve.{relatedSignal}", action.PresentationKey);
                Assert.NotEqual("portfolio.presentation-completeness", relatedSignal);
            });
    }

    [Fact]
    public void Compose_WithSameExplicitInputs_ProducesTheSameAnalysis()
    {
        RepositorySnapshot snapshot = CreateSnapshot(["commits partial"]);
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["repository.readme.exists"] = new(SignalStatus.Missing, 0),
            ["validation.continuous-integration"] = new(SignalStatus.UnavailableEvidence, null)
        });

        RepositoryAnalysis first = Compose(snapshot, results);
        RepositoryAnalysis second = Compose(snapshot, results.Reverse());

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
    }

    [Fact]
    public void Compose_HasNoLocaleInputAndLocalizedPresentationDoesNotChangeTruth()
    {
        RepositoryAnalysisComposer composer = new();
        Assert.DoesNotContain(
            typeof(RepositoryAnalysisComposer).GetMethod(nameof(RepositoryAnalysisComposer.Compose))!.GetParameters(),
            parameter => parameter.ParameterType == typeof(CultureInfo)
                || parameter.Name!.Contains("locale", StringComparison.OrdinalIgnoreCase));

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), CreateResults());
        SignalResult readme = analysis.SignalResults[0];
        SignalPresentationLocalizer localizer = new();
        SignalPresentation english = localizer.Present(readme, "en");
        SignalPresentation turkish = localizer.Present(readme, "tr");

        Assert.NotEqual(english.StatusLabel, turkish.StatusLabel);
        Assert.Same(readme, analysis.SignalResults[0]);
        Assert.Equal(86, analysis.ScoreBreakdown.EarnedScore);
        Assert.Equal(SignalStatus.Present, readme.Status);
        Assert.Equal("src/Örnek.cs --flag", readme.Evidence[0].Value);
    }

    [Fact]
    public void Compose_EmitsStableWarningsAndLimitationsForExplicitUncertainty()
    {
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["process.issue-usage"] = new(SignalStatus.NotApplicable, null),
            ["validation.test-structure"] = new(SignalStatus.UnavailableEvidence, null),
            ["validation.continuous-integration"] = new(SignalStatus.RequiresHumanReview, null)
        });

        RepositoryAnalysis analysis = Compose(CreateSnapshot(["API pagination incomplete"]), results);

        Assert.Equal(AnalysisStatus.Partial, analysis.Status);
        Assert.Contains("limitation.snapshot.collection-partial", analysis.LimitationKeys);
        Assert.Contains("limitation.analysis.not-applicable-signals", analysis.LimitationKeys);
        Assert.Contains("limitation.analysis.unavailable-evidence", analysis.LimitationKeys);
        Assert.Contains("limitation.analysis.requires-human-review", analysis.LimitationKeys);
        Assert.Contains(analysis.Warnings, warning => warning.PresentationKey == "warning.signal.not-applicable");
        Assert.Contains(analysis.Warnings, warning => warning.PresentationKey == "warning.signal.unavailable-evidence");
        Assert.Contains(analysis.Warnings, warning => warning.PresentationKey == "warning.signal.requires-human-review");
    }

    [Fact]
    public void Compose_WhenSignalKeyIsDuplicateMissingOrUnknown_FailsFast()
    {
        IReadOnlyList<SignalResult> canonical = CreateResults();
        RepositoryAnalysisComposer composer = new();

        ArgumentException duplicate = Assert.Throws<ArgumentException>(() =>
            Compose(CreateSnapshot(), canonical.Append(canonical[0])));
        ArgumentException missing = Assert.Throws<ArgumentException>(() =>
            Compose(CreateSnapshot(), canonical.Skip(1)));
        SignalResult unknown = new(
            "unknown.signal",
            1,
            AnalysisCategory.RepositoryUnderstandability,
            SignalStatus.Present,
            1,
            1,
            true,
            [CreateEvidence("unknown.signal")],
            EvaluatedAt);
        ArgumentException unknownException = Assert.Throws<ArgumentException>(() =>
            composer.Compose(
                CreateSnapshot(),
                canonical.Append(unknown),
                AnalysisId,
                Version,
                StartedAt,
                CompletedAt));

        Assert.Contains("Duplicate", duplicate.Message);
        Assert.Contains("Missing", missing.Message);
        Assert.Contains("Unknown", unknownException.Message);
    }

    [Fact]
    public void Compose_WhenCanonicalMetadataDoesNotMatchDefinition_FailsFast()
    {
        SignalSpec readme = CanonicalSpecs[0];
        IReadOnlyList<SignalResult> canonical = CreateResults();

        SignalResult[] mismatches =
        [
            CreateResult(readme with { Category = AnalysisCategory.PortfolioPresentation }, new(SignalStatus.Present, 5)),
            CreateResult(readme with { MaximumScore = 6 }, new(SignalStatus.Present, 5)),
            CreateResult(readme with { IncludeInOverallScore = false }, new(SignalStatus.Present, 5)),
            CreateResult(readme with { Version = 2 }, new(SignalStatus.Present, 5))
        ];

        foreach (SignalResult mismatch in mismatches)
        {
            IReadOnlyList<SignalResult> input = canonical
                .Select(result => result.SignalKey == readme.Key ? mismatch : result)
                .ToArray();

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                Compose(CreateSnapshot(), input));

            Assert.Contains("canonical definition metadata", exception.Message);
        }
    }

    [Fact]
    public void Compose_ProducesOnlyResolvableEvidenceReferences()
    {
        IReadOnlyList<SignalResult> results = CreateResults(new Dictionary<string, ResultState>
        {
            ["repository.readme.exists"] = new(SignalStatus.Missing, 0),
            ["process.commit-history"] = new(SignalStatus.Strong, 7),
            ["validation.test-structure"] = new(SignalStatus.Strong, 10)
        });

        RepositoryAnalysis analysis = Compose(CreateSnapshot(), results);
        IReadOnlyDictionary<string, SignalResult> byKey = analysis.SignalResults.ToDictionary(
            result => result.SignalKey,
            StringComparer.Ordinal);

        Assert.All(
            analysis.Strengths.Concat(analysis.CriticalGaps),
            highlight => Assert.All(
                highlight.EvidenceReferences,
                reference => Assert.Contains(byKey[highlight.SignalKey].Evidence, evidence => evidence.Key == reference)));
        Assert.All(
            analysis.ImprovementPlan.Actions,
            action => Assert.All(
                action.EvidenceReferences,
                reference => Assert.Contains(
                    action.RelatedSignals.SelectMany(signal => byKey[signal].Evidence),
                    evidence => evidence.Key == reference)));
    }

    [Fact]
    public void Compose_AcceptsTheCanonicalResultsProducedByAllProductionEvaluators()
    {
        TimeProvider timeProvider = TimeProvider.System;
        ISignalEvaluator[] evaluators =
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
        RepositorySignalAnalyzer analyzer = new(
            evaluators,
            new PresentationCompletenessSignalEvaluator(timeProvider));
        RepositorySnapshot snapshot = CreateSnapshot();

        RepositoryAnalysis analysis = Compose(snapshot, analyzer.Analyze(snapshot));

        Assert.Equal(15, analysis.SignalResults.Count);
        Assert.Equal(4, analysis.CategoryResults.Count);
    }

    private static RepositoryAnalysis Compose(
        RepositorySnapshot snapshot,
        IEnumerable<SignalResult> results) =>
        new RepositoryAnalysisComposer().Compose(
            snapshot,
            results,
            AnalysisId,
            Version,
            StartedAt,
            CompletedAt);

    private static IReadOnlyList<SignalResult> CreateResults(
        IReadOnlyDictionary<string, ResultState>? overrides = null,
        ResultState? defaultState = null)
    {
        ResultState fallback = defaultState ?? new ResultState(SignalStatus.Present, null);

        return CanonicalSpecs
            .Select(spec => CreateResult(
                spec,
                overrides is not null && overrides.TryGetValue(spec.Key, out ResultState? state)
                    ? state
                    : fallback))
            .ToArray();
    }

    private static SignalResult CreateResult(SignalSpec spec, ResultState state)
    {
        int? score = state.Status switch
        {
            SignalStatus.Missing => 0,
            SignalStatus.NotApplicable
                or SignalStatus.UnavailableEvidence
                or SignalStatus.RequiresHumanReview => null,
            _ => state.Score ?? spec.MaximumScore
        };
        IEnumerable<EvidenceItem>? evidence = state.HasEvidence
            ? [CreateEvidence(spec.Key)]
            : null;

        return new SignalResult(
            spec.Key,
            spec.Version,
            spec.Category,
            state.Status,
            score,
            spec.MaximumScore,
            spec.IncludeInOverallScore,
            evidence,
            EvaluatedAt);
    }

    private static EvidenceItem CreateEvidence(string signalKey) =>
        new(
            $"evidence.{signalKey}",
            "repository.fixture",
            "src/Örnek.cs --flag",
            "src/Örnek.cs",
            EvaluatedAt,
            EvidenceConfidence.High,
            notes: null);

    private static RepositorySnapshot CreateSnapshot(IEnumerable<string>? warnings = null)
    {
        RepositoryIdentity repository = new(
            "octocat",
            "Örnek Repo",
            new Uri("https://github.com/octocat/ornek-repo"),
            "main",
            isPublic: true,
            isArchived: false,
            isFork: false,
            isTemplate: false,
            primaryLanguage: "C#");

        return new RepositorySnapshot(
            repository,
            CapturedAt,
            "ABC123",
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
            collectionWarnings: warnings);
    }

    private sealed record SignalSpec(
        string Key,
        AnalysisCategory Category,
        int MaximumScore,
        bool IncludeInOverallScore,
        int Version = 1);

    private sealed record ResultState(
        SignalStatus Status,
        int? Score,
        bool HasEvidence = true);
}

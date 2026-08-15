using System.Reflection;
using System.Text.Json;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis.Presentation;

public sealed class RepositoryReportProjectorTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("04500000-0000-0000-0000-000000000001");

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-16T00:00:00Z");

    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-16T00:01:00Z");

    private static readonly DateTimeOffset StartedAt =
        DateTimeOffset.Parse("2026-08-16T00:02:00Z");

    private static readonly DateTimeOffset CompletedAt =
        DateTimeOffset.Parse("2026-08-16T00:03:00Z");

    private static readonly AnalysisVersion Version =
        new("0.1.0", "2026.08.16", 1);

    private static readonly IReadOnlyList<SignalSpec> Specs =
    [
        new("repository.readme.exists",
            AnalysisCategory.RepositoryUnderstandability, 5, true),

        new("repository.description.present",
            AnalysisCategory.RepositoryUnderstandability, 4, true),

        new("documentation.installation-guide",
            AnalysisCategory.RepositoryUnderstandability, 8, true),

        new("repository.license.present",
            AnalysisCategory.RepositoryUnderstandability, 3, true),

        new("documentation.usage-or-demo",
            AnalysisCategory.RepositoryUnderstandability, 7, true),

        new("process.commit-history",
            AnalysisCategory.DevelopmentProcessVisibility, 7, true),

        new("process.commit-distribution",
            AnalysisCategory.DevelopmentProcessVisibility, 8, true),

        new("process.issue-usage",
            AnalysisCategory.DevelopmentProcessVisibility, 5, true),

        new("process.pull-request-usage",
            AnalysisCategory.DevelopmentProcessVisibility, 6, true),

        new("process.release-usage",
            AnalysisCategory.DevelopmentProcessVisibility, 6, true),

        new("validation.test-structure",
            AnalysisCategory.ValidationAndUsability, 10, true),

        new("validation.continuous-integration",
            AnalysisCategory.ValidationAndUsability, 8, true),

        new("portfolio.topics",
            AnalysisCategory.PortfolioPresentation, 4, true),

        new("portfolio.homepage-or-live-demo",
            AnalysisCategory.PortfolioPresentation, 5, true),

        new("portfolio.presentation-completeness",
            AnalysisCategory.PortfolioPresentation, 5, false)
    ];

    private readonly RepositoryReportProjector _projector = new();

    [Fact]
    public void SameAnalysis_ProjectsEnglishAndTurkishWithoutChangingSemanticTruth()
    {
        RepositoryAnalysis analysis = Compose(
            new Dictionary<string, ResultState>
            {
                ["documentation.installation-guide"] =
                    new(SignalStatus.Missing, 0),

                ["process.commit-distribution"] =
                    new(SignalStatus.Strong, 8),

                ["validation.continuous-integration"] =
                    new(SignalStatus.UnavailableEvidence, null),

                ["portfolio.homepage-or-live-demo"] =
                    new(SignalStatus.RequiresHumanReview, null),

                ["portfolio.topics"] =
                    new(SignalStatus.NotApplicable, null)
            });

        string before = JsonSerializer.Serialize(analysis);

        RepositoryReport english =
            _projector.Project(analysis, "en-US");

        RepositoryReport turkish =
            _projector.Project(analysis, "tr-TR");

        string after = JsonSerializer.Serialize(analysis);

        Assert.Equal(before, after);

        Assert.Equal("en", english.Locale);
        Assert.Equal("tr", turkish.Locale);

        Assert.Equal(
            RepositoryReportSectionKeys.CanonicalOrder,
            english.SectionOrder);

        Assert.Equal(
            RepositoryReportSectionKeys.CanonicalOrder,
            turkish.SectionOrder);

        Assert.Equal(
            "What this repository proves",
            english.Proves.Title);

        Assert.Equal(
            "Bu repository neyi kanıtlıyor?",
            turkish.Proves.Title);

        Assert.Equal(
            analysis.Repository.FullName,
            english.Identity.FullName);

        Assert.Equal(
            analysis.Repository.FullName,
            turkish.Identity.FullName);

        Assert.Equal(
            analysis.Repository.Url,
            english.Identity.Url);

        Assert.Equal(
            analysis.Repository.Url,
            turkish.Identity.Url);

        Assert.Equal(
            analysis.ScoreBreakdown.NormalizedScore,
            english.ScoreSummary.NormalizedScore);

        Assert.Equal(
            english.ScoreSummary.NormalizedScore,
            turkish.ScoreSummary.NormalizedScore);

        Assert.Equal(
            analysis.Strengths.Select(x => x.SignalKey),
            english.Proves.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.Strengths.Select(x => x.SignalKey),
            turkish.Proves.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.CriticalGaps.Select(x => x.SignalKey),
            english.Weakens.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.CriticalGaps.Select(x => x.SignalKey),
            turkish.Weakens.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.ImprovementPlan.Actions.Select(x => x.PresentationKey),
            english.DoFirst.Items.Select(x => x.PresentationKey));

        Assert.Equal(
            analysis.ImprovementPlan.Actions.Select(x => x.PresentationKey),
            turkish.DoFirst.Items.Select(x => x.PresentationKey));

        Assert.Equal(
            analysis.SignalResults.Select(x => x.SignalKey),
            english.SignalDetails.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.SignalResults.Select(x => x.SignalKey),
            turkish.SignalDetails.Items.Select(x => x.SignalKey));

        Assert.NotEqual(
            english.Disclaimer.Text,
            turkish.Disclaimer.Text);
    }

    [Fact]
    public void UnknownLocale_FallsBackToEnglish()
    {
        RepositoryAnalysis analysis = Compose();

        RepositoryReport fallback =
            _projector.Project(analysis, "de-DE");

        RepositoryReport english =
            _projector.Project(analysis, "en");

        Assert.Equal("en", fallback.Locale);

        Assert.Equal(
            english.Proves.Title,
            fallback.Proves.Title);

        Assert.Equal(
            english.ScoreSummary.Label,
            fallback.ScoreSummary.Label);

        Assert.Equal(
            english.Disclaimer.Text,
            fallback.Disclaimer.Text);

        Assert.Equal(
            english.SignalDetails.Items.Select(x => x.Title),
            fallback.SignalDetails.Items.Select(x => x.Title));
    }

    [Fact]
    public void ExplicitUncertainty_ContainsOnlyUncertaintyStatuses()
    {
        RepositoryAnalysis analysis = Compose(
            new Dictionary<string, ResultState>
            {
                ["repository.readme.exists"] =
                    new(SignalStatus.Missing, 0),

                ["process.issue-usage"] =
                    new(SignalStatus.NotApplicable, null),

                ["validation.test-structure"] =
                    new(SignalStatus.UnavailableEvidence, null),

                ["validation.continuous-integration"] =
                    new(SignalStatus.RequiresHumanReview, null)
            });

        RepositoryReport report =
            _projector.Project(analysis, "en");

        Assert.Equal(
            [
                "process.issue-usage",
                "validation.test-structure",
                "validation.continuous-integration"
            ],
            report.Uncertainty.Items.Select(x => x.SignalKey));

        Assert.All(
            report.Uncertainty.Items,
            item => Assert.Contains(
                item.Status,
                new[]
                {
                    SignalStatus.NotApplicable,
                    SignalStatus.UnavailableEvidence,
                    SignalStatus.RequiresHumanReview
                }));

        Assert.DoesNotContain(
            report.Uncertainty.Items,
            item => item.SignalKey == "repository.readme.exists");

        Assert.Contains(
            report.Weakens.Items,
            item => item.SignalKey == "repository.readme.exists");
    }

    [Fact]
    public void RepositoryAndEvidenceLiterals_ArePreservedAcrossLocales()
    {
        RepositoryAnalysis analysis = Compose();

        RepositoryReport english =
            _projector.Project(analysis, "en");

        RepositoryReport turkish =
            _projector.Project(analysis, "tr");

        Assert.Equal(
            "fatihGunalp/Örnek Repo",
            english.Identity.FullName);

        Assert.Equal(
            english.Identity.FullName,
            turkish.Identity.FullName);

        Assert.Equal(
            "feature/kanıt",
            english.Identity.DefaultBranch);

        Assert.Equal(
            english.Identity.DefaultBranch,
            turkish.Identity.DefaultBranch);

        Assert.Equal(
            "C#",
            english.Identity.PrimaryLanguage);

        Assert.Equal(
            english.Identity.PrimaryLanguage,
            turkish.Identity.PrimaryLanguage);

        SignalPresentation enSignal =
            english.SignalDetails.Items[0];

        SignalPresentation trSignal =
            turkish.SignalDetails.Items[0];

        EvidenceItem enEvidence =
            Assert.Single(enSignal.Evidence);

        EvidenceItem trEvidence =
            Assert.Single(trSignal.Evidence);

        Assert.Equal(enEvidence, trEvidence);

        Assert.Equal(
            "dotnet test ./Örnek.sln --filter Smoke",
            enEvidence.Value);

        Assert.Equal(
            "docs/Çalıştırma Rehberi.md#quick-start",
            enEvidence.Location);

        Assert.Contains(
            "https://example.test/demo?ref=Örnek",
            enEvidence.Notes);
    }

    [Fact]
    public void Projection_PreservesComposerSelectionAndMaximumThreeRule()
    {
        RepositoryAnalysis analysis = Compose(
            new Dictionary<string, ResultState>
            {
                ["repository.readme.exists"] =
                    new(SignalStatus.Missing, 0),

                ["documentation.installation-guide"] =
                    new(SignalStatus.Missing, 0),

                ["process.commit-distribution"] =
                    new(SignalStatus.Missing, 0),

                ["validation.continuous-integration"] =
                    new(SignalStatus.Missing, 0),

                ["portfolio.topics"] =
                    new(SignalStatus.Missing, 0)
            });

        RepositoryReport report =
            _projector.Project(analysis, "en");

        Assert.InRange(report.Proves.Items.Count, 0, 3);
        Assert.InRange(report.Weakens.Items.Count, 0, 3);
        Assert.InRange(report.DoFirst.Items.Count, 0, 3);

        Assert.Equal(
            analysis.Strengths.Select(x => x.SignalKey),
            report.Proves.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.CriticalGaps.Select(x => x.SignalKey),
            report.Weakens.Items.Select(x => x.SignalKey));

        Assert.Equal(
            analysis.ImprovementPlan.Actions.Select(x => x.PresentationKey),
            report.DoFirst.Items.Select(x => x.PresentationKey));

        Assert.Equal(
            analysis.ImprovementPlan.Actions.Select(x => x.Priority),
            report.DoFirst.Items.Select(x => x.Priority));
    }

    [Fact]
    public void ProductBoundaryDisclaimer_IsVersionedAndCompleteInBothLocales()
    {
        RepositoryAnalysis analysis = Compose();

        RepositoryReport english =
            _projector.Project(analysis, "en");

        RepositoryReport turkish =
            _projector.Project(analysis, "tr");

        Assert.Equal(
            RepositoryReportLocalizer.ProductBoundaryPolicyVersion,
            english.Disclaimer.PolicyVersion);

        Assert.Equal(
            english.Disclaimer.PolicyVersion,
            turkish.Disclaimer.PolicyVersion);

        Assert.Contains(
            "seniority",
            english.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "employability",
            english.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "authorship",
            english.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "originality",
            english.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "AI usage",
            english.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "overall developer quality",
            english.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "kıdemi",
            turkish.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "işe alınabilirlik",
            turkish.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "yazarlık",
            turkish.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "özgünlük",
            turkish.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "AI kullanımı",
            turkish.Disclaimer.Text,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReportLocalizationCatalog_HasRequiredEnglishAndTurkishCopy()
    {
        RepositoryReportLocalizer localizer = new();

        foreach (string sectionKey in
                 RepositoryReportSectionKeys.CanonicalOrder)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetSectionTitle(sectionKey, "en")));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetSectionTitle(sectionKey, "tr")));
        }

        foreach (AnalysisStatus status in
                 Enum.GetValues<AnalysisStatus>())
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetAnalysisStatusLabel(status, "en")));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetAnalysisStatusLabel(status, "tr")));
        }

        foreach (CategoryStatus status in
                 Enum.GetValues<CategoryStatus>())
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetCategoryStatusLabel(status, "en")));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetCategoryStatusLabel(status, "tr")));
        }

        foreach (EffortLevel effort in
                 Enum.GetValues<EffortLevel>())
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetEffortLabel(effort, "en")));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetEffortLabel(effort, "tr")));
        }

        string[] scoreKeys =
        [
            "score.label.limited-visible-evidence",
            "score.label.basic-visible-evidence",
            "score.label.developing-visible-evidence",
            "score.label.strong-visible-evidence",
            "score.label.very-strong-visible-evidence"
        ];

        foreach (string scoreKey in scoreKeys)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetScoreLabel(scoreKey, "en")));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    localizer.GetScoreLabel(scoreKey, "tr")));
        }

        SignalPresentationLocalizer signalLocalizer = new();

        Assert.Equal(
            "Portfolio Presentation",
            signalLocalizer.GetCategoryLabel(
                AnalysisCategory.PortfolioPresentation,
                "en"));

        Assert.Equal(
            "Portföy Sunumu",
            signalLocalizer.GetCategoryLabel(
                AnalysisCategory.PortfolioPresentation,
                "tr"));
    }

    [Fact]
    public void Projector_DependsOnCompletedAnalysisNotAnalyzerOrComposer()
    {
        MethodInfo project =
            typeof(RepositoryReportProjector)
                .GetMethod(nameof(RepositoryReportProjector.Project))!;

        ParameterInfo[] parameters =
            project.GetParameters();

        Assert.Equal(2, parameters.Length);

        Assert.Equal(
            typeof(RepositoryAnalysis),
            parameters[0].ParameterType);

        Assert.Equal(
            typeof(string),
            parameters[1].ParameterType);

        Assert.DoesNotContain(
            typeof(RepositoryReportProjector).GetConstructors()
                .SelectMany(constructor => constructor.GetParameters()),
            parameter =>
                parameter.ParameterType == typeof(RepositoryAnalysisComposer)
                || parameter.ParameterType.Name.Contains(
                    "RepositorySignalAnalyzer",
                    StringComparison.Ordinal));
    }

    private static RepositoryAnalysis Compose(
        IReadOnlyDictionary<string, ResultState>? overrides = null)
    {
        RepositorySnapshot snapshot = CreateSnapshot();

        IReadOnlyList<SignalResult> results =
            Specs.Select(spec =>
            {
                ResultState state =
                    overrides is not null
                    && overrides.TryGetValue(spec.Key, out ResultState? configured)
                    && configured is not null
                        ? configured
                        : new ResultState(
                            SignalStatus.Present,
                            spec.MaximumScore);

                return new SignalResult(
                    spec.Key,
                    signalVersion: 1,
                    spec.Category,
                    state.Status,
                    state.Score,
                    spec.MaximumScore,
                    spec.IncludeInOverallScore,
                    [CreateEvidence(spec.Key)],
                    EvaluatedAt);
            }).ToArray();

        RepositoryAnalysisComposer composer = new();

        return composer.Compose(
            snapshot,
            results,
            AnalysisId,
            Version,
            StartedAt,
            CompletedAt);
    }

    private static RepositorySnapshot CreateSnapshot()
    {
        RepositoryIdentity repository = new(
            owner: "fatihGunalp",
            name: "Örnek Repo",
            url: new Uri(
                "https://github.com/fatihGunalp/%C3%96rnek-Repo"),
            defaultBranch: "feature/kanıt",
            isPublic: true,
            isArchived: false,
            isFork: false,
            isTemplate: false,
            primaryLanguage: "C#");

        return new RepositorySnapshot(
            repository,
            CapturedAt,
            headCommitSha:
                "0123456789abcdef0123456789abcdef01234567",
            metadata: null,
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: ["dotnet", "kanıt"],
            license: null,
            languages:
                new Dictionary<string, long>
                {
                    ["C#"] = 12345
                },
            contributors: null,
            availability: null,
            collectionWarnings: null,
            tags: null,
            licenseFile: null,
            documentationMarkdown: null);
    }

    private static EvidenceItem CreateEvidence(
        string signalKey) =>
        new(
            key: $"evidence.{signalKey}",
            source: "Command",
            value: "dotnet test ./Örnek.sln --filter Smoke",
            location:
                "docs/Çalıştırma Rehberi.md#quick-start",
            observedAt: EvaluatedAt,
            confidence: EvidenceConfidence.High,
            notes:
                "Source URL: https://example.test/demo?ref=Örnek");

    private sealed record SignalSpec(
        string Key,
        AnalysisCategory Category,
        int MaximumScore,
        bool IncludeInOverallScore);

    private sealed record ResultState(
        SignalStatus Status,
        int? Score);
}
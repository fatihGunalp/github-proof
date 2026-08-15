using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis.Presentation;

public sealed class SignalPresentationLocalizerTests
{
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");

    private readonly SignalPresentationLocalizer _localizer = new();

    [Fact]
    public void SameSemanticResult_ProducesEnglishAndTurkishPresentation()
    {
        SignalResult result = CreateResult();

        SignalPresentation english = _localizer.Present(result, "en");
        SignalPresentation turkish = _localizer.Present(result, "tr");

        Assert.Equal("en", english.Locale);
        Assert.Equal("Repository description", english.Title);
        Assert.Equal("Missing", english.StatusLabel);
        Assert.Equal("tr", turkish.Locale);
        Assert.Equal("Repository açıklaması", turkish.Title);
        Assert.Equal("Eksik", turkish.StatusLabel);
        Assert.NotEqual(english.Explanation, turkish.Explanation);
        Assert.NotEqual(english.Recommendation, turkish.Recommendation);
    }

    [Fact]
    public void LocaleSwitch_DoesNotRerunOrChangeSemanticResult()
    {
        SignalResult result = CreateResult();
        SignalResult before = result;

        SignalPresentation english = _localizer.Present(result, "en-US");
        SignalPresentation turkish = _localizer.Present(result, "tr-TR");

        Assert.Same(before, result);
        Assert.Equal(result.SignalKey, english.SignalKey);
        Assert.Equal(result.SignalKey, turkish.SignalKey);
        Assert.Equal(result.Status, english.Status);
        Assert.Equal(result.Status, turkish.Status);
        Assert.Equal(result.Score, english.Score);
        Assert.Equal(result.Score, turkish.Score);
        Assert.Same(result.Evidence, english.Evidence);
        Assert.Same(result.Evidence, turkish.Evidence);
    }

    [Fact]
    public void UnknownLocale_FallsBackToEnglish()
    {
        SignalResult result = CreateResult();

        SignalPresentation fallback = _localizer.Present(result, "de-DE");
        SignalPresentation english = _localizer.Present(result, "en");

        Assert.Equal("en", fallback.Locale);
        Assert.Equal(english.Title, fallback.Title);
        Assert.Equal(english.Description, fallback.Description);
        Assert.Equal(english.StatusLabel, fallback.StatusLabel);
        Assert.Equal(english.Explanation, fallback.Explanation);
        Assert.Equal(english.Recommendation, fallback.Recommendation);
        Assert.Equal(english.Limitations, fallback.Limitations);
    }

    [Fact]
    public void RepositoryLiteralValues_RemainUnchangedAcrossLocales()
    {
        SignalResult result = CreateResult();

        SignalPresentation english = _localizer.Present(result, "en");
        SignalPresentation turkish = _localizer.Present(result, "tr");

        EvidenceItem expected = Assert.Single(result.Evidence);
        Assert.Equal(expected, Assert.Single(english.Evidence));
        Assert.Equal(expected, Assert.Single(turkish.Evidence));
        Assert.Equal("dotnet test ./Acme.sln --filter Smoke", expected.Value);
        Assert.Equal("docs/Çalıştırma Rehberi.md#quick-start", expected.Location);
        Assert.Contains("https://example.test/demo?ref=Örnek", expected.Notes);
    }

    [Fact]
    public void CanonicalFifteenSignals_HaveEnglishAndTurkishResources()
    {
        string[] signalKeys =
        [
            "repository.readme.exists",
            "repository.description.present",
            "repository.license.present",
            "documentation.installation-guide",
            "documentation.usage-or-demo",
            "process.commit-history",
            "process.commit-distribution",
            "process.issue-usage",
            "process.pull-request-usage",
            "process.release-usage",
            "validation.test-structure",
            "validation.continuous-integration",
            "portfolio.topics",
            "portfolio.homepage-or-live-demo",
            "portfolio.presentation-completeness"
        ];

        foreach (string signalKey in signalKeys)
        {
            SignalResult result = CreateResult(signalKey);

            Assert.False(string.IsNullOrWhiteSpace(_localizer.Present(result, "en").Title));
            Assert.False(string.IsNullOrWhiteSpace(_localizer.Present(result, "tr").Title));
        }

        Assert.Equal(15, signalKeys.Length);
    }

    [Fact]
    public void CategoryAndProductBoundaryCopy_AreLocalized()
    {
        Assert.Equal(
            "Documentation",
            _localizer.GetCategoryLabel(AnalysisCategory.RepositoryUnderstandability, "en"));
        Assert.Equal(
            "Dokümantasyon",
            _localizer.GetCategoryLabel(AnalysisCategory.RepositoryUnderstandability, "tr"));
        Assert.Contains("seniority", _localizer.GetProductBoundaryDisclaimer("en"));
        Assert.Contains("kıdemi", _localizer.GetProductBoundaryDisclaimer("tr"));
    }

    [Theory]
    [InlineData(SignalStatus.NotApplicable)]
    [InlineData(SignalStatus.UnavailableEvidence)]
    public void NonRemediationUncertainty_DoesNotBecomeAMissingRecommendation(
        SignalStatus status)
    {
        SignalResult result = CreateResult(status: status, score: null);

        SignalPresentation presentation = _localizer.Present(result, "en");

        Assert.Equal(status, presentation.Status);
        Assert.Null(presentation.Recommendation);
        Assert.NotEmpty(presentation.Limitations);
    }

    private static SignalResult CreateResult(
        string signalKey = "repository.description.present",
        SignalStatus status = SignalStatus.Missing,
        int? score = 0) =>
        new(
            signalKey,
            signalVersion: 1,
            AnalysisCategory.RepositoryUnderstandability,
            status,
            score,
            maximumScore: 5,
            includeInOverallScore: true,
            evidence:
            [
                new EvidenceItem(
                    "repository.literal",
                    "Command",
                    "dotnet test ./Acme.sln --filter Smoke",
                    "docs/Çalıştırma Rehberi.md#quick-start",
                    EvaluatedAt,
                    EvidenceConfidence.High,
                    "Source URL: https://example.test/demo?ref=Örnek")
            ],
            EvaluatedAt);
}

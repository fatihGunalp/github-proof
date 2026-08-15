using System.Reflection;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis.Presentation;

public sealed class RepositoryReportMarkdownRendererTests
{
    private static readonly DateTimeOffset ObservedAt =
        DateTimeOffset.Parse("2026-08-16T00:30:00Z");

    private readonly RepositoryReportMarkdownRenderer _renderer = new();

    [Fact]
    public void SameRepositoryReport_ProducesByteForByteSameLfMarkdown()
    {
        RepositoryReport report = CreateReport("en");

        string first = _renderer.Render(report);
        string second = _renderer.Render(report);

        Assert.Equal(first, second);
        Assert.DoesNotContain("\r", first);
        Assert.EndsWith("\n", first, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_UsesExactCanonicalEvidenceFirstSectionOrder()
    {
        RepositoryReport report = CreateReport("en");

        string markdown = _renderer.Render(report);

        string[] titles =
        [
            report.Identity.SectionTitle,
            report.Proves.Title,
            report.Weakens.Title,
            report.DoFirst.Title,
            report.Uncertainty.Title,
            report.ScoreSummary.SectionTitle,
            report.SignalDetails.Title,
            report.Disclaimer.SectionTitle
        ];

        int previous = -1;

        foreach (string title in titles)
        {
            int current = markdown.IndexOf(
                $"## {title}\n",
                StringComparison.Ordinal);

            Assert.True(
                current > previous,
                $"Section '{title}' was not rendered in canonical order.");

            previous = current;
        }
    }

    [Fact]
    public void Render_AcceptsEnglishAndTurkishLocalizedReportsWithoutLocalizing()
    {
        RepositoryReport english = CreateReport("en");
        RepositoryReport turkish = CreateReport("tr");

        string englishMarkdown = _renderer.Render(english);
        string turkishMarkdown = _renderer.Render(turkish);

        Assert.Contains(
            "## What this repository proves",
            englishMarkdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "## Bu repository neyi kanıtlıyor?",
            turkishMarkdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "## What this report does not claim",
            englishMarkdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "## Bu rapor ne iddia etmez?",
            turkishMarkdown,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Render_PreservesStrengthGapAndActionOrderFromReport()
    {
        RepositoryReport report = CreateReport("en");

        string markdown = _renderer.Render(report);

        AssertOrdered(
            markdown,
            "### Commit history",
            "### Automated tests",
            "### Usage demo");

        AssertOrdered(
            markdown,
            "### Missing installation",
            "### Missing CI",
            "### Missing release");

        AssertOrdered(
            markdown,
            "### 1. Document installation",
            "### 2. Add CI",
            "### 3. Publish release");
    }

    [Fact]
    public void Render_KeepsMissingGapSeparateFromExplicitUncertainty()
    {
        RepositoryReport report = CreateReport("en");

        string markdown = _renderer.Render(report);

        int weakensStart = markdown.IndexOf(
            $"## {report.Weakens.Title}",
            StringComparison.Ordinal);

        int doFirstStart = markdown.IndexOf(
            $"## {report.DoFirst.Title}",
            StringComparison.Ordinal);

        string weakens = markdown[weakensStart..doFirstStart];

        int uncertaintyStart = markdown.IndexOf(
            $"## {report.Uncertainty.Title}",
            StringComparison.Ordinal);

        int scoreStart = markdown.IndexOf(
            $"## {report.ScoreSummary.SectionTitle}",
            StringComparison.Ordinal);

        string uncertainty = markdown[uncertaintyStart..scoreStart];

        Assert.Contains(
            "Missing installation",
            weakens,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Evidence unavailable",
            weakens,
            StringComparison.Ordinal);

        Assert.Contains(
            "Evidence unavailable",
            uncertainty,
            StringComparison.Ordinal);

        Assert.Contains(
            "Human review required",
            uncertainty,
            StringComparison.Ordinal);

        Assert.Contains(
            "Not applicable",
            uncertainty,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Render_PreservesRepositoryAndEvidenceLiterals()
    {
        RepositoryReport report = CreateReport("en");

        string markdown = _renderer.Render(report);

        Assert.Contains(
            "fatihGunalp/Örnek Repo",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "https://github.com/fatihGunalp/%C3%96rnek-Repo",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "feature/kanıt",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "0123456789abcdef0123456789abcdef01234567",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "dotnet test ./Örnek.sln --filter Smoke",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "docs/Çalıştırma Rehberi.md#quick-start",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "https://example.test/demo?ref=Örnek",
            markdown,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Render_KeepsSecondaryScoreAfterEvidenceFirstSections()
    {
        RepositoryReport report = CreateReport("en");

        string markdown = _renderer.Render(report);

        int proves = markdown.IndexOf(
            $"## {report.Proves.Title}",
            StringComparison.Ordinal);

        int weakens = markdown.IndexOf(
            $"## {report.Weakens.Title}",
            StringComparison.Ordinal);

        int doFirst = markdown.IndexOf(
            $"## {report.DoFirst.Title}",
            StringComparison.Ordinal);

        int uncertainty = markdown.IndexOf(
            $"## {report.Uncertainty.Title}",
            StringComparison.Ordinal);

        int score = markdown.IndexOf(
            $"## {report.ScoreSummary.SectionTitle}",
            StringComparison.Ordinal);

        int details = markdown.IndexOf(
            $"## {report.SignalDetails.Title}",
            StringComparison.Ordinal);

        Assert.True(proves < score);
        Assert.True(weakens < score);
        Assert.True(doFirst < score);
        Assert.True(uncertainty < score);
        Assert.True(score < details);
    }

    [Fact]
    public void Render_AlwaysIncludesVersionedProductBoundaryDisclaimerAtEnd()
    {
        RepositoryReport report = CreateReport("en");

        string markdown = _renderer.Render(report);

        int disclaimerHeading = markdown.LastIndexOf(
            $"## {report.Disclaimer.SectionTitle}",
            StringComparison.Ordinal);

        int disclaimerText = markdown.LastIndexOf(
            report.Disclaimer.Text,
            StringComparison.Ordinal);

        Assert.True(disclaimerHeading >= 0);
        Assert.True(disclaimerText > disclaimerHeading);

        Assert.EndsWith(
            $"v{report.Disclaimer.PolicyVersion}\n",
            markdown,
            StringComparison.Ordinal);

        Assert.Contains(
            "seniority",
            markdown,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "employability",
            markdown,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "authorship",
            markdown,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "originality",
            markdown,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "AI usage",
            markdown,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "overall developer quality",
            markdown,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Render_HandlesEmptyOptionalCollectionsDeterministically()
    {
        RepositoryReport report = CreateReport(
            "en",
            includeOptionalCollections: false);

        string first = _renderer.Render(report);
        string second = _renderer.Render(report);

        Assert.Equal(first, second);

        Assert.Contains(
            $"## {report.Proves.Title}",
            first,
            StringComparison.Ordinal);

        Assert.Contains(
            $"## {report.Weakens.Title}",
            first,
            StringComparison.Ordinal);

        Assert.Contains(
            $"## {report.DoFirst.Title}",
            first,
            StringComparison.Ordinal);

        Assert.Contains(
            $"## {report.Uncertainty.Title}",
            first,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "### Missing installation",
            first,
            StringComparison.Ordinal);

        Assert.Contains(
            report.Disclaimer.Text,
            first,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_PublicBoundaryAcceptsOnlyRepositoryReportAndHasNoAnalysisDependencies()
    {
        ConstructorInfo constructor =
            Assert.Single(
                typeof(RepositoryReportMarkdownRenderer).GetConstructors());

        Assert.Empty(constructor.GetParameters());

        MethodInfo[] declaredPublicMethods =
            typeof(RepositoryReportMarkdownRenderer)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly);

        MethodInfo render = Assert.Single(declaredPublicMethods);

        Assert.Equal(
            nameof(RepositoryReportMarkdownRenderer.Render),
            render.Name);

        ParameterInfo parameter =
            Assert.Single(render.GetParameters());

        Assert.Equal(
            typeof(RepositoryReport),
            parameter.ParameterType);

        Assert.Equal(
            typeof(string),
            render.ReturnType);
    }

    private static void AssertOrdered(
        string markdown,
        params string[] values)
    {
        int previous = -1;

        foreach (string value in values)
        {
            int current = markdown.IndexOf(
                value,
                StringComparison.Ordinal);

            Assert.True(
                current > previous,
                $"'{value}' was not rendered in the expected order.");

            previous = current;
        }
    }

    private static RepositoryReport CreateReport(
        string locale,
        bool includeOptionalCollections = true)
    {
        bool turkish = string.Equals(
            locale,
            "tr",
            StringComparison.Ordinal);

        IReadOnlyList<EvidenceItem> evidence =
        [
            new EvidenceItem(
                "evidence.literal",
                "Command",
                "dotnet test ./Örnek.sln --filter Smoke",
                "docs/Çalıştırma Rehberi.md#quick-start",
                ObservedAt,
                EvidenceConfidence.High,
                "Source URL: https://example.test/demo?ref=Örnek")
        ];

        RepositoryReportIdentity identity = new(
            RepositoryReportSectionKeys.Identity,
            turkish ? "Repository bağlamı" : "Repository context",
            Guid.Parse("04600000-0000-0000-0000-000000000001"),
            "fatihGunalp",
            "Örnek Repo",
            "fatihGunalp/Örnek Repo",
            new Uri("https://github.com/fatihGunalp/%C3%96rnek-Repo"),
            "feature/kanıt",
            "0123456789abcdef0123456789abcdef01234567",
            DateTimeOffset.Parse("2026-08-16T00:20:00Z"),
            true,
            false,
            false,
            false,
            "C#",
            AnalysisStatus.Partial,
            turkish ? "Kısmi" : "Partial",
            "0.1.0",
            "2026.08.16",
            1);

        IReadOnlyList<RepositoryReportHighlight> proves =
            includeOptionalCollections
                ?
                [
                    CreateHighlight(
                        "process.commit-history",
                        turkish ? "Commit geçmişi" : "Commit history",
                        SignalStatus.Strong,
                        turkish ? "Güçlü" : "Strong",
                        7,
                        7,
                        evidence),

                    CreateHighlight(
                        "validation.test-structure",
                        turkish ? "Otomatik testler" : "Automated tests",
                        SignalStatus.Strong,
                        turkish ? "Güçlü" : "Strong",
                        10,
                        10,
                        evidence),

                    CreateHighlight(
                        "documentation.usage-or-demo",
                        turkish ? "Kullanım demosu" : "Usage demo",
                        SignalStatus.Present,
                        turkish ? "Mevcut" : "Present",
                        7,
                        7,
                        evidence)
                ]
                : [];

        IReadOnlyList<RepositoryReportHighlight> weakens =
            includeOptionalCollections
                ?
                [
                    CreateGap(
                        "documentation.installation-guide",
                        turkish ? "Eksik kurulum" : "Missing installation",
                        0,
                        8,
                        evidence),

                    CreateGap(
                        "validation.continuous-integration",
                        turkish ? "Eksik CI" : "Missing CI",
                        0,
                        8,
                        evidence),

                    CreateGap(
                        "process.release-usage",
                        turkish ? "Eksik release" : "Missing release",
                        0,
                        6,
                        evidence)
                ]
                : [];

        IReadOnlyList<RepositoryReportAction> actions =
            includeOptionalCollections
                ?
                [
                    new RepositoryReportAction(
                        1,
                        "action.improve.documentation.installation-guide",
                        turkish
                            ? "Kurulumu belgele"
                            : "Document installation",
                        turkish
                            ? "Başka biri projeyi yeniden kurabilmelidir."
                            : "Another person should be able to reproduce the setup.",
                        turkish
                            ? "Kurulum adımlarını README'ye ekleyin."
                            : "Add installation steps to the README.",
                        EffortLevel.Unknown,
                        turkish ? "Tahmin edilmedi" : "Not estimated",
                        ["documentation.installation-guide"],
                        evidence),

                    new RepositoryReportAction(
                        2,
                        "action.improve.validation.continuous-integration",
                        turkish ? "CI ekle" : "Add CI",
                        turkish
                            ? "Tekrarlanabilir doğrulama görünür olmalıdır."
                            : "Repeatable validation should be visible.",
                        turkish
                            ? "Doğrulama komutlarını çalıştıran CI ekleyin."
                            : "Add CI that runs the validation commands.",
                        EffortLevel.Unknown,
                        turkish ? "Tahmin edilmedi" : "Not estimated",
                        ["validation.continuous-integration"],
                        evidence),

                    new RepositoryReportAction(
                        3,
                        "action.improve.process.release-usage",
                        turkish ? "Release yayımla" : "Publish release",
                        turkish
                            ? "Kilometre taşları görünür olmalıdır."
                            : "Project milestones should be visible.",
                        turkish
                            ? "Anlamlı bir release yayımlayın."
                            : "Publish a meaningful release.",
                        EffortLevel.Unknown,
                        turkish ? "Tahmin edilmedi" : "Not estimated",
                        ["process.release-usage"],
                        evidence)
                ]
                : [];

        IReadOnlyList<RepositoryReportUncertainty> uncertainty =
            includeOptionalCollections
                ?
                [
                    new RepositoryReportUncertainty(
                        "process.issue-usage",
                        SignalStatus.NotApplicable,
                        turkish ? "Issue kullanımı" : "Issue usage",
                        turkish ? "Uygulanamaz" : "Not applicable",
                        turkish
                            ? "Bu sinyal repository için uygulanabilir değil."
                            : "This signal does not apply to this repository.",
                        evidence),

                    new RepositoryReportUncertainty(
                        "portfolio.homepage-or-live-demo",
                        SignalStatus.UnavailableEvidence,
                        turkish ? "Canlı demo" : "Live demo",
                        turkish
                            ? "Kanıta erişilemiyor"
                            : "Evidence unavailable",
                        turkish
                            ? "Gerekli kanıta erişilemedi."
                            : "The required evidence was unavailable.",
                        evidence),

                    new RepositoryReportUncertainty(
                        "validation.continuous-integration",
                        SignalStatus.RequiresHumanReview,
                        turkish ? "CI incelemesi" : "CI review",
                        turkish
                            ? "İnsan incelemesi gerekli"
                            : "Human review required",
                        turkish
                            ? "Deterministik sonuç için insan incelemesi gerekiyor."
                            : "Human review is required before a deterministic conclusion.",
                        evidence)
                ]
                : [];

        IReadOnlyList<RepositoryReportCategory> categories =
        [
            new RepositoryReportCategory(
                AnalysisCategory.RepositoryUnderstandability,
                turkish ? "Dokümantasyon" : "Documentation",
                CategoryStatus.Completed,
                turkish ? "Tamamlandı" : "Completed",
                20,
                27,
                74),

            new RepositoryReportCategory(
                AnalysisCategory.DevelopmentProcessVisibility,
                turkish ? "Geliştirme Süreci" : "Development Process",
                CategoryStatus.Completed,
                turkish ? "Tamamlandı" : "Completed",
                26,
                32,
                81),

            new RepositoryReportCategory(
                AnalysisCategory.ValidationAndUsability,
                turkish ? "Doğrulama" : "Validation",
                CategoryStatus.Partial,
                turkish ? "Kısmi" : "Partial",
                10,
                18,
                56),

            new RepositoryReportCategory(
                AnalysisCategory.PortfolioPresentation,
                turkish ? "Portföy Sunumu" : "Portfolio Presentation",
                CategoryStatus.Partial,
                turkish ? "Kısmi" : "Partial",
                7,
                14,
                50)
        ];

        RepositoryReportScoreSummary score = new(
            RepositoryReportSectionKeys.CategoriesAndScore,
            turkish ? "Kategori özeti" : "Category summary",
            63,
            86,
            73,
            "score.label.strong-visible-evidence",
            turkish
                ? "Güçlü görünür kanıt"
                : "Strong visible evidence",
            ScoreCalculationMethod.RawApplicableScoreNormalization,
            categories);

        IReadOnlyList<SignalPresentation> details =
        [
            new SignalPresentation(
                locale,
                "process.commit-history",
                turkish ? "Commit geçmişi" : "Commit history detail",
                turkish
                    ? "Commit geçmişini kontrol eder."
                    : "Checks visible commit history.",
                SignalStatus.Strong,
                turkish ? "Güçlü" : "Strong",
                7,
                7,
                turkish
                    ? "Güçlü commit kanıtı var."
                    : "Strong commit evidence is visible.",
                turkish
                    ? "Commit geçmişi geliştirme izini gösterir."
                    : "Commit history exposes a development trail.",
                null,
                [
                    turkish
                        ? "Yalnız görünür snapshot kanıtını yansıtır."
                        : "Only visible snapshot evidence is represented."
                ],
                evidence,
                1),

            new SignalPresentation(
                locale,
                "documentation.installation-guide",
                turkish ? "Kurulum rehberi" : "Installation guide",
                turkish
                    ? "Kurulum talimatlarını kontrol eder."
                    : "Checks installation instructions.",
                SignalStatus.Missing,
                turkish ? "Eksik" : "Missing",
                0,
                8,
                turkish
                    ? "Görünür kurulum kanıtı bulunamadı."
                    : "No visible installation evidence was found.",
                turkish
                    ? "Kurulum tekrar üretilebilirliği destekler."
                    : "Installation guidance supports reproducibility.",
                turkish
                    ? "Kurulum adımlarını ekleyin."
                    : "Add installation steps.",
                [
                    turkish
                        ? "Eksik, incelenen kapsamda kanıt bulunmadığı anlamına gelir."
                        : "Missing means no qualifying evidence was found in scope."
                ],
                evidence,
                1)
        ];

        IReadOnlyList<RepositoryReportWarning> warnings =
            includeOptionalCollections
                ?
                [
                    new RepositoryReportWarning(
                        "signal.unavailable.portfolio.homepage-or-live-demo",
                        WarningSeverity.Warning,
                        turkish
                            ? "Bazı gerekli kanıtlara erişilemedi."
                            : "Some required evidence was unavailable.",
                        [],
                        ["portfolio.homepage-or-live-demo"])
                ]
                : [];

        IReadOnlyList<string> limitations =
            includeOptionalCollections
                ?
                [
                    turkish
                        ? "Bazı sinyaller açık belirsizlik içeriyor."
                        : "Some signals contain explicit uncertainty."
                ]
                : [];

        RepositoryReportDisclaimer disclaimer = new(
            RepositoryReportSectionKeys.Disclaimer,
            turkish
                ? "Bu rapor ne iddia etmez?"
                : "What this report does not claim",
            1,
            turkish
                ? "GitHub Proof görünür repository kanıtını değerlendirir. Geliştirici kıdemi, işe alınabilirlik, yazarlık, özgünlük, AI kullanımı veya genel geliştirici kalitesi hakkında çıkarım yapmaz."
                : "GitHub Proof evaluates visible repository evidence. It does not infer developer seniority, employability, authorship, originality, AI usage, or overall developer quality.");

        return new RepositoryReport(
            locale,
            RepositoryReportSectionKeys.CanonicalOrder,
            identity,
            new RepositoryReportSection<RepositoryReportHighlight>(
                RepositoryReportSectionKeys.Proves,
                turkish
                    ? "Bu repository neyi kanıtlıyor?"
                    : "What this repository proves",
                proves),
            new RepositoryReportSection<RepositoryReportHighlight>(
                RepositoryReportSectionKeys.Weakens,
                turkish
                    ? "Kanıtı ne zayıflatıyor?"
                    : "What weakens the evidence",
                weakens),
            new RepositoryReportSection<RepositoryReportAction>(
                RepositoryReportSectionKeys.DoFirst,
                turkish ? "Önce bunları yap" : "Do these first",
                actions),
            new RepositoryReportSection<RepositoryReportUncertainty>(
                RepositoryReportSectionKeys.Uncertainty,
                turkish ? "Belirsizlik" : "Uncertainty",
                uncertainty),
            score,
            new RepositoryReportSection<SignalPresentation>(
                RepositoryReportSectionKeys.SignalDetails,
                turkish ? "Sinyal ayrıntıları" : "Signal details",
                details),
            warnings,
            limitations,
            disclaimer);
    }

    private static RepositoryReportHighlight CreateHighlight(
        string signalKey,
        string title,
        SignalStatus status,
        string statusLabel,
        int score,
        int maximumScore,
        IReadOnlyList<EvidenceItem> evidence) =>
        new(
            signalKey,
            title,
            $"{title} visible evidence is present.",
            $"{title} contributes observable repository evidence.",
            null,
            status,
            statusLabel,
            score,
            maximumScore,
            evidence);

    private static RepositoryReportHighlight CreateGap(
        string signalKey,
        string title,
        int score,
        int maximumScore,
        IReadOnlyList<EvidenceItem> evidence) =>
        new(
            signalKey,
            title,
            $"{title} has no qualifying visible evidence.",
            $"{title} weakens the visible repository evidence.",
            $"Improve {title}.",
            SignalStatus.Missing,
            "Missing",
            score,
            maximumScore,
            evidence);
}
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Web.Development;

public static class RepositoryReportPreviewFactory
{
    private static readonly DateTimeOffset ObservedAt =
        DateTimeOffset.Parse("2026-08-16T00:20:00Z");

    public static RepositoryReport Create(string? locale)
    {
        bool tr = string.Equals(
            locale,
            "tr",
            StringComparison.OrdinalIgnoreCase);

        string normalizedLocale = tr ? "tr" : "en";

        IReadOnlyList<EvidenceItem> evidence =
        [
            new EvidenceItem(
                "preview.readme",
                "README",
                "dotnet test GitHubProof.slnx",
                "README.md#validation",
                ObservedAt,
                EvidenceConfidence.High,
                "Development-only preview evidence.")
        ];

        RepositoryReportIdentity identity = new(
            RepositoryReportSectionKeys.Identity,
            L(tr, "Analysis source", "Analiz kaynağı"),
            Guid.Parse("04700000-0000-0000-0000-000000000008"),
            "fatihGunalp",
            "github-proof",
            "fatihGunalp/github-proof",
            new Uri("https://github.com/fatihGunalp/github-proof"),
            "main",
            "a262394a62ba9084df09954864d36a22a290c5fc",
            ObservedAt,
            true,
            false,
            false,
            false,
            "C#",
            AnalysisStatus.Partial,
            L(tr, "Partial", "Kısmi"),
            "0.1.0",
            "2026.08.16",
            1);

        IReadOnlyList<RepositoryReportHighlight> proves =
        [
            Highlight(
                tr,
                "process.commit-history",
                L(tr, "Commit history", "Commit geçmişi"),
                SignalStatus.Strong,
                7,
                7,
                evidence),

            Highlight(
                tr,
                "validation.test-structure",
                L(tr, "Automated tests", "Otomatik testler"),
                SignalStatus.Strong,
                10,
                10,
                evidence),

            Highlight(
                tr,
                "documentation.usage-or-demo",
                L(tr, "Usage guidance", "Kullanım rehberi"),
                SignalStatus.Present,
                7,
                7,
                evidence)
        ];

        IReadOnlyList<RepositoryReportHighlight> weakens =
        [
            Gap(
                tr,
                "documentation.installation-guide",
                L(tr, "Installation guidance", "Kurulum rehberi"),
                0,
                8,
                evidence),

            Gap(
                tr,
                "validation.continuous-integration",
                L(tr, "Continuous integration", "Sürekli entegrasyon"),
                0,
                8,
                evidence),

            Gap(
                tr,
                "process.release-usage",
                L(tr, "Release usage", "Release kullanımı"),
                0,
                6,
                evidence)
        ];

        IReadOnlyList<RepositoryReportAction> actions =
        [
            new RepositoryReportAction(
                1,
                "preview.action.installation",
                L(tr, "Document installation", "Kurulumu belgele"),
                L(
                    tr,
                    "Another person should be able to reproduce the setup.",
                    "Başka biri kurulumu tekrar üretebilmelidir."),
                L(
                    tr,
                    "Add explicit installation steps to the README.",
                    "README'ye açık kurulum adımları ekle."),
                EffortLevel.Unknown,
                L(tr, "Not estimated", "Tahmin edilmedi"),
                ["documentation.installation-guide"],
                evidence),

            new RepositoryReportAction(
                2,
                "preview.action.ci",
                L(tr, "Add CI", "CI ekle"),
                L(
                    tr,
                    "Repeatable validation should be visible.",
                    "Tekrarlanabilir doğrulama görünür olmalıdır."),
                L(
                    tr,
                    "Run the documented validation commands in CI.",
                    "Belgelenmiş doğrulama komutlarını CI içinde çalıştır."),
                EffortLevel.Unknown,
                L(tr, "Not estimated", "Tahmin edilmedi"),
                ["validation.continuous-integration"],
                evidence),

            new RepositoryReportAction(
                3,
                "preview.action.release",
                L(tr, "Publish a release", "Release yayımla"),
                L(
                    tr,
                    "Visible milestones make repository progress easier to understand.",
                    "Görünür kilometre taşları repository ilerlemesini anlamayı kolaylaştırır."),
                L(
                    tr,
                    "Publish a meaningful tagged release.",
                    "Anlamlı ve etiketlenmiş bir release yayımla."),
                EffortLevel.Unknown,
                L(tr, "Not estimated", "Tahmin edilmedi"),
                ["process.release-usage"],
                evidence)
        ];

        IReadOnlyList<RepositoryReportUncertainty> uncertainty =
        [
            new RepositoryReportUncertainty(
                "process.issue-usage",
                SignalStatus.NotApplicable,
                L(tr, "Issue usage", "Issue kullanımı"),
                StatusLabel(tr, SignalStatus.NotApplicable),
                L(
                    tr,
                    "This signal does not apply to this repository.",
                    "Bu sinyal bu repository için uygulanabilir değil."),
                evidence),

            new RepositoryReportUncertainty(
                "portfolio.homepage-or-live-demo",
                SignalStatus.UnavailableEvidence,
                L(tr, "Live demo", "Canlı demo"),
                StatusLabel(tr, SignalStatus.UnavailableEvidence),
                L(
                    tr,
                    "The required evidence was unavailable.",
                    "Gerekli kanıta erişilemedi."),
                evidence),

            new RepositoryReportUncertainty(
                "validation.continuous-integration",
                SignalStatus.RequiresHumanReview,
                L(tr, "CI review", "CI incelemesi"),
                StatusLabel(tr, SignalStatus.RequiresHumanReview),
                L(
                    tr,
                    "Human review is required before a deterministic conclusion.",
                    "Deterministik sonuçtan önce insan incelemesi gerekiyor."),
                evidence)
        ];

        IReadOnlyList<RepositoryReportCategory> categories =
        [
            new(
                AnalysisCategory.RepositoryUnderstandability,
                L(tr, "Documentation", "Dokümantasyon"),
                CategoryStatus.Completed,
                L(tr, "Completed", "Tamamlandı"),
                20,
                27,
                74),

            new(
                AnalysisCategory.DevelopmentProcessVisibility,
                L(tr, "Development Process", "Geliştirme Süreci"),
                CategoryStatus.Completed,
                L(tr, "Completed", "Tamamlandı"),
                26,
                32,
                81),

            new(
                AnalysisCategory.ValidationAndUsability,
                L(tr, "Validation", "Doğrulama"),
                CategoryStatus.Partial,
                L(tr, "Partial", "Kısmi"),
                10,
                18,
                56),

            new(
                AnalysisCategory.PortfolioPresentation,
                L(tr, "Portfolio Presentation", "Portföy Sunumu"),
                CategoryStatus.Partial,
                L(tr, "Partial", "Kısmi"),
                7,
                14,
                50)
        ];

        RepositoryReportScoreSummary score = new(
            RepositoryReportSectionKeys.CategoriesAndScore,
            L(tr, "Category summary", "Kategori özeti"),
            63,
            86,
            73,
            "score.label.strong-visible-evidence",
            L(tr, "Strong visible evidence", "Güçlü görünür kanıt"),
            ScoreCalculationMethod.RawApplicableScoreNormalization,
            categories);

        IReadOnlyList<SignalPresentation> signals =
        [
            Signal(
                normalizedLocale,
                tr,
                "process.commit-history",
                L(tr, "Commit history", "Commit geçmişi"),
                SignalStatus.Strong,
                7,
                7,
                evidence),

            Signal(
                normalizedLocale,
                tr,
                "documentation.usage-or-demo",
                L(tr, "Usage guidance", "Kullanım rehberi"),
                SignalStatus.Present,
                7,
                7,
                evidence),

            Signal(
                normalizedLocale,
                tr,
                "validation.test-structure",
                L(tr, "Test structure", "Test yapısı"),
                SignalStatus.Partial,
                5,
                10,
                evidence),

            Signal(
                normalizedLocale,
                tr,
                "documentation.installation-guide",
                L(tr, "Installation guide", "Kurulum rehberi"),
                SignalStatus.Missing,
                0,
                8,
                evidence),

            Signal(
                normalizedLocale,
                tr,
                "process.issue-usage",
                L(tr, "Issue usage", "Issue kullanımı"),
                SignalStatus.NotApplicable,
                null,
                6,
                evidence),

            Signal(
                normalizedLocale,
                tr,
                "portfolio.homepage-or-live-demo",
                L(tr, "Live demo", "Canlı demo"),
                SignalStatus.UnavailableEvidence,
                null,
                5,
                evidence),

            Signal(
                normalizedLocale,
                tr,
                "validation.continuous-integration",
                L(tr, "CI review", "CI incelemesi"),
                SignalStatus.RequiresHumanReview,
                null,
                8,
                evidence)
        ];

        IReadOnlyList<RepositoryReportWarning> warnings =
        [
            new(
                "preview.unavailable-evidence",
                WarningSeverity.Warning,
                L(
                    tr,
                    "Some required evidence was unavailable.",
                    "Bazı gerekli kanıtlara erişilemedi."),
                [],
                ["portfolio.homepage-or-live-demo"])
        ];

        IReadOnlyList<string> limitations =
        [
            L(
                tr,
                "Some signals contain explicit uncertainty.",
                "Bazı sinyaller açık belirsizlik içeriyor."),

            L(
                tr,
                "This preview is static and does not perform repository analysis.",
                "Bu önizleme statiktir ve repository analizi çalıştırmaz.")
        ];

        RepositoryReportDisclaimer disclaimer = new(
            RepositoryReportSectionKeys.Disclaimer,
            L(
                tr,
                "What this report does not claim",
                "Bu rapor ne iddia etmez?"),
            1,
            L(
                tr,
                "GitHub Proof evaluates visible repository evidence. It does not infer developer seniority, employability, authorship, originality, AI usage, or overall developer quality.",
                "GitHub Proof görünür repository kanıtını değerlendirir. Geliştirici kıdemi, işe alınabilirlik, yazarlık, özgünlük, AI kullanımı veya genel geliştirici kalitesi hakkında çıkarım yapmaz."));

        return new RepositoryReport(
            normalizedLocale,
            RepositoryReportSectionKeys.CanonicalOrder,
            identity,
            new(
                RepositoryReportSectionKeys.Proves,
                L(
                    tr,
                    "Strong Evidence",
                    "Güçlü Kanıtlar"),
                proves),
            new(
                RepositoryReportSectionKeys.Weakens,
                L(
                    tr,
                    "Weak Points",
                    "Zayıf Noktalar"),
                weakens),
            new(
                RepositoryReportSectionKeys.DoFirst,
                L(tr, "Do these first", "Önce bunları yap"),
                actions),
            new(
                RepositoryReportSectionKeys.Uncertainty,
                L(tr, "Uncertainty", "Belirsizlik"),
                uncertainty),
            score,
            new(
                RepositoryReportSectionKeys.SignalDetails,
                L(tr, "Signal details", "Sinyal ayrıntıları"),
                signals),
            warnings,
            limitations,
            disclaimer);
    }

    private static RepositoryReportHighlight Highlight(
        bool tr,
        string key,
        string title,
        SignalStatus status,
        int score,
        int maximumScore,
        IReadOnlyList<EvidenceItem> evidence) =>
        new(
            key,
            title,
            L(
                tr,
                $"{title} exposes qualifying visible evidence.",
                $"{title} için nitelikli görünür kanıt mevcut."),
            L(
                tr,
                "This contributes directly to observable repository evidence.",
                "Bu, doğrudan gözlemlenebilir repository kanıtına katkı sağlar."),
            null,
            status,
            StatusLabel(tr, status),
            score,
            maximumScore,
            evidence);

    private static RepositoryReportHighlight Gap(
        bool tr,
        string key,
        string title,
        int score,
        int maximumScore,
        IReadOnlyList<EvidenceItem> evidence) =>
        new(
            key,
            title,
            L(
                tr,
                "No qualifying visible evidence was found in the evaluated scope.",
                "Değerlendirilen kapsamda nitelikli görünür kanıt bulunamadı."),
            L(
                tr,
                "The missing evidence weakens how clearly the repository communicates its engineering work.",
                "Eksik kanıt repository'nin mühendislik çalışmasını ne kadar açık anlattığını zayıflatıyor."),
            L(
                tr,
                $"Improve {title}.",
                $"{title} alanını iyileştir."),
            SignalStatus.Missing,
            StatusLabel(tr, SignalStatus.Missing),
            score,
            maximumScore,
            evidence);

    private static SignalPresentation Signal(
        string locale,
        bool tr,
        string key,
        string title,
        SignalStatus status,
        int? score,
        int maximumScore,
        IReadOnlyList<EvidenceItem> evidence) =>
        new(
            locale,
            key,
            title,
            L(
                tr,
                $"Checks the visible evidence for {title}.",
                $"{title} için görünür kanıtı kontrol eder."),
            status,
            StatusLabel(tr, status),
            score,
            maximumScore,
            L(
                tr,
                $"The current observable status is {StatusLabel(tr, status)}.",
                $"Mevcut gözlemlenebilir durum: {StatusLabel(tr, status)}."),
            L(
                tr,
                "This signal contributes to understanding what the repository visibly demonstrates.",
                "Bu sinyal repository'nin görünür biçimde ne gösterdiğini anlamaya katkı sağlar."),
            status is SignalStatus.Missing or SignalStatus.Partial
                ? L(
                    tr,
                    $"Improve the visible evidence for {title}.",
                    $"{title} için görünür kanıtı iyileştir.")
                : null,
            [
                L(
                    tr,
                    "Only evidence visible in the evaluated repository scope is represented.",
                    "Yalnızca değerlendirilen repository kapsamında görünür kanıt temsil edilir.")
            ],
            evidence,
            1);

    private static string StatusLabel(
        bool tr,
        SignalStatus status) =>
        status switch
        {
            SignalStatus.Strong =>
                L(tr, "Strong", "Güçlü"),

            SignalStatus.Present =>
                L(tr, "Present", "Mevcut"),

            SignalStatus.Partial =>
                L(tr, "Partial", "Kısmi"),

            SignalStatus.Missing =>
                L(tr, "Missing", "Eksik"),

            SignalStatus.NotApplicable =>
                L(tr, "Not applicable", "Uygulanamaz"),

            SignalStatus.UnavailableEvidence =>
                L(tr, "Evidence unavailable", "Kanıta erişilemiyor"),

            SignalStatus.RequiresHumanReview =>
                L(tr, "Human review required", "İnsan incelemesi gerekli"),

            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };

    private static string L(
        bool tr,
        string english,
        string turkish) =>
        tr ? turkish : english;
}
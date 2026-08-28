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
            L(tr, "Some data is missing", "Bazı veriler eksik"),
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
                    "Issue usage does not apply to this repository.",
                    "Issue kullanımı bu repository için geçerli değil."),
                evidence),

            new RepositoryReportUncertainty(
                "portfolio.homepage-or-live-demo",
                SignalStatus.UnavailableEvidence,
                L(tr, "Live demo", "Canlı demo"),
                StatusLabel(tr, SignalStatus.UnavailableEvidence),
                L(
                    tr,
                    "Live demo could not be evaluated because the required data was unavailable.",
                    "Gerekli veriye erişilemediği için Canlı demo değerlendirilemedi."),
                evidence),

            new RepositoryReportUncertainty(
                "validation.continuous-integration",
                SignalStatus.RequiresHumanReview,
                L(tr, "CI review", "CI incelemesi"),
                StatusLabel(tr, SignalStatus.RequiresHumanReview),
                L(
                    tr,
                    "CI review requires human review before a definite result can be given.",
                    "CI incelemesi için kesin bir sonuç vermeden önce insan incelemesi gerekiyor."),
                evidence)
        ];

        IReadOnlyList<RepositoryReportCategory> categories =
        [
            new(
                AnalysisCategory.RepositoryUnderstandability,
                L(tr, "Documentation", "Dokümantasyon"),
                CategoryStatus.Completed,
                L(tr, "Result available", "Sonuç var"),
                20,
                27,
                74),

            new(
                AnalysisCategory.DevelopmentProcessVisibility,
                L(tr, "Development Process", "Geliştirme Süreci"),
                CategoryStatus.Completed,
                L(tr, "Result available", "Sonuç var"),
                26,
                32,
                81),

            new(
                AnalysisCategory.ValidationAndUsability,
                L(tr, "Validation", "Doğrulama"),
                CategoryStatus.Partial,
                L(tr, "Some data is missing", "Bazı veriler eksik"),
                10,
                18,
                56),

            new(
                AnalysisCategory.PortfolioPresentation,
                L(tr, "Portfolio Presentation", "Portföy Sunumu"),
                CategoryStatus.Partial,
                L(tr, "Some data is missing", "Bazı veriler eksik"),
                7,
                14,
                50)
        ];

        RepositoryReportScoreSummary score = new(
            RepositoryReportSectionKeys.CategoriesAndScore,
            L(tr, "Categories and score", "Kategoriler ve skor"),
            63,
            86,
            73,
            RepositoryReportScoreLabelKeys.StrongEvidence,
            L(tr, "Strong evidence", "Güçlü kanıt"),
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
                RepositoryReportWarningKeys.SignalUnavailableEvidence,
                L(
                    tr,
                    "Required data was unavailable for some signals. This does not mean the evidence is missing.",
                    "Bazı sinyaller için gerekli veriye erişilemedi. Bu, kanıtın eksik olduğu anlamına gelmez."),
                [],
                ["portfolio.homepage-or-live-demo"])
        ];

        IReadOnlyList<RepositoryReportLimitation> limitations =
        [
            new(
                RepositoryReportLimitationKeys.AnalysisUnavailableEvidence,
                L(
                    tr,
                    "Some signals could not be evaluated because the required data was unavailable.",
                    "Bazı sinyaller için gerekli veriye erişilemediği için değerlendirilemedi.")),

            new(
                "preview.limitation.static",
                L(
                    tr,
                    "This preview is static and does not perform repository analysis.",
                    "Bu önizleme statiktir ve repository analizi çalıştırmaz."))
        ];

        RepositoryReportDisclaimer disclaimer = new(
            RepositoryReportSectionKeys.Disclaimer,
            L(
                tr,
                "What this analysis does not claim",
                "Bu analiz neyi söylemez?"),
            1,
            L(
                tr,
                "GitHub Proof only looks at evidence visible in the repository during the analysis. It does not infer the developer's seniority, employability, authorship, originality, AI usage, or overall quality.",
                "GitHub Proof yalnızca analiz sırasında repository'de görülebilen kanıtlara bakar. Geliştiricinin kıdemi, işe alınabilirliği, yazarlığı, özgünlüğü, AI kullanımı veya genel kalitesi hakkında çıkarım yapmaz."));

        return new RepositoryReport(
            normalizedLocale,
            RepositoryReportSectionKeys.CanonicalOrder,
            identity,
            new(
                RepositoryReportSectionKeys.Proves,
                L(
                    tr,
                    "What does this repository prove?",
                    "Bu repository neyi kanıtlıyor?"),
                proves),
            new(
                RepositoryReportSectionKeys.Weakens,
                L(
                    tr,
                    "What weakens the evidence?",
                    "Kanıtı ne zayıflatıyor?"),
                weakens),
            new(
                RepositoryReportSectionKeys.DoFirst,
                L(tr, "Do these first", "Önce bunları yap"),
                actions),
            new(
                RepositoryReportSectionKeys.Uncertainty,
                L(tr, "Things to keep in mind", "Dikkate alınması gerekenler"),
                uncertainty),
            score,
            new(
                RepositoryReportSectionKeys.SignalDetails,
                L(tr, "Evidence details", "Kanıt ayrıntıları"),
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
                L(tr, "Strong evidence", "Güçlü kanıt"),

            SignalStatus.Present =>
                L(tr, "Evidence found", "Kanıt var"),

            SignalStatus.Partial =>
                L(tr, "Partial evidence", "Kısmi kanıt"),

            SignalStatus.Missing =>
                L(tr, "No evidence found", "Kanıt bulunamadı"),

            SignalStatus.NotApplicable =>
                L(tr, "Does not apply", "Geçerli değil"),

            SignalStatus.UnavailableEvidence =>
                L(tr, "Data unavailable", "Veriye erişilemedi"),

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
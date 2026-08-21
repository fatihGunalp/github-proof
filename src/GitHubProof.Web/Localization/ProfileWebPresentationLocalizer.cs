using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;

namespace GitHubProof.Web.Localization;

// Web-owned presentation mapping over existing structured Profile identities.
// It never projects or changes ProfileReport truth.
public sealed class ProfileWebPresentationLocalizer
{
    public string GetSectionLabel(
        ProfileReportSectionKey section,
        string? culture) =>
        (section, WebCulture.NormalizeOrEnglish(culture)) switch
        {
            (ProfileReportSectionKey.PublicProfileSummary, WebCulture.English) =>
                "What does this GitHub communicate?",
            (ProfileReportSectionKey.PublicProfileSummary, WebCulture.Turkish) =>
                "Bu GitHub ne anlatıyor?",
            (ProfileReportSectionKey.TechnicalFocusAreas, WebCulture.English) =>
                "Visible technical direction",
            (ProfileReportSectionKey.TechnicalFocusAreas, WebCulture.Turkish) =>
                "Görünen teknik yön",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.English) =>
                "Recurring engineering practices",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.Turkish) =>
                "Tekrar eden üretim pratikleri",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.English) =>
                "Starting point",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.Turkish) =>
                "Başlangıç noktası",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.English) =>
                "Why this view",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.Turkish) =>
                "Bu görünümü neden görüyoruz?",
            (ProfileReportSectionKey.Limitations, WebCulture.English) =>
                "Limits of this view",
            (ProfileReportSectionKey.Limitations, WebCulture.Turkish) =>
                "Bu görünümün sınırları",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.English) =>
                "Repository evidence",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.Turkish) =>
                "Repository kanıtı",
            _ => throw new InvalidOperationException(
                $"Profile section '{section}' has no active Web presentation mapping.")
        };

    public string GetPatternLabel(
        ProfilePatternFamily family,
        string patternKey,
        string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patternKey);

        if (family == ProfilePatternFamily.TechnicalFocus)
        {
            // GitHub programming-language identity is literal evidence.
            return patternKey;
        }

        if (family != ProfilePatternFamily.EngineeringPractice)
        {
            throw new InvalidOperationException(
                $"Profile pattern family '{family}' has no active Web presentation mapping.");
        }

        bool isTurkish =
            WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        return patternKey switch
        {
            ProfilePatternPromotionPolicy.AutomatedTestingPatternKey =>
                isTurkish ? "Otomatik testler" : "Automated testing",
            ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey =>
                isTurkish ? "Sürekli entegrasyon" : "Continuous integration",
            ProfilePatternPromotionPolicy.DocumentationPatternKey =>
                isTurkish ? "Dokümantasyon" : "Documentation",
            ProfilePatternPromotionPolicy.ReleaseUsagePatternKey =>
                isTurkish ? "Sürüm yayınlama pratiği" : "Release practice",
            ProfilePatternPromotionPolicy.IssueUsagePatternKey =>
                isTurkish ? "Issue kullanımı" : "Issue usage",
            ProfilePatternPromotionPolicy.PullRequestUsagePatternKey =>
                isTurkish ? "Pull request kullanımı" : "Pull request usage",
            _ => throw new InvalidOperationException(
                $"Unknown engineering-practice pattern identity '{patternKey}'.")
        };
    }

    public string GetLimitationLabel(
        ProfileLimitationCode code,
        string? culture) =>
        (code, WebCulture.NormalizeOrEnglish(culture)) switch
        {
            (ProfileLimitationCode.LimitedIndependentEvidenceSurface, WebCulture.English) =>
                "The public profile contains a limited independent evidence surface.",
            (ProfileLimitationCode.LimitedIndependentEvidenceSurface, WebCulture.Turkish) =>
                "Herkese açık profil sınırlı bir bağımsız kanıt yüzeyi içeriyor.",
            (ProfileLimitationCode.PartialEvidenceSurface, WebCulture.English) =>
                "Some public repository evidence was incomplete or unavailable.",
            (ProfileLimitationCode.PartialEvidenceSurface, WebCulture.Turkish) =>
                "Bazı herkese açık repository kanıtları eksik veya erişilemezdi.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };

    public string GetWarningLabel(
        ProfileWarningCode code,
        string? culture)
    {
        bool isTurkish =
            WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        return code switch
        {
            ProfileWarningCode.RepositoryEvidenceUnavailable =>
                isTurkish
                    ? "Bazı repository kanıtlarına erişilemedi."
                    : "Some repository evidence was unavailable.",
            ProfileWarningCode.RepositoryEvidenceRateLimited =>
                isTurkish
                    ? "GitHub hız sınırı bazı repository kanıtlarını sınırladı."
                    : "GitHub rate limiting constrained some repository evidence.",
            ProfileWarningCode.RepositoryAnalysisFailed =>
                isTurkish
                    ? "Bazı repository analizleri tamamlanamadı."
                    : "Some repository analyses could not be completed.",
            ProfileWarningCode.RepositoryEligibilityUndetermined =>
                isTurkish
                    ? "Bazı repository'lerin uygunluğu belirlenemedi."
                    : "Eligibility could not be determined for some repositories.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}

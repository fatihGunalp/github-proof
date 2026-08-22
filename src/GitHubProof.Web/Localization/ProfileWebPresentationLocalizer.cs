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

    // P10-I3 Meaning layer (presentation-language-contract.md §8.4). Reads
    // exactly the two Supported TechnicalFocusAreas/EngineeringPracticePatterns
    // facts, reduces each to a presence bit (ItemCount is never rendered) and
    // selects exactly one of the four locked M1-M4 variants. RepresentativeRepositories
    // and every other section never participate.
    public string GetMeaningSummary(
        IReadOnlyList<ProfileSummaryFact> summaryFacts,
        string? culture)
    {
        ArgumentNullException.ThrowIfNull(summaryFacts);

        bool technicalDirectionPresent = RequireSupportedPresence(
            summaryFacts,
            ProfileReportSectionKey.TechnicalFocusAreas);
        bool engineeringPracticePresent = RequireSupportedPresence(
            summaryFacts,
            ProfileReportSectionKey.EngineeringPracticePatterns);

        bool isTurkish =
            WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        return (technicalDirectionPresent, engineeringPracticePresent) switch
        {
            (true, true) => isTurkish
                ? "Görünen herkese açık kanıtlar, tekrar eden teknik yön ve tekrar eden üretim pratikleri gösteriyor."
                : "The visible public evidence shows recurring technical direction and recurring engineering practices.",
            (true, false) => isTurkish
                ? "Görünen herkese açık kanıtlar, tekrar eden teknik yön gösteriyor."
                : "The visible public evidence shows recurring technical direction.",
            (false, true) => isTurkish
                ? "Görünen herkese açık kanıtlar, tekrar eden üretim pratikleri gösteriyor."
                : "The visible public evidence shows recurring engineering practices.",
            (false, false) => isTurkish
                ? "Mevcut analiz, görünen herkese açık kanıtlardan tekrar eden bir teknik yön veya üretim pratiği ortaya koymadı."
                : "The current analysis did not establish a recurring technical direction or engineering-practice pattern from the visible public evidence."
        };
    }

    // §8.5 Supported+empty copy for TechnicalFocusAreas, EngineeringPracticePatterns
    // and RepresentativeRepositories only. Never called for Unsupported/Deferred
    // sections (ProblemAreaPatterns, RoleEvidenceAlignment): those are omitted
    // entirely, not rendered as an empty result.
    public string GetEmptySectionMessage(
        ProfileReportSectionKey section,
        string? culture) =>
        (section, WebCulture.NormalizeOrEnglish(culture)) switch
        {
            (ProfileReportSectionKey.TechnicalFocusAreas, WebCulture.English) =>
                "The visible public evidence did not establish a repeated technical direction under the current analysis rules.",
            (ProfileReportSectionKey.TechnicalFocusAreas, WebCulture.Turkish) =>
                "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden bir teknik yön ortaya koymadı.",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.English) =>
                "The visible public evidence did not establish recurring engineering practices under the current analysis rules.",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.Turkish) =>
                "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden üretim pratikleri ortaya koymadı.",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.English) =>
                "No starting-point repository was selected from the current visible public evidence.",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.Turkish) =>
                "Mevcut görünen herkese açık kanıtlardan bir başlangıç noktası repository'si seçilmedi.",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.English) =>
                "The visible public evidence did not establish a repeated pattern to explain under the current analysis rules.",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.Turkish) =>
                "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında açıklanacak tekrar eden bir örüntü ortaya koymadı.",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.English) =>
                "The visible public evidence contains no public repository to inspect.",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.Turkish) =>
                "Görünen herkese açık kanıtlar, incelenebilecek herkese açık repository içermiyor.",
            _ => throw new InvalidOperationException(
                $"Profile section '{section}' has no active Supported+empty presentation mapping.")
        };

    // P10-I4 row status (presentation-language-contract.md §8.6.6). Reads only
    // RepositoryEligibilityStatus; RepositoryAnalysisReference is never read
    // by presentation. Exhaustive over the enum — no fifth state.
    public string GetRepositoryStatusLabel(
        RepositoryEligibilityStatus status,
        string? culture) =>
        (status, WebCulture.NormalizeOrEnglish(culture)) switch
        {
            (RepositoryEligibilityStatus.Contributing, WebCulture.English) =>
                "Contributed evidence to this view",
            (RepositoryEligibilityStatus.Contributing, WebCulture.Turkish) =>
                "Bu görünüme kanıt sağladı",
            (RepositoryEligibilityStatus.Restricted, WebCulture.English) =>
                "Contributed evidence to this view with restrictions",
            (RepositoryEligibilityStatus.Restricted, WebCulture.Turkish) =>
                "Bu görünüme kısıtlamalarla kanıt sağladı",
            (RepositoryEligibilityStatus.NonContributing, WebCulture.English) =>
                "Did not contribute evidence to this view",
            (RepositoryEligibilityStatus.NonContributing, WebCulture.Turkish) =>
                "Bu görünüme kanıt sağlamadı",
            (RepositoryEligibilityStatus.Undetermined, WebCulture.English) =>
                "Its contribution to this view could not be determined",
            (RepositoryEligibilityStatus.Undetermined, WebCulture.Turkish) =>
                "Bu görünüme katkısı belirlenemedi",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    // Fixed P10-I4 presentation slots (presentation-language-contract.md
    // §8.6.7/§13.8 Session 104 daraltması): none of these carries a canonical
    // semantic identity of its own, so each lives in its own narrow method
    // keyed only by the selected request culture — never by presentation text,
    // and no new analysis/Domain/Application identity is created to key them.
    public string GetWhySupportingRepositoriesLeadIn(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Şu herkese açık repository'lerde gözlemlendi"
            : "Observed across these public repositories";

    public string GetRepositoryFreshAnalysisLeadIn(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Bir repository seçmek yeni bir Repository Proof analizi başlatır."
            : "Selecting a repository runs a fresh Repository Proof analysis.";

    public string GetRepositoryActionLabel(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Bu repository'yi analiz et"
            : "Analyze this repository";

    // Repository FullName is data, substituted into the locked format; it is
    // never translated.
    public string GetRepositoryActionAccessibleName(
        string repositoryFullName,
        string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryFullName);

        return WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? $"{repositoryFullName} repository'sini analiz et"
            : $"Analyze repository {repositoryFullName}";
    }

    // Fails loudly (missing, duplicated or non-Supported) rather than inventing
    // an empty Meaning state: presentation-language-contract.md §8.4.1.
    private static bool RequireSupportedPresence(
        IReadOnlyList<ProfileSummaryFact> summaryFacts,
        ProfileReportSectionKey section)
    {
        ProfileSummaryFact? match = null;
        int matchCount = 0;

        foreach (ProfileSummaryFact fact in summaryFacts)
        {
            if (fact.ReferencedSection != section)
            {
                continue;
            }

            matchCount++;
            match = fact;
        }

        if (matchCount != 1)
        {
            throw new InvalidOperationException(
                $"PublicProfileSummary must carry exactly one '{section}' fact "
                + $"for the Meaning layer, but found {matchCount}.");
        }

        if (match!.ReferencedSectionAvailability != ProfileReportSectionAvailability.Supported)
        {
            throw new InvalidOperationException(
                $"PublicProfileSummary fact for '{section}' must be Supported "
                + $"for the Meaning layer, but was {match.ReferencedSectionAvailability}.");
        }

        return match.ItemCount > 0;
    }

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

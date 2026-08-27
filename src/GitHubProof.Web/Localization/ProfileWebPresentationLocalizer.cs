using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Web.Localization;

// Web-owned presentation mapping over existing structured Profile identities.
// It never projects or changes ProfileReport truth.
public sealed class ProfileWebPresentationLocalizer
{
    private const string EnglishMeaningProvenance =
        "This summary is based only on traces visible in public GitHub work under the current analysis rules.";

    private const string TurkishMeaningProvenance =
        "Bu anlatım yalnızca herkese açık GitHub çalışmalarında görülebilen izlere dayanıyor.";

    public string GetProfileIdentityContext(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Herkese açık GitHub çalışmalarının görünümü"
            : "View of public GitHub work";

    public string GetOpenProfileOnGitHubLabel(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "GitHub'da aç"
            : "Open on GitHub";

    public string GetOpenProfileOnGitHubAccessibleName(
        string profileIdentity,
        string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileIdentity);

        return WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? $"{profileIdentity} profilini GitHub'da aç; yeni sekmede açılır"
            : $"Open {profileIdentity} on GitHub; opens in a new tab";
    }

    // The real provider-returned avatar's accessible name. Rendered only when
    // Model.AvatarUrl is available; the decorative fallback mark stays
    // aria-hidden and never uses this text.
    public string GetAvatarAccessibleName(string profileIdentity, string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileIdentity);

        return WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? $"{profileIdentity} GitHub profil fotoğrafı"
            : $"{profileIdentity}'s GitHub profile photo";
    }

    public string GetWorkTypeCardHeading(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Ne tür çalışmalar görünüyor?"
            : "What kinds of work are visible?";

    public string GetRecurringTechnologiesLeadIn(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Tekrar eden teknolojiler"
            : "Recurring technologies";

    public string GetWhyDisclosureDescription(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Gözlemler hangi repository'lere dayandığıyla birlikte açıklanır."
            : "Each observation is explained with the repositories that support it.";

    public string GetRepositoryEvidenceDisclosureDescription(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "İncelenen herkese açık repository'ler burada görülür."
            : "The public repositories examined for this view appear here.";

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
                "Working style",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.Turkish) =>
                "Çalışma biçimi",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.English) =>
                "Starting point",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.Turkish) =>
                "Başlangıç noktası",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.English) =>
                "What is this based on?",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.Turkish) =>
                "Bunu neye dayanarak söylüyoruz?",
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
    // exactly the accepted Profile identity and existing canonical items in
    // the two supported pattern sections. It neither reads nor derives any
    // other Profile report truth.
    public ProfileMeaningPresentation GetMeaningPresentation(
        string profileIdentity,
        ProfileReportSection<PatternReportItem> technicalFocusAreas,
        ProfileReportSection<PatternReportItem> engineeringPracticePatterns,
        string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileIdentity);

        IReadOnlyList<string> technicalLabels = GetMeaningLabels(
            technicalFocusAreas,
            ProfileReportSectionKey.TechnicalFocusAreas,
            ProfilePatternFamily.TechnicalFocus,
            culture);
        IReadOnlyList<string> practices = GetMeaningPractices(
            engineeringPracticePatterns,
            culture);

        bool isTurkish = WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        return CreateMeaningPresentation(
            profileIdentity,
            technicalLabels,
            practices,
            isTurkish);
    }

    private static ProfileMeaningPresentation CreateMeaningPresentation(
        string profileIdentity,
        IReadOnlyList<string> technicalLabels,
        IReadOnlyList<string> practices,
        bool isTurkish)
    {
        string technicalLabelsText = JoinMeaningLabels(technicalLabels, isTurkish);
        string practiceLabelsText = JoinMeaningLabels(practices, isTurkish);
        string practiceLabelsSentenceStart = StartSentence(practiceLabelsText);

        return (technicalLabels.Count > 0, practices.Count > 0) switch
        {
            (true, true) => isTurkish
                ? new ProfileMeaningPresentation(
                    technicalLabels.Count == 1
                        ? $"{profileIdentity} GitHub çalışmalarında {technicalLabelsText} farklı repository'lerde tekrar karşımıza çıkıyor. Bu nedenle {technicalLabelsText} profil boyunca görünür olan tekrar eden teknik çalışma alanı."
                        : $"{profileIdentity} GitHub çalışmalarında {technicalLabelsText} farklı repository'lerde tekrar karşımıza çıkıyor. Bu profil, tek bir teknolojiyle sınırlı olmayan farklı teknik çalışmalar gösteriyor.",
                    $"Çalışma biçiminde de bazı ortak yöntemler tekrar ediyor. {practiceLabelsSentenceStart} birden fazla repository'de görülüyor. Yani bu GitHub yalnızca hangi teknolojilerle çalışıldığını değil; çalışmaların nasıl yürütüldüğüne ilişkin yöntemlerin de farklı çalışmalarda tekrarlandığını gösteriyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    technicalLabels.Count == 1
                        ? $"{profileIdentity}'s GitHub work shows {technicalLabelsText} recurring across different repositories. This makes {technicalLabelsText} a recurring visible area of technical work across the profile."
                        : $"{profileIdentity}'s GitHub work shows {technicalLabelsText} recurring across different repositories. The profile shows technical work spanning more than one technology.",
                    $"Some ways of working recur as well. {practiceLabelsSentenceStart} appears across multiple repositories. This GitHub therefore shows not only which technologies recur, but also that ways of carrying out the work recur across different projects.",
                    EnglishMeaningProvenance),
            (true, false) => isTurkish
                ? new ProfileMeaningPresentation(
                    technicalLabels.Count == 1
                        ? $"{profileIdentity} GitHub çalışmalarında {technicalLabelsText} farklı repository'lerde tekrar karşımıza çıkıyor. Bu nedenle {technicalLabelsText} profil boyunca görünür olan tekrar eden teknik çalışma alanı."
                        : $"{profileIdentity} GitHub çalışmalarında {technicalLabelsText} farklı repository'lerde tekrar karşımıza çıkıyor. Bu profil, tek bir teknolojiyle sınırlı olmayan farklı teknik çalışmalar gösteriyor.",
                    "Çalışma biçiminde ise repository'ler arasında aynı şekilde tekrar eden belirgin bir yöntem görünmüyor. Yani bu GitHub teknik yön konusunda daha fazla şey gösterirken, çalışmaların nasıl yürütüldüğü konusunda daha sınırlı iz sunuyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    technicalLabels.Count == 1
                        ? $"{profileIdentity}'s GitHub work shows {technicalLabelsText} recurring across different repositories. This makes {technicalLabelsText} a recurring visible area of technical work across the profile."
                        : $"{profileIdentity}'s GitHub work shows {technicalLabelsText} recurring across different repositories. The profile shows technical work spanning more than one technology.",
                    "The repositories do not show one equally recurring way of working. This GitHub says more about recurring technical direction and offers more limited visible traces of how the work was carried out.",
                    EnglishMeaningProvenance),
            (false, true) => isTurkish
                ? new ProfileMeaningPresentation(
                    $"{profileIdentity} GitHub çalışmalarında repository'ler arasında tekrar eden tek bir teknik yön şu anda görünür hâle gelmiyor.",
                    $"Çalışma biçiminde ise bazı ortak yöntemler tekrar ediyor. {practiceLabelsSentenceStart} birden fazla repository'de görülüyor. Bu görünüm, tek bir teknik yönden çok çalışma yöntemlerinin farklı çalışmalarda tekrarlandığını gösteriyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    $"{profileIdentity}'s GitHub work does not currently make one recurring technical direction visible across public repositories.",
                    $"Some ways of working do recur. {practiceLabelsSentenceStart} appears across multiple repositories. This view shows recurring ways of carrying out the work more clearly than one technical direction.",
                    EnglishMeaningProvenance),
            _ => isTurkish
                ? new ProfileMeaningPresentation(
                    $"{profileIdentity} için herkese açık GitHub çalışmalarında şu anda repository'ler arasında tekrar eden belirgin bir teknik yön veya üretim pratiği görünmüyor.",
                    "Çalışma biçiminde de repository'ler arasında tekrar eden belirgin bir yöntem görünmüyor. Bu yalnızca şu an görünür olan GitHub çalışmalarının sunduğu sınırlı izi anlatıyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    $"{profileIdentity}'s GitHub work does not currently make a recurring technical direction or working practice visible across public repositories.",
                    "No clearly recurring way of working is visible across the repositories either. This describes only the limited traces in the public GitHub work currently visible.",
                    EnglishMeaningProvenance)
        };
    }

    private IReadOnlyList<string> GetMeaningPractices(
        ProfileReportSection<PatternReportItem> section,
        string? culture)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (section.SectionKey != ProfileReportSectionKey.EngineeringPracticePatterns
            || section.Availability != ProfileReportSectionAvailability.Supported)
        {
            throw new InvalidOperationException(
                "Meaning requires the canonical Supported 'EngineeringPracticePatterns' section.");
        }

        List<string> practices = new(section.Items.Count);

        foreach (PatternReportItem item in section.Items)
        {
            if (item.Pattern.Family != ProfilePatternFamily.EngineeringPractice)
            {
                throw new InvalidOperationException(
                    "Meaning EngineeringPracticePatterns contains a pattern from another family.");
            }

            practices.Add(LowercaseInitial(GetWorkingStyleLabel(item.Pattern.PatternKey, culture)));
        }

        return practices;
    }

    private IReadOnlyList<string> GetMeaningLabels(
        ProfileReportSection<PatternReportItem> section,
        ProfileReportSectionKey expectedSectionKey,
        ProfilePatternFamily expectedFamily,
        string? culture)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (section.SectionKey != expectedSectionKey
            || section.Availability != ProfileReportSectionAvailability.Supported)
        {
            throw new InvalidOperationException(
                $"Meaning requires the canonical Supported '{expectedSectionKey}' section.");
        }

        List<string> labels = new(section.Items.Count);

        foreach (PatternReportItem item in section.Items)
        {
            if (item.Pattern.Family != expectedFamily)
            {
                throw new InvalidOperationException(
                    $"Meaning section '{expectedSectionKey}' contains a pattern from '{item.Pattern.Family}'.");
            }

            labels.Add(GetPatternLabel(item.Pattern.Family, item.Pattern.PatternKey, culture));
        }

        return labels;
    }

    private static string JoinMeaningLabels(
        IReadOnlyList<string> labels,
        bool isTurkish)
    {
        return labels.Count switch
        {
            0 => string.Empty,
            1 => labels[0],
            2 => $"{labels[0]} {(isTurkish ? "ve" : "and")} {labels[1]}",
            _ => string.Join(
                ", ",
                labels.Take(labels.Count - 1))
                + (isTurkish ? " ve " : ", and ")
                + labels[^1]
        };
    }

    private static string LowercaseInitial(string value) =>
        value.Length == 0
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];

    private static string StartSentence(string value) =>
        value.Length == 0
            ? value
            : char.ToUpperInvariant(value[0]) + value[1..];

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
                "No recurring working method is visible across the current public work.",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.Turkish) =>
                "Farklı çalışmalarda tekrar eden belirgin bir yöntem görünmüyor.",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.English) =>
                "No starting-point repository was selected from the current visible public evidence.",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.Turkish) =>
                "Mevcut görünen herkese açık kanıtlardan bir başlangıç noktası repository'si seçilmedi.",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.English) =>
                "The public GitHub work did not provide a recurring conclusion to explain.",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.Turkish) =>
                "Herkese açık GitHub çalışmaları, açıklanacak tekrar eden bir sonuç ortaya koymadı.",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.English) =>
                "The visible public evidence contains no public repository to inspect.",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.Turkish) =>
                "Görünen herkese açık kanıtlar, incelenebilecek herkese açık repository içermiyor.",
            (ProfileReportSectionKey.Limitations, WebCulture.English) =>
                "No limits were recorded for this profile view.",
            (ProfileReportSectionKey.Limitations, WebCulture.Turkish) =>
                "Bu profil görünümü için bir sınır kaydedilmedi.",
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
            ? "Destekleyen repository'ler"
            : "Supporting repositories";

    public string GetWhyEntryQuestion(
        ProfilePatternFamily family,
        string patternKey,
        string? culture)
    {
        string label = GetWhyLabel(family, patternKey, culture);

        return WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? $"{label} neden burada?"
            : $"Why is {label} here?";
    }

    public string GetWhyExplanation(
        ProfilePatternFamily family,
        string patternKey,
        string? culture)
    {
        string label = GetWhyLabel(family, patternKey, culture);
        bool isTurkish = WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        return (family, isTurkish) switch
        {
            (ProfilePatternFamily.TechnicalFocus, true) =>
                $"{label} farklı herkese açık repository'lerde yeniden görüldüğü için profilin tekrar eden teknik yönlerinden biri olarak gösteriliyor.",
            (ProfilePatternFamily.TechnicalFocus, false) =>
                $"{label} appears again across different public repositories, so it is shown as one of the profile's recurring technical directions.",
            (ProfilePatternFamily.EngineeringPractice, true) =>
                $"{label} farklı herkese açık repository'lerde yeniden görüldüğü için çalışma biçiminde tekrar eden yöntemlerden biri olarak gösteriliyor.",
            (ProfilePatternFamily.EngineeringPractice, false) =>
                $"{label} appears again across different public repositories, so it is shown as one of the recurring ways of working.",
            _ => throw new InvalidOperationException(
                $"Profile pattern family '{family}' has no active Why presentation mapping.")
        };
    }

    private string GetWhyLabel(
        ProfilePatternFamily family,
        string patternKey,
        string? culture) =>
        family switch
        {
            ProfilePatternFamily.TechnicalFocus =>
                GetPatternLabel(family, patternKey, culture),
            ProfilePatternFamily.EngineeringPractice =>
                GetWorkingStyleLabel(patternKey, culture),
            _ => throw new InvalidOperationException(
                $"Profile pattern family '{family}' has no active Why presentation mapping.")
        };

    public string GetRepositoryFreshAnalysisLeadIn(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Bir repository seçmek yeni bir Repository Proof analizi başlatır."
            : "Selecting a repository runs a fresh Repository Proof analysis.";

    // Session 111 STEP 3: the neutral empty-metadata statement used only when
    // GitHub itself returned no repository Description. It describes metadata
    // absence only — never a claim about the repository's purpose, quality or
    // documentation.
    public string GetStartingPointDescriptionFallback(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "GitHub'da kısa bir açıklama bulunmuyor."
            : "No short description is provided on GitHub.";

    // Session 137 / p10 §8.6.1b: the Starting Point pattern relationship. It
    // reads ONLY the already-projected PatternSupports identities
    // (PatternFamily + PatternKey) that the partial already holds. It never
    // touches SupportingEvidenceReferences/SupportingObservationKeys — that
    // Representative Support Evidence Trace belongs to the evidence layer,
    // never to first read — and it never exposes selection or promotion
    // mechanics, a count or an ordering claim. Every support is rendered in
    // its existing canonical order: no cap, sort, filter or first-pattern
    // shortcut, because any of those would invent a hidden top-N.
    public string GetStartingPointPatternRelationship(
        IReadOnlyList<RepresentativePatternSupport> patternSupports,
        string? culture)
    {
        ArgumentNullException.ThrowIfNull(patternSupports);

        if (patternSupports.Count == 0)
        {
            throw new InvalidOperationException(
                "A representative repository must carry at least one represented pattern.");
        }

        bool isTurkish =
            WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        List<string> labels = new(patternSupports.Count);

        foreach (RepresentativePatternSupport support in patternSupports)
        {
            ArgumentNullException.ThrowIfNull(support);

            labels.Add(support.PatternFamily switch
            {
                // The literal GitHub language identity, never translated.
                ProfilePatternFamily.TechnicalFocus => GetPatternLabel(
                    support.PatternFamily,
                    support.PatternKey,
                    culture),
                // The concise §8.4.8 Working Style label, never the raw key.
                ProfilePatternFamily.EngineeringPractice => GetWorkingStyleLabel(
                    support.PatternKey,
                    culture),
                // ProblemArea is Unsupported in V1 and has no active Web
                // presentation mapping. Failing loudly is the contract; a
                // fallback label or a silent omission is not.
                _ => throw new InvalidOperationException(
                    $"Profile pattern family '{support.PatternFamily}' has no active "
                    + "Starting Point relationship mapping.")
            });
        }

        string joined = JoinMeaningLabels(labels, isTurkish);

        // Singular/plural is grammatical agreement only. No number is
        // rendered, and the labels themselves are identities the first-read
        // pattern cards already show.
        return (labels.Count == 1, isTurkish) switch
        {
            (true, true) => $"Bu tekrar eden sonuç için başlangıç noktası: {joined}",
            (true, false) => $"Starting point for this recurring result: {joined}",
            (false, true) => $"Bu tekrar eden sonuçlar için başlangıç noktası: {joined}",
            (false, false) => $"Starting point for these recurring results: {joined}"
        };
    }

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

    // P10-I6-A Working Style card: concise, evidence-bounded noun phrases for
    // the existing EngineeringPractice identities only. Meaning may join the
    // same labels into a short synthesis; Why keeps its distinct trace reason.
    public string GetWorkingStyleLabel(string patternKey, string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patternKey);

        bool isTurkish =
            WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish;

        return patternKey switch
        {
            ProfilePatternPromotionPolicy.AutomatedTestingPatternKey =>
                isTurkish ? "Otomatik test kullanımı" : "Automated testing",
            ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey =>
                isTurkish ? "Otomatik kontrol süreçleri" : "Automated integration checks",
            ProfilePatternPromotionPolicy.DocumentationPatternKey =>
                isTurkish ? "Dokümantasyon" : "Documentation",
            ProfilePatternPromotionPolicy.ReleaseUsagePatternKey =>
                isTurkish ? "Sürüm yayınları" : "Published releases",
            ProfilePatternPromotionPolicy.IssueUsagePatternKey =>
                isTurkish ? "Issue ile takip" : "Issue tracking",
            ProfilePatternPromotionPolicy.PullRequestUsagePatternKey =>
                isTurkish ? "Pull request ile değişiklik akışı" : "Pull request workflow",
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

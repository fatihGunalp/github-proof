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
        "This summary is based only on traces visible in public GitHub work.";

    private const string TurkishMeaningProvenance =
        "Bu anlatım yalnızca public GitHub çalışmalarında görülebilen izlere dayanıyor.";

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
            ? "Buradaki ifadelerin hangi repository'lere dayandığını görebilirsiniz."
            : "See which repositories each statement is based on.";

    public string GetRepositoryEvidenceDisclosureDescription(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Analizde incelenen public repository'leri burada görebilirsiniz."
            : "See the public repositories examined in this analysis.";

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
                "Technical direction",
            (ProfileReportSectionKey.TechnicalFocusAreas, WebCulture.Turkish) =>
                "Teknik yön",
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
                "Limits of this analysis",
            (ProfileReportSectionKey.Limitations, WebCulture.Turkish) =>
                "Bu analizin sınırları",
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

        // Session 141 (contract 04). The same four deterministic states, the
        // same two paragraphs and the same provenance line — roughly half the
        // words, and without restating one recurrence fact in a second
        // sentence. Selection is still keyed only by whether each of the two
        // canonical pattern sections has items; no new inference is added.
        return (technicalLabels.Count > 0, practices.Count > 0) switch
        {
            // Variant A — technical direction and working style both recur.
            (true, true) => isTurkish
                ? new ProfileMeaningPresentation(
                    $"{profileIdentity} GitHub çalışmalarında {technicalLabelsText} farklı repository'lerde tekrar ediyor.",
                    $"{practiceLabelsSentenceStart} gibi çalışma biçimleri de birden fazla repository'de tekrar görülüyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    $"{profileIdentity}'s GitHub work shows {technicalLabelsText} recurring across different repositories.",
                    $"Working methods such as {practiceLabelsText} also recur across multiple repositories.",
                    EnglishMeaningProvenance),

            // Variant B — technical direction recurs, working style does not.
            (true, false) => isTurkish
                ? new ProfileMeaningPresentation(
                    $"{profileIdentity} GitHub çalışmalarında {technicalLabelsText} farklı repository'lerde tekrar ediyor.",
                    "Repository'ler arasında tekrar eden belirgin bir çalışma biçimi görünmüyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    $"{profileIdentity}'s GitHub work shows {technicalLabelsText} recurring across different repositories.",
                    "No clear working method recurs across the repositories.",
                    EnglishMeaningProvenance),

            // Variant C — working style recurs, technical direction does not.
            (false, true) => isTurkish
                ? new ProfileMeaningPresentation(
                    "Repository'ler arasında tekrar eden belirgin bir teknik yön görünmüyor.",
                    $"Buna karşılık {practiceLabelsText} gibi çalışma biçimleri birden fazla repository'de tekrar görülüyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    "No clear technical direction recurs across the repositories.",
                    $"Working methods such as {practiceLabelsText} do recur across multiple repositories.",
                    EnglishMeaningProvenance),

            // Variant D — neither recurs. The second paragraph must not repeat
            // the same absence, so it stays the shortest authorized fallback.
            _ => isTurkish
                ? new ProfileMeaningPresentation(
                    "Public repository'ler arasında belirgin biçimde tekrar eden bir teknik yön veya çalışma biçimi görünmüyor.",
                    "Çalışma biçimi için de ek bir tekrar görünmüyor.",
                    TurkishMeaningProvenance)
                : new ProfileMeaningPresentation(
                    "No clear technical direction or working method recurs across the public repositories.",
                    "No additional recurring working method is visible.",
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
                "No clear technical direction recurs across the public repositories.",
            (ProfileReportSectionKey.TechnicalFocusAreas, WebCulture.Turkish) =>
                "Public repository'ler arasında tekrar eden belirgin bir teknik yön görünmüyor.",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.English) =>
                "No clear working method recurs across the public repositories.",
            (ProfileReportSectionKey.EngineeringPracticePatterns, WebCulture.Turkish) =>
                "Farklı çalışmalarda tekrar eden belirgin bir yöntem görünmüyor.",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.English) =>
                "No starting point is visible right now.",
            (ProfileReportSectionKey.RepresentativeRepositories, WebCulture.Turkish) =>
                "Şu anda bir başlangıç noktası görünmüyor.",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.English) =>
                "There is no recurring trace to explain here.",
            (ProfileReportSectionKey.WhyThisConclusion, WebCulture.Turkish) =>
                "Burada açıklanacak tekrar eden bir iz yok.",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.English) =>
                "No public repository is available to inspect in this analysis.",
            (ProfileReportSectionKey.RepositoryDrillDown, WebCulture.Turkish) =>
                "Bu analizde incelenebilecek bir public repository görünmüyor.",
            // Kept for the contract's exhaustiveness, but Session 141
            // (contract 10 §7) prefers not rendering the Limitations section at
            // all when the collection is empty, so this is no longer reached by
            // the Profile page.
            (ProfileReportSectionKey.Limitations, WebCulture.English) =>
                "No additional limitations are visible for this analysis.",
            (ProfileReportSectionKey.Limitations, WebCulture.Turkish) =>
                "Bu analiz için ek bir sınırlama görünmüyor.",
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
                "Contributed evidence to the profile analysis",
            (RepositoryEligibilityStatus.Contributing, WebCulture.Turkish) =>
                "Profil analizine kanıt sağladı",
            // Deliberately "with restrictions" and not "limited evidence": the
            // canonical state says the contribution was restricted, never that
            // the evidence quantity itself was small (contract 11 §5).
            (RepositoryEligibilityStatus.Restricted, WebCulture.English) =>
                "Contributed evidence to the profile analysis with some restrictions",
            (RepositoryEligibilityStatus.Restricted, WebCulture.Turkish) =>
                "Profil analizine bazı kısıtlamalarla kanıt sağladı",
            (RepositoryEligibilityStatus.NonContributing, WebCulture.English) =>
                "Did not contribute evidence to the profile analysis",
            (RepositoryEligibilityStatus.NonContributing, WebCulture.Turkish) =>
                "Profil analizine kanıt sağlamadı",
            // Undetermined says only that the contribution could not be
            // determined — never that data was missing or the repository
            // failed (contract 11 §7).
            (RepositoryEligibilityStatus.Undetermined, WebCulture.English) =>
                "It could not be determined whether this repository contributed evidence to the profile analysis.",
            (RepositoryEligibilityStatus.Undetermined, WebCulture.Turkish) =>
                "Profil analizine kanıt sağlayıp sağlamadığı belirlenemedi",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    // Fixed P10-I4 presentation slots (presentation-language-contract.md
    // §8.6.7/§13.8 Session 104 daraltması): none of these carries a canonical
    // semantic identity of its own, so each lives in its own narrow method
    // keyed only by the selected request culture — never by presentation text,
    // and no new analysis/Domain/Application identity is created to key them.
    public string GetWhySupportingRepositoriesLeadIn(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Görüldüğü repository'ler"
            : "Repositories where it appears";

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

        // Session 141 (contract 08): a short "Because …" reason. The two
        // families keep separate mappings — the switch is still exhaustive over
        // the family identity and still fails loudly for an unsupported one —
        // even though the approved reason sentence is now the same for both.
        return (family, isTurkish) switch
        {
            (ProfilePatternFamily.TechnicalFocus, true) =>
                $"Çünkü {label} birden fazla repository'de tekrar görülüyor.",
            (ProfilePatternFamily.TechnicalFocus, false) =>
                $"Because {label} appears across multiple repositories.",
            (ProfilePatternFamily.EngineeringPractice, true) =>
                $"Çünkü {label} birden fazla repository'de tekrar görülüyor.",
            (ProfilePatternFamily.EngineeringPractice, false) =>
                $"Because {label} appears across multiple repositories.",
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
            ? "Bir repository seçtiğinizde yeni bir Repository Proof analizi başlar."
            : "Selecting a repository starts a new Repository Proof analysis.";

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

        // Session 141 (contract 07): the relationship is stated directly
        // through the pattern labels themselves, so the internal phrase
        // "recurring result" leaves the user surface and the singular/plural
        // split it forced is no longer needed. No number, ranking or
        // recommendation is rendered, and the labels are identities the
        // first-read pattern cards already show.
        return isTurkish
            ? $"{joined} için başlangıç noktası"
            : $"Starting point for {joined}";
    }

    // Contract 07 §"Repository Action" / 11 §8: Starting Point and Repository
    // Evidence must show the same action language, because both start the same
    // fresh Repository Proof analysis.
    public string GetRepositoryActionLabel(string? culture) =>
        WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? "Repository Proof'u gör"
            : "View Repository Proof";

    // Repository FullName is data, substituted into the locked format; it is
    // never translated.
    public string GetRepositoryActionAccessibleName(
        string repositoryFullName,
        string? culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryFullName);

        return WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
            ? $"{repositoryFullName} için Repository Proof'u gör"
            : $"View Repository Proof for {repositoryFullName}";
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
                "There are limited traces available to evaluate in the public GitHub work.",
            (ProfileLimitationCode.LimitedIndependentEvidenceSurface, WebCulture.Turkish) =>
                "Public GitHub'da değerlendirilebilecek izler sınırlı.",
            // "Incomplete" and "unavailable" stay two separate conditions; they
            // are never collapsed into one stronger claim (contract 10 §6).
            (ProfileLimitationCode.PartialEvidenceSurface, WebCulture.English) =>
                "Some public repository data was incomplete or unavailable.",
            (ProfileLimitationCode.PartialEvidenceSurface, WebCulture.Turkish) =>
                "Bazı public repository verileri eksik veya erişilemez durumdaydı.",
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
                    ? "Bazı repository verilerine erişilemedi."
                    : "Some repository data was unavailable.",
            ProfileWarningCode.RepositoryEvidenceRateLimited =>
                isTurkish
                    ? "GitHub hız sınırı nedeniyle bazı repository verilerine erişilemedi."
                    : "GitHub rate limiting prevented access to some repository data.",
            ProfileWarningCode.RepositoryAnalysisFailed =>
                isTurkish
                    ? "Bazı repository analizleri tamamlanamadı."
                    : "Some repository analyses could not be completed.",
            // Contract 10 §3 asked for the internal "eligibility" wording to go,
            // but required the replacement to stay semantically equivalent to
            // the canonical state. Its suggested "enough data could not be
            // obtained" is a stronger claim than the state supports: an
            // Undetermined eligibility also arises from
            // EligibilityReason.IndependenceUndetermined, where the analysis
            // itself succeeded and only the fork/template lineage stayed
            // inconclusive. This wording drops "eligibility" while asserting
            // exactly what the state asserts, and matches the row-status copy
            // for the same canonical state (contract 11 §7).
            ProfileWarningCode.RepositoryEligibilityUndetermined =>
                isTurkish
                    ? "Bazı repository'lerin analize kanıt sağlayıp sağlamadığı belirlenemedi."
                    : "It could not be determined whether some repositories contributed evidence to the analysis.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}

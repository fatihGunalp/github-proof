namespace GitHubProof.Web.Localization;

public sealed class WebPresentationLocalizer
{
    private static readonly IReadOnlyDictionary<WebTextKey, string> English =
        new Dictionary<WebTextKey, string>
        {
            [WebTextKey.ThemeSwitchToLight] = "Switch to light theme",
            [WebTextKey.ThemeSwitchToDark] = "Switch to dark theme",
            [WebTextKey.LanguageSelectorLabel] = "Presentation language",
            [WebTextKey.LandingTitle] = "See what your GitHub communicates.",
            [WebTextKey.LandingSummary] = "See what the visible traces across your public GitHub work communicate.",
            [WebTextKey.BoundariesSummary] = "GitHub Proof does not evaluate you; it only shows traces visible in public GitHub work.",
            [WebTextKey.BoundariesDisclosureLabel] = "View boundaries →",
            [WebTextKey.NoDeveloperRankingTitle] = "No developer ranking",
            [WebTextKey.NoSeniorityTitle] = "No seniority judgment",
            [WebTextKey.NoAiAuthorshipTitle] = "No AI authorship claims",
            [WebTextKey.NoHireVerdictTitle] = "No hire/no-hire verdict",
            [WebTextKey.InputLabel] = "GitHub username, profile, or repository URL",
            [WebTextKey.InputPlaceholder] = "octocat or https://github.com/owner/repository",
            [WebTextKey.Analyze] = "See what it communicates",
            [WebTextKey.ProcessingRequest] = "Starting the analysis…",
            [WebTextKey.InvalidTarget] = "Enter a GitHub username (octocat), a profile URL (https://github.com/octocat), or a repository URL (https://github.com/owner/repository).",
            [WebTextKey.RepositoryPublicOnly] = "Only public GitHub repositories can be analyzed.",
            [WebTextKey.RepositoryRateLimited] = "GitHub's rate limit was reached. Try again later.",
            [WebTextKey.RepositoryForbidden] = "GitHub denied access to this repository. Confirm that it is public and accessible.",
            [WebTextKey.RepositoryFailed] = "GitHub could not be reached reliably. Try the analysis again.",
            [WebTextKey.RepositoryUnavailable] = "We could not access that public GitHub repository. Check the URL and try again.",
            [WebTextKey.ProfileCouldNotCompose] = "We could not build a profile analysis from that GitHub account's public data.",
            [WebTextKey.ProfilePublicOnly] = "Only public GitHub profiles can be analyzed.",
            [WebTextKey.ProfileRateLimited] = "GitHub temporarily limited access to this profile's public data. Try again later.",
            [WebTextKey.ProfileForbidden] = "GitHub denied access to this profile. Confirm that it is public and accessible.",
            [WebTextKey.ProfileFailed] = "GitHub could not be reached reliably. Try the analysis again.",
            [WebTextKey.ProfileUnavailable] = "We could not access that public GitHub profile. Check the username and try again.",
            [WebTextKey.ProfilePartialTitle] = "Some data is missing",
            [WebTextKey.ProfilePartialMessage] = "The result is available, but some public GitHub data could not be accessed. The view below is based only on the traces that were available.",
            [WebTextKey.ProfileObservationNotesTitle] = "Analysis notes",
            [WebTextKey.ProfileRequestLimitedTitle] = "Profile requests are temporarily limited",
            [WebTextKey.ProfileRequestLimited] = "This profile request could not start right now. Try again later.",
            [WebTextKey.ProfileAnalysisInProgressTitle] = "This profile is already being analyzed",
            [WebTextKey.ProfileAnalysisInProgress] = "Another analysis of this profile is still in progress. Try again after it finishes.",
            [WebTextKey.ProfileServiceBusyTitle] = "Profile analysis is temporarily busy",
            [WebTextKey.ProfileServiceBusy] = "The service cannot start another profile analysis right now. Try again later.",
            [WebTextKey.ProfileProviderInterruptedTitle] = "Profile analysis is temporarily unavailable",
            [WebTextKey.ProfileProviderInterrupted] = "The analysis could not continue with GitHub right now. Try again later.",
            [WebTextKey.ProfileDeadlineTitle] = "Profile analysis took too long",
            [WebTextKey.ProfileDeadline] = "The analysis could not be completed within the available time. Try again later.",
            [WebTextKey.ProfileUnavailableTitle] = "Profile unavailable",
            [WebTextKey.ProfileUnsupportedTitle] = "This profile is not supported",
            [WebTextKey.ProfileCompositionFailedTitle] = "Profile analysis could not be completed",
            [WebTextKey.AnalyzeSomethingElse] = "Check another GitHub",
            [WebTextKey.ReportPreviewTitle] = "Repository Report Preview",
            [WebTextKey.SignalFieldWhatWeCheck] = "What we check",
            [WebTextKey.SignalFieldWhatWeFound] = "What we found",
            [WebTextKey.SignalFieldWhyItMatters] = "Why it matters",
            [WebTextKey.SignalFieldWhatYouCanDo] = "What you can do",
            [WebTextKey.SignalFieldLimit] = "Limit",
            [WebTextKey.SignalFieldEvidence] = "Evidence",
            [WebTextKey.ProgressRepositoryTargetPending] = "Repository check",
            [WebTextKey.ProgressRepositoryTargetActive] = "Checking the repository",
            [WebTextKey.ProgressRepositoryTargetCompleted] = "Repository checked",
            [WebTextKey.ProgressRepositoryDataPending] = "Public repository data",
            [WebTextKey.ProgressRepositoryDataActive] = "Looking through public repository data",
            [WebTextKey.ProgressRepositoryDataCompleted] = "Public repository data reviewed",
            [WebTextKey.ProgressRepositoryTracesPending] = "Trace review",
            [WebTextKey.ProgressRepositoryTracesActive] = "Reviewing the traces",
            [WebTextKey.ProgressRepositoryTracesCompleted] = "Traces reviewed",
            [WebTextKey.ProgressRepositoryProofPending] = "Repository Proof",
            [WebTextKey.ProgressRepositoryProofActive] = "Preparing Repository Proof",
            [WebTextKey.ProgressRepositoryProofCompleted] = "Repository Proof ready",
            [WebTextKey.ProgressProfileTargetPending] = "GitHub profile check",
            [WebTextKey.ProgressProfileTargetActive] = "Checking the GitHub profile",
            [WebTextKey.ProgressProfileTargetCompleted] = "GitHub profile checked",
            [WebTextKey.ProgressProfileRepositoriesPending] = "Public repositories",
            [WebTextKey.ProgressProfileRepositoriesActive] = "Looking through public repositories",
            [WebTextKey.ProgressProfileRepositoriesCompleted] = "Public repositories reviewed",
            [WebTextKey.ProgressProfileTracesPending] = "Recurring traces",
            [WebTextKey.ProgressProfileTracesActive] = "Looking for recurring traces",
            [WebTextKey.ProgressProfileTracesCompleted] = "Recurring traces reviewed"
        };

    private static readonly IReadOnlyDictionary<WebTextKey, string> Turkish =
        new Dictionary<WebTextKey, string>
        {
            [WebTextKey.ThemeSwitchToLight] = "Açık temaya geç",
            [WebTextKey.ThemeSwitchToDark] = "Koyu temaya geç",
            [WebTextKey.LanguageSelectorLabel] = "Sunum dili",
            [WebTextKey.LandingTitle] = "GitHub'ınızda ne olduğunu değil, ne anlattığını görün.",
            [WebTextKey.LandingSummary] = "Public GitHub çalışmalarınızdaki görünür izleri bir araya getirerek ne anlattığını görün.",
            [WebTextKey.BoundariesSummary] = "GitHub Proof sizi değerlendirmez; yalnızca public GitHub'da görülebilen izleri gösterir.",
            [WebTextKey.BoundariesDisclosureLabel] = "Sınırları gör →",
            [WebTextKey.NoDeveloperRankingTitle] = "Geliştirici sıralaması yok",
            [WebTextKey.NoSeniorityTitle] = "Kıdem yargısı yok",
            [WebTextKey.NoAiAuthorshipTitle] = "Yapay zekâ yazarlığı iddiası yok",
            [WebTextKey.NoHireVerdictTitle] = "İşe al / alma hükmü yok",
            [WebTextKey.InputLabel] = "GitHub kullanıcı adı, profil veya repository URL'si",
            [WebTextKey.InputPlaceholder] = "octocat veya https://github.com/owner/repository",
            [WebTextKey.Analyze] = "Ne anlattığını gör",
            [WebTextKey.ProcessingRequest] = "Analiz başlıyor…",
            [WebTextKey.InvalidTarget] = "Bir GitHub kullanıcı adı (octocat), profil URL'si (https://github.com/octocat) veya repository URL'si (https://github.com/owner/repository) girin.",
            [WebTextKey.RepositoryPublicOnly] = "Yalnızca public GitHub repository'leri analiz edilebilir.",
            [WebTextKey.RepositoryRateLimited] = "GitHub hız sınırına ulaşıldı. Daha sonra yeniden deneyin.",
            [WebTextKey.RepositoryForbidden] = "GitHub bu repository'ye erişimi reddetti. Public ve erişilebilir olduğunu doğrulayın.",
            [WebTextKey.RepositoryFailed] = "GitHub'a güvenilir biçimde ulaşılamadı. Analizi yeniden deneyin.",
            [WebTextKey.RepositoryUnavailable] = "Bu public GitHub repository'sine erişemedik. URL'yi kontrol edip yeniden deneyin.",
            [WebTextKey.ProfileCouldNotCompose] = "Bu GitHub hesabının public verilerinden bir profil analizi oluşturamadık.",
            [WebTextKey.ProfilePublicOnly] = "Yalnızca public GitHub profilleri analiz edilebilir.",
            [WebTextKey.ProfileRateLimited] = "GitHub bu profilin public verilerine erişimi geçici olarak sınırladı. Daha sonra yeniden deneyin.",
            [WebTextKey.ProfileForbidden] = "GitHub bu profile erişimi reddetti. Public ve erişilebilir olduğunu doğrulayın.",
            [WebTextKey.ProfileFailed] = "GitHub'a güvenilir biçimde ulaşılamadı. Analizi yeniden deneyin.",
            [WebTextKey.ProfileUnavailable] = "Bu public GitHub profiline erişemedik. Kullanıcı adını kontrol edip yeniden deneyin.",
            [WebTextKey.ProfilePartialTitle] = "Bazı veriler eksik",
            [WebTextKey.ProfilePartialMessage] = "Sonuç hazır; ancak bazı public GitHub verilerine erişilemedi. Aşağıdaki anlatım yalnızca erişilebilen izlere dayanıyor.",
            [WebTextKey.ProfileObservationNotesTitle] = "Analiz notları",
            [WebTextKey.ProfileRequestLimitedTitle] = "Profil istekleri geçici olarak sınırlandı",
            [WebTextKey.ProfileRequestLimited] = "Bu profil isteği şu anda başlatılamadı. Daha sonra yeniden deneyin.",
            [WebTextKey.ProfileAnalysisInProgressTitle] = "Bu profil zaten analiz ediliyor",
            [WebTextKey.ProfileAnalysisInProgress] = "Bu profil için başka bir analiz hâlâ sürüyor. Tamamlandıktan sonra yeniden deneyin.",
            [WebTextKey.ProfileServiceBusyTitle] = "Profil analizi geçici olarak yoğun",
            [WebTextKey.ProfileServiceBusy] = "Hizmet şu anda yeni bir profil analizi başlatamıyor. Daha sonra yeniden deneyin.",
            [WebTextKey.ProfileProviderInterruptedTitle] = "Profil analizi geçici olarak kullanılamıyor",
            [WebTextKey.ProfileProviderInterrupted] = "Analiz şu anda GitHub ile devam edemedi. Daha sonra yeniden deneyin.",
            [WebTextKey.ProfileDeadlineTitle] = "Profil analizi çok uzun sürdü",
            [WebTextKey.ProfileDeadline] = "Analiz kullanılabilir süre içinde tamamlanamadı. Daha sonra yeniden deneyin.",
            [WebTextKey.ProfileUnavailableTitle] = "Profile erişilemiyor",
            [WebTextKey.ProfileUnsupportedTitle] = "Bu profil desteklenmiyor",
            [WebTextKey.ProfileCompositionFailedTitle] = "Profil analizi tamamlanamadı",
            [WebTextKey.AnalyzeSomethingElse] = "Başka bir GitHub'ı incele",
            [WebTextKey.ReportPreviewTitle] = "Repository Raporu Önizlemesi",
            [WebTextKey.SignalFieldWhatWeCheck] = "Neye bakıyoruz?",
            [WebTextKey.SignalFieldWhatWeFound] = "Ne gördük?",
            [WebTextKey.SignalFieldWhyItMatters] = "Neden önemli?",
            [WebTextKey.SignalFieldWhatYouCanDo] = "Ne yapabilirsiniz?",
            [WebTextKey.SignalFieldLimit] = "Sınır",
            [WebTextKey.SignalFieldEvidence] = "Kanıt",
            [WebTextKey.ProgressRepositoryTargetPending] = "Repository kontrol edilecek",
            [WebTextKey.ProgressRepositoryTargetActive] = "Repository'yi kontrol ediyoruz",
            [WebTextKey.ProgressRepositoryTargetCompleted] = "Repository kontrol edildi",
            [WebTextKey.ProgressRepositoryDataPending] = "Public repository verilerine bakılacak",
            [WebTextKey.ProgressRepositoryDataActive] = "Repository'deki public verilere bakıyoruz",
            [WebTextKey.ProgressRepositoryDataCompleted] = "Public repository verileri incelendi",
            [WebTextKey.ProgressRepositoryTracesPending] = "İzler değerlendirilecek",
            [WebTextKey.ProgressRepositoryTracesActive] = "İzleri değerlendiriyoruz",
            [WebTextKey.ProgressRepositoryTracesCompleted] = "İzler değerlendirildi",
            [WebTextKey.ProgressRepositoryProofPending] = "Repository Proof hazırlanacak",
            [WebTextKey.ProgressRepositoryProofActive] = "Repository Proof'u hazırlıyoruz",
            [WebTextKey.ProgressRepositoryProofCompleted] = "Repository Proof hazır",
            [WebTextKey.ProgressProfileTargetPending] = "GitHub profili kontrol edilecek",
            [WebTextKey.ProgressProfileTargetActive] = "GitHub profilini kontrol ediyoruz",
            [WebTextKey.ProgressProfileTargetCompleted] = "GitHub profili kontrol edildi",
            [WebTextKey.ProgressProfileRepositoriesPending] = "Public repository'lere bakılacak",
            [WebTextKey.ProgressProfileRepositoriesActive] = "Public repository'lere bakıyoruz",
            [WebTextKey.ProgressProfileRepositoriesCompleted] = "Public repository'lere bakıldı",
            [WebTextKey.ProgressProfileTracesPending] = "Tekrar eden izlere bakılacak",
            [WebTextKey.ProgressProfileTracesActive] = "Tekrar eden izlere bakıyoruz",
            [WebTextKey.ProgressProfileTracesCompleted] = "Tekrar eden izler incelendi"
        };

    public string Culture =>
        WebCulture.NormalizeOrEnglish(
            System.Globalization.CultureInfo.CurrentUICulture.Name);

    public string Get(WebTextKey key) =>
        Get(key, Culture);

    public string Get(WebTextKey key, HttpContext? context) =>
        Get(key, WebCulture.Resolve(context));

    public string Get(WebTextKey key, string? culture)
    {
        IReadOnlyDictionary<WebTextKey, string> catalog =
            WebCulture.NormalizeOrEnglish(culture) == WebCulture.Turkish
                ? Turkish
                : English;

        return catalog.TryGetValue(key, out string? value)
            ? value
            : throw new InvalidOperationException(
                $"Missing Web presentation text '{key}' for culture '{culture}'.");
    }
}

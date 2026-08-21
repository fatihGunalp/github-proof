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
            [WebTextKey.LandingSummary] = "Discover the bigger picture formed by the visible traces in your GitHub profile.",
            [WebTextKey.BoundariesSummary] = "GitHub Proof doesn't rank developers or judge seniority, AI authorship, or hiring.",
            [WebTextKey.BoundariesDisclosureLabel] = "View boundaries →",
            [WebTextKey.NoDeveloperRankingTitle] = "No developer ranking",
            [WebTextKey.NoSeniorityTitle] = "No seniority judgment",
            [WebTextKey.NoAiAuthorshipTitle] = "No AI authorship claims",
            [WebTextKey.NoHireVerdictTitle] = "No hire/no-hire verdict",
            [WebTextKey.InputLabel] = "GitHub username, profile URL, or repository URL",
            [WebTextKey.InputPlaceholder] = "octocat or https://github.com/owner/repository",
            [WebTextKey.Analyze] = "Analyze",
            [WebTextKey.ProcessingRequest] = "GitHub Proof is processing your request…",
            [WebTextKey.InvalidTarget] = "Enter a GitHub username (octocat), a profile URL (https://github.com/octocat), or a repository URL (https://github.com/owner/repository).",
            [WebTextKey.RepositoryPublicOnly] = "Only public GitHub repositories are supported in the current MVP.",
            [WebTextKey.RepositoryRateLimited] = "GitHub's API rate limit was reached. Try again later or configure GITHUB_TOKEN.",
            [WebTextKey.RepositoryForbidden] = "GitHub denied access to this repository. Confirm that it is public and accessible.",
            [WebTextKey.RepositoryFailed] = "GitHub could not be reached reliably. Try the analysis again.",
            [WebTextKey.RepositoryUnavailable] = "We could not access that public GitHub repository. Check the URL and try again.",
            [WebTextKey.ProfileView] = "Profile view",
            [WebTextKey.ProfileCollected] = "Public evidence was collected and composed for this profile.",
            [WebTextKey.ProfileCouldNotCompose] = "We could not compose a profile from that GitHub account's public evidence.",
            [WebTextKey.ProfilePublicOnly] = "Only public GitHub profiles are supported in the current MVP.",
            [WebTextKey.ProfileRateLimited] = "GitHub's API rate limit was reached. Try again later or configure GITHUB_TOKEN.",
            [WebTextKey.ProfileForbidden] = "GitHub denied access to this profile. Confirm that it is public and accessible.",
            [WebTextKey.ProfileFailed] = "GitHub could not be reached reliably. Try the analysis again.",
            [WebTextKey.ProfileUnavailable] = "We could not access that public GitHub profile. Check the username and try again.",
            [WebTextKey.AnalyzeSomethingElse] = "Analyze something else",
            [WebTextKey.ReportPreviewTitle] = "Repository Report Preview"
        };

    private static readonly IReadOnlyDictionary<WebTextKey, string> Turkish =
        new Dictionary<WebTextKey, string>
        {
            [WebTextKey.ThemeSwitchToLight] = "Açık temaya geç",
            [WebTextKey.ThemeSwitchToDark] = "Koyu temaya geç",
            [WebTextKey.LanguageSelectorLabel] = "Sunum dili",
            [WebTextKey.LandingTitle] = "GitHub'ınızda ne olduğunu değil, ne anlattığını görün.",
            [WebTextKey.LandingSummary] = "GitHub profilinizde bıraktığınız izlerden oluşan büyük resmi keşfedin.",
            [WebTextKey.BoundariesSummary] = "GitHub Proof geliştiricileri puanlamaz; kıdem, AI yazarlığı veya işe alım hükmü vermez.",
            [WebTextKey.BoundariesDisclosureLabel] = "Sınırları gör →",
            [WebTextKey.NoDeveloperRankingTitle] = "Geliştirici sıralaması yok",
            [WebTextKey.NoSeniorityTitle] = "Kıdem yargısı yok",
            [WebTextKey.NoAiAuthorshipTitle] = "Yapay zekâ yazarlığı iddiası yok",
            [WebTextKey.NoHireVerdictTitle] = "İşe al / alma hükmü yok",
            [WebTextKey.InputLabel] = "GitHub kullanıcı adı, profil URL'si veya repository URL'si",
            [WebTextKey.InputPlaceholder] = "octocat veya https://github.com/owner/repository",
            [WebTextKey.Analyze] = "Analiz et",
            [WebTextKey.ProcessingRequest] = "GitHub Proof isteğinizi işliyor…",
            [WebTextKey.InvalidTarget] = "Bir GitHub kullanıcı adı (octocat), profil URL'si (https://github.com/octocat) veya repository URL'si (https://github.com/owner/repository) girin.",
            [WebTextKey.RepositoryPublicOnly] = "Mevcut MVP yalnız herkese açık GitHub repository'lerini destekler.",
            [WebTextKey.RepositoryRateLimited] = "GitHub API hız sınırına ulaşıldı. Daha sonra yeniden deneyin veya GITHUB_TOKEN yapılandırın.",
            [WebTextKey.RepositoryForbidden] = "GitHub bu repository'ye erişimi reddetti. Herkese açık ve erişilebilir olduğunu doğrulayın.",
            [WebTextKey.RepositoryFailed] = "GitHub'a güvenilir biçimde ulaşılamadı. Analizi yeniden deneyin.",
            [WebTextKey.RepositoryUnavailable] = "Bu herkese açık GitHub repository'sine erişemedik. URL'yi kontrol edip yeniden deneyin.",
            [WebTextKey.ProfileView] = "Profil görünümü",
            [WebTextKey.ProfileCollected] = "Bu profil için herkese açık kanıt toplandı ve bir araya getirildi.",
            [WebTextKey.ProfileCouldNotCompose] = "Bu GitHub hesabının herkese açık kanıtlarından bir profil görünümü oluşturamadık.",
            [WebTextKey.ProfilePublicOnly] = "Mevcut MVP yalnız herkese açık GitHub profillerini destekler.",
            [WebTextKey.ProfileRateLimited] = "GitHub API hız sınırına ulaşıldı. Daha sonra yeniden deneyin veya GITHUB_TOKEN yapılandırın.",
            [WebTextKey.ProfileForbidden] = "GitHub bu profile erişimi reddetti. Herkese açık ve erişilebilir olduğunu doğrulayın.",
            [WebTextKey.ProfileFailed] = "GitHub'a güvenilir biçimde ulaşılamadı. Analizi yeniden deneyin.",
            [WebTextKey.ProfileUnavailable] = "Bu herkese açık GitHub profiline erişemedik. Kullanıcı adını kontrol edip yeniden deneyin.",
            [WebTextKey.AnalyzeSomethingElse] = "Başka bir GitHub hedefini analiz et",
            [WebTextKey.ReportPreviewTitle] = "Repository Raporu Önizlemesi"
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

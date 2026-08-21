using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace GitHubProof.Web.Localization;

public static class WebCulture
{
    public const string English = "en";
    public const string Turkish = "tr";

    public static IReadOnlyList<CultureInfo> SupportedCultures { get; } =
        Array.AsReadOnly(
        [
            CultureInfo.GetCultureInfo(English),
            CultureInfo.GetCultureInfo(Turkish)
        ]);

    public static bool TryNormalize(string? culture, out string normalized)
    {
        string language = culture?
            .Trim()
            .Split('-', '_')[0]
            .ToLowerInvariant() ?? string.Empty;

        if (language is English or Turkish)
        {
            normalized = language;
            return true;
        }

        normalized = English;
        return false;
    }

    public static string NormalizeOrEnglish(string? culture) =>
        TryNormalize(culture, out string normalized)
            ? normalized
            : English;

    public static string Resolve(HttpContext? context)
    {
        if (context is null)
        {
            return English;
        }

        string? requestCulture = context?
            .Features
            .Get<IRequestCultureFeature>()?
            .RequestCulture
            .UICulture
            .Name;

        return NormalizeOrEnglish(requestCulture);
    }
}

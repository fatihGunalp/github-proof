using System.Globalization;
using System.Resources;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Application.Analysis.Presentation;

public sealed class SignalPresentationLocalizer
{
    public const string DefaultLocale = "en";
    public const string TurkishLocale = "tr";

    private static readonly ResourceManager Resources = new(
        "GitHubProof.Application.Analysis.Presentation.Resources.SignalPresentation",
        typeof(SignalPresentationLocalizer).Assembly);

    private static readonly HashSet<string> CanonicalSignalKeys =
    [
        "repository.readme.exists",
        "repository.description.present",
        "repository.license.present",
        "documentation.installation-guide",
        "documentation.usage-or-demo",
        "process.commit-history",
        "process.commit-distribution",
        "process.issue-usage",
        "process.pull-request-usage",
        "process.release-usage",
        "validation.test-structure",
        "validation.continuous-integration",
        "portfolio.topics",
        "portfolio.homepage-or-live-demo",
        "portfolio.presentation-completeness"
    ];

    public SignalPresentation Present(SignalResult result, string? locale)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!CanonicalSignalKeys.Contains(result.SignalKey))
        {
            throw new ArgumentException(
                $"Unknown signal key '{result.SignalKey}'.",
                nameof(result));
        }

        string normalizedLocale = NormalizeLocale(locale);
        CultureInfo culture = CultureInfo.GetCultureInfo(normalizedLocale);
        string title = GetRequired($"{result.SignalKey}.title", culture);
        string statusLabel = GetRequired($"status.{result.Status}", culture);
        string explanationTemplate = GetRequired(
            $"explanation.{result.Status}",
            culture);
        string explanation = string.Format(
            culture,
            explanationTemplate,
            title,
            statusLabel,
            result.Score,
            result.MaximumScore);
        string? recommendation = RequiresRecommendation(result.Status)
            ? GetRequired($"{result.SignalKey}.recommendation", culture)
            : null;
        string limitation = GetRequired($"limitation.{result.Status}", culture);

        return new SignalPresentation(
            normalizedLocale,
            result.SignalKey,
            title,
            GetRequired($"{result.SignalKey}.description", culture),
            result.Status,
            statusLabel,
            result.Score,
            result.MaximumScore,
            explanation,
            GetRequired($"{result.SignalKey}.whyItMatters", culture),
            recommendation,
            [limitation],
            result.Evidence,
            result.SignalVersion);
    }

    public string GetCategoryLabel(AnalysisCategory category, string? locale) =>
        GetRequired(
            $"category.{category}",
            CultureInfo.GetCultureInfo(NormalizeLocale(locale)));

    public string GetProductBoundaryDisclaimer(string? locale) =>
        GetRequired(
            "disclaimer.product-boundary",
            CultureInfo.GetCultureInfo(NormalizeLocale(locale)));

    public static string NormalizeLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return DefaultLocale;
        }

        string language = locale.Trim().Split('-', '_')[0];
        return string.Equals(language, TurkishLocale, StringComparison.OrdinalIgnoreCase)
            ? TurkishLocale
            : DefaultLocale;
    }

    private static bool RequiresRecommendation(SignalStatus status) =>
        status is SignalStatus.Partial
            or SignalStatus.Missing
            or SignalStatus.RequiresHumanReview;

    private static string GetRequired(string key, CultureInfo culture) =>
        Resources.GetString(key, culture)
        ?? throw new InvalidOperationException(
            $"Missing presentation resource '{key}' for locale '{culture.Name}'.");
}

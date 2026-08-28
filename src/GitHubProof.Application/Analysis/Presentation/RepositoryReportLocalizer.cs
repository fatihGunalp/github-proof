using System.Globalization;
using System.Resources;
using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Application.Analysis.Presentation;

public sealed class RepositoryReportLocalizer
{
    public const int ProductBoundaryPolicyVersion = 1;

    private static readonly ResourceManager Resources = new(
        "GitHubProof.Application.Analysis.Presentation.Resources.RepositoryReport",
        typeof(RepositoryReportLocalizer).Assembly);

    private static readonly HashSet<string> KnownSectionKeys =
        RepositoryReportSectionKeys.CanonicalOrder.ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> KnownScoreLabelKeys =
        RepositoryReportScoreLabelKeys.All.ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> KnownWarningKeys =
        RepositoryReportWarningKeys.All.ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> KnownLimitationKeys =
        RepositoryReportLimitationKeys.All.ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> KnownActionKeys =
    [
        "action.improve.repository.readme.exists",
        "action.improve.repository.description.present",
        "action.improve.repository.license.present",
        "action.improve.documentation.installation-guide",
        "action.improve.documentation.usage-or-demo",
        "action.improve.process.commit-history",
        "action.improve.process.commit-distribution",
        "action.improve.process.issue-usage",
        "action.improve.process.pull-request-usage",
        "action.improve.process.release-usage",
        "action.improve.validation.test-structure",
        "action.improve.validation.continuous-integration",
        "action.improve.portfolio.topics",
        "action.improve.portfolio.homepage-or-live-demo"
    ];

    public string GetSectionTitle(string sectionKey, string? locale)
    {
        if (!KnownSectionKeys.Contains(sectionKey))
        {
            throw new ArgumentException(
                $"Unknown report section key '{sectionKey}'.",
                nameof(sectionKey));
        }

        return GetRequired(sectionKey, locale);
    }

    public string? GetScoreLabel(string? labelKey, string? locale)
    {
        if (labelKey is null)
        {
            return null;
        }

        if (!KnownScoreLabelKeys.Contains(labelKey))
        {
            throw new ArgumentException(
                $"Unknown score label key '{labelKey}'.",
                nameof(labelKey));
        }

        return GetRequired(labelKey, locale);
    }

    public string GetAnalysisStatusLabel(
        AnalysisStatus status,
        string? locale) =>
        GetRequired($"analysis-status.{status}", locale);

    public string GetCategoryStatusLabel(
        CategoryStatus status,
        string? locale) =>
        GetRequired($"category-status.{status}", locale);

    public string GetEffortLabel(
        EffortLevel effort,
        string? locale) =>
        GetRequired($"effort.{effort}", locale);

    public string GetActionTitle(
        string presentationKey,
        string? locale)
    {
        if (!KnownActionKeys.Contains(presentationKey))
        {
            throw new ArgumentException(
                $"Unknown improvement action presentation key '{presentationKey}'.",
                nameof(presentationKey));
        }

        return GetRequired(presentationKey, locale);
    }

    public string GetWarning(
        string presentationKey,
        string? locale)
    {
        if (!KnownWarningKeys.Contains(presentationKey))
        {
            throw new ArgumentException(
                $"Unknown warning presentation key '{presentationKey}'.",
                nameof(presentationKey));
        }

        return GetRequired(presentationKey, locale);
    }

    public string GetLimitation(
        string limitationKey,
        string? locale)
    {
        if (!KnownLimitationKeys.Contains(limitationKey))
        {
            throw new ArgumentException(
                $"Unknown limitation key '{limitationKey}'.",
                nameof(limitationKey));
        }

        return GetRequired(limitationKey, locale);
    }

    public string GetProductBoundaryDisclaimer(string? locale) =>
        GetRequired(
            $"disclaimer.product-boundary.v{ProductBoundaryPolicyVersion}",
            locale);

    private static string GetRequired(
        string key,
        string? locale)
    {
        string normalizedLocale = SignalPresentationLocalizer.NormalizeLocale(locale);
        CultureInfo culture = CultureInfo.GetCultureInfo(normalizedLocale);

        return Resources.GetString(key, culture)
            ?? throw new InvalidOperationException(
                $"Missing report presentation resource '{key}' for locale '{culture.Name}'.");
    }
}
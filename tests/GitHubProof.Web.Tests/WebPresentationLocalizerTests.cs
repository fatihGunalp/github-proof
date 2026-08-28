using GitHubProof.Web.Localization;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class WebPresentationLocalizerTests
{
    [Fact]
    public void Every_Web_text_identity_has_English_and_Turkish_copy()
    {
        WebPresentationLocalizer localizer = new();

        foreach (WebTextKey key in Enum.GetValues<WebTextKey>())
        {
            Assert.False(string.IsNullOrWhiteSpace(localizer.Get(key, WebCulture.English)));
            Assert.False(string.IsNullOrWhiteSpace(localizer.Get(key, WebCulture.Turkish)));
        }
    }

    [Fact]
    public void Unsupported_locale_falls_back_to_English()
    {
        WebPresentationLocalizer localizer = new();

        Assert.Equal(
            localizer.Get(WebTextKey.Analyze, WebCulture.English),
            localizer.Get(WebTextKey.Analyze, "de-DE"));
    }

    [Fact]
    public void Processing_copy_is_truthful_and_indeterminate_in_both_cultures()
    {
        WebPresentationLocalizer localizer = new();

        Assert.Equal(
            "Starting the analysis…",
            localizer.Get(WebTextKey.ProcessingRequest, WebCulture.English));
        Assert.Equal(
            "Analiz başlıyor…",
            localizer.Get(WebTextKey.ProcessingRequest, WebCulture.Turkish));
    }

    [Fact]
    public void P10I5_operational_states_have_distinct_bounded_copy_in_every_supported_culture()
    {
        WebPresentationLocalizer localizer = new();
        WebTextKey[] messages =
        [
            WebTextKey.ProfileRequestLimited,
            WebTextKey.ProfileAnalysisInProgress,
            WebTextKey.ProfileServiceBusy,
            WebTextKey.ProfileProviderInterrupted,
            WebTextKey.ProfileDeadline
        ];

        foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
        {
            string[] localized = messages
                .Select(key => localizer.Get(key, culture))
                .ToArray();

            Assert.Equal(localized.Length, localized.Distinct(StringComparer.Ordinal).Count());
        }
    }

    [Fact]
    public void P10I5_copy_does_not_expose_internal_guard_or_transport_vocabulary()
    {
        WebPresentationLocalizer localizer = new();
        WebTextKey[] keys =
        [
            WebTextKey.ProfilePartialTitle,
            WebTextKey.ProfilePartialMessage,
            WebTextKey.ProfileObservationNotesTitle,
            WebTextKey.ProfileRequestLimitedTitle,
            WebTextKey.ProfileRequestLimited,
            WebTextKey.ProfileAnalysisInProgressTitle,
            WebTextKey.ProfileAnalysisInProgress,
            WebTextKey.ProfileServiceBusyTitle,
            WebTextKey.ProfileServiceBusy,
            WebTextKey.ProfileProviderInterruptedTitle,
            WebTextKey.ProfileProviderInterrupted,
            WebTextKey.ProfileDeadlineTitle,
            WebTextKey.ProfileDeadline
        ];
        string[] prohibited =
        [
            "execution availability",
            "admission lease",
            "semaphore",
            "budget floor",
            "provider circuit",
            "pipeline",
            "cancellation token",
            "transport timeout",
            "remaining requests",
            "rate-limit header"
        ];

        foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
        {
            foreach (WebTextKey key in keys)
            {
                string value = localizer.Get(key, culture);

                foreach (string term in prohibited)
                {
                    Assert.DoesNotContain(term, value, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}

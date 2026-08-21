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
            "GitHub Proof is processing your request…",
            localizer.Get(WebTextKey.ProcessingRequest, WebCulture.English));
        Assert.Equal(
            "GitHub Proof isteğinizi işliyor…",
            localizer.Get(WebTextKey.ProcessingRequest, WebCulture.Turkish));
    }
}

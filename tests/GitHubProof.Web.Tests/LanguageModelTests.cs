using GitHubProof.Web.Localization;
using GitHubProof.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class LanguageModelTests
{
    [Theory]
    [InlineData("tr-TR", WebCulture.Turkish)]
    [InlineData("en-GB", WebCulture.English)]
    public void Explicit_supported_choice_is_normalized_persisted_and_returns_to_landing(
        string requested,
        string expected)
    {
        DefaultHttpContext context = new();
        LanguageModel model = CreateModel(context);
        model.Culture = requested;

        RedirectToPageResult result =
            Assert.IsType<RedirectToPageResult>(model.OnPost());

        Assert.Equal("/Index", result.PageName);
        Assert.False(result.PreserveMethod);

        string setCookie = context.Response.Headers.SetCookie.ToString();
        Assert.Contains(
            CookieRequestCultureProvider.DefaultCookieName,
            setCookie,
            StringComparison.Ordinal);
        Assert.Contains(
            Uri.EscapeDataString(
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(expected))),
            setCookie,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Unsupported_choice_does_not_write_a_culture_cookie()
    {
        DefaultHttpContext context = new();
        LanguageModel model = CreateModel(context);
        model.Culture = "de-DE";

        RedirectToPageResult result =
            Assert.IsType<RedirectToPageResult>(model.OnPost());

        Assert.Equal("/Index", result.PageName);
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public void Get_never_runs_or_replays_analysis()
    {
        LanguageModel model = CreateModel(new DefaultHttpContext());

        RedirectToPageResult result =
            Assert.IsType<RedirectToPageResult>(model.OnGet());

        Assert.Equal("/Index", result.PageName);
        Assert.False(result.PreserveMethod);
    }

    private static LanguageModel CreateModel(HttpContext context) =>
        new()
        {
            PageContext = new PageContext
            {
                HttpContext = context
            }
        };
}

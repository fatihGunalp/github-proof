using GitHubProof.Web.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class WebRequestLocalizationTests
{
    [Fact]
    public void Configuration_supports_exactly_cookie_then_accept_language_then_english()
    {
        RequestLocalizationOptions options = CreateOptions();

        Assert.Equal(WebCulture.English, options.DefaultRequestCulture.Culture.Name);
        Assert.Equal(
            [WebCulture.English, WebCulture.Turkish],
            options.SupportedCultures!.Select(culture => culture.Name));
        Assert.Equal(
            [WebCulture.English, WebCulture.Turkish],
            options.SupportedUICultures!.Select(culture => culture.Name));
        Assert.Collection(
            options.RequestCultureProviders,
            provider => Assert.IsType<CookieRequestCultureProvider>(provider),
            provider => Assert.IsType<AcceptLanguageHeaderRequestCultureProvider>(provider));
        Assert.DoesNotContain(
            options.RequestCultureProviders,
            provider => provider is QueryStringRequestCultureProvider);
    }

    [Fact]
    public async Task Persisted_explicit_choice_wins_over_browser_preference()
    {
        string culture = await ResolveAsync(
            acceptLanguage: "tr-TR,tr;q=0.9",
            cookieCulture: WebCulture.English);

        Assert.Equal(WebCulture.English, culture);
    }

    [Theory]
    [InlineData("tr-TR,tr;q=0.8", WebCulture.Turkish)]
    [InlineData("en-GB,en;q=0.8", WebCulture.English)]
    [InlineData("de-DE, tr-TR;q=0.8, en;q=0.7", WebCulture.Turkish)]
    [InlineData("de-DE, fr-FR;q=0.9, es-ES;q=0.8, tr-TR;q=0.7", WebCulture.Turkish)]
    [InlineData("de-DE, fr-FR;q=0.8", WebCulture.English)]
    [InlineData(null, WebCulture.English)]
    public async Task Browser_language_uses_supported_family_or_english_fallback(
        string? acceptLanguage,
        string expected)
    {
        string culture = await ResolveAsync(acceptLanguage, cookieCulture: null);

        Assert.Equal(expected, culture);
    }

    private static RequestLocalizationOptions CreateOptions()
    {
        ServiceCollection services = new();
        services.AddWebExperienceLocalization();

        using ServiceProvider provider = services.BuildServiceProvider();

        return provider
            .GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value;
    }

    private static async Task<string> ResolveAsync(
        string? acceptLanguage,
        string? cookieCulture)
    {
        RequestLocalizationOptions options = CreateOptions();
        DefaultHttpContext context = new();

        if (acceptLanguage is not null)
        {
            context.Request.Headers.AcceptLanguage = acceptLanguage;
        }

        if (cookieCulture is not null)
        {
            string value = CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(cookieCulture));
            context.Request.Headers.Cookie =
                $"{CookieRequestCultureProvider.DefaultCookieName}={Uri.EscapeDataString(value)}";
        }

        string? resolved = null;
        RequestDelegate next = requestContext =>
        {
            resolved = WebCulture.Resolve(requestContext);
            return Task.CompletedTask;
        };

        RequestLocalizationMiddleware middleware = new(
            next,
            Options.Create(options),
            NullLoggerFactory.Instance);

        await middleware.Invoke(context);

        return Assert.IsType<string>(resolved);
    }
}

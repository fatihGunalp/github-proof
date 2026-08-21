using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace GitHubProof.Web.Localization;

public static class WebLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddWebExperienceLocalization(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<WebPresentationLocalizer>();
        services.AddSingleton<ProfileWebPresentationLocalizer>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture =
                new RequestCulture(WebCulture.English);
            options.SupportedCultures = [.. WebCulture.SupportedCultures];
            options.SupportedUICultures = [.. WebCulture.SupportedCultures];
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;

            options.RequestCultureProviders =
            [
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider
                {
                    // The contract searches preferences in q-order until a
                    // supported family is found. The server's request-header
                    // size limit remains the transport bound.
                    MaximumAcceptLanguageHeaderValuesToTry = int.MaxValue
                }
            ];
        });

        return services;
    }

    public static IApplicationBuilder UseWebExperienceLocalization(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RequestLocalizationOptions options = app.ApplicationServices
            .GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value;

        return app.UseRequestLocalization(options);
    }
}

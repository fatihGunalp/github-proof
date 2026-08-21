using GitHubProof.Web.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubProof.Web.Pages;

public sealed class LanguageModel : PageModel
{
    [BindProperty]
    public string Culture { get; set; } = string.Empty;

    public IActionResult OnGet() =>
        RedirectToPage("/Index");

    public IActionResult OnPost()
    {
        if (!WebCulture.TryNormalize(Culture, out string normalized))
        {
            return RedirectToPage("/Index");
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(normalized)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        // Repository and Profile results are transient POST results. Culture
        // changes persist the choice and return to GET /; they never replay
        // the analysis request or create result persistence.
        return RedirectToPage("/Index");
    }
}

using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Web.Development;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubProof.Web.Pages;

public sealed class ReportPreviewModel(
    IWebHostEnvironment environment)
    : PageModel
{
    public RepositoryReport Report { get; private set; } = null!;

    public IActionResult OnGet(string? locale = null)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        Report = RepositoryReportPreviewFactory.Create(locale);

        return Page();
    }
}
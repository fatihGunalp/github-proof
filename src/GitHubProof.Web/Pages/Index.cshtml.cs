using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Web.Analysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubProof.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly LiveRepositoryReportService reportService;
    private readonly AnalysisVersion analysisVersion;

    public IndexModel(
        LiveRepositoryReportService reportService,
        AnalysisVersion analysisVersion)
    {
        this.reportService = reportService
            ?? throw new ArgumentNullException(nameof(reportService));
        this.analysisVersion = analysisVersion
            ?? throw new ArgumentNullException(nameof(analysisVersion));
    }

    [BindProperty]
    public string RepositoryUrl { get; set; } = string.Empty;

    public RepositoryReport? Report { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        Report = null;
        ErrorMessage = null;

        LiveRepositoryAnalysisResult result;

        try
        {
            result = await reportService.AnalyzeAsync(
                RepositoryUrl,
                analysisVersion,
                Guid.NewGuid(),
                cancellationToken);
        }
        catch (ArgumentException exception)
            when (exception.ParamName is "repositoryUrl" or "value")
        {
            ErrorMessage =
                "Enter a canonical public GitHub repository URL, for example https://github.com/owner/repository.";

            return Page();
        }

        if (!result.IsSuccessful)
        {
            ErrorMessage =
                CreateRepositoryFailureMessage(
                    result.MetadataAvailability);

            return Page();
        }

        RepositoryAnalysis analysis = result.Analysis
            ?? throw new InvalidOperationException(
                "A successful live analysis result must contain RepositoryAnalysis.");

        Report = reportService.Project(
            analysis,
            locale: "en");

        return Page();
    }

    private static string CreateRepositoryFailureMessage(
        DataAvailability availability) =>
        availability.Status switch
        {
            DataAvailabilityStatus.NotApplicable =>
                "Only public GitHub repositories are supported in the current MVP.",

            DataAvailabilityStatus.RateLimited =>
                "GitHub's API rate limit was reached. Try again later or configure GITHUB_TOKEN.",

            DataAvailabilityStatus.Forbidden =>
                "GitHub denied access to this repository. Confirm that it is public and accessible.",

            DataAvailabilityStatus.Failed =>
                "GitHub could not be reached reliably. Try the analysis again.",

            _ =>
                "We could not access that public GitHub repository. Check the URL and try again."
        };
}

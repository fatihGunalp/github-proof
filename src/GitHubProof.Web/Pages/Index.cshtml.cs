using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Web.Analysis;
using GitHubProof.Web.Entry;
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
    public string GitHubTarget { get; set; } = string.Empty;

    public RepositoryReport? Report { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        Report = null;
        ErrorMessage = null;

        // Deterministic shape-based routing between the two separate
        // analysis semantics. Nothing is inferred and no GitHub call is made
        // to discover what the user meant.
        if (!GitHubEntryClassifier.TryClassify(GitHubTarget, out GitHubEntryRoute? route))
        {
            ErrorMessage = CreateUnclassifiedInputMessage();

            return Page();
        }

        if (route!.Kind == GitHubEntryKind.Profile)
        {
            // Profile Proof (macro) owns its own route and result flow.
            // Preserve-method (307) so the identity travels in the re-posted
            // form body and profile analysis stays a POST: a GET of /profile
            // — refresh, back, prefetch, crawler, link preview — must never
            // start a live collection.
            return RedirectToPagePreserveMethod("/Profile");
        }

        return await AnalyzeRepositoryAsync(
            route.Value,
            cancellationToken);
    }

    private async Task<IActionResult> AnalyzeRepositoryAsync(
        string repositoryUrl,
        CancellationToken cancellationToken)
    {
        LiveRepositoryAnalysisResult result;

        try
        {
            result = await reportService.AnalyzeAsync(
                repositoryUrl,
                analysisVersion,
                Guid.NewGuid(),
                cancellationToken);
        }
        catch (ArgumentException exception)
            when (exception.ParamName is "repositoryUrl" or "value")
        {
            ErrorMessage = CreateUnclassifiedInputMessage();

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

    // One message for every shape the unified entry cannot resolve to exactly
    // one analysis flow. It restates the three accepted shapes instead of
    // reinterpreting the submitted value.
    private static string CreateUnclassifiedInputMessage() =>
        "Enter a GitHub username (octocat), a profile URL "
        + "(https://github.com/octocat), or a repository URL "
        + "(https://github.com/owner/repository).";

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

using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Web.Analysis;
using GitHubProof.Web.Entry;
using GitHubProof.Web.Localization;
using GitHubProof.Web.Progress;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubProof.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly LiveRepositoryReportService reportService;
    private readonly AnalysisVersion analysisVersion;
    private readonly WebPresentationLocalizer text;
    private readonly AnalysisProgressRegistry analysisProgress;

    public IndexModel(
        LiveRepositoryReportService reportService,
        AnalysisVersion analysisVersion,
        WebPresentationLocalizer text,
        AnalysisProgressRegistry analysisProgress)
    {
        this.reportService = reportService
            ?? throw new ArgumentNullException(nameof(reportService));
        this.analysisVersion = analysisVersion
            ?? throw new ArgumentNullException(nameof(analysisVersion));
        this.text = text
            ?? throw new ArgumentNullException(nameof(text));
        this.analysisProgress = analysisProgress
            ?? throw new ArgumentNullException(nameof(analysisProgress));
    }

    [BindProperty]
    public string GitHubTarget { get; set; } = string.Empty;

    // The client-generated correlation token (crypto.randomUUID) for live
    // progress, carried as a hidden field on the landing form only. Never an
    // analysis identifier; never reaches Domain, Application or Infrastructure
    // (live-analysis-progress-runtime-contract.md §8).
    [BindProperty]
    public string? AnalysisProgressToken { get; set; }

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
            // start a live collection. The progress token, if any, travels in
            // that same re-posted body; ProfileModel — never this model —
            // registers the profile run (§8).
            return RedirectToPagePreserveMethod("/Profile");
        }

        // Registration happens once the classifier has resolved a repository
        // route — the earliest point the flow is committed to analyzing (§8).
        // A null handle (no token, malformed token, duplicate, or cap
        // exhausted) simply disables progress; the analysis is untouched.
        using AnalysisProgressRun? progress = analysisProgress.TryBegin(
            AnalysisProgressToken,
            AnalysisProgressTargetKind.Repository);

        return await AnalyzeRepositoryAsync(
            route.Value,
            progress,
            cancellationToken);
    }

    private async Task<IActionResult> AnalyzeRepositoryAsync(
        string repositoryUrl,
        AnalysisProgressRun? progress,
        CancellationToken cancellationToken)
    {
        LiveRepositoryAnalysisResult result;

        try
        {
            result = await reportService.AnalyzeAsync(
                repositoryUrl,
                analysisVersion,
                Guid.NewGuid(),
                cancellationToken,
                progress);
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
            locale: WebCulture.Resolve(PageContext?.HttpContext));

        // Stage 4 `repository-proof-prepared`: the projector has returned. This
        // publishes the mandatory coherent terminal snapshot — every stage
        // completed, no active stage (§7.1) — before the `using` releases the
        // entry.
        progress?.CompleteCurrentStage();

        return Page();
    }

    // One message for every shape the unified entry cannot resolve to exactly
    // one analysis flow. It restates the three accepted shapes instead of
    // reinterpreting the submitted value.
    private string CreateUnclassifiedInputMessage() =>
        text.Get(WebTextKey.InvalidTarget, PageContext?.HttpContext);

    private string CreateRepositoryFailureMessage(
        DataAvailability availability) =>
        availability.Status switch
        {
            DataAvailabilityStatus.NotApplicable =>
                text.Get(WebTextKey.RepositoryPublicOnly, PageContext?.HttpContext),

            DataAvailabilityStatus.RateLimited =>
                text.Get(WebTextKey.RepositoryRateLimited, PageContext?.HttpContext),

            DataAvailabilityStatus.Forbidden =>
                text.Get(WebTextKey.RepositoryForbidden, PageContext?.HttpContext),

            DataAvailabilityStatus.Failed =>
                text.Get(WebTextKey.RepositoryFailed, PageContext?.HttpContext),

            _ =>
                text.Get(WebTextKey.RepositoryUnavailable, PageContext?.HttpContext)
        };
}

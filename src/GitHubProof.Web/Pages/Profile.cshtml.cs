using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Web.Entry;
using GitHubProof.Web.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubProof.Web.Pages;

// P10-I2 Profile Proof route boundary: deterministic invocation of the
// existing P10-I1 LiveProfileReportService and a minimal typed result
// boundary. It renders no profile report section — Meaning / Direction /
// Starting Point presentation is P10-I3, and the final failure/partial
// presentation UX is P10-I5. It makes no eligibility, composition or report
// decision of its own.
//
// Live profile analysis is POST-only. GET carries no analysis at all, so a
// refresh, a browser back/forward, a prefetch, a crawler or a link-preview
// fetch of /profile can never start a GitHub collection or run
// ProfilePipeline — which matters because profile analysis fans out across
// the account's repositories and consumes rate limit.
public sealed class ProfileModel : PageModel
{
    private readonly LiveProfileReportService profileReportService;
    private readonly AnalysisVersion analysisVersion;

    public ProfileModel(
        LiveProfileReportService profileReportService,
        AnalysisVersion analysisVersion)
    {
        this.profileReportService = profileReportService
            ?? throw new ArgumentNullException(nameof(profileReportService));
        this.analysisVersion = analysisVersion
            ?? throw new ArgumentNullException(nameof(analysisVersion));
    }

    // Re-posted by the landing's preserve-method redirect, under the same
    // field name the unified entry uses.
    [BindProperty]
    public string GitHubTarget { get; set; } = string.Empty;

    public string? Identity { get; private set; }

    public bool HasComposedProfile { get; private set; }

    public string? ErrorMessage { get; private set; }

    // Entry only: the unified input lives on the landing page, and this route
    // owns no analysis on GET.
    public IActionResult OnGet() =>
        RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        // Re-checked rather than trusted: a direct POST here must not be able
        // to push a repository-shaped or non-canonical value into the profile
        // flow.
        if (!GitHubEntryClassifier.TryClassify(GitHubTarget, out GitHubEntryRoute? route)
            || route!.Kind != GitHubEntryKind.Profile)
        {
            return RedirectToPage("/Index");
        }

        Identity = route.Value;

        LiveProfileAnalysisResult result = await profileReportService.AnalyzeAsync(
            Identity,
            analysisVersion,
            cancellationToken);

        if (!result.IsSuccessful)
        {
            ErrorMessage = CreateProfileFailureMessage(
                result.ProfileAvailability);

            return Page();
        }

        ProfilePipelineResult pipelineResult = result.PipelineResult
            ?? throw new InvalidOperationException(
                "A successful live profile result must contain a pipeline result.");

        if (!pipelineResult.HasProfile)
        {
            ErrorMessage =
                "We could not compose a profile from that GitHub account's public evidence.";

            return Page();
        }

        HasComposedProfile = true;

        return Page();
    }

    // Mirrors the Repository Proof failure vocabulary so a rate limit stays a
    // rate limit and never collapses into a negative claim about the profile.
    private static string CreateProfileFailureMessage(
        DataAvailability availability) =>
        availability.Status switch
        {
            DataAvailabilityStatus.NotApplicable =>
                "Only public GitHub profiles are supported in the current MVP.",

            DataAvailabilityStatus.RateLimited =>
                "GitHub's API rate limit was reached. Try again later or configure GITHUB_TOKEN.",

            DataAvailabilityStatus.Forbidden =>
                "GitHub denied access to this profile. Confirm that it is public and accessible.",

            DataAvailabilityStatus.Failed =>
                "GitHub could not be reached reliably. Try the analysis again.",

            _ =>
                "We could not access that public GitHub profile. Check the username and try again."
        };
}

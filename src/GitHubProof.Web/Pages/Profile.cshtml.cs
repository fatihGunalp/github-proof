using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Entry;
using GitHubProof.Web.Localization;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace GitHubProof.Web.Pages;

// P10-I2 Profile Proof route boundary: deterministic invocation of the
// existing P10-I1 LiveProfileReportService and a minimal typed result
// boundary. P10-I3 exposes the already-projected typed ProfileReport for
// Meaning / Direction / Starting Point presentation; WhyThisConclusion +
// evidence drill-down is P10-I4. P10-I5 consumes only the resulting typed
// Partial/failure/operational states. It makes no eligibility, composition or
// report decision of its own, and never re-projects or reinterprets report
// truth.
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
    private readonly WebPresentationLocalizer text;
    private readonly IProfileExecutionAdmissionGuard admissionGuard;

    public ProfileModel(
        LiveProfileReportService profileReportService,
        AnalysisVersion analysisVersion,
        WebPresentationLocalizer text,
        IProfileExecutionAdmissionGuard admissionGuard)
    {
        this.profileReportService = profileReportService
            ?? throw new ArgumentNullException(nameof(profileReportService));
        this.analysisVersion = analysisVersion
            ?? throw new ArgumentNullException(nameof(analysisVersion));
        this.text = text
            ?? throw new ArgumentNullException(nameof(text));
        this.admissionGuard = admissionGuard
            ?? throw new ArgumentNullException(nameof(admissionGuard));
    }

    // Re-posted by the landing's preserve-method redirect, under the same
    // field name the unified entry uses.
    [BindProperty]
    public string GitHubTarget { get; set; } = string.Empty;

    public string? Identity { get; private set; }

    public bool HasComposedProfile { get; private set; }

    // The P10-I3 presentation truth: the already-projected typed report from
    // the pipeline run that produced HasComposedProfile == true. Never
    // re-projected, re-derived from Profile, or computed from raw evidence.
    public ProfileReport? Report { get; private set; }

    public bool IsPartialProfile { get; private set; }

    public string? ErrorTitle { get; private set; }

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

        // The canonical parser owns normalization. Its login is used only as
        // the operational same-profile key; the raw accepted identity still
        // travels to the collector as before.
        string normalizedProfileLogin = GitHubProfileHandle.Parse(route.Value).Login;
        using ProfileAdmissionLease admission = admissionGuard.TryAcquire(
            normalizedProfileLogin,
            PageContext?.HttpContext?.Connection.RemoteIpAddress);

        if (!admission.IsAdmitted)
        {
            ApplyAdmissionRejection(admission);
            return Page();
        }

        LiveProfileAnalysisResult result = await profileReportService.AnalyzeAsync(
            Identity,
            analysisVersion,
            cancellationToken);

        if (!result.IsSuccessful)
        {
            if (IsBudgetProtectionRejection(result.ExecutionAvailability))
            {
                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                ApplyFailure(
                    WebTextKey.ProfileProviderInterruptedTitle,
                    WebTextKey.ProfileProviderInterrupted);
            }
            else if (IsExecutionDeadline(result.ExecutionAvailability))
            {
                Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                ApplyFailure(
                    WebTextKey.ProfileDeadlineTitle,
                    WebTextKey.ProfileDeadline);
            }
            else
            {
                DataAvailability availability = result.ProfileAvailability
                    ?? throw new InvalidOperationException(
                        "A non-deadline Profile failure must preserve profile availability.");

                ApplyProfileFailure(availability);
            }

            return Page();
        }

        ProfilePipelineResult pipelineResult = result.PipelineResult
            ?? throw new InvalidOperationException(
                "A successful live profile result must contain a pipeline result.");

        ApplyPipelineResult(pipelineResult);

        return Page();
    }

    private void ApplyPipelineResult(ProfilePipelineResult pipelineResult)
    {
        ArgumentNullException.ThrowIfNull(pipelineResult);

        if (!pipelineResult.HasProfile)
        {
            ApplyFailure(
                WebTextKey.ProfileCompositionFailedTitle,
                WebTextKey.ProfileCouldNotCompose);

            return;
        }

        // A composed (HasProfile == true) pipeline result always carries its
        // projected report (ProfilePipelineResult.Composed requires one); a
        // null Report here would mean the pipeline contract was violated, not
        // a valid empty presentation state.
        Report = pipelineResult.Report
            ?? throw new InvalidOperationException(
                "A composed profile pipeline result must always carry a "
                + "projected report.");

        HasComposedProfile = true;
        IsPartialProfile = pipelineResult.Status == ProfileCompositionStatus.Partial;
    }

    // Mirrors the Repository Proof failure vocabulary so a rate limit stays a
    // rate limit and never collapses into a negative claim about the profile.
    private void ApplyProfileFailure(DataAvailability availability)
    {
        WebTextKey message = availability.Status switch
        {
            DataAvailabilityStatus.NotApplicable =>
                WebTextKey.ProfilePublicOnly,

            DataAvailabilityStatus.RateLimited =>
                WebTextKey.ProfileRateLimited,

            DataAvailabilityStatus.Forbidden =>
                WebTextKey.ProfileForbidden,

            DataAvailabilityStatus.Failed =>
                WebTextKey.ProfileFailed,

            _ =>
                WebTextKey.ProfileUnavailable
        };

        WebTextKey title = availability.Status == DataAvailabilityStatus.NotApplicable
            ? WebTextKey.ProfileUnsupportedTitle
            : WebTextKey.ProfileUnavailableTitle;

        ApplyFailure(title, message);
    }

    private static bool IsExecutionDeadline(DataAvailability availability) =>
        availability.Status == DataAvailabilityStatus.Failed
        && string.Equals(
            availability.Reason,
            LiveProfileAnalysisResult.DeadlineExpiredReason,
            StringComparison.Ordinal);

    private static bool IsBudgetProtectionRejection(
        DataAvailability availability) =>
        availability.Status == DataAvailabilityStatus.Unavailable
        && string.Equals(
            availability.Reason,
            LiveProfileAnalysisResult.BudgetProtectionRejectedReason,
            StringComparison.Ordinal);

    private void ApplyAdmissionRejection(ProfileAdmissionLease admission)
    {
        Response.StatusCode = admission.Rejection switch
        {
            ProfileAdmissionRejection.VisitorRateLimited =>
                StatusCodes.Status429TooManyRequests,
            ProfileAdmissionRejection.SameProfileBusy =>
                StatusCodes.Status409Conflict,
            ProfileAdmissionRejection.GlobalCapacityBusy =>
                StatusCodes.Status503ServiceUnavailable,
            _ => throw new InvalidOperationException(
                "An admission rejection must carry a known reason.")
        };

        if (admission.RetryAfter is TimeSpan retryAfter)
        {
            double boundedSeconds = Math.Min(
                int.MaxValue,
                Math.Max(1, Math.Ceiling(retryAfter.TotalSeconds)));

            Response.Headers.RetryAfter = ((int)boundedSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        (WebTextKey title, WebTextKey message) = admission.Rejection switch
        {
            ProfileAdmissionRejection.VisitorRateLimited =>
                (WebTextKey.ProfileRequestLimitedTitle,
                    WebTextKey.ProfileRequestLimited),
            ProfileAdmissionRejection.SameProfileBusy =>
                (WebTextKey.ProfileAnalysisInProgressTitle,
                    WebTextKey.ProfileAnalysisInProgress),
            ProfileAdmissionRejection.GlobalCapacityBusy =>
                (WebTextKey.ProfileServiceBusyTitle,
                    WebTextKey.ProfileServiceBusy),
            _ => throw new InvalidOperationException(
                "An admission rejection must carry a known reason.")
        };

        ApplyFailure(title, message);
    }

    private void ApplyFailure(WebTextKey title, WebTextKey message)
    {
        ErrorTitle = text.Get(title, PageContext?.HttpContext);
        ErrorMessage = text.Get(message, PageContext?.HttpContext);
    }
}

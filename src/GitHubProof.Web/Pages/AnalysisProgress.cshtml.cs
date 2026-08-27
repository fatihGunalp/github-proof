using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Web.Progress;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubProof.Web.Pages;

// GET /analysis-progress?run=<token>
//
// Read-only, no side effects: it never registers a run, never touches the
// admission guard or the budget circuit, never starts, resumes, extends or
// cancels any analysis, and never writes to the registry
// (live-analysis-progress-runtime-contract.md §10.4). It carries no user-facing
// prose — its whole vocabulary is a target kind and the fixed stage
// identities.
//
// The only routing surface is MapRazorPages(); this is a Razor Page with a GET
// handler, not a second routing mechanism.
public sealed class AnalysisProgressModel : PageModel
{
    private static readonly JsonSerializerOptions ResponseJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    private readonly AnalysisProgressRegistry registry;

    public AnalysisProgressModel(AnalysisProgressRegistry registry)
    {
        this.registry = registry
            ?? throw new ArgumentNullException(nameof(registry));
    }

    public IActionResult OnGet(string? run)
    {
        // Ephemeral runtime state: a stale snapshot served as current progress
        // would be worse than none. Every response, on both paths, is
        // non-cacheable (§10.5). ETag/conditional semantics are prohibited and
        // never emitted.
        Response.Headers.CacheControl = "no-store";

        // A missing, malformed or non-canonical-UUID `run` value is answered
        // with HTTP 400 and an empty body — never `{ "known": false }`. It
        // counts against the transport-failure stop limit, not the
        // `known:false` retry limit (§10.4).
        if (!Guid.TryParseExact(run, "D", out Guid token))
        {
            return BadRequest();
        }

        if (!registry.TryGetSnapshot(token, out AnalysisProgressSnapshot snapshot))
        {
            return Content("{\"known\":false}", "application/json");
        }

        AnalysisProgressResponse body = new(
            Known: true,
            Target: snapshot.TargetKind == AnalysisProgressTargetKind.Repository
                ? "repository"
                : "profile",
            Active: snapshot.ActiveStage,
            Completed: snapshot.CompletedStages);

        return Content(
            JsonSerializer.Serialize(body, ResponseJson),
            "application/json");
    }

    private sealed record AnalysisProgressResponse(
        bool Known,
        string Target,
        string? Active,
        IReadOnlyList<string> Completed);
}

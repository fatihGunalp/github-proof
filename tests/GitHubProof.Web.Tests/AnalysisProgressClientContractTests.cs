using Xunit;

namespace GitHubProof.Web.Tests;

// The client-side and markup half of the Session 138 contract
// (live-analysis-progress-runtime-contract.md §6, §10, §13, §15, §16, §17;
// p10 §8.3.1–§8.3.6, §8.6.5). String-level guards only — there is no JS
// framework or node toolchain in this repository.
public sealed class AnalysisProgressClientContractTests
{
    private static string Js() => ReadWebFile("wwwroot", "js", "analysis-progress.js");

    private static string LandingInput() =>
        ReadWebFile("Pages", "Shared", "_GitHubInput.cshtml");

    [Fact]
    public void Client_generates_a_uuid_and_polls_the_read_only_json_endpoint()
    {
        string js = Js();

        Assert.Contains("crypto?.randomUUID", js, StringComparison.Ordinal);
        Assert.Contains("fetch(", js, StringComparison.Ordinal);
        Assert.Contains("/analysis-progress", js, StringComparison.Ordinal);
        Assert.Contains("?run=", js, StringComparison.Ordinal);
        Assert.Contains("AnalysisProgressToken", js, StringComparison.Ordinal);
        Assert.Contains("cache: \"no-store\"", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Client_uses_the_exact_locked_runtime_values()
    {
        string js = Js();

        Assert.Contains("POLL_INTERVAL_MS = 2000", js, StringComparison.Ordinal);
        Assert.Contains("INITIAL_UNKNOWN_LIMIT = 5", js, StringComparison.Ordinal);
        Assert.Contains("TRANSPORT_FAILURE_LIMIT = 3", js, StringComparison.Ordinal);

        // Constant interval polling; no backoff, acceleration or adaptive
        // interval, and nothing derived from elapsed time (§13).
        Assert.Contains("setInterval(poll, POLL_INTERVAL_MS)", js, StringComparison.Ordinal);
        Assert.DoesNotContain("setTimeout", js, StringComparison.Ordinal);
        Assert.DoesNotContain("Date.now", js, StringComparison.Ordinal);
        Assert.DoesNotContain("performance.now", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Client_pauses_while_hidden_and_resets_on_bfcache_restore()
    {
        string js = Js();

        Assert.Contains("visibilitychange", js, StringComparison.Ordinal);
        Assert.Contains("document.hidden", js, StringComparison.Ordinal);
        Assert.Contains("pageshow", js, StringComparison.Ordinal);
        Assert.Contains("resetState", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Client_stops_and_keeps_the_last_snapshot_once_a_known_response_was_seen()
    {
        string js = Js();

        // known:false after a known response → stop polling, keep snapshot;
        // never revert to indeterminate (§15).
        Assert.Contains("if (sawKnown)", js, StringComparison.Ordinal);
        Assert.Contains("stopPolling()", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Client_streak_limits_are_consecutive_and_reset_each_other()
    {
        string js = Js();

        // §13: the 5 / 3 limits are "consecutive" / "in a row" — a response of
        // the other kind breaks the streak.
        Assert.Contains("failureStreak = 0", js, StringComparison.Ordinal);
        Assert.Contains("unknownStreak = 0", js, StringComparison.Ordinal);
        Assert.Contains("unknownStreak >= INITIAL_UNKNOWN_LIMIT", js, StringComparison.Ordinal);
        Assert.Contains("failureStreak >= TRANSPORT_FAILURE_LIMIT", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Client_uses_no_streaming_or_push_transport()
    {
        string js = Js();

        foreach (string prohibited in new[]
        {
            "EventSource", "WebSocket", "SignalR", "signalr",
            "text/event-stream", "new WebSocket", "long-poll"
        })
        {
            Assert.DoesNotContain(prohibited, js, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Client_renders_no_percentage_eta_or_progress_bar()
    {
        string js = Js();

        Assert.DoesNotContain("valuenow", js, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("progressbar", js, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Math.round", js, StringComparison.Ordinal);
        Assert.DoesNotContain("%\"", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Landing_form_carries_the_progress_marker_and_both_stage_lists()
    {
        string content = LandingInput();

        Assert.Contains("data-analysis-progress", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-stages", content, StringComparison.Ordinal);
        Assert.Contains("data-stage=", content, StringComparison.Ordinal);

        foreach (string stage in new[]
        {
            "repository-target-validated",
            "repository-public-data-read",
            "repository-traces-analyzed",
            "repository-proof-prepared",
            "profile-target-validated",
            "profile-public-surface-read",
            "profile-evidence-examined"
        })
        {
            Assert.Contains($"\"{stage}\"", content, StringComparison.Ordinal);
        }

        // Both lists render in PENDING state, server-side.
        Assert.Contains("data-state=\"pending\"", content, StringComparison.Ordinal);
        // The stage list is an ordinary list, not a live region.
        Assert.Contains("<ol", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Landing_form_keeps_one_narrowed_live_region_with_the_three_attributes()
    {
        string content = LandingInput();

        Assert.Contains("data-analysis-live", content, StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", content, StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite\"", content, StringComparison.Ordinal);
        Assert.Contains("aria-atomic=\"true\"", content, StringComparison.Ordinal);

        // Before the first snapshot the live region carries the canonical
        // indeterminate sentence (p10 §8.3.6).
        Assert.Contains("WebTextKey.ProcessingRequest", content, StringComparison.Ordinal);

        // The existing indeterminate activity indicator is retained (§8.3.4).
        Assert.Contains("analysis-progress__indicator", content, StringComparison.Ordinal);

        // Inline only — no modal / overlay (§8.3.5).
        Assert.DoesNotContain("modal", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dialog", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Landing_form_stage_labels_and_state_tokens_come_only_from_the_localizer()
    {
        string content = LandingInput();

        foreach (string key in new[]
        {
            "ProgressRepositoryTargetValidated",
            "ProgressProfileEvidenceExamined",
            "ProgressStatePending",
            "ProgressStateActive",
            "ProgressStateCompleted"
        })
        {
            Assert.Contains($"WebTextKey.{key}", content, StringComparison.Ordinal);
        }

        // No progress copy is a literal string in the markup — every label and
        // state token is a WebTextKey lookup (p10 §8.3.2).
        Assert.DoesNotContain("Validating the GitHub", content, StringComparison.Ordinal);
        Assert.DoesNotContain("In progress<", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("_ProfileRepositoryEvidence.cshtml")]
    [InlineData("_ProfileStartingPoint.cshtml")]
    public void Profile_drill_down_forms_stay_progress_free(string partial)
    {
        string content = ReadWebFile("Pages", "Shared", partial);

        // They keep the existing submit / duplicate-submit hook...
        Assert.Contains("data-analysis-form", content, StringComparison.Ordinal);

        // ...but receive no token, no polling and no stage UI (§8, p10 §8.6.5).
        Assert.DoesNotContain("data-analysis-progress", content, StringComparison.Ordinal);
        Assert.DoesNotContain("data-analysis-stages", content, StringComparison.Ordinal);
        Assert.DoesNotContain("data-stage=", content, StringComparison.Ordinal);
        Assert.DoesNotContain("AnalysisProgressToken", content, StringComparison.Ordinal);
        Assert.DoesNotContain("data-analysis-live", content, StringComparison.Ordinal);
    }

    [Fact]
    public void The_progress_script_binds_stage_ui_only_to_the_marked_landing_form()
    {
        string js = Js();

        Assert.Contains("hasAttribute(\"data-analysis-progress\")", js, StringComparison.Ordinal);
    }

    private static string ReadWebFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GitHubProof.slnx")))
            {
                return File.ReadAllText(
                    Path.Combine(
                        new[] { directory.FullName, "src", "GitHubProof.Web" }
                            .Concat(relativeSegments)
                            .ToArray()));
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }
}

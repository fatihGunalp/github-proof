using Xunit;

namespace GitHubProof.Web.Tests;

// Session 140: while the landing analysis form is actively submitting a
// synchronous analysis POST, the EN/TR language control must not initiate
// navigation or a culture change (p10 §8.1). String-level guards only — there
// is no JS test harness in this repository.
public sealed class LanguageAnalysisGuardTests
{
    private static string Js() =>
        ReadWebFile("wwwroot", "js", "analysis-progress.js");

    private static string Layout() =>
        ReadWebFile("Pages", "Shared", "_Layout.cshtml");

    [Fact]
    public void Layout_marks_the_language_form_and_keeps_its_existing_contract()
    {
        string layout = Layout();

        Assert.Contains("data-language-form", layout, StringComparison.Ordinal);

        // The existing quiet EN|TR utility is otherwise unchanged.
        Assert.Contains("class=\"language-selector\"", layout, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Language\"", layout, StringComparison.Ordinal);
        Assert.Contains("name=\"Culture\"", layout, StringComparison.Ordinal);
        Assert.Contains("value=\"en\"", layout, StringComparison.Ordinal);
        Assert.Contains("value=\"tr\"", layout, StringComparison.Ordinal);
        Assert.Contains("aria-pressed", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void An_active_landing_analysis_blocks_the_language_submission()
    {
        string js = Js();

        // The block is on the form's submit event — the one path both a pointer
        // click and Enter/Space on a focused choice go through.
        Assert.Contains("languageForm?.addEventListener(\"submit\"", js, StringComparison.Ordinal);
        Assert.Contains("landingAnalysisActive()", js, StringComparison.Ordinal);
        Assert.Contains("event.preventDefault()", js, StringComparison.Ordinal);

        // Perceivable + accessible + keyboard: aria-disabled and out of the
        // tab order for the duration.
        Assert.Contains("aria-disabled", js, StringComparison.Ordinal);
        Assert.Contains("\"tabindex\", \"-1\"", js, StringComparison.Ordinal);
        Assert.Contains("button[name=\\\"Culture\\\"]", js, StringComparison.Ordinal);
    }

    [Fact]
    public void The_busy_signal_is_the_existing_submitting_state_not_a_new_flag()
    {
        string js = Js();

        Assert.Contains(
            "candidate.hasAttribute(\"data-analysis-progress\")",
            js,
            StringComparison.Ordinal);
        Assert.Contains(
            "candidate.dataset.submitting === \"true\"",
            js,
            StringComparison.Ordinal);
    }

    [Fact]
    public void The_language_block_is_conditional_so_idle_behaviour_is_unchanged()
    {
        string js = Js();

        int listener = js.IndexOf(
            "languageForm?.addEventListener(\"submit\"",
            StringComparison.Ordinal);
        Assert.True(listener >= 0);

        // Inside the listener, preventDefault is reached only under the guard.
        string body = js[listener..(js.IndexOf("});", listener, StringComparison.Ordinal) + 3)];
        Assert.Contains("if (landingAnalysisActive())", body, StringComparison.Ordinal);
        Assert.Contains("event.preventDefault()", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Reset_and_pageshow_restore_the_language_controls()
    {
        string js = Js();

        Assert.Contains("setLanguageControlsLocked(false)", js, StringComparison.Ordinal);

        // reset() carries the restore, and pageshow runs reset() for every form.
        int reset = js.IndexOf("const reset = form =>", StringComparison.Ordinal);
        int resetEnd = js.IndexOf("\n    };", reset, StringComparison.Ordinal);
        string resetBody = js[reset..resetEnd];
        Assert.Contains("setLanguageControlsLocked(false)", resetBody, StringComparison.Ordinal);

        Assert.Contains("pageshow", js, StringComparison.Ordinal);
        Assert.Contains("forms.forEach(reset)", js, StringComparison.Ordinal);
    }

    [Fact]
    public void Only_the_landing_progress_form_locks_or_restores_the_language_controls()
    {
        string js = Js();

        // Both the lock (submit) and the restore (reset) are gated on the
        // landing progress marker, so a Profile drill-down form neither
        // disables nor re-enables EN/TR.
        int lockCalls = CountOccurrences(js, "setLanguageControlsLocked(true)");
        int unlockCalls = CountOccurrences(js, "setLanguageControlsLocked(false)");
        Assert.Equal(1, lockCalls);
        Assert.Equal(1, unlockCalls);

        int lockIndex = js.IndexOf("setLanguageControlsLocked(true)", StringComparison.Ordinal);
        string beforeLock = js[Math.Max(0, lockIndex - 120)..lockIndex];
        Assert.Contains("hasAttribute(\"data-analysis-progress\")", beforeLock, StringComparison.Ordinal);

        int unlockIndex = js.IndexOf("setLanguageControlsLocked(false)", StringComparison.Ordinal);
        string beforeUnlock = js[Math.Max(0, unlockIndex - 160)..unlockIndex];
        Assert.Contains("hasAttribute(\"data-analysis-progress\")", beforeUnlock, StringComparison.Ordinal);
    }

    [Fact]
    public void Session_139_progress_behaviour_is_untouched()
    {
        string js = Js();

        Assert.Contains("POLL_INTERVAL_MS = 2000", js, StringComparison.Ordinal);
        Assert.Contains("INITIAL_UNKNOWN_LIMIT = 5", js, StringComparison.Ordinal);
        Assert.Contains("TRANSPORT_FAILURE_LIMIT = 3", js, StringComparison.Ordinal);
        Assert.Contains("createProgressController(form)", js, StringComparison.Ordinal);
        Assert.Contains("crypto?.randomUUID", js, StringComparison.Ordinal);
        Assert.Contains("/analysis-progress", js, StringComparison.Ordinal);
        Assert.Contains("AnalysisProgressToken", js, StringComparison.Ordinal);
        Assert.Contains("stopPolling()", js, StringComparison.Ordinal);

        // Still no streaming / push transport.
        Assert.DoesNotContain("EventSource", js, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WebSocket", js, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void The_locked_state_is_styled_as_inert()
    {
        string css = ReadWebFile("wwwroot", "css", "site.css");

        Assert.Contains(
            ".language-selector[data-analysis-locked]",
            css,
            StringComparison.Ordinal);
        Assert.Contains("pointer-events: none", css, StringComparison.Ordinal);
    }

    [Fact]
    public void The_theme_control_is_not_affected()
    {
        string js = Js();
        string themeJs = ReadWebFile("wwwroot", "js", "theme.js");

        // analysis-progress.js touches only the language form.
        Assert.DoesNotContain("data-theme-toggle", js, StringComparison.Ordinal);
        // theme.js is unchanged and still owns the toggle.
        Assert.Contains("data-theme-toggle", themeJs, StringComparison.Ordinal);
        Assert.DoesNotContain("data-analysis", themeJs, StringComparison.Ordinal);
    }

    [Fact]
    public void P10_8_1_records_the_in_flight_language_clarification_in_its_owning_paragraph()
    {
        string contract = ReadRepoFile(
            "docs", "product", "p10-presentation-language-contract.md");

        int section = contract.IndexOf(
            "## 8.1 Web Culture Resolution and Persistence",
            StringComparison.Ordinal);
        int nextSection = contract.IndexOf("## 8.2", section, StringComparison.Ordinal);
        string body = contract[section..nextSection];

        Assert.Contains("is actively submitting a synchronous analysis", body, StringComparison.Ordinal);
        Assert.Contains("initiates no navigation and no culture change", body, StringComparison.Ordinal);
        Assert.Contains("does not translate, restart or persist", body, StringComparison.Ordinal);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0;
        int index = 0;

        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }

    private static string ReadWebFile(params string[] relativeSegments) =>
        ReadRepoFile(
            new[] { "src", "GitHubProof.Web" }.Concat(relativeSegments).ToArray());

    private static string ReadRepoFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GitHubProof.slnx")))
            {
                return File.ReadAllText(
                    Path.Combine(
                        new[] { directory.FullName }
                            .Concat(relativeSegments)
                            .ToArray()));
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }
}

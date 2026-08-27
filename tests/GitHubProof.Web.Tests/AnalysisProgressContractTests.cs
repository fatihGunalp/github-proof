using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class AnalysisProgressContractTests
{
    [Fact]
    public void Unified_form_exposes_truthful_accessible_indeterminate_state()
    {
        string content = ReadWebFile("Pages", "Shared", "_GitHubInput.cshtml");

        Assert.Contains("data-analysis-form", content, StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"false\"", content, StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", content, StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite\"", content, StringComparison.Ordinal);
        Assert.Contains("aria-atomic=\"true\"", content, StringComparison.Ordinal);
        Assert.Contains("WebTextKey.ProcessingRequest", content, StringComparison.Ordinal);
        Assert.Contains("name=\"GitHubTarget\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("disabled", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Client_state_blocks_duplicates_without_disabling_serialized_controls_and_resets_on_restore()
    {
        string content = ReadWebFile("wwwroot", "js", "analysis-progress.js");

        Assert.Contains("form.checkValidity()", content, StringComparison.Ordinal);
        Assert.Contains("form.dataset.submitting === \"true\"", content, StringComparison.Ordinal);
        Assert.Contains("event.preventDefault()", content, StringComparison.Ordinal);
        Assert.Contains("aria-busy", content, StringComparison.Ordinal);
        Assert.Contains("pageshow", content, StringComparison.Ordinal);
        Assert.Contains("status.hidden = true", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".disabled", content, StringComparison.Ordinal);
        Assert.DoesNotContain("target.disabled", content, StringComparison.Ordinal);
        Assert.DoesNotContain("target.value =", content, StringComparison.Ordinal);
        Assert.DoesNotContain("removeAttribute(\"name\")", content, StringComparison.Ordinal);

        // Session 139 supersedes the pre-138 `setInterval` and `fetch(`
        // exclusions: constant-interval polling of the read-only JSON endpoint
        // is exactly the selected transport
        // (live-analysis-progress-runtime-contract.md §10). The streaming
        // transports stay prohibited (§17).
        Assert.DoesNotContain("EventSource", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WebSocket", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("text/event-stream", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("signalr", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GithubTarget_becomes_readonly_during_submit_and_editable_again_on_reset()
    {
        string script = ReadWebFile("wwwroot", "js", "analysis-progress.js");
        string styles = ReadWebFile("wwwroot", "css", "repository-input.css");

        Assert.Contains(
            "form.elements.namedItem(\"GitHubTarget\")",
            script,
            StringComparison.Ordinal);
        Assert.Contains("target.readOnly = true", script, StringComparison.Ordinal);
        Assert.Contains(
            "target.setAttribute(\"aria-readonly\", \"true\")",
            script,
            StringComparison.Ordinal);
        Assert.Contains("target.readOnly = false", script, StringComparison.Ordinal);
        Assert.Contains(
            "target.removeAttribute(\"aria-readonly\")",
            script,
            StringComparison.Ordinal);
        Assert.Contains("forms.forEach(reset)", script, StringComparison.Ordinal);
        Assert.Contains(".repository-input__field[readonly]", styles, StringComparison.Ordinal);
        Assert.Contains("caret-color: transparent", styles, StringComparison.Ordinal);
        Assert.Contains("cursor: default", styles, StringComparison.Ordinal);

        string readonlyKeyboardFocus = ExtractRule(
            styles,
            ".repository-input__field[readonly]:focus-visible {");
        Assert.Contains("var(--focus-ring-width)", readonlyKeyboardFocus, StringComparison.Ordinal);
        Assert.Contains("var(--border-strong)", readonlyKeyboardFocus, StringComparison.Ordinal);
        Assert.Contains("var(--focus-ring-offset)", readonlyKeyboardFocus, StringComparison.Ordinal);
        Assert.DoesNotContain("outline: none", readonlyKeyboardFocus, StringComparison.Ordinal);
    }

    [Fact]
    public void Motion_is_indeterminate_and_respects_reduced_motion()
    {
        string content = ReadWebFile("wwwroot", "css", "repository-input.css");

        Assert.Contains("@keyframes analysis-progress-spin", content, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion: reduce", content, StringComparison.Ordinal);
        Assert.Contains("animation: none", content, StringComparison.Ordinal);
        Assert.DoesNotContain("width: 100%", ExtractIndicatorBlock(content), StringComparison.Ordinal);
    }

    private static string ExtractIndicatorBlock(string content)
    {
        int start = content.IndexOf(".analysis-progress__indicator {", StringComparison.Ordinal);
        int end = content.IndexOf('}', start);
        return content[start..(end + 1)];
    }

    private static string ExtractRule(string content, string selector)
    {
        int start = content.IndexOf(selector, StringComparison.Ordinal);
        int end = content.IndexOf('}', start);
        return content[start..(end + 1)];
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

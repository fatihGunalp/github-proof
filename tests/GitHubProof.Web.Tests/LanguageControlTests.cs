using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class LanguageControlTests
{
    [Fact]
    public void Layout_uses_request_culture_for_html_and_exposes_quiet_EN_TR_utility()
    {
        string content = ReadWebFile("Pages", "Shared", "_Layout.cshtml");

        Assert.Contains("<html lang=\"@Text.Culture\">", content, StringComparison.Ordinal);
        Assert.Contains("class=\"language-selector\"", content, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Language\"", content, StringComparison.Ordinal);
        Assert.Contains("value=\"en\"", content, StringComparison.Ordinal);
        Assert.Contains("value=\"tr\"", content, StringComparison.Ordinal);
        Assert.Contains("aria-pressed", content, StringComparison.Ordinal);
        Assert.Contains("data-theme-toggle", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Language_change_has_no_target_repost_or_result_replay_surface()
    {
        string layout = ReadWebFile("Pages", "Shared", "_Layout.cshtml");
        string handler = ReadWebFile("Pages", "Language.cshtml.cs");

        Assert.DoesNotContain("GitHubTarget", layout, StringComparison.Ordinal);
        Assert.DoesNotContain("returnUrl", layout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LiveRepositoryReportService", handler, StringComparison.Ordinal);
        Assert.DoesNotContain("LiveProfileReportService", handler, StringComparison.Ordinal);
        Assert.DoesNotContain("PreserveMethod", handler, StringComparison.Ordinal);
        Assert.Contains("RedirectToPage(\"/Index\")", handler, StringComparison.Ordinal);
    }

    [Fact]
    public void Theme_accessible_copy_is_resolved_from_selected_Web_culture()
    {
        string layout = ReadWebFile("Pages", "Shared", "_Layout.cshtml");
        string script = ReadWebFile("wwwroot", "js", "theme.js");

        Assert.Contains("WebTextKey.ThemeSwitchToLight", layout, StringComparison.Ordinal);
        Assert.Contains("WebTextKey.ThemeSwitchToDark", layout, StringComparison.Ordinal);
        Assert.Contains("toggle.dataset.labelLight", script, StringComparison.Ordinal);
        Assert.Contains("toggle.dataset.labelDark", script, StringComparison.Ordinal);
        Assert.DoesNotContain("`Switch to", script, StringComparison.Ordinal);
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

using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class ThemeControlTests
{
    [Fact]
    public void Layout_exposes_one_light_dark_theme_toggle()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_Layout.cshtml");

        Assert.Contains(
            "data-theme-toggle",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "theme-toggle__icon--sun",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "theme-toggle__icon--moon",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "data-theme-choice",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            ">System<",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Initial_theme_uses_saved_light_dark_choice_or_os_preference()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_Layout.cshtml");

        Assert.Contains(
            "github-proof-theme",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "prefers-color-scheme: dark",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "storedTheme === \"light\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "storedTheme === \"dark\"",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Theme_controller_toggles_only_light_and_dark()
    {
        string content = ReadWebFile(
            "wwwroot",
            "js",
            "theme.js");

        Assert.Contains(
            "currentTheme() === \"dark\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "? \"light\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            ": \"dark\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "localStorage.setItem",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "localStorage.removeItem",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "\"system\"",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Theme_button_uses_semantic_tokens_and_focus_visibility()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "site.css");

        Assert.Contains(
            ".theme-toggle",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            ".theme-toggle:focus-visible",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "var(--surface-hover)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "var(--fg-primary)",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "#",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "rgb(",
            content,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "--primitive-",
            content,
            StringComparison.Ordinal);
    }

    private static string ReadWebFile(
        params string[] relativeSegments)
    {
        DirectoryInfo? directory =
            new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath =
                Path.Combine(
                    directory.FullName,
                    "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path =
                    Path.Combine(
                        new[]
                        {
                            directory.FullName,
                            "src",
                            "GitHubProof.Web"
                        }
                        .Concat(relativeSegments)
                        .ToArray());

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        $"Expected Web file was not found: {path}");
                }

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }
}

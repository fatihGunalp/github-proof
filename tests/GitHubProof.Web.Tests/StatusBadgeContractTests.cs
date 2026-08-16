using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class StatusBadgeContractTests
{
    [Fact]
    public void Status_badge_uses_localized_status_label_as_visible_text()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_StatusBadge.cshtml");

        Assert.Contains(
            "@Model.StatusLabel",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Status_badge_does_not_use_color_as_the_only_status_cue()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_StatusBadge.cshtml");

        Assert.Contains(
            "status-badge__icon",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "_StatusIcon",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "status-badge__label",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Status_icon_uses_one_geometric_outline_svg_language()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_StatusIcon.cshtml");

        Assert.Contains(
            "<svg",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke=\"currentColor\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke-width=\"1.75\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke-linecap=\"round\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke-linejoin=\"round\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "aria-hidden=\"true\"",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "✓",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "◐",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Status_badge_css_preserves_missing_and_unavailable_as_distinct_semantics()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "status-badge.css");

        Assert.Contains(
            ".status-badge--missing",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "var(--status-missing-fg)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            ".status-badge--unavailable-evidence",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "var(--status-unavailable-fg)",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Status_badge_css_uses_all_seven_semantic_status_token_families()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "status-badge.css");

        string[] requiredTokens =
        {
            "--status-strong-",
            "--status-present-",
            "--status-partial-",
            "--status-missing-",
            "--status-not-applicable-",
            "--status-unavailable-",
            "--status-human-review-"
        };

        foreach (string token in requiredTokens)
        {
            Assert.Contains(
                token,
                content,
                StringComparison.Ordinal);
        }
    }

    private static string ReadWebFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(
                directory.FullName,
                "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path = Path.Combine(
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
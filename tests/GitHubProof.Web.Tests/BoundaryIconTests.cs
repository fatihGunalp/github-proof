using System.Text.RegularExpressions;
using GitHubProof.Web.Presentation;
using Xunit;

namespace GitHubProof.Web.Tests;

// The four "What GitHub Proof is not" icons are decorative only. They must
// stay inline SVG in the existing icon system, share the theme-toggle icon
// language, and never become the carrier of the boundary meaning — the text
// remains the contract.
public sealed class BoundaryIconTests
{
    private static readonly UiIcon[] BoundaryIcons =
    [
        UiIcon.ChartColumnOff,
        UiIcon.ChevronsUpOff,
        UiIcon.SparklesOff,
        UiIcon.CircleCheckOff
    ];

    [Fact]
    public void Every_boundary_icon_is_rendered_by_the_shared_inline_svg_partial()
    {
        string content = ReadWebFile("Pages", "Shared", "_UiIcon.cshtml");

        foreach (UiIcon icon in BoundaryIcons)
        {
            Assert.Contains(
                $"case UiIcon.{icon}:",
                content,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Shared_icon_partial_matches_the_theme_toggle_icon_language()
    {
        string content = ReadWebFile("Pages", "Shared", "_UiIcon.cshtml");

        Assert.Contains("fill=\"none\"", content, StringComparison.Ordinal);
        Assert.Contains("stroke=\"currentColor\"", content, StringComparison.Ordinal);
        Assert.Contains("stroke-width=\"1.75\"", content, StringComparison.Ordinal);
        Assert.Contains("stroke-linecap=\"round\"", content, StringComparison.Ordinal);
        Assert.Contains("stroke-linejoin=\"round\"", content, StringComparison.Ordinal);
        Assert.Contains("aria-hidden=\"true\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Landing_renders_one_icon_per_boundary_item()
    {
        string content = ReadWebFile("Pages", "Index.cshtml");

        foreach (UiIcon icon in BoundaryIcons)
        {
            Assert.Contains(
                $"UiIcon.{icon}",
                content,
                StringComparison.Ordinal);
        }

        int iconSlots = Regex.Matches(
            content,
            "boundaries__item-icon").Count;

        int items = Regex.Matches(
            content,
            "boundaries__item-title").Count;

        Assert.Equal(4, items);
        Assert.Equal(items, iconSlots);
    }

    [Fact]
    public void Icons_do_not_carry_the_boundary_text_contract()
    {
        // The four statements must remain real text, not icon-only or
        // image-encoded content — the icons add nothing readable.
        string content = ReadWebFile("Pages", "Index.cshtml");

        string[] boundaryStatements =
        [
            "We don't rank or score developers.",
            "We don't judge experience or seniority.",
            "We don't guess or claim AI authorship.",
            "We don't tell you who to hire or not."
        ];

        foreach (string statement in boundaryStatements)
        {
            Assert.Contains(statement, content, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("<img", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Boundary_icons_use_no_external_asset_or_emoji_source()
    {
        string content = ReadWebFile("Pages", "Shared", "_UiIcon.cshtml");

        Assert.DoesNotContain("http://", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<img", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<use", content, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadWebFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path = Path.Combine(
                    new[] { directory.FullName, "src", "GitHubProof.Web" }
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

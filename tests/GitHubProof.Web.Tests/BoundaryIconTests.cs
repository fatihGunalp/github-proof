using GitHubProof.Web.Localization;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class BoundaryDisclosureTests
{
    [Fact]
    public void Landing_uses_native_details_and_summary_without_scripted_disclosure()
    {
        string content = ReadWebFile("Pages", "Index.cshtml");

        Assert.Contains("<details class=\"boundaries\">", content, StringComparison.Ordinal);
        Assert.Contains("<summary class=\"boundaries__summary\">", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" open", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Localized_summary_and_disclosure_label_match_the_locked_copy()
    {
        WebPresentationLocalizer text = new();

        Assert.Equal(
            "GitHub Proof does not evaluate you; it only shows traces visible in public GitHub work.",
            text.Get(WebTextKey.BoundariesSummary, WebCulture.English));
        Assert.Equal(
            "View boundaries →",
            text.Get(WebTextKey.BoundariesDisclosureLabel, WebCulture.English));
        Assert.Equal(
            "GitHub Proof sizi değerlendirmez; yalnızca public GitHub'da görülebilen izleri gösterir.",
            text.Get(WebTextKey.BoundariesSummary, WebCulture.Turkish));
        Assert.Equal(
            "Sınırları gör →",
            text.Get(WebTextKey.BoundariesDisclosureLabel, WebCulture.Turkish));
    }

    [Fact]
    public void Four_localized_boundary_meanings_remain_text_content_when_expanded()
    {
        string content = ReadWebFile("Pages", "Index.cshtml");

        WebTextKey[] boundaryMeanings =
        [
            WebTextKey.NoDeveloperRankingTitle,
            WebTextKey.NoSeniorityTitle,
            WebTextKey.NoAiAuthorshipTitle,
            WebTextKey.NoHireVerdictTitle
        ];

        foreach (WebTextKey meaning in boundaryMeanings)
        {
            Assert.Contains($"WebTextKey.{meaning}", content, StringComparison.Ordinal);
        }

        Assert.Equal(4, content.Split("<li>", StringSplitOptions.None).Length - 1);

        WebPresentationLocalizer text = new();
        string[] english =
        [
            "No developer ranking",
            "No seniority judgment",
            "No AI authorship claims",
            "No hire/no-hire verdict"
        ];
        string[] turkish =
        [
            "Geliştirici sıralaması yok",
            "Kıdem yargısı yok",
            "Yapay zekâ yazarlığı iddiası yok",
            "İşe al / alma hükmü yok"
        ];

        Assert.Equal(
            english,
            boundaryMeanings.Select(key => text.Get(key, WebCulture.English)));
        Assert.Equal(
            turkish,
            boundaryMeanings.Select(key => text.Get(key, WebCulture.Turkish)));
    }

    [Fact]
    public void Landing_boundary_renders_no_icons_or_competing_primary_action()
    {
        string content = ReadWebFile("Pages", "Index.cshtml");

        Assert.DoesNotContain("_UiIcon", content, StringComparison.Ordinal);
        Assert.DoesNotContain("UiIcon.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("boundaries__item-icon", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<button", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("class=\"button", content, StringComparison.OrdinalIgnoreCase);
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

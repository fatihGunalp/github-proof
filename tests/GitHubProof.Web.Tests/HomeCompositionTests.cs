using GitHubProof.Web.Localization;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class HomeCompositionTests
{
    // Supersedes Repository_input_accepts_only_canonical_repository_url_scope.
    // The separate repository-only entry field was replaced by one unified
    // GitHub input accepting a username, a profile URL or a repository URL;
    // which analysis flow runs is decided by GitHubEntryClassifier, not by the
    // field. type="url" is therefore no longer correct — it would reject a
    // bare login before the form is even submitted.
    [Fact]
    public void Github_input_accepts_the_three_unified_entry_shapes()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_GitHubInput.cshtml");

        Assert.Contains(
            "WebTextKey.InputLabel",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "type=\"text\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "name=\"GitHubTarget\"",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "type=\"url\"",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Landing_exposes_exactly_one_entry_form_and_no_separate_profile_link()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.DoesNotContain(
            "Analyze a profile instead",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "home__profile-link",
            content,
            StringComparison.Ordinal);

        Assert.Equal(
            1,
            CountOccurrences(content, "name=\"_GitHubInput\""));

        Assert.DoesNotContain(
            "_RepositoryInput",
            content,
            StringComparison.Ordinal);
    }

    private static int CountOccurrences(string content, string value)
    {
        int count = 0;
        int index = 0;

        while ((index = content.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    [Fact]
    public void Home_states_the_landing_hero_headline()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.Contains(
            "WebTextKey.LandingTitle",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "WebTextKey.LandingSummary",
            content,
            StringComparison.Ordinal);

        WebPresentationLocalizer text = new();
        Assert.Equal(
            "See what your GitHub communicates.",
            text.Get(WebTextKey.LandingTitle, WebCulture.English));
        Assert.Equal(
            "GitHub'ınızda ne olduğunu değil, ne anlattığını görün.",
            text.Get(WebTextKey.LandingTitle, WebCulture.Turkish));
    }

    [Fact]
    public void Home_exposes_one_secondary_native_boundary_disclosure()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.Contains(
            "<details class=\"boundaries\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "<summary class=\"boundaries__summary\">",
            content,
            StringComparison.Ordinal);

        Assert.Equal(1, CountOccurrences(content, "<details"));
        Assert.Equal(1, CountOccurrences(content, "<summary"));
        Assert.DoesNotContain("boundaries__grid", content, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"button\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Home_does_not_promote_a_numeric_score_as_primary_content()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.DoesNotContain(
            "/ 100",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Score:",
            content,
            StringComparison.OrdinalIgnoreCase);

    }

    [Fact]
    public void Github_input_posts_to_real_analysis_flow()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_GitHubInput.cshtml");

        Assert.Contains(
            "method=\"post\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "asp-page=\"/Index\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "type=\"submit\"",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "disabled",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Home_renders_existing_typed_report_after_success()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.Contains(
            "_RepositoryReport",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "Model.Report",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Markdown",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Composition_root_uses_canonical_live_seams()
    {
        string content = ReadWebFile(
            "Program.cs");

        Assert.Contains(
            "GitHubRepositorySnapshotOrchestrator.CreateDefault",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "RepositoryAnalysisRunner",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "RepositoryReportProjector",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "LiveRepositoryReportService",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "new GitHubRepositoryMetadataCollector",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "new GitHubRepositoryContentsCollector",
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

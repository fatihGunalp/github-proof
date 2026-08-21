using System.Text.RegularExpressions;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I3 Meaning / Direction / Starting Point result composition. These are
// static-source structural tests, matching this repository's existing
// RepositoryReportShellTests/CssArchitectureTests convention: they prove the
// .cshtml source keeps the canonical experience order and boundaries without
// standing up a full Razor rendering pipeline, which this Web test project
// does not otherwise carry.
public sealed class ProfileResultPresentationTests
{
    [Fact]
    public void Profile_page_renders_the_four_P10I3_sections_in_canonical_experience_order()
    {
        string content = ReadWebFile("Pages", "Profile.cshtml");

        int meaning = RequireSingleCall(content, "_ProfileMeaning");
        int technicalFocus = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.TechnicalFocusAreas");
        int engineeringPractice = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.EngineeringPracticePatterns");
        int startingPoint = RequireSingleCall(content, "_ProfileStartingPoint");

        Assert.True(meaning < technicalFocus, "Meaning must precede Visible technical direction.");
        Assert.True(technicalFocus < engineeringPractice, "Visible technical direction must precede Recurring engineering practices.");
        Assert.True(engineeringPractice < startingPoint, "Recurring engineering practices must precede Starting point.");
    }

    [Fact]
    public void Profile_page_introduces_no_P10I4_or_P10I5_section()
    {
        // Razor comments (§8.4-style explanatory documentation) legitimately
        // name the deferred concepts to explain why they are absent; only the
        // executable markup/code must never reference them.
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        string[] forbidden =
        [
            "WhyThisConclusion",
            "RepositoryDrillDown",
            "Limitations",
            "Warnings",
            "ProblemAreaPatterns",
            "RoleEvidenceAlignment"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Meaning_partial_is_bounded_to_summary_facts_only()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileMeaning.cshtml");

        Assert.Contains(
            "@model IReadOnlyList<ProfileSummaryFact>",
            raw,
            StringComparison.Ordinal);

        string content = StripRazorComments(raw);

        string[] forbidden =
        [
            "PatternReportItem",
            "RepresentativeRepository",
            "ProfileRepositoryReference",
            "ProfileReport ",
            "Snapshot",
            "Signal",
            "Eligibility",
            "Score"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Pattern_section_partial_is_bounded_to_one_report_section_and_never_renders_ItemCount()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfilePatternSection.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<PatternReportItem>",
            raw,
            StringComparison.Ordinal);

        string content = StripRazorComments(raw);

        // Items.Count is used only as a structural presence check
        // (Model.Items.Count > 0); the count itself is never interpolated
        // into rendered output.
        Assert.DoesNotContain("@Model.Items.Count", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ItemCount", content, StringComparison.Ordinal);
        Assert.DoesNotContain("RepresentativeRepository", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_partial_renders_repository_identity_as_text_only_with_no_drilldown_link()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<RepresentativeRepository>",
            raw,
            StringComparison.Ordinal);

        Assert.Contains("FullName", raw, StringComparison.Ordinal);

        string content = StripRazorComments(raw);

        string[] forbidden =
        [
            "<a ",
            "href=",
            "PatternSupports",
            "AnalysisReference",
            "Eligibility",
            "\"best\"",
            "\"top\"",
            "\"strongest\"",
            "\"recommended\"",
            "ItemCount"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Pattern_and_starting_point_partials_never_hardcode_a_ranking_or_quality_adjective()
    {
        string[] partials =
        [
            "_ProfileMeaning.cshtml",
            "_ProfilePatternSection.cshtml",
            "_ProfileStartingPoint.cshtml"
        ];

        string[] forbiddenWords =
        [
            "strong", "weak", "senior", "junior", "expert", "skilled",
            "employable", "suitable", "best", "top", "strongest",
            "recommended", "impressive", "consistent"
        ];

        foreach (string partial in partials)
        {
            string content = StripRazorComments(
                    ReadWebFile("Pages", "Shared", partial))
                .ToLowerInvariant();

            foreach (string word in forbiddenWords)
            {
                Assert.DoesNotContain(word, content, StringComparison.Ordinal);
            }
        }
    }

    private static readonly Regex RazorCommentPattern = new(
        @"@\*.*?\*@",
        RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static string StripRazorComments(string content) =>
        RazorCommentPattern.Replace(content, string.Empty);

    private static int RequireSingleCall(
        string content,
        string partialName,
        string? modelExpression = null)
    {
        string modelPattern = modelExpression is null
            ? @"[^)]+"
            : Regex.Escape(modelExpression);

        Regex regex = new(
            $@"@await\s+Html\.PartialAsync\(\s*""{Regex.Escape(partialName)}""\s*,\s*{modelPattern}\s*\)",
            RegexOptions.CultureInvariant);

        MatchCollection matches = regex.Matches(content);

        Assert.True(
            matches.Count == 1,
            $"Expected exactly one call to '{partialName}'"
            + (modelExpression is null ? string.Empty : $" with model '{modelExpression}'")
            + $", found {matches.Count}.");

        return matches[0].Index;
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
                    [directory.FullName, "src", "GitHubProof.Web", .. relativeSegments]);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Expected Web file was not found: {path}");
                }

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the repository root (GitHubProof.slnx) from "
            + AppContext.BaseDirectory);
    }
}

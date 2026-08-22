using System.Text.RegularExpressions;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I3 Meaning / Direction / Starting Point and P10-I4 Why / Repository
// Evidence result composition. These are static-source structural tests,
// matching this repository's existing RepositoryReportShellTests/
// CssArchitectureTests convention: they prove the .cshtml source keeps the
// canonical experience order and boundaries without standing up a full Razor
// rendering pipeline, which this Web test project does not otherwise carry.
public sealed class ProfileResultPresentationTests
{
    [Fact]
    public void Profile_page_renders_the_six_P10I3_and_P10I4_sections_in_canonical_experience_order()
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
        int why = RequireSingleCall(
            content,
            "_ProfileWhyThisView",
            "report.WhyThisConclusion");
        int repositoryEvidence = RequireSingleCall(
            content,
            "_ProfileRepositoryEvidence",
            "report.RepositoryDrillDown");

        Assert.True(meaning < technicalFocus, "Meaning must precede Visible technical direction.");
        Assert.True(technicalFocus < engineeringPractice, "Visible technical direction must precede Recurring engineering practices.");
        Assert.True(engineeringPractice < startingPoint, "Recurring engineering practices must precede Starting point.");
        Assert.True(startingPoint < why, "Starting point must precede Why this view.");
        Assert.True(why < repositoryEvidence, "Why this view must precede Repository evidence.");
    }

    [Fact]
    public void Profile_page_still_introduces_no_P10I5_section()
    {
        // Razor comments legitimately name deferred concepts to explain why
        // they are absent; only the executable markup/code must never
        // reference them. WhyThisConclusion and RepositoryDrillDown are now
        // legitimately present (P10-I4) and are intentionally excluded from
        // this forbidden list.
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        string[] forbidden =
        [
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
        // Regression guard: P10-I4 explicitly keeps Starting Point text-only
        // (presentation-language-contract.md §8.6.1) — the Repository Proof
        // action lives only in _ProfileRepositoryEvidence.
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
            "<form",
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
    public void Why_partial_is_bounded_to_the_conclusion_evidence_model_and_guards_section_and_availability()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<ConclusionEvidenceReportItem>",
            raw,
            StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionKey.WhyThisConclusion", raw, StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionAvailability.Supported", raw, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void Why_partial_uses_native_collapsed_disclosure_for_non_empty_and_plain_section_for_empty()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml"));

        Assert.Contains("<details", content, StringComparison.Ordinal);
        Assert.Contains("<summary", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", content, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-expanded", content, StringComparison.Ordinal);
        Assert.Contains("GetEmptySectionMessage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Why_partial_renders_pattern_label_and_supporting_repositories_only()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml"));

        Assert.Contains("GetPatternLabel", content, StringComparison.Ordinal);
        Assert.Contains("GetWhySupportingRepositoriesLeadIn", content, StringComparison.Ordinal);
        Assert.Contains("SupportingRepositories", content, StringComparison.Ordinal);
        Assert.Contains("repository.FullName", content, StringComparison.Ordinal);

        string[] forbidden =
        [
            "IndependentSupportingRepositories",
            "SupportingEvidence",
            "EvidenceKey",
            "EvidenceKeys",
            "AnalysisReference",
            "ItemCount",
            "RuleVersion",
            "ObservationRuleVersion",
            "ObservationKey"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }

        // The presence check (Model.Items.Count > 0) is structural, never an
        // interpolated count.
        Assert.DoesNotContain("@Model.Items.Count", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_partial_is_bounded_to_the_drilldown_model_and_guards_section_and_availability()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<RepositoryDrillDownReportItem>",
            raw,
            StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionKey.RepositoryDrillDown", raw, StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionAvailability.Supported", raw, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_partial_uses_native_collapsed_disclosure_for_non_empty_and_plain_section_for_empty()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains("<details", content, StringComparison.Ordinal);
        Assert.Contains("<summary", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", content, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-expanded", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryFreshAnalysisLeadIn", content, StringComparison.Ordinal);
        Assert.Contains("GetEmptySectionMessage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_partial_iterates_every_item_with_no_presentation_filtering_or_analysis_reference()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains("foreach", content, StringComparison.Ordinal);
        Assert.Contains("Model.Items", content, StringComparison.Ordinal);
        Assert.Contains("item.Repository.FullName", content, StringComparison.Ordinal);
        Assert.Contains("item.EligibilityStatus", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryStatusLabel", content, StringComparison.Ordinal);

        string[] forbidden =
        [
            "AnalysisReference",
            ".Where(",
            ".OrderBy(",
            ".OrderByDescending(",
            ".GroupBy(",
            ".Take(",
            ".Skip(",
            "ItemCount",
            "EvidenceKey",
            "EvidenceKeys"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Repository_evidence_partial_posts_repository_url_as_GitHubTarget_and_never_uses_it_as_href()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains("<form", content, StringComparison.Ordinal);
        Assert.Contains("method=\"post\"", content, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Index\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"GitHubTarget\"", content, StringComparison.Ordinal);
        Assert.Contains("value=\"@item.Repository.Url\"", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-form", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-submit", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryActionAccessibleName", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryActionLabel", content, StringComparison.Ordinal);

        Assert.DoesNotContain("<a ", content, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"@item.Repository.Url\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("href=@item.Repository.Url", content, StringComparison.Ordinal);
        Assert.DoesNotContain("method=\"get\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void New_P10I4_partials_never_hardcode_a_ranking_quality_or_capability_adjective()
    {
        string[] partials =
        [
            "_ProfileMeaning.cshtml",
            "_ProfilePatternSection.cshtml",
            "_ProfileStartingPoint.cshtml",
            "_ProfileWhyThisView.cshtml",
            "_ProfileRepositoryEvidence.cshtml"
        ];

        string[] forbiddenWords =
        [
            "strong", "weak", "senior", "junior", "expert", "skilled",
            "employable", "suitable", "best", "top", "strongest",
            "recommended", "impressive", "consistent", "confidence",
            "capability", "rank", "score"
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

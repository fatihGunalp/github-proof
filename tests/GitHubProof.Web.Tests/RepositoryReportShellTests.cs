using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class RepositoryReportShellTests
{
    [Fact]
    public void Report_shell_is_strongly_typed_to_repository_report()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "@model RepositoryReport",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_delegates_section_content_to_bounded_partials()
    {
        string shell = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        string[] partials =
        {
            "_RepositoryReportIdentity",
            "_RepositoryReportScoreCard",
            "_RepositoryReportProves",
            "_RepositoryReportWeakens",
            "_RepositoryReportActions",
            "_RepositoryReportUncertainty",
            "_RepositoryReportScore",
            "_RepositoryReportSignals",
            "_RepositoryReportDisclaimer"
        };

        int previousIndex = -1;

        foreach (string partial in partials)
        {
            int index = shell.IndexOf(
                $"\"{partial}\"",
                StringComparison.Ordinal);

            Assert.True(
                index > previousIndex,
                $"Report partial order is invalid at {partial}.");

            previousIndex = index;
        }

        Assert.DoesNotContain(
            "foreach (RepositoryReportHighlight",
            shell,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "foreach (SignalPresentation",
            shell,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_preserves_canonical_evidence_first_section_order()
    {
        string content = ReadReportComposition();

        string[] sectionMarkers =
        {
            "RepositoryReportSectionKeys.Identity",
            "RepositoryReportSectionKeys.Proves",
            "RepositoryReportSectionKeys.Weakens",
            "RepositoryReportSectionKeys.DoFirst",
            "RepositoryReportSectionKeys.Uncertainty",
            "RepositoryReportSectionKeys.CategoriesAndScore",
            "RepositoryReportSectionKeys.SignalDetails",
            "RepositoryReportSectionKeys.Disclaimer"
        };

        int previousIndex = -1;

        foreach (string marker in sectionMarkers)
        {
            int index = content.IndexOf(
                $"data-report-section=\"@{marker}\"",
                StringComparison.Ordinal);

            Assert.True(
                index > previousIndex,
                $"Report section order is invalid at {marker}.");

            previousIndex = index;
        }
    }

    [Fact]
    public void Report_shell_builds_four_card_decision_viewport_without_changing_canonical_section_order()
    {
        string content = ReadReportComposition();

        int grid = content.IndexOf(
            "report-first-reading report-decision-grid",
            StringComparison.Ordinal);

        int scoreCard = content.IndexOf(
            "report-summary-card--score",
            StringComparison.Ordinal);

        int proves = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Proves\"",
            StringComparison.Ordinal);

        int weakens = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Weakens\"",
            StringComparison.Ordinal);

        int doFirst = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.DoFirst\"",
            StringComparison.Ordinal);

        int uncertainty = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Uncertainty\"",
            StringComparison.Ordinal);

        int firstReadingClassCount =
            content.Split(
                "report-section--first-reading",
                StringSplitOptions.None)
            .Length - 1;

        Assert.True(grid >= 0);
        Assert.True(grid < scoreCard);
        Assert.True(scoreCard < proves);
        Assert.True(proves < weakens);
        Assert.True(weakens < doFirst);
        Assert.True(doFirst < uncertainty);
        Assert.Equal(3, firstReadingClassCount);
    }

    [Fact]
    public void First_reading_layer_avoids_repeating_deep_action_and_evidence_detail()
    {
        string content = ReadReportComposition();

        Assert.DoesNotContain(
            "@action.EffortLabel",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@action.WhyItMatters",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@item.WhyItMatters",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@item.Evidence",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_reuses_canonical_status_badge()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "new StatusBadgeModel(",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "item.StatusLabel",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "signal.StatusLabel",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_identity_matches_locked_reference_hero_composition()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "report-identity-card",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.Github",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.Link2",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.GitBranch",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.Code2",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-identity__url-chip",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-identity__status-card",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-identity__status-visual",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.ChartPie",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-identity__generated",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.Clock",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "report-identity-details",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "UiIcon.FolderGit2",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_identity_surfaces_url_branch_language_and_snapshot_time_in_first_layer()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReportIdentity.cshtml");

        Assert.Contains(
            "@Model.Identity.Url",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Identity.DefaultBranch",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Identity.PrimaryLanguage",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Identity.CapturedAt",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@Model.Identity.HeadCommitSha",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void First_reading_uses_semantic_icons_and_does_not_repeat_highlight_explanations()
    {
        string content = ReadReportComposition();

        int start = content.IndexOf(
            "<div class=\"report-first-reading report-decision-grid\">",
            StringComparison.Ordinal);

        int end = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Uncertainty\"",
            StringComparison.Ordinal);

        Assert.True(start >= 0);
        Assert.True(end > start);

        string firstReading =
            content[start..end];

        Assert.Contains(
            "StatusIcon.CircleCheck",
            firstReading,
            StringComparison.Ordinal);

        Assert.Contains(
            "StatusIcon.CircleX",
            firstReading,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.ListTree",
            firstReading,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@item.Explanation",
            firstReading,
            StringComparison.Ordinal);

        Assert.Contains(
            "@action.Recommendation",
            firstReading,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Decision_viewport_uses_proportional_four_two_one_responsive_card_grid()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "report.css");

        Assert.Contains(
            ".report-decision-grid",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "minmax(12rem, 0.85fr)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "minmax(18rem, 1.45fr)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@media (max-width: 64rem)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "grid-template-columns: repeat(2, minmax(0, 1fr));",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@media (max-width: 48rem)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "grid-template-columns: 1fr;",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            ".report-evidence-snapshot",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Decision_viewport_surfaces_overall_score_without_moving_canonical_score_section()
    {
        string content = ReadReportComposition();

        int summaryCard = content.IndexOf(
            "report-summary-card--score",
            StringComparison.Ordinal);

        int proves = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Proves\"",
            StringComparison.Ordinal);

        int canonicalScore = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.CategoriesAndScore\"",
            StringComparison.Ordinal);

        Assert.True(summaryCard >= 0);
        Assert.True(summaryCard < proves);
        Assert.True(canonicalScore > proves);

        Assert.Contains(
            "@Model.ScoreSummary.NormalizedScore",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Score_summary_card_prioritizes_decision_counts_over_raw_scoring_metadata()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReportScoreCard.cshtml");

        Assert.Contains(
            "@Model.Proves.Items.Count",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Weakens.Items.Count",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Uncertainty.Items.Count",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-score-card__metrics",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@Model.ScoreSummary.Categories.Count",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@Model.ScoreSummary.EarnedScore",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "@Model.ScoreSummary.ApplicableMaximumScore",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Category_summary_is_collapsed_by_default_and_surfaces_secondary_score()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "report-layer__details--categories",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.ChartNoAxesColumnIncreasing",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.ScoreSummary.NormalizedScore/100",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "report-layer__details--categories\" open",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_boundary_is_collapsed_by_default_as_a_secondary_layer()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "report-layer__details--disclaimer",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.Info",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "report-layer__details--disclaimer\" open",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Curated_ui_icons_use_lucide_outline_geometry_and_preserve_license_notice()
    {
        string icon = ReadWebFile(
            "Pages",
            "Shared",
            "_UiIcon.cshtml");

        string license = ReadWebFile(
            "wwwroot",
            "vendor",
            "lucide",
            "LICENSE.txt");

        Assert.Contains(
            "stroke=\"currentColor\"",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke-width=\"1.75\"",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.ChartNoAxesColumnIncreasing",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.Info",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "UiIcon.ChevronRight",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "ISC License",
            license,
            StringComparison.Ordinal);

        Assert.Contains(
            "Lucide Icons and Contributors",
            license,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Category_summary_uses_compact_diagnostic_grid_instead_of_vertical_dashboard_rows()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "report.css");

        Assert.Contains(
            "grid-template-columns: repeat(4, minmax(0, 1fr));",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "font-variant-numeric: tabular-nums;",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@media (max-width: 64rem)",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "grid-template-columns: repeat(2, minmax(0, 1fr));",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_keeps_score_after_uncertainty_and_before_signal_details()
    {
        string content = ReadReportComposition();

        int uncertainty = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Uncertainty\"",
            StringComparison.Ordinal);

        int score = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.CategoriesAndScore\"",
            StringComparison.Ordinal);

        int signals = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.SignalDetails\"",
            StringComparison.Ordinal);

        Assert.True(uncertainty < score);
        Assert.True(score < signals);
    }

    [Fact]
    public void Uncertainty_uses_compact_progressive_disclosure_and_hides_when_empty()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "Model.Uncertainty.Items.Count > 0",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "|| Model.Warnings.Count > 0",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "|| Model.Limitations.Count > 0",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "<details class=\"report-uncertainty\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "<summary class=\"report-uncertainty__summary\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-uncertainty__count",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            ".Distinct(StringComparer.Ordinal)",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_disclosure_affordances_use_one_outline_svg_icon()
    {
        string report = ReadReportComposition();

        string icon = ReadWebFile(
            "Pages",
            "Shared",
            "_DisclosureIcon.cshtml");

        Assert.Contains(
            "_DisclosureIcon",
            report,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-uncertainty__chevron",
            report,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-signal__chevron",
            report,
            StringComparison.Ordinal);

        Assert.Contains(
            "<svg",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke=\"currentColor\"",
            icon,
            StringComparison.Ordinal);

        Assert.Contains(
            "stroke-width=\"1.75\"",
            icon,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "›",
            report,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Signal_details_layer_is_collapsed_by_default_behind_top_level_progressive_disclosure()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "<details class=\"report-signal-section\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "<summary class=\"report-signal-section__summary\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-signal-section__count",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.SignalDetails.Items.Count",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-signal-section__chevron",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "<details class=\"report-signal-section\" open>",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Signal_details_use_progressive_disclosure()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "<details class=\"report-signal\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "<summary class=\"report-signal__summary\">",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Signal_details_expose_a_non_textual_disclosure_affordance()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "report-signal__chevron",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "aria-hidden=\"true\"",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_renders_disclaimer_policy_version_with_explicit_razor_expression()
    {
        string content = ReadReportComposition();

        Assert.Contains(
            "v@(Model.Disclaimer.PolicyVersion)",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "v@Model.Disclaimer.PolicyVersion",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_does_not_parse_markdown()
    {
        string content = ReadReportComposition();

        Assert.DoesNotContain(
            "RepositoryReportMarkdownRenderer",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Markdown",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadReportComposition()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        string[] partialNames =
        {
            "_RepositoryReportIdentity",
            "_RepositoryReportScoreCard",
            "_RepositoryReportProves",
            "_RepositoryReportWeakens",
            "_RepositoryReportActions",
            "_RepositoryReportUncertainty",
            "_RepositoryReportScore",
            "_RepositoryReportSignals",
            "_RepositoryReportDisclaimer"
        };

        foreach (string partialName in partialNames)
        {
            string contentValue = ReadWebFile(
                "Pages",
                "Shared",
                $"{partialName}.cshtml");

            string escapedPartialName =
                System.Text.RegularExpressions.Regex.Escape(
                    partialName);

            string pattern =
                $@"@await\s+Html\.PartialAsync\(\s*""{escapedPartialName}"",\s*Model\s*\)";

            var regex =
                new System.Text.RegularExpressions.Regex(
                    pattern,
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            System.Text.RegularExpressions.MatchCollection matches =
                regex.Matches(content);

            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one report partial call for "
                    + $"{partialName}, found {matches.Count}.");
            }

            content = regex.Replace(
                content,
                _ => contentValue,
                1);
        }

        return content;
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
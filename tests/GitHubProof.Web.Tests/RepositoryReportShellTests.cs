using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class RepositoryReportShellTests
{
    [Fact]
    public void Report_shell_is_strongly_typed_to_repository_report()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        Assert.Contains(
            "@model RepositoryReport",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Report_shell_preserves_canonical_evidence_first_section_order()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
    public void Report_shell_groups_three_canonical_first_reading_sections_for_scan_first_layout()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        int group = content.IndexOf(
            "<div class=\"report-first-reading\">",
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

        int evidenceSnapshot = content.IndexOf(
            "<div class=\"report-evidence-snapshot\">",
            StringComparison.Ordinal);

        Assert.True(group >= 0);
        Assert.True(group < evidenceSnapshot);
        Assert.True(evidenceSnapshot < proves);
        Assert.True(proves < weakens);
        Assert.True(weakens < doFirst);
        Assert.True(doFirst < uncertainty);
        Assert.Equal(3, firstReadingClassCount);
    }

    [Fact]
    public void First_reading_layer_avoids_repeating_deep_action_and_evidence_detail()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
    public void Repository_identity_defers_deep_trust_metadata_behind_progressive_disclosure()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        Assert.Contains(
            "<details class=\"report-identity-details\">",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "report-identity__name-link",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Identity.HeadCommitSha",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@Model.Identity.CapturedAt.ToString(\"O\")",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "report-section__eyebrow",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "report-identity__meta",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void First_reading_uses_semantic_icons_and_does_not_repeat_highlight_explanations()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        int start = content.IndexOf(
            "<div class=\"report-first-reading\">",
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
    public void First_reading_groups_proves_and_weakens_as_two_column_evidence_snapshot_on_desktop()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "report.css");

        Assert.Contains(
            ".report-evidence-snapshot",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "grid-template-columns: repeat(2, minmax(0, 1fr));",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "border-left: 1px solid var(--border-subtle);",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "@media (max-width: 64rem)",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Category_summary_is_collapsed_by_default_and_surfaces_secondary_score()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string report = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

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
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryReport.cshtml");

        Assert.DoesNotContain(
            "RepositoryReportMarkdownRenderer",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Markdown",
            content,
            StringComparison.OrdinalIgnoreCase);
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
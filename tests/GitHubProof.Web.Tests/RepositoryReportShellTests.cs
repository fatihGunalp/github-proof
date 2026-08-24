using System.Text;
using System.Text.RegularExpressions;
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

    // Session 119. The first-read decision layer is evidence-first: exactly
    // Proves -> Weakens -> Do First. The evidence score used to lead this layer
    // as a fourth card with a circular gauge, which contradicted ui-system §2
    // ("Evidence -> Meaning -> Action -> Score", explicitly avoiding
    // "72 / 100 -> her şey"), §22's canonical first-reading order, §21.10's
    // "Avoid: circular gauge", and scoring-and-report-contract §14 ("Toplam ve
    // kategori skorları korunur ancak ilk okuma katmanının ana mesajı olmaz").
    // The score is unchanged; it now appears only in its canonical
    // Categories + Score surface below.
    [Fact]
    public void Report_shell_builds_an_evidence_first_decision_viewport_without_changing_canonical_section_order()
    {
        string content = ReadReportComposition();

        int grid = content.IndexOf(
            "report-first-reading report-decision-grid",
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
        Assert.True(grid < proves);
        Assert.True(proves < weakens);
        Assert.True(weakens < doFirst);
        Assert.True(doFirst < uncertainty);
        Assert.Equal(3, firstReadingClassCount);
    }

    // The score must not return to the first-read layer in any form — neither
    // as the old gauge card nor as another quiet summary card.
    [Fact]
    public void First_reading_layer_does_not_render_the_evidence_score()
    {
        string content = ReadReportComposition();
        string shell = ReadWebFile("Pages", "Shared", "_RepositoryReport.cshtml");

        foreach (string marker in new[]
        {
            "_RepositoryReportScoreCard",
            "report-summary-card--score",
            "report-score-card"
        })
        {
            Assert.DoesNotContain(marker, content, StringComparison.Ordinal);
        }

        int grid = shell.IndexOf(
            "report-first-reading report-decision-grid",
            StringComparison.Ordinal);
        int gridEnd = shell.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Uncertainty\"",
            StringComparison.Ordinal);

        Assert.True(grid >= 0 && gridEnd > grid);

        string firstRead = shell[grid..gridEnd];

        Assert.DoesNotContain("ScoreSummary", firstRead, StringComparison.Ordinal);
        Assert.DoesNotContain("NormalizedScore", firstRead, StringComparison.Ordinal);
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

    // Session 119: three columns, not four — the score column was removed from
    // the first-read layer. The retained Proves / Weakens / Do First keep their
    // previous relative proportions, and the 64rem -> 2 column and 48rem -> 1
    // column collapse is unchanged.
    //
    // The desktop column structure is parsed out of the .report-decision-grid
    // declaration itself rather than string-matched against the whole file:
    // a bare Contains() would still pass if one of the two evidence columns
    // disappeared, which is exactly the regression this guard exists to catch.
    [Fact]
    public void Decision_viewport_uses_proportional_three_two_one_responsive_card_grid()
    {
        string content = ReadWebFile(
            "wwwroot",
            "css",
            "report.css");

        Assert.Contains(
            ".report-decision-grid",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "minmax(12rem, 0.85fr)",
            content,
            StringComparison.Ordinal);

        string[] columns = ReadDesktopDecisionGridColumns(content);

        Assert.Equal(3, columns.Length);
        Assert.Equal("minmax(0, 1.1fr)", columns[0]);
        Assert.Equal("minmax(0, 1.1fr)", columns[1]);
        Assert.Equal("minmax(18rem, 1.45fr)", columns[2]);

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

    // Score semantics are untouched by the first-read correction: the canonical
    // Categories + Score surface still renders the normalized score, the
    // per-category scores and the /100 presentation, and it still sits after
    // uncertainty and before signal details (ui-system §22 position 6).
    [Fact]
    public void Canonical_score_surface_still_renders_score_after_the_first_read_correction()
    {
        string content = ReadReportComposition();

        int proves = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.Proves\"",
            StringComparison.Ordinal);

        int canonicalScore = content.IndexOf(
            "data-report-section=\"@RepositoryReportSectionKeys.CategoriesAndScore\"",
            StringComparison.Ordinal);

        Assert.True(canonicalScore > proves);

        foreach (string marker in new[]
        {
            "@Model.ScoreSummary.NormalizedScore",
            "@category.NormalizedScore",
            "@category.Score / @category.MaximumScore",
            "/100"
        })
        {
            Assert.Contains(marker, content, StringComparison.Ordinal);
        }
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

    // Extracts the desktop grid-template-columns track list of the top-level
    // .report-decision-grid rule (the one outside any @media block) and returns
    // one entry per column, so column COUNT and ORDER are both assertable.
    private static string[] ReadDesktopDecisionGridColumns(string css)
    {
        Match rule = Regex.Match(
            css,
            @"(?m)^\.report-decision-grid\s*\{(?<body>[^}]*)\}",
            RegexOptions.CultureInvariant);

        Assert.True(
            rule.Success,
            "No top-level .report-decision-grid rule was found in report.css.");

        Match tracks = Regex.Match(
            rule.Groups["body"].Value,
            @"grid-template-columns\s*:(?<value>[^;]*);",
            RegexOptions.CultureInvariant);

        Assert.True(
            tracks.Success,
            ".report-decision-grid declares no grid-template-columns.");

        // Split on the whitespace that separates tracks, not on the characters
        // inside minmax(...).
        string value = tracks.Groups["value"].Value;
        List<string> columns = [];
        int depth = 0;
        StringBuilder current = new();

        foreach (char character in value)
        {
            if (character == '(')
            {
                depth++;
            }
            else if (character == ')')
            {
                depth--;
            }

            current.Append(character);

            if (depth == 0 && character == ')')
            {
                columns.Add(Regex.Replace(current.ToString(), @"\s+", " ").Trim());
                current.Clear();
            }
        }

        // A track that is not a function call (e.g. a bare `0.85fr`) leaves a
        // remainder behind. Without this it would be silently discarded and a
        // four-column regression could still pass.
        string remainder = Regex.Replace(current.ToString(), @"\s+", " ").Trim();

        if (remainder.Length > 0)
        {
            columns.Add(remainder);
        }

        return [.. columns];
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
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
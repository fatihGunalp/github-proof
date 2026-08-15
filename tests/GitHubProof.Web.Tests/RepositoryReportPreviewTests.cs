using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Web.Development;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class RepositoryReportPreviewTests
{
    [Fact]
    public void Preview_factory_produces_canonical_report()
    {
        RepositoryReport report =
            RepositoryReportPreviewFactory.Create("en");

        Assert.Equal(
            RepositoryReportSectionKeys.CanonicalOrder,
            report.SectionOrder);

        Assert.True(report.Proves.Items.Count <= 3);
        Assert.True(report.Weakens.Items.Count <= 3);
        Assert.True(report.DoFirst.Items.Count <= 3);
    }

    [Fact]
    public void Preview_exercises_all_seven_signal_status_semantics()
    {
        RepositoryReport report =
            RepositoryReportPreviewFactory.Create("en");

        SignalStatus[] actual =
            report.Proves.Items
                .Select(item => item.Status)
                .Concat(report.Weakens.Items.Select(item => item.Status))
                .Concat(report.Uncertainty.Items.Select(item => item.Status))
                .Concat(report.SignalDetails.Items.Select(item => item.Status))
                .Distinct()
                .OrderBy(status => status)
                .ToArray();

        SignalStatus[] expected =
            Enum.GetValues<SignalStatus>()
                .OrderBy(status => status)
                .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Preview_preserves_repository_literals_between_locales()
    {
        RepositoryReport english =
            RepositoryReportPreviewFactory.Create("en");

        RepositoryReport turkish =
            RepositoryReportPreviewFactory.Create("tr");

        Assert.Equal(
            english.Identity.FullName,
            turkish.Identity.FullName);

        Assert.Equal(
            english.Identity.Url,
            turkish.Identity.Url);

        Assert.NotEqual(
            english.Proves.Title,
            turkish.Proves.Title);
    }

    [Fact]
    public void Preview_page_is_explicitly_development_only()
    {
        string model = ReadWebFile(
            "Pages",
            "ReportPreview.cshtml.cs");

        Assert.Contains(
            "environment.IsDevelopment()",
            model,
            StringComparison.Ordinal);

        Assert.Contains(
            "return NotFound();",
            model,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Preview_is_not_linked_from_home()
    {
        string home = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.DoesNotContain(
            "report-preview",
            home,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadWebFile(params string[] segments)
    {
        DirectoryInfo? directory =
            new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(
                Path.Combine(
                    directory.FullName,
                    "GitHubProof.slnx")))
            {
                string path = Path.Combine(
                    new[]
                    {
                        directory.FullName,
                        "src",
                        "GitHubProof.Web"
                    }
                    .Concat(segments)
                    .ToArray());

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Repository root not found.");
    }
}
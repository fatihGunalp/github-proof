using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class AnalysisHighlightTests
{
    [Fact]
    public void Constructor_NormalizesRequiredValuesAndEvidenceReferences()
    {
        AnalysisHighlight highlight = new(
            HighlightType.CriticalGap,
            " repository.readme.exists ",
            " README is missing ",
            " A public README makes the repository easier to understand. ",
            scoreImpact: -5,
            [" evidence.readme-inspected ", "evidence.readme-inspected", " "]);

        Assert.Equal(HighlightType.CriticalGap, highlight.Type);
        Assert.Equal("repository.readme.exists", highlight.SignalKey);
        Assert.Equal("README is missing", highlight.Title);
        Assert.Equal(
            "A public README makes the repository easier to understand.",
            highlight.Summary);
        Assert.Equal(-5, highlight.ScoreImpact);
        Assert.Equal(["evidence.readme-inspected"], highlight.EvidenceReferences);
    }

    [Theory]
    [InlineData("signalKey")]
    [InlineData("title")]
    [InlineData("summary")]
    public void Constructor_RejectsMissingRequiredText(string missingParameter)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateHighlight(
                signalKey: missingParameter == "signalKey" ? " " : "repository.readme.exists",
                title: missingParameter == "title" ? " " : "Title",
                summary: missingParameter == "summary" ? " " : "Summary"));

        Assert.Equal(missingParameter, exception.ParamName);
    }

    [Fact]
    public void Constructor_RequiresAtLeastOneEvidenceReference()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateHighlight(evidenceReferences: [" "]));

        Assert.Equal("evidenceReferences", exception.ParamName);
    }

    private static AnalysisHighlight CreateHighlight(
        string signalKey = "repository.readme.exists",
        string title = "Title",
        string summary = "Summary",
        IEnumerable<string>? evidenceReferences = null) =>
        new(
            HighlightType.Strength,
            signalKey,
            title,
            summary,
            scoreImpact: 5,
            evidenceReferences ?? ["evidence.readme"]);
}

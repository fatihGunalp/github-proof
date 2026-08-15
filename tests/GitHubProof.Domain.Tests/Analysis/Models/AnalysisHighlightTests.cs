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
            scoreImpact: -5,
            evidenceReferences: [" evidence.readme-inspected ", "evidence.readme-inspected", " "]);

        Assert.Equal(HighlightType.CriticalGap, highlight.Type);
        Assert.Equal("repository.readme.exists", highlight.SignalKey);
        Assert.Equal(-5, highlight.ScoreImpact);
        Assert.Equal(["evidence.readme-inspected"], highlight.EvidenceReferences);
    }

    [Fact]
    public void Constructor_RejectsMissingRequiredText()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateHighlight(
                signalKey: " "));

        Assert.Equal("signalKey", exception.ParamName);
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
        IEnumerable<string>? evidenceReferences = null) =>
        new(
            HighlightType.Strength,
            signalKey,
            scoreImpact: 5,
            evidenceReferences: evidenceReferences ?? ["evidence.readme"]);
}

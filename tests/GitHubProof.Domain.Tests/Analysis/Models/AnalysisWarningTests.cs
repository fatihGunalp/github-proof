using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class AnalysisWarningTests
{
    [Fact]
    public void Constructor_NormalizesWarningAndRelatedReferences()
    {
        AnalysisWarning warning = new(
            " github.rate-limit ",
            WarningSeverity.Warning,
            " Commit history could not be fully collected. ",
            [" commits ", "commits", " "],
            [" process.commit-history ", "process.commit-history"]);

        Assert.Equal("github.rate-limit", warning.Code);
        Assert.Equal(WarningSeverity.Warning, warning.Severity);
        Assert.Equal("Commit history could not be fully collected.", warning.PresentationKey);
        Assert.Equal(["commits"], warning.RelatedDataKeys);
        Assert.Equal(["process.commit-history"], warning.RelatedSignals);
    }

    [Theory]
    [InlineData("code")]
    [InlineData("presentationKey")]
    public void Constructor_RejectsMissingRequiredText(string missingParameter)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateWarning(
                code: missingParameter == "code" ? " " : "warning.code",
                presentationKey: missingParameter == "presentationKey" ? " " : "warning.message"));

        Assert.Equal(missingParameter, exception.ParamName);
    }

    [Fact]
    public void Constructor_AllowsWarningsWithoutRelatedReferences()
    {
        AnalysisWarning warning = CreateWarning(
            relatedDataKeys: null,
            relatedSignals: null);

        Assert.Empty(warning.RelatedDataKeys);
        Assert.Empty(warning.RelatedSignals);
    }

    private static AnalysisWarning CreateWarning(
        string code = "warning.code",
        string presentationKey = "warning.message",
        IEnumerable<string>? relatedDataKeys = null,
        IEnumerable<string>? relatedSignals = null) =>
        new(
            code,
            WarningSeverity.Info,
            presentationKey,
            relatedDataKeys,
            relatedSignals);
}

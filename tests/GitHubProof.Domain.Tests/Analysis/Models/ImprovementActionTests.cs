using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class ImprovementActionTests
{
    [Fact]
    public void Constructor_NormalizesActionAndTraceabilityReferences()
    {
        ImprovementAction action = new(
            priority: 1,
            title: " Add an installation section ",
            reason: " The repository cannot be reproduced from its visible documentation. ",
            expectedImpact: " Visitors can set up and run the repository. ",
            relatedSignals:
            [
                " documentation.installation-guide ",
                "documentation.installation-guide",
                " "
            ],
            effort: EffortLevel.Low,
            evidenceReferences:
            [
                " evidence.readme-inspected ",
                "evidence.readme-inspected"
            ]);

        Assert.Equal(1, action.Priority);
        Assert.Equal("Add an installation section", action.Title);
        Assert.Equal(
            "The repository cannot be reproduced from its visible documentation.",
            action.Reason);
        Assert.Equal(
            "Visitors can set up and run the repository.",
            action.ExpectedImpact);
        Assert.Equal(["documentation.installation-guide"], action.RelatedSignals);
        Assert.Equal(EffortLevel.Low, action.Effort);
        Assert.Equal(["evidence.readme-inspected"], action.EvidenceReferences);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_PreservesPriorityWithoutAddingAnUndocumentedConstraint(int priority)
    {
        ImprovementAction action = CreateAction(priority: priority);

        Assert.Equal(priority, action.Priority);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("reason")]
    [InlineData("expectedImpact")]
    public void Constructor_RejectsMissingRequiredText(string missingParameter)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAction(
                title: missingParameter == "title" ? " " : "Title",
                reason: missingParameter == "reason" ? " " : "Reason",
                expectedImpact: missingParameter == "expectedImpact" ? " " : "Impact"));

        Assert.Equal(missingParameter, exception.ParamName);
    }

    [Theory]
    [InlineData("relatedSignals")]
    [InlineData("evidenceReferences")]
    public void Constructor_RequiresSignalAndEvidenceReferences(string missingParameter)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAction(
                relatedSignals: missingParameter == "relatedSignals" ? [" "] : null,
                evidenceReferences: missingParameter == "evidenceReferences" ? [" "] : null));

        Assert.Equal(missingParameter, exception.ParamName);
    }

    private static ImprovementAction CreateAction(
        int priority = 1,
        string title = "Title",
        string reason = "Reason",
        string expectedImpact = "Impact",
        IEnumerable<string>? relatedSignals = null,
        IEnumerable<string>? evidenceReferences = null) =>
        new(
            priority,
            title,
            reason,
            expectedImpact,
            relatedSignals ?? ["repository.readme.exists"],
            EffortLevel.Medium,
            evidenceReferences ?? ["evidence.readme"]);
}

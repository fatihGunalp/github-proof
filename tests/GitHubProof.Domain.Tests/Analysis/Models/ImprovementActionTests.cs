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
            presentationKey: " action.add-installation-guide ",
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
        Assert.Equal("action.add-installation-guide", action.PresentationKey);
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

    [Fact]
    public void Constructor_RejectsMissingPresentationKey()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAction(presentationKey: " "));

        Assert.Equal("presentationKey", exception.ParamName);
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
        string presentationKey = "action.test",
        IEnumerable<string>? relatedSignals = null,
        IEnumerable<string>? evidenceReferences = null) =>
        new(
            priority,
            presentationKey,
            relatedSignals ?? ["repository.readme.exists"],
            EffortLevel.Medium,
            evidenceReferences ?? ["evidence.readme"]);
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class ImprovementPlanTests
{
    [Fact]
    public void Constructor_NormalizesVersionAndGeneratedAtAndRetainsUpToThreeActions()
    {
        DateTimeOffset localTime = new(
            2026,
            8,
            15,
            12,
            30,
            0,
            TimeSpan.FromHours(3));
        ImprovementAction first = CreateAction(1);
        ImprovementAction second = CreateAction(2);
        ImprovementAction third = CreateAction(3);

        ImprovementPlan plan = new(
            [first, second, third],
            localTime,
            " 2026.08.01 ");

        Assert.Equal([first, second, third], plan.Actions);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:30:00Z"), plan.GeneratedAt);
        Assert.Equal("2026.08.01", plan.RuleSetVersion);
    }

    [Fact]
    public void Constructor_RejectsMoreThanThreeActions()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new ImprovementPlan(
                [CreateAction(1), CreateAction(2), CreateAction(3), CreateAction(4)],
                DateTimeOffset.UtcNow,
                "2026.08.01"));

        Assert.Equal("actions", exception.ParamName);
    }

    [Fact]
    public void Constructor_AllowsAnEmptyPlan()
    {
        ImprovementPlan plan = new(
            Array.Empty<ImprovementAction>(),
            DateTimeOffset.UtcNow,
            "2026.08.01");

        Assert.Empty(plan.Actions);
    }

    [Fact]
    public void Constructor_RejectsMissingRuleSetVersion()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new ImprovementPlan(
                Array.Empty<ImprovementAction>(),
                DateTimeOffset.UtcNow,
                " "));

        Assert.Equal("ruleSetVersion", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsNonContiguousPriorities()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new ImprovementPlan(
                [CreateAction(1), CreateAction(3)],
                DateTimeOffset.UtcNow,
                "2026.08.01"));

        Assert.Equal("actions", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsDuplicatePresentationKeys()
    {
        ImprovementAction first = CreateAction(1);
        ImprovementAction duplicate = new(
            2,
            first.PresentationKey,
            ["signal.2"],
            EffortLevel.Low,
            ["evidence.2"]);

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new ImprovementPlan(
                [first, duplicate],
                DateTimeOffset.UtcNow,
                "2026.08.01"));

        Assert.Equal("actions", exception.ParamName);
    }

    private static ImprovementAction CreateAction(int priority) =>
        new(
            priority,
            $"action.{priority}",
            [$"signal.{priority}"],
            EffortLevel.Low,
            [$"evidence.{priority}"]);
}

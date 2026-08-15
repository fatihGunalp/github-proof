namespace GitHubProof.Domain.Analysis.Models;

public sealed record ImprovementPlan
{
    public const int MaximumActionCount = 3;

    public ImprovementPlan(
        IEnumerable<ImprovementAction> actions,
        DateTimeOffset generatedAt,
        string ruleSetVersion)
    {
        ArgumentNullException.ThrowIfNull(actions);

        ImprovementAction[] normalizedActions = actions
            .Where(action => action is not null)
            .Distinct()
            .ToArray();

        if (normalizedActions.Length > MaximumActionCount)
        {
            throw new ArgumentException(
                $"An improvement plan cannot contain more than {MaximumActionCount} actions.",
                nameof(actions));
        }

        Actions = normalizedActions;
        GeneratedAt = generatedAt.ToUniversalTime();
        RuleSetVersion = RequireValue(ruleSetVersion, nameof(ruleSetVersion));
    }

    public IReadOnlyList<ImprovementAction> Actions { get; }

    public DateTimeOffset GeneratedAt { get; }

    public string RuleSetVersion { get; }

    private static string RequireValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }
}

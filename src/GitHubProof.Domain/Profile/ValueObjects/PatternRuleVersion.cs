namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record PatternRuleVersion
{
    public PatternRuleVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

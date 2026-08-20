namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record PatternRuleVersion
{
    // The first canonical Domain runtime pattern rule version, established by
    // Session 088 for the production P8-I5 integration pipeline.
    public static readonly PatternRuleVersion V1 = new("1.0.0");

    public PatternRuleVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

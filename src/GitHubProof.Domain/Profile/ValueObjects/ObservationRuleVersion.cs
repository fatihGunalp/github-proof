namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record ObservationRuleVersion
{
    public ObservationRuleVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

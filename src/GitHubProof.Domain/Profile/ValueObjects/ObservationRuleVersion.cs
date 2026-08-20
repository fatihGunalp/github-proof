namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record ObservationRuleVersion
{
    // The first canonical Domain runtime observation rule version, established
    // by Session 088 for the production P8-I5 integration pipeline.
    public static readonly ObservationRuleVersion V1 = new("1.0.0");

    public ObservationRuleVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

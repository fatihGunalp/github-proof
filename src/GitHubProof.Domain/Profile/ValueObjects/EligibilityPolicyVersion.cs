namespace GitHubProof.Domain.Profile.ValueObjects;

public sealed record EligibilityPolicyVersion
{
    public EligibilityPolicyVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

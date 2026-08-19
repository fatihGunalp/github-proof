namespace GitHubProof.Domain.Profile.ValueObjects;

// Dedicated version identity for representative-repository selection rules.
// It never substitutes for PatternRuleVersion, ObservationRuleVersion or
// EligibilityPolicyVersion.
public sealed record RepresentativeRepositoryRuleVersion
{
    // The first canonical Domain runtime rule version, established by
    // Session 075 against Contract Version 1
    // (docs/architecture/public-profile-representative-repositories.md).
    public static readonly RepresentativeRepositoryRuleVersion V1 = new("1.0.0");

    public RepresentativeRepositoryRuleVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

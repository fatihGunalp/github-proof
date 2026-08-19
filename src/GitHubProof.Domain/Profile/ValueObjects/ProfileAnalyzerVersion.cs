namespace GitHubProof.Domain.Profile.ValueObjects;

// Version identity of the profile composition and aggregate semantics that
// produced one DeveloperPublicEvidenceProfile.
//
// It never substitutes for the individual upstream rule and policy versions:
// snapshot identity, this version and those rule versions are separate
// provenance dimensions, so an input change stays distinguishable from a rule
// change.
public sealed record ProfileAnalyzerVersion
{
    // The first canonical profile-level analyzer version, established by
    // Session 085 against Contract Version 1
    // (docs/architecture/public-profile-evidence-domain-model.md §14).
    public static readonly ProfileAnalyzerVersion V1 = new("1.0.0");

    public ProfileAnalyzerVersion(string value)
    {
        Value = ProfileValue.RequireValue(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}

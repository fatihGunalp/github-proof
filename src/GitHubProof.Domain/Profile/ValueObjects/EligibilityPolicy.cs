namespace GitHubProof.Domain.Profile.ValueObjects;

// One immutable eligibility policy definition. An EligibilityPolicyVersion
// never travels without the policy values it identifies, so no evaluation can
// read a CurrentActivityWindow that is not owned by a version.
//
// A canonical policy version uniquely identifies one definition: rebinding a
// canonical version to a different window is rejected, so changing the window
// genuinely requires a new EligibilityPolicyVersion.
public sealed record EligibilityPolicy
{
    // The first canonical Domain runtime eligibility policy, established by
    // Session 084 against the locked repository eligibility contract
    // (docs/architecture/public-profile-evidence-repository-eligibility.md
    // §19.3.1: Profile V1 CurrentActivityWindow = exactly 365 days).
    public static readonly EligibilityPolicy V1 = new(
        new EligibilityPolicyVersion("1.0.0"),
        TimeSpan.FromDays(365));

    private static readonly IReadOnlyList<EligibilityPolicy> Canonical = [V1];

    private EligibilityPolicy(
        EligibilityPolicyVersion version,
        TimeSpan currentActivityWindow)
    {
        Version = version;
        CurrentActivityWindow = currentActivityWindow;
    }

    public EligibilityPolicyVersion Version { get; }

    public TimeSpan CurrentActivityWindow { get; }

    public static EligibilityPolicy Create(
        EligibilityPolicyVersion version,
        TimeSpan currentActivityWindow)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (currentActivityWindow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentActivityWindow),
                currentActivityWindow,
                "A usable eligibility policy requires an exact positive current activity window.");
        }

        EligibilityPolicy? canonical = Canonical.SingleOrDefault(policy =>
            policy.Version == version);

        if (canonical is not null
            && canonical.CurrentActivityWindow != currentActivityWindow)
        {
            throw new ArgumentException(
                $"Eligibility policy version '{version.Value}' is canonically bound to "
                + $"{canonical.CurrentActivityWindow}. Changing the current activity window "
                + "requires a new eligibility policy version.",
                nameof(currentActivityWindow));
        }

        return canonical ?? new EligibilityPolicy(version, currentActivityWindow);
    }

    // The deterministic activity cutoff for one profile snapshot. It is
    // anchored to the snapshot capture time; no system clock participates.
    public DateTimeOffset ResolveActivityCutoff(DateTimeOffset snapshotCapturedAt) =>
        snapshotCapturedAt.ToUniversalTime() - CurrentActivityWindow;

    public override string ToString() =>
        $"{Version.Value} (current activity window: {CurrentActivityWindow})";
}

using GitHubProof.Domain.Profile.Enums;

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
        TimeSpan.FromDays(365),
        ForkAdmissionRule.Analyze);

    // The second canonical policy, established by Session 099 against the
    // Session 098 amendment (§6.3, §19.3.2). It withdraws a contribution the
    // previous version allowed — a known fork is now skipped before any
    // repository collection — which is a breaking eligibility change and
    // therefore a major version. The current activity window is deliberately
    // identical: nothing about recency changed.
    public static readonly EligibilityPolicy V2 = new(
        new EligibilityPolicyVersion("2.0.0"),
        TimeSpan.FromDays(365),
        ForkAdmissionRule.Skip);

    private static readonly IReadOnlyList<EligibilityPolicy> Canonical = [V1, V2];

    private EligibilityPolicy(
        EligibilityPolicyVersion version,
        TimeSpan currentActivityWindow,
        ForkAdmissionRule forkAdmission)
    {
        Version = version;
        CurrentActivityWindow = currentActivityWindow;
        ForkAdmission = forkAdmission;
    }

    public EligibilityPolicyVersion Version { get; }

    public TimeSpan CurrentActivityWindow { get; }

    // Belongs immutably to this version, exactly like CurrentActivityWindow.
    public ForkAdmissionRule ForkAdmission { get; }

    public static EligibilityPolicy Create(
        EligibilityPolicyVersion version,
        TimeSpan currentActivityWindow,
        ForkAdmissionRule forkAdmission = ForkAdmissionRule.Analyze)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (!Enum.IsDefined(forkAdmission))
        {
            throw new ArgumentOutOfRangeException(
                nameof(forkAdmission),
                forkAdmission,
                "Fork admission rule is not defined.");
        }

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

        // A canonical version identifies exactly one immutable rule set, so a
        // caller cannot rebind it to a different fork admission rule either.
        if (canonical is not null && canonical.ForkAdmission != forkAdmission)
        {
            throw new ArgumentException(
                $"Eligibility policy version '{version.Value}' is canonically bound to "
                + $"fork admission '{canonical.ForkAdmission}'. Changing it requires a new "
                + "eligibility policy version.",
                nameof(forkAdmission));
        }

        return canonical
            ?? new EligibilityPolicy(version, currentActivityWindow, forkAdmission);
    }

    // The deterministic activity cutoff for one profile snapshot. It is
    // anchored to the snapshot capture time; no system clock participates.
    public DateTimeOffset ResolveActivityCutoff(DateTimeOffset snapshotCapturedAt) =>
        snapshotCapturedAt.ToUniversalTime() - CurrentActivityWindow;

    public override string ToString() =>
        $"{Version.Value} (current activity window: {CurrentActivityWindow}, "
        + $"fork admission: {ForkAdmission})";
}

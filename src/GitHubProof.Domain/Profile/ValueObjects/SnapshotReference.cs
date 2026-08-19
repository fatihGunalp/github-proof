namespace GitHubProof.Domain.Profile.ValueObjects;

// Stable provenance link from a profile result to the exact normalized profile
// snapshot composition consumed. It exists so provenance never requires
// copying the snapshot itself.
//
// It answers three separate questions, and the last two are never merged:
// whose data this was, when it was observed, and which normalized semantic
// snapshot was observed.
public sealed record SnapshotReference
{
    public SnapshotReference(
        GitHubProfileIdentity profileIdentity,
        DateTimeOffset capturedAt,
        SnapshotIdentity snapshotIdentity)
    {
        ProfileIdentity = profileIdentity
            ?? throw new ArgumentNullException(nameof(profileIdentity));
        CapturedAt = capturedAt.ToUniversalTime();
        SnapshotIdentity = snapshotIdentity
            ?? throw new ArgumentNullException(nameof(snapshotIdentity));
    }

    public GitHubProfileIdentity ProfileIdentity { get; }

    // Observation time. It is deliberately outside SnapshotIdentity, and it
    // remains available to consumers that need it — including the eligibility
    // activity rules, which anchor on it instead of a system clock.
    public DateTimeOffset CapturedAt { get; }

    public SnapshotIdentity SnapshotIdentity { get; }
}

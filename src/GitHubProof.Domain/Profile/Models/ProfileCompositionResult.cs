using GitHubProof.Domain.Profile.Enums;

namespace GitHubProof.Domain.Profile.Models;

// Outcome boundary of one profile composition attempt.
//
// Failed lives here rather than on the profile, so there is no partially valid
// canonical root: a profile that exists is truth a consumer may safely use.
//
// Status is derived from what composition actually produced; it is never set
// independently of the result it describes.
public sealed class ProfileCompositionResult
{
    private ProfileCompositionResult(DeveloperPublicEvidenceProfile? profile)
    {
        Profile = profile;

        Status = profile is null
            ? ProfileCompositionStatus.Failed
            : profile.AnalysisStatus switch
            {
                ProfileAnalysisStatus.Complete => ProfileCompositionStatus.Complete,
                _ => ProfileCompositionStatus.Partial
            };
    }

    public ProfileCompositionStatus Status { get; }

    // Present exactly when composition produced canonical truth.
    public DeveloperPublicEvidenceProfile? Profile { get; }

    public bool HasProfile => Profile is not null;

    public static ProfileCompositionResult Composed(
        DeveloperPublicEvidenceProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new ProfileCompositionResult(profile);
    }

    // Coherent canonical profile truth could not be composed, so no root is
    // produced. This is not an evidence-quantity statement.
    public static ProfileCompositionResult Failed() => new(profile: null);
}

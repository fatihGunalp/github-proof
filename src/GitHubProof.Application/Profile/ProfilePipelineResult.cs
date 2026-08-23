using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Application.Profile;

// The outcome boundary of one ProfilePipeline run. A composed profile always
// carries its projected report; a failed composition carries neither. Partial
// is not distinguished from Complete here — both are a composed profile with
// its report, exactly as ProfileCompositionResult already represents them.
//
// Pairing a composition with a report is a provenance claim ("this report was
// projected from this exact profile"), not a value-shape check, so it is not
// safe to leave to an arbitrary public (composition, report) pair supplied by
// an external caller. Composed construction is therefore internal — owned
// exclusively by ProfilePipeline, the one place that actually produced both
// from the same run. The only public construction path, Failed, carries no
// report parameter at all, so it cannot be misused to attach one.
public sealed class ProfilePipelineResult
{
    private ProfilePipelineResult(
        ProfileCompositionResult composition,
        ProfileReport? report,
        Uri? avatarUrl,
        RepositoryPresentationMetadata repositoryPresentationMetadata)
    {
        Composition = composition;
        Report = report;
        AvatarUrl = avatarUrl;
        RepositoryPresentationMetadata = repositoryPresentationMetadata;
    }

    public ProfileCompositionResult Composition { get; }

    public ProfileReport? Report { get; }

    // Provider-returned canonical GitHub avatar for the analyzed profile,
    // sourced directly from the same run's GitHubProfileSnapshot.Metadata. It
    // is Profile identity/presentation metadata only — never evidence, never
    // part of ProfileReport, and always null for a Failed composition.
    public Uri? AvatarUrl { get; }

    // Session 111 STEP 3: provider-returned repository Description(s) for the
    // same run, sourced directly from GitHubProfileSnapshot.
    // RepositoryPresentationMetadata and looked up by canonical
    // ProfileRepositoryReference. Non-evidence repository presentation
    // metadata only — never part of ProfileReport, RepresentativeRepository
    // or representative selection. Always Empty for a Failed composition.
    public RepositoryPresentationMetadata RepositoryPresentationMetadata { get; }

    public ProfileCompositionStatus Status => Composition.Status;

    public bool HasProfile => Composition.HasProfile;

    public DeveloperPublicEvidenceProfile? Profile => Composition.Profile;

    // The only public construction path. It accepts exclusively a Failed
    // composition and always produces Report = null — it cannot be used to
    // attach a report to anything.
    public static ProfilePipelineResult Failed(ProfileCompositionResult composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        if (composition.HasProfile)
        {
            throw new ArgumentException(
                "Failed requires a Failed composition result.",
                nameof(composition));
        }

        return new ProfilePipelineResult(
            composition,
            report: null,
            avatarUrl: null,
            RepositoryPresentationMetadata.Empty);
    }

    // Internal to the Application assembly: only ProfilePipeline, which
    // produced this exact composition and this exact report from the same
    // run, may pair them.
    internal static ProfilePipelineResult Composed(
        ProfileCompositionResult composition,
        ProfileReport report,
        Uri? avatarUrl,
        RepositoryPresentationMetadata repositoryPresentationMetadata)
    {
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(repositoryPresentationMetadata);

        if (!composition.HasProfile)
        {
            throw new ArgumentException(
                "Composed requires a composed (non-Failed) composition result.",
                nameof(composition));
        }

        return new ProfilePipelineResult(
            composition,
            report,
            avatarUrl,
            repositoryPresentationMetadata);
    }
}

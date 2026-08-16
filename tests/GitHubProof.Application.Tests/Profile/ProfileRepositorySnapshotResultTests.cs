using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

public sealed class ProfileRepositorySnapshotResultTests
{
    [Fact]
    public void Constructor_WhenSnapshotHasUnavailableMetadata_Rejects()
    {
        Assert.Throws<ArgumentException>(() => new ProfileRepositorySnapshotResult(
            CreateRepositorySnapshot("alpha"),
            DataAvailability.Unavailable(
                MetadataDataKey,
                DataAvailabilityStatus.Forbidden,
                "GitHub denied access to this repository.")));
    }

    [Fact]
    public void Constructor_WhenAvailableMetadataHasNoSnapshot_Rejects()
    {
        Assert.Throws<ArgumentException>(() => new ProfileRepositorySnapshotResult(
            snapshot: null,
            DataAvailability.Available(MetadataDataKey, CapturedAt)));
    }

    [Fact]
    public void Constructor_WhenAvailabilityIsNull_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(() => new ProfileRepositorySnapshotResult(
            CreateRepositorySnapshot("alpha"),
            availability: null!));
    }
}

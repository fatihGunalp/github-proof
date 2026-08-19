using System.Reflection;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ProfileCompositionResultTests
{
    [Fact]
    public void Failed_ProducesNoProfileRoot()
    {
        ProfileCompositionResult result = ProfileCompositionResult.Failed();

        Assert.Equal(ProfileCompositionStatus.Failed, result.Status);
        Assert.Null(result.Profile);
        Assert.False(result.HasProfile);
    }

    [Fact]
    public void Composed_RequiresAProfile() =>
        Assert.Throws<ArgumentNullException>(() =>
            ProfileCompositionResult.Composed(profile: null!));

    // Status is derived from what composition produced, never set beside it,
    // so the wrapper can never disagree with the root it carries.
    [Fact]
    public void Status_IsDerivedFromTheProducedResult()
    {
        Assert.Equal(
            [ProfileCompositionStatus.Complete, ProfileCompositionStatus.Partial, ProfileCompositionStatus.Failed],
            Enum.GetValues<ProfileCompositionStatus>());

        Assert.DoesNotContain(
            typeof(ProfileCompositionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => property.CanWrite);
    }

    [Fact]
    public void FailedFactory_IsTheOnlyWayToReachFailed()
    {
        Assert.Empty(typeof(ProfileCompositionResult).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
    }
}

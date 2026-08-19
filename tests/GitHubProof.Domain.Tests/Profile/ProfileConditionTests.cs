using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ProfileConditionTests
{
    // ------------------------------------------------------------- taxonomy

    [Fact]
    public void LimitationTaxonomy_IsExactlyTheLockedV1Set()
    {
        Assert.Equal(
            [
                ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                ProfileLimitationCode.PartialEvidenceSurface
            ],
            Enum.GetValues<ProfileLimitationCode>());
    }

    [Fact]
    public void WarningTaxonomy_IsExactlyTheLockedV1Set()
    {
        Assert.Equal(
            [
                ProfileWarningCode.RepositoryEvidenceUnavailable,
                ProfileWarningCode.RepositoryEvidenceRateLimited,
                ProfileWarningCode.RepositoryAnalysisFailed,
                ProfileWarningCode.RepositoryEligibilityUndetermined
            ],
            Enum.GetValues<ProfileWarningCode>());
    }

    [Fact]
    public void ScopeValues_AreExactlyProfileAndRepository()
    {
        Assert.Equal(
            [ProfileConditionScope.Profile, ProfileConditionScope.Repository],
            Enum.GetValues<ProfileConditionScope>());
    }

    [Fact]
    public void UndefinedCodeOrScope_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProfileLimitation(
            (ProfileLimitationCode)99,
            ProfileConditionScope.Profile));

        Assert.Throws<ArgumentOutOfRangeException>(() => new ProfileWarning(
            ProfileWarningCode.RepositoryAnalysisFailed,
            (ProfileConditionScope)99,
            [CreateReference("alpha")]));
    }

    // ------------------------------------------------------------- identity

    [Fact]
    public void ProfileScope_CannotNameAffectedRepositories()
    {
        Assert.Throws<ArgumentException>(() => new ProfileLimitation(
            ProfileLimitationCode.PartialEvidenceSurface,
            ProfileConditionScope.Profile,
            [CreateReference("alpha")]));
    }

    [Fact]
    public void RepositoryScope_MustNameAffectedRepositories()
    {
        Assert.Throws<ArgumentException>(() => new ProfileWarning(
            ProfileWarningCode.RepositoryAnalysisFailed,
            ProfileConditionScope.Repository));
    }

    // Profile scoped conditions carry no affected list, and that absence is
    // not a missing value.
    [Fact]
    public void ProfileScope_HasAnEmptyAffectedList()
    {
        ProfileLimitation limitation = new(
            ProfileLimitationCode.LimitedIndependentEvidenceSurface,
            ProfileConditionScope.Profile);

        Assert.Empty(limitation.AffectedRepositories);
    }

    [Fact]
    public void Identity_IsCodePlusScopePlusAffectedIdentities()
    {
        ProfileWarning first = Warning(
            ProfileWarningCode.RepositoryAnalysisFailed,
            "alpha");

        Assert.Equal(
            first,
            Warning(ProfileWarningCode.RepositoryAnalysisFailed, "alpha"));

        Assert.NotEqual(
            first,
            Warning(ProfileWarningCode.RepositoryEvidenceUnavailable, "alpha"));

        Assert.NotEqual(
            first,
            Warning(ProfileWarningCode.RepositoryAnalysisFailed, "beta"));
    }

    // A limitation and a warning are different concepts even when their
    // numeric codes coincide.
    [Fact]
    public void LimitationAndWarning_AreNeverEqual()
    {
        Assert.NotEqual<object>(
            new ProfileLimitation(
                ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                ProfileConditionScope.Profile),
            new ProfileWarning(
                ProfileWarningCode.RepositoryEvidenceUnavailable,
                ProfileConditionScope.Repository,
                [CreateReference("alpha")]));
    }

    [Fact]
    public void AffectedRepositories_AreNormalizedAndDeduplicated()
    {
        ProfileWarning warning = new(
            ProfileWarningCode.RepositoryEvidenceRateLimited,
            ProfileConditionScope.Repository,
            [CreateReference("beta"), CreateReference("alpha"), CreateReference("beta")]);

        Assert.Equal(
            ["alpha", "beta"],
            warning.AffectedRepositories.Select(repository => repository.Name));
    }

    // Two entries describing the same condition for the same repositories are
    // one condition regardless of the order the repositories arrived in.
    [Fact]
    public void AffectedRepositoryOrder_DoesNotChangeIdentity()
    {
        ProfileWarning forward = new(
            ProfileWarningCode.RepositoryEvidenceUnavailable,
            ProfileConditionScope.Repository,
            [CreateReference("alpha"), CreateReference("beta")]);

        ProfileWarning reversed = new(
            ProfileWarningCode.RepositoryEvidenceUnavailable,
            ProfileConditionScope.Repository,
            [CreateReference("beta"), CreateReference("alpha")]);

        Assert.Equal(forward, reversed);
        Assert.Equal(forward.GetHashCode(), reversed.GetHashCode());
    }

    // No localized or presentation string is part of the type at all, so it
    // cannot participate in semantic identity.
    [Fact]
    public void NoLocalizedTextParticipatesInIdentity()
    {
        Assert.All(
            new[] { typeof(ProfileLimitation), typeof(ProfileWarning) },
            type => Assert.DoesNotContain(
                type.GetProperties(),
                property => property.PropertyType == typeof(string)));
    }

    private static ProfileWarning Warning(ProfileWarningCode code, string repository) =>
        new(code, ProfileConditionScope.Repository, [CreateReference(repository)]);
}

using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepresentativeRepositoryRuleVersionTests
{
    [Fact]
    public void V1_IsTheFirstCanonicalDomainRuntimeRuleVersion()
    {
        Assert.Equal("1.0.0", RepresentativeRepositoryRuleVersion.V1.Value);
    }

    [Fact]
    public void V1_IsUsableAsAnOrdinaryConstructedValue()
    {
        Assert.Equal(
            RepresentativeRepositoryRuleVersion.V1,
            new RepresentativeRepositoryRuleVersion("1.0.0"));
    }

    [Fact]
    public void Value_IsTrimmed()
    {
        Assert.Equal(
            "1.0.0",
            new RepresentativeRepositoryRuleVersion("  1.0.0  ").Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingValue_IsRejected(string? value)
    {
        Assert.Throws<ArgumentException>(
            () => new RepresentativeRepositoryRuleVersion(value!));
    }

    [Fact]
    public void EqualValues_AreEqual()
    {
        Assert.Equal(
            new RepresentativeRepositoryRuleVersion("1.0.0"),
            new RepresentativeRepositoryRuleVersion("1.0.0"));
    }

    [Fact]
    public void DifferentValues_AreNotEqual()
    {
        Assert.NotEqual(
            new RepresentativeRepositoryRuleVersion("1.0.0"),
            new RepresentativeRepositoryRuleVersion("2.0.0"));
    }

    [Fact]
    public void ToString_ReturnsTheValue()
    {
        Assert.Equal(
            "1.0.0",
            new RepresentativeRepositoryRuleVersion("1.0.0").ToString());
    }
}

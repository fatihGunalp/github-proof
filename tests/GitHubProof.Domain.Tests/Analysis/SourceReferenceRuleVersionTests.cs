using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis;

public sealed class SourceReferenceRuleVersionTests
{
    [Fact]
    public void V1_IsTheFirstRuntimeDetectionSemantics()
    {
        Assert.Equal("1.0.0", SourceReferenceRuleVersion.V1.Value);
    }

    [Fact]
    public void Value_IsTrimmed()
    {
        Assert.Equal("1.0.0", new SourceReferenceRuleVersion("  1.0.0  ").Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingValue_IsRejected(string? value)
    {
        Assert.Throws<ArgumentException>(() => new SourceReferenceRuleVersion(value!));
    }

    [Fact]
    public void EqualValues_AreEqual()
    {
        Assert.Equal(
            new SourceReferenceRuleVersion("1.0.0"),
            new SourceReferenceRuleVersion("1.0.0"));
    }

    [Fact]
    public void DifferentValues_AreNotEqual()
    {
        Assert.NotEqual(
            new SourceReferenceRuleVersion("1.0.0"),
            new SourceReferenceRuleVersion("2.0.0"));
    }

    [Fact]
    public void ToString_ReturnsTheValue()
    {
        Assert.Equal("1.0.0", new SourceReferenceRuleVersion("1.0.0").ToString());
    }
}

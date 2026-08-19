using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class EligibilityPolicyTests
{
    [Fact]
    public void V1_BindsExactlyThreeHundredSixtyFiveDaysToItsVersion()
    {
        Assert.Equal(TimeSpan.FromDays(365), EligibilityPolicy.V1.CurrentActivityWindow);
        Assert.Equal("1.0.0", EligibilityPolicy.V1.Version.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_RejectsNonPositiveWindow(int days)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EligibilityPolicy.Create(
            new EligibilityPolicyVersion("policy"),
            TimeSpan.FromDays(days)));
    }

    [Fact]
    public void Create_RequiresVersion()
    {
        Assert.Throws<ArgumentNullException>(() => EligibilityPolicy.Create(
            version: null!,
            TimeSpan.FromDays(365)));
    }

    // A canonical version uniquely identifies one definition: it cannot be
    // rebound to a different window.
    [Fact]
    public void Create_RejectsRebindingACanonicalVersionToAnotherWindow()
    {
        Assert.Throws<ArgumentException>(() => EligibilityPolicy.Create(
            new EligibilityPolicyVersion("1.0.0"),
            TimeSpan.FromDays(180)));
    }

    [Fact]
    public void Create_ReturnsTheCanonicalPolicyForItsOwnWindow()
    {
        Assert.Same(
            EligibilityPolicy.V1,
            EligibilityPolicy.Create(
                new EligibilityPolicyVersion("1.0.0"),
                TimeSpan.FromDays(365)));
    }

    [Fact]
    public void ToString_RevealsTheWindowOwnedByTheVersion()
    {
        Assert.Contains("1.0.0", EligibilityPolicy.V1.ToString(), StringComparison.Ordinal);
        Assert.Contains(
            TimeSpan.FromDays(365).ToString(),
            EligibilityPolicy.V1.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ResolveActivityCutoff_IsAnchoredToSnapshotCaptureTime()
    {
        DateTimeOffset capturedAt = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal(
            capturedAt.AddDays(-365),
            EligibilityPolicy.V1.ResolveActivityCutoff(capturedAt));
    }

    [Fact]
    public void ResolveActivityCutoff_IsIndependentOfInputOffset()
    {
        DateTimeOffset utc = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset shifted = utc.ToOffset(TimeSpan.FromHours(3));

        Assert.Equal(
            EligibilityPolicy.V1.ResolveActivityCutoff(utc),
            EligibilityPolicy.V1.ResolveActivityCutoff(shifted));
    }

    // A non-canonical version may still be defined once, and its window
    // remains bound to it rather than living as a free constant.
    [Fact]
    public void Create_BindsTheWindowToANonCanonicalVersion()
    {
        EligibilityPolicy policy = EligibilityPolicy.Create(
            new EligibilityPolicyVersion("experimental"),
            TimeSpan.FromDays(90));

        Assert.Equal(TimeSpan.FromDays(90), policy.CurrentActivityWindow);
        Assert.Equal("experimental", policy.Version.Value);
    }
}

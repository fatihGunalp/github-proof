using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Web.Presentation;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class StatusBadgePresentationTests
{
    public static TheoryData<SignalStatus, string, string> CanonicalStatuses =>
        new()
        {
            {
                SignalStatus.Strong,
                "status-badge--strong",
                "✓"
            },
            {
                SignalStatus.Present,
                "status-badge--present",
                "•"
            },
            {
                SignalStatus.Partial,
                "status-badge--partial",
                "◐"
            },
            {
                SignalStatus.Missing,
                "status-badge--missing",
                "×"
            },
            {
                SignalStatus.NotApplicable,
                "status-badge--not-applicable",
                "—"
            },
            {
                SignalStatus.UnavailableEvidence,
                "status-badge--unavailable-evidence",
                "?"
            },
            {
                SignalStatus.RequiresHumanReview,
                "status-badge--requires-human-review",
                "!"
            }
        };

    [Theory]
    [MemberData(nameof(CanonicalStatuses))]
    public void Maps_every_canonical_signal_status(
        SignalStatus status,
        string expectedModifier,
        string expectedGlyph)
    {
        StatusBadgePresentation result =
            StatusBadgePresentation.From(status);

        Assert.Equal(expectedModifier, result.CssModifier);
        Assert.Equal(expectedGlyph, result.Glyph);
    }

    [Fact]
    public void Rejects_unknown_signal_status()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StatusBadgePresentation.From((SignalStatus)999));
    }

    [Fact]
    public void Canonical_mapping_covers_the_entire_signal_status_enum()
    {
        SignalStatus[] statuses = Enum.GetValues<SignalStatus>();

        Assert.Equal(7, statuses.Length);

        foreach (SignalStatus status in statuses)
        {
            StatusBadgePresentation result =
                StatusBadgePresentation.From(status);

            Assert.False(string.IsNullOrWhiteSpace(result.CssModifier));
            Assert.False(string.IsNullOrWhiteSpace(result.Glyph));
        }
    }
}
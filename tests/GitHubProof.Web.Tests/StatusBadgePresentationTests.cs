using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Web.Presentation;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class StatusBadgePresentationTests
{
    public static TheoryData<SignalStatus, string, StatusIcon>
        CanonicalStatuses =>
        new()
        {
            {
                SignalStatus.Strong,
                "status-badge--strong",
                StatusIcon.CircleCheck
            },
            {
                SignalStatus.Present,
                "status-badge--present",
                StatusIcon.Check
            },
            {
                SignalStatus.Partial,
                "status-badge--partial",
                StatusIcon.CircleDashed
            },
            {
                SignalStatus.Missing,
                "status-badge--missing",
                StatusIcon.CircleX
            },
            {
                SignalStatus.NotApplicable,
                "status-badge--not-applicable",
                StatusIcon.CircleMinus
            },
            {
                SignalStatus.UnavailableEvidence,
                "status-badge--unavailable-evidence",
                StatusIcon.CircleHelp
            },
            {
                SignalStatus.RequiresHumanReview,
                "status-badge--requires-human-review",
                StatusIcon.Eye
            }
        };

    [Theory]
    [MemberData(nameof(CanonicalStatuses))]
    public void Maps_every_canonical_signal_status(
        SignalStatus status,
        string expectedModifier,
        StatusIcon expectedIcon)
    {
        StatusBadgePresentation result =
            StatusBadgePresentation.From(status);

        Assert.Equal(
            expectedModifier,
            result.CssModifier);

        Assert.Equal(
            expectedIcon,
            result.Icon);
    }

    [Fact]
    public void Rejects_unknown_signal_status()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StatusBadgePresentation.From(
                (SignalStatus)999));
    }

    [Fact]
    public void Canonical_mapping_covers_the_entire_signal_status_enum()
    {
        SignalStatus[] statuses =
            Enum.GetValues<SignalStatus>();

        Assert.Equal(
            7,
            statuses.Length);

        foreach (SignalStatus status in statuses)
        {
            StatusBadgePresentation result =
                StatusBadgePresentation.From(status);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    result.CssModifier));

            Assert.True(
                Enum.IsDefined(result.Icon));
        }
    }
}

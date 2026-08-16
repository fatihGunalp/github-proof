using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Web.Presentation;

public enum StatusIcon
{
    CircleCheck,
    Check,
    CircleDashed,
    CircleX,
    CircleMinus,
    CircleHelp,
    Eye
}

public sealed record StatusBadgePresentation(
    string CssModifier,
    StatusIcon Icon)
{
    public static StatusBadgePresentation From(SignalStatus status)
    {
        return status switch
        {
            SignalStatus.Strong =>
                new(
                    "status-badge--strong",
                    StatusIcon.CircleCheck),

            SignalStatus.Present =>
                new(
                    "status-badge--present",
                    StatusIcon.Check),

            SignalStatus.Partial =>
                new(
                    "status-badge--partial",
                    StatusIcon.CircleDashed),

            SignalStatus.Missing =>
                new(
                    "status-badge--missing",
                    StatusIcon.CircleX),

            SignalStatus.NotApplicable =>
                new(
                    "status-badge--not-applicable",
                    StatusIcon.CircleMinus),

            SignalStatus.UnavailableEvidence =>
                new(
                    "status-badge--unavailable-evidence",
                    StatusIcon.CircleHelp),

            SignalStatus.RequiresHumanReview =>
                new(
                    "status-badge--requires-human-review",
                    StatusIcon.Eye),

            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported signal status.")
        };
    }
}

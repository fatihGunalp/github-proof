using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Web.Presentation;

public sealed record StatusBadgePresentation(
    string CssModifier,
    string Glyph)
{
    public static StatusBadgePresentation From(SignalStatus status)
    {
        return status switch
        {
            SignalStatus.Strong =>
                new("status-badge--strong", "✓"),

            SignalStatus.Present =>
                new("status-badge--present", "•"),

            SignalStatus.Partial =>
                new("status-badge--partial", "◐"),

            SignalStatus.Missing =>
                new("status-badge--missing", "×"),

            SignalStatus.NotApplicable =>
                new("status-badge--not-applicable", "—"),

            SignalStatus.UnavailableEvidence =>
                new("status-badge--unavailable-evidence", "?"),

            SignalStatus.RequiresHumanReview =>
                new("status-badge--requires-human-review", "!"),

            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported signal status.")
        };
    }
}
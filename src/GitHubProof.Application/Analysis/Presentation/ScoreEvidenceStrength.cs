namespace GitHubProof.Application.Analysis.Presentation;

// Session 141 (contract 12 §3, 15 §8/§9/§10): the typed evidence-strength
// identity behind the score label. Presentation logic that needs to know how
// strong the visible evidence is reads THIS, derived from the canonical
// ScoreBreakdown.LabelKey — never by parsing a localized score label.
//
// It is a presentation identity only: it introduces no threshold, changes no
// scoring formula, and is derived from the canonical LabelKey the Domain
// already produced.
public enum ScoreEvidenceStrength
{
    Limited = 1,
    Basic = 2,
    Developing = 3,
    Strong = 4,
    VeryStrong = 5
}

public static class RepositoryReportScoreSemantics
{
    // A null LabelKey means the analysis produced no normalized score at all
    // (ApplicableMaximumScore == 0). No score is NOT a zero score and NOT a
    // weakest-evidence claim, so it maps to null, never to Limited.
    public static ScoreEvidenceStrength? FromLabelKey(string? labelKey) =>
        labelKey switch
        {
            null => null,
            RepositoryReportScoreLabelKeys.LimitedEvidence =>
                ScoreEvidenceStrength.Limited,
            RepositoryReportScoreLabelKeys.BasicEvidence =>
                ScoreEvidenceStrength.Basic,
            RepositoryReportScoreLabelKeys.DevelopingEvidence =>
                ScoreEvidenceStrength.Developing,
            RepositoryReportScoreLabelKeys.StrongEvidence =>
                ScoreEvidenceStrength.Strong,
            RepositoryReportScoreLabelKeys.VeryStrongEvidence =>
                ScoreEvidenceStrength.VeryStrong,
            _ => throw new ArgumentException(
                $"Unknown score label key '{labelKey}'.",
                nameof(labelKey))
        };

    // The exact predicate the Identity card used to obtain by searching the
    // localized label for "Güçlü" / "Strong": both the strong and the very
    // strong band matched that substring, and nothing else did.
    public static bool IsStrongEvidence(ScoreEvidenceStrength? strength) =>
        strength is ScoreEvidenceStrength.Strong
            or ScoreEvidenceStrength.VeryStrong;

    public static bool IsStrongEvidenceLabelKey(string? labelKey) =>
        IsStrongEvidence(FromLabelKey(labelKey));
}

using GitHubProof.Application.Analysis.Presentation;

namespace GitHubProof.Web.Presentation;

public sealed record RepositoryReportNotesModel(
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Limitations);

// Session 141 (contract 14 §12/§16/§20). Two jobs, both keyed on canonical
// identity and never on localized text:
//
//   1. Repeat collapsing. One warning presentation identity can be raised once
//      per affected signal, so several canonical warnings carry the same
//      sentence. Rendering it once is the behavior this surface already had —
//      it just obtained it by comparing localized strings.
//
//   2. Warning/limitation overlap. Where a warning and a limitation state the
//      SAME canonical fact, the duplicate user-facing message is suppressed.
//      Only the one pair the contract identifies as equivalent participates.
//
// Neither collection in the canonical report is altered, reordered or emptied;
// this decides what is rendered, nothing else.
public static class RepositoryReportNotesPresentation
{
    // Contract 14 §12: the NotApplicable warning and the NotApplicable
    // limitation communicate the same canonical semantic fact, and the warning
    // copy already carries the scoring consequence, so the limitation is the
    // redundant one.
    //
    // Deliberately NOT listed, because the contract keeps them semantically
    // distinct rather than duplicated: unavailable-evidence (§5 vs §10 — the
    // warning states the data was unavailable, the limitation states how that
    // affected the analysis) and requires-human-review (§6 vs §11).
    private static readonly IReadOnlyDictionary<string, string>
        LimitationSupersededByWarning =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [RepositoryReportLimitationKeys.AnalysisNotApplicableSignals] =
                    RepositoryReportWarningKeys.SignalNotApplicable
            };

    public static RepositoryReportNotesModel Create(RepositoryReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        HashSet<string> renderedWarningKeys = new(StringComparer.Ordinal);
        List<string> warnings = [];

        foreach (RepositoryReportWarning warning in report.Warnings)
        {
            if (renderedWarningKeys.Add(warning.PresentationKey))
            {
                warnings.Add(warning.Message);
            }
        }

        HashSet<string> seenLimitationKeys = new(StringComparer.Ordinal);
        List<string> limitations = [];

        foreach (RepositoryReportLimitation limitation in report.Limitations)
        {
            if (!seenLimitationKeys.Add(limitation.Key))
            {
                continue;
            }

            if (LimitationSupersededByWarning.TryGetValue(
                    limitation.Key,
                    out string? equivalentWarningKey)
                && renderedWarningKeys.Contains(equivalentWarningKey))
            {
                continue;
            }

            limitations.Add(limitation.Message);
        }

        return new RepositoryReportNotesModel(
            warnings.AsReadOnly(),
            limitations.AsReadOnly());
    }
}

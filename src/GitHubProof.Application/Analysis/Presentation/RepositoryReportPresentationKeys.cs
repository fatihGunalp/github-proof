namespace GitHubProof.Application.Analysis.Presentation;

// Session 141: the canonical presentation-resource identities, named rather
// than repeated as literals. They exist so presentation logic can key on a
// stable identity instead of on localized text (contract 12 §3, 14 §20,
// 15 §9). They are resource keys only — they carry no analysis semantics and
// change no Domain or Application truth.
public static class RepositoryReportScoreLabelKeys
{
    public const string LimitedEvidence = "score.label.limited-visible-evidence";
    public const string BasicEvidence = "score.label.basic-visible-evidence";
    public const string DevelopingEvidence = "score.label.developing-visible-evidence";
    public const string StrongEvidence = "score.label.strong-visible-evidence";
    public const string VeryStrongEvidence = "score.label.very-strong-visible-evidence";

    public static IReadOnlyList<string> All { get; } =
        Array.AsReadOnly(
        [
            LimitedEvidence,
            BasicEvidence,
            DevelopingEvidence,
            StrongEvidence,
            VeryStrongEvidence
        ]);
}

public static class RepositoryReportWarningKeys
{
    public const string SnapshotCollectionPartial = "warning.snapshot.collection-partial";
    public const string SignalNotApplicable = "warning.signal.not-applicable";
    public const string SignalUnavailableEvidence = "warning.signal.unavailable-evidence";
    public const string SignalRequiresHumanReview = "warning.signal.requires-human-review";
    public const string RepositoryArchived = "warning.repository.archived";
    public const string RepositoryFork = "warning.repository.fork";
    public const string RepositoryTemplate = "warning.repository.template";

    public static IReadOnlyList<string> All { get; } =
        Array.AsReadOnly(
        [
            SnapshotCollectionPartial,
            SignalNotApplicable,
            SignalUnavailableEvidence,
            SignalRequiresHumanReview,
            RepositoryArchived,
            RepositoryFork,
            RepositoryTemplate
        ]);
}

public static class RepositoryReportLimitationKeys
{
    public const string SnapshotCollectionPartial = "limitation.snapshot.collection-partial";
    public const string AnalysisUnavailableEvidence = "limitation.analysis.unavailable-evidence";
    public const string AnalysisRequiresHumanReview = "limitation.analysis.requires-human-review";
    public const string AnalysisNotApplicableSignals = "limitation.analysis.not-applicable-signals";
    public const string RepositoryArchived = "limitation.repository.archived";
    public const string RepositoryFork = "limitation.repository.fork";
    public const string RepositoryTemplate = "limitation.repository.template";

    public static IReadOnlyList<string> All { get; } =
        Array.AsReadOnly(
        [
            SnapshotCollectionPartial,
            AnalysisUnavailableEvidence,
            AnalysisRequiresHumanReview,
            AnalysisNotApplicableSignals,
            RepositoryArchived,
            RepositoryFork,
            RepositoryTemplate
        ]);
}

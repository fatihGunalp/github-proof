namespace GitHubProof.Web.Progress;

// The fixed stage identities and their locked order, exactly as
// live-analysis-progress-runtime-contract.md §4 (Repository, four stages) and
// §5 (Profile, three stages) define them. These identifiers are the wire values
// the observation endpoint transports in `active` and `completed`; the
// user-facing label for each is owned by
// p10-presentation-language-contract.md §8.3.2 and never appears here.
//
// No stage identity beyond these seven exists, and none may be added without a
// contract amendment (§17).
public static class AnalysisProgressStages
{
    public const string RepositoryTargetValidated = "repository-target-validated";
    public const string RepositoryPublicDataRead = "repository-public-data-read";
    public const string RepositoryTracesAnalyzed = "repository-traces-analyzed";
    public const string RepositoryProofPrepared = "repository-proof-prepared";

    public const string ProfileTargetValidated = "profile-target-validated";
    public const string ProfilePublicSurfaceRead = "profile-public-surface-read";
    public const string ProfileEvidenceExamined = "profile-evidence-examined";

    private static readonly string[] RepositoryOrder =
    [
        RepositoryTargetValidated,
        RepositoryPublicDataRead,
        RepositoryTracesAnalyzed,
        RepositoryProofPrepared
    ];

    private static readonly string[] ProfileOrder =
    [
        ProfileTargetValidated,
        ProfilePublicSurfaceRead,
        ProfileEvidenceExamined
    ];

    public static IReadOnlyList<string> For(AnalysisProgressTargetKind kind) =>
        kind == AnalysisProgressTargetKind.Repository
            ? RepositoryOrder
            : ProfileOrder;
}

namespace GitHubProof.Web.Localization;

public enum WebTextKey
{
    ThemeSwitchToLight,
    ThemeSwitchToDark,
    LanguageSelectorLabel,
    LandingTitle,
    LandingSummary,
    BoundariesSummary,
    BoundariesDisclosureLabel,
    NoDeveloperRankingTitle,
    NoSeniorityTitle,
    NoAiAuthorshipTitle,
    NoHireVerdictTitle,
    InputLabel,
    InputPlaceholder,
    Analyze,
    ProcessingRequest,
    InvalidTarget,
    RepositoryPublicOnly,
    RepositoryRateLimited,
    RepositoryForbidden,
    RepositoryFailed,
    RepositoryUnavailable,
    ProfileCouldNotCompose,
    ProfilePublicOnly,
    ProfileRateLimited,
    ProfileForbidden,
    ProfileFailed,
    ProfileUnavailable,
    ProfilePartialTitle,
    ProfilePartialMessage,
    ProfileObservationNotesTitle,
    ProfileRequestLimitedTitle,
    ProfileRequestLimited,
    ProfileAnalysisInProgressTitle,
    ProfileAnalysisInProgress,
    ProfileServiceBusyTitle,
    ProfileServiceBusy,
    ProfileProviderInterruptedTitle,
    ProfileProviderInterrupted,
    ProfileDeadlineTitle,
    ProfileDeadline,
    ProfileUnavailableTitle,
    ProfileUnsupportedTitle,
    ProfileCompositionFailedTitle,
    AnalyzeSomethingElse,
    ReportPreviewTitle,

    // Session 141 (contract 16 §6): quiet labels that name each signal-detail
    // paragraph's job. They add no semantic meaning to the report — they only
    // say which question the paragraph beneath them answers.
    SignalFieldWhatWeCheck,
    SignalFieldWhatWeFound,
    SignalFieldWhyItMatters,
    SignalFieldWhatYouCanDo,
    SignalFieldLimit,
    SignalFieldEvidence,

    // Live analysis progress. Session 141 (contract 02) replaced the single
    // workflow-noun label plus a separate state token with one sentence per
    // canonical state, so the copy says what is happening rather than naming a
    // state-machine node. The three runtime states themselves are unchanged:
    // PENDING -> ACTIVE -> COMPLETED, still server-reported, still with no
    // percentage, ETA, count or failure state.
    ProgressRepositoryTargetPending,
    ProgressRepositoryTargetActive,
    ProgressRepositoryTargetCompleted,
    ProgressRepositoryDataPending,
    ProgressRepositoryDataActive,
    ProgressRepositoryDataCompleted,
    ProgressRepositoryTracesPending,
    ProgressRepositoryTracesActive,
    ProgressRepositoryTracesCompleted,
    ProgressRepositoryProofPending,
    ProgressRepositoryProofActive,
    ProgressRepositoryProofCompleted,
    ProgressProfileTargetPending,
    ProgressProfileTargetActive,
    ProgressProfileTargetCompleted,
    ProgressProfileRepositoriesPending,
    ProgressProfileRepositoriesActive,
    ProgressProfileRepositoriesCompleted,
    ProgressProfileTracesPending,
    ProgressProfileTracesActive,
    ProgressProfileTracesCompleted
}

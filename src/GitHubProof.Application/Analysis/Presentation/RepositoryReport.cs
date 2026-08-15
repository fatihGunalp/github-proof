using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Application.Analysis.Presentation;

public static class RepositoryReportSectionKeys
{
    public const string Identity = "report.section.identity";
    public const string Proves = "report.section.proves";
    public const string Weakens = "report.section.weakens";
    public const string DoFirst = "report.section.do-first";
    public const string Uncertainty = "report.section.uncertainty";
    public const string CategoriesAndScore = "report.section.categories-score";
    public const string SignalDetails = "report.section.signal-details";
    public const string Disclaimer = "report.section.disclaimer";

    public static IReadOnlyList<string> CanonicalOrder { get; } =
        Array.AsReadOnly(
        [
            Identity,
            Proves,
            Weakens,
            DoFirst,
            Uncertainty,
            CategoriesAndScore,
            SignalDetails,
            Disclaimer
        ]);
}

public sealed record RepositoryReport(
    string Locale,
    IReadOnlyList<string> SectionOrder,
    RepositoryReportIdentity Identity,
    RepositoryReportSection<RepositoryReportHighlight> Proves,
    RepositoryReportSection<RepositoryReportHighlight> Weakens,
    RepositoryReportSection<RepositoryReportAction> DoFirst,
    RepositoryReportSection<RepositoryReportUncertainty> Uncertainty,
    RepositoryReportScoreSummary ScoreSummary,
    RepositoryReportSection<SignalPresentation> SignalDetails,
    IReadOnlyList<RepositoryReportWarning> Warnings,
    IReadOnlyList<string> Limitations,
    RepositoryReportDisclaimer Disclaimer);

public sealed record RepositoryReportSection<T>(
    string Key,
    string Title,
    IReadOnlyList<T> Items);

public sealed record RepositoryReportIdentity(
    string SectionKey,
    string SectionTitle,
    Guid AnalysisId,
    string Owner,
    string Name,
    string FullName,
    Uri Url,
    string DefaultBranch,
    string? HeadCommitSha,
    DateTimeOffset CapturedAt,
    bool IsPublic,
    bool IsArchived,
    bool IsFork,
    bool IsTemplate,
    string? PrimaryLanguage,
    AnalysisStatus AnalysisStatus,
    string AnalysisStatusLabel,
    string AnalyzerVersion,
    string RuleSetVersion,
    int ReportSchemaVersion);

public sealed record RepositoryReportHighlight(
    string SignalKey,
    string Title,
    string Explanation,
    string WhyItMatters,
    string? Recommendation,
    SignalStatus Status,
    string StatusLabel,
    int? Score,
    int MaximumScore,
    IReadOnlyList<EvidenceItem> Evidence);

public sealed record RepositoryReportAction(
    int Priority,
    string PresentationKey,
    string Title,
    string WhyItMatters,
    string Recommendation,
    EffortLevel Effort,
    string EffortLabel,
    IReadOnlyList<string> RelatedSignals,
    IReadOnlyList<EvidenceItem> Evidence);

public sealed record RepositoryReportUncertainty(
    string SignalKey,
    SignalStatus Status,
    string Title,
    string StatusLabel,
    string Explanation,
    IReadOnlyList<EvidenceItem> Evidence);

public sealed record RepositoryReportCategory(
    AnalysisCategory Category,
    string Label,
    CategoryStatus Status,
    string StatusLabel,
    int Score,
    int MaximumScore,
    int? NormalizedScore);

public sealed record RepositoryReportScoreSummary(
    string SectionKey,
    string SectionTitle,
    int EarnedScore,
    int ApplicableMaximumScore,
    int? NormalizedScore,
    string? LabelKey,
    string? Label,
    ScoreCalculationMethod CalculationMethod,
    IReadOnlyList<RepositoryReportCategory> Categories);

public sealed record RepositoryReportWarning(
    string Code,
    WarningSeverity Severity,
    string Message,
    IReadOnlyList<string> RelatedDataKeys,
    IReadOnlyList<string> RelatedSignals);

public sealed record RepositoryReportDisclaimer(
    string SectionKey,
    string SectionTitle,
    int PolicyVersion,
    string Text);
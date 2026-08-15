using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Application.Analysis.Presentation;

public sealed class RepositoryReportProjector
{
    private readonly SignalPresentationLocalizer _signalLocalizer;
    private readonly RepositoryReportLocalizer _reportLocalizer;

    public RepositoryReportProjector()
        : this(
            new SignalPresentationLocalizer(),
            new RepositoryReportLocalizer())
    {
    }

    public RepositoryReportProjector(
        SignalPresentationLocalizer signalLocalizer,
        RepositoryReportLocalizer reportLocalizer)
    {
        _signalLocalizer = signalLocalizer
            ?? throw new ArgumentNullException(nameof(signalLocalizer));
        _reportLocalizer = reportLocalizer
            ?? throw new ArgumentNullException(nameof(reportLocalizer));
    }

    public RepositoryReport Project(
        RepositoryAnalysis analysis,
        string? locale)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        string resolvedLocale =
            SignalPresentationLocalizer.NormalizeLocale(locale);

        IReadOnlyDictionary<string, SignalPresentation> presentations =
            analysis.SignalResults.ToDictionary(
                result => result.SignalKey,
                result => _signalLocalizer.Present(result, resolvedLocale),
                StringComparer.Ordinal);

        RepositoryReportIdentity identity =
            CreateIdentity(analysis, resolvedLocale);

        RepositoryReportSection<RepositoryReportHighlight> proves =
            new(
                RepositoryReportSectionKeys.Proves,
                _reportLocalizer.GetSectionTitle(
                    RepositoryReportSectionKeys.Proves,
                    resolvedLocale),
                analysis.Strengths
                    .Select(highlight =>
                        CreateHighlight(
                            highlight,
                            presentations[highlight.SignalKey]))
                    .ToArray());

        RepositoryReportSection<RepositoryReportHighlight> weakens =
            new(
                RepositoryReportSectionKeys.Weakens,
                _reportLocalizer.GetSectionTitle(
                    RepositoryReportSectionKeys.Weakens,
                    resolvedLocale),
                analysis.CriticalGaps
                    .Select(highlight =>
                        CreateHighlight(
                            highlight,
                            presentations[highlight.SignalKey]))
                    .ToArray());

        RepositoryReportSection<RepositoryReportAction> doFirst =
            new(
                RepositoryReportSectionKeys.DoFirst,
                _reportLocalizer.GetSectionTitle(
                    RepositoryReportSectionKeys.DoFirst,
                    resolvedLocale),
                analysis.ImprovementPlan.Actions
                    .Select(action =>
                        CreateAction(
                            action,
                            presentations,
                            resolvedLocale))
                    .ToArray());

        RepositoryReportSection<RepositoryReportUncertainty> uncertainty =
            new(
                RepositoryReportSectionKeys.Uncertainty,
                _reportLocalizer.GetSectionTitle(
                    RepositoryReportSectionKeys.Uncertainty,
                    resolvedLocale),
                analysis.SignalResults
                    .Where(result => result.Status is
                        SignalStatus.NotApplicable
                        or SignalStatus.UnavailableEvidence
                        or SignalStatus.RequiresHumanReview)
                    .Select(result =>
                        CreateUncertainty(presentations[result.SignalKey]))
                    .ToArray());

        RepositoryReportScoreSummary scoreSummary =
            CreateScoreSummary(
                analysis,
                resolvedLocale);

        RepositoryReportSection<SignalPresentation> signalDetails =
            new(
                RepositoryReportSectionKeys.SignalDetails,
                _reportLocalizer.GetSectionTitle(
                    RepositoryReportSectionKeys.SignalDetails,
                    resolvedLocale),
                analysis.SignalResults
                    .Select(result => presentations[result.SignalKey])
                    .ToArray());

        IReadOnlyList<RepositoryReportWarning> warnings =
            analysis.Warnings
                .Select(warning =>
                    new RepositoryReportWarning(
                        warning.Code,
                        warning.Severity,
                        _reportLocalizer.GetWarning(
                            warning.PresentationKey,
                            resolvedLocale),
                        warning.RelatedDataKeys,
                        warning.RelatedSignals))
                .ToArray();

        IReadOnlyList<string> limitations =
            analysis.LimitationKeys
                .Select(key =>
                    _reportLocalizer.GetLimitation(
                        key,
                        resolvedLocale))
                .ToArray();

        RepositoryReportDisclaimer disclaimer =
            new(
                RepositoryReportSectionKeys.Disclaimer,
                _reportLocalizer.GetSectionTitle(
                    RepositoryReportSectionKeys.Disclaimer,
                    resolvedLocale),
                RepositoryReportLocalizer.ProductBoundaryPolicyVersion,
                _reportLocalizer.GetProductBoundaryDisclaimer(
                    resolvedLocale));

        return new RepositoryReport(
            resolvedLocale,
            RepositoryReportSectionKeys.CanonicalOrder,
            identity,
            proves,
            weakens,
            doFirst,
            uncertainty,
            scoreSummary,
            signalDetails,
            warnings,
            limitations,
            disclaimer);
    }

    private RepositoryReportIdentity CreateIdentity(
        RepositoryAnalysis analysis,
        string locale) =>
        new(
            RepositoryReportSectionKeys.Identity,
            _reportLocalizer.GetSectionTitle(
                RepositoryReportSectionKeys.Identity,
                locale),
            analysis.AnalysisId,
            analysis.Repository.Owner,
            analysis.Repository.Name,
            analysis.Repository.FullName,
            analysis.Repository.Url,
            analysis.Repository.DefaultBranch,
            analysis.Snapshot.HeadCommitSha,
            analysis.Snapshot.CapturedAt,
            analysis.Repository.IsPublic,
            analysis.Repository.IsArchived,
            analysis.Repository.IsFork,
            analysis.Repository.IsTemplate,
            analysis.Repository.PrimaryLanguage,
            analysis.Status,
            _reportLocalizer.GetAnalysisStatusLabel(
                analysis.Status,
                locale),
            analysis.AnalyzerVersion,
            analysis.RuleSetVersion,
            analysis.ReportSchemaVersion);

    private static RepositoryReportHighlight CreateHighlight(
        AnalysisHighlight highlight,
        SignalPresentation presentation) =>
        new(
            highlight.SignalKey,
            presentation.Title,
            presentation.Explanation,
            presentation.WhyItMatters,
            presentation.Recommendation,
            presentation.Status,
            presentation.StatusLabel,
            presentation.Score,
            presentation.MaximumScore,
            presentation.Evidence);

    private RepositoryReportAction CreateAction(
        ImprovementAction action,
        IReadOnlyDictionary<string, SignalPresentation> presentations,
        string locale)
    {
        SignalPresentation primary = presentations[
            action.RelatedSignals[0]];

        string recommendation = primary.Recommendation
            ?? throw new InvalidOperationException(
                $"Improvement action '{action.PresentationKey}' references a signal without recommendation copy.");

        IReadOnlyList<EvidenceItem> evidence =
            ResolveActionEvidence(
                action,
                presentations);

        return new RepositoryReportAction(
            action.Priority,
            action.PresentationKey,
            _reportLocalizer.GetActionTitle(
                action.PresentationKey,
                locale),
            primary.WhyItMatters,
            recommendation,
            action.Effort,
            _reportLocalizer.GetEffortLabel(
                action.Effort,
                locale),
            action.RelatedSignals,
            evidence);
    }

    private static IReadOnlyList<EvidenceItem> ResolveActionEvidence(
        ImprovementAction action,
        IReadOnlyDictionary<string, SignalPresentation> presentations)
    {
        IReadOnlyDictionary<string, EvidenceItem> byKey =
            action.RelatedSignals
                .SelectMany(signal => presentations[signal].Evidence)
                .GroupBy(evidence => evidence.Key, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.Ordinal);

        return action.EvidenceReferences
            .Select(reference =>
                byKey.TryGetValue(reference, out EvidenceItem? evidence)
                    ? evidence
                    : throw new InvalidOperationException(
                        $"Unable to resolve action evidence reference '{reference}'."))
            .ToArray();
    }

    private static RepositoryReportUncertainty CreateUncertainty(
        SignalPresentation presentation) =>
        new(
            presentation.SignalKey,
            presentation.Status,
            presentation.Title,
            presentation.StatusLabel,
            presentation.Explanation,
            presentation.Evidence);

    private RepositoryReportScoreSummary CreateScoreSummary(
        RepositoryAnalysis analysis,
        string locale) =>
        new(
            RepositoryReportSectionKeys.CategoriesAndScore,
            _reportLocalizer.GetSectionTitle(
                RepositoryReportSectionKeys.CategoriesAndScore,
                locale),
            analysis.ScoreBreakdown.EarnedScore,
            analysis.ScoreBreakdown.ApplicableMaximumScore,
            analysis.ScoreBreakdown.NormalizedScore,
            analysis.ScoreBreakdown.LabelKey,
            _reportLocalizer.GetScoreLabel(
                analysis.ScoreBreakdown.LabelKey,
                locale),
            analysis.ScoreBreakdown.CalculationMethod,
            analysis.CategoryResults
                .Select(category =>
                    new RepositoryReportCategory(
                        category.Category,
                        _signalLocalizer.GetCategoryLabel(
                            category.Category,
                            locale),
                        category.Status,
                        _reportLocalizer.GetCategoryStatusLabel(
                            category.Status,
                            locale),
                        category.Score,
                        category.MaximumScore,
                        category.NormalizedScore))
                .ToArray());
}
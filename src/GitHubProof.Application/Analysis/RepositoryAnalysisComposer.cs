using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Application.Analysis;

public sealed class RepositoryAnalysisComposer
{
    public const int MaximumSelectionCount = 3;

    private static readonly IReadOnlyList<SignalContract> CanonicalContracts =
        Array.AsReadOnly<SignalContract>(
        [
            new(ReadmeExistsSignalEvaluator.SignalKey, AnalysisCategory.RepositoryUnderstandability, 5, true, 1),
            new(DescriptionPresentSignalEvaluator.SignalKey, AnalysisCategory.RepositoryUnderstandability, 4, true, 1),
            new(InstallationGuideSignalEvaluator.SignalKey, AnalysisCategory.RepositoryUnderstandability, 8, true, 1),
            new(LicensePresentSignalEvaluator.SignalKey, AnalysisCategory.RepositoryUnderstandability, 3, true, 1),
            new(UsageDemoSignalEvaluator.SignalKey, AnalysisCategory.RepositoryUnderstandability, 7, true, 1),
            new(CommitHistorySignalEvaluator.SignalKey, AnalysisCategory.DevelopmentProcessVisibility, 7, true, 1),
            new(CommitDistributionSignalEvaluator.SignalKey, AnalysisCategory.DevelopmentProcessVisibility, 8, true, 1),
            new(IssueUsageSignalEvaluator.SignalKey, AnalysisCategory.DevelopmentProcessVisibility, 5, true, 1),
            new(PullRequestUsageSignalEvaluator.SignalKey, AnalysisCategory.DevelopmentProcessVisibility, 6, true, 1),
            new(ReleaseUsageSignalEvaluator.SignalKey, AnalysisCategory.DevelopmentProcessVisibility, 6, true, 1),
            new(TestStructureSignalEvaluator.SignalKey, AnalysisCategory.ValidationAndUsability, 10, true, 1),
            new(ContinuousIntegrationSignalEvaluator.SignalKey, AnalysisCategory.ValidationAndUsability, 8, true, 1),
            new(TopicsSignalEvaluator.SignalKey, AnalysisCategory.PortfolioPresentation, 4, true, 1),
            new(HomepageLiveDemoSignalEvaluator.SignalKey, AnalysisCategory.PortfolioPresentation, 5, true, 1),
            new(PresentationCompletenessSignalEvaluator.SignalKey, AnalysisCategory.PortfolioPresentation, 5, false, 1)
        ]);

    private static readonly IReadOnlyDictionary<string, SignalContract> ContractByKey =
        CanonicalContracts.ToDictionary(contract => contract.Key, StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, int> CanonicalIndexByKey =
        CanonicalContracts
            .Select((contract, index) => (contract.Key, Index: index))
            .ToDictionary(item => item.Key, item => item.Index, StringComparer.Ordinal);

    private static readonly IReadOnlyList<AnalysisCategory> CanonicalCategories =
        Array.AsReadOnly(
        [
            AnalysisCategory.RepositoryUnderstandability,
            AnalysisCategory.DevelopmentProcessVisibility,
            AnalysisCategory.ValidationAndUsability,
            AnalysisCategory.PortfolioPresentation
        ]);

    public static IReadOnlyList<string> CanonicalSignalKeys { get; } =
        Array.AsReadOnly(CanonicalContracts.Select(contract => contract.Key).ToArray());

    public RepositoryAnalysis Compose(
        RepositorySnapshot snapshot,
        IEnumerable<SignalResult> signalResults,
        Guid analysisId,
        AnalysisVersion version,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(signalResults);
        ArgumentNullException.ThrowIfNull(version);

        IReadOnlyList<SignalResult> orderedResults = ValidateAndOrder(signalResults);
        IReadOnlyList<CategoryResult> categories = CanonicalCategories
            .Select(category => new CategoryResult(
                category,
                orderedResults.Where(result => result.Category == category)))
            .ToArray();
        ScoreBreakdown scoreBreakdown = new(categories);
        IReadOnlyList<AnalysisHighlight> strengths = SelectStrengths(orderedResults);
        IReadOnlyList<AnalysisHighlight> criticalGaps = SelectCriticalGaps(orderedResults);
        ImprovementPlan improvementPlan = CreateImprovementPlan(
            criticalGaps,
            orderedResults,
            completedAt,
            version.RuleSetVersion);
        IReadOnlyList<AnalysisWarning> warnings = CreateWarnings(snapshot, orderedResults);
        IReadOnlyList<string> limitationKeys = CreateLimitationKeys(snapshot, orderedResults);
        AnalysisStatus status = DetermineStatus(snapshot, orderedResults, scoreBreakdown);

        return new RepositoryAnalysis(
            analysisId,
            snapshot.Repository,
            snapshot,
            status,
            version,
            startedAt,
            completedAt,
            orderedResults,
            categories,
            scoreBreakdown,
            strengths,
            criticalGaps,
            improvementPlan,
            limitationKeys,
            warnings);
    }

    private static IReadOnlyList<SignalResult> ValidateAndOrder(
        IEnumerable<SignalResult> signalResults)
    {
        SignalResult[] results = signalResults.ToArray();

        if (results.Any(result => result is null))
        {
            throw new ArgumentException(
                "Signal results cannot contain null values.",
                nameof(signalResults));
        }

        string[] duplicateKeys = results
            .GroupBy(result => result.SignalKey, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        if (duplicateKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Duplicate signal result keys: {string.Join(", ", duplicateKeys)}.",
                nameof(signalResults));
        }

        string[] unknownKeys = results
            .Select(result => result.SignalKey)
            .Where(key => !ContractByKey.ContainsKey(key))
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        if (unknownKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Unknown signal result keys: {string.Join(", ", unknownKeys)}.",
                nameof(signalResults));
        }

        Dictionary<string, SignalResult> byKey = results.ToDictionary(
            result => result.SignalKey,
            StringComparer.Ordinal);
        string[] missingKeys = CanonicalSignalKeys
            .Where(key => !byKey.ContainsKey(key))
            .ToArray();

        if (missingKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Missing canonical signal result keys: {string.Join(", ", missingKeys)}.",
                nameof(signalResults));
        }

        SignalResult[] ordered = CanonicalContracts
            .Select(contract => ValidateMetadata(byKey[contract.Key], contract))
            .ToArray();

        return Array.AsReadOnly(ordered);
    }

    private static SignalResult ValidateMetadata(
        SignalResult result,
        SignalContract expected)
    {
        if (result.SignalVersion != expected.Version
            || result.Category != expected.Category
            || result.MaximumScore != expected.MaximumScore
            || result.IncludeInOverallScore != expected.IncludeInOverallScore)
        {
            throw new ArgumentException(
                $"Signal result '{result.SignalKey}' does not match its canonical definition metadata.",
                "signalResults");
        }

        return result;
    }

    private static IReadOnlyList<AnalysisHighlight> SelectStrengths(
        IReadOnlyList<SignalResult> results)
    {
        IEnumerable<SignalResult> strong = RankStrengthTier(
            results.Where(result =>
                result.Status == SignalStatus.Strong
                && result.Score > 0
                && result.Evidence.Count > 0));
        IEnumerable<SignalResult> present = RankStrengthTier(
            results.Where(result =>
                result.Status == SignalStatus.Present
                && result.Score > 0
                && result.Evidence.Count > 0));
        IReadOnlyList<SignalResult> selected = SelectWithCategoryDiversity([strong, present]);

        return selected
            .Select(result => CreateHighlight(result, HighlightType.Strength, result.Score!.Value))
            .ToArray();
    }

    private static IReadOnlyList<AnalysisHighlight> SelectCriticalGaps(
        IReadOnlyList<SignalResult> results)
    {
        IEnumerable<SignalResult> missing = results
            .Where(result => result.Status == SignalStatus.Missing && result.Evidence.Count > 0)
            .OrderByDescending(result => result.MaximumScore)
            .ThenBy(GetCanonicalIndex);
        IEnumerable<SignalResult> partial = results
            .Where(result =>
                result.Status == SignalStatus.Partial
                && result.Score < result.MaximumScore
                && result.Evidence.Count > 0)
            .OrderByDescending(result => result.MaximumScore - result.Score!.Value)
            .ThenByDescending(result => result.MaximumScore)
            .ThenBy(GetCanonicalIndex);
        IReadOnlyList<SignalResult> selected = SelectWithCategoryDiversity([missing, partial]);

        return selected
            .Select(result => CreateHighlight(
                result,
                HighlightType.CriticalGap,
                -(result.MaximumScore - result.Score!.Value)))
            .ToArray();
    }

    private static IEnumerable<SignalResult> RankStrengthTier(
        IEnumerable<SignalResult> candidates) =>
        candidates
            .OrderByDescending(result => (decimal)result.Score!.Value / result.MaximumScore)
            .ThenByDescending(result => result.MaximumScore)
            .ThenBy(GetCanonicalIndex);

    private static IReadOnlyList<SignalResult> SelectWithCategoryDiversity(
        IEnumerable<IEnumerable<SignalResult>> tiers)
    {
        List<SignalResult> selected = new(MaximumSelectionCount);
        HashSet<AnalysisCategory> selectedCategories = [];

        foreach (IEnumerable<SignalResult> tier in tiers)
        {
            SignalResult[] orderedTier = tier.ToArray();

            foreach (SignalResult candidate in orderedTier)
            {
                if (selected.Count == MaximumSelectionCount)
                {
                    return selected.AsReadOnly();
                }

                if (selectedCategories.Add(candidate.Category))
                {
                    selected.Add(candidate);
                }
            }

            foreach (SignalResult candidate in orderedTier)
            {
                if (selected.Count == MaximumSelectionCount)
                {
                    return selected.AsReadOnly();
                }

                if (!selected.Contains(candidate))
                {
                    selected.Add(candidate);
                }
            }
        }

        return selected.AsReadOnly();
    }

    private static AnalysisHighlight CreateHighlight(
        SignalResult result,
        HighlightType type,
        int scoreImpact) =>
        new(
            type,
            result.SignalKey,
            scoreImpact,
            result.Evidence.Select(evidence => evidence.Key));

    private static ImprovementPlan CreateImprovementPlan(
        IReadOnlyList<AnalysisHighlight> criticalGaps,
        IReadOnlyList<SignalResult> results,
        DateTimeOffset generatedAt,
        string ruleSetVersion)
    {
        IReadOnlyDictionary<string, SignalResult> byKey = results.ToDictionary(
            result => result.SignalKey,
            StringComparer.Ordinal);
        List<ImprovementAction> actions = [];

        foreach (AnalysisHighlight gap in criticalGaps)
        {
            if (gap.SignalKey == PresentationCompletenessSignalEvaluator.SignalKey)
            {
                continue;
            }

            SignalResult result = byKey[gap.SignalKey];
            string presentationKey = $"action.improve.{result.SignalKey}";

            if (actions.Any(action => string.Equals(
                    action.PresentationKey,
                    presentationKey,
                    StringComparison.Ordinal)))
            {
                continue;
            }

            actions.Add(new ImprovementAction(
                actions.Count + 1,
                presentationKey,
                [result.SignalKey],
                EffortLevel.Unknown,
                gap.EvidenceReferences));

            if (actions.Count == ImprovementPlan.MaximumActionCount)
            {
                break;
            }
        }

        return new ImprovementPlan(actions, generatedAt, ruleSetVersion);
    }

    private static IReadOnlyList<AnalysisWarning> CreateWarnings(
        RepositorySnapshot snapshot,
        IReadOnlyList<SignalResult> results)
    {
        List<AnalysisWarning> warnings = [];

        for (int index = 0; index < snapshot.CollectionWarnings.Count; index++)
        {
            warnings.Add(new AnalysisWarning(
                $"snapshot.collection-warning.{index + 1}",
                WarningSeverity.Warning,
                "warning.snapshot.collection-partial",
                relatedDataKeys: null,
                relatedSignals: null));
        }

        foreach (SignalResult result in results)
        {
            if (result.Status == SignalStatus.NotApplicable)
            {
                warnings.Add(CreateSignalWarning(
                    result,
                    WarningSeverity.Info,
                    "warning.signal.not-applicable"));
            }
            else if (result.Status == SignalStatus.UnavailableEvidence)
            {
                warnings.Add(CreateSignalWarning(
                    result,
                    WarningSeverity.Warning,
                    "warning.signal.unavailable-evidence"));
            }
            else if (result.Status == SignalStatus.RequiresHumanReview)
            {
                warnings.Add(CreateSignalWarning(
                    result,
                    WarningSeverity.Warning,
                    "warning.signal.requires-human-review"));
            }
        }

        if (snapshot.Repository.IsArchived)
        {
            warnings.Add(CreateRepositoryWarning("archived", WarningSeverity.Info));
        }

        if (snapshot.Repository.IsFork)
        {
            warnings.Add(CreateRepositoryWarning("fork", WarningSeverity.Info));
        }

        if (snapshot.Repository.IsTemplate)
        {
            warnings.Add(CreateRepositoryWarning("template", WarningSeverity.Info));
        }

        return warnings.AsReadOnly();
    }

    private static AnalysisWarning CreateSignalWarning(
        SignalResult result,
        WarningSeverity severity,
        string presentationKey) =>
        new(
            $"signal.{GetStatusKey(result.Status)}.{result.SignalKey}",
            severity,
            presentationKey,
            relatedDataKeys: null,
            relatedSignals: [result.SignalKey]);

    private static string GetStatusKey(SignalStatus status) => status switch
    {
        SignalStatus.NotApplicable => "not-applicable",
        SignalStatus.UnavailableEvidence => "unavailable-evidence",
        SignalStatus.RequiresHumanReview => "requires-human-review",
        _ => throw new ArgumentOutOfRangeException(
            nameof(status),
            status,
            "Only explicit uncertainty statuses can produce signal warnings.")
    };

    private static AnalysisWarning CreateRepositoryWarning(
        string state,
        WarningSeverity severity) =>
        new(
            $"repository.{state}",
            severity,
            $"warning.repository.{state}",
            relatedDataKeys: null,
            relatedSignals: null);

    private static IReadOnlyList<string> CreateLimitationKeys(
        RepositorySnapshot snapshot,
        IReadOnlyList<SignalResult> results)
    {
        List<string> keys = [];

        if (snapshot.CollectionWarnings.Count > 0)
        {
            keys.Add("limitation.snapshot.collection-partial");
        }

        if (results.Any(result => result.Status == SignalStatus.UnavailableEvidence))
        {
            keys.Add("limitation.analysis.unavailable-evidence");
        }

        if (results.Any(result => result.Status == SignalStatus.RequiresHumanReview))
        {
            keys.Add("limitation.analysis.requires-human-review");
        }

        if (results.Any(result => result.Status == SignalStatus.NotApplicable))
        {
            keys.Add("limitation.analysis.not-applicable-signals");
        }

        if (snapshot.Repository.IsArchived)
        {
            keys.Add("limitation.repository.archived");
        }

        if (snapshot.Repository.IsFork)
        {
            keys.Add("limitation.repository.fork");
        }

        if (snapshot.Repository.IsTemplate)
        {
            keys.Add("limitation.repository.template");
        }

        return keys.AsReadOnly();
    }

    private static AnalysisStatus DetermineStatus(
        RepositorySnapshot snapshot,
        IReadOnlyList<SignalResult> results,
        ScoreBreakdown scoreBreakdown)
    {
        if (scoreBreakdown.ApplicableMaximumScore == 0)
        {
            return AnalysisStatus.InsufficientData;
        }

        return snapshot.CollectionWarnings.Count > 0
            || results.Any(result => result.Status is
                SignalStatus.UnavailableEvidence or SignalStatus.RequiresHumanReview)
            ? AnalysisStatus.Partial
            : AnalysisStatus.Completed;
    }

    private static int GetCanonicalIndex(SignalResult result) =>
        CanonicalIndexByKey[result.SignalKey];

    private sealed record SignalContract(
        string Key,
        AnalysisCategory Category,
        int MaximumScore,
        bool IncludeInOverallScore,
        int Version);
}

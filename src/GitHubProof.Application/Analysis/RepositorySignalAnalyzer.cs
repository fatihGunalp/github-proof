using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;

namespace GitHubProof.Application.Analysis;

public sealed class RepositorySignalAnalyzer
{
    public static IReadOnlyList<string> CanonicalIndependentSignalKeys { get; } = Array.AsReadOnly(
    [
        ReadmeExistsSignalEvaluator.SignalKey,
        DescriptionPresentSignalEvaluator.SignalKey,
        InstallationGuideSignalEvaluator.SignalKey,
        LicensePresentSignalEvaluator.SignalKey,
        UsageDemoSignalEvaluator.SignalKey,
        CommitHistorySignalEvaluator.SignalKey,
        CommitDistributionSignalEvaluator.SignalKey,
        IssueUsageSignalEvaluator.SignalKey,
        PullRequestUsageSignalEvaluator.SignalKey,
        ReleaseUsageSignalEvaluator.SignalKey,
        TestStructureSignalEvaluator.SignalKey,
        ContinuousIntegrationSignalEvaluator.SignalKey,
        TopicsSignalEvaluator.SignalKey,
        HomepageLiveDemoSignalEvaluator.SignalKey
    ]);

    private readonly IReadOnlyDictionary<string, ISignalEvaluator> _evaluators;
    private readonly PresentationCompletenessSignalEvaluator _presentationCompleteness;

    public RepositorySignalAnalyzer(
        IEnumerable<ISignalEvaluator> independentEvaluators,
        PresentationCompletenessSignalEvaluator presentationCompleteness)
    {
        ArgumentNullException.ThrowIfNull(independentEvaluators);
        _presentationCompleteness = presentationCompleteness
            ?? throw new ArgumentNullException(nameof(presentationCompleteness));

        ISignalEvaluator[] evaluators = independentEvaluators.ToArray();

        if (evaluators.Any(evaluator => evaluator is null))
        {
            throw new ArgumentException(
                "Independent evaluators cannot contain null values.",
                nameof(independentEvaluators));
        }

        string[] duplicateKeys = evaluators
            .GroupBy(evaluator => evaluator.Definition.Key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        if (duplicateKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Duplicate independent evaluator keys: {string.Join(", ", duplicateKeys)}.",
                nameof(independentEvaluators));
        }

        HashSet<string> expected = new(
            CanonicalIndependentSignalKeys,
            StringComparer.Ordinal);
        string[] unknownKeys = evaluators
            .Select(evaluator => evaluator.Definition.Key)
            .Where(key => !expected.Contains(key))
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        if (unknownKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Unknown independent evaluator keys: {string.Join(", ", unknownKeys)}.",
                nameof(independentEvaluators));
        }

        HashSet<string> actual = evaluators
            .Select(evaluator => evaluator.Definition.Key)
            .ToHashSet(StringComparer.Ordinal);
        string[] missingKeys = CanonicalIndependentSignalKeys
            .Where(key => !actual.Contains(key))
            .ToArray();

        if (missingKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Missing independent evaluator keys: {string.Join(", ", missingKeys)}.",
                nameof(independentEvaluators));
        }

        _evaluators = evaluators.ToDictionary(
            evaluator => evaluator.Definition.Key,
            StringComparer.Ordinal);
    }

    public IReadOnlyList<SignalResult> Analyze(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        List<SignalResult> results = new(capacity: CanonicalIndependentSignalKeys.Count + 1);

        foreach (string key in CanonicalIndependentSignalKeys)
        {
            SignalResult result = _evaluators[key].Evaluate(snapshot);

            if (!string.Equals(result.SignalKey, key, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Evaluator '{key}' returned result key '{result.SignalKey}'.");
            }

            results.Add(result);
        }

        results.Add(_presentationCompleteness.Evaluate(results));
        return results.AsReadOnly();
    }
}

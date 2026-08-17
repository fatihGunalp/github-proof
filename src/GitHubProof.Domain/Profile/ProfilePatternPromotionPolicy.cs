using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile;

// P6 V1 pattern promotion. Observations never promote themselves: a pattern
// exists only when the contracted repetition condition holds over independent
// repositories. No weighting, strength or confidence is produced, and
// ProblemArea has no V1 promotion rule.
public sealed class ProfilePatternPromotionPolicy
{
    public const int MinimumIndependentSupportingRepositories =
        RepeatedProfilePattern.MinimumIndependentSupportingRepositories;

    public const string AutomatedTestingPatternKey = "automated-testing";
    public const string ContinuousIntegrationPatternKey = "continuous-integration";
    public const string DocumentationPatternKey = "documentation";
    public const string ReleaseUsagePatternKey = "release-usage";
    public const string IssueUsagePatternKey = "issue-usage";
    public const string PullRequestUsagePatternKey = "pull-request-usage";

    // Canonical V1 EngineeringPractice taxonomy. Presentation and metadata
    // signals, commit history and commit distribution are deliberately absent.
    private static readonly IReadOnlyList<PracticeContract> EngineeringPractices =
        Array.AsReadOnly<PracticeContract>(
        [
            new(
                AutomatedTestingPatternKey,
                [TestStructureSignalEvaluator.SignalKey]),
            new(
                ContinuousIntegrationPatternKey,
                [ContinuousIntegrationSignalEvaluator.SignalKey]),
            new(
                DocumentationPatternKey,
                [
                    InstallationGuideSignalEvaluator.SignalKey,
                    UsageDemoSignalEvaluator.SignalKey
                ]),
            new(
                ReleaseUsagePatternKey,
                [ReleaseUsageSignalEvaluator.SignalKey]),
            new(
                IssueUsagePatternKey,
                [IssueUsageSignalEvaluator.SignalKey]),
            new(
                PullRequestUsagePatternKey,
                [PullRequestUsageSignalEvaluator.SignalKey])
        ]);

    public ProfilePatternPromotionPolicy(PatternRuleVersion ruleVersion)
    {
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
    }

    public PatternRuleVersion RuleVersion { get; }

    public ProfilePatternSet Promote(CrossRepositoryEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        HashSet<ProfileRepositoryReference> independentRepositories =
        [
            .. evidence.RepositoryContributions
                .Where(contribution => contribution.IndependentRepetition.IsAllowed)
                .Select(contribution => contribution.Reference)
        ];
        IReadOnlyDictionary<string, ObservedProfileEvidence> observations =
            evidence.Observations.ToDictionary(
                observation => observation.ObservationKey,
                StringComparer.Ordinal);
        List<RepeatedProfilePattern> patterns = [];

        patterns.AddRange(PromoteEngineeringPractices(
            observations,
            independentRepositories));
        patterns.AddRange(PromoteTechnicalFocus(
            evidence,
            independentRepositories));

        return new ProfilePatternSet(
            patterns,
            RuleVersion,
            evidence.RuleVersion,
            evidence.PolicyVersion,
            evidence.IsEvidenceSurfaceComplete);
    }

    private IEnumerable<RepeatedProfilePattern> PromoteEngineeringPractices(
        IReadOnlyDictionary<string, ObservedProfileEvidence> observations,
        IReadOnlySet<ProfileRepositoryReference> independentRepositories)
    {
        foreach (PracticeContract practice in EngineeringPractices)
        {
            ObservedProfileEvidence[] supporting = practice.SignalKeys
                .Select(signalKey => observations.TryGetValue(
                    signalKey,
                    out ObservedProfileEvidence? observation)
                    ? observation
                    : null)
                .Where(observation =>
                    observation?.Kind
                        == ObservedProfileEvidenceKind.RepositorySignalEvidence)
                .Select(observation => observation!)
                .ToArray();

            RepeatedProfilePattern? pattern = TryPromote(
                ProfilePatternFamily.EngineeringPractice,
                practice.PatternKey,
                supporting,
                independentRepositories);

            if (pattern is not null)
            {
                yield return pattern;
            }
        }
    }

    private IEnumerable<RepeatedProfilePattern> PromoteTechnicalFocus(
        CrossRepositoryEvidence evidence,
        IReadOnlySet<ProfileRepositoryReference> independentRepositories)
    {
        foreach (ObservedProfileEvidence observation in evidence.Observations)
        {
            if (observation.Kind != ObservedProfileEvidenceKind.RepositoryLanguageEvidence
                || !ProfileEvidenceKeys.TryReadLanguage(
                    observation.ObservationKey,
                    out string language))
            {
                continue;
            }

            // The language literal reported by GitHub stays the canonical
            // pattern identity: no grouping, aliasing or framework inference.
            RepeatedProfilePattern? pattern = TryPromote(
                ProfilePatternFamily.TechnicalFocus,
                language,
                [observation],
                independentRepositories);

            if (pattern is not null)
            {
                yield return pattern;
            }
        }
    }

    private RepeatedProfilePattern? TryPromote(
        ProfilePatternFamily family,
        string patternKey,
        IReadOnlyList<ObservedProfileEvidence> supportingObservations,
        IReadOnlySet<ProfileRepositoryReference> independentRepositories)
    {
        if (supportingObservations.Count == 0)
        {
            return null;
        }

        ProfileRepositoryReference[] independentSupport = supportingObservations
            .SelectMany(observation => observation.ContributingRepositories)
            .Distinct()
            .Where(independentRepositories.Contains)
            .ToArray();

        return independentSupport.Length < MinimumIndependentSupportingRepositories
            ? null
            : new RepeatedProfilePattern(
                family,
                patternKey,
                supportingObservations,
                independentSupport,
                RuleVersion);
    }

    private sealed record PracticeContract(
        string PatternKey,
        IReadOnlyList<string> SignalKeys);
}

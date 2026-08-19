using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile;

// P7 V1 representative-repository selection. Every RepeatedProfilePattern in
// the supplied ProfilePatternSet independently receives exactly one winning
// candidate, drawn only from IndependentSupportingRepositories and compared
// only by PatternSupportObservationCount, tie-broken by Owner-then-Name
// ordinal. The same winning repository across several patterns is grouped
// into one RepresentativeRepository after selection; grouping never
// influences a winner. No rank, score, quality, strength, confidence,
// capability, seniority or employability is produced.
public sealed class RepresentativeRepositorySelectionPolicy
{
    public RepresentativeRepositorySelectionPolicy(
        RepresentativeRepositoryRuleVersion ruleVersion)
    {
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
    }

    public RepresentativeRepositoryRuleVersion RuleVersion { get; }

    public RepresentativeRepositorySelection Select(ProfilePatternSet patternSet)
    {
        ArgumentNullException.ThrowIfNull(patternSet);

        RequireCanonicalRepositoryIdentity(patternSet);

        Dictionary<ProfileRepositoryReference, List<RepresentativePatternSupport>> grouped = [];

        foreach (RepeatedProfilePattern pattern in patternSet.Patterns)
        {
            CandidateEvaluation winner = SelectWinner(pattern);
            RepresentativePatternSupport support = new(
                pattern,
                winner.Repository,
                winner.SupportingEvidenceReferences);

            if (!grouped.TryGetValue(
                    winner.Repository,
                    out List<RepresentativePatternSupport>? supports))
            {
                supports = [];
                grouped[winner.Repository] = supports;
            }

            supports.Add(support);
        }

        IEnumerable<RepresentativeRepository> representatives = grouped
            .Select(entry => new RepresentativeRepository(entry.Key, entry.Value));

        return new RepresentativeRepositorySelection(
            representatives,
            patternSet.RuleVersion,
            patternSet.ObservationRuleVersion,
            patternSet.PolicyVersion,
            patternSet.IsEvidenceSurfaceComplete,
            RuleVersion);
    }

    // Evaluates every candidate's Repository, scoped evidence and
    // PatternSupportObservationCount together, once, so the winner's already
    // -computed scoped evidence can be carried straight into its
    // RepresentativePatternSupport instead of being re-derived from Pattern.
    private static CandidateEvaluation SelectWinner(RepeatedProfilePattern pattern) =>
        pattern.IndependentSupportingRepositories
            .Select(candidate => EvaluateCandidate(pattern, candidate))
            .OrderByDescending(evaluation => evaluation.PatternSupportObservationCount)
            .ThenBy(
                evaluation => evaluation.Repository,
                Comparer<ProfileRepositoryReference>.Create(
                    static (left, right) => left.CompareTo(right)))
            .First();

    private static CandidateEvaluation EvaluateCandidate(
        RepeatedProfilePattern pattern,
        ProfileRepositoryReference candidate)
    {
        List<ProfileEvidenceReference> supportingEvidenceReferences = pattern
            .SupportingObservations
            .SelectMany(observation => observation.EvidenceReferences)
            .Where(reference => reference.Repository == candidate)
            .ToList();

        return new CandidateEvaluation(candidate, supportingEvidenceReferences);
    }

    private readonly record struct CandidateEvaluation(
        ProfileRepositoryReference Repository,
        IReadOnlyList<ProfileEvidenceReference> SupportingEvidenceReferences)
    {
        public int PatternSupportObservationCount => SupportingEvidenceReferences.Count;
    }

    // §6.1: within one selection execution, CompareTo must return 0 only for
    // equal candidate references. Canonical P3-P6 input always satisfies
    // this; a violation is rejected deterministically rather than resolved
    // by enumeration order, a silent pick or a new heuristic.
    private static void RequireCanonicalRepositoryIdentity(ProfilePatternSet patternSet)
    {
        Dictionary<(string Owner, string Name), ProfileRepositoryReference> canonical = [];

        foreach (ProfileRepositoryReference candidate in patternSet.Patterns
            .SelectMany(pattern => pattern.IndependentSupportingRepositories))
        {
            (string Owner, string Name) key = (candidate.Owner, candidate.Name);

            if (canonical.TryGetValue(key, out ProfileRepositoryReference? existing))
            {
                if (existing != candidate)
                {
                    throw new ArgumentException(
                        $"Repository '{candidate.FullName}' has two non-canonical candidate references within one selection execution.",
                        nameof(patternSet));
                }
            }
            else
            {
                canonical[key] = candidate;
            }
        }
    }
}

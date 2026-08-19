using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// The immutable canonical result root of one Public Profile Evidence analysis.
//
// It represents profile-level public evidence truth. It is not the developer,
// not a capability profile, not a resume, not a work-experience or
// employability model, and it carries no score, rank, quality, confidence,
// seniority, job fit, inferred role, localized label, generated prose, UI
// state or narration.
//
// A profile that exists is truth a consumer may safely use: a failed
// composition produces no root at all (ProfileCompositionResult).
public sealed class DeveloperPublicEvidenceProfile
{
    private static readonly IReadOnlyList<RoleEvidenceAlignment> NoRoleEvidenceAlignments =
        new ReadOnlyCollection<RoleEvidenceAlignment>([]);

    public DeveloperPublicEvidenceProfile(
        SnapshotReference snapshotReference,
        ProfileAnalysisStatus analysisStatus,
        ProfileAnalyzerVersion analyzerVersion,
        IEnumerable<RepositoryEligibilityResult> repositoryEligibilityResults,
        CrossRepositoryEvidence crossRepositoryEvidence,
        ProfilePatternSet patternSet,
        RepresentativeRepositorySelection representativeRepositorySelection,
        IEnumerable<ProfileLimitation>? limitations,
        IEnumerable<ProfileWarning>? warnings,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        SnapshotReference = snapshotReference
            ?? throw new ArgumentNullException(nameof(snapshotReference));

        if (!Enum.IsDefined(analysisStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(analysisStatus),
                analysisStatus,
                "Profile analysis status is not defined.");
        }

        AnalysisStatus = analysisStatus;
        AnalyzerVersion = analyzerVersion
            ?? throw new ArgumentNullException(nameof(analyzerVersion));
        CrossRepositoryEvidence = crossRepositoryEvidence
            ?? throw new ArgumentNullException(nameof(crossRepositoryEvidence));
        PatternSet = patternSet
            ?? throw new ArgumentNullException(nameof(patternSet));
        RepresentativeRepositorySelection = representativeRepositorySelection
            ?? throw new ArgumentNullException(
                nameof(representativeRepositorySelection));

        RepositoryEligibilityResults = NormalizeEligibilityResults(
            repositoryEligibilityResults);

        RequireVersionCoherence();

        StartedAt = startedAt.ToUniversalTime();
        CompletedAt = completedAt.ToUniversalTime();

        if (CompletedAt < StartedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedAt),
                completedAt,
                "Analysis cannot complete before it started.");
        }

        HashSet<ProfileRepositoryReference> surface = [.. RepositoryEligibilityResults
            .Select(result => result.Reference)];

        Limitations = NormalizeConditions(
            limitations,
            surface,
            nameof(limitations));
        Warnings = NormalizeConditions(warnings, surface, nameof(warnings));
    }

    public GitHubProfileIdentity ProfileIdentity => SnapshotReference.ProfileIdentity;

    public SnapshotReference SnapshotReference { get; }

    public ProfileAnalysisStatus AnalysisStatus { get; }

    public ProfileAnalyzerVersion AnalyzerVersion { get; }

    public IReadOnlyList<RepositoryEligibilityResult> RepositoryEligibilityResults { get; }

    // References to existing repository-level truth. RepositoryAnalysis itself
    // is never duplicated into this aggregate.
    public IReadOnlyList<RepositoryAnalysisReference> RepositoryAnalysisReferences =>
        [.. RepositoryEligibilityResults
            .Select(result => result.AnalysisReference)
            .OfType<RepositoryAnalysisReference>()];

    public CrossRepositoryEvidence CrossRepositoryEvidence { get; }

    // The single canonical pattern store. TechnicalFocus, EngineeringPractice
    // and ProblemArea are deterministic typed projections over it, never
    // parallel stores holding conflicting truth.
    public IReadOnlyList<RepeatedProfilePattern> RepeatedProfilePatterns =>
        PatternSet.Patterns;

    // The already-determined P7 selection. Composition never reranks or
    // reselects it.
    public IReadOnlyList<RepresentativeRepository> RepresentativeRepositories =>
        RepresentativeRepositorySelection.RepresentativeRepositories;

    // Structurally available and validly empty in V1 under the explicit
    // RoleEvidenceAlignment deferral. Empty is not negative evidence and does
    // not mean "no role match".
    public IReadOnlyList<RoleEvidenceAlignment> RoleEvidenceAlignments =>
        NoRoleEvidenceAlignments;

    public IReadOnlyList<ProfileLimitation> Limitations { get; }

    public IReadOnlyList<ProfileWarning> Warnings { get; }

    // Audit data only. These never influence semantic analysis and never
    // participate in the semantic equality of the result.
    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset CompletedAt { get; }

    internal ProfilePatternSet PatternSet { get; }

    internal RepresentativeRepositorySelection RepresentativeRepositorySelection { get; }

    private IReadOnlyList<RepositoryEligibilityResult> NormalizeEligibilityResults(
        IEnumerable<RepositoryEligibilityResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        List<RepositoryEligibilityResult> normalized = [.. results];

        if (normalized.Any(result => result is null))
        {
            throw new ArgumentException(
                "Repository eligibility results cannot contain null values.",
                nameof(results));
        }

        HashSet<ProfileRepositoryReference> observed = [];

        foreach (RepositoryEligibilityResult result in normalized)
        {
            if (!observed.Add(result.Reference))
            {
                throw new ArgumentException(
                    $"Duplicate eligibility result '{result.Reference.FullName}'.",
                    nameof(results));
            }
        }

        normalized.Sort(static (left, right) =>
            left.Reference.CompareTo(right.Reference));

        return new ReadOnlyCollection<RepositoryEligibilityResult>(normalized);
    }

    // One coherent analysis execution carries one identified set of semantic
    // rule versions. Results produced under different version identities must
    // not be merged into one canonical root.
    //
    // This is the structural half of coherence that belongs at construction.
    // Deciding whether a given lineage is compatible in the broader sense
    // remains composition's responsibility.
    private void RequireVersionCoherence()
    {
        foreach (RepositoryEligibilityResult result in RepositoryEligibilityResults)
        {
            if (result.PolicyVersion != CrossRepositoryEvidence.PolicyVersion)
            {
                throw new ArgumentException(
                    $"Eligibility result '{result.Reference.FullName}' was produced under a "
                    + "different eligibility policy version than the composed evidence.",
                    nameof(RepositoryEligibilityResults));
            }
        }

        Require(
            PatternSet.ObservationRuleVersion == CrossRepositoryEvidence.RuleVersion,
            "The pattern set was promoted from a different observation rule version "
            + "than the composed evidence.");

        Require(
            PatternSet.PolicyVersion == CrossRepositoryEvidence.PolicyVersion,
            "The pattern set was promoted under a different eligibility policy version "
            + "than the composed evidence.");

        Require(
            RepresentativeRepositorySelection.PatternRuleVersion == PatternSet.RuleVersion,
            "The representative selection was produced from a different pattern rule "
            + "version than the composed pattern set.");

        Require(
            RepresentativeRepositorySelection.ObservationRuleVersion
                == PatternSet.ObservationRuleVersion,
            "The representative selection was produced from a different observation rule "
            + "version than the composed pattern set.");

        Require(
            RepresentativeRepositorySelection.EligibilityPolicyVersion
                == PatternSet.PolicyVersion,
            "The representative selection was produced under a different eligibility "
            + "policy version than the composed pattern set.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message);
        }
    }

    // Deduplication uses the structured identity; ordering uses the canonical
    // keys. Localized text participates in neither.
    private static IReadOnlyList<TCondition> NormalizeConditions<TCondition, TCode>(
        IEnumerable<TCondition>? conditions,
        HashSet<ProfileRepositoryReference> surface,
        string parameterName)
        where TCondition : ProfileCondition<TCode>
        where TCode : struct, Enum
    {
        List<TCondition> normalized = conditions?.ToList() ?? [];

        if (normalized.Any(condition => condition is null))
        {
            throw new ArgumentException(
                "Profile conditions cannot contain null values.",
                parameterName);
        }

        foreach (TCondition condition in normalized)
        {
            foreach (ProfileRepositoryReference repository in condition.AffectedRepositories)
            {
                if (!surface.Contains(repository))
                {
                    throw new ArgumentException(
                        $"Condition affects repository '{repository.FullName}', which does "
                        + "not belong to this profile evidence surface.",
                        parameterName);
                }
            }
        }

        List<TCondition> distinct = [.. normalized.Distinct()];
        distinct.Sort(static (left, right) => left.CompareOrderTo(right));

        return new ReadOnlyCollection<TCondition>(distinct);
    }

    private IReadOnlyList<ProfileLimitation> NormalizeConditions(
        IEnumerable<ProfileLimitation>? conditions,
        HashSet<ProfileRepositoryReference> surface,
        string parameterName) =>
        NormalizeConditions<ProfileLimitation, ProfileLimitationCode>(
            conditions,
            surface,
            parameterName);

    private IReadOnlyList<ProfileWarning> NormalizeConditions(
        IEnumerable<ProfileWarning>? conditions,
        HashSet<ProfileRepositoryReference> surface,
        string parameterName) =>
        NormalizeConditions<ProfileWarning, ProfileWarningCode>(
            conditions,
            surface,
            parameterName);
}

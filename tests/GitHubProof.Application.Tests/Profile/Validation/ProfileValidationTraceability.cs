using System.Runtime.CompilerServices;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Tests.Profile.Validation;

// The single §6 trace-chain check locked by
// docs/product/p9-profile-validation-contract.md §6.1/§6.2:
//
//     Profile-level claim -> pattern -> supporting observations
//     -> repository references -> repository-level evidence
//
// It introduces no second traceability model. It reuses the exact same
// reference-identity ("Canonical Pattern/Observation Trace") technique
// ProfileComposer and ProfileReportProjector already apply in production
// (ReferenceEqualityComparer.Instance), so a claim is only ever traceable
// here if it is traceable by the same rule the real pipeline already
// enforces at construction time.
internal static class ProfileValidationTraceability
{
    // TechnicalFocusAreas / EngineeringPracticePatterns claims.
    public static bool IsTraceable(
        DeveloperPublicEvidenceProfile profile,
        PatternReportItem item)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(item);

        return IsPatternTraceable(profile, item.Pattern)
            && (item.RepresentativeRepository is null
                || profile.RepresentativeRepositories.Any(
                    representative => representative.Repository == item.RepresentativeRepository));
    }

    // WhyThisConclusion claims.
    public static bool IsTraceable(
        DeveloperPublicEvidenceProfile profile,
        ConclusionEvidenceReportItem item)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(item);

        return IsPatternTraceable(profile, item.Pattern)
            && item.SupportingEvidence.Count > 0
            && item.SupportingEvidence.All(evidence => IsEvidenceTraceable(profile, evidence));
    }

    // RepresentativeRepositories claims.
    public static bool IsTraceable(
        DeveloperPublicEvidenceProfile profile,
        RepresentativeRepository representative)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(representative);

        return representative.PatternSupports.All(support =>
            IsPatternTraceable(profile, support.Pattern)
            && support.SupportingEvidenceReferences.Count > 0
            && support.SupportingEvidenceReferences.All(
                evidence => IsEvidenceTraceable(profile, evidence)));
    }

    // Bounded to the four meaningful claim surfaces locked by contract §6.3:
    // TechnicalFocusAreas, EngineeringPracticePatterns, RepresentativeRepositories,
    // WhyThisConclusion.
    public static bool AllMeaningfulClaimsAreTraceable(
        DeveloperPublicEvidenceProfile profile,
        ProfileReport report)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(report);

        return report.TechnicalFocusAreas.Items.All(item => IsTraceable(profile, item))
            && report.EngineeringPracticePatterns.Items.All(item => IsTraceable(profile, item))
            && report.RepresentativeRepositories.Items.All(item => IsTraceable(profile, item))
            && report.WhyThisConclusion.Items.All(item => IsTraceable(profile, item));
    }

    private static bool IsPatternTraceable(
        DeveloperPublicEvidenceProfile profile,
        RepeatedProfilePattern pattern) =>
        profile.RepeatedProfilePatterns.Contains(pattern, ReferenceEqualityComparer.Instance)
        && pattern.SupportingObservations.All(observation =>
            profile.CrossRepositoryEvidence.Observations.Contains(
                observation,
                ReferenceEqualityComparer.Instance));

    private static bool IsEvidenceTraceable(
        DeveloperPublicEvidenceProfile profile,
        ProfileEvidenceReference evidence) =>
        evidence.Analysis.AnalysisId != Guid.Empty
        && profile.RepositoryEligibilityResults.Any(
            result => result.Reference == evidence.Repository);
}

using System.Runtime.CompilerServices;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile.Report;

// The deterministic, one-way projection from DeveloperPublicEvidenceProfile
// to ProfileReport. It reruns nothing: no eligibility, no observation, no
// pattern promotion, no representative selection, no profile composition.
// It reads only what the canonical root already provides, in the order the
// root already provides it, and produces no claim absent from that root.
public sealed class ProfileReportProjector
{
    public ProfileReport Project(DeveloperPublicEvidenceProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        IReadOnlyDictionary<RepeatedProfilePattern, ProfileRepositoryReference>
            representativeByPattern = IndexRepresentativesByPattern(profile);

        ProfileReportSection<PatternReportItem> technicalFocusAreas = ProjectPatterns(
            ProfileReportSectionKey.TechnicalFocusAreas,
            profile,
            ProfilePatternFamily.TechnicalFocus,
            representativeByPattern);

        ProfileReportSection<PatternReportItem> engineeringPracticePatterns = ProjectPatterns(
            ProfileReportSectionKey.EngineeringPracticePatterns,
            profile,
            ProfilePatternFamily.EngineeringPractice,
            representativeByPattern);

        // ProblemArea has no V1 taxonomy or promotion rule: no pattern is ever
        // read for it, regardless of what the pattern set happens to contain.
        ProfileReportSection<PatternReportItem> problemAreaPatterns = new(
            ProfileReportSectionKey.ProblemAreaPatterns,
            ProfileReportSectionAvailability.Unsupported,
            items: null);

        ProfileReportSection<RepresentativeRepository> representativeRepositories = new(
            ProfileReportSectionKey.RepresentativeRepositories,
            ProfileReportSectionAvailability.Supported,
            profile.RepresentativeRepositories);

        // Only the V1 supported pattern-conclusion families may surface a
        // conclusion trace. ProblemArea is Unsupported in V1 (no taxonomy, no
        // promotion rule), so a ProblemArea pattern must never appear here
        // even if one exists in the upstream pattern set.
        ProfileReportSection<ConclusionEvidenceReportItem> whyThisConclusion = new(
            ProfileReportSectionKey.WhyThisConclusion,
            ProfileReportSectionAvailability.Supported,
            profile.RepeatedProfilePatterns
                .Where(pattern => pattern.Family is ProfilePatternFamily.TechnicalFocus
                    or ProfilePatternFamily.EngineeringPractice)
                .Select(pattern => new ConclusionEvidenceReportItem(pattern)));

        ProfileReportSection<ProfileLimitation> limitations = new(
            ProfileReportSectionKey.Limitations,
            ProfileReportSectionAvailability.Supported,
            profile.Limitations);

        ProfileReportSection<RepositoryDrillDownReportItem> repositoryDrillDown = new(
            ProfileReportSectionKey.RepositoryDrillDown,
            ProfileReportSectionAvailability.Supported,
            profile.RepositoryEligibilityResults.Select(
                result => new RepositoryDrillDownReportItem(
                    result.Reference,
                    result.Status,
                    result.AnalysisReference)));

        // Structurally always empty: RoleEvidenceAlignment has no accessible
        // constructor until a future contract defines the capability.
        ProfileReportSection<RoleEvidenceAlignment> roleEvidenceAlignment = new(
            ProfileReportSectionKey.RoleEvidenceAlignment,
            ProfileReportSectionAvailability.Deferred,
            items: null);

        ProfileReportSection<ProfileSummaryFact> publicProfileSummary = new(
            ProfileReportSectionKey.PublicProfileSummary,
            ProfileReportSectionAvailability.Supported,
            [
                Fact(technicalFocusAreas),
                Fact(engineeringPracticePatterns),
                Fact(problemAreaPatterns),
                Fact(representativeRepositories),
                Fact(whyThisConclusion),
                Fact(limitations),
                Fact(repositoryDrillDown),
                Fact(roleEvidenceAlignment)
            ]);

        return new ProfileReport(
            publicProfileSummary,
            technicalFocusAreas,
            engineeringPracticePatterns,
            problemAreaPatterns,
            representativeRepositories,
            whyThisConclusion,
            limitations,
            repositoryDrillDown,
            roleEvidenceAlignment,
            profile.Warnings);
    }

    private static ProfileSummaryFact Fact<TItem>(ProfileReportSection<TItem> section) =>
        new(section.SectionKey, section.Availability, section.Items.Count);

    // Family filtering preserves profile.RepeatedProfilePatterns' own
    // canonical order (Family then PatternKey, ordinal) — no independent sort
    // is introduced here.
    private static ProfileReportSection<PatternReportItem> ProjectPatterns(
        ProfileReportSectionKey sectionKey,
        DeveloperPublicEvidenceProfile profile,
        ProfilePatternFamily family,
        IReadOnlyDictionary<RepeatedProfilePattern, ProfileRepositoryReference>
            representativeByPattern) =>
        new(
            sectionKey,
            ProfileReportSectionAvailability.Supported,
            profile.RepeatedProfilePatterns
                .Where(pattern => pattern.Family == family)
                .Select(pattern => new PatternReportItem(
                    pattern,
                    representativeByPattern.TryGetValue(
                        pattern,
                        out ProfileRepositoryReference? representative)
                        ? representative
                        : null)));

    // Every RepeatedProfilePattern a representative selection cites is, by
    // reference, one of the patterns the profile's own pattern set produced
    // (the "Canonical Pattern Trace"), so keying by reference identity here
    // never conflates two distinct promoted patterns.
    private static IReadOnlyDictionary<RepeatedProfilePattern, ProfileRepositoryReference>
        IndexRepresentativesByPattern(DeveloperPublicEvidenceProfile profile)
    {
        Dictionary<RepeatedProfilePattern, ProfileRepositoryReference> index = new(
            ReferenceEqualityComparer.Instance);

        foreach (RepresentativeRepository representative in profile.RepresentativeRepositories)
        {
            foreach (RepresentativePatternSupport support in representative.PatternSupports)
            {
                index[support.Pattern] = representative.Repository;
            }
        }

        return index;
    }
}

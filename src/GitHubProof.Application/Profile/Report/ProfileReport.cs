using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Application.Profile.Report;

// The typed, one-way projection of one DeveloperPublicEvidenceProfile
// (public-profile-evidence-report-contract.md). It represents presentable
// report truth; it never flows back into canonical profile truth, and it
// carries no score, rank, quality, confidence, capability, seniority,
// employability, job fit, inferred role or LLM narration.
//
// All nine canonical sections are always present, in canonical order,
// regardless of availability or emptiness. Warnings are canonical root truth
// carried for presentation consumers; they are not a tenth report section.
public sealed class ProfileReport
{
    public ProfileReport(
        ProfileReportSection<ProfileSummaryFact> publicProfileSummary,
        ProfileReportSection<PatternReportItem> technicalFocusAreas,
        ProfileReportSection<PatternReportItem> engineeringPracticePatterns,
        ProfileReportSection<PatternReportItem> problemAreaPatterns,
        ProfileReportSection<RepresentativeRepository> representativeRepositories,
        ProfileReportSection<ConclusionEvidenceReportItem> whyThisConclusion,
        ProfileReportSection<ProfileLimitation> limitations,
        ProfileReportSection<RepositoryDrillDownReportItem> repositoryDrillDown,
        ProfileReportSection<RoleEvidenceAlignment> roleEvidenceAlignment,
        IReadOnlyList<ProfileWarning> warnings)
    {
        PublicProfileSummary = RequireSection(
            publicProfileSummary,
            ProfileReportSectionKey.PublicProfileSummary,
            ProfileReportSectionAvailability.Supported);
        TechnicalFocusAreas = RequireSection(
            technicalFocusAreas,
            ProfileReportSectionKey.TechnicalFocusAreas,
            ProfileReportSectionAvailability.Supported);
        EngineeringPracticePatterns = RequireSection(
            engineeringPracticePatterns,
            ProfileReportSectionKey.EngineeringPracticePatterns,
            ProfileReportSectionAvailability.Supported);
        ProblemAreaPatterns = RequireSection(
            problemAreaPatterns,
            ProfileReportSectionKey.ProblemAreaPatterns,
            ProfileReportSectionAvailability.Unsupported);
        RepresentativeRepositories = RequireSection(
            representativeRepositories,
            ProfileReportSectionKey.RepresentativeRepositories,
            ProfileReportSectionAvailability.Supported);
        WhyThisConclusion = RequireSection(
            whyThisConclusion,
            ProfileReportSectionKey.WhyThisConclusion,
            ProfileReportSectionAvailability.Supported);
        Limitations = RequireSection(
            limitations,
            ProfileReportSectionKey.Limitations,
            ProfileReportSectionAvailability.Supported);
        RepositoryDrillDown = RequireSection(
            repositoryDrillDown,
            ProfileReportSectionKey.RepositoryDrillDown,
            ProfileReportSectionAvailability.Supported);
        RoleEvidenceAlignment = RequireSection(
            roleEvidenceAlignment,
            ProfileReportSectionKey.RoleEvidenceAlignment,
            ProfileReportSectionAvailability.Deferred);

        // ProblemArea has no V1 promotion rule at all: the capability boundary
        // is unconditional, not merely "usually empty".
        if (ProblemAreaPatterns.Items.Count > 0)
        {
            throw new ArgumentException(
                "ProblemAreaPatterns is Unsupported in V1 and must be empty.",
                nameof(problemAreaPatterns));
        }

        Warnings = warnings is null
            ? throw new ArgumentNullException(nameof(warnings))
            : new ReadOnlyCollection<ProfileWarning>([.. warnings]);

        // Fixed, locale-independent (report contract §7). RoleEvidenceAlignment
        // is uninstantiable in V1, so its section can never carry an item —
        // no runtime emptiness guard is needed for it, unlike ProblemArea.
        Sections = new ReadOnlyCollection<ProfileReportSectionDescriptor>(
        [
            PublicProfileSummary.Descriptor,
            TechnicalFocusAreas.Descriptor,
            EngineeringPracticePatterns.Descriptor,
            ProblemAreaPatterns.Descriptor,
            RepresentativeRepositories.Descriptor,
            WhyThisConclusion.Descriptor,
            Limitations.Descriptor,
            RepositoryDrillDown.Descriptor,
            RoleEvidenceAlignment.Descriptor
        ]);
    }

    public ProfileReportSection<ProfileSummaryFact> PublicProfileSummary { get; }

    public ProfileReportSection<PatternReportItem> TechnicalFocusAreas { get; }

    public ProfileReportSection<PatternReportItem> EngineeringPracticePatterns { get; }

    // Contractually Unsupported + empty in V1: ProblemArea has no taxonomy,
    // no promotion rule and no production code path yet.
    public ProfileReportSection<PatternReportItem> ProblemAreaPatterns { get; }

    public ProfileReportSection<RepresentativeRepository> RepresentativeRepositories { get; }

    public ProfileReportSection<ConclusionEvidenceReportItem> WhyThisConclusion { get; }

    public ProfileReportSection<ProfileLimitation> Limitations { get; }

    public ProfileReportSection<RepositoryDrillDownReportItem> RepositoryDrillDown { get; }

    // Contractually Deferred + empty in V1: the type itself is uninstantiable
    // until a future contract defines the capability.
    public ProfileReportSection<RoleEvidenceAlignment> RoleEvidenceAlignment { get; }

    // Canonical root truth, carried for presentation consumers. Not one of
    // the nine main report sections (report contract §13); never used to
    // determine section availability or item membership.
    public IReadOnlyList<ProfileWarning> Warnings { get; }

    // The canonical section set, in canonical order, independent of item
    // type. Always exactly nine entries.
    public IReadOnlyList<ProfileReportSectionDescriptor> Sections { get; }

    private static ProfileReportSection<TItem> RequireSection<TItem>(
        ProfileReportSection<TItem> section,
        ProfileReportSectionKey expectedKey,
        ProfileReportSectionAvailability expectedAvailability)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (section.SectionKey != expectedKey)
        {
            throw new ArgumentException(
                $"Expected section '{expectedKey}' but received '{section.SectionKey}'.",
                nameof(section));
        }

        if (section.Availability != expectedAvailability)
        {
            throw new ArgumentException(
                $"Section '{expectedKey}' must be {expectedAvailability} in V1, "
                + $"but was {section.Availability}.",
                nameof(section));
        }

        return section;
    }
}

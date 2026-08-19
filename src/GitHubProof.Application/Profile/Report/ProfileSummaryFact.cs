namespace GitHubProof.Application.Profile.Report;

// One structured, traceable summary fact: a reference to another report
// section plus that section's item count. PublicProfileSummary may only
// compress existing projected truth this way (report contract §8) — it must
// never introduce a claim absent from another section, and it carries no
// generated prose.
public sealed class ProfileSummaryFact
{
    public ProfileSummaryFact(
        ProfileReportSectionKey referencedSection,
        ProfileReportSectionAvailability referencedSectionAvailability,
        int itemCount)
    {
        if (!Enum.IsDefined(referencedSection))
        {
            throw new ArgumentOutOfRangeException(
                nameof(referencedSection),
                referencedSection,
                "Profile report section key is not defined.");
        }

        if (!Enum.IsDefined(referencedSectionAvailability))
        {
            throw new ArgumentOutOfRangeException(
                nameof(referencedSectionAvailability),
                referencedSectionAvailability,
                "Profile report section availability is not defined.");
        }

        if (itemCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                "Item count cannot be negative.");
        }

        ReferencedSection = referencedSection;
        ReferencedSectionAvailability = referencedSectionAvailability;
        ItemCount = itemCount;
    }

    public ProfileReportSectionKey ReferencedSection { get; }

    public ProfileReportSectionAvailability ReferencedSectionAvailability { get; }

    public int ItemCount { get; }
}

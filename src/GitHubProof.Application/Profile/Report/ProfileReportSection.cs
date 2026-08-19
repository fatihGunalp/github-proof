using System.Collections.ObjectModel;

namespace GitHubProof.Application.Profile.Report;

// One typed report section: its canonical identity, its availability, and
// its items in the order the canonical upstream truth already provides. This
// type never sorts or reorders Items — that is the responsibility of
// whichever canonical Domain result the projector reads from.
public sealed class ProfileReportSection<TItem>
{
    public ProfileReportSection(
        ProfileReportSectionKey sectionKey,
        ProfileReportSectionAvailability availability,
        IEnumerable<TItem>? items)
    {
        if (!Enum.IsDefined(sectionKey))
        {
            throw new ArgumentOutOfRangeException(
                nameof(sectionKey),
                sectionKey,
                "Profile report section key is not defined.");
        }

        if (!Enum.IsDefined(availability))
        {
            throw new ArgumentOutOfRangeException(
                nameof(availability),
                availability,
                "Profile report section availability is not defined.");
        }

        SectionKey = sectionKey;
        Availability = availability;
        Items = items is null
            ? Array.Empty<TItem>()
            : new ReadOnlyCollection<TItem>([.. items]);
    }

    public ProfileReportSectionKey SectionKey { get; }

    public ProfileReportSectionAvailability Availability { get; }

    public IReadOnlyList<TItem> Items { get; }

    public ProfileReportSectionDescriptor Descriptor => new(SectionKey, Availability);
}

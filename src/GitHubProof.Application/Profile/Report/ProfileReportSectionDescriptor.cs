namespace GitHubProof.Application.Profile.Report;

// A section's identity and availability, independent of its typed item
// shape. Used to verify the canonical section set/order/availability without
// requiring callers to know each section's item type.
public sealed record ProfileReportSectionDescriptor(
    ProfileReportSectionKey SectionKey,
    ProfileReportSectionAvailability Availability);

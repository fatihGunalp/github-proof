using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile.Report;

// A TechnicalFocusAreas/EngineeringPracticePatterns report item. Pattern is
// the canonical RepeatedProfilePattern itself (report contract §9: the
// canonical pattern key/family is identity, never re-derived here);
// RepresentativeRepository is the P7 selection already made for this pattern,
// carried by reference only. It is never re-selected or re-ranked here.
public sealed class PatternReportItem
{
    public PatternReportItem(
        RepeatedProfilePattern pattern,
        ProfileRepositoryReference? representativeRepository)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        RepresentativeRepository = representativeRepository;
    }

    public RepeatedProfilePattern Pattern { get; }

    public ProfileRepositoryReference? RepresentativeRepository { get; }
}

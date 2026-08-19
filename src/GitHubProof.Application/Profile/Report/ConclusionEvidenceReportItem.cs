using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile.Report;

// The WhyThisConclusion evidence-first trace for one promoted pattern:
// Pattern -> supporting repositories -> repository-level evidence
// (report contract §11). SupportingEvidence is read directly from the
// pattern's own already-owned observations; it exposes the evidence chain,
// never the internal selection algorithm.
public sealed class ConclusionEvidenceReportItem
{
    public ConclusionEvidenceReportItem(RepeatedProfilePattern pattern)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        SupportingEvidence = new ReadOnlyCollection<ProfileEvidenceReference>(
            [.. pattern.SupportingObservations.SelectMany(
                observation => observation.EvidenceReferences)]);
    }

    public RepeatedProfilePattern Pattern { get; }

    public IReadOnlyList<ProfileEvidenceReference> SupportingEvidence { get; }
}

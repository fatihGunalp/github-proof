namespace GitHubProof.Domain.Profile.Enums;

// The canonical contribution purposes locked by the repository eligibility
// contract. ProfileContext is context-only and is excluded from overall
// evidence contribution derivation; the remaining four are evidence-bearing.
public enum ContributionPurpose
{
    ProfileContext = 1,
    ProfileObservation = 2,
    IndependentRepetition = 3,
    CurrentFocus = 4,
    HistoricalEvidence = 5
}

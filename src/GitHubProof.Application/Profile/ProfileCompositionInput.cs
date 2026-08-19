using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// The bundle of already-produced P3-P8 results ProfileComposer assembles into
// one DeveloperPublicEvidenceProfile. It performs no coherence or semantic
// validation of its own: reconciling these results, or failing when they
// cannot be reconciled, is entirely ProfileComposer's responsibility.
public sealed class ProfileCompositionInput
{
    public ProfileCompositionInput(
        ProfileAnalysisOrchestrationResult orchestration,
        IReadOnlyList<RepositoryEligibilityResult> repositoryEligibilityResults,
        CrossRepositoryEvidence crossRepositoryEvidence,
        ProfilePatternSet patternSet,
        RepresentativeRepositorySelection representativeRepositorySelection,
        ProfileAnalyzerVersion analyzerVersion,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        Orchestration = orchestration
            ?? throw new ArgumentNullException(nameof(orchestration));
        RepositoryEligibilityResults = repositoryEligibilityResults
            ?? throw new ArgumentNullException(nameof(repositoryEligibilityResults));
        CrossRepositoryEvidence = crossRepositoryEvidence
            ?? throw new ArgumentNullException(nameof(crossRepositoryEvidence));
        PatternSet = patternSet
            ?? throw new ArgumentNullException(nameof(patternSet));
        RepresentativeRepositorySelection = representativeRepositorySelection
            ?? throw new ArgumentNullException(
                nameof(representativeRepositorySelection));
        AnalyzerVersion = analyzerVersion
            ?? throw new ArgumentNullException(nameof(analyzerVersion));
        StartedAt = startedAt;
        CompletedAt = completedAt;
    }

    public ProfileAnalysisOrchestrationResult Orchestration { get; }

    public IReadOnlyList<RepositoryEligibilityResult> RepositoryEligibilityResults { get; }

    public CrossRepositoryEvidence CrossRepositoryEvidence { get; }

    public ProfilePatternSet PatternSet { get; }

    public RepresentativeRepositorySelection RepresentativeRepositorySelection { get; }

    public ProfileAnalyzerVersion AnalyzerVersion { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset CompletedAt { get; }
}

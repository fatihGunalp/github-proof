using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// Orchestration result over one profile evidence surface. It carries no
// cross-repository aggregation, pattern or developer-level interpretation.
public sealed class ProfileAnalysisOrchestrationResult
{
    public ProfileAnalysisOrchestrationResult(
        GitHubProfileSnapshot snapshot,
        AnalysisVersion analysisVersion,
        EligibilityPolicyVersion policyVersion,
        IEnumerable<ProfileRepositoryAnalysisResult> results)
    {
        Snapshot = snapshot
            ?? throw new ArgumentNullException(nameof(snapshot));
        AnalysisVersion = analysisVersion
            ?? throw new ArgumentNullException(nameof(analysisVersion));
        PolicyVersion = policyVersion
            ?? throw new ArgumentNullException(nameof(policyVersion));
        Results = NormalizeResults(snapshot, results);
    }

    public GitHubProfileSnapshot Snapshot { get; }

    public AnalysisVersion AnalysisVersion { get; }

    public EligibilityPolicyVersion PolicyVersion { get; }

    public IReadOnlyList<ProfileRepositoryAnalysisResult> Results { get; }

    public bool IsRepositoryManifestComplete => Snapshot.Repositories.IsComplete;

    private static IReadOnlyList<ProfileRepositoryAnalysisResult> NormalizeResults(
        GitHubProfileSnapshot snapshot,
        IEnumerable<ProfileRepositoryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        List<ProfileRepositoryAnalysisResult> normalized = results.ToList();

        if (normalized.Any(result => result is null))
        {
            throw new ArgumentException(
                "Repository results cannot contain null values.",
                nameof(results));
        }

        HashSet<ProfileRepositoryReference> manifest = snapshot
            .Repositories
            .Repositories
            .Select(repository => repository.Reference)
            .ToHashSet();
        HashSet<ProfileRepositoryReference> observed = [];

        foreach (ProfileRepositoryAnalysisResult result in normalized)
        {
            if (!manifest.Contains(result.Reference))
            {
                throw new ArgumentException(
                    $"Repository '{result.Reference.FullName}' does not belong to the analyzed profile snapshot.",
                    nameof(results));
            }

            if (!observed.Add(result.Reference))
            {
                throw new ArgumentException(
                    $"Duplicate repository result '{result.Reference.FullName}'.",
                    nameof(results));
            }
        }

        if (observed.Count != manifest.Count)
        {
            throw new ArgumentException(
                "Every repository in the analyzed profile snapshot requires a result.",
                nameof(results));
        }

        normalized.Sort(static (left, right) =>
            ProfileRepositoryReferenceOrder.Instance.Compare(
                left.Reference,
                right.Reference));

        return new ReadOnlyCollection<ProfileRepositoryAnalysisResult>(normalized);
    }
}

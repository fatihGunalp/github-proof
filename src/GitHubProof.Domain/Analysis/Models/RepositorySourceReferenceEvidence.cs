using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Analysis.Models;

// Non-scoring repository result derived from one RepositoryAnalysis and one
// SourceReferenceRuleVersion. It participates in no signal, category, score or
// report, it is never written back into RepositorySnapshot, and it is never
// ProducedWorkEvidence or ProblemArea evidence.
//
// The surface is positive-only, as locked by
// docs/architecture/repository-dependency-source-reference-detection.md §11:
// there is no unreferenced-declaration collection, no count of declarations
// without references, no ratio and no coverage value, and a consumer must not
// compute one either.
public sealed class RepositorySourceReferenceEvidence
{
    public RepositorySourceReferenceEvidence(
        RepositoryAnalysisReference analysis,
        SourceReferenceRuleVersion ruleVersion,
        DataAvailability? dependencyManifestsAvailability,
        DataAvailability? sourceFilesAvailability,
        bool areDependencyDeclarationsComplete,
        bool areSourceFilesComplete,
        bool hasUnreliableSourceArtefact,
        IEnumerable<DependencySourceReferenceEvidence> references)
    {
        Analysis = analysis
            ?? throw new ArgumentNullException(nameof(analysis));
        RuleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
        DependencyManifestsAvailability = dependencyManifestsAvailability;
        SourceFilesAvailability = sourceFilesAvailability;
        AreDependencyDeclarationsComplete = areDependencyDeclarationsComplete;
        AreSourceFilesComplete = areSourceFilesComplete;
        HasUnreliableSourceArtefact = hasUnreliableSourceArtefact;
        References = NormalizeReferences(references);
    }

    public RepositoryAnalysisReference Analysis { get; }

    public SourceReferenceRuleVersion RuleVersion { get; }

    // Null when the analysed snapshot recorded no availability at all.
    // Unavailable and unrecorded are both undetermined, never absence.
    public DataAvailability? DependencyManifestsAvailability { get; }

    public DataAvailability? SourceFilesAvailability { get; }

    public bool AreDependencyDeclarationsComplete { get; }

    public bool AreSourceFilesComplete { get; }

    // True when at least one collected artefact reached end of file while the
    // lexical scanner was still inside an unterminated modeled string or block
    // comment. Every observation from such an artefact is discarded under §6.1,
    // and detection is incomplete.
    public bool HasUnreliableSourceArtefact { get; }

    public IReadOnlyList<DependencySourceReferenceEvidence> References { get; }

    // Either required input was unavailable or unrecorded, so detection says
    // nothing at all about this repository. It is never "no references".
    public bool IsDetectionUndetermined =>
        DependencyManifestsAvailability?.IsAvailable != true
        || SourceFilesAvailability?.IsAvailable != true;

    // True only when both supported surfaces were available, both were scanned
    // completely, and no artefact was discarded as unreliable. An empty
    // reference list under this flag means only that no qualifying reference was
    // observed inside the supported bounded surface. Detection is normally
    // incomplete, because the collection bounds sample a repository rather than
    // enumerate it; that is the expected state and never a defect.
    public bool IsDetectionSurfaceComplete =>
        !IsDetectionUndetermined
        && AreDependencyDeclarationsComplete
        && AreSourceFilesComplete
        && !HasUnreliableSourceArtefact;

    private static IReadOnlyList<DependencySourceReferenceEvidence> NormalizeReferences(
        IEnumerable<DependencySourceReferenceEvidence> references)
    {
        ArgumentNullException.ThrowIfNull(references);

        List<DependencySourceReferenceEvidence> normalized = references.ToList();
        HashSet<(Enums.DependencyEcosystem, string, string, string)> observed = [];

        foreach (DependencySourceReferenceEvidence reference in normalized)
        {
            ArgumentNullException.ThrowIfNull(reference);

            if (!observed.Add((
                    reference.Ecosystem,
                    reference.Declaration.ManifestPath,
                    reference.Declaration.PackageId,
                    reference.Declaration.DeclaredVersion ?? string.Empty)))
            {
                throw new ArgumentException(
                    $"Declaration '{reference.Declaration.PackageId}' is observed more than once.",
                    nameof(references));
            }
        }

        normalized.Sort(CompareReferences);

        return new ReadOnlyCollection<DependencySourceReferenceEvidence>(normalized);
    }

    // §13: ecosystem, then declaration identity, then the declaration's manifest
    // path and declared version literal, all Ordinal ascending.
    private static int CompareReferences(
        DependencySourceReferenceEvidence left,
        DependencySourceReferenceEvidence right)
    {
        int ecosystem = string.CompareOrdinal(
            left.Ecosystem.ToString(),
            right.Ecosystem.ToString());

        if (ecosystem != 0)
        {
            return ecosystem;
        }

        int package = string.CompareOrdinal(
            left.Declaration.PackageId,
            right.Declaration.PackageId);

        if (package != 0)
        {
            return package;
        }

        int manifest = string.CompareOrdinal(
            left.Declaration.ManifestPath,
            right.Declaration.ManifestPath);

        return manifest != 0
            ? manifest
            : string.CompareOrdinal(
                left.Declaration.DeclaredVersion ?? string.Empty,
                right.Declaration.DeclaredVersion ?? string.Empty);
    }
}

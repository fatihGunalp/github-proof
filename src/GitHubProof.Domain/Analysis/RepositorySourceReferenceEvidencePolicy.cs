using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Analysis;

// Derives RepositorySourceReferenceEvidence deterministically from one
// RepositoryAnalysis, implementing
// docs/architecture/repository-dependency-source-reference-detection.md.
//
// It reads collected declarations and collected source artefacts only. It never
// retrieves anything, never executes, builds or evaluates repository source,
// never logs source content, never scores, never writes into RepositorySnapshot
// and never produces ProducedWorkEvidence or ProblemArea evidence.
public sealed class RepositorySourceReferenceEvidencePolicy
{
    private readonly SourceReferenceRuleVersion ruleVersion;

    public RepositorySourceReferenceEvidencePolicy()
        : this(SourceReferenceRuleVersion.V1)
    {
    }

    public RepositorySourceReferenceEvidencePolicy(SourceReferenceRuleVersion ruleVersion)
    {
        this.ruleVersion = ruleVersion
            ?? throw new ArgumentNullException(nameof(ruleVersion));
    }

    public RepositorySourceReferenceEvidence Compose(RepositoryAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        IReadOnlyList<RepositoryDependencyDeclaration> candidates =
            SelectCandidateDeclarations(analysis.Snapshot.DependencyDeclarations.Items);

        Dictionary<RepositoryDependencyDeclaration, ObservationBuilder> observations = [];
        bool hasUnreliableArtefact = false;

        foreach (RepositorySourceFile artefact in analysis.Snapshot.SourceFiles.Items)
        {
            DependencyEcosystem? ecosystem = SourceArtefactEcosystem.Resolve(artefact.Path);

            // §4: an ecosystem outside V1, and an artefact carrying no allow-list
            // extension, produce no observation of either polarity.
            if (ecosystem is null || !SourceArtefactEcosystem.IsSupported(ecosystem.Value))
            {
                continue;
            }

            SourceLexicalScan scan = SourceLexicalScanner.Scan(artefact.Content, ecosystem.Value);

            // §6.1: an artefact whose scan reached end of file inside an
            // unterminated modeled string or block comment is unreliably
            // classified. Every observation from it is discarded and detection
            // becomes incomplete. A partial result is never returned.
            if (scan.IsDesynchronized)
            {
                hasUnreliableArtefact = true;
                continue;
            }

            foreach (SourceImportCandidate candidate in
                SourceImportReader.Read(artefact.Content, ecosystem.Value, scan))
            {
                RepositoryDependencyDeclaration? owner = ResolveOwner(
                    candidates,
                    ecosystem.Value,
                    candidate.Identity);

                if (owner is null)
                {
                    continue;
                }

                if (!observations.TryGetValue(owner, out ObservationBuilder? builder))
                {
                    builder = new ObservationBuilder(ecosystem.Value, owner);
                    observations.Add(owner, builder);
                }

                builder.Add(candidate, artefact.Path);
            }
        }

        return new RepositorySourceReferenceEvidence(
            new RepositoryAnalysisReference(
                analysis.AnalysisId,
                analysis.Repository,
                analysis.Version),
            ruleVersion,
            analysis.Snapshot.FindAvailability(RepositoryDataKeys.DependencyManifests),
            analysis.Snapshot.FindAvailability(RepositoryDataKeys.SourceFiles),
            analysis.Snapshot.DependencyDeclarations.IsComplete,
            analysis.Snapshot.SourceFiles.IsComplete,
            hasUnreliableArtefact,
            observations.Values.Select(builder => builder.Build()));
    }

    // §10: only classes actually identifiable from the normalized declaration
    // surface are excluded. An npm `@types/*` package is identifiable by name and
    // an MSBuild property placeholder by its `$(...)` form. A DotNet
    // analyzer-only or build-only package is deliberately not excluded, because
    // PrivateAssets and IncludeAssets are not part of the declaration surface,
    // and §10.1 records that a Go indirect requirement is indistinguishable from
    // an ordinary Go declaration and is therefore treated as one.
    private static IReadOnlyList<RepositoryDependencyDeclaration> SelectCandidateDeclarations(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations)
    {
        List<RepositoryDependencyDeclaration> selected = [];

        foreach (RepositoryDependencyDeclaration declaration in declarations)
        {
            if (!SourceArtefactEcosystem.IsSupported(declaration.Ecosystem))
            {
                continue;
            }

            if (declaration.Ecosystem == DependencyEcosystem.JavaScript
                && declaration.PackageId.StartsWith("@types/", StringComparison.Ordinal))
            {
                continue;
            }

            if (declaration.PackageId.Contains("$(", StringComparison.Ordinal))
            {
                continue;
            }

            selected.Add(declaration);
        }

        return selected;
    }

    // §9: a reference identity accepted by more than one declaration of the same
    // ecosystem, after the §8 tie-breakers, produces no observation. Ambiguity is
    // never resolved by declaration order, manifest path, alphabetical order,
    // version, popularity, recency or any heuristic.
    private static RepositoryDependencyDeclaration? ResolveOwner(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations,
        DependencyEcosystem ecosystem,
        string identity)
    {
        RepositoryDependencyDeclaration? best = null;
        int bestScore = -1;
        bool tied = false;

        foreach (RepositoryDependencyDeclaration declaration in declarations)
        {
            if (declaration.Ecosystem != ecosystem
                || !SourceReferenceIdentityRules.Accepts(declaration, identity))
            {
                continue;
            }

            int score = SourceReferenceIdentityRules.TieBreakScore(declaration);

            if (score > bestScore)
            {
                best = declaration;
                bestScore = score;
                tied = false;
                continue;
            }

            if (score == bestScore && !ReferenceEquals(best, declaration))
            {
                tied = true;
            }
        }

        return tied ? null : best;
    }

    private sealed class ObservationBuilder
    {
        private readonly DependencyEcosystem ecosystem;
        private readonly RepositoryDependencyDeclaration declaration;
        private readonly HashSet<DependencySourceReferenceLocation> locations = [];
        private string identity = string.Empty;

        public ObservationBuilder(
            DependencyEcosystem ecosystem,
            RepositoryDependencyDeclaration declaration)
        {
            this.ecosystem = ecosystem;
            this.declaration = declaration;
        }

        public void Add(SourceImportCandidate candidate, string artefactPath)
        {
            // The resolved reference identity is the source-visible identity.
            // Several source specifiers may share one head, so the ordinally
            // smallest is kept to stay deterministic.
            string resolved = ecosystem == DependencyEcosystem.JavaScript
                ? SourceReferenceIdentityRules.SpecifierHead(candidate.Identity)
                : candidate.Identity;

            if (identity.Length == 0
                || string.CompareOrdinal(resolved, identity) < 0)
            {
                identity = resolved;
            }

            // Duplicate locations collapse.
            locations.Add(new DependencySourceReferenceLocation(
                artefactPath,
                candidate.Form,
                candidate.Line));
        }

        public DependencySourceReferenceEvidence Build() =>
            new(ecosystem, declaration, identity, locations);
    }
}

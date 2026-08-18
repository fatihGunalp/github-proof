using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis;

// The total, deterministic, case-insensitive extension-to-ecosystem map locked
// by docs/architecture/repository-dependency-source-reference-detection.md §4.1
// over the repository-source-evidence-collection.md §6 allow-list.
//
// This map is detection semantics and is owned by SourceReferenceRuleVersion.
// It must be revised whenever the collection allow-list changes.
public static class SourceArtefactEcosystem
{
    // §4: the supported V1 detection surface. Java, Php and Ruby artefacts are
    // mapped for completeness but never scanned, and they produce no
    // observation of either polarity.
    private static readonly IReadOnlySet<DependencyEcosystem> Supported =
        new HashSet<DependencyEcosystem>
        {
            DependencyEcosystem.DotNet,
            DependencyEcosystem.Go,
            DependencyEcosystem.JavaScript,
            DependencyEcosystem.Python,
            DependencyEcosystem.Rust
        };

    private static readonly IReadOnlyDictionary<string, DependencyEcosystem> ByExtension =
        new Dictionary<string, DependencyEcosystem>(StringComparer.Ordinal)
        {
            [".cs"] = DependencyEcosystem.DotNet,
            [".fs"] = DependencyEcosystem.DotNet,
            [".vb"] = DependencyEcosystem.DotNet,
            [".razor"] = DependencyEcosystem.DotNet,
            [".cshtml"] = DependencyEcosystem.DotNet,

            [".java"] = DependencyEcosystem.Java,
            [".kt"] = DependencyEcosystem.Java,
            [".kts"] = DependencyEcosystem.Java,
            [".scala"] = DependencyEcosystem.Java,
            [".groovy"] = DependencyEcosystem.Java,

            [".js"] = DependencyEcosystem.JavaScript,
            [".jsx"] = DependencyEcosystem.JavaScript,
            [".mjs"] = DependencyEcosystem.JavaScript,
            [".cjs"] = DependencyEcosystem.JavaScript,
            [".ts"] = DependencyEcosystem.JavaScript,
            [".tsx"] = DependencyEcosystem.JavaScript,
            [".d.ts"] = DependencyEcosystem.JavaScript,
            [".vue"] = DependencyEcosystem.JavaScript,
            [".svelte"] = DependencyEcosystem.JavaScript,

            [".py"] = DependencyEcosystem.Python,
            [".go"] = DependencyEcosystem.Go,
            [".rs"] = DependencyEcosystem.Rust,
            [".php"] = DependencyEcosystem.Php,
            [".rb"] = DependencyEcosystem.Ruby
        };

    public static bool IsSupported(DependencyEcosystem ecosystem) =>
        Supported.Contains(ecosystem);

    // Null when the artefact carries no allow-list extension. That is outside
    // the supported surface, never negative evidence.
    public static DependencyEcosystem? Resolve(string sourcePath)
    {
        string? extension = NormalizeExtension(sourcePath);

        return extension is not null
            && ByExtension.TryGetValue(extension, out DependencyEcosystem ecosystem)
                ? ecosystem
                : null;
    }

    // The repository-source-evidence-collection.md §6 rule, reused unchanged:
    // the final path segment is lowercased; a name ending in `.d.ts` normalizes
    // to `.d.ts`; otherwise the extension is the substring from the last dot.
    private static string? NormalizeExtension(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return null;
        }

        int separator = sourcePath.LastIndexOfAny(['/', '\\']);
        string segment = separator >= 0
            ? sourcePath[(separator + 1)..]
            : sourcePath;

        segment = segment.ToLowerInvariant();

        if (segment.EndsWith(".d.ts", StringComparison.Ordinal))
        {
            return ".d.ts";
        }

        int dot = segment.LastIndexOf('.');

        return dot > 0 ? segment[dot..] : null;
    }
}

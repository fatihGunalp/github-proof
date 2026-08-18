using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Domain.Analysis;

// The identity resolution rules locked by
// docs/architecture/repository-dependency-source-reference-detection.md §8.
//
// The declaration's PackageId is never rewritten. These rules only decide
// whether a declaration accepts a source-visible reference identity, and how a
// tie between two accepting declarations is broken.
//
// V1 uses no identity registry: every supported ecosystem resolves by rule.
public static class SourceReferenceIdentityRules
{
    public static bool Accepts(RepositoryDependencyDeclaration declaration, string identity)
    {
        ArgumentNullException.ThrowIfNull(declaration);

        if (string.IsNullOrWhiteSpace(identity))
        {
            return false;
        }

        return declaration.Ecosystem switch
        {
            DependencyEcosystem.Go => AcceptsGo(declaration.PackageId, identity),
            DependencyEcosystem.JavaScript => AcceptsJavaScript(declaration.PackageId, identity),
            DependencyEcosystem.Rust => AcceptsRust(declaration.PackageId, identity),
            DependencyEcosystem.Python => AcceptsPython(declaration.PackageId, identity),
            DependencyEcosystem.DotNet => AcceptsDotNet(declaration.PackageId, identity),
            _ => false
        };
    }

    // §8.1: a repository may contain nested Go modules, so one import path can
    // be a valid prefix match for several declared module paths. The longest
    // matching module path wins, which follows Go module semantics in which the
    // more specific module owns the package. This is the only Go tie-breaker.
    public static int TieBreakScore(RepositoryDependencyDeclaration declaration) =>
        declaration.Ecosystem == DependencyEcosystem.Go
            ? declaration.PackageId.Length
            : 0;

    // §8: equal to the import path, or a `/` segment prefix of it.
    private static bool AcceptsGo(string modulePath, string importPath)
    {
        if (string.Equals(modulePath, importPath, StringComparison.Ordinal))
        {
            return true;
        }

        return importPath.Length > modulePath.Length
            && importPath.StartsWith(modulePath, StringComparison.Ordinal)
            && importPath[modulePath.Length] == '/';
    }

    // §8: the specifier head — `pkg`, or `@scope/pkg` for a scoped package —
    // must equal the declared npm name exactly. Any subpath is discarded.
    private static bool AcceptsJavaScript(string packageId, string specifier) =>
        string.Equals(packageId, SpecifierHead(specifier), StringComparison.Ordinal);

    public static string SpecifierHead(string specifier)
    {
        if (specifier.StartsWith('@'))
        {
            int first = specifier.IndexOf('/');

            if (first < 0)
            {
                return specifier;
            }

            int second = specifier.IndexOf('/', first + 1);

            return second < 0 ? specifier : specifier[..second];
        }

        int slash = specifier.IndexOf('/');

        return slash < 0 ? specifier : specifier[..slash];
    }

    // §8: the declared crate name with every `-` replaced by `_` must equal the
    // first `use` or `extern crate` path segment.
    private static bool AcceptsRust(string crateName, string segment) =>
        string.Equals(crateName.Replace('-', '_'), segment, StringComparison.Ordinal);

    // §8: the top-level module segment, lowercased, must equal the declared
    // distribution name under PEP 503 normalization, or that name with `-` and
    // `.` replaced by `_`.
    private static bool AcceptsPython(string distribution, string module)
    {
        string candidate = module.ToLowerInvariant();
        string normalized = DependencyIdentityNormalizer.Normalize(
            DependencyEcosystem.Python,
            distribution);

        return string.Equals(candidate, normalized, StringComparison.Ordinal)
            || string.Equals(candidate, normalized.Replace('-', '_'), StringComparison.Ordinal);
    }

    // §8.2: exact, case-insensitive equality between the declared package id and
    // the namespace, or the namespace being the package id followed by `.` and a
    // further segment. Reverse-prefix matching is prohibited, because it made
    // one namespace acceptable to both a package and its own extension package.
    private static bool AcceptsDotNet(string packageId, string ns)
    {
        if (string.Equals(packageId, ns, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ns.Length > packageId.Length
            && ns.StartsWith(packageId, StringComparison.OrdinalIgnoreCase)
            && ns[packageId.Length] == '.';
    }
}

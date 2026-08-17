using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis;

// Lookup normalization for registry matching only. The source
// DependencyDeclaration.PackageId is never rewritten, and normalization never
// collapses distinct package distributions or generations.
public static class DependencyIdentityNormalizer
{
    public static string Normalize(
        DependencyEcosystem ecosystem,
        string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                nameof(packageId));
        }

        string identity = packageId.Trim();

        return ecosystem switch
        {
            DependencyEcosystem.DotNet => identity.ToLowerInvariant(),
            DependencyEcosystem.JavaScript => identity.ToLowerInvariant(),
            DependencyEcosystem.Php => identity.ToLowerInvariant(),
            DependencyEcosystem.Python => NormalizePythonDistribution(identity),

            // Java, Go, Rust and Ruby identities are case-sensitive, and a Go
            // module path carries its major-version element. Equivalence is
            // expressed by explicit registry aliases, never by normalization.
            _ => identity
        };
    }

    private static string NormalizePythonDistribution(string identity)
    {
        System.Text.StringBuilder normalized = new(identity.Length);
        bool previousSeparator = false;

        foreach (char current in identity.ToLowerInvariant())
        {
            if (current is '-' or '_' or '.')
            {
                if (!previousSeparator)
                {
                    normalized.Append('-');
                    previousSeparator = true;
                }

                continue;
            }

            normalized.Append(current);
            previousSeparator = false;
        }

        return normalized.ToString();
    }
}

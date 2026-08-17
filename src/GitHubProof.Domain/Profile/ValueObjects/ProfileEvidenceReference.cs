namespace GitHubProof.Domain.Profile.ValueObjects;

// Canonical traceability pointer from profile-level evidence to the exact
// repository-level evidence that supports it. EvidenceKey names the observed
// repository evidence source: a canonical signal key, or the normalized
// snapshot data key for non-signal evidence such as languages.
public sealed class ProfileEvidenceReference
{
    public ProfileEvidenceReference(
        ProfileRepositoryReference repository,
        RepositoryAnalysisReference analysis,
        string evidenceKey,
        IEnumerable<string>? evidenceKeys)
    {
        Repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        Analysis = analysis
            ?? throw new ArgumentNullException(nameof(analysis));
        EvidenceKey = ProfileValue.RequireValue(evidenceKey, nameof(evidenceKey));
        EvidenceKeys = NormalizeEvidenceKeys(evidenceKeys);
    }

    public ProfileRepositoryReference Repository { get; }

    public RepositoryAnalysisReference Analysis { get; }

    public string EvidenceKey { get; }

    public IReadOnlyList<string> EvidenceKeys { get; }

    private static IReadOnlyList<string> NormalizeEvidenceKeys(
        IEnumerable<string>? evidenceKeys)
    {
        if (evidenceKeys is null)
        {
            return Array.Empty<string>();
        }

        return evidenceKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

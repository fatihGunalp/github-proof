namespace GitHubProof.Domain.Profile.ValueObjects;

// Canonical traceability pointer from profile-level evidence to the exact
// repository-level signal evidence that supports it.
public sealed class ProfileEvidenceReference
{
    public ProfileEvidenceReference(
        ProfileRepositoryReference repository,
        RepositoryAnalysisReference analysis,
        string signalKey,
        IEnumerable<string>? evidenceKeys)
    {
        Repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        Analysis = analysis
            ?? throw new ArgumentNullException(nameof(analysis));
        SignalKey = ProfileValue.RequireValue(signalKey, nameof(signalKey));
        EvidenceKeys = NormalizeEvidenceKeys(evidenceKeys);
    }

    public ProfileRepositoryReference Repository { get; }

    public RepositoryAnalysisReference Analysis { get; }

    public string SignalKey { get; }

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

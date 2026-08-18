using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record RepositorySnapshot
{
    public RepositorySnapshot(
        RepositoryIdentity repository,
        DateTimeOffset capturedAt,
        string? headCommitSha,
        RepositoryMetadata? metadata,
        RepositoryReadme? readme,
        RepositoryCollection<RepositoryFile>? fileTree,
        RepositoryCollection<RepositoryCommit>? commits,
        RepositoryCollection<RepositoryIssue>? issues,
        RepositoryCollection<RepositoryPullRequest>? pullRequests,
        RepositoryCollection<RepositoryRelease>? releases,
        RepositoryCollection<RepositoryFile>? workflows,
        IEnumerable<string>? topics,
        RepositoryLicense? license,
        IReadOnlyDictionary<string, long>? languages,
        RepositoryCollection<RepositoryContributor>? contributors,
        IEnumerable<DataAvailability>? availability,
        IEnumerable<string>? collectionWarnings,
        RepositoryCollection<RepositoryTag>? tags = null,
        RepositoryFile? licenseFile = null,
        RepositoryCollection<RepositoryFile>? documentationMarkdown = null,
        RepositoryCollection<RepositoryDependencyDeclaration>? dependencyDeclarations = null,
        RepositoryCollection<RepositorySourceFile>? sourceFiles = null)
    {
        Repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        CapturedAt = capturedAt.ToUniversalTime();
        HeadCommitSha = SnapshotValue.NormalizeOptional(headCommitSha);
        Metadata = metadata;
        Readme = readme;
        FileTree = fileTree ?? RepositoryCollection<RepositoryFile>.Empty;
        Commits = commits ?? RepositoryCollection<RepositoryCommit>.Empty;
        Issues = issues ?? RepositoryCollection<RepositoryIssue>.Empty;
        PullRequests = pullRequests
            ?? RepositoryCollection<RepositoryPullRequest>.Empty;
        Releases = releases ?? RepositoryCollection<RepositoryRelease>.Empty;
        Tags = tags ?? RepositoryCollection<RepositoryTag>.Empty;
        Workflows = workflows ?? RepositoryCollection<RepositoryFile>.Empty;
        Topics = NormalizeTopics(topics);
        License = license;
        LicenseFile = licenseFile;
        DocumentationMarkdown = documentationMarkdown
            ?? new RepositoryCollection<RepositoryFile>(isComplete: false);
        Languages = NormalizeLanguages(languages);
        DependencyDeclarations = NormalizeDependencyDeclarations(dependencyDeclarations);
        SourceFiles = NormalizeSourceFiles(sourceFiles);
        Contributors = contributors
            ?? RepositoryCollection<RepositoryContributor>.Empty;
        Availability = NormalizeAvailability(availability);
        CollectionWarnings = NormalizeWarnings(collectionWarnings);
    }

    public RepositoryIdentity Repository { get; }

    public DateTimeOffset CapturedAt { get; }

    public string? HeadCommitSha { get; }

    public RepositoryMetadata? Metadata { get; }

    public RepositoryReadme? Readme { get; }

    public RepositoryCollection<RepositoryFile> FileTree { get; }

    public RepositoryCollection<RepositoryCommit> Commits { get; }

    public RepositoryCollection<RepositoryIssue> Issues { get; }

    public RepositoryCollection<RepositoryPullRequest> PullRequests { get; }

    public RepositoryCollection<RepositoryRelease> Releases { get; }

    public RepositoryCollection<RepositoryTag> Tags { get; }

    public RepositoryCollection<RepositoryFile> Workflows { get; }

    public IReadOnlyList<string> Topics { get; }

    public RepositoryLicense? License { get; }

    public RepositoryFile? LicenseFile { get; }

    public RepositoryCollection<RepositoryFile> DocumentationMarkdown { get; }

    public IReadOnlyDictionary<string, long> Languages { get; }

    public RepositoryCollection<RepositoryContributor> Contributors { get; }

    // Non-scoring dependency declaration evidence. It participates in no
    // signal, category or score.
    public RepositoryCollection<RepositoryDependencyDeclaration> DependencyDeclarations { get; }

    // Non-scoring bounded raw source evidence. It participates in no signal,
    // category or score, and it is never a dependency source reference.
    public RepositoryCollection<RepositorySourceFile> SourceFiles { get; }

    public IReadOnlyList<DataAvailability> Availability { get; }

    public IReadOnlyList<string> CollectionWarnings { get; }

    public DataAvailability? FindAvailability(string dataKey)
    {
        string normalizedKey = SnapshotValue.RequireValue(
            dataKey,
            nameof(dataKey));

        return Availability.SingleOrDefault(item => string.Equals(
            item.DataKey,
            normalizedKey,
            StringComparison.Ordinal));
    }

    // Deterministic ordering by manifest path, package identity and declared
    // version literal; identical triples collapse to one declaration.
    private static RepositoryCollection<RepositoryDependencyDeclaration> NormalizeDependencyDeclarations(
        RepositoryCollection<RepositoryDependencyDeclaration>? declarations)
    {
        if (declarations is null)
        {
            return RepositoryCollection<RepositoryDependencyDeclaration>.Empty;
        }

        RepositoryDependencyDeclaration[] normalized = declarations.Items
            .Distinct()
            .OrderBy(declaration => declaration.ManifestPath, StringComparer.Ordinal)
            .ThenBy(declaration => declaration.PackageId, StringComparer.Ordinal)
            .ThenBy(
                declaration => declaration.DeclaredVersion ?? string.Empty,
                StringComparer.Ordinal)
            .ToArray();

        return new RepositoryCollection<RepositoryDependencyDeclaration>(
            normalized,
            declarations.IsComplete ? normalized.Length : declarations.TotalCount,
            declarations.IsComplete);
    }

    // Deterministic ordering by path, Ordinal ascending; identical paths
    // collapse to one artefact.
    private static RepositoryCollection<RepositorySourceFile> NormalizeSourceFiles(
        RepositoryCollection<RepositorySourceFile>? sourceFiles)
    {
        if (sourceFiles is null)
        {
            return RepositoryCollection<RepositorySourceFile>.Empty;
        }

        RepositorySourceFile[] normalized = sourceFiles.Items
            .GroupBy(file => file.Path, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .ToArray();

        return new RepositoryCollection<RepositorySourceFile>(
            normalized,
            sourceFiles.IsComplete ? normalized.Length : sourceFiles.TotalCount,
            sourceFiles.IsComplete);
    }

    private static IReadOnlyList<string> NormalizeTopics(
        IEnumerable<string>? topics)
    {
        if (topics is null)
        {
            return Array.Empty<string>();
        }

        return topics
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => topic.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, long> NormalizeLanguages(
        IReadOnlyDictionary<string, long>? languages)
    {
        if (languages is null)
        {
            return new ReadOnlyDictionary<string, long>(
                new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase));
        }

        Dictionary<string, long> normalized = new(
            StringComparer.OrdinalIgnoreCase);

        foreach ((string language, long bytes) in languages)
        {
            string normalizedLanguage = SnapshotValue.RequireValue(
                language,
                nameof(languages));

            if (bytes < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(languages),
                    bytes,
                    "Language byte count cannot be negative.");
            }

            if (!normalized.TryAdd(normalizedLanguage, bytes))
            {
                throw new ArgumentException(
                    $"Duplicate language '{normalizedLanguage}'.",
                    nameof(languages));
            }
        }

        return new ReadOnlyDictionary<string, long>(normalized);
    }

    private static IReadOnlyList<DataAvailability> NormalizeAvailability(
        IEnumerable<DataAvailability>? availability)
    {
        if (availability is null)
        {
            return Array.Empty<DataAvailability>();
        }

        List<DataAvailability> normalized = [];
        HashSet<string> keys = new(StringComparer.Ordinal);

        foreach (DataAvailability item in availability)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!keys.Add(item.DataKey))
            {
                throw new ArgumentException(
                    $"Duplicate availability key '{item.DataKey}'.",
                    nameof(availability));
            }

            normalized.Add(item);
        }

        return normalized.AsReadOnly();
    }

    private static IReadOnlyList<string> NormalizeWarnings(
        IEnumerable<string>? warnings)
    {
        if (warnings is null)
        {
            return Array.Empty<string>();
        }

        return warnings
            .Where(warning => !string.IsNullOrWhiteSpace(warning))
            .Select(warning => warning.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

public sealed record RepositoryMetadata
{
    public RepositoryMetadata(
        string? description,
        string? homepage,
        DateTimeOffset createdAt,
        bool issuesEnabled)
    {
        Description = SnapshotValue.NormalizeOptional(description);
        Homepage = SnapshotValue.NormalizeOptional(homepage);
        CreatedAt = createdAt.ToUniversalTime();
        IssuesEnabled = issuesEnabled;
    }

    public string? Description { get; }

    public string? Homepage { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool IssuesEnabled { get; }
}

public sealed record RepositoryReadme
{
    public RepositoryReadme(string path, string content)
    {
        Path = SnapshotValue.RequireValue(path, nameof(path));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public string Path { get; }

    public string Content { get; }
}

public sealed record RepositoryFile
{
    public RepositoryFile(
        string path,
        bool isDirectory,
        long? size,
        string? content)
    {
        Path = SnapshotValue.RequireValue(path, nameof(path));
        IsDirectory = isDirectory;

        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                size,
                "File size cannot be negative.");
        }

        Size = size;
        Content = content;
    }

    public string Path { get; }

    public bool IsDirectory { get; }

    public long? Size { get; }

    public string? Content { get; }
}

public sealed record RepositoryCommit
{
    public RepositoryCommit(
        string sha,
        DateTimeOffset committedAt,
        string? message,
        string? authorLogin)
    {
        Sha = SnapshotValue.RequireValue(sha, nameof(sha));
        CommittedAt = committedAt.ToUniversalTime();
        Message = SnapshotValue.NormalizeOptional(message);
        AuthorLogin = SnapshotValue.NormalizeOptional(authorLogin);
    }

    public string Sha { get; }

    public DateTimeOffset CommittedAt { get; }

    public string? Message { get; }

    public string? AuthorLogin { get; }
}

public sealed record RepositoryIssue
{
    public RepositoryIssue(
        int number,
        string title,
        string? body,
        bool isClosed,
        DateTimeOffset createdAt)
    {
        Number = SnapshotValue.RequirePositive(number, nameof(number));
        Title = SnapshotValue.RequireValue(title, nameof(title));
        Body = SnapshotValue.NormalizeOptional(body);
        IsClosed = isClosed;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public int Number { get; }

    public string Title { get; }

    public string? Body { get; }

    public bool IsClosed { get; }

    public DateTimeOffset CreatedAt { get; }
}

public sealed record RepositoryPullRequest
{
    public RepositoryPullRequest(
        int number,
        string title,
        bool isClosed,
        bool isMerged,
        DateTimeOffset createdAt)
    {
        Number = SnapshotValue.RequirePositive(number, nameof(number));
        Title = SnapshotValue.RequireValue(title, nameof(title));
        IsClosed = isClosed;
        IsMerged = isMerged;
        CreatedAt = createdAt.ToUniversalTime();

        if (isMerged && !isClosed)
        {
            throw new ArgumentException(
                "A merged pull request must be closed.",
                nameof(isClosed));
        }
    }

    public int Number { get; }

    public string Title { get; }

    public bool IsClosed { get; }

    public bool IsMerged { get; }

    public DateTimeOffset CreatedAt { get; }
}

public sealed record RepositoryRelease
{
    public RepositoryRelease(
        string tagName,
        bool hasPublishedRelease,
        bool isDraft,
        string? name,
        string? notes,
        DateTimeOffset? publishedAt)
    {
        TagName = SnapshotValue.RequireValue(tagName, nameof(tagName));
        HasPublishedRelease = hasPublishedRelease;
        IsDraft = isDraft;
        Name = SnapshotValue.NormalizeOptional(name);
        Notes = SnapshotValue.NormalizeOptional(notes);
        PublishedAt = publishedAt?.ToUniversalTime();

        if (hasPublishedRelease && isDraft)
        {
            throw new ArgumentException(
                "A draft release cannot be published.",
                nameof(hasPublishedRelease));
        }
    }

    public string TagName { get; }

    public bool HasPublishedRelease { get; }

    public bool IsDraft { get; }

    public string? Name { get; }

    public string? Notes { get; }

    public DateTimeOffset? PublishedAt { get; }
}

public sealed record RepositoryTag
{
    public RepositoryTag(string name)
    {
        Name = SnapshotValue.RequireValue(name, nameof(name));
    }

    public string Name { get; }
}

public sealed record RepositoryLicense
{
    public RepositoryLicense(
        string key,
        string name,
        string? spdxIdentifier,
        string? path)
    {
        Key = SnapshotValue.RequireValue(key, nameof(key));
        Name = SnapshotValue.RequireValue(name, nameof(name));
        SpdxIdentifier = SnapshotValue.NormalizeOptional(spdxIdentifier);
        Path = SnapshotValue.NormalizeOptional(path);
    }

    public string Key { get; }

    public string Name { get; }

    public string? SpdxIdentifier { get; }

    public string? Path { get; }
}

// One dependency declaration observed in a repository manifest. It records what
// a manifest declares; it never states that the dependency is used, that work
// was produced with it, or that any problem area is proven.
public sealed record RepositoryDependencyDeclaration
{
    public RepositoryDependencyDeclaration(
        DependencyEcosystem ecosystem,
        string manifestPath,
        string packageId,
        string? declaredVersion)
    {
        if (!Enum.IsDefined(ecosystem))
        {
            throw new ArgumentOutOfRangeException(
                nameof(ecosystem),
                ecosystem,
                "Dependency ecosystem is not defined.");
        }

        Ecosystem = ecosystem;
        ManifestPath = SnapshotValue.RequireValue(manifestPath, nameof(manifestPath));
        PackageId = SnapshotValue.RequireValue(packageId, nameof(packageId));
        DeclaredVersion = SnapshotValue.NormalizeOptional(declaredVersion);
    }

    public DependencyEcosystem Ecosystem { get; }

    public string ManifestPath { get; }

    public string PackageId { get; }

    public string? DeclaredVersion { get; }
}

// One bounded raw source artefact collected from a supported regular repository
// source blob at the pinned commit. It records what the supported V1 source
// surface contained; it never states that a dependency is referenced, that work
// was produced, or that any problem area is proven.
public sealed record RepositorySourceFile
{
    public RepositorySourceFile(string path, string content)
    {
        Path = SnapshotValue.RequireValue(path, nameof(path));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public string Path { get; }

    public string Content { get; }
}

public sealed record RepositoryContributor
{
    public RepositoryContributor(string login, int contributions)
    {
        Login = SnapshotValue.RequireValue(login, nameof(login));

        if (contributions < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contributions),
                contributions,
                "Contribution count cannot be negative.");
        }

        Contributions = contributions;
    }

    public string Login { get; }

    public int Contributions { get; }
}

public sealed class RepositoryCollection<T>
{
    public RepositoryCollection(
        IEnumerable<T>? items = null,
        int? totalCount = null,
        bool isComplete = true)
    {
        List<T> normalizedItems = items?.ToList() ?? [];

        if (normalizedItems.Any(item => item is null))
        {
            throw new ArgumentException(
                "Collection items cannot contain null values.",
                nameof(items));
        }

        if (totalCount < normalizedItems.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalCount),
                totalCount,
                "Total count cannot be less than the collected item count.");
        }

        Items = new ReadOnlyCollection<T>(normalizedItems);
        TotalCount = totalCount ?? (isComplete ? normalizedItems.Count : null);
        IsComplete = isComplete;
    }

    public IReadOnlyList<T> Items { get; }

    public int? TotalCount { get; }

    public bool IsComplete { get; }

    public static RepositoryCollection<T> Empty { get; } = new();
}

internal static class SnapshotValue
{
    public static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }

    public static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public static int RequirePositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than zero.");
        }

        return value;
    }
}

using System.Collections.ObjectModel;

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

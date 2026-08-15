using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositorySnapshotOrchestrator
{
    private const string SnapshotConsistencyWarning =
        "Repository sections other than default-branch contents are collected from live GitHub state and may not be atomic with HeadCommitSha.";

    private readonly IGitHubRepositoryMetadataCollector metadataCollector;
    private readonly IGitHubRepositoryContentsCollector contentsCollector;
    private readonly IGitHubRepositoryDescriptiveDataCollector descriptiveDataCollector;
    private readonly IGitHubRepositoryCommitCollector commitCollector;
    private readonly IGitHubRepositoryIssueCollector issueCollector;
    private readonly IGitHubRepositoryPullRequestCollector pullRequestCollector;
    private readonly IGitHubRepositoryReleaseCollector releaseCollector;
    private readonly IGitHubRepositoryTagCollector tagCollector;
    private readonly IGitHubRepositoryContributorCollector contributorCollector;
    private readonly TimeProvider timeProvider;

    public GitHubRepositorySnapshotOrchestrator(
        IGitHubRepositoryMetadataCollector metadataCollector,
        IGitHubRepositoryContentsCollector contentsCollector,
        IGitHubRepositoryDescriptiveDataCollector descriptiveDataCollector,
        IGitHubRepositoryCommitCollector commitCollector,
        IGitHubRepositoryIssueCollector issueCollector,
        IGitHubRepositoryPullRequestCollector pullRequestCollector,
        IGitHubRepositoryReleaseCollector releaseCollector,
        IGitHubRepositoryTagCollector tagCollector,
        IGitHubRepositoryContributorCollector contributorCollector,
        TimeProvider? timeProvider = null)
    {
        this.metadataCollector = metadataCollector
            ?? throw new ArgumentNullException(nameof(metadataCollector));
        this.contentsCollector = contentsCollector
            ?? throw new ArgumentNullException(nameof(contentsCollector));
        this.descriptiveDataCollector = descriptiveDataCollector
            ?? throw new ArgumentNullException(nameof(descriptiveDataCollector));
        this.commitCollector = commitCollector
            ?? throw new ArgumentNullException(nameof(commitCollector));
        this.issueCollector = issueCollector
            ?? throw new ArgumentNullException(nameof(issueCollector));
        this.pullRequestCollector = pullRequestCollector
            ?? throw new ArgumentNullException(nameof(pullRequestCollector));
        this.releaseCollector = releaseCollector
            ?? throw new ArgumentNullException(nameof(releaseCollector));
        this.tagCollector = tagCollector
            ?? throw new ArgumentNullException(nameof(tagCollector));
        this.contributorCollector = contributorCollector
            ?? throw new ArgumentNullException(nameof(contributorCollector));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositorySnapshotResult> CollectAsync(
        string repositoryUrl,
        CancellationToken cancellationToken = default)
    {
        GitHubRepositoryUrl repository = GitHubRepositoryUrl.Parse(repositoryUrl);
        GitHubRepositoryMetadataCollection metadata = await metadataCollector.CollectAsync(
            repository,
            cancellationToken);

        if (!metadata.IsAvailable)
        {
            return new GitHubRepositorySnapshotResult(
                snapshot: null,
                metadata.Availability);
        }

        if (metadata.Repository is null || metadata.Metadata is null)
        {
            throw new InvalidOperationException(
                "Available repository metadata must include repository identity and metadata.");
        }

        DateTimeOffset capturedAt = timeProvider.GetUtcNow();
        GitHubRepositoryContentsCollection contents = await contentsCollector.CollectAsync(
            repository,
            metadata,
            cancellationToken);
        GitHubRepositoryDescriptiveDataCollection descriptive =
            await descriptiveDataCollector.CollectAsync(
                repository,
                metadata,
                cancellationToken);
        GitHubRepositoryCommitCollection commits = await commitCollector.CollectAsync(
            repository,
            metadata,
            cancellationToken);
        GitHubRepositoryIssueCollection issues = await issueCollector.CollectAsync(
            repository,
            metadata,
            cancellationToken);
        GitHubRepositoryPullRequestCollection pullRequests =
            await pullRequestCollector.CollectAsync(
                repository,
                metadata,
                cancellationToken);
        GitHubRepositoryReleaseCollection releases = await releaseCollector.CollectAsync(
            repository,
            metadata,
            cancellationToken);
        GitHubRepositoryTagCollection tags = await tagCollector.CollectAsync(
            repository,
            metadata,
            cancellationToken);
        GitHubRepositoryContributorCollection contributors =
            await contributorCollector.CollectAsync(
                repository,
                metadata,
                cancellationToken);

        RepositorySnapshot snapshot = new(
            metadata.Repository,
            capturedAt,
            contents.HeadCommitSha,
            metadata.Metadata,
            descriptive.Readme,
            MergeFileTree(contents, descriptive.LicenseFile),
            commits.Commits,
            MapIssues(issues.Issues),
            MapPullRequests(pullRequests.PullRequests),
            releases.Releases,
            contents.Workflows,
            metadata.Topics,
            descriptive.License,
            descriptive.Languages,
            contributors.Contributors,
            CollectAvailability(
                metadata,
                contents,
                descriptive,
                commits,
                issues,
                pullRequests,
                releases,
                tags,
                contributors),
            CollectWarnings(
                contents,
                commits,
                issues,
                pullRequests,
                releases,
                tags,
                contributors),
            MapTags(tags.Tags));

        return new GitHubRepositorySnapshotResult(snapshot, metadata.Availability);
    }

    private static RepositoryCollection<RepositoryFile> MergeFileTree(
        GitHubRepositoryContentsCollection contents,
        RepositoryFile? licenseFile)
    {
        Dictionary<string, RepositoryFile> overlays = contents.ProjectFiles.Items
            .ToDictionary(file => file.Path, StringComparer.Ordinal);

        if (licenseFile is not null)
        {
            overlays.TryAdd(licenseFile.Path, licenseFile);
        }

        List<RepositoryFile> files = [];
        HashSet<string> observedPaths = new(StringComparer.Ordinal);

        foreach (RepositoryFile file in contents.FileTree.Items)
        {
            files.Add(overlays.TryGetValue(file.Path, out RepositoryFile? overlay)
                ? overlay
                : file);
            observedPaths.Add(file.Path);
        }

        foreach (RepositoryFile overlay in overlays.Values)
        {
            if (observedPaths.Add(overlay.Path))
            {
                files.Add(overlay);
            }
        }

        int? totalCount = contents.FileTree.IsComplete
            ? Math.Max(contents.FileTree.TotalCount ?? 0, files.Count)
            : contents.FileTree.TotalCount;

        return new RepositoryCollection<RepositoryFile>(
            files,
            totalCount,
            contents.FileTree.IsComplete);
    }

    private static RepositoryCollection<RepositoryIssue> MapIssues(
        RepositoryCollection<GitHubRepositoryIssue> issues) =>
        MapCollection(
            issues,
            issue => new RepositoryIssue(
                issue.Number,
                issue.Title,
                issue.Body,
                issue.IsClosed,
                issue.CreatedAt));

    private static RepositoryCollection<RepositoryPullRequest> MapPullRequests(
        RepositoryCollection<GitHubRepositoryPullRequest> pullRequests) =>
        MapCollection(
            pullRequests,
            pullRequest => new RepositoryPullRequest(
                pullRequest.Number,
                pullRequest.Title,
                pullRequest.IsClosed,
                pullRequest.IsMerged,
                pullRequest.CreatedAt));

    private static RepositoryCollection<RepositoryTag> MapTags(
        RepositoryCollection<GitHubRepositoryTag> tags) =>
        MapCollection(tags, tag => new RepositoryTag(tag.Name));

    private static RepositoryCollection<TTarget> MapCollection<TSource, TTarget>(
        RepositoryCollection<TSource> source,
        Func<TSource, TTarget> map) =>
        new(
            source.Items.Select(map),
            source.TotalCount,
            source.IsComplete);

    private static IReadOnlyList<DataAvailability> CollectAvailability(
        GitHubRepositoryMetadataCollection metadata,
        GitHubRepositoryContentsCollection contents,
        GitHubRepositoryDescriptiveDataCollection descriptive,
        GitHubRepositoryCommitCollection commits,
        GitHubRepositoryIssueCollection issues,
        GitHubRepositoryPullRequestCollection pullRequests,
        GitHubRepositoryReleaseCollection releases,
        GitHubRepositoryTagCollection tags,
        GitHubRepositoryContributorCollection contributors) =>
        [
            metadata.Availability,
            contents.FileTreeAvailability,
            contents.ProjectFilesAvailability,
            contents.WorkflowsAvailability,
            .. descriptive.Availability,
            commits.Availability,
            issues.Availability,
            pullRequests.Availability,
            releases.Availability,
            tags.Availability,
            contributors.Availability
        ];

    private static IReadOnlyList<string> CollectWarnings(
        GitHubRepositoryContentsCollection contents,
        GitHubRepositoryCommitCollection commits,
        GitHubRepositoryIssueCollection issues,
        GitHubRepositoryPullRequestCollection pullRequests,
        GitHubRepositoryReleaseCollection releases,
        GitHubRepositoryTagCollection tags,
        GitHubRepositoryContributorCollection contributors)
    {
        List<string> warnings =
        [
            .. contents.CollectionWarnings,
            .. commits.CollectionWarnings,
            .. issues.CollectionWarnings,
            .. pullRequests.CollectionWarnings,
            .. releases.CollectionWarnings,
            .. tags.CollectionWarnings,
            .. contributors.CollectionWarnings
        ];

        if (contents.HeadCommitSha is not null)
        {
            warnings.Add(SnapshotConsistencyWarning);
        }

        return warnings;
    }
}

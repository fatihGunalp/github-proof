using GitHubProof.Domain.Analysis.Collection;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositorySnapshotOrchestrator
    : IGitHubRepositorySnapshotCollector
{
    private const string SnapshotConsistencyWarning =
        "Repository sections other than default-branch contents are collected from live GitHub state and may not be atomic with HeadCommitSha.";

    public static GitHubRepositorySnapshotOrchestrator CreateDefault(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        return new GitHubRepositorySnapshotOrchestrator(
            new GitHubRepositoryMetadataCollector(httpClient),
            new GitHubRepositoryContentsCollector(httpClient),
            new GitHubRepositoryDescriptiveDataCollector(httpClient),
            new GitHubRepositoryCommitCollector(httpClient),
            new GitHubRepositoryIssueCollector(httpClient),
            new GitHubRepositoryPullRequestCollector(httpClient),
            new GitHubRepositoryReleaseCollector(httpClient),
            new GitHubRepositoryTagCollector(httpClient),
            new GitHubRepositoryContributorCollector(httpClient),
            new GitHubRepositoryDependencyManifestCollector(httpClient, timeProvider),
            new GitHubRepositorySourceFileCollector(httpClient, timeProvider),
            timeProvider);
    }

    // The Profile V1 composition contracted by
    // docs/architecture/public-profile-runtime-collection.md §3.2 and §5: the
    // same repository snapshot assembly with the three omitted surfaces left
    // out — SourceFiles, DependencyDeclarations and Contributors — so a Profile
    // Proof run issues no source-tree, source-content, dependency-manifest or
    // contributor request at all.
    //
    // Nothing else about the collected result differs for the surfaces that
    // stay: every one the canonical 15 signals, the eligibility contract and
    // CrossRepositoryEvidenceComposer actually read is still collected by the
    // same collector, in the same order, from the same HttpClient.
    //
    // Session 099 adds two things to this composition and to nothing else:
    // Contributors is omitted (contract §5, no production consumer), and the
    // five paginated surfaces are given their canonical decision boundaries
    // (§15) so they may stop once the authorized V1 decision is already
    // irreversible. The boundaries come from the Domain evaluators that own the
    // thresholds; this composition restates none of them.
    //
    // CreateDefault stays the Repository Proof composition, still builds the
    // full collector set and supplies no boundary at all, so it keeps
    // enumerating fully.
    //
    // Session 125 (S2) adds one optional parameter and nothing else. When
    // `languagesShadow` is null — the default, and what every existing caller
    // passes — the construction below is exactly the Session 099 composition,
    // collector for collector. Only a non-null shadow wraps the descriptive
    // collector, and even then the wrapper returns that collector's result
    // unchanged: REST remains authoritative for the languages surface (§18.1).
    public static GitHubRepositorySnapshotOrchestrator CreateProfileComposition(
        HttpClient httpClient,
        TimeProvider? timeProvider = null) =>
        CreateProfileComposition(httpClient, timeProvider, languagesShadow: null);

    // The S2 overload. It is a separate overload rather than a third optional
    // parameter so the existing two-argument signature stays byte-identical for
    // every current caller, including method-group conversions.
    public static GitHubRepositorySnapshotOrchestrator CreateProfileComposition(
        HttpClient httpClient,
        TimeProvider? timeProvider,
        IGitHubGraphQlLanguagesShadow? languagesShadow)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        IGitHubRepositoryDescriptiveDataCollector descriptiveDataCollector =
            new GitHubRepositoryDescriptiveDataCollector(httpClient);

        if (languagesShadow is not null)
        {
            descriptiveDataCollector = new ShadowedDescriptiveDataCollector(
                descriptiveDataCollector,
                languagesShadow);
        }

        return new GitHubRepositorySnapshotOrchestrator(
            new GitHubRepositoryMetadataCollector(httpClient),
            new GitHubRepositoryContentsCollector(httpClient),
            descriptiveDataCollector,
            new GitHubRepositoryCommitCollector(
                httpClient,
                timeProvider,
                RepositoryCollectionDecisionBoundary.AreCommitDecisionsIrreversible),
            new GitHubRepositoryIssueCollector(
                httpClient,
                timeProvider,
                RepositoryCollectionDecisionBoundary.IsIssueDecisionIrreversible),
            new GitHubRepositoryPullRequestCollector(
                httpClient,
                timeProvider,
                RepositoryCollectionDecisionBoundary.IsPullRequestDecisionIrreversible),
            new GitHubRepositoryReleaseCollector(
                httpClient,
                timeProvider,
                RepositoryCollectionDecisionBoundary.IsReleaseDecisionIrreversible),
            new GitHubRepositoryTagCollector(
                httpClient,
                timeProvider,
                RepositoryCollectionDecisionBoundary.IsTagDecisionIrreversible),
            timeProvider);
    }

    private readonly IGitHubRepositoryMetadataCollector metadataCollector;
    private readonly IGitHubRepositoryContentsCollector contentsCollector;
    private readonly IGitHubRepositoryDescriptiveDataCollector descriptiveDataCollector;
    private readonly IGitHubRepositoryCommitCollector commitCollector;
    private readonly IGitHubRepositoryIssueCollector issueCollector;
    private readonly IGitHubRepositoryPullRequestCollector pullRequestCollector;
    private readonly IGitHubRepositoryReleaseCollector releaseCollector;
    private readonly IGitHubRepositoryTagCollector tagCollector;
    // Null exactly when this composition omits the surface entirely (the
    // Profile V1 composition), for the same reason and with the same
    // absent-not-failed meaning as the two enrichment collectors below.
    private readonly IGitHubRepositoryContributorCollector? contributorCollector;
    // Null exactly when this composition omits the surface entirely (the
    // Profile V1 composition). Null never means "collection failed": the
    // omitted surface contributes no availability entry, no declaration, no
    // source file and no warning, which is the absent-surface state the
    // existing Application isolation tests already prove is analysis-neutral.
    private readonly IGitHubRepositoryDependencyManifestCollector? dependencyManifestCollector;
    private readonly IGitHubRepositorySourceFileCollector? sourceFileCollector;
    private readonly TimeProvider timeProvider;

    // The Profile V1 composition constructor: the eight required collectors,
    // with the two non-scoring enrichment surfaces and Contributors omitted. It
    // is deliberately a separate overload rather than nullable parameters on the
    // full constructor, so the Repository Proof constructor below keeps its
    // existing non-null contract exactly.
    public GitHubRepositorySnapshotOrchestrator(
        IGitHubRepositoryMetadataCollector metadataCollector,
        IGitHubRepositoryContentsCollector contentsCollector,
        IGitHubRepositoryDescriptiveDataCollector descriptiveDataCollector,
        IGitHubRepositoryCommitCollector commitCollector,
        IGitHubRepositoryIssueCollector issueCollector,
        IGitHubRepositoryPullRequestCollector pullRequestCollector,
        IGitHubRepositoryReleaseCollector releaseCollector,
        IGitHubRepositoryTagCollector tagCollector,
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
        contributorCollector = null;
        dependencyManifestCollector = null;
        sourceFileCollector = null;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

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
        IGitHubRepositoryDependencyManifestCollector dependencyManifestCollector,
        IGitHubRepositorySourceFileCollector sourceFileCollector,
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
        this.dependencyManifestCollector = dependencyManifestCollector
            ?? throw new ArgumentNullException(nameof(dependencyManifestCollector));
        this.sourceFileCollector = sourceFileCollector
            ?? throw new ArgumentNullException(nameof(sourceFileCollector));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositorySnapshotResult> CollectAsync(
        string repositoryUrl,
        CancellationToken cancellationToken = default) =>
        await CollectAsync(repositoryUrl, contentReference: null, cancellationToken);

    public async Task<GitHubRepositorySnapshotResult> CollectAtCommitAsync(
        string repositoryUrl,
        string commitSha,
        CancellationToken cancellationToken = default) =>
        await CollectAsync(
            repositoryUrl,
            GitHubCommitReference.Parse(commitSha),
            cancellationToken);

    private async Task<GitHubRepositorySnapshotResult> CollectAsync(
        string repositoryUrl,
        GitHubCommitReference? contentReference,
        CancellationToken cancellationToken)
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
            contentReference,
            cancellationToken);

        if (contentReference is not null
            && !string.Equals(
                contents.HeadCommitSha,
                contentReference.CommitSha,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Repository content at requested commit SHA '{contentReference.CommitSha}' "
                + $"could not be collected ({contents.FileTreeAvailability.Status}: "
                + $"{contents.FileTreeAvailability.Reason}). "
                + "The collector will not fall back to the current default-branch HEAD.");
        }

        GitHubRepositoryDescriptiveDataCollection descriptive =
            await descriptiveDataCollector.CollectAsync(
                repository,
                metadata,
                contentReference,
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
        // Omitted compositions never reach the collector, so no contributor
        // request is issued at all.
        GitHubRepositoryContributorCollection? contributors =
            contributorCollector is null
                ? null
                : await contributorCollector.CollectAsync(
                    repository,
                    metadata,
                    cancellationToken);
        // Omitted compositions never reach the collector, so no dependency
        // manifest request is issued at all.
        GitHubRepositoryDependencyManifestCollection? dependencyManifests =
            dependencyManifestCollector is null
                ? null
                : await dependencyManifestCollector.CollectAsync(
                    repository,
                    contents,
                    cancellationToken);

        // Raw source collection is an independent surface. It is never gated by
        // dependency declarations or dependency-manifest availability, and an
        // omitted composition never reaches the collector, so neither the two
        // tree-resolution requests nor any source-content request is issued.
        GitHubRepositorySourceFileCollection? sourceFiles =
            sourceFileCollector is null
                ? null
                : await sourceFileCollector.CollectAsync(
                    repository,
                    contents,
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

            // Null on an omitted composition; RepositorySnapshot normalizes it
            // to the same empty collection the absent-surface baseline uses.
            contributors?.Contributors,
            CollectAvailability(
                metadata,
                contents,
                descriptive,
                commits,
                issues,
                pullRequests,
                releases,
                tags,
                contributors,
                dependencyManifests,
                sourceFiles),
            CollectWarnings(
                contents,
                commits,
                issues,
                pullRequests,
                releases,
                tags,
                contributors,
                contentReference),
            MapTags(tags.Tags),
            descriptive.LicenseFile,
            contents.DocumentationMarkdownFiles,

            // Null on an omitted composition. RepositorySnapshot normalizes it
            // to the same empty collection the absent-surface baseline uses.
            dependencyManifests?.Declarations,
            sourceFiles?.SourceFiles);

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

    // The observation state travels with the mapped collection. Dropping it
    // would silently relabel a deliberate DecisionBounded stop as an Incomplete
    // failure and make the evaluators' lower-bound evidence unreachable.
    private static RepositoryCollection<TTarget> MapCollection<TSource, TTarget>(
        RepositoryCollection<TSource> source,
        Func<TSource, TTarget> map) =>
        new(
            source.Items.Select(map),
            source.TotalCount,
            source.IsComplete,
            source.ObservationState);

    private static IReadOnlyList<DataAvailability> CollectAvailability(
        GitHubRepositoryMetadataCollection metadata,
        GitHubRepositoryContentsCollection contents,
        GitHubRepositoryDescriptiveDataCollection descriptive,
        GitHubRepositoryCommitCollection commits,
        GitHubRepositoryIssueCollection issues,
        GitHubRepositoryPullRequestCollection pullRequests,
        GitHubRepositoryReleaseCollection releases,
        GitHubRepositoryTagCollection tags,
        GitHubRepositoryContributorCollection? contributors,
        GitHubRepositoryDependencyManifestCollection? dependencyManifests,
        GitHubRepositorySourceFileCollection? sourceFiles) =>
        [
            metadata.Availability,
            contents.FileTreeAvailability,
            contents.ProjectFilesAvailability,
            contents.WorkflowsAvailability,
            contents.DocumentationMarkdownAvailability,
            .. descriptive.Availability,
            commits.Availability,
            issues.Availability,
            pullRequests.Availability,
            releases.Availability,
            tags.Availability,

            // An omitted composition contributes no entry at all: an omission is
            // not an availability outcome.
            .. contributors is null
                ? Array.Empty<DataAvailability>()
                : new[] { contributors.Availability },

            // The dependency evidence surface contributes availability only.
            // Its warnings are deliberately not merged into the snapshot
            // collection warnings, which would change RepositoryAnalysis.Status.
            //
            // An omitted composition contributes no entry at all: an omission is
            // not an availability outcome, so it must never surface as
            // NotRequested, Unavailable or Failed.
            .. dependencyManifests is null
                ? Array.Empty<DataAvailability>()
                : new[] { dependencyManifests.Availability },

            // The bounded raw source surface contributes availability only, for
            // the same isolation reason, and is omitted the same way.
            .. sourceFiles is null
                ? Array.Empty<DataAvailability>()
                : new[] { sourceFiles.Availability }
        ];

    private static IReadOnlyList<string> CollectWarnings(
        GitHubRepositoryContentsCollection contents,
        GitHubRepositoryCommitCollection commits,
        GitHubRepositoryIssueCollection issues,
        GitHubRepositoryPullRequestCollection pullRequests,
        GitHubRepositoryReleaseCollection releases,
        GitHubRepositoryTagCollection tags,
        GitHubRepositoryContributorCollection? contributors,
        GitHubCommitReference? contentReference)
    {
        List<string> warnings =
        [
            .. contents.CollectionWarnings,
            .. commits.CollectionWarnings,
            .. issues.CollectionWarnings,
            .. pullRequests.CollectionWarnings,
            .. releases.CollectionWarnings,
            .. tags.CollectionWarnings,
            .. contributors?.CollectionWarnings ?? []
        ];

        if (contents.HeadCommitSha is not null)
        {
            // The live-state list names only what this composition actually
            // collected. A composition that omits contributors must not claim
            // they were read from live state, and the omission itself is never
            // described — it stays neutral, with no warning of its own.
            string liveSections = contributors is null
                ? "Metadata, languages, commits, issues, pull requests, releases, and tags"
                : "Metadata, languages, commits, issues, pull requests, releases, tags, and contributors";

            warnings.Add(contentReference is null
                ? SnapshotConsistencyWarning
                : $"Repository file tree, project files, workflows, documentation Markdown, README, and license evidence were collected from requested commit SHA '{contentReference.CommitSha}'. {liveSections} were collected from live GitHub state and may not be atomic with that commit.");
        }

        return warnings;
    }
}

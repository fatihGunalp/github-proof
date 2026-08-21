using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 097 implements docs/architecture/public-profile-runtime-collection.md.
// These tests run the full Repository Proof composition and the Profile V1
// composition against the same deterministic GitHub fixture and prove both
// halves of the locked rule at once:
//
//   the collection composition may differ  (§3.2, §5, §14.3)
//   the analysis truth may not             (§6, §14.4)
public sealed class ProfileRepositoryCollectionCompositionTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("09700000-0000-0000-0000-000000000001");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-21T09:30:00Z");

    private static string SourceContentPath =>
        $"/contents/{FakeGitHubApiHandler.SourceFilePath}";

    private static string DependencyManifestContentPath =>
        $"/contents/{FakeGitHubApiHandler.DependencyManifestPath}";

    [Fact]
    public async Task ProfileComposition_IssuesNoSourceFileRequestAtAll()
    {
        CollectionRun profile = await RunProfileCompositionAsync();

        Assert.Equal(0, profile.Handler.CountRequests(SourceContentPath));
    }

    [Fact]
    public async Task ProfileComposition_IssuesNoDependencyManifestRequestAtAll()
    {
        CollectionRun profile = await RunProfileCompositionAsync();

        Assert.Equal(
            0,
            profile.Handler.CountRequests(DependencyManifestContentPath));
    }

    // The source collector resolves its own pinned tree with two requests
    // (git/commits then git/trees). The contents collector legitimately issues
    // one of each as well, so the proof is the exact count, not the absence of
    // the URL shape.
    [Fact]
    public async Task ProfileComposition_IssuesTheTreeResolutionPairExactlyOnce()
    {
        CollectionRun full = await RunFullCompositionAsync();
        CollectionRun profile = await RunProfileCompositionAsync();

        Assert.Equal(2, full.Handler.CountRequests("/git/commits/"));
        Assert.Equal(2, full.Handler.CountRequests("/git/trees/"));

        Assert.Equal(1, profile.Handler.CountRequests("/git/commits/"));
        Assert.Equal(1, profile.Handler.CountRequests("/git/trees/"));
    }

    [Fact]
    public async Task ProfileComposition_RemovesExactlyTheEnrichmentRequests()
    {
        CollectionRun full = await RunFullCompositionAsync();
        CollectionRun profile = await RunProfileCompositionAsync();

        // Every profile request is one the full composition also issues: the
        // optimized composition adds nothing and substitutes nothing.
        Assert.Empty(profile.Handler.RequestPaths.Except(full.Handler.RequestPaths));

        List<string> removed =
        [
            .. full.Handler.RequestPaths.Except(profile.Handler.RequestPaths)
        ];

        Assert.All(
            removed,
            path => Assert.True(
                path.Contains(SourceContentPath, StringComparison.Ordinal)
                    || path.Contains(
                        DependencyManifestContentPath,
                        StringComparison.Ordinal),
                $"Unexpected request removed from the profile composition: {path}"));

        Assert.Equal(
            full.Handler.RequestPaths.Count - 4,
            profile.Handler.RequestPaths.Count);
    }

    [Fact]
    public async Task FullComposition_StillCollectsBothEnrichmentSurfaces()
    {
        CollectionRun full = await RunFullCompositionAsync();

        Assert.Equal(1, full.Handler.CountRequests(SourceContentPath));
        Assert.Equal(1, full.Handler.CountRequests(DependencyManifestContentPath));
        Assert.NotEmpty(full.Snapshot.SourceFiles.Items);
        Assert.NotEmpty(full.Snapshot.DependencyDeclarations.Items);
        Assert.Contains(
            full.Snapshot.Availability,
            availability => availability.DataKey == "source-files");
        Assert.Contains(
            full.Snapshot.Availability,
            availability => availability.DataKey == "dependency-manifests");
    }

    [Fact]
    public async Task ProfileComposition_ModelsBothSurfacesAsAbsentNotUnavailable()
    {
        CollectionRun profile = await RunProfileCompositionAsync();

        Assert.Empty(profile.Snapshot.SourceFiles.Items);
        Assert.Empty(profile.Snapshot.DependencyDeclarations.Items);

        // Absent, never NotRequested / Unavailable / Failed: an omission is not
        // an availability outcome (§5.2).
        Assert.DoesNotContain(
            profile.Snapshot.Availability,
            availability => availability.DataKey == "source-files");
        Assert.DoesNotContain(
            profile.Snapshot.Availability,
            availability => availability.DataKey == "dependency-manifests");
    }

    [Fact]
    public async Task ProfileComposition_KeepsEveryOtherAvailabilityEntryIdentical()
    {
        CollectionRun full = await RunFullCompositionAsync();
        CollectionRun profile = await RunProfileCompositionAsync();

        string[] expected =
        [
            .. full.Snapshot.Availability
                .Where(availability =>
                    availability.DataKey is not "source-files"
                        and not "dependency-manifests")
                .Select(availability => $"{availability.DataKey}:{availability.Status}")
        ];
        string[] actual =
        [
            .. profile.Snapshot.Availability
                .Select(availability => $"{availability.DataKey}:{availability.Status}")
        ];

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ProfileComposition_AddsNoCollectionWarning()
    {
        CollectionRun full = await RunFullCompositionAsync();
        CollectionRun profile = await RunProfileCompositionAsync();

        Assert.Equal(
            full.Snapshot.CollectionWarnings,
            profile.Snapshot.CollectionWarnings);
    }

    // The §6 semantic-equivalence invariant at repository level.
    [Fact]
    public async Task ProfileComposition_ProducesIdenticalRepositoryAnalysisTruth()
    {
        RepositoryAnalysis full = Analyze((await RunFullCompositionAsync()).Snapshot);
        RepositoryAnalysis profile = Analyze((await RunProfileCompositionAsync()).Snapshot);

        Assert.Equal(full.Status, profile.Status);
        Assert.Equal(
            full.ScoreBreakdown.EarnedScore,
            profile.ScoreBreakdown.EarnedScore);
        Assert.Equal(
            full.ScoreBreakdown.ApplicableMaximumScore,
            profile.ScoreBreakdown.ApplicableMaximumScore);
        Assert.Equal(
            full.ScoreBreakdown.NormalizedScore,
            profile.ScoreBreakdown.NormalizedScore);

        Assert.Equal(15, profile.SignalResults.Count);
        Assert.Equal(Describe(full), Describe(profile));

        Assert.Equal(full.LimitationKeys, profile.LimitationKeys);
        Assert.Equal(
            full.Warnings.Select(warning => warning.Code),
            profile.Warnings.Select(warning => warning.Code));
        Assert.Equal(
            full.CategoryResults.Select(category =>
                $"{category.Category}:{category.Score}:{category.MaximumScore}:{category.Status}"),
            profile.CategoryResults.Select(category =>
                $"{category.Category}:{category.Score}:{category.MaximumScore}:{category.Status}"));
    }

    // Signal key, status, score and the evidence keys behind each signal.
    private static IReadOnlyList<string> Describe(RepositoryAnalysis analysis) =>
        [
            .. analysis.SignalResults.Select(signal =>
                $"{signal.SignalKey}:{signal.Status}:{signal.Score}:"
                + string.Join('|', signal.Evidence.Select(evidence => evidence.Key)))
        ];

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(CapturedAt))
            .Analyze(snapshot, Version, AnalysisId);

    private static Task<CollectionRun> RunFullCompositionAsync() =>
        RunAsync(GitHubRepositorySnapshotOrchestrator.CreateDefault);

    private static Task<CollectionRun> RunProfileCompositionAsync() =>
        RunAsync(GitHubRepositorySnapshotOrchestrator.CreateProfileComposition);

    private static async Task<CollectionRun> RunAsync(
        Func<HttpClient, TimeProvider?, GitHubRepositorySnapshotOrchestrator> create)
    {
        FakeGitHubApiHandler handler = new();
        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        GitHubRepositorySnapshotResult result =
            await create(httpClient, new FixedTimeProvider(CapturedAt))
                .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

        Assert.True(result.IsSuccessful);

        return new CollectionRun(handler, result.Snapshot!);
    }

    private sealed record CollectionRun(
        FakeGitHubApiHandler Handler,
        RepositorySnapshot Snapshot);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

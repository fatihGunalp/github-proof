using System.Text.Json;
using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis;

public sealed class RepositoryAnalysisRunnerTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("04800000-0000-0000-0000-000000000001");

    private static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-16T00:00:00Z");

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T23:55:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public void Analyze_RunsCanonicalPipelineAndPreservesExecutionMetadata()
    {
        FixedTimeProvider timeProvider = new(EvaluationTime);
        RepositoryAnalysisRunner runner = new(timeProvider);
        RepositorySnapshot snapshot = CreateSnapshot();

        RepositoryAnalysis analysis =
            runner.Analyze(snapshot, Version, AnalysisId);

        Assert.Equal(AnalysisId, analysis.AnalysisId);
        Assert.Same(snapshot, analysis.Snapshot);
        Assert.Equal(snapshot.Repository, analysis.Repository);
        Assert.Equal(Version, analysis.Version);
        Assert.Equal(EvaluationTime, analysis.StartedAt);
        Assert.Equal(EvaluationTime, analysis.CompletedAt);

        Assert.Equal(15, analysis.SignalResults.Count);
        Assert.Equal(
            RepositoryAnalysisComposer.CanonicalSignalKeys,
            analysis.SignalResults.Select(result => result.SignalKey));

        Assert.Equal(4, analysis.CategoryResults.Count);
    }

    [Fact]
    public void Analyze_WithSameExplicitInputsAndFixedClock_IsDeterministic()
    {
        RepositorySnapshot snapshot = CreateSnapshot();

        RepositoryAnalysis first =
            new RepositoryAnalysisRunner(
                new FixedTimeProvider(EvaluationTime))
            .Analyze(snapshot, Version, AnalysisId);

        RepositoryAnalysis second =
            new RepositoryAnalysisRunner(
                new FixedTimeProvider(EvaluationTime))
            .Analyze(snapshot, Version, AnalysisId);

        Assert.Equal(
            JsonSerializer.Serialize(first),
            JsonSerializer.Serialize(second));
    }

    [Fact]
    public void Analyze_WhenSnapshotIsNull_FailsFast()
    {
        RepositoryAnalysisRunner runner = new();

        Assert.Throws<ArgumentNullException>(
            () => runner.Analyze(
                null!,
                Version,
                AnalysisId));
    }

    [Fact]
    public void Analyze_WhenVersionIsNull_FailsFast()
    {
        RepositoryAnalysisRunner runner = new();

        Assert.Throws<ArgumentNullException>(
            () => runner.Analyze(
                CreateSnapshot(),
                null!,
                AnalysisId));
    }

    [Fact]
    public void Analyze_WhenAnalysisIdIsEmpty_FailsFast()
    {
        RepositoryAnalysisRunner runner = new();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => runner.Analyze(
                    CreateSnapshot(),
                    Version,
                    Guid.Empty));

        Assert.Contains(
            "Analysis ID",
            exception.Message,
            StringComparison.Ordinal);
    }

    private static RepositorySnapshot CreateSnapshot() =>
        new(
            new RepositoryIdentity(
                "owner",
                "repository",
                new Uri("https://github.com/owner/repository"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            metadata: null,
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability: null,
            collectionWarnings: null);

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

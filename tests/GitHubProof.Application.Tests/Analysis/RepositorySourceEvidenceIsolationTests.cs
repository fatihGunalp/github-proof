using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis;

// The bounded raw source evidence surface is non-scoring. These tests lock that
// it can never change an existing analysis result.
public sealed class RepositorySourceEvidenceIsolationTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("07100000-0000-0000-0000-000000000001");

    private static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-18T00:00:00Z");

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-17T23:55:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Analyze_SourceEvidence_DoesNotChangeAnyAnalysisResult(
        bool withReadmeEvidence)
    {
        RepositoryAnalysis baseline = Analyze(CreateSnapshot(
            withReadmeEvidence,
            sourceFiles: null,
            sourceAvailability: null));
        RepositoryAnalysis withEvidence = Analyze(CreateSnapshot(
            withReadmeEvidence,
            new RepositoryCollection<RepositorySourceFile>(
            [
                new RepositorySourceFile("src/Program.cs", "class Program { }"),
                new RepositorySourceFile("tests/ProgramTests.cs", "class ProgramTests { }")
            ]),
            DataAvailability.Available("source-files", CapturedAt)));

        Assert.Equal(baseline.Status, withEvidence.Status);
        Assert.Equal(
            baseline.ScoreBreakdown.EarnedScore,
            withEvidence.ScoreBreakdown.EarnedScore);
        Assert.Equal(
            baseline.ScoreBreakdown.ApplicableMaximumScore,
            withEvidence.ScoreBreakdown.ApplicableMaximumScore);
        Assert.Equal(
            baseline.ScoreBreakdown.NormalizedScore,
            withEvidence.ScoreBreakdown.NormalizedScore);
        Assert.Equal(15, withEvidence.SignalResults.Count);
        Assert.Equal(
            baseline.SignalResults.Select(signal =>
                $"{signal.SignalKey}:{signal.Status}:{signal.Score}"),
            withEvidence.SignalResults.Select(signal =>
                $"{signal.SignalKey}:{signal.Status}:{signal.Score}"));
        Assert.Equal(baseline.LimitationKeys, withEvidence.LimitationKeys);
        Assert.Equal(
            baseline.Warnings.Select(warning => warning.Code),
            withEvidence.Warnings.Select(warning => warning.Code));
        Assert.Equal(
            baseline.CategoryResults.Select(category =>
                $"{category.Category}:{category.Score}:{category.MaximumScore}:{category.Status}"),
            withEvidence.CategoryResults.Select(category =>
                $"{category.Category}:{category.Score}:{category.MaximumScore}:{category.Status}"));
    }

    [Fact]
    public void Analyze_UnavailableSourceEvidence_DoesNotDegradeAnalysisStatus()
    {
        RepositoryAnalysis baseline = Analyze(CreateSnapshot(
            withReadmeEvidence: true,
            sourceFiles: null,
            sourceAvailability: null));
        RepositoryAnalysis withFailure = Analyze(CreateSnapshot(
            withReadmeEvidence: true,
            new RepositoryCollection<RepositorySourceFile>(isComplete: false),
            DataAvailability.Unavailable(
                "source-files",
                DataAvailabilityStatus.RateLimited,
                "GitHub rate limit was reached while retrieving source-files data.")));

        Assert.Equal(baseline.Status, withFailure.Status);
        Assert.Equal(baseline.LimitationKeys, withFailure.LimitationKeys);
        Assert.DoesNotContain(
            "limitation.snapshot.collection-partial",
            withFailure.LimitationKeys);
    }

    [Fact]
    public void Snapshot_SourceFiles_AreOrderedByPathAndCollapseDuplicates()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            withReadmeEvidence: true,
            new RepositoryCollection<RepositorySourceFile>(
            [
                new RepositorySourceFile("src/b.cs", "b"),
                new RepositorySourceFile("src/a.cs", "a"),
                new RepositorySourceFile("src/b.cs", "b")
            ]),
            DataAvailability.Available("source-files", CapturedAt));

        Assert.Equal(
            ["src/a.cs", "src/b.cs"],
            snapshot.SourceFiles.Items.Select(file => file.Path));
    }

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(EvaluationTime))
            .Analyze(snapshot, Version, AnalysisId);

    private static RepositorySnapshot CreateSnapshot(
        bool withReadmeEvidence,
        RepositoryCollection<RepositorySourceFile>? sourceFiles,
        DataAvailability? sourceAvailability)
    {
        List<DataAvailability> availability = [];

        if (withReadmeEvidence)
        {
            availability.Add(DataAvailability.Available("readme", CapturedAt));
        }

        if (sourceAvailability is not null)
        {
            availability.Add(sourceAvailability);
        }

        return new RepositorySnapshot(
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
            withReadmeEvidence
                ? new RepositoryReadme("README.md", "# Project\n\nOverview.")
                : null,
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
            availability,
            collectionWarnings: null,
            tags: null,
            licenseFile: null,
            documentationMarkdown: null,
            dependencyDeclarations: null,
            sourceFiles);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

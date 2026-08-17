using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis;

// The dependency evidence surface is non-scoring. These tests lock that it can
// never change an existing analysis result.
public sealed class RepositoryDependencyEvidenceIsolationTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("04800000-0000-0000-0000-000000000001");

    private static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-16T00:00:00Z");

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T23:55:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Analyze_DependencyEvidence_DoesNotChangeAnyAnalysisResult(
        bool withReadmeEvidence)
    {
        RepositoryAnalysis baseline = Analyze(CreateSnapshot(
            withReadmeEvidence,
            dependencyDeclarations: null,
            dependencyAvailability: null));
        RepositoryAnalysis withEvidence = Analyze(CreateSnapshot(
            withReadmeEvidence,
            new RepositoryCollection<RepositoryDependencyDeclaration>(
            [
                new RepositoryDependencyDeclaration(
                    DependencyEcosystem.DotNet,
                    "src/App.csproj",
                    "Serilog",
                    "3.1.1")
            ]),
            DataAvailability.Available("dependency-manifests", CapturedAt)));

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
    }

    [Fact]
    public void Analyze_UnavailableDependencyEvidence_DoesNotDegradeAnalysisStatus()
    {
        RepositoryAnalysis baseline = Analyze(CreateSnapshot(
            withReadmeEvidence: true,
            dependencyDeclarations: null,
            dependencyAvailability: null));
        RepositoryAnalysis withFailure = Analyze(CreateSnapshot(
            withReadmeEvidence: true,
            new RepositoryCollection<RepositoryDependencyDeclaration>(isComplete: false),
            DataAvailability.Unavailable(
                "dependency-manifests",
                DataAvailabilityStatus.Failed,
                "GitHub dependency manifest request failed.")));

        Assert.Equal(baseline.Status, withFailure.Status);
        Assert.Equal(baseline.LimitationKeys, withFailure.LimitationKeys);
        Assert.DoesNotContain(
            "limitation.snapshot.collection-partial",
            withFailure.LimitationKeys);
    }

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(EvaluationTime))
            .Analyze(snapshot, Version, AnalysisId);

    private static RepositorySnapshot CreateSnapshot(
        bool withReadmeEvidence,
        RepositoryCollection<RepositoryDependencyDeclaration>? dependencyDeclarations,
        DataAvailability? dependencyAvailability)
    {
        List<DataAvailability> availability = [];

        if (withReadmeEvidence)
        {
            availability.Add(DataAvailability.Available("readme", CapturedAt));
        }

        if (dependencyAvailability is not null)
        {
            availability.Add(dependencyAvailability);
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
            dependencyDeclarations);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

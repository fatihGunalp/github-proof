using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class RepositoryAnalysisTests
{
    [Fact]
    public void Constructor_PreservesAConsistentCompletedAnalysisGraph()
    {
        AnalysisFixture fixture = CreateFixture();
        DateTimeOffset startedAt = new(
            2026,
            8,
            15,
            12,
            0,
            0,
            TimeSpan.FromHours(3));
        DateTimeOffset completedAt = startedAt.AddMinutes(5);

        RepositoryAnalysis analysis = new(
            Guid.NewGuid(),
            fixture.Repository,
            fixture.Snapshot,
            AnalysisStatus.Completed,
            fixture.Version,
            startedAt,
            completedAt,
            fixture.SignalResults,
            fixture.CategoryResults,
            fixture.ScoreBreakdown,
            [fixture.Strength],
            [fixture.CriticalGap],
            fixture.ImprovementPlan,
            [" partial commit history ", "partial commit history"],
            [fixture.Warning]);

        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:00:00Z"), analysis.StartedAt);
        Assert.Equal(DateTimeOffset.Parse("2026-08-15T09:05:00Z"), analysis.CompletedAt);
        Assert.Equal("0.1.0", analysis.AnalyzerVersion);
        Assert.Equal("2026.08.01", analysis.RuleSetVersion);
        Assert.Equal(1, analysis.ReportSchemaVersion);
        Assert.Equal(fixture.SignalResults, analysis.SignalResults);
        Assert.Equal(fixture.CategoryResults, analysis.CategoryResults);
        Assert.Equal(["partial commit history"], analysis.LimitationKeys);
        Assert.Equal([fixture.Warning], analysis.Warnings);
    }

    [Fact]
    public void Constructor_RejectsSnapshotForAnotherRepository()
    {
        AnalysisFixture fixture = CreateFixture();
        RepositoryIdentity otherRepository = CreateRepository("other");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, repository: otherRepository));

        Assert.Equal("snapshot", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsAResultGraphWithMismatchedSignals()
    {
        AnalysisFixture fixture = CreateFixture();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, signalResults: Array.Empty<SignalResult>()));

        Assert.Equal("SignalResults", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsPlanFromAnotherRuleSetVersion()
    {
        AnalysisFixture fixture = CreateFixture();
        ImprovementPlan otherPlan = new(
            fixture.ImprovementPlan.Actions,
            DateTimeOffset.UtcNow,
            "2026.08.02");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, improvementPlan: otherPlan));

        Assert.Equal("ImprovementPlan", exception.ParamName);
    }

    [Fact]
    public void Constructor_EnforcesHighlightTypeAndReportLimit()
    {
        AnalysisFixture fixture = CreateFixture();
        AnalysisHighlight wrongType = fixture.CriticalGap;

        ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, strengths: [wrongType]));

        Assert.Equal("strengths", typeException.ParamName);

        ArgumentException limitException = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(
                fixture,
                strengths:
                [
                    fixture.Strength,
                    CreateHighlight(HighlightType.Strength, fixture.SignalResults[0].SignalKey, "evidence.readme", 2),
                    CreateHighlight(HighlightType.Strength, fixture.SignalResults[0].SignalKey, "evidence.readme", 3),
                    CreateHighlight(HighlightType.Strength, fixture.SignalResults[0].SignalKey, "evidence.readme", 4)
                ]));

        Assert.Equal("strengths", limitException.ParamName);
    }

    [Fact]
    public void Constructor_RejectsHighlightWithUnknownSignalReference()
    {
        AnalysisFixture fixture = CreateFixture();
        AnalysisHighlight unknownSignal = CreateHighlight(
            HighlightType.Strength,
            "unknown.signal",
            "evidence.readme");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, strengths: [unknownSignal]));

        Assert.Equal("Strengths", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsHighlightWithUnknownEvidenceReference()
    {
        AnalysisFixture fixture = CreateFixture();
        AnalysisHighlight unknownEvidence = CreateHighlight(
            HighlightType.Strength,
            fixture.SignalResults[0].SignalKey,
            "evidence.unknown");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, strengths: [unknownEvidence]));

        Assert.Equal("Strengths", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsImprovementActionWithUnknownEvidenceReference()
    {
        AnalysisFixture fixture = CreateFixture();
        ImprovementAction action = new(
            1,
            "action.fix-readme",
            [fixture.SignalResults[0].SignalKey],
            EffortLevel.Low,
            ["evidence.unknown"]);
        ImprovementPlan plan = new(
            [action],
            DateTimeOffset.UtcNow,
            fixture.Version.RuleSetVersion);

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CreateAnalysis(fixture, improvementPlan: plan));

        Assert.Equal("ImprovementPlan", exception.ParamName);
    }

    private static RepositoryAnalysis CreateAnalysis(
        AnalysisFixture fixture,
        RepositoryIdentity? repository = null,
        IEnumerable<SignalResult>? signalResults = null,
        ImprovementPlan? improvementPlan = null,
        IEnumerable<AnalysisHighlight>? strengths = null) =>
        new(
            Guid.NewGuid(),
            repository ?? fixture.Repository,
            fixture.Snapshot,
            AnalysisStatus.Completed,
            fixture.Version,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            signalResults ?? fixture.SignalResults,
            fixture.CategoryResults,
            fixture.ScoreBreakdown,
            strengths ?? [fixture.Strength],
            [fixture.CriticalGap],
            improvementPlan ?? fixture.ImprovementPlan,
            limitationKeys: null,
            warnings: null);

    private static AnalysisFixture CreateFixture()
    {
        RepositoryIdentity repository = CreateRepository("repository");
        RepositorySnapshot snapshot = new(
            repository,
            DateTimeOffset.UtcNow,
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
        SignalResult signal = new(
            "repository.readme.exists",
            signalVersion: 1,
            AnalysisCategory.RepositoryUnderstandability,
            SignalStatus.Present,
            score: 4,
            maximumScore: 5,
            includeInOverallScore: true,
            evidence:
            [
                new EvidenceItem(
                    "evidence.readme",
                    "repository.readme",
                    "README.md",
                    "README.md",
                    DateTimeOffset.UtcNow,
                    EvidenceConfidence.High,
                    notes: null)
            ],
            evaluatedAt: DateTimeOffset.UtcNow);
        CategoryResult category = new(
            AnalysisCategory.RepositoryUnderstandability,
            [signal]);
        ScoreBreakdown breakdown = new([category]);
        AnalysisVersion version = new("0.1.0", "2026.08.01", 1);
        ImprovementAction action = new(
            priority: 1,
            "action.add-installation-guide",
            [signal.SignalKey],
            EffortLevel.Low,
            ["evidence.readme"]);
        ImprovementPlan plan = new([action], DateTimeOffset.UtcNow, version.RuleSetVersion);

        return new AnalysisFixture(
            repository,
            snapshot,
            version,
            [signal],
            [category],
            breakdown,
            CreateHighlight(HighlightType.Strength, signal.SignalKey, "evidence.readme"),
            CreateHighlight(HighlightType.CriticalGap, signal.SignalKey, "evidence.readme"),
            plan,
            new AnalysisWarning(
                "snapshot.partial",
                WarningSeverity.Warning,
                "Commit history is incomplete.",
                ["commits"],
                ["process.commit-history"]));
    }

    private static RepositoryIdentity CreateRepository(string name) =>
        new(
            "owner",
            name,
            new Uri($"https://github.com/owner/{name}"),
            "main",
            isPublic: true,
            isArchived: false,
            isFork: false,
            isTemplate: false,
            primaryLanguage: "C#");

    private static AnalysisHighlight CreateHighlight(
        HighlightType type,
        string signalKey,
        string evidenceReference,
        int scoreImpact = 1) =>
        new(
            type,
            signalKey,
            scoreImpact,
            evidenceReferences: [evidenceReference]);

    private sealed record AnalysisFixture(
        RepositoryIdentity Repository,
        RepositorySnapshot Snapshot,
        AnalysisVersion Version,
        IReadOnlyList<SignalResult> SignalResults,
        IReadOnlyList<CategoryResult> CategoryResults,
        ScoreBreakdown ScoreBreakdown,
        AnalysisHighlight Strength,
        AnalysisHighlight CriticalGap,
        ImprovementPlan ImprovementPlan,
        AnalysisWarning Warning);
}

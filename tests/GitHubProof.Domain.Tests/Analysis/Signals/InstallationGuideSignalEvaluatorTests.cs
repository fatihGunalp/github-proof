using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class InstallationGuideSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalInstallationGuideContract()
    {
        SignalDefinition definition = new InstallationGuideSignalEvaluator().Definition;

        Assert.Equal("documentation.installation-guide", definition.Key);
        Assert.Equal(AnalysisCategory.RepositoryUnderstandability, definition.Category);
        Assert.Equal(8, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["readme", "documentation-markdown"], definition.RequiredDataKeys);
    }

    [Theory]
    [InlineData("kurulum")]
    [InlineData("installation")]
    [InlineData("install")]
    [InlineData("setup")]
    [InlineData("getting started")]
    [InlineData("quick start")]
    [InlineData("başlangıç")]
    [InlineData("çalıştırma")]
    [InlineData("running")]
    [InlineData("run locally")]
    public void Evaluate_WithCanonicalAtxHeading_MatchesCaseInsensitively(string heading)
    {
        string upperHeading = heading.ToUpperInvariant();
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            $"## {upperHeading} ##\n1. Install the dependencies.");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
        Assert.Contains(heading.Replace(' ', '-'), Assert.Single(result.Evidence).Location!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluate_WithSetextHeading_ReturnsPresent()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "Installation\n------------\nRun the application.");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal("README.md#installation", Assert.Single(result.Evidence).Location);
    }

    [Fact]
    public void Evaluate_StopsAtNextSameLevelHeading()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\nThis section explains the environment.\n## Usage\n```bash\ndotnet run\n```");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Fact]
    public void Evaluate_IncludesLowerLevelSubsections()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n### Dependencies\n```bash\ndotnet restore\n```");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
    }

    [Theory]
    [InlineData("This project uses npm for installation.")]
    [InlineData("[![Install](https://example.test/install.svg)](https://example.test)")]
    [InlineData("InstallerFactory is the entry point.")]
    [InlineData("## Setup Architecture\n1. Describe the architecture.")]
    [InlineData("## How Our Installer Works\n1. Read the details.")]
    [InlineData("## Instalation\n1. Install dependencies.")]
    public void Evaluate_DoesNotTreatNonCanonicalTextAsInstallationEvidence(string content)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(content));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Evaluate_IgnoresHeadingInsideFencedCodeBlock()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "```markdown\n## Installation\n1. Install dependencies.\n```");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_WithNonEmptyFencedBlock_ReturnsPresent()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n~~~bash\nnpm install\n~~~~");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("fenced code block", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_WithEmptyFencedBlock_ReturnsPartial()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n```bash\n   \n```");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Theory]
    [InlineData("1. Clone the repository.")]
    [InlineData("2) Projeyi çalıştırın.")]
    public void Evaluate_WithOrderedStep_ReturnsPresent(string step)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot($"## Installation\n{step}"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("ordered step", Assert.Single(result.Evidence).Notes);
    }

    [Theory]
    [InlineData("1. ![Install badge](https://example.test/install.svg)")]
    [InlineData("1. https://example.test/install")]
    [InlineData("1. `InstallerFactory`")]
    [InlineData("1. v1.2.3")]
    public void Evaluate_WithOrderedArtifactOnly_DoesNotTreatArtifactAsActionable(string item)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot($"## Installation\n{item}"));

        Assert.Equal(SignalStatus.Partial, result.Status);
    }

    [Theory]
    [InlineData("Install the required packages.")]
    [InlineData("Run the application locally.")]
    [InlineData("Kurun gerekli paketleri.")]
    [InlineData("Çalıştırın uygulamayı.")]
    [InlineData("Geri yükleyin bağımlılıkları.")]
    public void Evaluate_WithCanonicalImperative_ReturnsPresent(string instruction)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            $"## Installation\n{instruction}"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("imperative instruction", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_WithExplanatoryProseOnly_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Installation\nThis project uses npm for installation and Node.js for execution."));

        Assert.Equal(SignalStatus.Partial, result.Status);
    }

    [Fact]
    public void Evaluate_WithTwoDistinctFeatures_ReturnsPresent()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n```bash\ndotnet restore\ndotnet build\n```");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void Evaluate_WithThreeDistinctFeaturesInOneSection_ReturnsStrong()
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        InstallationGuideSignalEvaluator evaluator = new(timeProvider);
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n```bash\ndotnet restore\ndotnet build\ndotnet run\n```");

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
        Assert.Equal(EvaluatedAt, result.EvaluatedAt);
        Assert.Equal(1, timeProvider.CallCount);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Contains("dependency installation", evidence.Value);
        Assert.Contains("build", evidence.Value);
        Assert.Contains("run", evidence.Value);
    }

    [Fact]
    public void Evaluate_DetectsPrerequisiteConfigurationAndRunFeatures()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n### Requirements\n.NET 10 is required.\n### Configuration\n1. Copy .env.example to .env.\n```bash\ndotnet run\n``" + "`");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Contains("prerequisites", evidence.Value);
        Assert.Contains("configuration", evidence.Value);
        Assert.Contains("run", evidence.Value);
    }

    [Fact]
    public void Evaluate_DoesNotTreatRequirementTriggerWithoutToolValueOrVersionAsFeature()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\nThis is required.\n```bash\ndotnet build\ndotnet run\n``" + "`");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
    }

    [Fact]
    public void Evaluate_DetectsMigrationPreparationAsDistinctFeature()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n```bash\ndotnet restore\ndotnet build\ndotnet ef database update\n``" + "`");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Contains("migration/preparation", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_DoesNotDoubleCountOneOperationAcrossFeatures()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n```bash\nnpm install\nnpm start\n``" + "`\nCopy example configuration.\n");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Contains("configuration", evidence.Value);
        Assert.DoesNotContain("migration/preparation", evidence.Value);
    }

    [Fact]
    public void Evaluate_DoesNotCombineStrongFeaturesAcrossSources()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "## Installation\n```bash\ndotnet restore\n``" + "`",
            new RepositoryCollection<RepositoryFile>(
            [
                MarkdownFile(
                    "docs/setup.md",
                    "## Setup\n```bash\ndotnet build\ndotnet run\n``" + "`")
            ]),
            Available("readme"),
            Available("documentation-markdown"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
    }

    [Fact]
    public void Evaluate_WithCompleteCorpusAndHeadingOnly_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot("# Installation\nOverview only."));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Fact]
    public void Evaluate_WithCompleteCorpusAndNoHeading_ReturnsMissing()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot("# Project\nOverview.");

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.Equal(2, result.Evidence.Count);
        Assert.All(result.Evidence, item => Assert.Contains("matched installation section count = 0", item.Notes));
    }

    [Fact]
    public void Evaluate_WhenDocsAreUnavailableAndReadmeHasNoEvidence_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Project\nOverview.",
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WithReadmePartialAndIncompleteDocs_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "## Installation\nOverview only.",
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            Available("readme"),
            Available("documentation-markdown"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WithIncompleteDocsAndCollectedStrongEvidence_PreservesStrong()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>(
            [
                MarkdownFile(
                    "docs/setup.md",
                    "# Setup\n```bash\nnpm install\nnpm run build\nnpm start\n``" + "`")
            ],
            isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void Evaluate_WithIncompleteDocsAndCollectedPresentEvidence_PreservesPresent()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>(
                [MarkdownFile("docs/setup.md", "# Setup\n1. Install packages.")],
                isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void Evaluate_WithReadmePresentAndUnavailableDocs_PreservesPresent()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "## Installation\n1. Install dependencies.",
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(8, result.Score);
    }

    [Fact]
    public void Evaluate_WithNearCanonicalActionableHeading_RequiresHumanReview()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Installation (Windows)\n1. Install the dependencies."));

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.Equal(EvidenceConfidence.Medium, Assert.Single(result.Evidence).Confidence);
    }

    [Fact]
    public void Evaluate_WithTypoHeading_DoesNotRequireHumanReview()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Instalation\n1. Install the dependencies."));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_WithUnclosedFenceInMatchedSection_RequiresHumanReview()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Installation\n```bash\ndotnet restore"));

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("kapanmamış", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_DocumentationInputOrderDoesNotChangeSemanticResultOrEvidenceOrder()
    {
        RepositoryFile first = MarkdownFile(
            "docs/a.md",
            "## Setup\n```bash\ndotnet restore\ndotnet build\ndotnet run\n``" + "`");
        RepositoryFile second = MarkdownFile(
            "docs/z.md",
            "## Installation\n1. Install packages.");
        RepositorySnapshot ordered = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>([first, second]),
            Available("readme"),
            Available("documentation-markdown"));
        RepositorySnapshot reversed = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>([second, first]),
            Available("readme"),
            Available("documentation-markdown"));

        SignalResult firstResult = Evaluate(ordered);
        SignalResult secondResult = Evaluate(reversed);

        AssertEquivalent(firstResult, secondResult);
        Assert.Equal("docs/a.md#setup", Assert.Single(firstResult.Evidence).Location);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentDomainState()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "## Installation\n```bash\ndotnet restore\ndotnet build\ndotnet run\n``" + "`");

        SignalResult first = new InstallationGuideSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
        SignalResult second = new InstallationGuideSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        AssertEquivalent(first, second);
    }

    private static SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        return new InstallationGuideSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
    }

    private static RepositorySnapshot CreateCompleteSnapshot(string? readmeContent)
    {
        return CreateSnapshot(
            readmeContent,
            new RepositoryCollection<RepositoryFile>(),
            Available("readme"),
            Available("documentation-markdown"));
    }

    private static RepositorySnapshot CreateSnapshot(
        string? readmeContent,
        RepositoryCollection<RepositoryFile> documentationMarkdown,
        DataAvailability readmeAvailability,
        DataAvailability documentationAvailability)
    {
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
            readmeContent is null ? null : new RepositoryReadme("README.md", readmeContent),
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
            availability: [readmeAvailability, documentationAvailability],
            collectionWarnings: null,
            documentationMarkdown: documentationMarkdown);
    }

    private static RepositoryFile MarkdownFile(string path, string content)
    {
        return new RepositoryFile(path, isDirectory: false, content.Length, content);
    }

    private static DataAvailability Available(string dataKey)
    {
        return DataAvailability.Available(dataKey, RetrievedAt);
    }

    private static DataAvailability Unavailable(string dataKey)
    {
        return DataAvailability.Unavailable(
            dataKey,
            DataAvailabilityStatus.Failed,
            "Collection could not complete.");
    }

    private static void AssertEquivalent(SignalResult first, SignalResult second)
    {
        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.SignalVersion, second.SignalVersion);
        Assert.Equal(first.Category, second.Category);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.MaximumScore, second.MaximumScore);
        Assert.Equal(first.IncludeInOverallScore, second.IncludeInOverallScore);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
    }

    private sealed class CountingTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public int CallCount { get; private set; }

        public override DateTimeOffset GetUtcNow()
        {
            CallCount++;
            return utcNow;
        }
    }
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class UsageDemoSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        new(2026, 8, 15, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RetrievedAt =
        new(2026, 8, 15, 8, 1, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EvaluatedAt =
        new(2026, 8, 15, 8, 2, 0, TimeSpan.Zero);

    [Fact]
    public void Definition_MatchesCanonicalUsageDemoContract()
    {
        UsageDemoSignalEvaluator evaluator = new();

        Assert.Equal("documentation.usage-or-demo", evaluator.Definition.Key);
        Assert.Equal(AnalysisCategory.RepositoryUnderstandability, evaluator.Definition.Category);
        Assert.Equal(7, evaluator.Definition.MaximumScore);
        Assert.True(evaluator.Definition.IncludeInOverallScore);
        Assert.Equal(1, evaluator.Definition.Version);
        Assert.Equal(
            ["readme", "documentation-markdown", "metadata"],
            evaluator.Definition.RequiredDataKeys);
    }

    [Theory]
    [InlineData("usage")]
    [InlineData("HOW TO USE")]
    [InlineData("using")]
    [InlineData("example")]
    [InlineData("examples")]
    [InlineData("quick start")]
    [InlineData("getting started")]
    [InlineData("kullanım")]
    [InlineData("nasıl kullanılır")]
    [InlineData("örnek")]
    [InlineData("örnekler")]
    [InlineData("başlangıç")]
    public void Evaluate_WithExactAtxUsageHeading_ReturnsPresent(string heading)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            $"## {heading} ##\n1. Open the application and create an item."));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(5, result.Score);
    }

    [Theory]
    [InlineData("Usage", "=====")]
    [InlineData("Kullanım", "--------")]
    public void Evaluate_WithSetextUsageHeading_ReturnsPresent(
        string heading,
        string underline)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            $"{heading}\n{underline}\n1. Open the application."));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("#", Assert.Single(result.Evidence).Location);
    }

    [Fact]
    public void Evaluate_StopsUsageSectionAtNextSameLevelHeading()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Usage\nOverview only.\n## Source\n```csharp\nConsole.WriteLine(\"hello\");\n```"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_IncludesLowerLevelUsageSubsections()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Usage\n### First request\n1. Open the application."));

        Assert.Equal(SignalStatus.Present, result.Status);
    }

    [Fact]
    public void Evaluate_IgnoresUsageHeadingInsideFence()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "```markdown\n## Usage\n1. Open the application.\n```"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Theory]
    [InlineData("```csharp\nConsole.WriteLine(\"hello\");\n```")]
    [InlineData("## Usage\n```bash\nnpm install\n```")]
    [InlineData("![logo](assets/logo.png)")]
    [InlineData("![build badge](https://img.shields.io/build.svg)")]
    [InlineData("![app icon](assets/icon.png)")]
    [InlineData("![architecture](docs/architecture.png)")]
    [InlineData("[Website](https://example.test)")]
    [InlineData("[Channel](https://youtube.com/@example)")]
    public void Evaluate_DoesNotPromoteFalsePositiveContent(string content)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(content));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
    }

    [Theory]
    [InlineData("dotnet run")]
    [InlineData("npm start -- --port 5000")]
    [InlineData("npm run dev")]
    [InlineData("python app.py --verbose")]
    [InlineData("docker compose up -d")]
    [InlineData("curl https://example.test/api")]
    [InlineData("git clone https://github.com/owner/repository")]
    public void Evaluate_WithCanonicalUsageCommand_ReturnsPresent(string command)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            $"# Usage\n{command}"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("usage", Assert.Single(result.Evidence).Value);
    }

    [Theory]
    [InlineData("python")]
    [InlineData("curl")]
    [InlineData("git clone")]
    [InlineData("dotnet runner")]
    [InlineData("npm starter")]
    public void Evaluate_WithIncompleteOrPrefixCommand_DoesNotReturnPresent(string command)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            $"# Usage\n{command}"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_WithMeaningfulFencedExample_ReturnsPresent()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\n```csharp\nConsole.WriteLine(\"Hello\");\n```"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("fenced code example", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithNonInstallationOrderedStep_ReturnsPresent()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\n1. Open the application and submit the form."));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("ordered usage step", Assert.Single(result.Evidence).Value);
    }

    [Theory]
    [InlineData("1. npm install")]
    [InlineData("1. dotnet build")]
    [InlineData("1. Configure the environment variables.")]
    [InlineData("1. Run the database migration.")]
    public void Evaluate_WithInstallationOnlyOrderedSteps_ReturnsMissing(string step)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot($"# Usage\n{step}"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_WithSampleRequestResponse_ReturnsPresent()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\n## Example Request\nPOST /items\n## Response\n201 Created"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("sample request/response", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithStandaloneSampleOutput_ReturnsPresent()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\nSample Output:\nCreated item 42"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Contains("sample output", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithLabelOnly_DoesNotReturnPresent()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\nOutput:"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_WithUsageAndScreenshotInSameSource_ReturnsStrong()
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        UsageDemoSignalEvaluator evaluator = new(timeProvider);
        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(
            "# Usage\ndotnet run\n![Application screenshot](assets/screenshot1.png)"));

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(7, result.Score);
        Assert.Equal(2, result.Evidence.Count);
        Assert.Null(result.Recommendation);
        Assert.Single(result.Limitations);
        Assert.Equal(EvaluatedAt, result.EvaluatedAt);
        Assert.Equal(1, timeProvider.CallCount);
    }

    [Fact]
    public void Evaluate_WithScreenshotOnly_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "![Product screenshot](assets/screenshot.png)"));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Equal(EvidenceConfidence.High, Assert.Single(result.Evidence).Confidence);
    }

    [Fact]
    public void Evaluate_WithGenericImageInDemoSection_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "## Demo\n![Application](assets/product.png)"));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Theory]
    [InlineData("![Logo](assets/product.png)")]
    [InlineData("![Badge](assets/badge.svg)")]
    [InlineData("![Avatar](assets/avatar.png)")]
    public void Evaluate_WithExcludedImageInDemoSection_ReturnsMissing(string image)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot($"## Demo\n{image}"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_WithPositiveAndExclusionImageMarkers_ReturnsWeakPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "![Demo logo](assets/screenshot-logo.png)"));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal(EvidenceConfidence.Medium, evidence.Confidence);
        Assert.Contains("bounded weak", evidence.Notes);
    }

    [Fact]
    public void Evaluate_WithReferenceStyleScreenshot_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "![Product preview][screen]\n\n[screen]: assets/ui.png"));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(EvidenceConfidence.High, Assert.Single(result.Evidence).Confidence);
    }

    [Theory]
    [InlineData("[Video](https://youtube.com/watch?v=abc)")]
    [InlineData("[Walkthrough](https://player.vimeo.com/video/123)")]
    [InlineData("[Demo video](assets/demo.webm?raw=1)")]
    [InlineData("<a href=\"assets/demo.mp4\">Video</a>")]
    public void Evaluate_WithCanonicalVideoOnly_ReturnsPartial(string video)
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(video));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Contains("video", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithGenericVideoLinkInsideUsageSection_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\n[Watch](https://youtu.be/abc)"));

        Assert.Equal(SignalStatus.Partial, result.Status);
    }

    [Fact]
    public void Evaluate_WithCorroboratedLiveDemoOnly_ReturnsPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "[Live Demo](HTTPS://EXAMPLE.TEST:443/app/#intro)",
            homepage: "https://example.test/app"));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Contains("live demo", Assert.Single(result.Evidence).Value);
    }

    [Fact]
    public void Evaluate_WithUncorroboratedDemoIntentLink_ReturnsWeakPartial()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "[Demo](https://preview.example.test)",
            homepage: "https://example.test"));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(EvidenceConfidence.Medium, Assert.Single(result.Evidence).Confidence);
        Assert.Contains("corroborate edilmedi", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_WithHomepageAlone_ReturnsMissing()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Project\nOverview.",
            homepage: "https://example.test/app"));

        Assert.Equal(SignalStatus.Missing, result.Status);
    }

    [Fact]
    public void Evaluate_DoesNotCombineUsageAndVisualAcrossSourcesForStrong()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Usage\ndotnet run",
            new RepositoryCollection<RepositoryFile>(
                [MarkdownFile("docs/demo.md", "![Screenshot](assets/screenshot.png)")]),
            Available("readme"),
            Available("documentation-markdown"),
            homepage: null,
            Available("metadata"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(5, result.Score);
        Assert.Equal("README.md#usage", Assert.Single(result.Evidence).Location);
    }

    [Fact]
    public void Evaluate_WithUnavailableDocsAndNoTerminalStrong_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Project\nOverview.",
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"),
            homepage: null,
            Available("metadata"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Null(result.Recommendation);
    }

    [Fact]
    public void Evaluate_WithReadmeUsageAndUnavailableDocs_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Usage\ndotnet run",
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"),
            homepage: null,
            Available("metadata"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_WithIncompleteCorpusAndCollectedStrongEvidence_PreservesStrong()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>(
                [MarkdownFile(
                    "docs/demo.md",
                    "# Usage\ndotnet run\n![Screenshot](assets/screenshot.png)")],
                isComplete: false),
            Available("readme"),
            Unavailable("documentation-markdown"),
            homepage: null,
            Available("metadata"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Strong, result.Status);
        Assert.Equal(7, result.Score);
    }

    [Fact]
    public void Evaluate_WithUnavailableMetadataThatCouldUpgradeUsage_ReturnsUnavailableEvidence()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "# Usage\ndotnet run\n[Live Demo](https://example.test/app)",
            new RepositoryCollection<RepositoryFile>(),
            Available("readme"),
            Available("documentation-markdown"),
            homepage: null,
            Unavailable("metadata"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Contains(result.Evidence, item => item.Source == "MetadataAvailability");
    }

    [Fact]
    public void Evaluate_WithUnavailableMetadataAndVisualOnlyDemoLink_ReturnsPartial()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            "[Live Demo](https://example.test/app)",
            new RepositoryCollection<RepositoryFile>(),
            Available("readme"),
            Available("documentation-markdown"),
            homepage: null,
            Unavailable("metadata"));

        SignalResult result = Evaluate(snapshot);

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Fact]
    public void Evaluate_WithUnclosedFenceInUsageSection_RequiresHumanReview()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "# Usage\n```bash\ndotnet run"));

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("kapanmamış", Assert.Single(result.Evidence).Notes);
    }

    [Fact]
    public void Evaluate_WithMalformedScreenshotCandidate_RequiresHumanReview()
    {
        SignalResult result = Evaluate(CreateCompleteSnapshot(
            "![Screenshot](assets/screenshot.png"));

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_DocumentationInputOrderDoesNotChangeResultOrEvidenceOrder()
    {
        RepositoryFile usage = MarkdownFile(
            "docs/z-usage.md",
            "# Usage\ndotnet run");
        RepositoryFile visual = MarkdownFile(
            "docs/a-demo.md",
            "![Screenshot](assets/screenshot.png)");
        RepositorySnapshot ordered = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>([visual, usage]),
            Available("readme"),
            Available("documentation-markdown"),
            homepage: null,
            Available("metadata"));
        RepositorySnapshot reversed = CreateSnapshot(
            "# Project",
            new RepositoryCollection<RepositoryFile>([usage, visual]),
            Available("readme"),
            Available("documentation-markdown"),
            homepage: null,
            Available("metadata"));

        SignalResult first = Evaluate(ordered);
        SignalResult second = Evaluate(reversed);

        AssertEquivalent(first, second);
        Assert.Equal("docs/z-usage.md#usage", Assert.Single(first.Evidence).Location);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentDomainState()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            "# Usage\ndotnet run\n![Screenshot](assets/screenshot.png)");

        SignalResult first = new UsageDemoSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
        SignalResult second = new UsageDemoSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);

        AssertEquivalent(first, second);
    }

    private static SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        return new UsageDemoSignalEvaluator(
            new CountingTimeProvider(EvaluatedAt)).Evaluate(snapshot);
    }

    private static RepositorySnapshot CreateCompleteSnapshot(
        string? readmeContent,
        string? homepage = null)
    {
        return CreateSnapshot(
            readmeContent,
            new RepositoryCollection<RepositoryFile>(),
            Available("readme"),
            Available("documentation-markdown"),
            homepage,
            Available("metadata"));
    }

    private static RepositorySnapshot CreateSnapshot(
        string? readmeContent,
        RepositoryCollection<RepositoryFile> documentationMarkdown,
        DataAvailability readmeAvailability,
        DataAvailability documentationAvailability,
        string? homepage,
        DataAvailability metadataAvailability)
    {
        RepositoryMetadata? metadata = metadataAvailability.IsAvailable
            ? new RepositoryMetadata(
                description: null,
                homepage,
                CapturedAt.AddYears(-1),
                issuesEnabled: true)
            : null;

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
            metadata,
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
            availability:
            [
                readmeAvailability,
                documentationAvailability,
                metadataAvailability
            ],
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
        Assert.Equal(first.Explanation, second.Explanation);
        Assert.Equal(first.Recommendation, second.Recommendation);
        Assert.Equal(first.Limitations, second.Limitations);
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

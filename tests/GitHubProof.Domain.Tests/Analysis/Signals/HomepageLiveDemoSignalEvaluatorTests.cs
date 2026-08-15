using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

public sealed class HomepageLiveDemoSignalEvaluatorTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-15T09:00:00Z");
    private static readonly DateTimeOffset MetadataRetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:00Z");
    private static readonly DateTimeOffset ReadmeRetrievedAt =
        DateTimeOffset.Parse("2026-08-15T09:01:30Z");
    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-15T09:02:00Z");

    [Fact]
    public void Definition_MatchesCanonicalHomepageContract()
    {
        SignalDefinition definition = new HomepageLiveDemoSignalEvaluator().Definition;

        Assert.Equal("portfolio.homepage-or-live-demo", definition.Key);
        Assert.Equal(AnalysisCategory.PortfolioPresentation, definition.Category);
        Assert.Equal(5, definition.MaximumScore);
        Assert.Equal(1, definition.Version);
        Assert.True(definition.IncludeInOverallScore);
        Assert.Equal(["metadata", "readme"], definition.RequiredDataKeys);
    }

    [Fact]
    public void Evaluate_WhenMetadataHomepageIsMeaningful_ReturnsTerminalPresentFive()
    {
        CountingTimeProvider timeProvider = new(EvaluatedAt);
        HomepageLiveDemoSignalEvaluator evaluator = new(timeProvider);
        RepositorySnapshot snapshot = CreateSnapshot(
            homepage: "HTTPS://Example.COM:443/app/#overview",
            readme: null,
            MetadataAvailable(),
            ReadmeUnavailable());

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(5, result.Score);
        Assert.Equal(1, timeProvider.CallCount);

        EvidenceItem evidence = Assert.Single(result.Evidence);
        Assert.Equal("RepositoryMetadata", evidence.Source);
        Assert.Equal("https://example.com/app", evidence.Value);
        Assert.Equal("homepage", evidence.Location);
        Assert.Equal(MetadataRetrievedAt, evidence.ObservedAt);
    }

    [Theory]
    [InlineData("https://github.com/owner/repository")]
    [InlineData("https://github.com/OWNER/REPOSITORY/")]
    [InlineData("https://github.com/owner/repository/issues/12")]
    [InlineData("https://github.com/owner/repository/pulls")]
    [InlineData("https://github.com/owner/repository/wiki/Guide")]
    [InlineData("https://github.com/owner/repository/blob/main/README.md")]
    public void Evaluate_WhenHomepageIsRepositoryManagementUrl_DoesNotAwardHomepageScore(
        string homepage)
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage, "# Project"));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.Contains("repository-management", result.Evidence[0].Notes);
    }

    [Theory]
    [InlineData("https://docs.example.com/project")]
    [InlineData("https://example.com/docs")]
    [InlineData("https://example.com/product/documentation/start")]
    [InlineData("https://example.com/product-docs-v2")]
    public void Evaluate_ClassifiesOnlyExactDocumentationMarkersAsDocsOnly(
        string homepage)
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage, "# Project"));

        if (homepage.EndsWith("product-docs-v2", StringComparison.Ordinal))
        {
            Assert.Equal(SignalStatus.Present, result.Status);
            Assert.Equal(5, result.Score);
        }
        else
        {
            Assert.Equal(SignalStatus.Missing, result.Status);
            Assert.Equal(0, result.Score);
            Assert.Contains("documentation-only", result.Evidence[0].Notes);
        }
    }

    [Theory]
    [InlineData("Demo")]
    [InlineData("live demo")]
    [InlineData("TRY IT")]
    [InlineData("open app")]
    [InlineData("launch app")]
    [InlineData("canlı demo")]
    [InlineData("uygulamayı aç")]
    [InlineData("deneyin")]
    public void Evaluate_WhenReadmeHasCanonicalDemoIntent_ReturnsPresentFour(string label)
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();
        string readme = $"# Project\n\n[{label}](https://demo.example.com/app/)";

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage: null, readme));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
        EvidenceItem evidence = result.Evidence[1];
        Assert.Equal("README", evidence.Source);
        Assert.Contains("https://demo.example.com/app", evidence.Value);
        Assert.Contains("explicit demo intent", evidence.Notes);
    }

    [Fact]
    public void Evaluate_WhenReadmeHasCanonicalHtmlAnchor_ReturnsPresentFour()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();
        const string readme = "<a href=\"https://example.com/app\"><strong>Open App</strong></a>";

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage: null, readme));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Theory]
    [InlineData("[Demo](<https://example.com/app>)")]
    [InlineData("[Demo](https://example.com/app \"Open the demo\")")]
    public void Evaluate_ReusesCanonicalMarkdownTargetNormalization(string readme)
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage: null, readme));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
        Assert.Contains("https://example.com/app", result.Evidence[1].Value);
    }

    [Fact]
    public void Evaluate_WhenReadmeHasBoundedAmbiguousIntent_ReturnsPartialTwo()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();
        const string readme = "[Live preview](https://example.com/preview)";

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage: null, readme));

        Assert.Equal(SignalStatus.Partial, result.Status);
        Assert.Equal(2, result.Score);
        Assert.Contains("ambiguous presentation intent", result.Evidence[1].Notes);
    }

    [Fact]
    public void Evaluate_WhenCompleteEvidenceHasNoCandidate_ReturnsMissing()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(
            homepage: null,
            "# Project\n\nNo links here."));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
        Assert.Equal("0", result.Evidence[1].Value);
    }

    [Fact]
    public void Evaluate_WhenMetadataUnavailableAndReadmeHasDemo_ReturnsUnavailableEvidence()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();
        RepositorySnapshot snapshot = CreateSnapshot(
            homepage: null,
            new RepositoryReadme("README.md", "[Demo](https://example.com)"),
            MetadataUnavailable(),
            ReadmeAvailable());

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Single(result.Evidence);
        Assert.Equal("MetadataAvailability", result.Evidence[0].Source);
    }

    [Fact]
    public void Evaluate_WhenReadmeUnavailableWithoutTerminalHomepage_ReturnsUnavailableEvidence()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();
        RepositorySnapshot snapshot = CreateSnapshot(
            homepage: null,
            readme: null,
            MetadataAvailable(),
            ReadmeUnavailable());

        SignalResult result = evaluator.Evaluate(snapshot);

        Assert.Equal(SignalStatus.UnavailableEvidence, result.Status);
        Assert.Null(result.Score);
        Assert.Equal("ReadmeAvailability", result.Evidence[1].Source);
    }

    [Fact]
    public void Evaluate_WhenMetadataHomepageExcludedAndReadmeHasDemo_ReturnsPresentFour()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(
            "https://github.com/owner/repository/issues",
            "[Demo](https://example.com/app)"));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(4, result.Score);
    }

    [Fact]
    public void Evaluate_ExcludesGenericLinksBadgesImagesAndFencedExamples()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();
        const string readme = """
            [Docs](https://docs.example.com)
            [Source](https://github.com/owner/repository)
            [Issues](https://github.com/owner/repository/issues)
            [Dependency](https://nuget.org/packages/example)
            [Website](https://example.com)
            [![Build](https://img.shields.io/build.svg)](https://example.com/actions)

            ```markdown
            [Demo](https://example.com/demo)
            ```
            """;

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage: null, readme));

        Assert.Equal(SignalStatus.Missing, result.Status);
        Assert.Equal(0, result.Score);
    }

    [Theory]
    [InlineData("[Demo](https://example.com")]
    [InlineData("[Live preview](not-a-url)")]
    public void Evaluate_WhenRelevantReadmeCandidateIsMalformed_ReturnsRequiresHumanReview(
        string readme)
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(homepage: null, readme));

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.Contains("malformed", result.Evidence[1].Notes);
    }

    [Fact]
    public void Evaluate_WhenMetadataHomepageIsMalformed_ReturnsRequiresHumanReviewBeforeReadme()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(
            "not-an-absolute-url",
            "[Demo](https://example.com/app)"));

        Assert.Equal(SignalStatus.RequiresHumanReview, result.Status);
        Assert.Null(result.Score);
        Assert.Single(result.Evidence);
        Assert.Equal(EvidenceConfidence.Low, result.Evidence[0].Confidence);
    }

    [Fact]
    public void Evaluate_DoesNotPerformRemoteHealthValidation()
    {
        HomepageLiveDemoSignalEvaluator evaluator = CreateEvaluator();

        SignalResult result = evaluator.Evaluate(CreateCompleteSnapshot(
            "https://does-not-exist.invalid/app",
            readme: null));

        Assert.Equal(SignalStatus.Present, result.Status);
        Assert.Equal(5, result.Score);
    }

    [Fact]
    public void Evaluate_WithSameSnapshotAndTime_ProducesEquivalentDomainState()
    {
        RepositorySnapshot snapshot = CreateCompleteSnapshot(
            homepage: null,
            "[Live preview](https://example.com/preview)\n[Demo site](https://example.com/demo)");

        SignalResult first = CreateEvaluator().Evaluate(snapshot);
        SignalResult second = CreateEvaluator().Evaluate(snapshot);

        Assert.Equal(first.SignalKey, second.SignalKey);
        Assert.Equal(first.SignalVersion, second.SignalVersion);
        Assert.Equal(first.Category, second.Category);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(first.EvaluatedAt, second.EvaluatedAt);
    }

    private static HomepageLiveDemoSignalEvaluator CreateEvaluator() =>
        new(new CountingTimeProvider(EvaluatedAt));

    private static RepositorySnapshot CreateCompleteSnapshot(
        string? homepage,
        string? readme) =>
        CreateSnapshot(
            homepage,
            readme is null ? null : new RepositoryReadme("README.md", readme),
            MetadataAvailable(),
            ReadmeAvailable());

    private static RepositorySnapshot CreateSnapshot(
        string? homepage,
        RepositoryReadme? readme,
        DataAvailability metadataAvailability,
        DataAvailability readmeAvailability) =>
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
            metadata: new RepositoryMetadata(null, homepage, CapturedAt, issuesEnabled: true),
            readme,
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
            availability: [metadataAvailability, readmeAvailability],
            collectionWarnings: null);

    private static DataAvailability MetadataAvailable() =>
        DataAvailability.Available("metadata", MetadataRetrievedAt);

    private static DataAvailability ReadmeAvailable() =>
        DataAvailability.Available("readme", ReadmeRetrievedAt);

    private static DataAvailability MetadataUnavailable() =>
        DataAvailability.Unavailable(
            "metadata",
            DataAvailabilityStatus.RateLimited,
            "Metadata request was rate limited.");

    private static DataAvailability ReadmeUnavailable() =>
        DataAvailability.Unavailable(
            "readme",
            DataAvailabilityStatus.Failed,
            "README request failed.");

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

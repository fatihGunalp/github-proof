using System.Net.Http.Headers;
using System.Text.Json;
using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using Xunit.Abstractions;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositorySnapshotLiveTests(ITestOutputHelper output)
{
    private const string LiveValidationVariable = "GITHUBPROOF_RUN_LIVE_TESTS";
    private const string ValidationOutputVariable = "GITHUBPROOF_VALIDATION_OUTPUT_ROOT";
    private static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-15T12:00:00Z");

    [Fact]
    [Trait("Category", "LiveIntegration")]
    public async Task CollectAndAnalyzeAsync_ValidatesFrozenRepositorySetAndNotFound()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(LiveValidationVariable),
                "1",
                StringComparison.Ordinal))
        {
            output.WriteLine(
                $"Live integration disabled. Set {LiveValidationVariable}=1 to opt in.");
            return;
        }

        using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(15));
        using HttpClient httpClient = CreateHttpClient();
        GitHubRepositorySnapshotOrchestrator orchestrator = CreateOrchestrator(httpClient);
        IReadOnlyList<ValidationRepository> repositories = ValidationRepositoryManifest.Load();
        string? outputRoot = Environment.GetEnvironmentVariable(ValidationOutputVariable);
        List<string> failures = [];
        int successfulAnalyses = 0;
        bool observedAvailableEmptySection = false;
        Dictionary<SignalStatus, int> observedStatuses = [];

        foreach (ValidationRepository repository in repositories)
        {
            try
            {
                GitHubRepositorySnapshotResult result = await orchestrator.CollectAsync(
                    repository.Url,
                    timeout.Token);

                RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
                Assert.True(result.IsSuccessful);
                Assert.Equal(DataAvailabilityStatus.Available, result.MetadataAvailability.Status);
                Assert.Equal(repository.Owner, snapshot.Repository.Owner);
                Assert.Equal(repository.Name, snapshot.Repository.Name);
                Assert.Equal(new Uri(repository.Url), snapshot.Repository.Url);
                Assert.True(snapshot.Repository.IsPublic);
                Assert.Equal(repository.IsArchived, snapshot.Repository.IsArchived);
                Assert.Equal(repository.IsFork, snapshot.Repository.IsFork);
                Assert.Equal(repository.IsTemplate, snapshot.Repository.IsTemplate);
                Assert.Equal(repository.PrimaryLanguage, snapshot.Repository.PrimaryLanguage);
                Assert.Equal(TimeSpan.Zero, snapshot.CapturedAt.Offset);
                Assert.NotNull(snapshot.Metadata);

                string resolvedHeadSha = await GetResolvedHeadShaAsync(
                    httpClient,
                    snapshot.Repository,
                    timeout.Token);
                Assert.Equal(repository.CommitSha, resolvedHeadSha);
                Assert.Equal(repository.CommitSha, snapshot.HeadCommitSha);

                string[] requiredAvailabilityKeys =
                [
                    "metadata",
                    "readme",
                    "file-tree",
                    "project-files",
                    "documentation-markdown",
                    "commits",
                    "issues",
                    "pull-requests",
                    "releases",
                    "tags",
                    "workflows",
                    "license",
                    "languages",
                    "contributors"
                ];

                foreach (string dataKey in requiredAvailabilityKeys)
                {
                    DataAvailability availability = Assert.IsType<DataAvailability>(
                        snapshot.FindAvailability(dataKey));
                    AssertAvailabilityIsCoherent(availability);
                }

                observedAvailableEmptySection |=
                    IsAvailableAndEmpty(snapshot, "issues", snapshot.Issues.Items.Count)
                    || IsAvailableAndEmpty(snapshot, "releases", snapshot.Releases.Items.Count)
                    || IsAvailableAndEmpty(snapshot, "tags", snapshot.Tags.Items.Count)
                    || IsAvailableAndEmpty(snapshot, "workflows", snapshot.Workflows.Items.Count);

                Assert.Contains(
                    snapshot.CollectionWarnings,
                    warning => warning.Contains("may not be atomic", StringComparison.Ordinal));
                string[] warningsBeforeAnalysis = snapshot.CollectionWarnings.ToArray();

                RepositorySignalAnalyzer firstAnalyzer = CreateAnalyzer(EvaluationTime);
                RepositorySignalAnalyzer secondAnalyzer = CreateAnalyzer(EvaluationTime);
                IReadOnlyList<SignalResult> first = firstAnalyzer.Analyze(snapshot);
                IReadOnlyList<SignalResult> second = secondAnalyzer.Analyze(snapshot);

                AssertSignalContract(first);
                AssertEquivalent(first, second);
                AssertNoForbiddenInference(first);
                Assert.Equal(warningsBeforeAnalysis, snapshot.CollectionWarnings);

                foreach (SignalResult signal in first)
                {
                    observedStatuses[signal.Status] =
                        observedStatuses.GetValueOrDefault(signal.Status) + 1;
                }

                if (!string.IsNullOrWhiteSpace(outputRoot))
                {
                    await ValidationArtifactWriter.WriteAsync(
                        outputRoot,
                        repository,
                        snapshot,
                        first,
                        EvaluationTime,
                        timeout.Token);
                }

                successfulAnalyses++;
                output.WriteLine(
                    $"Repository={snapshot.Repository.FullName}; "
                    + $"HeadCommitSha={snapshot.HeadCommitSha}; "
                    + $"Signals={first.Count}; "
                    + $"Archived={snapshot.Repository.IsArchived}; "
                    + $"Fork={snapshot.Repository.IsFork}; "
                    + $"Template={snapshot.Repository.IsTemplate}");
                output.WriteLine($"Sections={FormatAvailability(snapshot)}");
                output.WriteLine($"Statuses={FormatStatuses(first)}");
                output.WriteLine($"Warnings={string.Join(" | ", snapshot.CollectionWarnings)}");
            }
            catch (Exception exception)
            {
                string failure = $"{repository.Id} ({repository.Owner}/{repository.Name}): "
                    + $"{exception.GetType().Name}: {exception.Message}";
                failures.Add(failure);
                output.WriteLine($"FAILURE={failure}");

                if (!string.IsNullOrWhiteSpace(outputRoot))
                {
                    await ValidationArtifactWriter.WriteFailureAsync(
                        outputRoot,
                        repository,
                        exception,
                        CancellationToken.None);
                }
            }
        }

        Assert.Equal(repositories.Count, successfulAnalyses);
        Assert.True(
            observedAvailableEmptySection,
            "The validation set must demonstrate Available + empty separately from unavailable data.");

        const string missingRepositoryUrl =
            "https://github.com/githubproof-validation/definitely-not-a-repository-20260815";
        GitHubRepositorySnapshotResult missing = await orchestrator.CollectAsync(
            missingRepositoryUrl,
            timeout.Token);

        Assert.False(missing.IsSuccessful);
        Assert.Null(missing.Snapshot);
        Assert.Equal(DataAvailabilityStatus.Unavailable, missing.MetadataAvailability.Status);
        output.WriteLine(
            $"NotFound={missingRepositoryUrl}; "
            + $"SnapshotProduced={missing.Snapshot is not null}; "
            + $"Metadata={missing.MetadataAvailability.Status}");
        output.WriteLine(
            "ObservedStatuses="
            + string.Join(
                ", ",
                observedStatuses.OrderBy(item => item.Key).Select(item => $"{item.Key}:{item.Value}")));
        Assert.Empty(failures);
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri("https://api.github.com/"),
            Timeout = TimeSpan.FromMinutes(2)
        };
        string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return httpClient;
    }

    private static GitHubRepositorySnapshotOrchestrator CreateOrchestrator(
        HttpClient httpClient) =>
        new(
            new GitHubRepositoryMetadataCollector(httpClient),
            new GitHubRepositoryContentsCollector(httpClient),
            new GitHubRepositoryDescriptiveDataCollector(httpClient),
            new GitHubRepositoryCommitCollector(httpClient),
            new GitHubRepositoryIssueCollector(httpClient),
            new GitHubRepositoryPullRequestCollector(httpClient),
            new GitHubRepositoryReleaseCollector(httpClient),
            new GitHubRepositoryTagCollector(httpClient),
            new GitHubRepositoryContributorCollector(httpClient));

    private static RepositorySignalAnalyzer CreateAnalyzer(DateTimeOffset evaluationTime)
    {
        FixedTimeProvider timeProvider = new(evaluationTime);
        ISignalEvaluator[] evaluators =
        [
            new ReadmeExistsSignalEvaluator(timeProvider),
            new DescriptionPresentSignalEvaluator(timeProvider),
            new InstallationGuideSignalEvaluator(timeProvider),
            new LicensePresentSignalEvaluator(timeProvider),
            new UsageDemoSignalEvaluator(timeProvider),
            new CommitHistorySignalEvaluator(timeProvider),
            new CommitDistributionSignalEvaluator(timeProvider),
            new IssueUsageSignalEvaluator(timeProvider),
            new PullRequestUsageSignalEvaluator(timeProvider),
            new ReleaseUsageSignalEvaluator(timeProvider),
            new TestStructureSignalEvaluator(timeProvider),
            new ContinuousIntegrationSignalEvaluator(timeProvider),
            new TopicsSignalEvaluator(timeProvider),
            new HomepageLiveDemoSignalEvaluator(timeProvider)
        ];

        return new RepositorySignalAnalyzer(
            evaluators,
            new PresentationCompletenessSignalEvaluator(timeProvider));
    }

    private static void AssertSignalContract(IReadOnlyList<SignalResult> results)
    {
        Assert.Equal(15, results.Count);
        Assert.Equal(
            RepositorySignalAnalyzer.CanonicalIndependentSignalKeys
                .Append(PresentationCompletenessSignalEvaluator.SignalKey),
            results.Select(result => result.SignalKey));
        Assert.Equal(15, results.Select(result => result.SignalKey).Distinct().Count());
        Assert.False(results[^1].IncludeInOverallScore);

        foreach (SignalResult result in results)
        {
            if (result.Status is SignalStatus.NotApplicable
                or SignalStatus.UnavailableEvidence
                or SignalStatus.RequiresHumanReview)
            {
                Assert.Null(result.Score);
            }
            else
            {
                Assert.NotNull(result.Score);
            }

            if (result.Status == SignalStatus.Missing)
            {
                Assert.Equal(0, result.Score);
            }
        }
    }

    private static void AssertEquivalent(
        IReadOnlyList<SignalResult> first,
        IReadOnlyList<SignalResult> second)
    {
        Assert.Equal(first.Count, second.Count);

        for (int index = 0; index < first.Count; index++)
        {
            Assert.Equal(first[index].SignalKey, second[index].SignalKey);
            Assert.Equal(first[index].SignalVersion, second[index].SignalVersion);
            Assert.Equal(first[index].Category, second[index].Category);
            Assert.Equal(first[index].Status, second[index].Status);
            Assert.Equal(first[index].Score, second[index].Score);
            Assert.Equal(first[index].MaximumScore, second[index].MaximumScore);
            Assert.Equal(first[index].IncludeInOverallScore, second[index].IncludeInOverallScore);
            Assert.Equal(first[index].Evidence, second[index].Evidence);
            Assert.Equal(first[index].EvaluatedAt, second[index].EvaluatedAt);
        }
    }

    private static void AssertNoForbiddenInference(IReadOnlyList<SignalResult> results)
    {
        string outputText = string.Join(
            '\n',
            results.SelectMany(result =>
                result.Evidence.SelectMany(evidence => new[]
                    {
                        evidence.Value,
                        evidence.Notes
                    }))
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        string[] forbiddenInferencePhrases =
        [
            "junior developer",
            "senior developer",
            "ai-generated",
            "generated by ai",
            "copied code",
            "kopya kod",
            "kopyalanmış kod",
            "yapay zekâ tarafından"
        ];

        foreach (string phrase in forbiddenInferencePhrases)
        {
            Assert.DoesNotContain(phrase, outputText, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static async Task<string> GetResolvedHeadShaAsync(
        HttpClient httpClient,
        RepositoryIdentity repository,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Get,
            $"repos/{Uri.EscapeDataString(repository.Owner)}/"
            + $"{Uri.EscapeDataString(repository.Name)}/git/ref/heads/"
            + Uri.EscapeDataString(repository.DefaultBranch));
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        using JsonDocument document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        return document.RootElement
            .GetProperty("object")
            .GetProperty("sha")
            .GetString()!;
    }

    private static void AssertAvailabilityIsCoherent(DataAvailability availability)
    {
        if (availability.IsAvailable)
        {
            Assert.NotNull(availability.RetrievedAt);
            Assert.Null(availability.Reason);
        }
        else
        {
            Assert.Null(availability.RetrievedAt);
            Assert.False(string.IsNullOrWhiteSpace(availability.Reason));
        }
    }

    private static bool IsAvailableAndEmpty(
        RepositorySnapshot snapshot,
        string dataKey,
        int itemCount) =>
        snapshot.FindAvailability(dataKey)?.IsAvailable == true && itemCount == 0;

    private static string FormatAvailability(RepositorySnapshot snapshot) =>
        string.Join(
            ", ",
            snapshot.Availability.Select(item => $"{item.DataKey}:{item.Status}"));

    private static string FormatStatuses(IReadOnlyList<SignalResult> results) =>
        string.Join(
            ", ",
            results
                .GroupBy(result => result.Status)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key}:{group.Count()}"));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

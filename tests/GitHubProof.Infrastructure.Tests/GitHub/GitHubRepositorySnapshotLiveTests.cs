using System.Net.Http.Headers;
using System.Text.Json;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using Xunit;
using Xunit.Abstractions;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositorySnapshotLiveTests(ITestOutputHelper output)
{
    private const string LiveValidationVariable = "GITHUBPROOF_RUN_LIVE_TESTS";

    [Fact]
    [Trait("Category", "LiveIntegration")]
    public async Task CollectAsync_ValidatesBoundedPublicRepositoriesAndNotFound()
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

        using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(3));
        using HttpClient httpClient = CreateHttpClient();
        GitHubRepositorySnapshotOrchestrator orchestrator = CreateOrchestrator(httpClient);
        (string Owner, string Name, bool IsArchived)[] repositories =
        [
            ("githubtraining", "github-games", false),
            ("githubtraining", "hellogitworld", true)
        ];
        bool observedAvailableEmptySection = false;

        foreach ((string owner, string name, bool isArchived) in repositories)
        {
            string repositoryUrl = $"https://github.com/{owner}/{name}";
            GitHubRepositorySnapshotResult result = await orchestrator.CollectAsync(
                repositoryUrl,
                timeout.Token);

            RepositorySnapshot snapshot = Assert.IsType<RepositorySnapshot>(result.Snapshot);
            Assert.True(result.IsSuccessful);
            Assert.Equal(DataAvailabilityStatus.Available, result.MetadataAvailability.Status);
            Assert.Equal(owner, snapshot.Repository.Owner);
            Assert.Equal(name, snapshot.Repository.Name);
            Assert.Equal(new Uri(repositoryUrl), snapshot.Repository.Url);
            Assert.True(snapshot.Repository.IsPublic);
            Assert.Equal(isArchived, snapshot.Repository.IsArchived);
            Assert.Equal(TimeSpan.Zero, snapshot.CapturedAt.Offset);
            Assert.False(string.IsNullOrWhiteSpace(snapshot.HeadCommitSha));
            Assert.NotNull(snapshot.Metadata);

            string resolvedHeadSha = await GetResolvedHeadShaAsync(
                httpClient,
                snapshot.Repository,
                timeout.Token);
            Assert.Equal(resolvedHeadSha, snapshot.HeadCommitSha);

            string[] requiredAvailabilityKeys =
            [
                "metadata",
                "readme",
                "file-tree",
                "project-files",
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

            output.WriteLine(
                $"Repository={snapshot.Repository.FullName}; "
                + $"HeadCommitSha={snapshot.HeadCommitSha}; "
                + $"Archived={snapshot.Repository.IsArchived}; "
                + $"Fork={snapshot.Repository.IsFork}; "
                + $"Template={snapshot.Repository.IsTemplate}");
            output.WriteLine($"Sections={FormatAvailability(snapshot)}");
            output.WriteLine($"Warnings={string.Join(" | ", snapshot.CollectionWarnings)}");
        }

        Assert.True(
            observedAvailableEmptySection,
            "The bounded live set must demonstrate Available + empty separately from unavailable data.");

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
}

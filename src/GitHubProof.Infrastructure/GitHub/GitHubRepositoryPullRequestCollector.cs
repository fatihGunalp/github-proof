using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Collection;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryPullRequestCollector : IGitHubRepositoryPullRequestCollector
{
    private const string PullRequestsDataKey = "pull-requests";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    // Null for every composition that must enumerate fully. Only the
    // Profile V1 composition supplies a boundary, which is why Repository
    // Proof cannot inherit early stopping.
    private readonly CountCollectionDecisionBoundary? decisionBoundary;

    public GitHubRepositoryPullRequestCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null,
        CountCollectionDecisionBoundary? decisionBoundary = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.decisionBoundary = decisionBoundary;
    }

    public async Task<GitHubRepositoryPullRequestCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(metadata);

        if (!metadata.IsAvailable || metadata.Repository is null)
        {
            return CreateNotRequestedResult();
        }

        List<GitHubRepositoryPullRequest> pullRequests = [];
        HashSet<int> observedNumbers = [];
        Uri? nextPage = CreateInitialPageUri(repository);

        while (nextPage is not null)
        {
            PageResult page = await CollectPageAsync(nextPage, cancellationToken);

            if (page.Availability is not null)
            {
                return CreateIncompleteResult(pullRequests, page.Availability);
            }

            foreach (GitHubRepositoryPullRequest pullRequest in page.PullRequests!)
            {
                if (observedNumbers.Add(pullRequest.Number))
                {
                    pullRequests.Add(pullRequest);
                }
            }

            // The observed prefix already decides every authorized consumer of
            // this surface, and another page exists, so stopping here is a
            // deliberate bound rather than an incomplete collection.
            if (page.NextPage is not null && decisionBoundary?.Invoke(pullRequests.Count(pullRequest => pullRequest.IsMerged)) == true)
            {
                return CreateDecisionBoundedResult(pullRequests);
            }

            nextPage = page.NextPage;
        }

        return new GitHubRepositoryPullRequestCollection(
            new RepositoryCollection<GitHubRepositoryPullRequest>(pullRequests),
            DataAvailability.Available(PullRequestsDataKey, timeProvider.GetUtcNow()),
            Array.Empty<string>());
    }

    private async Task<PageResult> CollectPageAsync(
        Uri pageUri,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(pageUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return PageResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(response, PullRequestsDataKey));
            }

            List<GitHubPullRequestResponse>? responseModel = await response.Content
                .ReadFromJsonAsync<List<GitHubPullRequestResponse>>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return PageResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty pull requests response."));
            }

            return new PageResult(
                responseModel.Select(NormalizePullRequest).ToArray(),
                GitHubPagination.FindNextPage(response),
                Availability: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub pull requests request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub pull requests response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub pull requests response could not be normalized: {exception.Message}"));
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        Uri pageUri,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, pageUri);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        return await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
    }

    private static Uri CreateInitialPageUri(GitHubRepositoryUrl repository) =>
        new(
            $"https://api.github.com/repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/pulls?state=all&per_page=100",
            UriKind.Absolute);

    private static GitHubRepositoryPullRequest NormalizePullRequest(
        GitHubPullRequestResponse response) =>
        new(
            RequirePositive(response.Number, "number"),
            RequireValue(response.Title, "title"),
            IsClosed(response.State, "state"),
            response.MergedAt is not null,
            RequireTimestamp(response.CreatedAt, "created_at"));

    private static GitHubRepositoryPullRequestCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before pull requests are collected.";

        return new GitHubRepositoryPullRequestCollection(
            new RepositoryCollection<GitHubRepositoryPullRequest>(isComplete: false),
            DataAvailability.Unavailable(
                PullRequestsDataKey,
                DataAvailabilityStatus.NotRequested,
                reason),
            [reason]);
    }

    // A deliberate bound is not a failure: availability stays Available and
    // no collection warning is produced, so RepositoryAnalysis.Status cannot
    // be downgraded by the bound itself.
    private GitHubRepositoryPullRequestCollection CreateDecisionBoundedResult(
        IEnumerable<GitHubRepositoryPullRequest> pullRequests) =>
        new(
            RepositoryCollection<GitHubRepositoryPullRequest>.DecisionBounded(pullRequests),
            DataAvailability.Available(PullRequestsDataKey, timeProvider.GetUtcNow()),
            Array.Empty<string>());

    private static GitHubRepositoryPullRequestCollection CreateIncompleteResult(
        IEnumerable<GitHubRepositoryPullRequest> pullRequests,
        DataAvailability availability) =>
        new(
            new RepositoryCollection<GitHubRepositoryPullRequest>(pullRequests, isComplete: false),
            availability,
            ["Pull request collection did not complete; collected pull requests are partial."]);

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(
            PullRequestsDataKey,
            DataAvailabilityStatus.Failed,
            reason);

    private static int RequirePositive(int? value, string name)
    {
        if (value is null || value <= 0)
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' must be positive.",
                name);
        }

        return value.Value;
    }

    private static string RequireValue(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' is required.",
                name);
        }

        return value.Trim();
    }

    private static bool IsClosed(string? value, string name) =>
        value switch
        {
            "open" => false,
            "closed" => true,
            _ => throw new ArgumentException(
                $"GitHub response field '{name}' must be 'open' or 'closed'.",
                name)
        };

    private static DateTimeOffset RequireTimestamp(DateTimeOffset? value, string name)
    {
        if (value is null)
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' is required.",
                name);
        }

        return value.Value;
    }

    private sealed record PageResult(
        IReadOnlyList<GitHubRepositoryPullRequest>? PullRequests,
        Uri? NextPage,
        DataAvailability? Availability)
    {
        public static PageResult Failed(DataAvailability availability) =>
            new(PullRequests: null, NextPage: null, availability);
    }

    private sealed record GitHubPullRequestResponse(
        [property: JsonPropertyName("number")] int? Number,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("state")] string? State,
        [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
        [property: JsonPropertyName("merged_at")] DateTimeOffset? MergedAt);
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryCommitCollector
{
    private const string CommitsDataKey = "commits";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositoryCommitCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositoryCommitCollection> CollectAsync(
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

        List<RepositoryCommit> commits = [];
        HashSet<string> observedShas = new(StringComparer.Ordinal);
        Uri? nextPage = CreateInitialPageUri(repository, metadata.Repository.DefaultBranch);

        while (nextPage is not null)
        {
            PageResult page = await CollectPageAsync(nextPage, cancellationToken);

            if (page.Availability is not null)
            {
                return CreateIncompleteResult(commits, page.Availability);
            }

            foreach (RepositoryCommit commit in page.Commits!)
            {
                if (observedShas.Add(commit.Sha))
                {
                    commits.Add(commit);
                }
            }

            nextPage = page.NextPage;
        }

        return new GitHubRepositoryCommitCollection(
            new RepositoryCollection<RepositoryCommit>(commits),
            DataAvailability.Available(CommitsDataKey, timeProvider.GetUtcNow()),
            Array.Empty<string>());
    }

    private async Task<PageResult> CollectPageAsync(
        Uri pageUri,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(pageUri, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                GitHubErrorResponse? error = await response.Content
                    .ReadFromJsonAsync<GitHubErrorResponse>(JsonOptions, cancellationToken);

                if (string.Equals(
                        error?.Message,
                        "Git Repository is empty.",
                        StringComparison.Ordinal))
                {
                    return PageResult.Empty;
                }

                return PageResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(response, CommitsDataKey));
            }

            if (!response.IsSuccessStatusCode)
            {
                return PageResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(response, CommitsDataKey));
            }

            List<GitHubCommitResponse>? responseModel = await response.Content
                .ReadFromJsonAsync<List<GitHubCommitResponse>>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return PageResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty commits response."));
            }

            return new PageResult(
                responseModel.Select(NormalizeCommit).ToArray(),
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
                $"GitHub commits request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub commits response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub commits response could not be normalized: {exception.Message}"));
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

    private static Uri CreateInitialPageUri(
        GitHubRepositoryUrl repository,
        string defaultBranch) =>
        new(
            $"https://api.github.com/repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/commits?sha={Uri.EscapeDataString(defaultBranch)}&per_page=100",
            UriKind.Absolute);

    private static RepositoryCommit NormalizeCommit(GitHubCommitResponse response) =>
        new(
            RequireValue(response.Sha, "sha"),
            RequireTimestamp(response.Commit?.Committer?.Date, "commit.committer.date"),
            message: null,
            authorLogin: null);

    private static GitHubRepositoryCommitCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before commits are collected.";

        return new GitHubRepositoryCommitCollection(
            new RepositoryCollection<RepositoryCommit>(isComplete: false),
            DataAvailability.Unavailable(
                CommitsDataKey,
                DataAvailabilityStatus.NotRequested,
                reason),
            [reason]);
    }

    private static GitHubRepositoryCommitCollection CreateIncompleteResult(
        IEnumerable<RepositoryCommit> commits,
        DataAvailability availability) =>
        new(
            new RepositoryCollection<RepositoryCommit>(commits, isComplete: false),
            availability,
            ["Commit collection did not complete; collected commits are partial."]);

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(
            CommitsDataKey,
            DataAvailabilityStatus.Failed,
            reason);

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
        IReadOnlyList<RepositoryCommit>? Commits,
        Uri? NextPage,
        DataAvailability? Availability)
    {
        public static PageResult Empty { get; } = new([], NextPage: null, Availability: null);

        public static PageResult Failed(DataAvailability availability) =>
            new(Commits: null, NextPage: null, availability);
    }

    private sealed record GitHubCommitResponse(
        [property: JsonPropertyName("sha")] string? Sha,
        [property: JsonPropertyName("commit")] GitHubCommitDetails? Commit);

    private sealed record GitHubCommitDetails(
        [property: JsonPropertyName("committer")] GitHubCommitActor? Committer);

    private sealed record GitHubCommitActor(
        [property: JsonPropertyName("date")] DateTimeOffset? Date);

    private sealed record GitHubErrorResponse(
        [property: JsonPropertyName("message")] string? Message);
}

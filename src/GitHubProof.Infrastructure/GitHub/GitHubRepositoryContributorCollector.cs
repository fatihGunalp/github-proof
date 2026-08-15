using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryContributorCollector : IGitHubRepositoryContributorCollector
{
    private const string ContributorsDataKey = "contributors";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositoryContributorCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositoryContributorCollection> CollectAsync(
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

        List<RepositoryContributor> contributors = [];
        HashSet<string> observedLogins = new(StringComparer.OrdinalIgnoreCase);
        Uri? nextPage = CreateInitialPageUri(repository);

        while (nextPage is not null)
        {
            PageResult page = await CollectPageAsync(nextPage, cancellationToken);

            if (page.Availability is not null)
            {
                return CreateIncompleteResult(contributors, page.Availability);
            }

            foreach (RepositoryContributor contributor in page.Contributors!)
            {
                if (observedLogins.Add(contributor.Login))
                {
                    contributors.Add(contributor);
                }
            }

            nextPage = page.NextPage;
        }

        return new GitHubRepositoryContributorCollection(
            new RepositoryCollection<RepositoryContributor>(contributors),
            DataAvailability.Available(ContributorsDataKey, timeProvider.GetUtcNow()),
            Array.Empty<string>());
    }

    private async Task<PageResult> CollectPageAsync(Uri pageUri, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(pageUri, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return PageResult.Empty;
            }

            if (!response.IsSuccessStatusCode)
            {
                return PageResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(response, ContributorsDataKey));
            }

            List<GitHubContributorResponse>? responseModel = await response.Content
                .ReadFromJsonAsync<List<GitHubContributorResponse>>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return PageResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty contributors response."));
            }

            return new PageResult(
                responseModel.Select(NormalizeContributor).ToArray(),
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
                $"GitHub contributors request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub contributors response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub contributors response could not be normalized: {exception.Message}"));
        }
    }

    private async Task<HttpResponseMessage> SendAsync(Uri pageUri, CancellationToken cancellationToken)
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
            $"https://api.github.com/repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/contributors?per_page=100",
            UriKind.Absolute);

    private static RepositoryContributor NormalizeContributor(GitHubContributorResponse response) =>
        new(
            RequireValue(response.Login, "login"),
            RequireNonNegative(response.Contributions, "contributions"));

    private static GitHubRepositoryContributorCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before contributors are collected.";

        return new GitHubRepositoryContributorCollection(
            new RepositoryCollection<RepositoryContributor>(isComplete: false),
            DataAvailability.Unavailable(
                ContributorsDataKey,
                DataAvailabilityStatus.NotRequested,
                reason),
            [reason]);
    }

    private static GitHubRepositoryContributorCollection CreateIncompleteResult(
        IEnumerable<RepositoryContributor> contributors,
        DataAvailability availability) =>
        new(
            new RepositoryCollection<RepositoryContributor>(contributors, isComplete: false),
            availability,
            ["Contributor collection did not complete; collected contributors are partial."]);

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(ContributorsDataKey, DataAvailabilityStatus.Failed, reason);

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

    private static int RequireNonNegative(int? value, string name)
    {
        if (value is null || value < 0)
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' must be non-negative.",
                name);
        }

        return value.Value;
    }

    private sealed record PageResult(
        IReadOnlyList<RepositoryContributor>? Contributors,
        Uri? NextPage,
        DataAvailability? Availability)
    {
        public static PageResult Empty { get; } = new([], NextPage: null, Availability: null);

        public static PageResult Failed(DataAvailability availability) =>
            new(Contributors: null, NextPage: null, availability);
    }

    private sealed record GitHubContributorResponse(
        [property: JsonPropertyName("login")] string? Login,
        [property: JsonPropertyName("contributions")] int? Contributions);
}

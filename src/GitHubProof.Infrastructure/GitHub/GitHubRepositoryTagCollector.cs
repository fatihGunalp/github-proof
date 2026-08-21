using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Collection;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryTagCollector : IGitHubRepositoryTagCollector
{
    private const string TagsDataKey = "tags";
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

    public GitHubRepositoryTagCollector(HttpClient httpClient, TimeProvider? timeProvider = null,
        CountCollectionDecisionBoundary? decisionBoundary = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.decisionBoundary = decisionBoundary;
    }

    public async Task<GitHubRepositoryTagCollection> CollectAsync(
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

        List<GitHubRepositoryTag> tags = [];
        HashSet<string> observedNames = new(StringComparer.OrdinalIgnoreCase);
        Uri? nextPage = CreateInitialPageUri(repository);

        while (nextPage is not null)
        {
            PageResult page = await CollectPageAsync(nextPage, cancellationToken);

            if (page.Availability is not null)
            {
                return CreateIncompleteResult(tags, page.Availability);
            }

            foreach (GitHubRepositoryTag tag in page.Tags!)
            {
                if (observedNames.Add(tag.Name))
                {
                    tags.Add(tag);
                }
            }

            // The observed prefix already decides every authorized consumer of
            // this surface, and another page exists, so stopping here is a
            // deliberate bound rather than an incomplete collection.
            if (page.NextPage is not null && decisionBoundary?.Invoke(tags.Count) == true)
            {
                return CreateDecisionBoundedResult(tags);
            }

            nextPage = page.NextPage;
        }

        return new GitHubRepositoryTagCollection(
            new RepositoryCollection<GitHubRepositoryTag>(tags),
            DataAvailability.Available(TagsDataKey, timeProvider.GetUtcNow()),
            Array.Empty<string>());
    }

    private async Task<PageResult> CollectPageAsync(Uri pageUri, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(pageUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return PageResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(response, TagsDataKey));
            }

            List<GitHubTagResponse>? responseModel = await response.Content
                .ReadFromJsonAsync<List<GitHubTagResponse>>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return PageResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty tags response."));
            }

            return new PageResult(
                responseModel.Select(tag => new GitHubRepositoryTag(RequireValue(tag.Name, "name"))).ToArray(),
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
                $"GitHub tags request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub tags response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub tags response could not be normalized: {exception.Message}"));
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
            $"https://api.github.com/repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/tags?per_page=100",
            UriKind.Absolute);

    private static GitHubRepositoryTagCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before tags are collected.";

        return new GitHubRepositoryTagCollection(
            new RepositoryCollection<GitHubRepositoryTag>(isComplete: false),
            DataAvailability.Unavailable(
                TagsDataKey,
                DataAvailabilityStatus.NotRequested,
                reason),
            [reason]);
    }

    // A deliberate bound is not a failure: availability stays Available and
    // no collection warning is produced, so RepositoryAnalysis.Status cannot
    // be downgraded by the bound itself.
    private GitHubRepositoryTagCollection CreateDecisionBoundedResult(
        IEnumerable<GitHubRepositoryTag> tags) =>
        new(
            RepositoryCollection<GitHubRepositoryTag>.DecisionBounded(tags),
            DataAvailability.Available(TagsDataKey, timeProvider.GetUtcNow()),
            Array.Empty<string>());

    private static GitHubRepositoryTagCollection CreateIncompleteResult(
        IEnumerable<GitHubRepositoryTag> tags,
        DataAvailability availability) =>
        new(
            new RepositoryCollection<GitHubRepositoryTag>(tags, isComplete: false),
            availability,
            ["Tag collection did not complete; collected tags are partial."]);

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(TagsDataKey, DataAvailabilityStatus.Failed, reason);

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

    private sealed record PageResult(
        IReadOnlyList<GitHubRepositoryTag>? Tags,
        Uri? NextPage,
        DataAvailability? Availability)
    {
        public static PageResult Failed(DataAvailability availability) =>
            new(Tags: null, NextPage: null, availability);
    }

    private sealed record GitHubTagResponse(
        [property: JsonPropertyName("name")] string? Name);
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryReleaseCollector
{
    private const string ReleasesDataKey = "releases";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositoryReleaseCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositoryReleaseCollection> CollectAsync(
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

        List<RepositoryRelease> releases = [];
        HashSet<string> observedTagNames = new(StringComparer.OrdinalIgnoreCase);
        Uri? nextPage = CreateInitialPageUri(repository);

        while (nextPage is not null)
        {
            PageResult page = await CollectPageAsync(nextPage, cancellationToken);

            if (page.Availability is not null)
            {
                return CreateIncompleteResult(releases, page.Availability);
            }

            foreach (RepositoryRelease release in page.Releases!)
            {
                if (observedTagNames.Add(release.TagName))
                {
                    releases.Add(release);
                }
            }

            nextPage = page.NextPage;
        }

        return new GitHubRepositoryReleaseCollection(
            new RepositoryCollection<RepositoryRelease>(releases),
            DataAvailability.Available(ReleasesDataKey, timeProvider.GetUtcNow()),
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
                    GitHubApiAvailability.FromFailureResponse(response, ReleasesDataKey));
            }

            List<GitHubReleaseResponse>? responseModel = await response.Content
                .ReadFromJsonAsync<List<GitHubReleaseResponse>>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return PageResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty releases response."));
            }

            return new PageResult(
                responseModel.Select(NormalizeRelease).ToArray(),
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
                $"GitHub releases request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub releases response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub releases response could not be normalized: {exception.Message}"));
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
            $"https://api.github.com/repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/releases?per_page=100",
            UriKind.Absolute);

    private static RepositoryRelease NormalizeRelease(GitHubReleaseResponse response)
    {
        bool isDraft = response.Draft ?? throw new ArgumentException(
            "GitHub response field 'draft' is required.",
            nameof(response));

        return new RepositoryRelease(
            RequireValue(response.TagName, "tag_name"),
            hasPublishedRelease: !isDraft,
            isDraft,
            response.Name,
            response.Body,
            response.PublishedAt);
    }

    private static GitHubRepositoryReleaseCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before releases are collected.";

        return new GitHubRepositoryReleaseCollection(
            new RepositoryCollection<RepositoryRelease>(isComplete: false),
            DataAvailability.Unavailable(
                ReleasesDataKey,
                DataAvailabilityStatus.NotRequested,
                reason),
            [reason]);
    }

    private static GitHubRepositoryReleaseCollection CreateIncompleteResult(
        IEnumerable<RepositoryRelease> releases,
        DataAvailability availability) =>
        new(
            new RepositoryCollection<RepositoryRelease>(releases, isComplete: false),
            availability,
            ["Release collection did not complete; collected releases are partial."]);

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(ReleasesDataKey, DataAvailabilityStatus.Failed, reason);

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
        IReadOnlyList<RepositoryRelease>? Releases,
        Uri? NextPage,
        DataAvailability? Availability)
    {
        public static PageResult Failed(DataAvailability availability) =>
            new(Releases: null, NextPage: null, availability);
    }

    private sealed record GitHubReleaseResponse(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("draft")] bool? Draft,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("body")] string? Body,
        [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt);
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed class GitHubProfileRepositoryCollector : IGitHubProfileRepositoryCollector
{
    internal const string ProfileRepositoriesDataKey = "profile-repositories";
    public const int DefaultMaxPageCount = 10;
    private const int PageSize = 100;
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;
    private readonly int maxPageCount;

    public GitHubProfileRepositoryCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null,
        int maxPageCount = DefaultMaxPageCount)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;

        if (maxPageCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPageCount),
                maxPageCount,
                "Page bound must be greater than zero.");
        }

        this.maxPageCount = maxPageCount;
    }

    public async Task<GitHubProfileRepositoryCollection> CollectAsync(
        GitHubProfileHandle profile,
        GitHubProfileMetadataCollection metadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(metadata);

        if (!metadata.IsAvailable || metadata.Identity is null)
        {
            return CreateNotRequestedResult();
        }

        // The collected profile identity is authoritative: GitHub redirects renamed
        // accounts and answers with the canonical login, which may differ from the
        // requested handle.
        List<ProfileRepository> repositories = [];
        Dictionary<ProfileRepositoryReference, string?> descriptionsByRepository = [];
        HashSet<string> observedRepositories = new(StringComparer.Ordinal);
        Uri? nextPage = CreateInitialPageUri(metadata.Identity.Login);
        int collectedPages = 0;

        while (nextPage is not null)
        {
            if (collectedPages == maxPageCount)
            {
                return CreateBoundedResult(
                    repositories,
                    descriptionsByRepository,
                    timeProvider.GetUtcNow());
            }

            PageResult page = await CollectPageAsync(nextPage, cancellationToken);
            collectedPages++;

            if (page.Availability is not null)
            {
                return CreateIncompleteResult(
                    repositories,
                    descriptionsByRepository,
                    page.Availability);
            }

            foreach (NormalizedRepository normalized in page.Repositories!)
            {
                if (observedRepositories.Add(normalized.Repository.Reference.FullName))
                {
                    repositories.Add(normalized.Repository);
                    descriptionsByRepository[normalized.Repository.Reference] = normalized.Description;
                }
            }

            nextPage = page.NextPage;
        }

        return new GitHubProfileRepositoryCollection(
            new ProfileRepositoryManifest(repositories),
            DataAvailability.Available(
                ProfileRepositoriesDataKey,
                timeProvider.GetUtcNow()),
            Array.Empty<string>(),
            new RepositoryPresentationMetadata(descriptionsByRepository));
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
                    GitHubApiAvailability.FromFailureResponse(
                        response,
                        ProfileRepositoriesDataKey));
            }

            List<GitHubProfileRepositoryResponse>? responseModel = await response.Content
                .ReadFromJsonAsync<List<GitHubProfileRepositoryResponse>>(
                    JsonOptions,
                    cancellationToken);

            if (responseModel is null)
            {
                return PageResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty profile repositories response."));
            }

            return new PageResult(
                responseModel
                    .Where(repository => !repository.Private)
                    .Select(NormalizeRepository)
                    .ToArray(),
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
                $"GitHub profile repositories request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub profile repositories response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return PageResult.Failed(CreateFailedAvailability(
                $"GitHub profile repositories response could not be normalized: {exception.Message}"));
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

    private static Uri CreateInitialPageUri(string login) =>
        new(
            $"https://api.github.com/users/{Uri.EscapeDataString(login)}/repos?type=owner&sort=full_name&direction=asc&per_page={PageSize}",
            UriKind.Absolute);

    private static NormalizedRepository NormalizeRepository(
        GitHubProfileRepositoryResponse response)
    {
        string owner = RequireValue(response.Owner?.Login, "owner.login");
        string name = RequireValue(response.Name, "name");

        ProfileRepository repository = new(
            new ProfileRepositoryReference(
                owner,
                name,
                CreateCanonicalRepositoryUri(owner, name)),
            response.DefaultBranch,
            response.Language,
            FromFlag(response.Fork),
            FromFlag(response.IsTemplate),
            FromFlag(response.Archived),
            DetermineEmptyState(response.Size, response.PushedAt),
            response.PushedAt,
            response.CreatedAt);

        return new NormalizedRepository(repository, NormalizeDescription(response.Description));
    }

    // The provider-returned repository Description: preserved verbatim (no
    // grammar rewrite, translation, summarization or inference), trimmed of
    // surrounding whitespace only. Null/whitespace is a legitimate absence,
    // never a collection failure.
    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static RepositoryConditionState FromFlag(bool? value) =>
        value switch
        {
            null => RepositoryConditionState.Undetermined,
            true => RepositoryConditionState.Present,
            false => RepositoryConditionState.Absent
        };

    private static RepositoryConditionState DetermineEmptyState(
        long? size,
        DateTimeOffset? pushedAt)
    {
        if (size is null)
        {
            return RepositoryConditionState.Undetermined;
        }

        return size == 0 && pushedAt is null
            ? RepositoryConditionState.Present
            : RepositoryConditionState.Absent;
    }

    private static GitHubProfileRepositoryCollection CreateNotRequestedResult()
    {
        const string reason =
            "Profile metadata must be available before profile repositories are collected.";

        return new GitHubProfileRepositoryCollection(
            new ProfileRepositoryManifest(isComplete: false),
            DataAvailability.Unavailable(
                ProfileRepositoriesDataKey,
                DataAvailabilityStatus.NotRequested,
                reason),
            [reason],
            RepositoryPresentationMetadata.Empty);
    }

    private static GitHubProfileRepositoryCollection CreateIncompleteResult(
        IEnumerable<ProfileRepository> repositories,
        IEnumerable<KeyValuePair<ProfileRepositoryReference, string?>> descriptionsByRepository,
        DataAvailability availability) =>
        new(
            new ProfileRepositoryManifest(repositories, isComplete: false),
            availability,
            [
                "Profile repository collection did not complete; the repository manifest is partial."
            ],
            new RepositoryPresentationMetadata(descriptionsByRepository));

    private GitHubProfileRepositoryCollection CreateBoundedResult(
        IEnumerable<ProfileRepository> repositories,
        IEnumerable<KeyValuePair<ProfileRepositoryReference, string?>> descriptionsByRepository,
        DateTimeOffset retrievedAt) =>
        new(
            new ProfileRepositoryManifest(repositories, isComplete: false),
            DataAvailability.Available(ProfileRepositoriesDataKey, retrievedAt),
            [
                $"Profile repository collection stopped at the configured bound of {maxPageCount} page(s); the repository manifest is partial."
            ],
            new RepositoryPresentationMetadata(descriptionsByRepository));

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(
            ProfileRepositoriesDataKey,
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

    private static Uri CreateCanonicalRepositoryUri(string owner, string name) =>
        new(
            $"https://github.com/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(name)}",
            UriKind.Absolute);

    private sealed record PageResult(
        IReadOnlyList<NormalizedRepository>? Repositories,
        Uri? NextPage,
        DataAvailability? Availability)
    {
        public static PageResult Failed(DataAvailability availability) =>
            new(Repositories: null, NextPage: null, availability);
    }

    // A normalized repository paired with its provider-returned Description,
    // kept together only long enough to populate the manifest and the
    // parallel RepositoryPresentationMetadata lookup in lockstep.
    private sealed record NormalizedRepository(
        ProfileRepository Repository,
        string? Description);

    private sealed record GitHubProfileRepositoryResponse(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("owner")] GitHubProfileRepositoryOwner? Owner,
        [property: JsonPropertyName("private")] bool Private,
        [property: JsonPropertyName("fork")] bool? Fork,
        [property: JsonPropertyName("is_template")] bool? IsTemplate,
        [property: JsonPropertyName("archived")] bool? Archived,
        [property: JsonPropertyName("size")] long? Size,
        [property: JsonPropertyName("language")] string? Language,
        [property: JsonPropertyName("default_branch")] string? DefaultBranch,
        [property: JsonPropertyName("pushed_at")] DateTimeOffset? PushedAt,
        [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record GitHubProfileRepositoryOwner(
        [property: JsonPropertyName("login")] string? Login);
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryMetadataCollector : IGitHubRepositoryMetadataCollector
{
    // Internal rather than private since Session 127, so the Profile hybrid
    // metadata path names the same data key by reference instead of repeating
    // the literal. The value and its meaning are unchanged.
    internal const string MetadataDataKey = "metadata";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositoryMetadataCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositoryMetadataCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        using HttpRequestMessage request = CreateRequest(repository);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return CreateUnavailableResult(response);
            }

            GitHubRepositoryResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubRepositoryResponse>(
                    JsonOptions,
                    cancellationToken);

            if (responseModel is null)
            {
                return CreateFailedResult("GitHub returned an empty repository metadata response.");
            }

            return Normalize(responseModel, timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return CreateFailedResult($"GitHub metadata request failed: {exception.Message}");
        }
        catch (JsonException exception)
        {
            return CreateFailedResult($"GitHub metadata response could not be read: {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            return CreateFailedResult($"GitHub metadata response could not be normalized: {exception.Message}");
        }
    }

    private static HttpRequestMessage CreateRequest(GitHubRepositoryUrl repository)
    {
        HttpRequestMessage request = new(
            HttpMethod.Get,
            $"repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}");

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        return request;
    }

    private static GitHubRepositoryMetadataCollection Normalize(
        GitHubRepositoryResponse response,
        DateTimeOffset retrievedAt)
    {
        if (response.Private)
        {
            return new GitHubRepositoryMetadataCollection(
                Repository: null,
                Metadata: null,
                Topics: Array.Empty<string>(),
                License: null,
                DataAvailability.Unavailable(
                    MetadataDataKey,
                    DataAvailabilityStatus.NotApplicable,
                    "Private repositories are outside the MVP scope."));
        }

        RepositoryIdentity repository = new(
            RequireResponseValue(response.Owner?.Login, "owner.login"),
            RequireResponseValue(response.Name, "name"),
            CreateCanonicalRepositoryUri(response.Owner!.Login!, response.Name!),
            RequireResponseValue(response.DefaultBranch, "default_branch"),
            isPublic: true,
            response.Archived,
            response.Fork,
            response.IsTemplate,
            response.Language);
        RepositoryMetadata metadata = new(
            response.Description,
            response.Homepage,
            RequireResponseTimestamp(response.CreatedAt, "created_at"),
            response.HasIssues);

        return new GitHubRepositoryMetadataCollection(
            repository,
            metadata,
            NormalizeTopics(response.Topics),
            response.License is null
                ? null
                : new RepositoryLicense(
                    RequireResponseValue(response.License.Key, "license.key"),
                    RequireResponseValue(response.License.Name, "license.name"),
                    response.License.SpdxIdentifier,
                    path: null),
            DataAvailability.Available(MetadataDataKey, retrievedAt));
    }

    private static GitHubRepositoryMetadataCollection CreateUnavailableResult(
        HttpResponseMessage response)
        => new(
            Repository: null,
            Metadata: null,
            Topics: Array.Empty<string>(),
            License: null,
            GitHubApiAvailability.FromFailureResponse(response, MetadataDataKey));

    private static GitHubRepositoryMetadataCollection CreateFailedResult(string reason) =>
        new(
            Repository: null,
            Metadata: null,
            Topics: Array.Empty<string>(),
            License: null,
            DataAvailability.Unavailable(
                MetadataDataKey,
                DataAvailabilityStatus.Failed,
                reason));

    private static IReadOnlyList<string> NormalizeTopics(IEnumerable<string>? topics) =>
        topics?
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => topic.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    private static string RequireResponseValue(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' is required.",
                name);
        }

        return value.Trim();
    }

    private static DateTimeOffset RequireResponseTimestamp(
        DateTimeOffset? value,
        string name)
    {
        if (value is null)
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' is required.",
                name);
        }

        return value.Value;
    }

    private static Uri CreateCanonicalRepositoryUri(string owner, string name) =>
        new(
            $"https://github.com/{Uri.EscapeDataString(owner.Trim())}/{Uri.EscapeDataString(name.Trim())}",
            UriKind.Absolute);

    private sealed record GitHubRepositoryResponse(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("owner")] GitHubRepositoryOwner? Owner,
        [property: JsonPropertyName("private")] bool Private,
        [property: JsonPropertyName("archived")] bool Archived,
        [property: JsonPropertyName("fork")] bool Fork,
        [property: JsonPropertyName("is_template")] bool IsTemplate,
        [property: JsonPropertyName("language")] string? Language,
        [property: JsonPropertyName("default_branch")] string? DefaultBranch,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("homepage")] string? Homepage,
        [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
        [property: JsonPropertyName("has_issues")] bool HasIssues,
        [property: JsonPropertyName("topics")] IReadOnlyList<string>? Topics,
        [property: JsonPropertyName("license")] GitHubRepositoryLicense? License);

    private sealed record GitHubRepositoryOwner(
        [property: JsonPropertyName("login")] string? Login);

    private sealed record GitHubRepositoryLicense(
        [property: JsonPropertyName("key")] string? Key,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("spdx_id")] string? SpdxIdentifier);
}

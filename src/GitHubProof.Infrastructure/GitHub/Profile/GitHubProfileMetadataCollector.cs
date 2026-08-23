using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

public sealed class GitHubProfileMetadataCollector : IGitHubProfileMetadataCollector
{
    internal const string ProfileDataKey = "profile";
    private const string UserAccountType = "User";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubProfileMetadataCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubProfileMetadataCollection> CollectAsync(
        GitHubProfileHandle profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        using HttpRequestMessage request = CreateRequest(profile);

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

            GitHubProfileResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubProfileResponse>(
                    JsonOptions,
                    cancellationToken);

            if (responseModel is null)
            {
                return CreateFailedResult("GitHub returned an empty profile response.");
            }

            return Normalize(responseModel, timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return CreateFailedResult($"GitHub profile request failed: {exception.Message}");
        }
        catch (JsonException exception)
        {
            return CreateFailedResult($"GitHub profile response could not be read: {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            return CreateFailedResult($"GitHub profile response could not be normalized: {exception.Message}");
        }
    }

    private static HttpRequestMessage CreateRequest(GitHubProfileHandle profile)
    {
        HttpRequestMessage request = new(
            HttpMethod.Get,
            $"users/{Uri.EscapeDataString(profile.Login)}");

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        return request;
    }

    private static GitHubProfileMetadataCollection Normalize(
        GitHubProfileResponse response,
        DateTimeOffset retrievedAt)
    {
        if (!string.Equals(response.Type, UserAccountType, StringComparison.Ordinal))
        {
            return new GitHubProfileMetadataCollection(
                Identity: null,
                Metadata: null,
                DataAvailability.Unavailable(
                    ProfileDataKey,
                    DataAvailabilityStatus.NotApplicable,
                    "Only public GitHub user profiles are within scope."));
        }

        string login = RequireResponseValue(response.Login, "login");
        GitHubProfileIdentity identity = new(
            login,
            CreateCanonicalProfileUri(login));
        GitHubProfileMetadata metadata = new(
            response.Name,
            RequireResponseTimestamp(response.CreatedAt, "created_at"),
            RequireResponseCount(response.PublicRepositoryCount, "public_repos"),
            RequireResponseAbsoluteUri(response.AvatarUrl, "avatar_url"));

        return new GitHubProfileMetadataCollection(
            identity,
            metadata,
            DataAvailability.Available(ProfileDataKey, retrievedAt));
    }

    private static GitHubProfileMetadataCollection CreateUnavailableResult(
        HttpResponseMessage response)
        => new(
            Identity: null,
            Metadata: null,
            GitHubApiAvailability.FromFailureResponse(response, ProfileDataKey));

    private static GitHubProfileMetadataCollection CreateFailedResult(string reason) =>
        new(
            Identity: null,
            Metadata: null,
            DataAvailability.Unavailable(
                ProfileDataKey,
                DataAvailabilityStatus.Failed,
                reason));

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

    private static int RequireResponseCount(int? value, string name)
    {
        if (value is null)
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' is required.",
                name);
        }

        return value.Value;
    }

    private static Uri RequireResponseAbsoluteUri(string? value, string name)
    {
        string raw = RequireResponseValue(value, name);

        if (!Uri.TryCreate(raw, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException(
                $"GitHub response field '{name}' must be an absolute URI.",
                name);
        }

        return uri;
    }

    private static Uri CreateCanonicalProfileUri(string login) =>
        new(
            $"https://github.com/{Uri.EscapeDataString(login)}",
            UriKind.Absolute);

    private sealed record GitHubProfileResponse(
        [property: JsonPropertyName("login")] string? Login,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
        [property: JsonPropertyName("public_repos")] int? PublicRepositoryCount,
        [property: JsonPropertyName("avatar_url")] string? AvatarUrl);
}

using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed class GitHubRepositoryDescriptiveDataCollector : IGitHubRepositoryDescriptiveDataCollector
{
    private const string ReadmeDataKey = "readme";
    private const string LicenseDataKey = "license";
    private const string LanguagesDataKey = "languages";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;
    private readonly Func<GitHubRepositoryMetadataCollection, GitHubRepositoryLanguages?>?
        suppliedLanguages;

    public GitHubRepositoryDescriptiveDataCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
        : this(httpClient, timeProvider, suppliedLanguages: null)
    {
    }

    // The Session 127 overload. `suppliedLanguages` is an opt-in a composition
    // uses when it has ALREADY collected the complete language map for this
    // repository over another transport
    // (docs/architecture/public-profile-runtime-collection.md §18.2). It follows
    // the delegate shape the five paginated collectors already use for their
    // §15 decision boundaries rather than introducing an interface.
    //
    // When it is null — the default, and what the Repository Proof composition
    // passes — this collector behaves exactly as it did before: it issues the
    // languages request itself. When it returns null for a given repository the
    // same is true, which is what makes a hybrid fallback structurally identical
    // to a REST-only run.
    //
    // When it returns a value the languages request is never built, so the
    // skipped GET is a property of the call graph rather than of an assertion.
    public GitHubRepositoryDescriptiveDataCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider,
        Func<GitHubRepositoryMetadataCollection, GitHubRepositoryLanguages?>? suppliedLanguages)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.suppliedLanguages = suppliedLanguages;
    }

    public async Task<GitHubRepositoryDescriptiveDataCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        GitHubCommitReference? contentReference = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(metadata);

        if (!metadata.IsAvailable)
        {
            return CreateNotRequestedResult();
        }

        EndpointResult<RepositoryReadme> readme = await CollectReadmeAsync(
            repository,
            contentReference,
            cancellationToken);
        EndpointResult<LicenseData> license = await CollectLicenseAsync(
            repository,
            contentReference,
            cancellationToken);
        // Already collected over another transport for this repository, or null.
        // Null keeps the original call below, so nothing about the REST path
        // changes when no composition supplies anything.
        GitHubRepositoryLanguages? preCollected = suppliedLanguages?.Invoke(metadata);
        EndpointResult<IReadOnlyDictionary<string, long>> languages =
            preCollected is null
                ? await CollectLanguagesAsync(repository, cancellationToken)
                : NormalizeSuppliedLanguages(preCollected);

        return new GitHubRepositoryDescriptiveDataCollection(
            readme.Value,
            license.Value?.License,
            license.Value?.File,
            languages.Value ?? GitHubRepositoryDescriptiveDataCollection.EmptyLanguages,
            [readme.Availability, license.Availability, languages.Availability]);
    }

    private async Task<EndpointResult<RepositoryReadme>> CollectReadmeAsync(
        GitHubRepositoryUrl repository,
        GitHubCommitReference? contentReference,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(
                repository,
                "readme",
                contentReference,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return EndpointResult<RepositoryReadme>.Available(
                    value: null,
                    ReadmeDataKey,
                    timeProvider.GetUtcNow());
            }

            if (!response.IsSuccessStatusCode)
            {
                return EndpointResult<RepositoryReadme>.Unavailable(
                    CreateFailureAvailability(response, ReadmeDataKey));
            }

            GitHubContentResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubContentResponse>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return EndpointResult<RepositoryReadme>.Unavailable(
                    CreateFailedAvailability(
                        ReadmeDataKey,
                        "GitHub returned an empty README response."));
            }

            return EndpointResult<RepositoryReadme>.Available(
                new RepositoryReadme(
                    RequireValue(responseModel.Path, "readme.path"),
                    DecodeBase64Content(responseModel.Content, responseModel.Encoding, "README")),
                ReadmeDataKey,
                timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return EndpointResult<RepositoryReadme>.Unavailable(
                CreateFailedAvailability(
                    ReadmeDataKey,
                    $"GitHub README request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return EndpointResult<RepositoryReadme>.Unavailable(
                CreateFailedAvailability(
                    ReadmeDataKey,
                    $"GitHub README response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return EndpointResult<RepositoryReadme>.Unavailable(
                CreateFailedAvailability(
                    ReadmeDataKey,
                    $"GitHub README response could not be normalized: {exception.Message}"));
        }
        catch (FormatException exception)
        {
            return EndpointResult<RepositoryReadme>.Unavailable(
                CreateFailedAvailability(
                    ReadmeDataKey,
                    $"GitHub README content could not be decoded: {exception.Message}"));
        }
    }

    private async Task<EndpointResult<LicenseData>> CollectLicenseAsync(
        GitHubRepositoryUrl repository,
        GitHubCommitReference? contentReference,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(
                repository,
                "license",
                contentReference,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return EndpointResult<LicenseData>.Available(
                    value: null,
                    LicenseDataKey,
                    timeProvider.GetUtcNow());
            }

            if (!response.IsSuccessStatusCode)
            {
                return EndpointResult<LicenseData>.Unavailable(
                    CreateFailureAvailability(response, LicenseDataKey));
            }

            GitHubLicenseResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubLicenseResponse>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return EndpointResult<LicenseData>.Unavailable(
                    CreateFailedAvailability(
                        LicenseDataKey,
                        "GitHub returned an empty license response."));
            }

            RepositoryFile file = new(
                RequireValue(responseModel.Path, "license.path"),
                isDirectory: false,
                responseModel.Size,
                content: null);
            RepositoryLicense? license = responseModel.License is null
                ? null
                : new RepositoryLicense(
                    RequireValue(responseModel.License.Key, "license.key"),
                    RequireValue(responseModel.License.Name, "license.name"),
                    responseModel.License.SpdxIdentifier,
                    file.Path);

            return EndpointResult<LicenseData>.Available(
                new LicenseData(license, file),
                LicenseDataKey,
                timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return EndpointResult<LicenseData>.Unavailable(
                CreateFailedAvailability(
                    LicenseDataKey,
                    $"GitHub license request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return EndpointResult<LicenseData>.Unavailable(
                CreateFailedAvailability(
                    LicenseDataKey,
                    $"GitHub license response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return EndpointResult<LicenseData>.Unavailable(
                CreateFailedAvailability(
                    LicenseDataKey,
                    $"GitHub license response could not be normalized: {exception.Message}"));
        }
    }

    // The supplied map is normalized with the SAME rules the REST response goes
    // through, so a composition cannot serve a map this collector would have
    // refused. The guard mirrors the REST branch exactly rather than trusting
    // the supplier: the delegate is public, and a normalization failure must
    // surface as the same typed unavailability, never as an exception escaping
    // into the snapshot assembly.
    private static EndpointResult<IReadOnlyDictionary<string, long>> NormalizeSuppliedLanguages(
        GitHubRepositoryLanguages supplied)
    {
        try
        {
            return EndpointResult<IReadOnlyDictionary<string, long>>.Available(
                NormalizeLanguages(supplied.Values),
                LanguagesDataKey,
                supplied.RetrievedAt);
        }
        catch (ArgumentException exception)
        {
            return EndpointResult<IReadOnlyDictionary<string, long>>.Unavailable(
                CreateFailedAvailability(
                    LanguagesDataKey,
                    $"GitHub languages response could not be normalized: {exception.Message}"));
        }
    }

    private async Task<EndpointResult<IReadOnlyDictionary<string, long>>> CollectLanguagesAsync(
        GitHubRepositoryUrl repository,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(
                repository,
                "languages",
                contentReference: null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return EndpointResult<IReadOnlyDictionary<string, long>>.Unavailable(
                    CreateFailureAvailability(response, LanguagesDataKey));
            }

            Dictionary<string, long>? responseModel = await response.Content
                .ReadFromJsonAsync<Dictionary<string, long>>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return EndpointResult<IReadOnlyDictionary<string, long>>.Unavailable(
                    CreateFailedAvailability(
                        LanguagesDataKey,
                        "GitHub returned an empty languages response."));
            }

            return EndpointResult<IReadOnlyDictionary<string, long>>.Available(
                NormalizeLanguages(responseModel),
                LanguagesDataKey,
                timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return EndpointResult<IReadOnlyDictionary<string, long>>.Unavailable(
                CreateFailedAvailability(
                    LanguagesDataKey,
                    $"GitHub languages request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return EndpointResult<IReadOnlyDictionary<string, long>>.Unavailable(
                CreateFailedAvailability(
                    LanguagesDataKey,
                    $"GitHub languages response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return EndpointResult<IReadOnlyDictionary<string, long>>.Unavailable(
                CreateFailedAvailability(
                    LanguagesDataKey,
                    $"GitHub languages response could not be normalized: {exception.Message}"));
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        GitHubRepositoryUrl repository,
        string endpoint,
        GitHubCommitReference? contentReference,
        CancellationToken cancellationToken)
    {
        string relativeUri =
            $"repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/{endpoint}";

        if (contentReference is not null)
        {
            relativeUri += $"?ref={Uri.EscapeDataString(contentReference.CommitSha)}";
        }

        using HttpRequestMessage request = new(
            HttpMethod.Get,
            relativeUri);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        return await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
    }

    private static GitHubRepositoryDescriptiveDataCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before descriptive data is collected.";

        return new GitHubRepositoryDescriptiveDataCollection(
            Readme: null,
            License: null,
            LicenseFile: null,
            GitHubRepositoryDescriptiveDataCollection.EmptyLanguages,
            [
                DataAvailability.Unavailable(
                    ReadmeDataKey,
                    DataAvailabilityStatus.NotRequested,
                    reason),
                DataAvailability.Unavailable(
                    LicenseDataKey,
                    DataAvailabilityStatus.NotRequested,
                    reason),
                DataAvailability.Unavailable(
                    LanguagesDataKey,
                    DataAvailabilityStatus.NotRequested,
                    reason)
            ]);
    }

    private static DataAvailability CreateFailureAvailability(
        HttpResponseMessage response,
        string dataKey) =>
        GitHubApiAvailability.FromFailureResponse(response, dataKey);

    private static DataAvailability CreateFailedAvailability(string dataKey, string reason) =>
        DataAvailability.Unavailable(dataKey, DataAvailabilityStatus.Failed, reason);

    private static IReadOnlyDictionary<string, long> NormalizeLanguages(
        IReadOnlyDictionary<string, long> languages)
    {
        Dictionary<string, long> normalized = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string language, long bytes) in languages)
        {
            if (bytes < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(languages),
                    bytes,
                    "Language byte count cannot be negative.");
            }

            if (!normalized.TryAdd(RequireValue(language, "languages.key"), bytes))
            {
                throw new ArgumentException(
                    $"Duplicate language '{language}'.",
                    nameof(languages));
            }
        }

        return new ReadOnlyDictionary<string, long>(normalized);
    }

    private static string DecodeBase64Content(
        string? content,
        string? encoding,
        string dataName)
    {
        if (!string.Equals(encoding, "base64", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"GitHub {dataName} content must use base64 encoding.",
                nameof(encoding));
        }

        if (content is null)
        {
            throw new ArgumentException(
                $"GitHub {dataName} content is required.",
                nameof(content));
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(content));
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

    private sealed record EndpointResult<T>(T? Value, DataAvailability Availability)
    {
        public static EndpointResult<T> Available(
            T? value,
            string dataKey,
            DateTimeOffset retrievedAt) =>
            new(value, DataAvailability.Available(dataKey, retrievedAt));

        public static EndpointResult<T> Unavailable(DataAvailability availability) =>
            new(default, availability);
    }

    private sealed record LicenseData(RepositoryLicense? License, RepositoryFile File);

    private sealed record GitHubContentResponse(
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("encoding")] string? Encoding);

    private sealed record GitHubLicenseResponse(
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("size")] long? Size,
        [property: JsonPropertyName("license")] GitHubLicenseMetadata? License);

    private sealed record GitHubLicenseMetadata(
        [property: JsonPropertyName("key")] string? Key,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("spdx_id")] string? SpdxIdentifier);
}

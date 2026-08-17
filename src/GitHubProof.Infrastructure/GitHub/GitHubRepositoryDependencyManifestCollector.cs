using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

// Collects the dependency declaration evidence surface contracted by
// docs/architecture/repository-dependency-evidence.md. Collected manifests are
// never merged into the file tree, so existing signal inputs stay unchanged.
public sealed class GitHubRepositoryDependencyManifestCollector
    : IGitHubRepositoryDependencyManifestCollector
{
    public const string DependencyManifestsDataKey =
        RepositoryDataKeys.DependencyManifests;
    public const int MaximumManifestFiles = 32;
    public const int MaximumManifestFileBytes = 131072;
    public const int MaximumManifestCorpusBytes = 524288;

    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositoryDependencyManifestCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositoryDependencyManifestCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryContentsCollection contents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(contents);

        if (!contents.FileTreeAvailability.IsAvailable)
        {
            return CreateNotRequestedResult();
        }

        if (contents.HeadCommitSha is null)
        {
            // An available but empty repository was scanned completely.
            return CreateResult(
                declarations: [],
                isComplete: true,
                failure: null);
        }

        RepositoryFile[] candidates = contents.FileTree.Items
            .Where(file => !file.IsDirectory
                && GitHubRepositoryDependencyManifestParser.MatchEcosystem(file.Path)
                    is not null)
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .ToArray();

        bool isComplete = contents.FileTree.IsComplete
            && candidates.Length <= MaximumManifestFiles;

        // Manifests already collected with content by the contents collector are
        // reused instead of being requested again.
        Dictionary<string, string> collectedContent = contents.ProjectFiles.Items
            .Where(file => file.Content is not null)
            .GroupBy(file => file.Path, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First().Content!,
                StringComparer.Ordinal);
        List<RepositoryDependencyDeclaration> declarations = [];
        DataAvailability? failure = null;
        int acceptedBytes = 0;

        foreach (RepositoryFile candidate in candidates.Take(MaximumManifestFiles))
        {
            if (candidate.Size is null
                || candidate.Size > MaximumManifestFileBytes
                || candidate.Size > MaximumManifestCorpusBytes - acceptedBytes)
            {
                isComplete = false;
                continue;
            }

            string? manifestContent;

            if (collectedContent.TryGetValue(candidate.Path, out string? reused))
            {
                manifestContent = reused;
            }
            else
            {
                ContentResult content = await CollectFileContentAsync(
                    repository,
                    contents.HeadCommitSha,
                    candidate,
                    cancellationToken);

                if (content.Availability is not null)
                {
                    failure ??= content.Availability;
                    isComplete = false;
                    continue;
                }

                manifestContent = content.Content;
            }

            int decodedBytes = Encoding.UTF8.GetByteCount(manifestContent!);

            if (decodedBytes > MaximumManifestFileBytes
                || decodedBytes > MaximumManifestCorpusBytes - acceptedBytes)
            {
                isComplete = false;
                continue;
            }

            acceptedBytes += decodedBytes;

            DependencyEcosystem ecosystem =
                GitHubRepositoryDependencyManifestParser.MatchEcosystem(candidate.Path)!.Value;

            declarations.AddRange(GitHubRepositoryDependencyManifestParser.Parse(
                ecosystem,
                candidate.Path,
                manifestContent!));
        }

        return CreateResult(declarations, isComplete, failure);
    }

    private async Task<ContentResult> CollectFileContentAsync(
        GitHubRepositoryUrl repository,
        string headCommitSha,
        RepositoryFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(
                $"repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}/contents/{EscapeRepositoryPath(file.Path)}?ref={Uri.EscapeDataString(headCommitSha)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ContentResult.Failed(GitHubApiAvailability.FromFailureResponse(
                    response,
                    DependencyManifestsDataKey));
            }

            GitHubContentResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubContentResponse>(JsonOptions, cancellationToken);

            if (responseModel?.Content is null
                || !string.Equals(responseModel.Type, "file", StringComparison.Ordinal)
                || !string.Equals(responseModel.Path, file.Path, StringComparison.Ordinal)
                || !string.Equals(
                    responseModel.Encoding,
                    "base64",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ContentResult.Failed(CreateFailedAvailability(
                    $"GitHub dependency manifest response for '{file.Path}' could not be read."));
            }

            return ContentResult.Success(Encoding.UTF8.GetString(
                Convert.FromBase64String(responseModel.Content)));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                $"GitHub dependency manifest request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                $"GitHub dependency manifest response could not be read: {exception.Message}"));
        }
        catch (FormatException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                $"GitHub dependency manifest content could not be decoded: {exception.Message}"));
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        string relativeUri,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, relativeUri);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        return await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
    }

    private GitHubRepositoryDependencyManifestCollection CreateResult(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations,
        bool isComplete,
        DataAvailability? failure) =>
        new(
            new RepositoryCollection<RepositoryDependencyDeclaration>(
                declarations,
                isComplete: isComplete),
            failure ?? DataAvailability.Available(
                DependencyManifestsDataKey,
                timeProvider.GetUtcNow()));

    private static GitHubRepositoryDependencyManifestCollection CreateNotRequestedResult() =>
        new(
            new RepositoryCollection<RepositoryDependencyDeclaration>(isComplete: false),
            DataAvailability.Unavailable(
                DependencyManifestsDataKey,
                DataAvailabilityStatus.NotRequested,
                "The repository file tree must be available before dependency manifests are collected."));

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(
            DependencyManifestsDataKey,
            DataAvailabilityStatus.Failed,
            reason);

    private static string EscapeRepositoryPath(string path) => string.Join(
        "/",
        path.Split('/', StringSplitOptions.None).Select(Uri.EscapeDataString));

    private sealed record ContentResult(string? Content, DataAvailability? Availability)
    {
        public static ContentResult Success(string content) =>
            new(content, Availability: null);

        public static ContentResult Failed(DataAvailability availability) =>
            new(Content: null, availability);
    }

    private sealed record GitHubContentResponse(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("encoding")] string? Encoding);
}

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

public sealed class GitHubRepositoryContentsCollector : IGitHubRepositoryContentsCollector
{
    private const string FileTreeDataKey = "file-tree";
    private const string ProjectFilesDataKey = "project-files";
    private const string WorkflowsDataKey = "workflows";
    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositoryContentsCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositoryContentsCollection> CollectAsync(
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

        TreeResult tree = await CollectTreeAsync(
            repository,
            metadata.Repository.DefaultBranch,
            cancellationToken);

        if (tree.Availability is not null)
        {
            return CreateUnavailableTreeResult(tree.Availability);
        }

        if (tree.IsEmpty)
        {
            return CreateEmptyResult();
        }

        SelectedFilesResult projectFiles = await CollectSelectedFilesAsync(
            repository,
            tree.HeadCommitSha!,
            tree.FileTree!,
            IsRelevantProjectFile,
            ProjectFilesDataKey,
            "project/dependency file",
            tree.IsComplete,
            cancellationToken);
        SelectedFilesResult workflows = await CollectSelectedFilesAsync(
            repository,
            tree.HeadCommitSha!,
            tree.FileTree!,
            IsWorkflowFile,
            WorkflowsDataKey,
            "workflow file",
            tree.IsComplete,
            cancellationToken);

        List<string> warnings = [];

        if (!tree.IsComplete)
        {
            warnings.Add("File tree was truncated; returned entries are usable but incomplete.");
        }

        warnings.AddRange(projectFiles.Warnings);
        warnings.AddRange(workflows.Warnings);

        return new GitHubRepositoryContentsCollection(
            tree.HeadCommitSha,
            new RepositoryCollection<RepositoryFile>(tree.FileTree!, isComplete: tree.IsComplete),
            projectFiles.Files,
            workflows.Files,
            DataAvailability.Available(FileTreeDataKey, timeProvider.GetUtcNow()),
            projectFiles.Availability,
            workflows.Availability,
            warnings);
    }

    private async Task<TreeResult> CollectTreeAsync(
        GitHubRepositoryUrl repository,
        string defaultBranch,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage referenceResponse = await SendAsync(
                $"repos/{EscapeRepositorySegment(repository.Owner)}/{EscapeRepositorySegment(repository.Name)}/git/ref/heads/{EscapeRepositorySegment(defaultBranch)}",
                cancellationToken);

            if (referenceResponse.StatusCode == HttpStatusCode.Conflict
                && await IsEmptyRepositoryResponseAsync(referenceResponse, cancellationToken))
            {
                return TreeResult.Empty;
            }

            if (!referenceResponse.IsSuccessStatusCode)
            {
                return TreeResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(referenceResponse, FileTreeDataKey));
            }

            GitHubReferenceResponse? reference = await referenceResponse.Content
                .ReadFromJsonAsync<GitHubReferenceResponse>(JsonOptions, cancellationToken);
            string headCommitSha = RequireValue(reference?.Object?.Sha, "reference.object.sha");

            using HttpResponseMessage commitResponse = await SendAsync(
                $"repos/{EscapeRepositorySegment(repository.Owner)}/{EscapeRepositorySegment(repository.Name)}/git/commits/{EscapeRepositorySegment(headCommitSha)}",
                cancellationToken);

            if (!commitResponse.IsSuccessStatusCode)
            {
                return TreeResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(commitResponse, FileTreeDataKey));
            }

            GitHubCommitResponse? commit = await commitResponse.Content
                .ReadFromJsonAsync<GitHubCommitResponse>(JsonOptions, cancellationToken);
            string rootTreeSha = RequireValue(commit?.Tree?.Sha, "commit.tree.sha");

            using HttpResponseMessage treeResponse = await SendAsync(
                $"repos/{EscapeRepositorySegment(repository.Owner)}/{EscapeRepositorySegment(repository.Name)}/git/trees/{EscapeRepositorySegment(rootTreeSha)}?recursive=1",
                cancellationToken);

            if (!treeResponse.IsSuccessStatusCode)
            {
                return TreeResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(treeResponse, FileTreeDataKey));
            }

            GitHubTreeResponse? responseModel = await treeResponse.Content
                .ReadFromJsonAsync<GitHubTreeResponse>(JsonOptions, cancellationToken);

            if (responseModel?.Tree is null)
            {
                return TreeResult.Failed(CreateFailedAvailability(
                    FileTreeDataKey,
                    "GitHub returned an empty file tree response."));
            }

            List<RepositoryFile> files = [];
            HashSet<string> observedPaths = new(StringComparer.Ordinal);

            foreach (GitHubTreeEntry entry in responseModel.Tree)
            {
                RepositoryFile file = NormalizeTreeEntry(entry);

                if (observedPaths.Add(file.Path))
                {
                    files.Add(file);
                }
            }

            return new TreeResult(
                headCommitSha,
                files,
                IsComplete: !responseModel.Truncated,
                IsEmpty: false,
                Availability: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return TreeResult.Failed(CreateFailedAvailability(
                FileTreeDataKey,
                $"GitHub file tree request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return TreeResult.Failed(CreateFailedAvailability(
                FileTreeDataKey,
                $"GitHub file tree response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return TreeResult.Failed(CreateFailedAvailability(
                FileTreeDataKey,
                $"GitHub file tree response could not be normalized: {exception.Message}"));
        }
    }

    private async Task<SelectedFilesResult> CollectSelectedFilesAsync(
        GitHubRepositoryUrl repository,
        string headCommitSha,
        IReadOnlyList<RepositoryFile> fileTree,
        Func<RepositoryFile, bool> selection,
        string dataKey,
        string dataName,
        bool treeIsComplete,
        CancellationToken cancellationToken)
    {
        List<RepositoryFile> files = [];
        List<string> warnings = [];
        DataAvailability? failure = null;

        foreach (RepositoryFile file in fileTree.Where(selection))
        {
            ContentResult content = await CollectFileContentAsync(
                repository,
                headCommitSha,
                file,
                dataKey,
                dataName,
                cancellationToken);

            if (content.Availability is not null)
            {
                failure ??= content.Availability;
                continue;
            }

            files.Add(content.File!);
        }

        bool isComplete = treeIsComplete && failure is null;

        if (failure is not null)
        {
            warnings.Add($"{dataName} collection did not complete; collected files are partial.");
        }
        else if (!treeIsComplete)
        {
            warnings.Add($"{dataName} selection may be incomplete because the file tree was truncated.");
        }

        return new SelectedFilesResult(
            new RepositoryCollection<RepositoryFile>(files, isComplete: isComplete),
            failure ?? DataAvailability.Available(dataKey, timeProvider.GetUtcNow()),
            warnings);
    }

    private async Task<ContentResult> CollectFileContentAsync(
        GitHubRepositoryUrl repository,
        string headCommitSha,
        RepositoryFile file,
        string dataKey,
        string dataName,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(
                $"repos/{EscapeRepositorySegment(repository.Owner)}/{EscapeRepositorySegment(repository.Name)}/contents/{EscapeRepositoryPath(file.Path)}?ref={Uri.EscapeDataString(headCommitSha)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ContentResult.Failed(
                    GitHubApiAvailability.FromFailureResponse(response, dataKey));
            }

            GitHubContentResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubContentResponse>(JsonOptions, cancellationToken);

            if (responseModel is null)
            {
                return ContentResult.Failed(CreateFailedAvailability(
                    dataKey,
                    $"GitHub returned an empty {dataName} response."));
            }

            if (!string.Equals(responseModel.Type, "file", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"GitHub {dataName} response must represent a file.",
                    nameof(responseModel.Type));
            }

            if (!string.Equals(responseModel.Path, file.Path, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"GitHub {dataName} response path does not match the requested path.",
                    nameof(responseModel.Path));
            }

            return ContentResult.Success(new RepositoryFile(
                file.Path,
                isDirectory: false,
                file.Size,
                DecodeBase64Content(responseModel.Content, responseModel.Encoding, dataName)));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                dataKey,
                $"GitHub {dataName} request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                dataKey,
                $"GitHub {dataName} response could not be read: {exception.Message}"));
        }
        catch (ArgumentException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                dataKey,
                $"GitHub {dataName} response could not be normalized: {exception.Message}"));
        }
        catch (FormatException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                dataKey,
                $"GitHub {dataName} content could not be decoded: {exception.Message}"));
        }
    }

    private async Task<bool> IsEmptyRepositoryResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        GitHubErrorResponse? error = await response.Content
            .ReadFromJsonAsync<GitHubErrorResponse>(JsonOptions, cancellationToken);

        return string.Equals(error?.Message, "Git Repository is empty.", StringComparison.Ordinal);
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

    private GitHubRepositoryContentsCollection CreateNotRequestedResult()
    {
        const string reason = "Repository metadata must be available before contents are collected.";

        return new GitHubRepositoryContentsCollection(
            HeadCommitSha: null,
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            DataAvailability.Unavailable(FileTreeDataKey, DataAvailabilityStatus.NotRequested, reason),
            DataAvailability.Unavailable(ProjectFilesDataKey, DataAvailabilityStatus.NotRequested, reason),
            DataAvailability.Unavailable(WorkflowsDataKey, DataAvailabilityStatus.NotRequested, reason),
            [reason]);
    }

    private GitHubRepositoryContentsCollection CreateUnavailableTreeResult(DataAvailability availability)
    {
        const string reason = "File tree must be available before project files and workflows are collected.";

        return new GitHubRepositoryContentsCollection(
            HeadCommitSha: null,
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            new RepositoryCollection<RepositoryFile>(isComplete: false),
            availability,
            DataAvailability.Unavailable(ProjectFilesDataKey, DataAvailabilityStatus.NotRequested, reason),
            DataAvailability.Unavailable(WorkflowsDataKey, DataAvailabilityStatus.NotRequested, reason),
            [reason]);
    }

    private GitHubRepositoryContentsCollection CreateEmptyResult()
    {
        DateTimeOffset retrievedAt = timeProvider.GetUtcNow();

        return new GitHubRepositoryContentsCollection(
            HeadCommitSha: null,
            new RepositoryCollection<RepositoryFile>(),
            new RepositoryCollection<RepositoryFile>(),
            new RepositoryCollection<RepositoryFile>(),
            DataAvailability.Available(FileTreeDataKey, retrievedAt),
            DataAvailability.Available(ProjectFilesDataKey, retrievedAt),
            DataAvailability.Available(WorkflowsDataKey, retrievedAt),
            Array.Empty<string>());
    }

    private static RepositoryFile NormalizeTreeEntry(GitHubTreeEntry entry) =>
        new(
            RequireValue(entry.Path, "tree.path"),
            isDirectory: string.Equals(entry.Type, "tree", StringComparison.Ordinal),
            entry.Size,
            content: null);

    private static bool IsRelevantProjectFile(RepositoryFile file)
    {
        if (file.IsDirectory)
        {
            return false;
        }

        string fileName = file.Path[(file.Path.LastIndexOf('/') + 1)..];

        return fileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "pom.xml", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "build.gradle", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "build.gradle.kts", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "pytest.ini", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "package.json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "Directory.Packages.props", StringComparison.OrdinalIgnoreCase)
            || fileName.StartsWith("jest.config.", StringComparison.OrdinalIgnoreCase)
            || fileName.StartsWith("vitest.config.", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWorkflowFile(RepositoryFile file) =>
        !file.IsDirectory
        && file.Path.StartsWith(".github/workflows/", StringComparison.Ordinal)
        && (file.Path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)
            || file.Path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));

    private static string DecodeBase64Content(string? content, string? encoding, string dataName)
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

    private static DataAvailability CreateFailedAvailability(string dataKey, string reason) =>
        DataAvailability.Unavailable(dataKey, DataAvailabilityStatus.Failed, reason);

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

    private static string EscapeRepositorySegment(string value) => Uri.EscapeDataString(value);

    private static string EscapeRepositoryPath(string path) => string.Join(
        "/",
        path.Split('/', StringSplitOptions.None).Select(Uri.EscapeDataString));

    private sealed record TreeResult(
        string? HeadCommitSha,
        IReadOnlyList<RepositoryFile>? FileTree,
        bool IsComplete,
        bool IsEmpty,
        DataAvailability? Availability)
    {
        public static TreeResult Empty { get; } = new(
            HeadCommitSha: null,
            FileTree: null,
            IsComplete: true,
            IsEmpty: true,
            Availability: null);

        public static TreeResult Failed(DataAvailability availability) =>
            new(
                HeadCommitSha: null,
                FileTree: null,
                IsComplete: false,
                IsEmpty: false,
                availability);
    }

    private sealed record SelectedFilesResult(
        RepositoryCollection<RepositoryFile> Files,
        DataAvailability Availability,
        IReadOnlyList<string> Warnings);

    private sealed record ContentResult(RepositoryFile? File, DataAvailability? Availability)
    {
        public static ContentResult Success(RepositoryFile file) => new(file, Availability: null);

        public static ContentResult Failed(DataAvailability availability) => new(File: null, availability);
    }

    private sealed record GitHubReferenceResponse(
        [property: JsonPropertyName("object")] GitHubReferenceObject? Object);

    private sealed record GitHubReferenceObject(
        [property: JsonPropertyName("sha")] string? Sha);

    private sealed record GitHubCommitResponse(
        [property: JsonPropertyName("tree")] GitHubCommitTree? Tree);

    private sealed record GitHubCommitTree(
        [property: JsonPropertyName("sha")] string? Sha);

    private sealed record GitHubTreeResponse(
        [property: JsonPropertyName("tree")] IReadOnlyList<GitHubTreeEntry>? Tree,
        [property: JsonPropertyName("truncated")] bool Truncated);

    private sealed record GitHubTreeEntry(
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("size")] long? Size);

    private sealed record GitHubContentResponse(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("encoding")] string? Encoding);

    private sealed record GitHubErrorResponse(
        [property: JsonPropertyName("message")] string? Message);
}

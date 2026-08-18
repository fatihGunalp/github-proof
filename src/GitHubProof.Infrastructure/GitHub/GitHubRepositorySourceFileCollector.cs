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

// Collects the bounded raw source evidence surface contracted by
// docs/architecture/repository-source-evidence-collection.md. Source content is
// never merged into the file tree and never contributes collection warnings, so
// existing signal inputs and RepositoryAnalysis.Status stay unchanged.
//
// The collector obtains its own pinned tree metadata because the regular-blob
// rule needs the Git mode, which RepositorySnapshot.FileTree does not carry.
public sealed class GitHubRepositorySourceFileCollector
    : IGitHubRepositorySourceFileCollector
{
    public const string SourceFilesDataKey = RepositoryDataKeys.SourceFiles;

    // Contract §10. Bounds never vary by ecosystem, extension or repository.
    public const int MaximumSourceFiles = 32;
    public const int MaximumSourceFileBytes = 65536;
    public const int MaximumSourceCorpusBytes = 393216;

    private const int Utf8BomLength = 3;
    private const char Utf8BomCharacter = '\uFEFF';

    // Contract §6. One global, deterministic, case-insensitive allow-list.
    private static readonly HashSet<string> SourceExtensions = new(StringComparer.Ordinal)
    {
        ".cs", ".fs", ".vb",
        ".java", ".kt", ".kts", ".scala", ".groovy",
        ".js", ".jsx", ".mjs", ".cjs", ".ts", ".tsx", ".d.ts", ".vue", ".svelte",
        ".py", ".go", ".rs", ".php", ".rb",
        ".razor", ".cshtml"
    };

    // Contract §7. Exact build filenames, not the whole .kts extension.
    private static readonly HashSet<string> ExcludedBuildFileNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "build.gradle.kts",
            "settings.gradle.kts"
        };

    // Contract §8. Collection budget policy, never an authorship, originality,
    // quality or generated-code judgment.
    private static readonly HashSet<string> ExcludedPathSegments =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "node_modules", "bower_components", "vendor", "third_party", "Pods",
            "bin", "obj", "dist", "out", "build", "target", "coverage",
            ".venv", "venv", "virtualenv", "site-packages", "__pycache__", ".tox",
            ".gradle", ".next", ".nuxt", ".vs", ".idea"
        };

    private static readonly Uri GitHubApiBaseUri = new("https://api.github.com/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly HttpClient httpClient;
    private readonly TimeProvider timeProvider;

    public GitHubRepositorySourceFileCollector(
        HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
        this.httpClient.BaseAddress ??= GitHubApiBaseUri;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubRepositorySourceFileCollection> CollectAsync(
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
            return CreateResult(sourceFiles: [], isComplete: true, failure: null);
        }

        TreeResult tree = await CollectTreeAsync(
            repository,
            contents.HeadCommitSha,
            cancellationToken);

        if (tree.Availability is not null)
        {
            return new GitHubRepositorySourceFileCollection(
                new RepositoryCollection<RepositorySourceFile>(isComplete: false),
                tree.Availability);
        }

        SourceTreeEntry[] eligible = tree.Entries!
            .Where(IsSupportedEligibleSource)
            .ToArray();

        // Contract §12. The total candidate order is fixed before any
        // content-budget acceptance decision is made.
        SourceTreeEntry[] candidateOrder = BuildTotalCandidateOrder(eligible);

        List<RepositorySourceFile> accepted = [];
        DataAvailability? failure = null;
        bool isComplete = tree.IsComplete;
        int consumedSlots = 0;
        int corpusBytes = 0;

        foreach (SourceTreeEntry candidate in candidateOrder)
        {
            if (consumedSlots >= MaximumSourceFiles)
            {
                // Remaining candidates were never evaluated for retrieval.
                isComplete = false;
                break;
            }

            // Contract §11.1 and §13. A pre-fetch rejection consumes no
            // retrieval slot and never becomes negative evidence.
            if (candidate.Size is null
                || candidate.Size > MaximumSourceFileBytes
                || candidate.Size > MaximumSourceCorpusBytes - corpusBytes)
            {
                isComplete = false;
                continue;
            }

            // Contract §11.2. Retrieval begins, so the slot is consumed
            // permanently whatever happens next.
            consumedSlots++;

            ContentResult content = await CollectFileContentAsync(
                repository,
                contents.HeadCommitSha,
                candidate.Path,
                cancellationToken);

            if (content.Availability is not null)
            {
                failure ??= content.Availability;
                isComplete = false;
                continue;
            }

            string? normalized = NormalizeSourceContent(content.Encoding, content.Content!);

            if (normalized is null)
            {
                // Unsupported or unprocessable text is never negative evidence.
                isComplete = false;
                continue;
            }

            // Contract §10.1 and §17. Canonical accounting is the normalized
            // Content UTF-8 byte count, recomputed after normalization.
            int normalizedBytes = Encoding.UTF8.GetByteCount(normalized);

            if (normalizedBytes > MaximumSourceFileBytes
                || normalizedBytes > MaximumSourceCorpusBytes - corpusBytes)
            {
                isComplete = false;
                continue;
            }

            accepted.Add(new RepositorySourceFile(candidate.Path, normalized));
            corpusBytes += normalizedBytes;
        }

        return CreateResult(accepted, isComplete, failure);
    }

    private async Task<TreeResult> CollectTreeAsync(
        GitHubRepositoryUrl repository,
        string headCommitSha,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage commitResponse = await SendAsync(
                $"repos/{EscapeSegment(repository.Owner)}/{EscapeSegment(repository.Name)}/git/commits/{EscapeSegment(headCommitSha)}",
                cancellationToken);

            if (!commitResponse.IsSuccessStatusCode)
            {
                return TreeResult.Failed(GitHubApiAvailability.FromFailureResponse(
                    commitResponse,
                    SourceFilesDataKey));
            }

            GitHubCommitResponse? commit = await commitResponse.Content
                .ReadFromJsonAsync<GitHubCommitResponse>(JsonOptions, cancellationToken);
            string? rootTreeSha = commit?.Tree?.Sha;

            if (string.IsNullOrWhiteSpace(rootTreeSha))
            {
                return TreeResult.Failed(CreateFailedAvailability(
                    "GitHub returned no source tree reference for the pinned commit."));
            }

            using HttpResponseMessage treeResponse = await SendAsync(
                $"repos/{EscapeSegment(repository.Owner)}/{EscapeSegment(repository.Name)}/git/trees/{EscapeSegment(rootTreeSha.Trim())}?recursive=1",
                cancellationToken);

            if (!treeResponse.IsSuccessStatusCode)
            {
                return TreeResult.Failed(GitHubApiAvailability.FromFailureResponse(
                    treeResponse,
                    SourceFilesDataKey));
            }

            GitHubTreeResponse? responseModel = await treeResponse.Content
                .ReadFromJsonAsync<GitHubTreeResponse>(JsonOptions, cancellationToken);

            if (responseModel?.Tree is null)
            {
                return TreeResult.Failed(CreateFailedAvailability(
                    "GitHub returned an empty source tree response."));
            }

            List<SourceTreeEntry> entries = [];
            HashSet<string> observedPaths = new(StringComparer.Ordinal);

            foreach (GitHubTreeEntry entry in responseModel.Tree)
            {
                if (string.IsNullOrWhiteSpace(entry.Path))
                {
                    continue;
                }

                string path = entry.Path.Trim();

                if (observedPaths.Add(path))
                {
                    entries.Add(new SourceTreeEntry(path, entry.Type, entry.Mode, entry.Size));
                }
            }

            // Contract §19. A truncated tree stays available but incomplete.
            return new TreeResult(entries, IsComplete: !responseModel.Truncated, Availability: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return TreeResult.Failed(CreateFailedAvailability(
                $"GitHub source tree request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return TreeResult.Failed(CreateFailedAvailability(
                $"GitHub source tree response could not be read: {exception.Message}"));
        }
    }

    private async Task<ContentResult> CollectFileContentAsync(
        GitHubRepositoryUrl repository,
        string headCommitSha,
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await SendAsync(
                $"repos/{EscapeSegment(repository.Owner)}/{EscapeSegment(repository.Name)}/contents/{EscapeRepositoryPath(path)}?ref={Uri.EscapeDataString(headCommitSha)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ContentResult.Failed(GitHubApiAvailability.FromFailureResponse(
                    response,
                    SourceFilesDataKey));
            }

            GitHubContentResponse? responseModel = await response.Content
                .ReadFromJsonAsync<GitHubContentResponse>(JsonOptions, cancellationToken);

            if (responseModel?.Content is null
                || !string.Equals(responseModel.Type, "file", StringComparison.Ordinal)
                || !string.Equals(responseModel.Path, path, StringComparison.Ordinal))
            {
                // Diagnostics carry the path and status only, never source payload.
                return ContentResult.Failed(CreateFailedAvailability(
                    $"GitHub source content response for '{path}' could not be read."));
            }

            return ContentResult.Success(responseModel.Encoding, responseModel.Content);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                $"GitHub source content request failed: {exception.Message}"));
        }
        catch (JsonException exception)
        {
            return ContentResult.Failed(CreateFailedAvailability(
                $"GitHub source content response could not be read: {exception.Message}"));
        }
    }

    // Contract §16. Returns null when the payload is unsupported or
    // unprocessable textual source. Source content never reaches a message.
    private static string? NormalizeSourceContent(string? encoding, string content)
    {
        if (!string.Equals(encoding, "base64", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        byte[] decoded;

        try
        {
            decoded = Convert.FromBase64String(content);
        }
        catch (FormatException)
        {
            return null;
        }

        // Post-decode safety guard. Only a leading BOM can shrink the payload,
        // so anything beyond that cannot satisfy the per-file bound.
        if (decoded.Length > MaximumSourceFileBytes + Utf8BomLength)
        {
            return null;
        }

        if (Array.IndexOf(decoded, (byte)0) >= 0)
        {
            return null;
        }

        string text;

        try
        {
            text = StrictUtf8.GetString(decoded);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }

        // One leading UTF-8 BOM is stripped. Line endings are not normalized.
        return text.Length > 0 && text[0] == Utf8BomCharacter
            ? text[1..]
            : text;
    }

    // Contract §12. Deterministic extension round-robin over Ordinal-ordered
    // groups and Ordinal-ordered paths. It expresses no importance or ranking.
    private static SourceTreeEntry[] BuildTotalCandidateOrder(
        IReadOnlyList<SourceTreeEntry> eligible)
    {
        SourceTreeEntry[][] groups = eligible
            .GroupBy(entry => NormalizeExtension(entry.Path), StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.OrderBy(entry => entry.Path, StringComparer.Ordinal).ToArray())
            .ToArray();
        List<SourceTreeEntry> order = [];

        for (int round = 0; ; round++)
        {
            bool appended = false;

            foreach (SourceTreeEntry[] group in groups)
            {
                if (round < group.Length)
                {
                    order.Add(group[round]);
                    appended = true;
                }
            }

            if (!appended)
            {
                return [.. order];
            }
        }
    }

    // Contract §9. Every check is metadata-only and consumes no retrieval slot.
    private static bool IsSupportedEligibleSource(SourceTreeEntry entry) =>
        IsSupportedRegularBlob(entry)
        && !HasExcludedPathSegment(entry.Path)
        && !IsExcludedBuildFileName(entry.Path)
        && SourceExtensions.Contains(NormalizeExtension(entry.Path));

    // Contract §5. Regular blobs only; symlinks, submodules, trees and any other
    // non-regular entry are outside the supported V1 source surface.
    private static bool IsSupportedRegularBlob(SourceTreeEntry entry) =>
        string.Equals(entry.Type, "blob", StringComparison.Ordinal)
        && (string.Equals(entry.Mode, "100644", StringComparison.Ordinal)
            || string.Equals(entry.Mode, "100755", StringComparison.Ordinal));

    private static bool HasExcludedPathSegment(string path)
    {
        string[] segments = path.Split('/');

        for (int index = 0; index < segments.Length - 1; index++)
        {
            if (ExcludedPathSegments.Contains(segments[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsExcludedBuildFileName(string path) =>
        ExcludedBuildFileNames.Contains(FileName(path));

    // Contract §6. The final path segment is lowercased; a name ending in
    // '.d.ts' normalizes to '.d.ts'; otherwise the extension starts at the last
    // '.'. Lowercasing is invariant, so the result is locale-independent.
    private static string NormalizeExtension(string path)
    {
        string name = FileName(path).ToLowerInvariant();

        if (name.EndsWith(".d.ts", StringComparison.Ordinal))
        {
            return ".d.ts";
        }

        int index = name.LastIndexOf('.');

        return index > 0 ? name[index..] : string.Empty;
    }

    private static string FileName(string path) =>
        path[(path.LastIndexOf('/') + 1)..];

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

    private GitHubRepositorySourceFileCollection CreateResult(
        IReadOnlyList<RepositorySourceFile> sourceFiles,
        bool isComplete,
        DataAvailability? failure) =>
        new(
            new RepositoryCollection<RepositorySourceFile>(sourceFiles, isComplete: isComplete),
            failure ?? DataAvailability.Available(
                SourceFilesDataKey,
                timeProvider.GetUtcNow()));

    private static GitHubRepositorySourceFileCollection CreateNotRequestedResult() =>
        new(
            new RepositoryCollection<RepositorySourceFile>(isComplete: false),
            DataAvailability.Unavailable(
                SourceFilesDataKey,
                DataAvailabilityStatus.NotRequested,
                "The repository file tree must be available before source files are collected."));

    private static DataAvailability CreateFailedAvailability(string reason) =>
        DataAvailability.Unavailable(
            SourceFilesDataKey,
            DataAvailabilityStatus.Failed,
            reason);

    private static string EscapeSegment(string value) => Uri.EscapeDataString(value);

    private static string EscapeRepositoryPath(string path) => string.Join(
        "/",
        path.Split('/', StringSplitOptions.None).Select(Uri.EscapeDataString));

    private sealed record SourceTreeEntry(
        string Path,
        string? Type,
        string? Mode,
        long? Size);

    private sealed record TreeResult(
        IReadOnlyList<SourceTreeEntry>? Entries,
        bool IsComplete,
        DataAvailability? Availability)
    {
        public static TreeResult Failed(DataAvailability availability) =>
            new(Entries: null, IsComplete: false, availability);
    }

    private sealed record ContentResult(
        string? Encoding,
        string? Content,
        DataAvailability? Availability)
    {
        public static ContentResult Success(string? encoding, string content) =>
            new(encoding, content, Availability: null);

        public static ContentResult Failed(DataAvailability availability) =>
            new(Encoding: null, Content: null, availability);
    }

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
        [property: JsonPropertyName("mode")] string? Mode,
        [property: JsonPropertyName("size")] long? Size);

    private sealed record GitHubContentResponse(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("encoding")] string? Encoding);
}

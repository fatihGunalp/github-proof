using System.Net;
using System.Text;
using System.Text.Json;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// A deterministic in-memory GitHub API for one fixture repository, used to run
// the full Repository Proof composition and the Profile V1 composition against
// byte-identical upstream data and then compare both the issued requests and
// the resulting analysis truth.
//
// It records every request path so a test can assert exactly which requests a
// composition issued, rather than asserting on coarse URL fragments that the
// contents collector legitimately uses too (it issues its own git/commits and
// git/trees requests).
internal sealed class FakeGitHubApiHandler : HttpMessageHandler
{
    public const string Owner = "octocat";
    public const string Name = "hello-world";
    public const string RepositoryUrl = "https://github.com/octocat/hello-world";
    public const string DefaultBranch = "main";
    public const string HeadCommitSha = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b";
    public const string RootTreeSha = "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a";

    // Only the source-file collector requests this path: the contents collector
    // never fetches a .cs file.
    public const string SourceFilePath = "src/Program.cs";

    // Only the dependency-manifest collector requests this path: requirements.txt
    // is a manifest but not a "relevant project file", so the contents collector
    // does not already carry its content for reuse.
    public const string DependencyManifestPath = "requirements.txt";

    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly List<string> requestPaths = [];
    private readonly Func<int, HttpResponseMessage>? graphQl;
    private readonly string metadataLicenseKey;

    private int graphQlCalls;

    // Session 127. Without a responder `/graphql` answers 404 exactly as it did
    // before, which is what the Session 125 envelope-failure test relies on.
    // With one, the argument is the 1-based call index, so a test can vary the
    // answer per repository without a second fixture.
    // `metadataLicenseKey` exists so a test can make the repository-metadata
    // license DIFFER from the /license response and prove which of the two the
    // snapshot actually carries. Default: the two agree, as they do upstream.
    public FakeGitHubApiHandler(
        Func<int, HttpResponseMessage>? graphQl = null,
        string metadataLicenseKey = "mit")
    {
        this.graphQl = graphQl;
        this.metadataLicenseKey = metadataLicenseKey;
    }

    public IReadOnlyList<string> RequestPaths => requestPaths;

    public int CountRequests(string pathFragment) =>
        requestPaths.Count(path =>
            path.Contains(pathFragment, StringComparison.Ordinal));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Uri uri = request.RequestUri!;
        string path = Uri.UnescapeDataString(uri.AbsolutePath);

        lock (requestPaths)
        {
            requestPaths.Add(uri.PathAndQuery);
        }

        return Task.FromResult(Route(path));
    }

    private HttpResponseMessage Route(string path)
    {
        if (path == "/graphql")
        {
            return graphQl is null ? NotFound() : graphQl(++graphQlCalls);
        }

        string prefix = $"/repos/{Owner}/{Name}";

        if (path == prefix)
        {
            return Json($$"""
                {
                  "name": "{{Name}}",
                  "owner": { "login": "{{Owner}}" },
                  "private": false,
                  "archived": false,
                  "fork": false,
                  "is_template": false,
                  "language": "C#",
                  "default_branch": "{{DefaultBranch}}",
                  "description": "A deterministic fixture repository.",
                  "homepage": "https://example.invalid/demo",
                  "created_at": "2024-01-01T00:00:00Z",
                  "has_issues": true,
                  "topics": ["dotnet", "github-api"],
                  "license": { "key": "{{metadataLicenseKey}}", "name": "MIT License", "spdx_id": "MIT" }
                }
                """);
        }

        if (path == $"{prefix}/git/ref/heads/{DefaultBranch}")
        {
            return Json($$"""{ "object": { "sha": "{{HeadCommitSha}}" } }""");
        }

        if (path == $"{prefix}/git/commits/{HeadCommitSha}")
        {
            return Json($$"""{ "tree": { "sha": "{{RootTreeSha}}" } }""");
        }

        if (path == $"{prefix}/git/trees/{RootTreeSha}")
        {
            return Json(TreeJson());
        }

        if (path.StartsWith($"{prefix}/contents/", StringComparison.Ordinal))
        {
            string filePath = path[$"{prefix}/contents/".Length..];

            return FileContents.TryGetValue(filePath, out string? content)
                ? Json($$"""
                    {
                      "type": "file",
                      "path": {{Quote(filePath)}},
                      "content": {{Quote(Base64(content))}},
                      "encoding": "base64"
                    }
                    """)
                : NotFound();
        }

        if (path == $"{prefix}/readme")
        {
            return Json($$"""
                {
                  "path": "README.md",
                  "content": {{Quote(Base64(FileContents["README.md"]))}},
                  "encoding": "base64"
                }
                """);
        }

        if (path == $"{prefix}/license")
        {
            return Json("""
                {
                  "path": "LICENSE",
                  "size": 20,
                  "license": { "key": "mit", "name": "MIT License", "spdx_id": "MIT" }
                }
                """);
        }

        if (path == $"{prefix}/languages")
        {
            return Json("""{ "C#": 1200, "JavaScript": 300 }""");
        }

        // Every list endpoint answers with a deterministic empty page. The two
        // compositions see identical data, which is all the equivalence proof
        // needs, and an empty list still exercises the full request sequence.
        if (path == $"{prefix}/commits"
            || path == $"{prefix}/issues"
            || path == $"{prefix}/pulls"
            || path == $"{prefix}/releases"
            || path == $"{prefix}/tags")
        {
            return Json("[]");
        }

        // Served with a real entry so a test can prove the full composition
        // genuinely collects contributors while the profile composition does not.
        if (path == $"{prefix}/contributors")
        {
            return Json($$"""[ { "login": "{{Owner}}", "contributions": 42 } ]""");
        }

        return NotFound();
    }

    // Mode 100644 regular blobs, so the source collector's regular-blob rule
    // accepts the .cs entry.
    private static string TreeJson() =>
        $$"""
        {
          "truncated": false,
          "tree": [
            { "path": "README.md", "type": "blob", "mode": "100644", "size": 42 },
            { "path": "LICENSE", "type": "blob", "mode": "100644", "size": 20 },
            { "path": "src", "type": "tree", "mode": "040000" },
            { "path": "src/App.csproj", "type": "blob", "mode": "100644", "size": 120 },
            { "path": "{{SourceFilePath}}", "type": "blob", "mode": "100644", "size": 46 },
            { "path": "{{DependencyManifestPath}}", "type": "blob", "mode": "100644", "size": 18 },
            { "path": ".github/workflows/ci.yml", "type": "blob", "mode": "100644", "size": 64 },
            { "path": "docs/setup.md", "type": "blob", "mode": "100644", "size": 38 }
          ]
        }
        """;

    private static readonly Dictionary<string, string> FileContents =
        new(StringComparer.Ordinal)
        {
            ["README.md"] =
                "# Hello World\n\n## Installation\n\nRun `dotnet build`.\n\n## Usage\n\n```bash\ndotnet run\n```\n",
            ["LICENSE"] = "MIT License\n",
            ["src/App.csproj"] =
                "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><PackageReference Include=\"Serilog\" Version=\"3.1.1\" /></ItemGroup></Project>",
            [SourceFilePath] =
                "internal static class Program { static void Main() { } }\n",
            [DependencyManifestPath] = "requests==2.31.0\n",
            [".github/workflows/ci.yml"] =
                "name: ci\non: [push]\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - run: dotnet test\n",
            ["docs/setup.md"] = "# Setup\n\nInstall the SDK first.\n"
        };

    private static string Quote(string value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static string Base64(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    // The GraphQL envelope for this fixture repository, mirroring the REST
    // bodies above field for field. A test that changes one side and not the
    // other is asserting a divergence on purpose.
    public const string RepositoryEnvelopeJson = $$"""
        {
          "data": {
            "repository": {
              "name": "{{Name}}",
              "owner": { "login": "{{Owner}}" },
              "isPrivate": false,
              "isArchived": false,
              "isFork": false,
              "isTemplate": false,
              "description": "A deterministic fixture repository.",
              "homepageUrl": "https://example.invalid/demo",
              "createdAt": "2024-01-01T00:00:00Z",
              "hasIssuesEnabled": true,
              "primaryLanguage": { "name": "C#" },
              "defaultBranchRef": { "name": "{{DefaultBranch}}" },
              "repositoryTopics": {
                "pageInfo": { "hasNextPage": false },
                "nodes": [
                  { "topic": { "name": "dotnet" } },
                  { "topic": { "name": "github-api" } }
                ]
              },
              "languages": {
                "pageInfo": { "hasNextPage": false },
                "edges": [
                  { "size": 1200, "node": { "name": "C#" } },
                  { "size": 300, "node": { "name": "JavaScript" } }
                ]
              }
            }
          }
        }
        """;

    public static HttpResponseMessage JsonResponse(
        string content,
        HttpStatusCode status = HttpStatusCode.OK) =>
        new(status)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage Json(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage NotFound() =>
        new(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """{ "message": "Not Found" }""",
                Encoding.UTF8,
                "application/json")
        };
}

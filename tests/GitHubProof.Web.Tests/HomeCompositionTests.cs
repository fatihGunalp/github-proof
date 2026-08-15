using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class HomeCompositionTests
{
    [Fact]
    public void Repository_input_accepts_only_canonical_repository_url_scope()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryInput.cshtml");

        Assert.Contains(
            "Public GitHub repository URL",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "type=\"url\"",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "https://github.com/owner/repository",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "username",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Home_states_the_mvp_repository_boundary()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.Contains(
            "public GitHub repository",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "What does this repository prove?",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_input_exposes_deterministic_and_non_judgment_context()
    {
        string content = ReadWebFile(
            "Pages",
            "Shared",
            "_RepositoryInput.cshtml");

        Assert.Contains(
            "15 deterministic signals",
            content,
            StringComparison.Ordinal);

        Assert.Contains(
            "No developer judgment",
            content,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Home_does_not_promote_score_as_primary_content()
    {
        string content = ReadWebFile(
            "Pages",
            "Index.cshtml");

        Assert.DoesNotContain(
            "/ 100",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "score",
            content,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadWebFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(
                directory.FullName,
                "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path = Path.Combine(
                    new[]
                    {
                        directory.FullName,
                        "src",
                        "GitHubProof.Web"
                    }
                    .Concat(relativeSegments)
                    .ToArray());

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        $"Expected Web file was not found: {path}");
                }

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }
}
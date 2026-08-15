using System.Text.RegularExpressions;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed partial class ValidationRepositoryManifestTests
{
    private static readonly string[] RequiredProfiles =
    [
        "strong-documentation-active",
        "weak-short-readme",
        "single-commit",
        "distributed-commit-history",
        "test-and-ci",
        "no-test-or-ci",
        "archived-completed",
        "small-single-purpose-tool",
        "education-example",
        "empty-or-insufficient-data"
    ];

    [Fact]
    public void Manifest_FreezesTenDistinctRepositoriesWithCanonicalSchema()
    {
        IReadOnlyList<ValidationRepository> repositories = ValidationRepositoryManifest.Load();

        Assert.Equal(10, repositories.Count);
        Assert.Equal(10, repositories.Select(repository => repository.Id).Distinct().Count());
        Assert.Equal(
            10,
            repositories
                .Select(repository => $"{repository.Owner}/{repository.Name}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.Equal(10, repositories.Select(repository => repository.Url).Distinct().Count());

        foreach (ValidationRepository repository in repositories)
        {
            Assert.Matches(ValidationIdPattern(), repository.Id);
            Assert.Matches(CommitShaPattern(), repository.CommitSha);
            Assert.DoesNotContain("PLACEHOLDER", repository.CommitSha, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(
                $"https://github.com/{repository.Owner}/{repository.Name}",
                repository.Url);
            Assert.Equal("Pending", repository.ManualReviewStatus);
            Assert.NotEmpty(repository.ExpectedEdgeCases);
            Assert.False(string.IsNullOrWhiteSpace(repository.Notes));
        }
    }

    [Fact]
    public void Manifest_CoversEveryInitialProfileAndMultipleEcosystems()
    {
        IReadOnlyList<ValidationRepository> repositories = ValidationRepositoryManifest.Load();

        Assert.Equal(
            RequiredProfiles.OrderBy(profile => profile),
            repositories.Select(repository => repository.RepositoryProfile).OrderBy(profile => profile));
        Assert.Contains(repositories, repository => repository.PrimaryLanguage == "C#");
        Assert.Contains(repositories, repository => repository.PrimaryLanguage == "Rust");
        Assert.Contains(repositories, repository => repository.PrimaryLanguage == "Python");
        Assert.Contains(repositories, repository => repository.PrimaryLanguage == "JavaScript");
        Assert.Contains(repositories, repository => repository.PrimaryLanguage == "Java");
        Assert.Contains(repositories, repository => repository.IsArchived);
        Assert.Contains(repositories, repository => repository.ExpectedEdgeCases.Contains("single-commit"));
        Assert.Contains(repositories, repository => repository.ExpectedEdgeCases.Contains("no-readme"));
    }

    [GeneratedRegex("^validation-[0-9]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidationIdPattern();

    [GeneratedRegex("^[0-9a-f]{40}$", RegexOptions.CultureInvariant)]
    private static partial Regex CommitShaPattern();
}

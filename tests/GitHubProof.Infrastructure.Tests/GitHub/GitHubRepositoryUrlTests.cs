using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubRepositoryUrlTests
{
    [Fact]
    public void Parse_NormalizesARepositoryRootUrl()
    {
        GitHubRepositoryUrl repository = GitHubRepositoryUrl.Parse(
            " https://github.com/OctoCat/Hello-World/ ");

        Assert.Equal("OctoCat", repository.Owner);
        Assert.Equal("Hello-World", repository.Name);
        Assert.Equal(
            new Uri("https://github.com/OctoCat/Hello-World"),
            repository.CanonicalUri);
    }

    [Theory]
    [InlineData("http://github.com/octocat/hello-world")]
    [InlineData("https://example.com/octocat/hello-world")]
    [InlineData("https://github.com/octocat/hello-world/issues")]
    [InlineData("https://github.com/octocat/hello-world?tab=readme")]
    [InlineData("https://user@github.com/octocat/hello-world")]
    public void Parse_RejectsNonCanonicalRepositoryUrls(string value)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            GitHubRepositoryUrl.Parse(value));

        Assert.Equal("value", exception.ParamName);
    }
}

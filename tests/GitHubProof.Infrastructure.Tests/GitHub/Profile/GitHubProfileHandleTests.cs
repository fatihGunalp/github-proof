using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

public sealed class GitHubProfileHandleTests
{
    [Theory]
    [InlineData("octocat")]
    [InlineData(" octocat ")]
    [InlineData("https://github.com/octocat")]
    [InlineData("https://github.com/octocat/")]
    public void Parse_AcceptsUsernameAndCanonicalProfileUrl(string value)
    {
        GitHubProfileHandle handle = GitHubProfileHandle.Parse(value);

        Assert.Equal("octocat", handle.Login);
        Assert.Equal(new Uri("https://github.com/octocat"), handle.CanonicalUri);
    }

    [Theory]
    [InlineData("a-b-c")]
    [InlineData("a1")]
    [InlineData("A")]
    public void Parse_AcceptsValidGitHubUsernameShapes(string value)
    {
        Assert.Equal(value, GitHubProfileHandle.Parse(value).Login);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-octocat")]
    [InlineData("octocat-")]
    [InlineData("octo--cat")]
    [InlineData("octo cat")]
    [InlineData("octo_cat")]
    [InlineData("octocatoctocatoctocatoctocatoctocatoctoc")]
    public void Parse_RejectsInvalidUsername(string value)
    {
        Assert.Throws<ArgumentException>(() => GitHubProfileHandle.Parse(value));
    }

    [Theory]
    [InlineData("http://github.com/octocat")]
    [InlineData("https://gitlab.com/octocat")]
    [InlineData("https://github.com/octocat/hello-world")]
    [InlineData("https://github.com/octocat?tab=repositories")]
    [InlineData("https://github.com/octocat#readme")]
    [InlineData("https://user@github.com/octocat")]
    public void Parse_RejectsNonCanonicalProfileUrl(string value)
    {
        Assert.Throws<ArgumentException>(() => GitHubProfileHandle.Parse(value));
    }
}

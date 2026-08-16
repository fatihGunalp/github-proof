using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class GitHubProfileIdentityTests
{
    [Fact]
    public void Constructor_NormalizesLoginAndSupportsDeterministicEquality()
    {
        GitHubProfileIdentity first = new(
            " octocat ",
            new Uri("https://github.com/octocat"));
        GitHubProfileIdentity second = new(
            "octocat",
            new Uri("https://github.com/octocat"));

        Assert.Equal("octocat", first.Login);
        Assert.Equal(second, first);
        Assert.Equal(second.GetHashCode(), first.GetHashCode());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsMissingLogin(string login)
    {
        Assert.Throws<ArgumentException>(() => new GitHubProfileIdentity(
            login,
            new Uri("https://github.com/octocat")));
    }

    [Fact]
    public void Constructor_RejectsRelativeProfileUrl()
    {
        Assert.Throws<ArgumentException>(() => new GitHubProfileIdentity(
            "octocat",
            new Uri("/octocat", UriKind.Relative)));
    }

    [Fact]
    public void Constructor_RejectsNonHttpProfileUrl()
    {
        Assert.Throws<ArgumentException>(() => new GitHubProfileIdentity(
            "octocat",
            new Uri("ftp://github.com/octocat")));
    }
}

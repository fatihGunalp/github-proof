using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

public sealed class GitHubCommitReferenceTests
{
    [Fact]
    public void Parse_NormalizesAFullHexadecimalCommitSha()
    {
        GitHubCommitReference reference = GitHubCommitReference.Parse(
            " C3EEA5BEC401935206D4697D0C73AF950514C1AC ");

        Assert.Equal("c3eea5bec401935206d4697d0c73af950514c1ac", reference.CommitSha);
        Assert.Equal(reference.CommitSha, reference.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc123")]
    [InlineData("c3eea5bec401935206d4697d0c73af950514c1ag")]
    [InlineData("c3eea5bec401935206d4697d0c73af950514c1ac0")]
    public void Parse_RejectsValuesThatAreNotFullCommitShas(string value)
    {
        Assert.Throws<ArgumentException>(() => GitHubCommitReference.Parse(value));
    }
}

using GitHubProof.Web.Entry;
using Xunit;

namespace GitHubProof.Web.Tests;

// The unified entry accepts one field but must still resolve to exactly one
// of two separate analysis semantics. These tests pin that the resolution is
// deterministic from the submitted shape alone: no GitHub call, no existence
// check, no intent inference, and no silent reinterpretation of a malformed
// value.
public sealed class GitHubEntryClassifierTests
{
    [Theory]
    [InlineData("octocat")]
    [InlineData("fatihGunalp")]
    [InlineData("  fatihGunalp  ")]
    public void BareLogin_RoutesToProfileProof(string input)
    {
        Assert.True(GitHubEntryClassifier.TryClassify(input, out GitHubEntryRoute? route));

        Assert.Equal(GitHubEntryKind.Profile, route!.Kind);
        Assert.Equal(input.Trim(), route.Value);
    }

    [Theory]
    [InlineData("https://github.com/fatihGunalp")]
    [InlineData("https://github.com/octocat")]
    [InlineData("  https://github.com/octocat  ")]
    public void CanonicalProfileUrl_RoutesToProfileProof(string input)
    {
        Assert.True(GitHubEntryClassifier.TryClassify(input, out GitHubEntryRoute? route));

        Assert.Equal(GitHubEntryKind.Profile, route!.Kind);
        Assert.Equal(input.Trim(), route.Value);
    }

    [Theory]
    [InlineData("https://github.com/fatihGunalp/github-proof")]
    [InlineData("https://github.com/owner/repository")]
    [InlineData("  https://github.com/owner/repository  ")]
    public void CanonicalRepositoryUrl_RoutesToRepositoryProof(string input)
    {
        Assert.True(GitHubEntryClassifier.TryClassify(input, out GitHubEntryRoute? route));

        Assert.Equal(GitHubEntryKind.Repository, route!.Kind);
        Assert.Equal(input.Trim(), route.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://github.com")]
    [InlineData("https://github.com/")]
    [InlineData("https://github.com/owner/repository/tree/main")]
    [InlineData("https://github.com/owner/repository/blob/main/README.md")]
    [InlineData("://not-a-uri")]
    public void UnresolvableShape_IsNotClassified(string? input)
    {
        Assert.False(GitHubEntryClassifier.TryClassify(input, out GitHubEntryRoute? route));
        Assert.Null(route);
    }

    // A bare login carries no path. These shapes are not part of the accepted
    // input contract, so they must be rejected at the unified entry — never
    // pushed into the profile flow, where the user would land on a
    // flow-specific error with no way back to the entry.
    [Theory]
    [InlineData("owner/repo")]
    [InlineData("github.com/owner/repo")]
    [InlineData("github.com/octocat")]
    [InlineData("www.github.com/owner/repo")]
    public void SchemelessPathShape_IsRejectedAndNeverRoutedToProfile(string input)
    {
        Assert.False(GitHubEntryClassifier.TryClassify(input, out GitHubEntryRoute? route));
        Assert.Null(route);
    }

    // Non-canonical URL forms are rejected by the canonical parsers the
    // classifier delegates to, so they never reach either analysis flow.
    [Theory]
    [InlineData("https://example.com/owner/repository")]
    [InlineData("https://example.com/octocat")]
    [InlineData("http://github.com/owner/repository")]
    [InlineData("https://github.com/owner/repository?tab=readme")]
    [InlineData("https://github.com/octocat#pinned")]
    [InlineData("https://user@github.com/octocat")]
    [InlineData("https://github.com/-invalid-login")]
    [InlineData("not a login")]
    [InlineData("bad.login")]
    public void NonCanonicalShape_IsRejectedByTheCanonicalParser(string input)
    {
        Assert.False(GitHubEntryClassifier.TryClassify(input, out GitHubEntryRoute? route));
        Assert.Null(route);
    }

    [Fact]
    public void Classification_IsStableForTheSameInput()
    {
        // Determinism: the same value must always resolve the same way. A
        // heuristic or existence-based classifier could not guarantee this.
        for (int attempt = 0; attempt < 5; attempt++)
        {
            Assert.True(GitHubEntryClassifier.TryClassify(
                "https://github.com/owner/repository",
                out GitHubEntryRoute? route));

            Assert.Equal(GitHubEntryKind.Repository, route!.Kind);
        }
    }

    [Fact]
    public void Classifier_DoesNotNormalizeTheAcceptedValue()
    {
        // Acceptance is delegated to the canonical parser, but the parsed
        // result is discarded: the raw trimmed string travels onward, so the
        // collector's own GitHubProfileHandle.Parse stays the parse of record
        // and the Web layer never derives its own identity.
        Assert.True(GitHubEntryClassifier.TryClassify(
            "  https://github.com/OctoCat  ",
            out GitHubEntryRoute? route));

        Assert.Equal("https://github.com/OctoCat", route!.Value);
    }

    [Fact]
    public void Classifier_DelegatesAcceptanceRatherThanReimplementingTheRules()
    {
        // Every rule exercised here belongs to a canonical parser, not to the
        // classifier: host and scheme (GitHubRepositoryUrl.Parse) and the
        // login pattern (GitHubProfileHandle.Parse). If the classifier had its
        // own copy of these rules, they could drift apart.
        Assert.False(GitHubEntryClassifier.TryClassify(
            "https://gitlab.com/owner/repository",
            out _));

        Assert.False(GitHubEntryClassifier.TryClassify(
            "invalid--login-",
            out _));

        Assert.True(GitHubEntryClassifier.TryClassify(
            "valid-login",
            out _));
    }
}

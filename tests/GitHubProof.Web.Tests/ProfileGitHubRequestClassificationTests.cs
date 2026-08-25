using System.Net;
using GitHubProof.Web.Profile.Runtime;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 120 — A-T: paginated continuation requests must land in the SAME
// closed category as their initial request.
//
// A GitHub collector builds only the first page's URI. Every following page is
// the absolute URI GitHub supplied in its `Link: rel="next"` header, which uses
// a different addressing prefix. The first live benchmark measured the
// consequence: a material share of requests fell into `other`, so pagination
// cost could not be attributed to any evidence surface. The measured figures
// live in the session record, not here.
//
// These guards fix the category to the RESOURCE SHAPE, never to the addressing
// prefix and never to a repository identity.
public sealed class ProfileGitHubRequestClassificationTests
{
    // Each row: the URI a collector builds, the absolute continuation URI in
    // GitHub's own numeric-id form, and the single category both must produce.
    public static TheoryData<string, string, string> PaginatedFamilies() =>
        new()
        {
            {
                "https://api.github.com/repos/octocat/hello-world/commits?sha=main&per_page=100",
                "https://api.github.com/repositories/1296269/commits?sha=main&per_page=100&page=2",
                ProfileGitHubRequestCategory.RepositoryCommits
            },
            {
                "https://api.github.com/repos/octocat/hello-world/issues?state=all&per_page=100",
                "https://api.github.com/repositories/1296269/issues?state=all&per_page=100&page=3",
                ProfileGitHubRequestCategory.RepositoryIssues
            },
            {
                "https://api.github.com/repos/octocat/hello-world/pulls?state=all&per_page=100",
                "https://api.github.com/repositories/1296269/pulls?state=all&per_page=100&page=4",
                ProfileGitHubRequestCategory.RepositoryPullRequests
            },
            {
                "https://api.github.com/repos/octocat/hello-world/releases?per_page=100",
                "https://api.github.com/repositories/1296269/releases?per_page=100&page=2",
                ProfileGitHubRequestCategory.RepositoryReleases
            },
            {
                "https://api.github.com/repos/octocat/hello-world/tags?per_page=100",
                "https://api.github.com/repositories/1296269/tags?per_page=100&page=2",
                ProfileGitHubRequestCategory.RepositoryTags
            },
            {
                "https://api.github.com/users/octocat/repos?type=owner&sort=full_name&direction=asc&per_page=100",
                "https://api.github.com/user/583231/repos?type=owner&sort=full_name&direction=asc&per_page=100&page=2",
                ProfileGitHubRequestCategory.ProfileRepositories
            }
        };

    [Theory]
    [MemberData(nameof(PaginatedFamilies))]
    public void InitialAndContinuationUri_ShareOneCategory(
        string initialUri,
        string continuationUri,
        string expected)
    {
        Assert.Equal(expected, Classify(initialUri));
        Assert.Equal(expected, Classify(continuationUri));
    }

    // The regression this whole fix exists for: a continuation page must never
    // be counted as an unclassified request.
    [Theory]
    [MemberData(nameof(PaginatedFamilies))]
    public void ContinuationUri_IsNeverUnclassified(
        string initialUri,
        string continuationUri,
        string expected)
    {
        _ = initialUri;
        _ = expected;

        Assert.NotEqual(ProfileGitHubRequestCategory.Other, Classify(continuationUri));
    }

    // Non-paginated surfaces addressed by numeric repository id must classify
    // too: the addressing prefix is GitHub's choice, not a different resource.
    [Theory]
    [InlineData("https://api.github.com/repositories/1296269", "repository-metadata")]
    [InlineData("https://api.github.com/repositories/1296269/readme", "repository-readme")]
    [InlineData("https://api.github.com/repositories/1296269/license", "repository-license")]
    [InlineData("https://api.github.com/repositories/1296269/languages", "repository-languages")]
    [InlineData("https://api.github.com/repositories/1296269/contents/src/App.csproj", "repository-contents")]
    [InlineData("https://api.github.com/repositories/1296269/git/ref/heads/main", "repository-git-reference")]
    [InlineData("https://api.github.com/repositories/1296269/git/commits/abc123", "repository-git-commit")]
    [InlineData("https://api.github.com/repositories/1296269/git/trees/abc123", "repository-git-tree")]
    [InlineData("https://api.github.com/repositories/1296269/contributors", "repository-contributors")]
    [InlineData("https://api.github.com/user/583231", "profile-metadata")]
    public void NumericAddressing_ClassifiesLikeNamedAddressing(
        string uri,
        string expected) =>
        Assert.Equal(expected, Classify(uri));

    // The pre-fix behaviour must stay fixed for every named form as well.
    [Theory]
    [InlineData("https://api.github.com/users/octocat", "profile-metadata")]
    [InlineData("https://api.github.com/users/octocat/repos", "profile-repositories")]
    [InlineData("https://api.github.com/repos/octocat/hello-world", "repository-metadata")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/readme", "repository-readme")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/license", "repository-license")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/languages", "repository-languages")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/commits", "repository-commits")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/issues", "repository-issues")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/pulls", "repository-pull-requests")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/releases", "repository-releases")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/tags", "repository-tags")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/contributors", "repository-contributors")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/git/ref/heads/main", "repository-git-reference")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/git/commits/abc", "repository-git-commit")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/git/trees/abc?recursive=1", "repository-git-tree")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/contents/src/App.csproj", "repository-contents")]
    public void NamedAddressing_IsUnchangedByTheFix(string uri, string expected) =>
        Assert.Equal(expected, Classify(uri));

    // Query strings are addressing detail, never resource shape.
    [Theory]
    [InlineData("https://api.github.com/repos/o/r/issues")]
    [InlineData("https://api.github.com/repos/o/r/issues?state=all")]
    [InlineData("https://api.github.com/repos/o/r/issues?per_page=100&state=all")]
    [InlineData("https://api.github.com/repos/o/r/issues?state=all&per_page=100&page=7")]
    [InlineData("https://api.github.com/repos/o/r/issues?page=7&per_page=100&state=all")]
    [InlineData("https://api.github.com/repos/o/r/issues?page=7#fragment")]
    public void QueryStringVariations_DoNotAlterTheCategory(string uri) =>
        Assert.Equal(ProfileGitHubRequestCategory.RepositoryIssues, Classify(uri));

    // A collector hands HttpClient a relative URI; HttpClient resolves it before
    // the handler runs. Both representations must agree.
    [Theory]
    [InlineData("repos/octocat/hello-world/commits?sha=main&per_page=100", "repository-commits")]
    [InlineData("repos/octocat/hello-world/git/trees/abc?recursive=1", "repository-git-tree")]
    [InlineData("repos/octocat/hello-world", "repository-metadata")]
    [InlineData("users/octocat", "profile-metadata")]
    public void RelativeUri_ClassifiesLikeItsAbsoluteForm(string relative, string expected)
    {
        Assert.Equal(expected, ProfileGitHubRequestCategory.Classify(
            new Uri(relative, UriKind.Relative)));
        Assert.Equal(expected, Classify("https://api.github.com/" + relative));
    }

    // `other` must stay meaningful: a genuinely unknown endpoint still falls
    // there, so the fix cannot hide a future blind spot behind a wrong category.
    [Theory]
    [InlineData("https://api.github.com/rate_limit")]
    [InlineData("https://api.github.com/unknown/endpoint")]
    [InlineData("https://api.github.com/repos/octocat")]
    [InlineData("https://api.github.com/repos/octocat/hello-world/stargazers")]
    [InlineData("https://api.github.com/repositories/1296269/stargazers")]
    [InlineData("https://api.github.com/users/octocat/gists")]
    [InlineData("https://api.github.com/orgs/github/repos")]
    // The id-addressed arms must not swallow an endpoint whose second segment is
    // a word rather than a numeric id. /user/repos is GitHub's authenticated-user
    // repository list — a different resource, and one that must never be mistaken
    // for profile-metadata, the sole category gating budget observation.
    [InlineData("https://api.github.com/user/repos")]
    [InlineData("https://api.github.com/user/emails")]
    [InlineData("https://api.github.com/user/installations")]
    [InlineData("https://api.github.com/repositories/not-a-number/commits")]
    [InlineData("https://api.github.com/repositories")]
    public void GenuinelyUnknownEndpoints_StillBecomeOther(string uri) =>
        Assert.Equal(ProfileGitHubRequestCategory.Other, Classify(uri));

    // The id-addressed forms exist only for GitHub's pagination links, where the
    // id is always numeric.
    [Theory]
    [InlineData("https://api.github.com/user/583231/repos?page=2", "profile-repositories")]
    [InlineData("https://api.github.com/repositories/1296269/issues?page=2", "repository-issues")]
    public void IdAddressedForms_RequireANumericId(string uri, string expected) =>
        Assert.Equal(expected, Classify(uri));

    [Fact]
    public void NullUri_BecomesOther() =>
        Assert.Equal(ProfileGitHubRequestCategory.Other, ProfileGitHubRequestCategory.Classify(null));

    // The vocabulary is closed and its size is locked: A-T was a classification
    // defect, never a reason to grow the category set.
    [Fact]
    public void CategoryVocabulary_IsUnchanged()
    {
        Assert.Equal(17, ProfileGitHubRequestCategory.All.Count);
        Assert.Equal(
            [
                "profile-metadata",
                "profile-repositories",
                "repository-metadata",
                "repository-readme",
                "repository-license",
                "repository-languages",
                "repository-commits",
                "repository-issues",
                "repository-pull-requests",
                "repository-releases",
                "repository-tags",
                "repository-contributors",
                "repository-git-reference",
                "repository-git-commit",
                "repository-git-tree",
                "repository-contents",
                "other"
            ],
            ProfileGitHubRequestCategory.All);
    }

    // Whatever the addressing form, the classifier returns a constant from the
    // closed vocabulary — never a login, owner, repository name or numeric id.
    [Fact]
    public void ClassificationNeverReturnsAnIdentity()
    {
        foreach (string uri in new[]
        {
            "https://api.github.com/users/some-secret-login",
            "https://api.github.com/user/583231/repos?page=9",
            "https://api.github.com/repos/some-owner/some-private-name/issues?page=4",
            "https://api.github.com/repositories/1296269/pulls?page=5",
            "https://api.github.com/repositories/1296269/contents/src/Secrets.cs",
            "https://api.github.com/totally/unknown/shape"
        })
        {
            string category = Classify(uri);

            Assert.Contains(category, ProfileGitHubRequestCategory.All);
            Assert.DoesNotContain("secret", category, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("owner", category, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("1296269", category, StringComparison.Ordinal);
            Assert.DoesNotContain("583231", category, StringComparison.Ordinal);
        }
    }

    // End-to-end through the real telemetry recorder: a paginated surface's
    // pages accumulate on ONE counter, which is the measurement A-T restores.
    [Fact]
    public void PaginatedPages_AccumulateOnASingleCounter()
    {
        ProfileExecutionTelemetry telemetry = new();

        foreach (string uri in new[]
        {
            "https://api.github.com/repos/octocat/hello-world/issues?state=all&per_page=100",
            "https://api.github.com/repositories/1296269/issues?state=all&per_page=100&page=2",
            "https://api.github.com/repositories/1296269/issues?state=all&per_page=100&page=3"
        })
        {
            telemetry.RecordGitHubRequest(Classify(uri));
        }

        Assert.Equal(3, telemetry.GitHubRequests);
        Assert.Equal(
            [ProfileGitHubRequestCategory.RepositoryIssues],
            telemetry.RequestCategories.Keys);
        Assert.Equal(
            3,
            telemetry.RequestCategories[ProfileGitHubRequestCategory.RepositoryIssues]);
    }

    // Classification is an observation of the request, never a rewrite of it.
    // The continuation URI GitHub supplied must reach the transport byte for
    // byte, and nothing about response handling or the budget carrier may move.
    [Fact]
    public async Task Classification_DoesNotAlterTheOutgoingRequestOrBudgetState()
    {
        const string continuationUri =
            "https://api.github.com/repositories/1296269/issues?state=all&per_page=100&page=2";

        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        EchoingInnerHandler inner = new();
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            using HttpResponseMessage response = await httpClient.GetAsync(continuationUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("payload", await response.Content.ReadAsStringAsync());
        }

        Assert.Equal(continuationUri, inner.ObservedUri);
        Assert.Equal(1, inner.SendCount);
        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Equal(
            [ProfileGitHubRequestCategory.RepositoryIssues],
            telemetry.RequestCategories.Keys);

        // A continuation page is not profile metadata, so the proactive budget
        // decision surface stays exactly where §10.4 put it.
        Assert.Null(budgetState.MetadataObservation);
    }

    private static string Classify(string uri) =>
        ProfileGitHubRequestCategory.Classify(new Uri(uri));

    private sealed class EchoingInnerHandler : HttpMessageHandler
    {
        private int sendCount;

        public int SendCount => Volatile.Read(ref sendCount);

        public string? ObservedUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref sendCount);
            ObservedUri = request.RequestUri?.ToString();

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("payload")
                });
        }
    }
}

using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Collection;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Infrastructure.GitHub;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub;

// docs/architecture/public-profile-runtime-collection.md §15.
//
// Every stop below is driven by the canonical Domain decision boundary, never
// by a page count: each test supplies pages whose CONTENT decides when stopping
// is safe, and the "never reaches the condition" tests prove the collector
// enumerates every supplied page instead.
//
// A composition that supplies no boundary — Repository Proof — enumerates fully
// in every one of these scenarios.
public sealed class DecisionBoundedCollectionTests
{
    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-21T09:30:00Z");

    // ------------------------------------------------------------- commits

    // Page 1 already carries ten commits over more than fourteen days on ten
    // distinct UTC days, so both commit signals are pinned.
    [Fact]
    public async Task Commits_WhenBothSignalsArePinned_StopsAndReportsDecisionBounded()
    {
        PagedHandler handler = new(
            [CommitsPage(0, 10), CommitsPage(10, 10), CommitsPage(20, 10)]);

        GitHubRepositoryCommitCollection result = await CollectCommitsAsync(handler, bounded: true);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.DecisionBounded,
            result.Commits.ObservationState);
        Assert.False(result.Commits.IsComplete);
        Assert.Null(result.Commits.TotalCount);
        Assert.Equal(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.Empty(result.CollectionWarnings);
    }

    // All commits on a single UTC day: the distribution decision can never be
    // pinned, so every supplied page is enumerated and the result is Enumerated.
    [Fact]
    public async Task Commits_WhenTheConditionIsNeverReached_EnumeratesEveryPage()
    {
        PagedHandler handler = new(
            [SameDayCommitsPage(0, 30), SameDayCommitsPage(30, 30), SameDayCommitsPage(60, 30)]);

        GitHubRepositoryCommitCollection result = await CollectCommitsAsync(handler, bounded: true);

        Assert.Equal(3, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.Enumerated,
            result.Commits.ObservationState);
        Assert.True(result.Commits.IsComplete);
        Assert.Equal(90, result.Commits.TotalCount);
    }

    // Repository Proof supplies no boundary, so the identical pages enumerate.
    [Fact]
    public async Task Commits_WithoutABoundary_EnumeratesEveryPage()
    {
        PagedHandler handler = new(
            [CommitsPage(0, 10), CommitsPage(10, 10), CommitsPage(20, 10)]);

        GitHubRepositoryCommitCollection result = await CollectCommitsAsync(handler, bounded: false);

        Assert.Equal(3, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.Enumerated,
            result.Commits.ObservationState);
        Assert.Equal(30, result.Commits.TotalCount);
    }

    // A transport failure is still Incomplete and still warns, exactly as today.
    [Fact]
    public async Task Commits_WhenAPageFails_IsIncompleteAndStillWarns()
    {
        PagedHandler handler = new(
            [SameDayCommitsPage(0, 5)],
            failAfterFirstPage: true);

        GitHubRepositoryCommitCollection result = await CollectCommitsAsync(handler, bounded: true);

        Assert.Equal(
            CollectionObservationState.Incomplete,
            result.Commits.ObservationState);
        Assert.False(result.Commits.IsComplete);
        Assert.NotEqual(DataAvailabilityStatus.Available, result.Availability.Status);
        Assert.NotEmpty(result.CollectionWarnings);
    }

    // -------------------------------------------------------------- issues

    [Fact]
    public async Task Issues_StopOnlyAfterTwoQualifyingIssues()
    {
        PagedHandler oneIssuePerPage = new(
            [IssuesPage(1, 1), IssuesPage(2, 1), IssuesPage(3, 1)]);

        GitHubRepositoryIssueCollection result = await CollectIssuesAsync(oneIssuePerPage);

        // Page 1 alone is not enough; page 2 reaches the threshold.
        Assert.Equal(2, oneIssuePerPage.RequestCount);
        Assert.Equal(
            CollectionObservationState.DecisionBounded,
            result.Issues.ObservationState);
        Assert.Empty(result.CollectionWarnings);
    }

    [Fact]
    public async Task Issues_WhenProviderEnumerationEndsNaturally_IsEnumerated()
    {
        PagedHandler handler = new([IssuesPage(1, 2)]);

        GitHubRepositoryIssueCollection result = await CollectIssuesAsync(handler);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(CollectionObservationState.Enumerated, result.Issues.ObservationState);
        Assert.Equal(2, result.Issues.TotalCount);
    }

    // -------------------------------------------------------- pull requests

    [Fact]
    public async Task PullRequests_StopOnlyAfterTwoMergedPullRequests()
    {
        // Page 1 has three unmerged PRs, so the decision is not yet pinned.
        PagedHandler handler = new(
            [PullRequestsPage(1, 3, mergedCount: 0), PullRequestsPage(4, 3, mergedCount: 2), PullRequestsPage(7, 3, mergedCount: 3)]);

        GitHubRepositoryPullRequestCollection result = await CollectPullRequestsAsync(handler);

        Assert.Equal(2, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.DecisionBounded,
            result.PullRequests.ObservationState);
    }

    [Fact]
    public async Task PullRequests_WithNoMergedPullRequest_EnumeratesEveryPage()
    {
        PagedHandler handler = new(
            [PullRequestsPage(1, 3, mergedCount: 0), PullRequestsPage(4, 3, mergedCount: 0)]);

        GitHubRepositoryPullRequestCollection result = await CollectPullRequestsAsync(handler);

        Assert.Equal(2, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.Enumerated,
            result.PullRequests.ObservationState);
    }

    // ------------------------------------------------------------ releases

    [Fact]
    public async Task Releases_StopOnlyAfterAPublishedReleaseWithNotes()
    {
        PagedHandler handler = new(
            [ReleasesPage(1, draft: true, withNotes: false), ReleasesPage(2, draft: false, withNotes: true), ReleasesPage(3, draft: false, withNotes: true)]);

        GitHubRepositoryReleaseCollection result = await CollectReleasesAsync(handler);

        Assert.Equal(2, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.DecisionBounded,
            result.Releases.ObservationState);
    }

    [Fact]
    public async Task Releases_WithoutNotes_EnumerateEveryPage()
    {
        PagedHandler handler = new(
            [ReleasesPage(1, draft: false, withNotes: false), ReleasesPage(2, draft: false, withNotes: false)]);

        GitHubRepositoryReleaseCollection result = await CollectReleasesAsync(handler);

        Assert.Equal(2, handler.RequestCount);
        Assert.Equal(
            CollectionObservationState.Enumerated,
            result.Releases.ObservationState);
    }

    // ---------------------------------------------------------------- tags

    [Fact]
    public async Task Tags_StopAfterTagPresenceIsEstablished()
    {
        PagedHandler handler = new([TagsPage(1, 2), TagsPage(3, 2)]);

        GitHubRepositoryTagCollection result = await CollectTagsAsync(handler);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(CollectionObservationState.DecisionBounded, result.Tags.ObservationState);
        Assert.Null(result.Tags.TotalCount);
    }

    // No tag and no next page: genuinely enumerated, not bounded.
    [Fact]
    public async Task Tags_WhenTheFirstPageIsEmptyAndFinal_IsEnumerated()
    {
        PagedHandler handler = new([TagsPage(1, 0)]);

        GitHubRepositoryTagCollection result = await CollectTagsAsync(handler);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(CollectionObservationState.Enumerated, result.Tags.ObservationState);
        Assert.Equal(0, result.Tags.TotalCount);
    }

    // Tags present but the provider ends on that same page: also Enumerated.
    [Fact]
    public async Task Tags_WhenTheOnlyPageCarriesTags_IsEnumerated()
    {
        PagedHandler handler = new([TagsPage(1, 3)]);

        GitHubRepositoryTagCollection result = await CollectTagsAsync(handler);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(CollectionObservationState.Enumerated, result.Tags.ObservationState);
        Assert.Equal(3, result.Tags.TotalCount);
    }

    // -------------------------------------------------------------- drivers

    private static Task<GitHubRepositoryCommitCollection> CollectCommitsAsync(
        PagedHandler handler,
        bool bounded) =>
        new GitHubRepositoryCommitCollector(
                Client(handler),
                Clock(),
                bounded
                    ? RepositoryCollectionDecisionBoundary.AreCommitDecisionsIrreversible
                    : null)
            .CollectAsync(
                GitHubCollectionTestSupport.RepositoryUrl,
                GitHubCollectionTestSupport.AvailableMetadata());

    private static Task<GitHubRepositoryIssueCollection> CollectIssuesAsync(
        PagedHandler handler) =>
        new GitHubRepositoryIssueCollector(
                Client(handler),
                Clock(),
                RepositoryCollectionDecisionBoundary.IsIssueDecisionIrreversible)
            .CollectAsync(
                GitHubCollectionTestSupport.RepositoryUrl,
                GitHubCollectionTestSupport.AvailableMetadata());

    private static Task<GitHubRepositoryPullRequestCollection> CollectPullRequestsAsync(
        PagedHandler handler) =>
        new GitHubRepositoryPullRequestCollector(
                Client(handler),
                Clock(),
                RepositoryCollectionDecisionBoundary.IsPullRequestDecisionIrreversible)
            .CollectAsync(
                GitHubCollectionTestSupport.RepositoryUrl,
                GitHubCollectionTestSupport.AvailableMetadata());

    private static Task<GitHubRepositoryReleaseCollection> CollectReleasesAsync(
        PagedHandler handler) =>
        new GitHubRepositoryReleaseCollector(
                Client(handler),
                Clock(),
                RepositoryCollectionDecisionBoundary.IsReleaseDecisionIrreversible)
            .CollectAsync(
                GitHubCollectionTestSupport.RepositoryUrl,
                GitHubCollectionTestSupport.AvailableMetadata());

    private static Task<GitHubRepositoryTagCollection> CollectTagsAsync(
        PagedHandler handler) =>
        new GitHubRepositoryTagCollector(
                Client(handler),
                Clock(),
                RepositoryCollectionDecisionBoundary.IsTagDecisionIrreversible)
            .CollectAsync(
                GitHubCollectionTestSupport.RepositoryUrl,
                GitHubCollectionTestSupport.AvailableMetadata());

    private static HttpClient Client(PagedHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    private static TimeProvider Clock() => new FixedTimeProvider(RetrievedAt);

    // ------------------------------------------------------------- payloads

    private static string CommitsPage(int start, int count) =>
        CommitPayload(start, count, index => RetrievedAt.AddDays(-index * 2));

    private static string SameDayCommitsPage(int start, int count) =>
        CommitPayload(start, count, index => RetrievedAt.AddMinutes(-index));

    private static string CommitPayload(
        int start,
        int count,
        Func<int, DateTimeOffset> committedAt) =>
        "[" + string.Join(",", Enumerable.Range(start, count).Select(index =>
            "{\"sha\":\"sha-" + index
            + "\",\"commit\":{\"committer\":{\"date\":\""
            + committedAt(index).ToString("O")
            + "\"},\"message\":\"commit " + index + "\"}}")) + "]";

    private static string IssuesPage(int start, int count) =>
        "[" + string.Join(",", Enumerable.Range(start, count).Select(index =>
            "{\"number\":" + index
            + ",\"title\":\"issue " + index
            + "\",\"body\":null,\"state\":\"open\",\"created_at\":\""
            + RetrievedAt.ToString("O") + "\"}")) + "]";

    private static string PullRequestsPage(int start, int count, int mergedCount) =>
        "[" + string.Join(",", Enumerable.Range(0, count).Select(offset =>
        {
            int number = start + offset;
            string timestamp = RetrievedAt.ToString("O");

            return offset < mergedCount
                ? "{\"number\":" + number + ",\"title\":\"pr\",\"state\":\"closed\",\"created_at\":\"" + timestamp + "\",\"merged_at\":\"" + timestamp + "\"}"
                : "{\"number\":" + number + ",\"title\":\"pr\",\"state\":\"open\",\"created_at\":\"" + timestamp + "\",\"merged_at\":null}";
        })) + "]";

    private static string ReleasesPage(int index, bool draft, bool withNotes)
    {
        string notes = withNotes ? "\"notes\"" : "null";
        string publishedAt = draft ? "null" : "\"" + RetrievedAt.ToString("O") + "\"";

        return "[{\"tag_name\":\"v" + index + "\",\"draft\":" + (draft ? "true" : "false")
            + ",\"name\":\"release\",\"body\":" + notes
            + ",\"published_at\":" + publishedAt + "}]";
    }

    private static string TagsPage(int start, int count) =>
        "[" + string.Join(",", Enumerable.Range(start, count).Select(index =>
            "{\"name\":\"v" + index + "\"}")) + "]";

    // A deterministic Link-header pager: every page but the last advertises a
    // rel="next", so stopping early is always observable as a skipped request.
    private sealed class PagedHandler(
        string[] pages,
        bool failAfterFirstPage = false) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            int index = RequestCount;
            RequestCount++;

            if (failAfterFirstPage && index >= pages.Length)
            {
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(
                            """{ "message": "boom" }""",
                            Encoding.UTF8,
                            "application/json")
                    });
            }

            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    pages[index],
                    Encoding.UTF8,
                    "application/json")
            };

            bool hasNext = failAfterFirstPage || index < pages.Length - 1;

            if (hasNext)
            {
                response.Headers.TryAddWithoutValidation(
                    "Link",
                    $"<https://api.github.com/next?page={index + 2}>; rel=\"next\"");
            }

            return Task.FromResult(response);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

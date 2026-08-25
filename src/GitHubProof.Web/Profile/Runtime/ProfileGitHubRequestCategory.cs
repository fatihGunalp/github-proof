namespace GitHubProof.Web.Profile.Runtime;

// The closed, bounded request-category vocabulary required by
// docs/architecture/public-profile-runtime-collection.md §9.
//
// Categories are derived from the GitHub API route shape only. No repository
// name, owner, login, file path, SHA or any other identifier ever becomes a
// category name, so the vocabulary can never grow at runtime.
public static class ProfileGitHubRequestCategory
{
    public const string ProfileMetadata = "profile-metadata";
    public const string ProfileRepositories = "profile-repositories";
    public const string RepositoryMetadata = "repository-metadata";
    public const string RepositoryReadme = "repository-readme";
    public const string RepositoryLicense = "repository-license";
    public const string RepositoryLanguages = "repository-languages";
    public const string RepositoryCommits = "repository-commits";
    public const string RepositoryIssues = "repository-issues";
    public const string RepositoryPullRequests = "repository-pull-requests";
    public const string RepositoryReleases = "repository-releases";
    public const string RepositoryTags = "repository-tags";
    public const string RepositoryContributors = "repository-contributors";
    public const string RepositoryGitReference = "repository-git-reference";
    public const string RepositoryGitCommit = "repository-git-commit";
    public const string RepositoryGitTree = "repository-git-tree";
    public const string RepositoryContents = "repository-contents";

    // Session 124 (S1). The single GraphQL HTTP category authorized by
    // docs/architecture/public-profile-runtime-collection.md §18.9. One
    // physical POST /graphql increments the total request count once and this
    // category once. A GraphQL envelope is never counted once per surface it
    // served: the counting rule is locked in §18.9 precisely because that would
    // make §9's total request count untrue.
    //
    // Per-surface GraphQL served/fallback facts, if they are ever added, are
    // separate non-additive structural counters and are not HTTP request
    // counts. They are not part of S1.
    public const string GraphQl = "graphql";

    public const string Other = "other";

    public static IReadOnlyList<string> All { get; } =
    [
        ProfileMetadata,
        ProfileRepositories,
        RepositoryMetadata,
        RepositoryReadme,
        RepositoryLicense,
        RepositoryLanguages,
        RepositoryCommits,
        RepositoryIssues,
        RepositoryPullRequests,
        RepositoryReleases,
        RepositoryTags,
        RepositoryContributors,
        RepositoryGitReference,
        RepositoryGitCommit,
        RepositoryGitTree,
        RepositoryContents,
        GraphQl,
        Other
    ];

    // Session 120 — A-T.
    //
    // A GitHub request reaches this classifier in one of two shapes, and the
    // application controls only the first:
    //
    //   application-built   repos/{owner}/{repo}/issues?state=all&per_page=100
    //   provider-supplied   https://api.github.com/repositories/{id}/issues?page=2
    //
    // The second shape is whatever GitHub puts in its `Link: rel="next"` header,
    // which the paginated collectors follow verbatim through
    // GitHubPagination.FindNextPage. The earlier classifier only recognised the
    // owner/name form, so every continuation page fell into `other` and
    // pagination cost could not be attributed to any evidence surface.
    //
    // The fix is to classify by the RESOURCE SHAPE that follows the addressing
    // prefix, instead of requiring one particular prefix. The owner, repository
    // name and numeric repository id all sit in wildcard positions: they are
    // read to find where the resource path begins and are never returned,
    // stored or emitted. The vocabulary is unchanged.
    public static string Classify(Uri? requestUri)
    {
        if (requestUri is null)
        {
            return Other;
        }

        string[] segments = ReadPathSegments(requestUri);

        return segments switch
        {
            // Profile addressing: by login, or by the numeric form GitHub uses
            // in its own pagination links.
            ["users", _] => ProfileMetadata,
            ["users", _, .. var byLogin] => ClassifyProfileResource(byLogin),
            ["user", var userId, .. var byUserId] when IsNumericId(userId) =>
                ClassifyProfileResource(byUserId),

            // Repository addressing: by owner/name, or by numeric repository id.
            ["repos", _, _, .. var byName] => ClassifyRepositoryResource(byName),
            ["repositories", var repositoryId, .. var byRepositoryId]
                when IsNumericId(repositoryId) =>
                ClassifyRepositoryResource(byRepositoryId),

            // The GraphQL endpoint has no resource path of its own: one opaque
            // POST serves whatever the query asked for. It is matched exactly,
            // so nothing below /graphql/ can widen the vocabulary, and it stays
            // one category no matter how many surfaces a future envelope serves.
            ["graphql"] => GraphQl,

            _ => Other
        };
    }

    // The id-addressed forms exist only because GitHub emits them in pagination
    // links, and there the id is always numeric. Requiring that keeps the arm
    // from swallowing a genuinely different endpoint whose second segment is a
    // word: `/user/repos` is the authenticated-user repository list, not this
    // profile's metadata, and it must stay `other` rather than hide behind a
    // known category — the more so because `profile-metadata` is the single
    // category gating the proactive budget observation.
    private static bool IsNumericId(string segment)
    {
        if (segment.Length == 0)
        {
            return false;
        }

        foreach (char character in segment)
        {
            if (!char.IsAsciiDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static string ClassifyProfileResource(string[] resource) =>
        resource switch
        {
            [] => ProfileMetadata,
            ["repos", ..] => ProfileRepositories,
            _ => Other
        };

    private static string ClassifyRepositoryResource(string[] resource) =>
        resource switch
        {
            [] => RepositoryMetadata,
            ["git", "ref", ..] => RepositoryGitReference,
            ["git", "commits", ..] => RepositoryGitCommit,
            ["git", "trees", ..] => RepositoryGitTree,
            ["contents", ..] => RepositoryContents,
            ["readme", ..] => RepositoryReadme,
            ["license", ..] => RepositoryLicense,
            ["languages", ..] => RepositoryLanguages,
            ["commits", ..] => RepositoryCommits,
            ["issues", ..] => RepositoryIssues,
            ["pulls", ..] => RepositoryPullRequests,
            ["releases", ..] => RepositoryReleases,
            ["tags", ..] => RepositoryTags,
            ["contributors", ..] => RepositoryContributors,
            _ => Other
        };

    // The query string never participates: page, per_page, state, sha and
    // friends are addressing detail, not resource shape. A relative URI is
    // accepted too, so the classifier behaves identically whether it is handed
    // the URI a collector built or the absolute one HttpClient resolved.
    private static string[] ReadPathSegments(Uri requestUri)
    {
        string path = requestUri.IsAbsoluteUri
            ? requestUri.AbsolutePath
            : TrimQueryAndFragment(requestUri.OriginalString);

        return path.Split('/', StringSplitOptions.RemoveEmptyEntries);
    }

    private static string TrimQueryAndFragment(string value)
    {
        int end = value.IndexOfAny(['?', '#']);

        return end < 0 ? value : value[..end];
    }
}

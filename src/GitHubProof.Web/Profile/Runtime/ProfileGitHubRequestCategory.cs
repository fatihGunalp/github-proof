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
        Other
    ];

    public static string Classify(Uri? requestUri)
    {
        if (requestUri is null)
        {
            return Other;
        }

        string[] segments = requestUri.AbsolutePath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);

        return segments switch
        {
            ["users", _] => ProfileMetadata,
            ["users", _, "repos"] => ProfileRepositories,
            ["repos", _, _] => RepositoryMetadata,
            ["repos", _, _, "git", "ref", ..] => RepositoryGitReference,
            ["repos", _, _, "git", "commits", ..] => RepositoryGitCommit,
            ["repos", _, _, "git", "trees", ..] => RepositoryGitTree,
            ["repos", _, _, "contents", ..] => RepositoryContents,
            ["repos", _, _, "readme", ..] => RepositoryReadme,
            ["repos", _, _, "license", ..] => RepositoryLicense,
            ["repos", _, _, "languages", ..] => RepositoryLanguages,
            ["repos", _, _, "commits", ..] => RepositoryCommits,
            ["repos", _, _, "issues", ..] => RepositoryIssues,
            ["repos", _, _, "pulls", ..] => RepositoryPullRequests,
            ["repos", _, _, "releases", ..] => RepositoryReleases,
            ["repos", _, _, "tags", ..] => RepositoryTags,
            ["repos", _, _, "contributors", ..] => RepositoryContributors,
            _ => Other
        };
    }
}

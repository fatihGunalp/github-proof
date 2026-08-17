namespace GitHubProof.Domain.Analysis.Enums;

// The canonical V1 category set is closed by
// docs/architecture/repository-dependency-semantic-category.md §8.
// http-server is deferred and has no member.
public enum DependencySemanticCategory
{
    DatabaseAccess = 1,
    WebApplicationFramework = 2,
    HttpClient = 3,
    Authentication = 4,
    MachineLearning = 5
}

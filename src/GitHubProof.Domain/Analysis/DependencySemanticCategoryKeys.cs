using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis;

// Canonical V1 category keys from
// docs/architecture/repository-dependency-semantic-category.md §8. Domain owns
// both the taxonomy and its key vocabulary, so no consumer needs a second list.
public static class DependencySemanticCategoryKeys
{
    public const string DatabaseAccess = "database-access";
    public const string WebApplicationFramework = "web-application-framework";
    public const string HttpClient = "http-client";
    public const string Authentication = "authentication";
    public const string MachineLearning = "machine-learning";

    private static readonly IReadOnlyDictionary<string, DependencySemanticCategory> ByKey =
        new Dictionary<string, DependencySemanticCategory>(StringComparer.Ordinal)
        {
            [DatabaseAccess] = DependencySemanticCategory.DatabaseAccess,
            [WebApplicationFramework] = DependencySemanticCategory.WebApplicationFramework,
            [HttpClient] = DependencySemanticCategory.HttpClient,
            [Authentication] = DependencySemanticCategory.Authentication,
            [MachineLearning] = DependencySemanticCategory.MachineLearning
        };

    public static bool TryRead(string? key, out DependencySemanticCategory category)
    {
        if (key is not null && ByKey.TryGetValue(key, out category))
        {
            return true;
        }

        category = default;
        return false;
    }

    public static string ToKey(DependencySemanticCategory category) => category switch
    {
        DependencySemanticCategory.DatabaseAccess => DatabaseAccess,
        DependencySemanticCategory.WebApplicationFramework => WebApplicationFramework,
        DependencySemanticCategory.HttpClient => HttpClient,
        DependencySemanticCategory.Authentication => Authentication,
        DependencySemanticCategory.MachineLearning => MachineLearning,
        _ => throw new ArgumentOutOfRangeException(
            nameof(category),
            category,
            "Dependency semantic category is not defined.")
    };
}

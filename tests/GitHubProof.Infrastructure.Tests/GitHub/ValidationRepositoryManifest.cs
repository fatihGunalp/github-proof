using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitHubProof.Infrastructure.Tests.GitHub;

internal sealed record ValidationRepository(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("commitSha")] string CommitSha,
    [property: JsonPropertyName("primaryLanguage")] string? PrimaryLanguage,
    [property: JsonPropertyName("repositoryProfile")] string RepositoryProfile,
    [property: JsonPropertyName("isArchived")] bool IsArchived,
    [property: JsonPropertyName("isFork")] bool IsFork,
    [property: JsonPropertyName("isTemplate")] bool IsTemplate,
    [property: JsonPropertyName("expectedEdgeCases")] IReadOnlyList<string> ExpectedEdgeCases,
    [property: JsonPropertyName("manualReviewStatus")] string ManualReviewStatus,
    [property: JsonPropertyName("notes")] string Notes);

internal static class ValidationRepositoryManifest
{
    public static IReadOnlyList<ValidationRepository> Load()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "samples",
            "validation",
            "repositories.json");
        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<ValidationRepository[]>(json)
            ?? throw new InvalidOperationException("Validation repository manifest is empty.");
    }
}

using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

internal static class GitHubProfileTestSupport
{
    public static GitHubProfileHandle ProfileHandle { get; } =
        GitHubProfileHandle.Parse("octocat");

    public static GitHubProfileMetadataCollection AvailableProfileMetadata(
        int publicRepositoryCount = 1) =>
        new(
            new GitHubProfileIdentity("octocat", new Uri("https://github.com/octocat")),
            new GitHubProfileMetadata(
                "The Octocat",
                DateTimeOffset.Parse("2020-01-01T00:00:00Z"),
                publicRepositoryCount,
                avatarUrl: new Uri("https://avatars.githubusercontent.com/u/1")),
            DataAvailability.Available(
                "profile",
                DateTimeOffset.Parse("2026-08-15T09:30:00Z")));

    public static GitHubProfileMetadataCollection UnavailableProfileMetadata() =>
        new(
            Identity: null,
            Metadata: null,
            DataAvailability.Unavailable(
                "profile",
                DataAvailabilityStatus.RateLimited,
                "GitHub rate limit reached."));

    public static string RepositoryJson(
        string name,
        string? owner = "octocat",
        bool? fork = false,
        bool? isTemplate = false,
        bool? archived = false,
        long? size = 120,
        string? pushedAt = "2026-08-01T00:00:00Z",
        string? language = "C#",
        bool isPrivate = false,
        bool includeDescription = false,
        string? description = null)
    {
        List<string> fields =
        [
            $"\"name\": \"{name}\"",
            $"\"private\": {JsonBoolean(isPrivate)}",
            "\"default_branch\": \"main\"",
            "\"created_at\": \"2024-01-01T00:00:00Z\""
        ];

        if (owner is not null)
        {
            fields.Add($"\"owner\": {{ \"login\": \"{owner}\" }}");
        }

        if (fork is not null)
        {
            fields.Add($"\"fork\": {JsonBoolean(fork.Value)}");
        }

        if (isTemplate is not null)
        {
            fields.Add($"\"is_template\": {JsonBoolean(isTemplate.Value)}");
        }

        if (archived is not null)
        {
            fields.Add($"\"archived\": {JsonBoolean(archived.Value)}");
        }

        if (size is not null)
        {
            fields.Add($"\"size\": {size}");
        }

        if (language is not null)
        {
            fields.Add($"\"language\": \"{language}\"");
        }

        fields.Add(pushedAt is null
            ? "\"pushed_at\": null"
            : $"\"pushed_at\": \"{pushedAt}\"");

        if (includeDescription)
        {
            fields.Add(description is null
                ? "\"description\": null"
                : $"\"description\": \"{description}\"");
        }

        return $"{{ {string.Join(", ", fields)} }}";
    }

    private static string JsonBoolean(bool value) => value ? "true" : "false";
}

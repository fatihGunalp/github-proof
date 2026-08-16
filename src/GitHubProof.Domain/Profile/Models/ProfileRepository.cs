using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

public sealed record ProfileRepository
{
    public ProfileRepository(
        ProfileRepositoryReference reference,
        string? defaultBranch,
        string? primaryLanguage,
        RepositoryConditionState fork,
        RepositoryConditionState templateDerived,
        RepositoryConditionState archived,
        RepositoryConditionState empty,
        DateTimeOffset? lastPushedAt,
        DateTimeOffset? createdAt)
    {
        Reference = reference
            ?? throw new ArgumentNullException(nameof(reference));
        DefaultBranch = ProfileValue.NormalizeOptional(defaultBranch);
        PrimaryLanguage = ProfileValue.NormalizeOptional(primaryLanguage);
        Fork = RequireDefined(fork, nameof(fork));
        TemplateDerived = RequireDefined(templateDerived, nameof(templateDerived));
        Archived = RequireDefined(archived, nameof(archived));
        Empty = RequireDefined(empty, nameof(empty));
        LastPushedAt = lastPushedAt?.ToUniversalTime();
        CreatedAt = createdAt?.ToUniversalTime();
    }

    public ProfileRepositoryReference Reference { get; }

    public string? DefaultBranch { get; }

    public string? PrimaryLanguage { get; }

    public RepositoryConditionState Fork { get; }

    public RepositoryConditionState TemplateDerived { get; }

    public RepositoryConditionState Archived { get; }

    public RepositoryConditionState Empty { get; }

    public DateTimeOffset? LastPushedAt { get; }

    public DateTimeOffset? CreatedAt { get; }

    private static RepositoryConditionState RequireDefined(
        RepositoryConditionState state,
        string parameterName)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                state,
                "Repository condition state is not defined.");
        }

        return state;
    }
}

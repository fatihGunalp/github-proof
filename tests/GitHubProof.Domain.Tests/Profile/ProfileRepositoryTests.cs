using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ProfileRepositoryTests
{
    [Fact]
    public void Constructor_NormalizesOptionalValuesAndTimestamps()
    {
        DateTimeOffset localTime = new(
            2026,
            8,
            15,
            12,
            30,
            0,
            TimeSpan.FromHours(3));

        ProfileRepository repository = new(
            CreateReference("octocat", "hello-world"),
            " main ",
            " C# ",
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            localTime,
            localTime);

        Assert.Equal("main", repository.DefaultBranch);
        Assert.Equal("C#", repository.PrimaryLanguage);
        Assert.Equal(
            DateTimeOffset.Parse("2026-08-15T09:30:00Z"),
            repository.LastPushedAt);
        Assert.Equal(
            DateTimeOffset.Parse("2026-08-15T09:30:00Z"),
            repository.CreatedAt);
    }

    [Fact]
    public void Constructor_PreservesUndeterminedConditionsWithoutOptimisticFallback()
    {
        ProfileRepository repository = new(
            CreateReference("octocat", "hello-world"),
            defaultBranch: null,
            primaryLanguage: null,
            RepositoryConditionState.Undetermined,
            RepositoryConditionState.Undetermined,
            RepositoryConditionState.Undetermined,
            RepositoryConditionState.Undetermined,
            lastPushedAt: null,
            createdAt: null);

        Assert.Equal(RepositoryConditionState.Undetermined, repository.Fork);
        Assert.Equal(RepositoryConditionState.Undetermined, repository.TemplateDerived);
        Assert.Equal(RepositoryConditionState.Undetermined, repository.Archived);
        Assert.Equal(RepositoryConditionState.Undetermined, repository.Empty);
        Assert.Null(repository.LastPushedAt);
    }

    [Fact]
    public void Constructor_RejectsUndefinedConditionState()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProfileRepository(
            CreateReference("octocat", "hello-world"),
            defaultBranch: null,
            primaryLanguage: null,
            (RepositoryConditionState)99,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            lastPushedAt: null,
            createdAt: null));
    }

    private static ProfileRepositoryReference CreateReference(string owner, string name) =>
        new(owner, name, new Uri($"https://github.com/{owner}/{name}"));
}

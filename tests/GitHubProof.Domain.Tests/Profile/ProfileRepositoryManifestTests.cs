using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class ProfileRepositoryManifestTests
{
    [Fact]
    public void Constructor_OrdersRepositoriesDeterministicallyRegardlessOfInputOrder()
    {
        ProfileRepositoryManifest first = new(
        [
            CreateRepository("octocat", "zeta"),
            CreateRepository("octocat", "Alpha"),
            CreateRepository("Octocat", "beta")
        ]);
        ProfileRepositoryManifest second = new(
        [
            CreateRepository("Octocat", "beta"),
            CreateRepository("octocat", "zeta"),
            CreateRepository("octocat", "Alpha")
        ]);

        Assert.Equal(
            ["Octocat/beta", "octocat/Alpha", "octocat/zeta"],
            first.Repositories.Select(repository => repository.Reference.FullName));
        Assert.Equal(
            first.Repositories.Select(repository => repository.Reference.FullName),
            second.Repositories.Select(repository => repository.Reference.FullName));
    }

    [Fact]
    public void Constructor_RejectsDuplicateRepositoryReferences()
    {
        Assert.Throws<ArgumentException>(() => new ProfileRepositoryManifest(
        [
            CreateRepository("octocat", "hello-world"),
            CreateRepository("octocat", "hello-world")
        ]));
    }

    [Fact]
    public void Constructor_ReportsCollectedCountWhenComplete()
    {
        ProfileRepositoryManifest manifest = new(
        [
            CreateRepository("octocat", "hello-world")
        ]);

        Assert.True(manifest.IsComplete);
        Assert.Equal(1, manifest.TotalCount);
    }

    [Fact]
    public void Constructor_LeavesTotalCountUnknownWhenIncomplete()
    {
        ProfileRepositoryManifest manifest = new(
            [CreateRepository("octocat", "hello-world")],
            isComplete: false);

        Assert.False(manifest.IsComplete);
        Assert.Null(manifest.TotalCount);
    }

    [Fact]
    public void Constructor_RejectsTotalCountBelowCollectedCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProfileRepositoryManifest(
            [CreateRepository("octocat", "hello-world")],
            totalCount: 0));
    }

    [Fact]
    public void Empty_ExposesCompleteEmptyManifest()
    {
        Assert.Empty(ProfileRepositoryManifest.Empty.Repositories);
        Assert.True(ProfileRepositoryManifest.Empty.IsComplete);
        Assert.Equal(0, ProfileRepositoryManifest.Empty.TotalCount);
    }

    private static ProfileRepository CreateRepository(string owner, string name) =>
        new(
            new ProfileRepositoryReference(
                owner,
                name,
                new Uri($"https://github.com/{owner}/{name}")),
            defaultBranch: "main",
            primaryLanguage: null,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            lastPushedAt: null,
            createdAt: null);
}

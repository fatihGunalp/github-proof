using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;

namespace GitHubProof.Web.Tests;

// Session 131 — the minimum fixture the repository-concurrency wiring tests
// need to drive the production-composed orchestrator. It builds admissible
// repositories and trivially successful collection results; it makes no
// analysis, eligibility or evidence decision of its own.
internal static class ProfileConcurrencyWiringSupport
{
    private static readonly DateTimeOffset CapturedAt =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    public static readonly AnalysisVersion Version =
        new(
            analyzerVersion: "1.2.0",
            ruleSetVersion: "2026.08.21",
            reportSchemaVersion: 1);

    public static GitHubProfileSnapshot CreateSnapshot(int repositoryCount) =>
        new(
            new GitHubProfileIdentity("octocat", new Uri("https://github.com/octocat")),
            CapturedAt,
            metadata: null,
            new ProfileRepositoryManifest(
                Enumerable.Range(0, repositoryCount).Select(CreateRepository),
                isComplete: true),
            availability: null,
            collectionWarnings: null);

    public static ProfileRepositorySnapshotResult Collected(
        ProfileRepositoryReference reference) =>
        new(
            new RepositorySnapshot(
                new RepositoryIdentity(
                    reference.Owner,
                    reference.Name,
                    reference.Url,
                    "main",
                    isPublic: true,
                    isArchived: false,
                    isFork: false,
                    isTemplate: false,
                    primaryLanguage: "C#"),
                CapturedAt,
                headCommitSha: "abc123",
                metadata: null,
                readme: null,
                fileTree: null,
                commits: null,
                issues: null,
                pullRequests: null,
                releases: null,
                workflows: null,
                topics: null,
                license: null,
                languages: null,
                contributors: null,
                availability: null,
                collectionWarnings: null),
            DataAvailability.Available("repository-metadata", CapturedAt));

    private static ProfileRepository CreateRepository(int index) =>
        new(
            new ProfileRepositoryReference(
                "octocat",
                $"repo-{index}",
                new Uri($"https://github.com/octocat/repo-{index}")),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork: RepositoryConditionState.Absent,
            templateDerived: RepositoryConditionState.Absent,
            archived: RepositoryConditionState.Absent,
            empty: RepositoryConditionState.Absent,
            lastPushedAt: null,
            createdAt: null);
}

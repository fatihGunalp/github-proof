using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Tests.Profile;

internal static class ProfileAnalysisTestSupport
{
    public const string MetadataDataKey = "repository-metadata";

    public static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-16T09:00:00Z");

    public static readonly DateTimeOffset EvaluationTime =
        DateTimeOffset.Parse("2026-08-16T10:00:00Z");

    public static readonly AnalysisVersion TestAnalysisVersion =
        new("test-analyzer", "test-rules", 1);

    public static readonly EligibilityPolicyVersion TestPolicyVersion =
        new("test-eligibility-policy");

    public static ProfileRepositoryReference CreateReference(
        string name,
        string owner = "octocat") =>
        new(owner, name, new Uri($"https://github.com/{owner}/{name}"));

    public static ProfileRepository CreateProfileRepository(
        string name,
        RepositoryConditionState empty = RepositoryConditionState.Absent,
        string owner = "octocat") =>
        new(
            CreateReference(name, owner),
            defaultBranch: "main",
            primaryLanguage: "C#",
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            RepositoryConditionState.Absent,
            empty,
            lastPushedAt: null,
            createdAt: null);

    public static GitHubProfileSnapshot CreateProfileSnapshot(
        IEnumerable<ProfileRepository> repositories,
        bool isComplete = true) =>
        new(
            new GitHubProfileIdentity(
                "octocat",
                new Uri("https://github.com/octocat")),
            CapturedAt,
            metadata: null,
            new ProfileRepositoryManifest(repositories, isComplete: isComplete),
            [DataAvailability.Available("profile-metadata", CapturedAt)],
            collectionWarnings: null);

    public static RepositorySnapshot CreateRepositorySnapshot(
        string name,
        string owner = "octocat") =>
        new(
            new RepositoryIdentity(
                owner,
                name,
                new Uri($"https://github.com/{owner}/{name}"),
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
            collectionWarnings: null);

    public static ProfileRepositorySnapshotResult CreateCollected(
        RepositorySnapshot snapshot) =>
        new(snapshot, DataAvailability.Available(MetadataDataKey, CapturedAt));

    public static ProfileRepositorySnapshotResult CreateUnavailable(
        DataAvailabilityStatus status = DataAvailabilityStatus.Forbidden) =>
        new(
            snapshot: null,
            DataAvailability.Unavailable(
                MetadataDataKey,
                status,
                "GitHub denied access to this repository."));

    public static RepositoryAnalysisRunner CreateAnalysisRunner() =>
        new(new FixedTimeProvider(EvaluationTime));

    public sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    public sealed class FakeSnapshotSource(
        IReadOnlyDictionary<string, Func<ProfileRepositorySnapshotResult>> responses)
        : IProfileRepositorySnapshotSource
    {
        public List<string> RequestedRepositories { get; } = [];

        public Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(reference);
            RequestedRepositories.Add(reference.FullName);

            if (!responses.TryGetValue(
                    reference.FullName,
                    out Func<ProfileRepositorySnapshotResult>? response))
            {
                throw new InvalidOperationException(
                    $"No collection response is configured for '{reference.FullName}'.");
            }

            return Task.FromResult(response());
        }
    }

    public sealed class FailingAnalysisRunner(
        IRepositoryAnalysisRunner inner,
        string failingRepositoryName)
        : IRepositoryAnalysisRunner
    {
        public RepositoryAnalysis Analyze(
            RepositorySnapshot snapshot,
            AnalysisVersion version,
            Guid analysisId)
        {
            if (string.Equals(
                    snapshot.Repository.Name,
                    failingRepositoryName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Repository analysis could not be composed.");
            }

            return inner.Analyze(snapshot, version, analysisId);
        }
    }

    public sealed class SequentialAnalysisIdFactory
    {
        private int next;

        public Guid Next() =>
            Guid.Parse($"04800000-0000-0000-0000-{++next:D12}");
    }
}

using System.Globalization;
using System.Reflection;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class SnapshotIdentityTests
{
    private static readonly DateTimeOffset CapturedAt =
        new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    // The two questions the reference answers are never merged: the same
    // normalized semantic snapshot observed twice keeps one identity.
    [Fact]
    public void SameSemanticSnapshot_DifferentCapturedAt_HasTheSameIdentity()
    {
        SnapshotReference first = Snapshot(CapturedAt).CreateReference();
        SnapshotReference second = Snapshot(CapturedAt.AddDays(90)).CreateReference();

        Assert.Equal(first.SnapshotIdentity, second.SnapshotIdentity);
    }

    [Fact]
    public void SnapshotReference_StillPreservesTheDifferentCapturedAt()
    {
        SnapshotReference first = Snapshot(CapturedAt).CreateReference();
        SnapshotReference second = Snapshot(CapturedAt.AddDays(90)).CreateReference();

        Assert.Equal(CapturedAt, first.CapturedAt);
        Assert.Equal(CapturedAt.AddDays(90), second.CapturedAt);
        Assert.NotEqual(first, second);
    }

    // Observation time under another field name is excluded too.
    [Fact]
    public void AvailabilityRetrievalTime_DoesNotParticipate()
    {
        GitHubProfileSnapshot early = Snapshot(
            CapturedAt,
            availability: [DataAvailability.Available("repositories", CapturedAt)]);

        GitHubProfileSnapshot late = Snapshot(
            CapturedAt,
            availability:
            [
                DataAvailability.Available("repositories", CapturedAt.AddDays(30))
            ]);

        Assert.Equal(
            early.CreateReference().SnapshotIdentity,
            late.CreateReference().SnapshotIdentity);
    }

    [Fact]
    public void AvailabilityStatusAndReason_DoParticipate()
    {
        GitHubProfileSnapshot available = Snapshot(
            CapturedAt,
            availability: [DataAvailability.Available("repositories", CapturedAt)]);

        GitHubProfileSnapshot unavailable = Snapshot(
            CapturedAt,
            availability:
            [
                DataAvailability.Unavailable(
                    "repositories",
                    DataAvailabilityStatus.RateLimited,
                    "rate limited")
            ]);

        Assert.NotEqual(
            available.CreateReference().SnapshotIdentity,
            unavailable.CreateReference().SnapshotIdentity);
    }

    [Fact]
    public void Identity_IsDeterministicAcrossRepeatedConstruction()
    {
        SnapshotIdentity first = Snapshot(CapturedAt).CreateReference().SnapshotIdentity;
        SnapshotIdentity second = Snapshot(CapturedAt).CreateReference().SnapshotIdentity;
        SnapshotIdentity third = Snapshot(CapturedAt).CreateReference().SnapshotIdentity;

        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }

    // Repository order is normalized by the manifest, and availability and
    // collection warnings are written in canonical order, so enumeration order
    // that carries no meaning cannot change identity.
    [Fact]
    public void Identity_IsInsensitiveToSemanticallyIrrelevantEnumerationOrder()
    {
        DataAvailability repositories =
            DataAvailability.Available("repositories", CapturedAt);
        DataAvailability languages =
            DataAvailability.Available("languages", CapturedAt);

        GitHubProfileSnapshot forward = Snapshot(
            CapturedAt,
            repositories: [Repository("alpha"), Repository("beta")],
            availability: [repositories, languages],
            collectionWarnings: ["first", "second"]);

        GitHubProfileSnapshot reversed = Snapshot(
            CapturedAt,
            repositories: [Repository("beta"), Repository("alpha")],
            availability: [languages, repositories],
            collectionWarnings: ["second", "first"]);

        Assert.Equal(
            forward.CreateReference().SnapshotIdentity,
            reversed.CreateReference().SnapshotIdentity);
    }

    [Fact]
    public void RepositoryPresentationMetadata_DoesNotParticipateInSnapshotIdentity()
    {
        // Session 111 STEP 3: repository Description is presentation metadata,
        // never evidence, so a Description-only difference must never change
        // the deterministic snapshot identity.
        ProfileRepository alpha = Repository("alpha");

        GitHubProfileSnapshot withoutDescription = Snapshot(
            CapturedAt,
            repositories: [alpha]);
        GitHubProfileSnapshot withDescription = Snapshot(
            CapturedAt,
            repositories: [alpha],
            repositoryPresentationMetadata: new RepositoryPresentationMetadata(
                new Dictionary<ProfileRepositoryReference, string?>
                {
                    [alpha.Reference] = "A short description."
                }));

        Assert.Equal(
            withoutDescription.CreateReference().SnapshotIdentity,
            withDescription.CreateReference().SnapshotIdentity);
    }

    [Fact]
    public void Identity_ChangesWhenRepositoryEvidenceChanges()
    {
        GitHubProfileSnapshot one = Snapshot(
            CapturedAt,
            repositories: [Repository("alpha")]);

        GitHubProfileSnapshot two = Snapshot(
            CapturedAt,
            repositories: [Repository("alpha"), Repository("beta")]);

        Assert.NotEqual(
            one.CreateReference().SnapshotIdentity,
            two.CreateReference().SnapshotIdentity);
    }

    [Fact]
    public void Identity_ChangesWhenARepositoryConditionChanges()
    {
        GitHubProfileSnapshot active = Snapshot(
            CapturedAt,
            repositories: [Repository("alpha")]);

        GitHubProfileSnapshot archived = Snapshot(
            CapturedAt,
            repositories:
            [
                Repository("alpha", archived: RepositoryConditionState.Present)
            ]);

        Assert.NotEqual(
            active.CreateReference().SnapshotIdentity,
            archived.CreateReference().SnapshotIdentity);
    }

    [Fact]
    public void Identity_IsIndependentOfCurrentCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            SnapshotIdentity turkish =
                Snapshot(CapturedAt).CreateReference().SnapshotIdentity;

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            SnapshotIdentity invariant =
                Snapshot(CapturedAt).CreateReference().SnapshotIdentity;

            Assert.Equal(turkish, invariant);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Identity_IsIndependentOfCaptureOffsetRepresentation()
    {
        SnapshotIdentity utc = Snapshot(CapturedAt).CreateReference().SnapshotIdentity;
        SnapshotIdentity shifted = Snapshot(CapturedAt.ToOffset(TimeSpan.FromHours(5)))
            .CreateReference()
            .SnapshotIdentity;

        Assert.Equal(utc, shifted);
    }

    [Fact]
    public void Identity_ExposesItsDigestAlgorithmAndNoRandomComponent()
    {
        string value = Snapshot(CapturedAt).CreateReference().SnapshotIdentity.Value;

        Assert.StartsWith("sha256:", value, StringComparison.Ordinal);
        Assert.Equal(
            value,
            Snapshot(CapturedAt).CreateReference().SnapshotIdentity.Value);
    }

    // Guard against silent omission: every public value-carrying member of the
    // types that describe the canonical snapshot semantics must be named in
    // that type's own semantic content, except declared observation time.
    [Theory]
    [InlineData(typeof(GitHubProfileSnapshot), nameof(GitHubProfileSnapshot.CapturedAt))]
    [InlineData(typeof(GitHubProfileMetadata), null)]
    [InlineData(typeof(ProfileRepositoryManifest), null)]
    [InlineData(typeof(ProfileRepository), null)]
    [InlineData(typeof(ProfileRepositoryReference), null)]
    [InlineData(typeof(GitHubProfileIdentity), null)]
    public void SemanticContent_DoesNotSilentlyOmitCanonicalMembers(
        Type type,
        string? excludedObservationTime)
    {
        string source = ReadSource(type);
        int start = source.IndexOf("WriteSemanticContent", StringComparison.Ordinal);
        Assert.True(start >= 0, $"{type.Name} must describe its own semantic content.");

        string body = source[start..];

        foreach (PropertyInfo property in type.GetProperties(
                     BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0
                || property.Name == excludedObservationTime
                || IsDerived(type, property))
            {
                continue;
            }

            Assert.True(
                body.Contains(property.Name, StringComparison.Ordinal),
                $"{type.Name}.{property.Name} is not covered by its semantic content.");
        }
    }

    [Fact]
    public void CapturedAt_IsTheOnlyExcludedSnapshotMember()
    {
        string body = ReadSource(typeof(GitHubProfileSnapshot));

        Assert.DoesNotContain(
            ".Write(nameof(CapturedAt)",
            body,
            StringComparison.Ordinal);
    }

    private static bool IsDerived(Type type, PropertyInfo property) =>
        // Convenience projections over members already written.
        (type == typeof(ProfileRepositoryReference) && property.Name == "FullName")
        || (type == typeof(GitHubProfileSnapshot) && property.Name == "Availability")
        || (type == typeof(GitHubProfileSnapshot) && property.Name == "CollectionWarnings")
        // Session 111 STEP 3: non-evidence repository presentation metadata
        // (Description). It carries no evidence/eligibility/determinism
        // meaning, so it deliberately never participates in the semantic
        // snapshot identity.
        || (type == typeof(GitHubProfileSnapshot)
            && property.Name == "RepositoryPresentationMetadata");

    private static string ReadSource(Type type)
    {
        string root = FindRepositoryRoot();
        string[] matches = Directory.GetFiles(
            Path.Combine(root, "src"),
            $"{type.Name}.cs",
            SearchOption.AllDirectories);

        Assert.Single(matches);

        return File.ReadAllText(matches[0]);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null
               && !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return directory!.FullName;
    }

    private static ProfileRepository Repository(
        string name,
        RepositoryConditionState archived = RepositoryConditionState.Absent) =>
        new(
            CreateReference(name),
            defaultBranch: "main",
            primaryLanguage: "C#",
            fork: RepositoryConditionState.Absent,
            templateDerived: RepositoryConditionState.Absent,
            archived,
            empty: RepositoryConditionState.Absent,
            lastPushedAt: new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
            createdAt: new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static GitHubProfileSnapshot Snapshot(
        DateTimeOffset capturedAt,
        IReadOnlyList<ProfileRepository>? repositories = null,
        IReadOnlyList<DataAvailability>? availability = null,
        IReadOnlyList<string>? collectionWarnings = null,
        RepositoryPresentationMetadata? repositoryPresentationMetadata = null) =>
        new(
            new GitHubProfileIdentity("octocat", new Uri("https://github.com/octocat")),
            capturedAt,
            new GitHubProfileMetadata(
                "The Octocat",
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                publicRepositoryCount: 2,
                avatarUrl: new Uri("https://avatars.githubusercontent.com/u/1")),
            new ProfileRepositoryManifest(repositories ?? [Repository("alpha")]),
            availability,
            collectionWarnings,
            repositoryPresentationMetadata);
}

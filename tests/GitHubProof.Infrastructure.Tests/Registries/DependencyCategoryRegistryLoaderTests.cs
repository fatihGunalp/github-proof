using System.Text;
using System.Text.Json;
using GitHubProof.Domain.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Infrastructure.Registries;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.Registries;

public sealed class DependencyCategoryRegistryLoaderTests
{
    [Fact]
    public void Load_ReadsTheCanonicalSeedWithItsMappingVersion()
    {
        DependencyCategoryRegistry registry = new DependencyCategoryRegistryLoader().Load();

        Assert.Equal(
            DependencyCategoryRegistryLoader.CanonicalMappingVersion,
            registry.MappingVersion.Value);
        Assert.Equal(46, registry.Entries.Count);
    }

    [Fact]
    public void Load_SeedUsesOnlyTheLockedCategoryKeys()
    {
        DependencyCategoryRegistry registry = new DependencyCategoryRegistryLoader().Load();

        Assert.All(
            registry.Entries,
            entry => Assert.True(Enum.IsDefined(entry.Category)));
        Assert.Equal(
            new[]
            {
                DependencySemanticCategory.DatabaseAccess,
                DependencySemanticCategory.WebApplicationFramework,
                DependencySemanticCategory.HttpClient,
                DependencySemanticCategory.Authentication,
                DependencySemanticCategory.MachineLearning
            }.OrderBy(category => category),
            registry.Entries
                .Select(entry => entry.Category)
                .Distinct()
                .OrderBy(category => category));
    }

    [Fact]
    public void Load_SeedCategoryCountsMatchTheApprovedProposal()
    {
        DependencyCategoryRegistry registry = new DependencyCategoryRegistryLoader().Load();

        Assert.Equal(
            new[]
            {
                (DependencySemanticCategory.DatabaseAccess, 23),
                (DependencySemanticCategory.WebApplicationFramework, 6),
                (DependencySemanticCategory.HttpClient, 9),
                (DependencySemanticCategory.Authentication, 3),
                (DependencySemanticCategory.MachineLearning, 5)
            }.OrderBy(item => item.Item1),
            registry.Entries
                .GroupBy(entry => entry.Category)
                .Select(group => (group.Key, group.Count()))
                .OrderBy(item => item.Key));
    }

    [Fact]
    public void Load_SeedEntriesAreStoredAsNormalizedLookupIdentities()
    {
        DependencyCategoryRegistry registry = new DependencyCategoryRegistryLoader().Load();

        Assert.All(
            registry.Entries,
            entry => Assert.Equal(
                DependencyIdentityNormalizer.Normalize(entry.Ecosystem, entry.LookupIdentity),
                entry.LookupIdentity));
    }

    [Fact]
    public void Load_SeedResolvesKnownPackagesAndLeavesUnknownOnesUnmapped()
    {
        DependencyCategoryRegistry registry = new DependencyCategoryRegistryLoader().Load();

        Assert.Equal(
            DependencySemanticCategory.DatabaseAccess,
            registry.Resolve(Declaration(
                DependencyEcosystem.DotNet,
                "Microsoft.EntityFrameworkCore")));
        Assert.Equal(
            DependencySemanticCategory.MachineLearning,
            registry.Resolve(Declaration(DependencyEcosystem.Python, "scikit_learn")));
        Assert.Null(registry.Resolve(Declaration(DependencyEcosystem.Python, "numpy")));
        Assert.Null(registry.Resolve(Declaration(DependencyEcosystem.Python, "pandas")));
        Assert.Null(registry.Resolve(Declaration(DependencyEcosystem.JavaScript, "express")));
    }

    [Fact]
    public void CanonicalArtefact_IsStoredInDeterministicDiffFriendlyOrder()
    {
        using Stream stream = typeof(DependencyCategoryRegistryLoader).Assembly
            .GetManifestResourceStream(
                "GitHubProof.Infrastructure.Registries.dependency-category-mapping.1.0.0.json")!;
        using JsonDocument document = JsonDocument.Parse(stream);

        (string Ecosystem, string Identity)[] order = document.RootElement
            .GetProperty("entries")
            .EnumerateArray()
            .Select(entry => (
                entry.GetProperty("ecosystem").GetString()!,
                entry.GetProperty("identity").GetString()!))
            .ToArray();

        Assert.Equal(
            order
                .OrderBy(entry => entry.Ecosystem, StringComparer.Ordinal)
                .ThenBy(entry => entry.Identity, StringComparer.Ordinal),
            order);
    }

    [Fact]
    public void Load_WhenTheArtefactDeclaresAnotherVersion_Rejects()
    {
        Assert.Throws<InvalidOperationException>(() => new DependencyCategoryRegistryLoader()
            .Read(
                new MemoryStream(Encoding.UTF8.GetBytes(
                    """
                    {
                      "mappingVersion": "9.9.9",
                      "entries": [
                        { "ecosystem": "Rust", "identity": "sqlx", "category": "database-access", "rationale": "ORM." }
                      ]
                    }
                    """)),
                expectedMappingVersion: "1.0.0"));
    }

    [Fact]
    public void Load_WhenTheRequestedVersionIsNotPublished_Rejects()
    {
        Assert.Throws<InvalidOperationException>(
            () => new DependencyCategoryRegistryLoader().Load("1.1.0"));
    }

    [Fact]
    public void Read_RejectsAnAliasThatIsNotNormalized()
    {
        Assert.Throws<InvalidOperationException>(() => Read(
            """
            {
              "mappingVersion": "1.0.0",
              "entries": [
                {
                  "ecosystem": "DotNet",
                  "identity": "dapper",
                  "category": "database-access",
                  "aliases": ["Dapper.Contrib"],
                  "rationale": "Micro-ORM."
                }
              ]
            }
            """));
    }

    [Fact]
    public void Read_RejectsDuplicateEcosystemAndIdentity()
    {
        Assert.Throws<ArgumentException>(() => Read(
            """
            {
              "mappingVersion": "1.0.0",
              "entries": [
                { "ecosystem": "Rust", "identity": "sqlx", "category": "database-access", "rationale": "ORM." },
                { "ecosystem": "Rust", "identity": "sqlx", "category": "http-client", "rationale": "Duplicate." }
              ]
            }
            """));
    }

    [Fact]
    public void Read_RejectsAnEntryWithoutRationale()
    {
        Assert.Throws<InvalidOperationException>(() => Read(
            """
            {
              "mappingVersion": "1.0.0",
              "entries": [
                { "ecosystem": "Rust", "identity": "sqlx", "category": "database-access", "rationale": " " }
              ]
            }
            """));
    }

    [Fact]
    public void Read_RejectsAnUnknownCategoryKey()
    {
        Assert.Throws<InvalidOperationException>(() => Read(
            """
            {
              "mappingVersion": "1.0.0",
              "entries": [
                { "ecosystem": "Rust", "identity": "sqlx", "category": "http-server", "rationale": "Deferred category." }
              ]
            }
            """));
    }

    [Fact]
    public void Read_RejectsAnIdentityThatIsNotItsNormalizedLookupForm()
    {
        Assert.Throws<InvalidOperationException>(() => Read(
            """
            {
              "mappingVersion": "1.0.0",
              "entries": [
                { "ecosystem": "DotNet", "identity": "Microsoft.EntityFrameworkCore", "category": "database-access", "rationale": "ORM." }
              ]
            }
            """));
    }

    [Fact]
    public void Read_RejectsAMissingMappingVersion()
    {
        Assert.Throws<InvalidOperationException>(() => Read(
            """
            {
              "entries": [
                { "ecosystem": "Rust", "identity": "sqlx", "category": "database-access", "rationale": "ORM." }
              ]
            }
            """));
    }

    private static DependencyCategoryRegistry Read(string json) =>
        new DependencyCategoryRegistryLoader().Read(
            new MemoryStream(Encoding.UTF8.GetBytes(json)));

    private static RepositoryDependencyDeclaration Declaration(
        DependencyEcosystem ecosystem,
        string packageId) =>
        new(ecosystem, "manifest", packageId, declaredVersion: null);
}

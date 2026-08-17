using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class RepositoryDependencyDeclarationTests
{
    [Fact]
    public void Constructor_NormalizesIdentityAndPreservesTheDeclaredVersionLiteral()
    {
        RepositoryDependencyDeclaration declaration = new(
            DependencyEcosystem.JavaScript,
            " package.json ",
            " express ",
            " ^4.18.2 ");

        Assert.Equal("package.json", declaration.ManifestPath);
        Assert.Equal("express", declaration.PackageId);
        Assert.Equal("^4.18.2", declaration.DeclaredVersion);
    }

    [Fact]
    public void Constructor_WhenNoVersionIsDeclared_KeepsItAbsent()
    {
        RepositoryDependencyDeclaration declaration = new(
            DependencyEcosystem.Go,
            "go.mod",
            "github.com/gin-gonic/gin",
            declaredVersion: "   ");

        Assert.Null(declaration.DeclaredVersion);
    }

    [Theory]
    [InlineData("", "package")]
    [InlineData("manifest", "")]
    public void Constructor_WhenIdentityIsMissing_Rejects(
        string manifestPath,
        string packageId)
    {
        Assert.Throws<ArgumentException>(() => new RepositoryDependencyDeclaration(
            DependencyEcosystem.DotNet,
            manifestPath,
            packageId,
            declaredVersion: null));
    }

    [Fact]
    public void Constructor_WhenEcosystemIsUndefined_Rejects()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RepositoryDependencyDeclaration(
            (DependencyEcosystem)99,
            "package.json",
            "express",
            declaredVersion: null));
    }

    [Fact]
    public void Snapshot_OrdersDeclarationsDeterministicallyAndCollapsesRepeats()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryCollection<RepositoryDependencyDeclaration>(
            [
                new RepositoryDependencyDeclaration(
                    DependencyEcosystem.JavaScript, "package.json", "express", "1.0.0"),
                new RepositoryDependencyDeclaration(
                    DependencyEcosystem.DotNet, "src/App.csproj", "Serilog", "3.1.1"),
                new RepositoryDependencyDeclaration(
                    DependencyEcosystem.JavaScript, "package.json", "axios", "2.0.0"),
                new RepositoryDependencyDeclaration(
                    DependencyEcosystem.JavaScript, "package.json", "express", "1.0.0")
            ]));

        Assert.Equal(
            new[]
            {
                "package.json:axios",
                "package.json:express",
                "src/App.csproj:Serilog"
            },
            snapshot.DependencyDeclarations.Items.Select(declaration =>
                $"{declaration.ManifestPath}:{declaration.PackageId}"));
        Assert.True(snapshot.DependencyDeclarations.IsComplete);
    }

    [Fact]
    public void Snapshot_WhenDeclarationsAreIncomplete_PreservesIncompleteness()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            new RepositoryCollection<RepositoryDependencyDeclaration>(
                [
                    new RepositoryDependencyDeclaration(
                        DependencyEcosystem.Rust, "Cargo.toml", "serde", "1.0")
                ],
                isComplete: false));

        Assert.False(snapshot.DependencyDeclarations.IsComplete);
        Assert.Single(snapshot.DependencyDeclarations.Items);
    }

    [Fact]
    public void Snapshot_WithoutDependencyEvidence_ExposesAnEmptyCompleteCollection()
    {
        RepositorySnapshot snapshot = CreateSnapshot(declarations: null);

        Assert.Empty(snapshot.DependencyDeclarations.Items);
        Assert.True(snapshot.DependencyDeclarations.IsComplete);
    }

    private static RepositorySnapshot CreateSnapshot(
        RepositoryCollection<RepositoryDependencyDeclaration>? declarations) =>
        new(
            new RepositoryIdentity(
                "owner",
                "repository",
                new Uri("https://github.com/owner/repository"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            DateTimeOffset.Parse("2026-08-15T09:00:00Z"),
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
            collectionWarnings: null,
            tags: null,
            licenseFile: null,
            documentationMarkdown: null,
            declarations);
}

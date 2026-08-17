using GitHubProof.Domain.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis;

public sealed class RepositorySemanticEvidencePolicyTests
{
    private static readonly DependencyCategoryMappingVersion MappingVersion =
        new("1.0.0");

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-17T09:00:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public void Compose_TwoPackagesInOneCategory_ProduceOneObservationWithBothDeclarations()
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(
                Declaration(DependencyEcosystem.Python, "requirements.txt", "sqlalchemy"),
                Declaration(DependencyEcosystem.Python, "requirements.txt", "psycopg2")));

        DependencySemanticCategoryEvidence category = Assert.Single(evidence.Categories);

        Assert.Equal(DependencySemanticCategory.DatabaseAccess, category.Category);
        Assert.Equal(
            new[] { "psycopg2", "sqlalchemy" },
            category.SupportingDeclarations.Select(declaration => declaration.PackageId));
    }

    [Fact]
    public void Compose_DifferentCategories_ProduceSeparateObservations()
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(
                Declaration(DependencyEcosystem.Python, "requirements.txt", "sqlalchemy"),
                Declaration(DependencyEcosystem.Python, "requirements.txt", "requests"),
                Declaration(DependencyEcosystem.Python, "requirements.txt", "django")));

        Assert.Equal(
            new[]
            {
                DependencySemanticCategory.DatabaseAccess,
                DependencySemanticCategory.WebApplicationFramework,
                DependencySemanticCategory.HttpClient
            },
            evidence.Categories.Select(category => category.Category));
    }

    [Fact]
    public void Compose_UnknownPackage_RemainsUnmapped()
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(
                Declaration(DependencyEcosystem.Python, "requirements.txt", "some-internal-tool")));

        Assert.Empty(evidence.Categories);
        Assert.True(evidence.IsCategorySurfaceComplete);
    }

    [Fact]
    public void Compose_PreservesSourceDeclarationReferences()
    {
        RepositoryDependencyDeclaration declaration = Declaration(
            DependencyEcosystem.JavaScript,
            "package.json",
            "sequelize",
            "^6.0.0");

        RepositorySemanticEvidence evidence = Compose(CreateSnapshot(declaration));

        Assert.Same(
            declaration,
            Assert.Single(Assert.Single(evidence.Categories).SupportingDeclarations));
    }

    [Fact]
    public void Compose_GoMajorVersionIdentities_StayDistinct()
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(
                Declaration(DependencyEcosystem.Go, "go.mod", "github.com/jackc/pgx/v5"),
                Declaration(DependencyEcosystem.Go, "go.mod", "github.com/jackc/pgx/v4")));

        DependencySemanticCategoryEvidence category = Assert.Single(evidence.Categories);

        Assert.Equal(
            "github.com/jackc/pgx/v5",
            Assert.Single(category.SupportingDeclarations).PackageId);
    }

    [Theory]
    [InlineData("SQLx")]
    [InlineData("sql_x")]
    [InlineData("SQLX")]
    public void Compose_RustIdentity_IsNeverCaseOrHyphenNormalized(string packageId)
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(
                Declaration(DependencyEcosystem.Rust, "Cargo.toml", packageId)));

        Assert.Empty(evidence.Categories);
    }

    [Fact]
    public void Compose_DotNetAndPythonIdentities_UseTheirCanonicalLookupForm()
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(
                Declaration(DependencyEcosystem.DotNet, "src/App.csproj", "Microsoft.EntityFrameworkCore"),
                Declaration(DependencyEcosystem.Python, "pyproject.toml", "Psycopg2_Binary")));

        DependencySemanticCategoryEvidence category = Assert.Single(evidence.Categories);

        Assert.Equal(DependencySemanticCategory.DatabaseAccess, category.Category);
        Assert.Equal(2, category.SupportingDeclarations.Count);
    }

    [Fact]
    public void Compose_UnavailableDependencyEvidence_IsNotAbsence()
    {
        RepositorySemanticEvidence evidence = Compose(CreateSnapshot(
            availability: DataAvailability.Unavailable(
                RepositoryDataKeys.DependencyManifests,
                DataAvailabilityStatus.Failed,
                "GitHub dependency manifest request failed."),
            isComplete: false));

        Assert.Empty(evidence.Categories);
        Assert.False(evidence.IsCategorySurfaceComplete);
        Assert.Equal(
            DataAvailabilityStatus.Failed,
            evidence.DependencyManifestsAvailability!.Status);
    }

    [Fact]
    public void Compose_IncompleteDependencyEvidence_StaysIncomplete()
    {
        RepositorySemanticEvidence evidence = Compose(CreateSnapshot(
            new[] { Declaration(DependencyEcosystem.Rust, "Cargo.toml", "sqlx") },
            isComplete: false));

        Assert.Single(evidence.Categories);
        Assert.False(evidence.AreDependencyDeclarationsComplete);
        Assert.False(evidence.IsCategorySurfaceComplete);
    }

    [Fact]
    public void Compose_WhenAvailabilityWasNeverRecorded_IsUndetermined()
    {
        RepositorySemanticEvidence evidence = Compose(
            CreateSnapshot(availability: null));

        Assert.Null(evidence.DependencyManifestsAvailability);
        Assert.False(evidence.IsCategorySurfaceComplete);
    }

    [Fact]
    public void Compose_CarriesMappingVersionAndAnalysisReference()
    {
        RepositorySnapshot snapshot = CreateSnapshot(
            Declaration(DependencyEcosystem.Ruby, "Gemfile", "devise"));
        RepositoryAnalysis analysis = CreateAnalysis(snapshot);

        RepositorySemanticEvidence evidence =
            new RepositorySemanticEvidencePolicy(CreateRegistry()).Compose(analysis);

        Assert.Equal(MappingVersion, evidence.MappingVersion);
        Assert.Equal(analysis.AnalysisId, evidence.Analysis.AnalysisId);
        Assert.Equal(analysis.Repository, evidence.Analysis.Repository);
        Assert.Equal(analysis.Version, evidence.Analysis.Version);
    }

    [Fact]
    public void Compose_WithSameInputs_IsDeterministic()
    {
        static RepositorySnapshot Snapshot() => CreateSnapshot(
            Declaration(DependencyEcosystem.Ruby, "Gemfile", "devise"),
            Declaration(DependencyEcosystem.Python, "requirements.txt", "requests"),
            Declaration(DependencyEcosystem.Python, "requirements.txt", "sqlalchemy"),
            Declaration(DependencyEcosystem.JavaScript, "package.json", "pg"));

        Assert.Equal(Describe(Compose(Snapshot())), Describe(Compose(Snapshot())));
    }

    [Fact]
    public void Compose_WhenAnalysisIsMissing_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(
            () => new RepositorySemanticEvidencePolicy(CreateRegistry()).Compose(null!));

        Assert.Throws<ArgumentNullException>(
            () => new RepositorySemanticEvidencePolicy(null!));
    }

    private static IEnumerable<string> Describe(RepositorySemanticEvidence evidence) =>
        evidence.Categories.Select(category =>
            $"{category.Category}:{string.Join(
                ",",
                category.SupportingDeclarations.Select(declaration =>
                    $"{declaration.ManifestPath}/{declaration.PackageId}"))}");

    private static RepositorySemanticEvidence Compose(RepositorySnapshot snapshot) =>
        new RepositorySemanticEvidencePolicy(CreateRegistry())
            .Compose(CreateAnalysis(snapshot));

    private static DependencyCategoryRegistry CreateRegistry() =>
        new(
            MappingVersion,
            [
                Entry(DependencyEcosystem.DotNet, "microsoft.entityframeworkcore", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Python, "sqlalchemy", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Python, "psycopg2", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Python, "psycopg2-binary", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Python, "requests", DependencySemanticCategory.HttpClient),
                Entry(DependencyEcosystem.Python, "django", DependencySemanticCategory.WebApplicationFramework),
                Entry(DependencyEcosystem.JavaScript, "sequelize", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.JavaScript, "pg", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Go, "github.com/jackc/pgx/v5", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Rust, "sqlx", DependencySemanticCategory.DatabaseAccess),
                Entry(DependencyEcosystem.Ruby, "devise", DependencySemanticCategory.Authentication)
            ]);

    private static DependencyCategoryMappingEntry Entry(
        DependencyEcosystem ecosystem,
        string identity,
        DependencySemanticCategory category) =>
        new(ecosystem, identity, category);

    private static RepositoryDependencyDeclaration Declaration(
        DependencyEcosystem ecosystem,
        string manifestPath,
        string packageId,
        string? declaredVersion = null) =>
        new(ecosystem, manifestPath, packageId, declaredVersion);

    private static RepositorySnapshot CreateSnapshot(
        params RepositoryDependencyDeclaration[] declarations) =>
        CreateSnapshot(declarations, isComplete: true);

    private static RepositorySnapshot CreateSnapshot(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations,
        bool isComplete) =>
        CreateSnapshot(
            declarations,
            isComplete,
            DataAvailability.Available(
                RepositoryDataKeys.DependencyManifests,
                CapturedAt));

    private static RepositorySnapshot CreateSnapshot(
        DataAvailability? availability,
        bool isComplete = true) =>
        CreateSnapshot([], isComplete, availability);

    private static RepositorySnapshot CreateSnapshot(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations,
        bool isComplete,
        DataAvailability? availability) =>
        new(
            new RepositoryIdentity(
                "octocat",
                "hello-world",
                new Uri("https://github.com/octocat/hello-world"),
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
            availability is null ? null : new[] { availability },
            collectionWarnings: null,
            tags: null,
            licenseFile: null,
            documentationMarkdown: null,
            new RepositoryCollection<RepositoryDependencyDeclaration>(
                declarations,
                isComplete: isComplete));

    private static RepositoryAnalysis CreateAnalysis(RepositorySnapshot snapshot) =>
        new(
            Guid.Parse("04800000-0000-0000-0000-000000000001"),
            snapshot.Repository,
            snapshot,
            AnalysisStatus.Partial,
            Version,
            CapturedAt,
            CapturedAt,
            signalResults: null,
            categoryResults: null,
            new ScoreBreakdown(categoryBreakdowns: null),
            strengths: null,
            criticalGaps: null,
            new ImprovementPlan([], CapturedAt, Version.RuleSetVersion),
            limitationKeys: null,
            warnings: null);
}

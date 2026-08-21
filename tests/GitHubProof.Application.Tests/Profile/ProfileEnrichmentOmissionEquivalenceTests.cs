using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile;

// The profile-level half of the Session 096 §6 semantic-equivalence invariant,
// implemented in Session 097. The Infrastructure tests prove the optimized
// collection composition issues no source or dependency request and produces a
// snapshot whose only difference is the two absent enrichment surfaces; these
// tests prove that exact difference changes nothing a Profile V1 result is made
// of, all the way to the projected ProfileReport.
public sealed class ProfileEnrichmentOmissionEquivalenceTests
{
    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    [Fact]
    public async Task OmittingBothEnrichmentSurfaces_ChangesNoProfileLevelTruth()
    {
        ProfilePipelineResult withEnrichment = await RunAsync(withEnrichment: true);
        ProfilePipelineResult withoutEnrichment = await RunAsync(withEnrichment: false);

        AssertSemanticallyEquivalent(withEnrichment, withoutEnrichment);
    }

    [Fact]
    public async Task OmittingBothEnrichmentSurfaces_ChangesNoProfileAnalysisStatus()
    {
        ProfilePipelineResult withEnrichment = await RunAsync(withEnrichment: true);
        ProfilePipelineResult withoutEnrichment = await RunAsync(withEnrichment: false);

        Assert.Equal(
            withEnrichment.Profile!.AnalysisStatus,
            withoutEnrichment.Profile!.AnalysisStatus);
        Assert.Equal(
            ProfileAnalysisStatus.Complete,
            withoutEnrichment.Profile.AnalysisStatus);
    }

    [Fact]
    public async Task OmittingBothEnrichmentSurfaces_ChangesNoLimitationOrWarning()
    {
        ProfilePipelineResult withEnrichment = await RunAsync(withEnrichment: true);
        ProfilePipelineResult withoutEnrichment = await RunAsync(withEnrichment: false);

        Assert.Equal(
            withEnrichment.Profile!.Limitations.Select(limitation => limitation.Code),
            withoutEnrichment.Profile!.Limitations.Select(limitation => limitation.Code));
        Assert.Equal(
            withEnrichment.Profile.Warnings.Select(warning => warning.Code),
            withoutEnrichment.Profile.Warnings.Select(warning => warning.Code));
    }

    // TechnicalFocus and EngineeringPractice are the two supported V1 pattern
    // families, so they are checked by family rather than only in aggregate.
    [Fact]
    public async Task OmittingBothEnrichmentSurfaces_ChangesNoPromotedPatternFamily()
    {
        ProfilePipelineResult withEnrichment = await RunAsync(withEnrichment: true);
        ProfilePipelineResult withoutEnrichment = await RunAsync(withEnrichment: false);

        Assert.Equal(
            PatternKeys(withEnrichment, ProfilePatternFamily.TechnicalFocus),
            PatternKeys(withoutEnrichment, ProfilePatternFamily.TechnicalFocus));
        Assert.Equal(
            PatternKeys(withEnrichment, ProfilePatternFamily.EngineeringPractice),
            PatternKeys(withoutEnrichment, ProfilePatternFamily.EngineeringPractice));

        // The fixture is only meaningful if it actually promotes something.
        Assert.NotEmpty(
            PatternKeys(withoutEnrichment, ProfilePatternFamily.TechnicalFocus));
    }

    [Fact]
    public async Task OmittingBothEnrichmentSurfaces_ChangesNoReportSectionContentOrOrdering()
    {
        ProfilePipelineResult withEnrichment = await RunAsync(withEnrichment: true);
        ProfilePipelineResult withoutEnrichment = await RunAsync(withEnrichment: false);

        Assert.Equal(
            withEnrichment.Report!.Sections.Select(section =>
                $"{section.SectionKey}:{section.Availability}"),
            withoutEnrichment.Report!.Sections.Select(section =>
                $"{section.SectionKey}:{section.Availability}"));
        Assert.Equal(
            withEnrichment.Report.TechnicalFocusAreas.Items.Select(item => item.Pattern.PatternKey),
            withoutEnrichment.Report.TechnicalFocusAreas.Items.Select(item => item.Pattern.PatternKey));
        Assert.Equal(
            withEnrichment.Report.EngineeringPracticePatterns.Items.Select(item => item.Pattern.PatternKey),
            withoutEnrichment.Report.EngineeringPracticePatterns.Items.Select(item => item.Pattern.PatternKey));
        Assert.Equal(
            withEnrichment.Report.RepresentativeRepositories.Items.Select(item => item.Repository),
            withoutEnrichment.Report.RepresentativeRepositories.Items.Select(item => item.Repository));
    }

    private static IReadOnlyList<string> PatternKeys(
        ProfilePipelineResult result,
        ProfilePatternFamily family) =>
        [
            .. result.Profile!.RepeatedProfilePatterns
                .Where(pattern => pattern.Family == family)
                .Select(pattern => pattern.PatternKey)
        ];

    private static async Task<ProfilePipelineResult> RunAsync(bool withEnrichment)
    {
        ProfileRepository[] repositories =
        [
            CreateRepository("alpha"),
            CreateRepository("beta"),
            CreateRepository("gamma")
        ];

        Dictionary<string, Func<ProfileRepositorySnapshotResult>> responses =
            repositories.ToDictionary(
                repository => repository.Reference.FullName,
                repository => new Func<ProfileRepositorySnapshotResult>(
                    () => new ProfileRepositorySnapshotResult(
                        CreateSnapshot(repository.Reference.Name, withEnrichment),
                        DataAvailability.Available("metadata", CapturedAt))));

        FakeSnapshotSource source = new(responses);

        return await CreatePipeline(source, new SequentialAnalysisIdFactory().Next)
            .RunAsync(CreateProfileSnapshot(repositories), Version);
    }

    // Identical in every canonical surface; the two arms differ only in the
    // enrichment collections and their availability entries — exactly the
    // difference the optimized Profile collection composition produces.
    private static RepositorySnapshot CreateSnapshot(string name, bool withEnrichment)
    {
        List<DataAvailability> availability =
        [
            DataAvailability.Available("readme", CapturedAt),
            DataAvailability.Available(ProfileEvidenceKeys.LanguagesDataKey, CapturedAt)
        ];

        if (withEnrichment)
        {
            availability.Add(
                DataAvailability.Available("dependency-manifests", CapturedAt));
            availability.Add(DataAvailability.Available("source-files", CapturedAt));
        }

        return new RepositorySnapshot(
            new RepositoryIdentity(
                "octocat",
                name,
                new Uri($"https://github.com/octocat/{name}"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            metadata: null,
            new RepositoryReadme("README.md", "# Project\n\nOverview."),
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            new Dictionary<string, long> { ["C#"] = 1200 },
            contributors: null,
            availability,
            collectionWarnings: null,
            tags: null,
            licenseFile: null,
            documentationMarkdown: null,
            withEnrichment
                ? new RepositoryCollection<RepositoryDependencyDeclaration>(
                    [
                        new RepositoryDependencyDeclaration(
                            DependencyEcosystem.DotNet,
                            "src/App.csproj",
                            "Serilog",
                            "3.1.1")
                    ])
                : null,
            withEnrichment
                ? new RepositoryCollection<RepositorySourceFile>(
                    [new RepositorySourceFile("src/Program.cs", "class Program { }")])
                : null);
    }
}

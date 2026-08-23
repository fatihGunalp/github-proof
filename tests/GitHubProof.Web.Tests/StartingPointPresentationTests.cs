using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Web.Presentation;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 111 STEP 2: StartingPointPresentation.Select is a Web-only
// PRESENTATION BOUND over the existing canonical RepresentativeRepositories
// order — never a ranking, score or reordering of its own. These tests prove
// the bound in isolation, against real (not stubbed) Domain types, since the
// Razor structural tests in ProfileResultPresentationTests cannot exercise
// actual rendering behavior.
public sealed class StartingPointPresentationTests
{
    [Fact]
    public void Select_WithNoItems_ReturnsAnEmptyResult()
    {
        Assert.Empty(StartingPointPresentation.Select([]));
    }

    [Fact]
    public void Select_WithOneItem_ReturnsThatOneItem()
    {
        RepresentativeRepository alpha = CreateRepresentative("alpha");

        IReadOnlyList<RepresentativeRepository> presented =
            StartingPointPresentation.Select([alpha]);

        Assert.Equal([alpha], presented);
    }

    [Fact]
    public void Select_WithTwoItems_ReturnsBothInOrder()
    {
        RepresentativeRepository alpha = CreateRepresentative("alpha");
        RepresentativeRepository beta = CreateRepresentative("beta");

        IReadOnlyList<RepresentativeRepository> presented =
            StartingPointPresentation.Select([alpha, beta]);

        Assert.Equal([alpha, beta], presented);
    }

    [Fact]
    public void Select_WithExactlyThreeItems_ReturnsAllThreeInOrder()
    {
        RepresentativeRepository alpha = CreateRepresentative("alpha");
        RepresentativeRepository beta = CreateRepresentative("beta");
        RepresentativeRepository gamma = CreateRepresentative("gamma");

        IReadOnlyList<RepresentativeRepository> presented =
            StartingPointPresentation.Select([alpha, beta, gamma]);

        Assert.Equal([alpha, beta, gamma], presented);
    }

    [Fact]
    public void Select_WithFourOrMoreItems_ReturnsOnlyTheFirstThreeInTheExistingOrder()
    {
        RepresentativeRepository alpha = CreateRepresentative("alpha");
        RepresentativeRepository beta = CreateRepresentative("beta");
        RepresentativeRepository gamma = CreateRepresentative("gamma");
        RepresentativeRepository delta = CreateRepresentative("delta");
        RepresentativeRepository epsilon = CreateRepresentative("epsilon");

        IReadOnlyList<RepresentativeRepository> presented =
            StartingPointPresentation.Select([alpha, beta, gamma, delta, epsilon]);

        Assert.Equal([alpha, beta, gamma], presented);
        Assert.DoesNotContain(delta, presented);
        Assert.DoesNotContain(epsilon, presented);
    }

    [Fact]
    public void Select_NeverReordersTheExistingCanonicalSequence()
    {
        // The bound must never re-rank: reversing the canonical input reverses
        // which three items survive the prefix cut, proving Select takes a
        // prefix rather than reselecting by any property of its own.
        RepresentativeRepository alpha = CreateRepresentative("alpha");
        RepresentativeRepository beta = CreateRepresentative("beta");
        RepresentativeRepository gamma = CreateRepresentative("gamma");
        RepresentativeRepository delta = CreateRepresentative("delta");

        IReadOnlyList<RepresentativeRepository> forward =
            StartingPointPresentation.Select([alpha, beta, gamma, delta]);
        IReadOnlyList<RepresentativeRepository> reversed =
            StartingPointPresentation.Select([delta, gamma, beta, alpha]);

        Assert.Equal([alpha, beta, gamma], forward);
        Assert.Equal([delta, gamma, beta], reversed);
    }

    [Fact]
    public void Select_DoesNotMutateTheCanonicalSourceCollection()
    {
        RepresentativeRepository alpha = CreateRepresentative("alpha");
        RepresentativeRepository beta = CreateRepresentative("beta");
        RepresentativeRepository gamma = CreateRepresentative("gamma");
        RepresentativeRepository delta = CreateRepresentative("delta");
        List<RepresentativeRepository> canonical = [alpha, beta, gamma, delta];

        StartingPointPresentation.Select(canonical);

        Assert.Equal([alpha, beta, gamma, delta], canonical);
        Assert.Equal(4, canonical.Count);
    }

    [Fact]
    public void Select_IsDeterministicAcrossRepeatedCalls()
    {
        RepresentativeRepository alpha = CreateRepresentative("alpha");
        RepresentativeRepository beta = CreateRepresentative("beta");
        RepresentativeRepository gamma = CreateRepresentative("gamma");
        RepresentativeRepository delta = CreateRepresentative("delta");
        RepresentativeRepository[] canonical = [alpha, beta, gamma, delta];

        Assert.Equal(
            StartingPointPresentation.Select(canonical),
            StartingPointPresentation.Select(canonical));
    }

    [Fact]
    public void Select_RejectsANullSource()
    {
        Assert.Throws<ArgumentNullException>(
            () => StartingPointPresentation.Select(null!));
    }

    // ---------------------------------------------------------------- fixture
    //
    // Builds a minimal but fully valid RepresentativeRepository: one
    // TechnicalFocus pattern independently supported by the given repository
    // and a fixed second "partner" repository (RepeatedProfilePattern requires
    // at least two independent supporting repositories), with the given
    // repository winning representative status for that pattern.

    private static readonly AnalysisVersion TestAnalysisVersion =
        new("test-analyzer", "test-rules", 1);

    private static RepresentativeRepository CreateRepresentative(string name)
    {
        ProfileRepositoryReference repository = CreateReference(name);
        ProfileRepositoryReference partner = CreateReference($"{name}-partner");

        ProfileEvidenceReference evidence = new(
            repository,
            new RepositoryAnalysisReference(
                Guid.NewGuid(),
                CreateRepositoryIdentity(name),
                TestAnalysisVersion),
            "languages",
            evidenceKeys: null);

        // Both repositories must contribute evidence to the same observation:
        // RepeatedProfilePattern requires every independent supporting
        // repository to actually be one of the observation's contributors.
        ProfileEvidenceReference partnerEvidence = new(
            partner,
            new RepositoryAnalysisReference(
                Guid.NewGuid(),
                CreateRepositoryIdentity($"{name}-partner"),
                TestAnalysisVersion),
            "languages",
            evidenceKeys: null);

        ObservedProfileEvidence observation = new(
            "languages",
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence,
            [evidence, partnerEvidence],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            ObservationRuleVersion.V1);

        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.TechnicalFocus,
            "C#",
            [observation],
            [repository, partner],
            PatternRuleVersion.V1);

        RepresentativePatternSupport support = new(
            pattern,
            repository,
            [evidence]);

        return new RepresentativeRepository(repository, [support]);
    }

    private static ProfileRepositoryReference CreateReference(string name) =>
        new(
            "octocat",
            name,
            new Uri($"https://github.com/octocat/{name}"));

    private static RepositoryIdentity CreateRepositoryIdentity(string name) =>
        new(
            "octocat",
            name,
            new Uri($"https://github.com/octocat/{name}"),
            "main",
            isPublic: true,
            isArchived: false,
            isFork: false,
            isTemplate: false,
            primaryLanguage: "C#");
}

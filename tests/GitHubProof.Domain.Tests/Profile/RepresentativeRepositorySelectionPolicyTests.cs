using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepresentativeRepositorySelectionPolicyTests
{
    private const string SignalKey = "validation.test-structure";

    [Fact]
    public void Select_EmptyProfilePatternSet_ProducesZeroRepresentatives()
    {
        ProfilePatternSet patternSet = new(
            [],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: false);

        RepresentativeRepositorySelection selection = Select(patternSet);

        Assert.Empty(selection.RepresentativeRepositories);
        Assert.Equal(TestPatternRuleVersion, selection.PatternRuleVersion);
        Assert.Equal(RuleVersion, selection.ObservationRuleVersion);
        Assert.Equal(PolicyVersion, selection.EligibilityPolicyVersion);
        Assert.False(selection.IsEvidenceSurfaceComplete);
        Assert.Equal(TestRepresentativeRepositoryRuleVersion, selection.RuleVersion);
    }

    [Fact]
    public void Select_OnePattern_ProducesExactlyOneSupportAndOneWinner()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [
                CreateObservation(SignalKey, [alpha, beta], 2),
                CreateObservation(
                    "validation.continuous-integration",
                    [alpha],
                    2)
            ],
            [alpha, beta]);

        RepresentativeRepositorySelection selection = Select(CreateSet([pattern]));

        RepresentativeRepository representative =
            Assert.Single(selection.RepresentativeRepositories);
        Assert.Equal("octocat/alpha", representative.Repository.FullName);
        RepresentativePatternSupport support =
            Assert.Single(representative.PatternSupports);
        Assert.Equal("automated-testing", support.PatternKey);
        Assert.Equal(2, support.PatternSupportObservationCount);
    }

    [Fact]
    public void Select_MultiplePatternsWithDifferentWinners_ProducesOneSupportPerPattern()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern patternA = CreatePattern(
            "pattern-a",
            [
                CreateObservation("process.issue-usage", [alpha, beta], 2),
                CreateObservation("process.release-usage", [alpha], 2)
            ],
            [alpha, beta]);
        RepeatedProfilePattern patternB = CreatePattern(
            "pattern-b",
            [
                CreateObservation(SignalKey, [alpha, beta], 2),
                CreateObservation("validation.continuous-integration", [beta], 2)
            ],
            [alpha, beta]);

        RepresentativeRepositorySelection selection =
            Select(CreateSet([patternA, patternB]));

        Assert.Equal(2, selection.RepresentativeRepositories.Count);
        Assert.All(
            selection.RepresentativeRepositories,
            representative => Assert.Single(representative.PatternSupports));
        Assert.Equal(
            "pattern-a",
            RepresentativeFor(selection, alpha).PatternSupports[0].PatternKey);
        Assert.Equal(
            "pattern-b",
            RepresentativeFor(selection, beta).PatternSupports[0].PatternKey);
    }

    [Fact]
    public void Select_MultiplePatternsWithTheSameWinner_GroupsIntoOneRepresentative()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern patternA = CreatePattern(
            "pattern-a",
            [CreateObservation("process.issue-usage", [alpha, beta], 2)],
            [alpha, beta]);
        RepeatedProfilePattern patternB = CreatePattern(
            "pattern-b",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha, beta]);
        RepeatedProfilePattern patternC = CreatePattern(
            "pattern-c",
            [CreateObservation("process.release-usage", [alpha, beta], 2)],
            [alpha, beta]);

        RepresentativeRepositorySelection selection =
            Select(CreateSet([patternA, patternB, patternC]));

        // All three patterns tie between alpha and beta, so Owner->Name
        // ordinal always resolves to alpha: one RepresentativeRepository
        // groups all three RepresentativePatternSupports.
        RepresentativeRepository representative =
            Assert.Single(selection.RepresentativeRepositories);
        Assert.Equal("octocat/alpha", representative.Repository.FullName);
        Assert.Equal(
            new[] { "pattern-a", "pattern-b", "pattern-c" },
            representative.PatternSupports.Select(support => support.PatternKey));
    }

    [Fact]
    public void Select_EveryPatternIsRepresentedExactlyOnceAcrossTheWholeSelection()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment gamma = CreateContribution("gamma");
        RepeatedProfilePattern patternA = CreatePattern(
            "pattern-a",
            [CreateObservation("process.issue-usage", [alpha, beta], 2)],
            [alpha, beta]);
        RepeatedProfilePattern patternB = CreatePattern(
            "pattern-b",
            [
                CreateObservation(SignalKey, [gamma, alpha], 2),
                CreateObservation("validation.continuous-integration", [gamma], 2)
            ],
            [alpha, gamma]);
        RepeatedProfilePattern patternC = CreatePattern(
            "pattern-c",
            [CreateObservation("process.release-usage", [alpha, beta], 2)],
            [alpha, beta]);

        ProfilePatternSet patternSet = CreateSet([patternA, patternB, patternC]);
        RepresentativeRepositorySelection selection = Select(patternSet);

        int totalSupports = selection.RepresentativeRepositories
            .Sum(representative => representative.PatternSupports.Count);
        Assert.Equal(patternSet.Patterns.Count, totalSupports);

        IEnumerable<string> representedPatternKeys = selection.RepresentativeRepositories
            .SelectMany(representative => representative.PatternSupports)
            .Select(support => support.PatternKey);
        Assert.Equal(
            representedPatternKeys.Distinct(),
            representedPatternKeys);
        Assert.Equal(
            patternSet.Patterns.Select(pattern => pattern.PatternKey).OrderBy(key => key),
            representedPatternKeys.OrderBy(key => key));
    }

    [Fact]
    public void Select_HoldsTheExactCanonicalRepeatedProfilePatternInstance()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha, beta]);
        ProfilePatternSet patternSet = CreateSet([pattern]);

        RepresentativeRepositorySelection selection = Select(patternSet);

        RepresentativePatternSupport support = selection.RepresentativeRepositories
            .Single()
            .PatternSupports
            .Single();
        Assert.Same(patternSet.Patterns.Single(), support.Pattern);
    }

    [Fact]
    public void Select_NeverPicksARepositoryOutsideIndependentSupportingRepositories()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment forked = CreateContribution(
            "forked",
            fork: RepositoryConditionState.Present);

        // forked has evidence in two observations (higher raw count than
        // alpha/beta) but is excluded from IndependentSupportingRepositories,
        // exactly as fork lineage requires (§4).
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [
                CreateObservation(SignalKey, [alpha, beta, forked], 3),
                CreateObservation(
                    "validation.continuous-integration",
                    [forked],
                    3)
            ],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion);

        RepresentativeRepositorySelection selection = Select(CreateSet([pattern]));

        RepresentativeRepository representative =
            Assert.Single(selection.RepresentativeRepositories);
        Assert.Equal("octocat/alpha", representative.Repository.FullName);
    }

    [Fact]
    public void Select_PicksTheCandidateWithTheHigherPatternSupportObservationCount()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "documentation",
            [
                CreateObservation("documentation.installation-guide", [alpha, beta], 2),
                CreateObservation("documentation.usage-or-demo", [alpha], 2)
            ],
            [alpha, beta]);

        RepresentativeRepositorySelection selection = Select(CreateSet([pattern]));

        Assert.Equal(
            "octocat/alpha",
            selection.RepresentativeRepositories.Single().Repository.FullName);
    }

    [Fact]
    public void Select_WhenCountsTie_BreaksByNameOrdinal()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha, beta]);

        RepresentativeRepositorySelection selection = Select(CreateSet([pattern]));

        Assert.Equal(
            "octocat/alpha",
            selection.RepresentativeRepositories.Single().Repository.FullName);
    }

    [Fact]
    public void Select_WhenCountsTie_OwnerOrdinalTakesPrecedenceOverName()
    {
        ProfileRepositoryReference first = CreateReference("zzz-repo", "aaa-owner");
        ProfileRepositoryReference second = CreateReference("aaa-repo", "zzz-owner");
        ObservedProfileEvidence observation = new(
            SignalKey,
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            [
                CreateRawEvidenceReference(first, SignalKey),
                CreateRawEvidenceReference(second, SignalKey)
            ],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            RuleVersion);
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [observation],
            [first, second],
            TestPatternRuleVersion);

        RepresentativeRepositorySelection selection = Select(CreateSet([pattern]));

        Assert.Equal(
            "aaa-owner/zzz-repo",
            selection.RepresentativeRepositories.Single().Repository.FullName);
    }

    [Fact]
    public void Select_WhenCandidateReferencesCollideNonCanonically_RejectsDeterministically()
    {
        ProfileRepositoryReference canonical = new(
            "octocat", "alpha", new Uri("https://github.com/octocat/alpha"));
        ProfileRepositoryReference mirrored = new(
            "octocat", "alpha", new Uri("https://mirror.example.com/octocat/alpha"));
        RepositoryContributionAssessment beta = CreateContribution("beta");

        // ProfilePatternSet always normalizes pattern order by (Family,
        // PatternKey) ordinal, so passing the same two patterns in reverse
        // array order would not actually change traversal order. To prove
        // rejection is independent of which conflicting reference is
        // encountered first, swap which pattern ("pattern-a", visited
        // first) carries the canonical vs. the mirrored reference instead.
        RepeatedProfilePattern canonicalFirst = CreateRawPattern(
            "pattern-a", canonical, beta);
        RepeatedProfilePattern mirroredSecond = CreateRawPattern(
            "pattern-b", mirrored, beta);
        RepeatedProfilePattern mirroredFirst = CreateRawPattern(
            "pattern-a", mirrored, beta);
        RepeatedProfilePattern canonicalSecond = CreateRawPattern(
            "pattern-b", canonical, beta);

        Assert.Throws<ArgumentException>(
            () => Select(CreateSet([canonicalFirst, mirroredSecond])));
        Assert.Throws<ArgumentException>(
            () => Select(CreateSet([mirroredFirst, canonicalSecond])));
    }

    [Fact]
    public void Select_PatternIsNeverReadAsRepresentativeSpecificEvidence()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [
                CreateObservation(SignalKey, [alpha, beta], 2),
                CreateObservation(
                    "validation.continuous-integration",
                    [alpha],
                    2)
            ],
            [alpha, beta]);

        RepresentativeRepositorySelection selection = Select(CreateSet([pattern]));

        RepresentativePatternSupport support = selection.RepresentativeRepositories
            .Single()
            .PatternSupports
            .Single();

        // The scoped evidence trace carries only alpha's own evidence.
        Assert.All(
            support.SupportingEvidenceReferences,
            reference => Assert.Equal(alpha.Reference, reference.Repository));

        // The canonical pattern trace still exposes beta's evidence too —
        // proving the two traces are genuinely different, and that beta's
        // evidence never leaked into the scoped trace.
        Assert.Contains(beta.Reference, support.Pattern.SupportingRepositories);
    }

    [Fact]
    public void Select_CrossPatternBreadthNeverInfluencesASinglePatternWinner()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment gamma = CreateContribution("gamma");

        // Pattern A: beta (2 observations) outscores alpha (1) for THIS
        // pattern, so beta must win it.
        RepeatedProfilePattern patternA = CreatePattern(
            "pattern-a",
            [
                CreateObservation(SignalKey, [alpha, beta], 2),
                CreateObservation("validation.continuous-integration", [beta], 2)
            ],
            [alpha, beta]);

        // Pattern B: alpha independently wins on its own merits, giving
        // alpha a broader overall profile footprint (two pattern wins)
        // than beta (one).
        RepeatedProfilePattern patternB = CreatePattern(
            "pattern-b",
            [
                CreateObservation("process.release-usage", [alpha, gamma], 2),
                CreateObservation("process.issue-usage", [alpha], 2)
            ],
            [alpha, gamma]);

        RepresentativeRepositorySelection selection =
            Select(CreateSet([patternA, patternB]));

        // If cross-pattern breadth had leaked in, alpha's greater overall
        // footprint could have wrongly flipped pattern A's own winner.
        Assert.Equal(
            "pattern-a",
            RepresentativeFor(selection, beta).PatternSupports[0].PatternKey);
        Assert.Equal(
            "pattern-b",
            RepresentativeFor(selection, alpha).PatternSupports[0].PatternKey);
    }

    [Fact]
    public void Select_IncompleteEvidenceSurfaceDoesNotBlockSelection()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha, beta]);
        ProfilePatternSet patternSet = new(
            [pattern],
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: false);

        RepresentativeRepositorySelection selection = Select(patternSet);

        Assert.NotEmpty(selection.RepresentativeRepositories);
        Assert.False(selection.IsEvidenceSurfaceComplete);
    }

    [Fact]
    public void Select_PreservesVersionsAndCompletenessFromTheSourceSet()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation(SignalKey, [alpha, beta], 2)],
            [alpha, beta]);
        ProfilePatternSet patternSet = CreateSet([pattern]);

        RepresentativeRepositorySelection selection = Select(patternSet);

        Assert.Equal(patternSet.RuleVersion, selection.PatternRuleVersion);
        Assert.Equal(patternSet.ObservationRuleVersion, selection.ObservationRuleVersion);
        Assert.Equal(patternSet.PolicyVersion, selection.EligibilityPolicyVersion);
        Assert.Equal(patternSet.IsEvidenceSurfaceComplete, selection.IsEvidenceSurfaceComplete);
        Assert.Equal(TestRepresentativeRepositoryRuleVersion, selection.RuleVersion);
        Assert.NotEqual(selection.RuleVersion.Value, selection.PatternRuleVersion.Value);
    }

    [Fact]
    public void Select_IsDeterministicAcrossRepeatedExecutions()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment gamma = CreateContribution("gamma");
        RepeatedProfilePattern patternA = CreatePattern(
            "pattern-a",
            [CreateObservation("process.issue-usage", [alpha, beta], 2)],
            [alpha, beta]);
        RepeatedProfilePattern patternB = CreatePattern(
            "pattern-b",
            [CreateObservation(SignalKey, [gamma, alpha], 2)],
            [alpha, gamma]);
        ProfilePatternSet patternSet = CreateSet([patternA, patternB]);
        RepresentativeRepositorySelectionPolicy policy =
            new(TestRepresentativeRepositoryRuleVersion);

        RepresentativeRepositorySelection first = policy.Select(patternSet);
        RepresentativeRepositorySelection second = policy.Select(patternSet);

        Assert.Equal(
            first.RepresentativeRepositories.Select(r => r.Repository.FullName),
            second.RepresentativeRepositories.Select(r => r.Repository.FullName));
        Assert.Equal(
            first.RepresentativeRepositories.Select(
                r => r.PatternSupports.Select(support => support.PatternKey)),
            second.RepresentativeRepositories.Select(
                r => r.PatternSupports.Select(support => support.PatternKey)));
    }

    [Fact]
    public void Constructor_WhenRuleVersionIsMissing_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(
            () => new RepresentativeRepositorySelectionPolicy(null!));
    }

    [Fact]
    public void Select_WhenPatternSetIsMissing_FailsFast()
    {
        Assert.Throws<ArgumentNullException>(
            () => new RepresentativeRepositorySelectionPolicy(
                    TestRepresentativeRepositoryRuleVersion)
                .Select(null!));
    }

    private static RepresentativeRepositorySelection Select(ProfilePatternSet patternSet) =>
        new RepresentativeRepositorySelectionPolicy(TestRepresentativeRepositoryRuleVersion)
            .Select(patternSet);

    private static RepresentativeRepository RepresentativeFor(
        RepresentativeRepositorySelection selection,
        RepositoryContributionAssessment contribution) =>
        selection.RepresentativeRepositories.Single(
            representative => representative.Repository == contribution.Reference);

    private static ProfilePatternSet CreateSet(IEnumerable<RepeatedProfilePattern> patterns) =>
        new(
            patterns,
            TestPatternRuleVersion,
            RuleVersion,
            PolicyVersion,
            isEvidenceSurfaceComplete: true);

    private static RepeatedProfilePattern CreatePattern(
        string patternKey,
        IEnumerable<ObservedProfileEvidence> supportingObservations,
        IEnumerable<RepositoryContributionAssessment> independentSupporters) =>
        new(
            ProfilePatternFamily.EngineeringPractice,
            patternKey,
            supportingObservations,
            independentSupporters.Select(contribution => contribution.Reference),
            TestPatternRuleVersion);

    private static RepeatedProfilePattern CreateRawPattern(
        string patternKey,
        ProfileRepositoryReference primary,
        RepositoryContributionAssessment secondarySupporter)
    {
        ObservedProfileEvidence observation = new(
            SignalKey,
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            [
                CreateRawEvidenceReference(primary, SignalKey),
                CreateEvidenceReference(secondarySupporter, SignalKey)
            ],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            RuleVersion);

        return new RepeatedProfilePattern(
            ProfilePatternFamily.EngineeringPractice,
            patternKey,
            [observation],
            [primary, secondarySupporter.Reference],
            TestPatternRuleVersion);
    }

    private static ProfileEvidenceReference CreateRawEvidenceReference(
        ProfileRepositoryReference repository,
        string evidenceKey) =>
        new(
            repository,
            CreateAnalysisReference(repository.Name, repository.Owner),
            evidenceKey,
            null);
}

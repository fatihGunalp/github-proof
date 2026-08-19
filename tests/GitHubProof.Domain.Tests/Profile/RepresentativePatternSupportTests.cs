using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Domain.Tests.Profile.ProfileContributionTestSupport;

namespace GitHubProof.Domain.Tests.Profile;

public sealed class RepresentativePatternSupportTests
{
    [Fact]
    public void Constructor_HoldsTheExactCanonicalPatternInstance()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha, beta]);

        RepresentativePatternSupport support = new(
            pattern,
            alpha.Reference,
            ScopedEvidenceFor(pattern, alpha.Reference));

        Assert.Same(pattern, support.Pattern);
        Assert.Equal(ProfilePatternFamily.EngineeringPractice, support.PatternFamily);
        Assert.Equal(pattern.PatternKey, support.PatternKey);
    }

    [Fact]
    public void Constructor_UsesTheSuppliedEvidenceDirectly_NeverReDerivedFromPattern()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "documentation",
            [
                CreateObservation("documentation.usage-or-demo", [alpha, beta], 2),
                CreateObservation("documentation.installation-guide", [alpha, beta], 2)
            ],
            [alpha, beta]);

        // Pattern.SupportingObservations would derive two evidence references
        // for alpha. Deliberately pass only one, pre-computed, to prove the
        // constructor uses exactly what it is given rather than falling back
        // to re-deriving the full set from Pattern.
        ProfileEvidenceReference[] partialEvidence =
        [
            ScopedEvidenceFor(pattern, alpha.Reference)
                .Single(reference => reference.EvidenceKey == "documentation.installation-guide")
        ];

        RepresentativePatternSupport support = new(
            pattern,
            alpha.Reference,
            partialEvidence);

        Assert.Equal(1, support.PatternSupportObservationCount);
        Assert.Equal(
            "documentation.installation-guide",
            Assert.Single(support.SupportingEvidenceReferences).EvidenceKey);

        // The full, un-scoped derivation from Pattern would have been 2 —
        // confirming the constructor did not silently recompute it.
        Assert.Equal(2, ScopedEvidenceFor(pattern, alpha.Reference).Count);
    }

    [Fact]
    public void Constructor_ScopesEvidenceToTheRepositoryOnly()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha, beta]);

        RepresentativePatternSupport support = new(
            pattern,
            alpha.Reference,
            ScopedEvidenceFor(pattern, alpha.Reference));

        Assert.All(
            support.SupportingEvidenceReferences,
            reference => Assert.Equal(alpha.Reference, reference.Repository));
        Assert.DoesNotContain(
            support.SupportingEvidenceReferences,
            reference => reference.Repository == beta.Reference);

        // Pattern remains the full canonical object: it still exposes beta's
        // evidence, proving the scoped evidence trace and the canonical
        // pattern trace are genuinely different views.
        Assert.Contains(beta.Reference, support.Pattern.SupportingRepositories);
    }

    [Fact]
    public void Constructor_OrdersSuppliedEvidenceByEvidenceKeyOrdinal()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "documentation",
            [
                CreateObservation("documentation.usage-or-demo", [alpha, beta], 2),
                CreateObservation("documentation.installation-guide", [alpha, beta], 2)
            ],
            [alpha, beta]);

        // Supplied deliberately out of EvidenceKey order to prove the
        // constructor still normalizes what it is given.
        ProfileEvidenceReference[] unordered =
        [
            .. ScopedEvidenceFor(pattern, alpha.Reference).OrderByDescending(
                reference => reference.EvidenceKey, StringComparer.Ordinal)
        ];

        RepresentativePatternSupport support = new(pattern, alpha.Reference, unordered);

        Assert.Equal(
            new[]
            {
                "documentation.installation-guide",
                "documentation.usage-or-demo"
            },
            support.SupportingEvidenceReferences.Select(
                reference => reference.EvidenceKey));
    }

    [Fact]
    public void Constructor_SupportingObservationKeysIsAProjectionOfScopedEvidence()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "documentation",
            [
                CreateObservation("documentation.usage-or-demo", [alpha, beta], 2),
                CreateObservation("documentation.installation-guide", [alpha, beta], 2)
            ],
            [alpha, beta]);

        RepresentativePatternSupport support = new(
            pattern,
            alpha.Reference,
            ScopedEvidenceFor(pattern, alpha.Reference));

        Assert.Equal(
            support.SupportingEvidenceReferences.Select(
                reference => reference.EvidenceKey),
            support.SupportingObservationKeys);
    }

    [Fact]
    public void SupportingObservationKeys_CannotBeMutatedIntoDivergingFromEvidence()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "documentation",
            [
                CreateObservation("documentation.usage-or-demo", [alpha, beta], 2),
                CreateObservation("documentation.installation-guide", [alpha, beta], 2)
            ],
            [alpha, beta]);

        RepresentativePatternSupport support = new(
            pattern,
            alpha.Reference,
            ScopedEvidenceFor(pattern, alpha.Reference));

        // SupportingObservationKeys must be a live projection, never a stored
        // second source of truth. A hostile/careless caller holding the
        // returned collection (here, casting to the concrete array type
        // .ToArray() actually returns) must not be able to make later reads
        // diverge from SupportingEvidenceReferences.
        IReadOnlyList<string> firstRead = support.SupportingObservationKeys;
        Assert.True(
            firstRead is string[],
            "Test assumption: the projection is currently backed by a fresh array on each read.");
        ((string[])firstRead)[0] = "tampered-key";

        IReadOnlyList<string> secondRead = support.SupportingObservationKeys;
        Assert.DoesNotContain("tampered-key", secondRead);
        Assert.Equal(
            support.SupportingEvidenceReferences.Select(
                reference => reference.EvidenceKey),
            secondRead);

        // Mutating one read must not have poisoned a later one either.
        Assert.NotEqual("tampered-key", secondRead[0]);
    }

    [Fact]
    public void PatternSupportObservationCount_MatchesSuppliedEvidenceCount()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "documentation",
            [
                CreateObservation("documentation.usage-or-demo", [alpha, beta], 2),
                CreateObservation("documentation.installation-guide", [alpha], 2)
            ],
            [alpha, beta]);

        RepresentativePatternSupport alphaSupport = new(
            pattern,
            alpha.Reference,
            ScopedEvidenceFor(pattern, alpha.Reference));
        RepresentativePatternSupport betaSupport = new(
            pattern,
            beta.Reference,
            ScopedEvidenceFor(pattern, beta.Reference));

        Assert.Equal(2, alphaSupport.PatternSupportObservationCount);
        Assert.Equal(1, betaSupport.PatternSupportObservationCount);
    }

    [Fact]
    public void Constructor_WhenRepositoryIsNotAnIndependentSupporter_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepositoryContributionAssessment forked = CreateContribution(
            "forked",
            fork: RepositoryConditionState.Present);

        // forked has real evidence (it is a SupportingRepository) but is
        // deliberately excluded from IndependentSupportingRepositories, as
        // fork lineage requires (§4). It must never become a support.
        RepeatedProfilePattern pattern = new(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta, forked], 3)],
            [alpha.Reference, beta.Reference],
            TestPatternRuleVersion);

        Assert.Contains(forked.Reference, pattern.SupportingRepositories);
        Assert.DoesNotContain(
            forked.Reference,
            pattern.IndependentSupportingRepositories);

        Assert.Throws<ArgumentException>(() => new RepresentativePatternSupport(
            pattern,
            forked.Reference,
            ScopedEvidenceFor(pattern, forked.Reference)));
    }

    [Fact]
    public void Constructor_WhenNoEvidenceIsSupplied_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha, beta]);

        Assert.Throws<ArgumentException>(() => new RepresentativePatternSupport(
            pattern,
            alpha.Reference,
            []));
    }

    [Fact]
    public void Constructor_WhenSuppliedEvidenceBelongsToAnotherRepository_Rejects()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha, beta]);

        Assert.Throws<ArgumentException>(() => new RepresentativePatternSupport(
            pattern,
            alpha.Reference,
            ScopedEvidenceFor(pattern, beta.Reference)));
    }

    [Fact]
    public void Constructor_WhenPatternRepositoryOrEvidenceIsMissing_FailsFast()
    {
        RepositoryContributionAssessment alpha = CreateContribution("alpha");
        RepositoryContributionAssessment beta = CreateContribution("beta");
        RepeatedProfilePattern pattern = CreatePattern(
            "automated-testing",
            [CreateObservation("validation.test-structure", [alpha, beta], 2)],
            [alpha, beta]);
        IReadOnlyList<ProfileEvidenceReference> evidence =
            ScopedEvidenceFor(pattern, alpha.Reference);

        Assert.Throws<ArgumentNullException>(
            () => new RepresentativePatternSupport(null!, alpha.Reference, evidence));
        Assert.Throws<ArgumentNullException>(
            () => new RepresentativePatternSupport(pattern, null!, evidence));
        Assert.Throws<ArgumentNullException>(
            () => new RepresentativePatternSupport(pattern, alpha.Reference, null!));
    }

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
}

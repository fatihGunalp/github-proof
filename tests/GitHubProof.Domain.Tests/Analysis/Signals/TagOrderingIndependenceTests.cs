using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.Signals;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Signals;

// Session 128 — the executable half of the tags ordering proof for
// docs/architecture/public-profile-runtime-collection.md §15.3, §15.5, §19.7.
//
// Half of the tags analysis is a claim about THIS evaluator rather than about
// GitHub: `ReleaseUsageSignalEvaluator` consumes the tag COUNT and nothing else,
// so no provider ordering can move any Profile output through tag identity.
//
// Scope, stated honestly: this file pins that property for
// `ReleaseUsageSignalEvaluator`, which is the only current reader. A NEW
// evaluator that read a tag name would not fail these tests — the claim that no
// evaluator, policy, composer, projector or report section reads a tag name
// rests on a source search recorded in the Session 128 worklog, not here.
//
// Tag identity being unread was necessary for ORDER-PASS but not sufficient:
// the collector deduplicates case-insensitively, which makes the observed COUNT
// order-dependent and is why tags ended ORDER-BLOCKED. That half is pinned in
// GitHubRepositoryTagCollectorTests.
//
// The last two tests are the other half, and they are deliberately negative:
// they show exactly which inputs DO move the output, so the independence above
// cannot be passing vacuously. One of them is the Session 128 blocker in
// miniature — the same observed count reports a different §15.5 evidence value
// depending only on whether another page was believed to exist.
public sealed class TagOrderingIndependenceTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-26T09:00:00Z");

    private static readonly DateTimeOffset RetrievedAt =
        DateTimeOffset.Parse("2026-08-26T09:01:00Z");

    private static readonly DateTimeOffset EvaluatedAt =
        DateTimeOffset.Parse("2026-08-26T09:02:00Z");

    // ------------------------------------------- ordering cannot move anything

    // The same tags in the opposite order. Whatever a provider's sort key is —
    // alphabetical, tag commit date, or undocumented — reversing it changes no
    // Profile output.
    [Theory]
    [InlineData(CollectionObservationState.DecisionBounded)]
    [InlineData(CollectionObservationState.Enumerated)]
    public void ReversingTheObservedTagOrder_ChangesNothing(
        CollectionObservationState state)
    {
        string[] names = ["v1.0.0", "v1.1.0", "v2.0.0", "v2.1.0"];

        Assert.Equal(
            Describe(Evaluate(names, state)),
            Describe(Evaluate([.. names.Reverse()], state)));
    }

    // The stronger claim, and the one that actually licenses ORDER-PASS: two
    // DISJOINT tag sets of equal size are indistinguishable. A different
    // provider ordering does not merely permute the observed prefix — with more
    // tags than one page it returns a different SET — and that is still
    // invisible here.
    [Theory]
    [InlineData(CollectionObservationState.DecisionBounded)]
    [InlineData(CollectionObservationState.Enumerated)]
    public void ADisjointTagSetOfTheSameSize_ChangesNothing(
        CollectionObservationState state)
    {
        SignalResult first = Evaluate(["v1.0.0", "v1.1.0", "v2.0.0"], state);
        SignalResult second = Evaluate(["alpha", "beta", "gamma"], state);

        Assert.Equal(Describe(first), Describe(second));
    }

    // Tag identity is not read even when the tag contribution is the branch that
    // decides the signal: the >= 1 tag rule pins Partial/3 for any non-empty
    // set.
    [Fact]
    public void TheTagContributionBranch_ReadsOnlyWhetherAnyTagExists()
    {
        SignalResult single = Evaluate(["only-one"], CollectionObservationState.Enumerated);
        SignalResult many = Evaluate(
            ["a", "b", "c"],
            CollectionObservationState.Enumerated);

        Assert.Equal(SignalStatus.Partial, single.Status);
        Assert.Equal(3, single.Score);
        Assert.Equal(single.Status, many.Status);
        Assert.Equal(single.Score, many.Score);
    }

    // ------------------------------------------------- what DOES move the output

    // Non-vacuity: the count is read, so the tests above are not passing because
    // the evaluator ignores the tags surface altogether.
    [Fact]
    public void TheObservedTagCount_IsReadAndDoesMoveTheEvidence()
    {
        string one = Describe(Evaluate(["a"], CollectionObservationState.DecisionBounded));
        string two = Describe(Evaluate(["a", "b"], CollectionObservationState.DecisionBounded));

        Assert.NotEqual(one, two);
        Assert.Contains("tagsAtLeast=1", one, StringComparison.Ordinal);
        Assert.Contains("tagsAtLeast=2", two, StringComparison.Ordinal);
    }

    // Why the observation state is load-bearing, in one assertion.
    //
    // The SAME observed tags produce a different §15.5 evidence value depending
    // only on whether the collection stopped deliberately or was enumerated. That
    // is what makes `CollectionObservationState` part of the §6 output rather
    // than an internal detail, and it is why a transport change may not alter it.
    //
    // Session 128 note: an earlier draft of that session claimed REST and GraphQL
    // could disagree about whether another page exists. The architecture owner
    // rejected that claim as unsupported and it is withdrawn — this test asserts
    // only the evaluator behaviour above, which is independent of it.
    [Fact]
    public void TheSameTagsUnderADifferentObservationState_ProduceDifferentEvidence()
    {
        string[] names = ["v1.0.0", "v1.1.0"];

        string bounded = Describe(
            Evaluate(names, CollectionObservationState.DecisionBounded));
        string enumerated = Describe(
            Evaluate(names, CollectionObservationState.Enumerated));

        Assert.NotEqual(bounded, enumerated);
        Assert.Contains("tagsAtLeast=2", bounded, StringComparison.Ordinal);
        Assert.Contains("tagsObservation=decision-bounded", bounded, StringComparison.Ordinal);
        Assert.Contains("tags=2", enumerated, StringComparison.Ordinal);
        Assert.Contains("tagsComplete=True", enumerated, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------- test support

    // Status, score and the full evidence value — everything this signal
    // contributes to a Profile result.
    private static string Describe(SignalResult result) =>
        $"{result.Status}|{result.Score}|" + string.Join(
            ";",
            result.Evidence.Select(item => $"{item.Source}={item.Value}"));

    private static SignalResult Evaluate(
        string[] tagNames,
        CollectionObservationState state) =>
        new ReleaseUsageSignalEvaluator(new FixedTimeProvider(EvaluatedAt))
            .Evaluate(CreateSnapshot(tagNames, state));

    private static RepositorySnapshot CreateSnapshot(
        string[] tagNames,
        CollectionObservationState state)
    {
        RepositoryTag[] tags = [.. tagNames.Select(name => new RepositoryTag(name))];

        RepositoryCollection<RepositoryTag> collection = state switch
        {
            CollectionObservationState.Enumerated =>
                new RepositoryCollection<RepositoryTag>(tags),

            CollectionObservationState.DecisionBounded =>
                RepositoryCollection<RepositoryTag>.DecisionBounded(tags),

            _ => new RepositoryCollection<RepositoryTag>(tags, isComplete: false)
        };

        return new RepositorySnapshot(
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
            CapturedAt,
            headCommitSha: "abc123",
            new RepositoryMetadata(null, null, CapturedAt.AddYears(-2), issuesEnabled: true),
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,

            // No releases at all, so the signal reaches the tag branch rather
            // than terminating on a published release first.
            releases: new RepositoryCollection<RepositoryRelease>([]),
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability:
            [
                DataAvailability.Available("metadata", RetrievedAt),
                DataAvailability.Available("releases", RetrievedAt),
                DataAvailability.Available("tags", RetrievedAt)
            ],
            collectionWarnings: null,
            tags: collection);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

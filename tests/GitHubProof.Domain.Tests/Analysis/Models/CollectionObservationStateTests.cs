using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

// docs/architecture/analysis-domain-model.md "CollectionObservationState".
// IsComplete keeps its exact meaning and is never overloaded; the new state is
// a separate fact standing beside it, and the equivalence is exact in both
// directions.
public sealed class CollectionObservationStateTests
{
    [Fact]
    public void DefaultConstruction_IsEnumeratedAndKeepsTheExactTotal()
    {
        RepositoryCollection<string> collection = new(["a", "b"]);

        Assert.Equal(CollectionObservationState.Enumerated, collection.ObservationState);
        Assert.True(collection.IsComplete);
        Assert.False(collection.IsDecisionBounded);
        Assert.Equal(2, collection.TotalCount);
    }

    // Every existing caller that only knew isComplete keeps meaning exactly
    // what it always meant.
    [Fact]
    public void ExistingIncompleteConstruction_IsIncompleteNotDecisionBounded()
    {
        RepositoryCollection<string> collection = new(["a"], isComplete: false);

        Assert.Equal(CollectionObservationState.Incomplete, collection.ObservationState);
        Assert.False(collection.IsComplete);
        Assert.False(collection.IsDecisionBounded);
        Assert.Null(collection.TotalCount);
    }

    // An Incomplete collection may still carry a provider-supplied total; only
    // DecisionBounded may not.
    [Fact]
    public void IncompleteConstruction_MayStillCarryAKnownTotal()
    {
        RepositoryCollection<string> collection = new(
            ["a"],
            totalCount: 9,
            isComplete: false);

        Assert.Equal(CollectionObservationState.Incomplete, collection.ObservationState);
        Assert.Equal(9, collection.TotalCount);
    }

    [Fact]
    public void DecisionBounded_IsNotCompleteAndClaimsNoTotal()
    {
        RepositoryCollection<string> collection =
            RepositoryCollection<string>.DecisionBounded(["a", "b", "c"]);

        Assert.Equal(
            CollectionObservationState.DecisionBounded,
            collection.ObservationState);
        Assert.False(collection.IsComplete);
        Assert.True(collection.IsDecisionBounded);
        Assert.Null(collection.TotalCount);
        Assert.Equal(3, collection.Items.Count);
    }

    [Fact]
    public void EmptyCollection_IsEnumerated()
    {
        Assert.Equal(
            CollectionObservationState.Enumerated,
            RepositoryCollection<string>.Empty.ObservationState);
    }

    // The impossible combinations the contract names explicitly.
    [Fact]
    public void EnumeratedWithIsCompleteFalse_IsRejected()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new RepositoryCollection<string>(
                ["a"],
                totalCount: null,
                isComplete: false,
                CollectionObservationState.Enumerated));

        Assert.Contains("Enumerated requires", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DecisionBoundedWithIsCompleteTrue_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new RepositoryCollection<string>(
                ["a"],
                totalCount: null,
                isComplete: true,
                CollectionObservationState.DecisionBounded));
    }

    [Fact]
    public void DecisionBoundedWithATotalCount_IsRejected()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new RepositoryCollection<string>(
                ["a"],
                totalCount: 5,
                isComplete: false,
                CollectionObservationState.DecisionBounded));

        Assert.Contains("lower bound", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void IncompleteWithIsCompleteTrue_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new RepositoryCollection<string>(
                ["a"],
                totalCount: null,
                isComplete: true,
                CollectionObservationState.Incomplete));
    }

    // Enumerated means the provider collection was fully enumerated, so an
    // explicitly supplied total that disagrees with the enumerated items is
    // structurally impossible.
    [Fact]
    public void EnumeratedWithAMatchingExplicitTotal_IsValid()
    {
        RepositoryCollection<string> collection = new(["a", "b"], totalCount: 2);

        Assert.Equal(CollectionObservationState.Enumerated, collection.ObservationState);
        Assert.Equal(2, collection.TotalCount);
    }

    [Fact]
    public void EnumeratedWithAMismatchedTotal_IsRejected()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new RepositoryCollection<string>(["a"], totalCount: 9));

        Assert.Contains("fully enumerated", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UndefinedObservationState_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RepositoryCollection<string>(
                ["a"],
                totalCount: null,
                isComplete: false,
                (CollectionObservationState)99));
    }
}

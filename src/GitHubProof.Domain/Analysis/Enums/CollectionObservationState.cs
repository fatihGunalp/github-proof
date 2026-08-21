namespace GitHubProof.Domain.Analysis.Enums;

// How a RepositoryCollection<T> was observed, contracted by
// docs/architecture/analysis-domain-model.md "CollectionObservationState".
//
// This is not a second meaning for IsComplete. It is a separate fact standing
// beside it, and the equivalence is exact:
//
//     IsComplete = true  <=>  Enumerated
//
// Repository Proof only ever produces Enumerated or Incomplete.
public enum CollectionObservationState
{
    // The provider collection was fully enumerated. TotalCount is exact.
    Enumerated = 1,

    // Enumeration stopped deliberately because every currently authorized
    // consumer of this Profile V1 surface could already decide irreversibly
    // from what was observed. Items is a lower bound, never a total, and the
    // bound is not a failure: it produces no collection warning, no
    // limitation and no analysis status downgrade of its own.
    DecisionBounded = 2,

    // Enumeration stopped because of failure, interruption or another
    // inability to complete the required observation. This keeps the exact
    // semantics an incomplete collection has always had.
    Incomplete = 3
}

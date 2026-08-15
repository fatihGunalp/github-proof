# Non-Gating Exploratory Validation

This namespace is separate from the canonical first-10 validation set in
`samples/validation/repositories.json`.

## Acceptance Isolation

- Exploratory repositories do not contribute to the Session 038 denominator or acceptance result.
- The immutable gating baseline remains 10 repositories, 150 signal comparisons, and 92.00% `Agreement + MinorDifference`.
- `samples/validation/results/human-review-summary.md` is not recalculated from this namespace.
- An exploratory result cannot change a canonical signal rule or score within the same session.

## validation-011 Layout

The repository record is created only after both the public GitHub repository URL and the frozen 40-character commit SHA are supplied and verified.

```text
samples/validation/exploratory/
├── repositories.json
└── results/
    └── validation-011/
        ├── snapshot-summary.json
        ├── signal-results.json
        ├── determinism.json
        ├── manual-review.md
        ├── comparison.md
        ├── provenance.md
        └── ai-era-stress-test-observations.md
```

## Blinding Boundary

Before automated results are frozen, the analysis input is limited to the public repository URL. The frozen commit SHA is used only to verify the observed repository head. Known provenance must not be added to:

- the repository manifest used by the collector;
- `RepositorySnapshot`;
- evaluator input;
- `EvidenceItem` or `SignalResult`;
- automated explanations, recommendations, or limitations.

The initial `snapshot-summary.json`, `signal-results.json`, and `determinism.json` must be written before `provenance.md` is created. Provenance is experiment ground-truth metadata, not scoring evidence.

## Human Comparison

After the automated result freeze:

1. Record the disclosed provenance in `provenance.md`.
2. Compare all 15 automated signal results using only `Agreement`, `MinorDifference`, `MajorDifference`, or `CannotDetermine`.
3. Evaluate whether each automated result represents externally observable repository evidence; do not evaluate whether AI was used.
4. Record cross-signal observations in `ai-era-stress-test-observations.md`.
5. Keep any possible rule change as a non-binding follow-up concern.

No result in this namespace may be merged into the first-10 Phase 3 acceptance calculation.

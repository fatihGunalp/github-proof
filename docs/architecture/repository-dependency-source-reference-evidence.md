# Repository Dependency Source Reference Evidence Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Repository-level deterministic dependency source-reference semantic evidence
**Session:** 068
**Date:** 2026-08-17

## 1. Purpose

This contract defines what `DependencySourceReferenceEvidence` may mean, and
what it may never mean.

Input contracts:

- `docs/architecture/repository-dependency-evidence.md`
- `docs/architecture/repository-dependency-semantic-category.md`

Downstream context:

- `docs/architecture/public-profile-evidence-patterns.md` §9

It owns semantics only. It defines no collector, no detector, no runtime
carrier, no availability key and no numeric bound.

## 2. Canonical Position

The expanded semantic ladder is:

    DependencyDeclaration
    != DependencySemanticCategoryEvidence
    != DependencySourceReferenceEvidence
    != ProducedWorkEvidence
    != RepositoryProblemAreaEvidence
    != RepeatedProfilePattern(ProblemArea)

This level is **additive**.

- It replaces no locked level.
- It does not weaken the existing
  `DependencySemanticCategoryEvidence != ProducedWorkEvidence` boundary locked
  by the Session 067 contract.
- `ProducedWorkEvidence` remains **unresolved**: no contract yet establishes
  what observable evidence could satisfy it.
- `ProducedWorkEvidence` remains **mandatory** before
  `RepositoryProblemAreaEvidence` under the Session 067 contract, and this
  contract does not change that requirement.

`DependencySourceReferenceEvidence` therefore does not satisfy
`ProducedWorkEvidence`, and it never unblocks `RepositoryProblemAreaEvidence`
on its own.

## 3. Maximum Canonical Meaning

One `DependencySourceReferenceEvidence` item means exactly:

> At pinned commit C, source artefact P contains a syntactic source-level
> reference that can be deterministically associated with dependency D.

It must not mean:

- the repository compiles,
- the source artefact participates in a successful build,
- the dependency is used at runtime,
- the dependency is used in production,
- meaningful implementation exists,
- `ProducedWorkEvidence` has been established,
- the source was authored by the profile owner,
- the implementation is correct,
- the implementation is high quality,
- the repository is commercial or production-grade,
- the developer knows or is capable with the technology,
- any ProblemArea is proven.

Epistemic strength may never exceed the observation that supports it.

## 4. Reference Boundary

This contract does not define the detector.

The exact definition of a qualifying source-level reference is owned by a
future versioned reference-detection rule contract.

Unresolved detection concerns include, without being limited to:

- imports,
- type references,
- API references,
- symbol references,
- comments,
- string literals,
- generated code,
- test and example code,
- build-excluded code,
- dynamic or reflection-based usage.

None of them is decided here.

A loose textual occurrence must never silently become canonical source-reference
evidence.

## 5. Raw Collection Prerequisite

Bounded source-evidence collection semantics are now defined by
`docs/architecture/repository-source-evidence-collection.md`, which owns the
supported V1 source surface, the normalized `RepositorySourceFile` artefact
shape, deterministic bounds and ordering, text normalization, and the dedicated
`source-files` availability and completeness semantics.

Production source collection is **not yet implemented**, so the required raw
input does not yet exist at runtime.

Inputs that already exist elsewhere in the current snapshot surface:

- source and file path through `FileTree`,
- file size when the provider supplies it,
- the pinned `HeadCommitSha`.

Until that bounded source-evidence collection surface is implemented, no
`DependencySourceReferenceEvidence` can be produced.

## 6. Raw versus Derived

    RAW COLLECTED SOURCE EVIDENCE
    != DERIVED SOURCE-REFERENCE EVIDENCE

`RepositorySnapshot` remains the canonical owner of normalized collected
repository evidence.

`DependencySourceReferenceEvidence` is derived semantic evidence. It must not be
stored as raw snapshot truth, and its final runtime carrier is not designed
here.

Infrastructure may retrieve and normalize provider data. Reference
interpretation is semantic policy and does not become an Infrastructure concern
merely because Infrastructure retrieves file contents.

## 7. Isolation

The two isolation principles established by the Session 066 contract carry
forward to any future source-evidence collection surface:

1. no `FileTree` content overlay that silently changes existing signal inputs;
2. no `CollectionWarnings` contamination that silently changes
   `RepositoryAnalysis.Status`.

The future source evidence surface and this derived evidence are **non-scoring**.

Repository MVP V1 still owns exactly 15 scoring signals. No signal is created,
and no score or category result consumes this evidence.

This contract introduces no report element, and the current repository report
contract is unchanged. Any future presentation or report consumption of this
evidence requires a separate approved report or presentation contract, and
presentation may never raise the evidence's epistemic meaning beyond §3.

The future availability key is not designed here.

## 8. Availability and Absence Ceiling

- unavailable source evidence != no source reference,
- incomplete source evidence != no source reference,
- unsupported source coverage != no source reference,
- no detected reference within a complete supported surface means only that no
  qualifying reference was observed within that supported surface,
- reference absence must never become evidence that the dependency is unused,
- ecosystem coverage gaps must never become developer-negative evidence.

Exact future `DataAvailability` keys and collection states are outside this
contract.

## 9. Ecosystem Neutrality

Locked semantic requirements:

- an equal epistemic ceiling across supported ecosystems,
- no ecosystem gains a stronger claim from the same evidence class,
- equivalent traceability expectations,
- equivalent unavailable, incomplete and absence semantics.

Identical detectors are not required, and ecosystem-specific detector rules are
not defined here.

Language semantics may affect a future detection implementation. They must never
automatically increase claim strength, because this pipeline does not build or
execute the repository.

## 10. Known Upstream Declaration Limitations

Go `// indirect` requirements and Maven `dependencyManagement` entries remain
known declaration-quality limitations recorded by
`repository-dependency-evidence.md` §11.1.

For this contract they:

- do not block these semantics,
- are not proven implementation prerequisites for this intermediate level,
- must be explicitly considered by every future downstream qualification rule,
- are not yet proven to require upstream correction before
  `RepositoryProblemAreaEvidence`.

Whether upstream correction is required is decided together with the future
produced-work and ProblemArea qualification rule, not here.

## 11. Traceability

Every future `DependencySourceReferenceEvidence` item must ultimately be
explainable through exact observable source evidence.

Conceptual chain:

    DependencySourceReferenceEvidence
    ↓
    exact source observation / source evidence reference
    ↓
    normalized collected source artefact
    ↓
    pinned repository snapshot
    ↓
    RepositoryAnalysis / RepositoryAnalysisReference, as appropriate for the
    eventual derived result architecture

`RepositoryAnalysisReference` is a derived-result concern. It is never a field
of a raw source artefact.

    SOURCE LOCATOR != EVIDENCE PAYLOAD

A source locator must at least be capable of identifying repository identity,
pinned commit and source path.

The minimum persisted evidence payload is **unresolved**. This contract does not
decide a content hash, a bounded snippet, full bounded content, a structured
observation payload, or whether self-contained replay is required.

Remote refetch is not equivalent to self-contained replay: a repository may be
deleted, made private, force-pushed or rate-limited.

Prose sources such as README text, topics or the repository description can
never appear in this chain as supporting evidence.

## 12. Version Ownership

- `AnalyzerVersion` owns future collection and snapshot normalization
  semantics.
- Reference-detection semantics must not be hidden inside `AnalyzerVersion`
  merely because source contents would be collected by the analyzer. Collection
  can change without detection changing, and detection can change without
  collection changing.
- A separate semantic version owner will be required for the future detection
  rule once detection semantics become executable and versioned. Its permanent
  name is deliberately not chosen here.
- `DependencyCategoryMappingVersion` remains unrelated to reference detection.
- `RuleSetVersion` remains scoring-only.
- `PatternRuleVersion` remains downstream profile-pattern ownership.

## 13. Determinism and Future Bounds

Future source collection and future source-reference detection must be
deterministic, bounded, versioned, locale-independent, clock-independent and
LLM-independent.

A future collection contract must define at least these limit dimensions:

- eligible source coverage,
- maximum source-file count,
- maximum bytes per source file,
- maximum total source corpus bytes,
- deterministic ordering,
- oversized-file semantics,
- truncation semantics,
- partial collection semantics,
- API failure semantics.

Source eligibility must be deterministic, and binary, unsupported or
unprocessable content must never silently become negative evidence. The
eligibility mechanism is versioned as collection semantics.

This contract does not lock numeric values, extension sets, the binary detection
mechanism, archive versus per-file retrieval, or the candidate-selection
algorithm. Those raw collection decisions are owned by
`docs/architecture/repository-source-evidence-collection.md`.

`StringComparer.Ordinal` is the existing repository convention for canonical
normalized output ordering and may be used as such. This document does not lock
the future candidate-selection strategy.

## 14. Prohibited Semantics

This evidence level must never produce or support:

- a repository score contribution,
- signal creation,
- weighting,
- confidence,
- ranking,
- runtime-use inference,
- production-use inference,
- `ProducedWorkEvidence` inference,
- ProblemArea inference,
- developer skill or capability inference,
- seniority,
- employability,
- hire or no-hire,
- authorship,
- originality,
- locale-dependent semantics,
- LLM-derived semantic evidence.

## 15. Explicitly Unresolved

The following remain undecided and require separate approval:

- the minimum persisted evidence payload and whether self-contained replay is
  required;
- the qualifying-reference definition and per-ecosystem detection design;
- the permanent name of the detection version owner;
- the future `AnalyzerVersion` bump value;
- whether Go `// indirect` and Maven `dependencyManagement` must be corrected
  before `RepositoryProblemAreaEvidence`;
- the resolution of `ProducedWorkEvidence` itself.

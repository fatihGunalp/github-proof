# Repository Dependency Evidence Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Repository-level structured evidence surface
**Session:** 066
**Date:** 2026-08-17

## 1. Purpose

This contract defines a non-scoring, deterministic, versioned and traceable
repository evidence surface that records dependency declarations observed in
repository manifests.

Its reason to exist is the ProblemArea evidence foundation deferred by
`docs/architecture/public-profile-evidence-patterns.md` §9.4.

It defines evidence collection only. It defines no semantic categorization, no
problem-area taxonomy and no profile-level promotion rule.

## 2. Ownership Boundary

Dependency evidence
!=
repository scoring signal

Repository MVP V1 owns exactly the 15 scoring signals locked by
`docs/product/analysis-categories-and-signal-boundaries.md` §6.

This surface adds no signal, participates in no score, and appears in no
category result.

Reasons this evidence cannot be a scoring signal:

- signal acceptance criterion §9.4 of that contract requires an actionable
  repository-improvement recommendation, which dependency presence cannot
  honestly produce;
- `docs/product/scoring-and-report-contract.md` §6 normalizes the total against
  the applicable maximum of scored signals, so a new scoring signal would move
  every existing repository score.

## 3. Canonical Position

The surface is normalized collection data carried by `RepositorySnapshot`,
exactly like `Languages`, `Topics` and `License`.

    RepositorySnapshot
    └── DependencyDeclarations

`RepositoryAnalysis` already references its snapshot, so no analysis result
member and no report element changes.

## 4. Isolation Rules

Two rules keep every existing analysis byte-identical.

### 4.1 No file-tree overlay

Collected manifests must not be merged into `RepositorySnapshot.FileTree`.

Signal evaluators read that tree, so an overlay would silently change existing
signal inputs, in particular `validation.test-structure`.

### 4.2 No snapshot collection warnings

This surface must not add entries to `RepositorySnapshot.CollectionWarnings`.

Non-empty collection warnings turn `RepositoryAnalysis.Status` into `Partial`
and emit `limitation.snapshot.collection-partial`, which would change existing
analysis results and propagate into profile contribution decisions.

The surface therefore carries its own availability and completeness state.

## 5. Covered Ecosystems and Manifest Set

The canonical V1 manifest set is exactly:

| Ecosystem | Manifest file names |
|---|---|
| DotNet | `*.csproj`, `*.fsproj`, `*.vbproj`, `Directory.Packages.props` |
| Java | `pom.xml`, `build.gradle`, `build.gradle.kts` |
| JavaScript | `package.json` |
| Python | `pyproject.toml`, `requirements.txt`, `requirements-*.txt` |
| Go | `go.mod` |
| Rust | `Cargo.toml` |
| Php | `composer.json` |
| Ruby | `Gemfile`, `*.gemspec` |

File-name matching is case-insensitive on the last path segment. Any path depth
is eligible.

Adding, removing or renaming a manifest pattern requires a new contract version
and an `AnalyzerVersion` change under §9.

## 6. Deterministic Collection Bounds

    Candidate ordering:              StringComparer.Ordinal ascending by path
    Maximum manifest files:          32
    Maximum decoded bytes per file:  131072 (128 KiB)
    Maximum decoded corpus bytes:    524288 (512 KiB)

Bounds are identical for every ecosystem. They must never vary by ecosystem,
repository size or execution order.

A candidate whose declared size is unknown or exceeds the per-file limit is not
fetched. A candidate that exceeds the remaining corpus budget is skipped, and
later smaller candidates remain eligible. Decoded byte counts are re-checked
after decoding.

Reaching any bound produces incomplete evidence under §7, never unavailable
evidence and never absence.

## 7. Availability and Completeness Semantics

The surface uses one dedicated availability key:

    dependency-manifests

No new `DataAvailability` status is introduced.

### 7.1 Complete and empty

Supported manifest surface scanned completely with no declaration found:

    Available + IsComplete = true + empty declaration collection

Canonical meaning:

> No declaration was observed within the supported manifest surface.

This does not mean:

- no dependencies exist,
- the ecosystem is absent,
- the developer does not use the technology.

### 7.2 Incomplete

File-tree truncation, a collection bound, or missing candidate content:

    Available + IsComplete = false

Partial results are preserved. Incomplete evidence must never be read as a
complete-and-empty result.

### 7.3 Unavailable

API or content retrieval failure keeps the real failure status
(`RateLimited`, `Forbidden`, `Failed`, `NotRequested`).

Unavailable is never absence.

### 7.4 Ecosystems outside the covered set

An ecosystem outside §5 is a product coverage limitation of this contract
version.

It is not a per-repository availability state, not `NotApplicable`, not negative
evidence and not developer absence. It must not be represented as a
repository-level fact at all.

## 8. Ecosystem Neutrality

"Ecosystem-neutral" in this contract means exactly:

1. every supported ecosystem maps to one canonical normalized shape;
2. no ecosystem gains a dedicated Domain field;
3. every supported ecosystem obeys the same availability and completeness rules;
4. deterministic collection bounds never vary by ecosystem;
5. an unsupported ecosystem produces no absence and no negative evidence;
6. future semantic consumers must not raise the epistemic strength of a
   declaration because of its ecosystem.

It does not mean that every programming ecosystem is supported. The supported
coverage set is explicit and versioned by §5.

## 9. Versioning

`RuleSetVersion` is unchanged: no scoring or evaluator semantics change.

`ReportSchemaVersion` is unchanged: the repository report contract is unchanged.

`AnalyzerVersion` owns collection and snapshot normalization semantics.

Canonical bump rule, recorded here because no earlier contract stated one, and
referenced from `docs/product/scoring-and-report-contract.md` §16 so it applies
to every collection or normalization change, not only to dependency evidence:

- an additive, backward-compatible collection or normalization change requires a
  minor `AnalyzerVersion` increase;
- a change that alters previously produced snapshot values requires a major
  increase;
- a fix that does not change produced values requires a patch increase.

Introducing this surface is an additive normalization change and therefore
requires a minor `AnalyzerVersion` increase.

No unversioned semantic collection rule is permitted.

## 10. Normalized Evidence Shape

One declaration carries exactly:

- ecosystem identity,
- manifest path,
- declared package identity,
- declared version or range literal when present.

No ecosystem-specific member exists. Version literals are preserved exactly as
declared, including ranges and unresolved placeholders. An absent version
literal stays absent and is never defaulted.

Deterministic ordering: manifest path, then package identity, then declared
version literal, all `Ordinal` ascending. Identical triples collapse to one
declaration.

## 11. Parsing Scope

Parsing is format normalization only. It reads exactly:

| Manifest | Read scope |
|---|---|
| `*.csproj`, `*.fsproj`, `*.vbproj`, `Directory.Packages.props` | `PackageReference` and `PackageVersion` elements: `Include`/`Update` identity, `Version` attribute or child element |
| `pom.xml` | `dependency` elements: `groupId:artifactId` identity, `version` element |
| `build.gradle`, `build.gradle.kts` | quoted `group:artifact[:version]` coordinates, scanned per line after stripping `#` and `//` line comments; block comments are not stripped |
| `package.json` | `dependencies` and `devDependencies` members |
| `composer.json` | `require` and `require-dev` members |
| `pyproject.toml` | `[project] dependencies` array and `[tool.poetry.dependencies]` table |
| `requirements.txt`, `requirements-*.txt` | requirement lines, excluding comments and option lines |
| `go.mod` | `require` lines and `require (...)` blocks |
| `Cargo.toml` | `[dependencies]`, `[dev-dependencies]`, `[build-dependencies]` tables |
| `Gemfile` | `gem` declarations |
| `*.gemspec` | `add_dependency`, `add_runtime_dependency`, `add_development_dependency` calls |

Unparseable or unrecognized content yields no declaration for that manifest and
never fails the analysis.

A manifest whose content was already collected by the contents collector is
reused rather than requested again. Reused content obeys the same byte bounds.

Parsing must not classify, group, weight or interpret a dependency.

## 12. Declared-Only Semantic Ceiling

The maximum canonical meaning of one declaration is:

> Manifest M declares package D for ecosystem E, with version literal V when
> present, at snapshot time.

It explicitly does not mean:

- the dependency is used at runtime,
- meaningful work was produced with that technology,
- the developer knows that technology,
- the repository proves any problem area.

Scaffolds, templates and tutorial repositories declare identical dependencies.

The declared-versus-produced boundary required by
`public-profile-evidence-patterns.md` §9.4 condition 4 remains unresolved and is
not addressed by this contract.

## 13. Traceability

Future profile consumers must traverse:

    profile evidence
    ↓
    ProfileEvidenceReference (EvidenceKey = dependency-manifests)
    ↓
    RepositoryAnalysisReference
    ↓
    RepositoryAnalysis
    ↓
    RepositorySnapshot.DependencyDeclarations

The profile layer must not parse manifests. It reads normalized declarations
only, exactly as it reads `Languages`.

No parallel or competing evidence truth may be created.

## 14. Prohibited Semantics

This surface must never produce or support:

- a repository score contribution,
- a developer capability, seniority, employability or hire inference,
- authorship or originality inference,
- problem-area truth,
- weighting or confidence values,
- locale-dependent results,
- LLM-derived evidence.

Locale changes may alter presentation only. This surface currently has no
presentation.

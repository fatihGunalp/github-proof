# Repository Dependency Semantic Category Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Repository-level semantic interpretation of dependency declarations
**Session:** 067
**Locked:** 2026-08-17 / Session 067
**Mapping registry:** `docs/architecture/registries/dependency-category-mapping/1.0.0.json`

## 1. Purpose

This contract defines what it means, and what it may never mean, to map a
normalized dependency declaration to a semantic category.

Input contract:
`docs/architecture/repository-dependency-evidence.md`

Downstream context:
`docs/architecture/public-profile-evidence-patterns.md` §9

It defines semantic interpretation only. It defines no produced-work evidence,
no ProblemArea evidence, no ProblemArea taxonomy and no profile promotion rule.

## 2. Canonical Evidence Ladder

The following are distinct levels. No rule may collapse them.

    DependencyDeclaration
    != DependencySemanticCategoryEvidence
    != ProducedWorkEvidence
    != RepositoryProblemAreaEvidence
    != RepeatedProfilePattern(ProblemArea)

This contract owns exactly the second level.

`DependencySemanticCategoryEvidence` must never be named, represented,
serialized, presented or consumed as ProblemArea evidence.

Produced-work evidence is mandatory for repository-level ProblemArea evidence.
Neither produced-work evidence nor its rule exists yet, so ProblemArea remains
deferred under `public-profile-evidence-patterns.md` §9.

## 3. Maximum Canonical Meaning

One semantic category evidence item means exactly:

> A dependency declaration mapped to category C was observed in the supported
> manifest surface of this repository, under mapping version V.

It never means:

- the dependency is used at runtime,
- the dependency is used in production,
- work was produced with it,
- the repository proves problem area C,
- the developer knows, prefers or is capable in C.

Epistemic strength may not exceed the declaration that supports it.

## 4. Declared Scope Limitation

Session 066 normalizes ecosystem, manifest path, package identity and declared
version literal. It does not record which manifest section produced a
declaration, so runtime, development, build and test dependencies are
indistinguishable.

This limitation is accepted. Session 066 is not reopened by this contract.

Consequence, which every consumer must honour:

> A category observation says only that a mapped declaration exists somewhere in
> the supported manifest surface.

Dependency scope may be added later only as an additive collection-contract
change, and only if produced-work correlation actually requires it.

### 4.1 Known input limitations

The declaration surface carries the limitations recorded in
`repository-dependency-evidence.md` §11.1: Composer platform requirements, Go
`// indirect` entries, Maven `dependencyManagement` entries, Gradle version
catalogs, MSBuild property-reference identities and the missing dependency
scope.

A category observation inherits every one of them. None may be read as runtime
usage, production usage or produced work.

## 5. Ownership

- **Infrastructure**: manifest format parsing and normalization only, unchanged
  from `repository-dependency-evidence.md` §11.
- **Domain**: the deterministic dependency-to-category policy and this
  contract's semantics.
- **Application and Profile**: orchestration and consumption only. The profile
  layer never parses package names, never re-interprets them and never keeps a
  competing package taxonomy.

## 6. RepositorySemanticEvidence

Semantic category evidence is carried by `RepositorySemanticEvidence`, a
non-scoring result composed deterministically from one `RepositoryAnalysis`.

It is not written into `RepositorySnapshot`, because the snapshot carries
collected data rather than derived semantics.

It is not added to `RepositoryAnalysis`, so the locked analysis result shape and
the repository report contract are untouched.

### 6.1 One observation per category

A repository's occurrence of a semantic category is **one observation**, carrying
one or more traceable supporting `DependencyDeclaration` references.

Multiple packages mapping to the same category inside one repository must never
increase evidence strength. Declaration counts are traceability, not weight, and
may not be compared, ranked or summed into a stronger claim.

`DependencySemanticCategoryEvidence` is never
`RepositoryProblemAreaEvidence`. Produced-work evidence remains mandatory before
any repository-level ProblemArea evidence may exist, and no produced-work
surface exists yet.

### 6.2 Required invariants

A `RepositorySemanticEvidence` result must carry:

1. a `RepositoryAnalysisReference` identifying the exact analysis it was
   composed from,
2. the `DependencyCategoryMappingVersion` it consumed,
3. the source `dependency-manifests` availability and completeness state,
4. for every category evidence item, references to the source
   `DependencyDeclaration` values that produced it,
5. deterministic ordering, by category then by source declaration ordering,
6. no score, no weighting, no confidence and no ranking.

### 6.3 Determinism

The same analysis plus the same mapping version must produce the same semantic
result. Input enumeration order and execution order must not change it. No
clock, no locale and no LLM participates.

### 6.4 Traceability

Required traversal:

    RepositorySemanticEvidence
    ↓
    category evidence item
    ↓
    source DependencyDeclaration(s)
    ↓
    RepositoryAnalysisReference
    ↓
    RepositoryAnalysis
    ↓
    RepositorySnapshot.DependencyDeclarations

A category evidence item without source declaration references is invalid.

### 6.5 Availability and completeness

Availability semantics are inherited from `repository-dependency-evidence.md`
§7 and must be carried forward, never flattened:

- `dependency-manifests` unavailable: the semantic result is undetermined for
  this repository and must not be read as "no categories";
- declarations incomplete: category observations are incomplete, never
  complete-and-empty;
- declarations complete and empty: no declaration was observed in the supported
  manifest surface, which is not absence of dependencies;
- a declaration that maps to no category is unmapped, which is not absence and
  not negative evidence.

The implementation shape stays unlocked. This contract locks semantics only.

## 7. Mapping Invariants

1. Mapping entries are `(ecosystem, package identity) → category`. Bare package
   names are never matched across ecosystems.
2. Aliases, renames and successor packages are explicit registry entries. No
   prefix, namespace, wildcard or regular-expression matching.
3. A package maps to at most one category in this version. A package whose
   purpose spans categories is left unmapped rather than guessed.
4. An unmapped or unknown package produces no evidence. Unknown is never
   absence and never negative evidence.
5. Category identity never depends on ecosystem, and no ecosystem grants higher
   epistemic strength than another.
6. No category may be inferred from README text, topics, repository
   description, language statistics, file paths or any keyword match.
7. No weighting, no confidence value, no locale dependence, no LLM.

### 7.1 Ambiguity Rule

When a package's primary purpose is not deterministically decidable from its
own documented purpose, it stays unmapped.

Unmapped is the safe default of this contract, because a missing category is
explicitly meaningless while a wrong category is a false claim.

### 7.2 Known Coverage Asymmetry

Some ecosystems express capabilities outside package declarations. For example,
a .NET web application obtains its hosting stack through the project SDK or a
framework reference, and Session 066 parses neither, so no
`web-application-framework` declaration may exist for it.

This asymmetry is acceptable only because category absence carries no meaning.
It must never be presented as evidence that a repository does not serve HTTP.

## 8. V1 Category Taxonomy

The canonical V1 category keys are exactly:

    database-access
    web-application-framework
    http-client
    authentication
    machine-learning

`http-server` is DEFERRED. No other key exists in this version, and adding one
requires a new contract version.

Each entry defines meaning, not repository behaviour.

The illustrative examples below carry **no mapping authority**. Only the
registry in §9 may assert that a specific package maps to a category.

### 8.1 `database-access`

- **Maximum meaning:** the declared package's primary purpose is application
  access to a database or database abstraction. It does not mean persistence
  work was produced, that a database is used at runtime, or that a persistence
  ProblemArea is proven.
- **Qualifying package properties:** the package's own primary purpose is
  database connectivity, object-relational mapping, query building, or schema
  migration for a persistent store.
- **False-positive boundary:** does not qualify when data access is incidental
  to a broader framework, when the package is generic serialization, caching or
  file I/O, or when the package is a database server distribution rather than a
  client.
- **Explicit non-meaning:** not persistence design, not schema modelling, not
  database operation, not a persistence ProblemArea.
- **Ambiguous, unmapped in V1:** in-memory or cache clients, embedded key-value
  stores, vector stores, meta-frameworks bundling an ORM.
- **Alias behaviour:** renamed or successor driver packages are separate
  explicit registry entries pointing at the same category.
- **Illustrative SHOULD map:** a dedicated ORM package; a first-party database
  driver.
- **Illustrative MUST NOT map:** a JSON serializer; an HTTP client used to call
  a data API; a test fixture library.

### 8.2 `web-application-framework`

- **Maximum meaning:** the declared package's primary purpose is providing a
  general web application framework.
- **Qualifying package properties:** the package's own primary purpose is an
  integrated framework for building web applications, covering routing and
  request handling as a framework rather than as a library.
- **False-positive boundary:** HTTP clients, template engines, standalone
  routers, testing utilities and asset build tools do not qualify.
- **Explicit non-meaning:** it does not imply authentication, database access,
  API production, backend experience, deployment or operation. A framework's
  embedded feature set never produces a second category.
- **Ambiguous, unmapped in V1:** micro-frameworks and bare HTTP server libraries
  that are not general application frameworks, and meta-package starters that
  are narrower or broader than the category.
- **Alias behaviour:** explicit registry entries only, including packages split
  or renamed across major versions.
- **Illustrative SHOULD map:** a broad web application framework package.
- **Illustrative MUST NOT map:** an HTTP client; a template engine; a bundler.

`http-server` is deferred in this version, so minimal HTTP server libraries stay
uncovered rather than being absorbed here.

### 8.3 `http-client`

- **Maximum meaning:** a declaration was observed whose package purpose is
  issuing outbound HTTP requests.
- **Qualifying package properties:** the package's own primary purpose is
  generic HTTP calling.
- **False-positive boundary:** vendor-specific SDKs are not mapped in V1,
  because "calls vendor X" is an integration claim this contract does not
  support.
- **Explicit non-meaning:** not API production, not integration work, not
  distributed-system design.
- **Ambiguous, unmapped in V1:** GraphQL clients, gRPC libraries, vendor SDKs.
- **Alias behaviour:** explicit entries only.
- **Illustrative SHOULD map:** a general-purpose HTTP client library.
- **Illustrative MUST NOT map:** a web server framework; a vendor cloud SDK.

### 8.4 `authentication`

- **Maximum meaning:** a declaration was observed whose package purpose is
  authentication or authorization.
- **Qualifying package properties:** the package's own primary purpose is
  identity verification, session or token issuance and validation, or access
  control.
- **False-positive boundary:** general cryptography, hashing and TLS packages do
  not qualify.
- **Explicit non-meaning:** not that authentication is implemented, correct,
  complete or secure.
- **Ambiguous, unmapped in V1:** standalone token encode/decode libraries and
  password-hashing utilities, which serve many purposes.
- **Alias behaviour:** explicit entries only.
- **Illustrative SHOULD map:** an authentication or authorization middleware
  library.
- **Illustrative MUST NOT map:** a bcrypt or hashing utility; a TLS library.

### 8.5 `machine-learning`

- **Maximum meaning:** a declaration was observed whose package purpose is
  machine-learning modelling or training.
- **Qualifying package properties:** the package's own primary purpose is
  building, training or serving machine-learning models.
- **False-positive boundary:** generic numeric, dataframe, plotting and notebook
  packages do not qualify, regardless of how often they appear in ML work.
- **Explicit non-meaning:** not that ML work was performed, not data-science
  capability, not an ML ProblemArea.
- **Ambiguous, unmapped in V1:** general NLP text utilities, vector database
  clients, experiment-tracking tools.
- **Alias behaviour:** explicit entries only.
- **Illustrative SHOULD map:** a deep-learning or classical machine-learning
  framework.
- **Illustrative MUST NOT map:** `numpy`, `pandas`, `scipy`, plotting libraries,
  notebook tooling.

### 8.6 Category exclusions

Containerization is not a category in this contract, because it is determined by
file paths rather than declarations, which is outside the Session 066 surface.

Role-shaped labels such as backend, frontend or full stack are permanently
excluded.

## 9. Mapping Registry

The registry is a separate canonical artefact. It is not embedded in this
contract.

Division of ownership:

- this contract defines how the registry is interpreted;
- the registry is the single canonical list of exact mappings.

They are never competing truth stores.

### 9.1 Registry requirements

1. Every entry is `(ecosystem, lookup identity) → category key`, plus explicit
   alias entries that name their target.
2. Entries are unique per `(ecosystem, lookup identity)`. A duplicate is invalid.
3. No wildcards, prefixes, regular expressions or heuristics.
4. Deterministic ordering by ecosystem then lookup identity, ordinal, so diffs
   stay reviewable.
5. Every entry records the qualifying package property from §8 that justifies
   it, as non-semantic review metadata.
6. The registry carries its own `DependencyCategoryMappingVersion`.
7. The canonical format is JSON, machine-consumable from one artefact, with no
   mirror and no second hard-coded list.
8. No version or version-range field participates in category matching.

### 9.2 Lookup identity normalization

The source `DependencyDeclaration.PackageId` is never rewritten. Normalization
produces a lookup identity used only for registry matching, and source
traceability is always preserved.

| Ecosystem | Lookup identity |
|---|---|
| DotNet | case-insensitive canonical lookup, invariant lowercase |
| Java | preserve exact case, `groupId:artifactId` |
| JavaScript | npm canonical lowercase identity, scope preserved |
| Python | PEP 503 style: lowercase, runs of `-`, `_`, `.` collapsed to one `-` |
| Go | preserve the exact module path, never strip a `/vN` element |
| Rust | preserve the actual package identity exactly; never lowercase and never map `_` to `-` |
| Php | Composer canonical lowercase `vendor/package` |
| Ruby | preserve exact identity |

Rust and Go identities are preserved because Cargo identities may be
case-sensitive, Session 066 may observe dependencies from registries other than
crates.io, and a Go major-version element is part of the module path. Any
equivalence between two identities is expressed as an explicit approved registry
alias, never through lossy normalization.

### 9.3 Immutable versions and change rules

Once a registry version has been used by a versioned semantic result:

1. its semantic mappings are never mutated in place;
2. a change creates a new version artefact;
3. historical versions remain available for deterministic replay;
4. historical versions are inactive rule versions, not competing truth stores;
5. exactly one version is active for a given execution, selected explicitly.

Version semantics:

- **minor**: an additive new mapping, or an additive approved alias;
- **major**: a mapping category change, a mapping removal, or an
  identity-normalization semantic change;
- **patch**: a non-semantic rationale correction only.

Rationale is review metadata. It must never participate in a runtime mapping
result.

A result produced under one mapping version must never be silently compared
with, or merged into, a result produced under another.

## 10. Version Ownership

`DependencyCategoryMappingVersion` governs exactly:

    package identity and aliases → semantic dependency category

It does not govern:

- produced-work detection,
- repository ProblemArea qualification,
- profile ProblemArea promotion.

`AnalyzerVersion` keeps collection and normalization ownership.
`RuleSetVersion` keeps scoring and evaluator ownership.
`ObservationRuleVersion` keeps P5 observation ownership.
`PatternRuleVersion` keeps P6 taxonomy and promotion ownership.

A future
`ProducedWorkEvidence + DependencySemanticCategoryEvidence → RepositoryProblemAreaEvidence`
rule governs qualification rather than package mapping, so it will require its
own version owner. That owner is deliberately not named or defined here.

## 11. Prohibited Semantics

This surface must never produce or support:

- a repository score contribution or any change to the 15 scoring signals,
- ProblemArea truth of any kind,
- produced-work, runtime-usage or production-usage claims,
- developer capability, role, seniority, employability, hire or ranking
  inference,
- authorship or originality inference,
- weighting, confidence or ranking values,
- locale-dependent canonical truth,
- LLM-created or LLM-adjusted mappings.

Unsupported, unavailable, incomplete and unmapped states are never absence and
never negative evidence.

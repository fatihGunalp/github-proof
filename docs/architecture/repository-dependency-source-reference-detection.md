# Repository Dependency Source Reference Detection Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Repository-level deterministic dependency source-reference detection
**Session:** 072
**Date:** 2026-08-18

**Amendments**

- Session 073, 2026-08-18 — §10 corrected: Go `// indirect` removed from the
  classes the detector must identify, with the reason recorded in §10.1. The
  amendment stays inside Contract Version 1 because no detector and no numeric
  `SourceReferenceRuleVersion` had ever existed, so no observation was ever
  produced under the removed sentence and none can be altered or re-attributed
  by removing it. No other clause was reopened.

## 1. Purpose

This contract defines the qualifying source-level reference: what may be
detected, how identity is resolved, and what a detected reference may never
mean.

Input contracts:

- `docs/architecture/repository-dependency-evidence.md`
- `docs/architecture/repository-dependency-semantic-category.md`
- `docs/architecture/repository-source-evidence-collection.md`
- `docs/architecture/repository-dependency-source-reference-evidence.md`

Downstream context:

- `docs/architecture/public-profile-evidence-patterns.md` §9

It owns detection semantics only: the supported V1 ecosystem surface, the
qualifying syntactic forms, the comment and string exclusion rule, the identity
resolution policy, ambiguity behavior, the observation payload, ordering,
traceability, version ownership and the security boundary.

It owns no collection rule, no `ProducedWorkEvidence`, no
`RepositoryProblemAreaEvidence`, no ProblemArea taxonomy or promotion, no
profile pattern, no scoring semantics and no report presentation.

`repository-dependency-source-reference-evidence.md` remains the owner of the
epistemic ceiling. This contract may narrow that ceiling and may never widen it.

## 2. Canonical Position

    DependencyDeclaration
    != DependencySemanticCategoryEvidence
    != RepositorySourceFile
    != DependencySourceReferenceEvidence
    != ProducedWorkEvidence
    != RepositoryProblemAreaEvidence
    != RepeatedProfilePattern(ProblemArea)
    != DeveloperExperienceOrCapability

This contract completes exactly one step: `RepositorySourceFile[]` plus
`DependencyDeclaration[]` to `DependencySourceReferenceEvidence`.

`ProducedWorkEvidence` remains unresolved and `RepositoryProblemAreaEvidence`
remains blocked by the Session 067 contract.

## 3. Maximum Canonical Meaning

One `DependencySourceReferenceEvidence` item means exactly:

> At pinned commit C, collected source artefact P contains a qualifying
> import-declaration form, outside comments and string literals, whose resolved
> reference identity is accepted by exactly one `DependencyDeclaration` D under
> the identity rules of this contract version.

It must not mean:

- the dependency is used at runtime,
- the dependency is used in production,
- the referenced code is executed,
- the repository compiles,
- the source participates in a successful build,
- meaningful implementation exists,
- the source was authored by the profile owner,
- the implementation is original, correct or high quality,
- the developer knows or is capable with the technology,
- `ProducedWorkEvidence` has been established,
- any ProblemArea is proven,
- any seniority, employability or ranking conclusion.

The inherited ceiling of
`repository-dependency-source-reference-evidence.md` §3 applies in full.

### 3.1 Import Declaration, Not Usage

V1 detects **import declarations only**. It does not detect usage.

An import declaration is not evidence that the imported identity is invoked,
instantiated, executed or reached. Type references, symbol references, API
calls, annotations, attributes, decorators, dependency-injection wiring,
container configuration and reflection are all outside V1 and must never be
detected, because resolving them requires a compiler or a symbol table that this
pipeline does not have and must not acquire by heuristic.

## 4. Supported V1 Ecosystem Surface

V1 supports exactly five ecosystems:

    Go
    JavaScript          (the JavaScript/TypeScript source family)
    Rust
    Python
    DotNet

Outside V1:

    Java
    Php
    Ruby

Exclusion rationale, recorded as motivation and never as runtime semantics:

- **Java** — Maven and Gradle coordinates are structurally independent of the
  package namespace, and aggregator artifacts ship no class at all. Session 072
  measured that 84.2% of the source identities that any coordinate-derived rule
  accepted were accepted by more than one declaration. No coordinate-only rule
  can separate them, and a single accepted match is not proof of correctness
  either, because the true owner may be an undeclared transitive artifact.
- **Php** — a Composer `vendor/package` identity is structurally independent of
  the PSR-4 root namespace, and one root namespace is routinely shared by many
  packages from the same vendor. Session 072 measured 46.2% ambiguity.
- **Ruby** — Bundler auto-require means a gem can be used with no `require`
  anywhere, so both presence and absence carry the least information of any
  supported ecosystem; Session 072 also measured Ruby's naive false-positive
  floor at 28.24%, an order of magnitude above every other ecosystem, and the
  largest share of lexical constructs a bounded scanner does not model. The
  Ruby measurement base was additionally the thinnest of the eight.

An ecosystem outside V1 produces **no observation of any polarity**. It is never
negative evidence, never a coverage defect and never a developer deficiency.

Adding an ecosystem requires a new contract version under §17.

### 4.1 Source Artefact Ecosystem

A collected `RepositorySourceFile` is assigned an ecosystem by a total,
deterministic, case-insensitive map over the
`repository-source-evidence-collection.md` §6 allow-list:

| Ecosystem | Extensions |
|---|---|
| DotNet | `.cs`, `.fs`, `.vb`, `.razor`, `.cshtml` |
| Java | `.java`, `.kt`, `.kts`, `.scala`, `.groovy` |
| JavaScript | `.js`, `.jsx`, `.mjs`, `.cjs`, `.ts`, `.tsx`, `.d.ts`, `.vue`, `.svelte` |
| Python | `.py` |
| Go | `.go` |
| Rust | `.rs` |
| Php | `.php` |
| Ruby | `.rb` |

Extension normalization is the §6 rule of the collection contract, reused
unchanged.

Detection runs only on artefacts whose ecosystem is inside the §4 V1 surface.
An artefact is only ever compared against declarations of its **own** ecosystem.
Cross-ecosystem matching is prohibited.

This map is detection semantics. It must be revised, under §17, whenever the
collection allow-list changes.

## 5. Qualifying Syntactic Forms

Exactly these forms qualify. Nothing else does.

| Ecosystem | Qualifying form | Reference identity |
|---|---|---|
| Go | `import "path"`, aliased `import alias "path"`, and entries of an `import ( ... )` block | the quoted import path |
| JavaScript | `import ... from "spec"`, `import "spec"`, `export ... from "spec"` | the specifier |
| JavaScript | `require("spec")` and `import("spec")` under §7 | the specifier |
| Python | `import a.b`, `from a.b import x` | the top-level module segment |
| Rust | `use crate_ident::...`, `use crate_ident;`, `extern crate crate_ident` | the first path segment |
| DotNet | `using Ns;`, `global using Ns;`, `using static Ns;`, `using Alias = Ns;` | the namespace, and for an alias the right-hand side |

Prohibited as qualifying forms in V1: bare textual occurrence anywhere, type and
symbol references, API calls, annotations and attributes, decorators, string
arguments to any function other than the two named in §7, build and
configuration files, and prose sources of any kind.

A relative specifier — one beginning with `.` or `/` — is a local module
reference, is never a package reference, and produces no observation.

## 6. Comment and String Exclusion

A match qualifies only when its **keyword token** lies in source code, outside
every comment and every string literal.

The detector must therefore model, per supported ecosystem, at minimum:

| Ecosystem | Required lexical handling |
|---|---|
| Go | `//` line comments, `/* */` block comments, quoted strings, backtick raw strings |
| JavaScript | `//`, `/* */`, single and double quoted strings, template literals, **regular-expression literals** |
| Rust | `//`, `///` doc comments, **nested** `/* */` block comments, quoted strings, **`r#"..."#` raw strings** |
| Python | `#` line comments, quoted strings, **triple-quoted strings and docstrings** |
| DotNet | `//`, `///`, `/* */`, quoted strings, `@"..."` verbatim strings, `"""` raw strings |

Line comments alone are insufficient. The existing Gradle manifest parser of
`repository-dependency-evidence.md` §11 strips only `#` and `//` line comments
and explicitly does not strip block comments; it is a manifest-format
convenience and is **not** a precedent for this contract.

Rationale, recorded as motivation only: Session 072 measured that a line-level
detector without this exclusion produces a false-positive rate of **1.54% across
the V1 surface**, over 4 358 import-shaped matches — 2.63% in Python, 1.83% in
JavaScript, 1.20% in Rust, 0.14% in Go and 0.00% in DotNet. Measured across all
eight ecosystems the figure is 1.30% over 6 974 matches, with Ruby, which is
outside V1, at 28.24%. Doc comments and docstrings carrying example imports are
the dominant cause.

### 6.1 Lexical Desynchronization

An unmodelled string or comment form can desynchronize a scanner, after which
every later position in that artefact carries an unreliable classification.

Required behavior: when the scanner reaches the end of an artefact while still
inside an unterminated string or block comment, that artefact is **unreliably
classified**. It produces no observation, and it sets detection incomplete under
§12. It never produces a partial or best-effort observation.

## 7. Literal-Only Call Forms

`require(...)` and `import(...)` qualify only when the parenthesised argument
consists of **exactly one string literal and nothing else**.

A concatenated, computed, templated, variable or otherwise non-literal argument
produces no observation. A literal followed by any further expression inside the
same parentheses is not a single literal and produces no observation.

Rationale: Session 072 measured that a permissive literal rule that merely
required the argument to *begin* with a quote produced a false positive on a
real concatenated dynamic import, while the strict single-literal rule removed
that false positive and lost nothing — 311 permissive matches against 310 strict
matches across the measured JavaScript corpus.

## 8. Identity Resolution

The `DependencyDeclaration.PackageId` is never rewritten. Resolution produces an
accepted reference identity used only for matching, and declaration
traceability is always preserved.

| Ecosystem | Rule |
|---|---|
| Go | the declaration's module path matches the import path when they are equal or the import path continues after a `/` segment boundary; **the longest matching module path wins** |
| JavaScript | the specifier head — `pkg`, or `@scope/pkg` for a scoped package — must equal the declared npm name exactly; any subpath after the head is discarded |
| Rust | the declared crate name with every `-` replaced by `_` must equal the first `use` or `extern crate` path segment |
| Python | the top-level module segment, lowercased, must equal the declared distribution name under PEP 503 normalization, or that name with `-` and `.` replaced by `_` |
| DotNet | **exact, case-insensitive equality** between the declared package id and the `using` namespace, or the namespace being the package id followed by `.` and a further segment |

### 8.1 Go Longest-Prefix Rule

A repository may contain nested Go modules, so one import path can be a valid
prefix match for more than one declared module path. The declaration with the
**longest** matching module path wins. This is deterministic and follows Go
module semantics, in which the more specific module owns the package.

Rationale: Session 072 observed nested modules in a real repository — five
`go.mod` files in one tree — producing exactly this collision. Under the
longest-prefix rule the measured ambiguity fell to zero.

### 8.2 DotNet Exact Equality

Namespace prefix matching in both directions is prohibited. Session 072
measured that a two-way prefix rule made `Autofac.Extensions.DependencyInjection`
acceptable to both `Autofac` and `Autofac.Extensions.DependencyInjection`, while
exact equality plus own-subnamespace matching produced zero ambiguity and lost
no unique match.

### 8.3 No Registry in V1

V1 introduces **no source-reference identity registry**. Every supported
ecosystem resolves by rule.

Should a future version extend the surface to an ecosystem that cannot resolve
by rule, that version must introduce a **separate** versioned identity registry
artefact. It must never be folded into `dependency-category-mapping`, because
`DependencyCategoryMappingVersion` is forbidden by
`repository-dependency-semantic-category.md` §10 from governing anything other
than package-identity-to-category mapping, because a reference identity may be
one-to-many while a category mapping is one-to-one, and because reference
detection must be able to resolve identities that carry no category.

## 9. Ambiguity

A qualifying reference identity accepted by **more than one** declaration of the
same ecosystem, after §8 including its tie-breaking rules, produces **no
observation**.

This mirrors the ambiguity rule of
`repository-dependency-semantic-category.md` §7.1: a missing observation is
explicitly meaningless, while a wrong attribution is a false claim.

Ambiguity is never resolved by declaration order, manifest path, alphabetical
order, version literal, popularity, recency or any heuristic. The only permitted
tie-breakers are the ones stated in §8.

## 10. Structurally Unreferenceable Declarations

These declaration classes can never carry a source reference. They produce no
observation and no absence claim, and they must never be counted, compared or
reported as unreferenced:

    Maven aggregator, starter and BOM artifacts
    Maven dependencyManagement entries                (S066 §11.1)
    Composer `php` and `ext-*` platform requirements  (S066 §11.1)
    npm `@types/*` type-only packages
    DotNet analyzer-only and build-only packages
    MSBuild property placeholders such as `$(SomePackage)` (S066 §11.1)

The Maven and Composer classes are listed for completeness; their ecosystems are
outside the V1 surface.

One of these classes is **not identifiable** from the V1 evidence surface and
therefore imposes no detector obligation: a DotNet analyzer-only or build-only
package is distinguished by `PrivateAssets` and `IncludeAssets` metadata, which
`repository-dependency-evidence.md` §11 does not read, so its declaration
carries nothing that separates it from an ordinary package.

Every other listed class is identifiable from the declaration itself — an npm
`@types/*` package by name, an MSBuild placeholder by its `$(...)` form — and
each remains a real detector obligation.

The detector must not attempt to recover an unidentifiable class by heuristic.
An unidentifiable member of these classes simply acquires no observation,
because it supplies no namespace or module that source can import, and §11
guarantees that its absence is never recorded, counted or compared.

### 10.1 Go `// indirect` Is Not a Detector Concern

Version 1 of this contract listed Go `// indirect` requirements as a
structurally unreferenceable class that the detector must identify. That was
wrong on two counts and is corrected here.

**The marker does not exist in the evidence surface.**
`RepositoryDependencyDeclaration` carries exactly ecosystem, manifest path,
package identity and declared version literal.
`repository-dependency-evidence.md` §11.1 records that dependency scope is not
recorded, and the Go manifest parser strips line comments before building a
declaration, so `// indirect` never reaches the declaration. No detector can
identify the class, and no implementation may invent a marker, a heuristic or a
declaration field in order to satisfy the removed sentence.

**The class was not structurally unreferenceable anyway.** An indirect Go
requirement is an ordinary module path. Source can legitimately import it.

Therefore, in V1:

- an indirect Go requirement is indistinguishable from an ordinary Go
  declaration and is treated as one;
- if a qualifying Go import deterministically matches it under §8, the detector
  produces the ordinary §3 syntactic observation, and nothing more;
- if no qualifying import matches it, that is a non-observation under §11 and
  §12 and carries no meaning whatsoever;
- the detector never infers, records or reports whether a module was intended
  to be direct or indirect.

Whether the upstream declaration surface should preserve the marker at all
remains open, exactly as
`repository-dependency-source-reference-evidence.md` §15 left it. It is not
resolved here and is not a prerequisite for V1.

## 11. Positive-Only Evidence

`DependencySourceReferenceEvidence` is a **positive-only** surface.

The result carries observations of detected references. It never carries a
"not referenced" item, an unreferenced-declaration list, a count of declarations
without references, a ratio, a coverage percentage or any derived comparison.

A consumer must not compute one either. Multiple references to the same
dependency inside one repository are **one** observation with several supporting
source locations; reference counts are traceability, never weight, and must
never be summed, compared or ranked.

## 12. Availability, Completeness and Absence

Detection inherits its availability semantics and adds none.

    dependency-manifests unavailable -> detection undetermined
    source-files unavailable         -> detection undetermined
    source-files incomplete          -> detection incomplete
    ecosystem outside V1             -> intentionally unsupported, not incomplete
    unreliably classified artefact   -> detection incomplete for that artefact
    ambiguous identity               -> no observation, not absence
    no observation                   -> no qualifying reference was observed
                                        inside the supported bounded surface

    unavailable != absence
    incomplete  != absence
    unsupported != absence
    absence     != negative evidence
    absence     != the dependency is unused

Detection is **normally incomplete**, and this is the expected state rather than
a defect. The `repository-source-evidence-collection.md` bounds admit at most 32
retrieval attempts and 384 KiB per repository, so a large repository yields a
small deterministic sample of its source surface. Session 072 measured bounded
corpora covering as little as 2% of a repository's supported eligible files.

Consequently, a dependency with no observation is the **normal** case, and
reading it as evidence of anything is prohibited by §3 and §11.

## 13. Deterministic Ordering

    same pinned repository state
    + same AnalyzerVersion
    + same SourceReferenceRuleVersion
    = the same detection result

Observations are ordered by ecosystem, then by declaration identity, then by
source path, then by the source location within that path, all `Ordinal`
ascending. Identical observations collapse to one.

No clock, no locale, no randomness, no popularity, no provider ranking and no
LLM participates in detection or in ordering.

## 14. Minimum Observation Payload

One observation carries exactly:

    ecosystem
    + a reference to the source DependencyDeclaration
    + the resolved reference identity that matched
    + one or more source locations, each carrying
        source Path
        + the qualifying form kind
        + the 1-based line number of the qualifying form

The pinned `HeadCommitSha` is not duplicated into the observation; it is carried
by `RepositorySnapshot` and reached through the traceability chain of §15.

Line numbers are deterministic because `repository-source-evidence-collection.md`
§16 preserves line endings and strips only a single leading BOM.

Explicitly **not** carried in V1:

- a source snippet or any source substring. `repository-source-evidence-collection.md`
  §21 records that public repositories may contain committed secrets and forbids
  duplicating source content beyond the collection surface. Persisting a snippet
  into a derived evidence payload reopens that surface and requires its own
  approved contract.
- a content hash. It is not self-contained replay and has no demonstrated V1
  need.
- a column offset, a byte offset, a role classification, a generated flag, a
  confidence value, a count, a weight or a score.

Long-term persisted evidence and self-contained replay remain **unresolved**,
exactly as `repository-dependency-source-reference-evidence.md` §11 left them.

## 15. Traceability

Required traversal:

    DependencySourceReferenceEvidence
    ↓
    reference observation
    ↓
    source location: Path + form kind + line
    ↓
    RepositorySourceFile
    ↓
    source DependencyDeclaration
    ↓
    RepositoryAnalysisReference
    ↓
    RepositoryAnalysis
    ↓
    RepositorySnapshot  (pinned HeadCommitSha)

An observation without both a source location and a source declaration
reference is invalid.

`RepositoryAnalysisReference` is a derived-result concern and is never a field
of a raw source artefact.

Prose sources such as README text, topics or the repository description can
never appear in this chain.

## 16. Domain and Infrastructure Ownership

Detection is semantic policy and lives in **Domain**, following the existing
`RepositorySemanticEvidencePolicy` precedent.

- Domain owns the qualifying forms, the lexical exclusion, identity resolution,
  ambiguity behavior, the observation shape and the ordering.
- The result is a derived, non-scoring result composed from one
  `RepositoryAnalysis`. It is not written into `RepositorySnapshot`, because the
  snapshot carries collected data rather than derived semantics.
- Infrastructure retrieves and normalizes provider data and, if a future version
  introduces an identity registry, loads that artefact — mirroring
  `DependencyCategoryRegistryLoader`. Reference interpretation never becomes an
  Infrastructure concern merely because Infrastructure retrieves file contents.

No class name is canonical product truth. This contract owns behavior, not
incidental naming.

## 17. Version Ownership

Detection semantics are owned by a dedicated version:

    SourceReferenceRuleVersion

It governs exactly: the supported V1 ecosystem surface; the extension to
ecosystem map; the qualifying syntactic forms; the comment and string exclusion
rules and the desynchronization rule; the literal-only call rule; the identity
resolution rules and their tie-breakers; the ambiguity rule; the structurally
unreferenceable classes; the observation payload; and the deterministic
ordering.

Version semantics:

- **minor** — an additive ecosystem, form or rule that cannot change a
  previously produced observation;
- **major** — any change that can alter, remove or re-attribute a previously
  produced observation, including an identity-resolution change, a tie-breaker
  change, an exclusion-rule change and a payload change;
- **patch** — a non-semantic correction only.

Detection semantics must **not** hide inside `AnalyzerVersion`: collection can
change without detection changing, and detection can change without collection
changing.

Unchanged owners: `AnalyzerVersion` keeps collection and snapshot normalization;
`RuleSetVersion` stays scoring-only; `DependencyCategoryMappingVersion` stays
category-mapping-only; `ObservationRuleVersion` keeps P5; `PatternRuleVersion`
keeps P6. None of them may own detection semantics.

A result produced under one `SourceReferenceRuleVersion` must never be silently
compared with, or merged into, a result produced under another.

## 18. Isolation

Detection must not:

- write into `RepositorySnapshot`,
- overlay or modify `RepositorySnapshot.FileTree`,
- write into `RepositorySnapshot.CollectionWarnings`,
- affect `RepositoryAnalysis.Status`,
- affect any existing availability or limitation key,
- affect any of the 15 scoring signals,
- create signal 16 or beyond,
- affect any category result,
- create a repository report element,
- change `AnalyzerVersion`, `RuleSetVersion`, `ReportSchemaVersion`,
  `DependencyCategoryMappingVersion` or `PatternRuleVersion`.

Repository MVP V1 still owns exactly 15 scoring signals.

Introducing this contract requires **no** `AnalyzerVersion` change, because it
changes no collected or normalized snapshot value.

## 19. Security and Privacy

Repository source code is untrusted data.

The detector must never execute, build, compile or evaluate it, must never send
it to an LLM, and must never log a source line, a source substring, a matched
region or any source payload. Source content must never appear in
`CollectionWarnings`, in exception messages or in report output.

Diagnostic identity may use bounded metadata such as a path, a line number and a
form kind, never a source payload.

Cancellation must remain honored.

## 20. Known Limitations

Accepted V1 limitations. None of them may ever be read as a developer
deficiency, and none of them may be corrected by heuristic.

**Guaranteed false negatives, from upstream evidence-surface losses:**

- an npm alias declaration — `"local": "npm:real@1"` — resolves under
  `repository-dependency-evidence.md` §10 to the referenced package and discards
  the manifest-local name, while the source imports the local name;
- a Cargo `package =` rename has the identical shape and the identical loss.
  Session 072 observed 13 such rename declarations in real repositories.

These are evidence-surface limitations, not detector defects, and V1 does not
guess a local alias.

**Guaranteed false negatives, from the §8.3 decision to ship no registry:**

A Python distribution name is not a function of its module name, so every
distribution whose module name is not derivable by the §8 normalization is
permanently unmatched in V1. Session 072 measured this over the canonical
46-entry population: `scikit-learn` supplies `sklearn`, `psycopg2-binary`
supplies `psycopg2`, and `pymongo` supplies `pymongo`, `bson` and `gridfs`, of
which only `pymongo` itself is derivable.

This is the accepted, and deliberately visible, cost of shipping no registry.
It moves coverage down and never moves attribution wrong.

**Accepted false negatives, by design:**

- dependency injection, container wiring and reflection;
- re-exports and transitive access through another package;
- every reference living outside the bounded collection surface;
- an artefact that the §6.1 desynchronization rule marks unreliable;
- any ambiguous identity under §9;
- every ecosystem outside §4.

**Residual false-positive paths, measured or reasoned, and explicitly not
claimed to be zero:**

- a module-resolution alias configured outside the manifest — a TypeScript
  `paths` entry, a bundler alias or an import map — that shadows a declared
  package name with a local module. Session 072 observed no such collision in
  its corpus and therefore records this as an **unmeasured** residual risk;
- a Go `replace` directive redirecting a declared module path to local code: the
  import path still matches the declaration, so the syntactic claim of §3 holds,
  but the code actually behind that path is local. Session 072 observed 34
  replace arrows in one external probe;
- an identity collision in which the declared package does not in fact supply
  the namespace or module that matched, and the true supplier is undeclared.

V1 therefore claims **no measured false positive within the supported surface**.
It does not claim zero false positives by construction.

**Coverage limitations:**

- Session 072 measured that **17.5% of the V1 surface's declarations** acquired a
  qualifying reference inside the bounded corpus, rising to 26.4% when the
  corpus was extended by one deterministic window. Across all eight ecosystems
  the same figures are 19.1% and 27.9%. Detection is a sample, never a census.
- The collection budget is shared with ecosystems that V1 cannot use.
  `repository-source-evidence-collection.md` §12 builds its total candidate
  order over the whole 24-extension allow-list, so `.java`, `.php`, `.kt`,
  `.scala`, `.groovy` and `.rb` artefacts consume retrieval slots and corpus
  bytes while producing no observation under §4. In a polyglot repository this
  lowers V1 yield systematically. Narrowing collection to the V1 surface is
  explicitly **not** done: `repository-source-evidence-collection.md` defines one
  global allow-list in §6, an ecosystem-blind eligibility pipeline in §9, a
  declaration-independent trigger in §9.1 and a total candidate order over every
  extension group in §12. Collection is deliberately ecosystem-blind, and
  detection must not reach back into collection policy.
- Detection alone establishes no `ProducedWorkEvidence` and no ProblemArea.

## 21. Prohibited Semantics

This surface must never produce or support:

- a repository score contribution or any change to the 15 scoring signals,
- signal creation, weighting, confidence or ranking,
- counts, ratios or coverage percentages of any kind,
- runtime-use or production-use inference,
- `ProducedWorkEvidence` inference,
- ProblemArea inference of any kind,
- developer skill, understanding, capability, seniority, employability, hire or
  no-hire inference,
- authorship or originality inference,
- locale-dependent semantics,
- LLM-derived or LLM-adjusted detection.

Unsupported, unavailable, incomplete, ambiguous and unobserved states are never
absence and never negative evidence.

## 22. Downstream Relationship

    DependencyDeclaration
    + RepositorySourceFile[]
        ↓
    this contract
        ↓
    DependencySourceReferenceEvidence
        ↓
    ProducedWorkEvidence decision            — UNRESOLVED
        ↓
    RepositoryProblemAreaEvidence            — BLOCKED by S067
        ↓
    ProblemArea promotion                    — DEFERRED
        ↓
    P7                                       — NOT STARTED

This contract completes no downstream stage beyond its own.

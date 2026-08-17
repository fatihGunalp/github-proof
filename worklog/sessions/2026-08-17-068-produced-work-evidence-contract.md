# Session 068 — ProducedWorkEvidence Contract / Foundation

Date: 2026-08-17
Result: IN_PROGRESS
Scope: produced-work evidence feasibility and semantic boundary
Model Route: Claude Opus / xhigh
Model Gate: APPROVED

## Goal

Decide whether the current repository evidence surface can support an evidence
level stronger than `DependencySemanticCategoryEvidence` without exceeding what
public repository evidence can honestly establish.

Implementation is out of scope for the first deliverable.

## Starting State

Branch:
session/068-produced-work-evidence-contract

Session Base:
d5373f62fc9a8b149418351fc79caa67c7a7efea

Previous sessions:
066 — Repository Evidence Surface Expansion — COMPLETE / REMOTE VERIFIED
067 — Dependency Semantic Category Contract + Registry — COMPLETE / REMOTE VERIFIED

Canonical inputs:
docs/architecture/repository-dependency-evidence.md
docs/architecture/repository-dependency-semantic-category.md
docs/architecture/analysis-domain-model.md
docs/architecture/public-profile-evidence-patterns.md §9

## Canonical Evidence Ladder

    DependencyDeclaration
    != DependencySemanticCategoryEvidence
    != DependencyUsageEvidence
    != ProducedWorkEvidence
    != RepositoryProblemAreaEvidence
    != RepeatedProfilePattern(ProblemArea)
    != DeveloperExperienceOrCapability

A package declaration is never usage evidence. A semantic category observation
is never produced-work evidence.

## Mandatory First Deliverable

PRODUCED WORK EVIDENCE DECISION REPORT, then STOP.

No canonical docs mutation, no production code, no detectors, no collectors, no
ProblemArea, no P7.

---

# PRODUCED WORK EVIDENCE DECISION REPORT

## 1. Executive decision

**Conclusion C, with a naming correction.** The current evidence surface is
sufficient to establish *declaration* and *category*, and it is **not**
sufficient to establish that a declared dependency is referenced by the
repository's own code. No evidence level between category and produced work is
derivable today.

Two findings drive this:

1. **Observed fact:** the collection pipeline stores general file *existence*
   but not general source-code *contents*.
2. **Naming:** even with source contents, the strongest honest claim would be a
   code reference, not "produced work". `ProducedWorkEvidence` is epistemically
   too strong for anything observable and should not be contracted under that
   name.

Recommended next reachable level: `DependencyUsageEvidence`, meaning a
compile-visible reference in the repository's own tracked source. It requires a
new bounded collection surface and is therefore a separate approved change.

## 2. Current evidence surface audit

Verified against the contracts and the implementation, not against class names.

| Surface | Existence | Contents | State |
|---|---|---|---|
| repository metadata | yes | description, homepage, createdAt, issuesEnabled | AVAILABLE, prose and flags only |
| file tree | full recursive paths and sizes | `content: null` at `GitHubRepositoryContentsCollector.cs:560-565` | PARTIALLY AVAILABLE, truncation possible |
| general source-code contents | paths only | none | **NOT AVAILABLE** |
| README | yes | full content | AVAILABLE, prose |
| documentation corpus `docs/**/*.md` | yes | bounded 16 files, 128 KiB per file, 512 KiB corpus | PARTIALLY AVAILABLE, prose |
| workflow files `.github/workflows/*.yml\|yaml` | yes | full content | AVAILABLE, configuration |
| project/dependency files | yes | content for `*.csproj`, `pom.xml`, `build.gradle(.kts)`, `pytest.ini`, `package.json`, `Directory.Packages.props`, `jest.config.*`, `vitest.config.*` | PARTIALLY AVAILABLE, narrow allow-list |
| dependency manifests (S066) | yes | content fetched during collection, **not retained**; only normalized declarations survive | AVAILABLE as derived declarations |
| dependency declarations | yes | ecosystem, manifest path, package identity, version literal | AVAILABLE, with §11.1 limitations |
| test artefact visibility | test paths visible | only through the project-file allow-list | PARTIALLY AVAILABLE |
| configuration artefact visibility | paths visible | only workflows and the project-file allow-list; `Dockerfile`, `appsettings.json`, `docker-compose.yml`, `.env`, web-server config are path-only | PARTIALLY AVAILABLE |
| releases, tags | yes | names, flags, dates | AVAILABLE, process evidence |
| commits, issues, pull requests, contributors | yes | metadata and titles | AVAILABLE, process evidence |
| languages | n/a | linguist byte counts | AVAILABLE |

**Audit hypothesis: CONFIRMED with one nuance.** General application source code
is not collected. The exception is that a few collected "project files" are
themselves source: `build.gradle` and `build.gradle.kts` are Groovy or Kotlin,
and `jest.config.*` and `vitest.config.*` are JavaScript or TypeScript. They are
build and test configuration written in a programming language, not application
code, and they are selected by filename rather than by being source.

Raw versus derived: `FileTree`, `Readme`, `Workflows`, `DocumentationMarkdown`
and `LicenseFile` are raw collected evidence. `DependencyDeclarations`,
`Languages` and every semantic category result are derived.

## 3. Canonical meaning and naming assessment

Maximum safe meaning of any code-derived level, if source contents were
collected:

> The repository's own tracked source at path P, at commit C, contains a
> reference to package D.

It must not mean: the code executes, is deployed, is used in production, is
correct, is high quality, that the developer is skilled, that the developer
authored it, that the repository is a real commercial project, or that the
dependency is used at runtime.

**The name is the problem.** "Produced work" asserts that meaningful work was
created. Reference presence cannot distinguish authored code from generated,
scaffolded or copied code, so no observable evidence supports the word
"produced". Recommendation: retire `ProducedWorkEvidence` as a target level and
contract `DependencyUsageEvidence` instead, with "produced work" recorded as a
qualitative construct that public repository evidence cannot establish.

Useful consequence: `public-profile-evidence-patterns.md` §9.4 condition 4 asks
for a structured boundary separating declared dependencies from produced work. A
**declared versus used** boundary is observable and satisfies the intent of that
condition without claiming production.

## 4. Evidence type assessment

| Type | Max safe meaning | Strength | Main false-positive risk | Collection need | Ecosystem dependence | Stand alone? |
|---|---|---|---|---|---|---|
| code-level import/reference | source at P references D | highest available | generated and scaffolded code contains real references | **new**: bounded source contents | high | yes, for usage only |
| framework structural evidence | conventional paths exist | weak | scaffolds produce identical trees | none, already path-visible | high | no |
| configuration wiring | a workflow or config references a tool | narrow | copied CI config; tool used in CI only | partly available | medium | no |
| structured artefacts (migrations, models, controllers, routes) | conventional artefacts exist | weak by path, medium with content | generators emit all of them | contents currently absent | high | no |
| generated artefacts | machine-generated files exist | negative | it is explicitly not authored work | contents absent | high | no, and must never support a work claim |
| executable or runtime proof | code builds or runs | strongest in principle | not obtainable; requires building untrusted code | out of product scope | n/a | not viable |

No evidence class is pre-approved. Prose sources (README, topics, description)
are excluded from every class.

## 5. False-positive analysis

| Scenario | Can it promote declared → used? | Can it promote used → produced work? |
|---|---|---|
| scaffolded project | yes for every path and structure detector | yes: scaffolds contain real references |
| template repository | yes | yes; P2 lineage excludes forks and templates only when the manifest flags them |
| tutorial or sample repository | yes | yes; indistinguishable in code |
| unused dependency | declaration-only detectors would; a code reference would not | n/a |
| generated code | yes | yes, and it is the clearest reason the word "produced" fails |
| test-only usage | reference is genuine | no; test usage is not production usage |
| dev-only dependency | S066 does not record dependency scope, so it cannot be excluded | no |
| copied configuration | yes for configuration wiring | no |
| transitive or indirect dependency | **yes today**: Go `// indirect` entries are recorded as declarations | no |
| Maven `dependencyManagement` | **yes today**: version-management entries are recorded as declarations | no |
| declared but never referenced | yes for declaration-only detectors | no |

The Go `// indirect` and Maven `dependencyManagement` cases are already a live
amplification risk recorded in Session 067. They are a **separate prerequisite
requiring separate approval**, and this report does not change their semantics.

## 6. Ecosystem-neutrality analysis

At this level neutrality must mean equal epistemic ceiling, equal absence
semantics, equal traceability and no ecosystem receiving a stronger claim from
weaker evidence. It must not mean identical detectors.

Legitimate asymmetries, each of which changes what absence means:

- **Go**: unused imports are a compile error, so an import is compile-verified
  usage. Strongest signal of any supported ecosystem.
- **Rust, Java, C#, Python, TypeScript**: explicit imports are usual, but
  reflection, dependency injection, annotations, implicit usings and dynamic
  imports allow real usage without a textual reference.
- **Ruby**: Bundler auto-requires gems, so a gem can be used with no `require`
  anywhere in source. Absence of a reference means almost nothing.
- **PHP**: autoloading and container wiring produce the same effect.

Conclusion: absence of a code reference may never be treated uniformly, and in
Ruby and PHP it carries close to zero information.

## 7. Generic versus category-specific rules

Recommended split: **one generic evidence kind**, a reference from repository
source to a declared package, plus **category-specific ceilings** on what that
reference is allowed to mean.

The five locked categories do not share an evidence signature. A single ORM
reference is reasonable evidence of database access; a single import of an ML
framework says far less about ML work; an authentication library reference in
one file says little about an authentication implementation. Building one
generic policy for symmetry would flatten those differences and inflate the
weakest categories.

No detector and no per-category rule is designed in this report.

## 8. Traceability requirements

Minimum trust chain for any future usage-level evidence:

    usage evidence
    → source artefact identity (path) + pinned commit SHA
    → the exact observation descriptor (what matched)
    → the corroborated DependencyDeclaration
    → RepositoryAnalysisReference
    → RepositoryAnalysis
    → RepositorySnapshot

`RepositorySnapshot.HeadCommitSha` already pins content, so path plus commit is
sufficient for deterministic replay without storing file contents. Whether a
bounded snippet or a content hash must also be stored for human review is a
separate decision with size and privacy implications, and it is not decided
here. Prose sources may never appear in this chain.

## 9. Version ownership assessment

| Owner | Owns | Fit |
|---|---|---|
| `AnalyzerVersion` | collection and snapshot normalization | owns a **new collection surface** if one is added; does not own detection semantics |
| `RuleSetVersion` | scoring and evaluator semantics | no: this evidence is non-scoring |
| `DependencyCategoryMappingVersion` | package identity and alias to category | no, and the locked contract §10 forbids it |
| `ObservationRuleVersion` | P5 profile observation aggregation | no |
| `PatternRuleVersion` | P6 taxonomy and promotion | no |

A **new version owner is required** for usage or implementation detection
semantics: which reference forms count, per-category ceilings, and corroboration
requirements. Bounded naming alternatives, none locked here:
`DependencyUsageRuleVersion`, `ImplementationEvidenceRuleVersion`,
`UsageDetectionRuleVersion`.

## 10. Bounded alternatives

- **A1 — stop at declaration and category.** Zero risk, zero cost. ProblemArea
  stays deferred indefinitely.
- **A2 — bounded source-content collection.** A capped, deterministic,
  commit-pinned set of source files enables code-reference evidence. Cost: a
  collection-contract change, a major `AnalyzerVersion` implication, more API
  calls and a size budget.
- **A3 — GitHub code search.** Cheaper, but index-dependent, non-replayable and
  auth-scoped. Rejected: it breaks determinism.
- **A4 — path and structure heuristics only.** Rejected: scaffolds defeat them
  and the false-positive floor is unacceptable.
- **A5 — ladder correction.** Replace the produced-work level with
  `DependencyUsageEvidence` and record produced work as non-derivable.
  Orthogonal to the others and safe on its own.

## 11. Smallest safe recommendation

1. Adopt **A5** now: correct the ladder terminology and record that produced
   work is not derivable from public repository evidence at V1.
2. Treat the Go `// indirect` and Maven `dependencyManagement` amplification as
   a **separate prerequisite with separate approval** before any usage-level
   work.
3. Only then consider **A2**, contracted separately, with its own version owner
   and its own `AnalyzerVersion` decision.
4. Lock no detector, no per-category rule and no collection shape in this
   session.

## 12. Explicit non-decisions and remaining unknowns

**Proven now:** the surface carries file existence but not general source
contents; declaration and category are the only levels currently derivable; the
name `ProducedWorkEvidence` exceeds any observable evidence.

**Unproven:** that any observable evidence can establish produced work; that
path or structure evidence can survive scaffolds; that absence of a reference
means non-use in Ruby or PHP.

**Prerequisites before the next step:** the indirect and dependency-management
correction; a decision on A5; and, if usage evidence is wanted, an approved
bounded source-collection contract with a new version owner.

**Not decided here:** detector design, source-collection limits, snippet or hash
storage, the new version owner's name, per-category ceilings, and whether
ProblemArea is ever reachable.

---

# STAGE RECORD

## Decision reports

- PRODUCED WORK EVIDENCE DECISION REPORT — Revision 2: **ACCEPTED** as a decision
  report. None of its "separate approvals required" items became approved by
  that acceptance.
- INTERMEDIATE SOURCE EVIDENCE DECISION REPORT — Revision 3: **ACCEPTED**.

Accepted conclusions carried into the contract: the current surface provides no
general application source-file contents; the intermediate level is named
`DependencySourceReferenceEvidence`; its ceiling is a syntactic source-level
reference at a pinned commit; Go `// indirect` and Maven `dependencyManagement`
are known declaration-quality limitations that are neither a contract-design nor
a proven implementation prerequisite for this level.

## Canonical contract stage 1

Created `docs/architecture/repository-dependency-source-reference-evidence.md`,
CONTRACT LOCKED version 1, owning semantics only: canonical meaning, epistemic
ceiling, ladder position, prohibited meanings, raw versus derived boundary,
isolation, availability and absence ceiling, ecosystem neutrality, traceability
at the conceptual level, version ownership boundary, determinism and the limit
dimensions a future collection contract must define.

No collector, detector, runtime carrier, availability key, numeric bound,
extension set or selection algorithm was defined.

The Session 066 and Session 067 contracts were left byte-identical.

## Current position

- `ProducedWorkEvidence`: **unresolved**, and still mandatory before
  `RepositoryProblemAreaEvidence` under the Session 067 contract.
- `RepositoryProblemAreaEvidence`: **blocked**.
- ProblemArea taxonomy and promotion: not started.
- P7: not started.
- Implementation: none. No collector, no detector, no test, no scoring change,
  no signal 16+.

## Unresolved, carried forward

Bounded source-evidence collection contract; normalized source artefact shape;
minimum persisted evidence payload and the self-contained replay question;
qualifying-reference definition and per-ecosystem detection design; permanent
detection version-owner name; future `AnalyzerVersion` bump value; whether the
Go and Maven declaration limitations must be corrected before
`RepositoryProblemAreaEvidence`; the resolution of `ProducedWorkEvidence`.

## Current Status

AWAITING_SOURCE_REFERENCE_CONTRACT_REVIEW

# Session 072 — Dependency Source Reference Detection Contract

Date: 2026-08-18
Result: IN_PROGRESS — decision report delivered, contract NOT locked
Scope: architecture analysis + reference-detection feasibility audit + worklog/canonical state sync
Model Route: Claude Opus / high
Model Gate: APPROVED

## Goal

Decide whether a reference-detection semantic contract can be defined safely and
deterministically, so that `DependencySourceReferenceEvidence` becomes
producible from `DependencyDeclaration[]` + `RepositorySourceFile[]`.

This is not an implementation session. The mandatory first deliverable is the
REFERENCE DETECTION DECISION REPORT with verdict `READY` or
`NEEDS_MORE_MEASUREMENT`.

Design principle carried in from the handoff:

    producing no false positive outranks increasing coverage
    absence never means the dependency is unused

## Starting State

Branch:
session/072-dependency-source-reference-detection-contract

Session Base:
d480a467ec90114beea58f80e5722aa4db745c5d

Working tree at session start: clean.

### Corrected stale state — VERIFIED

`worklog/CURRENT.md` claimed Session 071 was pushed to a session branch and
`NOT INTEGRATED INTO MAIN`, with `Last Remote Verified Main = 2a1809d`.

Actual Git state at session start:

    main == origin/main == d480a46
    d480a46 docs: close session 071 worklog after remote verification
    ce74d4b feat: collect bounded raw repository source evidence
    2a1809d docs: define product experience principles

Session 071 is therefore integrated into `main` and remote verified. The
`session/071-bounded-source-evidence-collection` branch still exists locally and
on `origin`. CURRENT was stale and is synchronized by this session.

## Context Read

- `docs/architecture/repository-dependency-source-reference-evidence.md` (S068, LOCKED v1)
- `docs/architecture/repository-source-evidence-collection.md` (S071, LOCKED v1)
- `docs/architecture/repository-dependency-evidence.md` (S066, LOCKED)
- `docs/architecture/repository-dependency-semantic-category.md` (S067, LOCKED)
- `docs/architecture/registries/dependency-category-mapping/1.0.0.json` (46 entries)
- `docs/architecture/public-profile-evidence-patterns.md` §9
- `worklog/CURRENT.md`
- `worklog/sessions/2026-08-17-068-produced-work-evidence-contract.md`
- `worklog/sessions/2026-08-18-071-bounded-source-evidence-collection.md`

## Surface Audit — Verified Starting Facts

VERIFIED against source, not assumed:

- `RepositorySnapshot.SourceFiles` exists as
  `RepositoryCollection<RepositorySourceFile>`
  (`src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs:102`).
- `GitHubRepositorySourceFileCollector` and `GitHubRepositorySourceFileCollection`
  exist in Infrastructure.
- `AnalyzerVersion` is `1.1.0` (`src/GitHubProof.Web/Program.cs:17`).
- `RepositorySemanticEvidencePolicy` and `DependencyCategoryRegistry` live in
  `GitHubProof.Domain/Analysis`; the registry artefact is loaded by
  `GitHubProof.Infrastructure/Registries/DependencyCategoryRegistryLoader.cs`.
  This is the existing precedent for "semantic policy in Domain, registry
  artefact loaded by Infrastructure".
- No reference detector, no `DependencySourceReferenceEvidence` type and no
  detection version owner exists anywhere in `src/`.

Consequently one canonical statement is now stale:
`repository-dependency-source-reference-evidence.md` §5 still says
"Production source collection is **not yet implemented**". Session 071 Step 5
implemented it. This session corrects that prerequisite statement only.

---

# REFERENCE DETECTION DECISION REPORT

## 0. Executive verdict

    NEEDS_MORE_MEASUREMENT

Fourteen decisions are resolvable now from locked contracts and documented
ecosystem semantics, and are recorded below as R1–R14 so they are not
re-derived. Five decisions are blocked on measurement and are recorded as
M1–M5.

No canonical reference-detection contract is created in this session. The
blocking reason is not that detection is impossible; it is that the two
mechanisms the whole false-positive floor depends on — per-language lexical
exclusion of comments and string literals, and the identity-resolution surface
for the ecosystems where package identity is not source identity — have never
been measured against real bounded corpora. Session 071 refused to lock numeric
bounds before measurement for the same reason.

## 1. Input surface audit

What the detector would actually receive:

| Input | Fields available | Source |
|---|---|---|
| `DependencyDeclaration` | ecosystem, manifest path, package identity, declared version literal | S066 §10 |
| `RepositorySourceFile` | `Path`, `Content` only | S071 §15 |
| pinned commit | `RepositorySnapshot.HeadCommitSha` | S071 §4 |
| availability | `dependency-manifests`, `source-files` keys with `IsComplete` | S066 §7, S071 §18 |

Deliberately absent from `RepositorySourceFile`, per S071 §15: language,
ecosystem, application/test/example role, generated flag, size, hash and
dependency identity. Anything of that kind the detector needs must be derived,
and the derivation becomes detection semantics.

ASSESSED: source-file ecosystem is derivable. The S071 §6 allow-list is a fixed
24-extension set and the extension to ecosystem map over it is total and
deterministic:

    .cs .fs .vb .razor .cshtml                      -> DotNet
    .java .kt .kts .scala .groovy                   -> Java
    .js .jsx .mjs .cjs .ts .tsx .d.ts .vue .svelte  -> JavaScript
    .py -> Python   .go -> Go   .rs -> Rust   .php -> Php   .rb -> Ruby

This map is not free: it is versioned detection semantics, and it must move
whenever the S071 allow-list moves.

## 2. Identity gap analysis — the crux

The central question is whether declared package identity can be turned into
source-level reference identity deterministically.

ASSESSED per ecosystem, from documented package-manager and language semantics.
This is desk analysis, not measurement.

| Ecosystem | Declared identity | Source reference identity | Derivation | Class |
|---|---|---|---|---|
| Go | module path, e.g. `github.com/jackc/pgx/v5` | import path; the module path is a prefix of every package in it, `/vN` included | exact prefix on path segments | **TOTAL** |
| JavaScript | npm name, scope preserved | bare specifier head: `pkg` or `@scope/pkg`, optional `/subpath` | strip subpath, compare | **TOTAL** |
| Rust | crate name | `use crate_name::`, `-` becomes `_` | total function, one documented inverse loss | **TOTAL, lossy inverse** |
| Python | PEP 503 distribution name | top-level module name | **not a function**; a distribution may ship zero, one or many modules under any name | **PARTIAL** |
| DotNet | NuGet package id | CLR namespace | convention only; a package may ship any namespace, or none | **PARTIAL** |
| Ruby | gem name | `require` argument | usually equal, documented exceptions | **PARTIAL** |
| Php | `vendor/package` | PSR-4 root namespace | independent by construction | **NONE** |
| Java | `groupId:artifactId` | package namespace | independent by construction; aggregator artifacts ship no class at all | **NONE** |

### 2.1 Probe against the canonical 46-entry registry

AUDITED: the 46 entries of `dependency-category-mapping/1.0.0.json` were
enumerated from the artefact. Their expected source identities are ASSESSED from
documented package semantics, not measured.

    Go          3/3   derivable
    JavaScript 10/10  derivable
    Rust        3/3   derivable
    DotNet      6/6   derivable by exact namespace equality, case caveat below
    Python      9/11  derivable; scikit-learn -> sklearn and
                      psycopg2-binary -> psycopg2 are not
    Ruby        4/5   derivable; activerecord -> active_record is not
    Php         1/4   derivable; doctrine/orm -> Doctrine\ORM,
                      guzzlehttp/guzzle -> GuzzleHttp and
                      laravel/framework -> Illuminate are not
    Java        1/4   derivable; com.squareup.okhttp3:okhttp -> okhttp3,
                      org.hibernate.orm:hibernate-core -> org.hibernate and
                      org.springframework:spring-webmvc -> org.springframework.web.*
                      are not

    total 37/46 derivable, 9/46 not

This probe is deliberately biased: the registry contains 46 curated, popular
packages. It establishes that the gap is real and ecosystem-structured. It does
**not** estimate the gap over a real declaration population, which is M3.

### 2.2 Named identity hazards

- **Java groupId prefixing is not identity.** `org.springframework` is a prefix
  of the namespaces of dozens of Spring artifacts. Matching
  `org.springframework:spring-webmvc` by groupId prefix would attribute a
  `spring-core` import to `spring-webmvc`. That is a false positive, not a
  coverage gap.
- **Maven aggregator artifacts can never be referenced.** A
  `spring-boot-starter-*` POM ships no class, so no import can ever name it.
  Under S066 §11 it is nevertheless a recorded declaration.
- **npm alias loss.** S066 §10 resolves `"foo": "npm:bar@1"` to the referenced
  package `bar` and discards the manifest-local name. Source imports `foo`. The
  information needed to match is destroyed upstream. This is a guaranteed false
  negative, not a defect of the detector.
- **Cargo `package =` rename loss.** Identical shape to the npm alias, same
  upstream loss, same guaranteed false negative.
- **DotNet case.** S067 §9.2 lowercases DotNet lookup identity; C# namespaces
  are case-sensitive. Detection must compare against the preserved `PackageId`,
  never the category-registry lookup identity, or else accept case-insensitive
  namespace comparison as a deliberate versioned rule.
- **Namespace collision.** Exact-namespace matching in DotNet, and top-level
  module matching in Python, both admit a residual false positive: package A is
  declared, package B actually supplies the namespace or module, and the
  reference is attributed to A. Rare, real, and unbounded by any rule available
  at this level.
- **Composer platform requirements.** S066 §11.1 records `php` and `ext-*` as
  declarations. They can never have a source reference.
- **Go `// indirect`.** S066 §11.1 records them. They are typically not
  imported, so they inflate the unreferenced-declaration population only. They
  create no false positive.

## 3. Syntactic reference form assessment

ASSESSED. The question is which forms are safe without a compiler, a symbol
table or a type resolver.

| Form | Safe for V1? | Reason |
|---|---|---|
| import / use / require **declaration** with a literal identity | **yes** | the package identity appears verbatim in a position whose grammar is line-anchored and unambiguous |
| JS `require('<literal>')` / `import('<literal>')` | **yes, literal argument only** | a literal argument is a specifier; a variable or concatenated argument is not resolvable and must yield nothing |
| Go grouped `import ( ... )` blocks | **yes** | the block grammar is regular and the entries are quoted paths |
| Ruby `require` with a literal argument | **yes** | same |
| PHP `use Ns\...;` | **yes** | resolvable to a root namespace |
| type references, symbol references, API calls | **no** | requires resolution the pipeline cannot perform; `new Client()` names no package |
| annotations, attributes, decorators | **no** | same |
| DI, container and reflection wiring | **no** | S068 §9 already records these as real usage without a textual reference |
| bare text occurrence of the identity anywhere | **never** | S068 §4 prohibits it |

RESOLVED: V1 detection is **import-declaration detection**, not usage detection.
That is a narrower claim than the word "reference" suggests, and the contract
wording must not overstate it.

## 4. Comment and string-literal exclusion assessment

This is the single largest unmeasured risk.

A line-anchored import grammar still matches inside:

- block comments — C#, Java, Kotlin, Scala, JS, TS, Go, Rust, PHP;
- docstrings and triple-quoted strings in Python;
- template literals in JS and TS;
- heredoc and nowdoc in PHP and Ruby;
- verbatim and raw string literals in C#;
- documentation blocks that legitimately contain example imports, which is
  extremely common in `.d.ts` and in library source.

Existing repository precedent is explicitly weaker than what is needed here:
S066 §11 strips `#` and `//` line comments from Gradle manifests and **does not
strip block comments**. That is tolerable for manifest parsing and is not
tolerable for a zero-false-positive evidence level.

ASSESSED: eliminating this class requires a minimal per-language lexical state
machine covering comment and string forms for eight language families. That is
definable and deterministic. Whether a hand-written one reaches an acceptable
false-positive floor across real bounded corpora is unproven. This is M1.

## 5. Declaration classes that can never produce a reference

RESOLVED as a required future contract clause: these classes must be recorded as
structurally unreferenceable, so their absence is never even counted as an
observation.

    Maven aggregator / starter / BOM artifacts
    Maven dependencyManagement entries                  (S066 §11.1)
    Go // indirect requirements                         (S066 §11.1)
    Composer php and ext-* platform requirements        (S066 §11.1)
    npm @types/* type-only packages
    DotNet analyzer-only and build-only packages
    MSBuild property placeholders, e.g. $(SomePackage)  (S066 §11.1)

## 6. False-positive and false-negative strategy

RESOLVED, mirroring the S067 §7.1 ambiguity rule, which is the correct existing
precedent:

- an unresolved identity produces **no evidence**, never a guessed match;
- a match outside an approved syntactic form produces **no evidence**;
- a match inside a comment or a string literal produces **no evidence**;
- a false negative is explicitly acceptable and semantically meaningless,
  because absence is already meaningless under S068 §8;
- a false positive is a **false claim** and is the only failure mode that
  damages the evidence ladder.

Accepted and enumerated false negatives: npm alias, Cargo rename, Ruby
`Bundler.require`, PHP autoloading and container wiring, DI and reflection,
implicit usings, aggregator artifacts, and every reference living outside the
bounded 32-file / 384 KiB corpus.

## 7. Absence semantics

RESOLVED — inherited, nothing new to decide. S068 §8 and S071 §18 already lock
this and must be carried forward verbatim, never flattened:

    source-files unavailable   -> detection undetermined
    source-files incomplete    -> detection incomplete, never complete-and-empty
    complete and empty corpus  -> no reference observed in the supported surface
    unmapped identity          -> no observation, not absence
    absence                    -> never negative evidence

One additional clause is required and is new: because the S071 bounds normally
leave large repositories at `IsComplete = false`, the **normal** state of this
evidence level is incomplete. The future contract must say so explicitly, so
that incompleteness is not read as a defect.

## 8. Registry requirement assessment

RESOLVED: a separate versioned identity registry is required, because Java and
Php are structurally non-derivable and Python, Ruby and DotNet are only
partially derivable.

It must not be folded into `dependency-category-mapping`:

- the category registry answers `identity -> category` and is owned by
  `DependencyCategoryMappingVersion`, which S067 §10 forbids from governing
  anything else;
- the category registry covers only packages that have a mapped category, while
  reference detection must be able to resolve identities that have no category;
- a reference identity is one-to-many — one distribution may ship several
  modules or namespaces — while a category mapping is one-to-one.

Its shape should reuse the S067 §9 discipline: a separate JSON artefact,
explicit entries only, no wildcards or regular expressions, unique per
`(ecosystem, identity)`, rationale as review metadata, immutable versions and
deterministic ordering.

What is **not** resolvable now is its required size, and therefore its
viability. That is M3.

## 9. Traceability and minimum observation payload

S068 §11 left this open. ASSESSED position, not locked.

Minimum defensible payload for one observation:

    ecosystem
    + the source DependencyDeclaration reference
    + source Path
    + pinned HeadCommitSha            (already on the snapshot)
    + the matched syntactic form kind (import | use | require | require-call)
    + the resolved reference identity that matched

Line number: usable, because S071 §16 preserves line endings and strips only a
leading BOM, so line counting over normalized `Content` is deterministic.

Bounded snippet: **assessed as unsafe for V1**. S071 §21 records that public
repositories may contain committed secrets, and forbids duplicating source into
logs, warnings, exception text and report output. Persisting a snippet into a
derived evidence payload reopens exactly that surface and needs its own
approval. Not decided here.

Content hash: S071 §15 already records that a hash is not self-contained replay
and has no demonstrated raw-evidence need. The same argument applies here.

Self-contained replay remains unresolved, exactly as S068 §11 left it.

## 10. Version ownership

RESOLVED in structure; the name is still open.

S068 §12 and S071 §23 both require a separate owner. Collection and detection
change independently, so detection semantics must not hide inside
`AnalyzerVersion`.

The owner must govern: the approved syntactic forms; the comment and string
exclusion rules; the extension to ecosystem map; the identity derivation rules;
the identity registry version binding; and the structurally-unreferenceable
declaration classes.

S068 §9 offered `DependencyUsageRuleVersion`,
`ImplementationEvidenceRuleVersion` and `UsageDetectionRuleVersion`.

ASSESSED recommendation: **`SourceReferenceRuleVersion`**. It names the evidence
type it governs, it does not claim "usage" or "implementation" — both of which
exceed the S068 §3 ceiling — and it parallels the existing
`ObservationRuleVersion` and `PatternRuleVersion` naming. Recommended, not
locked, because naming it belongs in the contract that defines it.

A second version binding is required for the identity registry artefact itself,
parallel to `DependencyCategoryMappingVersion`.

## 11. Layer placement

RESOLVED. Detection is semantic policy and belongs in **Domain**, following the
existing `RepositorySemanticEvidencePolicy` precedent:

- Domain owns the policy, the derived result and the deterministic ordering;
- Infrastructure owns only registry artefact loading, mirroring
  `DependencyCategoryRegistryLoader`;
- the result is a derived non-scoring result, not a `RepositorySnapshot` field,
  per S068 §6.

S068 §6 already states that reference interpretation does not become an
Infrastructure concern merely because Infrastructure retrieves file contents.

## 12. Resolved decisions

`RESOLVED` here means "settled by this audit, so the next session does not
re-derive it". It is **not** a canonical lock. No canonical doc owns R1–R14, and
worklog is never product or domain truth. When the reference-detection contract
is eventually written, every R item must be re-approved inside that canonical
doc rather than inherited from this file.

R1  Detection consumes `DependencyDeclaration[]` + `RepositorySourceFile[]` +
    the pinned commit, and nothing else. No new collection surface is required.
R2  Source-file ecosystem is derived from the S071 §6 extension allow-list by a
    total, versioned map, and moves whenever that allow-list moves.
R3  Package identity is not source identity. Go, JavaScript and Rust have a
    total derivation; Python, DotNet and Ruby are partial; Java and Php are
    structurally non-derivable.
R4  V1 is **import-declaration detection**, not usage detection. Type, symbol,
    API, annotation, DI and reflection references are out.
R5  Loose textual occurrence is never evidence, under S068 §4.
R6  `require()` and `import()` count only with a literal string argument. A
    computed argument yields nothing.
R7  A match inside a comment or a string literal is never evidence, and the
    exclusion mechanism is versioned detection semantics.
R8  An unresolved identity yields no observation, mirroring the S067 §7.1
    ambiguity rule. Guessing is prohibited.
R9  A separate versioned identity registry is required and must not be folded
    into `dependency-category-mapping`.
R10 The structurally unreferenceable declaration classes of §5 never produce an
    observation of any polarity.
R11 The npm alias and Cargo rename losses originate in S066 §10 and are accepted
    false negatives, not detector defects.
R12 Absence semantics are inherited unchanged from S068 §8 and S071 §18, plus a
    new explicit clause that incomplete is the normal state of this level.
R13 A separate detection version owner is required;
    `SourceReferenceRuleVersion` is the recommended name.
R14 The detector lives in Domain as a policy producing a derived non-scoring
    result; Infrastructure loads the registry artefact only.

## 13. Measurement-blocked decisions

M1  **Comment and string-literal exclusion floor.**
    Blocked because the entire zero-false-positive claim rests on eight
    hand-written lexical exclusions that have never been run against real
    source. The existing Gradle precedent (S066 §11) does not strip block
    comments, so the repository holds no evidence that this class is
    controllable.
    Measure: over a pinned multi-ecosystem corpus, count import-shaped matches
    falling inside comments, docstrings, template literals, heredocs and
    verbatim strings, per language family.

M2  **Reference yield of the bounded corpus.**
    Blocked because S071 measured that 3 of 10 canonical validation
    repositories carry no eligible source at all, that the largest carried 45
    eligible files, and that the 32-file round-robin candidate order is
    `StringComparer.Ordinal` on extension groups and on paths within each group
    (S071 §12). Whether that sample systematically misses
    import-bearing files is unknown, and it decides whether this level is worth
    its complexity.
    Measure: per pinned repository, declarations observed, declarations with at
    least one detected import inside the bounded corpus, and the same figure
    over the unbounded eligible surface.

M3  **Identity registry sizing and viability.**
    Blocked because 9 of 46 non-derivable entries is a curated sample, not a
    population estimate. If real Java and Php declaration populations need
    hundreds of hand-approved entries, the registry is not maintainable and V1
    must instead ship only the total-derivation ecosystems.
    Measure: over a pinned corpus, the share of declarations per ecosystem whose
    source identity is derivable by rule versus requiring an explicit entry.

M4  **Whether the "total" derivations actually hold.**
    Blocked because Go vendored paths and `replace` directives, TypeScript
    `paths` aliases shadowing bare specifiers, and monorepo workspace packages
    can each break a derivation that is total in theory.
    Measure: counter-example hunt on pinned Go and TypeScript repositories.

M5  **The V1 ecosystem surface.**
    Blocked by M1, M3 and M4. The candidate positions are: Go + JavaScript only;
    Go + JavaScript + Rust; or all eight behind a registry. This is the single
    most consequential clause of the future contract and it must not be chosen
    before M1 to M4 return.

Not blocked by measurement, but deliberately deferred to the contract that will
define them: the exact observation payload, whether a bounded snippet is
persisted, the detection version owner's final name, and the future
`AnalyzerVersion` implication if any.

## 14. Explicit non-decisions

Unchanged and untouched by this session:

- `ProducedWorkEvidence` remains unresolved. Import-declaration evidence does
  not satisfy it and never unblocks `RepositoryProblemAreaEvidence` on its own
  (S068 §2).
- `RepositoryProblemAreaEvidence` remains blocked by the S067 contract.
- ProblemArea taxonomy and promotion remain deferred (patterns §9).
- P7 remains NOT STARTED.
- The 15 scoring signals, `RuleSetVersion`, `ReportSchemaVersion`,
  `DependencyCategoryMappingVersion` and `PatternRuleVersion` are untouched.
- Whether Go `// indirect` and Maven `dependencyManagement` require upstream
  correction remains open, exactly as S068 §15 left it.

---

## Changes

Docs and worklog only. No `src/`, no `tests/`, no registry artefact, no report,
no UI.

## Validation

No build or test run is required: no production or test code was touched.

## Not Done

- No canonical reference-detection contract was created. The verdict is
  `NEEDS_MORE_MEASUREMENT`.
- No detector, no `DependencySourceReferenceEvidence` type, no identity registry
  artefact.
- No measurement was performed; measurement was outside this session's allowed
  scope.

## Next

Exact next action: run the M1 to M4 measurement round as a temporary, read-only,
pinned-commit measurement, following the Session 071 Step 2 method — GitHub REST
at pinned commits only, no clone, no build, no execution, no code search, no
LLM, no raw source printed or written into the repository, and no measurement
artefact left behind. Then re-run this decision report and lock the contract
only if M5 becomes single-valued.

## Git

Nothing committed or pushed.

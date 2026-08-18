# Session 072 — Dependency Source Reference Detection Contract

Date: 2026-08-18
Result: PASS / INTEGRATED INTO MAIN — REMOTE VERIFIED
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

    NEEDS_MORE_MEASUREMENT      (pre-measurement round; superseded)

SUPERSEDED by the measurement round below. The final verdict of Session 072 is
`READY`; see "MEASUREMENT ROUND M1-M4" and "REVISED DECISION REPORT". This
section is retained as the pre-measurement record and must not be read as the
session's conclusion.

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

SUPERSEDED by the M3 registry-sizing measurement below, which returns 38
derivable, 6 needing an entry and 2 needing multiple identities. The two blocks
disagree on Php — 1/4 assessed here against 3 derivable measured in M3 — and on
Python, where `pymongo` moved to the multi-identity column. M3 is MEASURED and
this block is ASSESSED, so M3 prevails wherever they differ.

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

# MEASUREMENT ROUND M1-M4

## Method

TEMPORARY MEASUREMENT ONLY, following the Session 071 Step 2 precedent.

Read-only, pinned-commit, GitHub REST only. No clone, no build, no execution, no
archive extraction, no GitHub code search, no LLM interpretation of source. No
measurement script, dataset, log or raw source was written into the repository;
`samples/validation/repositories.json` was not modified. No raw source appears in
this record.

    canonical set A   samples/validation/repositories.json, 10 repositories,
                      recorded commit SHAs, unchanged
    supplemental B    18 repositories, TEMPORARY, pinned at measurement time,
                      chosen for eight-ecosystem coverage and the named hazards
    hazard probes     4 manifest-only probes, no tree and no corpus

Pinned supplemental set:

    b01 gin-gonic/gin                    master   dcaa4296
    b02 prometheus/prometheus            main     3d36e183
    b03 psf/requests                     main     8f8b212d
    b04 pallets/flask                    main     d318b683
    b05 scikit-learn/scikit-learn        main     3b82db64
    b06 spring-projects/spring-petclinic main     88e37c15
    b07 square/okhttp                    main     a0ede49c
    b08 seanmonstar/reqwest              master   9f06fd28
    b09 launchbadge/sqlx                 main     6e57d054
    b10 slimphp/Slim                     4.x      80900fb3
    b11 guzzle/guzzle                    8.0      d1cbca76
    b12 sinatra/sinatra                  main     cb22afd7
    b13 jbogard/MediatR                  main     916ef1b3
    b14 axios/axios                      v1.x     e6824eec
    b15 vuejs/core                       main     a2b40db9
    b16 vitejs/vite                      main     c0f2fc60
    b17 expressjs/express                master   a3714473
    b18 encode/httpx                     master   b5addb64

    external probes: kubernetes/kubernetes go.mod, rails/rails Gemfile,
                     laravel/framework composer.json

Volume: 28 pinned trees, 0 truncated, 6 643 supported eligible source files,
1 383 source blobs and 194 manifest blobs retrieved, 0 retrieval failures.

The S071 collection contract was replicated exactly: regular blobs only, the
24-extension allow-list, the two excluded Gradle filenames, the 23 structural
path exclusions, 32 retrieval attempts, 64 KiB per file, 384 KiB corpus,
deterministic extension round-robin over a total candidate order, strict UTF-8
with one leading BOM stripped.

For repositories with at most 140 eligible files the whole eligible surface was
retrieved. For larger repositories a second deterministic window of 32
candidates was retrieved beyond the bounded corpus, so that inside-budget and
just-outside-budget yield could be compared without unbounded cost.

## M1 — comment and string exclusion false-positive floor

### MEASURED — naive line-level detector, no lexical exclusion

6 974 import-shaped matches across 1 370 normalized source files:

    ecosystem      files    hits    code  comment  string     FP%
    DotNet           109     333     333        0       0    0.00%
    Go               114     728     727        1       0    0.14%
    Java             132    1123    1123        0       0    0.00%
    JavaScript       317     819     804        8       7    1.83%
    Php              249    1408    1408        0       0    0.00%
    Python           246    1482    1443        0      39    2.63%
    Ruby              64      85      61       24       0   28.24%
    Rust             139     996     984       12       0    1.20%
    TOTAL           1370    6974    6883       45      46    1.30%

The dominant cause is documentation comments and docstrings carrying example
import lines. Ruby is an order of magnitude worse than every other ecosystem.

### MEASURED — lexical constructs a bounded hand-written scanner does not model

Share of that ecosystem's measured files containing the construct:

    JavaScript   regular-expression literals        7.9%
    Rust         r#"..."# raw strings              13.7%
    Python       nested-quote f-strings             6.9%
    Ruby         percent literals                  10.9%
    Ruby         heredocs                           4.7%
    Java         triple-quoted strings              0.8%
    DotNet, Go, Php                                 0.0%

### MEASURED — desynchronization, the only way a masked detector still misfires

    ecosystem     files  ends-in-open-string  col0-import-masked
    DotNet          109                    0                   0
    Go              114                    0                   0
    Java            132                    1                   0
    JavaScript      317                    0                   0
    Php             249                    0                   0
    Python          246                    0                   0
    Ruby             64                    0                   0
    Rust            139                    0                   0

One desynchronization signature across 1 370 files, in a `.groovy` artefact.
Zero column-0 import statements were ever wrongly masked, so the scanner never
lost a real import to a mis-classification.

Three files were additionally flagged by a weaker high-string-share heuristic
and were inspected individually: a `.vue` icon file that is genuinely
markup-dominated, a 26-byte `.rb` file whose single `require` argument is most
of the file, and the same `.groovy` artefact. Only the `.groovy` case was a real
desynchronization.

### MEASURED — residual false positives surviving the exclusion

One, in JavaScript: a dynamic `import('file://' + path.join(...))` whose
concatenated argument was captured by a permissive literal rule that only
required the argument to *begin* with a quote. The keyword itself was correctly
classified as code; the defect was in the call grammar, not the exclusion.

Under a strict rule requiring the parenthesised argument to be exactly one
string literal and nothing else:

    permissive require()/import() literal matches  311
    strict     require()/import() literal matches  310
    difference: the single false positive, and nothing legitimate

### M1 outcome

RESOLVED. Comment and string exclusion is deterministically achievable, it is
mandatory rather than optional, and it must be syntax-aware rather than
line-level.

Restricted to the ecosystems that became the V1 surface, the same naive floor is
1.54% over 4 358 matches — Python 2.63%, JavaScript 1.83%, Rust 1.20%, Go 0.14%,
DotNet 0.00%. The 1.30% figure above is the eight-ecosystem total and is not the
V1 figure. The exact per-language lexical obligations are now measured and are
locked by the contract. Ruby carries both the worst naive floor and the largest
unmodelled surface.

## M2 — bounded corpus reference yield

### MEASURED — declaration coverage inside the S071 bounds

    ecosystem      decls  hit in bounded   hit in bounded+window2  ambiguous
    DotNet            41      12   29.3%        13   31.7%              2
    Go               349      16    4.6%        29    8.3%              1
    Java              47      25   53.2%        25   53.2%             32
    JavaScript       438      76   17.4%       130   29.7%              0
    Php               44      14   31.8%        25   56.8%              6
    Python            64      23   35.9%        31   48.4%              0
    Ruby              44       4    9.1%         4    9.1%              0
    Rust             185      61   33.0%        81   43.8%              0
    TOTAL           1212     231   19.1%       338   27.9%             41

Go's low figure is explained by its declaration population: 283 of 349 Go
declarations are `// indirect`, which are structurally unreferenceable.

### MEASURED — what the collection budget hides

    repository            eligible  bounded  share of eligible surface collected
    vitejs/vite               1572       32    2.0%
    scikit-learn                1023       32    3.1%
    prometheus/prometheus       977       32    3.3%
    square/okhttp               654       32    4.9%
    vuejs/core                  536       32    6.0%
    gin-gonic/gin                99       32   32.3%
    khellang/Scrutor             45       32   71.1%

Three of the ten canonical validation repositories carry no eligible source at
all, reproducing the Session 071 figure.

13 supported eligible files were rejected by the 64 KiB per-file bound after
normalization, concentrated in PHP test files and large Go, Python and Rust
sources.

### M2 outcome

RESOLVED. The bounded corpus does produce useful positive evidence — 17.5% of
the V1 surface's 1 077 declarations acquire a qualifying reference inside the
bounded corpus and 26.4% with one extra window, against 19.1% and 27.9% across
all eight ecosystems — while covering as little
as 2% of a large repository's source surface. Detection is a deterministic
sample, never a census, and incompleteness is the normal state rather than a
defect. Extension round-robin did not starve any ecosystem: every supported
ecosystem produced qualifying references in the bounded window.

## M3 — identity registry size and viability

### MEASURED — attribution outcome over observed source identities

Ambiguous means one observed identity accepted by more than one declaration of
the same ecosystem:

    ecosystem     unique  ambiguous  unmatched   ambiguous share of matched
    DotNet            12          2         43         14.3%
    Go                57          1         81          1.7%
    Java               6         32        489         84.2%
    JavaScript       145          0         44          0.0%
    Php                7          6        118         46.2%
    Python            78          0        307          0.0%
    Ruby              16          0         13          0.0%
    Rust              81          0         37          0.0%

Representative ambiguity, recorded because it decides the V1 surface:

- Java: `org.springframework.boot.SpringApplication` was accepted by three
  different declared starter artifacts at once. All three attributions are
  wrong, because a starter POM ships no class and the true owner is an
  undeclared transitive artifact. No coordinate-only rule can repair this.
- Php: the root namespace `Psr` was accepted by four different `psr/*`
  declarations; `GuzzleHttp` by three different `guzzlehttp/*` declarations.
- Go: one nested-module collision, `github.com/prometheus/client_golang/exp/...`
  accepted by both the parent and the nested module.
- DotNet: `Autofac.Extensions.DependencyInjection` accepted by both `Autofac`
  and `Autofac.Extensions.DependencyInjection` under two-way prefix matching.

### MEASURED — rule repairs for the two repairable cases

    DotNet, exact equality plus own-subnamespace instead of two-way prefix:
        unique 12, ambiguous 0        (no unique match lost)
    Go, longest matching module path wins:
        unique 58, ambiguous 0        (one collision resolved, none lost)

### MEASURED — registry sizing over the canonical 46-entry population

    ecosystem     derivable  needs entry  multi-identity
    DotNet                6            0               0
    Go                    3            0               0
    Java                  1            2               1
    JavaScript           10            0               0
    Php                   3            1               0
    Python                8            2               1
    Ruby                  4            1               0
    Rust                  3            0               0
    TOTAL                38            6               2

Needing an explicit entry, 6: `com.squareup.okhttp3:okhttp`,
`org.hibernate.orm:hibernate-core`, `laravel/framework`, `psycopg2-binary`,
`scikit-learn`, `activerecord`.
Needing more than one accepted identity, 2: `org.springframework:spring-webmvc`,
`pymongo`. `pymongo` is counted only in this second column.

### M3 outcome

RESOLVED, and it resolves in an unexpected direction. Over a curated population
a registry looks small — 8 of 46. Over the observed declaration population it
does not, because the ecosystems that need it are exactly the ecosystems whose
ambiguity is structural: Java at 84.2% and Php at 46.2% would require
per-package namespace knowledge at population scale, which is neither small nor
auditable nor stable.

Therefore V1 needs **no registry at all**, provided the surface is restricted to
the ecosystems that resolve by rule with zero measured ambiguity. A registry
becomes necessary only if a later version admits Java or Php, and it must then be
a separate versioned artefact.

## M4 — stress test of the "total derivation" claims

### Go — the claim was WRONG as stated

    prometheus/prometheus   5 go.mod files, 2 replace statements, a vendor tree
    gin-gonic/gin           1 go.mod, 0 replace
    kubernetes/kubernetes   34 replace arrows (external manifest-only probe)

Nested modules break the plain prefix rule, and the collision was observed. The
repair is deterministic: longest matching module path wins, which follows Go
module semantics. Measured ambiguity then falls to zero.

Vendoring is already neutralized upstream: `vendor` is one of the 23 structural
path exclusions of S071 §8, so vendored source is never collected.

`replace` directives do not rewrite import paths, so identity matching still
holds. What they change is which code sits behind the path. That is an
epistemic caveat, not a matching failure, and it is recorded as a residual
false-positive path rather than repaired.

### JavaScript / TypeScript — the claim SURVIVES, with two named gaps

    npm alias declarations observed                        0
    repos with workspace:/file:/link: deps or workspaces    3  (b02, b15, b16)
    repos with tsconfig "paths" aliases observed            0
    subpath specifiers                                     28
    scoped specifiers                                      88
    require()/import() with a non-literal argument           5

Bare, scoped and subpath specifiers all resolve deterministically by stripping
the subpath from the head. Workspace and monorepo packages appear as ordinary
declarations and match normally, so they are not a false-positive path.

The npm alias hazard was not observed even once in this corpus, but the upstream
information loss is structural under S066 §10 and remains a guaranteed false
negative.

TypeScript `paths` aliases were not observed at all. That is an **unmeasured**
residual risk, not a cleared one, and it is recorded as such in the contract.

### Rust — the claim SURVIVES, with a confirmed evidence-surface loss

    Cargo `package =` rename declarations observed         13
    hyphenated crate names needing `-` -> `_`             120  (65 distinct)

The `-` to `_` normalization is required and is a total function. The rename
case is a confirmed, empirically observed loss: S066 §10 keeps the referenced
crate and discards the manifest-local name, while the source uses the local
name. That is a guaranteed false negative and V1 does not guess it.

### M4 outcome

RESOLVED. The word "total" does not survive for Go without an explicit
tie-breaker; it does survive for JavaScript and Rust under stated normalization.
All three repairs are deterministic and none of them is a heuristic.

---

# REVISED DECISION REPORT

## Verdict

    READY

M1 through M4 are resolved and M5 is single-valued.

## M5 — the derived V1 ecosystem surface

### MEASURED — per-ecosystem safety profile

    ecosystem     naiveFP%  unmodelled%  identities  unique  ambiguous  ambig%
    DotNet           0.00%         0.0%          57      12          0    0.0%   (exact rule)
    Go               0.14%         0.0%         139      58          0    0.0%   (longest prefix)
    JavaScript       1.83%         7.9%         189     145          0    0.0%
    Python           2.63%         6.9%         385      78          0    0.0%
    Rust             1.20%        13.7%         118      81          0    0.0%
    Ruby            28.24%        15.6%          29      16          0    0.0%
    Php              0.00%         0.0%         131       7          6   46.2%
    Java             0.00%         0.8%         527       6         32   84.2%

### Candidate comparison, in the mandated priority order

    surface                                worst ambiguity   worst naive FP   declaration coverage
    S1  Go + JavaScript                              0.0%            1.83%              92 / 787
    S2  S1 + Rust                                    0.0%            1.83%             153 / 972
    S3  S2 + Python                                  0.0%            2.63%             176 / 1036
    S4  S3 + DotNet                                  0.0%            2.63%             188 / 1077
    S5  all eight                                   84.2%           28.24%             231 / 1212

Criterion 1, false-positive safety, eliminates S5 outright: Java and Php carry
structural ambiguity that no rule repairs, and Ruby carries a naive floor an
order of magnitude above every other ecosystem together with the largest
unmodelled lexical surface and the thinnest measurement base of the eight.

Criteria 2 to 5 do not separate S1 through S4: all four are deterministic under
the same rules, share one traceability chain, need no registry and use identical
bounds.

Criterion 6, coverage, is the only criterion that separates them, and it selects
S4.

### DECISION

    V1 surface = Go + JavaScript/TypeScript + Rust + Python + DotNet
    outside V1 = Java, Php, Ruby

Ruby is excluded on measurement grounds rather than on structural grounds, and
is the most plausible candidate for a future additive version once its lexical
surface has its own measurement. Java and Php are excluded on structural
grounds.

## R1-R14 re-evaluation

    R1  VALID       inputs unchanged; no new collection surface needed
    R2  VALID       extension-to-ecosystem map used and confirmed total
    R3  REVISED     "TOTAL" is wrong for Go without the longest-prefix
                    tie-breaker; DotNet moves from PARTIAL to safe under exact
                    equality; Python's measured ambiguity was 0.0%, better than
                    the audit predicted
    R4  VALID       import-declaration detection, not usage detection
    R5  VALID       loose textual occurrence is never evidence
    R6  STRENGTHENED  "literal argument" is insufficient. The argument must be
                    exactly one string literal and nothing else; the permissive
                    reading produced the only measured residual false positive
    R7  VALID and STRENGTHENED  exclusion is mandatory, must be syntax-aware
                    rather than line-level, and needs an explicit
                    desynchronization rule
    R8  VALID       unresolved or ambiguous identity yields no observation
    R9  REVISED     no registry is required in V1. A registry is required only
                    if a later version admits Java or Php, and must then stay
                    separate from dependency-category-mapping
    R10 VALID       structurally unreferenceable classes confirmed; Go
                    `// indirect` measured at 283 of 349 Go declarations
    R11 VALID and CONFIRMED  Cargo rename observed 13 times; npm alias observed
                    zero times but structurally unchanged
    R12 VALID and EXTENDED  incomplete is not merely normal, it is the dominant
                    state; measured at 2% surface coverage for large repositories
    R13 VALID       `SourceReferenceRuleVersion` adopted
    R14 VALID       Domain owns the policy; Infrastructure loads artefacts only

Two additions that the pre-measurement report did not contain:

    R15 NEW   the evidence surface is positive-only: no unreferenced list, no
              count, no ratio and no coverage percentage may be produced or
              derived
    R16 NEW   an artefact whose lexical scan desynchronizes is unreliably
              classified, produces no observation, and marks detection
              incomplete

## Canonical contract

Created: `docs/architecture/repository-dependency-source-reference-detection.md`
CONTRACT LOCKED, version 1.

It locks the semantic ceiling; the import-declaration-only boundary; the
five-ecosystem V1 surface with Java, Php and Ruby explicitly outside; the
extension-to-ecosystem map; the qualifying syntactic forms per ecosystem; the
comment and string exclusion obligations per language with the
desynchronization rule; the strict single-literal `require()`/`import()` rule;
the identity resolution rules including the Go longest-prefix and DotNet
exact-equality tie-breakers; the no-registry-in-V1 decision and the separation
requirement for any future registry; ambiguity behavior; the structurally
unreferenceable declaration classes; positive-only evidence; availability,
completeness and absence semantics; deterministic ordering; the minimum
observation payload with snippets and hashes explicitly excluded; the
traceability chain; Domain and Infrastructure ownership;
`SourceReferenceRuleVersion` as the dedicated version owner with its bump rules;
isolation; the security boundary; and the known limitations including the
enumerated residual false-positive paths.

The contract claims **no measured false positive within the supported surface**.
It deliberately does not claim zero false positives by construction, because
module-resolution aliases configured outside the manifest were not observed and
therefore not cleared.

Introducing it requires no `AnalyzerVersion` change: it changes no collected or
normalized snapshot value.

## Independent review corrections

The contract review returned PASS WITH FINDINGS, no BLOCKER and no HIGH. All
five findings were corrected inside the locked contract without reopening any
approved decision:

- §6 and §20 now state the V1-surface measurement figures — 1.54% naive
  false-positive floor and 17.5% / 26.4% declaration coverage — and label the
  eight-ecosystem totals as such. The earlier text quoted the totals, which
  understated the V1 false-positive floor's peak and overstated its coverage.
- §10 now records that a DotNet analyzer-only or build-only package is **not
  identifiable** from the S066 declaration surface, because `PrivateAssets` and
  `IncludeAssets` are not parsed, so that one class imposes no detector
  obligation and must not be recovered by heuristic. Every other listed class —
  `@types/*` by name, Go `// indirect` by its marker, MSBuild placeholders by
  their form — stays identifiable and stays a real obligation.
- §20 now records the measured cost of the no-registry decision: `scikit-learn`,
  `psycopg2-binary` and `pymongo`'s secondary modules are permanently unmatched
  in V1. The decision is cheap, not free.
- §20 now records that the S071 §12 round-robin spends retrieval slots and
  corpus bytes on `.java`, `.php`, `.kt`, `.scala`, `.groovy` and `.rb`
  artefacts that V1 cannot use, lowering yield in polyglot repositories, and
  that narrowing collection is deliberately not done, because S071 §6, §9, §9.1
  and §12 together make collection ecosystem-blind by design.
- `worklog/INDEX.md`'s session row title was stale and now matches the outcome.

## Still unresolved after this session

- `ProducedWorkEvidence`: no contract yet establishes what observable evidence
  could satisfy it. Import-declaration evidence does not.
- `RepositoryProblemAreaEvidence`: still blocked by the Session 067 contract.
- Long-term persisted evidence payload and self-contained replay.
- The first numeric `SourceReferenceRuleVersion` value, which belongs to the
  implementing session.
- Whether Go `// indirect` and Maven `dependencyManagement` require upstream
  correction, exactly as S068 §15 left it. V1 neutralizes them for this level by
  §10 rather than correcting them upstream.
- Ruby's admission to a future version.

## Changes

Docs and worklog only.

- created `docs/architecture/repository-dependency-source-reference-detection.md`
- `docs/architecture/repository-dependency-source-reference-evidence.md` §5
  prerequisite sync, plus §4 and §15 pointer sync to the new contract
- `worklog/CURRENT.md` compaction and stale-state synchronization
- `worklog/INDEX.md` session row
- this session file

No `src/`, no `tests/`, no registry artefact, no report, no UI. No detector was
implemented.

## Validation

No build or test run applies: no production or test code was touched.

Measurement hygiene verified: `git status --short` shows no measurement
artefact, no dataset, no script and no raw source inside the repository;
`samples/validation/repositories.json` is unmodified; no secret, token or
credential was recorded.

## Not Done

- No detector implementation, no `DependencySourceReferenceEvidence` type, no
  Domain policy, no tests.
- No identity registry artefact — V1 requires none.
- Java, Php and Ruby detection.
- `ProducedWorkEvidence`, `RepositoryProblemAreaEvidence`, ProblemArea, P7.

## Next

Exact next action: implement the locked contract as a Domain policy plus its
tests, in a separate session. That session owns the first numeric
`SourceReferenceRuleVersion` value, the concrete per-language lexical scanner
including the raw-string, template-literal, regex-literal and triple-quoted
forms that §6 makes mandatory, and the desynchronization rule of §6.1.

## Git

Session base:
d480a467ec90114beea58f80e5722aa4db745c5d

Session branch commits:

    77d05f8  docs: audit dependency source reference detection feasibility
    942090a  docs: lock dependency source reference detection contract

Worklog closure commit:

    beb7f92  docs: close session 072 worklog after remote verification

**Session 072 is integrated into main and remote verified.** The transition was
a fast-forward from d480a46, so 77d05f8, 942090a and beb7f92 are all reachable
from `main`, and `main` equals `origin/main` at beb7f92.

Last Remote Verified Main is therefore beb7f92.

The session branch is retained at beb7f92, which is the repository's normal
practice: every closed session branch from 059 onward still exists on origin.
Deleting it is a user-owned Git side effect and is not pending work.

Every Git side effect in this session — commit, push and the main
fast-forward — was performed by the user.

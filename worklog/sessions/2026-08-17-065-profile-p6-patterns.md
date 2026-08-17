# Session 065 — Profile P6 Technical, Practice and Problem Patterns

Date: 2026-08-17
Result: PASS
Scope: Public Profile Evidence / Stage 7 / P6
Route: ChatGPT → Claude
Session Base: 91b3040ed6c9c9f20477ff5dcf52e9bdfd83a770
Branch: session/065-profile-p6-patterns

## Goal

Define and implement deterministic, versioned and fully traceable profile-level
repeated patterns over canonical CrossRepositoryEvidence.

Canonical direction:

RepositoryAnalysis[]
→ CrossRepositoryEvidence
→ ObservedProfileEvidence[]
→ deterministic/versioned pattern promotion
→ RepeatedProfilePattern[]
→ TechnicalFocus / EngineeringPractice / ProblemArea typed views

## Critical P1 Boundary

ObservedProfileEvidence != RepeatedProfilePattern.

A quantified observation does not automatically become a repeated pattern.

RepeatedProfilePattern owns canonical pattern truth.

TechnicalFocusArea, EngineeringPracticePattern and ProblemAreaPattern must not
become parallel conflicting stores of profile pattern truth.

## Contract-First Gate

Canonical docs intentionally defer the following exact decisions to P6:

- TechnicalFocus taxonomy
- EngineeringPractice taxonomy
- ProblemArea taxonomy
- repetition thresholds
- pattern promotion conditions
- pattern-strength semantics, if any

Claude must not invent these silently.

Before production implementation, produce a bounded:

P6 CONTRACT DECISION REPORT

The report must resolve:

1. exact V1 pattern-family and pattern-key identities
2. exact taxonomy for each family
3. exact structured evidence sources for every pattern
4. exact independent-repetition threshold
5. promotion conditions
6. supporting versus independent repository semantics
7. Partial / UnavailableEvidence / RequiresHumanReview behavior
8. deterministic ordering
9. PatternRuleVersion ownership and compatibility
10. full pattern → observation → evidence → RepositoryAnalysis traceability
11. TechnicalFocus structured language-evidence bridge
12. ProblemArea evidence requirements that prevent README/topic-only inflation
13. whether any additional P2 runtime is actually required

If canonical sources do not uniquely determine one of these decisions:

- do not mutate production code
- present bounded alternatives
- recommend the smallest safe V1
- STOP for approval

## P5 Starting State

P5 is COMPLETE / REMOTE VERIFIED.

Current P5 production truth:

- CrossRepositoryEvidence is deterministic and versioned
- RepositoryAnalysis semantics are preserved
- Strong / Present / Partial may support repository-signal observations
- UnavailableEvidence / RequiresHumanReview remain undetermined
- Missing does not become profile absence
- ProfileObservation and IndependentRepetition contribution decisions are explicit
- fork/template cannot inflate independent repetition
- ProfileEvidenceReference currently points to repository signal evidence
- ObservedProfileEvidenceKind currently contains RepositorySignalEvidence only

RepositoryAnalysis already owns RepositorySnapshot, including normalized Languages.

## Critical Technical Boundary

TechnicalFocusArea != P2 CurrentFocus.

Do not implement lifecycle activity classification or invent CurrentActivityWindow
simply because both concepts contain the word "Focus".

For TechnicalFocus, do not silently use:

- README prose
- localized presentation
- arbitrary keyword matching
- ProfileRepository.PrimaryLanguage as an untraceable shortcut

If RepositorySnapshot.Languages becomes profile evidence, preserve the canonical
traceability chain rather than bypassing it.

## In Scope

- P6 exact V1 pattern contract
- RepeatedProfilePattern
- PatternRuleVersion
- pattern-family classification
- exact bounded taxonomies
- exact evidence mappings
- deterministic promotion rules
- TechnicalFocus evidence bridge
- EngineeringPractice pattern mapping
- bounded ProblemArea rules
- repository/evidence traceability
- partial/unavailable semantics
- deterministic ordering
- rule-version coherence
- Domain/Application tests
- independent review
- worklog

## Out of Scope

- P7 RepresentativeRepository
- RoleEvidenceAlignment
- P8 DeveloperPublicEvidenceProfile assembly
- P9 validation profile set
- P10 Web Profile Flow
- Infrastructure/Web profile wiring
- JobRequirementProfile
- JobEvidenceAlignment
- developer/profile score
- candidate score
- seniority
- employability
- hire/no-hire
- candidate ranking
- LLM narration

Full P2 CurrentFocus / HistoricalEvidence / lifecycle implementation is not
automatically P6 scope.

## Safety Invariants

- no developer capability claim
- no seniority
- no employability
- no hire/reject
- no candidate ranking
- no developer aggregate score
- no hidden weighting
- no hidden clock
- no LLM before structured truth
- no README-keyword-only technical/problem pattern
- one repository cannot establish repeated pattern
- fork/template cannot inflate independent repetition
- Undetermined cannot silently become support
- Missing cannot become developer absence
- technical failure cannot become negative evidence
- locale cannot alter semantic truth
- input order cannot alter semantic truth
- same normalized inputs + same versions must reproduce the same patterns

## Implementation Gate

Production implementation begins only after the P6 contract decisions are
explicitly reviewed and approved.

Recommended canonical contract owner if a new document is required:

docs/architecture/public-profile-evidence-patterns.md

Do not silently rewrite P1 or P2 contracts.

## Validation After Implementation

Required at minimum:

- targeted Domain tests
- targeted Application tests
- taxonomy mapping tests
- promotion threshold tests
- single-repository non-promotion tests
- independent repetition tests
- fork/template inflation tests
- Partial / unavailable tests
- traceability tests
- deterministic ordering tests
- mixed input-order tests
- rule-version mismatch/coherence tests
- full solution regression

Independent review must leave no BLOCKER or HIGH findings.

## User-Owned Git Boundary

Claude must not:

- commit
- push
- merge
- change Git identity

User owns all Git finalization.

## Claude Routing

Model: opus
Effort: xhigh

## P6 Contract Decision Report

No production code was mutated. This report resolves the 13 required decisions
or presents bounded alternatives where canonical docs do not determine one.

### A. What canonical docs already determine

- `ObservedProfileEvidence` != `RepeatedProfilePattern`; promotion requires a
  deterministic versioned rule (domain model §10).
- Exactly three pattern families: TechnicalFocus, EngineeringPractice,
  ProblemArea. `RepeatedProfilePattern` owns canonical pattern truth; the three
  families are typed projections, never parallel stores (domain model §10).
- Required traceability: pattern → ObservedProfileEvidence[] →
  ProfileEvidenceReference[] → RepositoryAnalysisReference[] → repository
  evidence, plus the producing rule/version identity (domain model §10, §15).
- Repetition across more than one independent eligible repository is the
  distinguishing mechanism; a single README keyword is explicitly not a pattern
  (product contract §9, roadmap P6).
- README keyword matching alone cannot produce technical-focus or problem-area
  truth (product contract §10, §12, roadmap P6).
- No capability, seniority, employability, ranking, score or hidden weighting;
  deterministic, locale-independent, no hidden clock.
- Fork/template lineage cannot inflate independent repetition (eligibility
  contract §19, already enforced by P5).

### B. What canonical docs deliberately leave open

Product contract §27 explicitly does not lock: technical-focus taxonomy,
problem-area taxonomy, repetition thresholds, confidence calculation. These
must be decided by a versioned domain/architecture contract, which is what this
report proposes.

### C. Available structured evidence today

- P5 `ObservedProfileEvidence`, keyed by the 15 canonical repository signal
  keys, with per-repository `ProfileEvidenceReference` (signal key + evidence
  item keys) and supporting / independent-supporting / eligible / undetermined
  counts.
- `RepositoryAnalysis.Snapshot.Languages`: normalized GitHub linguist byte
  counts, with a `languages` availability entry. Structured, locale
  independent, not prose.
- `RepositoryAnalysis.Snapshot.Topics`: user-authored free-form strings.
- `ObservedProfileEvidenceKind` currently has only `RepositorySignalEvidence`,
  and `ProfileEvidenceReference` currently requires a signal key.

### D. Decision 1 — EngineeringPractice (determinable now)

Proposed V1 taxonomy, mapped only from canonical signal keys, using the example
areas named by product contract §11:

| Pattern key | Supporting signal keys |
|---|---|
| `practice.automated-testing` | `validation.test-structure` |
| `practice.continuous-integration` | `validation.continuous-integration` |
| `practice.documentation` | `documentation.installation-guide`, `documentation.usage-or-demo` |
| `practice.release-management` | `process.release-usage` |
| `practice.issue-tracking` | `process.issue-usage` |
| `practice.pull-request-workflow` | `process.pull-request-usage` |

Multi-signal rule: a repository supports the practice when at least one mapped
observation lists that repository (union, no weighting, no counting twice).

Deliberately excluded from V1: `process.commit-history` and
`process.commit-distribution` (weakest practice claim, not named by §11), and
all presentation/metadata signals (`repository.readme.exists`,
`repository.description.present`, `repository.license.present`,
`portfolio.topics`, `portfolio.homepage-or-live-demo`,
`portfolio.presentation-completeness`).

### E. Decision 2 — Repetition threshold (recommended)

`MinimumIndependentRepositories = 2`, counted only from repositories whose P5
`IndependentRepetition` contribution is Allowed.

Rationale: product contract §9 and §11 say "more than one independent eligible
repository"; the session invariants say one repository cannot establish a
repeated pattern. Two is the smallest value consistent with those statements,
so it is the least invented threshold available.

Undetermined independence never counts toward promotion and never blocks it; it
is reported alongside the pattern. Supporting-but-not-independent repositories
stay visible as context and do not contribute to the threshold.

### F. Decision 3 — TechnicalFocus requires a P5 evidence extension

TechnicalFocus cannot be produced from today's evidence model. The only
admissible structured technical evidence is
`RepositoryAnalysis.Snapshot.Languages`, and P5 observations can currently only
be keyed by signal keys. `RepositoryIdentity.PrimaryLanguage` is rejected: it is
the untraceable shortcut the session forbids.

Bounded alternatives:

- **F-A (recommended if TechnicalFocus is wanted in V1):** extend P5 with
  `ObservedProfileEvidenceKind.RepositoryLanguageEvidence` and generalize
  `ProfileEvidenceReference.SignalKey` into an evidence-source key so language
  observations are representable. Cost: a bounded modification of the already
  committed P5 contract plus composer and test updates. Traceability becomes
  pattern → observation → repository + analysis reference + `languages` data
  key. It is one link weaker than signal evidence because language data has no
  `EvidenceItem` keys.
- **F-B:** ship P6 V1 without TechnicalFocus and keep the family declared but
  unimplemented until the evidence bridge is contracted separately.

Language qualification rule, if F-A is approved: a repository supports a
language focus only for its dominant language by byte count (all languages tied
at the maximum qualify). This avoids inventing a percentage threshold. A share
based rule (for example >= 10%) is the alternative and would cover polyglot
repositories at the cost of an invented constant.

Language identity: one pattern key per normalized linguist language, for
example `technical-focus.language.c-sharp`. No framework grouping. The
product's "C# / .NET" phrasing would require an invented framework taxonomy and
is deliberately not implemented.

If the `languages` availability entry is not Available for a repository, its
language evidence is Undetermined, never absent.

### G. Decision 4 — ProblemArea has no admissible evidence source

No structured evidence in the current pipeline maps to problem areas. Topics are
user-authored strings, so promoting them is keyword matching under another name,
which product contract §12 and the roadmap forbid as a sole source. File-tree or
dependency derived problem evidence would require new repository-level signals,
which is Repository MVP contract scope, not P6.

Recommended: declare ProblemArea **Deferred** in the P6 contract with this exact
reason, and implement no ProblemArea production code in V1.

Alternative, not recommended: a closed topic allow-list requiring >= 2
independent repositories and co-occurring structured signal evidence. It remains
keyword-derived and risks exactly the inflation the contract prohibits.

### H. Decisions 5-13 — resolved without new invention

- **Promotion conditions:** a pattern is promoted when its independent
  supporting repository count >= threshold. No other condition, no weighting,
  no score. Otherwise no pattern exists (absence of a pattern is not a negative
  claim and is not stored as one).
- **Supporting versus independent:** both counts are carried; only the
  independent count gates promotion.
- **Partial / UnavailableEvidence / RequiresHumanReview:** inherited from P5
  unchanged. Partial analyses may support through evidence actually available;
  unavailable and human-review statuses stay undetermined counts and can never
  support or block.
- **Deterministic ordering:** patterns ordered by (family, pattern key)
  ordinal; supporting observations by observation key; repositories by
  `ProfileRepositoryReference`. Same convention as P5.
- **PatternRuleVersion:** new Domain value object owned by P6. A pattern set
  records the `PatternRuleVersion` plus the consumed `ObservationRuleVersion`,
  `EligibilityPolicyVersion` and `AnalysisVersion`, and rejects mixed versions
  exactly as `CrossRepositoryEvidence` does. Any taxonomy or threshold change
  requires a new `PatternRuleVersion`.
- **Traceability:** each `RepeatedProfilePattern` references its supporting
  `ObservedProfileEvidence` results; the observation layer already guarantees
  the rest of the chain. Structurally validated, not documented only.
- **Pattern strength:** none in V1. Confidence calculation is unlocked in the
  product contract and would be a capability-adjacent claim.
- **Additional P2 runtime:** none. TechnicalFocus is not P2 CurrentFocus; no
  lifecycle or activity classification and no `CurrentActivityWindow` is
  required.
- **Canonical owner:** a new document,
  `docs/architecture/public-profile-evidence-patterns.md`, authored before
  production code. P1 and P2 contracts are not rewritten.

### I. Blocking approvals required before implementation

1. EngineeringPractice taxonomy in section D, including the deliberate
   exclusions.
2. Repetition threshold of 2 independent repositories (section E).
3. TechnicalFocus: F-A (extend P5 evidence model) or F-B (defer), and if F-A,
   the dominant-language rule versus a share threshold.
4. ProblemArea deferral (section G).

## Approved Decisions (user, after report review)

1. EngineeringPractice V1 approved with keys: automated-testing,
   continuous-integration, documentation, release-usage, issue-usage,
   pull-request-usage. Documentation may only be supported by
   `documentation.installation-guide` and/or `documentation.usage-or-demo`.
   commit-history and commit-distribution stay out of V1. Semantic strength must
   not exceed the source evidence.
2. Threshold approved: `IndependentSupportingRepositoryCount >= 2`, counted only
   from P2/P5 `IndependentRepetition = Allowed` repositories. Undetermined is not
   support, is not negative evidence, and does not block a pattern that already
   has two or more certain independent supports.
3. TechnicalFocus F-A approved. Canonical source is
   `RepositoryAnalysis.Snapshot.Languages`; V1 emits dominant-language evidence
   only (positive byte count, repository maximum, exact ties share dominance).
   No semantic grouping: C# is not .NET, JavaScript is not Frontend, Python is
   not AI/ML. The language literal stays canonical evidence. If
   `ProfileEvidenceReference` must be generalized beyond signal keys, preserve
   the single canonical evidence-reference ownership from P1; no competing
   parallel reference truth.
4. ProblemArea deferral not yet approved; a bounded feasibility addendum was
   requested first.

## Problem Area Feasibility Addendum

Scope: inspect only the existing normalized structured evidence surface. No
README keyword matching, no topic-only truth, no new repository signal, no
unsupported semantic leap from raw evidence.

### 1. Exact evidence surface available to the profile layer

`RepositoryAnalysis.Snapshot` exposes:

- `FileTree`: recursive path plus size for every entry, `IsComplete = false`
  when GitHub truncates the tree. Tree entries carry no content, except that the
  snapshot orchestrator overlays the selected project files described below.
- Project files collected **with content**, and only these:
  `*.csproj`, `pom.xml`, `build.gradle`, `build.gradle.kts`, `pytest.ini`,
  `package.json`, `Directory.Packages.props`, `jest.config.*`, `vitest.config.*`
  (`GitHubRepositoryContentsCollector.IsRelevantProjectFile`).
- `Workflows`: `.github/workflows/*.yml|yaml` with content.
- `DocumentationMarkdown`: `docs/*.md` — prose, excluded by rule.
- `Topics`: user-authored free-form strings.
- `Languages`: linguist byte counts (already claimed by TechnicalFocus).
- `License`, `Metadata` (description is prose), `Commits`, `Issues`,
  `PullRequests`, `Releases`, `Tags`, `Contributors` — process and metadata
  facts, none of which identify a problem or application area.

### 2. Candidate areas from product contract §12, assessed against that surface

| Candidate | Determinable today? | Blocking reason |
|---|---|---|
| backend application development | No | Requires SDK/framework interpretation of `*.csproj` or `package.json` dependencies |
| HTTP / REST API | No | Same dependency interpretation, plus no evidence outside .NET/Java/JS |
| persistence / data access | No | Only derivable from package references (EF Core, Npgsql, prisma, mongoose) |
| domain modeling | No | No structured determinant of any kind exists |
| authentication / authorization | No | Package references only; scaffolded templates declare the same packages |
| integrations | No | No bounded determinant exists |
| ML experimentation | No | Python manifests are not collected at all (`requirements.txt`, `pyproject.toml`, `environment.yml` are outside the selection set). `*.ipynb` exists only as a path and notebooks are not ML-specific |
| containerization | Path-determinable | `Dockerfile` / `docker-compose.y*ml` appear in the file tree, but see §4 |

### 3. Two structural obstacles that block a V1 taxonomy

**O1 — Ownership.** Interpreting file paths and project-file contents into
semantic facts is exactly what repository-level signal evaluators already do;
`TestStructureSignalEvaluator` walks `FileTree` and parses `*.csproj` XML, then
exposes the conclusion as a `SignalResult` with `EvidenceItem` keys. The profile
layer sees signal results, not dependency facts. Re-parsing manifests inside P6
would duplicate repository-level interpretation, which the P4/P5 architecture
boundary forbids, and producing the same conclusions properly would mean a new
repository signal — explicitly excluded from this session.

**O2 — Ecosystem-bounded manifest surface.** Dependency content exists only for
.NET, Java and JavaScript. Python, Go, Rust, PHP, Ruby and C++ repositories
contribute no manifest content whatsoever. A dependency-derived taxonomy would
therefore produce a systematically different problem-area surface per stack.
That is not a negative developer claim, but it is an incoherent evidence surface
and a hidden stack bias that no rule version can justify.

### 4. Why containerization does not rescue V1

`Dockerfile` presence is determinable from tree paths without content parsing and
without ecosystem bias, so it is technically feasible. It is nevertheless
rejected for V1:

- The honest claim it supports is "container packaging configuration is
  visible", which is a tooling/practice statement, not a problem or application
  area. Filing it under ProblemArea would make the family mean something other
  than what product contract §12 describes.
- A one-key ProblemArea taxonomy whose single key is really a practice would
  lock a misleading family semantic into `PatternRuleVersion` V1.

### 5. False-positive boundary that cannot currently be drawn

Every candidate above proves **declaration**, not **production**: a declared
package or a scaffolded file is identical in structured evidence whether the
developer built the capability or generated the project. Fork and template
lineage are excluded by P2 only through repository lineage, so two scaffolded
non-fork repositories would still satisfy the two-repository threshold. No
structured evidence in the current surface separates declared from built, which
is precisely the inflation product contract §12 and roadmap P6 forbid.

### 6. Verdict

A small, high-confidence, explainable V1 ProblemArea taxonomy is **not possible**
from the current structured evidence. Recommended: explicit deferral.

`ProblemArea` stays a contracted pattern family with no V1 promotion rule and no
production code path that can emit it. Its absence is recorded as a contract
boundary, never as evidence of absence.

### 7. Conditions that would make ProblemArea feasible later

All four are required, and none is P6 scope:

1. A repository-level signal that emits framework/dependency category evidence
   with `EvidenceItem` keys, so profile aggregation consumes an interpreted
   result instead of re-parsing manifests.
2. A broadened manifest collection set covering the major ecosystems, so the
   evidence surface stops being stack-biased.
3. A versioned dependency-to-area mapping owned by a canonical contract.
4. A structured boundary that distinguishes declared dependencies from produced
   work, so scaffolded repositories cannot satisfy the repetition threshold.

Note: the `ProfileEvidenceReference` generalization approved for TechnicalFocus
(F-A) is the same mechanism that would later carry file-tree or dependency
evidence, so deferring ProblemArea costs nothing structurally.

## ProblemArea Decision (user)

Explicit deferral approved. ProblemArea stays a canonical family; P6 V1 defines
no taxonomy, no promotion rule and no production path that can emit it. An empty
ProblemArea collection is a contract boundary, never negative evidence or
developer absence. Containerization is moved neither into ProblemArea nor into
the EngineeringPractice taxonomy in this session.

## Implementation Result

Canonical contract authored: `docs/architecture/public-profile-evidence-patterns.md`.

Minimum cross-references added: the P6 subsection of the domain-model contract
and the P6 roadmap section now name that document. No existing canonical
decision was rewritten.

Canonical decisions realized in code:

- P6 promotion lives in Domain (`ProfilePatternPromotionPolicy`), consuming
  `CrossRepositoryEvidence` and producing `ProfilePatternSet`. No Application or
  Web logic owns pattern semantics.
- EngineeringPractice taxonomy is bound to canonical signal-key constants rather
  than duplicated string literals. Documentation unions its two mapped signals
  and counts each repository once.
- The threshold is enforced twice: `RepeatedProfilePattern` rejects fewer than
  two independent supporting repositories structurally, and the policy filters
  before construction. Only `IndependentRepetition = Allowed` repositories are
  counted, so fork and template lineage cannot inflate repetition.
- TechnicalFocus consumes `Snapshot.Languages` only. Dominance requires a
  positive byte count and the repository maximum; exact ties share dominance.
  The pattern key is the preserved language literal, with no grouping, aliasing
  or framework inference.
- P5 was generalized only as far as F-A required: `ObservedProfileEvidenceKind`
  gained `RepositoryLanguageEvidence` and `ProfileEvidenceReference.SignalKey`
  became `EvidenceKey`. One canonical evidence-reference type still owns
  traceability; no parallel reference truth was created.
- `ProfileEvidenceKeys` owns the observation key vocabulary in Domain, so the
  Application composer and the Domain policy share one key contract.
- Unavailable language data leaves every language observation undetermined for
  that repository instead of asserting absence. Available-but-empty language
  data is a determinate absence of language evidence and is neither support nor
  undetermined.
- ProblemArea has no promotion path; `ProfilePatternSet.ProblemAreaPatterns` is
  a projection that stays empty under this rule version.
- No pattern strength, confidence, weighting, score or clock exists.

## Validation

- `dotnet test GitHubProof.slnx`: 831/831 PASS
  (Domain 520, Application 94, Infrastructure 149, Web 68).
- Build: 0 warning / 0 error.
- `git diff --check`: clean.
- Session validation checklist covered: taxonomy mapping, promotion threshold,
  single-repository non-promotion, independent repetition, fork/template
  inflation, unavailable and empty language data, traceability, deterministic
  ordering, mixed input order, rule-version coherence, full regression.

## Independent Review

`github-proof-review`, forked context. Verdict: PASS WITH FINDINGS, no
BLOCKER/HIGH. Approved decisions 1-4 were each verified against the code and the
new contract.

Accepted findings, all fixed in this session:

- worklog state contradicted the working tree (this finalization),
- `worklog/CURRENT.md` had gained a UTF-8 BOM (removed),
- the composer-to-promotion seam had no integration test (added:
  `ComposedLanguageEvidence_PromotesIntoTechnicalFocus`),
- two excluded signals were not asserted in the exclusion theory (added).

## Remaining Risk

- ProblemArea remains deferred. Its four future conditions are recorded in the
  patterns contract §9.4.
- `RepeatedProfilePattern` cannot structurally prove that its observations came
  from one evidence surface, because the pattern carries no reference to that
  surface. The promoting policy guarantees it today; P8 assembly will hold both
  sides at once.
- `CrossRepositoryEvidenceComposer` keeps signal and language observations in one
  keyed map. A key collision would silently reuse the first observation kind.
  Unreachable today because canonical signal keys never start with `language.`.
- Presentation must not regroup language literals later; the product contract's
  "C# / .NET" illustration is presentation only and contract §8.4 forbids the
  grouping in canonical truth.
- `IProfileRepositorySnapshotSource` still has no Infrastructure adapter, so the
  profile pipeline remains library-only.

## Current Status

READY_FOR_MANUAL_COMMIT

# Session 066 — Repository Evidence Surface Expansion / ProblemArea Evidence Foundation

Date: 2026-08-17
Result: PASS
Scope: repository evidence architecture / collection / profile evidence foundation
Model Route: Claude Opus / xhigh
Model Gate: APPROVED

## Goal

Define the smallest safe, deterministic, versioned and ecosystem-neutral
repository evidence surface required to support future ProblemArea truth
without silently changing Repository MVP V1's canonical 15-signal scoring
contract.

This session exists between Profile P6 and P7.

P6 remains complete.

P7 must not begin until this evidence-foundation decision is resolved.

## Starting State

Branch:
session/066-repository-evidence-surface-expansion

Session Base:
cbde2e51c1814829ba737acd5f574f866d78b8eb

Previous Session:
065 — Profile P6 Technical, Practice and Problem Patterns — PASS / REMOTE VERIFIED

P6 canonical contract:
docs/architecture/public-profile-evidence-patterns.md

## Why This Session Exists

P6 deliberately deferred ProblemArea because the current structured repository
evidence surface cannot produce reliable and ecosystem-neutral problem-area
truth.

Known limitations include:

- dependency/content collection is uneven across ecosystems,
- profile-level parsing must not duplicate repository-level interpretation,
- declared dependency is not equivalent to produced work,
- Repository MVP V1 currently owns exactly 15 scoring signals,
- new ProblemArea-supporting evidence must not silently become additional
  scoring signals.

## Mandatory First Deliverable

Before any production code or canonical docs mutation, produce:

EVIDENCE SURFACE CONTRACT DECISION REPORT

Then STOP for user approval.

No production implementation may begin before that report is approved.

## Contract Questions

The report must resolve:

1. Ownership model

Evaluate explicitly:

A — new framework/dependency evidence becomes new Repository MVP scoring signals

B — new framework/dependency evidence becomes a separate non-scoring structured
repository evidence surface

C — an existing canonical repository evidence model can be safely generalized

Do not choose without inspecting canonical docs and current implementation.

Repository MVP V1 remains exactly 15 scoring signals unless an explicit,
versioned product-contract decision changes it.

2. Ecosystem coverage

Evaluate at minimum:

- .NET
- Python
- Java
- JavaScript / TypeScript
- Go
- Rust
- PHP
- Ruby

Determine exact manifest/config surfaces that are available or would need
collection.

Do not call a design ecosystem-neutral merely because several ecosystems are
listed.

The report must define bounded, testable acceptance criteria for what
ecosystem-neutral means for this product.

Unsupported ecosystems or unavailable manifests must never become negative
developer evidence.

3. Interpretation ownership

Determine where raw path, manifest, dependency and framework information becomes
semantic structured evidence.

Profile/P6 layers must not duplicate repository-level parsing or interpretation.

4. Availability and completeness

Define:

- unavailable != absent,
- incomplete/truncated != negative evidence,
- partial collection behavior,
- deterministic collection bounds,
- how unsupported manifest surfaces are represented.

5. Declared versus produced boundary

A dependency declaration, scaffold file or configuration presence must not
silently become proof that a problem area was meaningfully produced.

The report must define the maximum semantic strength available from each
proposed evidence type.

Do not invent a produced-work inference if the current evidence cannot support
one.

6. Traceability

Future traversal must remain possible:

ProblemArea
→ RepeatedProfilePattern
→ ObservedProfileEvidence
→ ProfileEvidenceReference
→ RepositoryAnalysisReference
→ RepositoryAnalysis
→ canonical repository-level structured evidence

No parallel competing evidence truth.

7. Versioning

Determine whether the new surface:

- belongs to RepositoryAnalysis versioning,
- requires an independent evidence-surface/rule version,
- or safely reuses an existing canonical version owner.

No unversioned semantic rule.

8. Compatibility and blast radius

Evaluate A, B and C against:

- Repository MVP V1 15-signal scoring
- score normalization
- RepositoryAnalysis
- repository report schema
- presentation/localization
- deterministic replay
- validation repository set
- existing tests
- Infrastructure collection
- profile P5/P6 traceability

Explicitly identify what remains unchanged for each viable option.

## Hard Safety Boundaries

- Do not silently create signal 16+.
- Do not modify repository score semantics without explicit contract approval.
- Do not parse manifests in the profile layer.
- Do not use README keyword matching.
- Do not use topic-only truth.
- Do not infer developer capability, seniority, employability or hire outcome.
- Do not treat unsupported ecosystem coverage as absence.
- Do not treat unavailable collection as absence.
- Do not treat declaration as produced-work evidence.
- Do not introduce hidden weighting.
- Do not introduce an LLM into canonical evidence truth.
- Preserve deterministic ordering and replay.
- Preserve full evidence traceability.

## In Scope After Contract Approval

Only if explicitly approved:

- bounded repository evidence-surface contract,
- necessary collection-surface expansion,
- ecosystem-neutral manifest/config coverage,
- structured non-prose repository evidence,
- availability/completeness semantics,
- deterministic/versioned repository interpretation,
- profile evidence bridge required for later ProblemArea,
- Domain/Application/Infrastructure tests where required,
- regression validation,
- independent review.

## Out of Scope

- ProblemArea taxonomy itself
- ProblemArea promotion implementation
- P7 Representative Repository
- P8 profile assembly
- P9 profile validation
- P10 Web flow
- Infrastructure/Web profile wiring unrelated to this evidence surface
- Job Evidence Alignment
- new repository scoring semantics unless separately approved
- LLM narration
- developer/candidate scoring
- seniority/employability/hire/ranking inference

## Stop Conditions

STOP before mutation when:

- canonical sources do not uniquely determine an architectural decision,
- a proposed solution changes the 15-signal scoring contract,
- ecosystem-neutrality cannot be bounded,
- declared-vs-produced semantics require a new product decision,
- a new semantic rule requires contract approval,
- scope would expand into ProblemArea taxonomy or P7.

When blocked, provide bounded alternatives and the smallest safe recommendation.

## Evidence Surface Contract Decision Report

No production code and no canonical docs were mutated for this report.

### 0. Inspected sources

Canonical: `docs/architecture/analysis-domain-model.md` §4 (RepositorySnapshot
field list, content-commit pinning, bounded docs corpus policy),
`docs/product/analysis-categories-and-signal-boundaries.md` §6 (the exact
15-signal list), §7 (prohibited inferences), §9 (signal acceptance criteria),
`docs/product/scoring-and-report-contract.md` §6 (RawApplicableScoreNormalization),
§16 (version fields), `docs/product/validation-repository-set.md` (acceptance
thresholds), `docs/architecture/public-profile-evidence-patterns.md` §9.

Code: `GitHubRepositoryContentsCollector` (selection sets and bounds),
`GitHubRepositorySnapshotOrchestrator.MergeFileTree`, `RepositorySnapshot`,
`RepositoryAnalysisComposer` (canonical key validation, `DetermineStatus`,
`CreateWarnings`, `CreateLimitationKeys`), `TestStructureSignalEvaluator`
(existing manifest parsing precedent), `RepositoryReportProjector`,
`CrossRepositoryEvidenceComposer` plus `ProfileEvidenceKeys` (existing
non-signal evidence bridge).

### 1. Ownership decision — recommend B

**A — new scoring signals: rejected on canonical grounds.**

- `analysis-categories-and-signal-boundaries.md` §9 criterion 4 requires every
  MVP signal to produce an actionable improvement suggestion for the developer.
  Dependency or framework presence produces no honest recommendation; "declare
  another package" is not repository-improvement advice.
- §6 fixes the 15-signal list, and `scoring-and-report-contract.md` §6 computes
  the total from the applicable maximum of scored signals. A sixteenth scoring
  signal changes the denominator for every repository, so every existing score
  moves. That requires a new `RuleSetVersion`, a new `ReportSchemaVersion`,
  localization for the new signal in every locale, and re-running the validation
  repository set against its acceptance thresholds.
- Code-level cost: `RepositoryAnalysisComposer` validates the exact canonical key
  set, and `SignalPresentationLocalizerTests` plus `RepositoryAnalysisComposerTests`
  pin the count of 15.

**C — generalizing an existing model: necessary but not sufficient.**

Broadening `GitHubRepositoryContentsCollector.IsRelevantProjectFile` is required
work, but it only widens raw collection. It leaves interpretation homeless: the
only component that turns manifests into semantic facts today is a signal
evaluator, and the profile layer is forbidden from parsing. C is therefore a
sub-step of B, not an alternative to it.

**B — separate non-scoring structured repository evidence surface: recommended.**

`RepositorySnapshot.Languages` is the existing precedent: normalized provider
data, carried on the snapshot, with its own `languages` availability key, no
scoring participation, and already consumed by the profile layer through
`ProfileEvidenceKeys` with full traceability. A declared-dependency surface can
follow exactly that shape.

Concretely: Infrastructure collects and normalizes dependency manifests into a
new snapshot field with dedicated availability, and no signal, score, category
or report element consumes it in this session.

### 2. Two silent-drift traps that constrain any B implementation

These come from current code and are the reason B is only safe under explicit
conditions:

1. **File-tree overlay.** `GitHubRepositorySnapshotOrchestrator.MergeFileTree`
   overlays collected project files into `RepositorySnapshot.FileTree` with
   content. Signal evaluators read that tree, so adding new manifests to the
   same overlay would silently change existing signal inputs, notably
   `validation.test-structure`. The new surface must therefore be a separate
   snapshot field and must not enter the file-tree overlay.
2. **Collection warnings.** `RepositoryAnalysisComposer.DetermineStatus` turns
   any non-empty `snapshot.CollectionWarnings` into `AnalysisStatus.Partial`,
   and `CreateLimitationKeys` emits `limitation.snapshot.collection-partial`.
   If the new collector pushes warnings into that list, previously `Completed`
   analyses become `Partial`, which propagates into P5 contribution decisions.
   The new surface must carry its own availability and completeness state and
   must stay out of `CollectionWarnings` in V1.

### 3. Ecosystem coverage

Currently collected with content: `*.csproj`, `Directory.Packages.props`,
`pom.xml`, `build.gradle`, `build.gradle.kts`, `package.json`, `pytest.ini`,
`jest.config.*`, `vitest.config.*`. Dependency declarations therefore exist only
for .NET, Java and JavaScript.

Proposed V1 manifest surface, one per ecosystem, dependency-declaring only:

| Ecosystem | Manifests |
|---|---|
| .NET | `*.csproj`, `*.fsproj`, `*.vbproj`, `Directory.Packages.props` |
| Java / JVM | `pom.xml`, `build.gradle`, `build.gradle.kts` |
| JavaScript / TypeScript | `package.json` |
| Python | `pyproject.toml`, `requirements*.txt` |
| Go | `go.mod` |
| Rust | `Cargo.toml` |
| PHP | `composer.json` |
| Ruby | `Gemfile`, `*.gemspec` |

Bounded acceptance criteria for calling this ecosystem-neutral, all testable:

1. every covered ecosystem has at least one collected manifest that declares
   third-party dependencies;
2. all ecosystems normalize into one shape with no ecosystem-specific field:
   ecosystem identity, declared package identity, declared version literal when
   present, and the source manifest path;
3. no ecosystem is represented only by a test-tool or build-tool config while
   another ecosystem's dependency manifest is collected;
4. an ecosystem outside the covered set is represented as a not-applicable
   coverage state, never as empty dependency facts;
5. a per-ecosystem collection test matrix asserts identical normalized output
   semantics for each manifest kind.

Collection bounds mirror the existing docs-corpus policy in
`analysis-domain-model.md` §4 rather than inventing a new policy shape: a
maximum manifest count, a per-file decoded byte limit, a total corpus budget,
and Ordinal candidate ordering, with bound exhaustion producing incomplete
rather than unavailable.

### 4. Interpretation ownership

Two steps, deliberately split:

- **Format normalization** (this session, if approved): Infrastructure parses
  manifest file formats into the normalized shape above. This matches how
  languages, topics and license metadata are already normalized, and it keeps
  one parser per format in one place.
- **Semantic categorization** (not this session): mapping declared packages to
  problem areas requires the versioned mapping contract named in
  `public-profile-evidence-patterns.md` §9.4 condition 3.

The profile layer parses nothing in either step. It reads normalized facts from
the analysis snapshot exactly as P6 reads `Languages`.

### 5. Availability and completeness

- Dedicated availability key, `dependency-manifests`, alongside the existing
  keys; unavailable never means absent.
- Bound exhaustion or tree truncation produces incomplete-but-usable evidence,
  never negative evidence.
- An ecosystem with no manifest present in a complete tree is a determinate
  "no declaration found" for that ecosystem, which is still not developer
  absence.
- Repositories whose ecosystem is outside the covered set are represented as
  not-applicable coverage, so consumers cannot read silence as absence.

### 6. Declared versus produced ceiling

Maximum honest semantic strength of this surface:

> This repository's manifest at path P declares dependency D for ecosystem E at
> snapshot time.

It is explicitly not: uses, integrates, implements, built, understands, or is
proficient in D. Scaffolds and templates declare identical dependencies.
`analysis-categories-and-signal-boundaries.md` §7 already forbids "this
developer really built this project" style inference, so no produced-work
inference is proposed here. The declared-versus-produced boundary demanded by
patterns §9.4 condition 4 remains unresolved and stays out of scope.

### 7. Traceability

The chain already exists and needs no new mechanism:

ProblemArea (future) → RepeatedProfilePattern → ObservedProfileEvidence →
ProfileEvidenceReference (`EvidenceKey` = `dependency-manifests`) →
RepositoryAnalysisReference → RepositoryAnalysis → snapshot dependency facts.

`ProfileEvidenceReference.EvidenceKey` and `ObservedProfileEvidenceKind` were
generalized in Session 065 exactly for non-signal evidence, so a future kind can
be added without a parallel evidence truth.

### 8. Versioning

The surface is normalized collection data, so it belongs to the existing
`RepositoryAnalysis` version identity and needs no new version type in this
session. The collected manifest set and bounds become part of the canonical
`analysis-domain-model.md` §4 contract, so changing them is a documented
contract change, not a silent behaviour change.

A new rule version becomes mandatory only when semantic categorization is
introduced, and that version belongs to the future mapping contract, not here.

### 9. Blast radius

Unchanged under B with the §2 conditions: the 15 signals, every signal result,
score normalization, `ScoreBreakdown`, `AnalysisStatus`, report schema,
localization, presentation, validation repository set expectations, deterministic
replay of existing analyses, and all P5/P6 results.

Changed under B: `RepositorySnapshot` gains one field and one availability key,
`analysis-domain-model.md` §4 gains the field plus the collection policy, and
Infrastructure gains one collector with tests.

Changed under A: score denominator, every stored score, `RuleSetVersion`,
`ReportSchemaVersion`, localization for all locales, validation set re-run,
and the two pinned signal-count tests.

### 10. Smallest safe V1 proposal

1. Broaden manifest collection to the ecosystem table in §3, as a **separate**
   collection, not merged into the file-tree overlay.
2. Normalize into one ecosystem-neutral declared-dependency shape on
   `RepositorySnapshot`, with a dedicated availability key and completeness
   flag, and no entries in `CollectionWarnings`.
3. Amend `analysis-domain-model.md` §4 with the field, the manifest set, the
   bounds and the availability semantics.
4. No signal, no score, no report change, no categorization, no ProblemArea
   taxonomy, no profile wiring.

### 11. Blocking approvals required

1. Ownership decision B, including the two §2 conditions that keep existing
   analyses byte-identical.
2. The §3 ecosystem manifest set and the five neutrality acceptance criteria.
3. The §5 availability model, including not-applicable coverage for uncovered
   ecosystems.
4. The §6 declared-only semantic ceiling, with produced-work inference left
   unresolved.
5. That format normalization lives in Infrastructure while semantic
   categorization stays deferred to a future versioned contract.

## Approved Decisions and Amendments (user)

Ownership: **B approved**. Framework/dependency evidence never joins the 15
scoring signals. A rejected. C is only the collection/model expansion sub-step
of B.

Approved constraints:

1. Collected manifests never enter the `FileTree` content overlay, so existing
   signal evaluator inputs cannot change.
2. The surface's unavailable/partial states never reach
   `RepositorySnapshot.CollectionWarnings` and therefore never flip
   `RepositoryAnalysis.Status` to `Partial`. The surface carries its own
   availability and completeness semantics.
3. Ecosystem coverage direction: .NET, Python, Java, JavaScript/TypeScript, Go,
   Rust, PHP, Ruby. The canonical contract must lock the exact manifest
   patterns, candidate ordering and file-count/per-file/total-byte bounds.
4. Format normalization is Infrastructure-owned and may normalize manifest path,
   ecosystem identity, package identity and declared version literal only.
   Semantic categorization (dependency to Backend/Persistence/Authentication/
   ProblemArea) is not part of this session.
5. Declared-only semantic ceiling; the produced-work boundary stays unresolved.
6. ProblemArea taxonomy and promotion remain out of scope.

**Availability amendment.** The report's "unsupported ecosystem =
NotApplicable" proposal was rejected. Corrected semantics, now in the contract:
complete scan with nothing found is Available + complete + empty; truncation,
bounds or missing candidate content is incomplete; retrieval failure keeps the
real failure availability; an ecosystem outside the covered set is a product
coverage limitation of the contract version, never a per-repository state and
never `NotApplicable`.

Verified before implementation: **no new `DataAvailability` status is required.**
All four cases map onto the existing statuses, so the existing contract was not
silently extended.

**Versioning amendment.** `RuleSetVersion` and `ReportSchemaVersion` stay
unchanged. `AnalyzerVersion` must move for a semantic collection change.

Canonical convention check: `docs/product/scoring-and-report-contract.md` §16
and `docs/product/sample-analysis-report.md` fix only the *shape* of
`AnalyzerVersion` (`0.1.0`), and no canonical document stated a bump rule. The
rule is therefore now recorded in the new contract §9 (additive normalization →
minor, changed produced values → major, no value change → patch), and the
production literal moved `0.1.0` → `0.2.0` accordingly. If a different value is
preferred, only that literal and contract §9 need to change.

**Ecosystem-neutral definition** was adopted verbatim as contract §8, including
that neutrality does not mean supporting every ecosystem and that the supported
set is explicit and versioned.

## Implementation Result

Canonical contract authored: `docs/architecture/repository-dependency-evidence.md`.

Cross-references: `analysis-domain-model.md` §4 gained the
`DependencyDeclarations` field, a dedicated subsection and the two isolation
rules; `analysis-categories-and-signal-boundaries.md` §6 now states that
dependency evidence sits outside the 15 signals.

Code, all additive:

- Domain: `DependencyEcosystem`, `RepositoryDependencyDeclaration`, and
  `RepositorySnapshot.DependencyDeclarations` with deterministic ordering by
  manifest path, package identity and declared version literal, plus repeat
  collapsing. The parameter is optional, so every existing construction site is
  unchanged.
- Infrastructure: `GitHubRepositoryDependencyManifestCollector` plus an internal
  parser covering all eleven manifest kinds, with a bounded TOML subset reader
  for `pyproject.toml` and `Cargo.toml` so no new package dependency was added.
  Bounds: 32 files, 128 KiB per file, 512 KiB corpus, Ordinal candidate order.
- Orchestrator: the new collector runs last, contributes exactly one
  availability entry, and its declarations reach the snapshot. It contributes
  no file-tree overlay and no collection warning.
- Web: `AnalyzerVersion` 0.1.0 → 0.2.0.

Unparseable content yields no declaration and never fails the analysis.

## Validation

- `dotnet test GitHubProof.slnx`: 864/864 PASS
  (Domain 528, Infrastructure 171, Application 97, Web 68).
- Build: 0 warning / 0 error.
- Isolation regression tests added:
  `GitHubRepositorySnapshotOrchestratorTests.CollectAsync_DependencyEvidence_LeavesFileTreeAndCollectionWarningsUnchanged`
  and `RepositoryDependencyEvidenceIsolationTests`, which lock that neither
  present nor failed dependency evidence changes analysis status, score,
  signal results, limitations or warnings.
- Ecosystem coverage matrix: one collector test per manifest kind across all
  eight ecosystems, asserting one normalized shape.
- Availability and bounds tests: complete-and-empty, truncated tree, per-file
  bound, unknown size, file-count bound with Ordinal ordering, content failure,
  unavailable file tree, empty repository, unparseable content, commit pinning.

## Independent Review

`github-proof-review`, forked context. Verdict: PASS WITH FINDINGS, no BLOCKER.
Every approved decision was verified against the code, including that
`RepositoryAnalysisComposer` never reads `snapshot.Availability`, which makes
the status isolation structural rather than incidental.

Accepted findings, all fixed in this session:

- HIGH: `worklog/INDEX.md` had been re-encoded through CP1252, corrupting 19
  pre-existing rows. The file was restored from the session base and the 066 row
  re-added in UTF-8, so the diff is now a single added line.
- HIGH: `worklog/CURRENT.md` still described the pre-implementation state. It
  now reflects the implemented surface, the validation result and the next
  action.
- MEDIUM: the `AnalyzerVersion` bump rule lived only in the new contract.
  `scoring-and-report-contract.md` §16 now references it as a cross-cutting
  convention.
- MEDIUM: manifests already collected with content by the contents collector
  were downloaded a second time. The collector now reuses that content under the
  same byte bounds, with a test asserting no additional request is made.
- LOW: the ecosystem matrix missed `*.fsproj`, `*.vbproj`,
  `Directory.Packages.props` (the `PackageVersion` path), `build.gradle.kts` and
  `requirements-*.txt`. All five are now asserted.
- LOW: contract §11 described Gradle parsing more narrowly than the
  implementation; the wording now matches the line-based, comment-stripped scan.

Advisory, recorded as remaining risk: an empty declaration collection reads as
complete, so consumers must inspect the `dependency-manifests` availability key
before treating it as a completed scan.

## Validation After Review Fixes

- `dotnet test GitHubProof.slnx`: 870/870 PASS
  (Domain 528, Infrastructure 177, Application 97, Web 68).
- Build: 0 warning / 0 error.

## Remaining Risk

- Semantic categorization is not implemented; ProblemArea still needs the
  versioned dependency-to-area mapping contract and a declared-versus-produced
  boundary before any taxonomy can exist.
- The empty-but-complete reading above must be enforced when the first profile
  consumer of this surface is written.
- Parsing is bounded per format. Unsupported ecosystems, unparseable manifests
  and skipped candidates produce no declaration, and none of those states may be
  read as absence.
- The collector duplicates the contents collector's file-fetch shape rather than
  sharing it. This follows the existing self-contained collector convention;
  extracting a shared reader would be a separate bounded refactor.

## Current Status

READY_FOR_MANUAL_COMMIT

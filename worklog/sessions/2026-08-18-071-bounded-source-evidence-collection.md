# Session 071 — Bounded Source Evidence Collection

Date: 2026-08-18
Result: PASS / PUSHED — REMOTE VERIFIED ON SESSION BRANCH
Scope: architecture analysis + bounded collection measurement + canonical contract + implementation
Model Route: Claude Opus / high
Model Gate: APPROVED

## Goal

Decide the smallest safe and useful RAW SOURCE COLLECTION contract boundary that
a future canonical contract should lock, so that
`DependencySourceReferenceEvidence` (Session 068, semantics only) becomes
producible.

Step 1 is analysis and decision reporting only. No canonical contract, collector,
Domain model, detector or test is created or modified.

All Step 1 decisions below are PROPOSED / AWAITING REVIEW. None is canonical
truth.

## Starting State

Branch:
session/071-bounded-source-evidence-collection

Session Base:
2a1809d128de43b7c07709a8d3377afb37168ea9

Invariants verified before any mutation:

    branch          = session/071-bounded-source-evidence-collection   OK
    merge-base main = 2a1809d                                          OK
    working tree    = clean                                            OK

Previous session:
070 — Jobs-Style Product Packaging — integrated on main at 2a1809d.
CURRENT still carried the pre-integration Session 070 operational state and is
reconciled by this session initialization.

## Context Read

    CLAUDE.md
    agents/02-context-loading.md
    worklog/CURRENT.md
    docs/architecture/repository-dependency-source-reference-evidence.md
    docs/architecture/repository-dependency-evidence.md
    docs/architecture/repository-dependency-semantic-category.md §1-§4
    docs/architecture/public-profile-evidence-patterns.md §9
    docs/product/analysis-categories-and-signal-boundaries.md §6 signal list
    src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
    src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
    src/GitHubProof.Domain/Analysis/Models/RepositorySemanticEvidence.cs
    src/GitHubProof.Domain/Analysis/RepositoryDataKeys.cs
    src/GitHubProof.Domain/Analysis/Enums/DependencyEcosystem.cs
    src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContentsCollector.cs
    src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContentsCollection.cs
    src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
    src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryDependencyManifestCollector.cs
    src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositoryCollector.cs
    src/GitHubProof.Application/Analysis/RepositoryAnalysisComposer.cs §warnings/status
    src/GitHubProof.Web/Program.cs (AnalysisVersion)

## Surface Audit — Verified Starting Facts

Every fact below was verified against branch code, not assumed.

    FileTree carries path + size where GitHub supplies size          CONFIRMED
    general application source contents are not collected            CONFIRMED
    HeadCommitSha pins repository content collection                 CONFIRMED
    project/config/workflow/documentation content collection exists  CONFIRMED
    dependency-manifest collection is deterministic and bounded      CONFIRMED
    manifest numeric limits are not source-collection limits         CONFIRMED
    DependencyDeclarations are raw normalized snapshot evidence      CONFIRMED
    RepositorySemanticEvidence is derived and non-scoring            CONFIRMED
    dependency evidence is isolated from FileTree + warnings         CONFIRMED
    Repository MVP owns exactly 15 scoring signals                   CONFIRMED

Corrections and additions that the starting facts did not state:

1. Isolation is a property of the **dependency** surface only, not of content
   collection in general. `GitHubRepositorySnapshotOrchestrator.MergeFileTree`
   overlays `ProjectFiles` (and the license file) **into** `FileTree`, and
   `GitHubRepositoryContentsCollector` warnings — including the documentation
   Markdown bound warnings — flow into `RepositorySnapshot.CollectionWarnings`.
2. `snapshot.CollectionWarnings` drives `AnalysisStatus.Partial`,
   `limitation.snapshot.collection-partial` and one `AnalysisWarning` entry per
   warning. A source collector placed inside the contents collector would
   therefore change existing report output.
3. Tree normalization sets `IsDirectory` from `type == "tree"` only, so a Git
   submodule entry (`type == "commit"`) appears as a file with `Size = null`,
   and blob mode is not collected, so symlink blobs are indistinguishable from
   regular blobs.
4. Existing decoding is `Encoding.UTF8.GetString(Convert.FromBase64String(...))`
   with the non-throwing default, so invalid UTF-8 currently becomes U+FFFD
   silently.
5. Profile-scale multiplication is currently unbounded: the profile repository
   collector pages up to 10 × 100 repositories and
   `ProfileRepositoryAnalysisOrchestrator` applies no analysis count cap.
6. `AnalyzerVersion` is `1.0.0` (`src/GitHubProof.Web/Program.cs`).

## Step 1 Decisions — PROPOSED / AWAITING REVIEW

Full reasoning, alternatives and rejections are recorded in the Step 1 decision
report delivered in the session transcript. Summary of the recommended smallest
safe V1:

    purpose        raw bounded pinned source corpus only; no dependency
                   interpretation inside the collector
    eligibility    one global versioned extension allow-list on the last path
                   segment; not ecosystem-conditioned; not Languages-derived
    exclusion      deterministic path-segment deny-list + minified/bundled and
                   .d.ts filename patterns; declared as a budget policy, never
                   an authorship or generated-code judgment
    selection      pure global StringComparer.Ordinal ascending, single tier,
                   no role or language tiering
    bounds         32 files / 65536 bytes per file / 262144 corpus bytes
                   (provisional; measurement required before locking)
    retrieval      Contents API by path + ref=HeadCommitSha; no archive, no
                   clone, no code search, no GraphQL in V1
    artefact       dedicated RepositorySourceFile
                   (Path, DeclaredSize, DecodedByteCount, Content, ContentHash)
    payload        full bounded content + SHA-256 content hash at runtime;
                   persisted payload deliberately unresolved
    text boundary  strict UTF-8, NUL rejection, BOM strip, no line-ending
                   normalization; unprocessable is never negative evidence
    availability   dedicated data key `source-files`, dependency-manifest
                   completeness semantics reused
    isolation      no FileTree overlay, no CollectionWarnings entry, no signal,
                   no report element, availability entry only
    version        AnalyzerVersion, additive, minor bump class; exact value at
                   implementation
    architecture   dedicated GitHubRepositorySourceFileCollector consuming
                   GitHubRepositoryContentsCollection, mirroring the dependency
                   manifest collector

## Explicit Non-Decisions

    exact V1 numeric bounds pending a repository-set measurement
    whether source collection may be skipped when no declaration exists
    whether eligibility identity or blob SHA must be carried
    persisted evidence payload and self-contained replay
    qualifying-reference definition and per-ecosystem detection
    detection version owner name
    ProducedWorkEvidence resolution
    ProblemArea taxonomy and promotion

## Step 1 Review Outcome

    PASS WITH FINDINGS
    READY FOR BOUNDED MEASUREMENT
    NOT READY FOR CONTRACT

Approved architecture direction: raw source != DependencySourceReferenceEvidence,
dedicated source collector, no FileTree overlay, no CollectionWarnings
contamination, dedicated availability, non-scoring, no signal 16+,
AnalyzerVersion owns raw collection semantics, raw source never logged,
repository code never built or executed, Go `// indirect` and Maven
`dependencyManagement` do not block raw collection.

Not contract-locked: extension allow-list, exclusion list, candidate selection
algorithm, file/byte/corpus bounds, RepositorySourceFile field set, persisted
replay payload.

Corrections carried into Step 2:

1. Intentionally excluded paths are OUTSIDE THE SUPPORTED V1 SURFACE and do not
   make collection incomplete. Only supported+eligible material lost to a bound,
   an unknown size, an API failure or unprocessable content sets
   `IsComplete = false`.
2. `ContentHash` is not assumed. `path + commit + hash` is not self-contained
   replay. Artefact minimum A/B/C remains unresolved.
3. Pure global Ordinal is not approved; deterministic extension round-robin is
   measured against it as a coverage experiment only, never as language or
   engineering importance.
4. Exact extension set unapproved; CORE and EXPANDED both measured.
5. Symlink / submodule / non-regular tree entries should not silently become
   source evidence; the smallest future requirement is reported, not implemented.

## Step 2 — Bounded Collection Measurement

Measurement set: the canonical pinned set in `samples/validation/repositories.json`
(10 repositories, recorded commit SHAs). No repository was added, replaced or
silently substituted.

Method: MEASURED via GitHub REST at the pinned commit only —
`git/commits/{sha}` then `git/trees/{tree}?recursive=1` (Phase A, tree metadata
only), plus one bounded deterministic content sample (Phase B). No clone, no
build, no execution, no archive extraction, no code search, no LLM
classification. No raw source was printed, logged or written to the repository.

### MEASURED — tree completeness and non-regular entries

    all 10 trees retrieved, 0 truncated
    entries 1..107 per repository (max 107)
    blobs 188 x mode 100644, 15 x mode 100755
    trees  85 x mode 040000
    submodules (type=commit)  0
    symlinks  (mode=120000)   0
    `mode`, `sha` and blob `size` present on every entry

### MEASURED — supported surface (CORE vs EXPANDED)

    CORE     raw 76   excluded-by-policy 0   supported eligible 76
    EXPANDED raw 86   excluded-by-policy 0   supported eligible 86

Three of ten repositories carry no eligible source at all. The structural
exclusion hypothesis (node_modules, obj, bin, dist, build, target, ...) removed
**zero** files across the entire set.

### MEASURED — size distribution (86 supported eligible files, EXPANDED)

    min 28   median 870   p90 9590   p99 31728   max 42415   unknown size 0
    > 32 KiB : 1 file  (1.2%), 1 repository
    > 64 KiB : 0 files
    > 128 KiB: 0 files

### MEASURED — bound activation

    largest per-repository eligible corpus: 206 428 bytes (validation-001)
    only validation-001 (45 eligible files) ever exceeds a file-count bound
    corpus 128 KiB binds (worst-case representation 89%)
    corpus 256 KiB and 512 KiB are indistinguishable on this set
    per-file 32 KiB rejects one legitimate 42 415-byte Rust source file
    per-file 64 KiB and 128 KiB are indistinguishable on this set

### MEASURED — candidate strategy comparison

Global Ordinal and deterministic extension round-robin produced **identical**
completeness, median representation, worst-case representation, extension
coverage ratio (1.00 everywhere), extension starvation (0 everywhere) and
path-prefix concentration in all 72 measured combinations
(2 extension sets x 2 strategies x {16,32,48,64} x {32,64,128 KiB} x
{128,256,512 KiB}).

Cause, MEASURED not assumed: the only repository that ever exceeds a bound has a
single eligible extension, so round-robin degenerates to Ordinal. The set cannot
discriminate the two strategies.

### MEASURED — extension findings

    .kts     4 files, 1 repo — ALL are build.gradle.kts / settings.gradle.kts,
             i.e. already part of the canonical dependency manifest surface
    .groovy  5 files, 1 repo — real source under src/; grows that repository's
             eligible surface from 2 to 7 files
    .d.ts    1 file, 1 repo — hand-authored index.d.ts; excluding it removes 25%
             of that repository's eligible surface
    .razor .cshtml .vue .svelte .scala — ABSENT from the validation set

### MEASURED — bounded content sample (Phase B)

Deterministic selection: EXPANDED eligible, global Ordinal, N=32,
per-file 64 KiB, corpus 256 KiB. PROJECTED_CONTENT_REQUESTS = 73 (<= 400).

    73/73 succeeded, encoding = base64 in 73/73
    declared tree size == decoded byte count in 73/73 (0 differences)
    strict UTF-8 invalid: 0
    NUL bytes: 0
    UTF-8 BOM: 16/73 (21.9%), all of them .cs files in validation-001

API cost: 20 tree-phase requests + 73 + 73 content requests = 166 total.

### Validation-set coverage limitation — MEASURED

The canonical set cannot exercise the decision. It contains no Go, PHP, Ruby,
TypeScript-dominant, Vue, Svelte, Razor or Scala repository; its largest
repository holds 45 eligible source files and a 206 KB eligible corpus; and no
repository committed dependency, build-output or vendored directories. Adding
repositories requires explicit review and was not done.

## Step 2 Outcome

    Contract Readiness Verdict: NEEDS_MORE_MEASUREMENT

RESOLVED by measurement: corpus 128 KiB is too small; per-file 32 KiB is too
small; `.kts` exclusion is evidence-supported; Tree API already supplies `mode`
and `sha`, so excluding non-regular entries only requires carrying `mode`
through normalization; declared size predicted decoded bytes exactly; BOM
handling is a real, ecosystem-correlated rule rather than an edge case.

UNRESOLVED: exact file count, 64 vs 128 KiB per file, 256 vs 512 KiB corpus,
Ordinal vs round-robin, `.d.ts` and `.groovy` membership, the structural
exclusion list, and the RepositorySourceFile field set.

## Step 2B — Targeted Supplemental Measurement

Review corrections carried in: `.kts` extension membership re-opened (only known
build/config filenames were ever observed); CORE unapproved and extension effects
measured per repository as well as globally; 64 vs 128 KiB and 256 vs 512 KiB not
locked on first-dataset survival; Ordinal vs round-robin still open.

Supplemental set is TEMPORARY MEASUREMENT ONLY. It is not canonical validation
truth, `samples/validation/repositories.json` was not edited, and no measurement
artefact was left in the repository.

### MEASURED — pinned supplemental set

    s01 gin-gonic/gin              master dcaa4296   147 entries
    s02 slimphp/Slim               4.x    80900fb3   170
    s03 sinatra/sinatra            main   cb22afd7   326
    s04 vitejs/vite                main   c0f2fc60  3 679
    s05 vuejs/core                 main   a2b40db9    805
    s06 sveltejs/svelte            main   c0987c09 12 746
    s07 dotnet/blazor-samples      main   567732e0  8 672
    s08 com-lihaoyi/os-lib         main   86e5ddb6    150
    s09 axios/axios                v1.x   e6824eec    550
    s10 torvalds/linux             master 0f23d56f 71 797  TRUNCATED

All 10 available. 14 943 supported eligible files, 0 unknown sizes.

### MEASURED — Ordinal vs round-robin now discriminates

    metric                          ORD          RR
    extension coverage ratio        0.64-0.91    1.00
    repos with starved extension    4-7 of 10    0
    repos with >=75% prefix conc.   3-8          3-4
    repos with different selection  6-7 of 10
    median representation cost      -            <= 1 pp

Step 2's identical result is explained: single-extension repositories degenerate
round-robin to Ordinal. Multi-extension repositories separate them decisively.

### MEASURED — bound saturation

    N=32: corpus saturates at 384 KiB (256->384 KiB gains 1 pp for RR, 0 for ORD)
    N=48: 256->384->512 KiB = 20.9 -> 24.9 -> 26.0 median; 1 MiB adds nothing
    N=64: 256->384->512 KiB = 24.2 -> 28.3 -> 34.8 median; 1 MiB adds nothing
    per-file: >64 KiB = 16 files (0.11%), >128 KiB = 2 files (0.01%)
    completeness on realistic repositories is ~0% at every tested bound

### MEASURED — exclusion activation

    dist          234 files, 5.4% of one repository's raw candidates (s07)
    node_modules  6 files (s04)
    bin           2 files (s04)
    out           2 files (s04)
    19 other segments   never activated

Excluded paths are OUTSIDE THE SUPPORTED SURFACE, not incomplete collection, and
carry no generated, copied, vendor-authored or quality claim.

### MEASURED — extension evidence

    inclusion evidence: .go .php .rb .js .jsx .mjs .cjs .ts .tsx .d.ts .vue
                        .svelte .cs .razor .cshtml .scala .java .rs .py
    small-n evidence:   .kt (6 files), .groovy (5 files) — canonical set only
    build-filename only: .kts — 4 files, all build.gradle.kts / settings.gradle.kts;
                        no other *.kts observed in either set
    no evidence at all: .fs .vb

### MEASURED — non-regular entries and truncation

    symlinks: 17 (s04 4, s08 5, s10 8); 2 of them carry a source extension and
              would be collected as source today
    submodules: 0
    truncation: torvalds/linux truncated=true at 71 797 returned entries

Symlink exclusion is now empirically demonstrated, not only API-capable.

### MEASURED — content sample

Deterministic round-robin, N=32, per-file 64 KiB, corpus 256 KiB, s10 excluded.
PROJECTED_CONTENT_REQUESTS = 286 (<= 400).

    286/286 succeeded, encoding base64 in 286/286
    declared size == decoded bytes in 286/286 (359/359 including Step 2)
    strict UTF-8 invalid 0, NUL 0
    BOM 11/286; concentrated in .cshtml 7/8, .razor 2/8, .js 2/36
    combined with Step 2: .NET-family extensions 25/48 sampled files carry BOM

### PROPOSED — smallest balanced configuration

    selection    deterministic extension round-robin
    file count   32
    per file     65 536  (64 KiB)
    corpus       393 216 (384 KiB)   <- measured saturation point for N=32
    exclusions   node_modules, bin, out, dist have empirical support;
                 the remaining 19 segments are unproven but harmless
    filenames    build.gradle.kts and settings.gradle.kts excluded by filename,
                 never by excluding the whole .kts extension
    non-regular  regular blobs only; requires `mode` carried through tree
                 normalization
    BOM          strip before normalized Content, recorded as a versioned rule
    artefact     DecodedByteCount and ContentHash not justified by measurement

### UNRESOLVED after Step 2B

    .fs, .vb, non-build .kts membership — zero observations
    whether V1 chooses 32/384 KiB or 48/512 KiB (cost decision, fully characterised)
    persisted evidence payload and self-contained replay
    the 19 never-activated exclusion segments

    Contract Readiness Verdict: READY_FOR_CONTRACT

## Step 3 — Canonical Contract

Step 2B returned READY_FOR_CONTRACT, so this step created the canonical contract
only. No production code, test, report or UI was written.

### Created

`docs/architecture/repository-source-evidence-collection.md`
CONTRACT LOCKED, version 1, Session 071.

It owns bounded raw source collection semantics, the supported V1 source
surface, normalization, availability and completeness, isolation, version
ownership and the resource and security boundary. It owns no detection rule, no
dependency-to-source association, no `ProducedWorkEvidence`, no
`RepositoryProblemAreaEvidence`, no ProblemArea taxonomy or promotion, no
profile pattern, no scoring semantics and no report presentation.

### Locked

    ladder position   RepositorySourceFile is a RAW COLLECTION INPUT, placed
                      between DependencySemanticCategoryEvidence and
                      DependencySourceReferenceEvidence, and it is explicitly
                      not DependencySourceReferenceEvidence
    pinned commit     all retrieval uses RepositorySnapshot.HeadCommitSha
    entry type        regular blobs only (mode 100644 / 100755); symlinks,
                      submodules and non-regular entries are outside the
                      supported surface, which is not incompleteness
    extensions        one global case-insensitive allow-list of 24 extensions,
                      never activated by declarations, ecosystem, Languages API,
                      topics or an LLM
    filenames         build.gradle.kts and settings.gradle.kts excluded by exact
                      filename; the .kts extension is not excluded; .d.ts is not
                      excluded and is never called generated from its extension
    path exclusion    23 segments, declared as collection budget policy and
                      never as an authorship, originality, quality or
                      generated-code judgment
    bounds            32 files / 65536 bytes per file / 393216 corpus bytes
    file-count        the budget counts accepted artefacts, not inspected tree
                      entries; unsupported and metadata-rejected entries never
                      consume it
    ordering          deterministic extension round-robin, Ordinal within and
                      across groups, expressing no importance or preference
    artefact          RepositorySourceFile(Path, Content) only — no DeclaredSize,
                      no DecodedByteCount, no ContentHash, no classification
    text              strict UTF-8, NUL and invalid UTF-8 unprocessable, one
                      leading BOM stripped as a versioned rule, line endings
                      preserved
    availability      dedicated `source-files` key, no new DataAvailability
                      status, complete+empty means only "not observed in the
                      supported V1 surface"
    truncation        truncated tree keeps Available with IsComplete=false and
                      never means "no source"
    isolation         no FileTree overlay, no CollectionWarnings, no Status
                      change, no signal 16+, no report element, no RuleSetVersion
                      / DependencyCategoryMappingVersion / PatternRuleVersion
                      change
    security          source is untrusted data; never executed, built, evaluated
                      or logged; never placed in warnings, exceptions or reports
    version           AnalyzerVersion owns it; additive, minor bump class; exact
                      numeric value deliberately not fixed

Measurement results from Steps 2 and 2B appear in the contract only as concise
rationale. No measurement metric became runtime semantics.

### Aligned

`docs/architecture/repository-dependency-source-reference-evidence.md`
§5 prerequisite state changed from "no bounded collection contract exists" to
"collection semantics are defined by the new contract, production collection not
yet implemented"; §13 now points at the new owner for numeric values, extension
sets and candidate selection; §15 dropped the two bullets that the new contract
resolved.

Session 068's epistemic ceiling, detection boundary and `ProducedWorkEvidence`
state are unchanged.

### Not done

No `src/**` or `tests/**` mutation. No collector, Domain model, detector,
availability key implementation, report or UI change. No roadmap or P7 change.
No canonical navigation index required: `docs/` carries no index file and no
existing document references the source-reference contract, so the new contract
is discoverable exactly like its siblings.

## Step 4 — Independent Contract Review

Verdict: PASS_WITH_FINDINGS. Two HIGH determinism gaps, three MEDIUM and three
LOW findings. Implementation blocked pending correction.

## Step 4.1 — Finding Corrections

All eight findings corrected. No approved decision was reopened; no production
code was written.

    1 HIGH    round-robin skip semantics were undefined, so two implementations
              could produce different snapshots. Locked R1: the round-robin
              produces one total candidate order before acceptance, each entry
              appears exactly once, a skipped candidate is consumed, acceptance
              moves to the next candidate and never pulls a replacement from the
              same group. Stopping condition narrowed to sequence exhausted,
              budget reached, or no remaining candidate acceptable.
              Contract §12, new §12.1 and §12.2, plus §13.
    2 HIGH    byte bounds were not bound to a byte count. Locked new §10.1:
              declared size is a pre-fetch guard, provider decoded bytes are a
              post-decode safety guard, and canonical per-file and corpus
              accounting is the normalized Content UTF-8 byte count measured
              after BOM strip. Oversized declared size still rejects pre-fetch.
              Contract §10.1, §13, §17.
    3 MEDIUM  §18.2 listed individual retrieval failure as a cause of
              Available + IsComplete=false while §18.5 deferred to the existing
              convention, which preserves the real failure status. Removed from
              §18.2; §18.5 now states the failure status is kept, IsComplete is
              false and collected artefacts are preserved.
    4 MEDIUM  §7 wrongly claimed settings.gradle.kts is already owned by the
              manifest surface. Verified against repository-dependency-evidence
              §5: only build.gradle.kts is. Rationale split per filename; both
              filenames stay excluded; the .kts extension stays included.
    5 MEDIUM  session worklog carried a false validation claim and stale
              Not Done / Known Risks text. Corrected below.
    6 LOW     worklog said 24 structural path exclusions; the contract list
              holds 23. Worklog corrected; the canonical list is unchanged.
    7 LOW     declaration-independence was unrecorded. New §9.1 locks that
              DependencyDeclarations presence does not activate, deactivate,
              skip, narrow or widen source collection, and that source-files
              availability is independent of dependency-manifests availability.
    8 LOW     the mode prerequisite was unrecorded. §24 now requires the source
              path to obtain type, mode, path and size through its own
              dedicated collection input and forbids modifying or enriching
              RepositorySnapshot.FileTree to support source collection.

Preserved unchanged: 32 / 65536 / 393216, extension round-robin,
RepositorySourceFile(Path, Content), regular blobs only, BOM strip, line endings
untouched, the source-files key, AnalyzerVersion ownership with a minor bump
class, and the no-score / no-signal / no-report boundary.

## Step 4.2 — Contract Re-Review

Verdict: PASS_WITH_FINDINGS. All eight Step 4 findings verified FIXED against the
working tree, no regression. One new MEDIUM finding: source-file slot consumption
after content retrieval begins was ambiguous, because §11 defined the budget as
retrieval-and-collection while §12.2 expressed the stop condition as accepted
artefacts. Two readings gave different snapshot membership and different request
cost.

## Step 4.3 — Slot Semantics Correction

FIXED. Only that finding was addressed; no approved decision was reopened and no
production code was written.

Locked in `docs/architecture/repository-source-evidence-collection.md`:

    §11      MaximumSourceFiles = maximum source-content RETRIEVAL ATTEMPTS per
             repository, after metadata eligibility and pre-fetch resource checks
    §11.1    pre-fetch rejection consumes no retrieval slot — unsupported surface,
             non-regular blob, structural exclusion, excluded build filename,
             unsupported extension, unknown declared size, oversized declared
             size, corpus-nonfitting candidate
    §11.2    once retrieval begins the slot is consumed permanently and is never
             returned, including provider Failed / RateLimited / Forbidden,
             malformed Base64, unsupported encoding, invalid UTF-8, NUL and
             post-decode validation failure; artefact count is 0..32 and may be
             lower than the consumed slot count
    §11.3    cost invariant: retrieval attempts <= MaximumSourceFiles <= 32,
             a resource bound only, never a scoring, evidence-strength or quality
             rule
    §12      explicit seven-step acceptance walk over the fixed total candidate
             order
    §12.1    pre-fetch skip consumes no slot; a begun retrieval keeps its slot
    §12.2    stop condition restated as consumed retrieval slots, with the
             "32 successful artefacts" reading explicitly rejected
    §13      oversized and corpus-nonfitting candidates named as pre-fetch
             rejections that consume no retrieval slot

Availability semantics are unchanged: post-fetch failure keeps `IsComplete=false`,
preserves already-collected artefacts, and a provider failure retains its real
`DataAvailability` status under §18.5.

## Step 4.4 — Final Contract Re-Review

Verdict: PASS. All fifteen determinism dimensions single-valued, four traces with
exactly one valid result each, no semantic, isolation, security, resource or
version regression. Contract declared implementation-ready.

## Step 5 — Implementation

IMPLEMENTATION_COMPLETE. Independently reviewed PASS WITH FINDINGS, no open
BLOCKER or HIGH.

### Production surfaces

    Domain      RepositorySourceFile(Path, Content)
                RepositorySnapshot.SourceFiles, additive trailing parameter,
                normalized by Path Ordinal with duplicate collapse
                RepositoryDataKeys.SourceFiles = "source-files"
    Infra       GitHubRepositorySourceFileCollector + collection record +
                IGitHubRepositorySourceFileCollector
                orchestrator wiring: availability entry only, snapshot surface,
                no CollectWarnings contribution
    Web         AnalyzerVersion 1.0.0 -> 1.1.0 (additive minor bump)

The collector fetches its own pinned tree metadata through
`git/commits/{head}` then `git/trees/{tree}?recursive=1`, because the
regular-blob rule needs Git `mode`, which `RepositorySnapshot.FileTree` does not
carry and must not be enriched to carry.

### Contract clauses realised

    §5    type == blob and mode in {100644, 100755}; symlink, submodule, tree
          and other non-regular entries are outside the supported surface
    §6    24-extension global allow-list, invariant lowercase normalization,
          `.d.ts` special case
    §7    build.gradle.kts and settings.gradle.kts excluded by exact filename
    §8    23 structural path segments, directory segments only
    §9.1  collection reads no declaration, manifest availability or semantic
          evidence
    §10   32 / 65536 / 393216
    §10.1 canonical accounting on normalized Content UTF-8 bytes after BOM strip
    §11.1 pre-fetch rejection consumes no retrieval slot
    §11.2 a begun retrieval consumes its slot permanently
    §12   total candidate order materialized before acceptance, round-robin over
          Ordinal groups and Ordinal paths
    §12.2 stop on exhausted order or consumed slots, never on one non-fitting
          candidate
    §16   base64, strict UTF-8, NUL rejection, one leading BOM stripped, line
          endings preserved
    §18   dedicated source-files key, provider failure keeps its real status
    §19   truncated tree stays available and incomplete
    §20   no FileTree overlay, no CollectionWarnings, no signal, no report
    §21   no execution, no logging of source, diagnostics carry path and status

### Tests

    GitHubRepositorySourceFileCollectorTests           51 cases
    RepositorySourceEvidenceIsolationTests              4 cases
    GitHubRepositorySnapshotOrchestratorTests           +1 isolation case,
                                                        existing call-order and
                                                        availability-count
                                                        expectations extended

Covered invariants include regular-blob acceptance, symlink and submodule
rejection without incompleteness, the allow-list, the exact Gradle filename
exclusions, structural exclusions, round-robin request order, oversized and
unknown-size pre-fetch rejection consuming no slot, the 33-candidate provider
failure trace (32 attempts, 31 artefacts, #33 unfetched), the oversized-#5 trace
(#33 becomes the 32nd retrieval), corpus skip continuing to a later fitting
candidate, BOM stripping and normalized byte accounting at the bound, invalid
UTF-8 and NUL handling, tree truncation, rate-limit status preservation with
partial artefacts, declaration independence, FileTree immutability and
CollectionWarnings isolation.

### Independent implementation review

Verdict: PASS WITH FINDINGS. No BLOCKER and no HIGH finding. The review
independently reproduced the build and test results and re-checked the diff
against every locked contract clause.

    MEDIUM  stale session worklog: Not Done, Known Risks and the suggested
            commit type still described the pre-implementation state
            -> FIXED in this session
    MEDIUM  contract §17 forbids unbounded allocation; per-file and corpus
            overflow are enforced but no response-size ceiling is applied before
            the provider body is deserialized. The exposure is identical in every
            existing collector and §17 leaves the strategy open
            -> recorded in Known Risks and handed to a separate bounded task
    LOW     suggested commit type and one stale risk entry
            -> FIXED in this session
    LOW     two extra metadata requests per repository, required by §24
            -> recorded in Known Risks, contract-compliant

### Validation

    dotnet build     0 warning / 0 error
    dotnet test      967/967 PASS
                     Domain 543, Application 101, Infrastructure 255, Web 68
    git diff --check PASS

## Changes

    Created:
    - docs/architecture/repository-source-evidence-collection.md
    - src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySourceFileCollector.cs
    - src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySourceFileCollection.cs
    - tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositorySourceFileCollectorTests.cs
    - tests/GitHubProof.Application.Tests/Analysis/RepositorySourceEvidenceIsolationTests.cs
    - worklog/sessions/2026-08-18-071-bounded-source-evidence-collection.md

    Modified:
    - docs/architecture/repository-dependency-source-reference-evidence.md
    - src/GitHubProof.Domain/Analysis/Models/RepositorySnapshot.cs
    - src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs
    - src/GitHubProof.Domain/Analysis/RepositoryDataKeys.cs
    - src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
    - src/GitHubProof.Infrastructure/GitHub/IGitHubRepositoryCollectors.cs
    - src/GitHubProof.Web/Program.cs
    - tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositorySnapshotOrchestratorTests.cs
    - worklog/CURRENT.md
    - worklog/INDEX.md

No report, UI or roadmap file was modified. No scoring signal changed.

## Validation

    dotnet build                        PASS — 0 warning / 0 error
    dotnet test                         PASS — 967/967
    git diff --check                    PASS
    git status --short                  reviewed

## Problems

None.

## Not Done

The canonical collection contract, the bounded source collector, the
`RepositorySourceFile` snapshot surface and the `source-files` availability key
are implemented.

Still deliberately out of scope: the reference detector,
`DependencySourceReferenceEvidence`, `ProducedWorkEvidence`,
`RepositoryProblemAreaEvidence`, ProblemArea taxonomy, P7, scoring, report and
UI changes, source persistence, commit and push.

## Known Risks

- Profile-scale memory and request amplification remain the binding constraints
  on corpus retention, because the analyzed repository count is currently
  uncapped.
- Realistic repositories will normally produce `Available + IsComplete = false`;
  downstream consumers must never read that as absence.
- `.fs`, `.vb` and non-build `.kts` are in the allow-list on ecosystem-coherence
  grounds, not on measured observation; their real-world behaviour is untested.
- The regular-blob filter needs Git tree `mode`, which the existing FileTree
  normalization does not carry. The collector therefore repeats the commit and
  recursive-tree requests, costing two extra metadata requests per repository.
  This is required by contract §24 and is outside the §11.3 retrieval-slot
  invariant, but it adds to profile-scale request amplification.
- Contract §17 also forbids unbounded allocation. Per-file and corpus bound
  overflow are enforced, but no response-size ceiling is applied before the
  provider body is deserialized. The exposure is identical to every existing
  collector and §17 leaves the implementation strategy open, so it is recorded
  as a repository-wide follow-up rather than fixed inside this session's scope.

## Next

Decide the main transition for Session 071. Merging the session branch into main,
pushing main and deleting the branch are separate explicit user-owned Git side
effects and none of them has been performed.

## Git

    Commit:  ce74d4be07c7f498659b8ec5c2a5e211ed5a9d8c
             feat: collect bounded raw repository source evidence
             parent 2a1809d128de43b7c07709a8d3377afb37168ea9
             16 files, +3193 / -115

    Push:    origin/session/071-bounded-source-evidence-collection
             = ce74d4be07c7f498659b8ec5c2a5e211ed5a9d8c

    Remote verification:
             GitHub API confirmed the commit SHA, message, parent and
             16-file / +3193 / -115 scope on origin.

    Main:    origin/main remains 2a1809d128de43b7c07709a8d3377afb37168ea9.
             Not merged, branch not deleted.

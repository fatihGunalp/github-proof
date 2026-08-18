# Repository Source Evidence Collection Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Repository-level bounded raw source collection
**Session:** 071
**Date:** 2026-08-18

## 1. Purpose

This contract defines what bounded raw repository source collection may mean,
and what it may never mean.

Input contracts:

- `docs/architecture/repository-dependency-evidence.md`
- `docs/architecture/repository-dependency-semantic-category.md`

Downstream context:

- `docs/architecture/repository-dependency-source-reference-evidence.md`
- `docs/architecture/public-profile-evidence-patterns.md` §9

It owns bounded raw source collection semantics, the supported V1 source
surface, normalization, availability and completeness, isolation, version
ownership and the resource and security boundary.

It owns no detection rule, no dependency-to-source association, no
`ProducedWorkEvidence`, no `RepositoryProblemAreaEvidence`, no ProblemArea
taxonomy or promotion, no profile pattern, no scoring semantics and no report
presentation.

## 2. Canonical Position

The expanded ladder is:

    DependencyDeclaration
    != DependencySemanticCategoryEvidence
    != RepositorySourceFile
    != DependencySourceReferenceEvidence
    != ProducedWorkEvidence
    != RepositoryProblemAreaEvidence
    != RepeatedProfilePattern(ProblemArea)
    != DeveloperExperienceOrCapability

`RepositorySourceFile` is a **raw collection input**, not a further semantic
evidence claim. It is normalized collected data carried by `RepositorySnapshot`,
exactly like `Languages`, `Topics`, `License` and `DependencyDeclarations`.

Explicitly:

    RepositorySourceFile != DependencySourceReferenceEvidence

Collecting source files establishes no level of this ladder beyond the raw
collection input itself.

## 3. Maximum Canonical Meaning

One `RepositorySourceFile` means exactly:

> At pinned commit C, path P was a supported regular repository source blob
> inside the supported V1 source surface, and its bounded normalized textual
> content is the content recorded here.

The collection as a whole answers exactly one question:

> What bounded source content was observable within the supported V1 source
> surface at pinned commit C?

It must not mean:

- a dependency is referenced by this source,
- a dependency is used at runtime,
- a dependency is used in production,
- the repository compiles,
- the source participates in a successful build,
- the source executes,
- meaningful implementation exists,
- work was produced with any technology,
- the source was authored by the profile owner,
- the implementation is original,
- the implementation is correct,
- the implementation is high quality,
- the developer knows, understands or is capable with any technology,
- any seniority or employability conclusion,
- `ProducedWorkEvidence` has been established,
- any ProblemArea is proven.

Epistemic strength may never exceed the observation that supports it.

## 4. Pinned Commit

Source collection is tied to `RepositorySnapshot.HeadCommitSha`. Every source
content retrieval uses that pinned commit.

    same pinned repository state + same AnalyzerVersion
    = same supported candidate ordering and the same normalized result

subject only to explicitly represented availability and failure states.

No current-branch drifting retrieval is permitted. GitHub code search is
permitted nowhere in this contract.

## 5. Supported V1 Entry Type

V1 source evidence supports **regular blobs only**.

A supported Git tree entry satisfies:

    type == "blob"
    AND mode in { "100644", "100755" }

Outside the supported V1 source surface:

- tree entries,
- symlink blobs, including mode `120000`,
- submodules and `type == "commit"` entries,
- any other non-regular entry.

A symlink whose path carries a source-looking extension remains outside the
supported V1 source surface. Its content is a link target, not source.

Rationale, recorded as motivation only and never as runtime semantics: the
Session 071 supplemental measurement observed real symlink blobs carrying source
extensions in a public repository.

    outside supported surface != collection incomplete

## 6. Source Extension Allow-List

Eligibility uses **one global, deterministic, case-insensitive** V1 source
extension allow-list, matched on the final path segment.

The allow-list must never activate or deactivate based on
`DependencyDeclarations`, a detected ecosystem, the GitHub Languages API,
repository topics or any LLM classification.

The exact V1 allow-list is:

| Family | Extensions |
|---|---|
| .NET | `.cs`, `.fs`, `.vb` |
| JVM | `.java`, `.kt`, `.kts`, `.scala`, `.groovy` |
| JavaScript / TypeScript | `.js`, `.jsx`, `.mjs`, `.cjs`, `.ts`, `.tsx`, `.d.ts`, `.vue`, `.svelte` |
| Other | `.py`, `.go`, `.rs`, `.php`, `.rb` |
| Markup-bearing source | `.razor`, `.cshtml` |

`.d.ts` is a member of the allow-list. It is never classified as generated from
its extension alone.

Extension normalization: the final path segment is lowercased; a name ending in
`.d.ts` normalizes to `.d.ts`; otherwise the extension is the substring from the
last `.`.

Rationale: most entries are directly supported by observed coverage. `.fs`,
`.vb` and non-build `.kts` remain legitimate source-capable members of supported
ecosystems even though the measurement corpus did not exercise them. Absence
from a measurement corpus is not evidence that a source language is not source.
Including an extension is a collection-surface decision, not an inference.

Configuration formats such as `.json`, `.yaml`, `.yml`, `.toml` and `.xml` are
not in the V1 source allow-list. Manifest and configuration collection remain
owned by their existing surfaces.

Adding, removing or renaming an allow-list entry requires a new contract version
and an `AnalyzerVersion` change under §23.

## 7. Excluded Build Filenames

Within the otherwise source-capable `.kts` extension, exactly these final path
segments are excluded, case-insensitively:

    build.gradle.kts
    settings.gradle.kts

Reasons, which differ per filename:

- `build.gradle.kts` is already part of the dependency and build manifest
  evidence surface owned by `repository-dependency-evidence.md` §5;
- `settings.gradle.kts` is a build and settings configuration script and is
  intentionally placed outside the V1 raw source surface. It is **not** claimed
  to be collected by any other canonical surface.

The `.kts` extension as a whole is not excluded. `.d.ts` is not excluded.

## 8. Structural Path Exclusion

Structural path exclusion is a **collection budget and supported-surface
policy**. It is never an authorship, originality, quality or generated-code
judgment.

A supported eligible candidate is excluded when any directory segment of its
path matches one of the following, case-insensitively:

    node_modules  bower_components  vendor  third_party  Pods
    bin  obj  dist  out  build  target  coverage
    .venv  venv  virtualenv  site-packages  __pycache__  .tox
    .gradle  .next  .nuxt  .vs  .idea

An excluded path is **outside the supported V1 source surface**. Its presence
does not set `IsComplete = false`.

Its exclusion does not mean that the code is generated, that the code is copied,
that the code is third-party authored, that the code is unimportant or that the
code is low quality.

V1 introduces no content-based generated-code heuristic and no authorship or
originality heuristic.

## 9. Eligibility Pipeline

The deterministic metadata eligibility order is:

1. a pinned recursive tree entry exists;
2. the entry is a supported regular blob under §5;
3. the path is not under a structurally excluded segment under §8;
4. the final filename is not an exact excluded build filename under §7;
5. the normalized extension is in the V1 allow-list under §6.

Only an entry satisfying all five is **supported eligible source**.

Entries rejected by these checks are outside the supported surface and do not
affect completeness.

### 9.1 Declaration Independence

Raw source collection is an independent raw evidence surface. It must not be
skipped, narrowed or widened because no `DependencyDeclaration` exists, because
dependency manifest collection is empty, because dependency manifest collection
is unavailable or incomplete, or because `RepositorySemanticEvidence` is empty.

    DependencyDeclarations presence
    does not activate, deactivate, skip, narrow or widen
    RepositorySourceFile collection

When repository orchestration requests source collection, it runs according to
this contract's own supported-surface rules. The `source-files` availability
state is therefore independent of the `dependency-manifests` availability state.

This introduces no trigger or policy system.

## 10. Deterministic Bounds

    MaximumSourceFiles         32
    MaximumSourceFileBytes     65536   (64 KiB)
    MaximumSourceCorpusBytes   393216  (384 KiB)

Bounds are identical for every ecosystem, extension and repository. They must
never vary by ecosystem, repository size or execution order.

Rationale, concise: 32 keeps profile-scale API amplification bounded; 64 KiB
excluded only a very small measured fraction of supported eligible files while
32 KiB was empirically too small; 384 KiB was the measured saturation point for
the chosen 32-file round-robin strategy; raising the file count to 48 materially
increases profile-scale request cost and is not required for V1.

Reaching any bound produces incomplete evidence under §18, never unavailable
evidence and never absence.

### 10.1 Canonical Byte Accounting

Three byte counts exist and must never be confused:

    declared Git tree blob size      pre-fetch guard only
    provider decoded bytes           post-decode safety guard
    normalized Content UTF-8 bytes   canonical accounting

Canonically:

- `MaximumSourceFileBytes` is the maximum UTF-8 byte count of one artefact's
  normalized `Content`, measured **after** the single leading UTF-8 BOM has been
  stripped under §16;
- `MaximumSourceCorpusBytes` is the sum of the UTF-8 byte counts of the accepted
  artefacts' normalized `Content`, measured the same way.

BOM bytes never count toward either budget, because the BOM is removed by the
versioned normalization rule.

A declared size exceeding `MaximumSourceFileBytes` nevertheless remains a
conservative pre-fetch rejection under §13, even when BOM removal could
theoretically bring the normalized content under the bound. Pre-fetch safety
must not require retrieving an oversized provider payload.

After decoding and normalization the normalized UTF-8 byte count is recomputed,
the per-file bound is enforced, the corpus bound is enforced, and no accepted
result may exceed either canonical bound.

## 11. File-Count Semantics

`MaximumSourceFiles = 32` means:

> the maximum number of source-content **retrieval attempts** permitted for one
> repository, after deterministic metadata eligibility and pre-fetch resource
> checks.

It does not mean "inspect only the first 32 raw tree entries", and it does not
mean "keep collecting until 32 artefacts have succeeded".

### 11.1 Pre-Fetch Rejection Consumes No Slot

A candidate that is never fetched consumes no retrieval slot. This covers:

- an entry outside the supported surface under §5;
- a non-regular blob under §5;
- a structurally excluded path under §8;
- an excluded exact build filename under §7;
- an unsupported extension under §6;
- an unknown declared size under §13;
- a declared size exceeding `MaximumSourceFileBytes` under §13;
- a candidate that cannot fit the remaining planned corpus budget under §13.

Such a candidate is consumed in the total candidate order under §12.1, sets
`IsComplete = false` where §18 requires it, never becomes negative evidence, and
never prevents later candidates from being considered while retrieval slots and
resource budget remain.

### 11.2 A Begun Retrieval Consumes the Slot Permanently

Once source-content retrieval **begins** for a candidate, one retrieval slot is
consumed.

The slot remains consumed even when retrieval or post-fetch processing later
returns a provider failure such as `Failed`, `RateLimited`, `Forbidden` or
another existing provider failure status; returns malformed Base64 or an
unsupported provider encoding; yields invalid UTF-8 or a NUL byte; fails the
post-decode resource validation of §17; or otherwise cannot produce a
`RepositorySourceFile`.

A consumed retrieval slot is **never** returned to the budget. The
implementation must not fetch further candidates merely to reach 32 successful
`RepositorySourceFile` artefacts.

Consequently the collected artefact count is `0..32` and may be lower than the
number of consumed retrieval slots.

### 11.3 Cost Invariant

For one repository analysis under V1:

    source-content retrieval attempts <= MaximumSourceFiles <= 32

This is a collection resource bound only. It is never a scoring rule, never an
evidence-strength rule and never a repository quality judgment.

## 12. Candidate Ordering

Ordering is **deterministic extension round-robin**, materialized as one total
candidate order **before** any content-budget acceptance decision is made:

1. normalize the extension under §6;
2. group supported eligible entries by normalized extension;
3. order the extension groups with `StringComparer.Ordinal`;
4. order paths inside each group with `StringComparer.Ordinal`;
5. cycle through the non-empty groups, appending at most one next candidate from
   each group per round, until every supported eligible entry has been appended;
6. the resulting sequence is the **total candidate order**, in which every
   supported eligible entry appears exactly once.

Acceptance then walks the total candidate order from beginning to end:

1. take the next candidate of the total candidate order;
2. evaluate the pre-fetch eligibility and resource guards of §6 to §9 and §13;
3. on pre-fetch rejection the candidate is consumed in the total order, no
   retrieval slot is consumed, and the walk continues;
4. otherwise, if retrieval slots remain under §11, consume one retrieval slot
   and perform the retrieval;
5. on success, decode and normalize under §16, and add a `RepositorySourceFile`
   when the result is valid and within the canonical post-normalization bounds
   of §10.1;
6. on post-fetch failure no artefact is added, the retrieval slot stays
   consumed, `IsComplete` becomes `false`, and the availability semantics of
   §18.5 apply unchanged;
7. continue while the total order remains, retrieval slots remain, and the
   deterministic pre-fetch resource guards can still admit a candidate.

### 12.1 Skip Semantics

A candidate is skipped when its declared size is unknown, its declared size
exceeds the per-file bound, it cannot fit the remaining corpus budget, or its
retrieval or processing later fails.

A skipped candidate is **consumed** in the total candidate order. Acceptance
proceeds to the next candidate of that sequence.

A skip must never pull another candidate from the same extension group forward
as a replacement, and must never reorder the remaining sequence.

A pre-fetch skip consumes no retrieval slot under §11.1. A candidate whose
retrieval had already begun keeps its slot consumed under §11.2.

Later candidates may still be fetched and accepted while retrieval slots remain
under §11 and the remaining corpus budget permits them.

### 12.2 Stopping Condition

Acceptance stops only when:

- the total candidate order is exhausted, or
- `MaximumSourceFiles` retrieval slots have been consumed, or
- no remaining candidate can pass the deterministic pre-fetch resource guards.

A single candidate that cannot fit never stops acceptance.

The stopping condition is deliberately **not** expressed as "32 successful
artefacts accepted", because that reading would permit more than 32 content
requests for one repository and would break the §11.3 cost invariant.

Prohibited in ordering: randomness, clock, locale, popularity, language
importance, ecosystem weighting, path-depth priority, LLM involvement and any
provider ranking.

Rationale, at a high level only: measurement showed round-robin prevented
extension starvation at negligible overall representation cost. Measurement
metrics are rationale, never runtime contract.

Round-robin ordering expresses no importance, preference or ranking among
extensions, languages or ecosystems.

## 13. Corpus Budget Selection

The declared Git tree blob size is the pre-fetch budgeting signal when
available. It is a guard only; canonical per-file and corpus accounting is the
normalized `Content` byte count defined by §10.1.

A supported eligible source file whose declared size exceeds
`MaximumSourceFileBytes`:

- is not fetched as source evidence,
- sets `IsComplete = false`,
- consumes no retrieval slot under §11.1,
- creates no negative evidence.

When a declared size is unavailable it is never treated as zero and unbounded
content is never silently fetched. The entry is not collected, source collection
becomes incomplete for that entry, and deterministic processing continues where
possible.

For corpus selection:

- the accepted source corpus never exceeds `MaximumSourceCorpusBytes`, measured
  under §10.1;
- a candidate that cannot fit within the remaining corpus budget is skipped as a
  pre-fetch rejection, consumes no retrieval slot under §11.1, and sets
  `IsComplete = false`;
- the skip follows §12.1: the candidate is consumed and acceptance moves to the
  next candidate of the total candidate order;
- later candidates of that sequence that do fit may still be accepted;
- no result may exceed the corpus bound.

The selection algorithm is deterministic under every skip path, because the
total candidate order is fixed before acceptance begins.

## 14. Retrieval Mode

The V1 provider direction is the GitHub Contents API, addressed by path, with
`ref` set to the pinned `HeadCommitSha`.

The contract requires pinned retrieval semantics.

Rejected as V1 canonical retrieval methods: GitHub code search, repository
clone, archive or tarball extraction, build, execution and runtime evaluation.

A Blob API or GraphQL optimization may be a future implementation change only if
it preserves identical canonical normalized snapshot semantics and versioning.

## 15. Normalized Artefact Shape

The V1 raw artefact carries exactly:

    RepositorySourceFile
    ├── Path
    └── Content

V1 defines no field for declared size, decoded byte count, content hash,
ecosystem, language classification, application/test/example role, generated
flag, dependency identity or `RepositoryAnalysisReference`.

Declared size is collection metadata, not required persisted raw evidence.
Decoded byte count is a collection safety calculation, not snapshot truth.

A content hash is not self-contained replay and has no demonstrated V1 raw
evidence need.

`RepositoryAnalysisReference` remains a derived-result concern and is never a
field of a raw source artefact.

Deterministic ordering of the collected artefacts is by `Path`, `Ordinal`
ascending. Identical paths collapse to one artefact.

## 16. Text Normalization

V1 source content is textual UTF-8.

- Provider content must be valid Base64 when that transport is used.
- Decoded bytes must be valid strict UTF-8.
- A payload containing a NUL byte is unsupported and unprocessable textual
  source.
- Invalid UTF-8 is unsupported and unprocessable.
- An unsupported provider encoding is unsupported and unprocessable.
- One leading UTF-8 BOM is stripped from the normalized `Content`.
- Line endings are not normalized.
- All remaining textual content is preserved exactly.

BOM stripping is a versioned normalization rule owned by `AnalyzerVersion`.

Consequently:

    normalized Content != byte-for-byte provider blob

because a leading UTF-8 BOM may have been removed.

Unsupported or unprocessable content is not collected, sets `IsComplete = false`
for an otherwise supported eligible entry, and never becomes negative evidence.

## 17. Post-Decode Resource Check

The declared Git tree size is a pre-fetch guard, not absolute trust. Resource
bounds are enforced again after decoding and normalization, against the
canonical normalized `Content` byte count defined by §10.1.

A malformed or inconsistent provider payload must never permit a per-file bound
overflow, a corpus bound overflow or unbounded allocation.

The exact implementation strategy is not owned by this contract.

Provider payload and source content must never appear in exception or warning
text.

## 18. Availability and Completeness

This additive raw surface uses one dedicated availability key:

    source-files

No new `DataAvailability` status is introduced; the existing vocabulary is used.

### 18.1 Available and complete

    Available + IsComplete = true

means the supported V1 source surface was fully evaluated and every supported
eligible artefact was collected within contract bounds.

### 18.2 Available and incomplete

    Available + IsComplete = false

means partial source evidence exists, or contract bounds or unprocessable
content prevented full coverage.

Causes include: recursive tree truncation; supported eligible count exceeding
the file bound; a supported eligible file exceeding the per-file bound; the
corpus bound preventing collection; a required size being unavailable; and
unprocessable supported eligible content.

A provider retrieval failure is deliberately **not** in this list. It is owned by
§18.5 and keeps a real failure availability status.

Partial positive source artefacts are always preserved.

### 18.3 Intentionally unsupported

An unsupported extension, a structurally excluded path, an exact excluded build
filename, a symlink, a submodule and any other non-regular entry are
intentionally outside the supported V1 surface. None of them makes the supported
V1 surface incomplete.

### 18.4 Complete and empty

    Available + IsComplete = true + empty collection

means exactly:

> No source artefact was observed inside the supported V1 source surface.

It does not mean that the repository has no source code, that a dependency has
no source reference, or that an implementation does not exist.

### 18.5 Unavailable

Provider failure keeps the real failure status and follows the existing
repository collection availability conventions. No new status semantics are
invented here.

When one supported eligible source-content retrieval produces a provider failure
status such as `RateLimited`, `Forbidden`, `Failed` or another existing
non-`Available` failure status, the dedicated `source-files` availability
reflects that real failure status, `IsComplete` is `false`, and every already
collected positive `RepositorySourceFile` artefact is preserved.

A provider failure is never downgraded to `Available`.

    bound / truncation / unsupported processing
        may be Available + IsComplete = false
    provider failure
        real provider failure status + IsComplete = false

    unavailable != absence
    incomplete  != absence
    unsupported != absence
    absence     != negative evidence

## 19. Tree Truncation

When recursive tree retrieval is truncated:

- `source-files` may still be `Available` when the partial tree data is usable;
- `source-files` `IsComplete` must be `false`;
- eligible artefacts from the returned partial tree may still be collected;
- truncation never means "no source".

Cloning or paginating through another semantic path merely to defeat the
canonical tree bound is not permitted.

## 20. Isolation

Source collection must not:

- overlay source content into `RepositorySnapshot.FileTree`,
- change any existing `FileTree` value,
- write source failures into `RepositorySnapshot.CollectionWarnings`,
- affect `RepositoryAnalysis.Status`,
- affect existing limitation keys,
- affect any existing scoring signal,
- affect any category result,
- create signal 16 or beyond,
- create a repository report element,
- change `RuleSetVersion`,
- change `DependencyCategoryMappingVersion`,
- change `PatternRuleVersion`.

Raw source collection lives on its own additive snapshot surface and carries its
own availability and completeness state.

Repository MVP V1 still owns exactly 15 scoring signals. The existing 15-signal
semantics remain isolated from this surface.

## 21. Security, Privacy and Logging

Repository source code is **untrusted data**.

The collector must never execute it, build it, compile it, evaluate it, run
scripts from it, expand repository archives, or send it to an LLM as part of raw
collection.

The collector must never log raw source content or any source substring, place
source content in `CollectionWarnings`, place source content in exception
messages, or place raw source in repository report output.

Public repositories may accidentally contain committed secrets. Public
visibility does not make raw source safe to duplicate into logs or telemetry.

Diagnostic identity may use bounded metadata such as a path and a status, never
a source payload.

Cancellation must remain honored.

## 22. Persistence and Replay

The runtime snapshot carries bounded normalized `Content` for downstream
deterministic analysis.

It is not claimed that `Path + ContentHash`, or `Path + commit + hash`, would
provide self-contained replay. Remote refetch is not equivalent to
self-contained replay.

The persisted long-term evidence payload remains out of scope and unresolved.
This contract introduces no source persistence.

## 23. Version Ownership

`AnalyzerVersion` owns raw source collection semantics.

This V1 surface is additive and backward compatible relative to the existing
snapshot and therefore requires a **minor** `AnalyzerVersion` increase when it is
implemented, under the canonical bump rule in
`repository-dependency-evidence.md` §9. The future numeric value is deliberately
not fixed here.

An `AnalyzerVersion` change is required whenever a change can alter normalized
raw source snapshot values, including changes to: the supported extension set;
the structural exclusion set; the exact filename exclusions; the file-count,
per-file or corpus bounds; the ordering; the regular-entry semantics; the
decoding or normalization rules; and any retrieval behavior that changes
normalized values.

`RuleSetVersion` remains scoring-only. `DependencyCategoryMappingVersion` remains
dependency-category-only. `PatternRuleVersion` remains downstream profile-pattern
ownership. None of them may own raw source collection semantics.

Future reference-detection semantics require a separate version owner and remain
out of scope, exactly as recorded by
`repository-dependency-source-reference-evidence.md` §12.

## 24. Implementation Direction

The behavioral direction, not a naming requirement:

- source collection runs on a dedicated, isolated collection path;
- its isolation is conceptually the same as dependency-manifest collection;
- its output feeds a dedicated raw snapshot surface;
- source content never flows through a `FileTree` overlay and never contributes
  to `CollectionWarnings`.

The source collection path must obtain the Git tree metadata it needs, namely
`type`, `mode`, `path` and `size`, through its own dedicated collection input.
It must not modify or enrich `RepositorySnapshot.FileTree` in order to support
source collection: existing `FileTree` semantics remain unchanged, source
collection must not depend on `FileTree` for a field that `FileTree` does not
canonically own, and the regular-blob filtering of §5 is performed inside the
dedicated source collection path.

No class name is canonical product truth. This contract owns behavior, not
incidental naming.

## 25. Known Limitations

Accepted V1 limitations. None of them may ever be read as a developer
deficiency.

- Bounded collection intentionally does not cover every programming or source
  format.
- An unsupported extension is not negative evidence.
- Source count and corpus limits mean large repositories normally produce
  `Available + IsComplete = false`.
- Symlinks and submodules are outside V1 source evidence.
- Long-term persisted replay is not defined.
- Source collection alone cannot establish any dependency reference.
- Source collection alone cannot establish `ProducedWorkEvidence` or any
  ProblemArea.

## 26. Downstream Relationship

The reachable pipeline is:

    DependencyDeclaration
    + DependencySemanticCategoryEvidence
    + RepositorySourceFile[]
        ↓
    future reference-detection contract
        ↓
    DependencySourceReferenceEvidence
        ↓
    ProducedWorkEvidence decision
        ↓
    RepositoryProblemAreaEvidence
        ↓
    ProblemArea promotion
        ↓
    P7

This contract completes no downstream stage. `ProducedWorkEvidence` remains
unresolved and `RepositoryProblemAreaEvidence` remains blocked by the Session
067 contract.

## 27. Prohibited Semantics

This surface must never produce or support:

- a repository score contribution,
- signal creation,
- weighting, confidence or ranking,
- runtime-use inference,
- production-use inference,
- dependency-reference inference,
- `ProducedWorkEvidence` inference,
- ProblemArea inference,
- developer skill, understanding or capability inference,
- seniority, employability, hire or no-hire inference,
- authorship or originality inference,
- locale-dependent semantics,
- LLM-derived semantic evidence.

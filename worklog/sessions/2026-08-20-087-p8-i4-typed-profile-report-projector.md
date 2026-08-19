# P8-I4 — Typed Profile Report and Projector

**Date:** 2026-08-20
**Result:** PASS — P8-I4 slice complete, including a post-review bounded
correction (1 MEDIUM finding + 1 test-adequacy gap, both corrected in-slice).
Additive Application implementation; no canonical docs changed; no Domain
file modified.
**Scope:** Application typed report contract + deterministic one-way
projector from `DeveloperPublicEvidenceProfile` to `ProfileReport`
**Model Route:** sonnet / medium (BOUNDED — contract fully locked by the
user-supplied handoff, no new semantic decision required)
**Model Gate:** APPROVED — user wrote `devam et`

---

## Verified Baseline

```text
branch (at gate)  main
working tree      clean
HEAD              09037be  feat(profile): compose deterministic public evidence profile
origin/main       09037be  (identical)

P8-I1  COMPLETE
P8-I2  COMPLETE
P8-I3  COMPLETE
```

Branched as `session/087-p8-i4-typed-profile-report-projector` from `main` @
`09037be`. Per the user's explicit instruction, this session did not perform
broad repository research or reread historical worklog sessions — the
handoff itself carried the complete, already-locked contract (section
set/order, availability semantics, per-section field requirements,
prohibited-semantics list, and the exact required test list). Context read
was narrowly targeted: `docs/architecture/public-profile-evidence-report-contract.md`
§5–§13 (canonical sections, availability, ordering, per-section field
requirements, warnings boundary) and the already-known `DeveloperPublicEvidenceProfile`/
`RepeatedProfilePattern`/`RepresentativeRepository`/`ProfileLimitation`/
`ProfileWarning` Domain shapes carried over from this session's own prior
P8-I3 work in the same conversation.

---

## What Was Built

Nine new Application files (`src/GitHubProof.Application/Profile/Report/`),
purely additive:

```text
add  ProfileReportSectionKey.cs           (9 canonical section identities, in order)
add  ProfileReportSectionAvailability.cs  (Supported | Unsupported | Deferred)
add  ProfileReportSectionDescriptor.cs    (SectionKey + Availability, type-erased)
add  ProfileReportSection.cs              (generic ProfileReportSection<TItem>)
add  ProfileSummaryFact.cs                (structured summary fact: section + count)
add  PatternReportItem.cs                 (RepeatedProfilePattern + representative ref)
add  ConclusionEvidenceReportItem.cs      (pattern -> flattened evidence trace)
add  RepositoryDrillDownReportItem.cs     (repository + eligibility status + analysis ref)
add  ProfileReport.cs                     (the nine-section aggregate + Warnings)
add  ProfileReportProjector.cs            (the one-way projection)
```

Two new test files
(`tests/GitHubProof.Application.Tests/Profile/Report/`):

```text
add  ProfileReportTestSupport.cs      (direct DeveloperPublicEvidenceProfile fixtures)
add  ProfileReportProjectorTests.cs   (28 tests — 26 initial + 2 from the
                                        post-review bounded correction below)
```

### Exact Report Shape

```text
ProfileReport
├── PublicProfileSummary          : ProfileReportSection<ProfileSummaryFact>       — Supported
├── TechnicalFocusAreas           : ProfileReportSection<PatternReportItem>        — Supported
├── EngineeringPracticePatterns   : ProfileReportSection<PatternReportItem>        — Supported
├── ProblemAreaPatterns           : ProfileReportSection<PatternReportItem>        — Unsupported (always [])
├── RepresentativeRepositories    : ProfileReportSection<RepresentativeRepository> — Supported
├── WhyThisConclusion             : ProfileReportSection<ConclusionEvidenceReportItem> — Supported
├── Limitations                   : ProfileReportSection<ProfileLimitation>        — Supported
├── RepositoryDrillDown           : ProfileReportSection<RepositoryDrillDownReportItem> — Supported
├── RoleEvidenceAlignment         : ProfileReportSection<RoleEvidenceAlignment>    — Deferred (always [])
├── Warnings                      : IReadOnlyList<ProfileWarning>  (canonical root truth; NOT a 10th section)
└── Sections                      : IReadOnlyList<ProfileReportSectionDescriptor>  (the 9, in canonical order)
```

`RepresentativeRepositories` and `Limitations` sections carry the exact
Domain objects (`RepresentativeRepository`, `ProfileLimitation`) by
reference — no report-owned duplicate type, no reselection, no re-derivation.
`RoleEvidenceAlignment`'s item type is the Domain placeholder itself
(`GitHubProof.Domain.Profile.Models.RoleEvidenceAlignment`, private
constructor), so its section is structurally incapable of ever carrying an
item — the deferral is compile-enforced, not just conventionally empty.

### Exact Availability Implementation

`ProfileReport`'s constructor hard-locks each section's expected
`ProfileReportSectionKey` and `ProfileReportSectionAvailability` (throws
`ArgumentException` on any mismatch) — the V1 availability table from the
handoff is enforced at construction, not left to projector discipline alone.
`ProblemAreaPatterns` additionally throws if given any non-empty item list
(ProblemArea has no V1 promotion rule at all, so this is unconditional).
`RoleEvidenceAlignment` needs no equivalent runtime guard: its item type has
no public constructor, so a non-empty list is impossible to construct.

### Pattern Projection

`ProfileReportProjector.Project` reads `profile.RepeatedProfilePatterns`
(the single canonical pattern source) once, filters by
`ProfilePatternFamily` for `TechnicalFocusAreas`/`EngineeringPracticePatterns`,
and never reads `ProfilePatternFamily.ProblemArea` at all — proven by a test
that plants a `ProblemArea`-family pattern in the upstream `ProfilePatternSet`
and asserts `ProblemAreaPatterns` still projects `Unsupported + []`. Each
`PatternReportItem` wraps the `RepeatedProfilePattern` itself (family, key,
independent count, supporting/independent-supporting repository references
all reachable through it) plus an optional representative-repository
reference, resolved by indexing `profile.RepresentativeRepositories` by
pattern **reference** (`ReferenceEqualityComparer`) — the same "Canonical
Pattern Trace" technique `ProfileComposer` already uses (Session 086), so a
representative is attached only when it genuinely traces to this exact
pattern instance.

### Ordering

The projector introduces no sort anywhere (verified: no `OrderBy`/`.Sort(`
in any new file). Every section's item order is exactly the order the
canonical root already provides: `RepeatedProfilePatterns` (Family then
PatternKey, already sorted by `ProfilePatternSet`), `RepresentativeRepositories`
(already sorted by `RepresentativeRepositorySelection`), `Limitations`
(already sorted by `ProfileCondition`'s canonical key), `RepositoryEligibilityResults`
(already sorted Owner-then-Name). `WhyThisConclusion`'s per-pattern evidence
list is `pattern.SupportingObservations.SelectMany(o => o.EvidenceReferences)`
— both already canonically sorted by the `RepeatedProfilePattern`/
`ObservedProfileEvidence` constructors.

### Localization Boundary

No localization resource files, no localized-label field, added in this
slice — the typed contract carries only canonical semantic identity
(`PatternKey`, `Family`, references). Locale independence is therefore
structural rather than merely tested-and-hoped: there is no field in the
output shape locale could change yet. Verified by projecting the same
profile under `en-US` and `tr-TR` (swapping `CultureInfo.CurrentCulture`/
`CurrentUICulture`) and asserting identical section order/content.

### Warnings

Carried on `ProfileReport.Warnings` as the exact `profile.Warnings` list —
never counted, never converted into a limitation, never a tenth entry in
`Sections`.

---

## Technical Choices Made

1. **Reuse Domain types directly as report items where no additional field
   is needed.** `RepresentativeRepository`, `ProfileLimitation`,
   `RoleEvidenceAlignment` are used as-is for their sections' `TItem`. This
   is the strongest possible guarantee against "reselect/rerank/re-derive":
   the report literally carries the same object.
2. **`ProfileReportSectionDescriptor` as a type-erased order/availability
   view.** `Sections` needed one homogeneous list across nine differently
   typed sections; a small record (`SectionKey` + `Availability`) avoids a
   non-generic base class or reflection over the nine typed properties.
3. **`ProfileSummaryFact` is (`ReferencedSection`, `ReferencedSectionAvailability`,
   `ItemCount`) — no prose, no string.** Directly satisfies "every semantic
   conclusion must be traceable to a report section" mechanically: a test
   cross-checks every fact's `ItemCount` against the actual referenced
   section's `Items.Count`.
4. **No locale parameter added to the projector.** Since nothing in the V1
   output depends on locale, adding an unused parameter would be dead
   surface. Locale-independence is instead proven by driving
   `CultureInfo.CurrentCulture`/`CurrentUICulture` around the call.
5. **`RepositoryDrillDownReportItem` includes `RepositoryEligibilityStatus`.**
   Not in the handoff's minimum field list, but it is an existing
   canonical, already-locked P2 field (not a new judgment) and is needed for
   the drill-down item to be self-explanatory without a second lookup — kept
   bounded rather than omitted or expanded further.

---

## Validation

Initial implementation validation (before the post-review correction below):

```text
targeted  dotnet test --filter FullyQualifiedName~Profile.Report
          26 / 26 PASS

Application  dotnet test tests/GitHubProof.Application.Tests
             149 / 149 PASS (123 baseline + 26 new)

build     dotnet build GitHubProof.slnx — 0 warning / 0 error

full suite (once, at the end)
          Domain          798 / 798   (unchanged)
          Infrastructure  255 / 255   (unchanged)
          Application     149 / 149   (123 baseline + 26 new)
          Web              68 /  68   (unchanged)
          total          1270 / 1270

git diff --check   clean
```

Final validation, after the post-review correction (see below), is recorded
under "Post-Review Correction".

The 26 tests cover all 24 required scenarios from the handoff (several
scenarios combined into one assertion-rich test where they shared a fixture,
e.g. representative-repository order-preservation and non-reselection are one
test): exactly nine sections in canonical order, order unchanged across
`en-US`/`tr-TR`; `TechnicalFocusAreas`/`EngineeringPracticePatterns` both
`Supported` populated and `Supported` empty; `ProblemAreaPatterns` exactly
`Unsupported + []` even when a `ProblemArea` pattern exists upstream;
`RoleEvidenceAlignment` exactly `Deferred + []`; `Unsupported != Deferred`;
all nine sections structurally present regardless of availability;
representative-repository order preserved and object-identical (not
reselected); pattern order preserved out of a deliberately non-alphabetical
construction order; limitation order preserved; warnings present as
structured truth without becoming a tenth section; repository drill-down
uses `RepositoryAnalysisReference` only; `WhyThisConclusion` traces pattern
evidence exactly; summary facts traceable to their referenced sections;
eligibility-list constructor order does not alter drill-down output order;
repeated projection of the same profile is semantically identical; no report
type exposes a bare `DateTime`/`DateTimeOffset`; and a reflection guard
across all five non-reused report types for the full prohibited-semantics
word list from the handoff.

---

## Self-Review Against the Brief

Per the user's explicit instruction, no broad `/github-proof-review` was
launched. Self-review confirmed, file by file:

- No Domain file changed (`git status --short` shows only the two new
  `Report/` directories).
- No re-run of eligibility/observation/pattern-promotion/representative-
  selection/composition anywhere in `ProfileReportProjector` — it only reads
  already-computed collections off the given `DeveloperPublicEvidenceProfile`.
- No `OrderBy`/`.Sort(`/`DateTime.Now`/`DateTimeOffset.UtcNow`/`DateTimeOffset.Now`
  anywhere in the nine new `src/` files (grepped).
- No prohibited-semantics word (score, rank, quality, confidence, capability,
  seniority, employability, job fit, hire, reject, inferred role,
  years-of-experience, authorship, originality, narration, prose, UI state)
  appears on any report type's public surface (reflection-tested).

No new semantic gap was encountered. No STOP condition was triggered — the
handoff's contract was sufficient to implement without inventing or
reopening any product/domain decision.

---

## Post-Review Correction

A subsequent bounded review of the initial implementation (above) raised one
MEDIUM finding plus a test-adequacy gap. Both were corrected in the same
slice, without reopening any semantic decision and without modifying Domain.

### Finding — MEDIUM: Unsupported ProblemArea patterns could leak into WhyThisConclusion

`ProblemAreaPatterns` correctly projected `Unsupported + []`, but
`WhyThisConclusion` read `profile.RepeatedProfilePatterns` unfiltered — so an
upstream `ProblemArea`-family pattern (however it got into the pattern set)
could still surface as a supported `WhyThisConclusion` item, violating the
V1 presentation capability boundary (ProblemArea has no taxonomy or
promotion rule in V1 at all).

**Correction** (`ProfileReportProjector.cs`): `WhyThisConclusion` now
projects only the V1 supported pattern-conclusion families —
`ProfilePatternFamily.TechnicalFocus` and
`ProfilePatternFamily.EngineeringPractice` — via one added `.Where(...)`
clause. `ProfilePatternFamily.ProblemArea` remains excluded, matching
`ProblemAreaPatterns`' own boundary. The existing upstream canonical order
(`RepeatedProfilePatterns`, Family then PatternKey) is preserved unchanged;
no sort was introduced; no pattern was recomputed; `ProblemAreaPatterns`
itself was not touched.

**Regression proof:** added
`WhyThisConclusion_ExcludesUnsupportedProblemAreaPatterns` — builds a profile
containing only an upstream `ProblemArea` pattern and asserts all of:
`ProblemAreaPatterns.Availability == Unsupported`, `ProblemAreaPatterns.Items`
empty, `WhyThisConclusion.Items` empty, and the `PublicProfileSummary` fact
referencing `WhyThisConclusion` has `ItemCount == 0`. Verified empirically in
both directions: the test was run against the pre-fix projector (temporarily
reverted the `.Where(...)` clause) and failed exactly as expected
(`Assert.Empty` failure, one `ConclusionEvidenceReportItem` carrying the
`ProblemArea` pattern present in the collection); the file was then restored
byte-identical (`diff` confirmed) and the test passed. The existing positive
trace test for a supported EngineeringPractice pattern
(`WhyThisConclusion_TracesPatternToRepositoryEvidence`) was left unchanged
and still passes.

### Finding — Test adequacy: locale test verified only section ordering

The original locale test (`Project_SectionOrderUnchangedByLocale`) proved
only that `en-US`/`tr-TR` produce the same `Sections` order — not that
locale leaves item-level semantic content unchanged.

**Correction:** added
`Project_UnderDifferentCultures_ProducesSemanticallyIdenticalReport` — builds
a non-trivial profile (two eligible repositories, one `TechnicalFocus` and
one `EngineeringPractice` pattern producing representative repositories, two
limitations, one warning), projects it under `en-US` and `tr-TR`
(`CultureInfo.CurrentCulture`/`CurrentUICulture` swapped), and asserts
equality of: `Sections` (key + availability), pattern item family/key
identity for both pattern sections, `RepresentativeRepositories.Items`
(value-equal — the report carries the same upstream objects), `Limitations`
identity/order, `Warnings` identity (`ProfileWarning` has value equality:
code + scope + affected identities), `RepositoryDrillDown` repository
membership/order, and `WhyThisConclusion` pattern identity plus
`SupportingEvidence` element-wise (compared as sequences rather than as a
tuple component, since the containing list is rebuilt fresh per projection
call even though its elements are the same upstream `ProfileEvidenceReference`
objects). No localization resources were added; no new production
localization behavior was introduced — the test proves culture cannot alter
semantic projection, which the empty-of-presentation-text V1 shape already
guarantees structurally.

### Final Validation (after correction)

```text
targeted     dotnet test --filter FullyQualifiedName~Profile.Report
             28 / 28 PASS

Application  dotnet test tests/GitHubProof.Application.Tests
             151 / 151 PASS

build        dotnet build GitHubProof.slnx — 0 warning / 0 error

full suite (once)
             Domain          798 / 798
             Infrastructure  255 / 255
             Application     151 / 151
             Web              68 /  68
             total          1272 / 1272

git diff --check   clean
```

### Final Review State

```text
CONTRACT COMPLIANCE:  PASS
TEST ADEQUACY:        PASS
Remaining findings:   0 BLOCKER / 0 HIGH / 0 MEDIUM / 0 LOW
New semantic decision required: NONE
```

---

## Out of Scope — Confirmed Absent

No localization resource files, no default-presentation/UI omission logic
(§6.5 of the report contract — deliberately deferred, all nine sections stay
structurally present), no orchestration wiring, no persistence, no P9/P10,
no rendering/UI.

---

## Working Set / Final Git State

```text
Untracked: src/GitHubProof.Application/Profile/Report/**  (10 files)
           tests/GitHubProof.Application.Tests/Profile/Report/**  (2 files)
Pre-existing unrelated: none
```

No `docs/` file changed. No `src/GitHubProof.Domain/**` file changed.

---

## Next

```text
P8-I5 — Integration and P8 Closure
```

Depends on P8-I4 (this slice). Full slice plan:
`worklog/sessions/2026-08-19-083-p8-implementation-planning.md`. P8-I5 is a
separate meaningful session and must be routed through its own model/effort
gate.

---

## Git

```text
Suggested commit:
feat(profile): implement typed profile report and projector

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
```

Claude did not commit, push, merge, rebase, tag, release or deploy.

---

## Completion Checklist

```text
[x] Goal tamamlandı: ProfileReport + ProfileReportProjector implementasyonu, self-review PASS
[x] Post-review bounded correction tamamlandı (1 MEDIUM + 1 test-adequacy gap, ikisi de düzeltildi)
[x] Targeted validation çalıştı (Profile/Report 28/28)
[x] Application test project çalıştı (151/151)
[x] Full build + full suite bir kez çalıştı (1272/1272, 0 warning/0 error)
[x] git diff --check kontrol edildi
[x] git status gözden geçirildi
[x] Contract/invariant drift kontrol edildi — none found
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Kullanıcıya kısa final rapor verildi (Türkçe)
```

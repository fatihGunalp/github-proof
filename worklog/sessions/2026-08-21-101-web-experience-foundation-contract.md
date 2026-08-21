# Session 101 — Web Experience Foundation Contract

**Date:** 2026-08-21
**Result:** COMPLETE / PASS
**Model Route:** Sol / medium
**Branch:** `session/101-web-experience-foundation-contract`
**Base HEAD:** `7a4ec151f4d837934f6ba7fdef58026985465cab`
**Pre-existing Working Tree:** `?? artifacts/` — user-owned, out of scope,
untouched

## Goal

Reconcile the Web analysis-in-progress experience and the English/Turkish
request-language contract before P10-I3, without changing production code,
tests, analysis semantics or Profile report architecture.

## Audit Result

Read-only audit: **PASS WITH FINDINGS**.

Five approved canonical drift surfaces:

1. `docs/product/p10-web-profile-flow-implementation-plan.md`
2. `docs/product/p10-presentation-language-contract.md`
3. `docs/product/report-information-architecture.md`
4. `docs/product/github-proof-design-principles-v1.md`
5. `docs/product/github-proof-ui-system-light-dark-v1.md`

## Locked Decisions

1. **New prerequisite slice:** `P10-I2A — Web Experience Foundation` is
   inserted after complete P10-I2 and before P10-I3; no existing slice is
   renumbered. P10-I2A must complete before P10-I3 begins.
2. **Web language support:** exactly `en` and `tr`; resolution is persistent
   explicit user choice → browser `Accept-Language` preference/q ordering →
   English fallback. Cookie provider precedes Accept-Language; ordinary query
   string culture selection is unnecessary. Geolocation/IP/device/browser
   location never participates. No external localization package is expected.
3. **Whole Web surface:** selected request culture owns `<html lang>`, header
   utilities, selector/theme accessibility copy, landing/input/validation,
   processing state, Repository/Profile presentation, failures,
   limitations/warnings and accessibility messages. Literal GitHub/evidence
   values remain unchanged.
4. **Profile localization boundary:** `ProfileReportProjector` remains
   locale-neutral. Web maps existing structured keys/codes deterministically
   into EN/TR. TechnicalFocusAreas and EngineeringPracticePatterns remain
   separate; ProblemAreaPatterns remains Unsupported/unrendered;
   RoleEvidenceAlignment remains Deferred/unrendered.
5. **Repository localization:** existing Application EN/TR catalogs and
   `RepositoryReportProjector` remain canonical. P10-I2A replaces Web's
   hard-coded `locale: "en"` with resolved request UI culture; analysis is not
   rerun. The stale `tr = first-launch locale` rule is removed.
6. **Language selector:** quiet persistent `EN | TR` utility beside theme,
   accessible and non-primary. On transient POST results, switching persists
   culture and returns to localized landing without hidden re-analysis,
   persistence or caching; `GitHubTarget` may be safely prefilled and a new
   analysis requires explicit Analyze.
7. **Analysis-in-progress:** immediate client-side indeterminate state begins
   only after native constraint validation succeeds, prevents accidental
   duplicate submit, preserves `GitHubTarget` serialization, uses
   `aria-busy` plus a polite atomic `role=status` live region, resets on
   back/BFCache and respects reduced motion. No percentage, ETA, fake stage,
   SSE, SignalR, polling, streaming or new progress telemetry.
8. **Truthful copy:** EN/TR primary processing copy and optional static
   “what this analysis includes” descriptions are allowed only as neutral
   general descriptions. They are never completed/active/pending sequential
   runtime states. Existing `ProfileExecutionTelemetry` is not a live UI
   progress source.

Required Profile labels are locked as:

| Identity | EN | TR |
|---|---|---|
| TechnicalFocusAreas | Visible technical direction | Görünen teknik yön |
| EngineeringPracticePatterns | Recurring engineering practices | Tekrar eden üretim pratikleri |
| RepresentativeRepositories | Starting point | Başlangıç noktası |
| WhyThisConclusion | Why this view | Bu görünümü neden görüyoruz? |
| Profile Report | Profile view | Profil görünümü |

## Exact Changes

- P10 current execution sequence now includes P10-I2A with explicit scope and
  exit criteria; I3/I4/I5/I6 responsibilities are preserved.
- Fixed-English current claims are superseded while Session 093 is retained
  only as clearly marked historical context.
- EN/TR resolution, persistence, whole-Web ownership, Profile/Repository
  boundaries, transient-result behavior and truthful processing copy are
  canonicalized.
- Design navigation/progress/accessibility contracts now describe the quiet
  selector and indeterminate loading state supported by the current runtime.

## Intentionally Unchanged

- `docs/architecture/`, `docs/roadmap/`, all other `docs/product/`
- `src/`, `tests/`, `AGENTS.md`, `agents/`, `CLAUDE.md`, `artifacts/`
- Domain, evidence, scoring, eligibility and report semantic identities
- Repository Application localization catalogs/projector
- `ProfileReportProjector` locale-neutral boundary
- P10-I5 final partial/failure semantic responsibility
- execution guard and concurrent-profile-execution policy

No production code or tests were changed. No build or test was run. No commit
or push was performed.

## Validation

- branch/base/worktree baseline verified before editing,
- exact authorized changed-file scope check: **PASS**,
- protected surfaces unchanged: **PASS**,
- stale-current contract scan completed; retained fixed-English and six-slice
  references are explicitly historical Session 093 records,
- `git diff --check`: **PASS**,
- no build/test was run because this session is docs/worklog-only.
- RepositoryPicker / Repository picker / Select repository / Show all
  repositories scan completed; retained discovery/picker wording is explicitly
  historical or explicitly denies a current entry step, while repository-list
  concepts are scoped to Profile Proof evidence exploration / P10-I4
  drill-down.

## Independent Review

Initial independent ChatGPT review:

```text
PASS WITH FINDINGS
0 BLOCKER / 0 HIGH / 1 MEDIUM / 1 LOW
```

- **F1 — MEDIUM:** the two authorized design contracts still expressed the
  current/global product boundary as single-repository-only and retained a
  stale proposed username → repository-discovery flow. Corrected in-session:
  both now define Repository Proof micro and Profile Proof macro explicitly;
  15-signal / 4-category / score/proves/weakens/do-first semantics are scoped
  to Repository Proof, while Profile Proof remains deterministic,
  job-agnostic and does not create a developer/profile score.
- **F2 — LOW:** this Validation section used future-tense wording for checks
  already completed. Corrected in-session with the completed facts above.

F1 and F2 were corrected before the independent re-review below.

Independent ChatGPT re-review:

```text
PASS WITH FINDINGS
0 BLOCKER / 0 HIGH / 1 MEDIUM / 0 LOW
```

- **F3 — MEDIUM:** residual `RepositoryPicker`, `RepositoryRow`, “Select
  repository” and repository-picker composition language could recreate the
  superseded username → choose repository → Repository Proof entry flow.
  Corrected in-session: ambiguous component/composition names now use
  `ProfileRepositoryList` / `RepositoryEvidenceRow`; remaining repository-list
  UI is explicitly Profile Proof evidence exploration / repository drill-down,
  never entry routing. Unified entry and separate Profile macro / Repository
  micro semantics are preserved. Final drill-down mechanics remain P10-I4
  ownership and were not implemented in Session 101.

First-review F1/F2 and re-review F3 were corrected in-session.

Final independent ChatGPT review:

```text
PASS
0 BLOCKER / 0 HIGH / 0 MEDIUM / 0 LOW
```

All findings are closed. No production code or tests changed, and no build or
test was required for this documentation/worklog-only session.

## Next

P10-I2A — Web Experience Foundation implementation. P10-I3 follows P10-I2A.

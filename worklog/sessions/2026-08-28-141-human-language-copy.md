# Human Language Copy Implementation

**Date:** 2026-08-28
**Result:** PASS WITH FINDINGS
**Scope:** web / report presentation / localization
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement the approved Session 141 contract package
(`docs/product/session-141-human-language-copy/`, 18 contracts + README) in
`GitHubProof.Web` and the `GitHubProof.Application` presentation layer: replace
system-like user-facing copy with the approved TR/EN wording, and remove the two
places where localized presentation text drives program semantics — without
changing any canonical Domain/Application analysis truth.

---

## Starting State

~~~text
Branch:
session/141-human-language-copy

Base HEAD:
1268671

Pre-existing Working Tree:
?? artifacts/                                    (user-owned, out of scope)
?? docs/product/session-141-human-language-copy/ (the approved contract package)

Relevant Previous Session:
worklog/sessions/2026-08-28-140-language-switch-analysis-guard.md
~~~

`main` == `origin/main` == `HEAD` == `1268671` verified before branching.
`artifacts/` is not owned by this session and is never staged or modified.

---

## Model Route

~~~text
Selected:
Opus / high

Why:
Session 141 spans the whole Web presentation surface and pairs copy changes
with behavior-preserving refactors that move copy-driven semantic logic onto
typed canonical identities. Many critical semantic distinctions are under
regression risk at once (Missing != UnavailableEvidence, no score != zero
score, warning != limitation, category status != signal status, Profile Proof
!= Repository Proof), so this is COMPLEX rather than INVARIANT_HEAVY.

User Approval:
devam et
~~~

---

## Model Escalation

~~~text
None.
~~~

---

## Context Read

~~~text
AGENTS.md
CLAUDE.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/product/session-141-human-language-copy/README.md
docs/product/session-141-human-language-copy/00-decisions.md .. 17-*.md (all 18)
src/GitHubProof.Web/Localization/*
src/GitHubProof.Web/Pages/**
src/GitHubProof.Application/Analysis/Presentation/**
~~~

---

## Contract / Invariants Checked

~~~text
- Copy follows canonical state; canonical state never comes from copy.
- SignalStatus taxonomy (7) stays distinct from CategoryStatus taxonomy (4).
- Missing => Score 0, denominator included.
- NotApplicable / UnavailableEvidence / RequiresHumanReview => Score null,
  numerator and denominator excluded.
- UnavailableEvidence is never Missing; RequiresHumanReview is never Failed.
- NormalizedScore null => no score rendered; never 0/100.
- ScoreBreakdown.LabelKey thresholds and RawApplicableScoreNormalization
  unchanged.
- RepositoryReportSectionKeys.CanonicalOrder unchanged.
- ProductBoundaryPolicyVersion unchanged (semantic policy unchanged).
- RepositoryEligibilityStatus identities and eligibility evaluation unchanged.
- Live progress state machine PENDING -> ACTIVE -> COMPLETED unchanged;
  no percentage, ETA or count added.
~~~

---

## Changes

~~~text
Created:
- src/GitHubProof.Application/Analysis/Presentation/RepositoryReportPresentationKeys.cs
- src/GitHubProof.Application/Analysis/Presentation/ScoreEvidenceStrength.cs
- src/GitHubProof.Web/Presentation/RepositoryReportNotesPresentation.cs
- tests/GitHubProof.Application.Tests/Analysis/Presentation/Session141ReportLanguageTests.cs
- tests/GitHubProof.Web.Tests/Session141WebLanguageTests.cs

Modified (production):
- Application/Analysis/Presentation: RepositoryReport.cs, RepositoryReportLocalizer.cs,
  RepositoryReportProjector.cs, RepositoryReportMarkdownRenderer.cs,
  Resources/RepositoryReport{,.tr}.resx, Resources/SignalPresentation{,.tr}.resx
- Web/Localization: WebTextKey.cs, WebPresentationLocalizer.cs,
  ProfileWebPresentationLocalizer.cs
- Web/Pages: Profile.cshtml, Shared/_GitHubInput.cshtml,
  Shared/_RepositoryReportIdentity.cshtml, Shared/_RepositoryReportSignals.cshtml,
  Shared/_RepositoryReportUncertainty.cshtml
- Web/Development: RepositoryReportPreviewFactory.cs
- Web/wwwroot: css/report.css, css/repository-input.css, js/analysis-progress.js

Modified (tests): 12 files, assertions moved to the new approved truth.

Deleted:
- two dead WebTextKey members and their catalog entries: ProfileView (its
  Turkish value was literally the prohibited "Profil görünümü") and
  ProfileCollected (removed in the review pass so both dead keys are treated
  the same way). Neither had a consumer.
~~~

---

## Decisions

~~~text
- Score evidence strength became a typed identity. ScoreEvidenceStrength +
  RepositoryReportScoreSemantics.FromLabelKey map the canonical
  ScoreBreakdown.LabelKey to a presentation identity, and the Identity card's
  "Güçlü"/"Strong" substring search is gone. Behavior-preserving: the old
  search matched exactly the strong and very-strong bands and nothing else.
- Warning/limitation identity now travels with the projection.
  RepositoryReportWarning gained PresentationKey; RepositoryReport.Limitations
  became IReadOnlyList<RepositoryReportLimitation> (Key + Message) instead of
  bare localized strings. Nothing canonical was dropped — the projection had
  simply been discarding the key.
- RepositoryReportNotesPresentation decides what the Uncertainty section
  renders, keyed on identity. It reproduces the previous repeat-collapsing and
  adds exactly one authorized suppression (contract 14 §12): the NotApplicable
  limitation when its equivalent warning already rendered. The
  unavailable-evidence and requires-human-review pairs are deliberately NOT
  deduped — contract 14 §5/§10 and §6/§11 keep them semantically distinct.
- Progress copy became state-aware. Each canonical stage carries one sentence
  per PENDING/ACTIVE/COMPLETED state, and the separate Bekliyor/Sürüyor/
  Tamamlandı token span was removed, so the state stays readable as text
  without producing the prohibited "workflow noun · Tamamlandı" combination.
  Stage identities, order, the three runtime states, polling and the live
  region are untouched. USER-APPROVED deviation from p10 §8.3.2/§8.3.3.
- Profile Limitations renders no section at all when the canonical collection
  is empty (contract 10 §7). The Supported-section guard still runs first.
- RepositoryEligibilityUndetermined copy deviates from contract 10 §3's
  suggested wording, using that section's own "Implementation caution" escape.
  The canonical state arises from BOTH EligibilityReason.RequiredInputUnavailable
  AND EligibilityReason.IndependenceUndetermined (RepositoryContributionPolicy),
  and in the second case the analysis succeeded — only fork/template lineage
  stayed inconclusive. "Enough data could not be obtained" would have been a
  stronger claim than the state supports, so the wording drops "eligibility"
  while asserting exactly what the state asserts, and matches contract 11 §7's
  row-status copy for the same canonical state.
- p10-presentation-language-contract.md was NOT edited (explicit user
  decision). Its locked copy sections are now contradicted by code and need a
  separate owner decision; listed under Known Risks.
~~~

---

## Validation

~~~text
dotnet build GitHubProof.slnx
PASS — 0 warnings, 0 errors

dotnet test GitHubProof.slnx
PASS — 2453/2453
  Domain         842/842   (unchanged from base — analysis semantics intact)
  Infrastructure 636/636   (unchanged from base)
  Application    319/319   (+43)
  Web            656/656   (+15)
Session base baseline was 2395/2395.

git diff --check
PASS

Live render smoke (local Development instance, no network/provider call):
- landing renders the approved TR copy
- /report-preview renders the full Repository Proof in TR and EN: canonical
  section order intact, analysis status and evidence summary shown as two
  distinct things, "Oluşturuldu:", signal field labels, approved score and
  status labels, null-score signals render no score at all, raw evidence
  Source/Value/Location verbatim, approved disclaimer
- landing progress markup carries three state sentences per stage and no
  state-token span
~~~

---

## Problems

~~~text
Problem:
The approved Session 141 copy contradicts locked sections of
docs/product/p10-presentation-language-contract.md (CONTRACT LOCKED).

Cause:
p10 is the canonical owner of most of the strings Session 141 replaces, and of
the progress label + state-token shape.

Resolution:
Raised to the user before any code was written. User decision: implement the
code, do NOT edit p10, record the stale sections as a finding for a separate
docs session. Applied as decided.
~~~

---

## Not Done

~~~text
- p10-presentation-language-contract.md not amended (user decision).
- Contract 12 §8 today/Bugün UTC-vs-local presentation-correctness finding was
  deliberately NOT fixed; that contract says to record it, not to change
  timezone behavior inside a copy pass.
- limitation.snapshot.collection-partial left as-is: contract 14 gives it no
  approved replacement and does not assert it duplicates its warning, so no
  dedupe pair was invented for it.
- No live provider run and no Profile analysis against real GitHub.
- Contract 16 §5 gives only a TR "preferred direction" for the README
  whyItMatters line; the matching EN was translated faithfully rather than
  taken from an approved string, because none exists.
- ProfileCouldNotCompose's message is not locked by contract 10 §8 (only its
  title is). The TR change was mandated — it carried the prohibited
  "profil görünümü" — and EN was aligned to it.
~~~

---

## Known Risks

~~~text
- STALE CANONICAL DOCS: p10-presentation-language-contract.md now contradicts
  shipped code in §8.3/§8.3.1 (canonical indeterminate sentence and the TR
  verbal-noun rationale), §8.3.2 (locked stage copy table AND its governing
  "one label serves all three states" rule, now inverted), §8.3.3 (state token
  required per stage), §8.3.6, §8.4.4 (the four Meaning narratives), §8.5
  (Supported+empty copy), §8.6.1b (starting point relationship), §8.6.6 (row
  status), §8.6.7 (why reasons, supporting-repositories lead-in,
  fresh-analysis lead-in, repository action label and accessible name) and
  §8.7.1 (identity context line). Needs an owner decision in a separate docs
  session.
- SECOND CANONICAL OWNER, same decision: rendering no Limitations section when
  the collection is empty (contract 10 §7) also collides with
  docs/architecture/public-profile-evidence-report-contract.md §6.5, which
  states "Omission is never permitted for a `Supported` section on the grounds
  that its content is empty, because its emptiness is a real analytical
  result." p10 §8.5 names this same boundary the **Empty Limitations
  Presentation Contract Gap** and requires "a separately approved presentation
  exception or a change to that canonical boundary". Session 141 contract 10 §7
  is that authorization at the product level, and no canonical data is dropped
  — the typed report still carries the Supported section with empty Items — but
  the architecture contract must be amended by the same owner decision that
  covers p10. Found by independent review; recorded, not silently changed.
- §8.4.4's two explicit non-claim clauses (state 2's "not a claim that any
  practice is absent" and state 4's "not an assessment of the person") are no
  longer rendered, because contract 04 halves the Meaning length. The person
  boundary is still carried by the landing Boundaries section and the report
  disclaimer, but the sentence-level protection is gone; worth the owner's
  attention in the same p10 decision.
- The Starting Point / Repository Evidence action label changed from
  "Bu repository'yi analiz et" to "Repository Proof'u gör". Behavior is
  unchanged (same POST, same fresh analysis), but the new label reads more like
  navigation than like starting work; worth watching in live use.
~~~

---

## Independent Review

~~~text
Forked cold context, Opus / high, author not involved.
Verdict: PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 1, LOW 6.

Independently re-verified: zero Domain/Infrastructure diff; the
"Güçlü"/"Strong" replacement is exactly behavior-preserving against BOTH the
old and the new label sets, including the null-score fallback path; the notes
helper reproduces the old string Distinct on every reachable input; all 7
signal statuses and all 4 category statuses are mutually distinct in both
cultures; raw evidence Source/Value/Location render verbatim; no whole-src
site remains where localized text drives a decision; no test was weakened
(several were strengthened). 196 changed string literals were reverse-checked
against the approved files.

Findings closed in this session:
- MEDIUM-1 — the empty-Limitations omission also collides with
  public-profile-evidence-report-contract.md §6.5 and p10 §8.5's named
  "Empty Limitations Presentation Contract Gap", which Known Risks did not
  mention. Verified against source and recorded; code unchanged (contract 10 §7
  authorizes the behavior, and no canonical data is dropped).
- LOW-1 — dead WebTextKey.ProfileCollected got the opposite treatment to the
  equally-dead ProfileView. Removed, so both dead keys are handled the same way.
- LOW-2 — two guard assertions became vacuous once the enum members were
  deleted (reintroduction is now a compile error). Retargeted to the markup
  hooks that can still regress.
- LOW-3 — dead ternary in a new test. Fixed.

Accepted without change, recorded instead:
- LOW-4 / LOW-5 — two strings where the contract gave a direction rather than
  locked copy (see Not Done).
- LOW-6 — "Bazı veriler eksik" now serves AnalysisStatus.Partial,
  CategoryStatus.Partial and ProfilePartialTitle. All three are
  contract-mandated (12 §2, 15 §3, 10 §1), the taxonomies stay distinct
  elsewhere, and this is not a regression: both statuses previously shared
  "Kısmi".
~~~

---

## Working Set / Final Git State

~~~text
Modified: 32 tracked files (20 production, 12 tests)
Untracked (this session): 5 source/test files + this session file

Pre-existing unrelated, untouched:
- artifacts/
- docs/product/session-141-human-language-copy/
~~~

---

## Next

Owner decision on amending
`docs/product/p10-presentation-language-contract.md` to the Session 141
language, then integrate this branch.

---

## Git

~~~text
Suggested commit:
feat(web): simplify user-facing language without changing analysis truth

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
~~~

---

## Completion Checklist

~~~text
[ ] Goal tamamlandı veya Result doğru final state'e getirildi
[ ] Targeted validation çalıştı
[ ] Gerekliyse broader validation çalıştı
[ ] git diff --check kontrol edildi
[ ] git diff/status gözden geçirildi
[ ] Contract/invariant drift kontrol edildi
[ ] Session Result güncellendi
[ ] INDEX aynı satır güncellendi
[ ] CURRENT.Active Session güncellendi
[ ] CURRENT.Next Immediate Action güncellendi
[ ] Kullanıcıya kısa final rapor verildi
~~~

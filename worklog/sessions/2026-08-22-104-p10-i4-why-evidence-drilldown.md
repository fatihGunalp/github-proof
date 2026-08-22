# Session 104 — P10-I4 Why + Evidence Drill-Down

**Date:** 2026-08-22
**Result:** PASS / COMPLETE
**Implementation:** COMPLETE
**Visual + Interaction Gate:** COMPLETE / PASS
**Scope:** docs / web / presentation / validation
**Model Route:** opus / high -> sonnet / high (see Model Route)
**Model Gate:** APPROVED — user wrote `devam et` for each phase

## Goal

Close the P10-I4 Why + Repository-evidence presentation/interaction contract
gap via reconciliation, then implement P10-I4: render `WhyThisConclusion` and
`RepositoryDrillDown` as a progressively disclosed evidence layer after the
existing P10-I3 first-read result, with a deterministic POST mechanism that
reaches the existing Repository Proof entry — with no new analysis truth.

## Starting State

~~~text
Branch:
session/104-p10-i4-why-evidence-drilldown

Base HEAD:
f0fc165745e888321c270d59ac206bb93041c430

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-21-103-p10-i3-meaning-direction-starting-point.md
~~~

`artifacts/` is user-owned, out of scope and remains untouched throughout.

## Model Route

~~~text
Phase 1 — Contract reconciliation + canonical documentation amendment:

Selected:
opus / high

Why:
Turn reconciled and locked a permanent interaction/presentation contract
(disclosure shape, POST mechanism, exhaustive RepositoryEligibilityStatus
mapping, EvidenceKey exclusion) across ~10 existing files and two canonical
docs; a wrong reconciliation could have authorized an unsafe interaction
(e.g. Repository.Url as an href) or unapproved inference semantics.

User Approval:
devam et (after three correction rounds: BLOCKER+MEDIUM+LOW on the exact-diff
review, then two further narrow consistency fixes, each re-validated)

Phase 2 — Implementation + visual/interaction validation (this turn's
predecessor turns):

Selected:
sonnet / high

Why:
All semantic and interaction decisions were already closed and independently
reviewed before this phase started. Remaining risk was implementation risk
against many simultaneously-held invariants (AnalysisReference never read,
EvidenceKey never rendered, Repository.Url never an href, every
RepositoryDrillDown item retained unfiltered), not new semantic judgment.

User Approval:
devam et, then explicit `/model claude-sonnet-5`
~~~

## Model Escalation

None.

## Context Read

- `CLAUDE.md`
- `docs/product/p10-presentation-language-contract.md` (full; then §8.5/§8.6 as amended)
- `docs/product/p10-web-profile-flow-implementation-plan.md` (§5, §7/P10-I4, §13.5, §13.6, §13.8, §13.14-§13.16, §13.19)
- `docs/product/product-experience-principles.md`
- `docs/architecture/public-profile-evidence-report-contract.md` (§3, §7, §11, §12, §16)
- `docs/architecture/public-profile-evidence-repository-eligibility.md` (`RepositoryEligibilityStatus` semantics)
- `src/GitHubProof.Application/Profile/Report/ConclusionEvidenceReportItem.cs`, `RepositoryDrillDownReportItem.cs`
- `src/GitHubProof.Domain/Profile/ValueObjects/ProfileEvidenceReference.cs`, `RepositoryAnalysisReference.cs`
- `src/GitHubProof.Domain/Profile/Models/RepositoryContributionAssessment.cs`, `RepositoryEligibilityResult.cs` (reachability proof for the status/AnalysisReference finding)
- `src/GitHubProof.Web/Pages/Index.cshtml(.cs)`, `_GitHubInput.cshtml`, `Shared/_RepositoryReportSignals.cshtml` (existing disclosure/POST conventions)
- `src/GitHubProof.Web/wwwroot/js/analysis-progress.js`, `wwwroot/css/tokens.css`, `components.css`

## Contract / Invariants Checked

- `WhyThisConclusion` items are exactly the same promoted patterns already
  rendered by `TechnicalFocusAreas`/`EngineeringPracticePatterns`; the Why
  layer adds only the supporting-repository trace beneath them.
- Why entry renders pattern label + `Pattern.SupportingRepositories` only —
  never `IndependentSupportingRepositories`, never `SupportingEvidence`,
  never `EvidenceKey`/`EvidenceKeys`, never a count.
- `RepositoryDrillDown` renders **every** item in canonical projector order —
  never filtered to Why participants, never sorted/ranked/grouped/searched/
  paginated/counted.
- Presentation reads `RepositoryEligibilityStatus` only; `AnalysisReference`
  is never read for any presentation decision (proven unreachable-useful:
  `Contributing`/`Restricted` always carry a reference, the other two never
  need one for a label).
- `Repository.Url` is the posted `GitHubTarget` value only — never an `href`;
  exactly one explicit POST to the existing `/Index` entry, no GET-triggered
  analysis, no query string, no JS auto-submit, normal Razor anti-forgery.
- `RepresentativeRepositories` / Starting Point stays text-only in P10-I4; no
  Repository Proof action added to it.
- `Supported`-but-empty renders an ordinary `h2` section, never an empty
  `details`/`summary`.
- No score/count/rank/quality/confidence/seniority/role/capability/
  employability language anywhere in the new surface.
- `ProblemAreaPatterns`/`RoleEvidenceAlignment` remain omitted;
  `Limitations` (position 7) is not yet rendered — a slice-sequencing state,
  not a canonical reordering.
- No new Domain/Application/analysis semantic identity created merely to key
  a fixed presentation slot (`ProfileWebPresentationLocalizer` owns those
  four fixed P10-I4 strings directly, keyed only by culture).
- `ProfileReportProjector`, Domain, Application, `Profile.cshtml.cs`,
  `Index.cshtml(.cs)`, `_GitHubInput.cshtml`, existing `_Repository*.cshtml`,
  `LiveRepositoryReportService`, `analysis-progress.js` unchanged.

## Changes

~~~text
Contract reconciliation (docs, three correction rounds — see Problems):

Modified:
- docs/product/p10-presentation-language-contract.md
  (new §8.6 P10-I4 Why + Repository Evidence Presentation Contract; §8.5
  scope widened to P10-I3/P10-I4 sections with two new empty-copy rows and
  the "no empty disclosure control" rule; §8.6.7 keying precision correction)
- docs/product/p10-web-profile-flow-implementation-plan.md
  (§7/P10-I4 mechanics corrected — the previous "prefill/redirect" wording
  was invalid; §13.6/§13.14 corrected to the exact POST mechanism and to
  withdraw AnalysisReference from presentation; §13.8 narrow amendment for
  fixed-slot copy; new §13.19 cross-reference + Limitations sequencing note)

Implementation:

Modified:
- src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs
  (two new Supported+empty rows; GetRepositoryStatusLabel; four narrow fixed-
  slot methods: GetWhySupportingRepositoriesLeadIn,
  GetRepositoryFreshAnalysisLeadIn, GetRepositoryActionLabel,
  GetRepositoryActionAccessibleName)
- src/GitHubProof.Web/Pages/Profile.cshtml
  (two new partial calls after Starting Point; comment updated)
- src/GitHubProof.Web/wwwroot/css/profile-result.css
  (bounded disclosure/evidence-row rules from existing tokens only, plus the
  disclosure-marker/H2 alignment correction — see below)
- tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs
- tests/GitHubProof.Web.Tests/ProfileWebPresentationLocalizerTests.cs
  (one stale P10-I3-era test corrected: WhyThisConclusion legitimately gained
  Supported+empty copy in P10-I4, so it was removed from the
  "has no mapping" negative-test InlineData)

Created:
- src/GitHubProof.Web/Pages/Shared/_ProfileWhyThisView.cshtml
  (model: ProfileReportSection<ConclusionEvidenceReportItem>)
- src/GitHubProof.Web/Pages/Shared/_ProfileRepositoryEvidence.cshtml
  (model: ProfileReportSection<RepositoryDrillDownReportItem>)

Visual correction (found by a separate pixel visual review of the rendered
`torvalds` surface, not by the earlier implementation code review — see
Validation/Review Sequence; 1 MEDIUM finding):

Modified:
- src/GitHubProof.Web/wwwroot/css/profile-result.css
  (narrowly scoped rule: `.profile-result__disclosure-summary
  .profile-result__section-title { display: inline; margin: 0; }` — puts the
  native disclosure marker and the H2 on the same line; no change to
  details/summary semantics, no custom marker, no JS, no new token)
~~~

## Decisions

- Repository evidence rows and Why supporting-repository lists both reuse
  the existing `.profile-result__repository-list` container class from
  P10-I3 rather than introducing a parallel list class, since the base list
  behavior (flex column, gap, no bullet) is identical; only the row content
  differs.
- The disclosure-marker/H2 alignment fix was scoped to
  `.profile-result__disclosure-summary .profile-result__section-title` only
  — the shared `.profile-result__section-title` class used by non-disclosure
  (ordinary/empty-state) sections was left untouched, so P10-I3's rendered
  empty-state headings are unaffected.
- The stale P10-I3-era localizer test asserting `WhyThisConclusion` has no
  Supported+empty mapping was corrected rather than left to fail: that
  assumption was accurate under P10-I3 (before §8.5 was widened) and became
  incorrect the moment P10-I4 legitimately added the mapping — not a defect
  in the new code.

## Validation

~~~text
dotnet build src/GitHubProof.Web/GitHubProof.Web.csproj -c Debug
PASS — 0 warnings, 0 errors

dotnet test tests/GitHubProof.Web.Tests/GitHubProof.Web.Tests.csproj
  --filter "ProfileResultPresentationTests|ProfileWebPresentationLocalizerTests|ProfileModelTests|CssArchitectureTests"
PASS — 72/72 (implementation turn), 16/16 (disclosure-CSS-correction turn,
  ProfileResultPresentationTests + CssArchitectureTests only)

dotnet test tests/GitHubProof.Web.Tests/GitHubProof.Web.Tests.csproj
PASS — 252/252 (was 230/230 before this session; +22 new P10-I4 tests)

dotnet test (full solution)
PASS — 1631/1631
  Domain 834/834, Application 257/257, Web 252/252, Infrastructure 288/288
  (Domain/Application/Infrastructure unchanged — no code there changed)

dotnet build --configuration Release
PASS — 0 warnings, 0 errors (re-confirmed after the disclosure-CSS correction)

git diff --check
PASS — exit 0 (informational LF->CRLF autocrlf notices only, not errors)
~~~

### Review Sequence

Two separate review events occurred, in this order; their findings must not
be conflated:

~~~text
1. Implementation code review:
   PASS WITH FINDINGS — 0 BLOCKER / 0 HIGH / 0 MEDIUM / 1 LOW
   LOW: potential mobile overflow with a very long repository FullName.

2. Pixel visual review (separate and later; real screenshots of the
   rendered `torvalds` Profile Proof surface):
   PASS WITH FINDINGS — 0 HIGH / 1 MEDIUM / 0 LOW
   MEDIUM: native disclosure marker rendered on its own line above its H2,
   separating the disclosure affordance from the heading it controls.
~~~

The MEDIUM marker finding did **not** originate in the implementation code
review. The bounded CSS correction (see Changes) closed the MEDIUM finding.
The later real TR mobile pixel review separately closed the prior LOW
overflow risk (repository names/status/action wrapped correctly, no
overflow).

### Live validation

Live browser validation ran against the real application (`dotnet run`, no
fabricated data), across two evidence sources:

~~~text
Assistant's own live DOM/server-log evidence this session (`octocat`):

- TR desktop / dark, real composed profile (Partial status,
  RepositoriesDiscovered=8): Meaning/Direction/Starting-point first read
  confirmed unchanged; Why disclosure genuinely collapsed by default
  (get_page_text showed nothing until inspected via the accessibility tree);
  Why entry showed pattern "Issue kullanımı" + locked lead-in + 2 real
  supporting repositories, no counts/scores/IDs/EvidenceKey; Repository
  evidence showed all 8 real rows with three of the four possible statuses
  genuinely represented (Restricted/NonContributing/Undetermined) and a
  correct locked action accessible name per row; each row's form carried a
  real `__RequestVerificationToken` (framework anti-forgery confirmed live).
- Repository-evidence POST mechanism: activating "Analyze this repository"
  POSTed to the app's own `/` (Index) endpoint (never github.com), reached
  the unmodified `IndexModel.OnPostAsync`, and issued a real
  `GET https://api.github.com/repos/octocat/Hello-World` call with the exact
  target repository.
- Duplicate-submit: first click set `submitting="true"` / `aria-busy="true"`
  / `aria-disabled="true"`; an immediate second click was blocked by the
  existing (unmodified) analysis-progress.js state guard.
- Mobile overflow (375px): a devtools-style layout probe using the actual
  shipped `profile-result.css` classes with a synthetic ~110-character
  repository-shaped string showed no horizontal overflow, correct wrapping,
  and the action button staying inside the viewport.
- Light/dark: theme toggle confirmed `data-theme` switches and all new rules
  resolve through existing `--fg-*`/`--border-*` tokens only.

Subsequently reported real pixel/visual evidence for the rendered Profile
result, the disclosure-marker/H2 alignment correction and responsive layout
(`torvalds`, `fatihGunalp`) — this is visual confirmation of the rendered
outcome, not a separate re-verification of the POST mechanism itself, which
was already established above via the `octocat` DOM/server-log run:

- TR desktop / dark: PASS
- TR mobile ~375px / dark: PASS
- disclosure marker/H2 alignment (the CSS correction): PASS
- no horizontal overflow: PASS
- long repository names/status/action wrapping: PASS
- Starting Point remains text-only: PASS
- Why / Repository Evidence remain progressive disclosure: PASS

EN was **not** completed as a live visual state in this session. EN/TR copy
correctness itself remains covered by the deterministic
`ProfileWebPresentationLocalizer` mappings and the automated localizer tests
(see Validation above), which is a stronger guarantee of textual correctness
than a screenshot would be — but EN layout/rendering was not visually
observed and is not claimed as such.
~~~

## Problems

~~~text
Observation:
During this session's own live validation turn, a single `octocat` profile
analysis consumed most of the unauthenticated GitHub REST quota (67 of 60
requests reported by the app's own telemetry crossed the hourly ceiling), so
a subsequent repository-evidence POST returned a rate-limited result instead
of a fresh Repository Proof report body, and further same-session live
states could not be captured.

Classification:
Environment/test observation only, not a product defect. This is the exact
pre-existing, unmodified P10-I2/I5-owned rate-limit failure path (same
message, same code) that Repository Proof already had before P10-I4 — its
appearance here is truthful behavior under a real external constraint, not a
new failure introduced by this session. Subsequent live runs (torvalds,
fatihGunalp) completed successfully end to end, confirming the interaction
mechanism itself has no defect. No GITHUB_TOKEN is configured in this
environment; configuring one removes the constraint for future sessions.

Observation:
Separately, one `fatihGunalp` live attempt (part of the same subsequent live
validation) ended with HTTP 499, with application telemetry recording
Status=cancelled / Outcome=cancelled; the analysis result therefore did not
reach the browser in that specific attempt.

Classification:
Environment/test observation only, not an unresolved P10-I4 product defect.
A subsequent retry of the same target completed successfully and rendered
the Profile result. Propagating `OperationCanceledException` without
fabricating a partial/failure result is existing, unmodified P10-I1/I2-owned
behavior (`docs/architecture/public-profile-runtime-collection.md` §11.1) —
this session did not change cancellation handling.
~~~

## Not Done

~~~text
- P10-I5 — Limitations / Warnings / final Partial / Failure UX (position 7
  still not rendered; typed truth unchanged, this is slice sequencing).
- P10-I6 — visual refinement.
- Production guardrail numeric values (execution guard, concurrent-profile
  guard) remain unselected, unchanged from prior sessions.
- P10 overall is NOT complete (P10-I5/P10-I6 remain), and Milestone B is NOT
  complete.
~~~

## Known Risks

~~~text
None currently open. The one LOW risk raised by the implementation code
review (very long repository FullName potentially overflowing at narrow
mobile widths) was verified closed via a live-CSS layout probe during this
session's own validation, and separately confirmed by the later real TR
mobile pixel review (names/status/action wrapped correctly, no overflow).

EN was not captured as a live visual state this session (see Live
validation); this is a coverage gap for future validation, not a currently
known defect.
~~~

## Working Set / Final Git State

~~~text
Modified:
- docs/product/p10-presentation-language-contract.md
- docs/product/p10-web-profile-flow-implementation-plan.md
- src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs
- src/GitHubProof.Web/Pages/Profile.cshtml
- src/GitHubProof.Web/wwwroot/css/profile-result.css
- tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs
- tests/GitHubProof.Web.Tests/ProfileWebPresentationLocalizerTests.cs

Untracked (new):
- src/GitHubProof.Web/Pages/Shared/_ProfileWhyThisView.cshtml
- src/GitHubProof.Web/Pages/Shared/_ProfileRepositoryEvidence.cshtml

Pre-existing unrelated:
- ?? artifacts/ — untouched
~~~

## Next

Production Guardrails (execution guard, concurrent-profile guard numeric
values). P10-I5 — Limitations + Partial/Failure UX — and P10-I6 remain ahead
after that; P10 overall and Milestone B are not yet complete.

## Git

~~~text
Suggested commit:
feat(web): implement P10-I4 why + repository evidence drill-down

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

## Completion Checklist

~~~text
[x] Goal tamamlandı veya Result doğru final state'e getirildi (contract,
    implementation, disclosure-CSS correction and visual/interaction gate
    all PASS; P10-I4 COMPLETE / validated — P10 and Milestone B remain open)
[x] Targeted validation çalıştı
[x] Gerekliyse broader validation çalıştı
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Kullanıcıya kısa final rapor verildi
~~~

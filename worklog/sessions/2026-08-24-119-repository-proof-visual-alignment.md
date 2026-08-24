# Repository Proof Visual Alignment

**Date:** 2026-08-24
**Result:** PASS
**Scope:** web (Repository Proof first-read presentation + guards)
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Align the Repository Proof Web experience with the shared GitHub Proof UI System
and the now-complete Profile Proof product language, **without redesigning
Repository Proof semantics**.

Primary question: moving Profile Proof → Repository Evidence → Repository Proof,
does it feel like the same product and the same design system?

---

## Starting State

~~~text
Branch:
session/119-repository-proof-visual-alignment

Base HEAD:
edb35fd5f953655c81327528a0b67f05a59530a6

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-24-118-p10-complete-exit-evaluation.md
~~~

Branch and base matched exactly; `main` was at the same commit.

---

## Canonical audit result

Answer to the primary question: **yes, already the same design system.** Tokens,
native `details` disclosure, focus behaviour and the status vocabulary are
shared. The differences that exist are measure and breakpoint vocabulary
(64rem/48rem vs 60rem/45rem), which the architecture owner deferred (C2).

**One concrete canonical violation was found: first-read score dominance.**

Four independent canonical owners agree on the same hierarchy:

~~~text
ui-system §2       Evidence -> Meaning -> Action -> Score
                   explicitly avoided: "72 / 100 -> her şey"
ui-system §22      canonical first-reading order puts
                   "Category summary + secondary score" at position 6
ui-system §21.10   Evidence Score: "Avoid: circular gauge / Developer score"
scoring-and-report-contract §14
                   "Toplam ve kategori skorları korunur ancak ilk okuma
                    katmanının ana mesajı olmaz."
design-principles §7.4
                   the first viewport should surface THREE sections:
                   Proves / Weakens / Do These First
~~~

Measured before the correction (live `/report-preview`, 1280 dark):

~~~text
first-read grid    [SCORE] -> Proves -> Weakens -> Do First
card areas         score 71,424px²   proves 50,561   weakens 50,564
score ring         76x76 conic-gradient(accent 73%), 28px/600 numeral
count strip        "3 Güçlü Kanıtlar / 3 Zayıf Noktalar / 3 Belirsizlik"
at 390px           Proves started at y=740 — outside the first viewport
~~~

Everything else audited clean: canonical `data-report-section` order intact,
evidence trace reachable through two native disclosure levels, seven distinct
status variants each with icon + label + distinct colour, and **zero WCAG AA
failures** on rendered text in both themes.

---

## A1 — CLOSED

Architecture-owner decision C1/B1/B2: remove the score card from the first-read
layer entirely; the count strip goes with it; the layer becomes exactly
Proves → Weakens → Do First.

~~~text
BEFORE   Score -> Proves -> Weakens -> Do First
AFTER    Proves -> Weakens -> Do First
~~~

The evidence score remains in its canonical later **Categories + Score**
disclosure — position 6 per §22, still after uncertainty and before signal
details.

Changes:

~~~text
_RepositoryReport.cshtml   score-card wrapper + partial call removed;
                           a Razor comment records the canonical reason
_RepositoryReportScoreCard.cshtml   RETIRED (99 lines) — verified unreferenced
                           across src/ and tests/ before deletion.
                           The count strip left with it (B1).
report.css                 17 rule blocks retired (report-score-card__* family
                           + report-summary-card__icon--score) and the
                           @media 48rem .report-score-card__body leftover.
                           Zero residual references; braces balanced 184/184.
                           .report-decision-grid -> three columns, keeping the
                           retained cards' previous proportions unchanged
                           (minmax(0,1.1fr) minmax(0,1.1fr) minmax(18rem,1.45fr));
                           the 64rem->2col and 48rem->1col collapse is unchanged.
~~~

Measured after (live, 1280 / 880 / 390, light and dark):

~~~text
first-read order   Proves -> Weakens -> Do First   at every width
score in first read absent
card areas (1280)  proves 63,976px²  weakens 63,976  action 105,617
at 390px           Proves at y=378 — INSIDE the first viewport (was y=740)
canonical score    still 73/100 in the Categories + Score surface
section order      8 sections, canonical, unchanged
page overflow      none at any width
~~~

**Score semantics unchanged.** `_RepositoryReportScore.cshtml` was not touched.
`NormalizedScore`, per-category `NormalizedScore`, `@category.Score /
@category.MaximumScore`, `/100` and `ScoreSummary.Label` are all intact.
**Domain / Application / Infrastructure: zero files changed** — scoring logic,
composer, projector and the report contracts were not modified.

---

## A2 — FALSE POSITIVE / current behaviour preserved

**This is a correction of my own Step 1 audit, recorded deliberately rather than
quietly dropped.**

The Step 1 audit reported an A-class finding: *"status badge label destroyed at
≤880px in the Proves/Weakens first-read cards"*. **That was wrong on two counts.**

**Wrong about the mechanism.** `report.css:564`
`.report-summary-item .status-badge__label` applies an *unconditional*
screen-reader-only pattern (`position:absolute; width/height 1px;
clip: rect(0,0,0,0); overflow:hidden`). The file's first `@media` is far below
it, so the rule is width-independent — there is no responsive collapse. It was
introduced deliberately in commit `40cab55` with an explicit rationale comment
("Summary cards already communicate the semantic group. Keep exact status
available to assistive technology, but avoid repeating the same label visually
on every row"). Measured live at 1280: `clientWidth: 1`, `position: absolute`,
`clip: rect(0,0,0,0)` — sr-only there too. My audit had read `textContent`
(which includes sr-only text) at 1280 and mistook the same unconditional rule at
390/880 for a regression.

**Wrong about it being a violation at all.** Verified against canonical text:

~~~text
§21.3 Proof Item anatomy (the Proves rows) is literally
      "Status icon / Title / One-line meaning / Optional evidence reference"
      with the example "✓ Test ve CI görünür" — an icon and a title, no
      visible status word. The Proves row matches that anatomy exactly.

§21.4 Gap Item anatomy (the Weakens rows) says "Status", not "Status icon".
      It does not, however, require a visible status WORD, and it is §29 that
      sets the actual accessibility floor. So §21.4 permits the current row
      rather than prescribing it — a weaker claim than for Proves.

§29   prohibits status conveyed by COLOUR ALONE. StatusBadgePresentation gives
      the seven statuses seven distinct icon SHAPES (CircleCheck, Check,
      CircleDashed, CircleX, CircleMinus, CircleHelp, Eye), so shape carries
      the status too. The accessible text label remains in the DOM and in the
      accessible name.

§38 Q5 (Missing vs unavailable) is resolved at information-architecture level,
      not by badge labels. RepositoryAnalysisComposer.SelectCriticalGaps takes
      SignalStatus.Missing AND SignalStatus.Partial (as scoring-and-report-
      contract §13 requires), while RepositoryReportProjector routes
      NotApplicable / UnavailableEvidence / RequiresHumanReview to the separate
      Uncertainty section. §21.4 states it outright: "Uncertainty durumları Gap
      olarak sunulmaz." Missing and UnavailableEvidence therefore never appear
      in the same summary card; Missing and Partial can, and they already carry
      two distinct icon shapes (CircleX vs CircleDashed).
~~~

Independent review reached the same conclusion independently and added that
implementing A2 would have been a **regression against §21.3 and §37**.

~~~text
A2 is NOT a deferred gap.
A2 is NOT an accepted defect.
A2 is a FALSE POSITIVE. Current behaviour is preserved unchanged.
~~~

Not modified: `status-badge.css`, the sr-only rule in `report.css`,
`StatusBadgePresentation` semantics.

Likely root cause of the audit error, worth carrying forward: §19 describes the
*generic* Status Badge as "text + icon/dot", while §21.3 describes this
surface's *domain* component as "Status icon". The relationship between the two
is not stated in the document, and the audit resolved it the wrong way.

---

## F2 — grid guard hardened

Independent review found `Decision_viewport_uses_proportional_three_two_one_
responsive_card_grid` used a bare `Contains("minmax(0, 1.1fr)")`, so one of the
two evidence columns could disappear while the assertion still passed.

The guard now parses the top-level `.report-decision-grid` rule out of
`report.css`, extracts its `grid-template-columns` track list, splits it on
track boundaries (not on the commas inside `minmax(...)`) and asserts **column
count and order**: exactly three columns, `minmax(0, 1.1fr)`,
`minmax(0, 1.1fr)`, `minmax(18rem, 1.45fr)`.

Mutation probe, as required:

~~~text
baseline report.css   git hash-object 5c28ded597d623be27b55faaafc9e5a43a398c38
probe                 one of the two minmax(0, 1.1fr) evidence columns removed
result                FAIL — "Assert.Equal() Failure: Values differ"
restore               git hash-object 5c28ded597d623be27b55faaafc9e5a43a398c38
                      => byte-identical
~~~

Independent review then probed the parser with five mutation classes and found
one blind spot: a track that is not a function call (a bare `0.85fr`) appended
AFTER the last `minmax(...)` left a remainder the splitter discarded, so a
four-column regression of that exact shape could still pass. The remainder is
now flushed as a column, and that probe was re-run:

~~~text
probe    trailing bare `0.85fr` track appended
result   FAIL
restore  git hash-object 5c28ded597d623be27b55faaafc9e5a43a398c38
         => byte-identical
~~~

No production CSS was changed to satisfy the test, and no pixel geometry was
turned into canonical semantics — the guard is a regression lock, not a
canonical statement about column widths.

---

## Decisions

~~~text
- C1/B1/B2 implemented as locked: score card removed, count strip removed,
  first read is exactly Proves -> Weakens -> Do First.
- The three retained cards keep their PREVIOUS relative proportions. Only the
  score column was dropped — minimum correction, not an aesthetic re-balance.
- C2 (measure / breakpoint unification), C3 (identity card) and B5
  (profile-result.css radius tokens) deferred by the owner; untouched.
- B2's broader card-system reduction was NOT performed beyond removing the
  obsolete score card.
- Two obsolete tests that had locked in the old behaviour were replaced, not
  merely deleted: the replacements were shown to fail against the pre-change
  content.
~~~

---

## Validation

~~~text
RepositoryReportShellTests                24/24    PASS
CssArchitectureTests                       8/8     PASS
report / theme / localization / badge     34/34    PASS
Web suite                                380/380   PASS
full solution                           1785/1785  PASS  (835/262/380/308)
dotnet build -c Release                  0 warnings, 0 errors
dotnet format --verify-no-changes        PASS
git diff --check                         PASS
F2 mutation probes                       5 classes; all FAIL on regression,
                                         byte-identical restore each time
~~~

Live `/report-preview` at 1280 / 880 / 390 in light and dark: first read is
Proves → Weakens → Do First, score absent from first read, canonical score still
73/100, 8 canonical sections, no horizontal overflow, Proves inside the first
viewport at 390.

---

## Not Done

~~~text
- No A2 production change (false positive).
- No Profile Proof change; tokens.css / status-badge.css / profile-result.css
  untouched.
- No docs/, Domain/, Application/ or Infrastructure/ change.
- C2, C3 and B5 deferred by owner decision.
- No broader card-system redesign.
- artifacts/ user-owned and unmodified.
~~~

---

## Known Risks

~~~text
- Evidence is measured layout geometry and computed style from the real engine
  plus a live /report-preview render; no human pixel review was performed. This
  matches the standing limitation recorded since Session 114.
- The §19 vs §21.3 Status Badge relationship remains unstated in the canonical
  document. It produced this session's audit error and could produce another.
  Recorded here; not fixed (docs are out of scope for this session).
~~~

---

## Independent Review

**PASS WITH FINDINGS** — no blocking finding. Scope verified as exactly seven
tracked/new files; `tokens.css`, `status-badge.css`, `profile-result.css`,
`docs/`, Domain/Application/Infrastructure all confirmed unchanged. All five
canonical citations behind A1 verified against source text. Review independently
re-ran the two directly affected test classes (32/32) and independently
replicated the F2 parser to probe it.

~~~text
F-1 MEDIUM  This record originally claimed SelectCriticalGaps takes "only
            SignalStatus.Missing". The code takes Missing AND Partial
            (RepositoryAnalysisComposer.cs:200-209), matching
            scoring-and-report-contract 13. The A2 verdict is unaffected
            (Partial is not an uncertainty status), but the evidence sentence
            was wrong. CORRECTED above.
F-2 LOW     The A2 defence was built only on 21.3 (Proof Item, "Status icon"),
            while the Weakens rows are Gap Items under 21.4, whose anatomy says
            "Status". "Matches this anatomy exactly" was too general for
            Weakens. CORRECTED above: 21.4 permits rather than prescribes, and
            29 carries the accessibility floor.
F-3 LOW     The hardened grid guard discarded a trailing non-function track, so
            one four-column regression shape could still pass. CLOSED: the
            remainder is now flushed as a column, and the probe re-run confirms
            it fails on that mutation.
F-4 LOW     Operational, for the user only: the partial deletion is staged
            while the other six files are unstaged, so a bare `git commit`
            would commit the deletion alone. `git add -A` first. No mutation
            was made for this.
~~~

Not independently re-run by review, accepted from this record: full solution
1785/1785, Release build, `dotnet format`, and the live 1280 / 880 / 390
measurements.

---

## Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Web/Pages/Shared/_RepositoryReport.cshtml   (production)
- src/GitHubProof.Web/wwwroot/css/report.css                  (production)
- tests/GitHubProof.Web.Tests/RepositoryReportShellTests.cs   (tests)
- worklog/CURRENT.md
- worklog/INDEX.md

Deleted:
- src/GitHubProof.Web/Pages/Shared/_RepositoryReportScoreCard.cshtml

Untracked:
- worklog/sessions/2026-08-24-119-repository-proof-visual-alignment.md

Seven tracked/new files in total.

Pre-existing unrelated:
- artifacts/ (user-owned, read-only, untouched)
~~~

---

## Next

Profile Runtime Performance & Budget, then the Milestone B production gate.

---

## Git

~~~text
Suggested commit:
fix(web): make repository proof first read evidence-first

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Read-only canonical audit before any mutation
[x] A1 implemented as locked (C1 + B1 + B2)
[x] Score semantics preserved; Domain/Application untouched
[x] A2 recorded as FALSE POSITIVE, current behaviour preserved
[x] F2 guard hardened and mutation-probed
[x] Live validation at 1280 / 880 / 390 in light and dark
[x] Full validation gate green
[x] Independent review — PASS WITH FINDINGS; F-1/F-2/F-3 closed in-session,
    F-4 is a staging note for the user
[ ] Kullanıcıya final rapor
~~~

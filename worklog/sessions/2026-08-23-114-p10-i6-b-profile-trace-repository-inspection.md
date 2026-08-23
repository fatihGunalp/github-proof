# P10-I6-B Profile Trace + Repository Inspection

**Date:** 2026-08-23
**Result:** PASS / COMPLETE
**Scope:** web (presentation / CSS + targeted tests)
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Complete P10-I6-B so the visitor can answer "Why does GitHub Proof say this,
and how can I inspect the evidence?" — by improving the visual continuity and
inspection usability between the two canonical trace stages, without creating
analysis truth.

---

## Starting State

~~~text
Branch:
session/114-p10-i6-b-profile-trace-repository-inspection

Base HEAD:
efe0a76

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-23-113-worklog-navigation-compaction.md
~~~

Branch and base matched the declared expectation exactly; `artifacts/` remained
user-owned, untouched and out of scope throughout.

---

## Model Route

~~~text
Selected:
Opus / high

Why:
A presentation slice with heavy invariant load — the prohibition list is
exactly the class where a plausible-looking UI improvement silently becomes
product truth (ordering supporting repositories would be ranking, showing a
number would be a new metric). Comparable slices on this same surface (103,
104, 110, 111) all ran at high effort and needed contract corrections in review.

User Approval:
devam et
~~~

## Model Escalation

~~~text
None.
~~~

---

## Context Read

~~~text
agents/02-context-loading.md
worklog/CURRENT.md
docs/product/p10-web-profile-flow-implementation-plan.md — P10-I6 scope, Non Goals
docs/product/p10-presentation-language-contract.md — 8.6.2-8.6.9, 8.7 (8.7.6)
src/GitHubProof.Web/Pages/Profile.cshtml
src/GitHubProof.Web/Pages/Shared/_ProfileWhyThisView.cshtml
src/GitHubProof.Web/Pages/Shared/_ProfileRepositoryEvidence.cshtml
src/GitHubProof.Web/Pages/Shared/_ProfileStartingPoint.cshtml
src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs — method inventory only
src/GitHubProof.Web/wwwroot/css/profile-result.css
tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs
~~~

Session 105 and Session 111 records were not needed: every decision resolved
from canonical docs plus current production truth.

---

## Contract / Invariants Checked

### Architecture-owner decision recorded

**Direct action from a Why entry is deliberately NOT introduced.** The canonical
trace is intentionally two-stage and is already owned by
`p10-presentation-language-contract.md` §8.6.3–§8.6.5:

~~~text
WhyThisConclusion   → conclusion → human reason → supporting repository identities
RepositoryDrillDown → repository identity/status → explicit existing Repository Proof POST
Repository Proof    → signal / concrete evidence
~~~

Step 1 of this session reported this as a possible "Profile Trace Continuity
Contract Gap". The architecture owner ruled it is **not** a blocker: P10-I6-B
improves the VISUAL continuity between the two stages and must not add an
action inside Why or change canonical body content. This session implements
that ruling.

### Invariants held (all verified against the final working tree)

~~~text
native details/summary, collapsed by default   — no partial changed
canonical DOM order                            — no partial changed
every RepositoryDrillDown item rendered        — no partial changed
no sort/filter/group/count/pagination          — no partial changed
RepositoryEligibilityStatus-only row status    — no partial changed
AnalysisReference unread                       — no partial changed
EvidenceKey / EvidenceKeys absent              — no partial changed
one existing POST action per evidence row      — no partial changed
Repository.Url only as posted GitHubTarget     — no partial changed
existing anti-forgery / IndexModel path        — untouched
no embedded RepositoryAnalysis                 — untouched
no score/percentage/confidence/ranking         — new CSS guard added
no role/seniority/employability/capability     — existing guard still passing
~~~

The strongest guarantee here is structural: **no Razor, localizer, Domain,
Application or Infrastructure file was modified.** Every listed invariant lives
in files this session did not touch, and the existing test suite that owns them
passed unchanged.

---

## Changes

~~~text
Modified:
- src/GitHubProof.Web/wwwroot/css/profile-result.css
- tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs

Created:
- worklog/sessions/2026-08-23-114-p10-i6-b-profile-trace-repository-inspection.md
~~~

### profile-result.css — the only production mutation

1. **Trace continuity (objective 1).** A supporting repository named under
   "What is this based on?" and the same repository under Repository evidence
   now share identity typography (`--font-mono` / `--font-size-mono` /
   `--line-height-mono`). Scoped to
   `.profile-result__section--why .profile-result__repository-item` because Why
   and Starting Point share `.profile-result__repository-item`; restyling the
   shared base class would have silently restyled the first-read Starting Point
   rail. Typography only — no emphasis, colour accent, weight or ordering.

2. **Deterministic evidence rows (objectives 2 and 4).** The row was
   `display: flex; flex-wrap: wrap; justify-content: space-between` with the
   identity at `flex: 1 1 16rem`, the status carrying no flex property and the
   form a block-level flex item. At intermediate widths the status could detach
   onto its own line, or the action could wrap under the identity while the
   status stayed beside it. The row is now
   `grid-template-columns: minmax(0, 1fr) minmax(0, auto) auto` with every cell
   explicitly placed (identity 1, status 2, action 3), so the three reading
   positions are fixed at every width.

3. **Long-name protection (objective 3).** The identity keeps
   `min-width: 0` + `overflow-wrap: anywhere` + `word-break: normal`; the status
   gained the same protection, and the identity column is `minmax(0, 1fr)` so
   the grid can shrink below content width instead of overflowing.

4. **Narrow-viewport inspection rule (objective 5).** `≤45rem` previously
   carried no evidence-row rule at all. The row now collapses to a single
   column and the status and action fall beneath their own identity in DOM
   order, `justify-self: start` — deliberately not full width, so the action
   stays a quiet secondary step rather than a call to action competing with the
   first-read layer (objective 8).

5. **First-row separator (objective 6).** `.profile-result__evidence-row`
   carried an unconditional `border-top`, so the first row drew a rule directly
   against the fresh-analysis lead-in paragraph rather than between two rows.
   Suppressed on `:first-child` only.

6. **Card bottom rhythm (objective 7).** The Repository evidence card's bottom
   padding was `var(--space-4)` while every other report card uses
   `var(--space-6)`; normalised using the existing token, no new token.

The action is selected through the markup's **existing** `data-analysis-form`
hook, so the partial gains no element, class or attribute.

---

## Decisions

~~~text
- CSS-only. Razor was deliberately left untouched so that 8.7.6's "their
  canonical body content are unchanged" needs no interpretation at all.
- Three explicit columns instead of a two-column/two-row layout. The first
  attempt used `grid-row: 1 / span 2` for the action and broke a pre-existing
  I6-A guard that forbids `grid-row:` anywhere in this stylesheet. The guard
  was NOT weakened — the layout was redesigned to not need it, which also
  produced a simpler and shorter row.
- The narrow-viewport action is left-aligned, not full width. A full-width
  button would read as importance/priority on the inspection surface.
- No new EN/TR copy. Every string on this surface stays the locked 8.6.7 /
  8.7.6 copy resolved by ProfileWebPresentationLocalizer, which is unchanged.
~~~

No new product, domain, architecture or presentation-semantic contract was
created. No `docs/` change is required.

---

## Validation

~~~text
dotnet test --filter ProfileResultPresentationTests
PASS — 40/40 (33 pre-existing + 7 new)

dotnet test tests/GitHubProof.Web.Tests
PASS — 374/374

dotnet test (full solution)
PASS — 1779/1779
  Domain 835 / Application 262 / Web 374 / Infrastructure 308

dotnet build -c Release
PASS — 0 warnings, 0 errors

dotnet format --verify-no-changes
PASS

git diff --check
PASS

changed-file / boundary review
PASS — exactly the two authorized files; `?? artifacts/` untouched
~~~

Three failures occurred mid-implementation and were all resolved before the
final run — see Problems.

Not run: live browser/visual verification. This session changed layout, so the
rendered result has not been seen by a human at any viewport. That gate is
recorded as remaining, not claimed.

---

## Tests added (7)

~~~text
Why_supporting_repository_identities_share_the_repository_evidence_identity_typography
    both stages carry the same identity typography; the Why rule carries no
    accent, weight, ordering or generated content

Why_identity_typography_is_scoped_so_starting_point_keeps_its_own_treatment
    shared-class regression guard: the base .profile-result__repository-item
    rule declares no font-family, and Starting Point's own item rule is not
    monospace

Repository_evidence_rows_keep_identity_and_status_together_beside_one_action
    three explicit columns, no flex-wrap, action selected via the existing
    data-analysis-form hook

Repository_evidence_identity_and_status_survive_a_long_repository_full_name
    min-width: 0 + overflow-wrap: anywhere + word-break: normal on both cells,
    and a shrinkable identity column

Repository_evidence_row_separator_does_not_rule_against_the_lead_in_paragraph
    :first-child suppresses the separator only

Repository_evidence_rows_stack_with_a_quiet_action_on_narrow_viewports
    single column at 45rem, status and action in column 1, justify-self: start,
    explicitly not width: 100%

Profile_result_css_generates_no_text_counter_or_ordering_behaviour
    no counter-reset/increment, counter()/counters(), attr(); every content:
    declaration is empty or none — a guard the Razor-source tests cannot give
~~~

Guards are structural (selector + declaration presence), not pixel snapshots.
Spacing token values are deliberately not asserted, so I6-C can retune rhythm
without fighting these tests.

---

## Problems

~~~text
Problem 1:
An initial `grid-row: 1 / span 2` on the action broke the pre-existing I6-A
guard `Profile_report_uses_the_handoff_card_grid_...`, which forbids
`grid-row:` anywhere in profile-result.css.

Cause:
A two-column/two-row row layout needs explicit row spanning.

Resolution:
The layout was redesigned as three explicit columns. The existing guard was
left exactly as it was — an existing invariant is not weakened to accommodate
new code.

Problem 2:
A new guard used a plain substring check for "order:", which matches "border:".

Cause:
Over-broad assertion.

Resolution:
Removed. The pre-existing card-grid test already guards `order:` with a
correctly anchored regex; duplicating it badly was worse than not duplicating
it.

Problem 3:
A new scoping guard asserted that a Starting Point item rule does not exist.
It does exist and legitimately so.

Cause:
Wrong assertion — the intent was "Starting Point must not become monospace",
not "Starting Point must have no rule".

Resolution:
Rewritten to assert that Starting Point's own item rule contains no
`var(--font-mono)`.
~~~

Problem 4 (found by independent review, not by me):
The same scoping guard located the rule with
`css.IndexOf(".profile-result__repository-item {")`, which matches the earlier
Starting-Point-scoped rule, not the grouped base rule. The
`DoesNotContain("font-family")` assertion therefore proved nothing.

Cause:
A selector fragment that legitimately appears inside a longer, earlier
selector.

Resolution:
The base rule is now located by its FIRST selector
(`.profile-result__pattern-item,`), verified to contain
`.profile-result__repository-item {`, and the block was confirmed by hand to
be the real base declaration.

Problem 5:
The visual gate could not render a real analyzed Profile.

Cause:
No `GITHUB_TOKEN` is configured, so the app runs unauthenticated at 60 GitHub
calls/hour. An `octocat` profile analysis exhausted the quota mid-run and the
app correctly rendered its typed rate-limit failure state, which is a real but
unrelated P10-I5 surface.

Resolution:
STEP 3 explicitly authorises a browser-only/devtools stress substitution for
layout verification. The gate injected DOM built verbatim from
`_ProfileWhyThisView.cshtml`, `_ProfileRepositoryEvidence.cshtml` and
`_ProfileStartingPoint.cshtml` into the running application, so the real
tokens/base/profile-result cascade and the real layout engine were exercised.
No repository file was modified, and none of the injected content is reported
as profile truth.

---

## Not Done

~~~text
- No Razor, localizer, docs/, Domain/, Application/, Infrastructure/ or
  wwwroot/js/ change.
- No new EN/TR copy and no new semantic contract.
- No action added inside a Why entry (architecture-owner decision).
- Broad I6-C responsive/theme/localization closure not started.
- Live browser / human visual verification not performed.
~~~

---

## Known Risks

~~~text
CLOSED - rendered-layout risk. STEP 3's bounded visual gate exercised all three
required viewports against the running application's real stylesheet. Evidence
is in "Visual Gate (STEP 3)" below.

CLOSED - 45-60rem band risk. Measured at 880 px with a long full name and the
longest status copy: no overflow, no orphaned status, no detached action.

CLOSED - CSS/markup coupling. A markup-side guard now proves the row's own form
still carries the `data-analysis-form` hook the layout selects on.

REMAINING:
- No human pixel/aesthetic review and no screenshot. The Browser pane was not
  displayed in this environment, so the gate is measured layout geometry from
  the real engine, not an eyeball. Every stated gate criterion is objective and
  was verified; subjective visual judgement is not claimed.
- No real analyzed Profile was rendered (Problem 5). The gate ran against
  browser-injected DOM built verbatim from the partial sources.
- Light theme and the EN/TR copy matrix remain untested here - I6-C scope by
  the owner's own instruction.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Web/wwwroot/css/profile-result.css
- tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs

Untracked:
- worklog/sessions/2026-08-23-114-p10-i6-b-profile-trace-repository-inspection.md

Pre-existing unrelated:
- artifacts/ (user-owned, read-only, untouched)
~~~

---

## Next

Human visual verification of the Repository evidence and Why surfaces at
desktop, the 45-60rem band and mobile, in EN and TR, before P10-I6-C is opened.

---

## Git

~~~text
Suggested commit:
feat(profile): improve profile trace and repository inspection

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Independent Review

`github-proof-review`, forked context. **VERDICT: PASS WITH FINDINGS —
0 BLOCKER / 0 HIGH / 2 MEDIUM / 2 LOW.** All four closed in-session.

Independently reproduced by the review: 1779/1779 tests, Release build clean,
`dotnet format` clean, `git diff --check` clean, scope containment (Razor,
localizer, Domain, Application, Infrastructure, `wwwroot/js/` untouched), token
existence in `tokens.css`, correctness of the `:first-child` target against the
real DOM (`li` rows inside `ul`, lead-in `<p>` a sibling of the `ul`), and that
`analysis-progress.js` only mutates attributes and injects no DOM into a row —
so the three-cell grid cannot be destabilised at runtime.

~~~text
MEDIUM  The shared-class regression guard located the Starting-Point-scoped
        rule instead of the grouped base rule, making its font-family
        assertion vacuous — precisely the regression it claimed to prevent.
        Fixed (Problem 4) and the corrected locator verified by hand.

MEDIUM  CURRENT.md asserted "Next is P10-I6-C" and had dropped Session 113's
        caveat that docs/ enumerates no P10-I6 sub-slices. The caveat is
        restored, the A/B/C split is labelled a worklog-side working split
        from the owner's Session 105 note, and the missing canonical scope is
        now an explicit Open Decision.

LOW     Two over-broad substring guards ("order:" matches "border:",
        "content:" matches "justify-content:") were still present although
        Problem 2 recorded them as removed. Replaced with anchored regexes,
        matching the convention the pre-existing card-grid test already uses.

LOW     Result was recorded as PASS before the independent review closed.
        Corrected to PASS WITH FINDINGS in this file, INDEX and CURRENT.
~~~

The review explicitly did not treat the missing rendered visual verification as
a finding, because it is recorded here in Known Risks, Not Done and CURRENT's
Open Decisions rather than hidden.

Post-fix re-run: targeted 40/40, and the corrected base-rule locator confirmed
non-vacuous against the actual stylesheet.

---

## Visual Gate (STEP 3)

Bounded live inspection of only the two changed surfaces, against the running
application at `http://localhost:5199`.

**Method and its limits.** No real analyzed Profile could be rendered - the
unauthenticated GitHub quota was exhausted (Problem 5) - so the gate used the
owner-authorised browser-only DOM substitution: markup built verbatim from the
three Profile partials, injected into the running app so the real stylesheet
cascade and the real layout engine applied. Stress content was a 115-character
repository full name and the longest status copy
(`Bu gorunume kisitlamalarla kanit sagladi`). The Browser pane was not
displayed in this environment, so screenshots were impossible; evidence is
measured layout geometry (`getBoundingClientRect`, `getComputedStyle`,
`checkVisibility`, `elementFromPoint`), not a pixel eyeball.

### A - Desktop, 1280x900

~~~text
grid-template-columns   764.5px | 162.5px | 200px   (three columns hold)
horizontal overflow     none (scrollWidth 1265 = clientWidth 1265)
Why identity font       ui-monospace... 12px
Evidence identity font  ui-monospace... 12px        -> identical, objective 1 met
action width            200px of a 1159px row       -> quiet, not full width
~~~

### B - Intermediate band, 880x900 (inside 45rem-60rem)

~~~text
grid-template-columns   364.5px | 162.5px | 200px
horizontal overflow     none (scrollWidth 865 = clientWidth 865)
long-name row           height 89px; identity wrapped to 281px over 3 lines,
                        status 246px over 2 lines, action at x=612
cells inside own row    true for all 3 rows   -> no orphaned status
no cell past row edge   true for all 3 rows   -> no detached action
~~~

This is the band the earlier flex layout could not hold deterministically.

### C - Narrow mobile, 390x844

~~~text
grid-template-columns   324px (single column)
all rows stacked        true - identity y -> status y+ -> action y++,
                        all sharing left edge x=33, in DOM order
horizontal overflow     none (scrollWidth 390 = clientWidth 390)
action width            200px of a 324px row -> normal secondary control,
                        deliberately not full width
long-name row           identity wraps to 54px tall, right edge 357 <= row edge
~~~

### Separators, collapsed state and Starting Point

~~~text
first evidence row      border-top 0px, padding-top 0px   -> removed separator
rows 2 and 3            border-top 1px, padding-top 16px  -> still separated
collapsed disclosures   both closed by default; no aria-expanded; chevron present
collapsed card height   94px vs 332px opened
                        body checkVisibility() = false, not hit-testable
                        (a >0 rect on the skipped subtree is a Chrome
                        rendering-skip artifact, verified, not a defect)
quiet + subordinate     each collapsed card is 20.7% of the first-read block
card bottom rhythm      24px, equal to the Starting Point card -> objective 7
Starting Point          .profile-result__repository-item still display:grid,
                        grid-column auto; its identity was ALREADY monospace at
                        efe0a76 (verified with git show), so the Why-scoped rule
                        changed nothing there
~~~

**Result: PASS. No I6-B visual defect found, so production CSS was not changed
again.**

---

## Completion Checklist

~~~text
[x] Goal tamamlandı
[x] Targeted validation çalıştı
[x] Broader validation çalıştı (full solution + Release + format)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX satırı eklendi
[x] CURRENT güncellendi
[x] Independent review — PASS WITH FINDINGS, 4 findings closed in-session
[x] Bounded visual gate - PASS (desktop / 880px / 390px)
[x] CSS-markup coupling guard eklendi
[x] Kullanıcıya final rapor
~~~

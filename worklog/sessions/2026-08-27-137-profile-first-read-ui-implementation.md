# Profile First-Read UI Implementation

**Date:** 2026-08-27
**Result:** PARTIAL — closure accepted by the architecture owner
**Scope:** web (Razor / CSS / localizer) + web tests + docs cross-reference
**Model Route:** Sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement the Session 136 first-read presentation decision in Profile Web:
render the recurring pattern → Starting Point relationship (p10 §8.6.1b), and
close G-2, G-3 and G-4.

---

## Starting State

~~~text
Branch:
session/137-profile-first-read-ui-implementation

Base HEAD:
7038159

Pre-existing Working Tree:
?? artifacts/   (user-owned, untouched)

Relevant Previous Session:
worklog/sessions/2026-08-27-136-profile-first-read-ux-contract.md
~~~

`main` == `origin/main` == `HEAD` == `7038159` verified before branching.

---

## Model Route

~~~text
Selected:
Sonnet / high

Why:
Bounded Web presentation implementation against an already-locked contract;
invariant-heavy (no-count / no-ranking / no-inference) but no new product or
domain semantics.

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
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/product/p10-presentation-language-contract.md  (§8.4, §8.6.1-§8.6.2, §8.7.6, §8.8)
docs/product/profile-proof-distinctive-outcomes-contract.md
docs/product/p10-web-profile-flow-implementation-plan.md  (P10-I5, §13.13, Starting Point notes)
src/GitHubProof.Web/Pages/Profile.cshtml
src/GitHubProof.Web/Pages/Shared/_Profile{PatternSection,StartingPoint,WhyThisView}.cshtml
src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs
src/GitHubProof.Web/wwwroot/css/profile-result.css
src/GitHubProof.Domain/Profile/Models/{RepresentativeRepository,RepresentativePatternSupport,RepeatedProfilePattern}.cs
tests/GitHubProof.Web.Tests/{ProfileResultPresentationTests,ProfileWebPresentationLocalizerTests,CssArchitectureTests}.cs
~~~

---

## Contract / Invariants Checked

~~~text
- §8.6.1b source is PatternSupports identities (PatternFamily + PatternKey) only;
  SupportingEvidenceReferences / SupportingObservationKeys never reach first read.
- TechnicalFocus -> literal language identity; EngineeringPractice -> §8.4.8
  Working Style label; ProblemArea and unknown practice fail loudly.
- Every PatternSupports entry renders in canonical order: no cap, sort, filter
  or first-pattern shortcut. §8.6.1a max-3 REPOSITORY bound untouched.
- Locked EN/TR copy; singular/plural is grammatical agreement, never a count.
- §8.8.3: no level may outrank Meaning and the two recurring-result cards.
- §8.8.4: warnings stay FIRST READ but subordinate; Partial notice unchanged.
- Canonical report ordering (report contract §7) unchanged.
- Working Style Recurrence Lead-In Gap left open; §8.4.8 untouched.
- Starting Point Prioritization Gap left open; a relationship is not a ranking.
~~~

---

## Changes

~~~text
Modified:
- src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs
    * new GetStartingPointPatternRelationship(...)
- src/GitHubProof.Web/Pages/Shared/_ProfileStartingPoint.cshtml
    * relationship line between identity and description
- src/GitHubProof.Web/Pages/Shared/_ProfilePatternSection.cshtml
    * G-2: recurrence lead-in moved above the one canonical list; the
      duplicate trailing PatternKey paragraph removed
- src/GitHubProof.Web/Pages/Profile.cshtml
    * G-4: warnings aside moved below both recurring-result cards
- src/GitHubProof.Web/wwwroot/css/profile-result.css
    * quiet relationship text, lead-in rule (renamed from footer), G-3 spacing
- docs/product/profile-proof-distinctive-outcomes-contract.md
    * Outcome 4 PARTIALLY SUPPORTED -> SUPPORTED NOW + stale-sentence
      corrections + amendment record. Outcome set, priority, acceptance
      criteria, moat and first-read order unchanged.
- docs/product/p10-web-profile-flow-implementation-plan.md
    * STALE pointer note only; §8.6.1b not duplicated
- tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs
- tests/GitHubProof.Web.Tests/ProfileWebPresentationLocalizerTests.cs

Created:
- worklog/sessions/2026-08-27-137-profile-first-read-ui-implementation.md

Deleted:
- none
~~~

No Domain, Application, Infrastructure, architecture-contract or runtime
configuration file was touched. No live provider call was made.

---

## Slice Results

~~~text
A  Starting Point pattern relationship            PASS   (G-1 CLOSED)
B  G-2 duplicated technology rendering removed    PASS   (G-2 CLOSED)
C  G-3 Why affordance perceptibility              PASS desktop / PARTIAL mobile
                                                  (G-3 OPEN — accepted)
D  G-4 warning placement                          PASS   (G-4 CLOSED)
~~~

**Rendered order inside a Starting Point item:** identity → relationship →
GitHub description → Repository Proof action.

**Rendered order in the primary column:** Meaning → Visible technical direction
→ Working style → observation notes (subordinate) → Starting point rail.

---

## Decisions

~~~text
- The relationship is one dedicated narrow localizer method; Razor hands it the
  whole PatternSupports collection so no inline Take/First/OrderBy can
  introduce a hidden bound. A source-scan test guards the method body too.
- G-2 was fixed by MOVING the existing §8.7.6 lead-in above the canonical list
  rather than deleting either surface, so the recurrence relationship survives
  and the identities render exactly once.
- G-3 uses SEPARATION ONLY (margin-top). A stronger border was implemented,
  then reverted during the review pass: it would have made the L3 Why card the
  most emphasised card after Meaning, outranking the L2 recurring-result cards
  that §8.8.3 protects unconditionally. An L2 >= L3 invariant test was added.
- The relationship line is deliberately NOT line-clamped. A repository can
  represent several patterns; clamping, capping or ellipsis would delete
  semantic content the contract requires to be complete. The rail's existing
  internal scroll absorbs the extra height.
- Outcome 4 was upgraded only after the independent review confirmed the full
  §8.6.1b delivery, not because code exists.
~~~

---

## Validation

~~~text
dotnet build (solution)
PASS — 0 warnings, 0 errors

dotnet test tests/GitHubProof.Web.Tests (targeted)
PASS — 585/585

dotnet test (full applicable suite)
PASS — 2338/2338 at the pre-review-fix point
       (Web 584, Domain 842, Infrastructure 636, Application 276);
       2339 after the review-pass guard test was added

git diff --check
PASS

git status --short
Only the changed files below plus pre-existing ?? artifacts/
~~~

**Prohibited-inference audit:** the changed user-facing surfaces render no
junior/senior/skilled/experienced/expert/capability/employability/candidate/
best/strongest/recommended/highest-quality/score/confidence/AI-authorship/
originality language. Every such word in `_ProfileStartingPoint.cshtml` sits
inside a Razor comment stating the prohibition. Two tests assert this
positively (rendered copy in both cultures, and all five Profile partials).

**Visual / responsive:** the human review results are recorded in their own
section below; 880px PASS, 390px PASS except the G-3 mobile finding. The
repository carries **no browser or DOM rendering
harness** (no Playwright/Puppeteer/Selenium/AngleSharp/WebApplicationFactory in
`tests/`), so responsive behaviour is covered only by the existing
deterministic static-CSS guards — desktop rail stretch/internal scroll,
`@media (max-width: 60rem)` single-column collapse, mobile natural flow,
no-horizontal-overflow and themed scrollbar tests all still pass, and the new
relationship rule is asserted to wrap rather than clamp. Human pixel review has since been performed by the
architecture owner and is recorded below; no harness was invented for it.

---

## Independent Review

`github-proof-review`, forked cold context, over `7038159` → working tree.

**Verdict: PASS WITH FINDINGS — BLOCKER 0, HIGH 1, MEDIUM 2, LOW 3.**

It independently confirmed §8.6.1b compliance (identity-only source, label
mapping, completeness/no-cap, character-exact EN/TR copy, loud failure paths),
G-2, G-4, DOM/report order, overflow behaviour and no-count/no-ranking/
no-inference, and verified 2338/2338 with a 0-warning build.

All six findings were closed in a bounded same-session correction pass:

~~~text
F-1 HIGH    Outcome 4 was still recorded PARTIALLY SUPPORTED in four places
            in the distinctive-outcomes contract, contradicting the delivered
            Web. FIX: classification moved to SUPPORTED NOW with an amendment
            record; the stale sentences corrected; §8.6.1b added to Outcome
            4's CURRENT CANONICAL BASIS. Outcome set, priority (SUPPORTING),
            acceptance criteria, moat, first-read order and the Starting Point
            Prioritization Gap all unchanged; §10 triggers no version bump for
            a capability classification.

F-2 MEDIUM  worklog/CURRENT.md still declared Session 136 active with base
            82569fa. FIX: this session file created; CURRENT and INDEX updated.

F-3 MEDIUM  G-3's --border-default override made the L3 Why card the most
            emphasised card after Meaning, outranking the L2 cards that
            §8.8.3 protects. FIX: border override reverted, prominence left to
            spacing; the test now forbids border/background/weight emphasis on
            the Why card and asserts the shared L2/L3 card treatment.

F-4 LOW     Profile.cshtml's header comment inserted warnings into the
            canonical experience order. FIX: reworded so the note is described
            as a subordinate status notice outside that order.

F-5 LOW     The p10 plan STALE note credited §8.6.1b to Session 137.
            FIX: Session 136 authorized it, Session 137 rendered it.

F-6 LOW     The new localizer method had behavioural tests only.
            FIX: added a source-scan guard over the method body forbidding
            SupportingEvidenceReferences / SupportingObservationKeys /
            EvidenceKey / AnalysisReference and any Take/Skip/OrderBy/Where.
~~~

**Second review round** (post-correction, forked cold context):
**PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 0, LOW 1.** It independently
re-ran the build and full suite (0 warnings, 2339/2339), confirmed all six
findings genuinely closed at canonical/CSS level rather than cosmetically, and
confirmed the correction pass produced no contract drift, no weakened test and
no scope expansion — in particular that the three superseded assertions were
replaced by strictly-narrower-or-equal ones and that the `PatternSupports`
allowance is scoped to `_ProfileStartingPoint.cshtml` only. Its one LOW was
closed in the same pass:

~~~text
F-7 LOW     Outcome 4's UI CONSEQUENCE opener still read as a future
            requirement ("sonraki UI/UX çalışması ... görünür kılmalıdır")
            even though the paragraph below it records the delivery.
            FIX: reworded to past tense with the Session 137 closure.
~~~

Two of its non-blocking observations were also acted on:
`worklog/INDEX.md`'s Session 136 row now carries its integrated commit
`7038159`; and the relationship line sharing the description's visual weight
(`--fg-tertiary` / `--font-size-small`) is recorded below as a specific item
for the pending human pixel review. Its remaining observation —
`p10-web-profile-flow-implementation-plan.md` still listing the Starting Point
Description Contract Gap as open — is **pre-existing and out of Session 137
scope**, deliberately not touched.

---

## Human Visual / Responsive Review

Performed by the architecture owner after the second review round, replacing
the previously PENDING entry.

~~~text
880px   PASS
390px   no horizontal overflow
        Starting Point relationship wraps correctly
        repository identity / description / action remain usable
        G-1 visual delivery PASS
        G-2 visual delivery PASS
        G-3 PARTIAL — see below
~~~

### G-3 mobile finding — PARTIAL (as raised; owner decision recorded below)

**Finding.** At ~390px the three Starting Point repository cards form a long
L4 block before the existing `Bunu neye dayanarak söylüyoruz?` /
`What is this based on?` summary becomes visible. The layout is technically
correct, but the Why affordance is visually buried behind the Starting Point
block and does not reach the intended first-read perceptibility on mobile.

**No source change was made.** A correction pass was scoped, measured and
deliberately not shipped, because every lever with enough magnitude to move the
Why summary into the mobile first read is a contract violation, and the only
authorized lever is far too small to resolve the finding.

**Measurement (≤45rem, 390 x 844).** Per presented repository item:

~~~text
item padding (0.75rem + 1rem)                       1.75rem
identity chip                                     ~ 1.90rem
3 x internal gap (--space-3)                        2.25rem
relationship line (small type, wraps ~3 lines)    ~ 3.75rem
description (2-line clamp)                        ~ 2.50rem
action button                                     ~ 2.50rem
                                                  ---------
per item                                          ~14.65rem  (~234px)
x3 items + card padding + heading                 ~  782px
~~~

With the identity header, Meaning and the two stacked recurring-result cards
above it, the Why summary begins roughly 1600-1800px down — about two viewport
heights below the fold.

**Authorized lever and why it is insufficient.** p10 §8.8.9 permits refining
spacing and grouping inside the §8.7.6 visual sequence. The maximum honest
density saving is the per-item internal gap and padding:

~~~text
item gap   --space-3 -> --space-2   0.25rem x 3 gaps x 3 items = 2.25rem
item padding trim                   ~0.25rem x 2 x 3 items     = 1.50rem
                                                               ---------
total                                                  ~3.75rem (~60px)
~~~

~60px against a ~1600px offset is ~4%. It cannot bring the affordance into the
first read, so shipping it would be cosmetic churn that lets the session claim
"improved" while the owner's actual criterion stays unmet. It was rejected on
that basis, not because it was unauthorized.

**Levers that would work, and the constraint each breaks:**

~~~text
reorder Why above Starting Point        canonical DOM/report order (report
                                        contract §7, p10 §8.6.1) - forbidden
keep the bounded rail height + internal
scroll at the single-column breakpoint  p10 §8.7.6 locks "returning to natural
                                        flow at the single-column breakpoint";
                                        also asserted by
                                        Starting_point_card_returns_to_natural_
                                        flow_on_mobile
present fewer than 3 repositories on
mobile                                  below the authorized §8.6.1a bound
cap or truncate PatternSupports         §8.6.1b completeness - forbidden
sticky / floated Why summary            reads as a second Why control and
                                        changes its canonical position
open Why by default on mobile           §8.6.2 locks collapsed-by-default
~~~

**Contract decision that was referred to the owner.** The narrowest
candidate is p10 §8.7.6's single sentence
`... returning to natural flow at the single-column breakpoint`. Retaining a
bounded height with the existing internal vertical scroll at mobile would keep
all three presented repositories present, complete and reachable — nothing
hidden, capped or reordered — while letting the Why affordance rise into the
first read. That is a §8.7.6 presentation amendment plus a matching update to
the mobile natural-flow test, and it is explicitly **out of Session 137 scope**:
this session was told not to reopen product contracts.

### Architecture-owner decision on G-3 (accepted)

~~~text
G-3 mobile PARTIAL            ACCEPTED as a known presentation gap
p10 §8.7.6 amendment          NOT made in Session 137
mobile nested scroll          NOT introduced
carousel                      NOT introduced
reordered Why                 NOT introduced
duplicate Why affordance      NOT introduced
any other workaround          NOT introduced
~~~

**Nature of the remaining gap.** It is a **presentation hierarchy** gap only:
on narrow viewports the Why affordance sits further down the single-column
stack than the first-read intent wants. It is **not** a correctness failure,
**not** a traceability failure, **not** an evidence failure and **not** an
Outcome 4 capability failure. Every canonical guarantee still holds on mobile:
the Why disclosure is present, collapsed by default, in canonical order, with
its locked EN/TR copy; the two-stage trace is intact; all three presented
Starting Point repositories render complete with their full `PatternSupports`
relationship; and nothing is hidden, capped, reordered or truncated.

**Recorded as:** *Mobile First-Read Affordance Hierarchy Gap*. This name is
**worklog-only**. It is deliberately **not** registered as a canonical p10 gap,
because doing so would be a product-contract change the owner explicitly
withheld from Session 137. Registering it canonically — most likely alongside
p10 §8.7.6 / §8.8 — remains a separate owner decision.

The G-3 desktop work (separation-only spacing, L2 >= L3 invariant) stands and
is unaffected. **G-3 is PASS on desktop and PARTIAL on mobile.**

---

## Closure

~~~text
Architecture-owner closure decision   ACCEPTED
Session 137 lifecycle                 PARTIAL

G-1  Starting Point pattern relationship        CLOSED
G-2  Duplicated technology rendering            CLOSED
G-3  Why affordance perceptibility              PARTIAL — mobile first-read
                                                affordance hierarchy gap OPEN
G-4  Warning placement                          CLOSED

Outcome 4                                       SUPPORTED NOW (unchanged)
Responsive result                               PASS WITH KNOWN MOBILE
                                                HIERARCHY GAP
Milestone B                                     NOT CLAIMED
~~~

`PARTIAL` rather than `PASS` because one of the session's four stated
objectives is only partly delivered on mobile. It is not a failed session: the
owner accepted the closure, and the remaining gap is presentation hierarchy,
not correctness, traceability, evidence or Outcome 4 capability.

Still open and untouched, as before: Starting Point Prioritization, Working
Style Recurrence Lead-In, Empty-Result First-Read Repetition, Empty
Limitations, Project-Type Presentation, Recency, Shareable Profile URL, Social
Preview — plus the worklog-recorded Mobile First-Read Affordance Hierarchy Gap.

---

## Outcome 4 Exit Check

~~~text
Can the user see  Recurring Pattern -> Starting Point Repository ...
  without opening evidence?            YES  (first-read Starting Point card)
  without seeing a count?              YES  (no digit; test-asserted)
  without selection mechanics?         YES  (labels only)
  without a ranking/recommendation?    YES  (test-asserted forbidden words)
~~~

**Outcome 4: SUPPORTED NOW.** Upgraded only after independent verification of
the full §8.6.1b delivery. Outcomes 1-3 unchanged; the outcome set is still
exactly four; evidence convergence remains NOT SUPPORTED.

---

## Problems

~~~text
Problem:
Two existing tests encoded the pre-§8.6.1b contract and forbade PatternSupports
in the Starting Point partial; one G-2 test asserted the duplicate list must be
present; one Working Style test forbade any <p> before the else branch.

Cause:
Those assertions were correct for the contract in force when written.

Resolution:
Updated to the current contract without weakening intent. PatternSupports is
now allowed and SupportingEvidenceReferences/SupportingObservationKeys/
EvidenceKey are forbidden instead; the G-2 test now asserts the duplicate is
gone and the lead-in precedes the single list; the Working Style guard was
sharpened to forbid a <p> inside the list itself and to require the single
permitted paragraph to be the TechnicalFocus-gated lead-in.
~~~

---

## Not Done

~~~text
- Human pixel / responsive review (no render harness; recorded PENDING).
- G-5 Working Style Recurrence Lead-In: left open, §8.4.8 untouched.
- G-6 Empty-Result First-Read Repetition: left open, §8.5 untouched.
- Starting Point Prioritization, Empty Limitations, Project-Type Presentation,
  Recency, Shareable Profile URL, Social Preview: all left open and untouched.
- No commit, push or merge.
~~~

---

## Known Risks

~~~text
- The Starting Point rail has a bounded desktop height with internal scroll.
  Each item is now one line taller, so a 3-item rail scrolls sooner. This is
  the designed behaviour and preserves the §8.6.1a presentation bound, but it
  is the main thing human pixel review should look at.
- A repository representing many patterns produces a long relationship line.
  It wraps by contract; the visual result at narrow widths is unverified
  without a render harness.
- The relationship line and the GitHub description line currently share the
  same visual weight (`--fg-tertiary`, `--font-size-small`). §8.6.1b states no
  relative weight between them, so this is not a contract violation, but it is
  a specific thing for the pending human pixel review to judge.
- The L2 >= L3 guard is scoped to one CSS block; emphasis introduced later
  from inside `@media (max-width: 60rem)` under a different selector would not
  be caught. This matches the repository's existing static-CSS test convention.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- docs/product/p10-web-profile-flow-implementation-plan.md
- docs/product/profile-proof-distinctive-outcomes-contract.md
- src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs
- src/GitHubProof.Web/Pages/Profile.cshtml
- src/GitHubProof.Web/Pages/Shared/_ProfilePatternSection.cshtml
- src/GitHubProof.Web/Pages/Shared/_ProfileStartingPoint.cshtml
- src/GitHubProof.Web/wwwroot/css/profile-result.css
- tests/GitHubProof.Web.Tests/ProfileResultPresentationTests.cs
- tests/GitHubProof.Web.Tests/ProfileWebPresentationLocalizerTests.cs
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- worklog/sessions/2026-08-27-137-profile-first-read-ui-implementation.md

Pre-existing unrelated:
- artifacts/   (user-owned, untouched)
~~~

---

## Next

None for Session 137 — the architecture owner accepted its closure as PARTIAL.
The Mobile First-Read Affordance Hierarchy Gap stays open and is picked up only
by a future, separately authorized owner decision on p10 §8.7.6 / §8.8.

---

## Git

~~~text
Suggested commit:
feat(profile): expose first-read evidence relationships

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
~~~

---

## Completion Checklist

~~~text
[x] Goal tamamlandı
[x] Targeted validation çalıştı
[x] Broader validation çalıştı (full suite + 0-warning build)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Exact Next Action güncellendi
[x] Independent review x2 PASS WITH FINDINGS; all 7 findings closed
[x] Human pixel / responsive review — 880 PASS, 390 PASS except G-3 mobile
[x] Architecture-owner closure decision recorded — PARTIAL, accepted
[x] Session Result set to PARTIAL (not PASS): G-3 mobile stays open
[x] Kullanıcıya kısa final rapor verildi
~~~

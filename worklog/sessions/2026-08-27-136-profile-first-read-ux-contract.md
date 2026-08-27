# Profile First-Read UI/UX Contract

**Date:** 2026-08-27
**Result:** DOCS
**Scope:** docs (product presentation contract)
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Lock what a visitor must understand in the first 5–10 seconds of a successful
Profile Proof result, and at what depth each kind of information is presented —
without deciding visual style and without touching any production surface.

---

## Starting State

~~~text
Branch:
session/136-profile-first-read-ux-contract

Base HEAD:
82569fa

Pre-existing Working Tree:
?? artifacts/   (user-owned, untouched)

Relevant Previous Session:
worklog/sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md
~~~

`main` == `origin/main` == `HEAD` == `82569fa` verified before branching.

---

## Model Route

~~~text
Selected:
Opus / high

Why:
Cross-contract product/presentation ownership decision over four locked
canonical owners; wrong ownership choice creates duplicate normative truth.

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
docs/product/profile-proof-distinctive-outcomes-contract.md
docs/product/p10-presentation-language-contract.md  (§1-§9)
docs/product/public-profile-evidence-product-contract.md  (§4, §24, §25)
docs/product/product-positioning.md  (§1-§3)
docs/product/p10-web-profile-flow-implementation-plan.md  (P10-I5, §13.13)
src/GitHubProof.Web/Pages/Profile.cshtml
src/GitHubProof.Web/Pages/Shared/_Profile{Meaning,PatternSection,StartingPoint,WhyThisView,RepositoryEvidence}.cshtml
src/GitHubProof.Web/Localization/ProfileWebPresentationLocalizer.cs
src/GitHubProof.Web/wwwroot/css/profile-result.css  (first-read layout only)
src/GitHubProof.Domain/Profile/Models/RepresentativeRepository.cs
~~~

---

## Contract / Invariants Checked

~~~text
- Distinctive outcome set is exactly four; Outcome 4 PARTIALLY SUPPORTED.
- Evidence convergence NOT SUPPORTED; not introduced.
- First read carries no count, score, ranking or badge
  (distinctive-outcomes §8.2/§8.3, p10 §8.4.7, §8.6.9).
- Supported + empty != ProfileLimitation != operational warning.
- Meaning may not read RepresentativeRepositories / Why / evidence (p10 §8.4.7).
- Starting Point selection semantics are consumed, not owned
  (public-profile-representative-repositories.md §3-§8).
- Canonical report ordering (report contract §7) unchanged.
- public-profile-evidence-product-contract.md §25 remains the owner of the
  first-read question set; not restated or overridden.
~~~

---

## Ownership Decision

Evaluated whether the first-read depth decision needs a new stable document.

~~~text
§25 (product contract)           owns WHICH QUESTIONS first read must answer
Session 135 §8                   owns WHICH DISTINCTIVE VALUE must be
                                 perceptible, + broad first-read no-count rule
p10 §8.4-§8.7                    owns surfaces, section behavior, exact copy
~~~

The open concern — at what DEPTH each information type is presented, and what
first read does in the boundary cases — is presentation information
architecture, which p10 §8.6.1 already partially owns via its experience-position
diagram. A new document would have become a third owner sitting between §25 and
p10 §8.x and would have duplicated normative truth.

**Decision: extend p10 minimally. No new document.**

---

## Changes

~~~text
Modified:
- docs/product/p10-presentation-language-contract.md
    * amendment-history entry (Session 136)
    * §8.6.1 "renders exactly" list: 3 -> 4 elements
    * NEW §8.6.1b Starting Point Pattern Relationship
    * §8.7.6 one-sentence reconciliation for the new card element
    * NEW §8.8 Profile First-Read Information Depth Contract (§8.8.1-§8.8.9)
- docs/product/profile-proof-distinctive-outcomes-contract.md
    * cross-reference only: header amendment record, §8.1 item 7 status note,
      §9 Outcome 4 status note. Outcome set, priorities, moat, acceptance
      criteria and capability classifications unchanged.
- worklog/sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md
    * worklog-only: appended architecture-owner ratification/integration block;
      Git block PENDING lines replaced by the integrated commit. Historical
      review evidence untouched.

Created:
- worklog/sessions/2026-08-27-136-profile-first-read-ux-contract.md

Deleted:
- none
~~~

No Razor, CSS, JavaScript, C#, domain, application, infrastructure, test or
config file was touched.

**Worklog accuracy correction (same session, worklog-only).** After the review
rounds, the architecture owner's Session 135 closure decision — **PASS**, with
the contract ratified and integrated into `main` as `82569fa` — was recorded,
and the now-obsolete "awaiting ratification" current-state wording was removed
from `CURRENT.md` (Active-Session block and Exact Next Action) and from the
`INDEX.md` Session 135 row, whose Commit cell moved from `—` to `82569fa`.
`CURRENT.md`'s last remote-verified `main` moved from `53991ac` to `82569fa`,
which is what this session's own baseline check verified
(`main` == `origin/main` == `HEAD`). The Session 135 session file keeps its
historical review evidence verbatim; a short ratification/integration block was
**appended** after it, and its Git block's `PENDING` commit/push lines were
replaced by the integrated commit. Session 135's lifecycle value stays `DOCS`:
`PASS` here is the architecture owner's closure decision, not a lifecycle
relabel of docs-only work. Session 136 stays `DOCS` with its own implementation
next action unchanged.

---

## Current First-Read Map (audit of delivered Web)

Desktop successful-result surface, in rendered order.

| # | Element | Current position | Semantic purpose | Outcome served | Problem | Decision |
|---|---|---|---|---|---|---|
| 1 | Back link | top | navigation | none | none | KEEP |
| 2 | Identity header (avatar, `h1` login, `github.com/{login}`, scope line, Open on GitHub) | above report | orientation + honest scope | 3 (scope line) | none | KEEP |
| 3 | Partial notice | above report body | CASE C signal | 3 | none | KEEP |
| 4 | Meaning card — "Bu GitHub ne anlatıyor?" + 2 paragraphs + provenance | first-read left column, top | L1 meaning; sole carrier of practice recurrence wording | 1, 3 | none | KEEP (leads) |
| 5 | Warnings aside "Gözlem notları" | left column, between Meaning and pattern cards | CASE D operational note | none (operational) | interrupts the Meaning → recurring-results reading chain | DEMOTE (stay in first read, below the pattern cards) |
| 6 | "Ne tür çalışmalar görünüyor?" card — `ul` of language labels | left column, paired | L2 technical recurrence | 1 | see #7 | KEEP |
| 7 | "Tekrar eden teknolojiler: {PatternKey list}" footer on that card | directly under #6's list | recurrence lead-in | 1 | renders the SAME labels a second time (`GetPatternLabel` returns the raw `PatternKey` for `TechnicalFocus`), so the card shows one list twice | MERGE (one lead-in + one list) |
| 8 | "Çalışma biçimi" card — bare noun-phrase rows | left column, paired | L2 practice recurrence | 1 | carries no recurrence wording at all; asymmetric with #7 | KEEP + recorded gap |
| 9 | Starting point rail (max 3 repos: full name, GitHub description, analyze action) | right column, adjacent to Meaning | L4 entry point | 4 (partially) | pattern relationship absent, so it reads as a generic repo recommendation; rail placement gives it near-parity with L1/L2 | PROMOTE semantics (add relationship) / keep secondary weight |
| 10 | "Bunu neye dayanarak söylüyoruz?" collapsed disclosure + one-line description | full width, below first-read block | L3 why affordance (first read) / why content (disclosure) | 2 | affordance sits below a full two-column block; on mobile it is far down the stack | KEEP as affordance; prominence is an implementation gap |
| 11 | "Bu görünümün sınırları" card | full width | L5 limitations | 3 | rendered even when empty | DISCLOSURE-ONLY not possible; KEEP subordinate (Empty Limitations gap open) |
| 12 | "Repository kanıtı" collapsed disclosure | full width, last | L5 declared surface + DEEP EVIDENCE entry | 3, 2 | none | KEEP |

Answers to the §13 audit questions:

~~~text
Meaning earns priority?        YES — it leads the primary column and is the
                               only always-rendered narrative surface.
Outcome 1 reads clearly?       PARTLY — technical side yes; practice side only
                               through Meaning (element 8).
Outcome 2 perceivable?         YES as an affordance, but weakly — below the
                               fold on common viewports.
Outcome 3 visible, secondary?  YES.
Outcome 4 looks generic?       YES — confirmed. Identity + GitHub description
                               + analyze action, with no pattern relationship.
~~~

First read is **not** dominated by profile metadata, repository counts,
technical lists, generic explanatory text or raw sections.

---

## Gap Map

~~~text
G-1  CANONICAL CONTRACT GAP (contract half CLOSED here) + IMPLEMENTATION GAP
     current:  Starting Point renders identity + GitHub description + action;
               RepresentativePatternSupport is never rendered.
     desired:  the item names the repeated result(s) it was selected for.
     contract: p10 §8.6.1 locked an exact three-element render shape ->
               amended to four; new §8.6.1b locks source, labels,
               completeness, EN/TR copy and prohibitions.
     impl:     OPEN — Razor/localizer work not done in this session.

G-2  IMPLEMENTATION GAP (presentation redundancy)
     current:  technical card renders its label list, then repeats the same
               labels after the "Tekrar eden teknolojiler" lead-in.
     desired:  one lead-in + one list.
     contract: none needed — §8.7.6 authorizes the labels WITH the lead-in,
               not the same list twice. §8.8.9 authorizes removal.
     impl:     OPEN.

G-3  IMPLEMENTATION GAP (why-affordance prominence)
     current:  the L3 affordance sits below the whole two-column first-read
               block; on mobile it is far down the stack.
     desired:  perceptible within the first read without reordering sections.
     contract: none needed — §8.7.6's visual sequence is preserved;
               §8.8.9 authorizes prominence work inside it.
     impl:     OPEN.

G-4  INFORMATION-HIERARCHY GAP (warning placement)
     current:  the operational warning aside sits between Meaning and its own
               two recurring-result cards.
     desired:  subordinate, not interrupting L1 -> L2.
     contract: none needed — plan §13.13's placement is illustrative ("ör.");
               P10-I5 keeps ownership. §8.8.4 states the constraint.
     impl:     OPEN.

G-5  COPY GAP -> CANONICAL CONTRACT GAP (NOT closed)
     current:  Working Style card carries no recurrence lead-in.
     desired:  symmetry with the technical card.
     contract: would change §8.4.8's locked card shape.
     recorded: "Working Style Recurrence Lead-In Contract Gap" (§8.8.5).

G-6  DISCLOSURE GAP -> CANONICAL CONTRACT GAP (NOT closed)
     current:  CASE B repeats the same absence statement several times.
     desired:  one honest abstention statement, sections still reporting.
     contract: would require amending §8.5's deliberate-repetition rule and
               interacts with the Empty Limitations gap.
     recorded: "Empty-Result First-Read Repetition Contract Gap" (§8.8.6).
~~~

No MISSING PRESENTATION CAPABILITY was found: every first-read requirement is
served by data Web already holds.

---

## Decisions

~~~text
- p10 extended; no new stable document created (see Ownership Decision).
- L1..L5 is a semantic-depth ladder, explicitly NOT a DOM order. This is what
  removes the apparent Session 135 §8.1(4)/(5) vs p10 §8.6.1 ordering
  conflict: Session 135 constrains both the why-path and the starting point
  only RELATIVE TO Meaning, and explicitly leaves concrete section order to
  p10 §8.4-§8.6. No material conflict; no STOP.
- L3 is split into affordance (FIRST READ) and content (PROGRESSIVE
  DISCLOSURE). Without that split the candidate hierarchy would have demanded
  a canonical reorder that neither p10 §8.6.1 nor report contract §7 permits.
- §8.6.1b renders only PatternFamily + PatternKey from PatternSupports.
  SupportingEvidenceReferences stays out of first read.
- The singular/plural copy pair is grammatical agreement, not a count; it
  renders no number and names only identities the pattern cards already show.
- The three-columns-of-differentiation test was applied per first-read
  element and recorded normatively in §8.8.7, not left in the worklog.
- Session 135 edits are cross-reference only; Outcome 4 stays PARTIALLY
  SUPPORTED because authorization is not delivery.
~~~

---

## Validation

~~~text
git diff --check
PASS — no whitespace error

git status --short
PASS — only the two docs + this worklog set; artifacts/ untouched

Semantic checks
PASS — four outcomes intact; Outcome 4 PARTIALLY SUPPORTED; no evidence
convergence, ranking, score, count, developer inference introduced;
Supported+empty / limitation / operational warning kept distinct;
no code, test or config change; single canonical owner per fact.

Prohibited-vocabulary scan of the new §8.6.1b/§8.8 text
PASS — every occurrence of best/top/strongest/recommended/score/ranking sits
inside a prohibition or a NEVER RENDERED row, never an authorization.

Independent review (github-proof-review, forked cold context) — two rounds
PASS WITH FINDINGS both times, BLOCKER 0 / HIGH 0 both times.
Round 1: MEDIUM 1, LOW 3 — all closed.
Round 2: MEDIUM 1, LOW 2 — all closed.
The outcome set, the two p10 additions and the gap map never changed across
either round (see Independent Review).

dotnet build / dotnet test
NOT RUN — docs-only change; no production or test file touched.
~~~

---

## Independent Review

`github-proof-review`, forked cold context, over `82569fa` -> working tree.

**Verdict: PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 1, LOW 3.**

The review independently re-verified the load-bearing claims against code and
canonical docs: `PatternSupports` reaches the Starting Point partial and
carries family + key separately from `SupportingEvidenceReferences`; the
existing localizer already keys labels by family + key, so §8.6.1b needs no new
mapping; the G-2 duplicated technology list is real; canonical report ordering
is unchanged; all eight gaps §8.8.9 holds open are genuinely recorded; and the
three Session 135 edits are cross-reference only with no §10 version trigger.

All four findings were closed in a bounded same-session correction pass:

~~~text
F-1 MEDIUM  §8.8.2's five understandings read as an original requirement set
            while overlapping §25 and Session 135 §8.1.
            FIX: the list is now explicitly DERIVED, with an owner-wins
            precedence rule, so it can never diverge silently.

F-2 LOW     §8.8.4 attributed the technical-label row to §8.4.8, which owns
            the Working Style card only.
            FIX: binding rule corrected to §8.4.5's literal-identity rule
            plus §8.7.6.

F-3 LOW     §8.8.3's "nothing outranks L1/L2" appeared to contradict §8.8.4 /
            CASE C, which place the Partial notice above the report body.
            FIX: §8.8.3 now states that the ladder ranks report content only;
            the identity header and the P10-I5 status area are not levels, the
            Partial notice's position is a truthfulness requirement, and it is
            the single status surface that may precede Meaning.

F-4 LOW     §8.6.1b left the ProblemArea family implicit.
            FIX: the two authorized families are now named as the whole scope,
            and a ProblemArea support reaching this surface is stated to be a
            contract violation that must fail loudly.
~~~

**Second review round** (post-correction, same forked cold-context skill):
**PASS WITH FINDINGS — BLOCKER 0, HIGH 0, MEDIUM 1, LOW 2.** It confirmed F-2
and F-4 fully closed against code and canonical docs (including that
`ProfilePatternFamily` really carries exactly three values and that the
localizer already throws for `ProblemArea`, so §8.6.1b locks existing behavior
rather than inventing it), and F-1/F-3 substantively closed with one low
residual each. All three were closed in a second bounded pass:

~~~text
R-1 MEDIUM  CURRENT.md and INDEX.md still said "Independent review PENDING"
            while this file recorded a completed review — three record
            surfaces disagreeing about the same event.
            FIX: both updated to the real two-round outcome; the now-obsolete
            "run the review" next step removed and the list renumbered.

R-2 LOW     §8.8.2 claimed an "order of necessity" that reordered items 3 and
            4 relative to distinctive-outcomes §8.1's locked prominence order,
            whose §10 treats a first-read order change as a version trigger.
            FIX: the owner's order restored verbatim and stated as carried
            over, not re-derived.

R-3 LOW     §8.8.3's "no non-report surface" clause read as either dangling or
            as still binding the two surfaces the correction paragraph exempts.
            FIX: the bullet now forbids introducing a NEW competing surface,
            and the paragraph names the exemption as exactly two existing
            surfaces and closes it.
~~~

**Recorded observation, not a finding of this session.**
`docs/product/p10-web-profile-flow-implementation-plan.md` (~L693-699) still
describes the Starting Point item as literal identity plus action with no
description line. That staleness began with Session 111 STEP 3, not here;
presentation ownership is p10's, so it is not contract drift. §8.6.1b makes it
one element staler. Deliberately **not** edited in this session under the
minimal-extension rule; the implementation session that touches that surface
should add the `Amended` note.

---

## Problems

~~~text
Problem:
The brief's §6E/§14 describe the Starting Point pattern relationship as a
canonical contract gap, while §23 also allows a session to merely state what
must change first. Both readings were available.

Cause:
Session brief ambiguity.

Resolution:
Performed the p10 §8.6.1 amendment in this session. The brief defines this as
a PRODUCT UX / PRESENTATION CONTRACT session and requires an IMPLEMENTATION
INPUT deliverable that says what implementation is ALLOWED to do; leaving the
authorization open would have made that deliverable vacuous for the session's
central gap. The gap map still records the gap and its classification, and the
implementation half stays explicitly OPEN.
~~~

---

## Not Done

~~~text
- No Razor/CSS/localizer implementation of §8.6.1b (G-1 implementation half).
- G-2, G-3, G-4 left as authorized-but-unimplemented.
- G-5 and G-6 recorded as named contract gaps, deliberately NOT closed.
- No visual style decision (color, type, spacing, animation, breakpoints).
- Starting Point Prioritization Contract Gap left open and untouched.
- No `Amended` note added to `p10-web-profile-flow-implementation-plan.md` for
  its stale Starting Point description (pre-existing since Session 111 STEP 3;
  minimal-extension rule).
~~~

---

## Known Risks

~~~text
- §8.6.1b adds a fourth element to a compact card that is already bounded to
  ~2 description lines inside a fixed-height desktop rail. The relationship
  line's length is unbounded when one repository represents many patterns;
  the implementation session must handle overflow without capping the pattern
  list, since capping would invent a hidden top-N.
- §8.8.4's depth table cites surface rules across §8.4-§8.7. Any future
  amendment to those sections must be checked against that table, which
  §8.8.1 already subordinates to the surface rules on conflict.
- `p10-web-profile-flow-implementation-plan.md`'s Starting Point description is
  now two elements behind p10 (description line + relationship line). An
  implementer reading the plan instead of p10 would build the wrong card.
  Presentation ownership is unambiguous, so this is a readability risk, not a
  contract conflict.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- docs/product/p10-presentation-language-contract.md
- docs/product/profile-proof-distinctive-outcomes-contract.md
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md

Untracked:
- worklog/sessions/2026-08-27-136-profile-first-read-ux-contract.md

Pre-existing unrelated:
- artifacts/   (user-owned, untouched)
~~~

---

## Next

Open a bounded Profile Web implementation session that renders the §8.6.1b
relationship line in `_ProfileStartingPoint.cshtml` +
`ProfileWebPresentationLocalizer`, then applies G-2, G-3 and G-4 under the
§8.8.9 boundary.

---

## Git

~~~text
Suggested commit:
docs(profile): lock Profile first-read information depth contract

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
~~~

---

## Completion Checklist

~~~text
[x] Goal tamamlandı
[x] Targeted validation çalıştı (docs-only)
[x] Broader validation gerekmedi (no code/test/config change)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Independent review x2 PASS WITH FINDINGS; all 7 findings closed
[x] Kullanıcıya kısa final rapor verildi
~~~

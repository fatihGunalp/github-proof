# `--fg-muted` Accessibility Audit & Correction

**Date:** 2026-08-24
**Result:** PASS
**Scope:** web (shared design token + regression guards)
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Resolve the Milestone B accessibility blocker raised by Session 116:
`--fg-muted` was below WCAG contrast for at least some non-exempt consumers.
Audit every consumer individually before mutating, then apply the smallest
correct repair.

---

## Starting State

~~~text
Branch:
session/117-fg-muted-accessibility-audit-correction

Base HEAD:
74f8444

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-24-116-shared-light-theme-tertiary-contrast.md
~~~

Branch and base matched exactly; `main` was at the same commit.

---

## Model Route

~~~text
Selected:
Opus / high

Why:
Only 4 consumer sites, but they span four different WCAG classes and Session 116
proved that treating low-contrast consumers as equivalent is wrong. Token
coupling is tighter than in 116: neutral-400 also serves light --fg-disabled,
dark --fg-tertiary and dark --status-not-applicable-icon. A Milestone B
production-gate blocker, so a wrong PASS propagates into the milestone exit.

User Approval:
devam et
~~~

## Model Escalation

~~~text
None. The gate-time checkpoint (incompatible consumer roles requiring a token
split) did not trigger — the audit proved the roles are compatible.
~~~

---

## Context Read

~~~text
worklog/CURRENT.md
wwwroot/css/tokens.css (full)
wwwroot/css/components.css, report.css, repository-input.css (only the
  --fg-muted rules and the backgrounds their consumers sit on)
Pages/Shared/_GitHubInput.cshtml
Pages/Shared/_RepositoryReportIdentity.cshtml
Pages/Shared/_RepositoryReportDisclaimer.cshtml
Pages/Shared/_RepositoryReportSignals.cshtml / _RepositoryReportUncertainty.cshtml
wwwroot/js/analysis-progress.js (readonly reachability)
tests/GitHubProof.Web.Tests/CssArchitectureTests.cs
~~~

---

## Audit — four consumers, four classifications

A repository-wide scan found exactly 4 `var(--fg-muted)` sites; no additional
consumer exists. Classification was made from what each actually renders, never
from the token or class name.

| # | Selector | Renders | Class | Size | Background | WCAG | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | `components.css:109` `.input::placeholder` | the GitHub-target placeholder — the app's only `.input` | **non-exempt text** | 16px | `--surface-default` | 1.4.3 → 4.5:1 | **FAILED** 2.462 light / 4.353 dark |
| 2 | `report.css:140` `.report-identity__metadata-icon` ×3 (Link2, GitBranch, Code2) | glyphs beside URL / default branch / primary language, all `aria-hidden="true"` | decorative, redundant with adjacent text | 14px | `--surface-subtle` | 1.4.11 **n/a** | exempt |
| 3 | `report.css:140` `.report-identity__url-external` ×1 | external-link glyph inside the URL chip, `aria-hidden="true"`; the chip's affordance is its visible URL text | decorative, redundant | 14px | `--surface-subtle` | 1.4.11 **n/a** | exempt |
| 4 | `report.css:867` `.report-notes li::before` | `content: "—"` list marker | pure decoration — `<ul>`/`<li>` already carries list semantics; removing the dash loses nothing | 14px | `--bg-canvas` | none | exempt |
| 5 | `report.css:1190` `.report-disclaimer__version` | `v@(Model.Disclaimer.PolicyVersion)` → `v1`, the analysis policy version (provenance, not repeated elsewhere) | **non-exempt text** | 13px | `--bg-canvas` | 1.4.3 → 4.5:1 | **FAILED** 2.401 light / 4.463 dark |

Two findings the audit added beyond Session 116's note:

~~~text
- The failure is in BOTH themes, not only light. Session 116 sampled only the
  version string; the placeholder measured 4.353 in dark, also below AA.
- .repository-input__field[readonly] would put the placeholder on
  --surface-subtle, but analysis-progress.js:55 sets readonly only on submit,
  when the field is required and non-empty, so no placeholder renders there.
  That combination is UNREACHABLE and was excluded from the required set.
~~~

Also noted, not acted on: `.report-disclaimer` itself is `--fg-tertiary`
(passing), so before this session the version string was *less* legible than the
prose surrounding it.

---

## Decision — why Option A, not Option B

Step 1 surfaced two coherent repairs. The architecture owner selected **A**.

~~~text
A  remap the shared --fg-muted semantic token   <- SELECTED
B  move the two non-exempt consumers to --fg-tertiary and leave --fg-muted
   untouched (it would then serve only exempt consumers)
~~~

Owner's reasoning, recorded: `--fg-muted` genuinely has non-exempt semantic
text consumers, so the shared role itself must be safe for muted semantic text.
Option B would fix today's two instances while leaving the token intrinsically
unsuitable for text and making future misuse easy. Decorative consumers becoming
somewhat stronger is an accepted side effect while the hierarchy stays coherent.

Token splitting (C) was rejected on evidence: all four consumers want the same
thing — the dimmest text tone. None needs something the others cannot have, so
the roles are compatible and the token still means one coherent thing.

---

## Value selection

Deterministic search along the existing neutral ramp, per theme, taking the
**smallest movement** that clears AA on every reachable non-exempt background
while staying strictly dimmer than the corresponding `--fg-tertiary`. No extra
contrast-distance threshold was introduced.

~~~text
LIGHT   from neutral-400 #9da6b2 toward neutral-550
        admissible band [4.500, 5.501)   (ceiling = light --fg-tertiary on --bg-canvas)
        no existing primitive lies inside it
        SELECTED #6c7582   -> 4.662 on --surface-default, 4.547 on --bg-canvas

DARK    from neutral-500 #6f7885 toward neutral-400
        admissible band [4.500, 7.900)   (ceiling = dark --fg-tertiary on --surface-default)
        no existing primitive lies inside it
        SELECTED #727b87   -> 4.536 on --surface-default, 4.650 on --bg-canvas
~~~

Both required new scale steps. Ordering (lower label = lighter) verified
monotonic across neutral-400 / 490 / 500 / 510 / 550 / 600.

---

## Changes

~~~text
Production:
- src/GitHubProof.Web/wwwroot/css/tokens.css

Tests:
- tests/GitHubProof.Web.Tests/CssArchitectureTests.cs

Canonical docs:
- docs/product/github-proof-ui-system-light-dark-v1.md

Worklog:
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-24-117-fg-muted-accessibility-audit-correction.md   (new)

artifacts/ remains user-owned and unmodified.
~~~

### tokens.css

~~~text
+ --primitive-neutral-490: #727b87;      (between 400 and 500)
+ --primitive-neutral-510: #6c7582;      (between 500 and 550)

  --fg-muted light  : neutral-400 -> neutral-510
  --fg-muted dark   : neutral-500 -> neutral-490   in BOTH declaration paths
                      ([data-theme="dark"] and the prefers-color-scheme fallback)
~~~

Untouched, deliberately: `--primitive-neutral-400/500/550` values,
`--fg-tertiary`, `--fg-secondary`, `--fg-disabled` (light keeps neutral-400 —
inactive controls are exempt), `--status-not-applicable-icon`. No consumer
stylesheet, Razor file or localizer changed.

The 10-unit label granularity is finer than the rest of the scale; it exists
only because both admissible bands are narrow, and is documented in the file.

---

## Regression guards

Extended the existing `CssArchitectureTests` pattern — no second test
architecture, and the Session 116 helpers (`ResolveSemantic`, `ReadPrimitives`,
`ContrastRatio`) are reused unchanged.

~~~text
Muted_text_token_meets_AA_contrast_for_its_non_exempt_text_consumers
    >= 4.5 for --fg-muted in both themes against --surface-default and
    --bg-canvas, both resolved through the semantic token graph.
    The exemption list is explicit in the comment: the decorative "—" marker
    and the four aria-hidden redundant icons impose no requirement, which is
    why --surface-subtle is absent from the required set.

Muted_text_token_declares_exactly_the_three_known_theme_paths
    exactly three declarations; both dark paths must resolve identically
    (ResolveSemantic already fails on disagreement); light != dark.

Muted_text_token_stays_less_prominent_than_tertiary
    DIRECTIONAL, both themes, against the same semantic background:
      contrast(muted, bg) < contrast(tertiary, bg)   and   >= 4.5
    Independent review proved an equality-only check was insufficient: mapping
    muted to a value MORE prominent than tertiary — inverting the hierarchy
    outright — had passed every guard. The directional form needs no invented
    minimum visual distance, and equality is caught as a side effect.
~~~

---

## Mutation probes

Each probe reversible in place, restored and confirmed byte-identical with
`git hash-object` (baseline `d90f35b360bbf51be5e8a02e88d373560025c99b`).

~~~text
A  light -> neutral-400 (pre-fix)
   FAIL "--fg-muted (light) #9da6b2 on --surface-default #ffffff is 2,462:1 ..."

B  dark -> neutral-500 (pre-fix), both paths
   FAIL "--fg-muted (dark) #6f7885 on --surface-default #0b0d11 is 4,353:1 ..."

C  only [data-theme="dark"] -> neutral-400, @media fallback left correct
   FAIL "--fg-muted has conflicting dark mappings: #9da6b2, #727b87"

D  light -> neutral-550 (exact light tertiary value)
   FAIL — caught by the directional guard, since equal is not "less"

G  light -> neutral-600 (MORE prominent than tertiary; still clears AA)
   FAIL "light --fg-muted #505965 contrasts 7,099:1 on --surface-default
         #ffffff, which is not less than --fg-tertiary #5f6875 at 5,640:1."
   This is the exact scenario the previous equality-only guard let through.

H  dark -> neutral-300, BOTH dark paths (MORE prominent than tertiary)
   FAIL "dark --fg-muted #c8ced6 contrasts 12,275:1 on --surface-default
         #0b0d11, which is not less than --fg-tertiary #9da6b2 at 7,900:1."
   Changing only [data-theme="dark"] fires the consistency guard first, so
   both paths were inverted together to isolate the hierarchy assertion.

final state   8/8 CssArchitectureTests PASS
~~~

No probe mutation remains in the tree.

---

## Validation

~~~text
1 fg-muted guards                          3/3      PASS
2 CssArchitectureTests                     8/8      PASS
3 theme / profile / report / localization  94/94    PASS
4 full Web suite                          380/380   PASS
5 full solution                          1785/1785  PASS
                                          (Domain 835 / App 262 / Web 380 / Infra 308)
6 dotnet build -c Release                  0 warnings, 0 errors
7 dotnet format --verify-no-changes        PASS
8 git diff --check                         PASS
9 changed-file scope                       PASS — six files: one production, one
                                           test, one canonical doc, three worklog
~~~

### Computed-style verification on the running app

Real server-rendered `/report-preview` (Development route, no GitHub token
needed), both themes:

~~~text
.report-disclaimer__version  "v1" 13px
    light  #6c7582 on --bg-canvas #fcfcfd = 4.547  PASS
    dark   #727b87 on --bg-canvas #07090c = 4.650  PASS

exempt, measured for the record only (no requirement asserted):
    "—" marker            light 4.547 / dark 4.650  (inherits the fix)
    aria-hidden icon on --surface-subtle
                          light 4.425 / dark 4.341  (below 4.5, correctly exempt)
~~~

Both non-exempt numbers match the static token math exactly.

**Placeholder measurement caveat.** The live landing-page reading is trustworthy
in dark (measured 4.536, matching the math exactly) but **not** in light: this
browser reports `prefers-color-scheme: dark` and applies automatic dark mode to
form controls, so `.input`'s computed background stayed `rgb(11,13,17)` even
with `data-theme="light"` and `--surface-default` correctly resolving to
`#ffffff` on the element itself. That is a browser artifact, not an application
defect — proven by walking the element's own custom-property chain. The light
placeholder figure (4.662) therefore rests on the token math and the guard, not
on a live reading.

---

## Problems

~~~text
Problem 1:
The first token edit script asserted on a 4-space-indented string that also
matched the 8-space @media declaration as a substring.

Cause:
Unanchored substring matching across two indentation levels.

Resolution:
The assert fired before any write, so tokens.css was untouched. Re-applied with
newline-anchored patterns that distinguish the two indentation levels.

Problem 2:
The live light-theme placeholder measurement disagreed with the token math.

Cause:
Chrome auto dark mode on form controls (prefers-color-scheme: dark in this
environment), not the application.

Resolution:
Diagnosed by resolving --surface-default on the input itself (#ffffff, correct)
against its computed background (dark), then verified the correction on a
non-form consumer instead. Reported rather than worked around.
~~~

---

## Not Done

~~~text
- No consumer stylesheet patched; no consumer-specific override created for the
  exempt consumers.
- No Razor, localizer, Domain/Application/Infrastructure change.
- One canonical doc WAS changed — see "Canonical Contract Reconciliation".
- No copy change, no font-size change, no hidden information.
- --fg-tertiary, --fg-secondary, --fg-disabled and --status-not-applicable-icon
  untouched; existing primitive values untouched.
- No broad design-system cleanup; artifacts/ untouched.
- P10 NOT marked complete.
~~~

---

## Canonical Contract Reconciliation (Sessions 116-117)

Independent review discovered a canonical mismatch that **spans Sessions 116 and
117** and that neither session had noticed.

~~~text
docs/product/github-proof-ui-system-light-dark-v1.md is a canonical owner of
the shared theme/token contract. It prescribed:

  §5.1  a neutral ramp with no 490 / 510 / 550 steps
  light --fg-tertiary neutral-500,  --fg-muted neutral-400
  dark  --fg-muted    neutral-500
  §31   the same values as literal hex examples

The same document's §29 requires WCAG 2.2 AA, normal text 4.5:1. Those two
clauses contradict each other: the prescribed mappings cannot satisfy §29 for
the semantic small text that actually uses them. Sessions 116 and 117 resolved
the contradiction in favour of §29 in code, but neither recorded it, and both
session records concluded "no docs change required".
~~~

**Session 116's history is not rewritten.** Its record's "No `docs/` change
required" conclusion was correct as far as that session looked, and was later
found **incomplete** — that is the truthful account and it stays. Session 116 is
already merged as `74f8444`.

Session 117 performs the reconciliation for **both** token corrections, under
the architecture owner's decision (Option (a)): the canonical document is
minimally amended so its token contract matches the accessibility requirement it
already owns. This is not a new design direction.

Applied to `github-proof-ui-system-light-dark-v1.md`, following the file's own
`> **Amended:**` convention:

~~~text
+ one amendment entry dated 2026-08-24 / Sessions 116-117
  §5.1  neutral-490 #727B87, neutral-510 #6C7582, neutral-550 #5F6875 added
        in ramp order; the rest of the palette untouched
  light --fg-tertiary neutral-500 -> neutral-550
        --fg-muted    neutral-400 -> neutral-510
  dark  --fg-muted    neutral-500 -> neutral-490
  §31   light --fg-tertiary #6F7885 -> #5F6875, --fg-muted #9DA6B2 -> #6C7582
        dark  --fg-muted    #6F7885 -> #727B87
~~~

Unrelated mappings (`--fg-primary`, `--fg-secondary`, `--fg-disabled`,
`--fg-inverse`, links, borders, surfaces, accents, status) were not touched, and
the wider document was not rewritten. Mechanically verified afterwards: every
one of the seven token facts now agrees between doc and `tokens.css`, and no
stale old mapping remains in a normative token example.

---

## Follow-up — context-loading miss (recorded, NOT fixed here)

Sessions 116 and 117 both missed
`docs/product/github-proof-ui-system-light-dark-v1.md` during context loading,
even though it is a canonical owner for shared theme/token semantics. Both
sessions routed context from `worklog/CURRENT.md` plus the stylesheets and
consumer markup, and neither consulted the document map for a canonical owner of
the token layer. Session 116's independent review missed it too; only Session
117's review caught it.

This is a process / knowledge-routing concern, not a product defect. Per the
architecture owner it is recorded here and deliberately left unfixed: no change
was made to `AGENTS.md`, `agents/*`, `docs/INDEX.md` or `CLAUDE.md` in this
session. It belongs to a separate knowledge-architecture follow-up.

---

## Known Risks

~~~text
- AA margins are thin by construction: the owner's rule was "smallest visual
  movement necessary" plus "no additional arbitrary contrast-distance
  threshold". Margins over 4.5 are +0.047 (light on --bg-canvas) and +0.036
  (dark on --surface-default). Session 116 had rejected a 0.152 margin as thin;
  the rule here explicitly forbids buying more headroom, so this is a deliberate
  consequence, recorded rather than silently padded.
- Several surface combinations sit below 4.5 with the selected --fg-muted.
  None is used by a non-exempt consumer today, so none is a required contrast
  background and the value was NOT changed to make unused combinations pass:

      dark  --surface-hover / --surface-raised   #14171c   4.190   <- lowest
      light --bg-muted / --surface-disabled      #f1f3f5   4.191
      dark  --surface-subtle / --bg-muted        #101318   4.341
      light --surface-subtle / --surface-hover   #f8f9fb   4.425

  The earlier draft of this record named --surface-subtle as the residual risk;
  that was incomplete — dark --surface-raised/--surface-hover at 4.190 are
  lower. Consequence: any future --fg-muted SEMANTIC-TEXT consumer on a
  background outside the required set needs its own contrast validation before
  it ships.
- No human pixel review: evidence is computed contrast plus live computed style.
- The light placeholder figure is math-and-guard backed, not live-measured
  (browser auto dark mode on form controls).
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Web/wwwroot/css/tokens.css                    (production)
- tests/GitHubProof.Web.Tests/CssArchitectureTests.cs           (tests)
- docs/product/github-proof-ui-system-light-dark-v1.md          (canonical docs)
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- worklog/sessions/2026-08-24-117-fg-muted-accessibility-audit-correction.md

Six tracked/new files in total.

Pre-existing unrelated:
- artifacts/ (user-owned, read-only, untouched)
~~~

---

## Next

The pending P10 exit evaluation.

---

## Git

~~~text
Suggested commit:
fix(web): raise muted text contrast to WCAG AA in both themes

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Audit completed before any mutation (4 consumers, individually classified)
[x] Option A implemented; no consumer reassignment, no token split
[x] Existing primitive values untouched
[x] Both dark declaration paths corrected identically
[x] Deterministic smallest-movement value selection, no invented threshold
[x] Regression guards extend the existing test architecture
[x] Six mutation probes (A, B, C, D, G, H); byte-identical restore after each
[x] Full solution + Release + format + diff --check
[x] Computed-style verification on the real /report-preview surface
[x] Independent review — PASS WITH FINDINGS (1 HIGH / 2 MEDIUM / 2 LOW)
[x] HIGH-1 canonical reconciliation applied (owner Option (a))
[x] MEDIUM-2 hierarchy guard made directional and probe-proven
[x] MEDIUM-3 unused-combination risk recorded accurately
[x] LOW-4 exemption comment corrected
[x] LOW-5 CURRENT present-state accuracy
[x] Context-loading miss recorded as a separate follow-up
[x] Independent re-review — PASS, no new findings; NEW-1/2/3 closed
[x] Kullanıcıya final rapor
~~~

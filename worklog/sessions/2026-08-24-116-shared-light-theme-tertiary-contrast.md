# Shared Light-Theme Tertiary Text Contrast Correction

**Date:** 2026-08-24
**Result:** PASS
**Scope:** web (shared design token + regression guard)
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Resolve the Session 115 escalated accessibility finding: light-theme semantic
tertiary text at ~13px fell below WCAG AA 4.5:1 across Web surfaces. Correct it
at the semantic-token level if — and only if — analysis proved a single bounded
change was safe.

---

## Starting State

~~~text
Branch:
session/116-shared-light-theme-tertiary-contrast

Base HEAD:
f085961

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-24-115-p10-i6-c-responsive-theme-localization-closure.md
~~~

Branch and base matched exactly; `main` was at the same commit.

---

## Model Route

~~~text
Selected:
Opus / high

Why:
The edit is small; the analysis is not. One shared token feeds 34 consumers
across two product surfaces and at least 15 foreground/background combinations
in two themes, the obvious candidate value silently collapses the type
hierarchy, and the repository had no contrast test at all to catch a wrong
choice.

User Approval:
devam et
~~~

## Model Escalation

~~~text
None. The gate-time escalation condition (multiple shared semantic changes or
meaningful visual regression) did not materialise: the correction is one
mapping plus one new primitive, with zero consumer changes.
~~~

---

## Context Read

~~~text
worklog/CURRENT.md
wwwroot/css/tokens.css (full)
wwwroot/css/{profile-result,report,site,home,repository-input,status-badge,components,base}.css
  — only the --fg-tertiary / --fg-muted / background declarations
Pages/ReportPreview.cshtml + .cshtml.cs (Development-only render harness)
tests/GitHubProof.Web.Tests/CssArchitectureTests.cs
~~~

---

## Analysis Phase (before mutation)

### 1-2. Exact mapping and source

~~~text
--fg-tertiary  light  -> --primitive-neutral-500  #6f7885
--fg-tertiary  dark   -> --primitive-neutral-400  #9da6b2   (2 places)
~~~

`--primitive-neutral-500` is NOT exclusively tertiary's: it also feeds light
`--status-not-applicable-icon` and dark `--fg-muted`. Changing the primitive
itself would silently move both. The correction therefore had to change what
light `--fg-tertiary` *points to*, never the primitive.

### 3. Consumers — 34 across six stylesheets

~~~text
report.css           19   evidence li/source, action effort, summary count,
                          score-card value/unavailable, uncertainty count,
                          category status/score, score summary, signal-section
                          count, signal score, disclaimer, section/layer/signal
                          icons and chevrons
profile-result.css    8   identity-context, meaning-provenance, section-empty,
                          technical-focus-footer, starting-point-description,
                          disclosure-description, lead-in, evidence-status
site.css              2   language-selector, language-selector__choice
home.css              2   boundaries__summary, boundaries__list
repository-input.css  2   disabled action states
status-badge.css      1   status-badge base
~~~

Most are semantic text. A minority are icons/chevrons (AA 1.4.3 does not apply)
and two are disabled controls (explicitly exempt). That distribution is what
justified fixing the token rather than patching consumers.

### 4-5. Backgrounds and current light contrast — **fails everywhere**

Session 115 sampled two backgrounds. The full audit shows the failure is
universal, not marginal on a couple of spots:

| Background | Token roles | Ratio |
|---|---|---|
| `#f1f3f5` | bg-muted / surface-disabled / status-not-applicable-bg | **4.016** |
| `#f5f3ff` | status-human-review-bg | 4.073 |
| `#eff5ff` | accent-soft / surface-selected / status-unavailable-bg | 4.079 |
| `#fef3f2` | status-missing-bg | 4.109 |
| `#ecfdf3` | status-strong-bg | 4.235 |
| `#f8f9fb` | bg-subtle / surface-subtle / surface-hover | 4.240 |
| `#fffaeb` | status-partial-bg | 4.281 |
| `#fcfcfd` | bg-canvas | 4.357 |
| `#ffffff` | surface-default / surface-raised | 4.467 |

**All nine fail.** Measured live on the real Repository Proof render
(`/report-preview`, Development-only): 43 tertiary/muted text nodes, 12 distinct
(class, background) combinations, **12 of 12 failing**, carrying genuinely
semantic content — `Tamamlandı`, `7 / 7`, `README`, `README.md#validation`,
`20 / 27`, `/100`.

### 6. Value selection

Requirement: clear 4.5 on the worst reachable light background (`#f1f3f5`) while
staying visibly lighter than `--fg-secondary`.

| Candidate | min across the 9 backgrounds | margin | verdict |
|---|---|---|---|
| `neutral-500` (current) | 4.016 | −0.484 | fails |
| `neutral-600` (= `--fg-secondary`) | 6.382 | +1.882 | **collapses the hierarchy** |
| `#646e7b` | 4.652 | +0.152 | passes, margin too thin for future placements |
| `#626c79` | 4.791 | +0.291 | passes |
| **`#5f6875`** | **5.070** | **+0.570** | **selected** |

`#5f6875` is the exact arithmetic midpoint of `neutral-500` and `neutral-600`,
so it belongs on the existing primitive scale rather than being an arbitrary
colour. The scale already carries intermediate steps (025, 150, 850, 925, 975),
so `neutral-550` follows the established convention.

Hierarchy on white after the change:

~~~text
fg-primary    neutral-925  #101318  18.611
fg-secondary  neutral-600  #505965   7.099
fg-tertiary   neutral-550  #5f6875   5.640   (was 4.467)
fg-muted      neutral-400  #9da6b2   2.462
~~~

Tertiary-vs-secondary separation 1.259 — three distinguishable levels retained.

### 7. Dark theme not degraded

Dark `--fg-tertiary` stays `neutral-400` and was not touched: 7.297 (raised) /
7.560 (subtle) / 7.900 (default) / 8.098 (canvas). Verified again after the
change on the live render: 42 nodes, min 7.297, zero failing.

### 8. Is `tokens.css` alone sufficient? Yes

All 34 consumers inherit the semantic token. No consumer stylesheet, Razor file
or localizer needed to change.

---

## Changes

~~~text
Production:
- src/GitHubProof.Web/wwwroot/css/tokens.css

Tests:
- tests/GitHubProof.Web.Tests/CssArchitectureTests.cs

Worklog:
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-24-116-shared-light-theme-tertiary-contrast.md   (new)

artifacts/ remains user-owned and unmodified.
~~~

### tokens.css — two functional lines

~~~text
+ --primitive-neutral-550: #5f6875;          (new step on the existing scale)
  --fg-tertiary (light): neutral-500 -> neutral-550
~~~

Plus a comment recording why. Dark mappings at both places untouched. No other
token, no accent/status meaning, no copy, no font size changed.

### CssArchitectureTests — two guards

`Tertiary_text_token_meets_AA_contrast_on_every_reachable_background` parses
`tokens.css`, resolves `--fg-tertiary` per theme, and asserts >= 4.5:1 against
the surfaces its consumers actually sit on. Backgrounds are resolved **through
the semantic token graph** (`--bg-canvas`, `--surface-default`,
`--surface-raised`, `--surface-subtle`), not from named primitives, so remapping
a surface token cannot leave the guard asserting a stale colour. Status surfaces
are deliberately excluded with evidence: every `.status-badge--*` variant
overrides `--status-badge-icon` together with `--status-badge-bg`, so
`--fg-tertiary` only ever renders on the base badge background
(`--surface-subtle`).

Every declaration is attributed to its **enclosing block** — bare `:root`,
`[data-theme="dark"]`, and the `@media (prefers-color-scheme: dark)` fallback —
instead of relying on first/last regex match order, and the dark declarations
must agree with each other.

`Tertiary_text_token_is_not_collapsed_into_secondary_in_light_theme` asserts
only that the two light values stay distinct. It deliberately sets **no numeric
visual-distance threshold**: no canonical design-system contract establishes
one, so inventing a ratio would be arbitrary design policy rather than a
protected invariant.

**Proven non-vacuous** by three mutation probes against the real suite, each
restored and confirmed byte-identical with `git hash-object`:

~~~text
baseline tokens.css   git hash-object 055182c4f332cd184112ee5cd901ad20482d6bb3

probe 1  light -> neutral-500 (the pre-fix value)
         FAIL  "--fg-tertiary (light) #6f7885 on --bg-canvas #fcfcfd is
                4,357:1, below the WCAG AA 4.5:1 ..."

probe 2  ONLY [data-theme="dark"] -> neutral-600, @media fallback left correct
         FAIL  "--fg-tertiary has conflicting dark mappings: #505965, #9da6b2"
         This is exactly the scenario the previous first/last-match guard let
         through; the primary dark path is now covered.

probe 3  light -> neutral-600 (collapse onto --fg-secondary)
         FAIL  "Light --fg-tertiary resolves to #505965, the same value as
                --fg-secondary, so the two text levels have collapsed into one."
         AA guard passes here (6.382) — the two guards assert different things.

after each probe   restored, git hash-object 055182c4f332cd184112ee5cd901ad20482d6bb3
final state        both guards PASS 2 / 2~~~

The revert/restore used a reversible in-place string swap rather than a file
copy, so the restore could not depend on a backup path existing (see
Problem 2). Only the AA guard flips; the secondary-separation guard passes in
both states, which is correct — they assert different things.

---

## Decisions

~~~text
- Change the semantic mapping, never --primitive-neutral-500 itself, because
  that primitive also feeds light --status-not-applicable-icon and dark
  --fg-muted.
- Target the worst REACHABLE background (#f1f3f5), not merely the two Session
  115 sampled, so a future placement on a muted or status surface cannot
  reintroduce the failure.
- Reject neutral-600 despite passing: it is --fg-secondary, so it would erase a
  type level to fix a contrast bug.
- Reject #646e7b: 0.152 margin is too thin to survive rounding and future
  background additions.
- Add the guard to CssArchitectureTests rather than a new file: it already owns
  stylesheet-architecture invariants, and nothing in the suite covered contrast.
~~~

No product, domain or architecture contract changed. No `docs/` change required.

---

## Validation

~~~text
targeted CssArchitectureTests
PASS — 5/5 (3 pre-existing + 2 new)

guard proven non-vacuous
PASS — fails on the pre-change value with the exact ratio, then restored

targeted CssArchitecture + ThemeControl + ProfileResultPresentation
PASS — 50/50

dotnet test tests/GitHubProof.Web.Tests
PASS — 377/377 (was 375; +2 new)

dotnet test (full solution)
PASS — 1782/1782
  Domain 835 / Application 262 / Web 377 / Infrastructure 308

dotnet build -c Release
PASS — 0 warnings, 0 errors

dotnet format --verify-no-changes
PASS

git diff --check
PASS

scope / status review
PASS — one production file, one test file and three worklog files; no other
production, test or docs file changed; ?? artifacts/ untouched
~~~

### Live re-measurement after the change

Real Repository Proof render (`/report-preview`), both themes:

~~~text
light --fg-tertiary   42 nodes   min 5.354  max 5.640   0 failing  (was 4.240-4.467, all failing)
dark  --fg-tertiary   42 nodes   min 7.297  max 8.098   0 failing  (unchanged)
~~~

Profile's three Session 115 escalation points, probed under the real cascade:

~~~text
evidence status          4.467 -> 5.640  PASS
disclosure description   4.467 -> 5.640  PASS
identity context         4.357 -> 5.501  PASS
Supported+empty copy (§8.5)       5.640  PASS
dark equivalents         7.297 / 8.098   PASS (unchanged)
~~~

The Profile probe used browser-only injected markup because no `GITHUB_TOKEN`
is configured; it is a theme/contrast probe, **not real Profile truth**. The
Repository Proof numbers above come from a genuine server-rendered page.

---

## Problems

~~~text
Problem 1:
/ReportPreview returned 404.

Cause:
Two reasons in sequence — the page requires the Development environment, and
its route is "/report-preview", not the page name.

Resolution:
Started the app with ASPNETCORE_ENVIRONMENT=Development and used the declared
route. This gave a real Repository Proof render with no GitHub calls, which is
what made the 43-node live audit possible despite an exhausted API quota.

Problem 2:
The verification revert left tokens.css briefly in its pre-change state because
the restore copy targeted a path that did not exist.

Cause:
Two fallback destinations in one command; the surviving backup was the other one.

Resolution:
Restored from /tmp/tokens.restore in the next step and re-verified the file
contents. The verification was then re-run end to end using a reversible
in-place swap with a git-hash equality check on both ends, so the second cycle
had no backup-path dependency at all. Full validation ran against the corrected
file.
~~~

---

## Not Done — reported, not silently solved

~~~text
SEPARATE FINDING — --fg-muted fails AA in BOTH themes.
(Full boundary recorded under "Deferred Blocker — --fg-muted" above.)

  light  neutral-400 #9da6b2 on canvas #fcfcfd   2.401  FAIL
  dark   neutral-500 #6f7885 on canvas #07090c   4.463  FAIL

Observed on .report-disclaimer__version (the "v1" analysis version string);
--fg-muted also appears at report.css:140, report.css:867, report.css:1190 and
components.css:109. This is a DIFFERENT semantic token with a different and
larger shortfall, outside this session's stated goal, and the boundaries
prohibit changing unrelated colours. Left for a separate architecture-owner
decision.

Also not done:
- No consumer stylesheet patched.
- No Razor, localizer, docs/, Domain/Application/Infrastructure change.
- No copy change, no font-size increase, no hidden tertiary information.
- No accent/status meaning altered, no dependency added, artifacts/ untouched.
~~~

---

## Deferred Blocker — `--fg-muted`

Surfaced by this session's audit, deliberately **not** touched here.

~~~text
- It is a SEPARATE semantic token from --fg-tertiary, with its own primitives
  (light neutral-400, dark neutral-500) and no consumer overlap.
- At least two observed consumers are non-exempt semantic text:
  components.css:109  .input::placeholder          (an active input's placeholder)
  report.css:1190     .report-disclaimer__version  (the analysis version string)
- Measured contrast is below WCAG AA in BOTH themes
  (.report-disclaimer__version: light 2.401, dark 4.463).
- It does NOT invalidate the Session 116 --fg-tertiary correction: the two
  tokens are independent and this session changed neither --fg-muted's
  mappings nor the primitives it resolves to.
- The architecture owner classified it as a separate Milestone B
  production-gate blocker, precisely because some consumers are non-exempt
  semantic text rather than decoration.
- Exact remediation is deferred to the next bounded session. No replacement
  token or value is preselected here.
~~~

---

## Known Risks

~~~text
- The change moves quiet text one step darker on every Web surface. It is a
  deliberate, measured shift (5.640 vs 7.099 secondary on white, separation
  1.259), but no human has looked at the rendered result: evidence is computed
  contrast and live computed styles, not a pixel review.
- The Profile-side confirmation used injected markup, not a real analyzed
  Profile, because the unauthenticated GitHub quota is exhausted. Repository
  Proof was verified on a genuinely server-rendered page.
- --fg-muted remains below AA in both themes (above).
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- src/GitHubProof.Web/wwwroot/css/tokens.css          (production)
- tests/GitHubProof.Web.Tests/CssArchitectureTests.cs (tests)
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- worklog/sessions/2026-08-24-116-shared-light-theme-tertiary-contrast.md

Pre-existing unrelated:
- artifacts/ (user-owned, read-only, untouched)
~~~

---

## Next

Architecture-owner decision on the separate `--fg-muted` AA finding, then the
pending P10 exit evaluation.

---

## Git

~~~text
Suggested commit:
fix(web): raise light theme tertiary text contrast to WCAG AA

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Analiz fazı mutasyondan önce tamamlandı (9 adım)
[x] Tek semantic-token düzeltmesi, sıfır consumer değişikliği
[x] Dark tema bozulmadı — ölçülerek doğrulandı
[x] Regression guard eklendi ve boş olmadığı kanıtlandı
[x] Full solution + Release + format + diff --check
[x] Canlı render ile öncesi/sonrası ölçüm
[x] --fg-muted bulgusu sessizce çözülmedi, raporlandı
[x] Independent review — PASS WITH FINDINGS (1 MEDIUM / 4 LOW); all closed
[x] Kullanıcıya final rapor
~~~

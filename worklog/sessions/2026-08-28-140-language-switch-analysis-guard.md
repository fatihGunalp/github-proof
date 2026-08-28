# Session 140 — Prevent Language Switch From Cancelling Active Analysis

**Date:** 2026-08-28
**Result:** PASS
**Scope:** web (+ one owning-paragraph p10 clarification, explicitly authorized by the task)
**Model Route:** Sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

Independent review (`github-proof-review`, forked cold context, Sonnet / high):
**PASS** — BLOCKER 0, HIGH 0, MEDIUM 0, LOW 0. No findings; nothing to fix. Web
641/641 re-verified independently.

---

## Goal

While the landing analysis form is actively submitting a synchronous analysis
POST, the header `EN/TR` control must not initiate navigation or a culture
change (which would abandon/cancel the in-flight `/Index` or `/Profile`
analysis). Idle language behavior unchanged.

---

## Starting State

~~~text
Branch:      session/140-language-switch-analysis-guard
Base HEAD:   d4963ac  (main == origin/main == HEAD verified equal)
Pre-existing: ?? artifacts/   (untouched)
Prev session: 2026-08-27-139-live-analysis-progress-web-implementation.md (PASS WITH FINDINGS, integrated as d4963ac)
~~~

---

## Context Read

AGENTS.md, CLAUDE.md, agents/02, agents/04, agents/06,
p10-presentation-language-contract.md §8.1 / §8.3,
`_Layout.cshtml`, `Language.cshtml.cs`, `Language.cshtml`, `theme.js`,
`analysis-progress.js`, `site.css`, `LanguageControlTests.cs`,
`LanguageModelTests.cs`, `AnalysisProgressContractTests.cs`,
`AnalysisProgressClientContractTests.cs`.

---

## Contract / Invariants Checked

~~~text
- Single source of analysis-busy truth: the landing progress form's existing
  data-submitting / aria-busy lifecycle (Session 138/139). No new flag.
- Synchronous POST architecture unchanged; no result persistence, no replay,
  no in-place translation of a running analysis.
- Session 138/139 progress semantics unchanged (registry, endpoint, polling,
  stage state machine, live region).
- Drill-down forms have no data-analysis-progress, so they never lock or
  restore the language control.
- p10 §8.1: language change from a completed transient result still persists
  culture and returns to localized landing (unchanged). §8.3 owns the
  analysis-running presentation state.
~~~

---

## Changes

~~~text
Modified:
- src/GitHubProof.Web/Pages/Shared/_Layout.cshtml
    + data-language-form on the EN|TR <form>
- src/GitHubProof.Web/wwwroot/js/analysis-progress.js
    + landingAnalysisActive() — reads the landing progress form's own
      data-analysis-progress + data-submitting state
    + setLanguageControlsLocked(locked) — toggles data-analysis-locked on the
      form and aria-disabled + tabindex="-1" on button[name="Culture"]
    + reset(form): restores the language controls (guarded to the
      data-analysis-progress form only), so pageshow / BFCache restore works
    + landing-form submit handler: locks the language controls (same guard)
    + language-form submit listener: event.preventDefault() only while
      landingAnalysisActive() — blocks pointer AND keyboard (both reach the
      form through submit) and the culture-cookie POST
- src/GitHubProof.Web/wwwroot/css/site.css
    + .language-selector[data-analysis-locked] { opacity: 0.5 } and
      .language-selector[data-analysis-locked] .language-selector__choice
      { pointer-events: none; cursor: default } — inactive-component styling
      (WCAG 1.4.3 contrast exemption applies)
- docs/product/p10-presentation-language-contract.md §8.1
    + one paragraph appended to the owning paragraph: while the landing form is
      actively submitting, EN|TR is inert (no navigation, no culture change),
      reuses that form's submitting state as the single busy signal, is
      perceivably/accessibly disabled, and restores on the existing
      reset/BFCache path; no translate/restart/persist; idle behavior unchanged.

Created:
- tests/GitHubProof.Web.Tests/LanguageAnalysisGuardTests.cs (10 tests)
~~~

No Domain / Application / Infrastructure / package / config / appsettings
change. No new user-facing copy.

---

## Decisions

~~~text
- The hard block is one submit-event listener on the language form; it covers
  a pointer click and Enter/Space on a focused choice with a single guard,
  and stops the /Language navigation and the culture cookie together.
- Perceivable/accessible disabled representation: aria-disabled="true" +
  tabindex="-1" (out of the tab order) + container opacity + pointer-events
  none. aria-pressed (current selection) is kept.
- The lock and the restore are both gated on data-analysis-progress, so a
  Profile drill-down submit never touches the language control.
- p10 §8.1 gained a one-paragraph clarification (task-authorized) rather than
  leaving the in-flight behavior undocumented; language semantics unchanged.
~~~

---

## Validation

~~~text
dotnet test tests/GitHubProof.Web.Tests   PASS — 641/641 (was 631; +10)
dotnet test GitHubProof.slnx              PASS — Domain 842, Infra 636, Web 641, App 276 = 2395/2395
dotnet build GitHubProof.slnx             PASS — 0 warnings, 0 errors
git diff --check                          PASS (only LF→CRLF advisories, exit 0)

Live browser verification (dotnet run, Development, JS inspection):
- idle: language form not locked, submit not prevented
- active landing analysis (data-submitting="true"): form gets
  data-analysis-locked; EN gets aria-disabled="true" + tabindex="-1";
  aria-pressed kept; language submit → preventDefault (blocked)
- pageshow reset: data-analysis-locked cleared, aria-disabled/tabindex
  removed, language submit allowed again
- theme toggle: no aria-disabled, no tabindex change — unaffected
- no console errors
~~~

---

## Problems

None.

---

## Not Done

~~~text
- No change to completed-result language behavior (p10 contract unchanged there).
- No in-place translation / replay / persistence of a running analysis (out of
  scope by instruction).
- Drill-down in-flight POSTs are not covered — by instruction, only the landing
  progress form owns this state.
~~~

---

## Known Risks

~~~text
- If JS fails to load, the language control keeps today's behavior (a click
  during analysis would still cancel it) — same graceful degradation as the
  Session 138/139 progress UI, not a regression.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- docs/product/p10-presentation-language-contract.md
- src/GitHubProof.Web/Pages/Shared/_Layout.cshtml
- src/GitHubProof.Web/wwwroot/css/site.css
- src/GitHubProof.Web/wwwroot/js/analysis-progress.js

Untracked (this session):
- tests/GitHubProof.Web.Tests/LanguageAnalysisGuardTests.cs
- worklog/sessions/2026-08-28-140-language-switch-analysis-guard.md

Pre-existing unrelated:
- artifacts/   (untouched)
~~~

---

## Next

Independent review done (PASS, no findings). Session 140 is complete and
STOPPED. Remaining step is user-owned: review the diff and, if accepted, commit
`fix(web): protect active analysis from language navigation` and integrate.

---

## Git

~~~text
Suggested commit:
fix(web): protect active analysis from language navigation

Commit / Push:  PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Goal implemented
[x] Targeted validation ran (Web 641/641)
[x] Broader validation ran (solution 2395/2395, build 0 warnings)
[x] git diff --check reviewed
[x] git diff/status reviewed
[x] Contract/invariant drift checked — p10 §8.1 clarified (task-authorized)
[x] Session Result updated — PASS
[x] INDEX row updated
[x] CURRENT updated
[x] Final user report
~~~

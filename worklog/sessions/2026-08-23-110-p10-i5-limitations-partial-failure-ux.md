# Session 110 — P10-I5 Limitations + Partial / Failure UX

**Date:** 2026-08-23
**Result:** PASS WITH FINDINGS — P10-I5 implemented; architecture-owner review
required; production configuration inputs remain separate/open
**Branch:** `session/110-p10-i5-limitations-partial-failure-ux`
**Base HEAD:** `959069d383a3179f063520621831f74f044bedd9`
**Model Route:** model gate completed before execution

## Goal

Implement the bounded P10-I5 Web presentation slice so a real canonical
Partial profile remains visible with its canonical limitations, while typed
profile/provider failures and operational admission/execution guard outcomes
remain distinct, localized recovery states and never become evidence,
limitations or completed-so-far profiles.

## Mutation Boundary

Scope is limited to the existing Profile Razor/PageModel presentation boundary,
Profile Web localization, bounded Profile result CSS, tests and this Session 110
worklog state. P10-I6 visual restructuring, Domain/Application semantics,
Infrastructure behavior, configuration, Repository Proof behavior and the
user-owned `artifacts/` handoff are out of scope.

## Pre-existing Working Tree

- Local `main`, `origin/main` and the requested base matched `959069d`.
- `?? artifacts/` is user-owned, untracked, read-only for this session and
  remains out of scope.

## Implemented UX Mappings

- Visitor admission rejection remains HTTP `429`, preserves bounded
  `Retry-After`, produces no report and now has its own calm retry-later copy.
- Same-profile admission rejection remains HTTP `409`, produces no queue,
  reused result or report and says only that this profile is already being
  analyzed.
- Global capacity rejection remains HTTP `503`, produces no report and uses a
  distinct temporary-capacity message.
- Profile-only GitHub budget protection remains the exact typed
  `Unavailable / github-budget-protection-rejected` execution outcome and HTTP
  `503`, with no report and no remaining-count/floor/header/token disclosure.
- Internal execution deadline remains exact
  `Failed / execution-deadline-expired`, HTTP `504`, no report and no Partial;
  the same presentation applies when truthful Profile availability had already
  resolved as Available.
- Typed Profile availability continues to distinguish `NotApplicable`,
  `RateLimited`, `Forbidden`, `Failed`, `Unavailable` and canonically valid
  `NotRequested` without reading or exposing raw provider reasons.
- A terminal `ProfilePipelineResult.Failed` uses composition-failure copy and
  is not mislabeled as admission, provider protection or deadline failure.

## Partial, Warnings and Limitations

- `IsPartialProfile` is derived only from the canonical terminal
  `ProfileCompositionStatus.Partial`. Operational/provider/admission outcomes
  cannot set it.
- A real Partial keeps the entire typed `ProfileReport` visible and receives an
  early localized explanation that the result is based only on observable
  evidence; no percentage, score, confidence or developer-strength statement
  was added.
- Canonical `Warnings[]` are rendered near Meaning as a concise notice, not as
  a numbered report section.
- The existing Supported `Limitations` section is rendered in canonical
  position 7, after Why and before Repository Evidence. Items come only from
  `report.Limitations`; the Supported+empty state has bounded EN/TR copy.
- Existing six visible sections keep their relative order and Starting Point
  remains text-only. No P10-I6-A shell/card/responsive/trace redesign began.

## Design Handoff and Contract Review

- `artifacts/design-handoff` inventory, Profile handoff/direction/copy/component
  sources and representative screenshots were inspected read-only.
- Safe intent reused: restrained status emphasis, semantic status colors,
  quiet typography and low interaction density.
- Conflicting prototype semantics were deliberately excluded: percentages,
  years/activity claims, scores/confidence, role/seniority/quality inference,
  invented repository state, Starting Point actions, export/download and
  uncontracted actions.
- No canonical contradiction and no typed contract gap was found.
- The task named historical filenames that no longer exist; the current
  canonical replacements (`p10-presentation-language-contract.md`,
  `public-profile-representative-repositories.md`) and the exact Session 105
  reconciliation worklog were located and used without modifying canonical
  docs.
- `artifacts/` was not modified, moved, generated into, staged or otherwise
  brought into git scope.

## Production / Semantic Boundary

- Production changes are Web/Profile presentation only. No Domain,
  Application, Infrastructure, configuration or Repository Proof production
  file changed.
- Operational/provider failures remain outside evidence, warnings,
  limitations, scoring and profile claims.
- Existing Repository Evidence action and Starting Point behavior are
  unchanged.
- P10 is not complete; P10-I6-A remains next only after review/integration.

## Validation

- Targeted Profile + localization tests: **197/197 PASS**.
- Full Web suite, including Repository Proof regressions: **313/313 PASS**.
- Full solution: **1701/1701 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 313 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS**.
- Exact changed-file and production-boundary review: **PASS**.

## Review State

Architecture-owner review required. No commit, push, merge, rebase, tag or
deploy was performed.

## Remaining Production Inputs / Findings

These are not P10-I5 typed/presentation gaps and were not selected here:

1. Deployment-owned trusted proxy/network/hop topology and measured visitor
   permit/window are still required before visitor limiting can receive
   production PASS.
2. Profile GitHub budget floor and exact execution deadline remain
   unselected/disabled; any deadline must remain greater than the existing
   transport timeout.
3. Production authentication still requires an endpoint-compatible credential
   choice and secure secret, expiry and rotation lifecycle.
4. Shared Repository/Profile provider circuiting remains unauthorized;
   Repository Proof stays unchanged.

## Suggested Commit

```text
feat(profile): present partial and operational states
```

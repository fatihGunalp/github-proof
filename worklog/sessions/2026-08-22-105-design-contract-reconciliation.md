# Session 105 — Design → Contract Reconciliation

**Date:** 2026-08-22  
**Result:** PASS WITH FINDINGS  
**Branch:** `session/105-design-contract-reconciliation`  
**Base HEAD:** `6a3eb45a8961879fd6d69696bb0984d0be3085db`  
**Model Route:** Sol / medium

## Purpose

Determine how the final Claude Design handoff can be implemented in the
existing GitHub Proof Razor/CSS architecture without changing product truth.
This was a read-only reconciliation/planning session; the reviewed handoff is
under `artifacts/design-handoff/extracted/`.

## Context and Surfaces Inspected

- Root/agent workflow contracts and current Session 103/104 worklog state.
- Canonical P10 presentation/profile contracts, Profile runtime collection,
  Repository Report information architecture and scoring/report contracts.
- Profile production: `Profile.cshtml(.cs)`, all `_Profile*.cshtml` partials,
  `ProfileWebPresentationLocalizer`, and `profile-result.css`.
- Repository Proof production: `_RepositoryReport*.cshtml` and `report.css`.
- Web foundation and entry: layout, semantic tokens, site/components CSS,
  unified input, `GitHubEntryClassifier`, localization and icon system.
- Relevant Profile, Repository shell, CSS architecture and entry-classifier
  tests.
- Complete handoff tree inventory; key Profile/Repository/Entry JSX, HTML,
  token/guideline/component specimens and representative screenshots.

## Authority Hierarchy

1. Explicit Session 105 constraints.
2. Canonical GitHub Proof product and architecture contracts.
3. Existing typed Domain/Application/report truth.
4. Existing production Razor/CSS/localization/tests.
5. Claude Design handoff as visual reference only.
6. Prototype behavior and placeholder content have no authority.

## Reconciliation Model

Every design element was classified as exactly one of:

- **KEEP** — already consistent with canonical truth and production behavior.
- **ADAPT** — visual intent is usable after preserving existing semantics.
- **PROHIBITED** — conflicts with canonical truth or introduces unowned
  behavior and must not be implemented.
- **CONTRACT DECISION REQUIRED** — cannot enter production without an explicit
  product/presentation contract decision.

## Major Prohibited or Contract-Conflicting Drift

- Unsupported Profile metrics: repository counts, activity years,
  percentages, signal clarity and confidence-like values.
- Role/backend/capability/seniority/developer-quality inference.
- Invented Active/Inactive repository states and confirmation marks.
- Starting Point link/action/external affordance and starting-point badge in
  Repository Evidence.
- Always-open Why/Repository Evidence drift; current non-empty sections remain
  collapsed native `details`/`summary` unless the contract changes.
- `GapExportRow`: browser-generated Markdown, hard-coded analysis status and
  `73/100`, plus new download behavior.
- Landing boundary icons/cards/two-column presentation instead of the locked
  quiet text-only disclosure.
- Prototype `owner/repo` classifier acceptance; production rejects schemeless
  repository paths.
- Primitive/raw violet consumer overrides; production continues to use
  semantic tokens and the existing blue accent.
- Uncontracted footer, Profile download, next-step and see-all actions.

## Findings

- BLOCKER: 0
- HIGH: 10
- MEDIUM: 3
- LOW: 1

Verdict: **PASS WITH FINDINGS**. The handoff is implementable only through the
KEEP/ADAPT path while excluding prohibited elements and deferring explicit
contract decisions.

## Architecture-Owner Review Corrections

Repository Proof visual alignment is **not** part of P10-I6 completion. The
correct sequence is:

```text
Production Guardrails
→ P10-I5 — Limitations + Partial / Failure UX
→ P10-I6-A — Profile shell + first-read hierarchy
→ P10-I6-B — Profile trace + repository inspection
→ P10-I6-C — Profile responsive/theme/localization closure
→ P10 COMPLETE
→ separate Repository Proof Visual Alignment session
→ final Milestone B production gate
```

Repository Proof visual alignment may happen before Milestone B deployment,
but it is not a P10 completion requirement.

“Profile identity as the single `h1`” is not locked canonical semantics. It
may be evaluated during P10-I6 as a presentation/accessibility refinement;
implementation must not treat it as a mandatory contract requirement.

## Semantic Boundary and Next Action

**NO production semantics changed.**  
**NO Domain/Application change required.**

P10-I5 and P10-I6 have not begun. The next official action is:

**Production Guardrails** — select and implement the execution-guard and
concurrent-profile-guard numeric values under their existing contract.

No production code, tests, canonical docs, artifacts, staging, commit or push
was changed by this session.

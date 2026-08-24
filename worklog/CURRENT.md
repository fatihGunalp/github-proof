# GitHub Proof — Current Development State

**Last Updated:** 2026-08-24 (Session 117)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 117 — `--fg-muted` Accessibility Audit & Correction —
  **PASS**. Branch `session/117-fg-muted-accessibility-audit-correction`,
  base `74f8444`, route Opus / high.
  Detail: [`sessions/2026-08-24-117-fg-muted-accessibility-audit-correction.md`](./sessions/2026-08-24-117-fg-muted-accessibility-audit-correction.md)
- **Last integrated commit on `main`:** `74f8444` (Session 116).
- `?? artifacts/` is user-owned, read-only and out of scope.

## Position

- Stage: **P10-I6 — Experience Refinement + Closure**
  (`docs/product/p10-web-profile-flow-implementation-plan.md`).
  I6-A closed in Session 111, I6-B in Session 114, **I6-C validated in
  Session 115**. The A/B/C split is a worklog-side working split from the
  architecture owner's Session 105 note — `docs/` enumerates no P10-I6
  sub-slices, so only I6-A has a canonical clause (§8.7).
- **P10 is NOT marked complete.** Completion is a separate architecture-owner
  exit evaluation, still pending. Milestone B remains open.
- **Text-contrast accessibility is closed for both shared foreground roles.**
  Session 116 remapped light `--fg-tertiary` to `--primitive-neutral-550`;
  Session 117 remapped `--fg-muted` in both themes to new `neutral-510` /
  `neutral-490` after classifying its four consumers individually (two
  non-exempt text, two exempt). Both corrections live only in `tokens.css`; no
  consumer stylesheet was patched. `CssArchitectureTests` now owns five
  contrast/token guards, each proven to fail on its pre-change value.
- **The canonical token contract now matches production.**
  `docs/product/github-proof-ui-system-light-dark-v1.md` had prescribed the
  pre-correction mappings while its own §29 required WCAG 2.2 AA — an internal
  contradiction. Session 117 amended §5.1, the Light/Dark foreground mappings
  and the §31 examples to the validated implementation, using the file's
  existing `> **Amended:**` convention. Doc and `tokens.css` are mechanically
  verified to agree; no stale mapping remains in a normative token example.
- Production guardrails — execution deadline, admission/concurrency, provider
  budget protection, Partial/failure UX — are implemented and integrated.
  Their exact semantics are owned by `docs/`; they are not restated here.

## Locked Decisions

- **Profile trace is deliberately two-stage** (architecture owner, Session 114).
  `WhyThisConclusion` names supporting repository identities;
  `RepositoryDrillDown` owns the single Repository Proof POST per repository.
  A direct action inside a Why entry is **not** introduced. Owned by
  `p10-presentation-language-contract.md` §8.6.3–§8.6.5.
- Architecture owner, Session 114: the visual continuity work is bounded to
  presentation; canonical body content is unchanged.

## Measured Observations (not canonical rules)

- Session 115 measured that theme currently changes tokens only, not layout:
  `profile-result.css` contains no `[data-theme]` selector and light/dark
  layout was byte-identical at 1280 and 390. Light communicates card structure
  through `--border-subtle` (1.156 vs canvas), dark through the surface delta
  (1.110) with its border invisible by construction; both were judged adequate.
  This is an observation of current behaviour, not a constraint `docs/`
  establishes.

## Open Decisions

These block a production PASS, not the next development slice.

| Open item | State |
|---|---|
| `--fg-muted` AA margin headroom | **accepted, documented.** Margins over 4.5 are only +0.047 (light on `--bg-canvas`) and +0.036 (dark on `--surface-default`), a deliberate consequence of the "smallest visual movement, no invented threshold" rule. Several surfaces sit below AA with this value — lowest is dark `--surface-raised` / `--surface-hover` at **4.190** — but **no non-exempt `--fg-muted` semantic consumer renders on any of them**, so none is a current defect. Any future semantic `--fg-muted` use on a new background requires its own contrast validation before it ships |
| Profile pixel review | no screenshot or human aesthetic review has been performed; Sessions 114/115 evidence is measured layout geometry and computed contrast from the real engine |
| Real analyzed Profile render | never captured — no `GITHUB_TOKEN`, unauthenticated quota is 60/hour and was exhausted. Both visual gates used browser-injected DOM carrying real canonical copy |
| Visitor rate limit `2/10 min` | unselected candidate; permit/window unset, so IP limiting is disabled |
| Trusted proxy / forwarded-header topology | deployment-owner input required; no forwarded-header parser exists |
| `ProfileExecutionDeadline:Timeout` | deliberately unset, no fallback; must exceed the 2-minute transport timeout if configured |
| `GitHubApiBudget:ProfileAdmissionRemainingFloor` | unset/disabled; `500` is a measurement candidate, not a decision |
| Profile concurrency `1/1`, queue `0` | implemented as provisional, externally configurable defaults |
| Beta credential | machine-user fine-grained PAT vs GitHub App undecided; neither rejected |
| Shared circuit behavior beyond Profile | unauthorized without a separate decision |
| P10-I6-A closure record | `INDEX.md` row 111 reads `PASS` per architecture-owner adjudication, while `sessions/2026-08-23-111-...md` still reads "acceptance remains required" |

## Deferred Canonical Gaps

RECENCY · SHAREABLE PROFILE URL · SOCIAL PREVIEW · Project-Type Presentation ·
Starting Point Prioritization · Empty Limitations. All six are still open and
none has been touched.

Evidence and reasoning: see [`INDEX.md`](./INDEX.md).

## Exact Next Action

The pending P10 exit evaluation. The Milestone B accessibility blocker raised in
Session 116 is closed by Session 117.

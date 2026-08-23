# GitHub Proof — Current Development State

**Last Updated:** 2026-08-24 (Session 115)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 115 — P10-I6-C Responsive + Theme + EN/TR Closure — **PASS**
  (validation-only; no production change). Branch
  `session/115-p10-i6-c-responsive-theme-localization-closure`, base `8ecf782`,
  route Opus / high.
  Detail: [`sessions/2026-08-24-115-p10-i6-c-responsive-theme-localization-closure.md`](./sessions/2026-08-24-115-p10-i6-c-responsive-theme-localization-closure.md)
- **Last integrated commit on `main`:** `8ecf782` (Session 114).
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
- Session 115 changed no production, test or docs file. A 6-state rendered
  matrix (1280 / 880 / 390 × dark / light × TR / EN) over all nine Profile
  surfaces found no defect: no page overflow, no clipped semantic text, no card
  containment break, correct breakpoint behaviour, I6-A/I6-B intact.
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
| Light-theme `--fg-tertiary` contrast | **escalated, unfixed.** 13px tertiary text measures 4.467 on raised cards and 4.357 on canvas (WCAG AA normal text = 4.5); dark passes at 7.30/8.10. The token is shared: 19 uses in `report.css`, 8 in `profile-result.css`, 7 elsewhere. A Profile-only patch would fork the design system, so the repair belongs in `tokens.css` — a shared file. Architecture-owner decision required |
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
Starting Point Prioritization · Empty Limitations. All six remain open;
Session 115 touched none of them.

Evidence and reasoning: sessions 106-110, 114, 115 (see [`INDEX.md`](./INDEX.md)).

## Exact Next Action

Architecture-owner decision on the escalated light-theme `--fg-tertiary`
contrast finding, then the separate P10 exit evaluation.

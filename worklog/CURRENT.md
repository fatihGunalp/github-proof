# GitHub Proof — Current Development State

**Last Updated:** 2026-08-23 (Session 114)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 114 — P10-I6-B Profile Trace + Repository Inspection —
  **PASS / COMPLETE**. Independent review findings closed in-session; bounded
  visual gate PASS at 1280 / 880 / 390 px.
  Branch `session/114-p10-i6-b-profile-trace-repository-inspection`,
  base `efe0a76`, route Opus / high.
  Detail: [`sessions/2026-08-23-114-p10-i6-b-profile-trace-repository-inspection.md`](./sessions/2026-08-23-114-p10-i6-b-profile-trace-repository-inspection.md)
- **Last integrated commit on `main`:** `efe0a76` (Session 113).
- `?? artifacts/` is user-owned, read-only and out of scope.

## Position

- Stage: **P10-I6 — Experience Refinement + Closure**
  (`docs/product/p10-web-profile-flow-implementation-plan.md`).
  P10-I6-A closed in Session 111; **P10-I6-B implemented in Session 114**.
  P10 and Milestone B remain open.
  The A/B/C sub-slice split is a worklog-side working split from the
  architecture owner's Session 105 note — `docs/` enumerates no P10-I6
  sub-slices, so the remaining scope after I6-B is not canonically named here.
- Session 114 changed only `wwwroot/css/profile-result.css` and its owning
  test file. No Razor, localizer, Domain, Application or Infrastructure file
  was touched, so every §8.6 evidence-boundary invariant holds structurally.
- Production guardrails — execution deadline, admission/concurrency, provider
  budget protection, Partial/failure UX — are implemented and integrated.
  Their exact semantics are owned by `docs/`; they are not restated here.

## Locked Decisions

- **Profile trace is deliberately two-stage** (architecture owner, Session 114).
  `WhyThisConclusion` names supporting repository identities;
  `RepositoryDrillDown` owns the single Repository Proof POST per repository.
  A direct action inside a Why entry is **not** introduced. P10-I6-B improves
  visual continuity between the two stages only. Owned by
  `p10-presentation-language-contract.md` §8.6.3–§8.6.5.

## Open Decisions

These block a production PASS, not the next development slice.

| Open item | State |
|---|---|
| Session 114 pixel review | the visual gate verified every stated criterion by measured layout geometry against the real stylesheet, but no screenshot or human aesthetic review was possible (Browser pane not displayed) and no real analyzed Profile was rendered (no `GITHUB_TOKEN`; unauthenticated quota exhausted) |
| Remaining P10-I6 scope | not canonically enumerated in `docs/`; only I6-A has a canonical clause (§8.7) |
| Visitor rate limit `2/10 min` | unselected candidate; permit/window unset, so IP limiting is disabled |
| Trusted proxy / forwarded-header topology | deployment-owner input required; no forwarded-header parser exists |
| `ProfileExecutionDeadline:Timeout` | deliberately unset, no fallback; must exceed the 2-minute transport timeout if configured |
| `GitHubApiBudget:ProfileAdmissionRemainingFloor` | unset/disabled; `500` is a measurement candidate, not a decision |
| Profile concurrency `1/1`, queue `0` | implemented as provisional, externally configurable defaults |
| Beta credential | machine-user fine-grained PAT vs GitHub App undecided; neither rejected |
| Shared circuit behavior beyond Profile | unauthorized without a separate decision |
| P10-I6-A closure record | `INDEX.md` row 111 reads `PASS` per architecture-owner adjudication, while `sessions/2026-08-23-111-...md` still reads "acceptance remains required" |

Evidence and reasoning: sessions 106-110 and 114 (see [`INDEX.md`](./INDEX.md)).

## Exact Next Action

Open a new model gate for the next P10-I6 slice (responsive / theme /
localization closure), which owns light theme and the EN/TR matrix.

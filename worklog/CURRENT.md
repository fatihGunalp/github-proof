# GitHub Proof — Current Development State

**Last Updated:** 2026-08-23 (Session 113)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 113 — Worklog Navigation & Compaction — PASS.
  Branch `session/113-worklog-navigation-compaction`, base `8ec3023`,
  route Opus / high.
  Detail: [`sessions/2026-08-23-113-worklog-navigation-compaction.md`](./sessions/2026-08-23-113-worklog-navigation-compaction.md)
- **Last integrated commit on `main`:** `8ec3023` (Session 112, remote-verified).
- `?? artifacts/` is user-owned, read-only and out of scope.

## Position

- Stage: **P10-I6 — Experience Refinement + Closure**
  (`docs/product/p10-web-profile-flow-implementation-plan.md`).
  P10-I6-A closed in Session 111. P10 and Milestone B remain open.
  Remaining P10-I6 sub-slices are not canonically enumerated in `docs/`.
- Production guardrails — execution deadline, admission/concurrency, provider
  budget protection, Partial/failure UX — are implemented and integrated.
  Their exact semantics are owned by `docs/`; they are not restated here.
- No Domain/Application production behavior is pending an agent fix.

## Open Decisions

These block a production PASS, not the next development slice.

| Open item | State |
|---|---|
| Visitor rate limit `2/10 min` | unselected candidate; permit/window unset, so IP limiting is disabled |
| Trusted proxy / forwarded-header topology | deployment-owner input required; no forwarded-header parser exists |
| `ProfileExecutionDeadline:Timeout` | deliberately unset, no fallback; must exceed the 2-minute transport timeout if configured |
| `GitHubApiBudget:ProfileAdmissionRemainingFloor` | unset/disabled; `500` is a measurement candidate, not a decision |
| Profile concurrency `1/1`, queue `0` | implemented as provisional, externally configurable defaults |
| Beta credential | machine-user fine-grained PAT vs GitHub App undecided; neither rejected |
| Shared circuit behavior beyond Profile | unauthorized without a separate decision |
| P10-I6-A closure record | `INDEX.md` row 111 reads `PASS` per architecture-owner adjudication, while `sessions/2026-08-23-111-...md` still reads "acceptance remains required"; the session header is out of Session 113's scope |

Evidence and reasoning: sessions 106-110 (see [`INDEX.md`](./INDEX.md)).

## Exact Next Action

Open a new model gate for the next P10-I6 slice.

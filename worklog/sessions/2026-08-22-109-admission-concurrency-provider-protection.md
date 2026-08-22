# Session 109 — Admission / Concurrency + Provider Protection Guardrails

**Date:** 2026-08-22
**Result:** PASS WITH FINDINGS — implementation complete; production
enablement inputs remain external
**Branch:** `session/109-admission-concurrency-provider-protection`
**Base HEAD:** `9dc70b4cb739e3bcc4dd63f6f84eecd329b31c9c`
**Model Route:** Sol / high

## Goal

Implement the remaining canonical Profile-only admission/concurrency and
proactive GitHub provider-protection boundaries without changing evidence,
Repository Proof behavior or the Session 108 deadline contract.

## Mutation Boundary

Scope is limited to Profile Web admission/runtime infrastructure, Profile-only
GitHub operational header/budget observation, configuration/options and their
tests. No Domain/Application evidence change, Repository Proof circuit change,
queue/result reuse/background/distributed execution, final P10-I5 presentation,
authentication lifecycle implementation or generic forwarded-header trust is
authorized. Exact visitor, budget and deadline numbers remain unselected.

## Pre-existing Working Tree

- `?? artifacts/` is user-owned, untracked, out of scope and untouched.
- Local `main`, `origin/main` and Session 109 base matched `9dc70b4`.

## Context Read

- `AGENTS.md`
- `agents/01-model-routing.md`
- `agents/02-context-loading.md`
- `agents/03-project-contract.md`
- `agents/04-development-workflow.md`
- `agents/06-review-and-validation.md`
- `agents/07-worklog-integration.md`
- `worklog/CURRENT.md`
- `docs/architecture/public-profile-runtime-collection.md` §§10–11
- `docs/product/p10-web-profile-flow-implementation-plan.md` P10-I5 / §13.12

## Progress

- Git baseline verified and dedicated Session 109 branch created.
- Valid Profile POST admission now runs in the canonical order: visitor,
  same-profile, global. The process-local singleton guard is fail-fast and
  queue-free. Same-profile/global decisions share one lock, use the canonical
  parsed login with ordinal-ignore-case identity, and return disposable leases
  released on every Web terminal path. Rejections start no live collection and
  produce no `LiveProfileAnalysisResult`.
- Provisional implementation defaults are global concurrency `1`, same-profile
  concurrency `1`, queue `0`. Both concurrency values are external options.
  Visitor permit/window have no default and the visitor branch remains disabled
  until both values are supplied. The Web boundary reads only ASP.NET's
  `RemoteIpAddress`; it does not parse raw forwarded headers and no generic
  forwarded-header trust was added.
- HTTP mappings are exact: visitor `429` with bounded `Retry-After`, normalized
  same-profile duplicate `409`, global busy `503`. Final guardrail-specific
  EN/TR copy remains P10-I5; the existing bounded generic failure copy is reused
  temporarily.
- The Profile-specific GitHub handler now observes only sanitized
  `X-RateLimit-Remaining` / `X-RateLimit-Reset` integers from the profile
  metadata response. Authorization, credentials, response bodies and identities
  are not copied or logged. Budget control state is a separate execution-local
  operational carrier, not telemetry or evidence.
- A configured Profile remaining floor is consulted only after truthful
  Available metadata publication and before repository-manifest collection.
  Rejection throws one typed operational signal to the outer Web runtime; it
  preserves Profile availability and produces exactly
  `Unavailable / github-budget-protection-rejected / RetrievedAt=null` with no
  pipeline result. Manifest/per-repository work does not start.
- Budget terminal-result creation is linearized with the existing owned
  execution deadline via `TryComplete()`. Caller cancellation remains first;
  if the deadline wins the terminal race, the budget candidate is discarded
  and the canonical deadline result is returned.
- Repository Proof does not receive the handler, state or guard. No
  Domain/Application evaluator, policy, composer, projector, evidence,
  availability, warning, limitation or report type changed.

## Configuration State

- `ProfileGuardrails:Admission:GlobalConcurrency`: provisional default `1`.
- `ProfileGuardrails:Admission:SameProfileConcurrency`: provisional default
  `1`.
- `ProfileGuardrails:Admission:VisitorPermitLimit` and `VisitorWindow`: both
  unset/disabled by default; validation requires both or neither.
- `GitHubApiBudget:ProfileAdmissionRemainingFloor`: unset/disabled by default.
- No visitor `2 / 10 minutes`, reserve `500`, deadline or trusted-proxy value
  was added to appsettings or canonicalized.

## Validation

- Targeted Infrastructure orchestrator tests: **13/13 PASS**.
- Targeted Web/Profile admission, provider, runtime, deadline, composition and
  model tests: **81/81 PASS**.
- Full solution: **1685/1685 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 297 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: PASS.
- `git diff --check`: PASS.
- Exact production-file review: Profile Web/runtime plus one narrow Profile
  Infrastructure continuation seam only. No Domain/Application or Repository
  Proof production path changed.
- User-owned `?? artifacts/` remains untouched.

## Remaining Production Inputs / Findings

1. Visitor IP limiting cannot receive production PASS until the deployment
   owner supplies the exact trusted proxy/network/hop topology and an approved
   visitor permit/window. No forwarded headers are trusted today.
2. The Profile GitHub remaining floor is deliberately unselected. A measured
   external value is required to activate proactive rejection; `500` remains
   only a candidate.
3. The exact execution deadline remains unselected/disabled and must stay
   greater than the two-minute transport timeout when configured.
4. Production beta authentication still requires endpoint-compatible
   machine-user fine-grained PAT or a separately completed GitHub App decision,
   plus secure secret, expiry and rotation handling. Session 109 did not change
   credential lifecycle behavior.
5. Shared Repository/Profile provider circuiting remains unauthorized.
   Repository Proof is unchanged.
6. P10-I5 still owns final localized guardrail copy/layout. P10-I6 and the final
   production gate remain downstream.

Architecture-owner re-review is required before integration. No commit, push,
merge, rebase, tag or deploy was performed.

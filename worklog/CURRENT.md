# GitHub Proof — Current Development State

**Last Updated:** 2026-08-22 (Session 106)
**Source of Truth:** `docs/`

## Current Session State

- Active Session: **NONE**.
- Session 106 — Production Guardrails Contract / Implementation Planning:
  **PASS WITH FINDINGS**.
- Branch: `session/106-production-guardrails-planning`
- Base HEAD: `f238e443878acc2b179c2e2c5013ac8b7fba7e6a`
- Pre-existing `?? artifacts/` is user-owned, out of scope and untouched.
- Last remote-verified main: `f238e44` (2026-08-22).

## Current Product State

- Repository Proof V1 and Profile Proof deterministic evidence semantics are
  unchanged. No production code, tests, configuration or canonical contract
  was changed in Session 106.
- Profile runtime readiness remains complete; P10-I3 and P10-I4 are complete.
- Production guardrail implementation has not begun. Provisional implementation
  recommendations are global profile concurrency `1`, same-profile concurrency
  `1`, queue `0`, and duplicate same-profile response `409`.
- Visitor `2/10 min` and GitHub reserve `500` remain measurement candidates, not
  decisions. The exact end-to-end deadline is unselected; `120 s` is rejected in
  its current form because it equals the Profile HttpClient timeout and cannot
  provide deterministic cancellation attribution.
- The architecture owner conceptually approved
  `ProfileAvailability + ExecutionAvailability + ProfilePipelineResult?` with
  no new Domain evidence status. The canonical amendment must still lock exact
  execution statuses/reason identities, deadline start and HTTP mapping. Both
  internal deadline expiration and proactive budget rejection terminate at the
  outer runtime/Web boundary with no pipeline result.
- Production IP limiting remains stopped until the deployment owner supplies
  the actual trusted proxy/network topology. Forwarded headers are not trusted
  generically.
- A machine-user fine-grained PAT remains a possible interim beta credential,
  subject to exact endpoint compatibility and secure secret/expiry handling.
  GitHub App remains a valid longer-term production candidate and is not
  rejected by Session 106.
- Repository Proof provider protection requires a separate architecture choice:
  Profile-only for the initial slice, or an explicitly authorized shared
  operational circuit that preserves Repository Proof evidence semantics.
- P10-I5 and P10-I6-A/B/C remain ahead. P10 and Milestone B are not complete.

## Exact Next Action

Architecture-owner re-review of the corrected Session 106. If approved, lock
the remaining typed-boundary, timeout, HTTP/authentication, proxy and provider-
circuit decisions in the two named canonical amendments before any production
guard code. Do not begin P10-I5 or guard implementation from this planning
branch without that decision.

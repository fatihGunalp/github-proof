# GitHub Proof — Current Development State

**Last Updated:** 2026-08-22 (Session 108)
**Source of Truth:** `docs/`

## Current Session State

- Active Session: **NONE**.
- Session 108 — Profile Execution Boundary + Deadline Implementation:
  **PASS**; architecture-owner HIGH race finding corrected, re-review pending.
- Branch: `session/108-profile-execution-deadline`
- Base HEAD: `c89065235c24d86f49ba2449e9b7eb7c0b2cb8f8`
- Pre-existing `?? artifacts/` is user-owned, out of scope and untouched.
- Last remote-verified main: `c890652` (2026-08-22).

## Current Product State

- Repository Proof V1 and Profile Proof deterministic evidence semantics are
  unchanged. No Domain/Application production file changed.
- The canonical live boundary is implemented as `ProfileAvailability? +
  ExecutionAvailability + ProfilePipelineResult?`. Exact DataKey/status/reason/
  RetrievedAt/nullability combinations are enforced at construction.
- A dedicated internal deadline begins inside `LiveProfileReportService` before
  the first live metadata operation and remains active through terminal outer
  result creation. Caller cancellation wins overlaps and propagates; owned
  deadline expiration returns no pipeline/Partial; transport/unrelated
  cancellation remains a propagated operational fault.
- Resolved metadata availability is preserved by a minimal Infrastructure
- Resolved metadata availability is preserved by a minimal Infrastructure
  progress carrier published before manifest collection. Its execution-token
  binding atomically arbitrates `unresolved → resolved(value)` against
  `unresolved → cancellation-won`; late typed metadata cannot overwrite the
  winning deadline, while earlier availability survives later cancellation.
  It is operational only and never evidence, warning, limitation, provenance
  or report truth.
- `ResponseHeadersRead` metadata and manifest content reads are proven to
  observe the execution token. Existing downstream Profile repository request/
  content APIs already receive the same token. Repository Proof is unchanged.
- Execution success uses `DataKey=profile-execution`, `Status=Available`, no
  reason, and UTC terminal-result `RetrievedAt`; non-success has no timestamp.
- Existing transport timeout remains 2 minutes. External
  `ProfileExecutionDeadline:Timeout` is deliberately unset/disabled with no
  fallback; if configured, startup validation requires it to exceed transport.
  Exact production deadline remains unselected and readiness is not claimed.
- Operational HTTP mapping is locked: visitor `429`, duplicate `409`, global
  busy `503`, execution deadline `504`, proactive budget rejection `503`.
- Initial provider-budget/header protection is Profile-only; Repository Proof
  is unchanged. Shared circuit behavior requires a separate future decision.
- Production IP limiting remains stopped until the deployment owner supplies
  the actual trusted proxy/network topology. Forwarded headers are not trusted
  generically.
- A machine-user fine-grained PAT remains a possible interim beta credential,
  subject to exact endpoint compatibility and secure secret/expiry handling.
  GitHub App remains a valid longer-term production candidate and is not
  rejected.
- Visitor `2/10 min`, reserve `500` and exact deadline remain measurement
  candidates. Global/same-profile concurrency `1/1` and queue `0` remain
  provisional implementation recommendations.
- P10-I5 and P10-I6-A/B/C remain ahead. P10 and Milestone B are not complete.

## Exact Next Action

Architecture-owner re-review of the corrected Session 108. If approved, Session 109
may implement the remaining admission/provider guardrails without changing the
typed deadline boundary. Retain the exact deadline measurement/configuration,
trusted-proxy topology, endpoint-authentication and Repository Proof circuit
scope gates. P10-I5 final guardrail presentation remains separate.

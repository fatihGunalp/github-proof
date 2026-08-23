# GitHub Proof — Current Development State

**Last Updated:** 2026-08-23 (Session 112)
**Source of Truth:** `docs/`

## Current Session State

- Active Session: **NONE**.
- Last Session: **112 — Agent Bootstrap & Context Alignment — PASS**
  (architecture-owner approved). Branch
  `session/112-agent-bootstrap-context-alignment`, Base `6158789`.
- Architecture outcome: `AGENTS.md` = shared Codex/Claude repository
  contract (progressive disclosure — only `agents/02` read next, further
  playbooks loaded per task route); `CLAUDE.md` = thin Claude adapter
  (`@AGENTS.md` import). Combined Claude startup context 4,792 chars, down
  from a 5,275-char pre-alignment baseline. `docs/`, `src/`, `tests/`,
  `artifacts/` untouched. Full detail:
  `worklog/sessions/2026-08-23-112-agent-bootstrap-context-alignment.md`.
- Only remaining action is user-owned: commit/push Session 112, remote
  verification, fast-forward integration to `main`.
- Session 111 (P10-I6-A Profile shell) is **CLOSED** — commit `6158789`,
  fast-forwarded to `main`, remote `main` independently verified identical.
- Pre-existing `?? artifacts/` remains user-owned, read-only and out of scope.

## Current Product State

- Repository Proof V1 and Profile Proof deterministic evidence semantics are
  unchanged. No Domain/Application production file changed.
- The canonical live boundary is implemented as `ProfileAvailability? +
  ExecutionAvailability + ProfilePipelineResult?`. Exact DataKey/status/reason/
  RetrievedAt/nullability combinations are enforced at construction.
- Valid Profile POST admission is fail-fast and queue-free in exact order:
  visitor `429`, same normalized profile `409`, global capacity `503`. The
  process-local concurrency defaults are the provisional `1/1`; leases cover
  the live execution and are released on every terminal path. Admission
  rejection creates no live analysis result.
- Visitor permit/window remain unset, so IP limiting is disabled. It reads only
  `RemoteIpAddress`; no forwarded-header parser/trust was added. Production PASS
  still requires deployment-owned trusted proxy/network/hop topology and
  measured visitor values.
- Profile-only sanitized metadata response-budget state can stop execution
  before repository-manifest collection. The exact result is Available Profile
  metadata plus execution `Unavailable / github-budget-protection-rejected`, no
  RetrievedAt and no pipeline/Partial; Web maps it to `503`. Budget state is
  separate from telemetry/evidence and Repository Proof is unchanged.
- `GitHubApiBudget:ProfileAdmissionRemainingFloor` is unset/disabled. No reserve
  value was selected; `500` remains a measurement candidate.
- A dedicated internal deadline begins inside `LiveProfileReportService` before
  the first live metadata operation and remains active through terminal outer
  result creation. Caller cancellation wins overlaps and propagates; owned
  deadline expiration returns no pipeline/Partial; transport/unrelated
  cancellation remains a propagated operational fault.
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
- Initial provider-budget/header protection is Profile-only; shared circuit
  behavior remains unauthorized without a separate future decision.
- Production IP limiting remains stopped until the deployment owner supplies
  the actual trusted proxy/network topology. Forwarded headers are not trusted
  generically.
- A machine-user fine-grained PAT remains a possible interim beta credential,
  subject to exact endpoint compatibility and secure secret/expiry handling.
  GitHub App remains a valid longer-term production candidate and is not
  rejected.
- Visitor `2/10 min`, reserve `500` and exact deadline remain unselected
  measurement candidates. Global/same-profile concurrency `1/1` and queue `0`
  are implemented as provisional, externally configurable defaults.
- Real canonical Partial now keeps the complete typed report visible with an
  early localized observation notice. Canonical Warnings are a status notice;
  Limitations render in position 7. Admission/provider/deadline/pipeline
  failures remain reportless localized operational states.
- P10-I5 is integrated; Session 111 P10-I6-A is **CLOSED** (commit `6158789`,
  on `main`, remote-verified). P10 and Milestone B are not complete.

## Exact Next Action

User-owned bounded commit/push of Session 112, followed by remote
verification and fast-forward integration to `main`. No further agent
mutation is pending; Session 113 has not started.

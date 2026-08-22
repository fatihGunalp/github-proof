# GitHub Proof — Current Development State

**Last Updated:** 2026-08-22 (Session 107)
**Source of Truth:** `docs/`

## Current Session State

- Active Session: **NONE**.
- Session 107 — Production Guardrails Contract Decision:
  **PASS WITH FINDINGS**.
- Architecture-owner HIGH is closed; the remaining MEDIUM was corrected and
  final architecture-owner review is pending.
- Branch: `session/107-production-guardrails-contract-decision`
- Base HEAD: `780685400379705765381620d5fb4edc101f87ac`
- Pre-existing `?? artifacts/` is user-owned, out of scope and untouched.
- Last remote-verified main: `7806854` (2026-08-22).

## Current Product State

- Repository Proof V1 and Profile Proof deterministic evidence semantics are
  unchanged. Session 107 changed only the canonical future operational runtime
  and Web contracts; no production code, tests or configuration changed.
- Profile runtime readiness remains complete; P10-I3 and P10-I4 are complete.
- Production guardrail implementation has not begun. The canonical live boundary
  is now `ProfileAvailability? + ExecutionAvailability +
  ProfilePipelineResult?`, with exhaustive operational status/reason identities.
  Deadline and proactive budget rejection produce no pipeline/Partial/evidence.
- The deadline starts after Web admission but before the first live GitHub
  metadata operation, and remains active through the live terminal result. If it
  expires before availability resolves, `ProfileAvailability=null` is outer
  runtime truth only. Canonical relation is
  `0 < transport timeout < execution deadline`; exact values remain unselected.
- Current collectors use `ResponseHeadersRead`; `HttpClient.Timeout` alone does
  not bound response-content reads. Every live content read must observe the
  internal deadline or another owned/distinguishable bounded token.
- `ExecutionAvailability.DataKey` is exactly `profile-execution`. Only successful
  execution has `Reason=null` and a `RetrievedAt` equal to the UTC terminal
  non-null pipeline-result instant; every non-success has `RetrievedAt=null`.
  The timestamp is operational metadata only and never evidence/provenance.
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

ChatGPT architecture-owner independent review of the Session 107 exact diff. If
approved, authorize a separate production-guardrail implementation session;
retain measurement/configuration, endpoint-authentication and trusted-proxy
gates. Do not implement guard code or begin P10-I5 from this contract branch.

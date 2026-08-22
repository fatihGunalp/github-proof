# Session 108 — Profile Execution Boundary + Deadline Implementation

**Date:** 2026-08-22
**Result:** PASS
**Branch:** `session/108-profile-execution-deadline`
**Base HEAD:** `c89065235c24d86f49ba2449e9b7eb7c0b2cb8f8`
**Model Route:** Sol / high

## Goal

Implement only the canonical typed live Profile runtime boundary, the internal
end-to-end execution deadline and deterministic caller/deadline/transport
cancellation attribution. Preserve resolved Profile availability and bound live
Profile response-content reads without changing Repository Proof behavior.

## Mutation Boundary

Production scope is limited to the smallest required Web/Profile and narrowly
required Profile Infrastructure runtime seams plus their tests. Visitor,
forwarded-header, same-profile/global concurrency, budget/provider circuit,
authentication lifecycle, UI/P10-I5 presentation, cache/queue/distributed work,
Domain/Application evidence semantics and user-owned `artifacts/` are out of
scope.

## Pre-existing Working Tree

- `?? artifacts/` is user-owned, untracked, out of scope and untouched.
- Local `main`, `origin/main` and task base matched `c890652`.

## Context Read

- `AGENTS.md`
- `agents/01-model-routing.md`
- `agents/02-context-loading.md`
- `agents/03-project-contract.md`
- `agents/04-development-workflow.md`
- `agents/06-review-and-validation.md`
- `agents/07-worklog-integration.md`
- `worklog/CURRENT.md`

## Progress

- Live Git baseline verified and dedicated Session 108 branch created.
- `LiveProfileAnalysisResult` now enforces the canonical outer shape:
  `ProfileAvailability? + ExecutionAvailability + ProfilePipelineResult?`.
  `ExecutionAvailability.DataKey` is exactly `profile-execution`; only the four
  Session 108 outcomes are constructible. A terminal failed pipeline remains an
  Available execution, while deadline outcomes never carry a pipeline result.
- `LiveProfileReportService` creates a dedicated internal deadline scope before
  the first collector call and passes its linked execution token through
  metadata, manifest, repository collection and `ProfilePipeline`. Caller
  cancellation is checked first and propagates; owned deadline cancellation is
  typed; unrelated/transport `OperationCanceledException` still propagates as
  an operational fault. Terminal result creation is linearized against the
  deadline timer.
- Infrastructure now exposes one minimal operational
  `GitHubProfileSnapshotProgress` carrier. The snapshot orchestrator publishes
  metadata availability immediately after it resolves and before manifest
  collection. The carrier never enters snapshots, warnings, evidence,
  evaluators, composers, projectors or reports.
- `ResponseHeadersRead` content consumption was audited. Metadata and manifest
  JSON reads already accept the supplied cancellation token; new blocking-stream
  tests prove the execution token reaches both content reads. The downstream
  Profile repository composition likewise passes the same token to its existing
  request and JSON-read APIs. Repository Proof composition/behavior was not
  modified.
- Profile deadline HTTP handling returns the already-canonical `504` mapping
  with existing generic failure copy only; final P10-I5 guardrail copy/layout
  remains out of scope.

## Timeout Configuration State

- Existing Profile GitHub transport timeout remains `2 minutes`; Session 108
  did not select or change it.
- New external key: `ProfileExecutionDeadline:Timeout`.
- The production/default value is deliberately `null` (unconfigured/disabled),
  with no fallback and no new value in appsettings/config files.
- If configured, startup options validation requires the execution deadline to
  be strictly greater than the existing transport timeout. Therefore the
  canonical `0 < transport < execution deadline` relationship is enforced
  without selecting a final execution value.
- Production guardrail readiness is not claimed while the value is unselected.

## Validation

- Targeted Web/Profile suite: **273/273 PASS**.
- Targeted Infrastructure suite: **295/295 PASS**.
- Full solution: **1659/1659 PASS**
  (`834 Domain + 257 Application + 295 Infrastructure + 273 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `git diff --check`: PASS.
- Exact production-file review: only Web/Profile, its composition registration,
  and the narrow Profile Infrastructure progress seam changed. No Domain or
  Application production file changed; no Repository Proof production file or
  behavior changed.
- User-owned `?? artifacts/` remains untouched.

## Architecture-owner HIGH Correction

Review found that the first progress carrier prevented duplicate availability
writes but did not linearize availability publication against internal execution
cancellation. A completed HTTP await could therefore be followed by deadline
expiration and then a late synchronous metadata publication, incorrectly making
availability visible after cancellation had already won.

The correction binds `GitHubProfileSnapshotProgress` to the exact execution
cancellation token and gives it one atomic state transition from `unresolved`
to either `resolved(DataAvailability)` or
`execution-cancellation-won-before-resolution`. The resolved value lives inside
the winning state object. Therefore:

- cancellation winning first permanently rejects and hides every late metadata
  publication;
- availability winning first remains visible through immediately subsequent
  cancellation;
- no before/after token-check sequence is used as the arbitration mechanism.

`LiveProfileReportService` now treats this carrier as authoritative after a
collector return. If cancellation won first, even a late Available or
non-Available typed metadata result produces the canonical deadline result with
`ProfileAvailability=null`. If Available publication won first, a later deadline
preserves it. If non-Available publication won first, that terminal provider
truth remains `NotRequested / profile-unavailable`; a later deadline cannot
overwrite it. Collector-result availability must match the atomically published
value whenever publication won.

Four deterministic Infrastructure cases and four deterministic Web cases were
added through the real `GitHubProfileSnapshotOrchestrator` boundary, covering
both orderings for Available and non-Available metadata. All previously passing
deadline, caller-overlap, transport, Partial suppression, content-read token,
configuration and Repository Proof behavior remains green. Architecture-owner
re-review is pending.

## Remaining Work

Session 108 completes only the typed runtime boundary and internal deadline
core. Session 109 still owns the remaining production guardrails: visitor/IP
limiting with deployment-owned trusted proxy topology, same-profile/global
admission, proactive GitHub budget/provider protection, and the separately
gated authentication/provider decisions. Exact deadline value also remains a
measurement/deployment decision. P10-I5 final guardrail presentation remains
separate.

# Session 107 — Production Guardrails Contract Decision

**Date:** 2026-08-22
**Result:** PASS WITH FINDINGS
**Branch:** `session/107-production-guardrails-contract-decision`
**Base HEAD:** `780685400379705765381620d5fb4edc101f87ac`
**Model Route:** Sol / high
**Review State:** Architecture-owner HIGH closed; MEDIUM corrected; final review required

## Goal

Convert the independently approved Session 106 production-guardrail planning
decisions into the minimum canonical architecture and P10 Web contract
amendments required before production guardrail implementation.

## Mutation Boundary

Contract-decision and worklog only. Allowed canonical scope is limited to
`docs/architecture/public-profile-runtime-collection.md` and
`docs/product/p10-web-profile-flow-implementation-plan.md`. Production code,
tests, project/configuration files, presentation assets, Domain/Application
semantics and user-owned `artifacts/` remain unchanged.

## Pre-existing Working Tree

- `?? artifacts/` is user-owned, untracked, out of scope and untouched.
- Local `main`, `origin/main` and the verified task base matched `7806854`.

## Context Read

- `AGENTS.md`
- `agents/01-model-routing.md`
- `agents/02-context-loading.md`
- `agents/03-project-contract.md`
- `agents/04-development-workflow.md`
- `agents/06-review-and-validation.md`
- `agents/07-worklog-integration.md`
- `worklog/CURRENT.md`
- `worklog/sessions/2026-08-22-106-production-guardrails-planning.md`
- `docs/architecture/public-profile-runtime-collection.md`
- `docs/product/p10-web-profile-flow-implementation-plan.md`
- `src/GitHubProof.Domain/Analysis/Enums/DataAvailabilityStatus.cs`
- `src/GitHubProof.Domain/Analysis/ValueObjects/DataAvailability.cs`
- `src/GitHubProof.Web/Profile/LiveProfileAnalysisResult.cs`
- `src/GitHubProof.Web/Profile/LiveProfileReportService.cs`
- `src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs`
- `src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileSnapshotOrchestrator.cs`
- `src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileSnapshotResult.cs`
- `src/GitHubProof.Web/Pages/Profile.cshtml.cs`
- `src/GitHubProof.Web/Program.cs`
- relevant existing Live Profile service/cancellation tests

## Canonical Decisions Locked

Architecture-owner re-review found one HIGH: the initial post-metadata deadline
did not bound metadata response-content consumption because the current collector
uses `ResponseHeadersRead`. The canonical boundary below supersedes that initial
Session 107 wording. The HIGH is closed. A later MEDIUM completeness finding is
also corrected below by locking all mandatory `DataAvailability` fields for
`ExecutionAvailability`.

### 1. Exhaustive live runtime boundary

The canonical Web/runtime shape is:

```text
ProfileAvailability?
+ ExecutionAvailability
+ ProfilePipelineResult?
```

`ExecutionAvailability` uses `DataKey=profile-execution` and only these exact
combinations:

| Profile | Execution | Exact reason | `RetrievedAt` | Pipeline |
|---|---|---|---|---|
| unresolved (`null`) | `Failed` | `execution-deadline-expired` | null | null |
| non-Available | `NotRequested` | `profile-unavailable` | null | null |
| Available | `Available` | null | UTC terminal-result instant | non-null |
| Available | `Failed` | `execution-deadline-expired` | null | null |
| Available | `Unavailable` | `github-budget-protection-rejected` | null | null |

Every execution availability uses exact `DataKey=profile-execution`. Successful
execution has `Reason=null`; `RetrievedAt` is the UTC instant at which the live
execution reaches its terminal non-null pipeline result. Every non-success has
`RetrievedAt=null`. This timestamp is operational runtime metadata only, never
evidence, profile observation time, repository retrieval time, analysis
provenance or report truth; Domain/Application and presentation cannot consume
it as developer evidence.

`RateLimited`, `Forbidden` and `NotApplicable` are prohibited execution
statuses. No additional execution reason is authorized. A non-null
`ProfilePipelineResult.Failed` is paired with execution `Available`, because
the pipeline result exists even though composition failed. Caller cancellation,
transport timeout and ordinary thrown faults retain their existing propagation
paths and do not fabricate a typed result.

Null `ProfileAvailability` means only that availability never resolved before
an execution-level terminal event; internal deadline expiration is the only
such event authorized initially. It is outer-runtime truth, never a provider
result or Domain/Application evidence input. If availability resolves first,
all later terminal outcomes preserve its truthful typed value.

### 2. Deadline boundary and attribution

After a valid Profile POST passes visitor/same-profile/global Web admission, the
deadline starts before the first live GitHub profile-metadata operation. It
covers metadata collection/content consumption, proactive budget admission,
repository-manifest collection, all repository collection/analysis,
composition, projection and live terminal-result construction; it ends before
Razor rendering/response transmission. It uses a dedicated internal source and
does not mutate `RequestAborted` or reuse ASP.NET request-timeout middleware.
The canonical configuration relation is strictly:

```text
0 < per-request transport timeout < end-to-end execution deadline
```

Exact values remain unselected. Caller cancellation wins overlaps and
propagates; otherwise the independently recorded terminal source distinguishes
internal deadline from transport timeout without exception-text inference.
Because current GitHub collectors use `ResponseHeadersRead`,
`HttpClient.Timeout` alone does not bound subsequent response-content reads.
Every live GitHub content read must observe the internal deadline token or an
independently owned, bounded and distinguishable token.

### 3. Operational Web mapping

| Outcome | HTTP |
|---|---:|
| visitor limiter | 429 |
| same-profile duplicate | 409 |
| global Profile capacity | 503 |
| internal execution deadline | 504 |
| proactive internal GitHub-budget rejection | 503 |

Deadline/budget outcomes are operational, produce no pipeline/Partial, and do
not become profile/repository availability, evidence, warning, limitation or
developer truth. Final EN/TR copy/layout remains P10-I5 work.

### 4. Initial provider, authentication and proxy scope

- Milestone B provider-budget/header protection is Profile-only. Repository
  Proof requests and behavior remain untouched. A shared circuit needs a later
  canonical decision and preservation proof.
- Unauthenticated production Profile beta is rejected for the measured request
  surface. A dedicated machine-user fine-grained PAT may be an interim external
  secret only after endpoint compatibility, least privilege, expiry and rotation
  are verified. GitHub App remains a valid longer-term candidate. No private
  repository or write/admin scope is authorized; credentials never enter
  telemetry/evidence.
- IP visitor limiting cannot receive production PASS until the deployment owner
  supplies the actual trusted proxy/network and hop topology. Raw
  `X-Forwarded-For` parsing and generic forwarded-header trust are prohibited.

### 5. Numeric decision boundary

Visitor `2/10 minutes`, reserve `500` and exact deadline remain measurement /
configuration candidates. Global concurrency `1`, same-profile concurrency `1`
and queue `0` remain provisional implementation recommendations, not canonical
numeric policy. Their rejection HTTP semantics are canonical as listed above.

## Findings / Remaining Inputs

1. Deployment owner must supply the actual trusted proxy/network topology.
2. Fine-grained PAT compatibility must be verified against every used endpoint;
   secret storage, expiry and rotation handling must be selected before beta.
3. Transport/deadline numbers, visitor window and budget reserve need
   implementation measurement and validated external configuration.
4. P10-I5 still owns final localized copy/layout for the newly authorized
   operational outcomes.

These findings do not block the contract-decision goal, but they remain
production enablement or downstream implementation gates.

## Scope / Semantic Review

- Repository Proof provider handling, collection and evidence semantics are
  unchanged; initial protection is explicitly Profile-only.
- No Domain/Application status, evidence, scoring, eligibility, pattern,
  composition or report semantic changed.
- Operational deadline/budget outcomes cannot enter repository availability or
  completed-so-far Profile composition.
- A null ProfileAvailability cannot enter evidence, warnings, limitations,
  scoring, composition or projection.
- Caller cancellation, transport timeout and internal deadline remain three
  distinct terminal sources.
- No production code, tests, project/configuration files or user-owned artifact
  changed.

## Validation

- Exact two-document canonical diff inspected for cross-document consistency.
- Status/reason/HTTP identities compared across both amendments.
- Production-path diff checked separately and empty.
- `git diff --check`: PASS.
- Build/tests not run: this session changes docs/worklog only.

## Suggested Commit

```text
docs(guardrails): lock production profile runtime contract
```

# Session 106 — Production Guardrails Contract / Implementation Planning

**Date:** 2026-08-22
**Result:** PASS WITH FINDINGS
**Branch:** `session/106-production-guardrails-planning`
**Base HEAD:** `f238e443878acc2b179c2e2c5013ac8b7fba7e6a`
**Model Route:** Sol / medium
**Review State:** Architecture-owner corrections applied; re-review required

## Goal

Determine the smallest production-safe guardrail architecture for public
Profile Proof before any production implementation. Keep visitor abuse
protection, global and same-profile concurrency, bounded execution lifetime,
GitHub API budget/rate-limit behavior, production authentication, partial
operational failure and guard telemetry distinct from product evidence.

## Mutation Boundary

Planning/worklog only. Production code, tests, configuration and canonical
product/domain semantics remain unchanged. Numeric recommendations are not a
canonical contract until independently reviewed and explicitly approved.

## Pre-existing Working Tree

- `?? artifacts/` is user-owned, untracked, out of scope and untouched.
- Local `main`, `origin/main` and the verified base all matched `f238e44`.

## Context Read

- `AGENTS.md`
- `agents/01-model-routing.md`
- `agents/02-context-loading.md`
- `agents/03-project-contract.md`
- `agents/04-development-workflow.md`
- `agents/06-review-and-validation.md`
- `agents/07-worklog-integration.md`
- `worklog/CURRENT.md`
- Session 105 handoff and `worklog/INDEX.md`

## Planning Result

### 1. Current Runtime Map

```text
POST /
  -> shape-only GitHubEntryClassifier
  -> 307 preserve-method redirect for a profile
POST /Profile
  -> GitHubEntryClassifier re-check
  -> LiveProfileReportService
  -> GitHubProfileSnapshotOrchestrator
       profile metadata
       public repository manifest (max 10 x 100 pages)
  -> ProfilePipeline
       ProfileRepositoryAnalysisOrchestrator (strictly sequential)
       per admitted repository: Profile-specific snapshot composition
       eligibility -> evidence -> patterns -> representatives -> composition
       -> ProfileReport
```

The expensive entry is only `POST /Profile`. `GET /Profile` redirects to `/`
without analysis. Static assets, locale operations and Repository Proof do not
enter this route.

Profile and Repository Proof use different named `HttpClient` instances, both
with a two-minute per-request transport timeout and the same optional
`GITHUB_TOKEN`. The token therefore owns one shared GitHub primary budget even
though request telemetry is separated. Profile repository execution is
sequential; no `Task.WhenAll` fan-out exists or is authorized.

No ASP.NET rate limiter, request-timeout policy, forwarded-header middleware,
trusted proxy/network list or tracked reverse-proxy configuration currently
exists. Consequently `RemoteIpAddress` is the proxy address in a normal Linux
reverse-proxy deployment unless deployment supplies and the app safely consumes
forwarded headers.

### 2. Verified GitHub Request and Failure Behavior

For a normal non-empty admitted repository, the current minimum request shape
is twelve requests before variable selected-file content and extra pagination:

```text
1 repository metadata
3 default-branch tree resolution (ref + commit + recursive tree)
3 descriptive endpoints (README + license + languages)
5 paginated signal surfaces (commits + issues + PRs + releases + tags)
= 12 minimum requests per admitted repository
```

The whole profile additionally needs one profile-metadata request and at least
one repository-manifest page. Selected project files and workflow files are
each fetched individually and currently have no file-count cap; documentation
Markdown is capped at 16 files. Decision-sufficient pagination bounds five
signal surfaces only when their decisions become irreversible. Therefore there
is no truthful universal hard request ceiling for one Profile Proof run.

The measured runs fit the request formula:

| Profile | Duration | Requests | Discovered | Admitted | Selected/other variable requests |
|---|---:|---:|---:|---:|---:|
| `fatihGunalp` | 19.523 s | 63 | 5 | 4 | 13 |
| `torvalds` | 61.438 s | 136 | 12 | 9 | 26 |

These are observations, not SLAs or maxima.

`GitHubApiAvailability` currently maps `429` to `RateLimited` and maps `403`
to `RateLimited` only when `X-RateLimit-Remaining: 0`; any other `403` becomes
`Forbidden`. This correctly covers primary exhaustion but can misclassify a
secondary-limit `403` whose primary remaining count is non-zero. It also does
not read `Retry-After`, `X-RateLimit-Limit`, `X-RateLimit-Reset` or
`X-RateLimit-Resource`.

Collectors continue to later surfaces after one surface receives a rate-limit
response. This conflicts operationally with GitHub's instruction to stop making
requests while limited. A provider-budget handler must suppress later outbound
attempts during a known blocked interval while returning a rate-limited
transport result to the existing collector mapping. It must never retry
automatically.

Current official GitHub facts verified on 2026-08-22:

- unauthenticated public REST: 60 requests/hour per originating IP;
- normal authenticated user/PAT: 5,000 requests/hour;
- secondary limits include 100 concurrent REST+GraphQL requests and currently
  documented point/CPU constraints, but these are provider-owned and may
  change;
- `Retry-After`, then remaining/reset headers, govern rate-limit backoff;
- continuing requests while rate limited can cause an integration ban;
- authenticated, serial requests are the recommended baseline.

Sources:

- <https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api>
- <https://docs.github.com/en/rest/using-the-rest-api/best-practices-for-using-the-rest-api>

### 3. Existing Typed-Result Capability Matrix

| Event | Current truthful representation | Finding |
|---|---|---|
| Profile metadata unavailable/rate-limited/forbidden/failed | `LiveProfileAnalysisResult` with no pipeline and typed `ProfileAvailability` | Sufficient |
| Per-repository provider failure | Existing `ProfileRepositoryAnalysisResult`, warnings/limitations and possibly composed `Partial` profile | Sufficient; operational failure never becomes negative evidence |
| Composition incoherence | `ProfilePipelineResult.Failed`, no profile/report | Sufficient |
| Caller/request cancellation | `OperationCanceledException` propagates | Sufficient and must remain unchanged |
| Per-request transport timeout before outer result | Non-caller cancellation propagates as operational fault | Distinguishable in telemetry, but no bounded Web result today |
| Internal end-to-end guard expiration | None | **Typed-boundary gap; minimum shape conceptually approved** |
| Proactive internal GitHub-budget stop | None | **Must terminate at outer execution boundary, never collector evidence availability** |
| Visitor/global/duplicate admission rejection | None | Web operational response must be added; no analysis result is fabricated |

Exact deadline gap: `LiveProfileAnalysisResult` permits only (a) a pipeline
result paired with available profile metadata, or (b) no pipeline paired with
unavailable profile metadata. A deadline may expire after metadata was
successfully collected but before the pipeline completes. Marking metadata
`Failed`, creating a `ProfilePipelineResult.Failed`, returning a partial report,
or using `RateLimited`/`Forbidden` would all be false.

The architecture owner conceptually approves the minimum future runtime shape:

```text
ProfileAvailability
+ ExecutionAvailability
+ ProfilePipelineResult?
```

It reuses `DataAvailability` / `DataAvailabilityStatus` and adds no Domain
evidence status. Required invariants are:

```text
profile unavailable
  -> execution NotRequested
  -> pipeline result null

profile available + execution available
  -> pipeline result non-null

profile available + internal deadline expiration
  -> execution non-success
  -> pipeline result null

profile available + proactive internal GitHub-budget rejection
  -> execution non-success
  -> pipeline result null
```

No operational outcome may enter Domain, Application evidence, repository
availability, scoring, composition or report projection. The exact reason
identities, allowed `ExecutionAvailability` statuses, and the deadline start
boundary needed to preserve the invariants must be locked in the canonical
amendment before implementation. The previous exact
`DataAvailabilityStatus.Failed` / `profile-execution` prescription is therefore
withdrawn as premature.

### 4. Guardrail Architecture

The guards remain six independent concepts:

1. **Visitor temporal limiter** — after a valid Profile input is parsed, consume
   one permit from a trusted-client-IP sliding-window partition. The permit is
   consumed even if a later capacity/budget guard rejects the attempt, preventing
   retry hammering. Queue length is zero. Rejection is `429` plus `Retry-After`.
2. **Proactive GitHub budget guard** — consult sanitized response-header state
   at an outer runtime seam where `ProfileAvailability` is already available
   and before `ProfilePipeline` starts. If the internal reserve policy rejects,
   terminate with non-success `ExecutionAvailability` and no pipeline result.
   Never synthesize collector `RateLimited`/`Failed` availability and never
   compose completed-so-far repositories into a `Partial` profile. No
   `/rate_limit` polling request.
3. **Same-profile guard** — parse with `GitHubProfileHandle.Parse`; key the
   validated login with `StringComparer.OrdinalIgnoreCase`, which collapses bare
   login, canonical URL and case variants before any provider call. Maximum one
   in-flight execution. Duplicate is rejected with `409`; no queue and no result
   reuse.
4. **Global profile concurrency** — one process-wide execution lease, acquired
   fail-fast with queue length zero. Rejection is `503`; concurrency controls
   simultaneous pressure only, never request count.
5. **End-to-end deadline** — exact duration remains unselected. Implementation
   must establish deterministic caller-vs-transport-vs-deadline attribution and
   a deliberate relationship between the per-request transport timeout and the
   longer outer execution deadline. A deadline equal to the current two-minute
   `HttpClient` timeout is rejected. The dedicated deadline token produces an
   outer execution non-success with no pipeline result; caller cancellation
   still propagates. ASP.NET request-timeout middleware remains unsuitable as
   the internal guard because it mutates `RequestAborted`.
6. **Provider rate limit** — GitHub-originated primary/secondary limit responses
   keep their existing typed provider-failure path where truthful. Proactive
   GitHub Proof reserve decisions stay outside that path. Whether provider
   header state/circuiting is Profile-only initially or shared with Repository
   Proof is a separate explicit architecture decision; Session 106 does not
   silently change Repository Proof.

Admission order is visitor -> same-profile -> global, followed by the outer
profile/budget/deadline runtime boundary. The exact budget checkpoint is part of
the canonical typed-boundary amendment.
Every acquired lease is released in `finally` on success, failure, caller
cancellation, transport timeout and deadline expiration.

All limiters are in-memory and intentionally process-local for the verified
single-VPS/single-instance Milestone B topology. Before horizontal scaling,
visitor/global/same-profile and credential-budget coordination must become
shared (for example a distributed limiter/lock or shared job coordinator), or
requests must be deterministically routed with an explicitly proven aggregate
limit. Sticky sessions alone do not provide a global credential budget.

### 5. Candidate Numeric Values (Recommendation Only)

| Value | What it limits | Evidence and reason | Rejection/failure | Configuration | Change trigger |
|---|---|---|---|---|---|
| Candidate `2 / 10 min` | Valid Profile POST attempts per trusted client IP | Initial abuse-control hypothesis only; requires actual proxy identity plus implementation/measurement validation | Candidate `429`, queue `0` | `ProfileGuardrails:VisitorRateLimit:*` | NAT false positives, rejection ratio, unique-client completion rate, abuse pattern |
| **Provisional `1` active, queue `0`** | Process-wide Profile executions | Single VPS, shared credential, serial inner pipeline, GitHub recommends serial API traffic | `503`, fail-fast | `ProfileGuardrails:GlobalConcurrency:*` | p95 duration/request count, CPU/memory, GitHub remaining slope, busy rejection ratio prove headroom for `2` |
| **Provisional `1` per normalized login, queue `0`** | Same-profile executions | Duplicate work adds no new truth; single-flight adds cancellation/result-sharing complexity | **`409`**, fail-fast | `ProfileGuardrails:SameProfile:Enabled=true` | only revisit if measured legitimate duplicate demand justifies cache/single-flight contract |
| **UNSELECTED** (`120 s` equal-timeout form rejected) | Snapshot + full pipeline lifetime | Current Profile transport timeout is also 120 s; equality makes cancellation attribution ambiguous. Measurements are insufficient to select both values safely | outer execution non-success; exact HTTP awaits P10-I5 amendment | `ProfileGuardrails:Execution:Timeout` plus Profile transport timeout configuration | deterministic attribution tests and p95/p99 duration/request measurements |
| Candidate `500` core remaining | Proactive Profile pipeline admission reserve | Initial 10% hypothesis only; measurements are not maxima and it cannot become collector evidence failure | outer execution non-success, no pipeline/report | `GitHubApiBudget:ProfileAdmissionRemainingFloor` | endpoint compatibility, p95 request cost, quota/reset observations |
| **UNSELECTED** | Trusted forwarded hop/proxy values | Repository contains no deployment topology | production gate cannot pass | `ReverseProxy:*` | deployment owner supplies exact proxy/network chain |

No global temporal start quota and no per-profile request-count cap is selected.
A start quota is a poor proxy for variable request cost, and a mid-run request
cap would silently truncate required evidence unless a new product/domain
contract defined the resulting completeness semantics.

### 6. Repository Proof vs Profile Proof Policy

| Policy | Profile Proof | Repository Proof |
|---|---|---|
| Visitor temporal limit | Protected Profile POST only | Unchanged; separate policy may be designed later from its own cost |
| Global/same-target concurrency | Profile-only | Unchanged |
| End-to-end deadline | Profile-only; exact value unselected, 120 s equal-timeout form rejected | Existing two-minute per-request transport timeout remains until a deliberate relationship is approved |
| Provider circuit/header state | Initial Profile-only protection **or** separately authorized shared circuit | Explicit architecture decision required; Repository Proof behavior cannot change silently |
| Typed evidence semantics | Never changed | Never changed |

Static assets, `GET /Profile`, locale changes and unrelated Repository Proof
traffic never consume a Profile visitor or concurrency permit.

### 7. Production Authentication Recommendation

**Corrected Milestone B decision:** a dedicated/machine-user fine-grained PAT
may be used as an interim, low-complexity beta credential through the existing
`GITHUB_TOKEN`, subject to exact compatibility verification for every current
Profile/Repository endpoint and secure secret, expiry and rotation handling.
It must not select private repositories or carry write/admin permission. Never
log its value; expose only an `authenticated=true/false` operational flag.

Why the alternatives are not the Milestone B default:

- unauthenticated: one measured 63-request run already exceeds the whole
  60/hour IP budget; unusable for public beta;
- GitHub App: **not rejected**. Current GitHub documentation gives installation
  flows public-repository read access and recommends GitHub Apps for long-lived
  integrations. It remains a valid longer-term production authentication
  candidate whose exact token mode, endpoint compatibility, lifecycle and
  operational complexity require a separate decision;
- OAuth or GitHub App user token: valid future per-user ownership model, but it
  adds interactive sign-in/session/token lifecycle and changes the current
  anonymous flow; defer to the later GitHub Sign-In milestone;
- classic/shared broad PAT: wider credential blast radius than the fine-grained
  public-read-only option.

Sources:

- <https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens>
- <https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/differences-between-github-apps-and-oauth-apps>

### 8. Reverse-Proxy / Visitor Identity Finding

The repository has no proxy configuration and `Program.cs` does not call
`UseForwardedHeaders`. Trusted client identity is therefore **not defined**.
`RemoteIpAddress` must not be used as the public security partition until the
actual VPS proxy IP/network is known and configured in `KnownProxies` or
`KnownNetworks` with `XForwardedFor | XForwardedProto`, `ForwardLimit=1`, and
`UseForwardedHeaders` before HTTPS redirection, localization, routing and all
guards. Unknown forwarders must be ignored; raw `X-Forwarded-For` must never be
parsed by application code. An absent client address uses one conservative
`unknown` partition. Raw IP/profile values are not telemetry labels.

This is a deployment-input STOP condition: the implementation may build typed
options and validation, but production enablement cannot pass until the exact
trusted proxy address/network is supplied.

Source:
<https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0>

### 9. Exact Configuration Surface

```text
GITHUB_TOKEN                                      interim beta candidate;
                                                  exact endpoint compatibility
                                                  and production requirement
                                                  await authentication decision

ProfileGuardrails:VisitorRateLimit:PermitLimit    UNSELECTED (candidate 2)
ProfileGuardrails:VisitorRateLimit:Window         UNSELECTED (candidate 00:10:00)
ProfileGuardrails:VisitorRateLimit:Segments       UNSELECTED
ProfileGuardrails:VisitorRateLimit:QueueLimit     0

ProfileGuardrails:GlobalConcurrency:PermitLimit   1
ProfileGuardrails:GlobalConcurrency:QueueLimit    0
ProfileGuardrails:SameProfile:Enabled             true
ProfileGuardrails:Execution:Timeout               UNSELECTED
GitHubProfileHttpClient:Timeout                   current 00:02:00;
                                                  deliberate relation required

GitHubApiBudget:ProfileAdmissionRemainingFloor    UNSELECTED (candidate 500)

ReverseProxy:ForwardLimit                         deployment-owned
ReverseProxy:KnownProxies                         deployment-owned list
ReverseProxy:KnownNetworks                        deployment-owned CIDR list
```

Bind selected values to validated options and validate on start. Non-zero
queues remain invalid. The production authentication decision owns whether a
missing interim PAT is a startup failure. An enabled IP limiter without an
explicit trusted-proxy topology is invalid (except an explicitly documented
direct-Kestrel deployment). Deadline/transport values are invalid unless their
relationship gives deterministic cancellation attribution.

### 10. Exact Proposed Production Files / Classes for a Later Session

Modify later:

- `src/GitHubProof.Web/Program.cs` — options validation, trusted forwarded
  headers, handler/guard registrations and middleware order.
- `src/GitHubProof.Web/Profile/LiveProfileReportService.cs` — service-owned
  deadline CTS and exhaustive caller/deadline/transport classification.
- `src/GitHubProof.Web/Profile/LiveProfileAnalysisResult.cs` — sanctioned
  `ProfileAvailability + ExecutionAvailability + ProfilePipelineResult?`
  boundary and its four required invariants.
- `src/GitHubProof.Web/Pages/Profile.cshtml.cs` — typed handle/admission,
  operational HTTP status and future P10-I5 mapping.
- `src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetry.cs` and
  `ProfileGitHubRequestTelemetryHandler.cs` — guard outcomes and sanitized
  provider headers.
- `src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs`
  — profile client handler composition; transport semantics unchanged.
- `src/GitHubProof.Infrastructure/GitHub/GitHubApiAvailability.cs` — secondary
  rate-limit classification using status + headers and, only when required, a
  bounded GitHub error response; never telemetry payload.

Add later (names are proposed, not locked):

- `ProfileGuardrailOptions.cs`
- `ProfileExecutionAdmissionService.cs`
- `ProfileExecutionLease.cs`
- `GitHubApiBudgetOptions.cs`
- `GitHubApiBudgetState.cs`
- `GitHubApiBudgetHandler.cs`
- `ReverseProxyOptions.cs`
- a tracked non-secret `appsettings.json` defaults surface if the architecture
  owner approves configuration files; secrets remain external.

No Domain/Application/evaluator/composer/projector change is proposed.

### 11. Exact Proposed Tests for the Implementation Session

1. Options binding/startup validation for every value and production token.
2. Only valid `POST /Profile` consumes a visitor permit; GET/static/locale and
   Repository Proof do not.
3. Trusted forwarded IP partitions visitors; spoofed/unknown forwarders are
   ignored; null IP uses the single `unknown` bucket.
4. The selected visitor candidate is parameterized and proven without assuming
   `2/10 min` is canonical; queue zero remains invariant.
5. One active profile holds the global lease; a different profile gets 503
   immediately; lease releases on all terminal paths.
6. bare login/canonical URL/case variants share the same-profile key; duplicate
   gets 409; different logins do not share that key.
7. Duplicate rejection never reuses a result and never queues.
8. The selected transport/deadline relationship deterministically attributes
   deadline vs transport timeout, yields no pipeline/report on internal expiry,
   and satisfies all four typed-boundary invariants.
9. Caller cancellation still propagates and wins an overlap race; transport
   timeout is neither caller cancellation nor deadline.
10. Completed-before-deadline can be Complete/Partial normally; completed-so-far
    repositories are never returned after expiration.
11. Primary 403, 429, secondary 403 with `Retry-After`, ordinary forbidden and
    reset behavior map distinctly.
12. A proactive internal budget rejection terminates at the outer runtime/Web
    boundary with no pipeline/report and cannot be caught as per-repository
    availability or composed into `Partial`.
13. Profile-only header protection is proven without changing Repository Proof;
    a shared-circuit variant is tested only after separate authorization.
14. Response telemetry records only bounded numeric/resource/status fields;
    never authorization, token, body, login, repository or file identity.
15. No `/rate_limit` request, no `Task.WhenAll`, no cache/single-flight, no new
    package and no Domain/Application change.
16. Existing Profile/Repository composition/cancellation/telemetry tests remain
    green, followed by full solution validation.

### 12. Telemetry Additions

Add bounded metrics/events without high-cardinality labels:

```text
profile_guard_admission_total
  decision = accepted | visitor-limited | github-budget-blocked |
             same-profile-active | global-busy
profile_guard_active_executions
profile_guard_deadline_expired_total
profile_guard_budget_rejected_total
profile_guard_rejection_retry_after_seconds (when known)

github_rate_limit_limit
github_rate_limit_remaining
github_rate_limit_reset_utc
github_retry_after_seconds
github_rate_limit_resource (closed known value or "other")
github_provider_blocked (boolean)
```

The canonical amendment must lock bounded operational outcome/reason identities
for deadline and proactive budget rejection before telemetry names are final.
Preserve `cancelled` and `faulted`. Existing duration, request categories,
discovered/admitted/completed/incomplete, final status and repository outcomes
stay. Never label metrics by IP, login, repository, path, token or error text.

### 13. Failure / Rejection to Future P10-I5 Presentation

| Operational event | HTTP | P10-I5 presentation | Evidence/report rule |
|---|---:|---|---|
| Visitor window exhausted | 429 | localized retry-later notice; `Retry-After` | no analysis/report |
| Global capacity busy | 503 | localized service-busy notice | no analysis/report |
| Same profile already active | 409 | localized already-running notice | no reuse, no analysis/report |
| Proactive internal budget guard rejects | exact HTTP pending P10-I5 amendment | localized operational protection notice; retry time only when known | execution non-success; no pipeline/report/Partial |
| Internal deadline expired | exact HTTP pending P10-I5 amendment | localized bounded-timeout notice | execution non-success; no pipeline/report/Partial |
| Entry metadata rate-limited | existing `RateLimited` | existing profile rate-limit branch, with HTTP status decision in P10-I5 | no profile/report |
| Repository-level rate limit/failure | 200 composed `Partial` where canonical pipeline permits | canonical warnings/limitations | completed evidence remains evidence, never whole-profile Complete if incomplete |
| Caller disconnect/cancellation | connection/cancellation path | no fabricated page | exception propagates |

The exact EN/TR copy remains P10-I5's presentation contract responsibility and
is not authored here.

### 14. Explicitly Rejected Alternatives

- IP partitioning from raw `X-Forwarded-For` or untrusted `RemoteIpAddress`.
- A global limiter on all routes or shared Repository/Profile visitor limits.
- Any queue: current contract does not authorize it and long executions create
  stale requests and cancellation complexity.
- Duplicate single-flight/result reuse: requires shared cancellation,
  lifetime/provenance and cache semantics not needed for Milestone B.
- Redis, distributed locks, background jobs or a new framework for a single
  instance.
- ASP.NET request timeout as the internal deadline because it mutates
  `RequestAborted` and erases the caller/deadline distinction.
- Per-profile request cap, hidden top-N repositories, sampling or concurrency
  as a fake request-cost reduction.
- Broad repository/collector fan-out.
- Polling `/rate_limit`; response headers already carry the needed state.
- Automatic retry after primary/secondary rate limit.
- Unauthenticated production traffic.
- Treating GitHub App as unsuitable solely because installation access is
  repository-scoped. GitHub App remains a valid longer-term candidate.
- OAuth/GitHub Sign-In expansion during anonymous Milestone B guardrails.

### 15. Open Decisions and STOP Conditions

1. **Typed-boundary detail STOP:** the architecture owner conceptually approved
   `ProfileAvailability + ExecutionAvailability + ProfilePipelineResult?` and
   its four invariants. Canonical amendment must still lock exact reason
   identities, allowed execution statuses, deadline start boundary and Web/HTTP
   mapping before implementation.
2. **Trusted-proxy STOP:** deployment owner must supply the exact VPS proxy
   address/network and real hop topology before the IP limiter can receive
   production PASS. Forwarded headers are never generically trusted.
3. Exact deadline is **unselected**. The previous `120 s` value is rejected in
   its equal-to-Profile-HttpClient-timeout form. Select transport and outer
   deadline values together and prove deterministic attribution.
4. Visitor `2/10 min` and reserve `500` remain unapproved measurement
   candidates. Global concurrency `1`, same-profile concurrency `1`, queue `0`
   and duplicate `409` remain provisional implementation recommendations.
5. Authentication remains open after the interim choice: verify every endpoint
   for machine-user fine-grained PAT compatibility; separately evaluate GitHub
   App as the valid longer-term production candidate.
6. Repository Proof provider protection requires an explicit choice: initial
   Profile-only guard, or separately authorized shared operational circuit that
   proves Repository Proof evidence semantics are preserved.

Canonical documentation amendments required after approval, but not made here:

- `docs/architecture/public-profile-runtime-collection.md`, §10 principle 3
  and §11.2: sanction the conceptually approved three-part runtime shape and
  four invariants; lock exact allowed execution statuses/reason identities;
  cover both deadline and proactive budget stop; require no pipeline result and
  prohibit entry into repository availability, evidence, composition or report;
  lock a deterministic transport-timeout/deadline relationship.
- `docs/product/p10-web-profile-flow-implementation-plan.md`, P10-I5 and §13.12:
  add the operational deadline and proactive-budget presentation inputs plus
  approved HTTP mapping, without adding any Domain evidence, profile or
  developer status.
- Deployment/proxy documentation owner must record trusted proxy/network and
  middleware order; no suitable canonical infrastructure file currently exists.

### 16. Proposed Independently Reviewable Implementation Slices

1. **Contract decision slice:** amend only the approved three-part typed runtime
   boundary, its four invariants, deadline/budget reason+status identities,
   deterministic timeout relationship and future P10-I5 inputs. Numeric values
   remain deployment recommendations selected through measurement.
2. **Trusted identity + visitor limiter:** forwarded headers, validated options,
   Profile POST-only sliding window, 429 telemetry/tests.
3. **Same-profile + global concurrency:** typed handle key, two fail-fast leases,
   409/503 telemetry/tests.
4. **Deadline boundary:** select a transport/deadline pair, implement
   `ExecutionAvailability`, linked-token ownership and exhaustive attribution
   tests; no presentation copy beyond the authorized P10-I5 contract.
5. **Profile budget + interim authentication:** endpoint-compatibility audit,
   secure interim PAT handling, Profile-only response-header state and
   measurement-selected reserve; proactive rejection is outer-runtime only,
   with no retry or `/rate_limit`.
6. **Optional shared-provider decision:** only if separately authorized, design
   and prove a shared operational circuit that preserves Repository Proof
   semantics.
7. **Telemetry + regression closure:** bounded metrics, full existing tests,
   controlled live measurements, config tuning evidence and independent review.

P10-I5 copy/layout and P10-I6 visual work remain separate downstream slices.

## Validation

- Planning-only source/contract reconciliation completed.
- Official GitHub and ASP.NET Core 10 documentation checked on 2026-08-22.
- Production `src/`, tests, project files, canonical docs and configuration are
  unchanged.
- Build/tests were not run because no production implementation changed and no
  planning claim required execution.

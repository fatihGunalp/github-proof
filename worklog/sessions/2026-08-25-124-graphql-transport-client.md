# S1 — GraphQL Transport Client

**Session:** 124
**Date:** 2026-08-25
**Route:** Opus / high
**Result:** PASS — S1 implemented and validated (build 0 warnings,
**1953/1953**). Independent review returned PASS WITH FINDINGS; the architecture
owner rejected F-2 with reason and accepted F-3 through F-6, all of which were
closed in-session and re-reviewed. The transport is registered, resolvable and
unused by production collection

---

## Goal

Implement the smallest isolated GraphQL transport slice authorized by
`docs/architecture/public-profile-runtime-collection.md` §§18-19 — Session 121's
**S1**: a Profile-only, read-only GraphQL client behind its own interface, with
no collector wired to it and no Profile result changed.

The load-bearing property of S1 is what does **not** happen. After this session
a `POST /graphql` is possible, correctly classified and correctly counted, and
nothing in the production Profile collection path issues one.

---

## Starting State

~~~text
Branch       session/124-graphql-transport-client
Base         2016501  (== main == origin/main, verified with git rev-parse)
             2016501 = "docs(profile): lock GraphQL transport partial-data and
                        error contract" (Session 123)
Working set  clean
Pre-existing ?? artifacts/  (user-owned, untouched, out of scope)
~~~

Precondition PASS. §18 (13 subsections) and §19 (17 criteria) were verified
present on `main` before any code was written, so this slice was gated by a
committed contract rather than by an uncommitted draft.

---

## Model Route

Opus / high. Real production code across Infrastructure, Web DI and the closed
telemetry vocabulary, with four failure modes that §18 exists to prevent: an
HTTP 200 carrying `errors` reading as ordinary success, one physical POST being
counted per surface, the D-2 profile-metadata budget premise drifting, and
GraphQL leaking into the collection path. Not `xhigh`: no canonical mutation,
isolated, reversible.

---

## Context Read

~~~text
AGENTS.md · CLAUDE.md
agents/02-context-loading.md · agents/03-project-contract.md
agents/04-development-workflow.md · agents/06-review-and-validation.md
agents/07-worklog-integration.md
docs/architecture/public-profile-runtime-collection.md
    §9, §9.1 · §18.4 · §18.7 · §18.9 · §19 (criteria 2, 11)
worklog/CURRENT.md
worklog/sessions/2026-08-25-121-...md, 2026-08-25-122-...md   (history only)
src  ProfilePipelineServiceCollectionExtensions · ProfileGitHubRequestCategory
     ProfileGitHubRequestTelemetryHandler · ProfileExecutionTelemetry
     ProfileGitHubBudgetScope · GitHubRepositorySnapshotOrchestrator
     GitHubRepositoryMetadataCollector (request-shaping convention)
tests existing Web/Infrastructure test conventions and handler fakes
~~~

---

## Implementation shape

Six small Infrastructure types plus two Web edits. No package, no second
HttpClient, no credential source, no generic GraphQL framework, no S2/S3
abstraction.

~~~text
Infrastructure/GitHub/Profile/
  IGitHubGraphQlTransport      one method: SendAsync(request, ct)
  GitHubGraphQlTransport       one physical POST on the EXISTING profile
                               named client; §18.4 state machine
  GitHubGraphQlRequest         query + optional JsonObject variables
  GitHubGraphQlResponse        transport outcome; no domain type anywhere
  GitHubGraphQlOutcome         Success / DataWithErrors / EnvelopeFailure /
                               Malformed
  GitHubGraphQlRateLimit       cost, limit, remaining, used, resetAt
~~~

**The §18.4 state machine**, decided from envelope shape only and never from
error wording:

~~~text
non-success HTTP status            -> EnvelopeFailure(status)
HttpRequestException               -> EnvelopeFailure(null)
body unparsable / not an object    -> Malformed
duplicate "data" or "errors" member -> Malformed
"errors" present and not an array  -> Malformed
errors[] non-empty                 -> DataWithErrors(errorCount)
no errors, no readable "data"      -> Malformed      <- the trap
no errors, "data" object           -> Success
caller cancellation                -> rethrown, never a result
~~~

The `no errors, no readable "data"` line is the point of the slice: `{}`,
`{"data": null}` and an empty body cannot become "success with nothing there",
which is exactly the successful absence §18.4 forbids. The duplicate-member line
closes the same hole from the other side (F-3): `System.Text.Json` resolves a
repeated member to the **last** occurrence, so
`{"errors":[{…}],"errors":[]}` would otherwise read as ordinary success. `IsSuccess` is deliberately narrow so callers branch on
the outcome rather than on `Data is not null`.

**What the transport deliberately does not carry.** GraphQL errors are
normalized to a **count**. No message, path or extension is retained, so nothing
downstream can branch on wording (§18.4) and nothing exists to leak into
telemetry (§18.9). `Data` *is* retained verbatim, which §18.5 will need for
isolation later. No `DataAvailability`, no domain type and no status is produced
anywhere — a test asserts that structurally, by reflection, so a later slice
cannot quietly add one here instead of at the surface adapter (§18.7).

**Headers.** Accept and User-Agent only. `X-GitHub-Api-Version` is deliberately
*not* sent: it is REST route versioning and GraphQL is not versioned that way.
Authorization is never touched in this code — it comes from the named client's
own configuration, which is what makes "no new credential source" true rather
than merely intended.

**Telemetry.** One new member in the closed vocabulary, `GraphQl = "graphql"`,
and one exact-match classifier arm. Because the existing handler records one
category per physical request, one POST increments the total once and the
GraphQL category once, with no per-surface counting possible by construction
(§18.9 counting rule). Nothing below `/graphql/` matches, so the vocabulary
cannot widen.

**Wiring.** `IGitHubGraphQlTransport` is registered scoped on the existing
profile named client. Nothing resolves it: the snapshot source still composes
the exact REST collector set, and no collector takes the dependency.

---

## Proof that no collector uses GraphQL

Two authoritative proofs and two supplementary ones. The claim is deliberately
bounded: it is **not** a proof that no mechanism in the assembly could ever
reach the transport, and it does not pretend to be.

**Authoritative:**

1. **Behavioural, at invocation time** — the production-wired
   `IProfileRepositorySnapshotSource`, resolved from `AddProfilePipeline` with an
   intercepting primary handler, issues REST requests and never addresses
   `/graphql`.
2. **DI, at construction and invocation time** — the transport registration is
   replaced with a factory that **throws**, and the graph is then resolved and
   exercised. Anything that took the dependency by constructor injection, or by
   calling `GetRequiredService` inside a registration factory, fails the test.

**Supplementary:**

3. **Constructor scan** — no Infrastructure type other than
   `GitHubGraphQlTransport` accepts `IGitHubGraphQlTransport` or
   `GitHubGraphQlTransport` as a constructor parameter. Bounded on purpose: it
   does not cover service-locator calls, `Lazy<>`/`Func<>`/`IEnumerable<>`
   wrappers, property injection, or types outside that assembly. Proofs 1 and 2
   are what cover those.
4. **Composition** — `GitHubRepositorySnapshotOrchestrator.CreateDefault`
   (Repository Proof) holds no field of the transport type, and
   `AddProfilePipeline` leaves the Repository Proof client name unconfigured
   while configuring only its own.

---

## Proof of the one-POST / one-count rule

A single envelope whose `data` carries five distinct fields is sent through the
real `ProfileGitHubRequestTelemetryHandler`:

~~~text
inner handler invocations                     1
telemetry.GitHubRequests                      1
RequestCategories["graphql"]                  1
RequestCategories keys                        exactly one — "graphql"
~~~

Five served fields, one count. Per-surface counting is not merely unimplemented,
it is unreachable: the handler classifies one physical request once.

The budget premise is proven separately in both directions — a GraphQL response
carrying a well-formed `X-RateLimit-Remaining` does **not** produce a
`MetadataObservation`, while a REST `users/{login}` response still does. REST and
GraphQL provider budgets stay separate exactly as §18.9 requires.

---

## Owner decision and F-2 to F-6 closure

The architecture owner **rejected F-2** with reason and **accepted F-3, F-4, F-5
and F-6**, directing that they be closed in this session. F-1 had already been
corrected. Every closure is additive or a narrowing; no test was weakened and no
canonical semantics moved.

| # | Owner ruling | What changed |
|---|---|---|
| **F-2** | **REJECTED as a finding.** The brief explicitly authorized a typed structural GraphQL rate-limit observation and made it required test **G**. Keep `GitHubGraphQlRateLimit`; do not remove it. Parsing `rateLimit` from a `DataWithErrors` response is acceptable because it is transport/provider metadata, never evidence or `DataAvailability` — and must not be strengthened into semantic use. | **Nothing removed.** `GitHubGraphQlRateLimit` and `ReadRateLimit` stay exactly as written. The reviewer did not have the session brief, which is why it read as unrequested groundwork. The ruling's boundary is already what the code does: the value is returned as bounded numbers on the transport result and is read by nothing. |
| **F-3** | **ACCEPT — fix required.** A duplicate top-level `errors` or `data` must never let an errored response become `Success`. No last-property-wins for these contract-bearing fields. Three named tests required. No raw error retention. | `GitHubGraphQlTransport.HasDuplicateContractMember` counts `data` and `errors` members via `EnumerateObject` — which yields duplicates, verified on this machine — and returns **Malformed** when either appears more than once. The guard runs before any member is read, so no precedence rule is ever applied. Four `Send_TreatsADuplicateContractMemberAsMalformed` cases cover the three required orderings plus `data:null` followed by a real `data`, and `Send_IgnoresDuplicatesOnMembersItDoesNotRead` keeps the guard narrow. |
| **F-4** | **ACCEPT as a proof-precision finding.** No generic DI scanner, no new abstraction. The behavioural production-composition test is authoritative; the structural constructor check is supplementary. Rewrite the claim so it does not pretend to rule out every theoretical mechanism. Add a small existing-DI assertion if it strengthens the proof without new framework code. | The claim is rewritten (below) and the supplementary test renamed to `NoInfrastructureConstructorTakesTheGraphQlTransport`, with its bounds stated in the test itself. One new test, `ProductionPipelineGraph_NeverResolvesTheGraphQlTransport`, replaces the transport registration with a **throwing** factory — last registration wins — then resolves `ProfilePipeline` and runs a real collection. No framework code: it uses the existing `ServiceCollection` only. |
| **F-5** | **ACCEPT — test quality fix required.** Replace both trivially-true assertions with structural proofs: a sentinel raw error message must not be retained in the response or any normalized error representation, and processing it must not place it into telemetry, categories or rate-limit observations. No logging abstraction, no `ToString()`-is-empty trivia. | `Send_RetainsNoGraphQlErrorTextAnywhereInTheResponse` uses three sentinels (message, path, extension) against a **populated** `data`, walks the whole response surface including the raw payload text and nested record fields, asserts the benign payload **is** present so the walk is not vacuous, and asserts no sentinel is. It also asserts the response exposes no non-`int` `Error*` member, so no normalized error representation can hold text. `Telemetry_CarriesNoGraphQlQueryErrorTextOrPayload` now sends rate-limit headers so a real bucket exists, asserts the surface actually recorded something, and only then asserts the three sentinels are absent from the log, every category key and every rate-limit window. |
| **F-6** | **ACCEPT — remove the mutation.** The transport must not assign or default `HttpClient.BaseAddress`; it receives the configured named client and trusts that contract. Tests may configure it explicitly. Do not alter the existing REST collectors for consistency. | `BaseAddress ??=` and the `GitHubApiBaseUri` constant are removed from `GitHubGraphQlTransport`. The constructor now only null-checks. The isolated test client already set `BaseAddress`; the helper says why. **No REST collector was touched.** |

---

## Changes

~~~text
Created (production):
- src/GitHubProof.Infrastructure/GitHub/Profile/IGitHubGraphQlTransport.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlTransport.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlRequest.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlResponse.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlOutcome.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlRateLimit.cs

Created (tests):
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GitHubGraphQlTransportTests.cs
- tests/GitHubProof.Web.Tests/ProfileGraphQlTransportWiringTests.cs

Modified (production):
- src/GitHubProof.Web/Profile/Runtime/ProfileGitHubRequestCategory.cs
    one constant, one All entry, one exact-match Classify arm
- src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
    one scoped registration on the existing named client

Modified (tests):
- tests/GitHubProof.Web.Tests/ProfileGitHubRequestClassificationTests.cs
- tests/GitHubProof.Web.Tests/ProfileRuntimeInstrumentationTests.cs
    the two vocabulary guards, 17 -> 18, with the new member named

Worklog:
- worklog/sessions/2026-08-25-124-graphql-transport-client.md (new)
- worklog/CURRENT.md
- worklog/INDEX.md

Canonical docs/: UNCHANGED. artifacts/: UNCHANGED.
No package, no migration, no benchmark, no Repository Proof change.
~~~

The two modified test files are guards that deliberately pin the vocabulary
size. Extending the vocabulary by exactly one GraphQL category is the change
§18.9 authorizes, so updating them is the authorized edit, not a weakened test:
both still assert the full ordered member list and both now name `graphql` in it.

---

## Decisions

~~~text
- Errors normalized to a count, not preserved. §18.5 attribution is S2/S3 work,
  and keeping error text would create exactly the leak §18.9 forbids.
- `RetrievedAt` deliberately NOT captured in this slice. §18.7 requires one
  instant per envelope taken after the body is accepted; the surface adapter
  receives the response only after the transport has parsed and validated it, so
  capturing it there satisfies §18.7 exactly. Adding an unused timestamp and a
  TimeProvider dependency now would be S2 groundwork.
- Variables typed as JsonObject, deep-cloned per send, so the body is built with
  no reflection-based serialization and a request stays reusable.
- No mutation-rejection guard. The transport is read-only by contract and S1
  ships no production query; a textual guard would be inventing a policy §18
  does not own.
- StatusCode kept on the response because it is the natural structural fact of
  an envelope failure; no other provider detail is retained.
~~~

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror   PASS — 0 warnings, 0 errors
GitHubGraphQlTransportTests                  31/31
focused Web wiring + telemetry + vocabulary  103/103
dotnet test GitHubProof.slnx                 1953/1953
    Domain 835 · Web 517 · Infrastructure 339 · Application 262
git diff --check                             PASS
git status --short                           src/tests/worklog only, plus the
                                             pre-existing artifacts/
~~~

Baseline was 1909 (Session 120); the +44 are this slice's own tests — 38 before
the review closure, plus 6 added by it (5 duplicate-member cases in
Infrastructure, 1 DI proof in Web). No existing test was deleted, skipped or
weakened; the two vocabulary guards were updated to 18 with the new member named
and still assert the full ordered list.

### Required test coverage A-N

~~~text
A  POST to /graphql with a JSON body                    Send_IssuesOnePostToGraphQlWithAJsonBody
B  existing Authorization reaches the request            Send_CarriesTheExistingClientAuthorizationHeader
C  one physical POST == one request/category count       OneGraphQlPost_CountsOnceInTotalAndOnceInTheGraphQlCategory
D  HTTP 200 + data + errors is not ordinary success      Send_DoesNotTreatHttp200WithErrorsAsOrdinarySuccess
E  no raw GraphQL error text in telemetry/logging        Send_RetainsNoGraphQlErrorTextAnywhereInTheResponse +
                                                         Telemetry_CarriesNoGraphQlQueryErrorTextOrPayload
F  successful data parsed, no domain evidence            Send_ParsesSuccessfulDataWithoutProducingDomainEvidence +
                                                         Response_ExposesNoDomainAvailabilityOrStatus
G  rateLimit parsed, not merged with the REST budget     Send_ParsesRateLimitAsBoundedStructuralValuesOnly +
                                                         GraphQlResponse_DoesNotFeedTheProfileMetadataBudgetDecision
H  non-success HTTP is envelope failure                  Send_ReportsNonSuccessStatusAsEnvelopeFailure (4 statuses) +
                                                         Send_ReportsATransportExceptionAsEnvelopeFailureWithoutAStatus
I  malformed/empty cannot become successful absence      Send_CannotTurnAnUnreadableBodyIntoSuccessfulAbsence (8 bodies)
J  caller cancellation propagates                        Send_PropagatesCallerCancellationInsteadOfReportingFailure
K  snapshot source still REST, GraphQL not wired         ProfileSnapshotSource_IssuesRestRequestsAndNeverGraphQl +
                                                         ProductionPipelineGraph_NeverResolvesTheGraphQlTransport +
                                                         ProfileSnapshotSource_StillResolvesTheRestComposition +
                                                         NoInfrastructureConstructorTakesTheGraphQlTransport
L  Repository Proof unchanged                            AddProfilePipeline_LeavesTheRepositoryProofClientUnconfigured +
                                                         RepositoryProofComposition_TakesNoGraphQlDependency
M  budget observation stays REST profile-metadata        GraphQlResponse_DoesNotFeedTheProfileMetadataBudgetDecision +
                                                         RestProfileMetadataResponse_StillFeedsTheBudgetDecision
N  existing tests green                                  1953/1953
~~~

---

## Problems

~~~text
- No canonical gap was hit: every decision this slice needed was already owned
  by §18/§19, so no STOP was required.
- Two existing vocabulary-guard tests failed on the first full run, exactly as
  intended — they pin the category count. Both were updated to 18 with the new
  member named rather than loosened.
- Independent review found one genuine §18.4-class hole (F-3, duplicate JSON
  `errors` key resolving to the last occurrence) that the eight malformed-body
  cases did not cover. It was unreachable in S1, so it was first reported rather
  than fixed; the owner then accepted it and it is now **closed** by the
  duplicate-contract-member guard, with the last-occurrence behaviour reproduced
  on this machine in both orderings before the guard was written.
~~~

---

## Not Done

~~~text
- No collector, snapshot source or pipeline uses the transport.
- No production GraphQL query, no repository envelope, no batching, no S2/S3.
- No concurrency, cache, retry, backoff or queue.
- No deadline, reserve, repository-cap or credential value selected.
- No package added. No canonical docs change. No performance or latency claim.
- No live GitHub call.
~~~

---

## Known Risks

~~~text
- §18.5 case 2 ("retain unaffected surfaces") is still unimplemented, and it is
  the clause most likely to be implemented optimistically in S2. The transport
  deliberately gives it nothing to be optimistic with: no error paths, and an
  outcome that is not Success.
- The transport is registered but unused, so nothing exercises it in production.
  That is the intent of S1, and it means the first real signal about GraphQL
  behaviour still comes from S2 onward.
- Provider behaviour is inherited from Session 121's documented facts; three of
  its schema facts remain explicitly unverified and none was assumed here.
~~~

---

## Independent Review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-25
Scope:    the six new Infrastructure types, both Web edits, both new test files,
          both modified vocabulary guards, CURRENT, INDEX, this record
Outcome:  2 MEDIUM, 4 LOW — none blocking
~~~

The reviewer re-derived the load-bearing behaviour from source rather than from
the tests: it traced `ReadEnvelope` against every input it was asked to try and
confirmed each lands in the right bucket, including 204/205 with no body and a
`text/html` body; confirmed `IsSuccess` is narrow enough that a careless caller
cannot branch back into the §18.4 failure mode; read
`ProfileGitHubRequestTelemetryHandler` directly and confirmed one physical POST
increments the total once and the `graphql` category once, with per-surface
counting structurally unreachable; confirmed the budget DECISION rule is
byte-for-byte unchanged and that rate-limit observations bucket by resource so
GraphQL and REST never merge; confirmed no domain type, `DataAvailability` or
status exists anywhere in the new code; confirmed the cancellation filter cannot
swallow a non-caller cancellation into the wrong bucket, and that it is the same
pattern the existing REST collectors use; and confirmed every brief prohibition
against the actual diff rather than the claims.

It also endorsed the deliberate decision **not** to capture `RetrievedAt` in the
transport: the surface adapter sees the response only after `ReadEnvelope` has
parsed, validated and accepted it, so §18.7's "captured only after the body has
been fully read, parsed, validated and accepted" is satisfied there, and the REST
side already stamps the instant in the collector rather than the HTTP layer.

### Findings and disposition

| # | Sev | Finding | Disposition |
|---|---|---|---|
| F-1 | MEDIUM | The session record declared `Result: PASS` while its own checklist showed the review unrun and CURRENT/INDEX said IN_PROGRESS — an internal contradiction and a self-granted closure (`AGENTS.md` §6). | **CORRECTED.** Purely mechanical, and once the review ran it was an actively false statement. The record now reads IN_PROGRESS with the review outcome. |
| F-2 | MEDIUM | `GitHubGraphQlRateLimit` and the three `ReadRateLimit` helpers were called unrequested S2 groundwork with no S1 consumer. | **FALSE POSITIVE**, with one real sub-observation kept. The Session 124 brief explicitly authorized it — *"the transport **may** expose a typed structural observation containing only bounded numeric/time fields such as cost, limit, remaining, used, resetAt"* — and made it required test **G**. The reviewer did not have the brief. The sub-observation stands and is reported unfixed: `ReadRateLimit` runs before the `errorCount` branch, so it reads from an envelope §18.5 would later discard. |
| F-3 | LOW | *(SUPERSEDED — owner accepted, now closed)* A duplicate JSON `errors` key reaches `Success`. `TryGetProperty` returns the **last** occurrence, so `{"data":{…},"errors":[{…}],"errors":[]}` classifies as Success despite carrying errors. | **REPORTED, NOT FIXED.** Independently reproduced on this machine (`has=True kind=Array len=0`). Unreachable in S1 — GitHub does not emit duplicate keys and no production caller exists — but it is a genuine §18.4-class hole. Closing it means a `Utf8JsonReader` pre-scan, which is a design decision beyond the brief. |
| F-4 | LOW | *(SUPERSEDED — owner accepted, now closed)* The unused-by-production proof is narrower than claimed: the reflection scan covers Infrastructure constructor parameters and exact types only, missing a Web-layer constructor, a `GetRequiredService` service-locator call inside a DI lambda, `Lazy<>`/`Func<>`/`IEnumerable<>` wrappers and property injection. The behavioural proof stops at the 404 metadata response. | **REPORTED, NOT FIXED.** Widening the scan to the Web assembly and to wrapper types is a real improvement and a scope decision. |
| F-5 | LOW | *(SUPERSEDED — owner accepted, now closed)* Two assertions are trivially true in their fixtures: `Send_KeepsNoGraphQlErrorText` asserts on `ToString()` where `Data` is null, and `Telemetry_CarriesNoGraphQlQueryErrorTextOrPayload` guards a `ToLogMessage()` that has no body-derived field at all. | **REPORTED, NOT FIXED.** Both are regression guards rather than proofs; the interesting case (`Data` populated **and** errors present, where `ToString()` would print the payload) is untested. |
| F-6 | LOW | *(SUPERSEDED — owner accepted, now closed)* `BaseAddress ??=` mutates an injected dependency and is dead in production, since the named client always sets it. | **REPORTED, NOT FIXED.** It matches the existing REST collector convention, so removing it here alone would create an inconsistency. |

**F-3 through F-6 were deliberately unapplied at review time.** Each was a
design or scope change rather than a mechanical contradiction, and the session
brief required reporting substantive findings for owner decision rather than
absorbing them.

**Superseded.** The architecture owner then rejected F-2 with reason and accepted
F-3 through F-6, directing their closure. What each fix does is recorded in
"Owner decision and F-2 to F-6 closure" above, and the closure was re-reviewed
independently. The disposition column in the table above is preserved as the
state *at first review*, not as the current state.

---

## Next

Architecture-owner acceptance of S1. The next slice is Session 121's **S2**
(one surface — languages — collected through GraphQL behind a default-off
switch, with §10 field-mapping and equivalence tests), which must satisfy the
relevant §19 criteria before it may be default-on.

The **Profile Production Runtime Decision Design** (D-1, D-2, D-3, D-5) remains
pending; the owner chose S1 first. The Session 122 §6.1 content-selector
duplicate-fetch defect still awaits its own bounded session.

---

## Git

~~~text
Suggested commit:
feat(profile): add profile-only graphql transport client

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Baseline and branch verified against Git before any mutation
[x] §§18-19 verified present on main before implementation
[x] Smallest transport: no package, no second client, no GraphQL framework
[x] §18.4 four states structurally distinguishable; 200+errors is not success
[x] No DataAvailability, no domain status, no new vocabulary beyond one category
[x] One physical POST counts once in total and once in the GraphQL category
[x] Caller cancellation propagates and is never converted into a failure result
[x] No collector, snapshot source or pipeline resolves the transport
[x] Repository Proof untouched
[x] Full build 0 warnings; full suite 1953/1953
[x] Independent review of this slice — PASS WITH FINDINGS (2 MEDIUM, 4 LOW,
    none blocking)
[x] F-1 corrected before the owner decision
[x] Owner rejected F-2 with reason; GitHubGraphQlRateLimit kept unchanged
[x] F-3 closed — duplicate contract-member guard + 5 tests
[x] F-4 closed — authoritative DI proof added, claim rewritten, scan narrowed
[x] F-5 closed — both trivially-true assertions replaced with structural proofs
[x] F-6 closed — BaseAddress mutation removed; no REST collector touched
[x] Re-review after closure
[ ] Architecture-owner acceptance of S1
~~~

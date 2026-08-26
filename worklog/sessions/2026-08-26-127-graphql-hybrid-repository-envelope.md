# S3 Hybrid Repository Envelope

**Session:** 127
**Date:** 2026-08-26
**Route:** Opus / high
**Result:** PASS — first AUTHORITATIVE hybrid slice, **DEFAULT OFF**, accepted
by the architecture owner with no production or test change requested. The
closure review returned PASS WITH FINDINGS (0 blocker, 0 high, 3 LOW) and all
three are corrected below.

---

## Baseline

~~~text
Branch : session/127-graphql-hybrid-repository-envelope
Base   : 09707f4  (== main == origin/main, verified before any mutation)
Working tree at start: ?? artifacts/  (user-owned, untouched)
~~~

Session 126 is **integrated as `09707f4`**. `CURRENT` and `INDEX` said "awaiting
integration" and named `5a4bae5` as the last integrated commit; that was stale
operational memory, and it is reconciled here rather than argued with.

---

## Goal

Implement the first authoritative Profile-only hybrid GraphQL repository
envelope, adopting exactly three surfaces:

~~~text
repository metadata
topics
languages
~~~

Everything else stays REST. This is S3. S4 batching/aliases is not started.

**Locked invariant:** transport is optimized; truth is not. For every input the
hybrid and REST compositions must preserve §6 semantic equivalence.

---

## Why this boundary

- metadata / topics / languages are the smallest useful **order-independent**
  bundle that can prove an authoritative seam at all.
- The `DecisionBounded` surfaces (commits, pull requests, releases, tags) stay
  out because §19.7 ordering and prefix parity is unproven.
- `defaultBranchRef` head and tree identity stays out because §18.13
  empty-repository parity is a separate proof obligation. Only the branch
  **name** is read here, and a null `defaultBranchRef` discards the candidate
  rather than being read as evidence.

---

## What was implemented

### New (Infrastructure / GitHub / Profile)

| File | Role |
|---|---|
| `GitHubGraphQlRepositoryEnvelope.cs` | The ACCEPTED envelope value: identity, metadata, topics, languages, one `RetrievedAt`. There is no partial form. |
| `IGitHubGraphQlRepositoryEnvelopeSource.cs` | The seam. `null` is the only failure shape, so no caller can act on a partial result. |
| `GitHubGraphQlRepositoryEnvelopeSource.cs` | One constant query, owner/name as variables; §18.4/§18.6/S126 classification; full structural validation; the single accepted instant. |
| `HybridRepositoryMetadataCollector.cs` | Profile-only decorator. Serves metadata/topics from the envelope and carries the accepted language map forward. |
| `ProfileGraphQlExecutionScope.cs` | The Session 125 scope, renamed and extended (below). |

### New (Infrastructure / GitHub)

`GitHubRepositoryLanguages.cs` — a languages surface already collected by
another transport, plus the instant it was retrieved.

### New (Web)

`ProfileGraphQlHybridOptions.cs` — one boolean, `RepositoryEnvelopeEnabled`,
section `ProfileGraphQlHybrid`, **default false**.

### Modified

- `GitHubRepositoryDescriptiveDataCollector` — one optional Profile-only
  delegate, following the shape the five paginated collectors already use for
  their §15 boundaries. When it supplies a map the languages request is never
  built; when it is null (Repository Proof, and every pre-S3 caller) the file
  behaves exactly as before.
- `GitHubRepositorySnapshotOrchestrator` — a fourth-argument
  `CreateProfileComposition` overload. `CreateDefault` is untouched.
- `GitHubRepositoryMetadataCollector` — `MetadataDataKey` private → internal, so
  the hybrid path names the same key instead of repeating the literal.
- `GitHubGraphQlLanguagesShadow`, `ShadowedDescriptiveDataCollector`,
  `LiveProfileReportService` — the scope rename only.
- `ProfileExecutionTelemetry` — two bounded counters and one log section.
- `ProfilePipelineServiceCollectionExtensions` — options binding, the envelope
  factory, and the precedence rule.

---

## Adopted field map

Every adopted field, and where it comes from on each transport:

~~~text
owner                REST owner.login          GraphQL repository.owner.login
name                 REST name                 GraphQL repository.name
url                  derived from owner/name   derived identically (NOT repository.url)
public/private       REST private == false     GraphQL isPrivate == false
archived             REST archived             GraphQL isArchived
fork                 REST fork                 GraphQL isFork
template             REST is_template          GraphQL isTemplate
primary language     REST language             GraphQL primaryLanguage.name
default branch NAME  REST default_branch       GraphQL defaultBranchRef.name
description          REST description          GraphQL description
homepage             REST homepage             GraphQL homepageUrl
created timestamp    REST created_at           GraphQL createdAt
has-issues flag      REST has_issues           GraphQL hasIssuesEnabled
topics               REST topics[]             GraphQL repositoryTopics.nodes[].topic.name
languages            REST /languages map       GraphQL languages.edges[].size + node.name
~~~

The canonical URL is deliberately **derived** on both sides rather than read
from GraphQL `url`, so it cannot diverge on a provider-side formatting
difference.

Topics use the exact REST normalization: blanks ignored, values trimmed,
case-insensitive distinct, first occurrence kept. Languages use
`LanguageEdge.size` as the exact byte count, keyed with the same
`OrdinalIgnoreCase` comparer, with no percentage, no total and no rounding.

---

## Fallback table

Any one of these discards the **whole** candidate bundle for that repository.
There is no partial merge between GraphQL metadata and REST languages, or the
reverse (§18.5).

| Condition | Run disabled? | Result |
|---|---|---|
| `EnvelopeFailure` (non-200, transport failure, client timeout) | **yes** | REST |
| `Malformed` body | **yes** | REST |
| HTTP 200 + errors + Session 126 provider-wide header | **yes** | REST |
| HTTP 200 + errors, no provider-wide header | no | REST, per-repository |
| `repository: null` | no | REST |
| missing or invalid required adopted field | no | REST |
| `defaultBranchRef: null` | no | REST |
| `isPrivate: true` | no | REST |
| `repositoryTopics.hasNextPage == true` | no | REST |
| `languages.hasNextPage == true` | no | REST |
| duplicate or invalid language edge | no | REST |
| GraphQL already disabled for this run | — | REST, no POST at all |

Fallback creates no warning, no limitation, no availability entry of its own and
no new status. It is a structural telemetry fact only.

**`hasNextPage` is a fallback, not a cap.** Nothing is truncated and no evidence
is omitted: REST then collects the whole surface. That keeps §7 intact and keeps
§18.7 simple — one accepted physical envelope, one accepted retrieval instant.

---

## Request graph, accepted repository

~~~text
before (REST)          after (hybrid, accepted)
---------------------  ------------------------
GET  /repos/O/N        POST /graphql
GET  /repos/O/N/languages
GET  /repos/O/N/readme GET  /repos/O/N/readme
GET  /repos/O/N/license GET /repos/O/N/license
... every other REST-retained request, unchanged
~~~

Two REST GETs become one POST: **one fewer physical provider request** for that
repository. That is a request-graph fact measured in tests. No latency,
percentage or production estimate is claimed or implied (§18.11).

REST-retained and proven still issued: profile metadata, repository manifest,
recursive/default-branch contents path, README, license/file, project files,
workflows, documentation Markdown, issues, commits, pull requests, releases,
tags.

---

## RetrievedAt

One UTC instant per accepted envelope, captured only after the body is read,
parsed, structurally validated and all three surfaces accepted. Metadata and
languages availability share exactly that instant; README, license and every
REST-retained surface keep their own.

The negative half is the real proof: a **discarded** candidate consumes no
instant at all, so no path can stamp a retrieval time onto something never
accepted. Wall-clock equality between separate REST and hybrid executions is
never asserted (§18.7, §19.2).

---

## Execution state

There is **one** run-scoped GraphQL disable. The Session 125
`GraphQlLanguagesShadowState` / `GraphQlLanguagesShadowScope` were renamed to
`ProfileGraphQlExecutionState` / `ProfileGraphQlExecutionScope` and given two
counters. Behaviour, lifetime and every existing test expectation are unchanged;
only the names moved. A languages-specific name would have described half of
what the state governs once an authoritative envelope shares it.

No second circuit, no persistence, no static or shared state, and no leak across
`AnalyzeAsync` executions.

---

## Configuration

`RepositoryEnvelopeEnabled` is **false** by default. Default OFF proves: the
exact existing Profile composition, zero authoritative GraphQL requests, no new
request, no result change, Repository Proof untouched.

**Precedence rule.** When the envelope is enabled the languages shadow is not
constructed at all. This is not a new policy: the envelope supplies the
languages surface itself, so a shadow comparing GraphQL against GraphQL would
observe nothing and would cost a second POST per repository — which the brief
explicitly forbids. Resolving precedence once at wiring time is the only shape
that satisfies it without a per-repository conditional, and the composition
factory **throws** if both are handed to it, so a wiring mistake fails loudly
rather than doubling GraphQL traffic.

---

## License boundary

`GitHubRepositoryMetadataCollection.License` has **no** Profile or Repository
semantic consumer, proven behaviourally rather than asserted: the fixture is
made to report different licenses on `/repos/{owner}/{name}` and
`/repos/{owner}/{name}/license`, and the snapshot carries the `/license` one.
The orchestrator builds `RepositorySnapshot` from `descriptive.License`, and
`LicensePresentSignalEvaluator` reads only `snapshot.License`.

The hybrid path therefore leaves that field null instead of creating a GraphQL
`licenseInfo` dependency to populate a value nothing reads. License and license
file stay REST (§18.2), and the hybrid run produces identical license evidence.

---

## Telemetry

Two new bounded structural counters — `RepositoryEnvelopeServed` and
`RepositoryEnvelopeFallback` — plus the existing `GraphQlDisabledForRun`. They
are **not** HTTP request counts and are never summed with one. One physical POST
serving three surfaces is still counted exactly once in `GitHubRequests` and
exactly once in the existing closed `graphql` category.

`ProfileGitHubRequestTelemetryHandler`, `ProfileGitHubRequestCategory` and
`ProfileGitHubBudgetState` are **unchanged as files**. No GraphQL header reaches
the §10.4 REST metadata budget decision. No repository identity, topic,
language, byte count, query, variable, payload, raw error or token can reach
telemetry: the recorder accepts two integers and a boolean.

---

## Error content

Not parsed: `message`, `path`, `type`, `extensions`. The Session 126 header-only
classification stays authoritative, and its residual is not papered over.

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror   0 Uyarı, 0 Hata
dotnet test  GitHubProof.slnx                2238/2238, 0 skipped
   Domain 835 · Infrastructure 590 · Web 551 · Application 262
git diff --check                             PASS
docs/                                        unchanged
artifacts/                                   untouched
~~~

2069 (S126) → 2238: **+169** (2232 before the review corrections, +6 after).

New test files:

~~~text
GitHubGraphQlRepositoryEnvelopeSourceTests.cs      unit, envelope + classification
ProfileHybridRepositoryEnvelopeSeamTests.cs        composition parity + request graph
HybridRepositoryEnvelopeRunDisableTests.cs         run-scoped disable, several repositories
ProfileGraphQlHybridEnvelopeWiringTests.cs         Web wiring, end-to-end parity, telemetry
~~~

### Required test surface, 1–26

| # | Requirement | Where |
|---|---|---|
| 1 | Default OFF → zero S3 requests, exact request graph | `DefaultOff_IssuesZeroGraphQlRequestsAndKeepsBothRestRequests`, `WithoutAnEnvelopeSource_TheRequestGraphIsUnchanged`, `RepositoryEnvelope_IsDisabledByDefault` |
| 2 | Accepted envelope: parity + both REST calls absent + one POST + README/license kept | `AnAcceptedEnvelope_SkipsTheRestMetadataAndLanguagesRequests`, `..._ProducesTheIdenticalAdoptedSurfaces`, `Enabled_ServesTheEnvelopeAndSkipsBothRestRequests` |
| 3 | Explicit field mapping per adopted field | `AnAcceptedEnvelope_MapsEveryAdoptedMetadataField`, `..._ProducesTheSameIdentityAndMetadataAsRest`, `..._CarriesEachAdoptedFlagThrough`, `ANullPrimaryLanguage_...`, `AnUnsetDescription_...`, `AnUnsetHomepage_...` |
| 4 | Topic normalization parity | `TopicsAreNormalizedExactlyAsRest` (5 cases, compared against the REST collector) |
| 5 | `LanguageEdge.size` exact byte parity | `LanguageEdgeSizeIsTheExactRestByteCount`, `ALargeLanguageSize_IsCarriedWithoutLoss`, `AZeroLanguageSize_IsAcceptedAsZeroBytes` |
| 6 | One envelope → one RetrievedAt | `AnAcceptedEnvelope_CapturesExactlyOneInstant`, `ADiscardedCandidate_CapturesNoInstantAtAll`, `AnAcceptedEnvelope_SharesOneInstantAcrossTheSurfacesItSupplied` |
| 7 | `languages.hasNextPage` → whole-candidate fallback | `AnIncompleteConnection_DiscardsTheWholeCandidate`, `AnIncompleteLanguagesConnection_AlsoDiscardsMetadataAndTopics` |
| 8 | `repositoryTopics.hasNextPage` → same | `AnIncompleteConnection_DiscardsTheWholeCandidate` |
| 9 | DataWithErrors, no signal → fallback, no disable, next repository may attempt | `ADataWithErrorsEnvelope_FallsBackWithoutDisablingTheRun`, `AFollowingRepository_MayStillBeServedAfterAnAbstention`, `AnAbstention_LeavesLaterRepositoriesServable` |
| 10 | S126 provider-wide header → fallback + disable, later repositories zero GraphQL | `AnEnvelopeLevelFailure_DisablesGraphQlForTheRun`, `AfterTheDisable_LaterRepositoriesIssueNoGraphQlRequest`, `AnEnvelopeLevelFailure_CostsOneAttemptForTheWholeExecution` |
| 11 | Malformed / EnvelopeFailure → fallback + disable | same two theories |
| 12 | `repository: null` → REST fallback, never observed absence | `ANullRepository_FallsBackWithoutDisablingTheRun` |
| 13 | Missing/invalid required metadata → fallback | `AMissingOrInvalidAdoptedField_DiscardsTheWholeCandidate` (21 cases), `ANullDefaultBranchRef_IsNeverReadAsEmptyEvidence` |
| 14 | Duplicate/invalid language → fallback | `AnInvalidLanguageEdge_DiscardsTheWholeCandidate` (9 cases) |
| 15 | Success + `remaining: 0`, no errors → not demoted | `ASuccessfulEnvelopeWithZeroRemaining_IsStillServed` |
| 16 | Cancellation propagates; not fallback/failure/disable | `CallerCancellation_PropagatesAndIsNeverAFallback`, `ATransportTimeout_DisablesTheRunRatherThanPropagating` |
| 17 | Fallback snapshot fingerprint == REST-only | `EveryFallback_ProducesTheIdenticalSnapshotAndRestRequests` (10 cases) |
| 18 | End-to-end Profile parity, accepted | `EnvelopeOffAndAccepted_ProduceTheIdenticalProfileReport` |
| 19 | End-to-end Profile parity, fallback | `EveryFallback_ProducesTheIdenticalProfileReport` (4 modes) |
| 20 | No new Domain/Application status | `AnAcceptedEnvelope_IntroducesNoNewAvailabilityVocabulary`, `EveryFallback_AddsNoWarningLimitationOrAvailabilityEntry` |
| 21 | No `totalCount` / `isEmpty` in the query | `TheQuery_NeverRequestsAnUnadoptedField` (14 tokens) + `TheQuery_RequestsEveryAdoptedSurface` so it cannot pass vacuously |
| 22 | Repository Proof composition/request graph untouched | `RepositoryProofComposition_HasNoEnvelopeSeam`, `RepositoryProofComposition_StillIssuesTheRestLanguagesRequest`, and the untouched Repository Proof suite |
| 23 | GraphQL budget isolation preserved | `AGraphQlRateLimitHeader_NeverReachesTheRestBudgetDecision`, plus the unchanged S126 transport test |
| 24 | One physical POST counted once | `TheEnvelopePost_CountsOnceInTheExistingGraphQlCategory`, `AnAcceptedRepository_CostsExactlyOneRequest` |
| 25 | No telemetry / error-content leakage | `Telemetry_EnvelopeSectionCarriesOnlyCountsAndTheDisableFlag`, `Telemetry_RendersNoEnvelopeSectionWhenTheEnvelopeNeverRan`, `Telemetry_RecordsEnvelopeOutcomesWithoutTouchingTheRequestCount` |
| 26 | Full suite green, 0 warnings | 2238/2238, 0 Uyarı |

The request-level assertions are deliberately paired with every parity claim: a
hybrid run that quietly kept issuing the two REST requests would satisfy every
parity test while consolidating nothing, so each parity test has a companion
that counts the requests.

**§19.4 is satisfied by non-adoption, not by coverage.** §19.4 asks for all three
§18.5 cases to be covered by test. S3 implements only case 3 — discard the whole
repository envelope — and cases 1 and 2 are structurally unreachable here,
because no partial envelope form exists to attribute an error to or to retain.
That is the conservative direction §18.5 mandates ("ambiguity is the default"),
but it is a deliberate non-adoption rather than a test that was written, and it
is named here rather than counted as coverage. A future slice that wants case 1
or case 2 has to earn them.

---

## Not done

- **S4 batching / aliases: NOT started.**
- **DecisionBounded surfaces NOT adopted** — commits, pull requests, releases,
  tags. §19.7 ordering and prefix parity remains unproven.
- **Default-branch head / commit / tree identity NOT adopted**; §18.13 parity is
  a separate obligation.
- Issues, README, license/file, contents, project files, workflows,
  documentation Markdown, profile metadata and the repository manifest are
  unchanged and stay REST.
- No canonical `docs/` change: §18.2 already lists these three as GraphQL
  candidates and §19 already states the acceptance bar, so this slice applies
  the locked contract rather than amending it.
- No live A/B, no production enablement, no commit, no push.

---

## Known risks

- **The Session 126 residual is unchanged and still blocks enablement.** A
  secondary rate limit that GitHub identifies only through the error message
  cannot be classified without parsing that message, which stays prohibited.
  `RepositoryEnvelopeEnabled` is therefore DEFAULT OFF and **not approved** for
  production, beta, the §19.14 live A/B validation or default-on use, even
  though S3 itself passes its own bar.
- **`homepageUrl` is a provider-typed field.** REST `homepage` is a raw string;
  GraphQL `homepageUrl` is a `URI` scalar. The mapping is pinned by test — the
  same input produces the same `RepositoryMetadata.Homepage` on both paths — but
  whether GitHub *emits* byte-identical values for a given repository is a live
  fact this slice cannot prove offline. `Homepage` is read by two signal
  evaluators, so this belongs in the §19.14 live A/B comparison rather than
  being assumed. It is recorded, not claimed closed.
- The same caveat, more weakly, applies to `primaryLanguage.name` versus REST
  `language`, and to `description`.
- `hasNextPage` fallback means a repository with more than 100 topics or more
  than 100 languages always costs the REST path. That is correct by design and
  costs requests, never evidence.

---

## Checklist

~~~text
[x] Precondition verified against Git before any mutation
[x] Session 126 reconciled as integrated (09707f4)
[x] S3 scope: metadata + topics + languages, authoritative, default OFF
[x] REST reference and fallback preserved, no partial merge
[x] One execution-scoped GraphQL disable, no second circuit
[x] Budget and telemetry isolation preserved
[x] Required test surface 1-26 covered
[x] Build 0 warnings, full suite green
[x] Hygiene sweep before review
[x] Independent closure review — PASS WITH FINDINGS (0 blocker, 0 high, 3 LOW);
    all three corrected under the accelerated-workflow rule
[x] Architecture-owner acceptance — PASS ACCEPTED, no production or test change
    requested; the remaining facts below are preserved verbatim as the owner
    stated them
~~~

---

## Architecture-owner verdict

**PASS ACCEPTED.** No production or test change requested. These remaining facts
are recorded as the owner stated them, and none of them is softened elsewhere in
this record:

- `RepositoryEnvelopeEnabled` remains **DEFAULT OFF**.
- **No production, beta, default-on or live-A/B approval exists.**
- The Session 126 **secondary rate-limit residual remains an enablement
  blocker** — a shape GitHub identifies only through the error message, which
  stays unparseable under the locked policy.
- **REST `homepage` versus GraphQL `homepageUrl` provider-value parity remains a
  live A/B validation risk. Provider-level equality must NOT be claimed before
  live evidence.** The field-mapping parity proven here is exactly that — a
  mapping proof, not a statement about what GitHub emits.
- `primaryLanguage` and `description` provider parity remain part of the **same
  rollout validation surface**.
- **`DecisionBounded` surfaces are NOT adopted** — commits, pull requests,
  releases, tags.
- **Default-branch head / tree is NOT adopted.**
- **S4 has NOT started.**
- **No latency or performance claim** is made or implied. The only measured
  claim is a request-graph one: two REST GETs become one POST for an accepted
  repository.

---

## Independent closure review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-26
Outcome:  0 BLOCKER, 0 HIGH, 3 LOW — none blocking
~~~

The reviewer re-ran the build and suite itself and confirmed from source: the
no-partial-merge property is structural rather than asserted (no partial envelope
form exists, and the language map is bound to the exact metadata instance the
same call produced); the `errors` array is never read beyond the Session 126
header classification; there is one `AsyncLocal` and one `Begin`, so no second
circuit; the category mapping is unchanged and one POST serving three surfaces is
counted once; the GraphQL rate-limit header leaves `MetadataObservation` null;
the four-argument overload with a null source is the previous composition
collector for collector; a clean `Success` cannot be demoted because the three
non-`DataWithErrors` factories never take the signal parameter; and
`GitHubRepositoryMetadataCollection.License` has no consumer, so the hybrid null
is safe.

The vacuity sweep found none, and specifically credited the anti-vacuity guards:
the negative query theory is paired with a positive half, and the field-mutation
theories each begin with `Assert.NotEqual(CompleteEnvelope, body)` so a
`Replace` that silently matched nothing cannot pass.

All three findings were corrected under the accelerated-workflow rule:

| # | Sev | Finding | Correction |
|---|---|---|---|
| F-1 | LOW | The run-scoped disable is shared, but both telemetry sections rendered on it. With the envelope enabled the shadow is never constructed, so a disabled run still logged `LanguagesShadow={Match=0;Mismatch=0;NotCompared=0;GraphQlDisabledForRun=True}` — a section for observations that never happened. | **CORRECTED.** Each section now renders only when its own recorder observed something; the shared disable flag is dropped from both gates. Nothing is lost, because both consumers record an outcome on the very call that disables the run. Two regression tests pin both directions, and the flag itself still reports the one §18.6 disable. |
| F-2 | LOW | The record claimed the 1-26 surface was covered without naming that §19.4's cases 1 and 2 are met by non-adoption rather than by test. | **CORRECTED.** The validation section now states it explicitly, above. |
| F-3 | LOW | The supplied-languages branch sat outside the REST branch's exception guard: `NormalizeLanguages` throws on a duplicate or negative byte count, and the REST branch turns that into a typed `Failed` availability while the supplied branch would have let it escape. Unreachable today — the envelope source already refuses both — but the delegate is public. | **CORRECTED.** The supplied branch now mirrors the REST guard exactly, so a refused map produces the same typed unavailability. Four regression tests cover a case-insensitive duplicate, a negative count, a blank name, and a well-formed map so the guard cannot pass by refusing everything. |

Two reviewer observations were recorded and deliberately not changed: the
negative-count guard also swallows `disabledForRun` on the same call (an exact
mirror of the pre-existing `RecordLanguagesShadow` guard, and unreachable), and
`GitHubRepositoryLanguages` plus the new public constructor overload sit in the
type shared with Repository Proof (justified by the §15 delegate pattern the five
paginated collectors already use, and introducing no new abstraction).

Only two production lines changed after the review — the two telemetry gates —
plus the new guard helper; the full suite was re-run afterwards.

---

## Git

~~~text
No commit. No push. HEAD == 09707f4.
Suggested commit (user-owned):

feat(profile): serve repository metadata, topics and languages over GraphQL
~~~
